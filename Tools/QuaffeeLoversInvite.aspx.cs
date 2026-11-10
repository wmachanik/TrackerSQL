using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Tools
{
    public partial class QuaffeeLoversInvite : Page
    {
        private readonly QuaffeeLoversMembershipManager _manager = new QuaffeeLoversMembershipManager();

        private const string VsRows = "QuaffeeLovers.Rows";
        private const string VsFilter = "QuaffeeLovers.Filter";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserCanManage())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = true;
                lblAccessDenied.Text = MessageProvider.Get(MessageKeys.SystemPreferences.AccessDenied);
                return;
            }

            if (!IsPostBack)
                BindFeatureNote();
        }

        private bool UserCanManage()
        {
            var user = Context?.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated &&
                   (user.IsInRole("Administrators") || user.IsInRole("Admin"));
        }

        private void BindFeatureNote()
        {
            int threshold = _manager.GetOrderThreshold();
            if (_manager.IsFeatureEnabled())
            {
                litFeatureNote.Text = "<p class=\"woo-map-section-note\">Quaffee Lovers is <strong>enabled</strong>. "
                    + "Order threshold: <strong>" + threshold + "</strong>. Woo role: <strong>"
                    + HttpUtility.HtmlEncode(_manager.GetTargetRole()) + "</strong>.</p>";
                return;
            }

            litFeatureNote.Text = "<p class=\"woo-map-section-note status-error\">Quaffee Lovers is "
                + "<strong>disabled</strong> or Woo integration is off. You can still scan, but invite will not run "
                + "until you enable it in WooCommerce Mapping (threshold currently "
                + threshold + ").</p>";
        }

        protected void btnScan_Click(object sender, EventArgs e)
        {
            _manager.EnsureReady();
            var rows = _manager.ScanInviteCandidates();
            ViewState[VsRows] = rows;
            gvInvites.PageIndex = 0;
            BindGrid();
            SetScanSummary(rows, "Scan complete. Select rows and click Verify on shop, then Invite selected when ready.");
            BindFeatureNote();
        }

        protected void btnCheckShop_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            var rows = GetRows();
            if (rows.Count == 0)
            {
                SetStatus("Run Scan contacts first.", true);
                return;
            }

            var ids = rows.Where(r => r.Selected).Select(r => r.ContactID).ToList();
            if (ids.Count == 0)
                ids = rows.Select(r => r.ContactID).ToList();

            string user = User.Identity?.Name ?? "system";
            QuaffeeLoversShopCheckResult check = _manager.RefreshShopRoles(ids, user);
            if (check.Checked == 0)
            {
                SetStatus(check.Errors.Count > 0 ? string.Join(" ", check.Errors) : "No contacts to check.", true);
                return;
            }

            var fresh = _manager.ScanInviteCandidates();
            ViewState[VsRows] = fresh;
            BindGrid();

            var sb = new StringBuilder();
            sb.Append("Checked ").Append(check.Checked).Append(" on the shop: ")
                .Append(check.AlreadyMember).Append(" already ")
                .Append(_manager.GetTargetRole()).Append(", ")
                .Append(check.OnShopNotMember).Append(" on shop but not ")
                .Append(_manager.GetTargetRole()).Append(", ")
                .Append(check.NotOnShop).Append(" not on shop.");
            if (check.Errors.Count > 0)
                sb.Append(" Issues: ").Append(string.Join("; ", check.Errors.Take(6)));
            SetScanSummary(GetRows(), sb.ToString());
        }

        protected void btnDisableMember_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            string user = User.Identity?.Name ?? "system";
            var rows = GetRows().Where(r => r.Selected).ToList();
            if (rows.Count == 0)
            {
                SetStatus("Select at least one contact.", true);
                return;
            }

            int ok = 0;
            var errors = new List<string>();
            foreach (var row in rows)
            {
                QuaffeeLoversProcessResult result = _manager.TryDisableMember(row.ContactID, user);
                if (result.Succeeded)
                    ok++;
                else if (!string.IsNullOrWhiteSpace(result.Message))
                    errors.Add(row.ContactID + ": " + result.Message);
            }

            var fresh = _manager.ScanInviteCandidates();
            ViewState[VsRows] = fresh;
            BindGrid();
            var sb = new StringBuilder();
            sb.Append("Disabled membership for ").Append(ok).Append(" of ").Append(rows.Count).Append(" contact(s).");
            if (errors.Count > 0)
                sb.Append(" Issues: ").Append(string.Join("; ", errors.Take(6)));
            SetScanSummary(fresh, sb.ToString());
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            ViewState[VsFilter] = (txtFilter.Text ?? string.Empty).Trim();
            gvInvites.PageIndex = 0;
            BindGrid();
        }

        protected void btnFilterClear_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            ViewState[VsFilter] = string.Empty;
            txtFilter.Text = string.Empty;
            gvInvites.PageIndex = 0;
            BindGrid();
        }

        protected void gvInvites_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            MergePageEdits();
            gvInvites.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void gvInvites_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvInvites, e.Row);
        }

        protected void gvInvites_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as QuaffeeLoversInviteRow;
            if (row == null)
                return;
            var chk = (CheckBox)e.Row.FindControl("chkSelect");
            if (chk != null)
                chk.Checked = row.Selected;
            var lbl = (Label)e.Row.FindControl("lblStatus");
            if (lbl != null)
                lbl.Text = FormatInviteStatus(row, _manager.GetTargetRole());
        }

        protected void btnSelectAll_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            var visible = new HashSet<int>(FilteredRows().Select(r => r.ContactID));
            var rows = GetRows();
            foreach (var r in rows)
                r.Selected = visible.Contains(r.ContactID);
            ViewState[VsRows] = rows;
            BindGrid();
        }

        protected void btnClearSelect_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            var rows = GetRows();
            foreach (var r in rows)
                r.Selected = false;
            ViewState[VsRows] = rows;
            BindGrid();
        }

        protected void btnInvite_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            if (!_manager.IsFeatureEnabled())
            {
                SetStatus("Enable Quaffee Lovers in WooCommerce Mapping first.", true);
                return;
            }

            string user = User.Identity?.Name ?? "system";
            var rows = GetRows().Where(r => r.Selected).ToList();
            if (rows.Count == 0)
            {
                SetStatus("Select at least one contact.", true);
                return;
            }

            int ok = 0;
            var errors = new List<string>();
            foreach (var row in rows)
            {
                QuaffeeLoversProcessResult result = _manager.InviteContact(row.ContactID, user);
                if (result.Succeeded)
                    ok++;
                else if (!string.IsNullOrWhiteSpace(result.Message))
                    errors.Add(row.ContactID + ": " + result.Message);
            }

            RefreshAfterAction(rows.Count, ok, errors, "Invited");
        }

        protected void btnDismiss_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            string user = User.Identity?.Name ?? "system";
            var rows = GetRows().Where(r => r.Selected).ToList();
            if (rows.Count == 0)
            {
                SetStatus("Select at least one contact.", true);
                return;
            }

            foreach (var row in rows)
                _manager.DismissContact(row.ContactID, user, "Dismissed from invite tool.");

            var fresh = _manager.ScanInviteCandidates();
            ViewState[VsRows] = fresh;
            BindGrid();
            SetScanSummary(fresh, "Dismissed " + rows.Count + " contact(s).");
        }

        private void RefreshAfterAction(int attempted, int ok, List<string> errors, string verb)
        {
            var fresh = _manager.ScanInviteCandidates();
            ViewState[VsRows] = fresh;
            BindGrid();
            var sb = new StringBuilder();
            sb.Append(verb).Append(" ").Append(ok).Append(" of ").Append(attempted).Append(" contact(s).");
            if (errors.Count > 0)
                sb.Append(" Issues: ").Append(string.Join("; ", errors.Take(8)));
            if (errors.Count > 8)
                sb.Append(" (and ").Append(errors.Count - 8).Append(" more)");
            SetScanSummary(fresh, sb.ToString());
        }

        private void SetScanSummary(List<QuaffeeLoversInviteRow> rows, string status)
        {
            litSummary.Text = "<p class=\"woo-map-section-note\"><strong>"
                + rows.Count + " pending invite(s)</strong> at or above the order threshold.</p>";
            SetStatus(status, false);
        }

        private IEnumerable<QuaffeeLoversInviteRow> FilteredRows()
        {
            var rows = GetRows();
            string filter = (ViewState[VsFilter] as string) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(filter))
                return rows;
            string f = filter.Trim();
            return rows.Where(r =>
                Contains(r.CompanyName, f)
                || Contains(r.Email, f)
                || Contains(r.Notes, f)
                || r.ContactID.ToString(CultureInfo.InvariantCulture).IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void BindGrid()
        {
            txtFilter.Text = (ViewState[VsFilter] as string) ?? string.Empty;
            var list = FilteredRows().ToList();
            if (gvInvites.PageIndex > 0
                && gvInvites.PageIndex * gvInvites.PageSize >= Math.Max(list.Count, 1))
                gvInvites.PageIndex = 0;
            gvInvites.DataSource = list;
            gvInvites.DataBind();
        }

        private void MergePageEdits()
        {
            var rows = GetRows();
            var byId = rows.ToDictionary(r => r.ContactID);
            foreach (GridViewRow row in gvInvites.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int id = Convert.ToInt32(gvInvites.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                if (!byId.TryGetValue(id, out var target))
                    continue;
                var chk = (CheckBox)row.FindControl("chkSelect");
                target.Selected = chk != null && chk.Checked;
            }
            ViewState[VsRows] = rows;
        }

        private List<QuaffeeLoversInviteRow> GetRows() =>
            ViewState[VsRows] as List<QuaffeeLoversInviteRow> ?? new List<QuaffeeLoversInviteRow>();

        private static bool Contains(string hay, string needle) =>
            !string.IsNullOrEmpty(hay)
            && hay.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        public static string FormatInviteStatus(QuaffeeLoversInviteRow row, string targetRole)
        {
            if (row == null)
                return string.Empty;
            if (row.Dismissed)
                return "Dismissed";
            if (QuaffeeLoversMembershipManager.IsMemberOnShop(row.WooRoleOnShop, targetRole))
            {
                if (!string.IsNullOrWhiteSpace(row.WooRoleOnShop))
                    return "Member on shop (" + row.WooRoleOnShop.Trim() + ")";
                return "Member on shop";
            }
            if (row.WelcomeSent)
                return "Welcome sent (not member)";
            if (!string.IsNullOrWhiteSpace(row.WooRoleOnShop))
                return "On shop as " + row.WooRoleOnShop.Trim();
            return "Eligible";
        }

        private void SetStatus(string message, bool isError)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                lblMessage.Visible = false;
                return;
            }
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = "status-message " + (isError ? "status-error" : "status-success");
        }
    }
}
