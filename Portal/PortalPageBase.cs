using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Portal
{
    /// <summary>
    /// Base for authenticated contact-portal pages.
    /// </summary>
    public class PortalPageBase : Page
    {
        protected readonly ContactPortalManager Portal = new ContactPortalManager();

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            Portal.EnsureReady();
        }

        /// <summary>Pages that also work signed out (the first-time set-password link).</summary>
        protected virtual bool AllowAnonymous => false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Request.IsAuthenticated)
            {
                if (AllowAnonymous)
                    return;
                Response.Redirect("~/Account/Login.aspx?ReturnUrl=" +
                    HttpUtility.UrlEncode(Request.RawUrl), true);
                return;
            }

            string user = User?.Identity?.Name;
            if (!IsAdminPreview && !ContactPortalManager.IsContactRole(user))
            {
                Response.Redirect("~/Default.aspx", true);
                return;
            }

            if (Portal.GetCurrentContactId() <= 0)
            {
                FormsAuthentication.SignOut();
                Response.Redirect("~/Account/Login.aspx", true);
                return;
            }

            bool isChangePassword = Request.Path.IndexOf("ChangePassword.aspx", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isChangePassword && Portal.MustChangePassword())
            {
                Response.Redirect("~/Portal/ChangePassword.aspx", true);
                return;
            }
        }

        protected int CurrentContactId => Portal.GetCurrentContactId();

        /// <summary>An administrator is viewing the portal as a contact (read-only).</summary>
        protected static bool IsAdminPreview => ContactPortalManager.IsViewingAs();

        protected void SignOutAndRedirect()
        {
            if (IsAdminPreview)
            {
                Response.Redirect("~/Portal/ViewAs.aspx?exit=1", true);
                return;
            }
            FormsAuthentication.SignOut();
            Session.Clear();
            Response.Redirect("~/Account/Login.aspx", true);
        }

        /// <summary>Status strip HTML for the bottom of a portal panel.</summary>
        protected static string StatusHtml(string text, bool error)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return "<p class=\"status-message "
                + (error ? "status-error" : "status-success")
                + "\">" + HttpUtility.HtmlEncode(text) + "</p>";
        }

        /// <summary>Title Case status chip; completed states use the orange "done" style.</summary>
        protected static string StatusBadgeHtml(string text, bool isDone)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return "<span class=\"status-badge " + (isDone ? "is-done" : "is-enabled") + "\">"
                + HttpUtility.HtmlEncode(text) + "</span>";
        }

        /// <summary>"Next coffee" highlight for Home and My Orders; empty when there is nothing to show.</summary>
        protected string NextCoffeeHtml()
        {
            var open = Portal.GetOwnOpenOrder();
            bool disabled = open == null && Portal.IsOwnContactDisabled();
            var next = open == null && !disabled ? Portal.GetOwnNextCoffee() : null;

            string heading;
            string detail;
            if (open != null)
            {
                heading = "You have an order in progress";
                detail = "Delivery / dispatch date: " + ContactPortalDisplay.FormatDate(open.RequiredByDate)
                    + ". Need something added? Open it and use Request A Change.";
            }
            else if (disabled)
            {
                heading = "Want to start ordering again?";
                detail = "Repeat your last order and we will re-activate your account once we have checked the order.";
            }
            else if (next == null)
            {
                heading = "Need more coffee?";
                detail = "Repeat your last order and we will send it on the next delivery / dispatch date for your area.";
            }
            else if (next.FromRecurring)
            {
                heading = next.IsDueNow ? "Your next recurring delivery is due now" : "Next recurring delivery: " + next.DateDisplay;
                detail = "From your recurring order. See My Recurring Orders for details.";
            }
            else
            {
                heading = next.IsDueNow
                    ? "Next predicted coffee: due now (estimated " + next.DateDisplay + ")"
                    : "Next predicted coffee: " + next.DateDisplay;
                detail = "Our estimate based on how much coffee you usually use. Need it sooner? Repeat your last order.";
            }

            return "<div class=\"portal-next-coffee\"><img src=\""
                + HttpUtility.HtmlAttributeEncode(ResolveUrl("~/images/imgButtons/icons8-order-16.png"))
                + "\" alt=\"\" /><div class=\"portal-next-coffee-text\"><strong>" + HttpUtility.HtmlEncode(heading) + "</strong><span>"
                + HttpUtility.HtmlEncode(detail) + "</span></div>"
                + (open != null
                    ? ButtonHtml("~/Portal/MyOrder.aspx?OrderID=" + open.OrderID, "View My Order", "Open the order that is in progress")
                    : ButtonHtml("~/Portal/RepeatOrder.aspx", "Repeat My Last Order", "Order the same items as last time"))
                + "</div>";
        }

        private string ButtonHtml(string appRelativeUrl, string text, string title)
        {
            return "<span class=\"image-button\" title=\"" + HttpUtility.HtmlAttributeEncode(title) + "\"><img src=\""
                + HttpUtility.HtmlAttributeEncode(ResolveUrl("~/images/imgButtons/View.png"))
                + "\" alt=\"\" /><a href=\"" + HttpUtility.HtmlAttributeEncode(ResolveUrl(appRelativeUrl))
                + "\">" + HttpUtility.HtmlEncode(text) + "</a></span>";
        }
    }
}
