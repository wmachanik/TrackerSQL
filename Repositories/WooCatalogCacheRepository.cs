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
            ExecNonQuery("DELETE FROM WooCatalogCacheTbl");
            if (products == null || products.Count == 0)
                return;

            DateTime pulled = DateTime.UtcNow;
            foreach (var p in products)
            {
                if (p == null)
                    continue;
                InsertRow(p, pulled);
            }
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

        private void InsertRow(WooProductDto p, DateTime pulledUtc)
        {
            long varId = p.IsParentGroup
                ? 0
                : (p.VariationId.HasValue && p.VariationId.Value > 0 ? p.VariationId.Value : 0);
            const string sql = @"
INSERT INTO WooCatalogCacheTbl
(WooProductId, WooVariationId, IsParentGroup, IsVariation, Name, Sku, ParentSku, ParentName,
 Status, StockStatus, ProductType, CategoriesLabel, CategoryIds, AttributesJson, PulledUtc,
 VariationTotalCount, VariationInStockCount)
VALUES
(@WooProductId, @WooVariationId, @IsParentGroup, @IsVariation, @Name, @Sku, @ParentSku, @ParentName,
 @Status, @StockStatus, @ProductType, @CategoriesLabel, @CategoryIds, @AttributesJson, @PulledUtc,
 @VariationTotalCount, @VariationInStockCount)";
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
            ExecNonQuery(sql, parameters);
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
                VariationInStockCount = row.VariationInStockCount
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
