using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks; // added for heartbeat runner

namespace MigrationRunner.UI
{
    internal sealed class AllInOneMigrationCommand : IMenuCommand
    {
        private readonly string _migrationsDir;
        private readonly MigrationConfig _config;

        public AllInOneMigrationCommand(string migrationsDir, MigrationConfig config)
        {
            _migrationsDir = migrationsDir;
            _config = config;
        }

        public string Key => "Y";
        public string Description => "Full pipeline: run A..N (DDL create, FKs, apply DDL, stage Access, generate/apply data migration) (alt key Y)";

        public int Execute()
        {
            // Gather connection strings
            var access = _config?.AccessConnectionString ?? "";
            var sql = _config?.TargetConnectionString ?? "";

            Console.Write("Access connection string [{0}]: ", string.IsNullOrWhiteSpace(access) ? "(empty)" : access);
            var aIn = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(aIn)) access = aIn.Trim();

            Console.Write("SQL connection string [{0}]: ", string.IsNullOrWhiteSpace(sql) ? "(empty)" : sql);
            var sIn = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(sIn)) sql = sIn.Trim();

            if (string.IsNullOrWhiteSpace(access) || string.IsNullOrWhiteSpace(sql))
            {
                Console.Error.WriteLine("Both Access and SQL connection strings are required.");
                return 2;
            }

            // Purge previously generated SQL files (same as P)
            try
            {
                var sqlFolder = Path.Combine(_migrationsDir, "Metadata", "PlanEdits", "Sql");
                if (Directory.Exists(sqlFolder))
                {
                    var files = Directory.GetFiles(sqlFolder, "*.sql", SearchOption.TopDirectoryOnly);
                    int deleted = 0;
                    foreach (var f in files)
                    {
                        try { File.Delete(f); deleted++; } catch { /* ignore per-file */ }
                    }
                    Console.WriteLine($"Purged {deleted} *.sql files from: {sqlFolder}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warning: purge of SQL folder failed: " + ex.Message);
            }

            // Ensure DB exists
            try
            {
                EnsureDatabaseExists(sql);
                Console.WriteLine("OK ensured database exists.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FAIL ensure database: " + ex.Message);
                return 1;
            }

            // Optional drop
            Console.Write("Drop all existing user tables in target DB? [y/N]: ");
            var dropIn = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dropIn) && dropIn.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase))
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
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
FROM sys.foreign_keys;
IF LEN(@sql) > 0 EXEC sp_executesql @sql;

SET @sql = N' ';
SELECT @sql = @sql + N'DROP TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N';'
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name NOT IN ('sys','INFORMATION_SCHEMA');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;";
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

            // A) Generate CREATE TABLE DDL
            try
            {
                var rcA = DdlScriptGenerator.GenerateCreateTables(_migrationsDir, false, false, out var createPath);
                Console.WriteLine("A) Generate CREATE TABLE rc=" + rcA + " file: " + createPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("A) Failed to generate CREATE TABLE script: " + ex.Message);
                return 1;
            }

            // B) Generate FK DDL
            try
            {
                var rcB = DdlScriptGenerator.GenerateForeignKeys(_migrationsDir, out var fkPath);
                Console.WriteLine("B) Generate FK script rc=" + rcB + " file: " + fkPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("B) Failed to generate FK script: " + ex.Message);
                return 1;
            }

            // C) Apply DDL (CREATEs + FKs)
            try
            {
                string ddlLog = null;
                int rcC = RunWithHeartbeat(() => DdlScriptApplier.ApplyLatest(_migrationsDir, sql, out ddlLog), "C) Apply DDL");
                Console.WriteLine("C) Apply DDL rc=" + rcC + " log: " + ddlLog);
                Tail(ddlLog, 32);
                if (rcC != 0)
                {
                    Console.WriteLine("Apply DDL reported issues. Aborting pipeline.");
                    return rcC;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("C) Failed to apply DDL: " + ex.Message);
                return 1;
            }

            // MS) Stage Access -> [AccessSrc]
            try
            {
                string stageLog = null;
                int rcStage = RunWithHeartbeat(() => AccessStagingImporter.StageAll(_migrationsDir, access, sql, out stageLog), "MS) Stage Access");
                Console.WriteLine("MS) Stage Access rc=" + rcStage + " log: " + stageLog);
                Tail(stageLog, 32);
                if (rcStage != 0) return rcStage;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FAIL staging: " + ex);
                return 1;
            }

            // M) Generate DataMigration script
            string dataSqlPath = null;
            try
            {
                // generation can take time for large catalogs; run with heartbeat
                int rcGen = RunWithHeartbeat(() => DmlScriptGenerator.GenerateDataMigration(_migrationsDir, out dataSqlPath), "M) Generate DataMigration");
                Console.WriteLine("M) Generate DataMigration rc=" + rcGen + " file: " + dataSqlPath);
                if (rcGen != 0) return rcGen;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("M) FAIL generate data migration: " + ex);
                return 1;
            }

            // N) Apply DataMigration
            string applyLog = null;
            try
            {
                int rcApply = RunWithHeartbeat(() => DmlScriptApplier.ApplyLatest(_migrationsDir, sql, out applyLog), "N) Apply DATA");
                Console.WriteLine("N) Apply DATA rc=" + rcApply + " log: " + applyLog);
                Tail(applyLog, 48);
                if (rcApply != 0)
                {
                    Console.WriteLine("Data migration reported issues.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("N) FAIL apply data migration: " + ex);
                return 1;
            }

            // V) VERIFY: Critical data quality checks
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("V) VERIFICATION: Checking data quality...");
            Console.WriteLine("========================================");
            try
            {
                VerifyMigrationDataQuality(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARNING: Verification checks failed: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("All steps A..N completed. Run 'O' to run the detailed spot-check verifier if desired.");
            return 0;
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
                    // Build a safely-escaped database name and run CREATE DATABASE if missing
                    var safeDb = (database ?? "").Replace("]", "]]").Replace("'", "''");
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

        // Run a blocking work function while printing periodic heartbeat messages so the user sees progress.
        private static int RunWithHeartbeat(Func<int> work, string label, int heartbeatSeconds = 5)
        {
            Console.WriteLine($"{label} START: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var task = Task.Run(work);
            while (!task.Wait(TimeSpan.FromSeconds(heartbeatSeconds)))
            {
                Console.WriteLine($"{label} still running... {DateTime.Now:yyyy-MM-dd HH:mm:ss} (elapsed {sw.Elapsed:c})");
            }
            sw.Stop();
            int rc = task.Result;
            Console.WriteLine($"{label} END: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (elapsed {sw.Elapsed:c}) rc={rc}");
            return rc;
        }

        private static void VerifyFromScript(string sqlConnString, string dataScriptPath)
        {
            if (string.IsNullOrWhiteSpace(dataScriptPath) || !File.Exists(dataScriptPath))
            {
                Console.WriteLine("Skip verification: DataMigration script not found.");
                return;
            }

            // Parse pairs like: "-- SourceTbl -> TargetTbl"
            var pairs = new List<(string Source, string Target)>();
            foreach (var line in File.ReadLines(dataScriptPath))
            {
                if (!line.StartsWith("-- ")) continue;
                var m = Regex.Match(line, @"--\s+(?<src>[^\s]+)\s+->\s+(?<tgt>[^\s]+)");
                if (m.Success)
                {
                    pairs.Add((m.Groups["src"].Value.Trim(), m.Groups["tgt"].Value.Trim()));
                }
            }
            if (pairs.Count == 0)
            {
                Console.WriteLine("No source/target pairs parsed from script for verification.");
                return;
            }

            using (var conn = new SqlConnection(sqlConnString))
            {
                conn.Open();
                Console.WriteLine("-- Verification: counts per migrated pair");
                foreach (var p in pairs.Distinct())
                {
                    try
                    {
                        var srcCnt = ExecuteCount(conn, $"SELECT COUNT(*) FROM [AccessSrc].[{p.Source.Replace("]", "]]")}] ");
                        var dstCnt = ExecuteCount(conn, $"SELECT COUNT(*) FROM [{p.Target.Replace("]", "]]")}] ");
                        Console.WriteLine($"  {p.Source} -> {p.Target}: source={srcCnt}, target={dstCnt}");

                        // Try sampling missing IDs if target has identity column with same name on source
                        var idCol = GetIdentityColumn(conn, p.Target);
                        if (!string.IsNullOrWhiteSpace(idCol) && SourceHasColumn(conn, p.Source, idCol))
                        {
                            var missing = SampleMissingIds(conn, p.Source, p.Target, idCol);
                            if (missing.Count > 0)
                            {
                                Console.WriteLine($"    Missing {missing.Count} sample(s) by [{idCol}]: " + string.Join(", ", missing));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  {p.Source} -> {p.Target}: verification error: {ex.Message}");
                    }
                }
            }
        }

        private static long ExecuteCount(SqlConnection conn, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandTimeout = 0; // allow long-running counts
                var o = cmd.ExecuteScalar();
                return Convert.ToInt64(o ?? 0);
            }
        }

        private static string GetIdentityColumn(SqlConnection conn, string targetTable)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 0; // may scan metadata on large schemas
                cmd.CommandText = @"
SELECT c.name 
FROM sys.identity_columns ic
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name = @t";
                cmd.Parameters.AddWithValue("@t", targetTable);
                var o = cmd.ExecuteScalar();
                return (o as string) ?? Convert.ToString(o);
            }
        }

        private static bool SourceHasColumn(SqlConnection conn, string sourceTable, string col)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 0; // metadata query
                cmd.CommandText = @"
SELECT 1 
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = N'AccessSrc' AND t.name = @t AND c.name = @c";
                cmd.Parameters.AddWithValue("@t", sourceTable);
                cmd.Parameters.AddWithValue("@c", col);
                var o = cmd.ExecuteScalar();
                return o != null && o != DBNull.Value;
            }
        }

        private static List<string> SampleMissingIds(SqlConnection conn, string sourceTable, string targetTable, string idCol)
        {
            var result = new List<string>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 0; // allow potentially slow sampling query

                // Escape closing bracket for safe identifier quoting
                string idColEsc = idCol?.Replace("]", "]]" ) ?? idCol;
                string srcEsc = sourceTable?.Replace("]", "]]" ) ?? sourceTable;
                string tgtEsc = targetTable?.Replace("]", "]]" ) ?? targetTable;

                cmd.CommandText = $@"
SELECT TOP 5 CONVERT(nvarchar(64), s.[{idColEsc}])
FROM [AccessSrc].[{srcEsc}] s
LEFT JOIN [{tgtEsc}] t ON t.[{idColEsc}] = s.[{idColEsc}]
WHERE t.[{idColEsc}] IS NULL
ORDER BY NEWID()";

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(rdr.IsDBNull(0) ? "NULL" : Convert.ToString(rdr.GetValue(0)));
                    }
                }
            }
            return result;
        }

        private static void VerifyMigrationDataQuality(string sqlConnString)
        {
            using (var conn = new SqlConnection(sqlConnString))
            {
                conn.Open();
                
                // 1. Check ContactsItemsPredictedTbl has dates (not NULL)
                Console.WriteLine("1) Checking ContactsItemsPredictedTbl dates...");
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = @"
                        SELECT 
                            COUNT(*) AS TotalRows,
                            SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS NextCoffeeBy_HasDates,
                            SUM(CASE WHEN NextCleanOn IS NOT NULL THEN 1 ELSE 0 END) AS NextCleanOn_HasDates,
                            SUM(CASE WHEN NextFilterEst IS NOT NULL THEN 1 ELSE 0 END) AS NextFilterEst_HasDates,
                            SUM(CASE WHEN NextDescaleEst IS NOT NULL THEN 1 ELSE 0 END) AS NextDescaleEst_HasDates
                        FROM ContactsItemsPredictedTbl";
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var total = Convert.ToInt32(reader["TotalRows"]);
                            var coffeeCount = Convert.ToInt32(reader["NextCoffeeBy_HasDates"] ?? 0);
                            var cleanCount = Convert.ToInt32(reader["NextCleanOn_HasDates"] ?? 0);
                            var filterCount = Convert.ToInt32(reader["NextFilterEst_HasDates"] ?? 0);
                            var descaleCount = Convert.ToInt32(reader["NextDescaleEst_HasDates"] ?? 0);
                            
                            Console.WriteLine($"   Total rows: {total}");
                            Console.WriteLine($"   NextCoffeeBy with dates: {coffeeCount} ({(total > 0 ? coffeeCount * 100.0 / total : 0):F1}%)");
                            Console.WriteLine($"   NextCleanOn with dates: {cleanCount} ({(total > 0 ? cleanCount * 100.0 / total : 0):F1}%)");
                            Console.WriteLine($"   NextFilterEst with dates: {filterCount} ({(total > 0 ? filterCount * 100.0 / total : 0):F1}%)");
                            Console.WriteLine($"   NextDescaleEst with dates: {descaleCount} ({(total > 0 ? descaleCount * 100.0 / total : 0):F1}%)");
                            
                            if (total > 0)
                            {
                                var coffeePercent = coffeeCount * 100.0 / total;
                                if (coffeePercent < 90)
                                {
                                    Console.WriteLine("   ??  WARNING: Less than 90% of rows have NextCoffeeBy dates!");
                                    Console.WriteLine("   ??  This may indicate the datetime import failed.");
                                }
                                else if (coffeePercent >= 95)
                                {
                                    Console.WriteLine("   ? GOOD: Date columns are properly populated!");
                                }
                                else
                                {
                                    Console.WriteLine("   ??  PARTIAL: Some dates are missing (90-95% populated)");
                                }
                            }
                            else
                            {
                                Console.WriteLine("   ??  WARNING: ContactsItemsPredictedTbl is empty!");
                            }
                        }
                    }
                }
                
                // 2. Check key tables have data (not empty)
                Console.WriteLine();
                Console.WriteLine("2) Checking key tables have data...");
                
                var criticalTables = new[] 
                { 
                    "ContactsTbl", 
                    "ContactsItemsPredictedTbl", 
                    "ContactsUsageTbl",
                    "ItemsTbl",
                    "AreasTbl"
                };
                
                foreach (var table in criticalTables)
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandText = $"SELECT COUNT(*) FROM {table}";
                            var count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                            
                            var status = count > 0 ? "?" : "?? ";
                            Console.WriteLine($"   {status} {table}: {count:N0} rows");
                            
                            if (count == 0)
                            {
                                Console.WriteLine($"      WARNING: {table} is empty!");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ? {table}: Error checking - {ex.Message}");
                    }
                }
                
                // 3. Check for orphan records (FK violations that were skipped)
                Console.WriteLine();
                Console.WriteLine("3) Checking for orphan records in ContactsItemsPredictedTbl...");
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = @"
                        SELECT COUNT(*) AS OrphanCount
                        FROM AccessSrc.ClientUsageTbl src
                        WHERE src.CustomerId IS NOT NULL
                          AND NOT EXISTS (
                              SELECT 1 FROM ContactsTbl c WHERE c.ContactID = src.CustomerId
                          )";
                    
                    var orphanCount = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    
                    if (orphanCount > 0)
                    {
                        Console.WriteLine($"   ??  Found {orphanCount} orphan records in AccessSrc.ClientUsageTbl");
                        Console.WriteLine($"   These were excluded from migration (no matching ContactID in ContactsTbl)");
                    }
                    else
                    {
                        Console.WriteLine($"   ? No orphan records found");
                    }
                }
                
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Verification Summary:");
                Console.WriteLine("- Date columns checked for NULL values");
                Console.WriteLine("- Table row counts verified");
                Console.WriteLine("- Orphan records identified");
                Console.WriteLine("========================================");
            }
        }
    }
}