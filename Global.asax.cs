using System;
using System.IO;
using System.Web;
using System.Web.UI;
using TrackerDotNet.Classes;

namespace TrackerDotNet
{
    public class Global : HttpApplication
    {
        private void Application_Start(object sender, EventArgs e)
        {
            // Force-disable unobtrusive validation
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;
        }

        private void Application_End(object sender, EventArgs e)
        {
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
                // Fallback: if error handling fails, just clear and continue
                try
                {
                    // Try to log the error handling failure
                    System.Diagnostics.EventLog.WriteEntry("TrackerDotNet",
                        $"Error handling failed: {ex.Message}",
                        System.Diagnostics.EventLogEntryType.Error);
                }
                catch
                {
                    // Ultimate fallback - do nothing
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
                // Try multiple logging approaches
                Exception root = error.InnerException ?? error;
                string logEntry = $"[{TimeZoneUtils.Now()}]\n{root.GetType()}: {root.Message}\n{root.StackTrace}\n----------------------\n";

                // Method 1: Try App_Data folder
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
                    return; // Success
                }
                catch
                {
                    // Fall through to next method
                }

                // Method 2: Try Temp folder
                try
                {
                    string tempPath = Path.GetTempPath();
                    string logPath = Path.Combine(tempPath, "TrackerDotNet_ErrorLog.txt");
                    File.AppendAllText(logPath, logEntry);
                    return; // Success
                }
                catch
                {
                    // Fall through to next method
                }

                // Method 3: Try Event Log
                try
                {
                    System.Diagnostics.EventLog.WriteEntry("TrackerDotNet",
                        $"{root.GetType()}: {root.Message}",
                        System.Diagnostics.EventLogEntryType.Error);
                }
                catch
                {
                    // Ultimate fallback - do nothing
                }
            }
            catch
            {
                // Silently fail - don't let logging errors break the app
            }
        }

        private void Session_Start(object sender, EventArgs e)
        {
        }

        private void Session_End(object sender, EventArgs e)
        {
        }
    }
}