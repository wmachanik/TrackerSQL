using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
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

            // Contact-portal users stay in Portal/ — never the staff shell.
            if (Request.IsAuthenticated
                && ContactPortalManager.IsContactRole(Context.User?.Identity?.Name))
            {
                string path = Request.AppRelativeCurrentExecutionFilePath ?? string.Empty;
                if (!path.StartsWith("~/Portal/", StringComparison.OrdinalIgnoreCase)
                    && path.IndexOf("Logout", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    Response.Redirect("~/Portal/Home.aspx", true);
                    return;
                }
            }

            bool wooOn = IsWooIntegrationEnabledCached();
            SyncWooSystemMenuItem(wooOn);
            SyncWooOrderImportMenuItem(wooOn);
            HighlightCurrentMenuItem();
            MobileStaffLayout.ApplyBodyClasses(bodySite, Request);

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

        /// <summary>Hide System → Woo submenu until WooCommerce integration is enabled.</summary>
        private void SyncWooSystemMenuItem(bool wooOn)
        {
            if (NavigationMenu == null)
                return;

            MenuItem systemMenu = FindMenuItemByValue(NavigationMenu.Items, "System");
            if (systemMenu == null)
                return;

            MenuItem wooMenu = FindMenuItemByValue(systemMenu.ChildItems, "Woo");
            if (wooMenu == null)
                return;

            if (!wooOn)
            {
                systemMenu.ChildItems.Remove(wooMenu);
                return;
            }

            MenuItem settings = FindMenuItemByValue(wooMenu.ChildItems, "WooSettings");
            MenuItem mapping = FindMenuItemByValue(wooMenu.ChildItems, "WooMapping");
            if (settings != null)
                settings.NavigateUrl = "~/Tools/SystemPreferences.aspx?section=woo";
            if (mapping != null)
                mapping.NavigateUrl = "~/Tools/WooCommerceMapping.aspx";
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
                if (TryHighlightMenuBranch(menuItem, currentPage))
                    parentSelected = true;
            }

            HighlightWooSettingsMenuWhenOnWooSection();

            if ((currentPage.Contains("DEFAULT.ASPX") || currentPage.EndsWith("/")) && !parentSelected)
            {
                MenuItem homeRoot = NavigationMenu.Items.Count > 0 ? NavigationMenu.Items[0] : null;
                if (homeRoot != null && homeRoot.ChildItems.Count > 0)
                {
                    foreach (MenuItem child in homeRoot.ChildItems)
                    {
                        if (string.IsNullOrEmpty(child.NavigateUrl))
                            continue;
                        string childUrl = ResolveUrl(child.NavigateUrl).ToUpper();
                        if (currentPage == childUrl)
                        {
                            child.Selected = true;
                            parentSelected = true;
                            break;
                        }
                    }

                    if (!parentSelected)
                    {
                        homeRoot.ChildItems[0].Selected = true;
                    }
                }
                else if (homeRoot != null && homeRoot.Selectable)
                {
                    homeRoot.Selected = true;
                }
            }
        }

        private void HighlightWooSettingsMenuWhenOnWooSection()
        {
            string currentPage = Request.AppRelativeCurrentExecutionFilePath.ToUpper();
            if (!currentPage.Contains("SYSTEMPREFERENCES.ASPX"))
                return;

            string raw = Request.QueryString["section"];
            string wiz = Request.QueryString["wizard"];
            bool wooSection = string.Equals(raw, "woo", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "woocommerce", StringComparison.OrdinalIgnoreCase)
                || raw == "1"
                || wiz == "1"
                || string.Equals(wiz, "true", StringComparison.OrdinalIgnoreCase);
            if (!wooSection)
                return;

            MenuItem wooSettings = FindMenuItemByValue(NavigationMenu.Items, "WooSettings");
            if (wooSettings == null || !wooSettings.Enabled)
                return;

            MenuItem sysPrefs = FindMenuItemByValue(NavigationMenu.Items, "SystemPreferences");
            if (sysPrefs != null)
                sysPrefs.Selected = false;

            SelectMenuItemAndAncestors(wooSettings);
        }

        private static void SelectMenuItemAndAncestors(MenuItem item)
        {
            if (item == null)
                return;

            item.Selected = true;
            MenuItem parent = item.Parent;
            while (parent != null)
            {
                if (parent.Selectable)
                    parent.Selected = true;
                parent = parent.Parent;
            }
        }

        private bool TryHighlightMenuBranch(MenuItem menuItem, string currentPage)
        {
            if (menuItem == null)
                return false;

            string menuUrl = string.IsNullOrEmpty(menuItem.NavigateUrl)
                ? string.Empty
                : ResolveUrl(menuItem.NavigateUrl).ToUpper();
            if (!string.IsNullOrEmpty(menuUrl) && currentPage == menuUrl)
            {
                if (menuItem.Selectable)
                    menuItem.Selected = true;
                return true;
            }

            foreach (MenuItem child in menuItem.ChildItems)
            {
                if (TryHighlightMenuBranch(child, currentPage))
                {
                    if (child.Selectable)
                        child.Selected = true;
                    if (menuItem.Selectable)
                        menuItem.Selected = true;
                    return true;
                }
            }

            return false;
        }
    }
}
