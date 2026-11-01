using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class AltAddressKindsRepository
    {
        private static bool? _tableSeen;

        public static bool TableSeen()
        {
            if (_tableSeen.HasValue)
                return _tableSeen.Value;
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    _tableSeen = db.ExecuteScalar<int>(@"
SELECT CASE WHEN OBJECT_ID(N'dbo.AltAddressKindsTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
                }
            }
            catch
            {
                _tableSeen = false;
            }
            return _tableSeen.Value;
        }

        public static void ResetSeen() => _tableSeen = null;

        public List<AltAddressKind> GetAll()
        {
            var list = new List<AltAddressKind>();
            if (!TableSeen())
                return list;

            const string sql = @"
SELECT AltAddressKindID, KindCode, KindName, CountsTowardTracking, SortOrder, Enabled
FROM dbo.AltAddressKindsTbl
ORDER BY SortOrder, KindName, AltAddressKindID";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                    list.Add(Map(rdr));
            }
            return list;
        }

        public List<AltAddressKind> GetEnabled()
        {
            var list = new List<AltAddressKind>();
            foreach (AltAddressKind row in GetAll())
            {
                if (row.Enabled)
                    list.Add(row);
            }
            return list;
        }

        public int Insert(AltAddressKind row)
        {
            if (row == null || !TableSeen())
                return 0;
            const string sql = @"
INSERT INTO dbo.AltAddressKindsTbl (KindCode, KindName, CountsTowardTracking, SortOrder, Enabled)
VALUES (@KindCode, @KindName, @CountsTowardTracking, @SortOrder, @Enabled);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<int>(sql, Params(row, includeId: false));
        }

        public bool Update(AltAddressKind row)
        {
            if (row == null || row.AltAddressKindID <= 0 || !TableSeen())
                return false;
            const string sql = @"
UPDATE dbo.AltAddressKindsTbl SET
    KindCode = @KindCode,
    KindName = @KindName,
    CountsTowardTracking = @CountsTowardTracking,
    SortOrder = @SortOrder,
    Enabled = @Enabled
WHERE AltAddressKindID = @AltAddressKindID";
            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(sql, Params(row, includeId: true)) > 0;
        }

        public bool Delete(int id)
        {
            if (id <= 0 || !TableSeen())
                return false;
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(
                    "DELETE FROM dbo.AltAddressKindsTbl WHERE AltAddressKindID = @Id",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@Id", DataValue = id, DataDbType = DbType.Int32 }
                    }) > 0;
            }
        }

        public int CountAddressesUsing(string kindCode)
        {
            if (string.IsNullOrWhiteSpace(kindCode) || !ContactDeliverToAddressRepository.TableSeen())
                return 0;
            using (var db = new TrackerSQLDb())
            {
                object n = db.ExecuteScalar(@"
SELECT COUNT(*) FROM dbo.ContactDeliverToAddressTbl WHERE AddressKind = @Code",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@Code", DataValue = kindCode.Trim(), DataDbType = DbType.String }
                    });
                return n == null || n == DBNull.Value ? 0 : Convert.ToInt32(n);
            }
        }

        private static AltAddressKind Map(IDataRecord r)
        {
            return new AltAddressKind
            {
                AltAddressKindID = Convert.ToInt32(r["AltAddressKindID"]),
                KindCode = r["KindCode"] == DBNull.Value ? string.Empty : r["KindCode"].ToString(),
                KindName = r["KindName"] == DBNull.Value ? string.Empty : r["KindName"].ToString(),
                CountsTowardTracking = r["CountsTowardTracking"] != DBNull.Value && Convert.ToBoolean(r["CountsTowardTracking"]),
                SortOrder = r["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(r["SortOrder"]),
                Enabled = r["Enabled"] == DBNull.Value || Convert.ToBoolean(r["Enabled"])
            };
        }

        private static List<DBParameter> Params(AltAddressKind row, bool includeId)
        {
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@KindCode", DataValue = row.KindCode ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@KindName", DataValue = row.KindName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@CountsTowardTracking", DataValue = row.CountsTowardTracking, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@SortOrder", DataValue = row.SortOrder, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Enabled", DataValue = row.Enabled, DataDbType = DbType.Boolean }
            };
            if (includeId)
                p.Add(new DBParameter { ParamName = "@AltAddressKindID", DataValue = row.AltAddressKindID, DataDbType = DbType.Int32 });
            return p;
        }
    }
}
