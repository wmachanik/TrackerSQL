//------------------------------------------------------------------------------
// TrackerSQL v3.x — RecoverPassword
// Membership / account page code-behind for RecoverPassword.
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;

namespace TrackerSQL.Account
{
    public partial class RecoverPassword : Page
    {
        protected Panel pnlRecoverPassword;
        protected TextBox tbxUserName;
        protected Button btnSubmit;
        protected HtmlGenericControl pnlError;
        protected HtmlGenericControl pnlSuccess;
        protected Literal ltrlError;
        protected Literal ltrlSuccess;

        protected void Page_Load(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "User recover password page entered.");
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ShowError(null);
            ShowSuccess(null);

            string input = (tbxUserName.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
            {
                ShowError("User name or email is required.");
                return;
            }

            try
            {
                MembershipUser user = FindMembershipUser(input);
                if (user == null)
                {
                    // Same generic message whether missing — avoid confirming accounts
                    ShowError("We were unable to access your information. Please check the user name or email and try again.");
                    AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                        "RecoverPassword: no membership user for '" + input + "'.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    ShowError("This account has no email address on file. Ask an administrator to reset the password.");
                    AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                        "RecoverPassword: user '" + user.UserName + "' has no email.");
                    return;
                }

                if (!user.IsApproved)
                {
                    ShowError("This account is not approved yet. Contact an administrator.");
                    return;
                }

                if (user.IsLockedOut)
                    user.UnlockUser();

                string newPassword = user.ResetPassword();
                if (string.IsNullOrEmpty(newPassword))
                {
                    ShowError("Password reset failed. Please try again or contact an administrator.");
                    return;
                }

                if (!SendPasswordResetEmail(user, newPassword))
                {
                    ShowError("Your password was reset, but the email could not be sent. Contact an administrator.");
                    AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                        "RecoverPassword: reset OK for '" + user.UserName
                        + "' but EmailMailKitCls send failed.");
                    return;
                }

                ShowSuccess("A new password has been emailed to the address on your account.");
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "RecoverPassword: reset email sent for user '" + user.UserName
                    + "' to '" + user.Email + "' via system EmailMailKitCls.");
                tbxUserName.Text = string.Empty;
            }
            catch (Exception ex)
            {
                ShowError("We were unable to reset your password. Please try again.");
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "RecoverPassword error: " + ex.Message);
            }
        }

        private static MembershipUser FindMembershipUser(string userNameOrEmail)
        {
            MembershipUser user = Membership.GetUser(userNameOrEmail, userIsOnline: false);
            if (user != null)
                return user;

            // Allow typing the account email when it differs from UserName
            string userName = Membership.GetUserNameByEmail(userNameOrEmail);
            if (string.IsNullOrEmpty(userName))
                return null;

            return Membership.GetUser(userName, userIsOnline: false);
        }

        /// <summary>
        /// Uses the same SMTP path as coffee checkup / repairs (EmailMailKitCls + Web.config EMail*).
        /// Does not use &lt;system.net&gt; mailSettings / PasswordRecovery SmtpClient.
        /// </summary>
        private static bool SendPasswordResetEmail(MembershipUser user, string newPassword)
        {
            var email = new EmailMailKitCls();
            // Password resets must reach the account holder even when EmailTestMode is on for checkup runs.
            email.IsTestMode = false;

            string fromAddress = ConfigHelper.GetString("SysEmailFrom", null);
            if (string.IsNullOrWhiteSpace(fromAddress))
                fromAddress = ConfigHelper.GetString("EMailLogIn", null);

            email.SetEmailFromTo(fromAddress, user.Email);
            email.SetEmailSubject("Tracker — password reset");
            email.AddStrAndNewLineToBody("Hello " + HttpUtility.HtmlEncode(user.UserName) + ",");
            email.AddStrAndNewLineToBody(string.Empty);
            email.AddStrAndNewLineToBody("Your Tracker password has been reset.");
            email.AddStrAndNewLineToBody("User name: <b>" + HttpUtility.HtmlEncode(user.UserName) + "</b>");
            email.AddStrAndNewLineToBody("Temporary password: <b>" + HttpUtility.HtmlEncode(newPassword) + "</b>");
            email.AddStrAndNewLineToBody(string.Empty);
            email.AddStrAndNewLineToBody("Please sign in and change this password as soon as you can.");

            bool sent = email.SendEmail();
            if (!sent)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "RecoverPassword SendPasswordResetEmail failed: " + (email.LastErrorSummary ?? "(no detail)"));
            }

            return sent;
        }

        private void ShowError(string message)
        {
            bool hasMessage = !string.IsNullOrWhiteSpace(message);
            if (pnlError != null)
                pnlError.Visible = hasMessage;
            if (ltrlError != null)
                ltrlError.Text = hasMessage ? HttpUtility.HtmlEncode(message) : string.Empty;
        }

        private void ShowSuccess(string message)
        {
            bool hasMessage = !string.IsNullOrWhiteSpace(message);
            if (pnlSuccess != null)
                pnlSuccess.Visible = hasMessage;
            if (ltrlSuccess != null)
                ltrlSuccess.Text = hasMessage ? HttpUtility.HtmlEncode(message) : string.Empty;
        }
    }
}
