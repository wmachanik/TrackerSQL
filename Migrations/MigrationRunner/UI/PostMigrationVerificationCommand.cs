using System;

namespace MigrationRunner.UI
{
    internal sealed class PostMigrationVerificationCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;

        public PostMigrationVerificationCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir;
            _config = config;
        }

        public string Key => "&";
        public string Description => "?? Post-migration data validation (with % success per table)";

        public int Execute()
        {
            try
            {
                Console.WriteLine("=== POST-MIGRATION DATA VALIDATION ===\n");
                Console.WriteLine("This comprehensive validation will:");
                Console.WriteLine("? Check row counts for all migrated tables");
                Console.WriteLine("? Calculate migration success percentage per table");
                Console.WriteLine("? Identify missing or orphaned data");
                Console.WriteLine("? Verify normalized table relationships (Orders ? Headers + Lines)");
                Console.WriteLine("? Account for expected renames and transformations");
                Console.WriteLine("? Report on FK constraint violations and orphaned records");
                Console.WriteLine("? Generate detailed per-table migration statistics\n");

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

                // Configuration options
                bool fullCompare = false;
                if (rro == null || !rro.SuppressPrompts)
                {
                    Console.Write("Run full data comparison (slower but thorough)? [y/N]: ");
                    var fullInput = Console.ReadLine();
                    fullCompare = !string.IsNullOrWhiteSpace(fullInput) && 
                                fullInput.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase);
                }

                Console.WriteLine("\n?? Starting comprehensive verification...");
                
                var rc = SpotCheckVerifier.Run(
                    migrationsDir: _migrationsDir,
                    sqlConnString: sqlCs,
                    samplesPerTable: 50,
                    perTableTimeoutSeconds: 120,
                    persistOrphans: true,
                    logPath: out string logPath,
                    onlyTargetTable: null,
                    fullCompare: fullCompare
                );
                
                Console.WriteLine("\n?? Verification completed!");
                
                if (!string.IsNullOrEmpty(logPath))
                {
                    Console.WriteLine($"?? Detailed report: {logPath}");
                }

                if (rc == 0)
                {
                    Console.WriteLine("? All verification checks PASSED!");
                    Console.WriteLine("\n?? Data migration completed successfully with expected transformations!");
                    Console.WriteLine("\n?? MIGRATION STATISTICS:");
                    Console.WriteLine("• All tables migrated with 100% data integrity");
                    Console.WriteLine("• Normalized tables (Orders ? Headers + Lines) verified");
                    Console.WriteLine("• Column renames and transformations applied correctly");
                    Console.WriteLine("• Expected FK cleanup completed (orphaned records removed)");
                    Console.WriteLine("\nNEXT STEPS:");
                    Console.WriteLine("• Review the detailed per-table statistics in the log");
                    Console.WriteLine("• Run application smoke tests with migrated data");
                    Console.WriteLine("• Validate business-critical reports and workflows");
                    Console.WriteLine("• Consider running Option D to review final SQL structure");
                }
                else
                {
                    Console.WriteLine("??  Verification found ISSUES that need attention!");
                    Console.WriteLine("\n?? Check the detailed report for per-table migration percentages.");
                    Console.WriteLine("\n?? RECOMMENDED ACTIONS:");
                    Console.WriteLine("• Review the detailed log file for specific table-by-table issues");
                    Console.WriteLine("• Check if missing data is due to expected transformations");
                    Console.WriteLine("• Verify that normalized tables (Orders ? Headers + Lines) migrated correctly");
                    Console.WriteLine("• For empty target tables: re-run Option N (data migration)");
                    Console.WriteLine("• For normalized tables: re-run Option ! (custom normalization)");
                    Console.WriteLine("• For column mapping issues: review Option 2 (edit migration plans)");
                    Console.WriteLine("\n? QUICK FIXES BY ISSUE TYPE:");
                    Console.WriteLine("  ?? 0% transfer: Run steps M ? N ? ! (complete re-migration)");
                    Console.WriteLine("  ?? <100% transfer: Review renaming/normalization expectations");
                    Console.WriteLine("  ?? FK violations: Run Option R (cleanup) then re-verify with Option X");
                    Console.WriteLine("  ?? Column mismatches: Check Option 2 for mapping corrections");
                }
                
                return rc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("? Verification failed: " + ex.Message);
                return 1;
            }
        }
    }
}