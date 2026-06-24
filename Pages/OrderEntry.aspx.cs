using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class OrderEntry : Page
    {
        private const int CONST_ORDERIDCOL = 1;

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
            string orderId = gvListOfOrders.SelectedRow.Cells[CONST_ORDERIDCOL].Text;
            Response.Redirect($"~/Pages/OrderDetail.aspx?OrderID={orderId}");
        }

        protected void gvOrderDetails_OnRowUpdated(object sender, GridViewUpdatedEventArgs e)
        {
            DataTable dataTable = (DataTable)Session["TaskTable"];
        }

        protected void gvOrderDetails_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
        }

        protected void btnGo_Click(object sender, EventArgs e) => gvListOfOrders.DataBind();

        protected void tbxSearchFor_TextChanged(object sender, EventArgs e)
        {
            gvListOfOrders.DataBind();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            tbxSearchFor.Text = string.Empty;
            ddlSearchFor.SelectedIndex = 0;
            gvListOfOrders.DataBind();
        }

        public string GetItemDesc(int pItemID)
        {
            return pItemID > 0 ? new ItemsRepository().GetItemDescById(pItemID) : string.Empty;
        }
    }
}
