using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Ensures WooCommerce companion tables exist by running create commands
    /// from App_Data/SQLCommands-WooCommerce-01.xml (same pack as XMLtoSQL).
    /// </summary>
    public class WooCommerceSchemaInstaller
    {
        public const string XmlFileName = "SQLCommands-WooCommerce-01.xml";

        public class EnsureResult
        {
            public bool Succeeded { get; set; }
            public int CommandsRun { get; set; }
            public string Message { get; set; }
        }

        public EnsureResult EnsureSchema()
        {
            var result = new EnsureResult();
            try
            {
                string path = ResolveXmlPath();
                if (!File.Exists(path))
                {
                    result.Succeeded = false;
                    result.Message = "Schema XML not found: " + XmlFileName;
                    AppLogger.WriteLog("woo", "Schema ensure failed — file missing: " + path);
                    return result;
                }

                var statements = LoadSchemaCommands(path);
                int ran = 0;
                using (var db = new TrackerSQLDb())
                {
                    foreach (string sql in statements)
                    {
                        db.ExecuteNonQuery(sql);
                        ran++;
                    }
                }

                result.Succeeded = true;
                result.CommandsRun = ran;
                result.Message = "Schema ensure completed (" + ran + " create/alter commands).";
                AppLogger.WriteLog("woo", result.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = ex.Message;
                AppLogger.WriteLog("woo", "Schema ensure failed: " + ex.Message);
                return result;
            }
        }

        public bool TablesExist()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    int n = db.ExecuteScalar<int>(@"
                        SELECT COUNT(*) FROM sys.tables
                        WHERE name IN (N'SystemPreferencesHdrTbl', N'WooCommerceSettingsTbl')");
                    return n >= 2;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string ResolveXmlPath()
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Server.MapPath("~/App_Data/" + XmlFileName);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", XmlFileName);
        }

        /// <summary>
        /// Runs create + alter in document order (alters add/drop columns on existing tables).
        /// </summary>
        private static List<string> LoadSchemaCommands(string path)
        {
            var list = new List<string>();
            var doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList nodes = doc.SelectNodes("//command[@type='create' or @type='alter']");
            if (nodes == null)
                return list;

            foreach (XmlNode node in nodes)
            {
                string sql = (node.InnerText ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(sql))
                    list.Add(sql);
            }
            return list;
        }
    }
}
