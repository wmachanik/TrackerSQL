//------------------------------------------------------------------------------
// TrackerSQL v3.x — ManageRoles
// Administration page code-behind for ManageRoles.
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Administration
{
    public partial class ManageRoles : Page
    {
        protected ScriptManager smManageRoles;
        protected UpdateProgress uprgManageRoles;
        protected UpdatePanel upnlManageRoles;
        protected Panel pnlManageRoles;
        protected GridView gvRolesManagement;
        protected Label MsgLabel;
        protected Label lblRoleName;
        protected TextBox RoleTextBox;
        protected Button CreateRoleButton;
        protected HtmlGenericControl pnlStatus;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.Page.IsPostBack)
                return;
            this.BindRoles();
        }

        private void BindRoles()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("RoleName", typeof(string));
            dataTable.Columns.Add("UserCount", typeof(int));
            foreach (string roleName in Roles.GetAllRoles())
            {
                int userCount = Roles.GetUsersInRole(roleName).Length;
                dataTable.Rows.Add(roleName, userCount);
            }
            this.gvRolesManagement.DataSource = dataTable;
            this.gvRolesManagement.DataBind();
        }

        private void SetStatus(string message, bool isError)
        {
            this.MsgLabel.Text = message ?? string.Empty;
            if (this.pnlStatus != null)
            {
                this.pnlStatus.Attributes["class"] = isError
                    ? "status-message status-error"
                    : string.IsNullOrWhiteSpace(message)
                        ? "status-message"
                        : "status-message status-success";
            }
            if (!string.IsNullOrWhiteSpace(message))
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, message);
        }

        public void CreateRole_OnClick(object sender, EventArgs args)
        {
            string roleName = (this.RoleTextBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(roleName))
            {
                this.SetStatus("Enter a role name.", isError: true);
                return;
            }

            try
            {
                if (Roles.RoleExists(roleName))
                {
                    this.SetStatus(
                        "Role '" + this.Server.HtmlEncode(roleName) + "' already exists. Please specify a different role name.",
                        isError: true);
                    return;
                }

                this.CreateRoleCompatible(roleName);
                this.RoleTextBox.Text = string.Empty;
                this.gvRolesManagement.EditIndex = -1;
                this.BindRoles();
                this.SetStatus("Role '" + this.Server.HtmlEncode(roleName) + "' created.", isError: false);
            }
            catch (Exception ex)
            {
                this.SetStatus(
                    "Role '" + this.Server.HtmlEncode(roleName) + "' was not created. " + this.Server.HtmlEncode(ex.Message),
                    isError: true);
            }
        }

        protected void gvRolesManagement_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.gvRolesManagement.EditIndex = e.NewEditIndex;
            this.BindRoles();
            this.SetStatus(string.Empty, isError: false);
        }

        protected void gvRolesManagement_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.gvRolesManagement.EditIndex = -1;
            this.BindRoles();
            this.SetStatus(string.Empty, isError: false);
        }

        protected void gvRolesManagement_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            string oldRoleName = Convert.ToString(this.gvRolesManagement.DataKeys[e.RowIndex].Value);
            string newRoleName = ResolveEditedRoleName(e);

            if (string.IsNullOrEmpty(oldRoleName))
            {
                this.SetStatus("Could not resolve the role to rename.", isError: true);
                return;
            }

            if (string.IsNullOrEmpty(newRoleName))
            {
                this.SetStatus("Role name cannot be empty.", isError: true);
                return;
            }

            // Exact match (including case) — nothing to do
            if (string.Equals(oldRoleName, newRoleName, StringComparison.Ordinal))
            {
                this.gvRolesManagement.EditIndex = -1;
                this.BindRoles();
                this.SetStatus("Role name unchanged.", isError: false);
                return;
            }

            try
            {
                // Membership role names are case-insensitive for lookup, so repair → Repair
                // needs a temp role (CreateRole("Repair") fails while "repair" still exists).
                if (string.Equals(oldRoleName, newRoleName, StringComparison.OrdinalIgnoreCase))
                    this.RenameRoleCasingOnly(oldRoleName, newRoleName);
                else if (Roles.RoleExists(newRoleName))
                {
                    this.SetStatus(
                        "Role '" + this.Server.HtmlEncode(newRoleName) + "' already exists.",
                        isError: true);
                    return;
                }
                else
                    this.RenameRoleAndUsers(oldRoleName, newRoleName);

                this.gvRolesManagement.EditIndex = -1;
                this.BindRoles();
                this.SetStatus(
                    "Role renamed from '" + this.Server.HtmlEncode(oldRoleName)
                        + "' to '" + this.Server.HtmlEncode(newRoleName) + "'.",
                    isError: false);
            }
            catch (Exception ex)
            {
                this.SetStatus(
                    "Could not rename role. " + this.Server.HtmlEncode(ex.Message),
                    isError: true);
            }
        }

        private string ResolveEditedRoleName(GridViewUpdateEventArgs e)
        {
            if (e != null && e.NewValues != null && e.NewValues["RoleName"] != null)
            {
                string fromNewValues = Convert.ToString(e.NewValues["RoleName"]);
                if (!string.IsNullOrWhiteSpace(fromNewValues))
                    return fromNewValues.Trim();
            }

            var row = this.gvRolesManagement.Rows[e.RowIndex];
            var tbxRoleName = row.FindControl("tbxRoleName") as TextBox;
            if (tbxRoleName != null)
            {
                string posted = this.Request.Form[tbxRoleName.UniqueID];
                if (!string.IsNullOrWhiteSpace(posted))
                    return posted.Trim();
                if (!string.IsNullOrWhiteSpace(tbxRoleName.Text))
                    return tbxRoleName.Text.Trim();
            }

            return string.Empty;
        }

        protected void gvRolesManagement_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string roleName = Convert.ToString(this.gvRolesManagement.DataKeys[e.RowIndex].Value);
            if (string.IsNullOrEmpty(roleName))
            {
                this.SetStatus("Could not resolve the role to delete.", isError: true);
                return;
            }

            try
            {
                if (!Roles.RoleExists(roleName))
                {
                    this.SetStatus(
                        "Role '" + this.Server.HtmlEncode(roleName) + "' was not found.",
                        isError: true);
                    this.gvRolesManagement.EditIndex = -1;
                    this.BindRoles();
                    return;
                }

                // false = remove users from the role first, then delete the role
                Roles.DeleteRole(roleName, throwOnPopulatedRole: false);
                this.gvRolesManagement.EditIndex = -1;
                this.BindRoles();
                this.SetStatus(
                    "Role '" + this.Server.HtmlEncode(roleName) + "' deleted.",
                    isError: false);
            }
            catch (Exception ex)
            {
                this.SetStatus(
                    "Could not delete role '" + this.Server.HtmlEncode(roleName) + "'. "
                        + this.Server.HtmlEncode(ex.Message),
                    isError: true);
            }
        }

        protected void gvRolesManagement_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            // Only wire confirm on the display-mode Delete button
            if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
                return;

            var btnDelete = e.Row.FindControl("btnDeleteRole") as LinkButton;
            if (btnDelete == null)
                return;

            string roleName = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "RoleName")) ?? string.Empty;
            int userCount = 0;
            object countObj = DataBinder.Eval(e.Row.DataItem, "UserCount");
            if (countObj != null && countObj != DBNull.Value)
                int.TryParse(countObj.ToString(), out userCount);

            string confirmMessage = userCount > 0
                ? "Delete role '" + roleName + "'? It has " + userCount
                    + " user(s). Those users will be removed from this role."
                : "Delete role '" + roleName + "'? This cannot be undone.";

            btnDelete.OnClientClick =
                "return confirm('" + HttpUtility.JavaScriptStringEncode(confirmMessage) + "');";
        }

        /// <summary>
        /// Membership has no RenameRole API — create the new role, move users, delete the old one.
        /// </summary>
        private void RenameRoleAndUsers(string oldRoleName, string newRoleName)
        {
            string[] usersInRole = Roles.GetUsersInRole(oldRoleName);
            this.CreateRoleCompatible(newRoleName);
            if (usersInRole != null && usersInRole.Length > 0)
            {
                Roles.AddUsersToRole(usersInRole, newRoleName);
                Roles.RemoveUsersFromRole(usersInRole, oldRoleName);
            }
            Roles.DeleteRole(oldRoleName, throwOnPopulatedRole: false);
            AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                "Role renamed from " + oldRoleName + " to " + newRoleName);
        }

        /// <summary>
        /// Case-only rename (e.g. repair → Repair). Uses a temporary role because the
        /// role provider treats names as case-insensitive, so the new casing cannot be
        /// created while the old name still exists.
        /// </summary>
        private void RenameRoleCasingOnly(string oldRoleName, string newRoleName)
        {
            string tempRoleName = "tmp_" + Guid.NewGuid().ToString("N");
            if (tempRoleName.Length > 256)
                tempRoleName = tempRoleName.Substring(0, 256);

            string[] usersInRole = Roles.GetUsersInRole(oldRoleName) ?? new string[0];

            this.CreateRoleCompatible(tempRoleName);
            if (usersInRole.Length > 0)
            {
                Roles.AddUsersToRole(usersInRole, tempRoleName);
                Roles.RemoveUsersFromRole(usersInRole, oldRoleName);
            }
            Roles.DeleteRole(oldRoleName, throwOnPopulatedRole: false);

            this.CreateRoleCompatible(newRoleName);
            if (usersInRole.Length > 0)
            {
                Roles.AddUsersToRole(usersInRole, newRoleName);
                Roles.RemoveUsersFromRole(usersInRole, tempRoleName);
            }
            Roles.DeleteRole(tempRoleName, throwOnPopulatedRole: false);

            AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                "Role casing renamed from " + oldRoleName + " to " + newRoleName);
        }

        /// <summary>
        /// Uses the standard role provider first. The migrated OtterDb schema currently lacks
        /// the RoleId default expected by aspnet_Roles_CreateRole, so error 515 falls back to
        /// an explicit transactional insert with a generated GUID.
        /// </summary>
        private void CreateRoleCompatible(string roleName)
        {
            try
            {
                Roles.CreateRole(roleName);
            }
            catch (Exception ex)
            {
                if (!ContainsSqlError(ex, 515))
                    throw;

                string applicationName = Roles.Provider?.ApplicationName ?? "/";
                if (!new MembershipRepository().CreateRole(roleName, applicationName))
                    throw;

                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "ManageRoles: created role '" + roleName
                    + "' using migrated-schema RoleId fallback.");
            }
        }

        private static bool ContainsSqlError(Exception exception, int errorNumber)
        {
            for (Exception current = exception; current != null; current = current.InnerException)
            {
                var sqlException = current as SqlException;
                if (sqlException != null && sqlException.Number == errorNumber)
                    return true;
            }

            return false;
        }
    }
}
