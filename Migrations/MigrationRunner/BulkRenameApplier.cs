using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    class BulkRenameApplier
    {
        public static int Run(string migrationsDir, bool dryRun, out string logPath)
        {
            var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
            var rulesPath = Path.Combine(migrationsDir, "Metadata", "BulkRenameRules.json");
            var changesDir = Path.Combine(migrationsDir, "Metadata", "PlanChanges");
            Directory.CreateDirectory(changesDir);
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            logPath = Path.Combine(changesDir, $"{stamp}_BulkRenameLog.csv");

            if (!Directory.Exists(accessSchemaDir))
            {
                Console.Error.WriteLine("Access schema folder not found: " + accessSchemaDir);
                return 2;
            }
            if (!File.Exists(rulesPath))
            {
                Console.Error.WriteLine("Rules file not found: " + rulesPath);
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

            var rules = BulkRenameEngine.LoadRules(rulesPath);
            var sb = new StringBuilder();
            sb.AppendLine("SourceTable,Type,Column,OldValue,NewValue,Reason,File");

            int filesChanged = 0, tableRenames = 0, columnRenames = 0;

            foreach (var file in files)
            {
                var schema = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(file));
                if (schema == null) continue;

                var changes = BulkRenameEngine.ApplyToTable(schema, rules);
                if (changes.Count == 0) continue;

                foreach (var c in changes)
                {
                    if (string.Equals(c.Type, "Table", StringComparison.OrdinalIgnoreCase)) tableRenames++;
                    if (string.Equals(c.Type, "Column", StringComparison.OrdinalIgnoreCase)) columnRenames++;
                    sb.AppendLine(string.Join(",",
                        Csv(c.SourceTable), Csv(c.Type), Csv(c.Column), Csv(c.OldValue), Csv(c.NewValue), Csv(c.Reason), Csv(Path.GetFileName(file))));
                }

                if (!dryRun)
                {
                    File.WriteAllText(file, JsonConvert.SerializeObject(schema, Formatting.Indented));
                    filesChanged++;
                }
            }

            File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
            Console.WriteLine($"Bulk rename {(dryRun ? "dry-run" : "applied")}.");
            Console.WriteLine($"Files changed: {filesChanged}, Table renames: {tableRenames}, Column renames: {columnRenames}");
            Console.WriteLine("Log: " + logPath);
            return 0;
        }

        private static string Csv(string v)
        {
            if (v == null) return "";
            return (v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0) ? "\"" + v.Replace("\"", "\"\"") + "\"" : v;
        }
    }
}