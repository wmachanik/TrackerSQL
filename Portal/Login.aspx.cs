using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Managers;

namespace TrackerSQL.Portal
{
    /// <summary>
    /// Contact "registration": request a temporary password, or claim one account from the
    /// multi-account email. Signing in itself happens on the shared ~/Account/Login.aspx.
    /// </summary>
    public partial class Login : Page
    {
        private readonly ContactPortalManager _portal = new ContactPortalManager();

        protected PlaceHolder phRequest;
        protected TextBox txtInviteEmail;
        protected Literal litMessage;
        protected Button btnRequestAccess;
        protected HyperLink hlSignIn;

        protected void Page_Load(object sender, EventArgs e)
        {
            _portal.EnsureReady();

            if (!IsPostBack && Request.IsAuthenticated
                && ContactPortalManager.IsContactRole(User.Identity.Name))
            {
                Response.Redirect("~/Portal/Home.aspx", true);
                return;
            }

            if (IsPostBack)
                return;

            string chooseToken = Request.QueryString["choose"];
            if (!string.IsNullOrEmpty(chooseToken))
            {
                bool claimed = _portal.ClaimChosenContact(chooseToken, out string msg);
                SetMsg(msg, !claimed);
                phRequest.Visible = !claimed;
                return;
            }

            // Old "sign in here" links pointed at this page; sign-in now lives on the shared login page.
            if (!string.Equals(Request.QueryString["request"], "1", StringComparison.Ordinal))
            {
                string returnUrl = Request.QueryString["ReturnUrl"];
                Response.Redirect("~/Account/Login.aspx"
                    + (string.IsNullOrEmpty(returnUrl) ? string.Empty : "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl)), true);
            }
        }

        protected void btnRequestAccess_Click(object sender, EventArgs e)
        {
            string msg = _portal.RequestAccess(txtInviteEmail.Text);
            SetMsg(msg, false);
            phRequest.Visible = false;
        }

        private void SetMsg(string text, bool error)
        {
            litMessage.Text = "<p class=\"status-message "
                + (error ? "status-error" : "status-info")
                + "\">" + HttpUtility.HtmlEncode(text) + "</p>";
        }
    }
}
