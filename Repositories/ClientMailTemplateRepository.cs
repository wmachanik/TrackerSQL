using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ClientMailTemplateRepository
    {
        public const string XmlFileName = "SQLCommands-ClientMail-01.xml";

        public void EnsureTable()
        {
            XmlSchemaCommandRunner.RunFiles(new[] { XmlFileName }, out _);
        }

        public List<ClientMailTemplate> GetAll()
        {
            EnsureTable();
            var list = new List<ClientMailTemplate>();
            const string sql = @"
SELECT ClientMailTemplateID, TemplateName, Subject, HeaderText, BodyText, FooterText, DateLastChange
FROM ClientMailTemplatesTbl
ORDER BY TemplateName";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(DbMapper.Map<ClientMailTemplate>(rdr));
            }
            return list;
        }

        public ClientMailTemplate GetById(int id)
        {
            EnsureTable();
            const string sql = @"
SELECT ClientMailTemplateID, TemplateName, Subject, HeaderText, BodyText, FooterText, DateLastChange
FROM ClientMailTemplatesTbl
WHERE ClientMailTemplateID = @Id";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Id", DataValue = id, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                    return DbMapper.Map<ClientMailTemplate>(rdr);
            }
            return null;
        }

        public int Insert(ClientMailTemplate template)
        {
            EnsureTable();
            const string sql = @"
INSERT INTO ClientMailTemplatesTbl (TemplateName, Subject, HeaderText, BodyText, FooterText, DateLastChange)
VALUES (@TemplateName, @Subject, @HeaderText, @BodyText, @FooterText, @DateLastChange);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var db = new TrackerSQLDb())
            {
                object id = db.ExecuteScalar(sql, Params(template, includeId: false));
                return Convert.ToInt32(id);
            }
        }

        public bool Update(ClientMailTemplate template)
        {
            EnsureTable();
            const string sql = @"
UPDATE ClientMailTemplatesTbl
SET TemplateName = @TemplateName,
    Subject = @Subject,
    HeaderText = @HeaderText,
    BodyText = @BodyText,
    FooterText = @FooterText,
    DateLastChange = @DateLastChange
WHERE ClientMailTemplateID = @ClientMailTemplateID";
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, Params(template, includeId: true)) > 0;
            }
        }

        private static List<DBParameter> Params(ClientMailTemplate template, bool includeId)
        {
            var list = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TemplateName", DataValue = (template.TemplateName ?? string.Empty).Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@Subject", DataValue = template.Subject ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@HeaderText", DataValue = template.HeaderText ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BodyText", DataValue = template.BodyText ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@FooterText", DataValue = template.FooterText ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@DateLastChange", DataValue = TimeZoneUtils.Now(), DataDbType = DbType.DateTime }
            };
            if (includeId)
            {
                list.Add(new DBParameter
                {
                    ParamName = "@ClientMailTemplateID",
                    DataValue = template.ClientMailTemplateID,
                    DataDbType = DbType.Int32
                });
            }
            return list;
        }
    }
}
