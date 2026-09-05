using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooPostalAreaMapRepository : RepositoryBase<WooPostalAreaMap>
    {
        protected override string TableName => "WooPostalAreaMapTbl";
        protected override string KeyColumn => "MapID";

        public List<WooPostalAreaMap> GetAllActiveOrdered()
        {
            return GetAllOrdered(includeInactive: false);
        }

        public List<WooPostalAreaMap> GetAllOrdered(bool includeInactive = true)
        {
            var list = new List<WooPostalAreaMap>();
            string sql = @"
SELECT m.*, a.AreaName
FROM WooPostalAreaMapTbl m
LEFT JOIN AreasTbl a ON a.AreaID = m.AreaID";
            if (!includeInactive)
                sql += " WHERE m.IsActive = 1";
            sql += " ORDER BY m.PostalFrom, m.PostalTo, m.Priority, m.MapID";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(MapRow(rdr));
            }
            return list;
        }

        public int InsertMap(WooPostalAreaMap map, string updatedBy)
        {
            const string sql = @"
INSERT INTO WooPostalAreaMapTbl
(PostalFrom, PostalTo, SuburbMatch, AreaID, Priority, Source, IsActive, Notes, UpdatedAt, UpdatedBy)
VALUES
(@PostalFrom, @PostalTo, @SuburbMatch, @AreaID, @Priority, @Source, @IsActive, @Notes, SYSUTCDATETIME(), @UpdatedBy);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return ExecuteScalar<int>(sql, BuildParams(map, updatedBy, includeId: false));
        }

        public int UpdateMap(WooPostalAreaMap map, string updatedBy)
        {
            const string sql = @"
UPDATE WooPostalAreaMapTbl SET
 PostalFrom = @PostalFrom,
 PostalTo = @PostalTo,
 SuburbMatch = @SuburbMatch,
 AreaID = @AreaID,
 Priority = @Priority,
 Source = @Source,
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
                "DELETE FROM WooPostalAreaMapTbl WHERE MapID = @MapID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@MapID", DataValue = mapId, DataDbType = DbType.Int32 }
                });
        }

        private static List<DBParameter> BuildParams(WooPostalAreaMap map, string updatedBy, bool includeId)
        {
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@PostalFrom", DataValue = map.PostalFrom, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PostalTo", DataValue = map.PostalTo, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SuburbMatch", DataValue = (object)NormaliseSuburb(map.SuburbMatch) ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AreaID", DataValue = map.AreaID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Priority", DataValue = map.Priority, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Source", DataValue = map.Source ?? "Manual", DataDbType = DbType.String },
                new DBParameter { ParamName = "@IsActive", DataValue = map.IsActive, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = (object)map.Notes ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@UpdatedBy", DataValue = updatedBy ?? string.Empty, DataDbType = DbType.String }
            };
            if (includeId)
                p.Add(new DBParameter { ParamName = "@MapID", DataValue = map.MapID, DataDbType = DbType.Int32 });
            return p;
        }

        private static string NormaliseSuburb(string suburb)
        {
            if (string.IsNullOrWhiteSpace(suburb))
                return null;
            string t = suburb.Trim();
            if (t == "*" || string.Equals(t, "(any)", StringComparison.OrdinalIgnoreCase))
                return null;
            return t;
        }

        private static WooPostalAreaMap MapRow(IDataRecord rdr)
        {
            return new WooPostalAreaMap
            {
                MapID = Convert.ToInt32(rdr["MapID"]),
                PostalFrom = Convert.ToInt32(rdr["PostalFrom"]),
                PostalTo = Convert.ToInt32(rdr["PostalTo"]),
                SuburbMatch = rdr["SuburbMatch"] == DBNull.Value ? null : rdr["SuburbMatch"].ToString(),
                AreaID = Convert.ToInt32(rdr["AreaID"]),
                Priority = Convert.ToInt32(rdr["Priority"]),
                Source = rdr["Source"] == DBNull.Value ? "Manual" : rdr["Source"].ToString(),
                IsActive = rdr["IsActive"] != DBNull.Value && Convert.ToBoolean(rdr["IsActive"]),
                Notes = rdr["Notes"] == DBNull.Value ? null : rdr["Notes"].ToString(),
                AreaName = HasColumn(rdr, "AreaName") && rdr["AreaName"] != DBNull.Value
                    ? rdr["AreaName"].ToString()
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
