using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Runs create/alter commands from App_Data SQLCommands-*.xml packs (XMLtoSQL format).
    /// </summary>
    public static class XmlSchemaCommandRunner
    {
        public static string ResolveXmlPath(string fileName)
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Server.MapPath("~/App_Data/" + fileName);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", fileName);
        }

        public static List<string> LoadSchemaCommands(string path)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return list;

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

        /// <summary>Execute all create/alter commands from the given XML file names under App_Data.</summary>
        public static int RunFiles(IEnumerable<string> fileNames, out string error)
        {
            error = null;
            if (fileNames == null)
                return 0;

            int ran = 0;
            using (var db = new TrackerSQLDb())
            {
                foreach (string file in fileNames)
                {
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    string path = ResolveXmlPath(file);
                    if (!File.Exists(path))
                    {
                        error = "Schema XML not found: " + file;
                        return ran;
                    }

                    foreach (string sql in LoadSchemaCommands(path))
                    {
                        db.ExecuteNonQuery(sql);
                        ran++;
                    }
                }
            }
            return ran;
        }
    }
}
