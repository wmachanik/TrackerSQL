//------------------------------------------------------------------------------
// TrackerSQL v3.x — ManageUsers
// Administration page code-behind for ManageUsers.
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;

namespace TrackerSQL.Administration
{
    public partial class ManageUsers : Page
    {
        protected GridView gvUserAccounts;
        protected Label lblStatusMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.Page.IsPostBack)
                return;
            this.BindUserAccounts();
        }

        private void BindUserAccounts()
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "User management page entered.");

            this.gvUserAccounts.DataSource = Membership.GetAllUsers();
            this.gvUserAccounts.DataBind();
        }

        protected void gvUserAccounts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var user = e.Row.DataItem as MembershipUser;
            var unlockShell = e.Row.FindControl("spnUnlockUser") as HtmlGenericControl;
            if (unlockShell != null)
                unlockShell.Visible = user != null && user.IsLockedOut;

            var btnUnlockUser = e.Row.FindControl("btnUnlockUser") as LinkButton;
            if (btnUnlockUser != null && user != null && user.IsLockedOut)
            {
                string confirmMessage = "Unlock account '" + user.UserName + "'?";
                btnUnlockUser.OnClientClick =
                    "return confirm('" + HttpUtility.JavaScriptStringEncode(confirmMessage) + "');";
            }
        }

        protected void gvUserAccounts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "UnlockUser", StringComparison.Ordinal))
                return;

            string userName = Convert.ToString(e.CommandArgument);
            if (string.IsNullOrWhiteSpace(userName))
            {
                SetStatus("No user selected.", isError: true);
                return;
            }

            MembershipUser user = Membership.GetUser(userName);
            if (user == null)
            {
                SetStatus("User '" + userName + "' was not found.", isError: true);
                return;
            }

            if (!user.IsLockedOut)
            {
                SetStatus("'" + user.UserName + "' is not locked out.");
                BindUserAccounts();
                return;
            }

            if (!user.UnlockUser())
            {
                SetStatus("Could not unlock '" + user.UserName + "'.", isError: true);
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "ManageUsers: UnlockUser failed for " + user.UserName);
                return;
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                "ManageUsers: unlocked account " + user.UserName);
            SetStatus("Unlocked '" + user.UserName + "'.");
            BindUserAccounts();
        }

        private void SetStatus(string message, bool isError = false)
        {
            if (lblStatusMessage == null)
                return;
            lblStatusMessage.Text = message;
            lblStatusMessage.CssClass = isError ? "status-error" : "status-info";
        }
    }
}
