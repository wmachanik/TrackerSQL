using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooItemMappingRepository : RepositoryBase<WooItemMapping>
    {
        protected override string TableName => "WooItemMappingsTbl";
        protected override string KeyColumn => "MappingID";

        public List<WooItemMapping> GetAllWithItems()
        {
            var list = new List<WooItemMapping>();
            string sql = @"
SELECT m.*, i.ItemDesc, i.SKU AS ItemSku, i.ItemEnabled
FROM WooItemMappingsTbl m
LEFT JOIN ItemsTbl i ON i.ItemID = m.ItemID
ORDER BY m.MappingID DESC";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                {
                    var row = DbMapper.Map<WooItemMapping>(rdr);
                    if (HasColumn(rdr, "ItemDesc")) row.ItemDesc = rdr["ItemDesc"] as string;
                    if (HasColumn(rdr, "ItemSku")) row.ItemSku = rdr["ItemSku"] as string;
                    if (HasColumn(rdr, "ItemEnabled") && rdr["ItemEnabled"] != DBNull.Value)
                        row.ItemEnabled = Convert.ToBoolean(rdr["ItemEnabled"]);
                    list.Add(row);
                }
            }
            return list;
        }

        public WooItemMapping FindExact(long productId, long? variationId)
        {
            string sql = variationId.HasValue && variationId.Value > 0
                ? "SELECT * FROM WooItemMappingsTbl WHERE WooProductId = @P AND WooVariationId = @V AND IsActive = 1"
                : "SELECT * FROM WooItemMappingsTbl WHERE WooProductId = @P AND (WooVariationId IS NULL OR WooVariationId = 0) AND IsActive = 1";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@P", DataValue = productId, DataDbType = DbType.Int64 }
            };
            if (variationId.HasValue && variationId.Value > 0)
                p.Add(new DBParameter { ParamName = "@V", DataValue = variationId.Value, DataDbType = DbType.Int64 });
            return ExecuteQuerySingle<WooItemMapping>(sql, p);
        }

        public override int Insert(WooItemMapping entity)
        {
            string sql = @"
INSERT INTO WooItemMappingsTbl
(ItemID, WooProductId, WooVariationId, MapType, SkuPattern, QtyFactor, PackagingID, DisableScope, LastSyncedUtc, LastWooStatus, IsActive, IncludeInImport)
VALUES
(@ItemID, @WooProductId, @WooVariationId, @MapType, @SkuPattern, @QtyFactor, @PackagingID, @DisableScope, @LastSyncedUtc, @LastWooStatus, @IsActive, @IncludeInImport);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return ExecuteScalar<int>(sql, BuildParams(entity, includeKey: false));
        }

        public override int Update(WooItemMapping entity)
        {
            string sql = @"
UPDATE WooItemMappingsTbl SET
 ItemID = @ItemID,
 WooProductId = @WooProductId,
 WooVariationId = @WooVariationId,
 MapType = @MapType,
 SkuPattern = @SkuPattern,
 QtyFactor = @QtyFactor,
 PackagingID = @PackagingID,
 DisableScope = @DisableScope,
 LastSyncedUtc = @LastSyncedUtc,
 LastWooStatus = @LastWooStatus,
 IsActive = @IsActive,
 IncludeInImport = @IncludeInImport
WHERE MappingID = @MappingID";
            return ExecNonQuery(sql, BuildParams(entity, includeKey: true));
        }

        public int DeleteMapping(int mappingId)
        {
            string sql = "DELETE FROM WooItemMappingsTbl WHERE MappingID = @MappingID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@MappingID", DataValue = mappingId, DataDbType = DbType.Int32 }
            };
            return ExecNonQuery(sql, p);
        }

        public int SetIncludeInImportForProduct(long productId, bool include)
        {
            const string sql = @"
UPDATE WooItemMappingsTbl
SET IncludeInImport = @IncludeInImport
WHERE WooProductId = @P AND IsActive = 1";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@IncludeInImport", DataValue = include, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@P", DataValue = productId, DataDbType = DbType.Int64 }
            };
            return ExecNonQuery(sql, p);
        }

        private static List<DBParameter> BuildParams(WooItemMapping e, bool includeKey)
        {
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemID", DataValue = e.ItemID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@WooProductId", DataValue = (object)e.WooProductId ?? DBNull.Value, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@WooVariationId", DataValue = (object)e.WooVariationId ?? DBNull.Value, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@MapType", DataValue = e.MapType ?? "Exact", DataDbType = DbType.String },
                new DBParameter { ParamName = "@SkuPattern", DataValue = (object)e.SkuPattern ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@QtyFactor", DataValue = e.QtyFactor, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@PackagingID", DataValue = DbParamHelpers.FkOrDbNull(e.PackagingID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@DisableScope", DataValue = (object)e.DisableScope ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@LastSyncedUtc", DataValue = (object)e.LastSyncedUtc ?? DBNull.Value, DataDbType = DbType.DateTime2 },
                new DBParameter { ParamName = "@LastWooStatus", DataValue = (object)e.LastWooStatus ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@IsActive", DataValue = e.IsActive, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@IncludeInImport", DataValue = e.IncludeInImport, DataDbType = DbType.Boolean }
            };
            if (includeKey)
                p.Add(new DBParameter { ParamName = "@MappingID", DataValue = e.MappingID, DataDbType = DbType.Int32 });
            return p;
        }

        private static bool HasColumn(IDataRecord rdr, string name)
        {
            for (int i = 0; i < rdr.FieldCount; i++)
                if (string.Equals(rdr.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
