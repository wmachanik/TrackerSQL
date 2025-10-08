using System;
using System.Collections.Generic;
using System.Linq;
using TrackerDotNet.Controls;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Tools
{
    public partial class HolidayClosures : System.Web.UI.Page
    {
        private readonly HolidayClosureProvider _provider = new HolidayClosureProvider();
        private const string SORTDIR_KEY = "HolidayClosures_SortDir";
        private const string SORTEXP_KEY = "HolidayClosures_SortExp";

        private class ClosureRow
        {
            public int ID { get; set; }
            public DateTime ClosureDate { get; set; }
            public DateTime EndDate { get; set; }
            public int DaysClosed { get; set; }
            public bool AppliesToPrep { get; set; }
            public bool AppliesToDelivery { get; set; }
            public string ShiftStrategy { get; set; }
            public string Description { get; set; }
        }

        private DateTime RangeFrom
        {
            get => (DateTime?)ViewState["HC_RangeFrom"] ?? TimeZoneUtils.Now().Date;
            set => ViewState["HC_RangeFrom"] = value;
        }
        private DateTime RangeTo
        {
            get => (DateTime?)ViewState["HC_RangeTo"] ?? TimeZoneUtils.Now().Date;
            set => ViewState["HC_RangeTo"] = value;
        }

        private void ApplySelectedDateRange()
        {
            DateTime today = TimeZoneUtils.Now().Date;
            DateTime from;
            DateTime to;
            switch (ddlDateRange.SelectedValue)
            {
                case "NextYear":
                    from = new DateTime(today.Year + 1, 1, 1);
                    to = new DateTime(today.Year + 1, 12, 31);
                    break;
                case "LastYear":
                    from = new DateTime(today.Year - 1, 1, 1);
                    to = new DateTime(today.Year - 1, 12, 31);
                    break;
                case "Current6":
                    from = today.AddMonths(-3);
                    to = today.AddMonths(3);
                    break;
                case "ThisYear":
                default:
                    from = new DateTime(today.Year, 1, 1);
                    to = new DateTime(today.Year, 12, 31);
                    break;
            }
            RangeFrom = from;
            RangeTo = to;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Ensure a default value exists if the markup was just added
                if (string.IsNullOrEmpty(ddlDateRange.SelectedValue))
                    ddlDateRange.SelectedValue = "ThisYear";

                ApplySelectedDateRange();
                BindGrid();
            }
        }

        // MISSING HANDLER (added to fix CS1061)
        protected void ddlDateRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySelectedDateRange();
            BindGrid();
        }

        private void BindGrid()
        {
            DateTime fromDate = RangeFrom;
            DateTime toDate = RangeTo;

            var closures = _provider.GetRange(fromDate, toDate).ToList();

            if (!string.IsNullOrEmpty(ddlFilterStrategy.SelectedValue))
                closures = closures
                    .Where(h => string.Equals(h.ShiftStrategy, ddlFilterStrategy.SelectedValue, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(txtFilterText.Text))
            {
                string f = txtFilterText.Text.Trim().ToLowerInvariant();
                closures = closures
                    .Where(h => (h.Description ?? string.Empty).ToLowerInvariant().Contains(f))
                    .ToList();
            }

            var rows = new List<ClosureRow>();
            foreach (var h in closures)
            {
                int days = h.DaysClosed < 1 ? 1 : h.DaysClosed;
                rows.Add(new ClosureRow
                {
                    ID = h.ID,
                    ClosureDate = h.ClosureDate,
                    DaysClosed = days,
                    EndDate = h.ClosureDate.AddDays(days - 1),
                    AppliesToPrep = h.AppliesToPrep,
                    AppliesToDelivery = h.AppliesToDelivery,
                    ShiftStrategy = h.ShiftStrategy,
                    Description = h.Description
                });
            }

            string sortExp = Convert.ToString(ViewState[SORTEXP_KEY]) ?? "ClosureDate";
            string sortDir = Convert.ToString(ViewState[SORTDIR_KEY]) ?? "ASC";
            rows = SortRows(rows, sortExp, sortDir);

            gvClosures.DataSource = rows;
            gvClosures.DataBind();

            ltrlStatus.Text = rows.Count + " closure(s)";
        }

        private List<ClosureRow> SortRows(List<ClosureRow> rows, string exp, string dir)
        {
            bool desc = dir == "DESC";
            switch (exp)
            {
                case "ID":
                    rows = desc ? rows.OrderByDescending(r => r.ID).ToList() : rows.OrderBy(r => r.ID).ToList();
                    break;
                case "EndDate":
                    rows = desc ? rows.OrderByDescending(r => r.EndDate).ToList() : rows.OrderBy(r => r.EndDate).ToList();
                    break;
                case "DaysClosed":
                    rows = desc ? rows.OrderByDescending(r => r.DaysClosed).ToList() : rows.OrderBy(r => r.DaysClosed).ToList();
                    break;
                case "ShiftStrategy":
                    rows = desc ? rows.OrderByDescending(r => r.ShiftStrategy).ToList() : rows.OrderBy(r => r.ShiftStrategy).ToList();
                    break;
                case "Description":
                    rows = desc ? rows.OrderByDescending(r => r.Description).ToList() : rows.OrderBy(r => r.Description).ToList();
                    break;
                default:
                    rows = desc ? rows.OrderByDescending(r => r.ClosureDate).ToList() : rows.OrderBy(r => r.ClosureDate).ToList();
                    break;
            }
            return rows;
        }

        protected void btnFilter_Click(object sender, EventArgs e) => BindGrid();

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ddlDateRange.SelectedValue = "ThisYear";
            ddlFilterStrategy.SelectedIndex = 0;
            txtFilterText.Text = string.Empty;
            ApplySelectedDateRange();
            ViewState[SORTEXP_KEY] = null;
            ViewState[SORTDIR_KEY] = null;
            BindGrid();
        }

        protected void gvClosures_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvClosures.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void gvClosures_Sorting(object sender, System.Web.UI.WebControls.GridViewSortEventArgs e)
        {
            string currentExp = Convert.ToString(ViewState[SORTEXP_KEY]);
            string currentDir = Convert.ToString(ViewState[SORTDIR_KEY]) ?? "ASC";

            if (string.Equals(currentExp, e.SortExpression, StringComparison.OrdinalIgnoreCase))
                currentDir = currentDir == "ASC" ? "DESC" : "ASC";
            else
            {
                currentExp = e.SortExpression;
                currentDir = "ASC";
            }

            ViewState[SORTEXP_KEY] = currentExp;
            ViewState[SORTDIR_KEY] = currentDir;
            BindGrid();
        }

        protected void gvClosures_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            int id = (int)gvClosures.DataKeys[e.RowIndex].Value;
            string err;
            if (_provider.Delete(id, out err))
                ltrlStatus.Text = "Deleted.";
            else
                ltrlStatus.Text = "Error: " + err;
            BindGrid();
        }

        protected void btnShowAddPanel_Click(object sender, EventArgs e)
        {
            pnlAddInline.Visible = !pnlAddInline.Visible;
        }

        protected void btnCancelInline_Click(object sender, EventArgs e)
        {
            pnlAddInline.Visible = false;
            ClearInlineAdd();
        }

        private void ClearInlineAdd()
        {
            txtNewDate.Text = "";
            txtNewDays.Text = "1";
            txtNewDesc.Text = "";
            ddlNewStrategy.SelectedIndex = 0;
            chkNewPrep.Checked = true;
            chkNewDelivery.Checked = true;
        }

        protected void btnAddInline_Click(object sender, EventArgs e)
        {
            DateTime d;
            if (!DateTime.TryParse(txtNewDate.Text, out d))
            {
                ltrlStatus.Text = "Invalid start date";
                return;
            }
            int days;
            if (!int.TryParse(txtNewDays.Text, out days) || days < 1) days = 1;

            string err;
            bool ok = _provider.Insert(d, days, chkNewPrep.Checked, chkNewDelivery.Checked,
                                       ddlNewStrategy.SelectedValue, txtNewDesc.Text, out err);

            if (ok)
            {
                ltrlStatus.Text = "Added.";
                ClearInlineAdd();
                pnlAddInline.Visible = false;
                HolidayClosureProvider.Invalidate();
                BindGrid();
            }
            else
            {
                ltrlStatus.Text = "Error: " + err;
            }
        }

        // OPTIONAL: if you also wired a "Copy To Next Year" button (btnCopyToNextYear_Click) add handler here.
        protected void btnCopyToNextYear_Click(object sender, EventArgs e)
        {
            // Only proceed on valid range
            DateTime from = RangeFrom;
            DateTime to = RangeTo;
            var source = _provider.GetRange(from, to);
            if (source == null || source.Count == 0)
            {
                ltrlStatus.Text = "No closures to copy.";
                return;
            }

            int inserted = 0;
            int skipped = 0;
            foreach (var dateToCopy in source)
            {
                DateTime targetStart = dateToCopy.ClosureDate.AddYears(1);
                // Avoid duplicates (same start & span)
                var overlap = _provider.GetRange(targetStart, targetStart.AddDays(dateToCopy.DaysClosed - 1))
                    .Any(x => x.ClosureDate == targetStart && x.DaysClosed == dateToCopy.DaysClosed);
                if (overlap)
                {
                    skipped++;
                    continue;
                }

                string err;
                if (_provider.Insert(targetStart, dateToCopy.DaysClosed, dateToCopy.AppliesToPrep, dateToCopy.AppliesToDelivery, dateToCopy.ShiftStrategy, dateToCopy.Description, out err))
                    inserted++;
                else
                    skipped++;
            }

            HolidayClosureProvider.Invalidate();
            BindGrid();
            ltrlStatus.Text = $"Copied {inserted} closure(s); {skipped} skipped.";
        }
    }
}