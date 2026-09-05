using TrackerSQL.Classes;

namespace TrackerSQL.Classes
{
    /// <summary>User-facing WooCommerce actions → App_Data/WooCom.log</summary>
    public static class WooCommerceUserLog
    {
        public const string LogName = "WooCom";

        public static void Write(string action, string detail = null, string user = null)
        {
            string msg = string.IsNullOrWhiteSpace(detail) ? action : action + " — " + detail;
            if (string.IsNullOrWhiteSpace(user))
                AppLogger.WriteLog(LogName, msg);
            else
                AppLogger.WriteLog(LogName, msg, user);
        }
    }
}
