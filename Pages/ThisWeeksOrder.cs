//------------------------------------------------------------------------------
// TrackerSQL v3.x — ThisWeeksOrder
// WebForms page code-behind for ThisWeeksOrder.
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public class ThisWeeksOrder : Page
    {
        protected System.Web.UI.ScriptManager smOrderSummary;
        protected DropDownList ddlOrdersPerPage;
        protected UpdateProgress uproOrderSummary;
        protected UpdatePanel upanOrderSummary;
        protected GridView gvOutstandingOrders;
        protected ObjectDataSource odsOpenOrders;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void ddlOrdersPerPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.gvOutstandingOrders.PageSize = (int)Convert.ToInt16(this.ddlOrdersPerPage.SelectedValue);
        }
    }
}
