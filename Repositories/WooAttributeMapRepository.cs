using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooAttributeMapRepository : RepositoryBase<WooAttributeMap>
    {
        protected override string TableName => "WooAttributeMapTbl";
        protected override string KeyColumn => "MapID";

        public List<WooAttributeMap> GetAllWithLookups()
        {
            var list = new List<WooAttributeMap>();
            const string sql = @"
SELECT m.*,
       p.ItemPrepDescription AS PackagingDesc,
       CASE WHEN m.ItemServiceTypeID = 0 THEN N'(all types)' ELSE st.ItemServiceTypeName END AS ItemServiceTypeName
FROM WooAttributeMapTbl m
LEFT JOIN ItemPackagingsTbl p ON p.ItemPackagingID = m.PackagingID
LEFT JOIN ItemServiceTypesTbl st ON st.ItemServiceTypeID = m.ItemServiceTypeID
ORDER BY m.AttributeName, m.AttributeOption, m.ItemServiceTypeID";

            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    var row = DbMapper.Map<WooAttributeMap>(rdr);
                    row.MapRole = WooAttributeMapRoles.Normalize(row.MapRole);
                    if (DbMapper.HasColumn(rdr, "PackagingDesc"))
                        row.PackagingDesc = rdr["PackagingDesc"] as string;
                    if (DbMapper.HasColumn(rdr, "ItemServiceTypeName"))
                        row.ItemServiceTypeName = rdr["ItemServiceTypeName"] as string;
                    list.Add(row);
                }
            }
            return list;
        }

        public List<WooAttributeMap> GetActive()
        {
            return GetAllWithLookups().FindAll(m => m.IsActive);
        }

        public override int Insert(WooAttributeMap entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            const string sql = @"
INSERT INTO WooAttributeMapTbl
(AttributeName, AttributeOption, QtyFactor, PackagingID, PrepTypeID, MapRole, ItemServiceTypeID, IsActive, Notes)
VALUES
(@AttributeName, @AttributeOption, @QtyFactor, @PackagingID, @PrepTypeID, @MapRole, @ItemServiceTypeID, @IsActive, @Notes);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return ExecuteScalar<int>(sql, BuildParams(entity, includeKey: false));
        }

        public override int Update(WooAttributeMap entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            const string sql = @"
UPDATE WooAttributeMapTbl SET
 AttributeName = @AttributeName,
 AttributeOption = @AttributeOption,
 QtyFactor = @QtyFactor,
 PackagingID = @PackagingID,
 PrepTypeID = @PrepTypeID,
 MapRole = @MapRole,
 ItemServiceTypeID = @ItemServiceTypeID,
 IsActive = @IsActive,
 Notes = @Notes
WHERE MapID = @MapID";
            return ExecNonQuery(sql, BuildParams(entity, includeKey: true));
        }

        public bool DeleteMap(int mapId)
        {
            const string sql = "DELETE FROM WooAttributeMapTbl WHERE MapID = @MapID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@MapID", DataValue = mapId, DataDbType = DbType.Int32 }
            };
            return ExecNonQuery(sql, parameters) > 0;
        }

        public WooAttributeMap FindExact(string attributeName, string attributeOption, int itemServiceTypeId)
        {
            const string sql = @"
SELECT TOP 1 * FROM WooAttributeMapTbl
WHERE AttributeName = @AttributeName
  AND AttributeOption = @AttributeOption
  AND ItemServiceTypeID = @ItemServiceTypeID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@AttributeName", DataValue = (attributeName ?? string.Empty).Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@AttributeOption", DataValue = (attributeOption ?? string.Empty).Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@ItemServiceTypeID", DataValue = itemServiceTypeId, DataDbType = DbType.Int32 }
            };
            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    var row = DbMapper.Map<WooAttributeMap>(rdr);
                    row.MapRole = WooAttributeMapRoles.Normalize(row.MapRole);
                    return row;
                }
            }
            return null;
        }

        private static List<DBParameter> BuildParams(WooAttributeMap e, bool includeKey)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@AttributeName", DataValue = (e.AttributeName ?? string.Empty).Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@AttributeOption", DataValue = (e.AttributeOption ?? string.Empty).Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@QtyFactor", DataValue = e.QtyFactor <= 0 ? 1.0 : e.QtyFactor, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@PackagingID", DataValue = DbParamHelpers.FkOrDbNull(e.PackagingID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PrepTypeID", DataValue = DbParamHelpers.FkOrDbNull(e.PrepTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@MapRole", DataValue = WooAttributeMapRoles.Normalize(e.MapRole), DataDbType = DbType.String },
                new DBParameter { ParamName = "@ItemServiceTypeID", DataValue = e.ItemServiceTypeID < 0 ? 0 : e.ItemServiceTypeID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@IsActive", DataValue = e.IsActive, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = (object)e.Notes ?? DBNull.Value, DataDbType = DbType.String }
            };
            if (includeKey)
                parameters.Add(new DBParameter { ParamName = "@MapID", DataValue = e.MapID, DataDbType = DbType.Int32 });
            return parameters;
        }
}
}
