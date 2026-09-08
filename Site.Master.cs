using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

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
            bool wooOn = IsWooIntegrationEnabledCached();
            SyncWooMappingMenuItem(wooOn);
            SyncWooOrderImportMenuItem(wooOn);
            HighlightCurrentMenuItem();

            sw.Stop();
            RequestTiming.Write("MASTER PAGE", sw.ElapsedMilliseconds + " ms");
        }

        private static bool IsWooIntegrationEnabledCached()
        {
            try
            {
                return new WooCommerceSettingsManager().IsIntegrationEnabled();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Hide Woo Mapping until WooCommerce integration is enabled.</summary>
        private void SyncWooMappingMenuItem(bool wooOn)
        {
            if (NavigationMenu == null)
                return;

            MenuItem item = FindMenuItemByValue(NavigationMenu.Items, "WooMapping");
            if (item == null)
                return;

            if (wooOn)
            {
                item.Enabled = true;
                item.NavigateUrl = "~/Tools/WooCommerceMapping.aspx";
                item.ToolTip = string.Empty;
            }
            else
            {
                // Keep visible but send to Preferences wizard instead of Mapping.
                item.Enabled = true;
                item.NavigateUrl = "~/Tools/SystemPreferences.aspx?section=woo&wizard=1";
                item.ToolTip = "Enable WooCommerce integration first (setup wizard)";
            }
        }

        /// <summary>Hide Woo Order Import under Orders until WooCommerce integration is enabled.</summary>
        private void SyncWooOrderImportMenuItem(bool wooOn)
        {
            if (NavigationMenu == null)
                return;

            MenuItem ordersMenu = FindMenuItemByValue(NavigationMenu.Items, "Orders");
            if (ordersMenu == null)
                return;

            MenuItem item = FindMenuItemByValue(ordersMenu.ChildItems, "WooOrderImport");
            if (item == null)
                return;

            if (!wooOn)
                ordersMenu.ChildItems.Remove(item);
        }

        private static MenuItem FindMenuItemByValue(MenuItemCollection items, string value)
        {
            if (items == null)
                return null;
            foreach (MenuItem item in items)
            {
                if (string.Equals(item.Value, value, StringComparison.OrdinalIgnoreCase))
                    return item;
                MenuItem nested = FindMenuItemByValue(item.ChildItems, value);
                if (nested != null)
                    return nested;
            }
            return null;
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
