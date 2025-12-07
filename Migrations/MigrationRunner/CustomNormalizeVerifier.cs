using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace MigrationRunner
{
    internal static class CustomNormalizeVerifier
    {
        public static void VerifyCustomNormalizedTables(string cs)
        {
            try
            {
                using (var sql = new SqlConnection(cs))
                {
                    sql.Open();
                    // Compare counts and random samples for Orders/OrderLines
                    VerifyTablePair(sql, "AccessSrc", "OrdersTbl", "dbo", "OrdersTbl", "OrderID", new[] { "CustomerId", "RequiredByDate", "ItemTypeID", "QuantityOrdered" });
                    VerifyTablePair(sql, "AccessSrc", "ReoccuringOrderTbl", "dbo", "RecurringOrdersTbl", "ID", new[] { "CustomerID", "ItemRequiredID", "ItemPackagingID", "QtyRequired" }, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Custom normalize verification failed: " + ex.Message);
            }
        }

        private static void VerifyTablePair(SqlConnection sql, string srcSchema, string srcTable, string tgtSchema, string tgtTable, string keyCol, string[] sampleCols, bool recurring = false)
        {
            Console.WriteLine($"Verifying {srcTable} -> {tgtTable}...");

            // Count source rows
            long srcCount = ExecuteScalarLong(sql, $"SELECT COUNT(*) FROM [{srcSchema}].[{srcTable}]");
            // Count target header rows
            long tgtCount = ExecuteScalarLong(sql, $"SELECT COUNT(*) FROM [{tgtSchema}].[{tgtTable}]");
            Console.WriteLine($"Counts: source={srcCount}, target={tgtCount}");

            // Pre-fetch target columns and types to avoid building invalid multi-part identifiers and to detect date-like types
            var targetCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var targetColTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var c = new SqlCommand(@"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table", sql))
            {
                c.Parameters.AddWithValue("@schema", tgtSchema);
                c.Parameters.AddWithValue("@table", tgtTable);
                using (var r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var name = r.GetString(0);
                        targetCols.Add(name);
                        var dt = r.IsDBNull(1) ? null : r.GetString(1);
                        targetColTypes[name] = dt ?? "";
                    }
                }
            }

            // Helper to try map a source column name to a target column (simple fallbacks for renamed columns)
            string MapToTargetCol(string srcCol)
            {
                if (targetCols.Contains(srcCol)) return srcCol;
                // common rename fallbacks
                var tries = new[] { ("Customer", "Contact"), ("Client", "Contact") };
                foreach (var t in tries)
                {
                    if (srcCol.IndexOf(t.Item1, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var cand = System.Text.RegularExpressions.Regex.Replace(srcCol, t.Item1, t.Item2, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (targetCols.Contains(cand)) return cand;
                    }
                }
                // fallback: try remove trailing 'ID' vs 'Id' differences
                if (srcCol.EndsWith("ID", StringComparison.Ordinal))
                {
                    var alt = srcCol.Substring(0, srcCol.Length - 2) + "Id";
                    if (targetCols.Contains(alt)) return alt;
                }
                if (srcCol.EndsWith("Id", StringComparison.Ordinal))
                {
                    var alt = srcCol.Substring(0, srcCol.Length - 2) + "ID";
                    if (targetCols.Contains(alt)) return alt;
                }
                return null;
            }

            // Sample random source rows and check mapping exists in target (simple heuristic)
            var q = $"SELECT TOP (100) {string.Join(", ", sampleCols.Select(c => "s." + c))}, s.{keyCol} FROM [{srcSchema}].[{srcTable}] s ORDER BY NEWID();";
            using (var cmd = new SqlCommand(q, sql))
            using (var rd = cmd.ExecuteReader())
            {
                int checkedRows = 0, ok = 0, missing = 0;
                while (rd.Read())
                {
                    checkedRows++;
                    // Build where clause to find header in target
                    var parts = new List<string>();
                    foreach (var c in sampleCols.Take(2)) // just use first two columns for match
                    {
                        var tgtCol = MapToTargetCol(c);
                        if (string.IsNullOrWhiteSpace(tgtCol))
                        {
                            Console.WriteLine($"WARN: target {tgtSchema}.{tgtTable} does not contain column '{c}'; skipping this column for matching in VerifyTablePair.");
                            continue;
                        }

                        var val = rd[c];
                        var tgtType = targetColTypes.ContainsKey(tgtCol) ? targetColTypes[tgtCol] : null;

                        if (val == DBNull.Value || val == null)
                        {
                            parts.Add($"({Qi(tgtSchema)}.{Qi(tgtTable)}.{Qi(tgtCol)} IS NULL)");
                        }
                        else if (IsDateLike(tgtType, tgtCol))
                        {
                            // For date-like types use TRY_CONVERT to avoid hard conversion errors
                            // Try to parse source value to a DateTime to emit canonical literal when possible
                            if (val is DateTime dtVal)
                            {
                                parts.Add($"(TRY_CONVERT(datetime2(7), {Qi(tgtSchema)}.{Qi(tgtTable)}.{Qi(tgtCol)}) = TRY_CONVERT(datetime2(7), '{dtVal.ToString("o")}', 127))");
                            }
                            else
                            {
                                DateTime parsed;
                                var sval = Convert.ToString(val);
                                if (DateTime.TryParse(sval, out parsed))
                                {
                                    parts.Add($"(TRY_CONVERT(datetime2(7), {Qi(tgtSchema)}.{Qi(tgtTable)}.{Qi(tgtCol)}) = TRY_CONVERT(datetime2(7), '{parsed.ToString("o")}', 127))");
                                }
                                else
                                {
                                    // compare via TRY_CONVERT on both sides; if parse fails it'll be NULL and won't match (safe)
                                    parts.Add($"(TRY_CONVERT(datetime2(7), {Qi(tgtSchema)}.{Qi(tgtTable)}.{Qi(tgtCol)}) = TRY_CONVERT(datetime2(7), N'{sval.Replace("'","''")}', 127))");
                                }
                            }
                        }
                        else
                        {
                            // normal literal comparison
                            parts.Add($"({Qi(tgtSchema)}.{Qi(tgtTable)}.{Qi(tgtCol)} = {ParamLiteral(val)})");
                        }
                    }

                    var where = string.Join(" AND ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
                    if (string.IsNullOrWhiteSpace(where)) continue;

                    var checkSql = $"SELECT TOP(1) 1 FROM [{tgtSchema}].[{tgtTable}] WHERE {where};";
                    using (var c2 = new SqlCommand(checkSql, sql))
                    {
                        var found = c2.ExecuteScalar();
                        if (found != null) ok++; else missing++;
                    }
                    if (checkedRows >= 100) break;
                }
                Console.WriteLine($"Sample checked={checkedRows}, ok={ok}, missing={missing}");
            }
        }

        private static long ExecuteScalarLong(SqlConnection sql, string sqlText)
        {
            using (var cmd = new SqlCommand(sqlText, sql))
            {
                var o = cmd.ExecuteScalar();
                return Convert.ToInt64(o ?? 0);
            }
        }

        private static string Qi(string s) => "[" + s.Replace("]", "]]") + "]";

        private static string ParamLiteral(object v)
        {
            if (v == null || v == DBNull.Value) return "NULL";
            if (v is string) return "N'" + v.ToString().Replace("'", "''") + "'";
            if (v is DateTime) return "'" + ((DateTime)v).ToString("o") + "'";
            return v.ToString();
        }

        private static bool IsDateLike(string sqlType, string colName)
        {
            if (!string.IsNullOrWhiteSpace(sqlType))
            {
                var t = sqlType.ToLowerInvariant();
                if (t.Contains("date") || t.Contains("time")) return true;
                return false;
            }
            if (string.IsNullOrWhiteSpace(colName)) return false;
            var n = colName.ToLowerInvariant();
            if (n.EndsWith("id")) return false;
            return n.Contains("date") || n.Contains("time");
        }
    }
}
