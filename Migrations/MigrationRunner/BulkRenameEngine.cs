using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    class BulkRenameEngine
    {
        public static List<RenameChange> ApplyToTable(TableSchema schema, RenameRules rules)
        {
            var changes = new List<RenameChange>();
            var cmp = rules != null && rules.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (schema == null) return changes;

            if (schema.Plan == null)
            {
                schema.Plan = new TablePlan
                {
                    Classification = "Copy",
                    TargetTable = schema.SourceTable,
                    PreserveIdsOnInsert = false,
                    Reviewed = false,
                    ColumnActions = (schema.Columns ?? new List<ColumnSchema>())
                        .Select(c => new ColumnPlan { Source = c.SourceName, Target = c.SourceName, Action = "Copy" })
                        .ToList()
                };
            }

            // Table rename
            if (rules != null && rules.TableRenames != null && !string.IsNullOrWhiteSpace(schema.SourceTable))
            {
                string newName;
                if (rules.TableRenames.TryGetValue(schema.SourceTable, out newName))
                {
                    var oldTarget = schema.Plan.TargetTable ?? schema.SourceTable;
                    if (!string.Equals(oldTarget, newName, cmp))
                    {
                        schema.Plan.TargetTable = newName;
                        if (string.Equals(schema.Plan.Classification ?? "Copy", "Copy", cmp))
                            schema.Plan.Classification = "Rename";
                        changes.Add(new RenameChange
                        {
                            SourceTable = schema.SourceTable,
                            Type = "Table",
                            OldValue = oldTarget,
                            NewValue = newName,
                            Reason = "TableRenames rule"
                        });
                    }
                }
            }

            // Column renames
            var actions = schema.Plan.ColumnActions ?? new List<ColumnPlan>();
            foreach (var ca in actions)
            {
                var act = (ca.Action ?? "Copy").Trim();
                if (act.Equals("Drop", StringComparison.OrdinalIgnoreCase) ||
                    act.Equals("Compute", StringComparison.OrdinalIgnoreCase))
                    continue;

                var oldTarget = string.IsNullOrWhiteSpace(ca.Target) ? ca.Source : ca.Target;
                var newTarget = oldTarget;

                // Exact first
                if (rules != null && rules.ExactColumnRenames != null && !string.IsNullOrWhiteSpace(ca.Source))
                {
                    string exact;
                    if (rules.ExactColumnRenames.TryGetValue(ca.Source, out exact))
                    {
                        newTarget = exact;
                    }
                }

                // Then token replacements
                if (rules != null && rules.ColumnTokenReplacements != null)
                {
                    foreach (var kv in rules.ColumnTokenReplacements)
                        newTarget = ReplaceToken(newTarget, kv.Key, kv.Value, rules.CaseSensitive);
                }

                if (!string.Equals(oldTarget, newTarget, cmp))
                {
                    ca.Target = newTarget;
                    if (!act.Equals("Rename", StringComparison.OrdinalIgnoreCase))
                        ca.Action = "Rename";

                    changes.Add(new RenameChange
                    {
                        SourceTable = schema.SourceTable,
                        Column = ca.Source,
                        Type = "Column",
                        OldValue = oldTarget,
                        NewValue = newTarget,
                        Reason = "Exact/Token rule"
                    });
                }
            }

            return changes;
        }

        public static RenameRules LoadRules(string rulesPath)
        {
            if (!File.Exists(rulesPath)) return new RenameRules();
            return JsonConvert.DeserializeObject<RenameRules>(File.ReadAllText(rulesPath)) ?? new RenameRules();
        }

        public static void WriteChangesLog(string migrationsDir, string sourceTable, IEnumerable<RenameChange> changes)
        {
            var dir = Path.Combine(migrationsDir, "Metadata", "PlanChanges");
            Directory.CreateDirectory(dir);
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var outPath = Path.Combine(dir, $"{stamp}_{sourceTable}_BulkRenameLog.csv");
            var sb = new StringBuilder();
            sb.AppendLine("SourceTable,Type,Column,OldValue,NewValue,Reason");
            foreach (var c in changes)
                sb.AppendLine(string.Join(",", Csv(c.SourceTable), Csv(c.Type), Csv(c.Column), Csv(c.OldValue), Csv(c.NewValue), Csv(c.Reason)));
            File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);
            Console.WriteLine("Rename log: " + outPath);
        }

        private static string ReplaceToken(string input, string token, string replacement, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(token)) return input;
            if (caseSensitive) return input.Replace(token, replacement);

            var idx = 0;
            while (true)
            {
                idx = input.IndexOf(token, idx, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) break;
                input = input.Substring(0, idx) + replacement + input.Substring(idx + token.Length);
                idx += replacement.Length;
            }
            return input;
        }

        private static string Csv(string v)
        {
            if (v == null) return "";
            if (v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                return "\"" + v.Replace("\"", "\"\"") + "\"";
            return v;
        }
    }

    class RenameRules
    {
        public bool CaseSensitive { get; set; }
        public Dictionary<string, string> TableRenames { get; set; }
        public Dictionary<string, string> ExactColumnRenames { get; set; }
        public Dictionary<string, string> ColumnTokenReplacements { get; set; }
    }

    class RenameChange
    {
        public string SourceTable { get; set; }
        public string Type { get; set; }     // Table | Column
        public string Column { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
    }
}