using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    [Flags]
    enum ExportKind
    {
        None = 0,
        Tables = 1,
        Columns = 2,
        Normalize = 4,
        Assignments = 8,
        All = Tables | Columns | Normalize | Assignments
    }

    class ImportChangeReport
    {
        public int TablesUpdated { get; set; }
        public int ColumnPlansUpdated { get; set; }
        public int NormalizeMetaUpdated { get; set; }
        public int NormalizeAssignmentsUpdated { get; set; }
        public List<string> AffectedTables { get; set; } = new List<string>();
        public List<string> Messages { get; set; } = new List<string>();
    }

    struct ExportPaths
    {
        public string TablesCsvPath;
        public string ColumnsCsvPath;
        public string NormalizeCsvPath;
        public string NormalizeAssignCsvPath;
    }

    static class PlanBulkCsv
    {
        // INTERACTIVE MENUS

        public static int RunExportMenu(string migrationsDir)
        {
            Console.WriteLine("Export plan to CSV");
            var defaultOutDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits");
            Console.Write("Output directory [{0}]: ", defaultOutDir);
            var outDir = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(outDir)) outDir = defaultOutDir;

            Console.WriteLine("What to export?");
            Console.WriteLine("  A) All   T) Tables   C) Columns   N) Normalize   S) Assignments");
            Console.Write("Choice (A/T/C/N/S or comma-separated, e.g. T,C): ");
            var choice = (Console.ReadLine() ?? "").Trim();

            var kind = ParseExportKind(choice);
            if (kind == ExportKind.None) kind = ExportKind.All;

            using (var log = CreateLogger(outDir, "Export"))
            {
                log?.Info("Export started. outDir=" + outDir + " kind=" + kind);
                var rc = ExportSelected(migrationsDir, outDir, kind, out var paths, out var errors, log);
                if (rc == 0)
                {
                    Console.WriteLine("Export complete:");
                    if ((kind & ExportKind.Tables) != 0) Console.WriteLine("  Tables:      " + paths.TablesCsvPath);
                    if ((kind & ExportKind.Columns) != 0) Console.WriteLine("  Columns:     " + paths.ColumnsCsvPath);
                    if ((kind & ExportKind.Normalize) != 0) Console.WriteLine("  Normalize:   " + paths.NormalizeCsvPath);
                    if ((kind & ExportKind.Assignments) != 0) Console.WriteLine("  Assignments: " + paths.NormalizeAssignCsvPath);
                }
                else
                {
                    Console.Error.WriteLine("Export finished with code " + rc);
                }

                if (errors.Count > 0)
                {
                    Console.Error.WriteLine("Errors (" + errors.Count + "):");
                    foreach (var e in errors.Take(10))
                    {
                        Console.Error.WriteLine("  - " + e);
                        log?.Error(e);
                    }
                    if (errors.Count > 10)
                    {
                        Console.Error.WriteLine("  ... " + (errors.Count - 10) + " more");
                        log?.Warn("... " + (errors.Count - 10) + " more errors not shown.");
                    }
                }
                log?.Info("Export finished with code " + rc);
                return rc;
            }
        }

        public static int RunImportMenu(string migrationsDir)
        {
            Console.WriteLine("Import plan from CSV");
            var defaultDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits");
            Console.Write("Base directory for CSVs [{0}]: ", defaultDir);
            var baseDir = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(baseDir)) baseDir = defaultDir;

            Console.WriteLine("What to import?");
            Console.WriteLine("  A) All   T) Tables   C) Columns   N) Normalize   S) Assignments");
            Console.Write("Choice (A/T/C/N/S or comma-separated): ");
            var choice = (Console.ReadLine() ?? "").Trim();
            var kind = ParseExportKind(choice);
            if (kind == ExportKind.None) kind = ExportKind.All;

            // Resolve paths (only for selected kinds)
            var tCsv = (kind & ExportKind.Tables) != 0 ? Path.Combine(baseDir, "PlanTables.csv") : null;
            var cCsv = (kind & ExportKind.Columns) != 0 ? Path.Combine(baseDir, "PlanColumns.csv") : null;
            var nCsv = (kind & ExportKind.Normalize) != 0 ? Path.Combine(baseDir, "PlanNormalize.csv") : null;
            var aCsv = (kind & ExportKind.Assignments) != 0 ? Path.Combine(baseDir, "PlanNormalizeAssignments.csv") : null;

            Console.Write("Override any CSV path(s)? (y/n): ");
            var yn = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(yn) && yn.StartsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                if ((kind & ExportKind.Tables) != 0) { Console.Write("Tables CSV [{0}]: ", tCsv); var v = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(v)) tCsv = v.Trim(); }
                if ((kind & ExportKind.Columns) != 0) { Console.Write("Columns CSV [{0}]: ", cCsv); var v = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(v)) cCsv = v.Trim(); }
                if ((kind & ExportKind.Normalize) != 0) { Console.Write("Normalize CSV [{0}]: ", nCsv); var v = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(v)) nCsv = v.Trim(); }
                if ((kind & ExportKind.Assignments) != 0) { Console.Write("Assignments CSV [{0}]: ", aCsv); var v = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(v)) aCsv = v.Trim(); }
            }

            using (var log = CreateLogger(baseDir, "Import"))
            {
                log?.Info("Import started. kind=" + kind);
                log?.Info("Paths: Tables=" + (tCsv ?? "(skip)") + " Columns=" + (cCsv ?? "(skip)") + " Normalize=" + (nCsv ?? "(skip)") + " Assignments=" + (aCsv ?? "(skip)"));

                var rc = ImportSelected(migrationsDir, tCsv, cCsv, nCsv, aCsv, out var report, out var errors, log);
                if (rc == 0 || rc == 1)
                {
                    Console.WriteLine("Import completed. Change summary:");
                    Console.WriteLine("  Tables updated:               " + report.TablesUpdated);
                    Console.WriteLine("  Column plan rows updated:     " + report.ColumnPlansUpdated);
                    Console.WriteLine("  Normalize meta tables updated:" + report.NormalizeMetaUpdated);
                    Console.WriteLine("  Assignments updated:          " + report.NormalizeAssignmentsUpdated);
                    if (report.AffectedTables.Count > 0)
                        Console.WriteLine("  Affected tables: " + string.Join(", ", report.AffectedTables.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s)));

                    // Print field-level change messages
                    if (report.Messages.Count > 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Field changes:");
                        foreach (var m in report.Messages.Take(100))
                            Console.WriteLine("  - " + m);
                        if (report.Messages.Count > 100)
                            Console.WriteLine("  ... " + (report.Messages.Count - 100) + " more");
                    }

                    log?.Info("Change summary: Tables=" + report.TablesUpdated + " Columns=" + report.ColumnPlansUpdated +
                              " NormalizeMeta=" + report.NormalizeMetaUpdated + " Assignments=" + report.NormalizeAssignmentsUpdated);
                    if (report.AffectedTables.Count > 0)
                        log?.Info("Affected tables: " + string.Join(", ", report.AffectedTables.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s)));
                    if (report.Messages.Count > 0)
                        foreach (var m in report.Messages.Take(100)) log?.Info("Change: " + m);
                }
                else
                {
                    Console.Error.WriteLine("Import failed with code " + rc);
                    log?.Error("Import failed with code " + rc);
                }

                if (errors.Count > 0)
                {
                    Console.Error.WriteLine("Errors (" + errors.Count + "):");
                    foreach (var e in errors.Take(10))
                    {
                        Console.Error.WriteLine("  - " + e);
                        log?.Error(e);
                    }
                    if (errors.Count > 10)
                    {
                        Console.Error.WriteLine("  ... " + (errors.Count - 10) + " more");
                        log?.Warn("... " + (errors.Count - 10) + " more errors not shown.");
                    }
                }

                // Pause so the user can read the results before the menu refreshes
                Console.WriteLine();
                Console.Write("Press any key to continue...");
                try { Console.ReadKey(true); } catch { /* ignore in non-interactive envs */ }
                Console.WriteLine();

                log?.Info("Import finished with code " + rc);
                return rc;
            }
        }

        private static ExportKind ParseExportKind(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return ExportKind.All;
            var s = input.Trim().ToUpperInvariant();
            if (s == "A") return ExportKind.All;
            if (s.Length == 1)
            {
                switch (s)
                {
                    case "T": return ExportKind.Tables;
                    case "C": return ExportKind.Columns;
                    case "N": return ExportKind.Normalize;
                    case "S": return ExportKind.Assignments;
                }
            }
            var result = ExportKind.None;
            foreach (var part in s.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (part.Trim())
                {
                    case "A": result |= ExportKind.All; break;
                    case "T": result |= ExportKind.Tables; break;
                    case "C": result |= ExportKind.Columns; break;
                    case "N": result |= ExportKind.Normalize; break;
                    case "S": result |= ExportKind.Assignments; break;
                }
            }
            return result == ExportKind.None ? ExportKind.All : result;
        }

        // GRANULAR EXPORT

        public static int ExportSelected(string migrationsDir, string outDir, ExportKind kind, out ExportPaths paths, out List<string> errors)
        {
            return ExportSelected(migrationsDir, outDir, kind, out paths, out errors, logger: null);
        }

        private static int ExportSelected(string migrationsDir, string outDir, ExportKind kind, out ExportPaths paths, out List<string> errors, Logger logger)
        {
            errors = new List<string>();
            paths = new ExportPaths();

            try { Directory.CreateDirectory(outDir); }
            catch (Exception ex) { errors.Add("Failed to create output dir: " + ex.Message); logger?.Error(errors[errors.Count - 1]); return 2; }

            var dir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
            string[] files;
            try
            {
                files = Directory.GetFiles(dir, "*.schema.json")
                    .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => Path.GetFileName(f))
                    .ToArray();
            }
            catch (Exception ex)
            {
                errors.Add("Failed to enumerate AccessSchema files: " + ex.Message);
                logger?.Error(errors[errors.Count - 1]);
                return 2;
            }
            if (files.Length == 0) { errors.Add("No *.schema.json files found."); logger?.Warn(errors[errors.Count - 1]); return 2; }

            logger?.Info("Exporting from " + files.Length + " schema files.");

            var sbT = new StringBuilder(); int tRows = 0; if ((kind & ExportKind.Tables) != 0) sbT.AppendLine("SourceTable,TargetTable,Classification,PreserveIds,Reviewed,Ignore");
            var sbC = new StringBuilder(); int cRows = 0; if ((kind & ExportKind.Columns) != 0) sbC.AppendLine("SourceTable,SourceColumn,TargetColumn,Action,Expression");
            var sbN = new StringBuilder(); int nRows = 0; if ((kind & ExportKind.Normalize) != 0) sbN.AppendLine("SourceTable,HeaderTable,LineTable,OldCompositeKey,NewHeaderKeyName,NewLineKeyName,LineLinkKeyName,HeaderPrimaryKey,LinePrimaryKey,PreserveHeaderIds,PreserveLineIds");
            var sbA = new StringBuilder(); int aRows = 0; if ((kind & ExportKind.Assignments) != 0) sbA.AppendLine("SourceTable,SourceColumn,Part,TargetColumn");

            foreach (var f in files)
            {
                TableSchema s = null;
                try
                {
                    var json = File.ReadAllText(f);
                    s = JsonConvert.DeserializeObject<TableSchema>(json);
                }
                catch (Exception ex) { var msg = "Failed to read/parse schema '" + f + "': " + ex.Message; errors.Add(msg); logger?.Error(msg); continue; }
                if (s == null) { var msg = "Schema deserialized to null: " + f; errors.Add(msg); logger?.Error(msg); continue; }
                try
                {
                    bool ignored = s.Plan?.Ignore == true;

                    // Tables CSV: include all tables (so Ignore state is visible)
                    if ((kind & ExportKind.Tables) != 0 && s.Plan != null)
                    {
                        sbT.AppendLine(string.Join(",", Csv(s.SourceTable),
                                                         Csv(s.Plan.TargetTable ?? s.SourceTable ?? ""),
                                                         Csv(s.Plan.Classification ?? "Copy"),
                                                         s.Plan.PreserveIdsOnInsert ? "Yes" : "No",
                                                         s.Plan.Reviewed ? "Yes" : "No",
                                                         s.Plan.Ignore ? "Yes" : "No"));
                        tRows++;
                    }

                    // Columns/Normalize/Assignments: skip ignored tables
                    if (ignored)
                    {
                        if ((kind & ExportKind.Columns) != 0) logger?.Info("Skipping columns export for ignored table: " + s.SourceTable);
                        if ((kind & ExportKind.Normalize) != 0) logger?.Info("Skipping normalize export for ignored table: " + s.SourceTable);
                        if ((kind & ExportKind.Assignments) != 0) logger?.Info("Skipping assignments export for ignored table: " + s.SourceTable);
                    }
                    else
                    {
                        if ((kind & ExportKind.Columns) != 0 && s.Plan?.ColumnActions != null)
                        {
                            foreach (var a in s.Plan.ColumnActions)
                            {
                                var action = string.IsNullOrWhiteSpace(a.Action) ? "Copy" : a.Action;
                                var tgt = string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target;
                                sbC.AppendLine(string.Join(",", Csv(s.SourceTable), Csv(a.Source), Csv(tgt), Csv(action), Csv(a.Expression ?? "")));
                                cRows++;
                            }
                        }
                        if ((kind & ExportKind.Normalize) != 0 && s.Plan?.Normalize != null)
                        {
                            var n = s.Plan.Normalize;
                            sbN.AppendLine(string.Join(",",
                                Csv(s.SourceTable),
                                Csv(n.HeaderTable ?? ""),
                                Csv(n.LineTable ?? ""),
                                Csv(JoinPipe(n.OldCompositeKey)),
                                Csv(n.NewHeaderKeyName ?? ""),
                                Csv(n.NewLineKeyName ?? ""),
                                Csv(n.LineLinkKeyName ?? ""),
                                Csv(JoinPipe(n.HeaderPrimaryKey)),
                                Csv(JoinPipe(n.LinePrimaryKey)),
                                n.PreserveHeaderIds ? "Yes" : "No",
                                n.PreserveLineIds ? "Yes" : "No"));
                            nRows++;
                        }
                        if ((kind & ExportKind.Assignments) != 0 && s.Plan?.Normalize != null && s.Plan?.ColumnActions != null)
                        {
                            var n = s.Plan.Normalize;
                            foreach (var a in s.Plan.ColumnActions)
                            {
                                var part = IsIn(n.HeaderColumns, a.Source) ? "H" : IsIn(n.LineColumns, a.Source) ? "L" : "";
                                var tgt = string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target;
                                sbA.AppendLine(string.Join(",", Csv(s.SourceTable), Csv(a.Source), Csv(part), Csv(tgt)));
                                aRows++;
                            }
                        }
                    }
                }
                catch (Exception ex) { var msg = "Failed to build CSV rows for schema '" + (s.SourceTable ?? f) + "': " + ex.Message; errors.Add(msg); logger?.Error(msg); }
            }

            int rc = 0;
            if ((kind & ExportKind.Tables) != 0)
            {
                paths.TablesCsvPath = Path.Combine(outDir, "PlanTables.csv");
                if (!SafeWriteFile(paths.TablesCsvPath, sbT.ToString(), errors)) { rc = 1; logger?.Error("Failed to write " + paths.TablesCsvPath); }
                else logger?.Info("Wrote Tables CSV: " + paths.TablesCsvPath + " (rows=" + tRows + ")");
            }
            if ((kind & ExportKind.Columns) != 0)
            {
                paths.ColumnsCsvPath = Path.Combine(outDir, "PlanColumns.csv");
                if (!SafeWriteFile(paths.ColumnsCsvPath, sbC.ToString(), errors)) { rc = 1; logger?.Error("Failed to write " + paths.ColumnsCsvPath); }
                else logger?.Info("Wrote Columns CSV: " + paths.ColumnsCsvPath + " (rows=" + cRows + ")");
            }
            if ((kind & ExportKind.Normalize) != 0)
            {
                paths.NormalizeCsvPath = Path.Combine(outDir, "PlanNormalize.csv");
                if (!SafeWriteFile(paths.NormalizeCsvPath, sbN.ToString(), errors)) { rc = 1; logger?.Error("Failed to write " + paths.NormalizeCsvPath); }
                else logger?.Info("Wrote Normalize CSV: " + paths.NormalizeCsvPath + " (rows=" + nRows + ")");
            }
            if ((kind & ExportKind.Assignments) != 0)
            {
                paths.NormalizeAssignCsvPath = Path.Combine(outDir, "PlanNormalizeAssignments.csv");
                if (!SafeWriteFile(paths.NormalizeAssignCsvPath, sbA.ToString(), errors)) { rc = 1; logger?.Error("Failed to write " + paths.NormalizeAssignCsvPath); }
                else logger?.Info("Wrote Assignments CSV: " + paths.NormalizeAssignCsvPath + " (rows=" + aRows + ")");
            }

            if (errors.Count > 0 && rc == 0) rc = 1;
            return rc;
        }

        // GRANULAR IMPORT + REPORT

        public static int ImportSelected(string migrationsDir,
            string tablesCsvPath,
            string columnsCsvPath,
            string normalizeCsvPath,
            string normalizeAssignCsvPath,
            out ImportChangeReport report,
            out List<string> errors)
        {
            return ImportSelected(migrationsDir, tablesCsvPath, columnsCsvPath, normalizeCsvPath, normalizeAssignCsvPath, out report, out errors, logger: null);
        }

        private static int ImportSelected(string migrationsDir,
            string tablesCsvPath,
            string columnsCsvPath,
            string normalizeCsvPath,
            string normalizeAssignCsvPath,
            out ImportChangeReport report,
            out List<string> errors,
            Logger logger)
        {
            report = new ImportChangeReport();
            errors = new List<string>();

            var dir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
            if (!Directory.Exists(dir)) { var m = "AccessSchema folder not found."; errors.Add(m); logger?.Error(m); return 2; }

            string[] files;
            try
            {
                files = Directory.GetFiles(dir, "*.schema.json")
                    .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }
            catch (Exception ex) { var m = "Failed to enumerate AccessSchema files: " + ex.Message; errors.Add(m); logger?.Error(m); return 2; }

            logger?.Info("Loaded schema file list. Count=" + files.Length);

            var map = new Dictionary<string, Tuple<string, TableSchema>>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in files)
            {
                try
                {
                    var s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(f));
                    if (s == null) { var m = "Schema deserialized to null: " + f; errors.Add(m); logger?.Error(m); continue; }
                    EnsurePlan(s);
                    map[s.SourceTable ?? Path.GetFileNameWithoutExtension(f)] = Tuple.Create(f, s);
                }
                catch (Exception ex) { var m = "Failed to read/parse schema '" + f + "': " + ex.Message; errors.Add(m); logger?.Error(m); }
            }

            // Import tables
            if (!string.IsNullOrWhiteSpace(tablesCsvPath))
            {
                try
                {
                    if (File.Exists(tablesCsvPath))
                    {
                        logger?.Info("Importing Tables CSV: " + tablesCsvPath);
                        var lines = File.ReadAllLines(tablesCsvPath);
                        if (lines.Length > 1)
                        {
                            var header = Parse(lines[0]);
                            int iSrc = IndexOf(header, "SourceTable"),
                                iTgt = IndexOf(header, "TargetTable"),
                                iCls = IndexOf(header, "Classification"),
                                iIds = IndexOf(header, "PreserveIds"),
                                iRev = IndexOf(header, "Reviewed"),
                                iIgn = IndexOf(header, "Ignore");

                            for (int i = 1; i < lines.Length; i++)
                            {
                                var cols = Parse(lines[i]);
                                if (cols.Count == 0) continue;
                                var src = Get(cols, iSrc);
                                if (string.IsNullOrWhiteSpace(src)) continue;
                                if (!map.TryGetValue(src, out var entry)) continue;
                                var s = entry.Item2;

                                bool tblChanged = false;
                                var tgt = Get(cols, iTgt);
                                if (!string.IsNullOrWhiteSpace(tgt))
                                    tblChanged |= AssignIfChanged(s.Plan.TargetTable, tgt.Trim(), v => s.Plan.TargetTable = v, StringComparison.Ordinal);

                                var cls = Get(cols, iCls);
                                if (!string.IsNullOrWhiteSpace(cls))
                                    tblChanged |= AssignIfChanged(s.Plan.Classification, cls.Trim(), v => s.Plan.Classification = v, StringComparison.Ordinal);

                                var ids = Get(cols, iIds);
                                if (!string.IsNullOrWhiteSpace(ids))
                                    tblChanged |= AssignIfChanged(s.Plan.PreserveIdsOnInsert, ids.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase), v => s.Plan.PreserveIdsOnInsert = v);

                                var rev = Get(cols, iRev);
                                if (!string.IsNullOrWhiteSpace(rev))
                                    tblChanged |= AssignIfChanged(s.Plan.Reviewed, rev.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase), v => s.Plan.Reviewed = v);

                                var ign = Get(cols, iIgn);
                                if (!string.IsNullOrWhiteSpace(ign))
                                    tblChanged |= AssignIfChanged(s.Plan.Ignore, ign.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase), v => s.Plan.Ignore = v);

                                if (tblChanged)
                                {
                                    report.TablesUpdated++;
                                    report.AffectedTables.Add(s.SourceTable);
                                }
                            }
                        }
                    }
                    else { var m = "Tables CSV not found: " + tablesCsvPath; errors.Add(m); logger?.Warn(m); }
                }
                catch (Exception ex) { var m = "Failed importing tables CSV '" + tablesCsvPath + "': " + ex.Message; errors.Add(m); logger?.Error(m); }
            }

            // Import columns
            if (!string.IsNullOrWhiteSpace(columnsCsvPath))
            {
                try
                {
                    if (File.Exists(columnsCsvPath))
                    {
                        logger?.Info("Importing Columns CSV: " + columnsCsvPath);
                        var lines = File.ReadAllLines(columnsCsvPath);
                        if (lines.Length > 1)
                        {
                            var header = Parse(lines[0]);
                            int iSrcTbl = IndexOf(header, "SourceTable"),
                                iSrcCol = IndexOf(header, "SourceColumn"),
                                iTgtCol = IndexOf(header, "TargetColumn"),
                                iAct = IndexOf(header, "Action"),
                                iExpr = IndexOf(header, "Expression");

                            for (int i = 1; i < lines.Length; i++)
                            {
                                var cols = Parse(lines[i]);
                                if (cols.Count == 0) continue;
                                var srcTbl = Get(cols, iSrcTbl);
                                var srcCol = Get(cols, iSrcCol);
                                if (string.IsNullOrWhiteSpace(srcTbl) || string.IsNullOrWhiteSpace(srcCol)) continue;
                                if (!map.TryGetValue(srcTbl, out var entry)) continue;
                                var s = entry.Item2;
                                EnsurePlan(s);

                                var tgtCol = Get(cols, iTgtCol);
                                var act = Get(cols, iAct);
                                var expr = Get(cols, iExpr);

                                var ca = s.Plan.ColumnActions.FirstOrDefault(a => string.Equals(a.Source, srcCol, StringComparison.OrdinalIgnoreCase));
                                if (ca == null)
                                {
                                    ca = new ColumnPlan { Source = srcCol, Target = srcCol, Action = "Copy" };
                                    s.Plan.ColumnActions.Add(ca);
                                }

                                // Track old values for detailed reporting
                                var oldTarget = ca.Target;
                                var oldAction = ca.Action;
                                var oldExpr = ca.Expression;

                                bool colChanged = false;
                                if (!string.IsNullOrWhiteSpace(tgtCol))
                                    colChanged |= AssignIfChanged(ca.Target, tgtCol.Trim(), v => ca.Target = v, StringComparison.Ordinal);
                                if (!string.IsNullOrWhiteSpace(act))
                                    colChanged |= AssignIfChanged(ca.Action, act.Trim(), v => ca.Action = v, StringComparison.Ordinal);
                                colChanged |= AssignIfChanged(ca.Expression, expr, v => ca.Expression = v, StringComparison.Ordinal);

                                if (colChanged)
                                {
                                    report.ColumnPlansUpdated++;
                                    report.AffectedTables.Add(s.SourceTable);

                                    var parts = new List<string>();
                                    if (!string.Equals(oldTarget ?? "", ca.Target ?? "", StringComparison.Ordinal)) parts.Add($"Target: '{oldTarget ?? ""}' -> '{ca.Target ?? ""}'");
                                    if (!string.Equals(oldAction ?? "", ca.Action ?? "", StringComparison.Ordinal)) parts.Add($"Action: '{oldAction ?? "Copy"}' -> '{ca.Action ?? "Copy"}'");
                                    if (!string.Equals(oldExpr ?? "", ca.Expression ?? "", StringComparison.Ordinal)) parts.Add($"Expr: '{(oldExpr ?? "")}' -> '{(ca.Expression ?? "")}'");
                                    if (parts.Count > 0)
                                        report.Messages.Add($"[{s.SourceTable}] {srcCol} " + string.Join(", ", parts));
                                }
                            }
                        }
                    }
                    else { var m = "Columns CSV not found: " + columnsCsvPath; errors.Add(m); logger?.Warn(m); }
                }
                catch (Exception ex) { var m = "Failed importing columns CSV '" + columnsCsvPath + "': " + ex.Message; errors.Add(m); logger?.Error(m); }
            }

            // Import Normalize meta
            if (!string.IsNullOrWhiteSpace(normalizeCsvPath))
            {
                try
                {
                    if (File.Exists(normalizeCsvPath))
                    {
                        logger?.Info("Importing Normalize CSV: " + normalizeCsvPath);
                        var lines = File.ReadAllLines(normalizeCsvPath);
                        if (lines.Length > 1)
                        {
                            var header = Parse(lines[0]);
                            int iSrc = IndexOf(header, "SourceTable"),
                                iHdr = IndexOf(header, "HeaderTable"),
                                iLin = IndexOf(header, "LineTable"),
                                iOld = IndexOf(header, "OldCompositeKey"),
                                iHKey = IndexOf(header, "NewHeaderKeyName"),
                                iLKey = IndexOf(header, "NewLineKeyName"),
                                iFk = IndexOf(header, "LineLinkKeyName"),
                                iHPk = IndexOf(header, "HeaderPrimaryKey"),
                                iLPk = IndexOf(header, "LinePrimaryKey"),
                                iPH = IndexOf(header, "PreserveHeaderIds"),
                                iPL = IndexOf(header, "PreserveLineIds");

                            for (int i = 1; i < lines.Length; i++)
                            {
                                var cols = Parse(lines[i]);
                                if (cols.Count == 0) continue;
                                var src = Get(cols, iSrc);
                                if (string.IsNullOrWhiteSpace(src)) continue;
                                if (!map.TryGetValue(src, out var entry)) continue;
                                var s = entry.Item2;
                                EnsurePlan(s);
                                EnsureNormalize(s);
                                var n = s.Plan.Normalize;

                                bool metaChanged = false;
                                var hdr = Get(cols, iHdr);
                                if (!string.IsNullOrWhiteSpace(hdr))
                                    metaChanged |= AssignIfChanged(n.HeaderTable, hdr.Trim(), v => n.HeaderTable = v, StringComparison.Ordinal);

                                var lin = Get(cols, iLin);
                                if (!string.IsNullOrWhiteSpace(lin))
                                    metaChanged |= AssignIfChanged(n.LineTable, lin.Trim(), v => n.LineTable = v, StringComparison.Ordinal);

                                var old = Get(cols, iOld);
                                metaChanged |= UpdateListIfDifferent(n.OldCompositeKey, ParsePipe(old));

                                var hkey = Get(cols, iHKey);
                                if (!string.IsNullOrWhiteSpace(hkey))
                                    metaChanged |= AssignIfChanged(n.NewHeaderKeyName, hkey.Trim(), v => n.NewHeaderKeyName = v, StringComparison.Ordinal);

                                var lkey = Get(cols, iLKey);
                                if (!string.IsNullOrWhiteSpace(lkey))
                                    metaChanged |= AssignIfChanged(n.NewLineKeyName, lkey.Trim(), v => n.NewLineKeyName = v, StringComparison.Ordinal);

                                var fk = Get(cols, iFk);
                                if (!string.IsNullOrWhiteSpace(fk))
                                    metaChanged |= AssignIfChanged(n.LineLinkKeyName, fk.Trim(), v => n.LineLinkKeyName = v, StringComparison.Ordinal);

                                var hpk = Get(cols, iHPk);
                                metaChanged |= UpdateListIfDifferent(n.HeaderPrimaryKey, ParsePipe(hpk));

                                var lpk = Get(cols, iLPk);
                                metaChanged |= UpdateListIfDifferent(n.LinePrimaryKey, ParsePipe(lpk));

                                var ph = Get(cols, iPH);
                                if (!string.IsNullOrWhiteSpace(ph))
                                    metaChanged |= AssignIfChanged(n.PreserveHeaderIds, ph.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase), v => n.PreserveHeaderIds = v);

                                var pl = Get(cols, iPL);
                                if (!string.IsNullOrWhiteSpace(pl))
                                    metaChanged |= AssignIfChanged(n.PreserveLineIds, pl.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase), v => n.PreserveLineIds = v);

                                if (metaChanged)
                                {
                                    report.NormalizeMetaUpdated++;
                                    report.AffectedTables.Add(s.SourceTable);
                                }
                            }
                        }
                    }
                    else { var m = "Normalize CSV not found: " + normalizeCsvPath; errors.Add(m); logger?.Warn(m); }
                }
                catch (Exception ex) { var m = "Failed importing normalize CSV '" + normalizeCsvPath + "': " + ex.Message; errors.Add(m); logger?.Error(m); }
            }

            // Import Normalize assignments
            if (!string.IsNullOrWhiteSpace(normalizeAssignCsvPath))
            {
                try
                {
                    if (File.Exists(normalizeAssignCsvPath))
                    {
                        logger?.Info("Importing Assignments CSV: " + normalizeAssignCsvPath);
                        var lines = File.ReadAllLines(normalizeAssignCsvPath);
                        if (lines.Length > 1)
                        {
                            var header = Parse(lines[0]);
                            int iSrc = IndexOf(header, "SourceTable"),
                                iCol = IndexOf(header, "SourceColumn"),
                                iPart = IndexOf(header, "Part"),
                                iTgt = IndexOf(header, "TargetColumn");

                            for (int i = 1; i < lines.Length; i++)
                            {
                                var cols = Parse(lines[i]);
                                if (cols.Count == 0) continue;
                                var srcTbl = Get(cols, iSrc);
                                var srcCol = Get(cols, iCol);
                                if (string.IsNullOrWhiteSpace(srcTbl) || string.IsNullOrWhiteSpace(srcCol)) continue;
                                if (!map.TryGetValue(srcTbl, out var entry)) continue;
                                var s = entry.Item2;
                                EnsurePlan(s);
                                EnsureNormalize(s);
                                var n = s.Plan.Normalize;

                                if (n.HeaderColumns == null) n.HeaderColumns = new List<string>();
                                if (n.LineColumns == null) n.LineColumns = new List<string>();

                                var part = (Get(cols, iPart) ?? "").Trim();
                                var tgt = Get(cols, iTgt);

                                // Optional rename (report if changed)
                                if (!string.IsNullOrWhiteSpace(tgt))
                                {
                                    var ca = s.Plan.ColumnActions.FirstOrDefault(a => string.Equals(a.Source, srcCol, StringComparison.OrdinalIgnoreCase));
                                    if (ca == null) { ca = new ColumnPlan { Source = srcCol, Target = srcCol, Action = "Copy" }; s.Plan.ColumnActions.Add(ca); }
                                    var oldTgt = ca.Target;
                                    if (AssignIfChanged(ca.Target, tgt.Trim(), v => ca.Target = v, StringComparison.Ordinal))
                                    {
                                        report.ColumnPlansUpdated++;
                                        report.AffectedTables.Add(s.SourceTable);
                                        report.Messages.Add($"[{s.SourceTable}] {srcCol} Rename: '{oldTgt ?? ""}' -> '{ca.Target ?? ""}'");
                                    }
                                }

                                // Assignment change (H/L)
                                var beforeH = IsIn(n.HeaderColumns, srcCol);
                                var beforeL = IsIn(n.LineColumns, srcCol);

                                RemoveCI(n.HeaderColumns, srcCol);
                                RemoveCI(n.LineColumns, srcCol);
                                if (part.Equals("H", StringComparison.OrdinalIgnoreCase)) { AddUniqueCI(n.HeaderColumns, srcCol); }
                                else if (part.Equals("L", StringComparison.OrdinalIgnoreCase)) { AddUniqueCI(n.LineColumns, srcCol); }

                                var afterH = IsIn(n.HeaderColumns, srcCol);
                                var afterL = IsIn(n.LineColumns, srcCol);
                                if (beforeH != afterH || beforeL != afterL)
                                {
                                    string from = beforeH ? "H" : beforeL ? "L" : "-";
                                    string to = afterH ? "H" : afterL ? "L" : "-";
                                    report.NormalizeAssignmentsUpdated++;
                                    report.AffectedTables.Add(s.SourceTable);
                                    report.Messages.Add($"[{s.SourceTable}] {srcCol} Assign: {from} -> {to}");
                                }
                            }
                        }
                    }
                    else { var m = "Normalize assignments CSV not found: " + normalizeAssignCsvPath; errors.Add(m); logger?.Warn(m); }
                }
                catch (Exception ex) { var m = "Failed importing normalize assignments CSV '" + normalizeAssignCsvPath + "': " + ex.Message; errors.Add(m); logger?.Error(m); }
            }

            // Save updated files
            int saved = 0;
            foreach (var kv in map)
            {
                try
                {
                    File.WriteAllText(kv.Value.Item1, JsonConvert.SerializeObject(kv.Value.Item2, Formatting.Indented));
                    saved++;
                }
                catch (Exception ex) { var m = "Failed to save schema '" + kv.Value.Item1 + "': " + ex.Message; errors.Add(m); logger?.Error(m); }
            }
            logger?.Info("Schemas saved: " + saved);

            if (errors.Count > 0) return 1;
            return 0;
        }

        // Existing bulk API (kept for backward compatibility)
        public static int ExportAll(string migrationsDir,
            out string tablesCsvPath,
            out string columnsCsvPath,
            out string normalizeCsvPath,
            out string normalizeAssignCsvPath)
        {
            var defaultOut = Path.Combine(migrationsDir, "Metadata", "PlanEdits");
            using (var log = CreateLogger(defaultOut, "Export"))
            {
                var rc = ExportSelected(migrationsDir, defaultOut, ExportKind.All,
                    out var paths, out var errors, log)
                    .WithPaths(out tablesCsvPath, out columnsCsvPath, out normalizeCsvPath, out normalizeAssignCsvPath, paths, errors);
                if (errors.Count > 0) foreach (var e in errors) log?.Error(e);
                log?.Info("ExportAll finished with code " + rc);
                return rc;
            }
        }

        public static int ImportAll(string migrationsDir,
            string tablesCsvPath,
            string columnsCsvPath,
            string normalizeCsvPath,
            string normalizeAssignCsvPath)
        {
            var defaultBase = Path.Combine(migrationsDir, "Metadata", "PlanEdits");
            using (var log = CreateLogger(defaultBase, "Import"))
            {
                var rc = ImportSelected(migrationsDir, tablesCsvPath, columnsCsvPath, normalizeCsvPath, normalizeAssignCsvPath, out var report, out var errors, log);
                if (report != null)
                {
                    Console.WriteLine("Import summary: Tables={0}, Columns={1}, NormalizeMeta={2}, Assignments={3}",
                        report.TablesUpdated, report.ColumnPlansUpdated, report.NormalizeMetaUpdated, report.NormalizeAssignmentsUpdated);
                    log?.Info("Import summary: Tables=" + report.TablesUpdated + " Columns=" + report.ColumnPlansUpdated +
                              " NormalizeMeta=" + report.NormalizeMetaUpdated + " Assignments=" + report.NormalizeAssignmentsUpdated);
                }
                if (errors.Count > 0)
                {
                    Console.Error.WriteLine("Errors (" + errors.Count + "):");
                    foreach (var e in errors.Take(10))
                    {
                        Console.Error.WriteLine("  - " + e);
                        log?.Error(e);
                    }
                    if (errors.Count > 10) Console.Error.WriteLine("  ... " + (errors.Count - 10) + " more");
                }
                log?.Info("ImportAll finished with code " + rc);
                return rc;
            }
        }

        // Helpers

        private static int WithPaths(this int rc,
            out string tablesCsvPath,
            out string columnsCsvPath,
            out string normalizeCsvPath,
            out string normalizeAssignCsvPath,
            ExportPaths p,
            List<string> errors)
        {
            tablesCsvPath = p.TablesCsvPath;
            columnsCsvPath = p.ColumnsCsvPath;
            normalizeCsvPath = p.NormalizeCsvPath;
            normalizeAssignCsvPath = p.NormalizeAssignCsvPath;

            if (errors.Count > 0 && rc == 0) rc = 1;
            if (rc == 0)
            {
                Console.WriteLine("Bulk plan exported:");
                if (!string.IsNullOrEmpty(tablesCsvPath)) Console.WriteLine("  Tables:      " + tablesCsvPath);
                if (!string.IsNullOrEmpty(columnsCsvPath)) Console.WriteLine("  Columns:     " + columnsCsvPath);
                if (!string.IsNullOrEmpty(normalizeCsvPath)) Console.WriteLine("  Normalize:   " + normalizeCsvPath);
                if (!string.IsNullOrEmpty(normalizeAssignCsvPath)) Console.WriteLine("  Assignments: " + normalizeAssignCsvPath);
            }
            return rc;
        }

        private static bool SafeWriteFile(string path, string content, List<string> errors)
        {
            try
            {
                File.WriteAllText(path, content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                errors.Add("Failed to write '" + path + "': " + ex.Message);
                return false;
            }
        }

        private static void EnsurePlan(TableSchema s)
        {
            if (s.Plan == null)
                s.Plan = new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
            if (s.Plan.ColumnActions == null)
                s.Plan.ColumnActions = new List<ColumnPlan>();
        }

        private static void EnsureNormalize(TableSchema s)
        {
            if (s.Plan.Normalize == null)
                s.Plan.Normalize = new NormalizePlan
                {
                    HeaderColumns = new List<string>(),
                    LineColumns = new List<string>(),
                    OldCompositeKey = new List<string>(),
                    HeaderPrimaryKey = new List<string>(),
                    LinePrimaryKey = new List<string>()
                };
            if (s.Plan.Normalize.HeaderColumns == null) s.Plan.Normalize.HeaderColumns = new List<string>();
            if (s.Plan.Normalize.LineColumns == null) s.Plan.Normalize.LineColumns = new List<string>();
            if (s.Plan.Normalize.OldCompositeKey == null) s.Plan.Normalize.OldCompositeKey = new List<string>();
            if (s.Plan.Normalize.HeaderPrimaryKey == null) s.Plan.Normalize.HeaderPrimaryKey = new List<string>();
            if (s.Plan.Normalize.LinePrimaryKey == null) s.Plan.Normalize.LinePrimaryKey = new List<string>();
        }

        private static List<string> Parse(string line)
        {
            var res = new List<string>(); if (line == null) return res;
            var sb = new StringBuilder(); bool inQ = false;
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (inQ)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else inQ = false;
                    }
                    else sb.Append(ch);
                }
                else
                {
                    if (ch == ',') { res.Add(sb.ToString()); sb.Clear(); }
                    else if (ch == '"') inQ = true;
                    else sb.Append(ch);
                }
            }
            res.Add(sb.ToString());
            return res;
        }

        private static int IndexOf(List<string> header, string name)
        {
            for (int i = 0; i < header.Count; i++)
                if (string.Equals(header[i], name, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        private static string Get(List<string> cols, int idx) => idx >= 0 && idx < cols.Count ? cols[idx] : null;

        private static string Csv(string v)
        {
            if (v == null) return "";
            if (v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0) return "\"" + v.Replace("\"", "\"\"") + "\"";
            return v;
        }

        private static string JoinPipe(List<string> list)
        {
            if (list == null || list.Count == 0) return "";
            return string.Join("|", list);
        }

        private static List<string> ParsePipe(string v)
        {
            var res = new List<string>();
            if (string.IsNullOrWhiteSpace(v)) return res;
            foreach (var p in v.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var s = p.Trim();
                if (s.Length > 0) res.Add(s);
            }
            return res;
        }

        private static bool IsIn(List<string> list, string col)
        {
            return list != null && list.Any(x => string.Equals(x, col, StringComparison.OrdinalIgnoreCase));
        }

        private static void RemoveCI(List<string> list, string col)
        {
            if (list == null) return;
            list.RemoveAll(x => string.Equals(x, col, StringComparison.OrdinalIgnoreCase));
        }

        private static void AddUniqueCI(List<string> list, string col)
        {
            if (list == null) return;
            if (!IsIn(list, col)) list.Add(col);
        }

        // Property-safe compare-and-assign helpers (avoid ref on properties)

        private static bool AssignIfChanged(string oldValue, string newValue, Action<string> setter, StringComparison comparison)
        {
            var existing = oldValue ?? "";
            var incoming = newValue ?? "";
            if (!string.Equals(existing, incoming, comparison))
            {
                setter(newValue);
                return true;
            }
            return false;
        }

        private static bool AssignIfChanged(bool oldValue, bool newValue, Action<bool> setter)
        {
            if (oldValue != newValue)
            {
                setter(newValue);
                return true;
            }
            return false;
        }

        private static bool UpdateListIfDifferent(List<string> target, List<string> incoming)
        {
            incoming = incoming ?? new List<string>();
            target = target ?? new List<string>();
            var equal = target.Count == incoming.Count &&
                        target.Zip(incoming, (a, b) => string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase)).All(x => x);
            if (!equal)
            {
                target.Clear();
                target.AddRange(incoming);
                return true;
            }
            return false;
        }

        // Simple file logger

        private sealed class Logger : IDisposable
        {
            private readonly StreamWriter _writer;
            public string LogPath { get; }

            public Logger(string path)
            {
                LogPath = path;
                _writer = new StreamWriter(path, false, Encoding.UTF8) { AutoFlush = true };
                Info("Log created");
            }

            public void Info(string msg) { Write("INFO", msg); }
            public void Warn(string msg) { Write("WARN", msg); }
            public void Error(string msg) { Write("ERROR", msg); }

            private void Write(string level, string msg)
            {
                var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + level + "] " + msg;
                try { _writer.WriteLine(line); }
                catch { /* ignore logging errors */ }
            }

            public void Dispose()
            {
                try { _writer.Dispose(); } catch { /* ignore */ }
            }
        }

        private static Logger CreateLogger(string baseDir, string prefix)
        {
            try
            {
                var logsDir = Path.Combine(baseDir ?? "", "Logs");
                Directory.CreateDirectory(logsDir);
                var path = Path.Combine(logsDir, prefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
                return new Logger(path);
            }
            catch
            {
                return null;
            }
        }
    }
}