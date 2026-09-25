using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactUserLinkRepository
    {
        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'ContactUserLinkTbl'") > 0;
            }
            catch
            {
                return false;
            }
        }

        public ContactUserLink GetByUserId(Guid userId)
        {
            if (userId == Guid.Empty || !TableExists())
                return null;

            const string sql = @"
SELECT LinkID, UserId, ContactID, MustChangePassword, CreatedAt, LastLoginAt
FROM ContactUserLinkTbl WHERE UserId = @UserId";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, new List<DBParameter>
            {
                new DBParameter { ParamName = "@UserId", DataValue = userId, DataDbType = DbType.Guid }
            }))
            {
                if (rdr == null || !rdr.Read())
                    return null;
                return Map(rdr);
            }
        }

        public ContactUserLink GetByContactId(int contactId)
        {
            if (contactId <= 0 || !TableExists())
                return null;

            const string sql = @"
SELECT TOP 1 LinkID, UserId, ContactID, MustChangePassword, CreatedAt, LastLoginAt
FROM ContactUserLinkTbl WHERE ContactID = @ContactID ORDER BY LinkID";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            }))
            {
                if (rdr == null || !rdr.Read())
                    return null;
                return Map(rdr);
            }
        }

        public bool Upsert(Guid userId, int contactId, bool mustChangePassword)
        {
            if (userId == Guid.Empty || contactId <= 0 || !TableExists())
                return false;

            var existing = GetByUserId(userId);
            if (existing != null)
            {
                const string upd = @"
UPDATE ContactUserLinkTbl
SET ContactID = @ContactID, MustChangePassword = @MustChangePassword
WHERE UserId = @UserId";
                using (var db = new TrackerSQLDb())
                {
                    return db.ExecuteNonQuery(upd, new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 },
                        new DBParameter { ParamName = "@MustChangePassword", DataValue = mustChangePassword, DataDbType = DbType.Boolean },
                        new DBParameter { ParamName = "@UserId", DataValue = userId, DataDbType = DbType.Guid }
                    }) > 0;
                }
            }

            const string ins = @"
INSERT INTO ContactUserLinkTbl (UserId, ContactID, MustChangePassword)
VALUES (@UserId, @ContactID, @MustChangePassword)";
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(ins, new List<DBParameter>
                {
                    new DBParameter { ParamName = "@UserId", DataValue = userId, DataDbType = DbType.Guid },
                    new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 },
                    new DBParameter { ParamName = "@MustChangePassword", DataValue = mustChangePassword, DataDbType = DbType.Boolean }
                }) > 0;
            }
        }

        public bool SetMustChangePassword(Guid userId, bool mustChange)
        {
            if (userId == Guid.Empty || !TableExists())
                return false;

            const string sql = @"
UPDATE ContactUserLinkTbl SET MustChangePassword = @MustChangePassword WHERE UserId = @UserId";
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, new List<DBParameter>
                {
                    new DBParameter { ParamName = "@MustChangePassword", DataValue = mustChange, DataDbType = DbType.Boolean },
                    new DBParameter { ParamName = "@UserId", DataValue = userId, DataDbType = DbType.Guid }
                }) > 0;
            }
        }

        public bool TouchLastLogin(Guid userId)
        {
            if (userId == Guid.Empty || !TableExists())
                return false;

            const string sql = @"
UPDATE ContactUserLinkTbl SET LastLoginAt = GETDATE() WHERE UserId = @UserId";
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, new List<DBParameter>
                {
                    new DBParameter { ParamName = "@UserId", DataValue = userId, DataDbType = DbType.Guid }
                }) >= 0;
            }
        }

        private static ContactUserLink Map(IDataRecord r)
        {
            return new ContactUserLink
            {
                LinkID = Convert.ToInt32(r["LinkID"]),
                UserId = (Guid)r["UserId"],
                ContactID = Convert.ToInt32(r["ContactID"]),
                MustChangePassword = r["MustChangePassword"] != DBNull.Value && Convert.ToBoolean(r["MustChangePassword"]),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                LastLoginAt = r["LastLoginAt"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(r["LastLoginAt"])
            };
        }
    }
}
