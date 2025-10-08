using System;
using System.Web;
using TrackerDotNet.Classes;

namespace TrackerDotNet
{
    public partial class HttpErrorPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    // Set error time
                    lblErrorTime.Text = TimeZoneUtils.Now().ToString("yyyy-MM-dd HH:mm:ss");

                    // Get error message from query string with better validation
                    string errorMessage = Request.QueryString["msg"];

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        // Decode and sanitize the error message
                        errorMessage = Server.UrlDecode(errorMessage);
                        errorMessage = Server.HtmlEncode(errorMessage);

                        // Limit message length to prevent UI issues
                        if (errorMessage.Length > 500)
                        {
                            errorMessage = errorMessage.Substring(0, 497) + "...";
                        }

                        lblErrorMessage.Text = errorMessage;
                    }
                    else
                    {
                        lblErrorMessage.Text = "An unexpected error occurred. Please try again or contact support if the problem persists.";
                    }

                    // Set appropriate HTTP status code
                    Response.StatusCode = 500;
                    Response.StatusDescription = "Internal Server Error";
                }
                catch (Exception ex)
                {
                    // Fallback error handling to prevent infinite loops
                    lblErrorMessage.Text = "A system error occurred.";
                    lblErrorTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // Log the error page error (if possible)
                    try
                    {
                        string logPath = Server.MapPath("~/App_Data/ErrorPageLog.txt");
                        System.IO.File.AppendAllText(logPath,
                            $"[{DateTime.Now}] Error in HttpErrorPage: {ex.Message}\n");
                    }
                    catch
                    {
                        // Silently fail if we can't log
                    }
                }
            }
        }
    }
}