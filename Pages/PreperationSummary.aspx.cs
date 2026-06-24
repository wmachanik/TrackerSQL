// Decompiled with JetBrains decompiler
// Type: TrackerSQL.Pages.PreperationSummary
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Web.UI;
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
        protected TextBox tbxDateFrom;
        protected CalendarExtender tbxDateFrom_CalendarExtender;
        protected TextBox tbxDateTo;
        protected CalendarExtender tbxDateTo_CalendarExtender;
        protected DropDownList ddlFilterByPrepDate;
        protected Button GoBtn;
        protected Button ResetBtn;
        protected Button BackBtn;
        protected Button ForwardBtn;
        protected GridView gvPreperationSummary;
        protected Literal ltrlDates;

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
            this.tbxDateFrom.Text = $"{this.GetFirstDoW(TimeZoneUtils.Now()):d}";
            this.tbxDateTo.Text = $"{this.GetLastDoW(TimeZoneUtils.Now()):d}";
            this.ZeroViewStateVals();
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

                var repository = new PreperationSummaryRepository();
                var summaryItems = repository.GetPreperationSummary(dateFrom, dateTo, usePrepDate);

                double weekNumber = (double)(dateFrom.DayOfYear / 7);
                this.ViewState[CONST_WEEKDESC] = $"Y{dateFrom.Year} Wk {Convert.ToString(Math.Ceiling(weekNumber) + 1.0)}";
                this.ltrlDates.Text = $"{(usePrepDate ? "Prep Date" : "Delivery/Required By Date")} - From: {this.tbxDateFrom.Text} to {this.tbxDateTo.Text}";

                this.ZeroViewStateVals();
                this.gvPreperationSummary.DataSource = summaryItems;
                this.gvPreperationSummary.DataBind();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "PreperationSummary GoBtn_Click error: " + ex.Message);
                this.ltrlDates.Text = "<span style='color:red;'>Error loading data: " + Server.HtmlEncode(ex.Message) + "</span>";
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
            this.tbxDateFrom.Text = $"{dateFrom:d}";
            this.tbxDateTo.Text = $"{dateTo:d}";
        }

        protected void ForwardBtn_Click(object sender, EventArgs e)
        {
            this.ZeroViewStateVals();
            DateTime dateFrom = Convert.ToDateTime(this.tbxDateFrom.Text).AddDays(7.0);
            DateTime dateTo = Convert.ToDateTime(this.tbxDateTo.Text).AddDays(7.0);
            this.tbxDateFrom.Text = $"{dateFrom:d}";
            this.tbxDateTo.Text = $"{dateTo:d}";
        }

        protected void ResetBtn_Click(object sender, EventArgs e) => this.ResetDates();
    }
}
