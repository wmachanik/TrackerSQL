using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace MigrationRunner.UI
{
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
        public string Description => "Apply data migration script (with enhanced data cleaning)";

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

                // Since MS already cleaned the dates automatically, we can just apply the migration
                Console.WriteLine("?? Applying data migration script...");
                Console.WriteLine("??  Note: Date cleaning was already applied during Step MS (staging)");
                
                var rc = DmlScriptApplier.ApplyLatest(_migrationsDir, sqlCs, out var logPath);
                Console.WriteLine("Data migration apply rc=" + rc + " log: " + logPath);
                
                if (rc != 0 && File.Exists(logPath))
                {
                    // Show error summary
                    var lines = File.ReadAllLines(logPath);
                    var errorLines = lines.Where(l => l.Contains("ERROR")).Take(10).ToArray();
                    
                    if (errorLines.Length > 0)
                    {
                        Console.WriteLine("\n?? Error Summary (first 10 errors):");
                        foreach (var error in errorLines)
                        {
                            Console.WriteLine("  " + error);
                        }
                    }
                }
                else if (rc == 0)
                {
                    Console.WriteLine("\n? Data migration completed successfully!");
                    Console.WriteLine("?? Tip: Run spot-check verification to confirm all tables migrated correctly");
                }
                
                return rc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("? Data migration apply failed: " + ex.Message);
                return 1;
            }
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
            var sqlCs = _config?.TargetConnectionString ?? "";
            return CustomNormalizeRunner.Run(_migrationsDir, sqlCs);
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
        public string Description => "Drop ALL tables in database (DANGER!)";

        public int Execute()
        {
            Console.WriteLine("?? WARNING: This will drop ALL user tables!");
            Console.Write("Type 'DROP ALL' to confirm: ");
            var confirm = Console.ReadLine();
            if (confirm != "DROP ALL")
            {
                Console.WriteLine("Cancelled.");
                return 0;
            }

            try
            {
                using (var conn = new SqlConnection(_config.TargetConnectionString))
                {
                    conn.Open();
                    var sql = @"
-- Drop FKs first
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + N''ALTER TABLE '' + QUOTENAME(SCHEMA_NAME(parent_object_id)) + N''.'' + QUOTENAME(OBJECT_NAME(parent_object_id)) + N'' DROP CONSTRAINT '' + QUOTENAME(name) + N'';''
FROM sys.foreign_keys;
IF LEN(@sql) > 0 EXEC sp_executesql @sql;

-- Drop tables
SET @sql = N'';
SELECT @sql = @sql + N''DROP TABLE '' + QUOTENAME(SCHEMA_NAME(schema_id)) + N''.'' + QUOTENAME(name) + N'';''
FROM sys.tables
WHERE type = ''U'' AND SCHEMA_NAME(schema_id) NOT IN (''sys'', ''INFORMATION_SCHEMA'');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 600;
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine("? All tables dropped");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error: {ex.Message}");
                return 1;
            }
        }
    }
}
