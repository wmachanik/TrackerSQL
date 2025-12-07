using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    // Partial extends the existing PlanSummaryExporter with the Human Review CSV
    partial class PlanSummaryExporter
    {
        public static int ExportHumanReviewCsv(string migrationsDir, out string csvPath)
        {
            csvPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "TableMigrationReport.csv");
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

                // Load BusinessRules once (best-effort)
                BusinessRulesIndex rules = null;
                try
                {
                    var rulesPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "BusinessRules.json");
                    if (File.Exists(rulesPath))
                        rules = JsonConvert.DeserializeObject<BusinessRulesIndex>(File.ReadAllText(rulesPath));
                }
                catch { /* ignore */ }

                // Load Constraints (PK/FK/Identity) once (best-effort)
                ConstraintsIndex constraints = null;
                var ctByName = new Dictionary<string, ConstraintTable>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    var constraintsPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "PlanConstraints.json");
                    if (File.Exists(constraintsPath))
                    {
                        constraints = JsonConvert.DeserializeObject<ConstraintsIndex>(File.ReadAllText(constraintsPath)) ?? new ConstraintsIndex();
                        foreach (var ct in (constraints.Tables ?? new List<ConstraintTable>()))
                            if (!string.IsNullOrWhiteSpace(ct.Table)) ctByName[ct.Table] = ct;
                    }
                }
                catch { /* ignore */ }

                Func<string, ConstraintTable> getCt = table =>
                {
                    if (string.IsNullOrWhiteSpace(table)) return null;
                    ctByName.TryGetValue(table, out var ct);
                    return ct;
                };
                Func<ForeignKeyDef, string, string> renderFk = (fk, headerTbl) =>
                {
                    if (fk == null) return null;
                    // Normalize link FK -> compact "FK (Header)"
                    if (!string.IsNullOrWhiteSpace(headerTbl) &&
                        !string.IsNullOrWhiteSpace(fk.RefTable) &&
                        fk.RefTable.Equals(headerTbl, StringComparison.OrdinalIgnoreCase))
                    {
                        return "FK (Header)";
                    }
                    var target = fk.RefTable ?? "";
                    if (!string.IsNullOrWhiteSpace(fk.RefColumn)) target += (target.Length > 0 ? "." : "") + fk.RefColumn;
                    return string.IsNullOrWhiteSpace(target) ? "FK" : "FK (" + target + ")";
                };
                Func<string, string, string, string, string> applyKey = (table, col, fallback, headerTblForNormalize) =>
                {
                    if (string.IsNullOrWhiteSpace(col) || string.IsNullOrWhiteSpace(table)) return fallback;
                    var ct = getCt(table);
                    if (ct == null) return fallback;

                    // If constraints define PK on this column, show PK (wins over fallback)
                    if ((ct.PrimaryKey ?? new List<string>()).Any(x => string.Equals(x, col, StringComparison.OrdinalIgnoreCase)))
                        return "PK";

                    // If constraints define FK on this column, show FK
                    var fk = (ct.ForeignKeys ?? new List<ForeignKeyDef>())
                                .FirstOrDefault(x => string.Equals(x.Column ?? "", col ?? "", StringComparison.OrdinalIgnoreCase));
                    if (fk != null)
                        return renderFk(fk, headerTblForNormalize);

                    return fallback;
                };
                Func<string, string, string, string> applyAuto = (table, col, fallbackAuto) =>
                {
                    if (string.Equals(fallbackAuto ?? "", "Yes", StringComparison.OrdinalIgnoreCase)) return fallbackAuto;
                    if (string.IsNullOrWhiteSpace(col) || string.IsNullOrWhiteSpace(table)) return fallbackAuto;
                    var ct = getCt(table);
                    if (ct == null) return fallbackAuto;
                    var isId = (ct.IdentityColumns ?? new List<string>()).Any(x => string.Equals(x, col, StringComparison.OrdinalIgnoreCase));
                    return isId ? "Yes" : fallbackAuto;
                };

                var schemas = new List<TableSchema>();
                foreach (var f in files)
                {
                    try
                    {
                        var s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(f));
                        if (s == null) continue;
                        if (s.Plan == null) s.Plan = new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                        if (s.Plan.ColumnActions == null) s.Plan.ColumnActions = new List<ColumnPlan>();
                        schemas.Add(s);
                    }
                    catch
                    {
                        // skip malformed schema
                    }
                }

                var sb = new StringBuilder();

                // Helpers
                const int W = 24; // bumped because we add NotNull columns
                Action<string[]> write = cells =>
                {
                    var list = new List<string>(cells ?? Array.Empty<string>());
                    while (list.Count < W) list.Add(string.Empty);
                    sb.AppendLine(string.Join(",", list.Select(Csv)));
                };
                Action divider = () => write(new[] { "-------------------------------/-------------------------------" });
                Func<ColumnSchema, string> beforeType = c => c?.RecommendedSqlType ?? c?.DotNetType ?? "";
                Func<TableSchema, Dictionary<string, ColumnSchema>> mapCols = s =>
                {
                    var dict = new Dictionary<string, ColumnSchema>(StringComparer.OrdinalIgnoreCase);
                    foreach (var c in (s.Columns ?? new List<ColumnSchema>()))
                        if (!string.IsNullOrWhiteSpace(c.SourceName)) dict[c.SourceName] = c;
                    return dict;
                };
                Action<TableSchema> writeBusinessRules = ts =>
                {
                    try
                    {
                        if (rules?.Tables == null) return;
                        var src = ts?.SourceTable ?? "";
                        if (string.IsNullOrWhiteSpace(src)) return;
                        if (!rules.Tables.TryGetValue(src, out var tbl) || tbl == null) return;

                        // CompositeKey rules (write one line per rule)
                        if (tbl.CompositeKeyRules != null)
                        {
                            foreach (var r in tbl.CompositeKeyRules)
                            {
                                var partRaw = r?.Part ?? "Header";
                                var part = partRaw.StartsWith("H", StringComparison.OrdinalIgnoreCase) ? "Header"
                                          : partRaw.StartsWith("L", StringComparison.OrdinalIgnoreCase) ? "Line"
                                          : partRaw;
                                var cond = r?.Condition ?? "";
                                var trueKey = string.Join(", ", r?.TrueKey ?? new List<string>());
                                var falseKey = string.Join(", ", r?.FalseKey ?? new List<string>());

                                var msg = $"CompositeKey({part}): IF {cond} THEN ({trueKey}) ELSE ({falseKey})";
                                write(new[] { "BusinessRules:", msg });
                            }
                        }

                        // Computed columns (write one line per compute)
                        if (tbl.ComputedColumns != null)
                        {
                            foreach (var c in tbl.ComputedColumns)
                            {
                                var partRaw = c?.Part ?? "Single";
                                var part = partRaw.StartsWith("H", StringComparison.OrdinalIgnoreCase) ? "Header"
                                          : partRaw.StartsWith("L", StringComparison.OrdinalIgnoreCase) ? "Line"
                                          : "Single";
                                var target = c?.Target ?? "";
                                var expr = c?.Expression ?? "";
                                // keep CSV compact
                                var exprShort = expr.Length > 160 ? (expr.Substring(0, 157) + "...") : expr;

                                var msg = $"Compute({part}): Target={target} Expr={exprShort}";
                                write(new[] { "BusinessRules:", msg });
                            }
                        }

                        // Recurring grouping (single summary line)
                        if (tbl.RecurringGrouping != null)
                        {
                            var rg = tbl.RecurringGrouping;
                            var headerBy = string.Join(", ", rg.HeaderGroupBy ?? new List<string>());
                            var lineBy = string.Join(", ", rg.LineGroupBy ?? new List<string>());
                            var aggs = (rg.LineAggregations != null && rg.LineAggregations.Count > 0)
                                ? string.Join(", ", rg.LineAggregations.Select(kv => kv.Key + "=" + kv.Value))
                                : "";
                            var filter = string.IsNullOrWhiteSpace(rg.Filter) ? "" : (" Filter=" + rg.Filter);

                            var msg = $"RecurringGrouping: HeaderBy=[{headerBy}] LineBy=[{lineBy}] Aggs=[{aggs}]{filter}";
                            write(new[] { "BusinessRules:", msg });
                        }
                    }
                    catch
                    {
                        // best-effort output; do not break export on BR formatting
                    }
                };

                // Header
                write(new[] { "Table Migration report: " + DateTime.Now.ToString("dd MMM yyyy") });
                write(new[] { "================================" });

                // Ignored tables section
                var ignored = schemas.Where(s => s.Plan.Ignore).OrderBy(s => s.SourceTable, StringComparer.OrdinalIgnoreCase).ToList();
                if (ignored.Count > 0)
                {
                    write(new[] { "Table", "Before", "After", "Action" });
                    foreach (var s in ignored)
                        write(new[] { "", s.SourceTable ?? "", "n/a", "Ignore" });
                    write(new[] { "Rows:", "None ignored" });
                    divider();
                }

                // Non-normalize (Copy/Rename/Refactor) excluding ignored
                var simple = schemas.Where(s => !s.Plan.Ignore && !string.Equals(s.Plan.Classification ?? "Copy", "Normalize", StringComparison.OrdinalIgnoreCase))
                                    .OrderBy(s => s.SourceTable, StringComparer.OrdinalIgnoreCase)
                                    .ToList();
                foreach (var s in simple)
                {
                    var cols = mapCols(s);

                    // Build a stable order index from Access column ordinals
                    var orderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    int __ord = 0;
                    foreach (var c in (s.Columns ?? new List<ColumnSchema>()).OrderBy(c => c.Ordinal)) orderIndex[c.SourceName] = __ord++;

                    // Helper for CI PK check (Access PK list + ColumnSchema.IsKey)
                    Func<string, bool> isPkSource = name =>
                        (s.PrimaryKey != null && s.PrimaryKey.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase))) ||
                        (cols.TryGetValue(name, out var ccs) && (ccs?.IsKey ?? false));

                    var actionKind = (s.Plan.Classification ?? "Copy");
                    var singleTargetTbl = s.Plan.TargetTable ?? s.SourceTable ?? "";
                    write(new[] { "Table", "Before", "After", "Action" });
                    write(new[] { "", s.SourceTable ?? "", singleTargetTbl, actionKind });

                    // BusinessRules summary line (informational)
                    writeBusinessRules(s);

                    // Rows header (added After NotNull)
                    write(new[]
                    {
                        "Rows:",
                        "Before Col name","Before Type","Before Key","Before Auto",
                        "--/--",
                        "After Col Name","After Type","After Key","After Auto","After NotNull","Presserve ID","Action","Source Col"
                    });

                    foreach (var a in (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                                    .OrderBy(x => orderIndex.TryGetValue(x.Source ?? "", out var pos) ? pos : int.MaxValue)
                                    .ThenBy(x => x.Source, StringComparer.OrdinalIgnoreCase))
                    {
                        var src = a.Source ?? "";
                        cols.TryGetValue(src, out var cs);
                        var bType = beforeType(cs);
                        var bKey = isPkSource(src) ? "PK" : "No";
                        var bAuto = (cs?.IsAutoIncrement ?? false) ? "Yes" : "No";

                        var act = string.IsNullOrWhiteSpace(a.Action) ? "Copy" : a.Action.Trim();
                        if (string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            write(new[]
                            {
                                "",
                                src, bType, bKey, bAuto,
                                "--/--",
                                "", "", "", "", "", "n/a", "Drop", src
                            });
                            continue;
                        }

                        var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;
                        var aType = bType;
                        var aKey = bKey;
                        var aAuto = bAuto;

                        // default NotNull from Access column and PK/Identity
                        var aNotNull = (!string.Equals(aKey, "No", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(aKey)) ||
                                       string.Equals(aAuto, "Yes", StringComparison.OrdinalIgnoreCase) ||
                                       (cs != null && !cs.AllowDBNull)
                                       ? "Yes" : "No";

                        var preserve = (s.Plan.PreserveIdsOnInsert && ((cs?.IsKey ?? false) || (cs?.IsAutoIncrement ?? false))) ? "Yes" : "n/a";

                        // Overlay with constraints (PK/FK/Identity) for the target table/column
                        if (!string.IsNullOrWhiteSpace(tgt) && !string.IsNullOrWhiteSpace(singleTargetTbl))
                        {
                            aKey = applyKey(singleTargetTbl, tgt, aKey, null);
                            aAuto = applyAuto(singleTargetTbl, tgt, aAuto);
                            if (string.Equals(aKey, "PK", StringComparison.OrdinalIgnoreCase) || string.Equals(aAuto, "Yes", StringComparison.OrdinalIgnoreCase))
                                aNotNull = "Yes";
                        }

                        write(new[]
                        {
                            "",
                            src, bType, bKey, bAuto,
                            "--/--",
                            tgt, aType, aKey, aAuto, aNotNull, preserve, act, src
                        });
                    }

                    divider();
                }

                // Normalize (excluding ignored)
                var norms = schemas.Where(s => !s.Plan.Ignore && string.Equals(s.Plan.Classification ?? "Copy", "Normalize", StringComparison.OrdinalIgnoreCase))
                                   .OrderBy(s => s.SourceTable, StringComparer.OrdinalIgnoreCase)
                                   .ToList();
                foreach (var s in norms)
                {
                    var cols = mapCols(s);
                    var n = s.Plan.Normalize ?? new NormalizePlan();

                    var headerTbl = n.HeaderTable ?? "";
                    var lineTbl = n.LineTable ?? "";
                    var headerSet = new HashSet<string>(n.HeaderColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var lineSet = new HashSet<string>(n.LineColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var headerPk = new HashSet<string>(n.HeaderPrimaryKey ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var linePk = new HashSet<string>(n.LinePrimaryKey ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var newHeaderKey = n.NewHeaderKeyName ?? "";
                    var newLineKey = n.NewLineKeyName ?? "";
                    var linkFk = n.LineLinkKeyName ?? "";

                    var orderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    int __ord = 0;
                    foreach (var c in (s.Columns ?? new List<ColumnSchema>()).OrderBy(c => c.Ordinal)) orderIndex[c.SourceName] = __ord++;

                    var mappedLineTargets = (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                        .Where(a => lineSet.Contains(a.Source ?? "", StringComparer.OrdinalIgnoreCase) &&
                                    !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                        .Select(a => string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target)
                        .ToList();
                    var headerKeyNamePref = !string.IsNullOrWhiteSpace(newHeaderKey)
                                              ? newHeaderKey
                                              : (n.HeaderPrimaryKey?.FirstOrDefault() ?? "");
                    var effectiveLinkFk = !string.IsNullOrWhiteSpace(linkFk) ? linkFk
                                              : (!string.IsNullOrWhiteSpace(headerKeyNamePref) &&
                                                 mappedLineTargets.Any(t => string.Equals(t, headerKeyNamePref, StringComparison.OrdinalIgnoreCase)))
                                                    ? headerKeyNamePref
                                                    : "";

                    Func<string, bool> isPkSource = name =>
                        (s.PrimaryKey != null && s.PrimaryKey.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase))) ||
                        ((s.Columns ?? new List<ColumnSchema>()).Any(c => string.Equals(c.SourceName ?? "", name ?? "", StringComparison.OrdinalIgnoreCase) && c.IsKey));

                    write(new[] { "Table:", "Before", "After Header Tbl", "After Lines Tbl", "Action" });
                    write(new[] { "=====", s.SourceTable ?? "", headerTbl, lineTbl, "Normalise" });

                    // BusinessRules summary line (informational)
                    writeBusinessRules(s);

                    write(new[]
                    {
                        "Rows:",
                        "Before Col name","Before Type","Before Key","Before Auto",
                        "--/--",
                        "After Header Col Name","After Header Type","After Header Key","After Header Auto","After Header NotNull","Presserve ID","Action","Source Col",
                        "--/--",
                        "After Lines Col Name","After Lines Type","After Lines Key","After Lines Auto","After Lines NotNull","Presserve ID","Action","Source Col"
                    });

                    foreach (var a in (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                                    .OrderBy(x => orderIndex.TryGetValue(x.Source ?? "", out var pos) ? pos : int.MaxValue)
                                    .ThenBy(x => x.Source, StringComparer.OrdinalIgnoreCase))
                    {
                        var src = a.Source ?? "";
                        cols.TryGetValue(src, out var cs);
                        var bType = beforeType(cs);
                        var bKey = isPkSource(src) ? "PK" : "No";
                        var bAuto = (cs?.IsAutoIncrement ?? false) ? "Yes" : "No";

                        var act = string.IsNullOrWhiteSpace(a.Action) ? "Copy" : a.Action.Trim();
                        var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;

                        var inHeader = headerSet.Contains(src);
                        var inLine = lineSet.Contains(src);

                        string ahCol = "", ahType = "", ahKey = "", ahAuto = "", ahNN = "", ahPreserve = "", ahAction = "", ahSource = "";
                        if (inHeader && !string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            ahCol = tgt;
                            ahType = bType;
                            var isPkH = (headerPk.Contains(tgt) || (!string.IsNullOrWhiteSpace(newHeaderKey) && string.Equals(tgt, newHeaderKey, StringComparison.OrdinalIgnoreCase)));
                            ahKey = isPkH ? "PK" : "No";
                            ahAuto = (isPkH && string.Equals(tgt, newHeaderKey, StringComparison.OrdinalIgnoreCase)) ? "Yes" : "No";
                            ahNN = (isPkH || string.Equals(ahAuto, "Yes", StringComparison.OrdinalIgnoreCase) || (cs != null && !cs.AllowDBNull)) ? "Yes" : "No";
                            ahPreserve = n.PreserveHeaderIds ? "Yes" : "No";
                            ahAction = act;
                            ahSource = src;

                            // Overlay header constraints
                            if (!string.IsNullOrWhiteSpace(ahCol) && !string.IsNullOrWhiteSpace(headerTbl))
                            {
                                ahKey = applyKey(headerTbl, ahCol, ahKey, null);
                                ahAuto = applyAuto(headerTbl, ahCol, ahAuto);
                                if (string.Equals(ahKey, "PK", StringComparison.OrdinalIgnoreCase) || string.Equals(ahAuto, "Yes", StringComparison.OrdinalIgnoreCase))
                                    ahNN = "Yes";
                            }
                        }

                        string alCol = "", alType = "", alKey = "", alAuto = "", alNN = "", alPreserve = "", alAction = "", alSource = "";
                        if (inLine && !string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            alCol = tgt;
                            alType = bType;
                            var isPkL = (linePk.Contains(tgt) || (!string.IsNullOrWhiteSpace(newLineKey) && string.Equals(tgt, newLineKey, StringComparison.OrdinalIgnoreCase)));
                            var isFkL = !string.IsNullOrWhiteSpace(effectiveLinkFk) && string.Equals(tgt, effectiveLinkFk, StringComparison.OrdinalIgnoreCase);
                            alKey = isPkL ? "PK" : (isFkL ? "FK (Header)" : "No");
                            alAuto = (isPkL && string.Equals(tgt, newLineKey, StringComparison.OrdinalIgnoreCase)) ? "Yes" : "No";
                            alNN = (isPkL || string.Equals(alAuto, "Yes", StringComparison.OrdinalIgnoreCase) || (cs != null && !cs.AllowDBNull)) ? "Yes" : "No";
                            alPreserve = n.PreserveLineIds ? "Yes" : "No";
                            alAction = act;
                            alSource = src;

                            // Overlay line constraints (render header link FK compactly)
                            if (!string.IsNullOrWhiteSpace(alCol) && !string.IsNullOrWhiteSpace(lineTbl))
                            {
                                var over = applyKey(lineTbl, alCol, alKey, headerTbl);
                                // Preserve "FK (Header)" if constraints point to header table
                                alKey = over;
                                alAuto = applyAuto(lineTbl, alCol, alAuto);
                                if (string.Equals(alKey, "PK", StringComparison.OrdinalIgnoreCase) || string.Equals(alAuto, "Yes", StringComparison.OrdinalIgnoreCase))
                                    alNN = "Yes";
                            }
                        }

                        if (inHeader || inLine || string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            write(new[]
                            {
                                "",
                                src, bType, bKey, bAuto,
                                "--/--",
                                ahCol, ahType, ahKey, ahAuto, ahNN, string.IsNullOrEmpty(ahCol) ? "" : ahPreserve, ahAction, ahSource,
                                "--/--",
                                alCol, alType, alKey, alAuto, alNN, string.IsNullOrEmpty(alCol) ? "" : alPreserve, alAction, alSource
                            });
                        }
                    }

                    // Synthetic key/link lines unchanged
                    Func<string, bool> existsTarget = target =>
                        (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                            .Any(x =>
                            {
                                var act = string.IsNullOrWhiteSpace(x.Action) ? "Copy" : x.Action;
                                if (string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase)) return false;
                                var tgt = string.IsNullOrWhiteSpace(x.Target) ? x.Source : x.Target;
                                return string.Equals(tgt ?? "", target ?? "", StringComparison.OrdinalIgnoreCase);
                            });

                    if (!string.IsNullOrWhiteSpace(newHeaderKey) && !existsTarget(newHeaderKey))
                    {
                        write(new[]
                        {
                            "",
                            "","", "", "",
                            "--/--",
                            newHeaderKey, "", "PK", "Yes", "Yes", n.PreserveHeaderIds ? "Yes" : "No", "New", "n/a",
                            "--/--",
                            "", "", "", "", "", "", "", ""
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(newLineKey) && !existsTarget(newLineKey))
                    {
                        write(new[]
                        {
                            "",
                            "","", "", "",
                            "--/--",
                            "", "", "", "", "", "", "", "",
                            "--/--",
                            newLineKey, "", "PK", "Yes", "Yes", n.PreserveLineIds ? "Yes" : "No", "New", "n/a"
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(effectiveLinkFk) && !existsTarget(effectiveLinkFk))
                    {
                        write(new[]
                        {
                            "",
                            "","", "", "",
                            "--/--",
                            "", "", "", "", "", "", "", "",
                            "--/--",
                            effectiveLinkFk, "", "FK (Header)", "", "No", "", "LinkFK", "n/a"
                        });
                    }

                    divider();
                }

                Directory.CreateDirectory(Path.GetDirectoryName(csvPath) ?? migrationsDir);
                File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
                Console.WriteLine("Human review CSV written: " + csvPath);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ExportHumanReviewCsv failed: " + ex.Message);
                csvPath = null;
                return 1;
            }
        }

        // Helper shared by both normalize sections: does any action already emit this target?
        private static bool ExistsTarget(TableSchema s, string target)
        {
            return (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                .Any(x =>
                {
                    var act = string.IsNullOrWhiteSpace(x.Action) ? "Copy" : x.Action;
                    if (string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase)) return false;
                    var tgt = string.IsNullOrWhiteSpace(x.Target) ? x.Source : x.Target;
                    return string.Equals(tgt ?? "", target ?? "", StringComparison.OrdinalIgnoreCase);
                });
        }
    }
}