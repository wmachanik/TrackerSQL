using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal static class PlanRelationsValidator
    {
        public static int Validate(string migrationsDir, out string logPath)
        {
            logPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs",
                "RelationsValidator_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            var sb = new StringBuilder();
            int warnings = 0, infos = 0, errors = 0;

            try
            {
                var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                var constraintsPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "PlanConstraints.json");

                Directory.CreateDirectory(Path.GetDirectoryName(logPath) ?? migrationsDir);

                if (!Directory.Exists(accessSchemaDir))
                {
                    File.WriteAllText(logPath, "Access schema folder not found: " + accessSchemaDir);
                    return 2;
                }
                if (!File.Exists(constraintsPath))
                {
                    File.WriteAllText(logPath, "Constraints file not found: " + constraintsPath + ". Run option 11 to extract constraints.");
                    return 2;
                }

                // Load constraints (FKs/PKs/Identity) from PlanConstraints.json
                ConstraintsIndex constraints;
                try
                {
                    constraints = JsonConvert.DeserializeObject<ConstraintsIndex>(File.ReadAllText(constraintsPath)) ?? new ConstraintsIndex();
                }
                catch (Exception ex)
                {
                    File.WriteAllText(logPath, "Failed to parse constraints: " + ex.Message);
                    return 2;
                }

                // Build effective target schema view from AccessSchema plans
                var targetTables = new Dictionary<string, TargetTableView>(StringComparer.OrdinalIgnoreCase);

                foreach (var file in Directory.EnumerateFiles(accessSchemaDir, "*.schema.json")
                                              .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase)))
                {
                    TableSchema s = null;
                    try { s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(file)); }
                    catch (Exception ex)
                    {
                        sb.AppendLine("WARN: Failed to parse " + file + ": " + ex.Message);
                        warnings++;
                        continue;
                    }
                    if (s == null) continue;

                    var plan = s.Plan ?? new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                    var actions = plan.ColumnActions ?? new List<ColumnPlan>();
                    var classification = plan.Classification ?? "Copy";
                    var isNormalize = string.Equals(classification, "Normalize", StringComparison.OrdinalIgnoreCase);
                    string singleTarget = plan.TargetTable ?? s.SourceTable ?? "";

                    if (!isNormalize)
                    {
                        var cols = actions.Where(a => !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                                          .Select(a => string.IsNullOrWhiteSpace(a.Target) ? (a.Source ?? "") : a.Target)
                                          .Where(t => !string.IsNullOrWhiteSpace(t))
                                          .Distinct(StringComparer.OrdinalIgnoreCase)
                                          .ToHashSet(StringComparer.OrdinalIgnoreCase);
                        if (!string.IsNullOrWhiteSpace(singleTarget))
                            GetOrAdd(targetTables, singleTarget).MergeColumns(cols);
                    }
                    else
                    {
                        var n = plan.Normalize ?? new NormalizePlan();
                        var headerSet = new HashSet<string>(n.HeaderColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                        var lineSet = new HashSet<string>(n.LineColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

                        // Header
                        if (!string.IsNullOrWhiteSpace(n.HeaderTable))
                        {
                            var headerTargets = actions
                                // Use the HashSet's comparer via Contains without passing a comparer argument
                                .Where(a => headerSet.Contains(a.Source ?? "") &&
                                            !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                                .Select(a => string.IsNullOrWhiteSpace(a.Target) ? (a.Source ?? "") : a.Target)
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            // Add synthetic NewHeaderKey if present
                            if (!string.IsNullOrWhiteSpace(n.NewHeaderKeyName))
                                headerTargets.Add(n.NewHeaderKeyName);

                            GetOrAdd(targetTables, n.HeaderTable).MergeColumns(headerTargets);
                        }

                        // Lines
                        if (!string.IsNullOrWhiteSpace(n.LineTable))
                        {
                            var lineTargets = actions
                                // Use the HashSet's comparer via Contains without passing a comparer argument
                                .Where(a => lineSet.Contains(a.Source ?? "") &&
                                            !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                                .Select(a => string.IsNullOrWhiteSpace(a.Target) ? (a.Source ?? "") : a.Target)
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            // Add synthetic NewLineKey and Link FK if present
                            if (!string.IsNullOrWhiteSpace(n.NewLineKeyName))
                                lineTargets.Add(n.NewLineKeyName);
                            if (!string.IsNullOrWhiteSpace(n.LineLinkKeyName))
                                lineTargets.Add(n.LineLinkKeyName);

                            GetOrAdd(targetTables, n.LineTable).MergeColumns(lineTargets);
                        }
                    }
                }

                // Build constraints lookup
                var ctByName = new Dictionary<string, ConstraintTable>(StringComparer.OrdinalIgnoreCase);
                foreach (var ct in constraints.Tables ?? new List<ConstraintTable>())
                    if (!string.IsNullOrWhiteSpace(ct.Table)) ctByName[ct.Table] = ct;

                // Validate per constraint table
                foreach (var kv in ctByName)
                {
                    var tblName = kv.Key;
                    var ct = kv.Value;
                    targetTables.TryGetValue(tblName, out var tv);

                    if (tv == null)
                    {
                        sb.AppendLine($"WARN: Constraints defined for table '{tblName}' but no target columns found in plan.");
                        warnings++;
                        continue;
                    }

                    // PK validation
                    foreach (var pkCol in ct.PrimaryKey ?? new List<string>())
                    {
                        if (!Exists(tv.Columns, pkCol))
                        {
                            var sim = Similar(tv.Columns, pkCol);
                            sb.AppendLine($"WARN [{tblName}]: PK column '{pkCol}' not found in plan targets. {(sim != null ? "Did you mean '" + sim + "'?'" : "")}");
                            warnings++;
                        }
                    }

                    // Identity validation
                    foreach (var idCol in ct.IdentityColumns ?? new List<string>())
                    {
                        if (!Exists(tv.Columns, idCol))
                        {
                            var sim = Similar(tv.Columns, idCol);
                            sb.AppendLine($"WARN [{tblName}]: Identity column '{idCol}' not found in plan targets. {(sim != null ? "Did you mean '" + sim + "'?'" : "")}");
                            warnings++;
                        }
                    }

                    // FK validation
                    foreach (var fk in ct.ForeignKeys ?? new List<ForeignKeyDef>())
                    {
                        if (string.IsNullOrWhiteSpace(fk.Column))
                        {
                            sb.AppendLine($"WARN [{tblName}]: FK has empty Column.");
                            warnings++;
                            continue;
                        }
                        if (!Exists(tv.Columns, fk.Column))
                        {
                            var sim = Similar(tv.Columns, fk.Column);
                            sb.AppendLine($"WARN [{tblName}]: FK column '{fk.Column}' not found in plan targets. {(sim != null ? "Did you mean '" + sim + "'?'" : "")}");
                            warnings++;
                        }

                        var refTbl = fk.RefTable ?? "";
                        if (string.IsNullOrWhiteSpace(refTbl))
                        {
                            sb.AppendLine($"WARN [{tblName}]: FK '{fk.Column}' missing RefTable.");
                            warnings++;
                            continue;
                        }
                        if (!ctByName.TryGetValue(refTbl, out var refCt))
                        {
                            if (!targetTables.ContainsKey(refTbl))
                            {
                                sb.AppendLine($"WARN [{tblName}]: FK '{fk.Column}' references unknown table '{refTbl}'.");
                                warnings++;
                                continue;
                            }
                            else
                            {
                                // No constraints for refTbl, but plan has it; log info
                                sb.AppendLine($"INFO: Referenced table '{refTbl}' not present in constraints; PK resolution may be inferred.");
                                infos++;
                            }
                        }

                        // Check referenced column
                        var refCols = targetTables.TryGetValue(refTbl, out var refTv) ? refTv.Columns : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var refCol = (fk.RefColumn ?? "").Trim();

                        if (!string.IsNullOrWhiteSpace(refCol))
                        {
                            if (!Exists(refCols, refCol))
                            {
                                var sim = Similar(refCols, refCol);
                                sb.AppendLine($"WARN [{tblName}]: FK '{fk.Column}' references '{refTbl}.{refCol}' but column not found. {(sim != null ? "Did you mean '" + sim + "'?'" : "")}");
                                warnings++;
                            }
                        }
                        else
                        {
                            // No explicit ref column -> infer from referenced PK
                            var pkRef = (refCt?.PrimaryKey ?? new List<string>()).FirstOrDefault() ?? "";
                            if (string.IsNullOrWhiteSpace(pkRef))
                            {
                                sb.AppendLine($"WARN [{tblName}]: FK '{fk.Column}' references '{refTbl}' with no RefColumn and no PK found to infer.");
                                warnings++;
                            }
                            else if (!Exists(refCols, pkRef))
                            {
                                var sim = Similar(refCols, pkRef);
                                sb.AppendLine($"WARN [{tblName}]: FK '{fk.Column}' would infer column '{pkRef}' from PK, but it is not found in plan targets. {(sim != null ? "Did you mean '" + sim + "'?'" : "")}");
                                warnings++;
                            }
                        }

                        // Naming consistency suggestion: ID suffix casing
                        var sugCol = SuggestIdCasing(fk.Column);
                        if (!string.Equals(sugCol, fk.Column, StringComparison.Ordinal))
                        {
                            if (Exists(tv.Columns, sugCol))
                            {
                                sb.AppendLine($"INFO [{tblName}]: FK column '{fk.Column}' casing differs; '{sugCol}' exists in plan.");
                                infos++;
                            }
                        }
                    }

                    // Detect mixed casing variants for "*Id" vs "*ID" only.
                    // Ignore non-ID columns (e.g., 'Area' vs 'AreaID') to avoid false positives.
                    var idCasingVariants = tv.Columns
                        .Where(c => c.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
                        .GroupBy(c => c.Substring(0, c.Length - 2), StringComparer.OrdinalIgnoreCase)
                        .Select(g => g.Select(x => x).Distinct(StringComparer.Ordinal).ToList())
                        .Where(list => list.Count > 1)
                        .ToList();

                    foreach (var variants in idCasingVariants)
                    {
                        sb.AppendLine($"WARN [{tblName}]: Inconsistent ID casing across columns: {string.Join(", ", variants.Select(x => "'" + x + "'"))}");
                        warnings++;
                    }
                }

                sb.Insert(0, $"Relations validation finished. errors={errors}, warnings={warnings}, info={infos}{Environment.NewLine}");
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return warnings > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(logPath, "Relations validation failed: " + ex.Message);
                return 1;
            }
        }

        private sealed class TargetTableView
        {
            public HashSet<string> Columns { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public void MergeColumns(IEnumerable<string> cols)
            {
                if (cols == null) return;
                foreach (var c in cols) if (!string.IsNullOrWhiteSpace(c)) Columns.Add(c.Trim());
            }
        }

        private static TargetTableView GetOrAdd(Dictionary<string, TargetTableView> map, string name)
        {
            if (!map.TryGetValue(name, out var v)) { v = new TargetTableView(); map[name] = v; }
            return v;
        }

        private static bool Exists(HashSet<string> set, string value)
        {
            return set.Contains(value ?? "");
        }

        private static string Similar(HashSet<string> set, string probe)
        {
            if (string.IsNullOrWhiteSpace(probe)) return null;
            // Prefer case-insensitive exact first
            var ci = set.FirstOrDefault(x => string.Equals(x, probe, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(ci)) return ci;
            // Then suggest ID casing variant
            var sug = SuggestIdCasing(probe);
            var hit = set.FirstOrDefault(x => string.Equals(x, sug, StringComparison.OrdinalIgnoreCase));
            return hit;
        }

        private static string SuggestIdCasing(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;
            if (name.EndsWith("Id", StringComparison.Ordinal)) return name.Substring(0, name.Length - 2) + "ID";
            return name;
        }
    }
}