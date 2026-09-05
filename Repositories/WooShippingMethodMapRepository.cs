using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooShippingMethodMapRepository : RepositoryBase<WooShippingMethodMap>
    {
        protected override string TableName => "WooShippingMethodMapTbl";
        protected override string KeyColumn => "MapID";

        public List<WooShippingMethodMap> GetAllOrdered(bool includeInactive = true)
        {
            var list = new List<WooShippingMethodMap>();
            string sql = @"
SELECT m.*, p.Abbreviation AS PersonAbbrev
FROM WooShippingMethodMapTbl m
LEFT JOIN PeopleTbl p ON p.PersonID = m.ToBeDeliveredByID";
            if (!includeInactive)
                sql += " WHERE m.IsActive = 1";
            sql += " ORDER BY m.MethodMatch, m.MapID";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(MapRow(rdr));
            }
            return list;
        }

        public int InsertMap(WooShippingMethodMap map, string updatedBy)
        {
            const string sql = @"
INSERT INTO WooShippingMethodMapTbl (MethodMatch, ToBeDeliveredByID, IsActive, Notes, UpdatedAt, UpdatedBy)
VALUES (@MethodMatch, @ToBeDeliveredByID, @IsActive, @Notes, SYSUTCDATETIME(), @UpdatedBy);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return ExecuteScalar<int>(sql, BuildParams(map, updatedBy, includeId: false));
        }

        public int UpdateMap(WooShippingMethodMap map, string updatedBy)
        {
            const string sql = @"
UPDATE WooShippingMethodMapTbl SET
 MethodMatch = @MethodMatch,
 ToBeDeliveredByID = @ToBeDeliveredByID,
 IsActive = @IsActive,
 Notes = @Notes,
 UpdatedAt = SYSUTCDATETIME(),
 UpdatedBy = @UpdatedBy
WHERE MapID = @MapID";
            return ExecNonQuery(sql, BuildParams(map, updatedBy, includeId: true));
        }

        public int DeleteMap(int mapId)
        {
            return ExecNonQuery(
                "DELETE FROM WooShippingMethodMapTbl WHERE MapID = @MapID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@MapID", DataValue = mapId, DataDbType = DbType.Int32 }
                });
        }

        private static List<DBParameter> BuildParams(WooShippingMethodMap map, string updatedBy, bool includeId)
        {
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@MethodMatch", DataValue = map.MethodMatch ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ToBeDeliveredByID", DataValue = map.ToBeDeliveredByID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@IsActive", DataValue = map.IsActive, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = (object)map.Notes ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@UpdatedBy", DataValue = updatedBy ?? string.Empty, DataDbType = DbType.String }
            };
            if (includeId)
                p.Add(new DBParameter { ParamName = "@MapID", DataValue = map.MapID, DataDbType = DbType.Int32 });
            return p;
        }

        private static WooShippingMethodMap MapRow(IDataRecord rdr)
        {
            return new WooShippingMethodMap
            {
                MapID = Convert.ToInt32(rdr["MapID"]),
                MethodMatch = rdr["MethodMatch"] == DBNull.Value ? string.Empty : rdr["MethodMatch"].ToString(),
                ToBeDeliveredByID = Convert.ToInt32(rdr["ToBeDeliveredByID"]),
                IsActive = rdr["IsActive"] != DBNull.Value && Convert.ToBoolean(rdr["IsActive"]),
                Notes = rdr["Notes"] == DBNull.Value ? null : rdr["Notes"].ToString(),
                PersonAbbrev = HasColumn(rdr, "PersonAbbrev") && rdr["PersonAbbrev"] != DBNull.Value
                    ? rdr["PersonAbbrev"].ToString()
                    : null
            };
        }

        private static bool HasColumn(IDataRecord rdr, string name)
        {
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                if (string.Equals(rdr.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
