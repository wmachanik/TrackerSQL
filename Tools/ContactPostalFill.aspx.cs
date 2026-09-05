using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Tools
{
    public partial class ContactPostalFill : Page
    {
        private readonly ContactPostalFillManager _manager = new ContactPostalFillManager();

        private const string VsRows = "ContactPostal.Rows";
        private const string VsFilter = "ContactPostal.Filter";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserCanManage())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = true;
                lblAccessDenied.Text = MessageProvider.Get(MessageKeys.SystemPreferences.AccessDenied);
            }
        }

        private bool UserCanManage()
        {
            var user = Context?.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated &&
                   (user.IsInRole("Administrators") || user.IsInRole("Admin"));
        }

        protected void btnScan_Click(object sender, EventArgs e)
        {
            var rows = _manager.ScanMissingPostalCodes(chkEnabledOnly.Checked);
            ViewState[VsRows] = rows;
            gvSuggestions.PageIndex = 0;
            BindGrid();
            SetScanSummary(rows, "Scan complete.");
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            ViewState[VsFilter] = (txtFilter.Text ?? string.Empty).Trim();
            gvSuggestions.PageIndex = 0;
            BindGrid();
        }

        protected void btnFilterClear_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            ViewState[VsFilter] = string.Empty;
            txtFilter.Text = string.Empty;
            gvSuggestions.PageIndex = 0;
            BindGrid();
        }

        protected void gvSuggestions_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            MergePageEdits();
            gvSuggestions.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void gvSuggestions_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvSuggestions, e.Row);
        }

        protected void gvSuggestions_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as ContactPostalSuggestion;
            if (row == null)
                return;
            var chk = (CheckBox)e.Row.FindControl("chkSelect");
            if (chk != null)
                chk.Checked = row.Selected;
        }

        protected void btnSelectAll_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            var visible = new HashSet<int>(FilteredRows().Select(r => r.ContactID));
            var rows = GetRows();
            foreach (var r in rows)
            {
                r.Selected = visible.Contains(r.ContactID)
                    && !string.IsNullOrWhiteSpace(r.SuggestedPostalCode);
            }
            ViewState[VsRows] = rows;
            BindGrid();
        }

        protected void btnSelectHigh_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            var rows = GetRows();
            foreach (var r in rows)
                r.Selected = r.Confidence >= 2 && !string.IsNullOrWhiteSpace(r.SuggestedPostalCode);
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

        protected void btnApply_Click(object sender, EventArgs e)
        {
            MergePageEdits();
            int n = _manager.ApplySuggestions(GetRows());
            var rows = _manager.ScanMissingPostalCodes(chkEnabledOnly.Checked);
            ViewState[VsRows] = rows;
            BindGrid();
            SetScanSummary(rows, "Updated postal code on " + n + " contact(s).");
        }

        private void SetScanSummary(List<ContactPostalSuggestion> rows, string status)
        {
            int withSug = rows.Count(r => !string.IsNullOrWhiteSpace(r.SuggestedPostalCode));
            litSummary.Text = "<p class=\"woo-map-section-note\"><strong>"
                + rows.Count + " listed</strong> (address present, not Collect). "
                + withSug + " with a suggestion. Skipped "
                + _manager.SkippedBlankAddress + " blank address, "
                + _manager.SkippedCollectArea + " Collect area.</p>";
            SetStatus(status, false);
        }

        private IEnumerable<ContactPostalSuggestion> FilteredRows()
        {
            var rows = GetRows();
            string filter = (ViewState[VsFilter] as string) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(filter))
                return rows;
            string f = filter.Trim();
            return rows.Where(r =>
                Contains(r.CompanyName, f)
                || Contains(r.BillingAddress, f)
                || Contains(r.AreaName, f)
                || Contains(r.MatchPlace, f)
                || Contains(r.SuggestedPostalCode, f)
                || Contains(r.Reason, f));
        }

        private void BindGrid()
        {
            txtFilter.Text = (ViewState[VsFilter] as string) ?? string.Empty;
            var list = FilteredRows().ToList();
            if (gvSuggestions.PageIndex > 0
                && gvSuggestions.PageIndex * gvSuggestions.PageSize >= Math.Max(list.Count, 1))
                gvSuggestions.PageIndex = 0;
            gvSuggestions.DataSource = list;
            gvSuggestions.DataBind();
        }

        private void MergePageEdits()
        {
            var rows = GetRows();
            var byId = rows.ToDictionary(r => r.ContactID);
            foreach (GridViewRow row in gvSuggestions.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int id = Convert.ToInt32(gvSuggestions.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                if (!byId.TryGetValue(id, out var target))
                    continue;
                var chk = (CheckBox)row.FindControl("chkSelect");
                var txt = (TextBox)row.FindControl("txtCode");
                target.Selected = chk != null && chk.Checked;
                if (txt != null)
                    target.SuggestedPostalCode = txt.Text;
            }
            ViewState[VsRows] = rows;
        }

        private List<ContactPostalSuggestion> GetRows()
        {
            return ViewState[VsRows] as List<ContactPostalSuggestion>
                ?? new List<ContactPostalSuggestion>();
        }

        private static bool Contains(string hay, string needle)
        {
            return !string.IsNullOrEmpty(hay)
                && hay.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= max)
                return text ?? string.Empty;
            return text.Substring(0, max) + "…";
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
