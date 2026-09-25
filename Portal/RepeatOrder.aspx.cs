using System;
using System.Web;
using System.Web.UI.WebControls;
using TrackerSQL.Models;

namespace TrackerSQL.Portal
{
    public partial class RepeatOrder : PortalPageBase
    {
        protected System.Web.UI.UpdatePanel upRepeat;
        protected PlaceHolder phForm, phDone;
        protected Literal litDelivery, litExisting, litMessage;
        protected Label lblPO;
        protected TextBox txtPO, txtChanges;
        protected GridView gvLines;
        protected Button btnPlace, btnBack, btnDoneBack;
        protected HyperLink hlViewOrder;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                Bind();
        }

        private void Bind()
        {
            ContactOrderSummary open = Portal.GetOwnOpenOrder();
            if (open != null)
            {
                Response.Redirect("~/Portal/MyOrder.aspx?OrderID=" + open.OrderID, true);
                return;
            }

            ContactPortalRepeatOrderPreview preview = Portal.GetRepeatOrderPreview();
            if (preview == null)
            {
                Response.Redirect("~/Portal/MyOrders.aspx", true);
                return;
            }

            litDelivery.Text = HttpUtility.HtmlEncode(preview.DeliveryDateDisplay);
            lblPO.Text = preview.RequiresPurchaseOrder ? "Purchase Order (PO) — required" : "Purchase Order (PO)";

            gvLines.DataSource = preview.Lines;
            gvLines.DataBind();

            bool hasExisting = preview.ExistingOrderId > 0;
            litExisting.Text = hasExisting
                ? "<p class=\"status-message status-info\">You already have an order for "
                    + HttpUtility.HtmlEncode(preview.DeliveryDateDisplay) + ". <a href=\""
                    + HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Portal/MyOrder.aspx?OrderID=" + preview.ExistingOrderId))
                    + "\">Open it</a> and use Request A Change if you need anything added.</p>"
                : string.Empty;
            btnPlace.Enabled = preview.Lines.Count > 0 && !hasExisting;
        }

        protected void btnPlace_Click(object sender, EventArgs e)
        {
            int orderId = Portal.PlaceRepeatOrder(txtPO.Text, txtChanges.Text, out string message);
            litMessage.Text = StatusHtml(message, orderId <= 0);
            if (orderId <= 0)
                return;

            phForm.Visible = false;
            phDone.Visible = true;
            hlViewOrder.NavigateUrl = "~/Portal/MyOrder.aspx?OrderID=" + orderId;
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Portal/MyOrders.aspx", true);
        }
    }
}
