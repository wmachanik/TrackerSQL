using System;
using System.IO;
using System.Linq;
using System.Web;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Provides centralized logging to App_Data with log rotation and named log files.
    /// </summary>
    public static class AppLogger
    {
        private const int MaxLines = 5000;

        private static string GetLoggedInUsername()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.User != null &&
                    HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    return HttpContext.Current.User.Identity.Name;
                }
            }
            catch
            {
                // Fallback if user can't be resolved
            }
            return "UnknownUser";
        }
        /// <summary>
        /// Primary log method with user resolution from context.
        /// </summary>
        public static void WriteLog(string logName, string message)
        {
            string user = GetLoggedInUsername();
            WriteLogInternal(logName, message, user);
        }

        /// <summary>
        /// Overload for explicitly passing a username.
        /// </summary>
        public static void WriteLog(string logName, string message, string usernameOverride)
        {
            string user = string.IsNullOrWhiteSpace(usernameOverride) ? GetLoggedInUsername() : usernameOverride;
            WriteLogInternal(logName, message, user);
        }

        /// <summary>
        /// Shared internal method to write to log.
        /// </summary>
        private static void WriteLogInternal(string logName, string message, string user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(logName))
                    logName = "general";

                string logDir = HttpContext.Current?.Server?.MapPath("~/App_Data/")
                                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string filePath = Path.Combine(logDir, $"{logName}.log");

                // Use user-specific timestamp
                string timeStamp = TimeZoneUtils.Now().ToString("yyyy-MM-dd HH:mm:ss");
                //string timeZoneId = TimeZoneUtils.GetZoneId(); -> full name
                string timeZoneId = TimeZoneUtils.GetZoneAbbreviation();

                string entry = $"[{timeStamp}] [User: {user}] [Zone: {timeZoneId}] {message}";

                File.AppendAllText(filePath, entry + Environment.NewLine);
                TrimLogFile(filePath);
            }
            catch
            {
                // Optional fallback
            }
        }

        /// <summary>
        /// Trims the log file to retain only the most recent MaxLines.
        /// </summary>
        private static void TrimLogFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length > MaxLines)
                {
                    var trimmed = lines.Skip(lines.Length - MaxLines).ToArray();
                    File.WriteAllLines(filePath, trimmed);
                }
            }
            catch
            {
                // Optional: suppress trim failure
            }
        }
    }
}
