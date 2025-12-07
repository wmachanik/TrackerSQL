using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    partial class PlanSummaryExporter
    {
        public static int Export(string migrationsDir, out string outputPath)
        {
            var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
            outputPath = Path.Combine(migrationsDir, "Metadata", "PlanSummary.csv");

            if (!Directory.Exists(accessSchemaDir))
            {
                Console.Error.WriteLine("Access schema folder not found: " + accessSchemaDir);
                return 2;
            }

            var files = Directory.GetFiles(accessSchemaDir, "*.schema.json")
                .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => Path.GetFileName(f))
                .ToArray();
            if (files.Length == 0)
            {
                Console.Error.WriteLine("No *.schema.json files found. Run Option 1 first.");
                return 2;
            }

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", new[]
            {
                "SourceTable","TargetTable","Classification","PreserveIds","Reviewed","Ignore",
                "TotalColumns","Copies","Renames","Drops","Computes","EmptyTargets","Status"
            }.Select(Csv)));

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var schema = JsonConvert.DeserializeObject<TableSchema>(json);
                if (schema == null || schema.Plan == null) continue;

                var actions = schema.Plan.ColumnActions ?? new List<ColumnPlan>();

                int copies = 0, renames = 0, drops = 0, computes = 0, emptyTargets = 0;
                foreach (var a in actions)
                {
                    var action = (a.Action ?? "Copy").Trim();
                    var tgt = string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target;

                    if (string.Equals(action, "Drop", StringComparison.OrdinalIgnoreCase)) drops++;
                    else if (string.Equals(action, "Compute", StringComparison.OrdinalIgnoreCase)) computes++;
                    else if (!string.Equals(a.Source, tgt, StringComparison.Ordinal)) renames++;
                    else copies++;

                    if (!string.Equals(action, "Drop", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(action, "Compute", StringComparison.OrdinalIgnoreCase) &&
                        string.IsNullOrWhiteSpace(tgt))
                        emptyTargets++;
                }

                var reviewed = schema.Plan.Reviewed;
                var ignore = schema.Plan.Ignore;
                var status = ignore ? "Ignored" : (reviewed && emptyTargets == 0 ? "Done" : "Pending");

                var row = new[]
                {
                    schema.SourceTable ?? "",
                    schema.Plan.TargetTable ?? schema.SourceTable ?? "",
                    schema.Plan.Classification ?? "Copy",
                    schema.Plan.PreserveIdsOnInsert ? "Yes" : "No",
                    reviewed ? "Yes" : "No",
                    ignore ? "Yes" : "No",
                    actions.Count.ToString(),
                    copies.ToString(),
                    renames.ToString(),
                    drops.ToString(),
                    computes.ToString(),
                    emptyTargets.ToString(),
                    status
                }.Select(Csv);

                sb.AppendLine(string.Join(",", row));
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? migrationsDir);
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            Console.WriteLine("Plan summary exported to: " + outputPath);
            return 0;
        }

        private static string Csv(string v)
        {
            if (v == null) return "";
            var needsQuote = v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
            if (!needsQuote) return v;
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        }

        public static int ExportFullReview(string migrationsDir, out string jsonPath, out string mdPath)
        {
            jsonPath = null;
            mdPath = null;

            try
            {
                var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                if (!Directory.Exists(accessSchemaDir))
                {
                    Console.Error.WriteLine("Access schema folder not found: " + accessSchemaDir);
                    return 2;
                }

                var files = Directory.GetFiles(accessSchemaDir, "*.schema.json")
                                     .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                                     .OrderBy(f => Path.GetFileName(f))
                                     .ToArray();
                if (files.Length == 0)
                {
                    Console.Error.WriteLine("No *.schema.json files found. Run option 1 first.");
                    return 2;
                }

                var review = new PlanReview
                {
                    GeneratedAtUtc = DateTime.UtcNow,
                    MigrationsDir = migrationsDir,
                    Tables = new List<PlanReviewTable>()
                };

                foreach (var f in files)
                {
                    TableSchema s = null;
                    try
                    {
                        s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(f));
                    }
                    catch (Exception ex)
                    {
                        review.Errors.Add("Failed to parse " + f + ": " + ex.Message);
                        continue;
                    }
                    if (s == null)
                    {
                        review.Errors.Add("Schema deserialized to null: " + f);
                        continue;
                    }

                    if (s.Plan == null)
                    {
                        s.Plan = new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                    }
                    if (s.Plan.ColumnActions == null) s.Plan.ColumnActions = new List<ColumnPlan>();

                    var tbl = new PlanReviewTable
                    {
                        SourceTable = s.SourceTable,
                        Classification = s.Plan.Classification ?? "Copy",
                        TargetTable = s.Plan.TargetTable ?? s.SourceTable,
                        PreserveIdsOnInsert = s.Plan.PreserveIdsOnInsert,
                        Reviewed = s.Plan.Reviewed,
                        Ignore = s.Plan.Ignore,
                        Columns = new List<PlanReviewColumn>(),
                        Warnings = new List<string>()
                    };

                    if (s.Plan.Normalize != null)
                    {
                        var n = s.Plan.Normalize;
                        tbl.Normalize = new PlanReviewNormalize
                        {
                            HeaderTable = n.HeaderTable,
                            LineTable = n.LineTable,
                            NewHeaderKeyName = n.NewHeaderKeyName,
                            NewLineKeyName = n.NewLineKeyName,
                            LineLinkKeyName = n.LineLinkKeyName,
                            HeaderPrimaryKey = (n.HeaderPrimaryKey ?? new List<string>()).ToList(),
                            LinePrimaryKey = (n.LinePrimaryKey ?? new List<string>()).ToList(),
                            HeaderColumns = (n.HeaderColumns ?? new List<string>()).ToList(),
                            LineColumns = (n.LineColumns ?? new List<string>()).ToList(),
                            OldCompositeKey = (n.OldCompositeKey ?? new List<string>()).ToList()
                        };
                    }

                    var dupGroups = s.Plan.ColumnActions
                        .Where(a => !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                        .GroupBy(a => (a.Target ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                        .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1)
                        .ToList();
                    foreach (var g in dupGroups)
                    {
                        tbl.Warnings.Add("Duplicate target mapping: " + g.Key + " mapped from [" + string.Join(", ", g.Select(x => x.Source)) + "]");
                    }

                    var headerCols = s.Plan.Normalize?.HeaderColumns ?? new List<string>();
                    var lineCols = s.Plan.Normalize?.LineColumns ?? new List<string>();
                    foreach (var a in s.Plan.ColumnActions.OrderBy(x => x.Source, StringComparer.OrdinalIgnoreCase))
                    {
                        var action = string.IsNullOrWhiteSpace(a.Action) ? "Copy" : a.Action;
                        var target = string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target;
                        var part = headerCols.Any(h => string.Equals(h, a.Source, StringComparison.OrdinalIgnoreCase)) ? "H"
                                   : lineCols.Any(l => string.Equals(l, a.Source, StringComparison.OrdinalIgnoreCase)) ? "L" : "";

                        tbl.Columns.Add(new PlanReviewColumn
                        {
                            Source = a.Source,
                            Target = target,
                            Action = action,
                            Expression = a.Expression,
                            Part = part
                        });

                        if (!string.Equals(action, "Drop", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(action, "Compute", StringComparison.OrdinalIgnoreCase) &&
                            string.IsNullOrWhiteSpace(target))
                        {
                            tbl.Warnings.Add("Column '" + a.Source + "' has empty Target for action '" + action + "'.");
                        }
                    }

                    if (string.Equals(tbl.Classification, "Normalize", StringComparison.OrdinalIgnoreCase))
                    {
                        var n = tbl.Normalize ?? new PlanReviewNormalize();

                        if (string.IsNullOrWhiteSpace(n.HeaderTable))
                            tbl.Warnings.Add("Normalize: HeaderTable is not set.");
                        if (string.IsNullOrWhiteSpace(n.LineTable))
                            tbl.Warnings.Add("Normalize: LineTable is not set.");

                        var hasHeaderPk = !string.IsNullOrWhiteSpace(n.NewHeaderKeyName) || (n.HeaderPrimaryKey != null && n.HeaderPrimaryKey.Count > 0);
                        var hasLinePk = !string.IsNullOrWhiteSpace(n.NewLineKeyName) || (n.LinePrimaryKey != null && n.LinePrimaryKey.Count > 0);
                        if (!hasHeaderPk)
                            tbl.Warnings.Add("Normalize: Header PK not defined (NewHeaderKeyName or HeaderPrimaryKey).");
                        if (!hasLinePk)
                            tbl.Warnings.Add("Normalize: Line PK not defined (NewLineKeyName or LinePrimaryKey).");

                        if (string.IsNullOrWhiteSpace(n.LineLinkKeyName))
                            tbl.Warnings.Add("Normalize: Line link FK (LineLinkKeyName) is not set.");

                        if ((n.HeaderColumns == null || n.HeaderColumns.Count == 0) &&
                            (n.LineColumns == null || n.LineColumns.Count == 0))
                        {
                            tbl.Warnings.Add("Normalize: No Header/Line column assignments.");
                        }

                        var knownSrc = new HashSet<string>(tbl.Columns.Select(c => c.Source), StringComparer.OrdinalIgnoreCase);
                        foreach (var hc in n.HeaderColumns ?? new List<string>())
                        {
                            if (!knownSrc.Contains(hc))
                                tbl.Warnings.Add("Normalize: Header assignment references unknown column '" + hc + "'.");
                        }
                        foreach (var lc in n.LineColumns ?? new List<string>())
                        {
                            if (!knownSrc.Contains(lc))
                                tbl.Warnings.Add("Normalize: Line assignment references unknown column '" + lc + "'.");
                        }
                    }

                    review.Tables.Add(tbl);
                }

                var outDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits");
                Directory.CreateDirectory(outDir);
                jsonPath = Path.Combine(outDir, "PlanReview.json");
                mdPath = Path.Combine(outDir, "PlanReview.md");

                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(review, Formatting.Indented), Encoding.UTF8);

                var md = BuildMarkdown(review);
                File.WriteAllText(mdPath, md, Encoding.UTF8);

                Console.WriteLine("Plan review written:");
                Console.WriteLine("  JSON: " + jsonPath);
                Console.WriteLine("  MD:   " + mdPath);
                if (review.Errors.Count > 0)
                {
                    Console.WriteLine("Errors (" + review.Errors.Count + "):");
                    foreach (var e in review.Errors.Take(10)) Console.WriteLine("  - " + e);
                    if (review.Errors.Count > 10) Console.WriteLine("  ... " + (review.Errors.Count - 10) + " more");
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to export plan review: " + ex.Message);
                return 1;
            }
        }

        public static int ExportBeforeAfterCsv(string migrationsDir, out string csvPath)
        {
            csvPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "BeforeAfter.csv");

            try
            {
                var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                if (!Directory.Exists(accessSchemaDir))
                {
                    Console.Error.WriteLine("Access schema folder not found: " + accessSchemaDir);
                    return 2;
                }

                var files = Directory.GetFiles(accessSchemaDir, "*.schema.json")
                                     .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                                     .OrderBy(f => Path.GetFileName(f))
                                     .ToArray();
                if (files.Length == 0)
                {
                    Console.Error.WriteLine("No *.schema.json files found. Run option 1 first.");
                    return 2;
                }

                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", new[]
                {
                    "SourceTable","Classification","Ignore","Part",
                    "BeforeTable","BeforeColumn","BeforeSqlType","BeforeIsPK","BeforeIsAuto",
                    "AfterTable","AfterColumn","AfterSqlType","AfterIsPK","AfterIsFK","AfterFkRefTable","AfterFkRefKey",
                    "Action","Expression"
                }.Select(Csv)));

                foreach (var f in files)
                {
                    TableSchema s;
                    try { s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(f)); }
                    catch (Exception ex) { Console.Error.WriteLine("Parse error: " + f + " -> " + ex.Message); continue; }
                    if (s == null) continue;

                    if (s.Plan == null)
                        s.Plan = new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                    if (s.Plan.ColumnActions == null)
                        s.Plan.ColumnActions = new List<ColumnPlan>();

                    // Map source columns and preserve source order by Ordinal
                    var colBySource = new Dictionary<string, ColumnSchema>(StringComparer.OrdinalIgnoreCase);
                    foreach (var c in (s.Columns ?? new List<ColumnSchema>()))
                        if (!string.IsNullOrWhiteSpace(c.SourceName)) colBySource[c.SourceName] = c;

                    var orderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    int __ord = 0;
                    foreach (var c in (s.Columns ?? new List<ColumnSchema>()).OrderBy(c => c.Ordinal))
                        orderIndex[c.SourceName] = __ord++;

                    // Classification + Normalize context
                    var classification = s.Plan.Classification ?? "Copy";
                    var isNormalize = string.Equals(classification, "Normalize", StringComparison.OrdinalIgnoreCase);

                    var headerSet = new HashSet<string>(isNormalize ? (s.Plan.Normalize?.HeaderColumns ?? new List<string>()) : new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var lineSet = new HashSet<string>(isNormalize ? (s.Plan.Normalize?.LineColumns ?? new List<string>()) : new List<string>(), StringComparer.OrdinalIgnoreCase);

                    var headerPk = new HashSet<string>(isNormalize ? (s.Plan.Normalize?.HeaderPrimaryKey ?? new List<string>()) : new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var linePk = new HashSet<string>(isNormalize ? (s.Plan.Normalize?.LinePrimaryKey ?? new List<string>()) : new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var newHeaderKey = isNormalize ? (s.Plan.Normalize?.NewHeaderKeyName ?? "") : "";
                    var newLineKey = isNormalize ? (s.Plan.Normalize?.NewLineKeyName ?? "") : "";
                    var linkFk = isNormalize ? (s.Plan.Normalize?.LineLinkKeyName ?? "") : "";
                    var headerTable = isNormalize ? (s.Plan.Normalize?.HeaderTable ?? "") : "";
                    var lineTable = isNormalize ? (s.Plan.Normalize?.LineTable ?? "") : "";
                    var targetSingle = s.Plan.TargetTable ?? s.SourceTable ?? "";

                    // Infer effective Link FK when not set
                    var mappedLineTargets = (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                        .Where(a => lineSet.Contains(a.Source ?? "", StringComparer.OrdinalIgnoreCase) &&
                                    !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                        .Select(a => string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target)
                        .ToList();
                    var headerKeyNamePref = !string.IsNullOrWhiteSpace(newHeaderKey) ? newHeaderKey
                                              : (headerPk != null && headerPk.Count > 0 ? headerPk.First() : "");
                    var effectiveLinkFk = !string.IsNullOrWhiteSpace(linkFk) ? linkFk
                                              : (!string.IsNullOrWhiteSpace(headerKeyNamePref) &&
                                                 mappedLineTargets.Any(t => string.Equals(t, headerKeyNamePref, StringComparison.OrdinalIgnoreCase)))
                                                    ? headerKeyNamePref
                                                    : "";

                    // PK detection from Access
                    Func<string, bool> isPkSource = name =>
                        (s.PrimaryKey != null && s.PrimaryKey.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase))) ||
                        (colBySource.TryGetValue(name, out var ccs) && (ccs?.IsKey ?? false));

                    // Emit synthetic Normalize keys/FK first (so PKs are first)
                    if (isNormalize)
                    {
                        Func<string, bool> targetExists = target =>
                            s.Plan.ColumnActions.Any(x =>
                                !string.Equals(x.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase) &&
                                string.Equals((string.IsNullOrWhiteSpace(x.Target) ? x.Source : x.Target) ?? "",
                                              target ?? "", StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrWhiteSpace(newHeaderKey) && !targetExists(newHeaderKey))
                        {
                            sb.AppendLine(string.Join(",", new[]
                            {
                                Csv(s.SourceTable), Csv(classification), s.Plan.Ignore ? "Yes" : "No", "Header",
                                Csv(s.SourceTable), "", "", "No", "No",
                                Csv(headerTable), Csv(newHeaderKey), "", "Yes", "No", "", "",
                                "NewKey", ""
                            }));
                        }
                        if (!string.IsNullOrWhiteSpace(newLineKey) && !targetExists(newLineKey))
                        {
                            sb.AppendLine(string.Join(",", new[]
                            {
                                Csv(s.SourceTable), Csv(classification), s.Plan.Ignore ? "Yes" : "No", "Line",
                                Csv(s.SourceTable), "", "", "No", "No",
                                Csv(lineTable), Csv(newLineKey), "", "Yes", "No", "", "",
                                "NewKey", ""
                            }));
                        }
                        if (!string.IsNullOrWhiteSpace(effectiveLinkFk) && !targetExists(effectiveLinkFk))
                        {
                            sb.AppendLine(string.Join(",", new[]
                            {
                                Csv(s.SourceTable), Csv(classification), s.Plan.Ignore ? "Yes" : "No", "Line",
                                Csv(s.SourceTable), "", "", "No", "No",
                                Csv(lineTable), Csv(effectiveLinkFk), "", "No", "Yes", Csv(headerTable),
                                Csv(headerPk.Count > 0 ? string.Join("|", headerPk) : (!string.IsNullOrWhiteSpace(newHeaderKey) ? newHeaderKey : "")),
                                "LinkFK", ""
                            }));
                        }
                    }

                    // Emit mapped rows: PK-first ordering
                    Func<ColumnPlan, int> pkPriority = a =>
                    {
                        if (!isNormalize) return isPkSource(a.Source ?? "") ? 0 : 1;

                        var src = a.Source ?? "";
                        var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;
                        if (headerSet.Contains(src) &&
                            (headerPk.Contains(tgt) || (!string.IsNullOrWhiteSpace(newHeaderKey) && string.Equals(tgt, newHeaderKey, StringComparison.OrdinalIgnoreCase))))
                            return 0;
                        if (lineSet.Contains(src) &&
                            (linePk.Contains(tgt) || (!string.IsNullOrWhiteSpace(newLineKey) && string.Equals(tgt, newLineKey, StringComparison.OrdinalIgnoreCase))))
                            return 0;
                        return 1;
                    };

                    foreach (var a in s.Plan.ColumnActions
                                     .OrderBy(x => pkPriority(x))
                                     .ThenBy(x => orderIndex.TryGetValue(x.Source ?? "", out var pos) ? pos : int.MaxValue)
                                     .ThenBy(x => x.Source, StringComparer.OrdinalIgnoreCase))
                    {
                        var src = a.Source ?? "";
                        var part = isNormalize ? (headerSet.Contains(src) ? "Header" : (lineSet.Contains(src) ? "Line" : "Single")) : "Single";

                        var beforeTbl = s.SourceTable ?? "";
                        var beforeCol = src;
                        colBySource.TryGetValue(src, out var cs);
                        var beforeType = cs?.RecommendedSqlType ?? cs?.DotNetType ?? "";
                        var beforeIsPk = isPkSource(src) ? "Yes" : "No";
                        var beforeIsAuto = (cs?.IsAutoIncrement ?? false) ? "Yes" : "No";

                        var action = string.IsNullOrWhiteSpace(a.Action) ? "Copy" : a.Action.Trim();
                        var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;
                        string afterTbl, afterCol, afterType, afterIsPk, afterIsFk = "No", fkRefTable = "", fkRefKey = "";

                        if (string.Equals(action, "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            afterTbl = "";
                            afterCol = "";
                            afterType = "";
                            afterIsPk = "No";
                        }
                        else
                        {
                            if (!isNormalize)
                            {
                                afterTbl = targetSingle;
                                afterCol = tgt;
                                afterType = beforeType;
                                // Non-normalize: assume PK unchanged
                                afterIsPk = beforeIsPk;
                            }
                            else
                            {
                                if (string.Equals(part, "Header", StringComparison.OrdinalIgnoreCase))
                                {
                                    afterTbl = headerTable;
                                    afterCol = tgt;
                                    afterType = beforeType;
                                    afterIsPk = (headerPk.Contains(tgt) || (!string.IsNullOrWhiteSpace(newHeaderKey) && string.Equals(tgt, newHeaderKey, StringComparison.OrdinalIgnoreCase))) ? "Yes" : "No";
                                }
                                else if (string.Equals(part, "Line", StringComparison.OrdinalIgnoreCase))
                                {
                                    afterTbl = lineTable;
                                    afterCol = tgt;
                                    afterType = beforeType;
                                    var isPk = (linePk.Contains(tgt) || (!string.IsNullOrWhiteSpace(newLineKey) && string.Equals(tgt, newLineKey, StringComparison.OrdinalIgnoreCase)));
                                    afterIsPk = isPk ? "Yes" : "No";

                                    if (!string.IsNullOrWhiteSpace(effectiveLinkFk) && string.Equals(tgt, effectiveLinkFk, StringComparison.OrdinalIgnoreCase))
                                    {
                                        afterIsFk = "Yes";
                                        fkRefTable = headerTable;
                                        fkRefKey = headerPk.Count > 0 ? string.Join("|", headerPk) : (!string.IsNullOrWhiteSpace(newHeaderKey) ? newHeaderKey : "");
                                    }
                                }
                                else
                                {
                                    // Unassigned in normalize -> treat as Single (rare)
                                    afterTbl = targetSingle;
                                    afterCol = tgt;
                                    afterType = beforeType;
                                    afterIsPk = "No";
                                }
                            }
                        }

                        sb.AppendLine(string.Join(",", new[]
                        {
                            Csv(s.SourceTable),
                            Csv(classification),
                            s.Plan.Ignore ? "Yes" : "No",
                            Csv(part),
                            Csv(beforeTbl), Csv(beforeCol), Csv(beforeType), beforeIsPk, beforeIsAuto,
                            Csv(afterTbl), Csv(afterCol), Csv(afterType), afterIsPk, afterIsFk, Csv(fkRefTable), Csv(fkRefKey),
                            Csv(action), Csv(a.Expression ?? "")
                        }));
                    }

                    // If table is ignored and had no rows (no actions), emit a marker line
                    if (s.Plan.Ignore && (s.Plan.ColumnActions == null || s.Plan.ColumnActions.Count == 0))
                    {
                        sb.AppendLine(string.Join(",", new[]
                        {
                            Csv(s.SourceTable), Csv(classification), "Yes", "Single",
                            Csv(s.SourceTable), "", "", "No", "No",
                            "", "", "", "No", "No", "", "",
                            "Ignored", ""
                        }));
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(csvPath) ?? migrationsDir);
                File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
                Console.WriteLine("Before/After CSV written: " + csvPath);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ExportBeforeAfterCsv failed: " + ex.Message);
                return 1;
            }
        }

        private static string BuildMarkdown(PlanReview review)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Plan Review");
            sb.AppendLine();
            sb.AppendLine("- Generated (UTC): " + review.GeneratedAtUtc.ToString("u"));
            sb.AppendLine("- Tables: " + review.Tables.Count);
            if (review.Errors.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## Errors");
                foreach (var e in review.Errors) sb.AppendLine("- " + e);
            }

            foreach (var t in review.Tables.OrderBy(x => x.SourceTable, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine();
                sb.AppendLine("## " + (t.SourceTable ?? "(unknown)"));
                sb.AppendLine("- Classification: " + (t.Classification ?? "Copy"));
                sb.AppendLine("- TargetTable: " + (t.TargetTable ?? "(unset)"));
                sb.AppendLine("- PreserveIdsOnInsert: " + (t.PreserveIdsOnInsert ? "Yes" : "No"));
                sb.AppendLine("- Reviewed: " + (t.Reviewed ? "Yes" : "No"));
                sb.AppendLine("- Ignore: " + (t.Ignore ? "Yes" : "No"));

                if (t.Normalize != null)
                {
                    var n = t.Normalize;
                    sb.AppendLine("- Normalize:");
                    sb.AppendLine("  - HeaderTable: " + (n.HeaderTable ?? "(unset)"));
                    sb.AppendLine("  - LineTable: " + (n.LineTable ?? "(unset)"));
                    sb.AppendLine("  - OldCompositeKey: " + ((n.OldCompositeKey != null && n.OldCompositeKey.Count > 0) ? string.Join(", ", n.OldCompositeKey) : "(unset)"));
                    sb.AppendLine("  - NewHeaderKeyName: " + (n.NewHeaderKeyName ?? "(unset)"));
                    sb.AppendLine("  - NewLineKeyName: " + (n.NewLineKeyName ?? "(unset)"));
                    sb.AppendLine("  - LineLinkKeyName: " + (n.LineLinkKeyName ?? "(unset)"));
                    sb.AppendLine("  - HeaderPrimaryKey: " + ((n.HeaderPrimaryKey != null && n.HeaderPrimaryKey.Count > 0) ? string.Join(", ", n.HeaderPrimaryKey) : "(unset)"));
                    sb.AppendLine("  - LinePrimaryKey: " + ((n.LinePrimaryKey != null && n.LinePrimaryKey.Count > 0) ? string.Join(", ", n.LinePrimaryKey) : "(unset)"));
                    sb.AppendLine("  - HeaderCols: " + ((n.HeaderColumns != null) ? n.HeaderColumns.Count.ToString() : "0"));
                    sb.AppendLine("  - LineCols: " + ((n.LineColumns != null) ? n.LineColumns.Count.ToString() : "0"));
                }

                if (t.Columns.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("| Source | Target | Action | Expr | Part |");
                    sb.AppendLine("|---|---|---|---|---|");
                    foreach (var c in t.Columns)
                    {
                        var expr = string.IsNullOrWhiteSpace(c.Expression) ? "" : c.Expression.Replace("\r", " ").Replace("\n", " ");
                        sb.AppendLine("| " + (c.Source ?? "") + " | " + (c.Target ?? "") + " | " + (c.Action ?? "") + " | " + expr + " | " + (c.Part ?? "") + " |");
                    }
                }

                if (t.Warnings.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("### Warnings");
                    foreach (var w in t.Warnings) sb.AppendLine("- " + w);
                }
            }

            return sb.ToString();
        }

        // DTOs for the review output
        private class PlanReview
        {
            public DateTime GeneratedAtUtc { get; set; }
            public string MigrationsDir { get; set; }
            public List<PlanReviewTable> Tables { get; set; } = new List<PlanReviewTable>();
            public List<string> Errors { get; set; } = new List<string>();
        }

        private class PlanReviewTable
        {
            public string SourceTable { get; set; }
            public string Classification { get; set; }
            public string TargetTable { get; set; }
            public bool PreserveIdsOnInsert { get; set; }
            public bool Reviewed { get; set; }
            public bool Ignore { get; set; }
            public PlanReviewNormalize Normalize { get; set; }
            public List<PlanReviewColumn> Columns { get; set; }
            public List<string> Warnings { get; set; }
        }

        private class PlanReviewNormalize
        {
            public string HeaderTable { get; set; }
            public string LineTable { get; set; }
            public string NewHeaderKeyName { get; set; }
            public string NewLineKeyName { get; set; }
            public string LineLinkKeyName { get; set; }
            public List<string> HeaderPrimaryKey { get; set; }
            public List<string> LinePrimaryKey { get; set; }
            public List<string> HeaderColumns { get; set; }
            public List<string> LineColumns { get; set; }
            public List<string> OldCompositeKey { get; set; }
        }

        private class PlanReviewColumn
        {
            public string Source { get; set; }
            public string Target { get; set; }
            public string Action { get; set; }
            public string Expression { get; set; }
            public string Part { get; set; } // H/L/""
        }
    }
}
