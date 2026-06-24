using System;
using System.Web;

namespace TrackerSQL.Classes
{
    [Serializable]
    public class ApplicationErrorInfo
    {
        public string Summary { get; set; }

        public string LogHint { get; set; }

        public string Source { get; set; }

        public DateTime OccurredAt { get; set; }
    }

    /// <summary>
    /// Cross-request flag for handled errors that were logged but may not have shown on the page.
    /// Site.Master reads this and prompts the user to check App_Data logs.
    /// </summary>
    public static class ApplicationErrorNotifier
    {
        public const string SessionKey = "TrackerSQL_ApplicationError";

        public static void Notify(string summary, string source = null, string logHint = null)
        {
            if (HttpContext.Current == null || string.IsNullOrWhiteSpace(summary))
                return;

            var info = new ApplicationErrorInfo
            {
                Summary = Truncate(summary.Trim(), 300),
                Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
                LogHint = string.IsNullOrWhiteSpace(logHint)
                    ? "App_Data/ErrorLog.txt and App_Data/*.log (or System → Log Viewer)"
                    : logHint.Trim(),
                OccurredAt = TimeZoneUtils.Now()
            };

            HttpContext.Current.Items[SessionKey] = info;

            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session[SessionKey] = info;
        }

        public static void Notify(Exception ex, string source = null, string logHint = null)
        {
            if (ex == null)
                return;

            Exception root = ex.InnerException ?? ex;
            Notify(root.Message, source, logHint);
        }

        public static ApplicationErrorInfo GetPending()
        {
            if (HttpContext.Current == null)
                return null;

            var fromRequest = HttpContext.Current.Items[SessionKey] as ApplicationErrorInfo;
            if (fromRequest != null)
                return fromRequest;

            return HttpContext.Current.Session?[SessionKey] as ApplicationErrorInfo;
        }

        public static void Clear()
        {
            if (HttpContext.Current == null)
                return;

            HttpContext.Current.Items.Remove(SessionKey);

            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session.Remove(SessionKey);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - 3) + "...";
        }
    }
}
