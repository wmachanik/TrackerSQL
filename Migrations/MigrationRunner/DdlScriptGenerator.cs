using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal static class DdlScriptGenerator
    {
        public static int GenerateCreateTables(string migrationsDir, out string sqlPath)
        {
            return GenerateCreateTables(migrationsDir, suppressIdentity: false, dropExisting: false, out sqlPath);
        }

        public static int GenerateCreateTables(string migrationsDir, bool suppressIdentity, out string sqlPath)
        {
            return GenerateCreateTables(migrationsDir, suppressIdentity, dropExisting: false, out sqlPath);
        }

        // NEW: dropExisting to force recreation (keeps column order exact)
        public static int GenerateCreateTables(string migrationsDir, bool suppressIdentity, bool dropExisting, out string sqlPath)
        {
            sqlPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Sql",
                "CreateTables_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".sql");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sqlPath) ?? migrationsDir);

                var model = BuildTargetModel(migrationsDir, out var constraints, out var errors);
                var planScan = BuildPlanScanReport(migrationsDir, out var metaPaths);

                if (errors.Count > 0)
                {
                    var eb = new StringBuilder();
                    eb.AppendLine("-- DDL generation failed. See reasons below.");
                    foreach (var e in errors) eb.AppendLine("-- " + e);
                    File.WriteAllText(sqlPath, eb.ToString(), Encoding.UTF8);
                    return 2;
                }

                var tableCount = model.Count;
                var colCount = model.Values.Sum(t => t.Columns.Count);

                if (tableCount == 0 || colCount == 0)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine("-- No CREATE TABLE statements generated.");
                    msg.AppendLine("-- Reason: target model is empty (tables=" + tableCount + ", columns=" + colCount + ").");
                    msg.AppendLine("-- Metadata paths probed:");
                    msg.AppendLine("--   AccessSchema: " + metaPaths.AccessSchemaDir);
                    msg.AppendLine("--   PlanConstraints: " + metaPaths.ConstraintsPath);
                    msg.AppendLine("-- Plan scan (source -> decision):");
                    foreach (var line in planScan) msg.AppendLine("-- " + line);
                    msg.AppendLine("-- Next steps:");
                    msg.AppendLine("--  1) Run option 1 (Export Access schema) to populate AccessSchema/*.schema.json");
                    msg.AppendLine("--  2) Run option 11 (Import CSV) so PlanConstraints.json exists and matches");
                    msg.AppendLine("--  3) Run option 8 (Full plan review) and option 12 (Validate) to confirm plan/constraints");
                    File.WriteAllText(sqlPath, msg.ToString(), Encoding.UTF8);
                    return 1;
                }

                var sb = new StringBuilder();
                // Header with scan report
                sb.AppendLine("-- Auto-generated CREATE TABLE script");
                sb.AppendLine("-- Metadata paths:");
                sb.AppendLine("--   AccessSchema: " + metaPaths.AccessSchemaDir);
                sb.AppendLine("--   PlanConstraints: " + metaPaths.ConstraintsPath);
                sb.AppendLine("-- Tables: " + tableCount + ", Columns: " + colCount);
                sb.AppendLine("-- Identity suppressed: " + (suppressIdentity ? "Yes" : "No"));
                sb.AppendLine("-- Drop existing tables: " + (dropExisting ? "Yes" : "No"));
                sb.AppendLine("-- Plan scan (source -> decision):");
                foreach (var line in planScan) sb.AppendLine("-- " + line);
                sb.AppendLine("SET ANSI_NULLS ON;");
                sb.AppendLine("SET QUOTED_IDENTIFIER ON;");
                sb.AppendLine();

                foreach (var t in model.Values.OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var ct = FindConstraints(constraints, t.Name);
                    var pk = new List<string>(ct?.PrimaryKey ?? new List<string>());
                    var fkColsForTable = new HashSet<string>(
                        (ct?.ForeignKeys ?? new List<ForeignKeyDef>())
                            .Select(f => f?.Column ?? string.Empty),
                        StringComparer.OrdinalIgnoreCase);

                    // Resolve a single identity column:
                    string chosenIdentity = null;
                    if (!suppressIdentity && ct?.IdentityColumns != null && ct.IdentityColumns.Count > 0)
                    {
                        var candidates = ct.IdentityColumns
                            .Where(c => !string.IsNullOrWhiteSpace(c))
                            .Where(c => !fkColsForTable.Contains(c)) // avoid FKs as identity
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        if (candidates.Count == 0)
                            candidates = (ct.IdentityColumns ?? new List<string>()).ToList();

                        if (pk.Count == 1 && candidates.Any(c => c.Equals(pk[0], StringComparison.OrdinalIgnoreCase)))
                            chosenIdentity = pk[0];
                        else
                            chosenIdentity = candidates.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
                    }

                    // Optional DROP to ensure exact recreate (column order kept)
                    if (dropExisting)
                    {
                        // Drop any FKs that reference or are owned by this table to avoid 3726 errors
                        sb.AppendLine($"-- Drop FKs referencing or owned by [{t.Name}]");
                        sb.AppendLine("DECLARE @sql nvarchar(max) = N'';");
                        sb.AppendLine("SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)");
                        sb.AppendLine("FROM sys.foreign_keys fk");
                        sb.AppendLine("JOIN sys.objects o ON fk.parent_object_id = o.object_id");
                        sb.AppendLine($"WHERE fk.parent_object_id = OBJECT_ID(N'[{t.Name}]') OR fk.referenced_object_id = OBJECT_ID(N'[{t.Name}]');");
                        sb.AppendLine("IF LEN(@sql) > 0 EXEC sp_executesql @sql;");

                        sb.AppendLine($"IF OBJECT_ID(N'[{t.Name}]', N'U') IS NOT NULL DROP TABLE [{t.Name}];");
                        sb.AppendLine("GO");
                    }

                    if (!dropExisting)
                    {
                        sb.AppendLine($"IF OBJECT_ID(N'[{t.Name}]', N'U') IS NULL");
                        sb.AppendLine("BEGIN");
                    }

                    sb.AppendLine($"    CREATE TABLE [{t.Name}]");
                    sb.AppendLine("    (");

                    // when building col lines, after computing sqlType from source:
                    // apply type override from constraints (CSV "After Type")
                    var colLines = new List<string>();
                    foreach (var col in GetColumnsInOrder(t, chosenIdentity))
                    {
                        var rawType = t.Columns[col] ?? "NVARCHAR(255)";
                        var sqlType = NormalizeSqlType(rawType);

                        // apply CSV override if present
                        string typeOverride;
                        if (ct?.ColumnTypes != null && ct.ColumnTypes.TryGetValue(col, out typeOverride) && !string.IsNullOrWhiteSpace(typeOverride))
                            sqlType = NormalizeSqlType(typeOverride);

                        var isIdentity = !suppressIdentity &&
                                         !string.IsNullOrWhiteSpace(chosenIdentity) &&
                                         string.Equals(col, chosenIdentity, StringComparison.OrdinalIgnoreCase);

                        if (isIdentity && !IsIdentityCompatibleType(sqlType))
                        {
                            sqlType = "INT";
                            colLines.Add($"        [{col}] {sqlType} IDENTITY(1,1) NOT NULL /* coerced to INT for IDENTITY */");
                            continue;
                        }

                        // NOT NULL from: PK, Identity, CSV NotNullColumns, Access AllowDBNull=false
                        var mustNotNull =
                            pk.Contains(col) ||
                            (ct?.IdentityColumns?.Contains(col, StringComparer.OrdinalIgnoreCase) ?? false) ||
                            t.NotNull.Contains(col);

                        var nullSql = mustNotNull ? " NOT NULL" : " NULL";
                        colLines.Add($"        [{col}] {sqlType}{(isIdentity ? " IDENTITY(1,1)" : "")}{nullSql}");
                    }

                    // emit with cleaner PK formatting
                    if (pk.Count > 0)
                    {
                        var pkName = $"PK_{SafeName(t.Name)}";
                        var cols = string.Join(", ", pk.Select(c => $"[{c}]"));

                        // Join columns with commas, then add the PK constraint prefixed by a comma
                        sb.AppendLine(string.Join(",\r\n", colLines));
                        sb.AppendLine($"        , CONSTRAINT [{pkName}] PRIMARY KEY CLUSTERED ({cols})");
                    }
                    else
                    {
                        sb.AppendLine(string.Join(",\r\n", colLines));
                    }

                    sb.AppendLine("    );");
                    if (!dropExisting)
                    {
                        sb.AppendLine("END");
                    }
                    sb.AppendLine("GO");
                    sb.AppendLine();
                }

                File.WriteAllText(sqlPath, sb.ToString(), Encoding.UTF8);
                // also save a stable "latest" copy to avoid picking old files
                SaveAlsoAsLatest(migrationsDir, "CreateTables_LATEST.sql", sb.ToString());
                return 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(sqlPath, "-- ERROR: " + ex, Encoding.UTF8);
                return 1;
            }
        }

        public static int GenerateForeignKeys(string migrationsDir, out string sqlPath)
        {
            sqlPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Sql",
                "AddForeignKeys_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".sql");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sqlPath) ?? migrationsDir);

                var model = BuildTargetModel(migrationsDir, out var constraints, out var errors);
                if (errors.Count > 0) return WriteErrors(sqlPath, errors);

                // Determine fk totals but only consider tables present in the current target model
                var allFks = (constraints.Tables ?? new List<ConstraintTable>())
                    .SelectMany(ct => (ct.ForeignKeys ?? new List<ForeignKeyDef>()).Select(fk => Tuple.Create(ct.Table, fk)))
                    .Where(t => !string.IsNullOrWhiteSpace(t.Item1) && t.Item2 != null)
                    .ToList();

                int total = allFks.Count;
                int emitted = 0, skipped = 0;

                if (total == 0)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine("-- No FK constraints generated.");
                    msg.AppendLine("-- Reason: PlanConstraints.json has no foreign keys or target tables not found.");
                    msg.AppendLine("-- Check PlanEdits/PlanConstraints.json and run option 11 to re-import.");
                    File.WriteAllText(sqlPath, msg.ToString(), Encoding.UTF8);
                    return 1;
                }

                var sb = new StringBuilder();
                sb.AppendLine("-- Auto-generated FK constraints script");
                sb.AppendLine($"-- FKs (total={total})");
                sb.AppendLine("SET ANSI_NULLS ON;");
                sb.AppendLine("SET QUOTED_IDENTIFIER ON;");
                sb.AppendLine();

                var ctByName = new Dictionary<string, ConstraintTable>(StringComparer.OrdinalIgnoreCase);
                foreach (var ct in constraints.Tables ?? new List<ConstraintTable>())
                    if (!string.IsNullOrWhiteSpace(ct.Table)) ctByName[ct.Table] = ct;

                foreach (var kv in ctByName)
                {
                    var tbl = kv.Key;
                    var ct = kv.Value;

                    // Skip tables that are not part of the generated model (e.g., Normalize targets handled by '!')
                    if (!model.ContainsKey(tbl))
                    {
                        // All fks on this table will be skipped
                        skipped += (ct.ForeignKeys ?? new List<ForeignKeyDef>()).Count;
                        foreach (var fk in ct.ForeignKeys ?? new List<ForeignKeyDef>())
                        {
                            sb.AppendLine($"-- Skipped FK for table not in model: [{tbl}].[{fk.Column}] -> [{fk.RefTable}]");
                        }
                        continue;
                    }

                    foreach (var fk in ct.ForeignKeys ?? new List<ForeignKeyDef>())
                    {
                        if (string.IsNullOrWhiteSpace(fk?.Column) || string.IsNullOrWhiteSpace(fk?.RefTable))
                        {
                            skipped++; continue;
                        }

                        // Only emit FK if both parent and referenced tables exist in model
                        if (!model.ContainsKey(tbl) || !model.ContainsKey(fk.RefTable))
                        {
                            sb.AppendLine($"-- Skipped: referenced table not in model for [{tbl}].[{fk.Column}] -> [{fk.RefTable}]");
                            skipped++;
                            continue;
                        }

                        var fkName = $"FK_{SafeName(tbl)}_{SafeName(fk.Column)}";
                        var refCol = fk.RefColumn;
                        if (string.IsNullOrWhiteSpace(refCol))
                        {
                            var refCt = ctByName.ContainsKey(fk.RefTable) ? ctByName[fk.RefTable] : null;
                            refCol = refCt?.PrimaryKey?.FirstOrDefault();
                        }
                        if (string.IsNullOrWhiteSpace(refCol))
                        {
                            sb.AppendLine($"-- Skipped: could not infer referenced column for [{tbl}].[{fk.Column}] -> [{fk.RefTable}]");
                            skipped++;
                            continue;
                        }
                        emitted++;

                        sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'{fkName}' AND parent_object_id = OBJECT_ID(N'[{tbl}]'))");
                        sb.AppendLine("BEGIN");
                        sb.AppendLine($"    ALTER TABLE [{tbl}] WITH CHECK ADD CONSTRAINT [{fkName}] FOREIGN KEY([{fk.Column}]) REFERENCES [{fk.RefTable}]([{refCol}]);");
                        sb.AppendLine("END");
                        sb.AppendLine("GO");
                        sb.AppendLine();
                    }
                }

                sb.Insert(0, $"-- Emitted FKs: {emitted}, Skipped: {skipped}\r\n");
                File.WriteAllText(sqlPath, sb.ToString(), Encoding.UTF8);
                // also save a stable "latest" copy
                SaveAlsoAsLatest(migrationsDir, "AddForeignKeys_LATEST.sql", sb.ToString());
                return 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(sqlPath, "-- ERROR: " + ex, Encoding.UTF8);
                return 1;
            }
        }

        // ---- internal helpers ----

        private sealed class TargetTable
        {
            public string Name { get; set; }
            public Dictionary<string, string> Columns { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> NotNull { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // NEW: preserve source/CSV order (from Access ColumnSchema.Ordinal)
            public Dictionary<string, int> OrdinalByCol { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        private struct MetaPaths
        {
            public string AccessSchemaDir;
            public string ConstraintsPath;
        }

        // identity-compatible types: INT, BIGINT, SMALLINT, TINYINT, DECIMAL/NUMERIC with scale 0
        private static bool IsIdentityCompatibleType(string sqlType)
        {
            if (string.IsNullOrWhiteSpace(sqlType)) return false;
            var t = sqlType.Trim().ToUpperInvariant();

            if (t == "INT" || t == "BIGINT" || t == "SMALLINT" || t == "TINYINT") return true;

            if (t.StartsWith("DECIMAL(") || t.StartsWith("NUMERIC("))
            {
                var open = t.IndexOf('('); var close = t.IndexOf(')', open + 1);
                if (open > 0 && close > open)
                {
                    var inside = t.Substring(open + 1, close - open - 1);
                    var parts = inside.Split(',');
                    int scale;
                    if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out scale) && scale == 0) return true;
                }
            }
            return false;
        }

        private static bool IsPlaceholderColName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            var n = name.Trim();
            if (n.Equals("--/--", StringComparison.OrdinalIgnoreCase)) return true;
            if (n.Equals("Rows:", StringComparison.OrdinalIgnoreCase)) return true;
            if (n.StartsWith("--", StringComparison.Ordinal)) return true;
            return false;
        }

        private static void AddColumnWithOrder(TargetTable t, string col, string sqlType, int ordinal)
        {
            t.Columns[col] = sqlType;
            if (!t.OrdinalByCol.ContainsKey(col))
            {
                t.OrdinalByCol[col] = ordinal;
            }
            else
            {
                // Keep the smallest ordinal if seen multiple times
                t.OrdinalByCol[col] = Math.Min(t.OrdinalByCol[col], ordinal);
            }
        }

        private static IEnumerable<string> GetColumnsInOrder(TargetTable t, string chosenIdentity)
        {
            // Base order: by recorded ordinal, then by name for stability
            var ordered = t.OrdinalByCol
                .Where(kv => !IsPlaceholderColName(kv.Key))
                .OrderBy(kv => kv.Value)
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => kv.Key)
                .ToList();

            // Make sure any remaining columns (no recorded ordinal) are appended (stable by name)
            foreach (var k in t.Columns.Keys)
                if (!IsPlaceholderColName(k) && !ordered.Contains(k))
                    ordered.Add(k);

            // Move identity to the front if present
            if (!string.IsNullOrWhiteSpace(chosenIdentity))
            {
                var idx = ordered.FindIndex(c => string.Equals(c, chosenIdentity, StringComparison.OrdinalIgnoreCase));
                if (idx > 0)
                {
                    ordered.RemoveAt(idx);
                    ordered.Insert(0, chosenIdentity);
                }
            }
            return ordered;
        }

        private static Dictionary<string, TargetTable> BuildTargetModel(string migrationsDir, out ConstraintsIndex constraints, out List<string> errors)
        {
            errors = new List<string>();
            constraints = new ConstraintsIndex();

            var accessPrimary = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
            var accessFallback = Path.Combine(migrationsDir, "Migrations", "Metadata", "AccessSchema");
            var accessSchemaDir = Directory.Exists(accessPrimary) ? accessPrimary
                                  : Directory.Exists(accessFallback) ? accessFallback
                                  : accessPrimary;

            var consPrimary = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "PlanConstraints.json");
            var consFallback = Path.Combine(migrationsDir, "Migrations", "Metadata", "PlanEdits", "PlanConstraints.json");
            var constraintsPath = File.Exists(consPrimary) ? consPrimary
                                : File.Exists(consFallback) ? consFallback
                                : consPrimary;

            if (!Directory.Exists(accessSchemaDir))
            {
                errors.Add("Access schema folder not found: " + accessSchemaDir + (Directory.Exists(accessFallback) ? "" : " (also checked: " + accessFallback + ")"));
                return new Dictionary<string, TargetTable>();
            }
            if (File.Exists(constraintsPath))
            {
                try { constraints = JsonConvert.DeserializeObject<ConstraintsIndex>(File.ReadAllText(constraintsPath)) ?? new ConstraintsIndex(); }
                catch (Exception ex) { errors.Add("Failed to read constraints: " + ex.Message + " at " + constraintsPath); }
            }

            var target = new Dictionary<string, TargetTable>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.EnumerateFiles(accessSchemaDir, "*.schema.json")
                                          .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase)))
            {
                TableSchema s = null;
                try { s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(file)); }
                catch (Exception ex) { errors.Add("Parse error: " + file + " -> " + ex.Message); continue; }
                if (s == null) continue;

                var plan = s.Plan ?? new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                var actions = plan.ColumnActions ?? new List<ColumnPlan>();
                var classification = plan.Classification ?? "Copy";
                var isNormalize = string.Equals(classification, "Normalize", StringComparison.OrdinalIgnoreCase);
                string singleTarget = plan.TargetTable ?? s.SourceTable ?? "";

                // Map source -> type/nullability/ordinal
                var typeBySource = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var notNullSource = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var ordBySource = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in (s.Columns ?? new List<ColumnSchema>()))
                {
                    var tSql = !string.IsNullOrWhiteSpace(c.RecommendedSqlType) ? c.RecommendedSqlType
                               : MapDotNetToSql(c.DotNetType ?? "");
                    if (!string.IsNullOrWhiteSpace(c.SourceName))
                    {
                        typeBySource[c.SourceName] = tSql;
                        if (!c.AllowDBNull) notNullSource.Add(c.SourceName);
                        ordBySource[c.SourceName] = c.Ordinal;
                    }
                }

                if (plan.Ignore) continue;

                // For Normalize plans we will include header/line targets in the target model
                // so CREATE TABLE (option A) will produce the necessary table DDL. Data migration
                // (option M) still filters out IsHeader/IsLine maps so normalized tables are
                // populated only by the custom '!' path.
                if (isNormalize)
                {
                    // Do not add header/line targets to the target model so they are NOT included in CREATE/DATA scripts
                    // continue;
                }

                if (!isNormalize)
                {
                    if (string.IsNullOrWhiteSpace(singleTarget)) continue;
                    var tt = GetOrAdd(target, singleTarget);

                    // Preserve source order
                    foreach (var a in actions.OrderBy(x => ordBySource.TryGetValue(x.Source ?? "", out var o) ? o : int.MaxValue)
                                             .ThenBy(x => x.Source, StringComparer.OrdinalIgnoreCase))
                    {
                        var act = a.Action ?? "Copy";
                        if (string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase)) continue;
                        var src = a.Source ?? "";
                        var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;
                        if (string.IsNullOrWhiteSpace(tgt)) continue;

                        var sqlType = typeBySource.TryGetValue(src, out var t) ? t : "NVARCHAR(255)";
                        var ord = ordBySource.TryGetValue(src, out var oo) ? oo : int.MaxValue;
                        AddColumnWithOrder(tt, tgt, sqlType, ord);

                        if (notNullSource.Contains(src)) tt.NotNull.Add(tgt);
                    }

                    // Constraints -> NOT NULL (PK/Identity/CSV NotNull)
                    var ct = FindConstraints(constraints, singleTarget);
                    if (ct != null)
                    {
                        foreach (var cpk in (ct.PrimaryKey ?? new List<string>())) tt.NotNull.Add(cpk);
                        foreach (var cid in (ct.IdentityColumns ?? new List<string>())) tt.NotNull.Add(cid);
                        foreach (var cnn in (ct.NotNullColumns ?? new List<string>())) tt.NotNull.Add(cnn);

                        // NEW: ensure columns from constraints exist (handles synthetic/new keys)
                        EnsureConstraintColumnsExist(tt, ct);
                    }
                }
                else
                {
                    var n = plan.Normalize ?? new NormalizePlan();
                    var headerSet = new HashSet<string>(n.HeaderColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var lineSet = new HashSet<string>(n.LineColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

                    if (!string.IsNullOrWhiteSpace(n.HeaderTable))
                    {
                        var th = GetOrAdd(target, n.HeaderTable);
                        foreach (var a in actions
                            .Where(x => headerSet.Contains(x.Source ?? ""))
                            .OrderBy(x => ordBySource.TryGetValue(x.Source ?? "", out var o) ? o : int.MaxValue)
                            .ThenBy(x => x.Source, StringComparer.OrdinalIgnoreCase))
                        {
                            var act = a.Action ?? "Copy";
                            if (string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase)) continue;
                            var src = a.Source ?? "";
                            var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;
                            if (string.IsNullOrWhiteSpace(tgt)) continue;

                            var sqlType = typeBySource.TryGetValue(src, out var t) ? t : "NVARCHAR(255)";
                            var ord = ordBySource.TryGetValue(src, out var oo) ? oo : int.MaxValue;
                            AddColumnWithOrder(th, tgt, sqlType, ord);

                            if (notNullSource.Contains(src)) th.NotNull.Add(tgt);
                        }
                        if (!string.IsNullOrWhiteSpace(n.NewHeaderKeyName))
                            AddColumnWithOrder(th, n.NewHeaderKeyName, "INT", int.MinValue + 10);

                        var cth = FindConstraints(constraints, n.HeaderTable);
                        if (cth != null)
                        {
                            foreach (var cpk in (cth.PrimaryKey ?? new List<string>())) th.NotNull.Add(cpk);
                            foreach (var cid in (cth.IdentityColumns ?? new List<string>())) th.NotNull.Add(cid);
                            foreach (var cnn in (cth.NotNullColumns ?? new List<string>())) th.NotNull.Add(cnn);

                            // NEW: ensure columns from constraints exist
                            EnsureConstraintColumnsExist(th, cth);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(n.LineTable))
                    {
                        var tl = GetOrAdd(target, n.LineTable);
                        foreach (var a in actions
                            .Where(x => lineSet.Contains(x.Source ?? ""))
                            .OrderBy(x => ordBySource.TryGetValue(x.Source ?? "", out var o) ? o : int.MaxValue)
                            .ThenBy(x => x.Source, StringComparer.OrdinalIgnoreCase))
                        {
                            var act = a.Action ?? "Copy";
                            if (string.Equals(act, "Drop", StringComparison.OrdinalIgnoreCase)) continue;
                            var src = a.Source ?? "";
                            var tgt = string.IsNullOrWhiteSpace(a.Target) ? src : a.Target;
                            if (string.IsNullOrWhiteSpace(tgt)) continue;

                            var sqlType = typeBySource.TryGetValue(src, out var t) ? t : "NVARCHAR(255)";
                            var ord = ordBySource.TryGetValue(src, out var oo) ? oo : int.MaxValue;
                            AddColumnWithOrder(tl, tgt, sqlType, ord);

                            if (notNullSource.Contains(src)) tl.NotNull.Add(tgt);
                        }
                        if (!string.IsNullOrWhiteSpace(n.NewLineKeyName))
                            AddColumnWithOrder(tl, n.NewLineKeyName, "INT", int.MinValue + 10);
                        if (!string.IsNullOrWhiteSpace(n.LineLinkKeyName))
                            AddColumnWithOrder(tl, n.LineLinkKeyName, "INT", int.MinValue + 20);

                        var ctl = FindConstraints(constraints, n.LineTable);
                        if (ctl != null)
                        {
                            foreach (var cpk in (ctl.PrimaryKey ?? new List<string>())) tl.NotNull.Add(cpk);
                            foreach (var cid in (ctl.IdentityColumns ?? new List<string>())) tl.NotNull.Add(cid);
                            foreach (var cnn in (ctl.NotNullColumns ?? new List<string>())) tl.NotNull.Add(cnn);

                            // NEW: ensure columns from constraints exist
                            EnsureConstraintColumnsExist(tl, ctl);
                        }
                    }
                }
            }

            return target;
        }

        // Ensure that any columns referenced by constraints (PK/Identity/NotNull/FK columns)
        // exist in the target model; synthesize them with sensible defaults when absent.
        private static void EnsureConstraintColumnsExist(TargetTable t, ConstraintTable ct)
        {
            if (t == null || ct == null) return;

            var needed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in (ct.PrimaryKey ?? new List<string>())) if (!string.IsNullOrWhiteSpace(c)) needed.Add(c);
            foreach (var c in (ct.IdentityColumns ?? new List<string>())) if (!string.IsNullOrWhiteSpace(c)) needed.Add(c);
            foreach (var c in (ct.NotNullColumns ?? new List<string>())) if (!string.IsNullOrWhiteSpace(c)) needed.Add(c);
            foreach (var fk in (ct.ForeignKeys ?? new List<ForeignKeyDef>())) if (!string.IsNullOrWhiteSpace(fk?.Column)) needed.Add(fk.Column);
            // NEW: include every CSV-defined column (After Type)
            foreach (var k in (ct.ColumnTypes ?? new Dictionary<string,string>()).Keys)
                if (!string.IsNullOrWhiteSpace(k)) needed.Add(k);

            foreach (var col in needed)
            {
                if (!t.Columns.ContainsKey(col))
                {
                    string chosenType = null;
                    string typeOverride;
                    if (ct.ColumnTypes != null && ct.ColumnTypes.TryGetValue(col, out typeOverride) && !string.IsNullOrWhiteSpace(typeOverride))
                        chosenType = NormalizeSqlType(typeOverride);
                    else if ((ct.IdentityColumns ?? new List<string>()).Contains(col, StringComparer.OrdinalIgnoreCase) || col.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
                        chosenType = "INT";
                    else
                        chosenType = "NVARCHAR(255)";

                    AddColumnWithOrder(t, col, chosenType, int.MinValue + 100);
                }

        if ((ct.PrimaryKey ?? new List<string>()).Contains(col, StringComparer.OrdinalIgnoreCase) ||
            (ct.IdentityColumns ?? new List<string>()).Contains(col, StringComparer.OrdinalIgnoreCase) ||
            (ct.NotNullColumns ?? new List<string>()).Contains(col, StringComparer.OrdinalIgnoreCase))
        {
            t.NotNull.Add(col);
        }
    }
        }

        private static MetaPaths GetMetaPaths(string migrationsDir)
        {
            var accessPrimary = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
            var accessFallback = Path.Combine(migrationsDir, "Migrations", "Metadata", "AccessSchema");
            var accessSchemaDir = Directory.Exists(accessPrimary) ? accessPrimary
                                  : Directory.Exists(accessFallback) ? accessFallback
                                  : accessPrimary;

            var consPrimary = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "PlanConstraints.json");
            var consFallback = Path.Combine(migrationsDir, "Migrations", "Metadata", "PlanEdits", "PlanConstraints.json");
            var constraintsPath = File.Exists(consPrimary) ? consPrimary
                                : File.Exists(consFallback) ? consFallback
                                : consPrimary;

            return new MetaPaths { AccessSchemaDir = accessSchemaDir, ConstraintsPath = constraintsPath };
        }

        private static List<string> BuildPlanScanReport(string migrationsDir, out MetaPaths paths)
        {
            paths = GetMetaPaths(migrationsDir);
            var outLines = new List<string>();
            if (!Directory.Exists(paths.AccessSchemaDir))
            {
                outLines.Add("Access schema folder not found: " + paths.AccessSchemaDir);
                return outLines;
            }

            foreach (var file in Directory.EnumerateFiles(paths.AccessSchemaDir, "*.schema.json")
                                          .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                                          .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                TableSchema s = null;
                try { s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(file)); }
                catch (Exception ex) { outLines.Add("Parse error: " + Path.GetFileName(file) + " -> " + ex.Message); continue; }
                if (s == null) { outLines.Add("Null schema: " + Path.GetFileName(file)); continue; }

                var src = s.SourceTable ?? Path.GetFileNameWithoutExtension(file);
                var plan = s.Plan ?? new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                var cls = plan.Classification ?? "Copy";
                var ignore = plan.Ignore;

                if (ignore)
                {
                    outLines.Add($"[{src}] IGNORE (plan.Ignore=true)");
                    continue;
                }

                var acts = plan.ColumnActions ?? new List<ColumnPlan>();

                if (!string.Equals(cls, "Normalize", StringComparison.OrdinalIgnoreCase))
                {
                    var tgtTbl = plan.TargetTable ?? s.SourceTable ?? "";
                    var keptCols = acts.Count(a => !string.Equals((a.Action ?? "Copy"), "Drop", StringComparison.OrdinalIgnoreCase));
                    outLines.Add($"[{src}] Class=Copy Target={tgtTbl} EmittedCols={keptCols}");
                }
                else
                {
                    var n = plan.Normalize ?? new NormalizePlan();
                    var headerSet = new HashSet<string>(n.HeaderColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                    var lineSet = new HashSet<string>(n.LineColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

                    int keptHeader = acts.Count(a =>
                        headerSet.Contains(a.Source ?? "") &&
                        !string.Equals((a.Action ?? "Copy"), "Drop", StringComparison.OrdinalIgnoreCase));
                    int keptLine = acts.Count(a =>
                        lineSet.Contains(a.Source ?? "") &&
                        !string.Equals((a.Action ?? "Copy"), "Drop", StringComparison.OrdinalIgnoreCase));

                    var synth = new List<string>();
                    if (!string.IsNullOrWhiteSpace(n.NewHeaderKeyName)) synth.Add("NewHeaderKey=" + n.NewHeaderKeyName);
                    if (!string.IsNullOrWhiteSpace(n.NewLineKeyName)) synth.Add("NewLineKey=" + n.NewLineKeyName);
                    if (!string.IsNullOrWhiteSpace(n.LineLinkKeyName)) synth.Add("LinkFK=" + n.LineLinkKeyName);

                    outLines.Add($"[{src}] Class=Normalize Header={n.HeaderTable} Lines={n.LineTable} Emitted(H/L)={keptHeader}/{keptLine} {(synth.Count > 0 ? "Synth=[" + string.Join(", ", synth) + "]" : "")} ");
                }
            }

            return outLines;
        }

        private static ConstraintTable FindConstraints(ConstraintsIndex constraints, string table)
        {
            foreach (var ct in constraints.Tables ?? new List<ConstraintTable>())
                if (string.Equals(ct.Table, table, StringComparison.OrdinalIgnoreCase))
                    return ct;
            return null;
        }

        private static TargetTable GetOrAdd(Dictionary<string, TargetTable> map, string name)
        {
            if (!map.TryGetValue(name, out var v)) { v = new TargetTable { Name = name }; map[name] = v; }
            return v;
        }

        private static string MapDotNetToSql(string dotnet)
        {
            var t = (dotnet ?? "").Trim();
            if (t.Length == 0) return "NVARCHAR(255)";

            var upper = t.ToUpperInvariant();
            // If this already looks like a SQL type, pass it through (normalize NVARCHAR length separately)
            var sqlPrefixes = new[]
            {
                "INT","BIGINT","SMALLINT","TINYINT","BIT","DATETIME","DATE","TIME",
                "FLOAT","REAL","DECIMAL","NUMERIC","NVARCHAR","VARCHAR","NCHAR","CHAR",
                "UNIQUEIDENTIFIER","MONEY","SMALLMONEY","TEXT","NTEXT"
            };
            foreach (var p in sqlPrefixes)
            {
                if (upper.StartsWith(p))
                    return NormalizeSqlType(upper);
            }

            // Map common .NET type names
            var lower = t.ToLowerInvariant();
            switch (lower)
            {
                case "int16": return "SMALLINT";
                case "byte": return "TINYINT";
                case "int32":
                case "int": return "INT";
                case "int64":
                case "long": return "BIGINT";
                case "boolean":
                case "bool": return "BIT";
                case "datetime": return "DATETIME";
                case "single": return "REAL";     // Access ?Single? -> REAL
                case "double": return "FLOAT";
                case "decimal": return "DECIMAL(18,2)";
                case "guid": return "UNIQUEIDENTIFIER";
                case "string": return "NVARCHAR(255)";
                default:
                    // Fallback: treat unknown tokens as SQL (e.g., ?SMALLINT?, ?REAL?, etc.)
                    return NormalizeSqlType(upper);
            }
        }

        private static string NormalizeSqlType(string raw)
        {
            var s = (raw ?? "").Trim();
            if (s.StartsWith("NVARCHAR(", StringComparison.OrdinalIgnoreCase))
            {
                var open = s.IndexOf('('); var close = s.IndexOf(')', open + 1);
                if (open >= 0 && close > open)
                {
                    var inside = s.Substring(open + 1, close - open - 1).Trim();
                    if (int.TryParse(inside, out var n) && n > 4000) return "NVARCHAR(MAX)";
                }
            }
            return s.Length == 0 ? "NVARCHAR(255)" : s;
        }

        private static string SafeName(string n)
        {
            var s = (n ?? "").Trim();
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_') sb.Append(ch);
                else sb.Append('_');
            }
            return sb.ToString();
        }

        private static int WriteErrors(string path, List<string> errors)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
                var sb = new StringBuilder();
                sb.AppendLine("-- DDL generation issues:");
                foreach (var e in errors) sb.AppendLine("-- " + e);
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            }
            catch { }
            return 2;
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
    }
}