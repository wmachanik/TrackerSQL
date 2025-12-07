using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using Newtonsoft.Json;

namespace MigrationRunner
{
    class AccessSchemaExporter
    {
        public static int Export(MigrationConfig config, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            var tables = ResolveAccessTables(config);
            var manifest = new SchemaManifest { GeneratedUtc = DateTime.UtcNow, Tables = new List<string>() };

            using (var conn = new OleDbConnection(config.AccessConnectionString))
            {
                conn.Open();

                foreach (var table in tables)
                {
                    var schema = ReadTableSchema(conn, table);
                    var path = Path.Combine(outputDir, table + ".schema.json");
                    File.WriteAllText(path, JsonConvert.SerializeObject(schema, Formatting.Indented));
                    manifest.Tables.Add(table);
                    Console.WriteLine("Exported schema: " + table);
                }
            }

            File.WriteAllText(Path.Combine(outputDir, "index.json"),
                JsonConvert.SerializeObject(manifest, Formatting.Indented));

            Console.WriteLine("Export complete. Files in: " + outputDir);
            return 0;
        }

        private static TableSchema ReadTableSchema(OleDbConnection conn, string table)
        {
            var schema = new TableSchema
            {
                SourceTable = table,
                Plan = new TablePlan
                {
                    Classification = "Copy",
                    TargetTable = table,
                    PreserveIdsOnInsert = false,
                    ColumnActions = new List<ColumnPlan>()
                },
                Columns = new List<ColumnSchema>(),
                PrimaryKey = new List<string>(),
                Indexes = new List<IndexSchema>()
            };

            using (var cmd = new OleDbCommand("SELECT * FROM [" + table + "] WHERE 1=0", conn))
            using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
            {
                var dt = reader.GetSchemaTable();
                if (dt == null) throw new InvalidOperationException("Unable to read schema for " + table);

                foreach (DataRow row in dt.Rows)
                {
                    var col = new ColumnSchema();
                    col.Ordinal = SafeGet<int>(row, "ColumnOrdinal");
                    col.SourceName = SafeGet<string>(row, "ColumnName");
                    col.DotNetType = ((Type)row["DataType"]).FullName;
                    col.ColumnSize = SafeGet<int?>(row, "ColumnSize");
                    col.NumericPrecision = SafeGet<short?>(row, "NumericPrecision");
                    col.NumericScale = SafeGet<short?>(row, "NumericScale");
                    col.AllowDBNull = SafeGet<bool>(row, "AllowDBNull");
                    col.IsAutoIncrement = SafeGet<bool>(row, "IsAutoIncrement");
                    col.IsKey = SafeGet<bool>(row, "IsKey");
                    col.RecommendedSqlType = RecommendSqlType(col);

                    schema.Columns.Add(col);
                    schema.Plan.ColumnActions.Add(new ColumnPlan
                    {
                        Source = col.SourceName,
                        Target = col.SourceName,
                        Action = "Copy"
                    });

                    if (col.IsKey)
                        schema.PrimaryKey.Add(col.SourceName);
                }
            }

            try
            {
                var restr = new object[] { null, null, null, null, table };
                var idx = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, restr);
                var byIndex = new Dictionary<string, IndexSchema>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow row in idx.Rows)
                {
                    var idxName = row["INDEX_NAME"] == DBNull.Value ? null : row["INDEX_NAME"].ToString();
                    var colName = row["COLUMN_NAME"] == DBNull.Value ? null : row["COLUMN_NAME"].ToString();
                    if (string.IsNullOrEmpty(idxName) || string.IsNullOrEmpty(colName)) continue;

                    IndexSchema ix;
                    if (!byIndex.TryGetValue(idxName, out ix))
                    {
                        ix = new IndexSchema
                        {
                            Name = idxName,
                            Columns = new List<string>(),
                            Unique = SafeGet<bool?>(row, "UNIQUE") ?? false,
                            PrimaryKey = SafeGet<bool?>(row, "PRIMARY_KEY") ?? false
                        };
                        byIndex[idxName] = ix;
                    }
                    ix.Columns.Add(colName);
                }
                schema.Indexes.AddRange(byIndex.Values);
            }
            catch
            {
                // Some providers lack full index metadata; ignore.
            }

            if (schema.PrimaryKey.Count == 1)
            {
                var pkName = schema.PrimaryKey[0];
                var pkCol = schema.Columns.Find(c => string.Equals(c.SourceName, pkName, StringComparison.OrdinalIgnoreCase));
                schema.Plan.PreserveIdsOnInsert = pkCol != null && (pkCol.IsAutoIncrement || string.Equals(pkCol.RecommendedSqlType, "INT", StringComparison.OrdinalIgnoreCase));
            }

            return schema;
        }

        private static T SafeGet<T>(DataRow row, string name)
        {
            if (!row.Table.Columns.Contains(name) || row[name] == DBNull.Value) return default(T);
            return (T)row[name];
        }

        private static string RecommendSqlType(ColumnSchema col)
        {
            var t = col.DotNetType;
            if (t == typeof(int).FullName) return "INT";
            if (t == typeof(long).FullName) return "BIGINT";
            if (t == typeof(short).FullName) return "SMALLINT";
            if (t == typeof(byte).FullName) return "TINYINT";
            if (t == typeof(bool).FullName) return "BIT";
            if (t == typeof(DateTime).FullName) return "DATETIME";
            if (t == typeof(decimal).FullName)
            {
                var prec = col.NumericPrecision.HasValue ? col.NumericPrecision.Value : (short)18;
                var scale = col.NumericScale.HasValue ? col.NumericScale.Value : (short)4;
                return "DECIMAL(" + prec + "," + scale + ")";
            }
            if (t == typeof(double).FullName) return "FLOAT";
            if (t == typeof(float).FullName) return "REAL";
            if (t == typeof(byte[]).FullName)
            {
                if (!col.ColumnSize.HasValue || col.ColumnSize.Value < 0) return "VARBINARY(MAX)";
                return "VARBINARY(" + col.ColumnSize.Value + ")";
            }
            var size = (!col.ColumnSize.HasValue || col.ColumnSize.Value < 0) ? "MAX" : col.ColumnSize.Value.ToString();
            return "NVARCHAR(" + size + ")";
        }

        private static List<string> ResolveAccessTables(MigrationConfig config)
        {
            if (config.AccessTables == null || config.AccessTables.Length == 0)
                throw new InvalidOperationException("AccessTables is empty. Use [\"*\"] or list specific tables.");

            if (config.AccessTables.Length == 1 && config.AccessTables[0] == "*")
            {
                using (var conn = new OleDbConnection(config.AccessConnectionString))
                {
                    conn.Open();
                    var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    var all = new List<string>();
                    foreach (DataRow row in schema.Rows)
                    {
                        var type = row["TABLE_TYPE"] == null ? null : row["TABLE_TYPE"].ToString();
                        if (!string.Equals(type, "TABLE", StringComparison.OrdinalIgnoreCase)) continue;
                        var name = row["TABLE_NAME"] == null ? "" : row["TABLE_NAME"].ToString();
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        if (name.StartsWith("MSys", StringComparison.OrdinalIgnoreCase)) continue;
                        all.Add(name);
                    }
                    var excludes = new HashSet<string>((config.AccessTableExcludes ?? new string[0]), StringComparer.OrdinalIgnoreCase);
                    var result = new List<string>();
                    foreach (var t in all)
                        if (!excludes.Contains(t)) result.Add(t);
                    result.Sort(StringComparer.OrdinalIgnoreCase);
                    return result;
                }
            }
            return new List<string>(config.AccessTables);
        }
    }

    class SchemaManifest
    {
        public DateTime GeneratedUtc { get; set; }
        public List<string> Tables { get; set; }
    }

    class TableSchema
    {
        public string SourceTable { get; set; }
        public List<ColumnSchema> Columns { get; set; }
        public List<string> PrimaryKey { get; set; }
        public List<IndexSchema> Indexes { get; set; }
        public TablePlan Plan { get; set; }
    }

    class ColumnSchema
    {
        public int Ordinal { get; set; }
        public string SourceName { get; set; }
        public string DotNetType { get; set; }
        public int? ColumnSize { get; set; }
        public short? NumericPrecision { get; set; }
        public short? NumericScale { get; set; }
        public bool AllowDBNull { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsKey { get; set; }
        public string RecommendedSqlType { get; set; }
    }

    class IndexSchema
    {
        public string Name { get; set; }
        public List<string> Columns { get; set; }
        public bool Unique { get; set; }
        public bool PrimaryKey { get; set; }
    }

    class TablePlan
    {
        public string Classification { get; set; }     // Copy | Rename | Refactor | Normalize
        public string TargetTable { get; set; }        // Default = SourceTable
        public bool PreserveIdsOnInsert { get; set; }  // For IDENTITY_INSERT later
        public bool Reviewed { get; set; }             // Human has reviewed this table's plan
        public bool Ignore { get; set; }               // Do not migrate this table
        public List<ColumnPlan> ColumnActions { get; set; }
        public NormalizePlan Normalize { get; set; }
    }

    class ColumnPlan
    {
        public string Source { get; set; }
        public string Target { get; set; }   // Per-column rename
        public string Action { get; set; }   // Copy | Rename | Drop | Compute
        public string Expression { get; set; } // For Compute
    }

    class NormalizePlan
    {
        public string HeaderTable { get; set; }              // e.g., OrderHeaderTbl
        public string LineTable { get; set; }                // e.g., OrderLinesTbl

        // Column assignment (source column names)
        public List<string> HeaderColumns { get; set; }      // which source columns go to header
        public List<string> LineColumns { get; set; }        // which source columns go to lines

        // Old composite key from source and new keys for target (ordered)
        public List<string> OldCompositeKey { get; set; }    // e.g., CustomerId, DeliveryDate, [optional Notes]
        public string NewHeaderKeyName { get; set; }         // e.g., OrderId
        public string NewLineKeyName { get; set; }           // e.g., OrderLineId
        public string LineLinkKeyName { get; set; }          // FK in lines referencing header key (e.g., OrderId)

        public List<string> HeaderPrimaryKey { get; set; }   // ordered PK columns for header (often [NewHeaderKeyName])
        public List<string> LinePrimaryKey { get; set; }     // ordered PK columns for lines (often [NewLineKeyName])

        public bool PreserveHeaderIds { get; set; }          // if you intend to reuse old IDs (rare with normalization)
        public bool PreserveLineIds { get; set; }            // likewise for lines

        public NormalizePlan()
        {
            HeaderColumns = new List<string>();
            LineColumns = new List<string>();
            OldCompositeKey = new List<string>();
            HeaderPrimaryKey = new List<string>();
            LinePrimaryKey = new List<string>();
        }
    }
}