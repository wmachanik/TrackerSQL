using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooAreaDeliveryDefaultRepository
    {
        private static bool _postalRangesColumnEnsured;

        private static void EnsurePostalRangesColumn()
        {
            if (_postalRangesColumnEnsured)
                return;
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    db.ExecuteNonQuery(@"
IF OBJECT_ID(N'dbo.WooAreaDeliveryDefaultTbl', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.WooAreaDeliveryDefaultTbl', N'PostalRanges') IS NULL
BEGIN
    ALTER TABLE dbo.WooAreaDeliveryDefaultTbl ADD PostalRanges NVARCHAR(MAX) NULL;
END");
                }
                _postalRangesColumnEnsured = true;
            }
            catch
            {
                // leave flag false so a later call can retry
            }
        }

        public List<WooAreaDeliveryDefault> GetAllWithAreas()
        {
            EnsurePostalRangesColumn();
            const string sql = @"
SELECT a.AreaID, a.AreaName, d.DefaultPreferredAgentID, d.PostalRanges, p.Abbreviation AS DefaultPersonAbbrev
FROM AreasTbl a
LEFT JOIN WooAreaDeliveryDefaultTbl d ON d.AreaID = a.AreaID
LEFT JOIN PeopleTbl p ON p.PersonID = d.DefaultPreferredAgentID
ORDER BY a.AreaName";
            var list = new List<WooAreaDeliveryDefault>();
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new WooAreaDeliveryDefault
                    {
                        AreaID = Convert.ToInt32(rdr["AreaID"]),
                        AreaName = rdr["AreaName"] == DBNull.Value ? string.Empty : rdr["AreaName"].ToString(),
                        DefaultPreferredAgentID = rdr["DefaultPreferredAgentID"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(rdr["DefaultPreferredAgentID"]),
                        PostalRanges = rdr["PostalRanges"] == DBNull.Value ? null : rdr["PostalRanges"].ToString(),
                        DefaultPersonAbbrev = rdr["DefaultPersonAbbrev"] == DBNull.Value
                            ? null
                            : rdr["DefaultPersonAbbrev"].ToString()
                    });
                }
            }
            return list;
        }

        public int? GetDefaultPersonForArea(int areaId)
        {
            EnsurePostalRangesColumn();
            const string sql = "SELECT DefaultPreferredAgentID FROM WooAreaDeliveryDefaultTbl WHERE AreaID = @AreaID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@AreaID", DataValue = areaId, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            {
                object val = db.ExecuteScalar<object>(sql, p);
                if (val == null || val == DBNull.Value)
                    return null;
                return Convert.ToInt32(val);
            }
        }

        public void Upsert(int areaId, int? personId, string postalRanges, string updatedBy)
        {
            EnsurePostalRangesColumn();
            const string sql = @"
IF EXISTS (SELECT 1 FROM WooAreaDeliveryDefaultTbl WHERE AreaID = @AreaID)
    UPDATE WooAreaDeliveryDefaultTbl
    SET DefaultPreferredAgentID = @PersonID,
        PostalRanges = @PostalRanges,
        UpdatedAt = SYSUTCDATETIME(),
        UpdatedBy = @UpdatedBy
    WHERE AreaID = @AreaID;
ELSE
    INSERT INTO WooAreaDeliveryDefaultTbl (AreaID, DefaultPreferredAgentID, PostalRanges, UpdatedAt, UpdatedBy)
    VALUES (@AreaID, @PersonID, @PostalRanges, SYSUTCDATETIME(), @UpdatedBy);";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@AreaID", DataValue = areaId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PersonID", DataValue = (object)personId ?? DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PostalRanges", DataValue = (object)NormaliseRanges(postalRanges) ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@UpdatedBy", DataValue = updatedBy ?? string.Empty, DataDbType = DbType.String }
            };
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(sql, p);
        }

        private static string NormaliseRanges(string postalRanges)
        {
            if (string.IsNullOrWhiteSpace(postalRanges))
                return null;
            // Woo style: 7800...7806. Accept .. or … on input; store as ...
            string text = postalRanges.Trim().Replace("…", "...");
            text = Regex.Replace(text, @"\s*\.\.\.\s*", "...");
            text = Regex.Replace(text, @"(?<!\.)\.\.(?!\.)", "...");
            text = Regex.Replace(text, @"\s*;\s*", ";");
            return text;
        }
    }
}
