using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactChangeLogRepository
    {
        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'ContactChangeLogTbl'") > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool Insert(ContactChangeLogEntry entry)
        {
            if (entry == null || entry.ContactID <= 0 || string.IsNullOrWhiteSpace(entry.Source))
                return false;
            if (!TableExists())
                return false;

            const string sql = @"
INSERT INTO ContactChangeLogTbl
    (ContactID, ChangedAt, ChangedBy, Source, Summary, FieldName, OldValue, NewValue)
VALUES
    (@ContactID, @ChangedAt, @ChangedBy, @Source, @Summary, @FieldName, @OldValue, @NewValue)";

            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = entry.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ChangedAt", DataValue = entry.ChangedAt == default ? TimeZoneUtils.Now() : entry.ChangedAt, DataDbType = DbType.DateTime },
                new DBParameter { ParamName = "@ChangedBy", DataValue = (object)entry.ChangedBy ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Source", DataValue = entry.Source.Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@Summary", DataValue = (object)Truncate(entry.Summary, 500) ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@FieldName", DataValue = (object)Truncate(entry.FieldName, 64) ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@OldValue", DataValue = (object)entry.OldValue ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@NewValue", DataValue = (object)entry.NewValue ?? DBNull.Value, DataDbType = DbType.String }
            };

            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteNonQuery(sql, p) > 0;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("system", "ContactChangeLog insert failed: " + ex.Message);
                return false;
            }
        }

        public int InsertMany(IEnumerable<ContactChangeLogEntry> entries)
        {
            if (entries == null)
                return 0;
            int n = 0;
            foreach (var e in entries)
            {
                if (Insert(e))
                    n++;
            }
            return n;
        }

        public List<ContactChangeLogEntry> GetByContactId(int contactId, int maxRows = 200)
        {
            var list = new List<ContactChangeLogEntry>();
            if (contactId <= 0 || !TableExists())
                return list;

            if (maxRows <= 0)
                maxRows = 200;

            const string sql = @"
SELECT TOP (@MaxRows)
    ChangeLogID, ContactID, ChangedAt, ChangedBy, Source, Summary, FieldName, OldValue, NewValue
FROM ContactChangeLogTbl
WHERE ContactID = @ContactID
ORDER BY ChangedAt DESC, ChangeLogID DESC";

            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@MaxRows", DataValue = maxRows, DataDbType = DbType.Int32 }
            };

            try
            {
                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader(sql, p))
                {
                    while (rdr.Read())
                    {
                        list.Add(new ContactChangeLogEntry
                        {
                            ChangeLogID = Convert.ToInt64(rdr["ChangeLogID"]),
                            ContactID = Convert.ToInt32(rdr["ContactID"]),
                            ChangedAt = Convert.ToDateTime(rdr["ChangedAt"]),
                            ChangedBy = rdr["ChangedBy"] as string,
                            Source = rdr["Source"] as string,
                            Summary = rdr["Summary"] as string,
                            FieldName = rdr["FieldName"] as string,
                            OldValue = rdr["OldValue"] as string,
                            NewValue = rdr["NewValue"] as string
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("system", "ContactChangeLog read failed: " + ex.Message);
            }

            return list;
        }

        private static string Truncate(string value, int maxLen)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            value = value.Trim();
            if (value.Length <= maxLen)
                return value;
            return value.Substring(0, maxLen - 1) + "…";
        }
    }
}
