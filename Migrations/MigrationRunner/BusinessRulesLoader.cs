using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal static class BusinessRulesLoader
    {
        public static int LoadAndValidate(string migrationsDir, out string logPath, out BusinessRulesIndex rules)
        {
            logPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs",
                "BusinessRules_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            rules = null;

            var sb = new StringBuilder();
            int warnings = 0;

            try
            {
                var rulesPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "BusinessRules.json");
                var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath) ?? migrationsDir);

                if (!File.Exists(rulesPath))
                {
                    File.WriteAllText(logPath, "BusinessRules.json not found: " + rulesPath);
                    return 2;
                }
                if (!Directory.Exists(accessSchemaDir))
                {
                    File.WriteAllText(logPath, "Access schema folder not found: " + accessSchemaDir);
                    return 2;
                }

                try
                {
                    rules = JsonConvert.DeserializeObject<BusinessRulesIndex>(File.ReadAllText(rulesPath));
                }
                catch (Exception ex)
                {
                    File.WriteAllText(logPath, "Failed to parse BusinessRules.json: " + ex.Message);
                    return 2;
                }
                if (rules == null || rules.Tables == null || rules.Tables.Count == 0)
                {
                    File.WriteAllText(logPath, "BusinessRules.json loaded but empty.");
                    return 1;
                }

                // Build effective plan views per source table
                var plans = new Dictionary<string, PlanView>(StringComparer.OrdinalIgnoreCase);
                foreach (var file in Directory.EnumerateFiles(accessSchemaDir, "*.schema.json")
                                              .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase)))
                {
                    TableSchema s = null;
                    try { s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(file)); } catch { continue; }
                    if (s == null) continue;

                    var plan = s.Plan ?? new TablePlan { ColumnActions = new List<ColumnPlan>() };
                    var actions = plan.ColumnActions ?? new List<ColumnPlan>();
                    var isNormalize = string.Equals(plan.Classification ?? "Copy", "Normalize", StringComparison.OrdinalIgnoreCase);
                    var pv = new PlanView();

                    if (!isNormalize)
                    {
                        foreach (var a in actions)
                        {
                            if (string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase)) continue;
                            var src = a.Source ?? "";
                            var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;
                            pv.HeaderSources.Add(src);
                            pv.HeaderTargets.Add(tgt);
                            if (!string.IsNullOrWhiteSpace(src)) pv.SrcToTgt[src] = tgt;
                        }
                        pv.HeaderTable = plan.TargetTable ?? s.SourceTable ?? "";
                    }
                    else
                    {
                        var n = plan.Normalize ?? new NormalizePlan();
                        pv.HeaderTable = n.HeaderTable ?? plan.TargetTable ?? s.SourceTable ?? "";
                        pv.LineTable = n.LineTable ?? "";

                        var headerSet = new HashSet<string>(n.HeaderColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                        var lineSet = new HashSet<string>(n.LineColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

                        foreach (var a in actions)
                        {
                            if (string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase)) continue;
                            var src = a.Source ?? "";
                            var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;
                            if (headerSet.Contains(src))
                            {
                                pv.HeaderSources.Add(src);
                                pv.HeaderTargets.Add(tgt);
                            }
                            if (lineSet.Contains(src))
                            {
                                pv.LineSources.Add(src);
                                pv.LineTargets.Add(tgt);
                            }
                            if (!string.IsNullOrWhiteSpace(src)) pv.SrcToTgt[src] = tgt;
                        }

                        if (!string.IsNullOrWhiteSpace(n.NewHeaderKeyName)) pv.HeaderTargets.Add(n.NewHeaderKeyName);
                        if (!string.IsNullOrWhiteSpace(n.NewLineKeyName)) pv.LineTargets.Add(n.NewLineKeyName);
                        if (!string.IsNullOrWhiteSpace(n.LineLinkKeyName)) pv.LineTargets.Add(n.LineLinkKeyName);
                    }

                    plans[s.SourceTable ?? ""] = pv;
                }

                foreach (var kv in rules.Tables)
                {
                    var sourceTable = kv.Key ?? "";
                    var tr = kv.Value ?? new TableBusinessRules();

                    if (!plans.TryGetValue(sourceTable, out var pv))
                    {
                        sb.AppendLine("WARN: No plan found for table '" + sourceTable + "' to validate business rules.");
                        warnings++;
                        continue;
                    }

                    Func<string, bool> HeaderHas = col =>
                    {
                        if (string.IsNullOrWhiteSpace(col)) return false;
                        if (pv.HeaderTargets.Contains(col)) return true;        // direct target name
                        if (pv.HeaderSources.Contains(col)) return true;        // source name
                        if (pv.SrcToTgt.TryGetValue(col, out var tgt) && pv.HeaderTargets.Contains(tgt)) return true; // source→target
                        return false;
                    };
                    Func<string, bool> LineHas = col =>
                    {
                        if (string.IsNullOrWhiteSpace(col)) return false;
                        if (pv.LineTargets.Contains(col)) return true;
                        if (pv.LineSources.Contains(col)) return true;
                        if (pv.SrcToTgt.TryGetValue(col, out var tgt) && pv.LineTargets.Contains(tgt)) return true;
                        return false;
                    };

                    // CompositeKey rules
                    foreach (var r in tr.CompositeKeyRules ?? new List<CompositeKeyConditionalRule>())
                    {
                        var part = (r.Part ?? "Header").Trim();
                        bool ok(string c) => string.Equals(part, "Line", StringComparison.OrdinalIgnoreCase) ? LineHas(c) : HeaderHas(c);

                        foreach (var col in r.TrueKey ?? new List<string>())
                            if (!ok(col)) { sb.AppendLine($"WARN [{sourceTable}] CompositeKey TrueKey column '{col}' not found in {part} (by source/target)."); warnings++; }

                        foreach (var col in r.FalseKey ?? new List<string>())
                            if (!ok(col)) { sb.AppendLine($"WARN [{sourceTable}] CompositeKey FalseKey column '{col}' not found in {part} (by source/target)."); warnings++; }

                        if (string.IsNullOrWhiteSpace(r.Condition))
                        {
                            sb.AppendLine($"WARN [{sourceTable}] CompositeKey rule has empty Condition.");
                            warnings++;
                        }
                    }

                    // Computed columns: validate target presence only as info
                    foreach (var r in tr.ComputedColumns ?? new List<ComputedColumnRule>())
                    {
                        var part = (r.Part ?? "Single").Trim();
                        var inHeader = !string.Equals(part, "L", StringComparison.OrdinalIgnoreCase);
                        var exists = inHeader ? pv.HeaderTargets.Contains(r.Target ?? "") : pv.LineTargets.Contains(r.Target ?? "");
                        if (string.IsNullOrWhiteSpace(r.Target))
                        {
                            sb.AppendLine($"WARN [{sourceTable}] Computed column has empty Target.");
                            warnings++;
                        }
                        else if (exists)
                        {
                            sb.AppendLine($"INFO [{sourceTable}] Computed column '{r.Target}' exists in plan targets; ETL will overwrite value.");
                        }
                        if (string.IsNullOrWhiteSpace(r.Expression))
                        {
                            sb.AppendLine($"WARN [{sourceTable}] Computed column '{r.Target}' has empty Expression.");
                            warnings++;
                        }
                    }

                    // Recurring grouping: group-by columns may also be source names
                    if (tr.RecurringGrouping != null)
                    {
                        foreach (var c in tr.RecurringGrouping.HeaderGroupBy ?? new List<string>())
                            if (!HeaderHas(c)) { sb.AppendLine($"WARN [{sourceTable}] Recurring.HeaderGroupBy column '{c}' not found in Header (by source/target)."); warnings++; }
                        foreach (var c in tr.RecurringGrouping.LineGroupBy ?? new List<string>())
                            if (!LineHas(c)) { sb.AppendLine($"WARN [{sourceTable}] Recurring.LineGroupBy column '{c}' not found in Line (by source/target)."); warnings++; }
                        foreach (var kvAgg in tr.RecurringGrouping.LineAggregations ?? new Dictionary<string, string>())
                            if (!LineHas(kvAgg.Key)) { sb.AppendLine($"WARN [{sourceTable}] Recurring aggregation column '{kvAgg.Key}' not found in Line (by source/target)."); warnings++; }
                    }
                }

                sb.Insert(0, $"Business rules validation finished. warnings={warnings}{Environment.NewLine}");
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return warnings > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(logPath, "Business rules validation failed: " + ex.Message);
                return 1;
            }
        }

        private sealed class PlanView
        {
            public string HeaderTable { get; set; } = "";
            public string LineTable { get; set; } = "";
            public HashSet<string> HeaderSources { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> LineSources { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> HeaderTargets { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> LineTargets { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, string> SrcToTgt { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}