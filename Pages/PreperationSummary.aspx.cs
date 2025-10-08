// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.PreperationSummary
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class PreperationSummary : Page
    {
        private const string CONST_GROUPTTOTAL = "GroupTotal";
        private const string CONST_LINENO = "LineNo";
        private const string CONST_WEEKDESC = "WeekDesc";
        private const int CONST_NUMWEEKS = 9;
        protected ScriptManager scrmOrderDetail;
        protected UpdateProgress udtpPrepSummary;
        protected UpdatePanel udtpnlPrepSummary;
        protected TextBox tbxDateFrom;
        protected CalendarExtender tbxDateFrom_CalendarExtender;
        protected TextBox tbxDateTo;
        protected CalendarExtender tbxDateTo_CalendarExtender;
        protected DropDownList ddlFilterByRoastDate;
        protected Button GoBtn;
        protected Button ResetBtn;
        protected Button BackBtn;
        protected Button ForwardBtn;
        protected GridView gvPreperationSummary;
        protected Literal ltrlDates;
        protected void Page_PreInit(object sender, EventArgs e)
        {
            //bool flag = new CheckBrowser().fBrowserIsMobile();
            //this.Session["RunningOnMoble"] = (object)flag;
            //if (flag)
            //    this.MasterPageFile = "~/MobileSite.master";
            //else
            //    this.MasterPageFile = "~/Site.master";
        }
        protected List<DateTime> ListOfDatesOnDoW(DayOfWeek pDoW)
        {
            List<DateTime> dateTimeList = new List<DateTime>();
            DateTime date = TimeZoneUtils.Now().AddDays((double)(pDoW - TimeZoneUtils.Now().DayOfWeek)).Date;
            for (int index = 0; index < 12; ++index)
                dateTimeList.Add(date.AddDays((double)(7 * index - 63 /*0x3F*/)));
            return dateTimeList;
        }

        protected void ZeroViewStateVals()
        {
            this.ViewState["GroupTotal"] = (object)0.0;
            this.ViewState["LineNo"] = (object)1;
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
            string str = "SELECT ItemTypeTbl.ItemDesc, ROUND(SUM(OrdersTbl.QuantityOrdered),2) AS Quantity FROM (OrdersTbl INNER JOIN ItemTypeTbl ON OrdersTbl.ItemTypeID = ItemTypeTbl.ItemTypeID)";
            bool flag = this.ddlFilterByRoastDate.SelectedValue.Equals("RoastDate");
            string strSQL = (!flag ? str + "  WHERE (OrdersTbl.RequiredByDate >= ?) AND (OrdersTbl.RequiredByDate <= ?) " : str + " WHERE (OrdersTbl.RoastDate >= ?) AND (OrdersTbl.RoastDate <= ?) ") + " AND (ItemTypeTbl.ServiceTypeID = 2) GROUP BY ItemTypeTbl.ItemDesc";
            TrackerDb trackerDb = new TrackerDb();
            DateTime dateTime = Convert.ToDateTime(this.tbxDateFrom.Text);
            trackerDb.AddWhereParams((object)dateTime, DbType.Date, "@RoastDateFrom");
            trackerDb.AddWhereParams((object)Convert.ToDateTime(this.tbxDateTo.Text), DbType.Date, "@RoastDateTo");
            double a = (double)(dateTime.DayOfYear / 7);
            this.ViewState["WeekDesc"] = (object)$"Y{dateTime.Year.ToString()} Wk {Convert.ToString(Math.Ceiling(a) + 1.0)}";
            this.ltrlDates.Text = $"{(flag ? (object)"Roast Date" : (object)"Prep Date")} - From: {this.tbxDateFrom.Text} to {this.tbxDateTo.Text}";
            this.ZeroViewStateVals();
            this.gvPreperationSummary.DataSource = (object)trackerDb.ReturnDataSet(strSQL);
            this.gvPreperationSummary.DataBind();
        }

        protected void gvPreperationSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
                ((Label)e.Row.FindControl("lblDescHdr")).Text = this.ViewState["WeekDesc"].ToString() + ":Ln1";
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label control1 = (Label)e.Row.FindControl("lblItemDesc");
                Label control2 = (Label)e.Row.FindControl("lblQty");
                Label control3 = (Label)e.Row.FindControl("lblDescItem");
                double num1 = Convert.ToDouble(control2.Text);
                this.ViewState["GroupTotal"] = (object)((this.ViewState["GroupTotal"] == null ? 0.0 : (double)this.ViewState["GroupTotal"]) + num1);
                int num2 = (int)this.ViewState["LineNo"];
                control3.Text = $"{this.ViewState["WeekDesc"].ToString()}-Ln:{num2}>{num1}kgs of {control1.Text}";
                this.ViewState["LineNo"] = (object)(num2 + 1);
            }
            else
            {
                if (e.Row.RowType != DataControlRowType.Footer)
                    return;
                double num = this.ViewState["GroupTotal"] == null ? 0.0 : (double)this.ViewState["GroupTotal"];
                ((Label)e.Row.FindControl("lblTotalQty")).Text = num.ToString();
            }
        }

        protected void ddlDateFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ZeroViewStateVals();
        }

        protected void ddlDateTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ZeroViewStateVals();
        }

        protected void BackBtn_Click(object sender, EventArgs e)
        {
            this.ZeroViewStateVals();
            DateTime dateTime1 = Convert.ToDateTime(this.tbxDateFrom.Text).AddDays(-7.0);
            DateTime dateTime2 = Convert.ToDateTime(this.tbxDateTo.Text).AddDays(-7.0);
            this.tbxDateFrom.Text = $"{dateTime1:d}";
            this.tbxDateTo.Text = $"{dateTime2:d}";
        }

        protected void ForwardBtn_Click(object sender, EventArgs e)
        {
            this.ZeroViewStateVals();
            DateTime dateTime1 = Convert.ToDateTime(this.tbxDateFrom.Text).AddDays(7.0);
            DateTime dateTime2 = Convert.ToDateTime(this.tbxDateTo.Text).AddDays(7.0);
            this.tbxDateFrom.Text = $"{dateTime1:d}";
            this.tbxDateTo.Text = $"{dateTime2:d}";
        }

        protected void ResetBtn_Click(object sender, EventArgs e) => this.ResetDates();
    }
}