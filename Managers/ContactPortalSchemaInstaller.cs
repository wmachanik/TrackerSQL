using System;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    public class ContactPortalSchemaInstaller
    {
        public const string XmlFileName = "SQLCommands-ContactPortal-01.xml";
        public const string ContactRoleName = "Contact";

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

                EnsureContactRole();

                result.Succeeded = true;
                result.CommandsRun = ran;
                result.Message = "Contact portal schema ready (" + ran + " command(s)).";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "Contact portal schema ensure failed: " + ex.Message);
                return result;
            }
        }

        /// <summary>Creates the Contact role when missing; throws if it cannot be created.</summary>
        public static void EnsureContactRole()
        {
            try
            {
                MembershipRoleManager.EnsureRoleExists(ContactRoleName, "ContactPortal");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "Ensure Contact role failed: " + ex.Message);
                throw;
            }
        }
    }
}
