using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal static class PreflightValidator
    {
        public static int Run(string migrationsDir, out string logPath)
        {
            logPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs",
                "Preflight_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            var sb = new StringBuilder();
            int errors = 0, warnings = 0;

            try
            {
                var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                if (!Directory.Exists(accessSchemaDir))
                {
                    File.WriteAllText(logPath, "Access schema folder not found: " + accessSchemaDir);
                    return 2;
                }

                var files = Directory.GetFiles(accessSchemaDir, "*.schema.json")
                    .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                foreach (var file in files)
                {
                    TableSchema s = null;
                    try { s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(file)); }
                    catch (Exception ex)
                    {
                        sb.AppendLine("ERROR: Failed to parse " + file + ": " + ex.Message);
                        errors++; continue;
                    }
                    if (s == null) { sb.AppendLine("ERROR: Null schema " + file); errors++; continue; }

                    var plan = s.Plan ?? new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                    var actions = plan.ColumnActions ?? new List<ColumnPlan>();
                    var name = s.SourceTable ?? Path.GetFileNameWithoutExtension(file);

                    if (plan.Ignore)
                    {
                        sb.AppendLine($"INFO: {name} is set to Ignore. Skipping.");
                        continue;
                    }

                    // Common: no bogus sources, no empty targets (unless Drop/Compute)
                    foreach (var a in actions)
                    {
                        var act = (a.Action ?? "Copy").Trim();
                        if (string.IsNullOrWhiteSpace(a.Source) || string.Equals(a.Source, "n/a", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine($"WARN [{name}]: Column action with empty or 'n/a' source. Source='{a.Source ?? ""}', Target='{a.Target ?? ""}', Action='{act}'");
                            warnings++;
                        }
                        var targetRequired = !string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase) &&
                                             !string.Equals(act, "Compute", StringComparison.OrdinalIgnoreCase);
                        if (targetRequired && string.IsNullOrWhiteSpace(a.Target))
                        {
                            sb.AppendLine($"WARN [{name}]: Action '{act}' requires a target for source '{a.Source}'.");
                            warnings++;
                        }
                    }

                    var cls = plan.Classification ?? "Copy";
                    var isNormalize = string.Equals(cls, "Normalize", StringComparison.OrdinalIgnoreCase);

                    if (!isNormalize)
                    {
                        if (string.IsNullOrWhiteSpace(plan.TargetTable))
                        {
                            sb.AppendLine($"ERROR [{name}]: TargetTable is empty.");
                            errors++;
                        }
                        continue;
                    }

                    // Normalize-specific checks
                    var n = plan.Normalize ?? new NormalizePlan();
                    var headerTbl = n.HeaderTable ?? "";
                    var lineTbl = n.LineTable ?? "";

                    if (string.IsNullOrWhiteSpace(headerTbl))
                    {
                        sb.AppendLine($"ERROR [{name}]: Normalize.HeaderTable is empty.");
                        errors++;
                    }
                    if (string.IsNullOrWhiteSpace(lineTbl))
                    {
                        sb.AppendLine($"ERROR [{name}]: Normalize.LineTable is empty.");
                        errors++;
                    }

                    var headerCols = new HashSet<string>(n.HeaderColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var lineCols = new HashSet<string>(n.LineColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

                    if (headerCols.Count == 0)
                    {
                        sb.AppendLine($"WARN [{name}]: No HeaderColumns assigned.");
                        warnings++;
                    }
                    if (lineCols.Count == 0)
                    {
                        sb.AppendLine($"WARN [{name}]: No LineColumns assigned.");
                        warnings++;
                    }

                    var headerPk = n.HeaderPrimaryKey ?? new List<string>();
                    var linePk = n.LinePrimaryKey ?? new List<string>();

                    if (string.IsNullOrWhiteSpace(n.NewHeaderKeyName) && headerPk.Count == 0)
                    {
                        sb.AppendLine($"WARN [{name}]: Header key not defined (neither NewHeaderKeyName nor HeaderPrimaryKey).");
                        warnings++;
                    }
                    if (string.IsNullOrWhiteSpace(n.NewLineKeyName) && linePk.Count == 0)
                    {
                        sb.AppendLine($"WARN [{name}]: Line key not defined (neither NewLineKeyName nor LinePrimaryKey).");
                        warnings++;
                    }
                    if (string.IsNullOrWhiteSpace(n.LineLinkKeyName))
                    {
                        sb.AppendLine($"WARN [{name}]: LineLinkKeyName (lines → header FK) is empty.");
                        warnings++;
                    }

                    // Ensure every assigned header/line source has an actionable ColumnPlan (not Drop)
                    var actionsBySource = actions.ToDictionary(x => x.Source ?? "", StringComparer.OrdinalIgnoreCase);
                    foreach (var src in headerCols)
                    {
                        if (!actionsBySource.TryGetValue(src, out var a) || string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine($"WARN [{name}]: HeaderColumns includes '{src}' but its ColumnAction is missing or Drop.");
                            warnings++;
                        }
                    }
                    foreach (var src in lineCols)
                    {
                        if (!actionsBySource.TryGetValue(src, out var a) || string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine($"WARN [{name}]: LineColumns includes '{src}' but its ColumnAction is missing or Drop.");
                            warnings++;
                        }
                    }
                }

                // Also run the relations validator (PK/FK/IDENTITY vs effective plan)
                string relationsLog;
                var rcRelations = PlanRelationsValidator.Validate(migrationsDir, out relationsLog);
                sb.AppendLine();
                sb.AppendLine($"Relations validator rc={rcRelations}. Log: {relationsLog}");
                if (rcRelations != 0) warnings++;

                // Done
                sb.Insert(0, $"Preflight finished. errors={errors}, warnings={warnings}{Environment.NewLine}");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath) ?? migrationsDir);
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);

                Console.WriteLine($"Preflight finished. errors={errors}, warnings={warnings}");
                Console.WriteLine("Log: " + logPath);
                return errors > 0 ? 1 : (warnings > 0 ? 1 : 0);
            }
            catch (Exception ex)
            {
                File.WriteAllText(logPath, "Preflight failed: " + ex);
                return 1;
            }
        }
    }
}