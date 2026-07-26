//------------------------------------------------------------------------------
// TrackerSQL v3.x — DeleteOrderLine
// WebForms page code-behind for DeleteOrderLine.
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Repositories;

//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public partial class DeleteOrderLine : Page
    {
        private readonly OrdersRepository _ordersRepository = new OrdersRepository();
        protected DetailsView dvDeleteOrderItem;
        protected SqlDataSource sdsOrderLine;
        protected Button btnDelete;
        protected Button btnReturn;
        protected Literal ltrlStatus;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;
            if (this.Request.UrlReferrer == (Uri)null)
            {
                this.Session["ReturnOrderURL"] = (object)"";
                this.btnReturn.Enabled = false;
            }
            else
                this.Session["ReturnOrderURL"] = (object)this.Request.UrlReferrer.OriginalString.ToString();
        }

        protected void ReturnToDetailPage()
        {
            string url = this.Session["ReturnOrderURL"].ToString();
            if (url.Length <= 0)
                return;
            this.Response.Redirect(url);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["OrderId"], out int orderLineId) || orderLineId <= 0)
                return;

            bool deleted = _ordersRepository.DeleteOrderLineById(orderLineId);
            ltrlStatus.Text = deleted ? "Item Deleted" : "Error deleting item.";
            ReturnToDetailPage();
        }

        protected void btnReturn_Click(object sender, EventArgs e) => this.ReturnToDetailPage();
    }
}
