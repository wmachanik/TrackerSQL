using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MigrationRunner
{
    class SchemaPlanEditor
    {
        private enum ViewFilter { All, Reviewed, Unreviewed, Ignored }

        public static int Run(string migrationsDir)
        {
            var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
            if (!Directory.Exists(accessSchemaDir))
            {
                Console.Error.WriteLine("Access schema folder not found: " + accessSchemaDir);
                Console.Error.WriteLine("Run option 1 first to export the Access schema.");
                return 2;
            }

            var filesAll = Directory.GetFiles(accessSchemaDir, "*.schema.json")
                .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => Path.GetFileName(f))
                .ToList();
            if (filesAll.Count == 0)
            {
                Console.Error.WriteLine("No *.schema.json files found. Run option 1 first.");
                return 2;
            }

            string filterText = "";
            int page = 0;
            const int pageSize = 30;
            bool threeColumnMode = true; // default to 3-col layout
            var viewFilter = ViewFilter.Unreviewed; // default view

            while (true)
            {
                var filtered = filesAll.Where(f =>
                {
                    var name = GetDisplayName(f);
                    if (!string.IsNullOrWhiteSpace(filterText) &&
                        name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) < 0)
                        return false;

                    switch (viewFilter)
                    {
                        case ViewFilter.Reviewed: return GetReviewed(f) && !GetIgnored(f);
                        case ViewFilter.Unreviewed: return !GetReviewed(f) && !GetIgnored(f);
                        case ViewFilter.Ignored: return GetIgnored(f);
                        default: return true;
                    }
                }).ToList();

                List<string> currentList;

                Console.WriteLine();
                if (!threeColumnMode)
                {
                    var totalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)pageSize));
                    if (page >= totalPages) page = totalPages - 1;
                    if (page < 0) page = 0;

                    currentList = filtered.Skip(page * pageSize).Take(pageSize).ToList();

                    Console.WriteLine("Tables (Page {0}/{1}) Filter: \"{2}\" View: {3} (default=Unreviewed)",
                        page + 1, totalPages, filterText, viewFilter);
                    Console.WriteLine("Commands: n=next, p=prev, f <text>=filter, a=all, vr=reviewed, vu=unreviewed, vi=ignored, m=3-cols,");
                    Console.WriteLine("          i <#>=ignore, r <#>=reviewed, d <#>=delete schema, <#>=open, 0=back");

                    for (int i = 0; i < currentList.Count; i++)
                    {
                        var name = GetDisplayName(currentList[i]);
                        var status = GetStatus(currentList[i]);
                        var key = GetStatusKey(status);
                        WriteStatusLine(i + 1, name, key);
                    }
                }
                else
                {
                    currentList = filtered; // 3-col shows full filtered list

                    Console.WriteLine("Tables (3 columns) Filter: \"{0}\" View: {1} (default=Unreviewed)", filterText, viewFilter);
                    Console.WriteLine("Commands: f <text>=filter, a=all, vr=reviewed, vu=unreviewed, vi=ignored, m=paged,");
                    Console.WriteLine("          i <#>=ignore, r <#>=reviewed, d <#>=delete schema, <#>=open, 0=back");

                    var cols = 3;
                    var rows = (int)Math.Ceiling(currentList.Count / (double)cols);
                    var colWidth = Math.Max(28, currentList.Select(f => GetDisplayName(f).Length).DefaultIfEmpty(0).Max() + 8);
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            int idx = r + (c * rows);
                            if (idx >= currentList.Count)
                            {
                                Console.Write(new string(' ', colWidth));
                                continue;
                            }
                            var name = GetDisplayName(currentList[idx]);
                            var status = GetStatus(currentList[idx]);
                            var key = GetStatusKey(status); // I/P/D
                            var label = string.Format("{0,2}) {1} [{2}]", idx + 1, name, key);
                            WriteStatusInline(label, key, colWidth);
                        }
                        Console.WriteLine();
                    }
                }

                Console.Write("Choice or command: ");
                var raw = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(raw)) continue;

                if (raw == "0") return 0;
                if (!threeColumnMode && string.Equals(raw, "n", StringComparison.OrdinalIgnoreCase)) { page++; continue; }
                if (!threeColumnMode && string.Equals(raw, "p", StringComparison.OrdinalIgnoreCase)) { page--; continue; }
                if (raw.StartsWith("f ", StringComparison.OrdinalIgnoreCase)) { filterText = raw.Substring(2).Trim(); page = 0; continue; }
                if (string.Equals(raw, "a", StringComparison.OrdinalIgnoreCase)) { filterText = ""; viewFilter = ViewFilter.All; page = 0; continue; }
                if (string.Equals(raw, "vr", StringComparison.OrdinalIgnoreCase)) { viewFilter = ViewFilter.Reviewed; page = 0; continue; }
                if (string.Equals(raw, "vu", StringComparison.OrdinalIgnoreCase)) { viewFilter = ViewFilter.Unreviewed; page = 0; continue; }
                if (string.Equals(raw, "vi", StringComparison.OrdinalIgnoreCase)) { viewFilter = ViewFilter.Ignored; page = 0; continue; }
                if (string.Equals(raw, "m", StringComparison.OrdinalIgnoreCase)) { threeColumnMode = !threeColumnMode; page = 0; continue; }

                // Quick commands: i <n>, r <n>, d <n>
                if (TryParseCommandWithIndex(raw, out var cmd, out var idx1))
                {
                    if (idx1 < 1 || idx1 > currentList.Count)
                    {
                        Console.WriteLine("Invalid index.");
                        continue;
                    }
                    var file = currentList[idx1 - 1];
                    if (string.Equals(cmd, "i", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ToggleIgnore(file, true))
                            Console.WriteLine("Ignore toggled.");
                        continue;
                    }
                    if (string.Equals(cmd, "r", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ToggleReviewed(file, true))
                            Console.WriteLine("Reviewed toggled.");
                        continue;
                    }
                    if (string.Equals(cmd, "d", StringComparison.OrdinalIgnoreCase))
                    {
                        var name = Path.GetFileName(file);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Delete schema file {0}? (y/n): ", name);
                        Console.ResetColor();
                        var ans = Console.ReadLine();
                        if (!string.IsNullOrEmpty(ans) && (ans.Equals("y", StringComparison.OrdinalIgnoreCase) || ans.Equals("yes", StringComparison.OrdinalIgnoreCase)))
                        {
                            try
                            {
                                File.Delete(file);
                                filesAll.Remove(file);
                                Console.WriteLine("Deleted " + name);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Delete failed: " + ex.Message);
                            }
                        }
                        continue;
                    }
                }

                // Open by number
                int sel;
                if (int.TryParse(raw, out sel) && sel >= 1 && sel <= currentList.Count)
                {
                    var file = currentList[sel - 1];
                    EditFile(migrationsDir, file);
                    continue;
                }

                Console.WriteLine("Invalid selection/command.");
            }
        }

        private static bool TryParseCommandWithIndex(string input, out string cmd, out int index)
        {
            cmd = null;
            index = -1;
            var parts = input.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && parts[0].Length == 1 && int.TryParse(parts[1], out index))
            {
                cmd = parts[0];
                return true;
            }
            return false;
        }

        private static bool ToggleReviewed(string schemaFile, bool save)
        {
            try
            {
                var s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(schemaFile));
                if (s == null) return false;
                if (s.Plan == null) s.Plan = new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, PreserveIdsOnInsert = false, Reviewed = false, Ignore = false, ColumnActions = new List<ColumnPlan>() };
                s.Plan.Reviewed = !s.Plan.Reviewed;
                if (save) File.WriteAllText(schemaFile, JsonConvert.SerializeObject(s, Formatting.Indented));
                return true;
            }
            catch { return false; }
        }

        private static bool ToggleIgnore(string schemaFile, bool save)
        {
            try
            {
                var s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(schemaFile));
                if (s == null) return false;
                if (s.Plan == null) s.Plan = new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, PreserveIdsOnInsert = false, Reviewed = false, Ignore = false, ColumnActions = new List<ColumnPlan>() };
                s.Plan.Ignore = !s.Plan.Ignore;
                if (save) File.WriteAllText(schemaFile, JsonConvert.SerializeObject(s, Formatting.Indented));
                return true;
            }
            catch { return false; }
        }

        private static bool GetReviewed(string schemaFile)
        {
            try
            {
                var s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(schemaFile));
                return s?.Plan?.Reviewed == true;
            }
            catch { return false; }
        }

        private static bool GetIgnored(string schemaFile)
        {
            try
            {
                var s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(schemaFile));
                return s?.Plan?.Ignore == true;
            }
            catch { return false; }
        }

        private static string GetStatus(string schemaFile)
        {
            try
            {
                var s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(schemaFile));
                if (s?.Plan == null) return "Pending";
                if (s.Plan.Ignore) return "Ignored";
                var actions = s.Plan.ColumnActions ?? new List<ColumnPlan>();
                var emptyTargets = actions.Any(a =>
                    !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(a.Action ?? "Copy", "Compute", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(a.Target));
                return s.Plan.Reviewed && !emptyTargets ? "Done" : "Pending";
            }
            catch { return "Pending"; }
        }

        private static string GetStatusKey(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "ignored": return "I";
                case "done": return "D";
                default: return "P";
            }
        }

        private static void WriteStatusLine(int index, string name, string statusKey)
        {
            Console.Write("  {0,2}) {1}  [", index, name);
            SetStatusColorByKey(statusKey);
            Console.Write(statusKey);
            Console.ResetColor();
            Console.WriteLine("]");
        }

        private static void WriteStatusInline(string label, string statusKey, int width)
        {
            var open = label.LastIndexOf('[');
            if (open >= 0)
            {
                Console.Write(label.Substring(0, open + 1));
                SetStatusColorByKey(statusKey);
                Console.Write(statusKey);
                Console.ResetColor();
                var rest = label.Substring(open + 1 + statusKey.Length);
                Console.Write(rest);
            }
            else
            {
                Console.Write(label);
            }
            var remaining = width - label.Length;
            if (remaining > 0) Console.Write(new string(' ', remaining));
        }

        private static void SetStatusColorByKey(string key)
        {
            switch ((key ?? "").ToUpperInvariant())
            {
                case "D": Console.ForegroundColor = ConsoleColor.Green; break;     // Done
                case "P": Console.ForegroundColor = ConsoleColor.Yellow; break;    // Pending
                case "I": Console.ForegroundColor = ConsoleColor.DarkGray; break;  // Ignored
                default: Console.ResetColor(); break;
            }
        }

        private static string GetDisplayName(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath); // removes .json
            if (name.EndsWith(".schema", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - ".schema".Length);
            return name;
        }

        private static void EditFile(string migrationsDir, string filePath)
        {
            Console.WriteLine();
            Console.WriteLine("Editing: {0}", Path.GetFileName(filePath));

            TableSchema schema;
            try
            {
                var json = File.ReadAllText(filePath);
                schema = JsonConvert.DeserializeObject<TableSchema>(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to read schema. " + ex.Message);
                return;
            }
            if (schema == null)
            {
                Console.Error.WriteLine("Invalid schema JSON.");
                return;
            }

            if (schema.Plan == null)
            {
                schema.Plan = new TablePlan
                {
                    Classification = "Copy",
                    TargetTable = schema.SourceTable,
                    PreserveIdsOnInsert = false,
                    Reviewed = false,
                    Ignore = false,
                    ColumnActions = (schema.Columns ?? new List<ColumnSchema>())
                        .Select(c => new ColumnPlan { Source = c.SourceName, Target = c.SourceName, Action = "Copy" })
                        .ToList()
                };
            }
            if (schema.Plan.ColumnActions == null && schema.Columns != null)
            {
                schema.Plan.ColumnActions = schema.Columns
                    .Select(c => new ColumnPlan { Source = c.SourceName, Target = c.SourceName, Action = "Copy" })
                    .ToList();
            }

            // local init for Normalize plan
            void EnsureNormalizePlan()
            {
                if (schema.Plan.Normalize == null)
                {
                    schema.Plan.Normalize = new NormalizePlan
                    {
                        HeaderTable = "",
                        LineTable = "",
                        NewHeaderKeyName = "",
                        NewLineKeyName = "",
                        LineLinkKeyName = ""
                    };
                }
            }

            var changed = false;

            while (true)
            {
                Console.WriteLine();
                if (changed) { Console.ForegroundColor = ConsoleColor.Yellow; }
                Console.WriteLine("Current plan:");
                Console.ResetColor();

                Console.Write("  SourceTable:    ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(schema.SourceTable);
                Console.ResetColor();

                Console.Write("  TargetTable:    ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(schema.Plan.TargetTable);
                Console.ResetColor();

                Console.Write("  Classification: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(schema.Plan.Classification ?? "Copy");
                Console.ResetColor();

                Console.Write("  Preserve IDs:   ");
                Console.ForegroundColor = schema.Plan.PreserveIdsOnInsert ? ConsoleColor.Green : ConsoleColor.DarkYellow;
                Console.WriteLine(schema.Plan.PreserveIdsOnInsert ? "Yes" : "No");
                Console.ResetColor();

                Console.Write("  Reviewed:       ");
                Console.ForegroundColor = schema.Plan.Reviewed ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.WriteLine(schema.Plan.Reviewed ? "Yes" : "No");
                Console.ResetColor();

                Console.Write("  Ignore:         ");
                Console.ForegroundColor = schema.Plan.Ignore ? ConsoleColor.DarkGray : ConsoleColor.Green;
                Console.WriteLine(schema.Plan.Ignore ? "Yes" : "No");
                Console.ResetColor();

                var isNormalize = string.Equals(schema.Plan.Classification, "Normalize", StringComparison.OrdinalIgnoreCase);
                if (isNormalize)
                {
                    EnsureNormalizePlan();
                    Console.WriteLine("  Normalize:");
                    Console.WriteLine("    HeaderTable:      {0}", string.IsNullOrWhiteSpace(schema.Plan.Normalize.HeaderTable) ? "(unset)" : schema.Plan.Normalize.HeaderTable);
                    Console.WriteLine("    LineTable:        {0}", string.IsNullOrWhiteSpace(schema.Plan.Normalize.LineTable) ? "(unset)" : schema.Plan.Normalize.LineTable);
                    Console.WriteLine("    OldKey:           {0}", (schema.Plan.Normalize.OldCompositeKey ?? new List<string>()).Count == 0 ? "(unset)" : string.Join(", ", schema.Plan.Normalize.OldCompositeKey));
                    Console.WriteLine("    New Header Key:   {0}", string.IsNullOrWhiteSpace(schema.Plan.Normalize.NewHeaderKeyName) ? "(unset)" : schema.Plan.Normalize.NewHeaderKeyName);
                    Console.WriteLine("    New Line Key:     {0}", string.IsNullOrWhiteSpace(schema.Plan.Normalize.NewLineKeyName) ? "(unset)" : schema.Plan.Normalize.NewLineKeyName);
                    Console.WriteLine("    Line Link FK:     {0}", string.IsNullOrWhiteSpace(schema.Plan.Normalize.LineLinkKeyName) ? "(unset)" : schema.Plan.Normalize.LineLinkKeyName);
                    Console.WriteLine("    Header PK:        {0}", (schema.Plan.Normalize.HeaderPrimaryKey ?? new List<string>()).Count == 0 ? "(unset)" : string.Join(", ", schema.Plan.Normalize.HeaderPrimaryKey));
                    Console.WriteLine("    Line PK:          {0}", (schema.Plan.Normalize.LinePrimaryKey ?? new List<string>()).Count == 0 ? "(unset)" : string.Join(", ", schema.Plan.Normalize.LinePrimaryKey));
                    Console.WriteLine("    HeaderCols:       {0}", (schema.Plan.Normalize.HeaderColumns ?? new List<string>()).Count);
                    Console.WriteLine("    LineCols:         {0}", (schema.Plan.Normalize.LineColumns ?? new List<string>()).Count);
                }

                Console.WriteLine("  Columns:");
                if (schema.Plan.ColumnActions != null)
                {
                    for (int i = 0; i < schema.Plan.ColumnActions.Count; i++)
                    {
                        var ca = schema.Plan.ColumnActions[i];
                        var act = string.IsNullOrWhiteSpace(ca.Action) ? "Copy" : ca.Action;

                        if (isNormalize && schema.Plan.Normalize != null)
                        {
                            if (schema.Plan.Normalize.HeaderColumns.Contains(ca.Source, StringComparer.OrdinalIgnoreCase))
                                Console.ForegroundColor = ConsoleColor.DarkCyan;         // mark header
                            else if (schema.Plan.Normalize.LineColumns.Contains(ca.Source, StringComparer.OrdinalIgnoreCase))
                                Console.ForegroundColor = ConsoleColor.DarkYellow;       // mark line
                            else
                                Console.ResetColor();
                        }
                        else
                        {
                            switch (act.ToLowerInvariant())
                            {
                                case "rename": Console.ForegroundColor = ConsoleColor.Cyan; break;
                                case "drop": Console.ForegroundColor = ConsoleColor.Red; break;
                                case "compute": Console.ForegroundColor = ConsoleColor.Magenta; break;
                                default: Console.ResetColor(); break;
                            }
                        }

                        var targetShown = string.IsNullOrWhiteSpace(ca.Target) ? "(null)" : ca.Target;
                        var tag = "";
                        if (isNormalize && schema.Plan.Normalize != null)
                        {
                            if (schema.Plan.Normalize.HeaderColumns.Contains(ca.Source, StringComparer.OrdinalIgnoreCase)) tag = " [H]";
                            if (schema.Plan.Normalize.LineColumns.Contains(ca.Source, StringComparer.OrdinalIgnoreCase)) tag = " [L]";
                        }

                        Console.WriteLine("    {0,2}) {1} -> {2}  [{3}]{4}{5}",
                            i + 1,
                            ca.Source,
                            targetShown,
                            act,
                            string.IsNullOrWhiteSpace(ca.Expression) ? "" : (" expr: " + ca.Expression),
                            tag);
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Actions:");
                Console.WriteLine("  C) Set classification (C/R/N/F)   [was 1]");
                Console.WriteLine("  T) Set target table name           [was 2]");
                Console.WriteLine("  V) Toggle preserve IDs             [was 3]");
                Console.WriteLine("  E) Edit a column                   [was 4]");
                Console.WriteLine("  S) Save                            [was 5]");
                Console.WriteLine("  R) Toggle reviewed                 [was 6]");
                Console.WriteLine("  B) Apply bulk rename rules         [was 7]");
                Console.WriteLine("  X) Export plan to CSV              [was 8]");
                Console.WriteLine("  M) Import plan from CSV            [was 9]");
                Console.WriteLine("  I) Toggle ignore (do not migrate)");
                if (isNormalize)
                {
                    Console.WriteLine("  ---- Normalize ----");
                    Console.WriteLine("  H) Set header table name");
                    Console.WriteLine("  L) Set line table name");
                    Console.WriteLine("  O) Set Old composite key (source cols)");
                    Console.WriteLine("  P) Set Primary Keys (Header/Line) (target cols)");
                    Console.WriteLine("  F) Set Line link FK (in Lines)");
                    Console.WriteLine("  K) Set new surrogate key names (Header/Line)");
                    Console.WriteLine("  h <#> assign to Header, l <#> assign to Lines, u <#> unassign");
                }
                Console.WriteLine("  0) Back (prompt to save if changed)");
                Console.Write("Choice: ");
                var ch = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ch)) continue;
                var upper = ch.Trim().ToUpperInvariant();

                if (upper == "0")
                {
                    if (changed && Confirm("Save changes before exiting? (y/n): "))
                    {
                        if (Save(filePath, schema)) Console.WriteLine("Saved.");
                    }
                    return;
                }
                if (upper == "C" || ch == "1")
                {
                    Console.Write("Enter classification [C/R/N/F or Copy/Rename/Normalize/Refactor]: ");
                    var val = Console.ReadLine();
                    var mapped = MapClassification(val);
                    if (!string.IsNullOrWhiteSpace(mapped))
                    {
                        schema.Plan.Classification = mapped;
                        if (string.Equals(mapped, "Normalize", StringComparison.OrdinalIgnoreCase))
                            EnsureNormalizePlan();
                        changed = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid classification.");
                    }
                }
                else if (upper == "T" || ch == "2")
                {
                    var current = schema.Plan.TargetTable;
                    var val = PromptString("Enter target table name", current);
                    if (val != current)
                    {
                        schema.Plan.TargetTable = val;
                        changed = true;
                    }
                }
                else if (upper == "V" || ch == "3")
                {
                    schema.Plan.PreserveIdsOnInsert = !schema.Plan.PreserveIdsOnInsert;
                    changed = true;
                }
                else if (upper == "E" || ch == "4")
                {
                    if (schema.Plan.ColumnActions == null || schema.Plan.ColumnActions.Count == 0)
                    {
                        Console.WriteLine("No columns in plan.");
                        continue;
                    }
                    Console.Write("Select column number: ");
                    var rawIdx = Console.ReadLine();
                    int ci;
                    if (!int.TryParse(rawIdx, out ci) || ci < 1 || ci > schema.Plan.ColumnActions.Count)
                    {
                        Console.WriteLine("Invalid column selection.");
                        continue;
                    }
                    var ca = schema.Plan.ColumnActions[ci - 1];

                    Console.WriteLine("Editing column: {0} -> {1} [{2}]", ca.Source, ca.Target, string.IsNullOrWhiteSpace(ca.Action) ? "Copy" : ca.Action);
                    Console.WriteLine("  a) Rename target");
                    Console.WriteLine("  b) Set action (Copy/Rename/Drop/Compute)");
                    Console.WriteLine("  c) Set compute expression");
                    Console.WriteLine("  d) Reset to copy");
                    Console.Write("Choice: ");
                    var cc = Console.ReadLine();
                    if (string.Equals(cc, "a", StringComparison.OrdinalIgnoreCase))
                    {
                        var nt = PromptString("New target name", ca.Target);
                        if (nt != ca.Target)
                        {
                            ca.Target = nt;
                            if (!string.Equals(ca.Action, "Drop", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(ca.Action, "Compute", StringComparison.OrdinalIgnoreCase))
                            {
                                ca.Action = "Rename";
                            }
                            changed = true;
                        }
                    }
                    else if (string.Equals(cc, "b", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Write("Enter action [Copy|Rename|Drop|Compute or C/R/D/M]: ");
                        var na = Console.ReadLine();
                        var mappedAction = MapAction(na);
                        if (!string.IsNullOrWhiteSpace(mappedAction) && !string.Equals(mappedAction, ca.Action, StringComparison.Ordinal))
                        {
                            ca.Action = mappedAction;
                            changed = true;
                        }
                    }
                    else if (string.Equals(cc, "c", StringComparison.OrdinalIgnoreCase))
                    {
                        var ex = PromptString("Compute expression", ca.Expression);
                        if (!string.Equals(ex ?? "", ca.Expression ?? "", StringComparison.Ordinal))
                        {
                            ca.Expression = ex;
                            ca.Action = "Compute";
                            changed = true;
                        }
                    }
                    else if (string.Equals(cc, "d", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!(string.Equals(ca.Target, ca.Source, StringComparison.Ordinal) && string.Equals(ca.Action, "Copy", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(ca.Expression)))
                        {
                            ca.Target = ca.Source;
                            ca.Action = "Copy";
                            ca.Expression = null;
                            changed = true;
                        }
                    }
                }
                else if (upper == "S" || ch == "5")
                {
                    if (Save(filePath, schema))
                    {
                        Console.WriteLine("Saved.");
                        changed = false;
                    }
                }
                else if (upper == "R" || ch == "6")
                {
                    schema.Plan.Reviewed = !schema.Plan.Reviewed;
                    Console.WriteLine("Reviewed: " + (schema.Plan.Reviewed ? "Yes" : "No"));
                    changed = true;
                }
                else if (upper == "B" || ch == "7")
                {
                    var rulesPath = Path.Combine(migrationsDir, "Metadata", "BulkRenameRules.json");
                    var rules = BulkRenameEngine.LoadRules(rulesPath);
                    var changes = BulkRenameEngine.ApplyToTable(schema, rules);
                    if (changes.Count == 0)
                    {
                        Console.WriteLine("No rename changes suggested by rules.");
                    }
                    else
                    {
                        Console.WriteLine("Applied {0} rename changes.", changes.Count);
                        BulkRenameEngine.WriteChangesLog(migrationsDir, schema.SourceTable, changes);
                        changed = true;
                    }
                }
                else if (upper == "X" || ch == "8")
                {
                    var outDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits");
                    Directory.CreateDirectory(outDir);
                    var fileName = (schema.Plan.TargetTable ?? schema.SourceTable ?? "Plan") + "_plan.csv";
                    var outPath = Path.Combine(outDir, fileName);
                    PlanCsvIO.Export(outPath, schema);
                    Console.WriteLine("Exported: " + outPath);
                }
                else if (upper == "M" || ch == "9")
                {
                    Console.Write("Enter CSV path to import (or leave blank to look in Metadata\\PlanEdits): ");
                    var p = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(p))
                    {
                        var inDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits");
                        var defaultName = (schema.Plan.TargetTable ?? schema.SourceTable ?? "Plan") + "_plan.csv";
                        p = Path.Combine(inDir, defaultName);
                    }
                    string msg;
                    var n = PlanCsvIO.Import(p, schema, out msg);
                    Console.WriteLine(msg);
                    if (n >= 0) changed = true;
                }
                else if (upper == "I")
                {
                    schema.Plan.Ignore = !schema.Plan.Ignore;
                    Console.WriteLine("Ignore: " + (schema.Plan.Ignore ? "Yes" : "No"));
                    changed = true;
                }
                // Normalize commands
                else if (isNormalize && upper == "H")
                {
                    EnsureNormalizePlan();
                    var current = schema.Plan.Normalize.HeaderTable ?? "";
                    var v = PromptString("Header table name", current);
                    if (!string.Equals(v, current, StringComparison.Ordinal))
                    {
                        schema.Plan.Normalize.HeaderTable = v;
                        changed = true;
                    }
                }
                else if (isNormalize && upper == "L")
                {
                    EnsureNormalizePlan();
                    var current = schema.Plan.Normalize.LineTable ?? "";
                    var v = PromptString("Line table name", current);
                    if (!string.Equals(v, current, StringComparison.Ordinal))
                    {
                        schema.Plan.Normalize.LineTable = v;
                        changed = true;
                    }
                }
                else if (isNormalize && upper == "O")
                {
                    EnsureNormalizePlan();
                    var current = schema.Plan.Normalize.OldCompositeKey ?? new List<string>();
                    var v = PromptList("Old composite key (source cols, comma-separated, in order)", current);
                    if (!ListsEqualCI(v, current))
                    {
                        if (ValidateSources(schema, v))
                        {
                            schema.Plan.Normalize.OldCompositeKey = v;
                            changed = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid column(s) in list. Changes discarded.");
                        }
                    }
                }
                else if (isNormalize && upper == "P")
                {
                    EnsureNormalizePlan();
                    var curH = schema.Plan.Normalize.HeaderPrimaryKey ?? new List<string>();
                    var curL = schema.Plan.Normalize.LinePrimaryKey ?? new List<string>();
                    var v1 = PromptList("Header PK columns (target cols, comma-separated, in order)", curH);
                    var v2 = PromptList("Line PK columns (target cols, comma-separated, in order)", curL);
                    if (!ListsEqualCI(v1, curH)) { schema.Plan.Normalize.HeaderPrimaryKey = v1; changed = true; }
                    if (!ListsEqualCI(v2, curL)) { schema.Plan.Normalize.LinePrimaryKey = v2; changed = true; }
                }
                else if (isNormalize && upper == "F")
                {
                    EnsureNormalizePlan();
                    var current = schema.Plan.Normalize.LineLinkKeyName ?? "";
                    var v = PromptString("Line link FK name (in Lines)", current);
                    if (!string.Equals(v ?? "", current ?? "", StringComparison.Ordinal))
                    {
                        schema.Plan.Normalize.LineLinkKeyName = v;
                        changed = true;
                    }
                }
                else if (isNormalize && upper == "K")
                {
                    EnsureNormalizePlan();
                    var curH = schema.Plan.Normalize.NewHeaderKeyName ?? "";
                    var curL = schema.Plan.Normalize.NewLineKeyName ?? "";

                    var nh = PromptString("New Header key column name", curH);
                    var nl = PromptString("New Line key column name", curL);

                    bool changedHere = false;

                    if (!string.Equals(nh ?? "", curH ?? "", StringComparison.Ordinal))
                    {
                        schema.Plan.Normalize.NewHeaderKeyName = nh;
                        changedHere = true;

                        // Auto-sync Header PK: if empty and new key set, fill it; if clearing and PK is that single key, clear PK.
                        var hp = schema.Plan.Normalize.HeaderPrimaryKey ?? new List<string>();
                        if (string.IsNullOrWhiteSpace(nh))
                        {
                            if (hp.Count == 1 && string.Equals(hp[0] ?? "", curH ?? "", StringComparison.Ordinal))
                                schema.Plan.Normalize.HeaderPrimaryKey = new List<string>();
                        }
                        else
                        {
                            if (hp.Count == 0)
                                schema.Plan.Normalize.HeaderPrimaryKey = new List<string> { nh };
                        }
                    }

                    if (!string.Equals(nl ?? "", curL ?? "", StringComparison.Ordinal))
                    {
                        schema.Plan.Normalize.NewLineKeyName = nl;
                        changedHere = true;

                        // Auto-sync Line PK: if empty and new key set, fill it; if clearing and PK is that single key, clear PK.
                        var lp = schema.Plan.Normalize.LinePrimaryKey ?? new List<string>();
                        if (string.IsNullOrWhiteSpace(nl))
                        {
                            if (lp.Count == 1 && string.Equals(lp[0] ?? "", curL ?? "", StringComparison.Ordinal))
                                schema.Plan.Normalize.LinePrimaryKey = new List<string>();
                        }
                        else
                        {
                            if (lp.Count == 0)
                                schema.Plan.Normalize.LinePrimaryKey = new List<string> { nl };
                        }
                    }

                    if (changedHere) changed = true;
                }
                // Normalize quick assignments: h/l/u <#>
                else if (isNormalize && ch.StartsWith("h ", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureNormalizePlan();
                    if (TryParseIndex(ch, out var i))
                    {
                        var col = GetColByIndex(schema, i);
                        if (col != null)
                        {
                            MoveTo(schema.Plan.Normalize.HeaderColumns, schema.Plan.Normalize.LineColumns, col);
                            changed = true;
                        }
                    }
                }
                else if (isNormalize && ch.StartsWith("l ", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureNormalizePlan();
                    if (TryParseIndex(ch, out var i))
                    {
                        var col = GetColByIndex(schema, i);
                        if (col != null)
                        {
                            MoveTo(schema.Plan.Normalize.LineColumns, schema.Plan.Normalize.HeaderColumns, col);
                            changed = true;
                        }
                    }
                }
                else if (isNormalize && ch.StartsWith("u ", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureNormalizePlan();
                    if (TryParseIndex(ch, out var i))
                    {
                        var col = GetColByIndex(schema, i);
                        if (col != null)
                        {
                            RemoveFrom(schema.Plan.Normalize.HeaderColumns, col);
                            RemoveFrom(schema.Plan.Normalize.LineColumns, col);
                            changed = true;
                        }
                    }
                }
            }
        }

        private static bool TryParseIndex(string input, out int idx)
        {
            idx = -1;
            var parts = input.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], out var n)) { idx = n; return true; }
            return false;
        }

        private static string GetColByIndex(TableSchema schema, int idx1)
        {
            if (schema?.Plan?.ColumnActions == null) return null;
            if (idx1 < 1 || idx1 > schema.Plan.ColumnActions.Count) return null;
            return schema.Plan.ColumnActions[idx1 - 1].Source;
        }

        private static void MoveTo(List<string> target, List<string> other, string col)
        {
            RemoveFrom(other, col);
            AddUnique(target, col);
        }

        private static void RemoveFrom(List<string> list, string col)
        {
            list.RemoveAll(x => string.Equals(x, col, StringComparison.OrdinalIgnoreCase));
        }

        private static void AddUnique(List<string> list, string col)
        {
            if (!list.Any(x => string.Equals(x, col, StringComparison.OrdinalIgnoreCase)))
                list.Add(col);
        }

        private static List<string> ParseList(string v)
        {
            var res = new List<string>();
            if (string.IsNullOrWhiteSpace(v)) return res;
            foreach (var p in v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var s = p.Trim();
                if (s.Length > 0) res.Add(s);
            }
            return res;
        }

        private static bool ValidateSources(TableSchema schema, List<string> cols)
        {
            var src = new HashSet<string>((schema?.Plan?.ColumnActions ?? new List<ColumnPlan>()).Select(a => a.Source), StringComparer.OrdinalIgnoreCase);
            return cols.All(c => src.Contains(c));
        }

        private static bool ListsEqualCI(List<string> a, List<string> b)
        {
            a = a ?? new List<string>(); b = b ?? new List<string>();
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
                if (!string.Equals(a[i] ?? "", b[i] ?? "", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private static string PromptString(string prompt, string current)
        {
            var shown = string.IsNullOrWhiteSpace(current) ? "(unset)" : current;
            Console.Write("{0} [{1}] (* to clear): ", prompt, shown);
            var input = Console.ReadLine();
            if (input == null) return current;
            input = input.Trim();
            if (input == "*") return "";
            if (input.Length == 0) return current; // keep
            return input;
        }

        private static List<string> PromptList(string prompt, List<string> current)
        {
            var shown = (current != null && current.Count > 0) ? string.Join(",", current) : "(unset)";
            Console.Write("{0} [{1}] (* to clear): ", prompt, shown);
            var input = Console.ReadLine();
            if (input == null || input.Trim().Length == 0) return current ?? new List<string>(); // keep
            input = input.Trim();
            if (input == "*") return new List<string>();
            return ParseList(input);
        }

        private static string MapClassification(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var v = input.Trim();
            if (v.Length == 1)
            {
                switch (char.ToUpperInvariant(v[0]))
                {
                    case 'C': return "Copy";
                    case 'R': return "Rename";
                    case 'N': return "Normalize";
                    case 'F': return "Refactor";
                    default: return null;
                }
            }
            var norm = v.ToLowerInvariant();
            if (norm.StartsWith("copy")) return "Copy";
            if (norm.StartsWith("rename")) return "Rename";
            if (norm.StartsWith("normalize")) return "Normalize";
            if (norm.StartsWith("refactor")) return "Refactor";
            return null;
        }

        private static string MapAction(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var v = input.Trim();
            if (v.Length == 1)
            {
                switch (char.ToUpperInvariant(v[0]))
                {
                    case 'C': return "Copy";
                    case 'R': return "Rename";
                    case 'D': return "Drop";
                    case 'M': return "Compute";
                    default: return null;
                }
            }
            var norm = v.ToLowerInvariant();
            if (norm.StartsWith("copy")) return "Copy";
            if (norm.StartsWith("rename")) return "Rename";
            if (norm.StartsWith("drop")) return "Drop";
            if (norm.StartsWith("compute")) return "Compute";
            return null;
        }

        private static bool Save(string path, TableSchema schema)
        {
            try
            {
                var json = JsonConvert.SerializeObject(schema, Formatting.Indented);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to save. " + ex.Message);
                return false;
            }
        }

        private static bool Confirm(string prompt)
        {
            Console.Write(prompt);
            var v = Console.ReadLine();
            return v != null && (v.Equals("y", StringComparison.OrdinalIgnoreCase) || v.Equals("yes", StringComparison.OrdinalIgnoreCase));
        }
    }
}