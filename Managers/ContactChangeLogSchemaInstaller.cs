using System;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    public class ContactChangeLogSchemaInstaller
    {
        public const string XmlFileName = "SQLCommands-ContactChangeLog-01.xml";

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
                result.Message = "Contact change log schema ready (" + ran + " command(s)).";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                AppLogger.WriteLog("system", "Contact change log schema ensure failed: " + ex.Message);
                return result;
            }
        }
    }
}
