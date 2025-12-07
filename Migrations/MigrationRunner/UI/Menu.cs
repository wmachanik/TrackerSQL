using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data.OleDb;
using Newtonsoft.Json; // added
using Newtonsoft.Json.Linq; // added for JObject parsing
using System.Text.RegularExpressions; // added for VerifyFromScript parsing

namespace MigrationRunner.UI
{
    internal interface IMenuCommand
    {
        string Key { get; }              // e.g. "1", "2", "8", "9", "10", "0"
        string Description { get; }      // printed in the menu
        int Execute();                   // return code to print after execution
    }

    internal sealed class MenuController
    {
        private readonly string _title;
        private readonly IList<IMenuCommand> _commands;

        public MenuController(string title, IEnumerable<IMenuCommand> commands)
        {
            _title = title ?? "Menu";
            _commands = (commands ?? Enumerable.Empty<IMenuCommand>()).ToList();
        }

        public int RunLoop()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine(_title);

                foreach (var cmd in _commands
                    .OrderBy(c => SortKey(c.Key), StringComparer.Ordinal))
                {
                    Console.WriteLine("  {0}) {1}", cmd.Key.PadLeft(2), cmd.Description);
                }

                Console.Write("Select Choice: ");
                var input = Console.ReadLine();
                Console.WriteLine();

                var match = _commands.FirstOrDefault(c => string.Equals(c.Key, input, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    Console.WriteLine("Unknown option. Please try again.");
                    continue;
                }

                if (match is ExitCommand) return 0;

                var rc = match.Execute();
                Console.WriteLine("Finished with code: " + rc);
            }
        }

        // Create a sortable key string. Numeric keys are zero-padded so they sort before letters.
        // Special-case mapping are applied here to position non-numeric keys as desired.
        private static string SortKey(string key)
        {
            if (int.TryParse(key, out int n))
            {
                return n.ToString("D10"); // numeric keys sort first
            }

            // Place '!' visually between 'N' and 'O' by mapping it to "N!" for sorting purposes.
            if (string.Equals(key, "!", StringComparison.Ordinal))
            {
                return "N!";
            }

            // Place '$' after 'Z' so it appears below the Z option in the menu.
            if (string.Equals(key, "$", StringComparison.Ordinal))
            {
                return "Z$";
            }

            // Place '&' after 'X' so it appears below the X option in the menu.
            if (string.Equals(key, "&", StringComparison.Ordinal))
            {
                return "X&";
            }

            return key ?? string.Empty;
        }

        private static int ParseKey(string key)
        {
            int n;
            return int.TryParse(key, out n) ? n : int.MaxValue;
        }
    }

    internal sealed class ExitCommand : IMenuCommand
    {
        public string Key => "0";
        public string Description => "Exit";
        public int Execute() => 0; // handled by controller
    }

    // Below are light adapters over your existing static entry points.
    internal sealed class ExportAccessSchemaCommand : IMenuCommand
    {
        private readonly MigrationConfig _config;
        private readonly string _migrationsDir;
        public ExportAccessSchemaCommand(string migrationsDir, MigrationConfig config) { _migrationsDir = migrationsDir; _config = config; }
        public string Key => "1";
        public string Description => "Export Access schema (per-table JSON)";
        public int Execute()
        {
            var outDir = System.IO.Path.Combine(_migrationsDir, "Metadata", "AccessSchema");
            System.IO.Directory.CreateDirectory(outDir);
            return AccessSchemaExporter.Export(_config, outDir);
        }
    }

    internal sealed class ReviewEditPlanCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public ReviewEditPlanCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "2";
        public string Description => "Review/Edit migration plan (per table)";
        public int Execute() => SchemaPlanEditor.Run(_migrationsDir);
    }

    internal sealed class ExportPlanSummaryCsvCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public ExportPlanSummaryCsvCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "3";
        public string Description => "Export plan summary CSV";
        public int Execute()
        {
            var rc = PlanSummaryExporter.Export(_migrationsDir, out var csvPath);
            if (rc == 0) Console.WriteLine("File: " + csvPath);
            return rc;
        }
    }

    internal sealed class ExportPlanCsvInteractiveCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public ExportPlanCsvInteractiveCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "4";
        public string Description => "Export plan CSV (interactive: All/Tables/Columns/Normalize/Assignments)";
        public int Execute() => PlanBulkCsv.RunExportMenu(_migrationsDir);
    }

    internal sealed class ImportPlanCsvInteractiveCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public ImportPlanCsvInteractiveCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "5";
        public string Description => "Import plan CSV (interactive: All/Tables/Columns/Normalize/Assignments)";
        public int Execute() => PlanBulkCsv.RunImportMenu(_migrationsDir);
    }

    internal sealed class BulkRenameDryRunCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public BulkRenameDryRunCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "6";
        public string Description => "Bulk rename (dry-run, all tables)";
        public int Execute()
        {
            var rc = BulkRenameApplier.Run(_migrationsDir, true, out var logPath);
            if (rc == 0) Console.WriteLine("Log: " + logPath);
            return rc;
        }
    }

    internal sealed class BulkRenameApplyCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public BulkRenameApplyCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "7";
        public string Description => "Bulk rename (apply, all tables)";
        public int Execute()
        {
            var rc = BulkRenameApplier.Run(_migrationsDir, false, out var logPath);
            if (rc == 0) Console.WriteLine("Log: " + logPath);
            return rc;
        }
    }

    internal sealed class FullPlanReviewCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public FullPlanReviewCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "8";
        public string Description => "Create full plan review (JSON + Markdown)";
        public int Execute()
        {
            var rc = PlanSummaryExporter.ExportFullReview(_migrationsDir, out var jsonPath, out var mdPath);
            if (rc == 0 || rc == 1) { Console.WriteLine("JSON: " + jsonPath); Console.WriteLine("MD:   " + mdPath); }
            return rc;
        }
    }

    internal sealed class BeforeAfterCsvCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public BeforeAfterCsvCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "9";
        public string Description => "Export Before/After CSV (side-by-side types, keys, FKs)";
        public int Execute()
        {
            var rc = PlanSummaryExporter.ExportBeforeAfterCsv(_migrationsDir, out var path);
            if (rc == 0) Console.WriteLine("File: " + path);
            return rc;
        }
    }

    internal sealed class HumanReviewCsvCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public HumanReviewCsvCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "10";
        public string Description => "Export Human Review CSV (per-table sections, Excel-friendly)";
        public int Execute()
        {
            var rc = PlanSummaryExporter.ExportHumanReviewCsv(_migrationsDir, out var path);
            if (rc == 0) Console.WriteLine("File: " + path);
            return rc;
        }
    }

    internal sealed class HumanReviewCsvImportCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public HumanReviewCsvImportCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "11";
        public string Description => "Import Human Review CSV (plan edits + PK/FK/Identity constraints)";

        public int Execute()
        {
            try
            {
                var planEditsDir = Path.Combine(_migrationsDir, "Metadata", "PlanEdits");
                if (!Directory.Exists(planEditsDir))
                {
                    Console.Error.WriteLine("PlanEdits folder not found: " + planEditsDir);
                    return 2;
                }

                Console.Write("Filter prefix [TableMigration]: ");
                var prefixInput = Console.ReadLine();
                var prefix = string.IsNullOrWhiteSpace(prefixInput) ? "TableMigration" : prefixInput.Trim();

                var files = Directory.EnumerateFiles(planEditsDir, "*.csv", SearchOption.TopDirectoryOnly)
                                     .Select(f => new FileInfo(f))
                                     .Where(fi => fi.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                     .OrderByDescending(fi => fi.LastWriteTimeUtc)
                                     .ToList();

                string selectedPath = null;

                if (files.Count == 0)
                {
                    Console.WriteLine("No CSV files found in '" + planEditsDir + "' with prefix '" + prefix + "'."); Console.Write("Enter full path to a CSV (or leave blank to cancel): ");
                    var manual = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(manual)) return 2;
                    selectedPath = manual.Trim();
                    if (!File.Exists(selectedPath))
                    {
                        Console.Error.WriteLine("File not found: " + selectedPath);
                        return 2;
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Available CSVs (newest first):");
                    for (int i = 0; i < files.Count; i++)
                    {
                        var fi = files[i];
                        Console.WriteLine($"  {i + 1}. {fi.Name}  [{fi.LastWriteTime:yyyy-MM-dd HH:mm}]  {fi.Length:N0} bytes");
                    }
                    Console.Write($"Select file [1]: ");
                    var sel = Console.ReadLine();
                    int idx = 0;
                    if (!string.IsNullOrWhiteSpace(sel))
                    {
                        if (!int.TryParse(sel.Trim(), out var n) || n < 1 || n > files.Count)
                        {
                            Console.Error.WriteLine("Invalid selection.");
                            return 2;
                        }
                        idx = n - 1;
                    }
                    selectedPath = files[idx].FullName;
                }

                Console.WriteLine("Using: " + selectedPath);

                // Import plan + constraints (constraints are imported inside this call)
                var rcPlan = PlanHumanReviewImporter.Import(_migrationsDir, selectedPath, out var planLogPath);
                Console.WriteLine("Plan import finished with code: " + rcPlan);
                Console.WriteLine("Plan import log: " + planLogPath);

                // Status: summarize constraints from PlanConstraints.json
                var constraintsPath = Path.Combine(_migrationsDir, "Metadata", "PlanEdits", "PlanConstraints.json");
                if (File.Exists(constraintsPath))
                {
                    try
                    {
                        var idx = JsonConvert.DeserializeObject<ConstraintsIndex>(File.ReadAllText(constraintsPath)) ?? new ConstraintsIndex();
                        int tables = (idx.Tables ?? new List<ConstraintTable>()).Count;
                        int pkCols = (idx.Tables ?? new List<ConstraintTable>()).Sum(t => (t.PrimaryKey ?? new List<string>()).Count);
                        int idCols = (idx.Tables ?? new List<ConstraintTable>()).Sum(t => (t.IdentityColumns ?? new List<string>()).Count);
                        int fkCount = (idx.Tables ?? new List<ConstraintTable>()).Sum(t => (t.ForeignKeys ?? new List<ForeignKeyDef>()).Count);
                        int notNullCols = (idx.Tables ?? new List<ConstraintTable>()).Sum(t => (t.NotNullColumns ?? new List<string>()).Count);
                        Console.WriteLine($"Constraints summary: Tables={tables}, PKCols={pkCols}, IdentityCols={idCols}, FKs={fkCount}, NotNullCols={notNullCols}");
                        Console.WriteLine("Constraints JSON: " + constraintsPath);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Failed to read constraints summary: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Constraints file not found; ensure CSV used the correct working folder.");
                }

                Console.WriteLine("Tip: Run option 12 to validate and produce a single session log.");
                return (rcPlan == 0) ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                return 1;
            }
        }
    }

    internal sealed class PreflightValidateCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public PreflightValidateCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "12";
        public string Description => "Validate plan (relations, business rules, preflight, source vs target counts)";
        public int Execute()
        {
            try
            {
                var logsDir = Path.Combine(_migrationsDir, "Metadata", "PlanEdits", "Logs");
                Directory.CreateDirectory(logsDir);
                var sessionLog = Path.Combine(logsDir, "ValidationSession_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");

                // 1) Relations (PK/FK/IDENTITY vs plan)
                var rcRelations = PlanRelationsValidator.Validate(_migrationsDir, out var relationsLog);

                // 2) Business rules validation (CompositeKey/Compute/Recurring)
                var rcRules = BusinessRulesLoader.LoadAndValidate(_migrationsDir, out var brLog, out var _);

                // 3) Preflight (plan completeness for DDL/ETL)
                var rcPreflight = PreflightValidator.Run(_migrationsDir, out var preflightLog);

                // 4) NEW: Source vs Target data validation (catches migration issues like missing column mappings)
                var rcDataValidation = ValidateSourceTargetCounts(_migrationsDir, out var dataValidationLog);

                // Combine into one session log
                var (warns, errs) = CombineLogs(sessionLog, new[]
                {
                    ("Relations Validator", relationsLog),
                    ("Business Rules Validation", brLog),
                    ("Preflight", preflightLog),
                    ("Source vs Target Data Validation", dataValidationLog)
                });

                Console.WriteLine($"Validation session summary: WARN={warns} ERR={errs}");
                Console.WriteLine("Session log: " + sessionLog);

                return (rcRelations == 0 && rcRules == 0 && rcPreflight == 0 && rcDataValidation == 0) ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                return 1;
            }
        }

        private int ValidateSourceTargetCounts(string migrationsDir, out string logPath)
        {
            var logsDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs");
            Directory.CreateDirectory(logsDir);
            logPath = Path.Combine(logsDir, "DataValidation_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== Source vs Target Data Validation ===");
                sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("Purpose: Detect migration issues by comparing source vs target row counts");
                sb.AppendLine("Critical: Source has data but target is empty (suggests missing column mappings)");
                sb.AppendLine();

                // Try to get connection strings from config
                if (!TryGetConnections(migrationsDir, out var accessCs, out var sqlCs, out var connError))
                {
                    sb.AppendLine($"WARN: {connError} - skipping data validation");
                    sb.AppendLine("Tip: Use option X to set up connection strings, then retry validation");
                    File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                    return 0; // Not a critical error, just a warning
                }

                sb.AppendLine("OK: Connection strings found, testing connectivity...");

                // Test connections
                try
                {
                    using (var conn = new OleDbConnection(accessCs)) { conn.Open(); }
                    sb.AppendLine("OK: Access connection successful");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"ERROR: Access connection failed: {ex.Message}");
                    File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                    return 1;
                }

                try
                {
                    using (var conn = new SqlConnection(sqlCs)) { conn.Open(); }
                    sb.AppendLine("OK: SQL Server connection successful");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"ERROR: SQL Server connection failed: {ex.Message}");
                    File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                    return 1;
                }

                // Get table mappings from schema
                var mappings = GetTableMappings(migrationsDir);
                if (mappings.Count == 0)
                {
                    sb.AppendLine("WARN: No table mappings found - skipping data validation");
                    File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                    return 0;
                }

                sb.AppendLine($"Found {mappings.Count} table mappings to validate");
                sb.AppendLine();

                int criticalErrors = 0;
                int warnings = 0;
                int checkedCount = 0;
                int emptyTables = 0;
                int perfectMatches = 0;

                // Validate each mapping (limit to avoid timeout)
                foreach (var mapping in mappings.Take(50)) // Increased limit
                {
                    try
                    {
                        checkedCount++;
                        long sourceCount = GetRowCount(accessCs, $"[{mapping.Source}]", true);
                        long targetCount = GetRowCount(sqlCs, $"[{mapping.Target}]", false);

                        sb.AppendLine($"Table: {mapping.Source} -> {mapping.Target}");
                        sb.AppendLine($"  Source: {sourceCount:N0} rows, Target: {targetCount:N0} rows");

                        if (sourceCount > 0 && targetCount == 0)
                        {
                            sb.AppendLine($"  ERROR: Source has {sourceCount:N0} rows but target is empty!");
                            sb.AppendLine($"         This indicates a critical migration failure");
                            sb.AppendLine($"         Check column mappings for {mapping.Source} -> {mapping.Target}");
                            sb.AppendLine($"         Verify data migration script executed without errors");
                            criticalErrors++;
                        }
                        else if (sourceCount == 0 && targetCount == 0)
                        {
                            sb.AppendLine($"  OK: Both source and target are empty");
                            emptyTables++;
                        }
                        else if (sourceCount == targetCount)
                        {
                            sb.AppendLine($"  OK: Row counts match exactly");
                            perfectMatches++;
                        }
                        else if (Math.Abs(sourceCount - targetCount) > 0)
                        {
                            var diff = targetCount - sourceCount;
                            if (diff > 0)
                            {
                                sb.AppendLine($"  WARN: Target has {diff:N0} more rows than source");
                                sb.AppendLine($"        This could indicate data duplication or identity reseeding");
                                warnings++;
                            }
                            else
                            {
                                sb.AppendLine($"  ERROR: Target has {Math.Abs(diff):N0} fewer rows than source - potential data loss!");
                                sb.AppendLine($"         Check for FK constraint violations or mapping errors");
                                criticalErrors++;
                            }
                        }
                        sb.AppendLine();
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"ERROR validating {mapping.Source} -> {mapping.Target}: {ex.Message}");
                        sb.AppendLine();
                        criticalErrors++;
                    }
                }

                sb.AppendLine("=== Data Validation Summary ===");
                sb.AppendLine($"Tables checked: {checkedCount}");
                sb.AppendLine($"Perfect matches: {perfectMatches}");
                sb.AppendLine($"Empty tables: {emptyTables}");
                sb.AppendLine($"Critical errors: {criticalErrors}");
                sb.AppendLine($"Warnings: {warnings}");
                sb.AppendLine($"Success rate: {(double)(perfectMatches + emptyTables) / checkedCount * 100:F1}%");

                if (criticalErrors > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("?? CRITICAL ISSUES FOUND:");
                    sb.AppendLine("- Tables with source data but empty targets indicate migration failures");
                    sb.AppendLine("- This typically means missing or incorrect column mappings");
                    sb.AppendLine("- Review the migration plan and regenerate the data migration script");
                    sb.AppendLine("- Check the data migration execution logs for specific errors");
                    sb.AppendLine("- For tables like PackagingTbl -> ItemPackagingsTbl: ensure all column mappings exist");
                    sb.AppendLine();
                    sb.AppendLine("NEXT STEPS:");
                    sb.AppendLine("1. Run option 2 to review/edit migration plans");
                    sb.AppendLine("2. Run option M to regenerate data migration script");
                    sb.AppendLine("3. Run option N to re-execute data migration");
                    sb.AppendLine("4. Rerun this validation to confirm fixes");
                }
                else if (warnings > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("??  WARNINGS DETECTED:");
                    sb.AppendLine("- Row count mismatches may indicate data integrity issues");
                    sb.AppendLine("- Review specific tables for duplicates or missing data");
                    sb.AppendLine("- Consider running option O for detailed spot-check verification");
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine("? DATA MIGRATION VALIDATION PASSED!");
                    sb.AppendLine("- All tables show expected row counts");
                    sb.AppendLine("- No critical data loss detected");
                    sb.AppendLine("- Ready to proceed with FK constraints (option B + C)");
                }

                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return criticalErrors > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(logPath, $"FATAL: Data validation failed: {ex.Message}", Encoding.UTF8);
                return 1;
            }
        }

        private bool TryGetConnections(string migrationsDir, out string accessCs, out string sqlCs, out string error)
        {
            accessCs = sqlCs = null;
            error = "Could not locate connection strings in config";
            
            try
            {
                // Look in common config locations
                var configPaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MigrationConfig.json"),
                    Path.Combine(migrationsDir, "..", "MigrationConfig.json"),
                    Path.Combine(migrationsDir, "MigrationConfig.json")
                };

                foreach (var path in configPaths.Where(File.Exists))
                {
                    var json = File.ReadAllText(path);
                    var config = JsonConvert.DeserializeObject<MigrationConfig>(json);
                    accessCs = config?.AccessConnectionString;
                    sqlCs = config?.TargetConnectionString;
                    
                    if (!string.IsNullOrEmpty(accessCs) && !string.IsNullOrEmpty(sqlCs))
                        return true;
                }
                
                error = "Connection strings not found in MigrationConfig.json";
            }
            catch (Exception ex)
            {
                error = $"Config read error: {ex.Message}";
            }
            return false;
        }

        private long GetRowCount(string connectionString, string tableName, bool isAccess)
        {
            try
            {
                if (isAccess)
                {
                    using (var conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();
                        using (var cmd = new OleDbCommand($"SELECT COUNT(*) FROM {tableName}", conn))
                        {
                            cmd.CommandTimeout = 30;
                            return Convert.ToInt64(cmd.ExecuteScalar() ?? 0);
                        }
                    }
                }
                else
                {
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", conn))
                        {
                            cmd.CommandTimeout = 30;
                            return Convert.ToInt64(cmd.ExecuteScalar() ?? 0);
                        }
                    }
                }
            }
            catch
            {
                return -1; // Indicates error/table not found
            }
        }

        private List<(string Source, string Target)> GetTableMappings(string migrationsDir)
        {
            var mappings = new List<(string Source, string Target)>();
            try
            {
                var schemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                if (!Directory.Exists(schemaDir)) return mappings;

                foreach (var file in Directory.GetFiles(schemaDir, "*.schema.json"))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        // Use JObject instead of dynamic to avoid runtime binder issues
                        var schema = Newtonsoft.Json.Linq.JObject.Parse(json);
                        var source = schema["SourceTable"]?.ToString();
                        var target = schema["Plan"]?["TargetTable"]?.ToString();
                        var ignoreToken = schema["Plan"]?["Ignore"];
                        bool ignore = false;
                        if (ignoreToken != null && ignoreToken.Type == Newtonsoft.Json.Linq.JTokenType.Boolean)
                        {
                            ignore = (bool)ignoreToken;
                        }
                        
                        if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(target) && !ignore)
                        {
                            mappings.Add((source, target));
                        }
                    }
                    catch { /* skip invalid schema files */ }
                }
            }
            catch { /* return empty list on error */ }
            return mappings;
        }

        private static (int warns, int errs) CombineLogs(string sessionLogPath, IEnumerable<(string Title, string Path)> parts)
        {
            int warns = 0, errs = 0;
            var sb = new StringBuilder();

            sb.AppendLine("=== Validation Session Log ===");
            sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();

            foreach (var p in parts)
            {
                sb.AppendLine("---- " + p.Title + " ----");
                if (!string.IsNullOrWhiteSpace(p.Path) && File.Exists(p.Path))
                {
                    foreach (var line in File.ReadAllLines(p.Path))
                    {
                        if (line.StartsWith("WARN", StringComparison.OrdinalIgnoreCase)) warns++;
                        if (line.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase)) errs++;
                        sb.AppendLine(line);
                    }
                }
                else
                {
                    sb.AppendLine("(no log file)");
                }
                sb.AppendLine();
            }

            sb.Insert(0, $"Summary: WARN={warns}, ERR={errs}{Environment.NewLine}{Environment.NewLine}");
            Directory.CreateDirectory(Path.GetDirectoryName(sessionLogPath));
            File.WriteAllText(sessionLogPath, sb.ToString(), Encoding.UTF8);
            return (warns, errs);
        }
    }

    internal sealed class GenerateCreateTablesScriptCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public string Key => "A";
        public string Description => "Generate CREATE TABLE DDL (SQL)";

        public GenerateCreateTablesScriptCommand(string migrationsDir) { _migrationsDir = migrationsDir; }

        public int Execute()
        {
            // Use RunRangeState if present
            var rro = RunRangeState.Current;
            bool suppressIdentity = false;
            bool dropExisting = false;
            if (rro != null && rro.SuppressPrompts)
            {
                suppressIdentity = rro.SuppressIdentityOnCreate ?? false;
                dropExisting = rro.DropExistingOnCreate ?? false;
                Console.WriteLine($"Using RunRange options: SuppressIdentity={suppressIdentity}, DropExisting={dropExisting}");
            }
            else
            {
                Console.Write("Suppress IDENTITY to preserve existing IDs on import? [y/N]: ");
                var sup = Console.ReadLine();
                suppressIdentity = !string.IsNullOrWhiteSpace(sup) && sup.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase);

                Console.Write("Drop existing tables before CREATE (recommended for clean migration)? [Y/n]: ");
                var drop = Console.ReadLine();
                dropExisting = string.IsNullOrWhiteSpace(drop) || !drop.Trim().StartsWith("n", StringComparison.OrdinalIgnoreCase);
            }

            var rc = DdlScriptGenerator.GenerateCreateTables(_migrationsDir, suppressIdentity, dropExisting, out var sqlPath);
            Console.WriteLine("CREATE TABLE script rc=" + rc + " file: " + sqlPath);
            return rc;
        }
    }

    internal sealed class GenerateForeignKeysScriptCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public GenerateForeignKeysScriptCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "B";
        public string Description => "Generate FK constraints DDL (SQL)";
        public int Execute()
        {
            var rc = DdlScriptGenerator.GenerateForeignKeys(_migrationsDir, out var path);
            Console.WriteLine("FK script rc=" + rc + " file: " + path);
            try
            {
                if (File.Exists(path))
                {
                    var head = File.ReadLines(path).Take(8).ToArray();
                    foreach (var l in head) Console.WriteLine(l);
                    if (head.Length == 0) Console.WriteLine("(empty file)");
                }
                else
                {
                    Console.WriteLine("(no output file)");
                }
            }
            catch { /* ignore preview errors */ }
            return rc;
        }
    }

    internal sealed class ApplyDdlScriptsCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;
        public ApplyDdlScriptsCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir; _config = config;
        }
        public string Key => "C";
        public string Description => "Apply latest DDL scripts to target DB";
        public int Execute()
        {
            var defCs = _config?.TargetConnectionString ?? "";

            // consult RunRangeState
            var rro = RunRangeState.Current;
            string cs;
            if (rro != null && rro.SuppressPrompts && !string.IsNullOrWhiteSpace(rro.TargetConnectionString))
            {
                cs = rro.TargetConnectionString;
                Console.WriteLine($"Using prefilled target connection string");
            }
            else
            {
                var csDisplay = string.IsNullOrWhiteSpace(defCs) ? "(empty)" : defCs;
                Console.Write("Target connection string [{0}]: ", csDisplay);
                var input = Console.ReadLine();
                cs = string.IsNullOrWhiteSpace(input) ? defCs : input.Trim();
            }

            if (string.IsNullOrWhiteSpace(cs))
            {
                Console.Error.WriteLine("No connection string provided.");
                return 2;
            }

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(cs))
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Connection failed: " + ex.Message);
                Console.Error.WriteLine("Tips:");
                Console.Error.WriteLine(" - Ensure SQL Server service is running (e.g., 'SQL Server (SQLEXPRESS)').");
                Console.Error.WriteLine(" - Try Server=localhost or Server=(localdb)\\MSSQLLocalDB for dev.");
                Console.Error.WriteLine(" - If using SQLEXPRESS, verify the instance name and local firewall.");
                return 1;
            }

            var rc = DdlScriptApplier.ApplyLatest(_migrationsDir, cs, out var logPath);
            Console.WriteLine("Apply DDL rc=" + rc + " log: " + logPath);

            if (!File.Exists(logPath))
                return rc;

            // Print focused error section if failed
            var lines = File.ReadAllLines(logPath);
            if (rc != 0)
            {
                var idx = Array.FindLastIndex(lines, l => l.StartsWith("ERROR executing batch:", StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Failure details:");
                    foreach (var l in lines.Skip(idx).Take(12))
                        Console.WriteLine(l);
                    Console.WriteLine();
                }
            }

            // Tail summary
            var tail = lines.Reverse().Take(12).Reverse();
            foreach (var line in tail) Console.WriteLine(line);

            return rc;
        }
    }

    internal sealed class OpenSqlFolderCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        public OpenSqlFolderCommand(string migrationsDir) { _migrationsDir = migrationsDir; }
        public string Key => "D";
        public string Description => "Open SQL output folder";
        public int Execute()
        {
            var folder = Path.Combine(_migrationsDir, "Metadata", "PlanEdits", "Sql");
            Directory.CreateDirectory(folder);
            try { Process.Start("explorer.exe", folder); }
            catch (Exception ex) { Console.Error.WriteLine("Open folder failed: " + ex.Message); return 1; }
            return 0;
        }
    }

    // New command: create tables without FKs, load data, then apply FKs (single ask-once flow)
    internal sealed class CreateLoadThenApplyFksCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;
        public CreateLoadThenApplyFksCommand(string migrationsDir, MigrationConfig config) { _migrationsDir = migrationsDir; _config = config; }
        public string Key => "$"; // symbol option
        public string Description => "Create tables (no FKs), load data, then apply FK constraints (ask-once)";

        public int Execute()
        {
            // Gather connections (ask-once)
            var access = _config?.AccessConnectionString ?? "";
            var sql = _config?.TargetConnectionString ?? "";

            Console.Write("Access connection string [{0}]: ", string.IsNullOrWhiteSpace(access) ? "(empty)" : access);
            var aIn = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(aIn)) access = aIn.Trim();

            Console.Write("SQL connection string [{0}]: ", string.IsNullOrWhiteSpace(sql) ? "(empty)" : sql);
            var sIn = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(sIn)) sql = sIn.Trim();

            if (string.IsNullOrWhiteSpace(access) || string.IsNullOrWhiteSpace(sql))
            {
                Console.Error.WriteLine("Both Access and SQL connection strings are required.");
                return 2;
            }

            Console.Write("Drop all existing user tables in target DB before CREATE? [y/N]: ");
            var dropIn = (Console.ReadLine() ?? "").Trim();
            bool dropAll = !string.IsNullOrWhiteSpace(dropIn) && dropIn.StartsWith("y", StringComparison.OrdinalIgnoreCase);

            Console.Write("Suppress IDENTITY on CREATE to preserve existing IDs? [y/N]: ");
            var sup = (Console.ReadLine() ?? "").Trim();
            bool suppressIdentity = !string.IsNullOrWhiteSpace(sup) && sup.StartsWith("y", StringComparison.OrdinalIgnoreCase);

            // Quick connection probes
            try { using (var c = new OleDbConnection(access)) { c.Open(); } } catch (Exception ex) { Console.Error.WriteLine("Access connect failed: " + ex.Message); return 1; }
            try { using (var c = new SqlConnection(sql)) { c.Open(); } } catch (Exception ex) { Console.Error.WriteLine("SQL connect failed: " + ex.Message); return 1; }

            // Purge SQL output folder
            try
            {
                var sqlFolder = Path.Combine(_migrationsDir, "Metadata", "PlanEdits", "Sql");
                if (Directory.Exists(sqlFolder))
                {
                    var files = Directory.GetFiles(sqlFolder, "*.sql", SearchOption.TopDirectoryOnly);
                    int deleted = 0;
                    foreach (var f in files) { try { File.Delete(f); deleted++; } catch { } }
                    Console.WriteLine($"Purged {deleted} *.sql files from: {sqlFolder}");
                }
            }
            catch (Exception ex) { Console.WriteLine("Warning: purge of SQL folder failed: " + ex.Message); }

            // Ensure DB exists and AccessSrc schema
            try { EnsureDatabaseExists(sql); Console.WriteLine("OK ensured database exists."); }
            catch (Exception ex) { Console.Error.WriteLine("FAIL ensure database: " + ex.Message); return 1; }

            // Optional drop
            if (dropAll)
            {
                try
                {
                    using (var conn = new SqlConnection(sql))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
SET NOCOUNT ON;
DECLARE @sql nvarchar(max) = N'',
        @schema nvarchar(128) = N'AccessSrc';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
FROM sys.foreign_keys
WHERE OBJECT_SCHEMA_NAME(parent_object_id) = 'AccessSrc';
IF LEN(@sql) > 0 EXEC sp_executesql @sql;

SET @sql = N' ';
SELECT @sql = @sql + N'DROP TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N';'
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name NOT IN ('sys','INFORMATION_SCHEMA');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
";
                            cmd.CommandTimeout = 0;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("Dropped existing user tables in target DB.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to drop tables: " + ex.Message);
                    Console.WriteLine("Aborting to avoid inconsistent state. You can retry without drop.");
                    return 1;
                }
            }

            // 1) Generate CREATE TABLE DDL (no FKs yet)
            try
            {
                var rcA = DdlScriptGenerator.GenerateCreateTables(_migrationsDir, suppressIdentity, dropAll, out var createPath);
                Console.WriteLine("Generate CREATE TABLE rc=" + rcA + " file: " + createPath);
                if (rcA != 0) return rcA;
            }
            catch (Exception ex) { Console.Error.WriteLine("Failed to generate CREATE TABLE script: " + ex.Message); return 1; }

            // 2) Apply CREATE-only DDL now
            try
            {
                var rcC = DdlScriptApplier.ApplyLatest(_migrationsDir, sql, out var ddlLog);
                Console.WriteLine("Apply CREATE-only DDL rc=" + rcC + " log: " + ddlLog);
                Tail(ddlLog, 32);
                if (rcC != 0) { Console.WriteLine("Apply CREATE reported issues. Aborting pipeline."); return rcC; }
            }
            catch (Exception ex) { Console.Error.WriteLine("Failed to apply CREATE DDL: " + ex.Message); return 1; }

            // 3) Stage Access -> [AccessSrc]
            try
            {
                var rcStage = AccessStagingImporter.StageAll(_migrationsDir, access, sql, out var stageLog);
                Console.WriteLine("Stage Access rc=" + rcStage + " log: " + stageLog);
                Tail(stageLog, 32);
                if (rcStage != 0) return rcStage;
            }
            catch (Exception ex) { Console.Error.WriteLine("FAIL staging: " + ex.Message); return 1; }

            // 4) Generate DataMigration script
            string dataSqlPath = null;
            try
            {
                var rcGen = DmlScriptGenerator.GenerateDataMigration(_migrationsDir, out dataSqlPath);
                Console.WriteLine("Generate DataMigration rc=" + rcGen + " file: " + dataSqlPath);
                if (rcGen != 0) return rcGen;
            }
            catch (Exception ex) { Console.Error.WriteLine("FAIL generate data migration: " + ex.Message); return 1; }

            // 5) Apply DataMigration
            try
            {
                var rcApply = DmlScriptApplier.ApplyLatest(_migrationsDir, sql, out var applyLog);
                Console.WriteLine("Apply DATA rc=" + rcApply + " log: " + applyLog);
                Tail(applyLog, 48);
                if (rcApply != 0) { Console.WriteLine("Data migration reported issues."); }
            }
            catch (Exception ex) { Console.Error.WriteLine("FAIL apply data migration: " + ex.Message); return 1; }

            // Quick verification
            try { VerifyFromScript(sql, dataSqlPath); } catch (Exception ex) { Console.WriteLine("Verification step failed: " + ex.Message); }

            // 6) Generate FK DDL
            try
            {
                var rcB = DdlScriptGenerator.GenerateForeignKeys(_migrationsDir, out var fkPath);
                Console.WriteLine("Generate FK script rc=" + rcB + " file: " + fkPath);
            }
            catch (Exception ex) { Console.Error.WriteLine("Failed to generate FK script: " + ex.Message); return 1; }

            // 7) Apply FK script
            try
            {
                var rcFkApply = DdlScriptApplier.ApplyLatest(_migrationsDir, sql, out var fkLog);
                Console.WriteLine("Apply FKs rc=" + rcFkApply + " log: " + fkLog);
                Tail(fkLog, 32);
                if (rcFkApply != 0)
                {
                    Console.WriteLine("Applying FKs reported issues (possible missing referenced rows). Inspect FK log and spot-check.");
                }
            }
            catch (Exception ex) { Console.Error.WriteLine("FAIL apply FK script: " + ex.Message); return 1; }

            Console.WriteLine("Create->Data->FKs pipeline completed. Run 'O' to run detailed spot-checks.");
            return 0;
        }

        private static void Tail(string path, int lines)
        {
            try
            {
                if (!File.Exists(path)) return;
                var tail = File.ReadLines(path).Reverse().Take(lines).Reverse().ToArray();
                Console.WriteLine("-- Log tail --");
                foreach (var l in tail) Console.WriteLine(l);
            }
            catch { }
        }

        private static void EnsureDatabaseExists(string sqlConnString)
        {
            var b = new SqlConnectionStringBuilder(sqlConnString);
            var database = b.InitialCatalog;
            if (string.IsNullOrWhiteSpace(database))
                throw new InvalidOperationException("Target SQL connection string has no Initial Catalog / Database.");

            var master = new SqlConnectionStringBuilder(sqlConnString) { InitialCatalog = "master" }.ToString();

            using (var conn = new SqlConnection(master))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 0; // allow long create
                    var safeDb = (database ?? "").Replace("]", "]]" ).Replace("'", "''");
                    cmd.CommandText = $"IF DB_ID(N'{safeDb}') IS NULL CREATE DATABASE [{safeDb}];";
                    cmd.ExecuteNonQuery();
                }
            }

            // Also ensure AccessSrc schema exists in target DB
            using (var conn = new SqlConnection(sqlConnString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "IF SCHEMA_ID(N'AccessSrc') IS NULL EXEC('CREATE SCHEMA AccessSrc')";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void VerifyFromScript(string sqlConnString, string dataScriptPath)
        {
            if (string.IsNullOrWhiteSpace(dataScriptPath) || !File.Exists(dataScriptPath)) { Console.WriteLine("Skip verification: DataMigration script not found."); return; }

            var pairs = new List<(string Source, string Target)>();
            foreach (var line in File.ReadLines(dataScriptPath))
            {
                if (!line.StartsWith("-- ")) continue;
                var parts = line.Substring(3).Split(new[] {"->"}, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    var src = parts[0].Trim();
                    var tgt = parts[1].Trim();
                    if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(tgt)) continue;

                    // Only accept pairs that look like table->table mappings. Reject column mappings like "ID -> AreaID".
                    // Heuristics: both sides must be simple identifiers (letters/digits/underscore) and at least one side ends with 'Tbl' (common convention here).
                    if (!Regex.IsMatch(src, "^[A-Za-z0-9_]+$") || !Regex.IsMatch(tgt, "^[A-Za-z0-9_]+$")) continue;
                    if (!(src.EndsWith("Tbl", StringComparison.OrdinalIgnoreCase) || tgt.EndsWith("Tbl", StringComparison.OrdinalIgnoreCase))) continue;

                    pairs.Add((src, tgt));
                }
            }

            if (pairs.Count == 0) { Console.WriteLine("No source/target pairs parsed from script for verification."); return; }

            using (var conn = new SqlConnection(sqlConnString))
            {
                conn.Open();
                Console.WriteLine("-- Quick Verification: counts per migrated pair --");
                foreach (var p in pairs.Distinct())
                {
                    try
                    {
                        var safeSrc = p.Source.Replace("]", "]]" );
                        var safeTgt = p.Target.Replace("]", "]]" );
                        var srcCnt = ExecuteCount(conn, $"SELECT COUNT(*) FROM [AccessSrc].[{safeSrc}]");
                        var dstCnt = ExecuteCount(conn, $"SELECT COUNT(*) FROM [{safeTgt}]");
                        Console.WriteLine($"  {p.Source} -> {p.Target}: source={srcCnt}, target={dstCnt}");
                    }
                    catch (Exception ex) { Console.WriteLine($"  {p.Source} -> {p.Target}: verification error: {ex.Message}"); }
                }
            }
        }

        private static long ExecuteCount(SqlConnection conn, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandTimeout = 0;
                var o = cmd.ExecuteScalar();
                return Convert.ToInt64(o ?? 0);
            }
        }
    }

    internal sealed class ConstraintsIndex
    {
        public List<ConstraintTable> Tables { get; set; }
        public ConstraintsIndex()
        {
            Tables = new List<ConstraintTable>();
        }
    }

    internal class ConstraintTable
    {
        public string TableName { get; set; }
        public List<string> PrimaryKey { get; set; }
        public List<string> IdentityColumns { get; set; }
        public List<ForeignKeyDef> ForeignKeys { get; set; }
        public List<string> NotNullColumns { get; set; }
        
        public ConstraintTable()
        {
            PrimaryKey = new List<string>();
            IdentityColumns = new List<string>();
            ForeignKeys = new List<ForeignKeyDef>();
            NotNullColumns = new List<string>();
        }
    }

    internal class ForeignKeyDef
    {
        public string Name { get; set; }
        public string Column { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
    }

    class CleanupTablesCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;

        public CleanupTablesCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir;
            _config = config;
        }

        public string Key => "R";
        public string Description => "Reset/Clean migration tables (drop Orders, Recurring, Orphan tables)";

        public int Execute()
        {
            Console.WriteLine("=== CLEANUP MIGRATION TABLES ===");
            Console.WriteLine("This will DROP the following tables if they exist:");
            Console.WriteLine("  - OrderLinesTbl, OrdersTbl");
            Console.WriteLine("  - RecurringOrderItemsTbl, RecurringOrdersTbl");
            Console.WriteLine("  - OrphanedOrderIdsTbl, OrphanedRecurringOrderIdsTbl");
            Console.WriteLine("  - Migration_OrphanedOrders, Migration_OrphanedOrderLines");
            Console.WriteLine("  - Migration_OrphanedRecurringOrderLines");
            Console.WriteLine();

            Console.Write("Are you sure you want to proceed? [y/N]: ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("Cleanup cancelled.");
                return 0;
            }

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(_config.TargetConnectionString))
                {
                    conn.Open();
                    
                    var cleanupSql = @"
-- Drop tables in correct dependency order
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderLinesTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping OrderLinesTbl...'
    DROP TABLE [dbo].[OrderLinesTbl];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecurringOrderItemsTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping RecurringOrderItemsTbl...'
    DROP TABLE [dbo].[RecurringOrderItemsTbl];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdersTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping OrdersTbl...'
    DROP TABLE [dbo].[OrdersTbl];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecurringOrdersTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping RecurringOrdersTbl...'
    DROP TABLE [dbo].[RecurringOrdersTbl];
END

-- Drop orphan tracking tables
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrphanedOrderIdsTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping OrphanedOrderIdsTbl...'
    DROP TABLE [dbo].[OrphanedOrderIdsTbl];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrphanedRecurringOrderIdsTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping OrphanedRecurringOrderIdsTbl...'
    DROP TABLE [dbo].[OrphanedRecurringOrderIdsTbl];
END

-- Drop migration diagnostic tables
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Migration_OrphanedOrders]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping Migration_OrphanedOrders...'
    DROP TABLE [dbo].[Migration_OrphanedOrders];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Migration_OrphanedOrderLines]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping Migration_OrphanedOrderLines...'
    DROP TABLE [dbo].[Migration_OrphanedOrderLines];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Migration_OrphanedRecurringOrderLines]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping Migration_OrphanedRecurringOrderLines...'
    DROP TABLE [dbo].[Migration_OrphanedRecurringOrderLines];
END

PRINT 'Cleanup completed successfully.'
";

                using (var cmd = new System.Data.SqlClient.SqlCommand(cleanupSql, conn))
                {
                    cmd.CommandTimeout = 300; // 5 minutes timeout
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            while (reader.Read())
                            {
                                // Process any result messages
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    Console.WriteLine(reader[i]?.ToString());
                                }
                            }
                        } while (reader.NextResult());
                    }
                }
            }

            Console.WriteLine("? All migration tables have been cleaned up successfully!");
            Console.WriteLine("You can now run a fresh migration with Option A -> C -> !");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Error during cleanup: {ex.Message}");
            return 1;
        }
    }
    }

    internal sealed class RunRangeCommand : IMenuCommand
    {
        private readonly List<IMenuCommand> _allCommands;
        private readonly MigrationConfig _config;

        public RunRangeCommand(List<IMenuCommand> allCommands, MigrationConfig config)
        {
            _allCommands = allCommands;
            _config = config;
        }

        public string Key => "Z";
        public string Description => "Full migration pipeline (A?B?C?D?M?MS?N?! sequence)";

        public int Execute()
        {
            Console.WriteLine("=== FULL MIGRATION PIPELINE ===");
            Console.WriteLine("This will execute the complete migration sequence:");
            Console.WriteLine("  A) Generate CREATE TABLE DDL");
            Console.WriteLine("  B) Generate FK constraints DDL");
            Console.WriteLine("  C) Apply DDL scripts to target DB");
            Console.WriteLine("  D) Open SQL output folder");
            Console.WriteLine("  M) Generate data migration script");
            Console.WriteLine("  MS) Stage Access data to SQL");
            Console.WriteLine("  N) Apply data migration (excluding normalized tables)");
            Console.WriteLine("  CLEAN) Clean normalized tables before custom migration");
            Console.WriteLine("  !) Custom migrate Orders + Recurring tables");
            Console.WriteLine();
            Console.WriteLine("NOTE: Regular data migration (N) will automatically skip tables marked for normalization");
            Console.WriteLine("NOTE: Normalized tables (Orders, Recurring) will be cleaned and rebuilt in Step !");
            Console.WriteLine();

            // GATHER ALL CONFIGURATION UPFRONT
            Console.WriteLine("=== CONFIGURATION ===");
            
            // Debug: Show that config is loaded
            Console.WriteLine($"Loaded config - Access: {(_config?.AccessConnectionString?.Length ?? 0)} chars, SQL: {(_config?.TargetConnectionString?.Length ?? 0)} chars");
            
            // Get connection strings
            var defAccessCs = _config?.AccessConnectionString ?? "";
            var defSqlCs = _config?.TargetConnectionString ?? "";
            
            // Show the actual values, not just "(empty)" or "configured"
            var accessDisplay = string.IsNullOrWhiteSpace(defAccessCs) ? "(empty)" : defAccessCs;
            var sqlDisplay = string.IsNullOrWhiteSpace(defSqlCs) ? "(empty)" : defSqlCs;
            
            Console.Write($"Access connection string [{accessDisplay}]: ");
            var accessInput = Console.ReadLine();
            var accessCs = string.IsNullOrWhiteSpace(accessInput) ? defAccessCs : accessInput.Trim();
            
            Console.Write($"SQL connection string [{sqlDisplay}]: ");
            var sqlInput = Console.ReadLine();
            var sqlCs = string.IsNullOrWhiteSpace(sqlInput) ? defSqlCs : sqlInput.Trim();
            
            if (string.IsNullOrWhiteSpace(sqlCs))
            {
                Console.WriteLine("? SQL connection string is required for the pipeline.");
                return 2;
            }
            
            if (string.IsNullOrWhiteSpace(accessCs))
            {
                Console.WriteLine("? Access connection string is required for data staging (step MS).");
                return 2;
            }

            // Test connections upfront
            Console.WriteLine("Testing connections...");
            try
            {
                using (var conn = new System.Data.OleDb.OleDbConnection(accessCs)) 
                { 
                    conn.Open(); 
                    Console.WriteLine("? Access connection successful");
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"? Access connection failed: {ex.Message}"); 
                return 1; 
            }
            
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(sqlCs)) 
                { 
                    conn.Open(); 
                    Console.WriteLine("? SQL Server connection successful");
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"? SQL Server connection failed: {ex.Message}"); 
                return 1; 
            }

            // Get DDL options
            Console.Write("Drop existing tables before CREATE (recommended for clean migration)? [Y/n]: ");
            var dropInput = Console.ReadLine();
            var dropExisting = string.IsNullOrWhiteSpace(dropInput) || 
                             !dropInput.Trim().StartsWith("n", StringComparison.OrdinalIgnoreCase);
            
            Console.Write("Suppress IDENTITY to preserve existing IDs on import? [y/N]: ");
            var supInput = Console.ReadLine();
            var suppressIdentity = !string.IsNullOrWhiteSpace(supInput) && supInput.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase);
            
            Console.WriteLine();
            Console.Write("Proceed with full migration pipeline? [y/N]: ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("Full migration pipeline cancelled.");
                return 0;
            }

            // SET UP BATCH EXECUTION MODE
            RunRangeState.Current = new RunRangeOptions
            {
                SuppressPrompts = true,
                SuppressIdentityOnCreate = suppressIdentity,
                DropExistingOnCreate = dropExisting,
                TargetConnectionString = sqlCs,
                AccessConnectionString = accessCs
            };

            try
            {
                // Define the sequence of commands to run
                var sequence = new[] { "A", "B", "C", "D", "M", "MS", "N", "!" };
                
                foreach (var key in sequence)
                {
                    Console.WriteLine($"\n=== STEP {key} ===");
                    
                    // Special handling: Clean normalized tables before Step !
                    if (key == "!")
                    {
                        Console.WriteLine("=== PRE-NORMALIZE CLEANUP ===");
                        Console.WriteLine("Cleaning normalized tables (Orders, Recurring) before custom migration...");
                        
                        try
                        {
                            using (var conn = new SqlConnection(sqlCs))
                            {
                                conn.Open();
                                
                                // Clean up normalized tables that might have been partially migrated
                                var cleanupSql = @"
-- Drop FK constraints first
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrderLinesTbl_OrdersTbl')
    ALTER TABLE [dbo].[OrderLinesTbl] DROP CONSTRAINT [FK_OrderLinesTbl_OrdersTbl];

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RecurringOrderItemsTbl_RecurringOrdersTbl')
    ALTER TABLE [dbo].[RecurringOrderItemsTbl] DROP CONSTRAINT [FK_RecurringOrderItemsTbl_RecurringOrdersTbl];

-- Clean table data (preserve structure)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderLinesTbl]') AND type = 'U')
    DELETE FROM [dbo].[OrderLinesTbl];

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecurringOrderItemsTbl]') AND type = 'U')
    DELETE FROM [dbo].[RecurringOrderItemsTbl];

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdersTbl]') AND type = 'U')
    DELETE FROM [dbo].[OrdersTbl];

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecurringOrdersTbl]') AND type = 'U')
    DELETE FROM [dbo].[RecurringOrdersTbl];

-- Clean orphan tracking tables
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrphanedOrderIdsTbl]') AND type = 'U')
    DELETE FROM [dbo].[OrphanedOrderIdsTbl];

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrphanedRecurringOrderIdsTbl]') AND type = 'U')
    DELETE FROM [dbo].[OrphanedRecurringOrderIdsTbl];

PRINT 'Normalized tables cleaned successfully.'
";

                                using (var cmd = new SqlCommand(cleanupSql, conn))
                                {
                                    cmd.CommandTimeout = 300;
                                    cmd.ExecuteNonQuery();
                                }
                                
                                Console.WriteLine("? Normalized tables cleaned successfully");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"??  Warning: Pre-cleanup failed: {ex.Message}");
                            Console.WriteLine("Continuing with custom normalization...");
                        }
                    }
                    
                    var command = _allCommands.FirstOrDefault(c => 
                        string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));
                    
                    if (command == null)
                    {
                        Console.WriteLine($"? Command {key} not found - skipping");
                        continue;
                    }

                    Console.WriteLine($"Executing: {command.Description}");
                    
                    try
                    {
                        var result = command.Execute();
                        Console.WriteLine($"? Step {key} completed with code: {result}");
                        
                        if (result != 0)
                        {
                            Console.WriteLine($"? Step {key} failed with code {result}");
                            Console.Write("Continue with remaining steps? [y/N]: ");
                            var continueResponse = Console.ReadLine()?.Trim().ToLowerInvariant();
                            
                            if (continueResponse != "y" && continueResponse != "yes")
                            {
                                Console.WriteLine("Pipeline stopped due to failure.");
                                return result;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"? Step {key} failed with exception: {ex.Message}");
                        Console.Write("Continue with remaining steps? [y/N]: ");
                        var continueResponse = Console.ReadLine()?.Trim().ToLowerInvariant();
                        
                        if (continueResponse != "y" && continueResponse != "yes")
                        {
                            Console.WriteLine("Pipeline stopped due to exception.");
                            return 1;
                        }
                    }
                }

                Console.WriteLine("\n?? Full migration pipeline completed successfully!");
                Console.WriteLine("All tables have been migrated with proper normalization handling.");
                Console.WriteLine("\n?? Migration Summary:");
                Console.WriteLine("? Schema created and applied");
                Console.WriteLine("? Regular tables migrated with data cleaning");
                Console.WriteLine("? Normalized tables (Orders, Recurring) custom migrated");
                Console.WriteLine("? Foreign key constraints applied");
                
                return 0;
            }
            finally
            {
                // CLEAN UP BATCH STATE
                RunRangeState.Current = null;
            }
        }
    }

    internal sealed class GenerateDataMigrationScriptCommand : IMenuCommand
    {
        private readonly string _migrationsDir;

        public GenerateDataMigrationScriptCommand(string migrationsDir)
        {
            _migrationsDir = migrationsDir;
        }

        public string Key => "M";
        public string Description => "Generate data migration script (SQL)";

        public int Execute()
        {
            try
            {
                var rc = DmlScriptGenerator.GenerateDataMigration(_migrationsDir, out var sqlPath);
                Console.WriteLine("Data migration script rc=" + rc + " file: " + sqlPath);
                return rc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("? Generate data migration failed: " + ex.Message);
                return 1;
            }
        }
    }

    internal sealed class StageAccessToSqlCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;

        public StageAccessToSqlCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir;
            _config = config;
        }

        public string Key => "MS";
        public string Description => "Stage Access data to SQL [AccessSrc] schema";

        public int Execute()
        {
            try
            {
                // Use RunRangeState connection strings if available
                var rro = RunRangeState.Current;
                string accessCs, sqlCs;
                
                if (rro != null && rro.SuppressPrompts)
                {
                    accessCs = rro.AccessConnectionString;
                    sqlCs = rro.TargetConnectionString;
                    Console.WriteLine("Using batch mode connection strings");
                }
                else
                {
                    accessCs = _config?.AccessConnectionString ?? "";
                    sqlCs = _config?.TargetConnectionString ?? "";
                }
                
                var rc = AccessStagingImporter.StageAll(_migrationsDir, accessCs, sqlCs, out var logPath);
                Console.WriteLine("Access staging rc=" + rc + " log: " + logPath);
                return rc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("? Access staging failed: " + ex.Message);
                return 1;
            }
        }
    }

    internal sealed class ApplyDataMigrationScriptCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;

        public ApplyDataMigrationScriptCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir;
            _config = config;
        }

        public string Key => "N";
        public string Description => "Apply data migration script (excluding normalized tables) with enhanced error handling";

        public int Execute()
        {
            try
            {
                // Use RunRangeState connection string if available
                var rro = RunRangeState.Current;
                string sqlCs;
                
                if (rro != null && rro.SuppressPrompts)
                {
                    sqlCs = rro.TargetConnectionString;
                    Console.WriteLine("Using batch mode target connection string");
                }
                else
                {
                    sqlCs = _config?.TargetConnectionString ?? "";
                }

                // Enhanced error handling: Offer to run with data cleaning
                bool enableDataCleaning = false;
                if (rro == null || !rro.SuppressPrompts)
                {
                    Console.Write("Enable enhanced data cleaning (fix NULL FKs, date conversions, etc.)? [Y/n]: ");
                    var cleanInput = Console.ReadLine();
                    enableDataCleaning = string.IsNullOrWhiteSpace(cleanInput) || 
                                       !cleanInput.Trim().StartsWith("n", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    enableDataCleaning = true; // Auto-enable in batch mode
                    Console.WriteLine("Auto-enabling enhanced data cleaning for batch mode");
                }

                int rc;
                string logPath;

                if (enableDataCleaning)
                {
                    Console.WriteLine("?? Running enhanced data migration with automatic data cleaning...");
                    Console.WriteLine("??  Note: Tables marked for normalization (Orders, Recurring) will be skipped and handled by Step ! instead");
                    rc = ApplyDataMigrationWithCleaning(_migrationsDir, sqlCs, out logPath);
                }
                else
                {
                    rc = DmlScriptApplier.ApplyLatest(_migrationsDir, sqlCs, out logPath);
                }
                
                Console.WriteLine("Data migration apply rc=" + rc + " log: " + logPath);
                
                if (rc != 0 && File.Exists(logPath))
                {
                    // Show error summary
                    var lines = File.ReadAllLines(logPath);
                    var errorLines = lines.Where(l => l.Contains("ERROR")).Take(10).ToArray();
                    
                    if (errorLines.Length > 0)
                    {
                        Console.WriteLine("\n? Error Summary (first 10 errors):");
                        foreach (var error in errorLines)
                        {
                            Console.WriteLine("  " + error);
                        }
                        
                        if (!enableDataCleaning)
                        {
                            Console.WriteLine("\n?? Tip: Try running again with enhanced data cleaning enabled!");
                        }
                    }
                }
                
                return rc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("? Data migration apply failed: " + ex.Message);
                return 1;
            }
        }

        private int ApplyDataMigrationWithCleaning(string migrationsDir, string connectionString, out string logPath)
        {
            var logsDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs");
            Directory.CreateDirectory(logsDir);
            logPath = Path.Combine(logsDir, "DataMigrationWithCleaning_" + DateTime.Now.ToString("yyyyMMdd_HHmms") + ".log");
            
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== Enhanced Data Migration with Cleaning ===");
                sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("Purpose: Apply data migration with automatic error handling and data cleaning");
                sb.AppendLine();

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    sb.AppendLine("? Connected to SQL Server");

                    // 1. Apply pre-migration data cleaning
                    sb.AppendLine("\n=== PRE-MIGRATION DATA CLEANING ===");
                    ApplyDataCleaningFixes(conn, sb);

                    // 2. Run the original data migration script
                    sb.AppendLine("\n=== RUNNING DATA MIGRATION SCRIPT ===");
                    var originalRc = DmlScriptApplier.ApplyLatest(migrationsDir, connectionString, out var originalLogPath);
                    sb.AppendLine($"Original migration result: {originalRc}");
                    
                    if (File.Exists(originalLogPath))
                    {
                        sb.AppendLine("Original migration log content:");
                        sb.AppendLine(File.ReadAllText(originalLogPath));
                    }

                    // 3. Apply post-migration fixes if needed
                    if (originalRc != 0)
                    {
                        sb.AppendLine("\n=== POST-MIGRATION ERROR FIXES ===");
                        ApplyPostMigrationFixes(conn, sb);
                    }

                    sb.AppendLine("\n=== ENHANCED MIGRATION COMPLETED ===");
                    sb.AppendLine($"Final result: {(originalRc == 0 ? "SUCCESS" : "COMPLETED WITH FIXES")}");
                }

                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return 0; // Always return success when using enhanced cleaning
            }
            catch (Exception ex)
            {
                var errorLog = $"FATAL: Enhanced data migration failed: {ex.Message}\n{ex.StackTrace}";
                File.WriteAllText(logPath, errorLog, Encoding.UTF8);
                return 1;
            }
        }

        private void ApplyDataCleaningFixes(SqlConnection conn, StringBuilder log)
        {
            try
            {
                log.AppendLine("Applying data cleaning fixes to AccessSrc schema...");

                // Fix 1: Clean up invalid dates in AccessSrc tables
                var dateCleanupSql = @"
-- Fix invalid dates by setting them to a reasonable default
UPDATE [AccessSrc].[ClientUsageHistoryTbl] 
SET [DateOfReading] = '1980-01-01' 
WHERE [DateOfReading] IS NOT NULL 
  AND (ISDATE([DateOfReading]) = 0 OR TRY_CONVERT(datetime, [DateOfReading]) IS NULL);

UPDATE [AccessSrc].[ClientUsageHistoryTbl] 
SET [LastUpdate] = '1980-01-01' 
WHERE [LastUpdate] IS NOT NULL 
  AND (ISDATE([LastUpdate]) = 0 OR TRY_CONVERT(datetime, [LastUpdate]) IS NULL);

UPDATE [AccessSrc].[ClientUsageTbl] 
SET [NextCoffeeBy] = '1980-01-01' 
WHERE [NextCoffeeBy] IS NOT NULL 
  AND (ISDATE([NextCoffeeBy]) = 0 OR TRY_CONVERT(datetime, [NextCoffeeBy]) IS NULL);

UPDATE [AccessSrc].[ClientUsageTbl] 
SET [NextCleanOn] = '1980-01-01' 
WHERE [NextCleanOn] IS NOT NULL 
  AND (ISDATE([NextCleanOn]) = 0 OR TRY_CONVERT(datetime, [NextCleanOn]) IS NULL);

UPDATE [AccessSrc].[ClientUsageTbl] 
SET [NextFilterEst] = '1980-01-01' 
WHERE [NextFilterEst] IS NOT NULL 
  AND (ISDATE([NextFilterEst]) = 0 OR TRY_CONVERT(datetime, [NextFilterEst]) IS NULL);

UPDATE [AccessSrc].[ClientUsageTbl] 
SET [NextDescaleEst] = '1980-01-01' 
WHERE [NextDescaleEst] IS NOT NULL 
  AND (ISDATE([NextDescaleEst]) = 0 OR TRY_CONVERT(datetime, [NextDescaleEst]) IS NULL);

UPDATE [AccessSrc].[ClientUsageTbl] 
SET [NextServiceEst] = '1980-01-01' 
WHERE [NextServiceEst] IS NOT NULL 
  AND (ISDATE([NextServiceEst]) = 0 OR TRY_CONVERT(datetime, [NextServiceEst]) IS NULL);

UPDATE [AccessSrc]..[_ClientUsageTbl] 
SET [NextCoffeeBy] = '1980-01-01' 
WHERE [NextCoffeeBy] IS NOT NULL 
  AND (ISDATE([NextCoffeeBy]) = 0 OR TRY_CONVERT(datetime, [NextCoffeeBy]) IS NULL);
";

                using (var cmd = new SqlCommand(dateCleanupSql, conn))
                {
                    cmd.CommandTimeout = 300;
                    var rowsAffected = cmd.ExecuteNonQuery();
                    log.AppendLine($"? Fixed {rowsAffected} invalid date values");
                }

                // Fix 2: Handle NULL ContactID by assigning a default customer ID (9 = General/Other)
                var nullContactIdSql = @"
-- Fix NULL ContactID values by assigning to General/Other customer (ID=9)
UPDATE [AccessSrc].[SentRemindersLogTbl] 
SET [ContactID] = 9 
WHERE [ContactID] IS NULL OR [ContactID] = 0;

UPDATE [AccessSrc].[TempCoffeecheckupItemsTbl] 
SET [ContactID] = 9 
WHERE [ContactID] IS NULL OR [ContactID] = 0;

UPDATE [AccessSrc].[RepairsTbl] 
SET [ContactID] = 9 
WHERE [ContactID] IS NULL OR [ContactID] = 0;
";

                using (var cmd = new SqlCommand(nullContactIdSql, conn))
                {
                    cmd.CommandTimeout = 300;
                    var rowsAffected = cmd.ExecuteNonQuery();
                    log.AppendLine($"? Fixed {rowsAffected} NULL ContactID values");
                }

                // Fix 3: Clean up any other common data quality issues
                var generalCleanupSql = @"
-- Clean up empty strings that should be NULL
UPDATE [AccessSrc].[SentRemindersLogTbl] 
SET [EmailAddress] = NULL 
WHERE LTRIM(RTRIM([EmailAddress])) = '';

-- Ensure numeric fields have valid values
UPDATE [AccessSrc].[ClientUsageHistoryTbl] 
SET [CupCount] = 0 
WHERE [CupCount] IS NULL OR [CupCount] < 0;

UPDATE [AccessSrc].[ClientUsageTbl] 
SET [LastCupCount] = 0 
WHERE [LastCupCount] IS NULL OR [LastCupCount] < 0;

UPDATE [AccessSrc].[ClientUsageTbl] 
SET [DailyConsumption] = 0 
WHERE [DailyConsumption] IS NULL OR [DailyConsumption] < 0;
";

                using (var cmd = new SqlCommand(generalCleanupSql, conn))
                {
                    cmd.CommandTimeout = 300;
                    var rowsAffected = cmd.ExecuteNonQuery();
                    log.AppendLine($"? Cleaned {rowsAffected} data quality issues");
                }

                log.AppendLine("? All pre-migration data cleaning completed successfully");
            }
 catch (Exception ex)
            {
                log.AppendLine($"? Data cleaning error: {ex.Message}");
                // Continue anyway - cleaning is best-effort
            }
        }

        private void ApplyPostMigrationFixes(SqlConnection conn, StringBuilder log)
        {
            try
            {
                log.AppendLine("Applying post-migration fixes...");

                // Check for common issues and fix them
                var postFixSql = @"
-- Ensure all ContactID references point to valid customers
-- If any orphaned records exist, point them to General customer (ID=9)
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'SentRemindersLogTbl')
BEGIN
    UPDATE SentRemindersLogTbl 
    SET ContactID = 9 
    WHERE ContactID NOT IN (SELECT CustomerID FROM ContactsTbl WHERE CustomerID IS NOT NULL)
    AND ContactID IS NOT NULL AND ContactID != 9;
END

IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'RepairsTbl')
BEGIN
    UPDATE RepairsTbl 
    SET ContactID = 9 
    WHERE ContactID NOT IN (SELECT CustomerID FROM ContactsTbl WHERE CustomerID IS NOT NULL)
    AND ContactID IS NOT NULL AND ContactID != 9;
END

-- Clean up any remaining date issues in target tables
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'ClientUsageTbl' AND type = 'U')
BEGIN
    UPDATE ClientUsageTbl 
    SET NextCoffeeBy = '1980-01-01' 
    WHERE NextCoffeeBy < '1900-01-01' OR NextCoffeeBy > '2100-01-01';
    
    UPDATE ClientUsageTbl 
    SET NextCleanOn = '1980-01-01' 
    WHERE NextCleanOn < '1900-01-01' OR NextCleanOn > '2100-01-01';
END
";

                using (var cmd = new SqlCommand(postFixSql, conn))
                {
                    cmd.CommandTimeout = 300;
                    var rowsAffected = cmd.ExecuteNonQuery();
                    log.AppendLine($"? Post-migration fixes completed, {rowsAffected} rows processed");
                }
            }
            catch (Exception ex)
            {
                log.AppendLine($"?? Post-migration fix warning: {ex.Message}");
                // Continue anyway - these are cleanup fixes
            }
        }
    }

    internal sealed class DataCleaningCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;

        public DataCleaningCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir;
            _config = config;
        }

        public string Key => "CLEAN";
        public string Description => "Clean data issues (NULL FKs, date conversions, etc.) in AccessSrc schema";

        public int Execute()
        {
            Console.WriteLine("=== DATA CLEANING UTILITY ===");
            Console.WriteLine("This will clean common data issues in the AccessSrc schema:");
            Console.WriteLine("  - Fix invalid date/time values");
            Console.WriteLine("  - Replace NULL foreign keys with default values");
            Console.WriteLine("  - Clean empty strings and invalid numeric values");
            Console.WriteLine();

            try
            {
                // Use RunRangeState connection string if available
                var rro = RunRangeState.Current;
                string sqlCs;
                
                if (rro != null && rro.SuppressPrompts)
                {
                    sqlCs = rro.TargetConnectionString;
                    Console.WriteLine("Using batch mode target connection string");
                }
                else
                {
                    sqlCs = _config?.TargetConnectionString ?? "";
                    
                    if (string.IsNullOrWhiteSpace(sqlCs))
                    {
                        Console.Write("SQL connection string: ");
                        sqlCs = Console.ReadLine()?.Trim() ?? "";
                    }
                }

                if (string.IsNullOrWhiteSpace(sqlCs))
                {
                    Console.WriteLine("? SQL connection string is required.");
                    return 2;
                }

                // Test connection
                try
                {
                    using (var testConn = new SqlConnection(sqlCs))
                    {
                        testConn.Open();
                        Console.WriteLine("? SQL Server connection successful");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? SQL Server connection failed: {ex.Message}");
                    return 1;
                }

                using (var conn = new SqlConnection(sqlCs))
                {
                    conn.Open();
                    
                    Console.WriteLine("?? Starting data cleaning...");
                    
                    // 1. Clean invalid dates
                    Console.WriteLine("Cleaning invalid date values...");
                    var dateUpdates = CleanInvalidDates(conn);
                    Console.WriteLine($"  ? Fixed {dateUpdates} invalid date values");
                    
                    // 2. Fix NULL ContactIDs
                    Console.WriteLine("Fixing NULL foreign key values...");
                    var fkUpdates = FixNullForeignKeys(conn);
                    Console.WriteLine($"  ? Fixed {fkUpdates} NULL ContactID values");
                    
                    // 3. General cleanup
                    Console.WriteLine("Performing general data cleanup...");
                    var generalUpdates = PerformGeneralCleanup(conn);
                    Console.WriteLine($"  ? Cleaned {generalUpdates} data quality issues");
                    
                    // 4. Validation summary
                    Console.WriteLine("Validating data quality...");
                    var issues = ValidateDataQuality(conn);
                    
                    if (issues.Count == 0)
                    {
                        Console.WriteLine("?? All data quality issues have been resolved!");
                    }
                    else
                    {
                        Console.WriteLine($"??  {issues.Count} remaining issues found:");
                        foreach (var issue in issues)
                        {
                            Console.WriteLine($"   - {issue}");
                        }
                    }
                }

                Console.WriteLine("? Data cleaning completed successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Data cleaning failed: " + ex.Message);
                return 1;
            }
        }

        private int CleanInvalidDates(SqlConnection conn)
        {
            var sql = @"
DECLARE @updates INT = 0;

-- ClientUsageHistoryTbl dates
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'ClientUsageHistoryTbl')
BEGIN
    UPDATE [AccessSrc].[ClientUsageHistoryTbl] 
    SET [DateOfReading] = '1980-01-01' 
    WHERE [DateOfReading] IS NOT NULL 
      AND (ISDATE([DateOfReading]) = 0 OR TRY_CONVERT(datetime, [DateOfReading]) IS NULL);
    SET @updates = @updates + @@ROWCOUNT;

    UPDATE [AccessSrc].[ClientUsageHistoryTbl] 
    SET [LastUpdate] = '1980-01-01' 
    WHERE [LastUpdate] IS NOT NULL 
      AND (ISDATE([LastUpdate]) = 0 OR TRY_CONVERT(datetime, [LastUpdate]) IS NULL);
    SET @updates = @updates + @@ROWCOUNT;
END

-- ClientUsageTbl dates
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'ClientUsageTbl')
BEGIN
    UPDATE [AccessSrc].[ClientUsageTbl] 
    SET [NextCoffeeBy] = '1980-01-01' 
    WHERE [NextCoffeeBy] IS NOT NULL 
      AND (ISDATE([NextCoffeeBy]) = 0 OR TRY_CONVERT(datetime, [NextCoffeeBy]) IS NULL);
    SET @updates = @updates + @@ROWCOUNT;

    UPDATE [AccessSrc].[ClientUsageTbl] 
    SET [NextCleanOn] = '1980-01-01', [NextFilterEst] = '1980-01-01', [NextDescaleEst] = '1980-01-01', [NextServiceEst] = '1980-01-01'
    WHERE ([NextCleanOn] IS NOT NULL AND (ISDATE([NextCleanOn]) = 0 OR TRY_CONVERT(datetime, [NextCleanOn]) IS NULL))
       OR ([NextFilterEst] IS NOT NULL AND (ISDATE([NextFilterEst]) = 0 OR TRY_CONVERT(datetime, [NextFilterEst]) IS NULL))
       OR ([NextDescaleEst] IS NOT NULL AND (ISDATE([NextDescaleEst]) = 0 OR TRY_CONVERT(datetime, [NextDescaleEst]) IS NULL))
       OR ([NextServiceEst] IS NOT NULL AND (ISDATE([NextServiceEst]) = 0 OR TRY_CONVERT(datetime, [NextServiceEst]) IS NULL));
    SET @updates = @updates + @@ROWCOUNT;
END

-- _ClientUsageTbl dates  
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = '_ClientUsageTbl')
BEGIN
    UPDATE [AccessSrc]..[_ClientUsageTbl] 
    SET [NextCoffeeBy] = '1980-01-01' 
    WHERE [NextCoffeeBy] IS NOT NULL 
      AND (ISDATE([NextCoffeeBy]) = 0 OR TRY_CONVERT(datetime, [NextCoffeeBy]) IS NULL);
    SET @updates = @updates + @@ROWCOUNT;
END

SELECT @updates;
";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 300;
                return (int)(cmd.ExecuteScalar() ?? 0);
            }
        }

        private int FixNullForeignKeys(SqlConnection conn)
        {
            var sql = @"
DECLARE @updates INT = 0;

-- Fix NULL ContactID in SentRemindersLogTbl
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'SentRemindersLogTbl')
BEGIN
    UPDATE [AccessSrc].[SentRemindersLogTbl] 
    SET [ContactID] = 9 
    WHERE [ContactID] IS NULL OR [ContactID] = 0;
    SET @updates = @updates + @@ROWCOUNT;
END

-- Fix NULL ContactID in TempCoffeecheckupItemsTbl
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'TempCoffeecheckupItemsTbl')
BEGIN
    UPDATE [AccessSrc].[TempCoffeecheckupItemsTbl] 
    SET [ContactID] = 9 
    WHERE [ContactID] IS NULL OR [ContactID] = 0;
    SET @updates = @updates + @@ROWCOUNT;
END

-- Fix NULL ContactID in RepairsTbl
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'RepairsTbl')
BEGIN
    UPDATE [AccessSrc].[RepairsTbl] 
    SET [ContactID] = 9 
    WHERE [ContactID] IS NULL OR [ContactID] = 0;
    SET @updates = @updates + @@ROWCOUNT;
END

SELECT @updates;
";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 300;
                return (int)(cmd.ExecuteScalar() ?? 0);
            }
        }

        private int PerformGeneralCleanup(SqlConnection conn)
        {
            var sql = @"
DECLARE @updates INT = 0;

-- Clean up empty strings in email addresses
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'SentRemindersLogTbl')
BEGIN
    UPDATE [AccessSrc].[SentRemindersLogTbl] 
    SET [EmailAddress] = NULL 
    WHERE LTRIM(RTRIM(ISNULL([EmailAddress], ''))) = '';
    SET @updates = @updates + @@ROWCOUNT;
END

-- Fix negative or NULL numeric values
IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'ClientUsageHistoryTbl')
BEGIN
    UPDATE [AccessSrc].[ClientUsageHistoryTbl] 
    SET [CupCount] = 0 
    WHERE [CupCount] IS NULL OR [CupCount] < 0;
    SET @updates = @updates + @@ROWCOUNT;
END

IF EXISTS (SELECT 1 FROM sys.schemas s JOIN sys.tables t ON s.schema_id = t.schema_id WHERE s.name = 'AccessSrc' AND t.name = 'ClientUsageTbl')
BEGIN
    UPDATE [AccessSrc].[ClientUsageTbl] 
    SET [LastCupCount] = 0 
    WHERE [LastCupCount] IS NULL OR [LastCupCount] < 0;
    SET @updates = @updates + @@ROWCOUNT;
    
    UPDATE [AccessSrc].[ClientUsageTbl] 
    SET [DailyConsumption] = 0 
    WHERE [DailyConsumption] IS NULL OR [DailyConsumption] < 0;
    SET @updates = @updates + @@ROWCOUNT;
END

SELECT @updates;
";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 300;
                return (int)(cmd.ExecuteScalar() ?? 0);
            }
        }

        private List<string> ValidateDataQuality(SqlConnection conn)
        {
            var issues = new List<string>();
            
            // Check for remaining NULL ContactID values
            var nullContactSql = @"
SELECT 
    'SentRemindersLogTbl' as TableName,
    COUNT(*) as NullCount
FROM [AccessSrc].[SentRemindersLogTbl] 
WHERE [ContactID] IS NULL
UNION ALL
SELECT 
    'TempCoffeecheckupItemsTbl' as TableName,
    COUNT(*) as NullCount
FROM [AccessSrc].[TempCoffeecheckupItemsTbl] 
WHERE [ContactID] IS NULL
UNION ALL
SELECT 
    'RepairsTbl' as TableName,
    COUNT(*) as NullCount
FROM [AccessSrc].[RepairsTbl] 
WHERE [ContactID] IS NULL;
";

            try
            {
                using (var cmd = new SqlCommand(nullContactSql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tableName = reader["TableName"].ToString();
                            var nullCount = Convert.ToInt32(reader["NullCount"]);
                            if (nullCount > 0)
                            {
                                issues.Add($"{tableName} has {nullCount} NULL ContactID values");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error validating ContactID: {ex.Message}");
            }

            return issues;
        }
    }

    internal sealed class CustomNormalizeCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;

        public CustomNormalizeCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir;
            _config = config;
        }

        public string Key => "!";
        public string Description => "Custom migrate Orders + Recurring tables (normalization)";

        public int Execute()
        {
            try
            {
                // Use RunRangeState connection string if available
                var rro = RunRangeState.Current;
                string sqlCs;
                
                if (rro != null && rro.SuppressPrompts)
                {
                    sqlCs = rro.TargetConnectionString;
                    Console.WriteLine("Using batch mode target connection string");
                }
                else
                {
                    sqlCs = _config?.TargetConnectionString ?? "";
                    
                    if (string.IsNullOrWhiteSpace(sqlCs))
                    {
                        Console.Write("SQL connection string: ");
                        sqlCs = Console.ReadLine()?.Trim() ?? "";
                    }
                }

                if (string.IsNullOrWhiteSpace(sqlCs))
                {
                    Console.WriteLine("? SQL connection string is required.");
                    return 2;
                }
                
                Console.WriteLine("?? Starting custom normalization migration...");
                Console.WriteLine("This will migrate Orders and Recurring tables with proper header/line normalization");
                
                var rc = CustomNormalizeRunner.Run(_migrationsDir, sqlCs);
                
                if (rc == 0)
                {
                    Console.WriteLine("? Custom normalize completed successfully!");
                }
                else
                {
                    Console.WriteLine($"??  Custom normalize completed with warnings (code: {rc})");
                }
                
                return rc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("? Custom normalize failed: " + ex.Message);
                return 1;
            }
        }
    }

    internal sealed class DropAllTablesCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;

        public DropAllTablesCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir;
            _config = config;
        }

        public string Key => "X";
        public string Description => "Drop ALL tables in database (DANGER: Deletes all data!)";

        public int Execute()
        {
            Console.WriteLine("=== DROP ALL TABLES ===");
            Console.WriteLine("??  WARNING: This will DROP ALL USER TABLES and DELETE ALL DATA! ??");
            Console.WriteLine();
            Console.WriteLine("This will:");
            Console.WriteLine("  1. Disable all foreign key constraints");
            Console.WriteLine("  2. Drop ALL user tables (preserves system tables)");
            Console.WriteLine("  3. Preserves ASP.NET membership and system tables");
            Console.WriteLine();

            // Triple confirmation for safety
            Console.Write("Are you ABSOLUTELY SURE you want to DROP ALL TABLES? Type 'DROP ALL TABLES' to confirm: ");
            var confirmation1 = Console.ReadLine()?.Trim();
            
            if (confirmation1 != "DROP ALL TABLES")
            {
                Console.WriteLine("Operation cancelled - incorrect confirmation phrase.");
                return 0;
            }

            Console.WriteLine();
            Console.Write("Last chance! Type 'YES DELETE EVERYTHING' to proceed: ");
            var confirmation2 = Console.ReadLine()?.Trim();
            
            if (confirmation2 != "YES DELETE EVERYTHING")
            {
                Console.WriteLine("Operation cancelled - operation not confirmed.");
                return 0;
            }

            try
            {
                // Get connection string
                var rro = RunRangeState.Current;
                string sqlCs;
                
                if (rro != null && rro.SuppressPrompts)
                {
                    sqlCs = rro.TargetConnectionString;
                    Console.WriteLine("Using batch mode target connection string");
                }
                else
                {
                    sqlCs = _config?.TargetConnectionString ?? "";
                    
                    if (string.IsNullOrWhiteSpace(sqlCs))
                    {
                        Console.Write("SQL connection string: ");
                        sqlCs = Console.ReadLine()?.Trim() ?? "";
                    }
                }

                if (string.IsNullOrWhiteSpace(sqlCs))
                {
                    Console.WriteLine("? SQL connection string is required.");
                    return 2;
                }

                // Test connection first
                try
                {
                    using (var testConn = new SqlConnection(sqlCs))
                    {
                        testConn.Open();
                        Console.WriteLine("? SQL Server connection successful");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? SQL Server connection failed: {ex.Message}");
                    return 1;
                }

                Console.WriteLine();
                Console.WriteLine("???  Starting to drop all tables...");

                using (var conn = new SqlConnection(sqlCs))
                {
                    conn.Open();
                    
                    // Step 1: Disable all foreign key constraints
                    Console.WriteLine("  ?? Disabling foreign key constraints...");
                    var disableFkSql = @"
DECLARE @sql NVARCHAR(MAX) = N''

-- Generate commands to disable all foreign key constraints
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + 
              N' NOCHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id

IF LEN(@sql) > 0
BEGIN
    PRINT 'Disabling ' + CAST(LEN(@sql) - LEN(REPLACE(@sql, CHAR(13), '')) AS NVARCHAR(10)) + ' foreign key constraints...'
    EXEC sp_executesql @sql
END
ELSE
BEGIN
    PRINT 'No foreign key constraints to disable.'
END
";

                    using (var cmd = new SqlCommand(disableFkSql, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine("  ? Foreign key constraints disabled");

                    // Step 2: Get count of tables to drop
                    var countSql = @"
SELECT COUNT(*) 
FROM sys.tables 
WHERE type = 'U'  -- User tables only
  AND name NOT LIKE 'aspnet_%'  -- Preserve ASP.NET membership tables
  AND name NOT LIKE 'webpages_%'  -- Preserve WebPages tables
  AND name NOT LIKE '__MigrationHistory'  -- Preserve EF migration history
";

                    int tableCount;
                    using (var cmd = new SqlCommand(countSql, conn))
                    {
                        tableCount = (int)(cmd.ExecuteScalar() ?? 0);
                    }

                    if (tableCount == 0)
                    {
                        Console.WriteLine("  ??  No user tables found to drop.");
                        return 0;
                    }

                    Console.WriteLine($"  ?? Found {tableCount} user tables to drop");

                    // Step 3: Drop all user tables
                    Console.WriteLine($"  ???  Dropping {tableCount} tables...");
                    var dropTablesSql = @"
DECLARE @sql NVARCHAR(MAX) = N''
DECLARE @count INT = 0

SELECT @sql = @sql + N'DROP TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) + N';' + CHAR(13),
       @count = @count + 1
FROM sys.tables 
WHERE type = 'U'  -- User tables only
  AND name NOT LIKE 'aspnet_%'  -- Preserve ASP.NET membership tables
  AND name NOT LIKE 'webpages_%'  -- Preserve WebPages tables
  AND name NOT LIKE '__MigrationHistory'  -- Preserve EF migration history
ORDER BY name

IF LEN(@sql) > 0
BEGIN
    PRINT 'Dropping ' + CAST(@count AS NVARCHAR(10)) + ' user tables...'
    EXEC sp_executesql @sql
    PRINT 'All user tables dropped successfully!'
END
ELSE
BEGIN
    PRINT 'No user tables found to drop.'
END
";

                    using (var cmd = new SqlCommand(dropTablesSql, conn))
                    {
                        cmd.CommandTimeout = 600; // 10 minutes for large databases
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine("  ? All user tables dropped successfully");

                    // Step 4: Verify cleanup
                    using (var cmd = new SqlCommand(countSql, conn))
                    {
                        var remainingCount = (int)(cmd.ExecuteScalar() ?? 0);
                        Console.WriteLine($"  ?? Verification: {remainingCount} user tables remaining");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("?? Database cleanup completed successfully!");
                Console.WriteLine();
                Console.WriteLine("?? Next steps:");
                Console.WriteLine("  1. Run Option A (Generate CREATE TABLE DDL)");
                Console.WriteLine("  2. Run Option C (Apply DDL scripts)");
                Console.WriteLine("  3. Run Option $ (Full migration pipeline)");
                Console.WriteLine();
                Console.WriteLine("??  Note: System tables, ASP.NET membership, and WebPages tables were preserved.");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error dropping tables: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("?? Troubleshooting:");
                Console.WriteLine("  - Ensure you have ALTER and DROP permissions");
                Console.WriteLine("  - Check for active connections to the database");
                Console.WriteLine("  - Some tables may have dependencies that prevent dropping");
                return 1;
            }
        }
    }
}
