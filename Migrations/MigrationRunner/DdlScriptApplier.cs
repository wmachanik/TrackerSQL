using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MigrationRunner
{
    internal static class DdlScriptApplier
    {
        public static int ApplyLatest(string migrationsDir, string connectionString, out string logPath)
        {
            var sqlDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Sql");
            var logsDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs");
            Directory.CreateDirectory(sqlDir);
            Directory.CreateDirectory(logsDir);
            logPath = Path.Combine(logsDir, "ApplyDdl_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");

            var create = Directory.EnumerateFiles(sqlDir, "CreateTables_*.sql", SearchOption.TopDirectoryOnly)
                                  .Select(p => new FileInfo(p)).OrderByDescending(fi => fi.LastWriteTimeUtc).FirstOrDefault();
            var fks = Directory.EnumerateFiles(sqlDir, "AddForeignKeys_*.sql", SearchOption.TopDirectoryOnly)
                               .Select(p => new FileInfo(p)).OrderByDescending(fi => fi.LastWriteTimeUtc).FirstOrDefault();

            var steps = new List<FileInfo>();
            if (create != null) steps.Add(create);
            if (fks != null) steps.Add(fks);

            var sb = new StringBuilder();
            if (steps.Count == 0)
            {
                sb.AppendLine("No SQL files found to apply in: " + sqlDir);
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return 2;
            }

            sb.AppendLine("Applying DDL scripts to target DB");
            sb.AppendLine("Files:");
            foreach (var fi in steps) sb.AppendLine("  - " + fi.FullName);

            // Stats
            var createSql = create != null ? SafeRead(create.FullName) : "";
            var fkSql = fks != null ? SafeRead(fks.FullName) : "";
            var expectedTables = ExtractCreateTableNames(createSql).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var createStmtCount = CountCreateTableStatements(createSql);
            var fkStmtCount = CountAddForeignKeyStatements(fkSql);

            if (expectedTables.Count > 0)
                sb.AppendLine("Expected tables from script: " + string.Join(", ", expectedTables));
            sb.AppendLine($"Script stats: CREATE TABLE stmts={createStmtCount}, FK stmts={fkStmtCount}");

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Snapshot tables before
                    var before = GetTableSet(conn);

                    int totalBatches = 0;
                    foreach (var fi in steps)
                    {
                        sb.AppendLine();
                        sb.AppendLine("-- Executing: " + fi.Name);
                        var script = File.ReadAllText(fi.FullName);
                        int batches = ExecuteBatches(conn, script, sb, fi.Name);
                        totalBatches += batches;
                        sb.AppendLine($"-- Done {fi.Name} (batches={batches})");
                    }

                    // Snapshot after and diff
                    var after = GetTableSet(conn);
                    var created = after.Except(before, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

                    int presentExpected = 0;
                    if (expectedTables.Count > 0)
                    {
                        presentExpected = expectedTables.Count(t => TableExists(conn, t));
                        sb.AppendLine();
                        sb.AppendLine($"Post-check: {presentExpected}/{expectedTables.Count} expected tables exist.");
                        var missing = expectedTables.Where(t => !TableExists(conn, t)).ToList();
                        if (missing.Count > 0) sb.AppendLine("Missing expected: " + string.Join(", ", missing));
                    }

                    sb.AppendLine();
                    sb.AppendLine($"Tables created in this session: {created.Count}");
                    if (created.Count > 0)
                        sb.AppendLine("Created: " + string.Join(", ", created));

                    sb.AppendLine();
                    sb.AppendLine($"Summary: batchesExecuted={totalBatches}, createStatements={createStmtCount}, fkStatements={fkStmtCount}, tablesExpected={expectedTables.Count}, tablesPresent={presentExpected}, tablesCreatedNow={created.Count}");
                }

                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return 0;
            }
            catch (Exception ex)
            {
                sb.AppendLine("ERROR: " + ex);
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return 1;
            }
        }

        private static string SafeRead(string path)
        {
            try { return File.Exists(path) ? File.ReadAllText(path) : ""; } catch { return ""; }
        }

        // Split on GO batch separators (line with only GO, any casing)
        private static int ExecuteBatches(SqlConnection conn, string script, StringBuilder log, string currentFile)
        {
            var regex = new Regex(@"^\s*GO\s*;$|^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var parts = regex.Split(script ?? string.Empty);
            int executed = 0;

            foreach (var part in parts.Select(p => p?.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = part;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        executed++;
                    }
                    catch (SqlException sex)
                    {
                        log.AppendLine("ERROR executing batch:");
                        log.AppendLine("  File: " + currentFile);
                        log.AppendLine("  Snippet: " + Summarize(part));
                        log.AppendLine("  SqlException: Number=" + sex.Number + " State=" + sex.State + " Class=" + sex.Class);
                        log.AppendLine("  Message: " + sex.Message);
                        try
                        {
                            foreach (SqlError e in sex.Errors)
                                log.AppendLine($"    -> {e.Number} (Line {e.LineNumber}): {e.Message}");
                        }
                        catch { /* ignore */ }
                        throw;
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine("ERROR executing batch:");
                        log.AppendLine("  File: " + currentFile);
                        log.AppendLine("  Snippet: " + Summarize(part));
                        log.AppendLine("  Exception: " + ex.Message);
                        throw;
                    }
                }
            }
            return executed;
        }

        private static List<string> ExtractCreateTableNames(string sql)
        {
            var res = new List<string>();
            if (string.IsNullOrWhiteSpace(sql)) return res;

            // Matches:
            // CREATE TABLE [schema].[Name] (    OR     CREATE TABLE [Name] (     OR    CREATE TABLE schema.[Name] (
            var rx = new Regex(@"CREATE\s+TABLE\s+(?:\[(?<schema>[^\]]+)\]|(?<schema>\w+))?(?:\.\s*)?\[(?<name>[^\]]+)\]\s*\(",
                               RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var m = rx.Matches(sql);
            foreach (Match x in m)
            {
                var schema = x.Groups["schema"]?.Value;
                var name = x.Groups["name"]?.Value;
                if (string.IsNullOrWhiteSpace(name)) continue;
                res.Add(string.IsNullOrWhiteSpace(schema) ? ("dbo." + name) : (schema + "." + name));
            }
            return res.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }

        private static int CountCreateTableStatements(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return 0;
            return Regex.Matches(sql, @"\bCREATE\s+TABLE\b", RegexOptions.IgnoreCase).Count;
        }

        private static int CountAddForeignKeyStatements(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return 0;
            // count ALTER TABLE ... ADD CONSTRAINT ... FOREIGN KEY
            return Regex.Matches(sql, @"\bALTER\s+TABLE\b[\s\S]*?\bFOREIGN\s+KEY\b", RegexOptions.IgnoreCase).Count;
        }

        private static bool TableExists(SqlConnection conn, string table)
        {
            string schema = "dbo";
            string name = table;
            var parts = table.Split('.');
            if (parts.Length == 2) { schema = parts[0]; name = parts[1]; }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT 1 FROM sys.tables t INNER JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE s.name=@s AND t.name=@n";
                cmd.Parameters.AddWithValue("@s", schema);
                cmd.Parameters.AddWithValue("@n", name);
                var o = cmd.ExecuteScalar();
                return o != null;
            }
        }

        private static HashSet<string> GetTableSet(SqlConnection conn)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT s.name + '.' + t.name FROM sys.tables t INNER JOIN sys.schemas s ON s.schema_id=t.schema_id";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        set.Add(rd.GetString(0));
                }
            }
            return set;
        }

        private static string Summarize(string sql)
        {
            var s = (sql ?? "").Replace("\r", "").Replace("\n", " ");
            return s.Length <= 400 ? s : s.Substring(0, 397) + "...";
        }
    }
}