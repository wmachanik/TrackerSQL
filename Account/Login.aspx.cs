//------------------------------------------------------------------------------
// TrackerSQL v3.x — Login
// Membership / account page code-behind for Login.
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

//- only form later versions #nullable disable
namespace TrackerSQL.Account
{
    /// <summary>
    /// Single sign-in for staff (username) and Contact Portal contacts (email).
    /// The account's role decides where it lands: Contact-role users go to the portal.
    /// </summary>
    public partial class Login : Page
    {
        protected PlaceHolder phAdmin;
        protected HyperLink RegisterHyperLink;
        protected System.Web.UI.WebControls.Login LoginUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            string returnUrl = Request.QueryString["ReturnUrl"];
            RegisterHyperLink.NavigateUrl = "Register.aspx"
                + (string.IsNullOrEmpty(returnUrl) ? string.Empty : "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl));
        }

        /// <summary>Allows signing in with the account email when it is not itself the user name.</summary>
        protected void LoginUser_LoggingIn(object sender, LoginCancelEventArgs e)
        {
            string resolved = SignInNameResolver.Resolve(LoginUser.UserName);
            if (!string.IsNullOrEmpty(resolved))
                LoginUser.UserName = resolved;
        }

        protected void LoginUser_LoggedIn(object sender, EventArgs e)
        {
            string username = LoginUser.UserName;

            if (ContactPortalManager.IsContactRole(username))
            {
                string target = new ContactPortalManager().CompleteContactSignIn(username, Request.QueryString["ReturnUrl"]);
                Response.Redirect(target, true);
                return;
            }

            UserPreferencesHelper.InitializeSessionForStaffUser(username);
        }
    }
}
