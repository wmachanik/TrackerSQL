using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Lightweight request/page timing for slow-load diagnosis.
    /// Writes to Debug + App_Data/timing.log when EnableRequestTiming=true.
    /// </summary>
    public static class RequestTiming
    {
        public const string StopwatchItemKey = "TrackerSQL.RequestStopwatch";
        private const string TimingLogFileName = "timing.log";

        public static bool IsEnabled
        {
            get
            {
                string raw = ConfigurationManager.AppSettings["EnableRequestTiming"];
                if (string.IsNullOrWhiteSpace(raw))
                    return false; // off unless explicitly enabled
                return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                    || raw == "1";
            }
        }

        public static void BeginRequest(HttpContext context)
        {
            if (!IsEnabled || context == null)
                return;
            context.Items[StopwatchItemKey] = Stopwatch.StartNew();
        }

        public static void EndRequest(HttpContext context)
        {
            if (!IsEnabled || context == null)
                return;

            var sw = context.Items[StopwatchItemKey] as Stopwatch;
            if (sw == null)
                return;

            sw.Stop();
            string path = "(unknown)";
            try
            {
                path = context.Request?.RawUrl ?? context.Request?.Url?.PathAndQuery ?? path;
            }
            catch
            {
            }

            // Skip noisy static assets unless very slow.
            if (IsStaticAsset(path) && sw.ElapsedMilliseconds < 1000)
                return;

            Write("REQUEST", path + " => " + sw.ElapsedMilliseconds + " ms");
        }

        public static void Write(string scope, string message)
        {
            if (!IsEnabled)
                return;

            string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] "
                + scope + ": " + message;

            try
            {
                Debug.WriteLine(line);
            }
            catch
            {
            }

            try
            {
                string appData = HttpContext.Current != null
                    ? HttpContext.Current.Server.MapPath("~/App_Data/")
                    : null;
                if (string.IsNullOrEmpty(appData))
                    return;
                if (!Directory.Exists(appData))
                    Directory.CreateDirectory(appData);

                string logPath = Path.Combine(appData, TimingLogFileName);
                using (var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(line);
                }
            }
            catch
            {
            }
        }

        private static bool IsStaticAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            string lower = path.ToLowerInvariant();
            int q = lower.IndexOf('?');
            if (q >= 0)
                lower = lower.Substring(0, q);
            return lower.EndsWith(".css")
                || lower.EndsWith(".js")
                || lower.EndsWith(".gif")
                || lower.EndsWith(".png")
                || lower.EndsWith(".jpg")
                || lower.EndsWith(".jpeg")
                || lower.EndsWith(".ico")
                || lower.EndsWith(".woff")
                || lower.EndsWith(".woff2")
                || lower.EndsWith(".map");
        }
    }
}
