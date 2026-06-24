// Decompiled with JetBrains decompiler
// Type: TrackerSQL.Pages.CoffeeRequired
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class ItemsRequired : Page
    {
        private OrdersRepository _ordersRepository = new OrdersRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set default date range (last 30 days)
                txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                
                BindPreparationDayData();
                BindItemsRequiredByDeliveryDate();
                UpdateFilterStatus();
            }
        }

        private void BindPreparationDayData()
        {
            try
            {
                DateTime? fromDate = GetFromDate();
                DateTime? toDate = GetToDate();
                
                List<ItemsRequiredSummary> data = _ordersRepository.GetItemsRequiredByPrepDate(fromDate, toDate);
                gvPreparationDay.DataSource = data;
                gvPreparationDay.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error binding preparation day data: {ex.Message}");
            }
        }

        private void BindItemsRequiredByDeliveryDate()
        {
            try
            {
                DateTime? fromDate = GetFromDate();
                DateTime? toDate = GetToDate();
                
                List<ItemsRequiredSummary> data = _ordersRepository.GetItemsRequiredByDeliveryDate(fromDate, toDate);
                gvItemsRequiredByDay.DataSource = data;
                gvItemsRequiredByDay.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error binding delivery date data: {ex.Message}");
            }
        }

        private DateTime? GetFromDate()
        {
            if (DateTime.TryParse(txtFromDate.Text, out DateTime fromDate))
            {
                return fromDate;
            }
            return null;
        }

        private DateTime? GetToDate()
        {
            if (DateTime.TryParse(txtToDate.Text, out DateTime toDate))
            {
                // Set to end of day
                var endOfDay = toDate.AddDays(1).AddSeconds(-1);
                System.Diagnostics.Debug.WriteLine($"GetToDate: txtToDate.Text='{txtToDate.Text}', parsed={toDate:yyyy-MM-dd}, endOfDay={endOfDay:yyyy-MM-dd HH:mm:ss}");
                return endOfDay;
            }
            System.Diagnostics.Debug.WriteLine($"GetToDate: Failed to parse txtToDate.Text='{txtToDate.Text}'");
            return null;
        }

        private void UpdateFilterStatus()
        {
            if (!string.IsNullOrEmpty(txtFromDate.Text) && !string.IsNullOrEmpty(txtToDate.Text))
            {
                lblFilterStatus.Text = $"Current filter: {txtFromDate.Text} to {txtToDate.Text}";
            }
            else
            {
                lblFilterStatus.Text = "No filter applied - showing default range";
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFromDate.Text) || string.IsNullOrEmpty(txtToDate.Text))
            {
                lblFilterStatus.Text = "Error: Please enter both From Date and To Date";
                return;
            }

            if (!DateTime.TryParse(txtFromDate.Text, out DateTime fromDate) || 
                !DateTime.TryParse(txtToDate.Text, out DateTime toDate))
            {
                lblFilterStatus.Text = "Error: Invalid date format";
                return;
            }

            if (fromDate > toDate)
            {
                lblFilterStatus.Text = "Error: From Date cannot be greater than To Date";
                return;
            }

            BindPreparationDayData();
            BindItemsRequiredByDeliveryDate();
            
            lblFilterStatus.Text = $"Filter applied: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd} at {DateTime.Now:HH:mm:ss}";
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            // Set default date range (last 30 days)
            txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            
            BindPreparationDayData();
            BindItemsRequiredByDeliveryDate();
            
            lblFilterStatus.Text = $"Filter cleared - showing last 30 days at {DateTime.Now:HH:mm:ss}";
        }

        protected void gvPreparationDay_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label control1 = (Label)e.Row.FindControl("lblGroupTitle");
                Label control2 = (Label)e.Row.FindControl("lblQty");
                string text = control1.Text;
                double num1 = Convert.ToDouble(control2.Text);
                string str1 = (string)this.ViewState["GroupTitle"];
                double num2 = this.ViewState["GroupTotal"] == null ? 0.0 : (double)this.ViewState["GroupTotal"];
                double num3;
                if (str1 == text)
                {
                    num3 = num2 + num1;
                    control1.Visible = false;
                    control1.Text = string.Empty;
                }
                else
                {
                    string str2 = text;
                    this.ViewState["GroupTitle"] = (object)str2;
                    control1.Visible = true;
                    string str3 = $"{(num2 == 0.0 ? "</td><td></td>" : $"<b>Total</b></td><td align='right'><b>{num2}</b></td>")}</tr><tr><td colspan='2'><b>Prep Date</b>: {str2}</td></tr><tr><td>";
                    control1.Text = str3;
                    num3 = num1;
                }
                this.ViewState["GroupTotal"] = (object)num3;
            }
            else
            {
                if (e.Row.RowType != DataControlRowType.Footer)
                    return;
                double num = this.ViewState["GroupTotal"] == null ? 0.0 : (double)this.ViewState["GroupTotal"];
                ((Label)e.Row.FindControl("lblFooterQty")).Text = num.ToString();
            }
        }

        protected void gvItemsRequiredByDay_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label control1 = (Label)e.Row.FindControl("lblByGroupTitle");
                Label control2 = (Label)e.Row.FindControl("lblByAbbreviation");
                Label control3 = (Label)e.Row.FindControl("lblByQty");
                string str1 = $"{control1.Text} ({control2.Text})";
                double num1 = Convert.ToDouble(control3.Text);
                string str2 = (string)this.ViewState["GroupByTitle"];
                double num2 = this.ViewState["GroupByTotal"] == null ? 0.0 : (double)this.ViewState["GroupByTotal"];
                double num3;
                if (str2 == str1)
                {
                    num3 = num2 + num1;
                    control1.Visible = false;
                    control1.Text = string.Empty;
                }
                else
                {
                    string str3 = str1;
                    this.ViewState["GroupByTitle"] = (object)str3;
                    control1.Visible = true;
                    string str4 = $"{(num2 == 0.0 ? "</td><td></td>" : $"<b>Total</b></td><td colspan='2' align='right'><b>{num2}</b></td>")}</tr><tr><td colspan='3'><b>Required Date (By)</b>: {str3}</td></tr><tr><td>";
                    control1.Text = str4;
                    num3 = num1;
                }
                this.ViewState["GroupByTotal"] = (object)num3;
            }
            else
            {
                if (e.Row.RowType != DataControlRowType.Footer)
                    return;
                double num = this.ViewState["GroupByTotal"] == null ? 0.0 : (double)this.ViewState["GroupByTotal"];
                ((Label)e.Row.FindControl("lblByFooterQty")).Text = num.ToString();
            }
        }
    }
}
