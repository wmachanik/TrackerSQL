using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooContactInfoRepository
    {
        public static bool TableSeen()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT CASE WHEN OBJECT_ID(N'dbo.WooContactInfoTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                return false;
            }
        }

        public WooContactInfo GetByContactId(int contactId)
        {
            if (!TableSeen() || contactId <= 0)
                return null;
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(
                "SELECT * FROM dbo.WooContactInfoTbl WHERE ContactID = @Id",
                new List<DBParameter> { P("@Id", contactId, DbType.Int32) }))
            {
                if (r != null && r.Read())
                    return Map(r);
            }
            return null;
        }

        public void UpsertImportStats(int contactId, long? wooCustomerId, string matchEmail, int wooOrderCount,
            bool quaffeeLoversMember, DateTime? checkedUtc)
        {
            if (!TableSeen() || contactId <= 0)
                return;

            var ps = new List<DBParameter>
            {
                P("@ContactID", contactId, DbType.Int32),
                P("@WooCustomerId", wooCustomerId.HasValue && wooCustomerId.Value > 0 ? (object)wooCustomerId.Value : DBNull.Value, DbType.Int64),
                P("@MatchEmail", (object)matchEmail ?? DBNull.Value, DbType.String),
                P("@WooOrderCount", wooOrderCount, DbType.Int32),
                P("@CheckedUtc", (object)checkedUtc ?? DBNull.Value, DbType.DateTime2),
                P("@Member", quaffeeLoversMember, DbType.Boolean)
            };

            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.WooContactInfoTbl WHERE ContactID = @ContactID)
    INSERT INTO dbo.WooContactInfoTbl (ContactID, WooCustomerId, MatchEmail, LastSyncedUtc, WooOrderCount, QuaffeeLoversMember, QuaffeeLoversCheckedUtc)
    VALUES (@ContactID, @WooCustomerId, @MatchEmail, SYSUTCDATETIME(), @WooOrderCount, @Member, @CheckedUtc)
ELSE
    UPDATE dbo.WooContactInfoTbl SET
        WooCustomerId = COALESCE(@WooCustomerId, WooCustomerId),
        MatchEmail = COALESCE(@MatchEmail, MatchEmail),
        LastSyncedUtc = SYSUTCDATETIME(),
        WooOrderCount = @WooOrderCount,
        QuaffeeLoversMember = @Member,
        QuaffeeLoversCheckedUtc = @CheckedUtc
    WHERE ContactID = @ContactID";

            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(sql, ps);
        }

        private static WooContactInfo Map(IDataRecord r)
        {
            var row = new WooContactInfo
            {
                ContactID = Convert.ToInt64(r["ContactID"]),
                WooCustomerId = r["WooCustomerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["WooCustomerId"]),
                MatchEmail = r["MatchEmail"] == DBNull.Value ? null : r["MatchEmail"].ToString(),
                LastSyncedUtc = r["LastSyncedUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["LastSyncedUtc"])
            };
            if (DbMapper.HasColumn(r, "WooOrderCount"))
                row.WooOrderCount = r["WooOrderCount"] == DBNull.Value ? 0 : Convert.ToInt32(r["WooOrderCount"]);
            if (DbMapper.HasColumn(r, "QuaffeeLoversMember"))
                row.QuaffeeLoversMember = r["QuaffeeLoversMember"] != DBNull.Value && Convert.ToBoolean(r["QuaffeeLoversMember"]);
            if (DbMapper.HasColumn(r, "QuaffeeLoversCheckedUtc"))
                row.QuaffeeLoversCheckedUtc = r["QuaffeeLoversCheckedUtc"] == DBNull.Value
                    ? (DateTime?)null : Convert.ToDateTime(r["QuaffeeLoversCheckedUtc"]);
            return row;
        }

        private static DBParameter P(string name, object value, DbType type) =>
            new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
    }
}
