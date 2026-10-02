//------------------------------------------------------------------------------
// TrackerSQL v3.x — UserInformation
// Administration page code-behind for UserInformation.
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

//- only form later versions #nullable disable
namespace TrackerSQL.Administration
{
    public partial class UserInformation : Page
    {
        protected Label lblUserName;
        protected CheckBox cbxUserIsApproved;
        protected CheckBoxList UserRolesCheckBoxList;
        protected Label lblUserLockedOut;
        protected Button btnUnlockUser;
        protected Label OnlineLabel;
        protected Label LastLoginDateLabel;
        protected Label EmailLabel;
        protected Button btnDeleteUser;
        protected Button btnUpdate;
        protected ImageButton btnReturnToManagerUser;
        protected Label lblStatusMessage;

        private void Page_PreRender()
        {
            this.UserRolesCheckBoxList.DataSource = (object)Roles.GetAllRoles();
            this.UserRolesCheckBoxList.DataBind();
            foreach (string str in Roles.GetRolesForUser(this.Request.QueryString["user"]))
                this.UserRolesCheckBoxList.Items.FindByValue(str).Selected = true;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            string username = Request.QueryString["user"];
            if (string.IsNullOrWhiteSpace(username))
            {
                Response.Redirect("ManageUsers.aspx");
                return;
            }

            MembershipUser user = Membership.GetUser(username);
            if (user == null)
            {
                Response.Redirect("ManageUsers.aspx");
                return;
            }

            lblUserName.Text = user.UserName;
            cbxUserIsApproved.Checked = user.IsApproved;
            lblUserLockedOut.Text = user.LastLockoutDate.Year >= 2000 ? user.LastLockoutDate.ToShortDateString() : "no";
            btnUnlockUser.Visible = user.IsLockedOut;
            OnlineLabel.Text = user.IsOnline ? "online" : "offline";
            LastLoginDateLabel.Text = $"{user.LastLoginDate:d}";
            EmailLabel.Text = user.Email;

            btnDeleteUser.OnClientClick =
                "return confirm('" + HttpUtility.JavaScriptStringEncode(
                    "Delete user '" + user.UserName + "'? This cannot be undone.") + "');";

            try
            {
                Guid userId = (Guid)user.ProviderUserKey;

                TrackerSQL.Classes.UserPreferencesHelper.EnsureUserPreferencesTableExists();
                var prefs = TrackerSQL.Classes.UserPreferencesHelper.GetCurrentPreferencesForUser(userId);

                ddlTimeZone.DataSource = TimeZoneInfo.GetSystemTimeZones();
                ddlTimeZone.DataTextField = "DisplayName";
                ddlTimeZone.DataValueField = "Id";
                ddlTimeZone.DataBind();

                if (ddlTimeZone.Items.FindByValue(prefs.TimeZoneId) != null)
                    ddlTimeZone.SelectedValue = prefs.TimeZoneId;
                else
                    ddlTimeZone.SelectedIndex = 0;

                cbxDriverTracking.Checked = prefs.DriverTracking;
            }
            catch (Exception ex)
            {
                ddlTimeZone.Items.Clear();
                ddlTimeZone.Items.Add(new ListItem("Unavailable"));
                AppLogger.WriteLog("userprefs", $"Error loading time zone for user {username}: {ex.Message}");
            }
        }

        protected void cbxUserIsApproved_CheckedChanged(object sender, EventArgs e)
        {
            MembershipUser user = Membership.GetUser(this.Request.QueryString["user"]);
            user.IsApproved = this.cbxUserIsApproved.Checked;
            Membership.UpdateUser(user);
            this.lblStatusMessage.Text = "The user's approved status has been updated.";
        }

        protected void btnUnlockUser_Click(object sender, EventArgs e)
        {
            MembershipUser user = Membership.GetUser(this.Request.QueryString["user"]);
            user.UnlockUser();
            Membership.UpdateUser(user);
            this.btnUnlockUser.Enabled = false;
            this.lblStatusMessage.Text = "The user account has been unlocked.";
        }

        private void UpdateUserRoles()
        {
            string text = this.lblUserName.Text;
            foreach (ListItem listItem in this.UserRolesCheckBoxList.Items)
            {
                if (listItem.Selected)
                {
                    if (!Roles.IsUserInRole(text, listItem.Text))
                        Roles.AddUserToRole(text, listItem.Text);
                }
                else if (Roles.IsUserInRole(text, listItem.Text))
                    Roles.RemoveUserFromRole(text, listItem.Text);
            }
        }

        protected void btnUpdateUser_Click(object sender, EventArgs e)
        {
            this.UpdateUserRoles();

            string username = lblUserName.Text;
            MembershipUser user = Membership.GetUser(username);
            if (user != null)
            {
                Guid userId = (Guid)user.ProviderUserKey;

                var prefs = TrackerSQL.Classes.UserPreferencesHelper.GetCurrentPreferencesForUser(userId);
                prefs.TimeZoneId = ddlTimeZone.SelectedValue;
                prefs.DriverTracking = cbxDriverTracking.Checked;

                TrackerSQL.Classes.UserPreferencesHelper.SaveOrUpdatePreferences(prefs); 
                if (string.Equals(username, User?.Identity?.Name, StringComparison.OrdinalIgnoreCase))
                    Session?.Remove("UserPreferences");

                AppLogger.WriteLog("userprefs", $"Updated preferences for user '{username}': zone '{prefs.TimeZoneId}', " +
                    $"driverTracking={prefs.DriverTracking}");
            }

            Response.Redirect("~/Administration/ManageUsers.aspx");
        }


        protected void btnDeleteUser_Click(object sender, EventArgs e)
        {
            string username = Request.QueryString["user"];
            if (string.IsNullOrWhiteSpace(username))
                username = lblUserName.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                Response.Redirect("~/Administration/ManageUsers.aspx");
                return;
            }

            if (string.Equals(username, User?.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                lblStatusMessage.Text = "You cannot delete the login you are signed in with.";
                return;
            }

            try
            {
                if (new UserAccountManager().DeleteUser(username, User?.Identity?.Name, out string error))
                {
                    Response.Redirect("~/Administration/ManageUsers.aspx", endResponse: false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                lblStatusMessage.Text = Server.HtmlEncode(error);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "UserInformation: delete user '" + username + "' failed: " + ex.Message);
                lblStatusMessage.Text = "Could not delete user. " + Server.HtmlEncode(ex.Message);
            }
        }
    }
}
