using System;
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
                int ran = XmlSchemaCommandRunner.RunFiles(new[] { XmlFileName }, out string error);
                if (!string.IsNullOrEmpty(error))
                {
                    result.Message = error;
                    return result;
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
    }
}
