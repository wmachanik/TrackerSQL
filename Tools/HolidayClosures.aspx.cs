using System;
using System.Collections.Generic;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Tools
{
    public partial class HolidayClosures : System.Web.UI.Page
    {
        private readonly HolidayClosureManager _manager = new HolidayClosureManager();
        private const string SORTDIR_KEY = "HolidayClosures_SortDir";
        private const string SORTEXP_KEY = "HolidayClosures_SortExp";

        private class ClosureRow
        {
            public int HolidayClosureID { get; set; }
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
                if (string.IsNullOrEmpty(ddlDateRange.SelectedValue))
                    ddlDateRange.SelectedValue = "ThisYear";

                ApplySelectedDateRange();
                BindGrid();
            }
        }

        protected void ddlDateRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySelectedDateRange();
            BindGrid();
        }

        private void BindGrid()
        {
            var closures = _manager.GetRange(RangeFrom, RangeTo).ToList();

            if (!string.IsNullOrEmpty(ddlFilterStrategy.SelectedValue))
            {
                closures = closures
                    .Where(h => string.Equals(h.ShiftStrategy, ddlFilterStrategy.SelectedValue, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(txtFilterText.Text))
            {
                string filter = txtFilterText.Text.Trim().ToLowerInvariant();
                closures = closures
                    .Where(h => (h.Description ?? string.Empty).ToLowerInvariant().Contains(filter))
                    .ToList();
            }

            var rows = new List<ClosureRow>();
            foreach (var closure in closures)
            {
                int days = HolidayClosureManager.GetDaysClosed(closure);
                rows.Add(new ClosureRow
                {
                    HolidayClosureID = closure.HolidayClosureID,
                    ClosureDate = closure.ClosureDate.Date,
                    DaysClosed = days,
                    EndDate = HolidayClosureManager.GetEndDate(closure),
                    AppliesToPrep = HolidayClosureManager.AppliesPrep(closure),
                    AppliesToDelivery = HolidayClosureManager.AppliesDelivery(closure),
                    ShiftStrategy = closure.ShiftStrategy,
                    Description = closure.Description
                });
            }

            string sortExp = Convert.ToString(ViewState[SORTEXP_KEY]) ?? "ClosureDate";
            string sortDir = Convert.ToString(ViewState[SORTDIR_KEY]) ?? "ASC";
            rows = SortRows(rows, sortExp, sortDir);

            gvClosures.DataSource = rows;
            gvClosures.DataBind();
            ltrlStatus.Text = rows.Count + " closure(s)";
        }

        private static List<ClosureRow> SortRows(List<ClosureRow> rows, string exp, string dir)
        {
            bool desc = dir == "DESC";
            switch (exp)
            {
                case "HolidayClosureID":
                    return desc
                        ? rows.OrderByDescending(r => r.HolidayClosureID).ToList()
                        : rows.OrderBy(r => r.HolidayClosureID).ToList();
                case "EndDate":
                    return desc
                        ? rows.OrderByDescending(r => r.EndDate).ToList()
                        : rows.OrderBy(r => r.EndDate).ToList();
                case "DaysClosed":
                    return desc
                        ? rows.OrderByDescending(r => r.DaysClosed).ToList()
                        : rows.OrderBy(r => r.DaysClosed).ToList();
                case "ShiftStrategy":
                    return desc
                        ? rows.OrderByDescending(r => r.ShiftStrategy).ToList()
                        : rows.OrderBy(r => r.ShiftStrategy).ToList();
                case "Description":
                    return desc
                        ? rows.OrderByDescending(r => r.Description).ToList()
                        : rows.OrderBy(r => r.Description).ToList();
                default:
                    return desc
                        ? rows.OrderByDescending(r => r.ClosureDate).ToList()
                        : rows.OrderBy(r => r.ClosureDate).ToList();
            }
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
            if (_manager.Delete(id, out string err))
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
            txtNewDate.Text = string.Empty;
            txtNewDays.Text = "1";
            txtNewDesc.Text = string.Empty;
            ddlNewStrategy.SelectedIndex = 0;
            chkNewPrep.Checked = true;
            chkNewDelivery.Checked = true;
        }

        protected void btnAddInline_Click(object sender, EventArgs e)
        {
            if (!DateTime.TryParse(txtNewDate.Text, out DateTime startDate))
            {
                ltrlStatus.Text = "Invalid start date";
                return;
            }

            if (!int.TryParse(txtNewDays.Text, out int days) || days < 1)
                days = 1;

            if (_manager.Insert(
                startDate,
                days,
                chkNewPrep.Checked,
                chkNewDelivery.Checked,
                ddlNewStrategy.SelectedValue,
                txtNewDesc.Text,
                out string err))
            {
                ltrlStatus.Text = "Added.";
                ClearInlineAdd();
                pnlAddInline.Visible = false;
                BindGrid();
            }
            else
            {
                ltrlStatus.Text = "Error: " + err;
            }
        }

        protected void btnCopyToNextYear_Click(object sender, EventArgs e)
        {
            var source = _manager.GetRange(RangeFrom, RangeTo);
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
                int days = HolidayClosureManager.GetDaysClosed(dateToCopy);

                // Same uniqueness / range-overlap rules as Insert (skip duplicates quietly).
                if (!_manager.ValidateUniqueDateRange(targetStart, days, excludeId: 0, out _))
                {
                    skipped++;
                    continue;
                }

                if (_manager.Insert(
                    targetStart,
                    days,
                    HolidayClosureManager.AppliesPrep(dateToCopy),
                    HolidayClosureManager.AppliesDelivery(dateToCopy),
                    dateToCopy.ShiftStrategy,
                    dateToCopy.Description,
                    out _))
                {
                    inserted++;
                }
                else
                {
                    skipped++;
                }
            }

            BindGrid();
            ltrlStatus.Text = $"Copied {inserted} closure(s); {skipped} skipped.";
        }
    }
}
