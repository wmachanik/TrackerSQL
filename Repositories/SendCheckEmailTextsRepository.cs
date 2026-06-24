using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class SendCheckEmailTextsRepository
    {
        public SendCheckEmailTexts GetTexts()
        {
            const string sql = "SELECT SCEMTID, Header, Body, Footer, DateLastChange, Notes FROM SendCheckEmailTextsTbl";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                if (rdr != null && rdr.Read())
                {
                    return Map(rdr);
                }
            }

            return new SendCheckEmailTexts();
        }

        public string UpdateTexts(SendCheckEmailTexts emailTexts, int originalId)
        {
            const string sql = @"
                UPDATE SendCheckEmailTextsTbl
                SET Header = @Header, Body = @Body, Footer = @Footer,
                    DateLastChange = @DateLastChange, Notes = @Notes
                WHERE SCEMTID = @SCEMTID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Header", DataValue = emailTexts.Header ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Body", DataValue = emailTexts.Body ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Footer", DataValue = emailTexts.Footer ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@DateLastChange", DataValue = TimeZoneUtils.Now().Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@Notes", DataValue = emailTexts.Notes ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SCEMTID", DataValue = originalId, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters) > 0 ? string.Empty : "Update failed";
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
