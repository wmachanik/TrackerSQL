using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MigrationRunner
{
    internal static class DmlScriptApplier
    {
        public static int ApplyLatest(string migrationsDir, string connectionString, out string logPath)
        {
            var sqlDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Sql");
            var logsDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs");
            Directory.CreateDirectory(sqlDir);
            Directory.CreateDirectory(logsDir);
            logPath = Path.Combine(logsDir, "ApplyData_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");

            var data = Directory.EnumerateFiles(sqlDir, "DataMigration_*.sql", SearchOption.TopDirectoryOnly)
                                .Select(p => new FileInfo(p)).OrderByDescending(fi => fi.LastWriteTimeUtc).FirstOrDefault();
            var dataLatest = Path.Combine(sqlDir, "DataMigration_LATEST.sql");
            var scriptPath = data?.FullName ?? (File.Exists(dataLatest) ? dataLatest : null);

            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
            {
                sb.AppendLine("No DataMigration SQL files found in: " + sqlDir);
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return 2;
            }

            sb.AppendLine("Applying DATA MIGRATION script to target DB");
            sb.AppendLine("File:");
            sb.AppendLine("  - " + scriptPath);

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    var infoBuf = new StringBuilder();
                    conn.InfoMessage += (s, e) =>
                    {
                        if (e?.Message != null)
                        {
                            foreach (var line in e.Message.Replace("\r", "").Split('\n'))
                            {
                                if (string.IsNullOrWhiteSpace(line)) continue;
                                infoBuf.AppendLine("SQL> " + line.TrimEnd());
                            }
                        }
                    };
                    conn.FireInfoMessageEventOnUserErrors = true;

                    conn.Open();

                    int batches = ExecuteBatches(conn, File.ReadAllText(scriptPath), sb, Path.GetFileName(scriptPath));

                    // Append captured PRINT/RAISERROR messages
                    var infoText = infoBuf.ToString();
                    if (infoText.Length > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine("-- Messages from SQL Server (PRINT/INFO):");
                        sb.Append(infoText);
                    }

                    // Heuristic: treat common severity messages printed via InfoMessage as failure
                    var patterns = new[] { "Invalid column name", "Incorrect syntax near", "Invalid object name", "ERROR migrate" };
                    bool infoHadErrors = patterns.Any(p => infoText.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0);

                    sb.AppendLine();
                    sb.AppendLine($"Summary: batchesExecuted={batches}{(infoHadErrors ? ", detectedErrorsInInfoMessages=true" : "")}");
                    File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);

                    return infoHadErrors ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("ERROR: " + ex);
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return 1;
            }
        }

        // Split on GO batch separators (line with only GO, any casing)
        private static int ExecuteBatches(SqlConnection conn, string script, StringBuilder log, string currentFile)
        {
            var regex = new Regex(@"^\s*GO\s*;$|^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var parts = regex.Split(script ?? string.Empty).Select(p => p?.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            int executed = 0;

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                log.AppendLine($"-- BEGIN BATCH {i + 1}/{parts.Count}");
                log.AppendLine("   Snippet: " + Summarize(part));
                log.AppendLine("   Full batch follows:");
                log.AppendLine(part);
                log.AppendLine("-- END BATCH BODY");

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

        private static string Summarize(string sql)
        {
            var s = (sql ?? "").Replace("\r", "").Replace("\n", " ");
            return s.Length <= 400 ? s : s.Substring(0, 397) + "...";
        }
    }
}