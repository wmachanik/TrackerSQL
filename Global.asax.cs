using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using TrackerSQL.Classes;

namespace TrackerSQL
{
    public class Global : HttpApplication
    {
        private const string StartupLogFileName = "startup.log";

        private void Application_Start(object sender, EventArgs e)
        {
            WriteStartupLog("Application_Start");

            // Force-disable unobtrusive validation
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;
        }

        private void Application_End(object sender, EventArgs e)
        {
            WriteStartupLog("Application_End");
        }

        /// <summary>
        /// Appends a timestamped line to App_Data/startup.log so AppDomain recycles are visible.
        /// Safe with no HttpContext (Application_End).
        /// </summary>
        private static void WriteStartupLog(string eventName)
        {
            try
            {
                string appData = HostingEnvironment.MapPath("~/App_Data/");
                if (string.IsNullOrEmpty(appData))
                    return;
                if (!Directory.Exists(appData))
                    Directory.CreateDirectory(appData);

                string line = string.Format(
                    "[{0:yyyy-MM-dd HH:mm:ss.fff}] {1} | PID={2} | AppDomainId={3} | ShutdownReason={4}",
                    DateTime.Now,
                    eventName,
                    Process.GetCurrentProcess().Id,
                    AppDomain.CurrentDomain.Id,
                    HostingEnvironment.ShutdownReason);

                File.AppendAllText(Path.Combine(appData, StartupLogFileName), line + Environment.NewLine);
            }
            catch
            {
                // Never break app start/stop for logging.
            }
        }

        private void Application_Error(object sender, EventArgs e)
        {
            Exception lastError = Server.GetLastError();
            if (lastError == null)
            {
                return; // No error to log
            }

            try
            {
                // Prevent infinite loops - don't redirect if we're already on the error page
                string currentUrl = Request.Url?.AbsolutePath?.ToLower() ?? "";
                if (currentUrl.Contains("httperrorpage.aspx"))
                {
                    Server.ClearError();
                    return;
                }

                // Log the error with better error handling
                LogError(lastError);
                ApplicationErrorNotifier.Notify(lastError, "Application_Error");

                // Get clean error message
                Exception root = lastError.InnerException ?? lastError;
                string cleanMessage = System.Text.RegularExpressions.Regex.Replace(root.Message, "<.*?>", "");

                // Limit message length to prevent URL issues
                if (cleanMessage.Length > 200)
                {
                    cleanMessage = cleanMessage.Substring(0, 197) + "...";
                }

                // Safer redirect with error handling
                string redirectUrl = "~/HttpErrorPage.aspx?msg=" + HttpUtility.UrlEncode(cleanMessage);
                Response.Redirect(redirectUrl, false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
                Server.ClearError();
            }
            catch (Exception ex)
            {
                try
                {
                    System.Diagnostics.EventLog.WriteEntry(
                        "TrackerSQL",
                        $"Error handling failed: {ex.Message}",
                        System.Diagnostics.EventLogEntryType.Error);
                }
                catch
                {
                }

                Server.ClearError();
                Response.Redirect("~/HttpErrorPage.aspx", false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        private void LogError(Exception error)
        {
            try
            {
                Exception root = error.InnerException ?? error;
                string logEntry = $"[{TimeZoneUtils.Now()}]\n{root.GetType()}: {root.Message}\n{root.StackTrace}\n----------------------\n";

                try
                {
                    string appDataPath = Server.MapPath("~/App_Data/");
                    if (!Directory.Exists(appDataPath))
                    {
                        Directory.CreateDirectory(appDataPath);
                    }

                    string logPath = Path.Combine(appDataPath, "ErrorLog.txt");
                    using (var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(logEntry);
                    }
                    return;
                }
                catch
                {
                }

                try
                {
                    string tempPath = Path.GetTempPath();
                    string logPath = Path.Combine(tempPath, "TrackerSQL_ErrorLog.txt");
                    File.AppendAllText(logPath, logEntry);
                    return;
                }
                catch
                {
                }

                try
                {
                    System.Diagnostics.EventLog.WriteEntry(
                        "TrackerSQL",
                        $"{root.GetType()}: {root.Message}",
                        System.Diagnostics.EventLogEntryType.Error);
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        private void Session_Start(object sender, EventArgs e)
        {
        }

        private void Session_End(object sender, EventArgs e)
        {
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            RequestTiming.BeginRequest(HttpContext.Current);
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            RequestTiming.EndRequest(HttpContext.Current);
        }
    }
}