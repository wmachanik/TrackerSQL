using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;

namespace TrackerSQL
{
    public partial class SiteMaster : MasterPage
    {
        protected Panel pnlApplicationError;
        protected Label lblApplicationError;
        protected HyperLink lnkViewLogs;
        protected LinkButton btnDismissAppError;
        protected Literal litAppVersionHeader;
        protected Literal litAppVersionFooter;

        protected void Page_Init(object sender, EventArgs e)
        {
            // Avoid <%= %> in <head> — ScriptManager cannot modify a head that contains code blocks.
            if (litUnsavedChangesScript != null)
            {
                litUnsavedChangesScript.Text =
                    "<script type=\"text/javascript\" src=\"" +
                    HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Scripts/unsavedChanges.js?v=20260812-1")) +
                    "\"></script>" +
                    "<script type=\"text/javascript\" src=\"" +
                    HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Scripts/comboBoxPosition.js?v=20260811-3")) +
                    "\"></script>";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();

            BindAppVersionLabels();
            BindApplicationErrorBanner();
            HighlightCurrentMenuItem();

            sw.Stop();
            RequestTiming.Write("MASTER PAGE", sw.ElapsedMilliseconds + " ms");
        }

        /// <summary>
        /// AppSettings are cached by ASP.NET — reading Version each request is negligible.
        /// </summary>
        private void BindAppVersionLabels()
        {
            string version = ConfigHelper.GetString("Version", string.Empty).Trim();
            string display = string.IsNullOrEmpty(version) ? string.Empty : "v" + version;

            if (litAppVersionHeader != null)
                litAppVersionHeader.Text = HttpUtility.HtmlEncode(display);
            if (litAppVersionFooter != null)
                litAppVersionFooter.Text = HttpUtility.HtmlEncode(display);
        }

        private void BindApplicationErrorBanner()
        {
            if (pnlApplicationError == null || lblApplicationError == null)
                return;

            ApplicationErrorInfo error = ApplicationErrorNotifier.GetPending();
            if (error == null)
            {
                pnlApplicationError.Visible = false;
                return;
            }

            pnlApplicationError.Visible = true;

            string sourceText = string.IsNullOrWhiteSpace(error.Source)
                ? string.Empty
                : $" Source: {HttpUtility.HtmlEncode(error.Source)}.";

            lblApplicationError.Text =
                $" At {error.OccurredAt:yyyy-MM-dd HH:mm}.{sourceText} " +
                $"{HttpUtility.HtmlEncode(error.Summary)} " +
                $"Check {HttpUtility.HtmlEncode(error.LogHint)} for details.";
        }

        protected void btnDismissAppError_Click(object sender, EventArgs e)
        {
            ApplicationErrorNotifier.Clear();
            pnlApplicationError.Visible = false;
        }

        private void HighlightCurrentMenuItem()
        {
            string currentPage = Request.AppRelativeCurrentExecutionFilePath.ToUpper();
            bool parentSelected = false;

            foreach (MenuItem menuItem in NavigationMenu.Items)
            {
                string menuUrl = ResolveUrl(menuItem.NavigateUrl).ToUpper();
                if (currentPage == menuUrl)
                {
                    menuItem.Selected = true;
                    parentSelected = true;
                }
                else
                {
                    foreach (MenuItem childItem in menuItem.ChildItems)
                    {
                        string childUrl = ResolveUrl(childItem.NavigateUrl).ToUpper();
                        if (currentPage == childUrl)
                        {
                            childItem.Selected = true;
                            menuItem.Selected = true;
                            parentSelected = true;
                        }
                    }
                }
            }

            if ((currentPage.Contains("DEFAULT.ASPX") || currentPage.EndsWith("/")) && !parentSelected)
                NavigationMenu.Items[0].Selected = true;
        }
    }
}
