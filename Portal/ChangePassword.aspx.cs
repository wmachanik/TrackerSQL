using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

namespace TrackerSQL.Portal
{
    /// <summary>
    /// Signed in: change own password. Signed out (link in the temporary-password email,
    /// ?u=email): email + temporary password + new password, then signs the contact in.
    /// </summary>
    public partial class ChangePassword : PortalPageBase
    {
        protected TextBox txtEmail;
        protected TextBox txtCurrent;
        protected TextBox txtNew;
        protected TextBox txtConfirm;
        protected Label lblCurrent;
        protected PlaceHolder phEmail, phAnonHelp;
        protected Literal litMessage, litSubtitle, litTitle, litRules;
        protected Button btnSave, btnCancel;

        protected override bool AllowAnonymous => true;

        private bool IsFirstTimeMode => !Request.IsAuthenticated;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsAdminPreview)
            {
                Response.Redirect("~/Portal/Home.aspx", true);
                return;
            }

            string linkEmail = (Request.QueryString["u"] ?? string.Empty).Trim();

            // Link for a different login than the one signed in on this browser: start fresh for that email.
            if (!IsPostBack && Request.IsAuthenticated && linkEmail.Length > 0)
            {
                string linkUser = SignInNameResolver.Resolve(linkEmail);
                if (!string.Equals(linkUser, User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                {
                    FormsAuthentication.SignOut();
                    Session.Clear();
                    Response.Redirect(Request.RawUrl, true);
                    return;
                }
            }

            litRules.Text = HttpUtility.HtmlEncode(PasswordRulesText());

            if (IsFirstTimeMode)
            {
                phEmail.Visible = true;
                phAnonHelp.Visible = true;
                btnCancel.Visible = false;
                lblCurrent.Text = "Temporary Password (From The Email)";
                if (!IsPostBack)
                {
                    litTitle.Text = "Choose Your Password";
                    litSubtitle.Text = "Enter the temporary password we emailed you, then choose your own.";
                    txtEmail.Text = linkEmail;
                }
                return;
            }

            bool mustChange = Portal.MustChangePassword();
            btnCancel.Visible = !mustChange;
            if (!IsPostBack)
            {
                litSubtitle.Text = mustChange
                    ? "Please choose your own password before using My Quaffee."
                    : "Choose a new password for your My Quaffee login.";
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string cur = txtCurrent.Text ?? string.Empty;
            string neu = txtNew.Text ?? string.Empty;
            string conf = txtConfirm.Text ?? string.Empty;

            if (!MeetsPasswordRules(neu))
            {
                litMessage.Text = StatusHtml("The new password is too simple. " + PasswordRulesText(), true);
                return;
            }
            if (neu != conf)
            {
                litMessage.Text = StatusHtml("New password and confirmation do not match.", true);
                return;
            }

            if (IsFirstTimeMode)
            {
                SaveFirstTimePassword(cur, neu);
                return;
            }

            var user = Membership.GetUser();
            if (user == null)
            {
                Response.Redirect("~/Account/Login.aspx", true);
                return;
            }

            if (!TryChangePassword(user, cur, neu))
            {
                litMessage.Text = StatusHtml("Could not change password. Check your current password.", true);
                return;
            }

            Portal.RecordPasswordChanged();
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Portal password changed: " + user.UserName);
            Response.Redirect("~/Portal/Home.aspx", true);
        }

        private void SaveFirstTimePassword(string temporary, string newPassword)
        {
            const string failed = "That email and temporary password do not match. Check the email we sent you, or request a new temporary password below.";
            string userName = SignInNameResolver.Resolve(txtEmail.Text);

            // Staff accounts use the normal sign-in page; this link is only for Contact Portal logins.
            if (string.IsNullOrEmpty(userName) || !ContactPortalManager.IsContactRole(userName)
                || !Membership.ValidateUser(userName, temporary))
            {
                litMessage.Text = StatusHtml(failed, true);
                return;
            }

            MembershipUser user = Membership.GetUser(userName);
            if (user == null || !TryChangePassword(user, temporary, newPassword))
            {
                litMessage.Text = StatusHtml("Could not set that password. " + PasswordRulesText(), true);
                return;
            }

            FormsAuthentication.SetAuthCookie(userName, false);
            Portal.RecordPasswordChanged(userName);
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Portal password set from email link: " + userName);
            Response.Redirect(Portal.CompleteContactSignIn(userName, null), true);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Portal/Home.aspx", true);
        }

        private static bool TryChangePassword(MembershipUser user, string oldPassword, string newPassword)
        {
            try
            {
                return user.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool MeetsPasswordRules(string password)
        {
            return password.Length >= Membership.MinRequiredPasswordLength
                && password.Count(ch => !char.IsLetterOrDigit(ch)) >= Membership.MinRequiredNonAlphanumericCharacters;
        }

        private static string PasswordRulesText()
        {
            int symbols = Membership.MinRequiredNonAlphanumericCharacters;
            return "Use at least " + Membership.MinRequiredPasswordLength + " characters"
                + (symbols > 0
                    ? ", including at least " + symbols + " symbol" + (symbols == 1 ? "" : "s") + " such as ! # or @."
                    : ".");
        }
    }
}
