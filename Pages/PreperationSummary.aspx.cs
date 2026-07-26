//------------------------------------------------------------------------------
// TrackerSQL v3.x — PreperationSummary (page file name legacy; UI uses Preparation)
// WebForms page code-behind for weekly preparation summary.
//------------------------------------------------------------------------------

using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class PreperationSummary : Page
    {
        private const string CONST_GROUPTTOTAL = "GroupTotal";
        private const string CONST_LINENO = "LineNo";
        private const string CONST_WEEKDESC = "WeekDesc";

        protected ScriptManager scrmOrderDetail;
        protected UpdateProgress udtpPrepSummary;
        protected UpdatePanel udtpnlPrepSummary;
        protected Panel pnlPrepSummary;
        protected TextBox tbxDateFrom;
        protected ImageButton btnCalendarFrom;
        protected CalendarExtender tbxDateFrom_CalendarExtender;
        protected TextBox tbxDateTo;
        protected ImageButton btnCalendarTo;
        protected CalendarExtender tbxDateTo_CalendarExtender;
        protected DropDownList ddlFilterByPrepDate;
        protected Button GoBtn;
        protected Button ResetBtn;
        protected Button PrevWeekBtn;
        protected Button NextWeekBtn;
        protected Button btnBack;
        protected GridView gvPreperationSummary;
        protected Literal ltrlDates;
        protected HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;

        protected List<DateTime> ListOfDatesOnDoW(DayOfWeek pDoW)
        {
            List<DateTime> dateTimeList = new List<DateTime>();
            DateTime date = TimeZoneUtils.Now().AddDays((double)(pDoW - TimeZoneUtils.Now().DayOfWeek)).Date;
            for (int index = 0; index < 12; ++index)
                dateTimeList.Add(date.AddDays((double)(7 * index - 63)));
            return dateTimeList;
        }

        protected void ZeroViewStateVals()
        {
            this.ViewState[CONST_GROUPTTOTAL] = 0.0;
            this.ViewState[CONST_LINENO] = 1;
        }

        protected DateTime GetFirstDoW(DateTime pDate)
        {
            int dayOfWeek = (int)pDate.DayOfWeek;
            if (dayOfWeek < 0)
                dayOfWeek += 7;
            return pDate.AddDays((double)(-1 * dayOfWeek)).Date;
        }

        protected DateTime GetLastDoW(DateTime pDate)
        {
            int num = (int)(6 - pDate.DayOfWeek);
            if (num < 0)
                num = 0;
            return pDate.AddDays((double)num).Date;
        }

        protected void ResetDates()
        {
            this.tbxDateFrom.Text = this.GetFirstDoW(TimeZoneUtils.Now()).ToString("yyyy-MM-dd");
            this.tbxDateTo.Text = this.GetLastDoW(TimeZoneUtils.Now()).ToString("yyyy-MM-dd");
            this.ZeroViewStateVals();
        }

        private void ShowPageStatus(string message, bool isError)
        {
            if (pnlStatus == null || ltrlStatus == null)
                return;

            bool hasMessage = !string.IsNullOrWhiteSpace(message);
            pnlStatus.Visible = hasMessage;
            ltrlStatus.Text = hasMessage ? HttpUtility.HtmlEncode(message) : string.Empty;
            pnlStatus.Attributes["class"] = isError
                ? "status-message status-error"
                : "status-message status-info";
        }

        private void ClearPageStatus()
        {
            if (pnlStatus != null)
            {
                pnlStatus.Visible = false;
                pnlStatus.Attributes["class"] = "status-message";
            }

            if (ltrlStatus != null)
                ltrlStatus.Text = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;
            this.ResetDates();
        }

        protected void GoBtn_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dateFrom = Convert.ToDateTime(this.tbxDateFrom.Text);
                DateTime dateTo = Convert.ToDateTime(this.tbxDateTo.Text);
                bool usePrepDate = this.ddlFilterByPrepDate.SelectedValue.Equals("PrepDate");

                var repository = new PreparationSummaryRepository();
                var summaryItems = repository.GetPreparationSummary(dateFrom, dateTo, usePrepDate);

                double weekNumber = (double)(dateFrom.DayOfYear / 7);
                this.ViewState[CONST_WEEKDESC] = $"Y{dateFrom.Year} Wk {Convert.ToString(Math.Ceiling(weekNumber) + 1.0)}";
                this.ltrlDates.Text =
                    $"<div class=\"status-message status-info\" style=\"margin-top:8px;\">{(usePrepDate ? "Prep Date" : "Delivery/Required By Date")} — From: {HttpUtility.HtmlEncode(this.tbxDateFrom.Text)} to {HttpUtility.HtmlEncode(this.tbxDateTo.Text)}</div>";

                this.ZeroViewStateVals();
                this.gvPreperationSummary.DataSource = summaryItems;
                this.gvPreperationSummary.DataBind();
                ClearPageStatus();

                if (summaryItems == null || summaryItems.Count == 0)
                    ShowPageStatus("No preparation items found for this date range.", false);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "PreparationSummary GoBtn_Click error: " + ex.Message);
                this.ltrlDates.Text = string.Empty;
                this.gvPreperationSummary.DataSource = null;
                this.gvPreperationSummary.DataBind();
                ShowPageStatus("Error loading data: " + ex.Message, true);
            }
        }

        protected void gvPreperationSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                var headerLabel = e.Row.FindControl("lblDescHdr") as Label;
                if (headerLabel != null)
                    headerLabel.Text = this.ViewState[CONST_WEEKDESC]?.ToString() + ":Ln1";
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var lblItemDesc = e.Row.FindControl("lblItemDesc") as Label;
                var lblQty = e.Row.FindControl("lblQty") as Label;
                var lblDescItem = e.Row.FindControl("lblDescItem") as Label;

                if (lblItemDesc != null && lblQty != null && lblDescItem != null)
                {
                    double quantity = Convert.ToDouble(lblQty.Text);
                    this.ViewState[CONST_GROUPTTOTAL] = (double)this.ViewState[CONST_GROUPTTOTAL] + quantity;

                    int lineNo = (int)this.ViewState[CONST_LINENO];
                    lblDescItem.Text = $"{this.ViewState[CONST_WEEKDESC]}-Ln:{lineNo}>{quantity}kgs of {lblItemDesc.Text}";
                    this.ViewState[CONST_LINENO] = lineNo + 1;
                }
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                var lblTotalQty = e.Row.FindControl("lblTotalQty") as Label;
                if (lblTotalQty != null)
                {
                    double total = this.ViewState[CONST_GROUPTTOTAL] == null ? 0.0 : (double)this.ViewState[CONST_GROUPTTOTAL];
                    lblTotalQty.Text = total.ToString();
                }
            }
        }

        protected void BackBtn_Click(object sender, EventArgs e)
        {
            this.ZeroViewStateVals();
            DateTime dateFrom = Convert.ToDateTime(this.tbxDateFrom.Text).AddDays(-7.0);
            DateTime dateTo = Convert.ToDateTime(this.tbxDateTo.Text).AddDays(-7.0);
            this.tbxDateFrom.Text = dateFrom.ToString("yyyy-MM-dd");
            this.tbxDateTo.Text = dateTo.ToString("yyyy-MM-dd");
            ClearPageStatus();
        }

        protected void ForwardBtn_Click(object sender, EventArgs e)
        {
            this.ZeroViewStateVals();
            DateTime dateFrom = Convert.ToDateTime(this.tbxDateFrom.Text).AddDays(7.0);
            DateTime dateTo = Convert.ToDateTime(this.tbxDateTo.Text).AddDays(7.0);
            this.tbxDateFrom.Text = dateFrom.ToString("yyyy-MM-dd");
            this.tbxDateTo.Text = dateTo.ToString("yyyy-MM-dd");
            ClearPageStatus();
        }

        protected void ResetBtn_Click(object sender, EventArgs e)
        {
            this.ResetDates();
            ClearPageStatus();
            this.ltrlDates.Text = string.Empty;
            this.gvPreperationSummary.DataSource = null;
            this.gvPreperationSummary.DataBind();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
