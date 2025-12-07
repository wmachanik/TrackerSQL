using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MigrationRunner
{
    static class PlanCsvIO
    {
        public static string Export(string path, TableSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Source,Target,Action,Expression");
            var rows = schema?.Plan?.ColumnActions ?? new List<ColumnPlan>();
            foreach (var a in rows)
            {
                var action = string.IsNullOrWhiteSpace(a.Action) ? "Copy" : a.Action;
                sb.AppendLine(string.Join(",", Csv(a.Source), Csv(a.Target ?? a.Source), Csv(action), Csv(a.Expression ?? "")));
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        public static int Import(string path, TableSchema schema, out string message)
        {
            message = "";
            if (!File.Exists(path)) { message = "File not found."; return -1; }
            var text = File.ReadAllText(path);
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length <= 1) { message = "No data rows."; return 0; }

            int updated = 0;
            var header = ParseCsvLine(lines[0]);
            int idxSource = IndexOf(header, "Source");
            int idxTarget = IndexOf(header, "Target");
            int idxAction = IndexOf(header, "Action");
            int idxExpr = IndexOf(header, "Expression");

            if (idxSource < 0) { message = "Header must contain 'Source'."; return -1; }

            var actions = schema.Plan.ColumnActions ?? new List<ColumnPlan>();
            schema.Plan.ColumnActions = actions;

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var cols = ParseCsvLine(lines[i]);
                if (cols.Count == 0) continue;

                var src = Get(cols, idxSource);
                if (string.IsNullOrWhiteSpace(src)) continue;

                var tgt = idxTarget >= 0 ? Get(cols, idxTarget) : null;
                var act = idxAction >= 0 ? Get(cols, idxAction) : null;
                var expr = idxExpr >= 0 ? Get(cols, idxExpr) : null;

                var existing = actions.Find(a => string.Equals(a.Source, src, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    existing = new ColumnPlan { Source = src };
                    actions.Add(existing);
                }

                if (!string.IsNullOrWhiteSpace(tgt)) existing.Target = tgt;
                if (!string.IsNullOrWhiteSpace(act)) existing.Action = act;
                existing.Expression = expr;
                updated++;
            }

            message = $"Updated {updated} rows.";
            return updated;
        }

        private static int IndexOf(List<string> header, string name)
        {
            for (int i = 0; i < header.Count; i++)
                if (string.Equals(header[i], name, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        private static string Get(List<string> cols, int idx) => idx >= 0 && idx < cols.Count ? cols[idx] : null;

        private static List<string> ParseCsvLine(string line)
        {
            var res = new List<string>();
            if (line == null) return res;
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else sb.Append(ch);
                }
                else
                {
                    if (ch == ',') { res.Add(sb.ToString()); sb.Clear(); }
                    else if (ch == '"') inQuotes = true;
                    else sb.Append(ch);
                }
            }
            res.Add(sb.ToString());
            return res;
        }

        private static string Csv(string v)
        {
            if (string.IsNullOrEmpty(v)) return "";
            if (v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                return "\"" + v.Replace("\"", "\"\"") + "\"";
            return v;
        }
    }
}