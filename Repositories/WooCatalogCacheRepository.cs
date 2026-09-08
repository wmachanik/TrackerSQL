using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooCatalogCacheRepository : RepositoryBase<WooCatalogCacheRow>
    {
        protected override string TableName => "WooCatalogCacheTbl";
        protected override string KeyColumn => "CacheID";

        private static readonly object TableExistsLock = new object();
        private static bool? _tableExistsCached;

        public bool TableExists()
        {
            if (_tableExistsCached.HasValue)
                return _tableExistsCached.Value;

            lock (TableExistsLock)
            {
                if (_tableExistsCached.HasValue)
                    return _tableExistsCached.Value;
                try
                {
                    using (var db = CreateDb())
                    {
                        int n = db.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM sys.tables WHERE name = N'WooCatalogCacheTbl'");
                        _tableExistsCached = n > 0;
                    }
                }
                catch
                {
                    _tableExistsCached = false;
                }
                return _tableExistsCached.Value;
            }
        }

        public static void InvalidateTableExistsCache()
        {
            lock (TableExistsLock)
                _tableExistsCached = null;
        }

        public int CountRows()
        {
            if (!TableExists())
                return 0;
            using (var db = CreateDb())
                return db.ExecuteScalar<int>("SELECT COUNT(*) FROM WooCatalogCacheTbl");
        }

        public List<WooProductDto> GetAllAsDtos()
        {
            var list = new List<WooProductDto>();
            if (!TableExists())
                return list;

            const string sql = @"
SELECT * FROM WooCatalogCacheTbl
ORDER BY WooProductId, CASE WHEN IsParentGroup = 1 THEN 0 ELSE 1 END, WooVariationId";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    var row = DbMapper.Map<WooCatalogCacheRow>(rdr);
                    list.Add(ToDto(row));
                }
            }
            return list;
        }

        public void ReplaceAll(IList<WooProductDto> products)
        {
            UpsertAll(products, DateTime.UtcNow);
        }

        /// <summary>Merge Woo catalog into cache via temp table + MERGE (bulk); preserve FirstSeenUtc on match.</summary>
        public int UpsertAll(IList<WooProductDto> products, DateTime pullStartedUtc)
        {
            EnsureFirstSeenColumn();
            if (!TableExists())
                return 0;

            if (products == null)
                products = new List<WooProductDto>();

            var table = BuildPullDataTable(products, pullStartedUtc);
            string cs = GetSqlConnectionString();
            int inserted = 0;

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
IF OBJECT_ID('tempdb..#WooCatPull') IS NOT NULL DROP TABLE #WooCatPull;
CREATE TABLE #WooCatPull (
  WooProductId BIGINT NOT NULL,
  WooVariationId BIGINT NOT NULL,
  IsParentGroup BIT NOT NULL,
  IsVariation BIT NOT NULL,
  Name NVARCHAR(500) NULL,
  Sku NVARCHAR(200) NULL,
  ParentSku NVARCHAR(200) NULL,
  ParentName NVARCHAR(500) NULL,
  Status NVARCHAR(50) NULL,
  StockStatus NVARCHAR(50) NULL,
  ProductType NVARCHAR(50) NULL,
  CategoriesLabel NVARCHAR(1000) NULL,
  CategoryIds NVARCHAR(500) NULL,
  AttributesJson NVARCHAR(MAX) NULL,
  PulledUtc DATETIME2 NOT NULL,
  FirstSeenUtc DATETIME2 NOT NULL,
  VariationTotalCount INT NOT NULL,
  VariationInStockCount INT NOT NULL,
  PRIMARY KEY (WooProductId, WooVariationId)
);";
                        cmd.ExecuteNonQuery();
                    }

                    using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tx))
                    {
                        bulk.DestinationTableName = "#WooCatPull";
                        bulk.BatchSize = 500;
                        bulk.BulkCopyTimeout = 120;
                        foreach (DataColumn col in table.Columns)
                            bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                        bulk.WriteToServer(table);
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandTimeout = 120;
                        cmd.CommandText = @"
MERGE WooCatalogCacheTbl AS t
USING #WooCatPull AS s
ON t.WooProductId = s.WooProductId AND t.WooVariationId = s.WooVariationId
WHEN MATCHED THEN UPDATE SET
  IsParentGroup = s.IsParentGroup,
  IsVariation = s.IsVariation,
  Name = s.Name,
  Sku = s.Sku,
  ParentSku = s.ParentSku,
  ParentName = s.ParentName,
  Status = s.Status,
  StockStatus = s.StockStatus,
  ProductType = s.ProductType,
  CategoriesLabel = s.CategoriesLabel,
  CategoryIds = s.CategoryIds,
  AttributesJson = s.AttributesJson,
  PulledUtc = s.PulledUtc,
  VariationTotalCount = s.VariationTotalCount,
  VariationInStockCount = s.VariationInStockCount
WHEN NOT MATCHED BY TARGET THEN INSERT
(WooProductId, WooVariationId, IsParentGroup, IsVariation, Name, Sku, ParentSku, ParentName,
 Status, StockStatus, ProductType, CategoriesLabel, CategoryIds, AttributesJson, PulledUtc, FirstSeenUtc,
 VariationTotalCount, VariationInStockCount)
VALUES
(s.WooProductId, s.WooVariationId, s.IsParentGroup, s.IsVariation, s.Name, s.Sku, s.ParentSku, s.ParentName,
 s.Status, s.StockStatus, s.ProductType, s.CategoriesLabel, s.CategoryIds, s.AttributesJson, s.PulledUtc, s.FirstSeenUtc,
 s.VariationTotalCount, s.VariationInStockCount)
WHEN NOT MATCHED BY SOURCE THEN DELETE
OUTPUT $action;";

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string action = Convert.ToString(rdr[0]);
                                if (string.Equals(action, "INSERT", StringComparison.OrdinalIgnoreCase))
                                    inserted++;
                            }
                        }
                    }

                    tx.Commit();
                }
            }

            return inserted;
        }

        private static string GetSqlConnectionString()
        {
            string cs = ConfigurationManager.ConnectionStrings["TrackerDataSQL"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                cs = ConfigurationManager.ConnectionStrings[SystemConstants.DatabaseConstants.ConnectionStringName]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new ConfigurationErrorsException("No SQL connection string found.");
            return cs;
        }

        private static DataTable BuildPullDataTable(IList<WooProductDto> products, DateTime pullStartedUtc)
        {
            var table = new DataTable();
            table.Columns.Add("WooProductId", typeof(long));
            table.Columns.Add("WooVariationId", typeof(long));
            table.Columns.Add("IsParentGroup", typeof(bool));
            table.Columns.Add("IsVariation", typeof(bool));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Sku", typeof(string));
            table.Columns.Add("ParentSku", typeof(string));
            table.Columns.Add("ParentName", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("StockStatus", typeof(string));
            table.Columns.Add("ProductType", typeof(string));
            table.Columns.Add("CategoriesLabel", typeof(string));
            table.Columns.Add("CategoryIds", typeof(string));
            table.Columns.Add("AttributesJson", typeof(string));
            table.Columns.Add("PulledUtc", typeof(DateTime));
            table.Columns.Add("FirstSeenUtc", typeof(DateTime));
            table.Columns.Add("VariationTotalCount", typeof(int));
            table.Columns.Add("VariationInStockCount", typeof(int));

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in products)
            {
                if (p == null)
                    continue;
                long varId = ResolveVariationId(p);
                string key = CacheKey(p.Id, varId);
                if (!seen.Add(key))
                    continue;

                table.Rows.Add(
                    p.Id,
                    varId,
                    p.IsParentGroup,
                    varId > 0,
                    (object)p.Name ?? DBNull.Value,
                    (object)p.Sku ?? DBNull.Value,
                    (object)p.ParentSku ?? DBNull.Value,
                    (object)p.ParentName ?? DBNull.Value,
                    (object)p.Status ?? DBNull.Value,
                    (object)p.StockStatus ?? DBNull.Value,
                    (object)p.Type ?? DBNull.Value,
                    (object)p.CategoriesLabel ?? DBNull.Value,
                    JoinIds(p.CategoryIds),
                    SerializeAttributes(p.Attributes),
                    pullStartedUtc,
                    pullStartedUtc,
                    p.VariationTotalCount,
                    p.VariationInStockCount);
            }

            return table;
        }

        public void ClearAll()
        {
            if (!TableExists())
                return;
            ExecNonQuery("DELETE FROM WooCatalogCacheTbl");
        }

        public void UpdateSku(long productId, long? variationId, string sku)
        {
            if (!TableExists())
                return;
            long varId = variationId.HasValue && variationId.Value > 0 ? variationId.Value : 0;
            const string sql = @"
UPDATE WooCatalogCacheTbl
SET Sku = @Sku
WHERE WooProductId = @P AND WooVariationId = @V";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Sku", DataValue = (object)sku ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@P", DataValue = productId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@V", DataValue = varId, DataDbType = DbType.Int64 }
            };
            ExecNonQuery(sql, p);

            if (varId == 0 && !string.IsNullOrWhiteSpace(sku))
            {
                const string childSql = @"
UPDATE WooCatalogCacheTbl
SET ParentSku = @Sku
WHERE WooProductId = @P AND WooVariationId <> 0";
                ExecNonQuery(childSql, new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Sku", DataValue = sku, DataDbType = DbType.String },
                    new DBParameter { ParamName = "@P", DataValue = productId, DataDbType = DbType.Int64 }
                });
            }
        }

        private void EnsureFirstSeenColumn()
        {
            if (!TableExists())
                return;
            try
            {
                using (var db = CreateDb())
                {
                    int n = db.ExecuteScalar<int>(@"
SELECT COUNT(*) FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.WooCatalogCacheTbl') AND name = N'FirstSeenUtc'");
                    if (n > 0)
                        return;
                }

                ExecNonQuery("ALTER TABLE dbo.WooCatalogCacheTbl ADD FirstSeenUtc DATETIME2 NULL");
                ExecNonQuery("UPDATE dbo.WooCatalogCacheTbl SET FirstSeenUtc = PulledUtc WHERE FirstSeenUtc IS NULL");
            }
            catch
            {
                // Schema XML / concurrent ensure handles this on next request.
            }
        }

        private static long ResolveVariationId(WooProductDto p)
        {
            if (p.IsParentGroup)
                return 0;
            return p.VariationId.HasValue && p.VariationId.Value > 0 ? p.VariationId.Value : 0;
        }

        private static string CacheKey(long productId, long variationId)
        {
            return productId.ToString(CultureInfo.InvariantCulture) + ":" + variationId.ToString(CultureInfo.InvariantCulture);
        }

        private static WooProductDto ToDto(WooCatalogCacheRow row)
        {
            var dto = new WooProductDto
            {
                Id = row.WooProductId,
                VariationId = row.WooVariationId > 0 ? row.WooVariationId : (long?)null,
                IsParentGroup = row.IsParentGroup,
                Name = row.Name,
                Sku = row.Sku,
                ParentSku = row.ParentSku,
                ParentName = row.ParentName,
                Status = row.Status,
                StockStatus = row.StockStatus,
                Type = row.ProductType,
                CategoriesLabel = row.CategoriesLabel,
                CategoryIds = ParseIds(row.CategoryIds),
                Attributes = DeserializeAttributes(row.AttributesJson),
                VariationTotalCount = row.VariationTotalCount,
                VariationInStockCount = row.VariationInStockCount,
                FirstSeenUtc = row.FirstSeenUtc ?? row.PulledUtc
            };
            return dto;
        }

        private static string JoinIds(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
                return string.Empty;
            return string.Join(",", ids.Select(id => id.ToString(CultureInfo.InvariantCulture)));
        }

        private static List<long> ParseIds(string raw)
        {
            var list = new List<long>();
            if (string.IsNullOrWhiteSpace(raw))
                return list;
            foreach (string part in raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                long id;
                if (long.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id) && id > 0)
                    list.Add(id);
            }
            return list;
        }

        private static string SerializeAttributes(List<WooAttributeValue> attrs)
        {
            if (attrs == null || attrs.Count == 0)
                return "[]";
            return JsonConvert.SerializeObject(attrs);
        }

        private static List<WooAttributeValue> DeserializeAttributes(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<WooAttributeValue>();
            try
            {
                return JsonConvert.DeserializeObject<List<WooAttributeValue>>(json) ?? new List<WooAttributeValue>();
            }
            catch
            {
                return new List<WooAttributeValue>();
            }
        }
    }
}
