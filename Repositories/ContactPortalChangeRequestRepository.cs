using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactPortalChangeRequestRepository
    {
        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'ContactPortalChangeRequestTbl'") > 0;
            }
            catch
            {
                return false;
            }
        }

        public long Insert(ContactPortalChangeRequest req)
        {
            if (req == null || req.ContactID <= 0 || string.IsNullOrWhiteSpace(req.RequestText) || !TableExists())
                return 0;

            const string sql = @"
INSERT INTO ContactPortalChangeRequestTbl
    (ContactID, UserId, Kind, RelatedId, RequestText, Status, CreatedAt)
OUTPUT INSERTED.RequestID
VALUES
    (@ContactID, @UserId, @Kind, @RelatedId, @RequestText, @Status, GETDATE())";

            using (var db = new TrackerSQLDb())
            {
                object id = db.ExecuteScalar<object>(sql, new List<DBParameter>
                {
                    new DBParameter { ParamName = "@ContactID", DataValue = req.ContactID, DataDbType = DbType.Int32 },
                    new DBParameter
                    {
                        ParamName = "@UserId",
                        DataValue = req.UserId.HasValue ? (object)req.UserId.Value : DBNull.Value,
                        DataDbType = DbType.Guid
                    },
                    new DBParameter { ParamName = "@Kind", DataValue = req.Kind ?? ContactPortalChangeKinds.Contact, DataDbType = DbType.String },
                    new DBParameter
                    {
                        ParamName = "@RelatedId",
                        DataValue = req.RelatedId.HasValue ? (object)req.RelatedId.Value : DBNull.Value,
                        DataDbType = DbType.Int64
                    },
                    new DBParameter { ParamName = "@RequestText", DataValue = req.RequestText.Trim(), DataDbType = DbType.String },
                    new DBParameter { ParamName = "@Status", DataValue = ContactPortalChangeStatuses.Open, DataDbType = DbType.String }
                });
                return id == null || id == DBNull.Value ? 0 : Convert.ToInt64(id);
            }
        }

        public List<ContactPortalChangeRequest> ListByStatus(string status, int maxRows = 100)
        {
            var list = new List<ContactPortalChangeRequest>();
            if (!TableExists())
                return list;

            string sql = @"
SELECT TOP (@Max) r.RequestID, r.ContactID, r.UserId, r.Kind, r.RelatedId, r.RequestText,
       r.Status, r.CreatedAt, r.ResolvedAt, r.ResolvedBy, r.StaffNote,
       ISNULL(c.CompanyName, '') AS CompanyName
FROM ContactPortalChangeRequestTbl r
LEFT JOIN ContactsTbl c ON c.ContactID = r.ContactID
WHERE (@Status = N'' OR r.Status = @Status)
ORDER BY r.CreatedAt DESC";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, new List<DBParameter>
            {
                new DBParameter { ParamName = "@Max", DataValue = Math.Max(1, maxRows), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Status", DataValue = status ?? string.Empty, DataDbType = DbType.String }
            }))
            {
                while (rdr != null && rdr.Read())
                    list.Add(Map(rdr));
            }
            return list;
        }

        public ContactPortalChangeRequest GetById(long requestId)
        {
            if (requestId <= 0 || !TableExists())
                return null;

            const string sql = @"
SELECT r.RequestID, r.ContactID, r.UserId, r.Kind, r.RelatedId, r.RequestText,
       r.Status, r.CreatedAt, r.ResolvedAt, r.ResolvedBy, r.StaffNote,
       ISNULL(c.CompanyName, '') AS CompanyName
FROM ContactPortalChangeRequestTbl r
LEFT JOIN ContactsTbl c ON c.ContactID = r.ContactID
WHERE r.RequestID = @RequestID";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, new List<DBParameter>
            {
                new DBParameter { ParamName = "@RequestID", DataValue = requestId, DataDbType = DbType.Int64 }
            }))
            {
                return rdr != null && rdr.Read() ? Map(rdr) : null;
            }
        }

        public bool Resolve(long requestId, string status, string resolvedBy, string staffNote)
        {
            if (requestId <= 0 || !TableExists())
                return false;
            if (status != ContactPortalChangeStatuses.Done && status != ContactPortalChangeStatuses.Rejected)
                return false;

            const string sql = @"
UPDATE ContactPortalChangeRequestTbl
SET Status = @Status, ResolvedAt = GETDATE(), ResolvedBy = @ResolvedBy, StaffNote = @StaffNote
WHERE RequestID = @RequestID AND Status = N'Open'";

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Status", DataValue = status, DataDbType = DbType.String },
                    new DBParameter { ParamName = "@ResolvedBy", DataValue = (object)resolvedBy ?? DBNull.Value, DataDbType = DbType.String },
                    new DBParameter { ParamName = "@StaffNote", DataValue = (object)staffNote ?? DBNull.Value, DataDbType = DbType.String },
                    new DBParameter { ParamName = "@RequestID", DataValue = requestId, DataDbType = DbType.Int64 }
                }) > 0;
            }
        }

        private static ContactPortalChangeRequest Map(IDataRecord r)
        {
            return new ContactPortalChangeRequest
            {
                RequestID = Convert.ToInt64(r["RequestID"]),
                ContactID = Convert.ToInt32(r["ContactID"]),
                UserId = r["UserId"] == DBNull.Value ? (Guid?)null : (Guid)r["UserId"],
                Kind = r["Kind"] == DBNull.Value ? string.Empty : r["Kind"].ToString(),
                RelatedId = r["RelatedId"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["RelatedId"]),
                RequestText = r["RequestText"] == DBNull.Value ? string.Empty : r["RequestText"].ToString(),
                Status = r["Status"] == DBNull.Value ? string.Empty : r["Status"].ToString(),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                ResolvedAt = r["ResolvedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["ResolvedAt"]),
                ResolvedBy = r["ResolvedBy"] == DBNull.Value ? null : r["ResolvedBy"].ToString(),
                StaffNote = r["StaffNote"] == DBNull.Value ? null : r["StaffNote"].ToString(),
                CompanyName = HasCol(r, "CompanyName") && r["CompanyName"] != DBNull.Value
                    ? r["CompanyName"].ToString()
                    : null
            };
        }

        private static bool HasCol(IDataRecord r, string name)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
