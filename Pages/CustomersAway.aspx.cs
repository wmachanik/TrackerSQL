using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;

namespace TrackerSQL.Pages
{
    public partial class CustomersAway : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set default filter to show currently away customers
                Session["CustomersAwayWhereFilter"] = BuildWhereFilter();
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
            ddlDateFilter.SelectedIndex = 0;
            tbxFromDate.Text = "";
            tbxToDate.Text = "";
            Session["CustomersAwayWhereFilter"] = BuildWhereFilter();
            gvCustomersAway.PageIndex = 0;
            gvCustomersAway.DataBind();
        }

        protected void ddlDateFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            divCustomDateRange.Visible = (ddlDateFilter.SelectedValue == "Custom");
            if (ddlDateFilter.SelectedValue == "Custom")
            {
                DateTime today = TimeZoneUtils.Now().Date;
                tbxFromDate.Text = today.AddMonths(-1).ToString("yyyy-MM-dd");
                tbxToDate.Text = today.AddMonths(1).ToString("yyyy-MM-dd");
            }
            ApplyFilters();
        }

        protected void btnApplyDateFilter_Click(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            Session["CustomersAwayWhereFilter"] = BuildWhereFilter();
            gvCustomersAway.PageIndex = 0;
            gvCustomersAway.DataBind();
        }

        private string BuildWhereFilter()
        {
            string where = "";

            // Filter by company name
            if (ddlFilterBy.SelectedValue == "CompanyName" && !string.IsNullOrWhiteSpace(tbxFilterBy.Text))
            {
                string filter = tbxFilterBy.Text.Replace("'", "''");
                if (filter.StartsWith("%"))
                    where += $"CompanyName LIKE '{filter}'";
                else
                    where += $"CompanyName LIKE '{filter}%'";
            }

            // Date filter
            string dateFilter = ddlDateFilter.SelectedValue;
            DateTime today = TimeZoneUtils.Now().Date;

            if (dateFilter == "Current")
            {
                if (where.Length > 0) where += " AND ";
                where += $"(AwayStartDate <= #{today:yyyy-MM-dd}# AND AwayEndDate >= #{today:yyyy-MM-dd}#)";
            }
            else if (dateFilter == "All")
            {
                // No date filter
            }
            else if (dateFilter == "ThisWeek")
            {
                DateTime start = DateCalculator.StartOfWeek(today, DayOfWeek.Monday); ;
                DateTime end = start.AddDays(6);
                if (where.Length > 0) where += " AND ";
                where += $"(AwayEndDate >= #{start:yyyy-MM-dd}# AND AwayStartDate <= #{end:yyyy-MM-dd}#)";
            }
            else if (dateFilter == "NextWeek")
            {
                DateTime start = DateCalculator.StartOfWeek(today, DayOfWeek.Monday).AddDays(7);
                DateTime end = start.AddDays(6);
                if (where.Length > 0) where += " AND ";
                where += $"(AwayEndDate >= #{start:yyyy-MM-dd}# AND AwayStartDate <= #{end:yyyy-MM-dd}#)";
            }
            else if (dateFilter == "NextMonth")
            {
                DateTime start = new DateTime(today.Year, today.Month, 1).AddMonths(1);
                DateTime end = start.AddMonths(1).AddDays(-1);
                if (where.Length > 0) where += " AND ";
                where += $"(AwayEndDate >= #{start:yyyy-MM-dd}# AND AwayStartDate <= #{end:yyyy-MM-dd}#)";
            }
            else if (dateFilter == "Custom" && !string.IsNullOrEmpty(tbxFromDate.Text) && !string.IsNullOrEmpty(tbxToDate.Text))
            {
                DateTime from, to;
                if (DateTime.TryParse(tbxFromDate.Text, out from) && DateTime.TryParse(tbxToDate.Text, out to))
                {
                    if (where.Length > 0) where += " AND ";
                    where += $"(AwayStartDate >= #{from:yyyy-MM-dd}# AND AwayEndDate <= #{to:yyyy-MM-dd}#)";
                }
            }
            return where;
        }
    }
}