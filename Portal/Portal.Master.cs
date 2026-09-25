using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Managers;

namespace TrackerSQL.Portal
{
    public partial class PortalMaster : MasterPage
    {
        protected HtmlGenericControl navPortal;
        protected HyperLink lnkHome;
        protected HyperLink lnkContact;
        protected HyperLink lnkOrders;
        protected HyperLink lnkRepairs;
        protected HyperLink lnkRecurring;
        protected LinkButton btnLogout;
        protected Literal litAccountNotice;

        protected void Page_Load(object sender, EventArgs e)
        {
            bool preview = Request.IsAuthenticated && SafeIsViewingAs();
            bool authed = preview || (Request.IsAuthenticated && ContactPortalManagerSafeIsContact());

            if (navPortal != null)
                navPortal.Visible = authed;
            if (btnLogout != null && preview)
            {
                btnLogout.Text = "Exit Preview";
                btnLogout.ToolTip = "Stop viewing the portal as this contact";
            }

            if (litAccountNotice != null)
                litAccountNotice.Text = authed ? PreviewNoticeHtml(preview) + AccountNoticeHtml() : string.Empty;
        }

        private string PreviewNoticeHtml(bool preview)
        {
            if (!preview)
                return string.Empty;
            try
            {
                var contact = new ContactPortalManager().GetOwnContact();
                string name = contact == null
                    ? "contact #" + ContactPortalManager.GetViewAsContactId()
                    : (contact.CompanyName ?? string.Empty).Trim() + " (#" + contact.ContactID + ")";
                return "<div class=\"portal-account-notice portal-preview-notice\" role=\"status\">"
                    + "<span class=\"status-badge is-done\">Admin Preview</span>"
                    + "<span>You are viewing the Contact Portal (My Quaffee) as <strong>" + HttpUtility.HtmlEncode(name) + "</strong>. "
                    + "Read-only: nothing you save or submit here is kept or emailed.</span>"
                    + "<a href=\"" + HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Portal/ViewAs.aspx?exit=1")) + "\">Exit preview</a></div>";
            }
            catch
            {
                return string.Empty;
            }
        }

        private string AccountNoticeHtml()
        {
            try
            {
                if (!new ContactPortalManager().IsOwnContactDisabled())
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }

            return "<div class=\"portal-account-notice\" role=\"status\">"
                + "<span class=\"status-badge is-disabled\">Account Disabled</span>"
                + "<span>Your Quaffee account is currently disabled. You can still view your details and history. "
                + "To start ordering again, repeat your last order or use Request A Change on "
                + "<a href=\"" + HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Portal/MyContact.aspx")) + "\">My Details</a>"
                + " and we will re-activate it.</span></div>";
        }

        private static bool SafeIsViewingAs()
        {
            try
            {
                return ContactPortalManager.IsViewingAs();
            }
            catch
            {
                return false;
            }
        }

        private static bool ContactPortalManagerSafeIsContact()
        {
            try
            {
                return ContactPortalManager.IsContactRole(
                    HttpContext.Current?.User?.Identity?.Name);
            }
            catch
            {
                return false;
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            if (SafeIsViewingAs())
            {
                Response.Redirect("~/Portal/ViewAs.aspx?exit=1", true);
                return;
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            Response.Redirect("~/Account/Login.aspx", true);
        }
    }
}
