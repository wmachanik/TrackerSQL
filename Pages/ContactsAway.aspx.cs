using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class ContactsAway : Page
    {
        private const string CONST_WHERECLAUSE_SESSIONVAR = "ContactsAwayWhereFilter";
        private const string CONST_SORTEXPRESSION_VIEWSTATE = "ContactsAwaySortExpression";
        private const string CONST_DATERANGE_VIEWSTATE = "ContactsAwayDateRange";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = "CompanyName";
                // Set default filter to Next 3 Months
                ddlDateFilter.SelectedValue = "Next3Months";
                Session[CONST_WHERECLAUSE_SESSIONVAR] = BuildWhereFilter();
                BindContactsAwayGrid();
            }
        }

        private void BindContactsAwayGrid()
        {
            try
            {
                var repo = new ContactsAwayPeriodRepository();
                string sortBy = ViewState[CONST_SORTEXPRESSION_VIEWSTATE] as string ?? "CompanyName";
                string whereFilter = Session[CONST_WHERECLAUSE_SESSIONVAR] as string ?? string.Empty;

                var result = repo.GetAwaySummaries(sortBy, whereFilter);

                if (!result.Success)
                {
                    lblFilter.Text = "<span style='color:red;'>Error loading data: " + HttpUtility.HtmlEncode(result.ErrorMessage) + "</span>";
                    gvContactsAway.DataSource = null;
                    gvContactsAway.DataBind();
                    return;
                }

                var awayPeriods = result.Items;
                gvContactsAway.DataSource = awayPeriods;
                gvContactsAway.DataBind();

                // Display count and date range
                string dateRangeText = ViewState[CONST_DATERANGE_VIEWSTATE] as string ?? "";
                lblFilter.Text = $"<strong>Showing {awayPeriods.Count} away period(s)</strong>";
                if (!string.IsNullOrEmpty(dateRangeText))
                {
                    lblFilter.Text += $"<br/><span style='color:#666;'>Filter: {dateRangeText}</span>";
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactsAway BindContactsAwayGrid error: " + ex.Message);
                lblFilter.Text = "<span style='color:red;'>Error loading data: " + ex.Message + "</span>";
            }
        }

        protected void gvContactsAway_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvContactsAway.PageIndex = e.NewPageIndex;
            BindContactsAwayGrid();
        }

        protected void gvContactsAway_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = e.SortExpression;
            BindContactsAwayGrid();
        }

        protected void gvContactsAway_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditPeriod")
            {
                int awayPeriodId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect($"ContactsAwayDetail.aspx?AwayPeriodID={awayPeriodId}");
            }
            else if (e.CommandName == "DeletePeriod")
            {
                try
                {
                    int awayPeriodId = Convert.ToInt32(e.CommandArgument);
                    var repo = new ContactsAwayPeriodRepository();
                    repo.Delete(awayPeriodId);

                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"Deleted away period ID={awayPeriodId}");
                    lblFilter.Text = "<span style='color:green;'>Away period deleted successfully.</span>";
                    BindContactsAwayGrid();
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactsAway Delete error: " + ex.Message);
                    lblFilter.Text = "<span style='color:red;'>Error deleting: " + ex.Message + "</span>";
                }
            }
        }

        protected void tbxFilterBy_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        protected void btnGo_Click(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ddlFilterBy.SelectedIndex = 0;
            tbxFilterBy.Text = "";
            ddlDateFilter.SelectedValue = "Next3Months"; // Reset to default
            tbxFromDate.Text = "";
            tbxToDate.Text = "";
            divCustomDateRange.Visible = false;
            Session[CONST_WHERECLAUSE_SESSIONVAR] = BuildWhereFilter();
            gvContactsAway.PageIndex = 0;
            BindContactsAwayGrid();
        }

        protected void ddlDateFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            divCustomDateRange.Visible = (ddlDateFilter.SelectedValue == "Custom");
            if (ddlDateFilter.SelectedValue == "Custom")
            {
                DateTime today = TimeZoneUtils.Now().Date;
                tbxFromDate.Text = today.AddMonths(-1).ToString("yyyy-MM-dd");
                tbxToDate.Text = today.AddMonths(3).ToString("yyyy-MM-dd");
            }
            ApplyFilters();
        }

        protected void btnApplyDateFilter_Click(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            Session[CONST_WHERECLAUSE_SESSIONVAR] = BuildWhereFilter();
            gvContactsAway.PageIndex = 0;
            BindContactsAwayGrid();
        }

        protected string FormatAwayDate(object value)
        {
            if (value == null || value == DBNull.Value)
                return "(not set)";

            if (value is DateTime date)
                return $"{date:yyyy-MM-dd} ({date:ddd})";

            return value.ToString();
        }

        private static string DateOverlapFilter(string startDateLiteral, string endDateLiteral)
        {
            return $"(a.AwayStartDate IS NULL OR a.AwayEndDate IS NULL OR (a.AwayStartDate <= '{endDateLiteral}' AND a.AwayEndDate >= '{startDateLiteral}'))";
        }

        private string BuildWhereFilter()
        {
            string where = "";
            DateTime today = TimeZoneUtils.Now().Date;
            string dateRangeDescription = "";

            // Filter by company name
            if (ddlFilterBy.SelectedValue == "CompanyName" && !string.IsNullOrWhiteSpace(tbxFilterBy.Text))
            {
                string filter = tbxFilterBy.Text.Replace("'", "''");
                if (filter.StartsWith("%"))
                    where += $"c.CompanyName LIKE '{filter}'";
                else
                    where += $"c.CompanyName LIKE '{filter}%'";
            }

            // Date filter - using SQL Server date format
            string dateFilter = ddlDateFilter.SelectedValue;

            switch (dateFilter)
            {
                case "Current":
                    // Currently away - today falls between start and end (requires both dates)
                    if (where.Length > 0) where += " AND ";
                    where += $"(a.AwayStartDate IS NOT NULL AND a.AwayEndDate IS NOT NULL AND a.AwayStartDate <= '{today:yyyy-MM-dd}' AND a.AwayEndDate >= '{today:yyyy-MM-dd}')";
                    dateRangeDescription = $"Currently Away (as of {today:yyyy-MM-dd})";
                    break;

                case "ThisMonth":
                    var monthStart = new DateTime(today.Year, today.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    if (where.Length > 0) where += " AND ";
                    where += DateOverlapFilter(monthStart.ToString("yyyy-MM-dd"), monthEnd.ToString("yyyy-MM-dd"));
                    dateRangeDescription = $"This Month ({monthStart:MMM yyyy}: {monthStart:yyyy-MM-dd} to {monthEnd:yyyy-MM-dd})";
                    break;

                case "Next3Months":
                    var threeMonthsEnd = today.AddMonths(3);
                    if (where.Length > 0) where += " AND ";
                    where += DateOverlapFilter(today.ToString("yyyy-MM-dd"), threeMonthsEnd.ToString("yyyy-MM-dd"));
                    dateRangeDescription = $"Next 3 Months ({today:yyyy-MM-dd} to {threeMonthsEnd:yyyy-MM-dd})";
                    break;

                case "ThisYear":
                    var yearStart = new DateTime(today.Year, 1, 1);
                    var yearEnd = new DateTime(today.Year, 12, 31);
                    if (where.Length > 0) where += " AND ";
                    where += DateOverlapFilter(yearStart.ToString("yyyy-MM-dd"), yearEnd.ToString("yyyy-MM-dd"));
                    dateRangeDescription = $"This Year ({today.Year}: {yearStart:yyyy-MM-dd} to {yearEnd:yyyy-MM-dd})";
                    break;

                case "All":
                    // No date filter
                    dateRangeDescription = "All Periods (no date filter)";
                    break;

                case "Custom":
                    if (!string.IsNullOrEmpty(tbxFromDate.Text) && !string.IsNullOrEmpty(tbxToDate.Text))
                    {
                        DateTime from, to;
                        if (DateTime.TryParse(tbxFromDate.Text, out from) && DateTime.TryParse(tbxToDate.Text, out to))
                        {
                            if (where.Length > 0) where += " AND ";
                            where += DateOverlapFilter(from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));
                            dateRangeDescription = $"Custom Range ({from:yyyy-MM-dd} to {to:yyyy-MM-dd})";
                        }
                    }
                    break;
            }

            // Store the date range description for display
            ViewState[CONST_DATERANGE_VIEWSTATE] = dateRangeDescription;

            return where;
        }
    }
}


