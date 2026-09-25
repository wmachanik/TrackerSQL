using System;
using System.Web;
using System.Web.UI.WebControls;
using TrackerSQL.Models;

namespace TrackerSQL.Portal
{
    public partial class MyOrder : PortalPageBase
    {
        protected Literal litOrderDate, litDelivery, litPO, litStatus, litNotes, litMessage;
        protected Literal litCourier, litWaybill, litDispatched;
        protected PlaceHolder phCourier, phTrack, phShop, phShopView;
        protected HyperLink hlTrack, hlShopView;
        protected Literal litShopOrder;
        protected GridView gvLines;
        protected TextBox txtRequest;
        protected Button btnRequest, btnBack;

        private int OrderId
        {
            get
            {
                int.TryParse(Request.QueryString["OrderID"], out int id);
                return id;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                Bind();
        }

        private void Bind()
        {
            var header = Portal.GetOwnOrderHeader(OrderId);
            if (header == null)
            {
                Response.Redirect("~/Portal/MyOrders.aspx", true);
                return;
            }

            ContactOrderSummary summary = Portal.GetOwnOrderSummary(OrderId);

            litOrderDate.Text = ContactPortalDisplay.FormatDate(header.OrderDate);
            litDelivery.Text = ContactPortalDisplay.FormatDate(header.RequiredByDate);
            litPO.Text = Display(header.PurchaseOrder);
            litStatus.Text = summary != null
                ? StatusBadgeHtml(summary.CustomerStatusDisplay, summary.Done)
                : StatusBadgeHtml(header.Done ? "Completed" : "Received", header.Done);
            litNotes.Text = Display(header.Notes);

            BindCourier();
            BindShopOrder();

            gvLines.DataSource = Portal.GetOwnOrderLinesForDisplay(OrderId);
            gvLines.DataBind();
        }

        private void BindCourier()
        {
            ContactPortalCourierInfo courier = Portal.GetOwnOrderCourier(OrderId);
            phCourier.Visible = courier != null;
            if (courier == null)
                return;

            litCourier.Text = Display(courier.CourierName);
            litDispatched.Text = Display(ContactPortalDisplay.FormatDate(courier.DispatchedAt));

            bool canTrack = !string.IsNullOrWhiteSpace(courier.TrackingUrl);
            litWaybill.Text = canTrack
                ? "<a href=\"" + HttpUtility.HtmlAttributeEncode(courier.TrackingUrl) + "\" target=\"_blank\" rel=\"noopener\">"
                    + HttpUtility.HtmlEncode(courier.WaybillNumber) + "</a>"
                : Display(courier.WaybillNumber);

            phTrack.Visible = canTrack;
            if (canTrack)
                hlTrack.NavigateUrl = courier.TrackingUrl;
        }

        private void BindShopOrder()
        {
            ContactPortalShopOrder shop = Portal.GetOwnOrderShopLink(OrderId);
            phShop.Visible = shop != null;
            if (shop == null)
                return;

            bool canView = !string.IsNullOrWhiteSpace(shop.ViewUrl);
            litShopOrder.Text = canView
                ? "<a href=\"" + HttpUtility.HtmlAttributeEncode(shop.ViewUrl) + "\" target=\"_blank\" rel=\"noopener\">#"
                    + HttpUtility.HtmlEncode(shop.OrderNumber) + "</a>"
                : "#" + HttpUtility.HtmlEncode(shop.OrderNumber);
            phShopView.Visible = canView;
            if (canView)
                hlShopView.NavigateUrl = shop.ViewUrl;
        }

        private static string Display(string value)
        {
            return HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "—" : value);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Portal/MyOrders.aspx", true);
        }

        protected void btnRequest_Click(object sender, EventArgs e)
        {
            long id = Portal.SubmitChangeRequest(
                ContactPortalChangeKinds.Order, OrderId, txtRequest.Text, out string message);
            litMessage.Text = StatusHtml(message, id <= 0);
            if (id > 0)
                txtRequest.Text = string.Empty;
        }
    }
}
