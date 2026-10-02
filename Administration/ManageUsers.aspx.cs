//------------------------------------------------------------------------------
// TrackerSQL v3.x — ManageUsers
// Administration page code-behind for ManageUsers.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Administration
{
    public partial class ManageUsers : Page
    {
        protected GridView gvUserAccounts;
        protected Label lblStatusMessage;
        protected DropDownList ddlShow;
        protected TextBox txtSearch;
        protected Button btnSearch;
        protected Button btnClear;
        protected Panel pnlUnused;
        protected Literal litUnused;
        protected Button btnShowUnused;
        protected Button btnDeleteUnused;
        protected Literal litCount;

        private readonly UserAccountManager _accounts = new UserAccountManager();

        private string SortField
        {
            get => ViewState["SortField"] as string ?? "UserName";
            set => ViewState["SortField"] = value;
        }

        private bool SortDescending
        {
            get => ViewState["SortDesc"] is bool b && b;
            set => ViewState["SortDesc"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.Page.IsPostBack)
                return;
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "User management page entered.");
            this.BindUserAccounts();
        }

        private void BindUserAccounts()
        {
            List<UserAccountRow> all = _accounts.ListAccounts();
            IEnumerable<UserAccountRow> rows = all;

            switch (ddlShow.SelectedValue)
            {
                case "staff": rows = rows.Where(r => !r.IsContact); break;
                case "contact": rows = rows.Where(r => r.IsContact); break;
                case "unused": rows = rows.Where(UserAccountManager.IsUnused); break;
            }

            string search = (txtSearch.Text ?? string.Empty).Trim();
            if (search.Length > 0)
                rows = rows.Where(r => Contains(r.UserName, search) || Contains(r.Email, search) || Contains(r.CompanyName, search));

            List<UserAccountRow> list = Sort(rows).ToList();
            this.gvUserAccounts.DataSource = list;
            this.gvUserAccounts.DataBind();

            litCount.Text = HttpUtility.HtmlEncode(list.Count == all.Count
                ? all.Count + " logins. "
                : list.Count + " of " + all.Count + " logins. ");

            BindUnusedBar(all);
        }

        private void BindUnusedBar(List<UserAccountRow> all)
        {
            string me = User?.Identity?.Name;
            int deletable = all.Count(r => UserAccountManager.IsUnused(r) && !r.IsAdmin
                && !string.Equals(r.UserName, me, StringComparison.OrdinalIgnoreCase));
            int unusedAdmins = all.Count(r => UserAccountManager.IsUnused(r) && r.IsAdmin);

            pnlUnused.Visible = deletable > 0 || unusedAdmins > 0;
            string since = UserAccountManager.UnusedBefore.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
            litUnused.Text = HttpUtility.HtmlEncode(
                (deletable == 1 ? "1 login has" : deletable + " logins have") + " not been used since " + since
                + " (" + UserAccountManager.UnusedYears + " years) and can be deleted."
                + (unusedAdmins > 0
                    ? " " + unusedAdmins + " unused administrator login(s) are left out; delete those one by one from Manage."
                    : string.Empty)) + " ";

            btnDeleteUnused.Visible = deletable > 0;
            btnDeleteUnused.Text = "Delete " + deletable + " unused login" + (deletable == 1 ? string.Empty : "s");
            btnDeleteUnused.OnClientClick = "return confirm('" + HttpUtility.JavaScriptStringEncode(
                "Delete " + deletable + " login(s) not used since " + since + "? Their Contact Portal access and driver app sign-ins go too. This cannot be undone.") + "');";
            btnShowUnused.Visible = ddlShow.SelectedValue != "unused";
        }

        private IEnumerable<UserAccountRow> Sort(IEnumerable<UserAccountRow> rows)
        {
            Func<UserAccountRow, object> key;
            switch (SortField)
            {
                case "Email": key = r => r.Email ?? string.Empty; break;
                case "Type": key = r => (r.IsContact ? "2" : "1") + (r.IsContact ? r.CompanyName : r.Roles); break;
                case "LastUsed": key = r => r.LastUsed; break;
                default: key = r => r.UserName ?? string.Empty; break;
            }
            var ordered = SortDescending ? rows.OrderByDescending(key) : rows.OrderBy(key);
            return ordered.ThenBy(r => r.UserName, StringComparer.OrdinalIgnoreCase);
        }

        private static bool Contains(string value, string part)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvUserAccounts.PageIndex = 0;
            BindUserAccounts();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlShow.SelectedValue = "all";
            txtSearch.Text = string.Empty;
            gvUserAccounts.PageIndex = 0;
            BindUserAccounts();
        }

        protected void btnShowUnused_Click(object sender, EventArgs e)
        {
            ddlShow.SelectedValue = "unused";
            SortField = "LastUsed";
            SortDescending = false;
            gvUserAccounts.PageIndex = 0;
            BindUserAccounts();
        }

        protected void btnDeleteUnused_Click(object sender, EventArgs e)
        {
            string me = User?.Identity?.Name;
            List<UserAccountRow> unused = _accounts.ListDeletableUnused(me);
            int deleted = 0;
            var failed = new List<string>();
            foreach (var row in unused)
            {
                try
                {
                    if (_accounts.DeleteUser(row.UserName, me, out string error))
                        deleted++;
                    else
                        failed.Add(row.UserName);
                }
                catch (Exception ex)
                {
                    failed.Add(row.UserName);
                    AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                        "ManageUsers: delete of unused login '" + row.UserName + "' failed: " + ex.Message);
                }
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                "ManageUsers: " + me + " deleted " + deleted + " login(s) not used since "
                + UserAccountManager.UnusedBefore.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                + (failed.Count > 0 ? "; failed: " + string.Join(", ", failed) : string.Empty));

            SetStatus("Deleted " + deleted + " unused login(s)."
                + (failed.Count > 0 ? " Could not delete: " + string.Join(", ", failed) + "." : string.Empty),
                isError: failed.Count > 0);
            gvUserAccounts.PageIndex = 0;
            BindUserAccounts();
        }

        protected void gvUserAccounts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUserAccounts.PageIndex = e.NewPageIndex;
            BindUserAccounts();
        }

        protected void gvUserAccounts_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (SortField == e.SortExpression)
                SortDescending = !SortDescending;
            else
            {
                SortField = e.SortExpression;
                SortDescending = e.SortExpression == "LastUsed";
            }
            gvUserAccounts.PageIndex = 0;
            BindUserAccounts();
        }

        protected void gvUserAccounts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var user = e.Row.DataItem as UserAccountRow;
            if (user == null)
                return;

            var unlockShell = e.Row.FindControl("spnUnlockUser") as HtmlGenericControl;
            if (unlockShell != null)
                unlockShell.Visible = user.IsLockedOut;

            var btnUnlockUser = e.Row.FindControl("btnUnlockUser") as LinkButton;
            if (btnUnlockUser != null && user.IsLockedOut)
            {
                string confirmMessage = "Unlock account '" + user.UserName + "'?";
                btnUnlockUser.OnClientClick =
                    "return confirm('" + HttpUtility.JavaScriptStringEncode(confirmMessage) + "');";
            }

            if (e.Row.FindControl("litType") is Literal litType)
            {
                if (user.IsContact)
                {
                    string company = user.ContactID.HasValue
                        ? "<a href=\"" + ResolveUrl("~/Pages/ContactDetails.aspx?ID=" + user.ContactID.Value.ToString(CultureInfo.InvariantCulture)) + "\">"
                          + HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(user.CompanyName) ? "contact #" + user.ContactID.Value : user.CompanyName) + "</a>"
                        : "<em>no contact linked</em>";
                    litType.Text = "<span class=\"users-type is-contact\">Contact Portal</span> " + company;
                }
                else
                {
                    litType.Text = "<span class=\"users-type\">Staff</span> "
                        + "<span class=\"small\">" + HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(user.Roles) ? "no roles" : user.Roles) + "</span>";
                }
            }

            if (e.Row.FindControl("litLastUsed") is Literal litLastUsed)
            {
                bool unused = UserAccountManager.IsUnused(user);
                string text = user.NeverUsed
                    ? "never (created " + user.Created.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ")"
                    : user.LastUsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                      + (user.LastUsedHow == "Tracker" ? string.Empty : " · " + user.LastUsedHow);
                litLastUsed.Text = "<span class=\"" + (unused ? "users-last is-unused" : "users-last") + "\" title=\""
                    + HttpUtility.HtmlAttributeEncode(user.LastUsed.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)) + "\">"
                    + HttpUtility.HtmlEncode(text) + "</span>";
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
            lblStatusMessage.Text = HttpUtility.HtmlEncode(message);
            lblStatusMessage.CssClass = isError ? "status-error" : "status-info";
        }
    }
}
