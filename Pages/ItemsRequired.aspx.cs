//------------------------------------------------------------------------------
// TrackerSQL v3.x — ItemsRequired
// WebForms page code-behind for ItemsRequired.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class ItemsRequired : Page
    {
        private readonly OrdersRepository _ordersRepository = new OrdersRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ResetDates();
                BindAll();
                UpdateFilterStatus();
            }
        }

        protected DateTime GetFirstDoW(DateTime pDate)
        {
            int dayOfWeek = (int)pDate.DayOfWeek;
            if (dayOfWeek < 0)
                dayOfWeek += 7;
            return pDate.AddDays(-1 * dayOfWeek).Date;
        }

        protected DateTime GetLastDoW(DateTime pDate)
        {
            int num = (int)(6 - pDate.DayOfWeek);
            if (num < 0)
                num = 0;
            return pDate.AddDays(num).Date;
        }

        protected void ResetDates()
        {
            DateTime now = TimeZoneUtils.Now();
            tbxDateFrom.Text = GetFirstDoW(now).ToString("yyyy-MM-dd");
            tbxDateTo.Text = GetLastDoW(now).ToString("yyyy-MM-dd");
        }

        private void BindAll()
        {
            BindPreparationDayData();
            BindItemsRequiredByDeliveryDate();
        }

        private void BindPreparationDayData()
        {
            try
            {
                ViewState["PrepGrandTotalKg"] = 0.0;
                ViewState["PrepGrandTotalPacks"] = 0.0;
                ViewState["PrepLastDate"] = null;

                List<ItemsRequiredSummary> data = _ordersRepository.GetItemsRequiredByPrepDate(GetFromDate(), GetToDate())
                    ?? new List<ItemsRequiredSummary>();
                gvPreparationDay.DataSource = data;
                gvPreparationDay.DataBind();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ItemsRequired BindPreparationDayData: " + ex.Message);
                SetStatus("Error loading prep-date items: " + ex.Message, isError: true);
                gvPreparationDay.DataSource = new List<ItemsRequiredSummary>();
                gvPreparationDay.DataBind();
            }
        }

        private void BindItemsRequiredByDeliveryDate()
        {
            try
            {
                ViewState["DeliveryGrandTotalKg"] = 0.0;
                ViewState["DeliveryGrandTotalPacks"] = 0.0;
                ViewState["DeliveryLastKey"] = null;

                List<ItemsRequiredSummary> data = _ordersRepository.GetItemsRequiredByDeliveryDate(GetFromDate(), GetToDate())
                    ?? new List<ItemsRequiredSummary>();
                gvItemsRequiredByDay.DataSource = data;
                gvItemsRequiredByDay.DataBind();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ItemsRequired BindItemsRequiredByDeliveryDate: " + ex.Message);
                SetStatus("Error loading delivery-date items: " + ex.Message, isError: true);
                gvItemsRequiredByDay.DataSource = new List<ItemsRequiredSummary>();
                gvItemsRequiredByDay.DataBind();
            }
        }

        private DateTime? GetFromDate()
        {
            if (DateTime.TryParse(tbxDateFrom.Text, out DateTime fromDate))
                return fromDate.Date;
            return null;
        }

        /// <summary>Inclusive end-of-day so DATE/DATETIME required/prep dates on the To date are included.</summary>
        private DateTime? GetToDate()
        {
            if (DateTime.TryParse(tbxDateTo.Text, out DateTime toDate))
                return toDate.Date.AddDays(1).AddTicks(-1);
            return null;
        }

        private void UpdateFilterStatus(int? prepCount = null, int? deliveryCount = null)
        {
            if (!string.IsNullOrEmpty(tbxDateFrom.Text) && !string.IsNullOrEmpty(tbxDateTo.Text))
            {
                string message = "Current week filter: " + tbxDateFrom.Text + " to " + tbxDateTo.Text;
                if (prepCount.HasValue || deliveryCount.HasValue)
                {
                    message += " — prep rows: " + (prepCount ?? 0)
                        + ", delivery rows: " + (deliveryCount ?? 0);
                }
                SetStatus(message, isError: false);
            }
            else
            {
                SetStatus("No dates selected", isError: false);
            }
        }

        private void SetStatus(string message, bool isError)
        {
            if (lblFilterStatus != null)
                lblFilterStatus.Text = message ?? string.Empty;

            if (pnlStatus != null)
            {
                pnlStatus.Visible = !string.IsNullOrWhiteSpace(message);
                pnlStatus.Attributes["class"] = isError
                    ? "status-message status-error"
                    : "status-message status-info";
            }
        }

        protected void GoBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbxDateFrom.Text) || string.IsNullOrEmpty(tbxDateTo.Text))
            {
                SetStatus("Error: Please enter both From Date and To Date", isError: true);
                return;
            }

            if (!DateTime.TryParse(tbxDateFrom.Text, out DateTime fromDate) ||
                !DateTime.TryParse(tbxDateTo.Text, out DateTime toDate))
            {
                SetStatus("Error: Invalid date format", isError: true);
                return;
            }

            if (fromDate > toDate)
            {
                SetStatus("Error: From Date cannot be greater than To Date", isError: true);
                return;
            }

            DateTime fromDay = fromDate.Date;
            DateTime toEnd = toDate.Date.AddDays(1).AddTicks(-1);

            var prep = _ordersRepository.GetItemsRequiredByPrepDate(fromDay, toEnd) ?? new List<ItemsRequiredSummary>();
            var delivery = _ordersRepository.GetItemsRequiredByDeliveryDate(fromDay, toEnd) ?? new List<ItemsRequiredSummary>();

            ViewState["PrepGrandTotalKg"] = 0.0;
            ViewState["PrepGrandTotalPacks"] = 0.0;
            ViewState["PrepLastDate"] = null;
            ViewState["DeliveryGrandTotalKg"] = 0.0;
            ViewState["DeliveryGrandTotalPacks"] = 0.0;
            ViewState["DeliveryLastKey"] = null;

            gvPreparationDay.DataSource = prep;
            gvPreparationDay.DataBind();
            gvItemsRequiredByDay.DataSource = delivery;
            gvItemsRequiredByDay.DataBind();

            SetStatus("Filter applied: " + fromDate.ToString("yyyy-MM-dd") + " to "
                + toDate.ToString("yyyy-MM-dd") + " — prep rows: " + prep.Count
                + ", delivery rows: " + delivery.Count, isError: false);
        }

        protected void ResetBtn_Click(object sender, EventArgs e)
        {
            ResetDates();
            BindAll();
            SetStatus("Reset to current week: " + tbxDateFrom.Text + " to " + tbxDateTo.Text, isError: false);
        }

        protected void PrevWeekBtn_Click(object sender, EventArgs e)
        {
            ShiftWeek(-7);
            BindAll();
            UpdateFilterStatus();
        }

        protected void NextWeekBtn_Click(object sender, EventArgs e)
        {
            ShiftWeek(7);
            BindAll();
            UpdateFilterStatus();
        }

        private void ShiftWeek(int days)
        {
            if (!DateTime.TryParse(tbxDateFrom.Text, out DateTime fromDate) ||
                !DateTime.TryParse(tbxDateTo.Text, out DateTime toDate))
            {
                ResetDates();
                return;
            }

            tbxDateFrom.Text = fromDate.AddDays(days).ToString("yyyy-MM-dd");
            tbxDateTo.Text = toDate.AddDays(days).ToString("yyyy-MM-dd");
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        protected void gvPreparationDay_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var item = e.Row.DataItem as ItemsRequiredSummary;
                double qtyKg = item != null ? item.Qty : 0;
                double packQty = item != null ? item.PackQty : 0;
                ViewState["PrepGrandTotalKg"] = GetViewDouble("PrepGrandTotalKg") + qtyKg;
                ViewState["PrepGrandTotalPacks"] = GetViewDouble("PrepGrandTotalPacks") + packQty;

                var lblPrepDate = e.Row.FindControl("lblPrepDate") as Label;
                if (lblPrepDate != null && item != null && item.PrepDate.HasValue)
                {
                    string key = item.PrepDate.Value.ToString("yyyy-MM-dd");
                    string last = ViewState["PrepLastDate"] as string;
                    if (string.Equals(last, key, StringComparison.Ordinal))
                        lblPrepDate.Text = string.Empty;
                    else
                        ViewState["PrepLastDate"] = key;
                }
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                SetFooterLabel(e.Row, "lblFooterPackQty", GetViewDouble("PrepGrandTotalPacks"), packFormat: true);
                SetFooterLabel(e.Row, "lblFooterQty", GetViewDouble("PrepGrandTotalKg"), packFormat: false);
            }
        }

        protected void gvItemsRequiredByDay_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var item = e.Row.DataItem as ItemsRequiredSummary;
                double qtyKg = item != null ? item.Qty : 0;
                double packQty = item != null ? item.PackQty : 0;
                ViewState["DeliveryGrandTotalKg"] = GetViewDouble("DeliveryGrandTotalKg") + qtyKg;
                ViewState["DeliveryGrandTotalPacks"] = GetViewDouble("DeliveryGrandTotalPacks") + packQty;

                var lblRequiredByDate = e.Row.FindControl("lblRequiredByDate") as Label;
                var lblByAbbreviation = e.Row.FindControl("lblByAbbreviation") as Label;
                if (item != null && item.RequiredByDate.HasValue)
                {
                    string key = item.RequiredByDate.Value.ToString("yyyy-MM-dd") + "|" + (item.Abbreviation ?? string.Empty);
                    string last = ViewState["DeliveryLastKey"] as string;
                    if (string.Equals(last, key, StringComparison.Ordinal))
                    {
                        if (lblRequiredByDate != null)
                            lblRequiredByDate.Text = string.Empty;
                        if (lblByAbbreviation != null)
                            lblByAbbreviation.Text = string.Empty;
                    }
                    else
                    {
                        ViewState["DeliveryLastKey"] = key;
                    }
                }
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                SetFooterLabel(e.Row, "lblByFooterPackQty", GetViewDouble("DeliveryGrandTotalPacks"), packFormat: true);
                SetFooterLabel(e.Row, "lblByFooterQty", GetViewDouble("DeliveryGrandTotalKg"), packFormat: false);
            }
        }

        private double GetViewDouble(string key)
        {
            return ViewState[key] == null ? 0.0 : (double)ViewState[key];
        }

        private static void SetFooterLabel(GridViewRow footerRow, string controlId, double value, bool packFormat)
        {
            var label = footerRow.FindControl(controlId) as Label;
            if (label != null)
                label.Text = packFormat ? value.ToString("0.0") : value.ToString("0.##");
        }
    }
}
