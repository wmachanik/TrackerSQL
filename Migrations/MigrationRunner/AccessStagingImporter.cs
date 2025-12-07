using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal static class AccessStagingImporter
    {
        private sealed class AccessSchemaDoc
        {
            public string SourceTable { get; set; }
            public List<AccessCol> Columns { get; set; }
        }
        private sealed class AccessCol
        {
            public string Name { get; set; }
        }

        public static int StageAll(string migrationsDir, string accessConnString, string sqlConnString, out string logPath)
        {
            var logsDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs");
            Directory.CreateDirectory(logsDir);
            logPath = Path.Combine(logsDir, "StageAccess_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            var log = new List<string>();

            try
            {
                var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                var schemaFiles = Directory.Exists(accessSchemaDir)
                    ? Directory.EnumerateFiles(accessSchemaDir, "*.schema.json", SearchOption.TopDirectoryOnly)
                        .Where(p => !string.Equals(Path.GetFileName(p), "index.json", StringComparison.OrdinalIgnoreCase))
                        .ToArray()
                    : Array.Empty<string>();

                using (var sql = new SqlConnection(sqlConnString))
                using (var acc = new OleDbConnection(accessConnString))
                {
                    sql.Open();
                    acc.Open();

                    EnsureSchema(sql, "AccessSrc", log);

                    var jsonDocs = schemaFiles.Select(p => TryReadSchema(p, log))
                                              .Where(d => d != null && !string.IsNullOrWhiteSpace(d.SourceTable))
                                              .ToList();

                    var jsonTableSet = new HashSet<string>(jsonDocs.Select(d => d.SourceTable.Trim()), StringComparer.OrdinalIgnoreCase);
                    var accessTables = GetAccessUserTables(acc);
                    var tablesToStage = jsonTableSet.Count > 0 ? jsonTableSet.ToList() : accessTables.ToList();

                    if (tablesToStage.Count == 0)
                    {
                        log.Add("No tables to stage: neither AccessSchema JSON nor Access user tables found.");
                        File.WriteAllLines(logPath, log);
                        return 2;
                    }

                    int ok = 0, fail = 0, skipped = 0;

                    foreach (var src in tablesToStage.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    {
                        var srcTrim = src.Trim();
                        var dstTwoPart = $"[AccessSrc].[{srcTrim.Replace("]", "]]")}]";

                        try
                        {
                            if (!AccessTableExists(acc, srcTrim))
                            {
                                log.Add($"SKIP [{srcTrim}]: not found in Access.");
                                skipped++;
                                continue;
                            }

                            var jsonCols = jsonDocs.FirstOrDefault(d => d.SourceTable.Equals(srcTrim, StringComparison.OrdinalIgnoreCase))?.Columns
                                           ?? new List<AccessCol>();
                            var colNames = jsonCols.Select(c => c?.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                            if (colNames.Count == 0)
                                colNames = GetAccessColumnNames(acc, srcTrim);
                            if (colNames.Count == 0)
                            {
                                log.Add($"SKIP [{srcTrim}]: no columns could be discovered.");
                                skipped++;
                                continue;
                            }

                            var srcCount = GetAccessCount(acc, srcTrim);

                            DropTableIfExists(sql, "AccessSrc", srcTrim);
                            CreateStagingTable(sql, "AccessSrc", srcTrim, colNames);

                            bool usedFallback = false;

                            using (var cmd = acc.CreateCommand())
                            {
                                cmd.CommandText = $"SELECT * FROM [{srcTrim}]";
                                cmd.CommandTimeout = 0;

                                try
                                {
                                    // Try fast streaming path (no SequentialAccess)
                                    using (var rdr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                                    using (var bulk = new SqlBulkCopy(sql, SqlBulkCopyOptions.KeepNulls, null))
                                    {
                                        bulk.DestinationTableName = dstTwoPart;
                                        bulk.BulkCopyTimeout = 0;
                                        bulk.BatchSize = 10000;
                                        bulk.EnableStreaming = true;

                                        var schemaTable = rdr.GetSchemaTable();
                                        var rows = schemaTable?.Rows.Cast<DataRow>()
                                            .OrderBy(r => SafeOrdinal(r))
                                            .ToList() ?? new List<DataRow>();

                                        foreach (var row in rows)
                                        {
                                            var colName = row["ColumnName"] as string;
                                            if (!string.IsNullOrWhiteSpace(colName))
                                                bulk.ColumnMappings.Add(colName, colName);
                                        }

                                        bulk.WriteToServer(rdr);
                                    }
                                }
                                catch (Exception)
                                {
                                    // Any reader/bulk copy issue -> robust fallback
                                    usedFallback = true;
                                    DeleteAll(sql, "AccessSrc", srcTrim);
                                    CopyViaDataTable(acc, sql, srcTrim, dstTwoPart, colNames);
                                }
                            }

                            var dstCount = GetSqlCount(sql, "AccessSrc", srcTrim);
                            log.Add($"OK staged [{srcTrim}] -> {dstTwoPart}: sourceRows={srcCount}, rowsCopied={dstCount}, columns={colNames.Count}{(usedFallback ? ", path=fallback" : ", path=stream")}");
                            ok++;
                        }
                        catch (Exception ex)
                        {
                            log.Add($"ERROR staging [{srcTrim}]: {ex.Message}");
                            fail++;
                        }
                    }

                    log.Add($"Summary: ok={ok} skipped={skipped} failed={fail}");
                }

                File.WriteAllLines(logPath, log);
                return 0;
            }
            catch (Exception ex)
            {
                log.Add("FATAL: " + ex);
                File.WriteAllLines(logPath, log);
                return 1;
            }
        }

        private static void CopyViaDataTable(OleDbConnection acc, SqlConnection sql, string srcTable, string dstTwoPart, List<string> colNames)
        {
            // Destination-shaped DataTable (all NVARCHAR)
            var dt = new DataTable();
            foreach (var c in colNames)
            {
                var name = c ?? "";
                dt.Columns.Add(name, typeof(string));
            }

            // Precompute ordinals once using GetOrdinal
            var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            using (var cmd = acc.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM [{srcTable}]";
                cmd.CommandTimeout = 0;

                using (var rdr = cmd.ExecuteReader(CommandBehavior.SingleResult)) // no SequentialAccess
                using (var bulk = new SqlBulkCopy(sql, SqlBulkCopyOptions.KeepNulls, null))
                {
                    bulk.DestinationTableName = dstTwoPart;
                    bulk.BulkCopyTimeout = 0;
                    bulk.BatchSize = 10000;

                    foreach (DataColumn dc in dt.Columns)
                        bulk.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);

                    // Build ordinals per requested column
                    foreach (var c in colNames)
                    {
                        int ord;
                        try { ord = rdr.GetOrdinal(c); }
                        catch { ord = -1; }
                        ordinals[c] = ord;
                    }

                    const int FlushSize = 10000;
                    while (rdr.Read())
                    {
                        var row = dt.NewRow();
                        foreach (var c in colNames)
                        {
                            var ord = ordinals[c];
                            if (ord < 0 || rdr.IsDBNull(ord))
                            {
                                row[c] = DBNull.Value;
                                continue;
                            }

                            object v = rdr.GetValue(ord);
                            if (v == null)
                            {
                                row[c] = DBNull.Value;
                            }
                            else if (v is byte[] bytes)
                            {
                                // Store binary as base64 in NVARCHAR(MAX)
                                row[c] = Convert.ToBase64String(bytes);
                            }
                            else if (v is DateTime dtVal)
                            {
                                row[c] = dtVal.ToString("o", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                row[c] = Convert.ToString(v, CultureInfo.InvariantCulture);
                            }
                        }
                        dt.Rows.Add(row);

                        if (dt.Rows.Count >= FlushSize)
                        {
                            bulk.WriteToServer(dt);
                            dt.Clear();
                        }
                    }

                    if (dt.Rows.Count > 0)
                    {
                        bulk.WriteToServer(dt);
                        dt.Clear();
                    }
                }
            }
        }

        private static int SafeOrdinal(DataRow row)
        {
            try
            {
                var o = row.Table.Columns.Contains("ColumnOrdinal") ? row["ColumnOrdinal"] : null;
                return o == null || o is DBNull ? int.MaxValue : Convert.ToInt32(o);
            }
            catch { return int.MaxValue; }
        }

        private static AccessSchemaDoc TryReadSchema(string path, List<string> log)
        {
            try
            {
                var json = File.ReadAllText(path);
                // Prefer deserializing the full TableSchema so we can honour Column ordinal/SourceName
                try
                {
                    var ts = JsonConvert.DeserializeObject<TableSchema>(json);
                    if (ts != null)
                    {
                        var doc = new AccessSchemaDoc { SourceTable = ts.SourceTable, Columns = new List<AccessCol>() };
                        if (ts.Columns != null)
                        {
                            // Preserve Access ordinal ordering when present
                            foreach (var c in ts.Columns.OrderBy(c => c.Ordinal))
                            {
                                var nm = c.SourceName ?? c.SourceName ?? "";
                                if (!string.IsNullOrWhiteSpace(nm)) doc.Columns.Add(new AccessCol { Name = nm });
                            }
                        }
                        return doc;
                    }
                }
                catch { /* non-fatal: try legacy shape */ }

                // Fallback: legacy minimal schema shape
                var doc2 = JsonConvert.DeserializeObject<AccessSchemaDoc>(json);
                return doc2;
            }
            catch (Exception ex)
            {
                log.Add("Skip schema '" + path + "': " + ex.Message);
                return null;
            }
        }

        private static void EnsureSchema(SqlConnection sql, string schema, List<string> log)
        {
            using (var cmd = sql.CreateCommand())
            {
                cmd.CommandText = $"IF SCHEMA_ID(N'{schema.Replace("'", "''")}') IS NULL EXEC('CREATE SCHEMA {schema}')";
                cmd.ExecuteNonQuery();
            }
            log.Add("Ensured schema [" + schema + "]");
        }

        private static void DropTableIfExists(SqlConnection sql, string schema, string name)
        {
            using (var cmd = sql.CreateCommand())
            {
                cmd.CommandText = $"IF OBJECT_ID(N'{schema}.{name}', 'U') IS NOT NULL DROP TABLE [{schema}].[{name.Replace("]", "]]")}]";
                cmd.ExecuteNonQuery();
            }
        }

        private static void CreateStagingTable(SqlConnection sql, string schema, string name, List<string> colNames)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [{schema}].[{name.Replace("]", "]]")}] (");
            for (int i = 0; i < colNames.Count; i++)
            {
                var c = colNames[i].Replace("]", "]]");
                sb.Append($"    [{c}] NVARCHAR(MAX) NULL");
                if (i < colNames.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine(");");
            using (var cmd = sql.CreateCommand())
            {
                cmd.CommandText = sb.ToString();
                cmd.ExecuteNonQuery();
            }
        }

        private static bool AccessTableExists(OleDbConnection acc, string table)
        {
            try
            {
                using (var cmd = acc.CreateCommand())
                {
                    cmd.CommandText = $"SELECT TOP 1 * FROM [{table}]";
                    using (cmd.ExecuteReader()) { }
                }
                return true;
            }
            catch { return false; }
        }

        private static List<string> GetAccessUserTables(OleDbConnection acc)
        {
            var tables = new List<string>();
            var schema = acc.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            foreach (DataRow row in schema.Rows)
            {
                var name = row["TABLE_NAME"] as string;
                var type = row["TABLE_TYPE"] as string;
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (name.StartsWith("MSys", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.Equals(type, "TABLE", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(type, "VIEW", StringComparison.OrdinalIgnoreCase))
                    continue;
                tables.Add(name);
            }
            return tables.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static List<string> GetAccessColumnNames(OleDbConnection acc, string table)
        {
            var cols = new List<string>();
            try
            {
                using (var cmd = acc.CreateCommand())
                {
                    cmd.CommandText = $"SELECT TOP 0 * FROM [{table}]";
                    using (var rdr = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        var schema = rdr.GetSchemaTable();
                        foreach (DataRow row in schema.Rows)
                        {
                            var col = row["ColumnName"] as string;
                            if (!string.IsNullOrWhiteSpace(col))
                                cols.Add(col);
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    var restrictions = new object[] { null, null, table, null };
                    var schema = acc.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions);
                    foreach (DataRow row in schema.Rows)
                    {
                        var col = row["COLUMN_NAME"] as string;
                        if (!string.IsNullOrWhiteSpace(col))
                            cols.Add(col);
                    }
                }
                catch { }
            }
            return cols.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static long GetAccessCount(OleDbConnection acc, string table)
        {
            try
            {
                using (var cmd = acc.CreateCommand())
                {
                    cmd.CommandText = $"SELECT COUNT(*) FROM [{table}]";
                    var o = cmd.ExecuteScalar();
                    return Convert.ToInt64(o ?? 0);
                }
            }
            catch { return -1; }
        }

        private static long GetSqlCount(SqlConnection sql, string schema, string table)
        {
            try
            {
                using (var cmd = sql.CreateCommand())
                {
                    cmd.CommandText = $"SELECT COUNT(*) FROM [{schema}].[{table.Replace("]", "]]")}]";
                    var o = cmd.ExecuteScalar();
                    return Convert.ToInt64(o ?? 0);
                }
            }
            catch { return -1; }
        }

        private static void DeleteAll(SqlConnection sql, string schema, string table)
        {
            using (var cmd = sql.CreateCommand())
            {
                // Clear any partially inserted rows if the table exists
                cmd.CommandText = $"IF OBJECT_ID(N'{schema}.{table}', 'U') IS NOT NULL DELETE FROM [{schema}].[{table.Replace("]", "]]")}]";
                cmd.ExecuteNonQuery();
            }
        }
    }
}