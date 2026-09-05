using System;
using System.Collections.Generic;
using System.Data;
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

        public bool TableExists()
        {
            try
            {
                using (var db = CreateDb())
                {
                    int n = db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'WooCatalogCacheTbl'");
                    return n > 0;
                }
            }
            catch
            {
                return false;
            }
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

        /// <summary>Merge Woo catalog into cache; new rows get FirstSeenUtc = pullStartedUtc.</summary>
        public int UpsertAll(IList<WooProductDto> products, DateTime pullStartedUtc)
        {
            EnsureFirstSeenColumn();
            if (products == null)
                products = new List<WooProductDto>();

            var existingFirstSeen = LoadFirstSeenByKey();
            var incomingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int inserted = 0;

            foreach (var p in products)
            {
                if (p == null)
                    continue;
                string key = CacheKey(p.Id, ResolveVariationId(p));
                incomingKeys.Add(key);
                if (existingFirstSeen.ContainsKey(key))
                    UpdateRow(p, pullStartedUtc, existingFirstSeen[key]);
                else
                {
                    InsertRow(p, pullStartedUtc, pullStartedUtc);
                    inserted++;
                }
            }

            DeleteExcept(incomingKeys);
            return inserted;
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

            // Parent group rows use variation 0; also stamp ParentSku on children when writing the parent SKU.
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

        private void InsertRow(WooProductDto p, DateTime pulledUtc, DateTime firstSeenUtc)
        {
            long varId = ResolveVariationId(p);
            const string sql = @"
INSERT INTO WooCatalogCacheTbl
(WooProductId, WooVariationId, IsParentGroup, IsVariation, Name, Sku, ParentSku, ParentName,
 Status, StockStatus, ProductType, CategoriesLabel, CategoryIds, AttributesJson, PulledUtc, FirstSeenUtc,
 VariationTotalCount, VariationInStockCount)
VALUES
(@WooProductId, @WooVariationId, @IsParentGroup, @IsVariation, @Name, @Sku, @ParentSku, @ParentName,
 @Status, @StockStatus, @ProductType, @CategoriesLabel, @CategoryIds, @AttributesJson, @PulledUtc, @FirstSeenUtc,
 @VariationTotalCount, @VariationInStockCount)";
            ExecNonQuery(sql, BuildRowParams(p, varId, pulledUtc, firstSeenUtc));
        }

        private void UpdateRow(WooProductDto p, DateTime pulledUtc, DateTime firstSeenUtc)
        {
            long varId = ResolveVariationId(p);
            const string sql = @"
UPDATE WooCatalogCacheTbl SET
 IsParentGroup = @IsParentGroup,
 IsVariation = @IsVariation,
 Name = @Name,
 Sku = @Sku,
 ParentSku = @ParentSku,
 ParentName = @ParentName,
 Status = @Status,
 StockStatus = @StockStatus,
 ProductType = @ProductType,
 CategoriesLabel = @CategoriesLabel,
 CategoryIds = @CategoryIds,
 AttributesJson = @AttributesJson,
 PulledUtc = @PulledUtc,
 VariationTotalCount = @VariationTotalCount,
 VariationInStockCount = @VariationInStockCount
WHERE WooProductId = @WooProductId AND WooVariationId = @WooVariationId";
            ExecNonQuery(sql, BuildRowParams(p, varId, pulledUtc, firstSeenUtc, includeFirstSeen: false));
        }

        private void DeleteExcept(HashSet<string> keepKeys)
        {
            if (!TableExists())
                return;
            var allKeys = LoadAllKeys();
            foreach (string key in allKeys)
            {
                if (keepKeys.Contains(key))
                    continue;
                ParseCacheKey(key, out long productId, out long variationId);
                ExecNonQuery(
                    "DELETE FROM WooCatalogCacheTbl WHERE WooProductId = @P AND WooVariationId = @V",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@P", DataValue = productId, DataDbType = DbType.Int64 },
                        new DBParameter { ParamName = "@V", DataValue = variationId, DataDbType = DbType.Int64 }
                    });
            }
        }

        private Dictionary<string, DateTime> LoadFirstSeenByKey()
        {
            var map = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            if (!TableExists())
                return map;

            EnsureFirstSeenColumn();
            const string sql = "SELECT WooProductId, WooVariationId, FirstSeenUtc, PulledUtc FROM WooCatalogCacheTbl";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    long productId = Convert.ToInt64(rdr["WooProductId"]);
                    long variationId = Convert.ToInt64(rdr["WooVariationId"]);
                    DateTime firstSeen = rdr["FirstSeenUtc"] != DBNull.Value
                        ? Convert.ToDateTime(rdr["FirstSeenUtc"])
                        : Convert.ToDateTime(rdr["PulledUtc"]);
                    map[CacheKey(productId, variationId)] = firstSeen;
                }
            }
            return map;
        }

        private List<string> LoadAllKeys()
        {
            var keys = new List<string>();
            if (!TableExists())
                return keys;
            const string sql = "SELECT WooProductId, WooVariationId FROM WooCatalogCacheTbl";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    keys.Add(CacheKey(
                        Convert.ToInt64(rdr["WooProductId"]),
                        Convert.ToInt64(rdr["WooVariationId"])));
                }
            }
            return keys;
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

        private static void ParseCacheKey(string key, out long productId, out long variationId)
        {
            productId = 0;
            variationId = 0;
            if (string.IsNullOrWhiteSpace(key))
                return;
            string[] parts = key.Split(':');
            if (parts.Length != 2)
                return;
            long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out productId);
            long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out variationId);
        }

        private static List<DBParameter> BuildRowParams(
            WooProductDto p,
            long varId,
            DateTime pulledUtc,
            DateTime firstSeenUtc,
            bool includeFirstSeen = true)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooProductId", DataValue = p.Id, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@WooVariationId", DataValue = varId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@IsParentGroup", DataValue = p.IsParentGroup, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@IsVariation", DataValue = varId > 0, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Name", DataValue = (object)p.Name ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Sku", DataValue = (object)p.Sku ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ParentSku", DataValue = (object)p.ParentSku ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ParentName", DataValue = (object)p.ParentName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Status", DataValue = (object)p.Status ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@StockStatus", DataValue = (object)p.StockStatus ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ProductType", DataValue = (object)p.Type ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@CategoriesLabel", DataValue = (object)p.CategoriesLabel ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@CategoryIds", DataValue = JoinIds(p.CategoryIds), DataDbType = DbType.String },
                new DBParameter { ParamName = "@AttributesJson", DataValue = SerializeAttributes(p.Attributes), DataDbType = DbType.String },
                new DBParameter { ParamName = "@PulledUtc", DataValue = pulledUtc, DataDbType = DbType.DateTime2 },
                new DBParameter { ParamName = "@VariationTotalCount", DataValue = p.VariationTotalCount, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@VariationInStockCount", DataValue = p.VariationInStockCount, DataDbType = DbType.Int32 }
            };
            if (includeFirstSeen)
                parameters.Add(new DBParameter { ParamName = "@FirstSeenUtc", DataValue = firstSeenUtc, DataDbType = DbType.DateTime2 });
            return parameters;
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
