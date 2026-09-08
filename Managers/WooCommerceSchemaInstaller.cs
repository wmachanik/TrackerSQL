using System;
using System.IO;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Ensures WooCommerce companion tables exist by running create commands
    /// from App_Data/SQLCommands-WooCommerce-*.xml (same pack as XMLtoSQL).
    /// </summary>
    public class WooCommerceSchemaInstaller
    {
        public const string XmlFileName = "SQLCommands-WooCommerce-01.xml";
        public const string XmlFileNameAreas = "SQLCommands-WooCommerce-02.xml";
        public const string XmlFileNameAddress = "SQLCommands-WooCommerce-03.xml";
        public const string XmlFileNameGeneral = "SQLCommands-WooCommerce-04.xml";

        public static readonly string[] XmlFileNames =
        {
            XmlFileName,
            XmlFileNameAreas,
            XmlFileNameAddress,
            XmlFileNameGeneral
        };

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
                int ran = XmlSchemaCommandRunner.RunFiles(XmlFileNames, out string error);
                if (!string.IsNullOrEmpty(error))
                {
                    result.Succeeded = false;
                    result.Message = error;
                    AppLogger.WriteLog("woo", "Schema ensure failed — " + error);
                    return result;
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
    }
}
