using System;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>Creates the mobile REST API tables (SQLCommands-MobileApi-01.xml) once per application start.</summary>
    public static class MobileApiSchemaInstaller
    {
        public const string XmlFileName = "SQLCommands-MobileApi-01.xml";

        private static readonly object Sync = new object();
        private static bool _ready;

        public static void EnsureReady()
        {
            if (_ready)
                return;

            lock (Sync)
            {
                if (_ready)
                    return;

                string message = Run();
                if (message != null)
                    throw new InvalidOperationException("Mobile API schema could not be created: " + message);
                _ready = true;
            }
        }

        /// <summary>Runs the schema pack now. Returns null on success, otherwise the error.</summary>
        public static string Run()
        {
            try
            {
                XmlSchemaCommandRunner.RunFiles(new[] { XmlFileName }, out string error);
                if (!string.IsNullOrEmpty(error))
                    return error;
                _ready = true;
                return null;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Mobile API schema ensure failed: " + ex.Message);
                return ex.Message;
            }
        }
    }
}
