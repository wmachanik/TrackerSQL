using System;
using System.Web.Security;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Administration
{
    public partial class ResetPasswordHelper : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            
            try
            {
                string username = txtUsername.Text;
                string newPassword = txtNewPassword.Text;

                // Get the user
                MembershipUser user = Membership.GetUser(username);

                if (user == null)
                {
                    lblError.Text = "User not found!";
                    return;
                }

                // Reset the password
                string tempPassword = user.ResetPassword();
                bool success = user.ChangePassword(tempPassword, newPassword);

                if (success)
                {
                    lblMessage.Text = $"Password for {username} was successfully reset to: {newPassword}";

                    // Unlock account if needed
                    if (user.IsLockedOut)
                    {
                        user.UnlockUser();
                    }
                }
                else
                {
                    lblError.Text = "Password reset failed. Check password requirements.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Error: " + ex.Message;
            }
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, $"Reset Password - {lblMessage.Text}");
        }
    }
}