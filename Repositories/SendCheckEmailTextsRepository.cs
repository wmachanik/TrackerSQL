using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class SendCheckEmailTextsRepository
    {
        private sealed class TextColumns
        {
            public string Header;
            public string Body;
            public string Footer;
        }

        private static readonly Lazy<TextColumns> Columns = new Lazy<TextColumns>(DetectColumns);

        public SendCheckEmailTexts GetTexts()
        {
            TextColumns cols = Columns.Value;
            string sql = "SELECT TOP (1) SCEMTID, " + cols.Header + " AS Header, " + cols.Body + " AS Body, "
                + cols.Footer + " AS Footer, DateLastChange, Notes FROM SendCheckupEmailTextsTbl ORDER BY SCEMTID";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                if (rdr != null && rdr.Read())
                    return Map(rdr);
            }

            return new SendCheckEmailTexts();
        }

        public string UpdateTexts(SendCheckEmailTexts emailTexts, int preferredId)
        {
            string idError = ResolveTemplateId(preferredId, out int templateId);
            if (idError != null)
                return idError;
            if (templateId <= 0)
                return "No email template row exists in SendCheckupEmailTextsTbl.";

            TextColumns cols = Columns.Value;
            string sql = @"
                UPDATE SendCheckupEmailTextsTbl
                SET " + cols.Header + " = @Header, " + cols.Body + " = @Body, " + cols.Footer + " = @Footer, "
                + @"DateLastChange = @DateLastChange, Notes = @Notes
                WHERE SCEMTID = @SCEMTID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Header", DataValue = emailTexts.Header ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Body", DataValue = emailTexts.Body ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Footer", DataValue = emailTexts.Footer ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@DateLastChange", DataValue = TimeZoneUtils.Now().Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@Notes", DataValue = emailTexts.Notes ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SCEMTID", DataValue = templateId, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters) > 0 ? string.Empty : "Update failed — no row was updated.";
            }
        }

        /// <summary>SendCheckupEmailTextsTbl is a singleton; duplicates block a safe update.</summary>
        private static string ResolveTemplateId(int preferredId, out int templateId)
        {
            templateId = 0;
            var ids = new List<int>();
            using (var db = new TrackerSQLDb())
            {
                DataTable dt = db.ReturnDataTable("SELECT SCEMTID FROM SendCheckupEmailTextsTbl ORDER BY SCEMTID");
                foreach (DataRow row in dt.Rows)
                    ids.Add(Convert.ToInt32(row["SCEMTID"]));
            }

            if (ids.Count == 0)
                return null;

            if (ids.Count == 1)
            {
                templateId = ids[0];
                return null;
            }

            if (preferredId > 0 && ids.Contains(preferredId))
            {
                templateId = preferredId;
                return null;
            }

            return "SendCheckupEmailTextsTbl should contain only one template row, but it has " + ids.Count
                + " (IDs: " + string.Join(", ", ids) + "). Delete the duplicate rows in SQL Server, keeping a single SCEMTID.";
        }

        private static TextColumns DetectColumns()
        {
            bool headerText = ColumnExists("HeaderText");
            bool header = ColumnExists("Header");
            if (!headerText && !header)
                throw new InvalidOperationException("SendCheckupEmailTextsTbl has no Header or HeaderText column.");

            return new TextColumns
            {
                Header = headerText ? "HeaderText" : "Header",
                Body = ColumnExists("BodyText") ? "BodyText" : "Body",
                Footer = ColumnExists("FooterText") ? "FooterText" : "Footer"
            };
        }

        private static bool ColumnExists(string columnName)
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    object o = db.ExecuteScalar(
                        "SELECT CASE WHEN COL_LENGTH('dbo.SendCheckupEmailTextsTbl', @Col) IS NULL THEN 0 ELSE 1 END",
                        new List<DBParameter>
                        {
                            new DBParameter { ParamName = "@Col", DataValue = columnName, DataDbType = DbType.String }
                        });
                    return Convert.ToInt32(o) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private static SendCheckEmailTexts Map(IDataReader rdr)
        {
            return new SendCheckEmailTexts
            {
                SCEMTID = rdr["SCEMTID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SCEMTID"]),
                Header = rdr["Header"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["Header"]),
                Body = rdr["Body"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["Body"]),
                Footer = rdr["Footer"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["Footer"]),
                DateLastChange = rdr["DateLastChange"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(rdr["DateLastChange"]).Date,
                Notes = rdr["Notes"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["Notes"])
            };
        }
    }
}
