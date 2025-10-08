// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Administration.UserInformation
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Administration
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
        protected Button btnReturnToManagerUser;
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

            try
            {
                Guid userId = (Guid)user.ProviderUserKey;

                TrackerDotNet.Classes.UserPreferencesHelper.EnsureUserPreferencesTableExists();
                var prefs = TrackerDotNet.Classes.UserPreferencesHelper.GetCurrentPreferencesForUser(userId);

                ddlTimeZone.DataSource = TimeZoneInfo.GetSystemTimeZones();
                ddlTimeZone.DataTextField = "DisplayName";
                ddlTimeZone.DataValueField = "Id";
                ddlTimeZone.DataBind();

                if (ddlTimeZone.Items.FindByValue(prefs.TimeZoneId) != null)
                    ddlTimeZone.SelectedValue = prefs.TimeZoneId;
                else
                    ddlTimeZone.SelectedIndex = 0;
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

                var prefs = TrackerDotNet.Classes.UserPreferencesHelper.GetCurrentPreferencesForUser(userId);
                prefs.TimeZoneId = ddlTimeZone.SelectedValue;

                TrackerDotNet.Classes.UserPreferencesHelper.SaveOrUpdatePreferences(prefs); 

                AppLogger.WriteLog("userprefs", $"Updated time zone for user '{username}' to '{prefs.TimeZoneId}'");
            }

            Response.Redirect("~/Administration/ManageUsers.aspx");
        }


        protected void btnDeleteUser_Click(object sender, EventArgs e)
        {
            Membership.DeleteUser(this.Request.QueryString["user"], true);
            this.lblStatusMessage.Text = "The user account deleted.";
        }
    }
}