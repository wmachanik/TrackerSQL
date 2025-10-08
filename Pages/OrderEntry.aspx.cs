// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.OrderEntry
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Controls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class OrderEntry : Page
    {
        private const int CONST_CUSTIDCOL = 2;
        private const int CONST_ROASTDATECOL = 4;
        protected CheckBox chkbxOrderDone;
        protected DropDownList ddlSearchFor;
        protected TextBox tbxSearchFor;
        protected Button btnGo;
        protected Button btnReset;
        protected ObjectDataSource odsDistinctOrders;
        protected GridView gvListOfOrders;
        protected GridView gvOrderDetails;
        protected ObjectDataSource odsCompanys;
        protected ObjectDataSource odsPersons;
        protected ObjectDataSource odsItems;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void gvCurrent_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Response.Redirect($"~/Pages/OrderDetail.aspx?CustomerID={this.gvListOfOrders.SelectedRow.Cells[2].Text}&PrepDate={this.gvListOfOrders.SelectedRow.Cells[4].Text}");
        }

        protected void gvOrderDetails_OnRowUpdated(object sender, GridViewUpdatedEventArgs e)
        {
            DataTable dataTable = (DataTable)this.Session["TaskTable"];
        }

        protected void gvOrderDetails_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
        }

        protected void btnGo_Click(object sender, EventArgs e) => this.gvOrderDetails.DataBind();

        protected void tbxSearchFor_TextChanged(object sender, EventArgs e)
        {
            this.gvOrderDetails.DataBind();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            this.tbxSearchFor.Text = string.Empty;
            this.ddlSearchFor.SelectedIndex = 1;
            this.gvOrderDetails.DataBind();
        }

        public string GetItemDesc(int pItemID)
        {
            return pItemID > 0 ? ItemTypeTbl.GetItemTypeDescById(pItemID) : string.Empty;
        }
    }

}