// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.Repairs
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using AjaxControlToolkit;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;
using TrackerDotNet.Managers;

namespace TrackerDotNet.Pages
{
    public partial class Repairs : Page
    {
        private const string CONST_WHERECLAUSE_SESSIONVAR = "CustomerRepairWhereFilter";
        private const int CONST_GVCOL_CONTACTNAME = 4;
        private const int CONST_GVCOL_JOBCARD = 5;
        private const int CONST_GVCOL_EQUIPMENT = 6;
        private const int CONST_GVCOL_MACHINESN = 7;
        private const int CONST_GVCOL_FAULT = 8;
        private const int CONST_GVCOL_FAULTDESC = 9;
        private const int CONST_GVCOL_ROID = 10;
        
        protected ScriptManager smRepairsSummary;
        protected UpdateProgress uprgRepairsSummary;
        protected UpdatePanel upnlSelection;
        protected DropDownList ddlFilterBy;
        protected TextBox tbxFilterBy;
        protected Button btnGo;
        protected Button btnReset;
        protected DropDownList ddlDateFilter;
        protected System.Web.UI.HtmlControls.HtmlGenericControl divCustomDateRange;
        protected TextBox tbxFromDate;
        protected TextBox tbxToDate;
        protected Button btnApplyDateFilter;
        protected DropDownList ddlRepairStatus;
        protected HyperLink hlAddRepair;
        protected UpdatePanel upnlRepairsSummary;
        protected GridView gvRepairs;
        protected ObjectDataSource odsRepairs;
        protected ObjectDataSource odsRepairsStatuses;
        protected Label lblFilter;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            bool flag = new CheckBrowser().fBrowserIsMobile();
            this.Session["RunningOnMoble"] = (object)flag;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RestoreFilterSettings();
            }
            
            if (this.IsPostBack || !(bool)this.Session["RunningOnMoble"])
                return;
            this.tbxFilterBy.Width = new Unit(8.0, UnitType.Em);
        }

        private void RestoreFilterSettings()
        {
            // Restore repair status dropdown selection
            if (Session["RepairStatusSelection"] != null)
            {
                string savedRepairStatus = Session["RepairStatusSelection"].ToString();
                if (ddlRepairStatus.Items.FindByValue(savedRepairStatus) != null)
                {
                    ddlRepairStatus.SelectedValue = savedRepairStatus;
                }
            }

            // Restore filter by dropdown selection
            if (Session["RepairFilterBySelection"] != null)
            {
                string savedFilterBy = Session["RepairFilterBySelection"].ToString();
                if (ddlFilterBy.Items.FindByValue(savedFilterBy) != null)
                {
                    ddlFilterBy.SelectedValue = savedFilterBy;
                }
            }

            // Restore filter text
            if (Session["RepairFilterByText"] != null)
            {
                tbxFilterBy.Text = Session["RepairFilterByText"].ToString();
            }

            // Restore date filter dropdown selection
            if (Session["RepairDateFilterSelection"] != null)
            {
                string savedDateFilter = Session["RepairDateFilterSelection"].ToString();
                if (ddlDateFilter.Items.FindByValue(savedDateFilter) != null)
                {
                    ddlDateFilter.SelectedValue = savedDateFilter;
                }
            }

            // Restore custom date range if it was selected
            if (ddlDateFilter.SelectedValue == "Custom")
            {
                divCustomDateRange.Visible = true;
                
                if (Session["RepairFilterFromDate"] != null && Session["RepairFilterFromDate"] is DateTime fromDate)
                {
                    tbxFromDate.Text = fromDate.ToString("yyyy-MM-dd");
                }
                
                if (Session["RepairFilterToDate"] != null && Session["RepairFilterToDate"] is DateTime toDate)
                {
                    tbxToDate.Text = toDate.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                divCustomDateRange.Visible = false;
            }

            // Only initialize with defaults if no existing filter settings
            if (Session["RepairDateFilterSelection"] == null && Session["RepairStatusSelection"] == null)
            {
                InitializeDateFilters();
            }
        }

        private void InitializeDateFilters()
        {
            // Set default values only if no existing settings
            Session["RepairFilterFromDate"] = null;
            Session["RepairFilterToDate"] = null;
            Session["RepairDateFilterSelection"] = "All";
            Session["RepairStatusSelection"] = "OPEN";
            Session["RepairFilterBySelection"] = "DateLogged";
            Session["RepairFilterByText"] = "";
            
            ddlDateFilter.SelectedValue = "All";
            ddlRepairStatus.SelectedValue = "OPEN";
            ddlFilterBy.SelectedIndex = 0;
            tbxFilterBy.Text = "";
        }

        public string GetCompanyName(long pCompanyID)
        {
            return pCompanyID > 0L ? new CompanyNames().GetCompanyNameByCompanyID(pCompanyID) : string.Empty;
        }

        public string GetMachineDesc(int pEquipID)
        {
            return pEquipID > 0 ? new EquipTypeTbl().GetEquipName(pEquipID) : string.Empty;
        }

        public string GetRepairFaultDesc(int pRepairFaultID)
        {
            return pRepairFaultID > 0 ? new RepairFaultsTbl().GetRepairFaultDesc(pRepairFaultID) : string.Empty;
        }

        public string GetRepairStatusDesc(int pRepairStatusID)
        {
            return pRepairStatusID > 0 ? new RepairStatusesTbl().GetRepairStatusDesc(pRepairStatusID) : string.Empty;
        }

        protected void btnGo_Click(object sender, EventArgs e)
        {
            // Save current filter settings
            Session["RepairFilterBySelection"] = ddlFilterBy.SelectedValue;
            Session["RepairFilterByText"] = tbxFilterBy.Text;
            // ✅ FIX: Clear any previous messages
            lblFilter.Text = "";
            // Trigger the data refresh
            this.odsRepairs.DataBind();
            this.upnlRepairsSummary.Update();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            this.Session["CustomerRepairWhereFilter"] = (object)"";
            this.ddlFilterBy.SelectedIndex = 0;
            this.tbxFilterBy.Text = "";
            this.ddlDateFilter.SelectedValue = "All";
            this.ddlRepairStatus.SelectedValue = "OPEN";
            this.divCustomDateRange.Visible = false;
            this.tbxFromDate.Text = "";
            this.tbxToDate.Text = "";
            
            // Reset all filter session variables
            Session["RepairFilterFromDate"] = null;
            Session["RepairFilterToDate"] = null;
            Session["RepairDateFilterSelection"] = "All";
            Session["RepairStatusSelection"] = "OPEN";
            Session["RepairFilterBySelection"] = "DateLogged";
            Session["RepairFilterByText"] = "";
            
            this.odsRepairs.DataBind();
            this.upnlSelection.Update();
            this.upnlRepairsSummary.Update();
        }

        protected void tbxFilterBy_TextChanged(object sender, EventArgs e)
        {
            // Save current filter text
            Session["RepairFilterByText"] = tbxFilterBy.Text;
            
            if (string.IsNullOrWhiteSpace(this.tbxFilterBy.Text) || this.ddlFilterBy.SelectedIndex != 0)
                return;
            this.ddlFilterBy.SelectedIndex = 1;
            
            // Save the updated filter by selection
            Session["RepairFilterBySelection"] = ddlFilterBy.SelectedValue;
            
            this.upnlSelection.Update();
        }

        protected void ddlRepairStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Save the current selection
            Session["RepairStatusSelection"] = ddlRepairStatus.SelectedValue;

            // ✅ FIX: If "Done" status is selected and no date filter is active, default to "This Month"
            if (ddlRepairStatus.SelectedValue == "7" && // Done status
                (Session["RepairFilterFromDate"] == null && Session["RepairFilterToDate"] == null) &&
                ddlDateFilter.SelectedValue == "All")
            {
                // Set date filter to This Month to avoid huge result sets
                ddlDateFilter.SelectedValue = "ThisMonth";
                Session["RepairDateFilterSelection"] = "ThisMonth";
                ApplyDateFilter("ThisMonth");

                // Show feedback to user
                lblFilter.Text = "Note: Date filter set to 'This Month' to limit results when viewing Done repairs.";
                lblFilter.ForeColor = System.Drawing.Color.Blue;

                // Update the date filter panel
                this.upnlSelection.Update();
            }
            this.odsRepairs.DataBind();
            this.upnlRepairsSummary.Update();
        }

        protected void ddlDateFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFilter = ddlDateFilter.SelectedValue;

            // Save the current selection
            Session["RepairDateFilterSelection"] = selectedFilter;

            // Show/hide custom date range controls
            divCustomDateRange.Visible = (selectedFilter == "Custom");

            if (selectedFilter != "Custom")
            {
                // Clear custom date inputs
                tbxFromDate.Text = "";
                tbxToDate.Text = "";

                // Apply predefined date filter
                ApplyDateFilter(selectedFilter);
                lblFilter.Text = "";
            }
            else
            {
                // ✅ FIX: Set default custom dates to last month and today
                var today = TimeZoneUtils.Now().Date;
                var oneMonthAgo = today.AddMonths(-1);

                tbxFromDate.Text = oneMonthAgo.ToString("yyyy-MM-dd");
                tbxToDate.Text = today.ToString("yyyy-MM-dd");

                // ✅ Automatically apply the default range
                Session["RepairFilterFromDate"] = oneMonthAgo;
                Session["RepairFilterToDate"] = today;

                lblFilter.Text = "Custom date range set to last month. Adjust dates and click Apply if needed.";
                lblFilter.ForeColor = System.Drawing.Color.Green;
            }

            this.upnlSelection.Update();

            // ✅ Always refresh data, including custom with defaults
            this.odsRepairs.DataBind();
            this.upnlRepairsSummary.Update();
        }

        protected void btnApplyDateFilter_Click(object sender, EventArgs e)
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;

            // Parse custom date range
            if (!string.IsNullOrEmpty(tbxFromDate.Text))
            {
                if (DateTime.TryParse(tbxFromDate.Text, out DateTime parsedFromDate))
                {
                    fromDate = parsedFromDate.Date;
                }
            }

            if (!string.IsNullOrEmpty(tbxToDate.Text))
            {
                if (DateTime.TryParse(tbxToDate.Text, out DateTime parsedToDate))
                {
                    toDate = parsedToDate.Date;
                }
            }

            // Set session variables for custom date range
            Session["RepairFilterFromDate"] = fromDate;
            Session["RepairFilterToDate"] = toDate;
            Session["RepairDateFilterSelection"] = "Custom";

            lblFilter.Text = $"Applied custom range: {fromDate?.ToString("yyyy-MM-dd")} to {toDate?.ToString("yyyy-MM-dd")}";
            lblFilter.ForeColor = System.Drawing.Color.Blue;

            // Refresh the grid
            this.odsRepairs.DataBind();
            this.upnlRepairsSummary.Update();
        }
        private void ApplyDateFilter(string dateFilter)
        {
            var repairManager = new RepairManager();
            DateTime? fromDate = null;
            DateTime? toDate = null;

            var today = TimeZoneUtils.Now().Date;

            switch (dateFilter?.ToUpper())
            {
                case "THISWEEK":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    fromDate = startOfWeek;
                    toDate = startOfWeek.AddDays(6);
                    break;

                case "LASTWEEK":
                    var lastWeekStart = today.AddDays(-(int)today.DayOfWeek - 7);
                    fromDate = lastWeekStart;
                    toDate = lastWeekStart.AddDays(6);
                    break;

                case "THISMONTH":
                    fromDate = new DateTime(today.Year, today.Month, 1);
                    toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                    break;

                case "LASTMONTH":
                    var lastMonthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    fromDate = lastMonthStart;
                    toDate = lastMonthStart.AddMonths(1).AddDays(-1);
                    break;

                case "ALL":
                default:
                    // No date filtering
                    fromDate = null;
                    toDate = null;
                    break;
            }

            // Set session variables for date filtering
            Session["RepairFilterFromDate"] = fromDate;
            Session["RepairFilterToDate"] = toDate;
        }
    }
}