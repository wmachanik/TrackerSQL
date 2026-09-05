using System;

using System.Collections.Generic;

using System.IO;

using System.Web;

using System.Xml;

using TrackerSQL.Classes;



namespace TrackerSQL.Managers

{

    public class PostalSchemaInstaller

    {

        public const string XmlFileName = "SQLCommands-Postal-01.xml";



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

                string path = ResolveXmlPath(XmlFileName);

                if (!File.Exists(path))

                {

                    result.Message = "Schema XML not found: " + XmlFileName;

                    return result;

                }



                int ran = 0;

                using (var db = new TrackerSQLDb())

                {

                    foreach (string sql in LoadSchemaCommands(path))

                    {

                        db.ExecuteNonQuery(sql);

                        ran++;

                    }

                }



                result.Succeeded = true;

                result.CommandsRun = ran;

                result.Message = "Postal schema ready (" + ran + " command(s)).";

                AppLogger.WriteLog("system", result.Message);

                return result;

            }

            catch (Exception ex)

            {

                result.Message = ex.Message;

                AppLogger.WriteLog("system", "Postal schema ensure failed: " + ex.Message);

                return result;

            }

        }



        private static string ResolveXmlPath(string fileName)

        {

            if (HttpContext.Current != null)

                return HttpContext.Current.Server.MapPath("~/App_Data/" + fileName);

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", fileName);

        }



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

