using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace MigrationRunner
{
    internal static class CustomNormalizeRunner
    {
        public static int Run(string migrationsDir, string sqlConnString)
        {
            try
            {
                using (var sql = new SqlConnection(sqlConnString))
                {
                    sql.Open();
                    using (var tx = sql.BeginTransaction())
                    {
                        // Migrate OrdersTbl
                        MigrateOrders(migrationsDir, sql, tx);

                        // Migrate ReoccuringOrderTbl -> RecurringOrders + RecurringOrderItems
                        MigrateRecurring(migrationsDir, sql, tx);

                        tx.Commit();
                    }
                }
                Console.WriteLine("Custom normalize migration completed.");
                try
                {
                    var logsDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs");
                    Directory.CreateDirectory(logsDir);
                    var marker = Path.Combine(logsDir, "CustomNormalizeRun.flag");
                    File.WriteAllText(marker, DateTime.UtcNow.ToString("o"));
                }
                catch { /* non-fatal */ }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Custom normalize migration failed: " + ex.Message);
                return 1;
            }
        }

        private static void MigrateOrders(string migrationsDir, SqlConnection sql, SqlTransaction tx)
        {
            Console.WriteLine("Starting custom migrate: OrdersTbl -> OrdersTbl + OrderLinesTbl");
            // Prepare log file for verbose diagnostics to avoid flooding console
            var logsDir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs");
            try { Directory.CreateDirectory(logsDir); } catch { }
            var logPath = Path.Combine(logsDir, "CustomNormalize_Run_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            StreamWriter log = null;
            try { log = new StreamWriter(logPath, false, System.Text.Encoding.UTF8) { AutoFlush = true }; log.WriteLine("Custom normalize run started: " + DateTime.Now.ToString("O")); } catch { log = null; }

            // Migrate OrdersTbl
            var accessSrc = "AccessSrc";
            var srcTable = "OrdersTbl";
            var hdrTable = "OrdersTbl";
            var lineTable = "OrderLinesTbl";

            var schemaPath = Path.Combine(migrationsDir, "Metadata", "AccessSchema", srcTable + ".schema.json");
            JObject schema = null;
            try { if (File.Exists(schemaPath)) schema = JObject.Parse(File.ReadAllText(schemaPath)); } catch { schema = null; }

            // Build mapping dictionaries from ColumnActions
            var srcToTarget = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var targetToSrc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            List<string> headerCols = null, lineCols = null;
            string lineLink = "OrderID";
            if (schema != null)
            {
                var plan = schema.SelectToken("Plan");
                var actionsToken = plan?.SelectToken("ColumnActions");
                
                // Fix: Check if actionsToken is actually a JArray before casting
                JArray actions = null;
                if (actionsToken != null)
                {
                    if (actionsToken is JArray actionArray)
                    {
                        actions = actionArray;
                    }
                    else if (actionsToken is JValue jValue && jValue.Value == null)
                    {
                        // Handle null case
                        actions = null;
                    }
                    else
                    {
                        Console.WriteLine($"Warning: ColumnActions is not a JArray, it is: {actionsToken.GetType().Name}");
                        Console.WriteLine($"ColumnActions content: {actionsToken.ToString()}");
                        actions = null;
                    }
                }
                
                if (actions != null)
                {
                    foreach (var a in actions)
                    {
                        var src = (a.Value<string>("Source") ?? "").Trim();
                        var tgt = (a.Value<string>("Target") ?? src).Trim();
                        if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(tgt)) continue;
                        if (!srcToTarget.ContainsKey(src)) srcToTarget[src] = tgt;
                        if (!targetToSrc.ContainsKey(tgt)) targetToSrc[tgt] = src;
                    }
                }
                
                // Fix: Safely handle Normalize section
                var normalizeToken = plan?.SelectToken("Normalize");
                if (normalizeToken != null && normalizeToken.Type != JTokenType.Null)
                {
                    var headerColsToken = normalizeToken.SelectToken("HeaderColumns");
                    var lineColsToken = normalizeToken.SelectToken("LineColumns");
                    var lineLinkToken = normalizeToken.SelectToken("LineLinkKeyName");
                    
                    if (headerColsToken is JArray headerArray)
                    {
                        headerCols = headerArray.Values<string>()?.ToList();
                    }
                    
                    if (lineColsToken is JArray lineArray) 
                    {
                        lineCols = lineArray.Values<string>()?.ToList();
                    }
                    
                    if (lineLinkToken != null && lineLinkToken.Type != JTokenType.Null)
                    {
                        lineLink = lineLinkToken.Value<string>() ?? "OrderID";
                    }
                }
            }

            // Load staging rows
            var srcFull = "[" + accessSrc + "].[" + srcTable + "]";
            var rows = new List<Dictionary<string, object>>();
            using (var cmd = new SqlCommand($"SELECT * FROM {srcFull}", sql, tx))
            using (var rdr = cmd.ExecuteReader())
            {
                var cols = Enumerable.Range(0, rdr.FieldCount).Select(i => rdr.GetName(i)).ToArray();
                while (rdr.Read())
                {
                    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var c in cols) dict[c] = rdr[c] is DBNull ? null : rdr[c];

                    // Augment dictionary so both source and target names are available
                    // If the staging table uses target column names (renamed), make the original source name point to same value
                    foreach (var c in cols)
                    {
                        // If this column is a target name -> map back to source
                        if (targetToSrc.TryGetValue(c, out var srcName))
                        {
                            if (!dict.ContainsKey(srcName)) dict[srcName] = dict[c];
                        }
                        // If this column is a source name -> map forward to target
                        if (srcToTarget.TryGetValue(c, out var tgtName))
                        {
                            if (!dict.ContainsKey(tgtName)) dict[tgtName] = dict[c];
                        }
                    }
                    rows.Add(dict);
                }
            }

            // Report discovered columns and mappings to help verify correctness
            var allKeys = rows.SelectMany(d => d.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Console.WriteLine($"Staging rows: {rows.Count}, distinct columns: {allKeys.Count}");
            Console.WriteLine("Columns in staging (sample): " + string.Join(", ", allKeys.Take(50)));
            Console.WriteLine("Plan header columns: " + (headerCols == null ? "(none)" : string.Join(", ", headerCols)));
            Console.WriteLine("Plan line columns: " + (lineCols == null ? "(none)" : string.Join(", ", lineCols)));

            var targetHdrCols = GetTableColumns(sql, tx, hdrTable).Where(c => !c.IsIdentity).Select(c => c.Name).ToList();
            var targetLineCols = GetTableColumns(sql, tx, lineTable).Where(c => !c.IsIdentity).Select(c => c.Name).ToList();
            Console.WriteLine("Target header table columns: " + string.Join(", ", targetHdrCols));
            Console.WriteLine("Target line table columns: " + string.Join(", ", targetLineCols));

            Console.WriteLine("Source->Target mappings (sample up to 50): " + string.Join(", ", srcToTarget.Take(50).Select(kv => kv.Key + "=>" + kv.Value)));
            Console.WriteLine("Target->Source mappings (sample up to 50): " + string.Join(", ", targetToSrc.Take(50).Select(kv => kv.Key + "=>" + kv.Value)));

            // Report any header plan columns missing from staging
            if (headerCols != null)
            {
                var missingInStaging = headerCols.Where(h => !allKeys.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();
                if (missingInStaging.Any()) Console.WriteLine("Warning: header plan columns not found in staging: " + string.Join(", ", missingInStaging));
            }
            if (lineCols != null)
            {
                var missingInStaging = lineCols.Where(h => !allKeys.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();
                if (missingInStaging.Any()) Console.WriteLine("Warning: line plan columns not found in staging: " + string.Join(", ", missingInStaging));
            }

            // Group by CustomerId + RequiredByDate, but when CustomerId == 9 include Notes in the group key
            string GetGroupKey(Dictionary<string, object> r)
            {
                object cidObj = null;
                if (r.ContainsKey("CustomerId")) cidObj = r["CustomerId"];
                var cidKey = cidObj?.ToString() ?? "";
                var reqKey = r.ContainsKey("RequiredByDate") ? (r["RequiredByDate"] ?? "").ToString() : "";
                var notesKey = r.ContainsKey("Notes") ? (r["Notes"] ?? "").ToString() : "";
                bool includeNotes = false;
                if (cidObj != null)
                {
                    long cidVal;
                    if (long.TryParse(cidObj.ToString(), out cidVal)) includeNotes = cidVal == 9;
                    else includeNotes = string.Equals(cidObj.ToString(), "9", StringComparison.OrdinalIgnoreCase);
                }
                return includeNotes ? (cidKey + "|" + reqKey + "|" + notesKey) : (cidKey + "|" + reqKey);
            }

            var groups = rows.GroupBy(r => GetGroupKey(r)).ToList();

            int totalGroups = groups.Count;
            Console.WriteLine($"Orders staging rows: {rows.Count}, groups: {totalGroups}");

            // Progress helpers
            var startTime = DateTime.UtcNow;
            // Update frequency: update progress every N groups to reduce console/file churn; choose ~200 updates across whole run
            int updateEvery = Math.Max(1, totalGroups / 200);
            // Shared progress file for external monitoring
            var progressFile = Path.Combine(logsDir, "CustomNormalize_Progress.json");

            // Track insert statistics and failures for diagnosis
            int headerInsertSuccess = 0, headerInsertFail = 0, lineInsertSuccess = 0, lineInsertFail = 0;
            var headerFailures = new List<string>();
            var lineFailures = new List<string>();
            var orphanMappings = new List<string>();
            int zeroTreatedCount = 0;

            void L(string msg)
            {
                try { if (log != null) log.WriteLine(msg); } catch { }
            }

            // Progress/console helpers: keep a single in-place progress line and clear it before printing other messages
            int prevProgressLen = 0;
            void ClearProgressLine()
            {
                try
                {
                    if (prevProgressLen > 0)
                    {
                        Console.Write("\r" + new string(' ', prevProgressLen) + "\r");
                        prevProgressLen = 0;
                    }
                }
                catch { }
            }
            void WriteProgress(string text)
            {
                try
                {
                    Console.Write("\r" + text);
                    try { Console.Out.Flush(); } catch { }
                    prevProgressLen = text.Length;
                }
                catch { }
            }

            int processedHeaders = 0;
            foreach (var g in groups)
            {
                var grpKey = g.Key;
                var first = g.First();

                // Update progress counter in-place so user sees migration progress (stays on one line)
                processedHeaders++;
                try
                {
                    // Show placeholder until chosenSrcOrderId is known
                    if (processedHeaders % updateEvery == 0 || processedHeaders == totalGroups)
                    {
                        var pct = totalGroups > 0 ? (processedHeaders * 100.0 / totalGroups) : 100.0;
                        var elapsed = DateTime.UtcNow - startTime;
                        var avgPer = processedHeaders > 0 ? elapsed.TotalSeconds / processedHeaders : 0.0;
                        var remaining = Math.Max(0, totalGroups - processedHeaders);
                        var etaSec = (int)Math.Round(avgPer * remaining);
                        var eta = TimeSpan.FromSeconds(Math.Max(0, etaSec));
                        var content = $"Processing headers {processedHeaders}/{totalGroups} ({pct:0.0}% ) ETA={eta:hh\\:mm\\:ss}  CurrentSrcOrderId=...";
                        WriteProgress(content);

                        // Write progress JSON for external monitoring
                        try
                        {
                            var prog = new {
                                 Timestamp = DateTime.UtcNow.ToString("o"),
                                 Processed = processedHeaders,
                                 Total = totalGroups,
                                 Percent = Math.Round(pct, 2),
                                 EtaSeconds = etaSec,
                                 Eta = eta.ToString(@"hh\:mm\:ss"),
                                 CurrentSrcOrderId = (long?)null
                             };
                            File.WriteAllText(progressFile, JsonConvert.SerializeObject(prog, Newtonsoft.Json.Formatting.Indented), Encoding.UTF8);
                        }
                        catch { }
                    }
                }
                catch { }

                // Determine chosen source OrderID for this group (use first non-null OrderID)
                long? chosenSrcOrderId = null;
                foreach (var r in g)
                {
                    if (r.ContainsKey("OrderID") && r["OrderID"] != null)
                    {
                        long tmp;
                        if (long.TryParse(r["OrderID"].ToString(), out tmp)) { chosenSrcOrderId = tmp; break; }
                    }
                }

                // Update progress to include discovered chosenSrcOrderId
                try
                {
                    if (processedHeaders % updateEvery == 0 || processedHeaders == totalGroups)
                    {
                        var pct = totalGroups > 0 ? (processedHeaders * 100.0 / totalGroups) : 100.0;
                        var elapsed = DateTime.UtcNow - startTime;
                        var avgPer = processedHeaders > 0 ? elapsed.TotalSeconds / processedHeaders : 0.0;
                        var remaining = Math.Max(0, totalGroups - processedHeaders);
                        var etaSec = (int)Math.Round(avgPer * remaining);
                        var eta = TimeSpan.FromSeconds(Math.Max(0, etaSec));
                        var content2 = $"Processing headers {processedHeaders}/{totalGroups} ({pct:0.0}% ) ETA={eta:hh\\:mm\\:ss}  CurrentSrcOrderId={chosenSrcOrderId ?? -1}";
                        WriteProgress(content2);

                        // Update shared progress file including currentSrcOrderId
                        try
                        {
                            var prog = new {
                                 Timestamp = DateTime.UtcNow.ToString("o"),
                                 Processed = processedHeaders,
                                 Total = totalGroups,
                                 Percent = Math.Round(pct, 2),
                                 EtaSeconds = etaSec,
                                 Eta = eta.ToString(@"hh\:mm\:ss"),
                                 CurrentSrcOrderId = chosenSrcOrderId ?? -1
                             };
                            File.WriteAllText(progressFile, JsonConvert.SerializeObject(prog, Newtonsoft.Json.Formatting.Indented), Encoding.UTF8);
                        }
                        catch { }
                    }
                }
                catch { }

                // Build header insert columns & values
                var insertCols = new List<string>();
                var insertParams = new List<SqlParameter>();
                int p = 0;

                // If chosenSrcOrderId present and target has OrderID identity, include it in insert
                var hdrHasOrderIdIdentity = GetTableColumns(sql, tx, hdrTable).Any(c => string.Equals(c.Name, "OrderID", StringComparison.OrdinalIgnoreCase) && c.IsIdentity);
                if (chosenSrcOrderId.HasValue)
                {
                    // Check if the OrderID already exists in the target table
                    using (var checkCmd = new SqlCommand($"SELECT COUNT(1) FROM [dbo].[{hdrTable}] WHERE OrderID = @id", sql, tx))
                    {
                        checkCmd.Parameters.AddWithValue("@id", chosenSrcOrderId.Value);
                        var existingCount = Convert.ToInt32(checkCmd.ExecuteScalar() ?? 0);
                        
                        if (existingCount > 0)
                        {
                            Console.WriteLine($"Warning: OrderID {chosenSrcOrderId.Value} already exists, letting SQL Server assign new ID");
                            chosenSrcOrderId = null; // Let SQL Server assign a new identity
                            hdrHasOrderIdIdentity = false; // Don't use IDENTITY_INSERT
                        }
                        else
                        {
                            // include OrderID as explicit column so header uses the source order id
                            insertCols.Add("[OrderID]");
                            insertParams.Add(new SqlParameter("@p" + (p++), chosenSrcOrderId.Value));
                        }
                    }
                }
                else
                {
                    // chosenSrcOrderId is null, so SQL Server will assign identity
                }

                foreach (var tcol in targetHdrCols)
                {
                    if (string.Equals(tcol, "OrderID", StringComparison.OrdinalIgnoreCase))
                    {
                        // already handled above if chosenSrcOrderId present
                        continue;
                    }
                    string srcName = null;
                    if (targetToSrc.TryGetValue(tcol, out var s)) srcName = s;
                        else if (tcol.Equals("ContactID", StringComparison.OrdinalIgnoreCase) && first.ContainsKey("CustomerId")) srcName = "CustomerId";
                        else if (tcol.Equals("ContactID", StringComparison.OrdinalIgnoreCase) && first.ContainsKey("ContactID")) srcName = "ContactID"; // Use ContactID if it exists (after CSV mapping)
                    else if (tcol.Equals("RequiredByDate", StringComparison.OrdinalIgnoreCase) && first.ContainsKey("RequiredByDate")) srcName = "RequiredByDate";
                    else if (first.ContainsKey(tcol)) srcName = tcol;

                    if (srcName == null) continue; // no source value

                    var rawVal = first.ContainsKey(srcName) ? first[srcName] : null;

                    // Treat explicit zero IDs as NULL (Access uses 0 like NULL)
                    object normVal = NormalizeParamValue(rawVal);
                    if (normVal != null && normVal != DBNull.Value)
                    {
                        if ((normVal is long ll && ll == 0) || (normVal is int ii && ii == 0) || (normVal is double dd && Math.Abs(dd) < double.Epsilon))
                        {
                            zeroTreatedCount++;
                            L($"Info: treating zero as NULL for header column {tcol} on source Order {chosenSrcOrderId ?? -1}");
                            normVal = DBNull.Value;
                        }
                    }

                    if (normVal == DBNull.Value)
                    {
                        // still include column but set DBNull so target gets NULL
                        insertCols.Add("[" + tcol + "]");
                        var paramName = "@p" + (p++);
                        insertParams.Add(new SqlParameter(paramName, (object)DBNull.Value));
                    }
                    else
                    {
                        insertCols.Add("[" + tcol + "]");
                        var paramName = "@p" + (p++);
                        insertParams.Add(new SqlParameter(paramName, normVal ?? (object)DBNull.Value));
                    }
                }

                if (insertCols.Count == 0)
                    continue;

                // If we will insert a specific OrderID and target has identity, enable IDENTITY_INSERT
                if (chosenSrcOrderId.HasValue && hdrHasOrderIdIdentity)
                {
                    using (var cmdIdOn = new SqlCommand($"SET IDENTITY_INSERT [dbo].[{hdrTable}] ON;", sql, tx)) cmdIdOn.ExecuteNonQuery();
                }

                var insertSql = $"INSERT INTO [dbo].[{hdrTable}] ({string.Join(", ", insertCols)}) VALUES ({string.Join(", ", insertParams.Select(pnm => pnm.ParameterName))});" +
                                (chosenSrcOrderId.HasValue ? " SELECT CAST(@_chosen_orderid AS bigint)" : " SELECT SCOPE_IDENTITY();");

                // Detailed diagnostics written to log (kept out of console to reduce verbosity)
                L("--- HEADER INSERT ---");
                L(insertSql);
                foreach (var prm in insertParams)
                {
                    var valDesc = prm.Value == null ? "NULL (null)" : prm.Value + " (" + prm.Value.GetType().Name + ")";
                    L(prm.ParameterName + " = " + valDesc);
                }

                // Pre-validate critical header FKs to avoid FK constraint failures.
                var missingFks = new List<Tuple<string,string,string,object>>(); // col, refTable, refCol, val
                for (int i = 0; i < insertCols.Count; i++)
                {
                    var col = insertCols[i].Trim('[', ']');
                    var val = insertParams[i].Value;
                    if (val == null || val == DBNull.Value) continue;
                    // check known header FK columns
                    if (string.Equals(col, "ContactID", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var chk = new SqlCommand("SELECT COUNT(1) FROM dbo.ContactsTbl WHERE ContactID = @v", sql, tx))
                        {
                            chk.Parameters.AddWithValue("@v", val);
                            var cnt = Convert.ToInt32(chk.ExecuteScalar() ?? 0);
                            if (cnt == 0) missingFks.Add(Tuple.Create(col, "ContactsTbl", "ContactID", val));
                        }
                    }
                    else if (string.Equals(col, "ToBeDeliveredByID", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var chk = new SqlCommand("SELECT COUNT(1) FROM dbo.PeopleTbl WHERE PersonID = @v", sql, tx))
                        {
                            chk.Parameters.AddWithValue("@v", val);
                            var cnt = Convert.ToInt32(chk.ExecuteScalar() ?? 0);
                            if (cnt == 0) missingFks.Add(Tuple.Create(col, "PeopleTbl", "PersonID", val));
                        }
                    }
                }

                if (missingFks.Count > 0)
                {
                    // Ensure orphan table exists
                    try
                    {
                        using (var cmd = new SqlCommand(@"IF OBJECT_ID(N'dbo.Migration_OrphanedOrders','U') IS NULL
BEGIN
CREATE TABLE dbo.Migration_OrphanedOrders (
    OrphanId INT IDENTITY(1,1) PRIMARY KEY,
    DetectedAt datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    SourceOrderId bigint NULL,
    MissingFkName nvarchar(200) NOT NULL,
    MissingFkValue nvarchar(4000) NULL,
    Details nvarchar(max) NULL
);
END", sql, tx)) cmd.ExecuteNonQuery();
                    }
                    catch (Exception) { /* best-effort */ }

                    // persist details and log
                    foreach (var mf in missingFks)
                    {
                        try
                        {
                            using (var icmd = new SqlCommand("INSERT INTO dbo.Migration_OrphanedOrders (SourceOrderId, MissingFkName, MissingFkValue, Details) VALUES (@src,@name,@val,@det)", sql, tx))
                            {
                                icmd.Parameters.AddWithValue("@src", (object)chosenSrcOrderId ?? DBNull.Value);
                                icmd.Parameters.AddWithValue("@name", mf.Item1);
                                icmd.Parameters.AddWithValue("@val", (object)mf.Item4 ?? DBNull.Value);
                                icmd.Parameters.AddWithValue("@det", $"TargetTable={hdrTable}; attemptedColumns={string.Join(",", insertCols)}");
                                icmd.ExecuteNonQuery();
                            }
                            orphanMappings.Add($"OrphanHeader SrcOrder={chosenSrcOrderId ?? -1} Missing={mf.Item1}:{mf.Item4}");
                            L($"OrphanHeader recorded: SrcOrder={chosenSrcOrderId ?? -1} Missing={mf.Item1}:{mf.Item4}");
                        }
                        catch (Exception ex)
                        {
                            L("Failed to record orphan header: " + ex.Message);
                        }
                    }

                    ClearProgressLine();
                    Console.WriteLine($"SKIP: header OrderID={chosenSrcOrderId ?? -1} - missing FK(s): {string.Join(",", missingFks.Select(x=>x.Item1+"="+Convert.ToString(x.Item4)))}. Recorded to Migration_OrphanedOrders.");
                    headerFailures.Add($"Header OrderID={chosenSrcOrderId ?? -1}: missing FK(s): {string.Join(",", missingFks.Select(x=>x.Item1+"="+Convert.ToString(x.Item4)))}");
                    continue;
                }

                long headerId;
                try
                {
                    using (var cmd = new SqlCommand(insertSql, sql, tx))
                    {
                        cmd.Parameters.AddRange(insertParams.ToArray());
                        if (chosenSrcOrderId.HasValue)
                        {
                            // supply @_chosen_orderid for consistent scalar return
                            cmd.Parameters.AddWithValue("@_chosen_orderid", chosenSrcOrderId.Value);
                            var o = cmd.ExecuteScalar();
                            headerId = Convert.ToInt64(o ?? chosenSrcOrderId.Value);
                        }
                        else
                        {
                            var o = cmd.ExecuteScalar();
                            headerId = Convert.ToInt64(o ?? 0);
                        }
                    }
                    headerInsertSuccess++;
                }
                catch (Exception ex)
                {
                    headerInsertFail++;
                    ClearProgressLine();
                    Console.WriteLine("ERROR: header INSERT failed for OrderID=" + (chosenSrcOrderId ?? -1) + ": " + ex.Message);
                    // write detailed params and SQL to log only
                    L("ERROR: header INSERT failed: " + ex.Message);
                    L(insertSql);
                    foreach (var prm in insertParams)
                    {
                        var valDesc = prm.Value == null ? "NULL (null)" : prm.Value + " (" + prm.Value.GetType().Name + ")";
                        L(prm.ParameterName + " = " + valDesc);
                    }
                    headerFailures.Add($"Header OrderID={chosenSrcOrderId ?? -1}: {ex.Message}");
                    // Can't proceed with lines for this header
                    continue;
                }

                if (chosenSrcOrderId.HasValue && hdrHasOrderIdIdentity)
                {
                    using (var cmdIdOff = new SqlCommand($"SET IDENTITY_INSERT [dbo].[{hdrTable}] OFF;", sql, tx)) cmdIdOff.ExecuteNonQuery();
                }

                // Create OrphanedOrderIdsTbl if not exists
                var createOrphanTableSql = @"IF OBJECT_ID(N'dbo.OrphanedOrderIdsTbl') IS NULL
BEGIN
    CREATE TABLE dbo.OrphanedOrderIdsTbl (
        OldOrderId BIGINT NULL,
        LinkedOrderId BIGINT NULL
    );
END";
                using (var cmd = new SqlCommand(createOrphanTableSql, sql, tx)) cmd.ExecuteNonQuery();

                // Insert orphan mappings for any other source OrderIDs in the group
                foreach (var r in g)
                {
                    if (r.ContainsKey("OrderID") && r["OrderID"] != null)
                    {
                        long v;
                        if (long.TryParse(r["OrderID"].ToString(), out v))
                        {
                            if (!chosenSrcOrderId.HasValue || v != chosenSrcOrderId.Value)
                            {
                                using (var cmd = new SqlCommand("INSERT INTO dbo.OrphanedOrderIdsTbl (OldOrderId, LinkedOrderId) VALUES (@old, @linked);", sql, tx))
                                {
                                    cmd.Parameters.AddWithValue("@old", v);
                                    cmd.Parameters.AddWithValue("@linked", headerId);
                                    try { cmd.ExecuteNonQuery(); orphanMappings.Add($"OrphanedOrder Old={v} -> Linked={headerId}"); L("OrphanedOrderIdsTbl INSERT: Old=" + v + ", Linked=" + headerId); } catch (Exception ex) { L("Orphan insert failed: " + ex.Message); }
                                }
                            }
                        }
                    }
                }

                // Insert lines for this group
                foreach (var row in g)
                {
                    var lnCols = new List<string>();
                    var lnParams = new List<SqlParameter>();
                    int pi = 0;
                    // Ensure LineLink column (OrderID) is set to headerId
                    if (targetLineCols.Contains(lineLink, StringComparer.OrdinalIgnoreCase))
                    {
                        lnCols.Add("[" + lineLink + "]");
                        lnParams.Add(new SqlParameter("@lp" + (pi++), headerId));
                    }

                    foreach (var tcol in targetLineCols)
                    {
                        if (string.Equals(tcol, lineLink, StringComparison.OrdinalIgnoreCase)) continue; // already added
                        string srcName = null;
                        if (targetToSrc.TryGetValue(tcol, out var s)) srcName = s;
                        else if (tcol.Equals("ItemID", StringComparison.OrdinalIgnoreCase) && row.ContainsKey("ItemTypeID")) srcName = "ItemTypeID";
                        else if (tcol.Equals("ItemID", StringComparison.OrdinalIgnoreCase) && row.ContainsKey("ItemID")) srcName = "ItemID"; // Use ItemID if it exists (after CSV mapping)
                        else if (tcol.Equals("QtyOrdered", StringComparison.OrdinalIgnoreCase) && row.ContainsKey("QuantityOrdered")) srcName = "QuantityOrdered";
                        else if (row.ContainsKey(tcol)) srcName = tcol;

                        if (srcName == null) continue;
                        var val = row.ContainsKey(srcName) ? row[srcName] : null;

                        // Resolve FK columns to ensure referenced row exists; if not, set NULL and log
                        object finalVal = val;
                        if (string.Equals(tcol, "PrepTypeID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(tcol, "PackagingID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(tcol, "ItemID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(tcol, "ToBeDeliveredByID", StringComparison.OrdinalIgnoreCase))
                        {
                            finalVal = ResolveFkValue(sql, tx, tcol, val, L);
                        }

                        // Normalize the value before type checks
                        var norm = NormalizeParamValue(finalVal);
                        
                        // Add debugging for problematic column mappings
                        if (string.Equals(tcol, "QuantityOrdered", StringComparison.OrdinalIgnoreCase))
                        {
                            L($"DEBUG: QuantityOrdered mapping - srcName='{srcName}', rawVal='{val}' ({val?.GetType().Name}), finalVal='{finalVal}' ({finalVal?.GetType().Name}), norm='{norm}' ({norm?.GetType().Name})");
                            
                            // Extra validation for QuantityOrdered
                            if (norm is DateTime)
                            {
                                L($"ERROR: DateTime detected in QuantityOrdered for OrderID={headerId} - this suggests wrong column mapping");
                                ClearProgressLine();
                                Console.WriteLine($"ERROR: DateTime detected in QuantityOrdered for OrderID={headerId}");
                                continue; // Skip this column entirely
                            }
                        }
                        lnCols.Add("[" + tcol + "]");
                        var paramName = "@lp" + (pi++);
                        lnParams.Add(new SqlParameter(paramName, norm ?? (object)DBNull.Value));
                    }

                    if (lnCols == null || lnCols.Count == 0) continue;

                    var lnSql = $"INSERT INTO [dbo].[{lineTable}] ({string.Join(", ", lnCols)}) VALUES ({string.Join(", ", lnParams.Select(pnm => pnm.ParameterName))});";

                    // Detailed diagnostics written to log (not to console)
                    L("--- LINE INSERT ---");
                    L(lnSql);
                    foreach (var prm in lnParams)
                    {
                        var valDesc = prm.Value == null ? "NULL (null)" : prm.Value + " (" + prm.Value.GetType().Name + ")";
                        L(prm.ParameterName + " = " + valDesc);
                    }

                    try
                    {
                        using (var cmd = new SqlCommand(lnSql, sql, tx))
                        {
                            cmd.Parameters.AddRange(lnParams.ToArray());
                            cmd.ExecuteNonQuery();
                        }
                        lineInsertSuccess++;
                    }
                    catch (Exception ex)
                    {
                        lineInsertFail++;
                        ClearProgressLine();
                        Console.WriteLine($"ERROR: line INSERT failed for HeaderId={headerId}: {ex.Message}");
                        L("ERROR: line INSERT failed: " + ex.Message);
                        L(lnSql);
                        foreach (var prm in lnParams)
                        {
                            var valDesc = prm.Value == null ? "NULL (null)" : prm.Value + " (" + prm.Value.GetType().Name + ")";
                            L(prm.ParameterName + " = " + valDesc);
                        }
                        lineFailures.Add($"HeaderId={headerId}, OrderLineInsert failed: {ex.Message}");
                        // continue to next line
                        continue;
                    }
                }
            }

            // Summary
            ClearProgressLine();
            var summary = $"Orders migration done. Header inserts: success={headerInsertSuccess}, fail={headerInsertFail}. Line inserts: success={lineInsertSuccess}, fail={lineInsertFail}. Orphan mappings recorded: {orphanMappings.Count}.";
            Console.WriteLine(summary);
            L(summary);
            // Print logfile location and condensed stats
            if (!string.IsNullOrEmpty(logPath)) Console.WriteLine("Detailed log: " + logPath);
            L("Log path: " + logPath);
            if (zeroTreatedCount > 0) Console.WriteLine($"Info: treated {zeroTreatedCount} zero-value IDs as NULL (logged).");
            L($"Zero-treated count: {zeroTreatedCount}");
            if (headerFailures.Any())
            {
                Console.WriteLine("Header failures (sample): " + string.Join("; ", headerFailures.Take(20)));
                L("Header failures: " + string.Join("; ", headerFailures));
            }
            if (lineFailures.Any())
            {
                Console.WriteLine($"Line failures: {lineFailures.Count} (showing up to 20 samples). See detailed log for full list.");
                foreach (var lf in lineFailures.Take(20)) Console.WriteLine("  - " + lf);
                L("Line failures: " + string.Join("; ", lineFailures));
            }
            if (orphanMappings.Count > 0)
            {
                Console.WriteLine($"Orphan mappings: {orphanMappings.Count} (showing up to 20 samples). See detailed log for full list.");
                foreach (var om in orphanMappings.Take(20)) Console.WriteLine("  - " + om);
                L("Orphan mappings: " + string.Join("; ", orphanMappings));
            }
            try { if (log != null) { log.WriteLine("Custom normalize run finished: " + DateTime.Now.ToString("O")); log.Dispose(); } } catch { }
        }

        private static void MigrateRecurring(string migrationsDir, SqlConnection sql, SqlTransaction tx)
        {
            Console.WriteLine("Starting custom migrate: ReoccuringOrderTbl -> RecurringOrdersTbl + RecurringOrderItemsTbl");
            var accessSrc = "AccessSrc";
            var srcTable = "ReoccuringOrderTbl";
            var hdrTable = "RecurringOrdersTbl";
            var lineTable = "RecurringOrderItemsTbl";

            var schemaPath = Path.Combine(migrationsDir, "Metadata", "AccessSchema", srcTable + ".schema.json");
            JObject schema = null;
            try { if (File.Exists(schemaPath)) schema = JObject.Parse(File.ReadAllText(schemaPath)); } catch { schema = null; }

            var srcToTarget = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var targetToSrc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (schema != null)
            {
                var plan = schema.SelectToken("Plan");
                var actionsToken = plan?.SelectToken("ColumnActions");
                
                // Fix: Check if actionsToken is actually a JArray before casting
                JArray actions = null;
                if (actionsToken != null)
                {
                    if (actionsToken is JArray actionArray)
                    {
                        actions = actionArray;
                    }
                    else if (actionsToken is JValue jValue && jValue.Value == null)
                    {
                        // Handle null case
                        actions = null;
                    }
                    else
                    {
                        Console.WriteLine($"Warning: ColumnActions is not a JArray for {srcTable}, it is: {actionsToken.GetType().Name}");
                        Console.WriteLine($"ColumnActions content: {actionsToken.ToString()}");
                        actions = null;
                    }
                }
                
                if (actions != null)
                {
                    foreach (var a in actions)
                    {
                        var src = (a.Value<string>("Source") ?? "").Trim();
                        var tgt = (a.Value<string>("Target") ?? src).Trim();
                        if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(tgt)) continue;
                        if (!srcToTarget.ContainsKey(src)) srcToTarget[src] = tgt;
                        if (!targetToSrc.ContainsKey(tgt)) targetToSrc[tgt] = src;
                    }
                }
            }

            // Load staging rows
            var srcFull = "[" + accessSrc + "].[" + srcTable + "]";
            var rows = new List<Dictionary<string, object>>();
            using (var cmd = new SqlCommand($"SELECT * FROM {srcFull}", sql, tx))
            using (var rdr = cmd.ExecuteReader())
            {
                var cols = Enumerable.Range(0, rdr.FieldCount).Select(i => rdr.GetName(i)).ToArray();
                while (rdr.Read())
                {
                    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var c in cols) dict[c] = rdr[c] is DBNull ? null : rdr[c];
                    rows.Add(dict);
                }
            }

            // Augment rows so lookups work whether staging uses source or target column names
            foreach (var dict in rows)
            {
                var presentCols = dict.Keys.ToList();
                foreach (var c in presentCols)
                {
                    if (targetToSrc.TryGetValue(c, out var srcName))
                    {
                        if (!dict.ContainsKey(srcName)) dict[srcName] = dict[c];
                    }
                    if (srcToTarget.TryGetValue(c, out var tgtName))
                    {
                        if (!dict.ContainsKey(tgtName)) dict[tgtName] = dict[c];
                    }
                }
            }

            // Group by CustomerID for headers
            var groups = rows.GroupBy(r => (r.ContainsKey("CustomerID") ? (r["CustomerID"] ?? "") : ""));
            Console.WriteLine($"Recurring staging rows: {rows.Count}, header groups: {groups.Count()}");

            foreach (var g in groups)
            {
                var first = g.First();

                // Determine chosen source recurring ID for this group (use first non-null ID)
                long? chosenSrcRecurringId = null;
                foreach (var r in g)
                {
                    if (r.ContainsKey("ID") && r["ID"] != null)
                    {
                        long tmp;
                        if (long.TryParse(r["ID"].ToString(), out tmp)) { chosenSrcRecurringId = tmp; break; }
                    }
                }

                // Insert header
                var hdrCols = GetTableColumns(sql, tx, hdrTable).Where(c => !c.IsIdentity).Select(c => c.Name).ToList();
                var insertCols = new List<string>(); var insertParams = new List<SqlParameter>(); int p = 0;

                // If chosen source ID present and target has RecurringOrderID identity, include it
                var hdrHasRecurringIdIdentity = GetTableColumns(sql, tx, hdrTable).Any(c => string.Equals(c.Name, "RecurringOrderID", StringComparison.OrdinalIgnoreCase) && c.IsIdentity);
                if (chosenSrcRecurringId.HasValue)
                {
                    // Check if the ID already exists in the target table
                    using (var checkCmd = new SqlCommand($"SELECT COUNT(1) FROM [dbo].[{hdrTable}] WHERE RecurringOrderID = @id", sql, tx))
                    {
                        checkCmd.Parameters.AddWithValue("@id", chosenSrcRecurringId.Value);
                        var existingCount = Convert.ToInt32(checkCmd.ExecuteScalar() ?? 0);
                        
                        if (existingCount > 0)
                        {
                            Console.WriteLine($"Warning: RecurringOrderID {chosenSrcRecurringId.Value} already exists, letting SQL Server assign a new ID");
                            chosenSrcRecurringId = null; // Let SQL Server assign a new identity
                        }
                        else
                        {
                            insertCols.Add("[RecurringOrderID]");
                            insertParams.Add(new SqlParameter("@p" + (p++), chosenSrcRecurringId.Value));
                        }
                    }
                }
                else
                {
                    // chosenSrcRecurringId is null, so SQL Server will assign identity
                }

                foreach (var tcol in hdrCols)
                {
                    if (string.Equals(tcol, "RecurringOrderID", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    string srcName = null;
                    if (targetToSrc.TryGetValue(tcol, out var s)) srcName = s;
                        else if (tcol.Equals("ContactID", StringComparison.OrdinalIgnoreCase) && first.ContainsKey("CustomerID")) srcName = "CustomerID";
                        else if (tcol.Equals("ContactID", StringComparison.OrdinalIgnoreCase) && first.ContainsKey("ContactID")) srcName = "ContactID"; // Use ContactID if it exists (after CSV mapping)
                    else if (first.ContainsKey(tcol)) srcName = tcol;
                    if (srcName == null) continue;
                    var val = first.ContainsKey(srcName) ? first[srcName] : null;
                    insertCols.Add("[" + tcol + "]");
                    insertParams.Add(new SqlParameter("@p" + (p++), NormalizeParamValue(val) ?? (object)DBNull.Value));
                }
                if (insertCols.Count == 0) continue;

                if (chosenSrcRecurringId.HasValue && hdrHasRecurringIdIdentity)
                {
                    using (var cmdIdOn = new SqlCommand($"SET IDENTITY_INSERT [dbo].[{hdrTable}] ON;", sql, tx)) cmdIdOn.ExecuteNonQuery();
                }

                var insertSql = $"INSERT INTO [dbo].[{hdrTable}] ({string.Join(", ", insertCols)}) VALUES ({string.Join(", ", insertParams.Select(pp => pp.ParameterName))});" +
                                (chosenSrcRecurringId.HasValue ? " SELECT CAST(@_chosen_recurringid AS bigint)" : " SELECT SCOPE_IDENTITY();");
                long headerId;
                using (var cmd = new SqlCommand(insertSql, sql, tx))
                {
                    cmd.Parameters.AddRange(insertParams.ToArray());
                    if (chosenSrcRecurringId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@_chosen_recurringid", chosenSrcRecurringId.Value);
                        var o = cmd.ExecuteScalar();
                        headerId = Convert.ToInt64(o ?? chosenSrcRecurringId.Value);
                    }
                    else
                    {
                        var o = cmd.ExecuteScalar();
                        headerId = Convert.ToInt64(o ?? 0);
                    }
                }

                if (chosenSrcRecurringId.HasValue && hdrHasRecurringIdIdentity)
                {
                    using (var cmdIdOff = new SqlCommand($"SET IDENTITY_INSERT [dbo].[{hdrTable}] OFF;", sql, tx)) cmdIdOff.ExecuteNonQuery();
                }

                // Create OrphanedRecurringOrderIdsTbl if not exists
                var createOrphanRecTableSql = @"IF OBJECT_ID(N'dbo.OrphanedRecurringOrderIdsTbl') IS NULL
BEGIN
    CREATE TABLE dbo.OrphanedRecurringOrderIdsTbl (
        OldRecurringId BIGINT NULL,
        LinkedRecurringId BIGINT NULL
    );
END";
                using (var cmd = new SqlCommand(createOrphanRecTableSql, sql, tx)) cmd.ExecuteNonQuery();

                // Insert orphan mappings for any other source IDs in the group
                foreach (var r in g)
                {
                    if (r.ContainsKey("ID") && r["ID"] != null)
                    {
                        long v;
                        if (long.TryParse(r["ID"].ToString(), out v))
                        {
                            if (!chosenSrcRecurringId.HasValue || v != chosenSrcRecurringId.Value)
                            {
                                using (var cmd = new SqlCommand("INSERT INTO dbo.OrphanedRecurringOrderIdsTbl (OldRecurringId, LinkedRecurringId) VALUES (@old, @linked);", sql, tx))
                                {
                                    cmd.Parameters.AddWithValue("@old", v);
                                    cmd.Parameters.AddWithValue("@linked", headerId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

                // Aggregate lines: group by ItemRequiredID + ItemPackagingID
                var lineGroups = g.GroupBy(r => ((r.ContainsKey("ItemRequiredID") ? (r["ItemRequiredID"] ?? "") : "") + "|" + (r.ContainsKey("ItemPackagingID") ? (r["ItemPackagingID"] ?? "") : "")));
                foreach (var lg in lineGroups)
                {
                    var representative = lg.First();
                    // Sum QtyRequired
                    double sumQty = 0;
                    foreach (var rr in lg)
                    {
                        var v = rr.ContainsKey("QtyRequired") ? rr["QtyRequired"] : rr.ContainsKey("QtyRequired") ? rr["QtyRequired"] : null;
                        if (v == null) continue;
                        double d; if (double.TryParse(v.ToString(), out d)) sumQty += d;
                    }

                    // Build insert into RecurringOrderItemsTbl
                    var lnCols = GetTableColumns(sql, tx, lineTable).Where(c => !c.IsIdentity).Select(c => c.Name).ToList();
                    var colsList = new List<string>(); var paramsList = new List<SqlParameter>(); int pi = 0;
                    // Ensure link column RecurringOrderID
                    if (lnCols.Contains("RecurringOrderID", StringComparer.OrdinalIgnoreCase))
                    {
                        colsList.Add("[RecurringOrderID]"); paramsList.Add(new SqlParameter("@lp" + (pi++), headerId));
                    }
                    foreach (var tcol in lnCols)
                    {
                        if (string.Equals(tcol, "RecurringOrderID", StringComparison.OrdinalIgnoreCase)) continue;
                        string srcName = null;
                        if (targetToSrc.TryGetValue(tcol, out var s)) srcName = s;
                        else if (tcol.Equals("ItemRequiredID", StringComparison.OrdinalIgnoreCase) && representative.ContainsKey("ItemRequiredID")) srcName = "ItemRequiredID";
                        else if (tcol.Equals("ItemPackagingID", StringComparison.OrdinalIgnoreCase) && representative.ContainsKey("ItemPackagingID")) srcName = "ItemPackagingID";
                        else if (tcol.Equals("QtyRequired", StringComparison.OrdinalIgnoreCase)) { /* use sumQty */ }

                        object val = null;
                        if (string.Equals(tcol, "QtyRequired", StringComparison.OrdinalIgnoreCase)) val = sumQty;
                        else if (srcName != null) val = representative.ContainsKey(srcName) ? representative[srcName] : null;
                        else continue;

                        // Normalize and validate FK existence for known FK columns
                        var finalVal = val;
                        if (string.Equals(tcol, "ItemRequiredID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(tcol, "ItemPackagingID", StringComparison.OrdinalIgnoreCase))
                        {
                            // reuse ResolveFkValue logic where appropriate
                            finalVal = ResolveFkValue(sql, tx, tcol.Equals("ItemRequiredID", StringComparison.OrdinalIgnoreCase) ? "ItemID" : tcol, val, null);
                        }

                        var norm = NormalizeParamValue(finalVal);
                        if (norm == DBNull.Value || norm == null)
                        {
                            // If target column allows NULL explicitly, include column with NULL; otherwise record orphan and skip the entire line
                            if (IsColumnNullable(sql, tx, lineTable, tcol))
                            {
                                colsList.Add("[" + tcol + "]");
                                paramsList.Add(new SqlParameter("@lp" + (pi++), (object)DBNull.Value));
                                continue;
                            }

                            // target column is NOT NULL -> record orphan for this recurring line and skip inserting this line
                            try
                            {
                                using (var cmd = new SqlCommand(@"IF OBJECT_ID(N'dbo.Migration_OrphanedRecurringOrderLines','U') IS NULL
BEGIN
CREATE TABLE dbo.Migration_OrphanedRecurringOrderLines (
    OrphanId INT IDENTITY(1,1) PRIMARY KEY,
    DetectedAt datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    SourceRecurringId bigint NULL,
    MissingFkName nvarchar(200) NOT NULL,
    MissingFkValue nvarchar(4000) NULL,
    LineDetails nvarchar(max) NULL
);
END", sql, tx)) cmd.ExecuteNonQuery();
                            }
                            catch { /* best-effort */ }

                            try
                            {
                                var lineDetails = string.Join(",", representative.Select(kv => kv.Key + "=" + (kv.Value == null ? "(null)" : kv.Value.ToString())));
                                using (var icmd = new SqlCommand("INSERT INTO dbo.Migration_OrphanedRecurringOrderLines (SourceRecurringId, MissingFkName, MissingFkValue, LineDetails) VALUES (@src,@name,@val,@det)", sql, tx))
                                {
                                    icmd.Parameters.AddWithValue("@src", (object)headerId ?? (object)DBNull.Value);
                                    icmd.Parameters.AddWithValue("@name", tcol);
                                    object missingVal = finalVal ?? (representative.ContainsKey(srcName) ? representative[srcName] : null);
                                    icmd.Parameters.AddWithValue("@val", missingVal ?? (object)DBNull.Value);
                                    icmd.Parameters.AddWithValue("@det", (object)lineDetails ?? (object)DBNull.Value);
                                    icmd.ExecuteNonQuery();
                                }
                                Console.WriteLine($"Orphan recurring line recorded: Header={headerId} Missing={tcol}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed to record orphan recurring line: " + ex.Message);
                            }

                            // skip entire line
                            colsList = null; break;
                        }

                        // Treat explicit zeros as null for IDs
                        if ((norm is long lnorm && lnorm == 0) || (norm is int inorm && inorm == 0) || (norm is double dnorm && Math.Abs(dnorm) < double.Epsilon))
                        {
                            // treat as missing
                            continue;
                        }

                        colsList.Add("[" + tcol + "]"); paramsList.Add(new SqlParameter("@lp" + (pi++), norm ?? (object)DBNull.Value));
                    }

                    if (colsList == null || colsList.Count == 0) continue;
                    var lnSql = $"INSERT INTO [dbo].[{lineTable}] ({string.Join(", ", colsList)}) VALUES ({string.Join(", ", paramsList.Select(pn => pn.ParameterName))});";
                    using (var cmd = new SqlCommand(lnSql, sql, tx)) { cmd.Parameters.AddRange(paramsList.ToArray()); cmd.ExecuteNonQuery(); }
                }
            }

            Console.WriteLine("Recurring migration done.");
        }

        private sealed class ColInfo { public string Name; public bool IsIdentity; }
        private static List<ColInfo> GetTableColumns(SqlConnection sql, SqlTransaction tx, string table)
        {
            var res = new List<ColInfo>();
            var sqlText = @"SELECT c.name, ic.is_identity
FROM sys.columns c
LEFT JOIN sys.identity_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name = @table AND s.name = @schema
ORDER BY c.column_id";
            using (var cmd = new SqlCommand(sqlText, sql, tx))
            {
                cmd.Parameters.AddWithValue("@table", table);
                cmd.Parameters.AddWithValue("@schema", "dbo");
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read()) res.Add(new ColInfo { Name = (string)rdr[0], IsIdentity = rdr[1] != DBNull.Value && (bool)rdr[1] });
                }
            }
            return res;
        }

        // Cache for column sql types: key = table + "@" + column
        private static readonly Dictionary<string, string> _columnSqlTypeCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // Cache for column nullable flag: key = table + "@" + column -> true if nullable, false if not, null if unknown (error)
        private static readonly Dictionary<string, bool?> _columnNullableCache = new Dictionary<string, bool?>(StringComparer.OrdinalIgnoreCase);

        private static bool IsColumnNullable(SqlConnection sql, SqlTransaction tx, string table, string column)
        {
            var key = table + "@" + column;
            if (_columnNullableCache.TryGetValue(key, out var cached) && cached.HasValue) return cached.Value;
            try
            {
                var sqlText = @"SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @table AND COLUMN_NAME = @col";
                using (var cmd = new SqlCommand(sqlText, sql, tx))
                {
                    cmd.Parameters.AddWithValue("@table", table);
                    cmd.Parameters.AddWithValue("@col", column);
                    var o = cmd.ExecuteScalar();
                    if (o != null && o != DBNull.Value)
                    {
                        var s = o.ToString();
                        var isNullable = string.Equals(s, "YES", StringComparison.OrdinalIgnoreCase);
                        _columnNullableCache[key] = isNullable;
                        return isNullable;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: failed to get nullability for {table}.{column}: {ex.Message}");
            }
            _columnNullableCache[key] = null;
            return false;
        }

        private static object NormalizeParamValue(object val)
        {
            if (val == null) return DBNull.Value;
            if (val is string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return DBNull.Value;
                
                // Try parsing in order of specificity
                // First try boolean (to avoid converting "true"/"false" to dates)
                bool b;
                if (bool.TryParse(s, out b)) return b;
                
                // Try integer values
                long l;
                if (long.TryParse(s, out l)) return l;
                
                // Try decimal values  
                double d;
                if (double.TryParse(s, out d)) return d;
                
                // Try dates only if it looks like a date
                if (s.Length >= 8 && (s.Contains("/") || s.Contains("-") || s.Contains(" ")))
                {
                    DateTime dt;
                    if (DateTime.TryParse(s, out dt)) return dt;
                }
                
                return s; // Return as string if no conversions work
            }
            
            // Handle JValue or other token types from Newtonsoft
            if (val is JValue jv)
            {
                return NormalizeParamValue(jv.Value);
            }
            
            // Handle already-typed values
            if (val is DateTime) return val;
            if (val is bool) return val;
            if (val is byte || val is short || val is int || val is long) return val;
            if (val is float || val is double || val is decimal) return val;
            
            return val;
        }

        // Resolve known FK columns by checking referenced table for existence of the value.
        // If the referenced row doesn't exist, return null so caller can skip/omit the column.
        private static object ResolveFkValue(SqlConnection sql, SqlTransaction tx, string tcol, object val)
        {
            return ResolveFkValue(sql, tx, tcol, val, null);
        }

        private static object ResolveFkValue(SqlConnection sql, SqlTransaction tx, string tcol, object val, Action<string> logWarning)
        {
            if (val == null) return null;
            var n = NormalizeParamValue(val);
            if (n == DBNull.Value) return null;

            string refTable = null, refCol = null;
            if (string.Equals(tcol, "PrepTypeID", StringComparison.OrdinalIgnoreCase)) { refTable = "ItemPrepTypesTbl"; refCol = "ItemPrepID"; }
            else if (string.Equals(tcol, "PackagingID", StringComparison.OrdinalIgnoreCase)) { refTable = "ItemPackagingTbl"; refCol = "ItemPackagingID"; }
            else if (string.Equals(tcol, "ItemID", StringComparison.OrdinalIgnoreCase)) { refTable = "ItemsTbl"; refCol = "ItemID"; }
            else if (string.Equals(tcol, "ToBeDeliveredByID", StringComparison.OrdinalIgnoreCase)) { refTable = "PeopleTbl"; refCol = "PersonID"; }
            else
            {
                // unknown FK column, return normalized value
                return n;
            }

            try
            {
                using (var cmd = new SqlCommand($"SELECT COUNT(1) FROM dbo.[{refTable}] WHERE [{refCol}] = @v", sql, tx))
                {
                    cmd.Parameters.AddWithValue("@v", n);
                    var cnt = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    if (cnt > 0) return n;
                    logWarning?.Invoke($"Warning: FK value for {tcol}='{n}' not found in {refTable}.{refCol}; inserting NULL instead.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                logWarning?.Invoke($"Warning: failed to validate FK {tcol} against {refTable}.{refCol}: {ex.Message}. Using value as-is.");
                return n;
            }
        }

        private static string GetColumnSqlType(SqlConnection sql, SqlTransaction tx, string table, string column)
        {
            var key = table + "@" + column;
            if (_columnSqlTypeCache.TryGetValue(key, out var cached)) return cached;
            try
            {
                var sqlText = @"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @table AND COLUMN_NAME = @col";
                using (var cmd = new SqlCommand(sqlText, sql, tx))
                {
                    cmd.Parameters.AddWithValue("@table", table);
                    cmd.Parameters.AddWithValue("@col", column);
                    var o = cmd.ExecuteScalar();
                    if (o != null && o != DBNull.Value)
                    {
                        var s = o.ToString();
                        _columnSqlTypeCache[key] = s;
                        return s;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: failed to get SQL type for {table}.{column}: {ex.Message}");
            }
            return null;
        }
    }
}
