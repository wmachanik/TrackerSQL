using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal static class PlanIntegrity
    {
        public static int CheckAndNormalize(string migrationsDir, out string logPath)
        {
            logPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs",
                "Integrity_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            var sb = new StringBuilder();
            int changedFiles = 0, warnings = 0;

            try
            {
                var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath) ?? migrationsDir);
                if (!Directory.Exists(accessSchemaDir))
                {
                    File.WriteAllText(logPath, "Access schema folder not found: " + accessSchemaDir);
                    return 2;
                }

                var files = Directory.GetFiles(accessSchemaDir, "*.schema.json")
                                     .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                                     .ToArray();

                foreach (var f in files)
                {
                    TableSchema s = null;
                    try { s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(f)); }
                    catch (Exception ex) { sb.AppendLine("WARN parse: " + f + " -> " + ex.Message); warnings++; continue; }
                    if (s == null) { sb.AppendLine("WARN null schema: " + f); warnings++; continue; }
                    if (s.Plan == null) s.Plan = new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                    if (s.Plan.ColumnActions == null) s.Plan.ColumnActions = new List<ColumnPlan>();

                    bool changed = false;
                    var isNormalize = string.Equals(s.Plan.Classification ?? "Copy", "Normalize", StringComparison.OrdinalIgnoreCase);
                    var n = s.Plan.Normalize;

                    if (isNormalize && n != null)
                    {
                        // 1) Keep TargetTable == HeaderTable for Normalize
                        if (!string.IsNullOrWhiteSpace(n.HeaderTable) &&
                            !string.Equals(s.Plan.TargetTable ?? "", n.HeaderTable ?? "", StringComparison.Ordinal))
                        {
                            sb.AppendLine($"FIX [{s.SourceTable}] TargetTable: '{s.Plan.TargetTable ?? ""}' -> '{n.HeaderTable ?? ""}'");
                            s.Plan.TargetTable = n.HeaderTable;
                            changed = true;
                        }

                        // 2) Ensure Link FK name exists or can be inferred from plan
                        var headerKeyPref = !string.IsNullOrWhiteSpace(n.NewHeaderKeyName)
                                                ? n.NewHeaderKeyName
                                                : (n.HeaderPrimaryKey != null && n.HeaderPrimaryKey.Count > 0 ? n.HeaderPrimaryKey[0] : "");
                        var headerSet = new HashSet<string>((n.HeaderColumns ?? new List<string>()), StringComparer.OrdinalIgnoreCase);
                        var lineSet = new HashSet<string>((n.LineColumns ?? new List<string>()), StringComparer.OrdinalIgnoreCase);

                        // Collect line targets actually emitted
                        var lineTargets = (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                            .Where(a => lineSet.Contains(a.Source ?? "", StringComparer.OrdinalIgnoreCase) &&
                                        !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                            .Select(a => string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        if (string.IsNullOrWhiteSpace(n.LineLinkKeyName))
                        {
                            if (!string.IsNullOrWhiteSpace(headerKeyPref) &&
                                lineTargets.Any(t => string.Equals(t, headerKeyPref, StringComparison.OrdinalIgnoreCase)))
                            {
                                n.LineLinkKeyName = headerKeyPref;
                                sb.AppendLine($"INF [{s.SourceTable}] Inferred LineLinkKeyName = '{n.LineLinkKeyName}'");
                                changed = true;
                            }
                            else
                            {
                                sb.AppendLine($"WARN [{s.SourceTable}] Missing LineLinkKeyName and cannot infer from Line targets.");
                                warnings++;
                            }
                        }
                        else
                        {
                            // Warn if link FK target not present in line targets
                            if (!lineTargets.Any(t => string.Equals(t ?? "", n.LineLinkKeyName ?? "", StringComparison.OrdinalIgnoreCase)))
                            {
                                sb.AppendLine($"WARN [{s.SourceTable}] LineLinkKeyName='{n.LineLinkKeyName}' not found among Lines targets.");
                                warnings++;
                            }
                        }

                        // 3) PK sanity: warn if PK columns are not mapped to their respective parts
                        if (n.HeaderPrimaryKey != null)
                        {
                            var headerTargets = (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                                .Where(a => headerSet.Contains(a.Source ?? "", StringComparer.OrdinalIgnoreCase) &&
                                            !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                                .Select(a => string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target)
                                .ToList();
                            foreach (var pk in n.HeaderPrimaryKey)
                            {
                                if (!headerTargets.Any(t => string.Equals(t ?? "", pk ?? "", StringComparison.OrdinalIgnoreCase)) &&
                                    !string.Equals(pk ?? "", n.NewHeaderKeyName ?? "", StringComparison.OrdinalIgnoreCase))
                                {
                                    sb.AppendLine($"WARN [{s.SourceTable}] Header PK '{pk}' not found in Header targets.");
                                    warnings++;
                                }
                            }
                        }
                        if (n.LinePrimaryKey != null)
                        {
                            var lTargets = lineTargets;
                            foreach (var pk in n.LinePrimaryKey)
                            {
                                if (!lTargets.Any(t => string.Equals(t ?? "", pk ?? "", StringComparison.OrdinalIgnoreCase)) &&
                                    !string.Equals(pk ?? "", n.NewLineKeyName ?? "", StringComparison.OrdinalIgnoreCase))
                                {
                                    sb.AppendLine($"WARN [{s.SourceTable}] Line PK '{pk}' not found in Line targets.");
                                    warnings++;
                                }
                            }
                        }
                    }

                    if (changed)
                    {
                        try
                        {
                            File.WriteAllText(f, JsonConvert.SerializeObject(s, Formatting.Indented), Encoding.UTF8);
                            changedFiles++;
                        }
                        catch (Exception ex) { sb.AppendLine("ERROR save: " + f + " -> " + ex.Message); }
                    }
                }

                sb.Insert(0, $"Integrity: changed={changedFiles}, warnings={warnings}{Environment.NewLine}");
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return warnings > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(logPath, "Integrity check failed: " + ex.Message);
                return 1;
            }
        }
    }
}