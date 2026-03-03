using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal static class DmlScriptGenerator
    {
        private static void EmitInsertBlock(StringBuilder sb, string target, string fromObj, List<string> targetCols, List<string> selectCols, bool useIdentityInsert, string chosenIdentity)
        {
            // Build a safe, truncated preview to avoid long literal issues
            var preview = $"INSERT INTO {Qi(target)} ({string.Join(", ", targetCols)}) SELECT {string.Join(", ", selectCols)} FROM {fromObj};";
            var previewTrunc = preview.Length > 1500 ? (preview.Substring(0, 1500) + " ... [truncated]") : preview;

            sb.AppendLine($"    PRINT {Qs("About to execute (IdentityInsert=" + (useIdentityInsert ? "ON" : "OFF") + "):")};");
            sb.AppendLine($"    PRINT {Qs(previewTrunc)};");

            sb.AppendLine("    BEGIN TRY");
            sb.AppendLine("        BEGIN TRAN;");
            if (useIdentityInsert) sb.AppendLine($"        SET IDENTITY_INSERT {Qi(target)} ON;");
            sb.AppendLine($"        INSERT INTO {Qi(target)}");
            sb.AppendLine("        (");
            sb.AppendLine("            " + string.Join(", ", targetCols));
            sb.AppendLine("        )");
            sb.AppendLine("        SELECT");
            sb.AppendLine("            " + string.Join(", ", selectCols));
            sb.AppendLine($"        FROM {fromObj};");
            if (useIdentityInsert)
            {
                sb.AppendLine($"        SET IDENTITY_INSERT {Qi(target)} OFF;");
                // Only reseed when table has an identity; use RESEED without value (reset to current max)
                sb.AppendLine($"        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID({Qs(Unq(target))}))");
                sb.AppendLine($"            DBCC CHECKIDENT ({Qs(Unq(target))}, RESEED);");
            }
            sb.AppendLine("        COMMIT;");
            sb.AppendLine($"        PRINT 'OK migrate [{target}] from ' + {Qs(fromObj)};");
            sb.AppendLine("    END TRY");
            sb.AppendLine("    BEGIN CATCH");
            sb.AppendLine("        IF XACT_STATE() <> 0 ROLLBACK;");
            if (useIdentityInsert)
            {
                sb.AppendLine("        BEGIN TRY");
                sb.AppendLine($"            IF OBJECT_ID({Qs(Unq(target))}) IS NOT NULL SET IDENTITY_INSERT {Qi(target)} OFF;");
                sb.AppendLine("        END TRY BEGIN CATCH END CATCH");
            }
            sb.AppendLine($"        PRINT 'ERROR migrate [{target}] from ' + {Qs(fromObj)} + ': ' + ERROR_MESSAGE();");
            sb.AppendLine("    END CATCH");
        }

        public static int GenerateDataMigration(string migrationsDir, out string sqlPath)
        {
            sqlPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Sql",
                "DataMigration_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".sql");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sqlPath) ?? migrationsDir);

                var accessSchemaDir = ResolveAccessSchemaDir(migrationsDir);
                var constraintsPath = ResolveConstraintsPath(migrationsDir);

                if (!Directory.Exists(accessSchemaDir))
                {
                    File.WriteAllText(sqlPath, "-- Access schema not found: " + accessSchemaDir);
                    return 2;
                }

                var constraints = new ConstraintsIndex();
                if (File.Exists(constraintsPath))
                {
                    try { constraints = JsonConvert.DeserializeObject<ConstraintsIndex>(File.ReadAllText(constraintsPath)) ?? new ConstraintsIndex(); }
                    catch { constraints = new ConstraintsIndex(); }
                }

                // Build full mappings, but filter out Normalize header/line targets for the generic data migration
                var mapsAll = BuildMappings(accessSchemaDir);
                var maps = mapsAll.Where(kv => !kv.Value.IsHeader && !kv.Value.IsLine)
                                  .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

                var order = TopoOrder(maps, constraints);

                // Load business rules if present (used for Normalize generation)
                BusinessRulesIndex brIndex = null;
                try
                {
                    var brPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "BusinessRules.json");
                    if (File.Exists(brPath)) brIndex = JsonConvert.DeserializeObject<BusinessRulesIndex>(File.ReadAllText(brPath));
                }
                catch { brIndex = null; }

                const string SourceSchema = "AccessSrc";

                var sb = new StringBuilder();
                sb.AppendLine("-- Auto-generated DATA MIGRATION script");
                sb.AppendLine("-- Assumes source data is available under schema [" + SourceSchema + "] using Access source table names.");
                sb.AppendLine("-- If you do not have [" + SourceSchema + "] objects, the generator will fall back to unqualified [Source] when Target != Source.");
                sb.AppendLine("-- If Target == Source and " + SourceSchema + ".Source is missing, the table is skipped (no self-select).");
                sb.AppendLine("-- Create schema once if needed: IF SCHEMA_ID('" + SourceSchema + "') IS NULL EXEC('CREATE SCHEMA " + SourceSchema + "');");
                sb.AppendLine("-- AccessSchema: " + accessSchemaDir);
                sb.AppendLine("-- PlanConstraints: " + constraintsPath);
                sb.AppendLine("-- Tables to migrate (ordered): " + order.Count);
                foreach (var t in order.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
                    sb.AppendLine("--   - " + t);
                sb.AppendLine("SET NOCOUNT ON;");
                sb.AppendLine("SET XACT_ABORT ON;");
                sb.AppendLine();

                // Disable FKs on all targets we’re about to load
                var targetNames = order.Select(n => maps[n].Target).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var inList = string.Join(", ", targetNames.Select(n => "N'" + (n ?? "").Replace("'", "''") + "'"));

                sb.AppendLine("-- Disable foreign keys on migrated targets");
                sb.AppendLine("DECLARE @tbl sysname, @fk sysname, @sql nvarchar(max);");
                sb.AppendLine("DECLARE fk_cur CURSOR LOCAL FAST_FORWARD FOR");
                sb.AppendLine("SELECT QUOTENAME(SCHEMA_NAME(t.schema_id))+'.'+QUOTENAME(t.name), QUOTENAME(fk.name)");
                sb.AppendLine("FROM sys.foreign_keys fk");
                sb.AppendLine("JOIN sys.tables t ON t.object_id=fk.parent_object_id");
                sb.AppendLine($"WHERE t.name IN ({inList});");
                sb.AppendLine("OPEN fk_cur;");
                sb.AppendLine("FETCH NEXT FROM fk_cur INTO @tbl, @fk;");
                sb.AppendLine("WHILE @@FETCH_STATUS = 0");
                sb.AppendLine("BEGIN");
                sb.AppendLine("    SET @sql = N'ALTER TABLE ' + @tbl + N' NOCHECK CONSTRAINT ' + @fk + N';';");
                sb.AppendLine("    PRINT @sql; EXEC sp_executesql @sql;");
                sb.AppendLine("    FETCH NEXT FROM fk_cur INTO @tbl, @fk;");
                sb.AppendLine("END");
                sb.AppendLine("CLOSE fk_cur; DEALLOCATE fk_cur;");
                sb.AppendLine("GO");
                sb.AppendLine();

                // Purge target tables before load (child-to-parent)
                var deleteTargets = new List<string>();
                var seenDel = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = order.Count - 1; i >= 0; i--)
                {
                    var t = maps[order[i]].Target;
                    if (seenDel.Add(t)) deleteTargets.Add(t);
                }

                sb.AppendLine("-- Purge target tables before load (child-to-parent)");
                foreach (var t in deleteTargets)
                {
                    var tEsc = (t ?? "").Replace("'", "''");
                    sb.AppendLine($"PRINT N'Purging {Qi(t)}';");
                    sb.AppendLine("BEGIN TRY");
                    sb.AppendLine($"    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk");
                    sb.AppendLine($"                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id");
                    sb.AppendLine($"                   WHERE rt.name = N'{tEsc}')");
                    sb.AppendLine($"        TRUNCATE TABLE {Qi(t)};");
                    sb.AppendLine("    ELSE");
                    sb.AppendLine("    BEGIN");
                    sb.AppendLine($"        PRINT N'INFO: {Qi(t)} has referencing foreign keys – using DELETE';");
                    sb.AppendLine($"        DELETE FROM {Qi(t)};");
                    sb.AppendLine("    END");
                    sb.AppendLine("END TRY");
                    sb.AppendLine("BEGIN CATCH");
                    sb.AppendLine($"    PRINT N'WARN: purge of {Qi(t)} failed: ' + ERROR_MESSAGE();");
                    sb.AppendLine($"    BEGIN TRY DELETE FROM {Qi(t)}; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH");
                    sb.AppendLine("END CATCH");
                }
                sb.AppendLine("GO");
                sb.AppendLine();

                foreach (var name in order)
                {
                    var tm = maps[name];

                    // Filtered mapping must be non-empty
                    if (tm.Columns.Count == 0)
                    {
                        sb.AppendLine($"PRINT 'SKIP migrate [{tm.Target}]: no valid source/target columns to migrate';");
                        sb.AppendLine("GO");
                        sb.AppendLine();
                        continue;
                    }

                    var ct = FindConstraints(constraints, name);
                    var existingTargets = new HashSet<string>(ct?.ColumnTypes?.Keys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                    var cols = existingTargets.Count == 0 ? tm.Columns : tm.Columns.Where(c => existingTargets.Contains(c.Target)).ToList();
                    if (cols.Count == 0) { /* print SKIP and continue */ }
                    var targetColsLocal = cols.Select(c => Qi(c.Target)).ToList();

                    // define chosenIdentity before using it
                    var chosenIdentity = ChooseIdentity(ct, tm);

                    var includeIdentityInInsert = !string.IsNullOrWhiteSpace(chosenIdentity) &&
                                                  tm.Columns.Any(c => c.Target != null && c.Target.Equals(chosenIdentity, StringComparison.OrdinalIgnoreCase));
                    var useIdentityInsert = includeIdentityInInsert; // allow for Temp* tables too

                    // Wrap each source with proper type coercion based on constraints (or name heuristics)
                    var selectCols = cols
                        .Select(c =>
                        {
                            var srcExpr = Qi(c.Source);                  // e.g., [OrderDate]
                            var nvExpr = WrapNullIf(srcExpr);            // NULLIF([OrderDate], N'')
                            var targetType = ResolveTargetType(ct, c.Target);

                            string typedExpr = nvExpr;
                            if (IsDateLike(targetType, c.Target))
                            {
                                // Try multiple date styles (ISO8601 w/ Z, ISO T, ODBC canonical, d/M/y) then default
                                typedExpr = TryConvertDate(nvExpr);
                            }
                            else if (IsBoolLike(targetType, c.Target))
                            {
                                // Map common truthy/falsey spellings to bit, else TRY_CONVERT(bit, ...)
                                typedExpr = TryConvertBit(nvExpr);
                            }

                            return $"{typedExpr} AS {Qi(c.Target)}";
                        })
                        .ToList();

                    // Build two-part object names
                    var accessSrcObj = Q2(SourceSchema, tm.Source);
                    var localSrcObj = Qi(tm.Source);
                    var isSelf = string.Equals(tm.Target, tm.Source, StringComparison.OrdinalIgnoreCase);

                    // Emit mapping diagnostics as comments to help troubleshoot Normalize mappings
                    sb.AppendLine($"-- {tm.Source} -> {tm.Target}" + (tm.IsLine ? " (Normalize: Lines)" : tm.IsHeader ? " (Normalize: Header)" : ""));
                    try
                    {
                        sb.AppendLine($"-- Mapping: columnsCount={tm.Columns.Count}");
                        foreach (var cc in tm.Columns)
                        {
                            sb.AppendLine($"--   {cc.Source} -> {cc.Target}");
                        }
                    }
                    catch { sb.AppendLine("-- Mapping: <failed to enumerate columns>"); }

                    // If this is a Normalize header and we have business rules, emit normalization SQL
                    if (tm.IsHeader && brIndex != null && !string.IsNullOrWhiteSpace(tm.Source) && brIndex.Tables != null && brIndex.Tables.TryGetValue(tm.Source, out var tb) && tb.CompositeKeyRules != null && tb.CompositeKeyRules.Count > 0)
                    {
                        var compRule = tb.CompositeKeyRules[0];
                        // need Line table info from Access schema Normalize plan
                        TableSchema srcSchema = null;
                        try { var srcPath = Path.Combine(accessSchemaDir, (tm.Source ?? "") + ".schema.json"); if (File.Exists(srcPath)) srcSchema = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(srcPath)); } catch { srcSchema = null; }
                        var nplan = srcSchema?.Plan?.Normalize;
                        var lineTable = nplan?.LineTable ?? "";
                        var newHeaderKey = nplan?.NewHeaderKeyName ?? "";
                        var lineLink = nplan?.LineLinkKeyName ?? "";

                        if (string.IsNullOrWhiteSpace(lineTable) || compRule == null)
                        {
                            sb.AppendLine($"-- SKIP normalize for {tm.Source}: missing normalize plan or composite rule");
                            sb.AppendLine("GO"); sb.AppendLine();
                            continue;
                        }

                        // Helpers to build concatenation of columns into a key
                        string BuildConcatSql_local(List<string> colList)
                        {
                            if (colList == null || colList.Count == 0) return "N''";
                            return string.Join(" + N'|' + ", colList.Select(c => $"COALESCE(CONVERT(nvarchar(max), s.{Qi(c)}), N'')"));
                        }

                        var cond = string.IsNullOrWhiteSpace(compRule.Condition) ? "1=0" : compRule.Condition;
                        var trueExpr = BuildConcatSql_local(compRule.TrueKey);
                        var falseExpr = BuildConcatSql_local(compRule.FalseKey);

                        var accessSrcObjNorm = Q2(SourceSchema, tm.Source);

                        sb.AppendLine($"-- Normalize: source={tm.Source} header={tm.Target} lines={lineTable}");
                        sb.AppendLine($"WITH _src AS (SELECT s.*, (CASE WHEN ({cond}) THEN ({trueExpr}) ELSE ({falseExpr}) END) AS __hdr_key FROM {accessSrcObjNorm} s),");
                        sb.AppendLine("_picked AS (");
                        sb.AppendLine("    SELECT *, ROW_NUMBER() OVER (PARTITION BY __hdr_key ORDER BY (SELECT 0)) AS __rn FROM _src");
                        sb.AppendLine(")");

                        // Ensure temp mapping table
                        sb.AppendLine("IF OBJECT_ID('tempdb..#hdr_map') IS NOT NULL DROP TABLE #hdr_map;");
                        sb.AppendLine("CREATE TABLE #hdr_map (HeaderId BIGINT NULL, HeaderKey nvarchar(max));");

                        try
                        {
                            var ctHdr = FindConstraints(constraints, name);
                            var hdrColsLocal = tm.Columns.Select(c => c.Target).ToList();
                            var hdrTargetCols = hdrColsLocal.Select(c => Qi(c)).ToList();
                            var chosenIdentityHdr = ChooseIdentity(ctHdr, tm) ?? hdrColsLocal.FirstOrDefault();

                            // Build header select expressions
                            var hdrSelectExprs = tm.Columns.Select(c =>
                            {
                                var tgtType = ResolveTargetType(ctHdr, c.Target);
                                var expr = $"COALESCE(CONVERT(nvarchar(max), _picked.{Qi(c.Source)}), N'')";
                                if (IsDateLike(tgtType, c.Target)) expr = TryConvertDate($"NULLIF(_picked.{Qi(c.Source)}, N'')");
                                else if (IsBoolLike(tgtType, c.Target)) expr = TryConvertBit($"NULLIF(_picked.{Qi(c.Source)}, N'')");
                                return expr + " AS " + Qi(c.Target);
                            }).ToList();

                            // Insert headers, capture mapping
                            sb.AppendLine("BEGIN TRY");
                            sb.AppendLine($"    INSERT INTO {Qi(tm.Target)} ({string.Join(", ", hdrTargetCols)})");
                            sb.AppendLine("    OUTPUT INSERTED." + (chosenIdentityHdr ?? hdrColsLocal.FirstOrDefault() ?? "0") + " AS HeaderId, _picked.__hdr_key AS HeaderKey INTO #hdr_map(HeaderId, HeaderKey)");
                            sb.AppendLine("    SELECT " + string.Join(", ", hdrSelectExprs));
                            sb.AppendLine("    FROM _picked WHERE __rn = 1;");

                            // Insert lines
                            if (maps.TryGetValue(lineTable, out var tmLine))
                            {
                                var ctLine = FindConstraints(constraints, lineTable);
                                var lineColsLocal = tmLine.Columns.Select(c => c.Target).ToList();
                                var lineTargetCols = lineColsLocal.Select(c => Qi(c)).ToList();

                                var lineSelectExprs = tmLine.Columns.Select(c =>
                                {
                                    var tgtType = ResolveTargetType(ctLine, c.Target);
                                    var expr = $"COALESCE(CONVERT(nvarchar(max), s.{Qi(c.Source)}), N'')";
                                    if (IsDateLike(tgtType, c.Target)) expr = TryConvertDate($"NULLIF(s.{Qi(c.Source)}, N'')");
                                    else if (IsBoolLike(tgtType, c.Target)) expr = TryConvertBit($"NULLIF(s.{Qi(c.Source)}, N'')");
                                    return expr + " AS " + Qi(c.Target);
                                }).ToList();

                                // inject header id mapping if link column present
                                if (!string.IsNullOrWhiteSpace(lineLink))
                                {
                                    var idx = tmLine.Columns.FindIndex(c => string.Equals(c.Target, lineLink, StringComparison.OrdinalIgnoreCase));
                                    if (idx >= 0) lineSelectExprs[idx] = "hm.HeaderId AS " + Qi(lineLink);
                                    else { lineTargetCols.Insert(0, Qi(lineLink)); lineSelectExprs.Insert(0, "hm.HeaderId AS " + Qi(lineLink)); }
                                }

                                sb.AppendLine($"    INSERT INTO {Qi(tmLine.Target)} ({string.Join(", ", lineTargetCols)})");
                                sb.AppendLine("    SELECT " + string.Join(", ", lineSelectExprs));
                                sb.AppendLine("    FROM _src s JOIN #hdr_map hm ON hm.HeaderKey = (CASE WHEN (" + cond + ") THEN (" + trueExpr + ") ELSE (" + falseExpr + ") END);");
                            }
                            else
                            {
                                sb.AppendLine($"    PRINT N'WARN: line table mapping not found for {lineTable}; skipping lines insert';");
                            }

                            sb.AppendLine($"    PRINT N'OK normalize migrate for ' + N'" + tm.Target + "';");
                            sb.AppendLine("END TRY");
                            sb.AppendLine("BEGIN CATCH");
                            sb.AppendLine("    PRINT N'ERROR during normalize migrate ' + N'" + tm.Target + "': ' + ERROR_MESSAGE();");
                            sb.AppendLine("    IF OBJECT_ID('tempdb..#hdr_map') IS NOT NULL DROP TABLE #hdr_map;" );
                            sb.AppendLine("END CATCH");
                            sb.AppendLine("GO");
                            sb.AppendLine();
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"-- FAIL generating normalize SQL for {tm.Target}: {ex.Message}");
                            sb.AppendLine("GO"); sb.AppendLine();
                        }

                        continue; // skip default emit
                    }

                    if (isSelf)
                    {
                        // Only use AccessSrc; skip if missing to avoid self-select
                        sb.AppendLine($"IF OBJECT_ID({Qs(U2(SourceSchema, tm.Source))}) IS NULL");
                        sb.AppendLine("BEGIN");
                        sb.AppendLine($"    PRINT 'SKIP migrate [{tm.Target}]: missing source {accessSrcObj}';");
                        sb.AppendLine("END");
                        sb.AppendLine("ELSE");
                        sb.AppendLine("BEGIN");
                        EmitInsertBlock(sb, tm.Target, accessSrcObj, targetColsLocal, selectCols, useIdentityInsert, chosenIdentity);
                        sb.AppendLine("END");
                        sb.AppendLine("GO");
                        sb.AppendLine();
                    }
                    else
                    {
                        // Prefer AccessSrc; else fall back to unqualified source
                        sb.AppendLine($"IF OBJECT_ID({Qs(U2(SourceSchema, tm.Source))}) IS NOT NULL");
                        sb.AppendLine("BEGIN");
                        EmitInsertBlock(sb, tm.Target, accessSrcObj, targetColsLocal, selectCols, useIdentityInsert, chosenIdentity);
                        sb.AppendLine("END");
                        sb.AppendLine("ELSE");
                        sb.AppendLine("BEGIN");
                        EmitInsertBlock(sb, tm.Target, localSrcObj, targetColsLocal, selectCols, useIdentityInsert, chosenIdentity);
                        sb.AppendLine("END");
                        sb.AppendLine("GO");
                        sb.AppendLine();
                    }
                }

                // Verification
                sb.AppendLine("PRINT 'Identities present:';");
                sb.AppendLine("SELECT t.name AS TableName, c.name AS IdentityColumn");
                sb.AppendLine("FROM sys.identity_columns ic");
                sb.AppendLine("JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id");
                sb.AppendLine("JOIN sys.tables t ON t.object_id=c.object_id;");
                sb.AppendLine("GO");
                sb.AppendLine("SELECT COUNT(*) AS ForeignKeysPresent FROM sys.foreign_keys;");
                sb.AppendLine("GO");

                // Re-enable FKs (try WITH CHECK; on failure, enable without checking and warn)
                targetNames = order.Select(n => maps[n].Target).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                inList = string.Join(", ", targetNames.Select(n => "N'" + (n ?? "").Replace("'", "''") + "'"));

                sb.AppendLine("-- Re-enable foreign keys on migrated targets");
                sb.AppendLine("DECLARE @tbl2 sysname, @fk2 sysname, @sql2 nvarchar(max);");
                sb.AppendLine("DECLARE fk_cur2 CURSOR LOCAL FAST_FORWARD FOR");
                sb.AppendLine("SELECT QUOTENAME(SCHEMA_NAME(t.schema_id))+'.'+QUOTENAME(t.name), QUOTENAME(fk.name)");
                sb.AppendLine("FROM sys.foreign_keys fk");
                sb.AppendLine("JOIN sys.tables t ON t.object_id=fk.parent_object_id");
                sb.AppendLine($"WHERE t.name IN ({inList});");
                sb.AppendLine("OPEN fk_cur2;");
                sb.AppendLine("FETCH NEXT FROM fk_cur2 INTO @tbl2, @fk2;");
                sb.AppendLine("WHILE @@FETCH_STATUS = 0");
                sb.AppendLine("BEGIN");
                sb.AppendLine("    BEGIN TRY");
                sb.AppendLine("        SET @sql2 = N'ALTER TABLE ' + @tbl2 + N' WITH CHECK CHECK CONSTRAINT ' + @fk2 + N';';");
                sb.AppendLine("        PRINT @sql2; EXEC sp_executesql @sql2;");
                sb.AppendLine("    END TRY");
                sb.AppendLine("    BEGIN CATCH");
                sb.AppendLine("        PRINT 'WARN: could not CHECK ' + @fk2 + ' on ' + @tbl2 + ': ' + ERROR_MESSAGE();");
                sb.AppendLine("        SET @sql2 = N'ALTER TABLE ' + @tbl2 + N' WITH NOCHECK CHECK CONSTRAINT ' + @fk2 + N';';");
                sb.AppendLine("        PRINT @sql2; EXEC sp_executesql @sql2;");
                sb.AppendLine("    END CATCH");
                sb.AppendLine("    FETCH NEXT FROM fk_cur2 INTO @tbl2, @fk2;");
                sb.AppendLine("END");
                sb.AppendLine("CLOSE fk_cur2; DEALLOCATE fk_cur2;");
                sb.AppendLine("GO");
                sb.AppendLine();

                // Orphan check: report rows in child tables that reference missing parents for any FK not trusted/enabled
                sb.AppendLine("-- Orphan check for foreign keys that could not be fully trusted after reload");
                sb.AppendLine("DECLARE @ps sysname, @pt sysname, @rs sysname, @rt sysname, @fkn sysname, @fkId int, @sql nvarchar(max), @pred nvarchar(max);");
                sb.AppendLine("DECLARE fk_orphans CURSOR LOCAL FAST_FORWARD FOR");
                sb.AppendLine("SELECT SCHEMA_NAME(tp.schema_id), tp.name, SCHEMA_NAME(tr.schema_id), tr.name, fk.name, fk.object_id");
                sb.AppendLine("FROM sys.foreign_keys fk");
                sb.AppendLine("JOIN sys.tables tp ON tp.object_id = fk.parent_object_id");
                sb.AppendLine("JOIN sys.tables tr ON tr.object_id = fk.referenced_object_id");
                sb.AppendLine($"WHERE tp.name IN ({inList}) AND (fk.is_not_trusted = 1 OR fk.is_disabled = 1);");
                sb.AppendLine("OPEN fk_orphans;");
                sb.AppendLine("FETCH NEXT FROM fk_orphans INTO @ps, @pt, @rs, @rt, @fkn, @fkId;");
                sb.AppendLine("WHILE @@FETCH_STATUS = 0");
                sb.AppendLine("BEGIN");
                sb.AppendLine("    -- Build multi-column join predicate for this FK");
                sb.AppendLine("    SELECT @pred = STUFF((");
                sb.AppendLine("        SELECT ' AND t.' + QUOTENAME(pc.name) + ' = p.' + QUOTENAME(rc.name)");
                sb.AppendLine("        FROM sys.foreign_key_columns fkc");
                sb.AppendLine("        JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id");
                sb.AppendLine("        JOIN sys.columns rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id");
                sb.AppendLine("        WHERE fkc.constraint_object_id = @fkId");
                sb.AppendLine("        ORDER BY fkc.constraint_column_id");
                sb.AppendLine("        FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 5, '');");
                sb.AppendLine("    DECLARE @pt2 nvarchar(300) = QUOTENAME(@ps) + N'.' + QUOTENAME(@pt);");
                sb.AppendLine("    DECLARE @rt2 nvarchar(300) = QUOTENAME(@rs) + N'.' + QUOTENAME(@rt);");
                sb.AppendLine("    SET @sql = N'PRINT ''ORPHAN CHECK [' + REPLACE(@fkn, '''', '''''') + N'] on ' + @pt2 + N' -> ' + @rt2 + N'''; ' +");
                sb.AppendLine("              N'SELECT COUNT(*) AS OrphanCount FROM ' + @pt2 + N' t WHERE NOT EXISTS (SELECT 1 FROM ' + @rt2 + N' p WHERE ' + @pred + N'); ' +");
                sb.AppendLine("              N'SELECT TOP 5 t.* FROM ' + @pt2 + N' t WHERE NOT EXISTS (SELECT 1 FROM ' + @rt2 + N' p WHERE ' + @pred + N') ORDER BY NEWID();';");
                sb.AppendLine("    EXEC sp_executesql @sql;");
                sb.AppendLine("    FETCH NEXT FROM fk_orphans INTO @ps, @pt, @rs, @rt, @fkn, @fkId;");
                sb.AppendLine("END");
                sb.AppendLine("CLOSE fk_orphans; DEALLOCATE fk_orphans;");      
                sb.AppendLine("GO");
                sb.AppendLine();

                // Save
                SaveAlsoAsLatest(migrationsDir, "DataMigration_LATEST.sql", sb.ToString());
                File.WriteAllText(sqlPath, sb.ToString(), Encoding.UTF8);
                return 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(sqlPath, "-- ERROR: " + ex, Encoding.UTF8);
                return 1;
            }
        }

        // Identifier helpers
        private static string Qi(string name) // QUOTENAME for identifiers: [Name] with escaping
        {
            var n = (name ?? "").Replace("]", "]]");
            return "[" + n + "]";
        }
        private static string Q2(string schema, string name) => Qi(schema) + "." + Qi(name);
        private static string U2(string schema, string name) => (schema ?? "dbo") + "." + (name ?? "");
        private static string Unq(string twoPartOrOnePart) => twoPartOrOnePart; // emit plain (no brackets) for OBJECT_ID/DBCC literals
        private static string Qs(string s) => "N'" + (s ?? "").Replace("'", "''") + "'";

        // ---- model building ----

        private sealed class ColMap { public string Source; public string Target; }

        private sealed class TableMap
        {
            public string Source;
            public string Target;
            public bool IsHeader;
            public bool IsLine;
            public List<ColMap> Columns { get; } = new List<ColMap>();
            public HashSet<string> DependsOn { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, TableMap> BuildMappings(string accessSchemaDir)
        {
            var maps = new Dictionary<string, TableMap>();
            
            if (!Directory.Exists(accessSchemaDir))
            {
                Console.WriteLine($"WARNING: AccessSchema directory not found: {accessSchemaDir}");
                return maps;
            }

            foreach (string schemaFile in Directory.GetFiles(accessSchemaDir, "*.schema.json"))
            {
                try
                {
                    var json = File.ReadAllText(schemaFile);
                    dynamic schema = JsonConvert.DeserializeObject(json);
                    
                    string sourceTable = schema?.SourceTable;
                    string targetTable = schema?.Plan?.TargetTable;
                    bool ignore = schema?.Plan?.Ignore ?? false;
                    
                    if (string.IsNullOrEmpty(sourceTable) || string.IsNullOrEmpty(targetTable))
                        continue;

                    // ? NEW: Skip tables marked for normalization or custom migration
                    if (ignore)
                    {
                        Console.WriteLine($"SKIPPING table {sourceTable} -> {targetTable} (marked as Ignore=true in schema)");
                        continue;
                    }

                    // Check if this table is handled by custom normalization
                    bool hasNormalize = schema?.Plan?.Normalize != null;
                    if (hasNormalize)
                    {
                        bool isHeader = schema?.Plan?.Normalize?.IsHeader ?? false;
                        bool isLine = schema?.Plan?.Normalize?.IsLine ?? false;
                        
                        if (isHeader || isLine)
                        {
                            Console.WriteLine($"SKIPPING table {sourceTable} -> {targetTable} (marked for normalization: IsHeader={isHeader}, IsLine={isLine})");
                            continue;
                        }
                    }

                    var tm = GetOrAddMap(maps, targetTable, sourceTable);

                    // Process column mappings (skip columns marked as "Drop")
                    if (schema?.Plan?.ColumnActions != null)
                    {
                        foreach (var colAction in schema.Plan.ColumnActions)
                        {
                            string srcCol = colAction?.Source;
                            string tarCol = colAction?.Target;
                            string action = colAction?.Action;
                            
                            // CRITICAL FIX: Skip columns marked as "Drop"
                            if (!string.IsNullOrEmpty(action) && 
                                string.Equals(action, "Drop", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"  SKIPPING dropped column: {srcCol} (Action: {action})");
                                continue;
                            }
                            
                            if (!string.IsNullOrEmpty(srcCol) && !string.IsNullOrEmpty(tarCol))
                            {
                                AddColMap(tm, srcCol, tarCol);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Failed to parse schema file {schemaFile}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"Built {maps.Count} table mappings for regular data migration");
            foreach (var kv in maps)
            {
                Console.WriteLine($"  {kv.Value.Source} -> {kv.Key} ({kv.Value.Columns.Count} columns)");
            }
            
            return maps;
        }

        private static TableMap GetOrAddMap(Dictionary<string, TableMap> dict, string target, string source)
        {
            if (!dict.TryGetValue(target, out var tm))
            {
                tm = new TableMap { Target = target, Source = source };
                dict[target] = tm;
            }
            return tm;
        }

        private static void AddColMap(TableMap tm, string src, string tar)
        {
            if (!tm.Columns.Any(c => c.Target.Equals(tar, StringComparison.OrdinalIgnoreCase)))
                tm.Columns.Add(new ColMap { Source = src, Target = tar });
        }

        private static bool IsPlaceholderColName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            var n = name.Trim();
            if (n.Equals("--/--", StringComparison.OrdinalIgnoreCase)) return true;
            if (n.Equals("Rows:", StringComparison.OrdinalIgnoreCase)) return true;
            if (n.Equals("n/a", StringComparison.OrdinalIgnoreCase)) return true;
            if (n.StartsWith("--", StringComparison.Ordinal)) return true;
            if (n.Equals("Copy", StringComparison.OrdinalIgnoreCase)) return true; // stray token from CSV/action columns
            return false;
        }

        // ---- dependency ordering ----

        private static List<string> TopoOrder(Dictionary<string, TableMap> maps, ConstraintsIndex constraints)
        {
            var deps = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in maps) deps[kv.Key] = new HashSet<string>(kv.Value.DependsOn, StringComparer.OrdinalIgnoreCase);

            foreach (var ct in constraints.Tables ?? new List<ConstraintTable>())
            {
                var tbl = ct.Table ?? "";
                if (!maps.ContainsKey(tbl)) continue;
                foreach (var fk in ct.ForeignKeys ?? new List<ForeignKeyDef>())
                {
                    if (string.IsNullOrWhiteSpace(fk?.RefTable)) continue;
                    // Use StringComparison here (NOT StringComparer)
                    if (maps.ContainsKey(fk.RefTable) && !string.Equals(fk.RefTable, tbl, StringComparison.OrdinalIgnoreCase))
                        deps[tbl].Add(fk.RefTable);
                }
            }

            var indeg = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var k in maps.Keys) indeg[k] = 0;
            foreach (var d in deps) foreach (var p in d.Value) indeg[d.Key]++;

            var q = new Queue<string>(indeg.Where(kv => kv.Value == 0).Select(kv => kv.Key).OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
            var ordered = new List<string>();
            while (q.Count > 0)
            {
                var v = q.Dequeue();
                ordered.Add(v);
                foreach (var u in deps.Where(kv => kv.Value.Contains(v)).Select(kv => kv.Key).ToList())
                {
                    indeg[u]--;
                    deps[u].Remove(v);
                    if (indeg[u] == 0) q.Enqueue(u);
                }
            }

            foreach (var k in maps.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                if (!ordered.Contains(k)) ordered.Add(k);

            return ordered;
        }

        // ---- helpers ----

        private static string ResolveAccessSchemaDir(string migrationsDir)
        {
            var primary = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
            var fallback = Path.Combine(migrationsDir, "Migrations", "Metadata", "AccessSchema");
            return Directory.Exists(primary) ? primary : Directory.Exists(fallback) ? fallback : primary;
        }

        private static string ResolveConstraintsPath(string migrationsDir)
        {
            var primary = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "PlanConstraints.json");
            var fallback = Path.Combine(migrationsDir, "Migrations", "Metadata", "PlanEdits", "PlanConstraints.json");
            return File.Exists(primary) ? primary : File.Exists(fallback) ? fallback : primary;
        }

        private static ConstraintTable FindConstraints(ConstraintsIndex constraints, string table)
        {
            foreach (var ct in constraints.Tables ?? new List<ConstraintTable>())
                if (string.Equals(ct.Table, table, StringComparison.OrdinalIgnoreCase))
                    return ct;
            return null;
        }

        private static string ChooseIdentity(ConstraintTable ct, TableMap tm)
        {
            if (ct == null || ct.IdentityColumns == null || ct.IdentityColumns.Count == 0) return null;

            var candidates = (ct.IdentityColumns ?? new List<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Prefer PK single-column identity
            var pk1 = (ct.PrimaryKey ?? new List<string>()).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(pk1) && candidates.Any(c => c.Equals(pk1, StringComparison.OrdinalIgnoreCase)))
                return pk1;

            return candidates.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        }

        private static bool IsTempTable(string table)
        {
            var t = (table ?? "").Trim();
            return t.StartsWith("Temp", StringComparison.OrdinalIgnoreCase) ||
                   t.StartsWith("tmp", StringComparison.OrdinalIgnoreCase);
        }

        private static void SaveAlsoAsLatest(string migrationsDir, string fileName, string content)
        {
            try
            {
                var dir = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Sql");
                Directory.CreateDirectory(dir);
                var latest = Path.Combine(dir, fileName);
                File.WriteAllText(latest, content, Encoding.UTF8);
            }
            catch { }
        }

        private static string WrapNullIf(string idExpr)
        {
            // idExpr is already escaped (e.g., [Col]); convert empty strings to NULL
            return $"NULLIF({idExpr}, N'')";
        }

        private static string ResolveTargetType(ConstraintTable ct, string targetCol)
        {
            if (ct?.ColumnTypes == null || string.IsNullOrWhiteSpace(targetCol)) return null;
            foreach (var kv in ct.ColumnTypes)
                if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Key.Equals(targetCol, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            return null;
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
            if (n.EndsWith("id")) return false; // don't infer dates for *ID columns
            return n.Contains("date") || n.Contains("time");
        }

        private static string TryConvertDate(string exprNvarchar)
        {
            // exprNvarchar is an NVARCHAR expression already wrapped with NULLIF(...)
            // Try to handle more date formats and NULL/empty values gracefully
            // Styles: 127 = ISO8601 with Z, 126 = ISO8601 T, 121 = ODBC canonical, 103 = dd/MM/yyyy, 101 = MM/dd/yyyy
            return $"CASE WHEN {exprNvarchar} IS NULL OR LEN(LTRIM(RTRIM({exprNvarchar}))) = 0 THEN NULL " +
                   $"ELSE COALESCE(" +
                   $"TRY_CONVERT(datetime2(7), {exprNvarchar}, 127), " +
                   $"TRY_CONVERT(datetime2(7), {exprNvarchar}, 126), " +
                   $"TRY_CONVERT(datetime2(7), {exprNvarchar}, 121), " +
                   $"TRY_CONVERT(datetime2(7), {exprNvarchar}, 103), " +
                   $"TRY_CONVERT(datetime2(7), {exprNvarchar}, 101), " +
                   $"TRY_CONVERT(datetime2(7), {exprNvarchar})) END";
        }

        private static bool IsBoolLike(string sqlType, string colName)
        {
            if (!string.IsNullOrWhiteSpace(sqlType) && sqlType.IndexOf("bit", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            // common boolean targets by naming
            if (string.IsNullOrWhiteSpace(colName)) return false;
            var n = colName;
            return n.StartsWith("Is", StringComparison.OrdinalIgnoreCase) ||
                   n.StartsWith("Has", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("Enabled", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("Done", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("UsesFilter", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("AutoFulfill", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("TypicallySecToo", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("ReminderSent", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("SendDeliveryConfirmation", StringComparison.OrdinalIgnoreCase);
        }

        private static string TryConvertBit(string exprNvarchar)
        {
            // Normalize common truthy/falsey strings, fallback to TRY_CONVERT(bit, ...)
            return $"CASE " +
                   $"WHEN {exprNvarchar} IS NULL THEN NULL " +
                   $"WHEN {exprNvarchar} IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 " +
                   $"WHEN {exprNvarchar} IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 " +
                   $"ELSE TRY_CONVERT(bit, {exprNvarchar}) END";
        }
    }
}