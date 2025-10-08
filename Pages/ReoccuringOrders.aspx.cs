// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.ReoccuringOrders
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using AjaxControlToolkit;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class ReoccuringOrders : Page
    {
        private const string CONST_WHERECLAUSE_SESSIONVAR = "ReoccuringOrderSummaryWhereFilter";
        protected ScriptManager smReoccuringOrderSummary;
        protected UpdateProgress uprgReoccuringOrderSummary;
        protected UpdatePanel upnlSelection;
        protected DropDownList ddlFilterBy;
        protected TextBox tbxFilterBy;
        protected Button btnGon;
        protected Button btnReset;
        protected DropDownList ddlReoccuringOrderEnabled;
        protected UpdatePanel upnlReoccuringOrderSummary;
        protected GridView gvReoccuringOrders;
        protected ObjectDataSource odsReoccuringOrderSummarys;
        protected Label lblFilter;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (this.Request.QueryString["CompanyName"] != null)
                {
                    this.ddlFilterBy.SelectedValue = "CompanyName";
                    this.Session["ReoccuringOrderSummaryWhereFilter"] = (object)$"CompanyName LIKE '%{this.Request.QueryString["CompanyName"].ToString()}%'";
                }
                if (this.Session["ReoccuringOrderSummaryWhereFilter"] != null)
                {
                    string str1 = (string)this.Session["ReoccuringOrderSummaryWhereFilter"];
                    string str2 = str1.Remove(0, str1.IndexOf("%") + 1);
                    this.tbxFilterBy.Text = str2.Remove(str2.IndexOf("%"));
                }
                this.gvReoccuringOrders.Sort("CompanyName", SortDirection.Ascending);
            }
            else if (this.Session["ReoccuringOrderSummaryWhereFilter"] != null)
                this.lblFilter.Text = this.Session["ReoccuringOrderSummaryWhereFilter"].ToString();
            GridViewHelper gridViewHelper = new GridViewHelper(this.gvReoccuringOrders);
            gridViewHelper.RegisterGroup("CompanyName", true, false);
            gridViewHelper.ApplyGroupSort();
        }

        protected void btnGon_Click(object sender, EventArgs e)
        {
            if (!(this.ddlFilterBy.SelectedValue != "0") || string.IsNullOrWhiteSpace(this.tbxFilterBy.Text))
                return;
            this.Session["ReoccuringOrderSummaryWhereFilter"] = (object)$"{this.ddlFilterBy.SelectedValue} LIKE '%{this.tbxFilterBy.Text}%'";
            this.odsReoccuringOrderSummarys.DataBind();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            this.Session["ReoccuringOrderSummaryWhereFilter"] = (object)"";
            this.ddlFilterBy.SelectedIndex = 0;
            this.tbxFilterBy.Text = "";
            this.Session["ReoccuringOrderSummaryWhereFilter"] = (object)null;
            this.odsReoccuringOrderSummarys.DataBind();
        }

        protected void tbxFilterBy_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.tbxFilterBy.Text) || this.ddlFilterBy.SelectedIndex != 0)
                return;
            this.ddlFilterBy.SelectedIndex = 1;
            this.upnlSelection.Update();
        }

        protected void btnCalcNextRequiredDate_Click(object sender, EventArgs e)
        {
            // Example: Get all recurring orders and update their next required date
            var reoccuringOrderDal = new ReoccuringOrderDAL();
            var dateCalculator = new DateCalculator();
            var allOrders = reoccuringOrderDal.GetAll(1, "CustomersTbl.CustomerID");

            if (allOrders != null && allOrders.Count > 0)
            {
                var updatedOrders = dateCalculator.FilterAndUpdateRecurringOrdersDates(allOrders);
                // Optionally, rebind your grid or show a message
                this.odsReoccuringOrderSummarys.DataBind();
                ScriptManager.RegisterStartupScript(this, GetType(), "alertSuccess", "alert('Next required dates calculated and updated.');", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alertNone", "alert('No recurring orders found.');", true);
            }
            // Rebind the ObjectDataSource and GridView to refresh the data
            odsReoccuringOrderSummarys.Select(); // Optional: forces ObjectDataSource to re-select
            gvReoccuringOrders.DataBind();
        }
    }
}