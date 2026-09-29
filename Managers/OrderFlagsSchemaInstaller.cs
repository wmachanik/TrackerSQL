using System;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Adds the 3.0.3.1 order flag columns (SQLCommands-OrderFlags-01.xml). Run from Application_Start
    /// because OrdersTbl.InvoiceTypeID is read by most order pages.
    /// </summary>
    public static class OrderFlagsSchemaInstaller
    {
        public const string XmlFileName = "SQLCommands-OrderFlags-01.xml";

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
                    throw new InvalidOperationException("Order flag schema could not be created: " + message);
            }
        }

        /// <summary>Runs the schema pack now. Returns null on success, otherwise the error.</summary>
        public static string Run()
        {
            try
            {
                int ran = XmlSchemaCommandRunner.RunFiles(new[] { XmlFileName }, out string error);
                if (!string.IsNullOrEmpty(error))
                    return error;
                _ready = true;
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Order flag schema ready (" + ran + " command(s)).");
                return null;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Order flag schema ensure failed: " + ex.Message);
                return ex.Message;
            }
        }
    }
}
