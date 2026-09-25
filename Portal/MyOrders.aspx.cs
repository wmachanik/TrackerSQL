using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using TrackerSQL.Models;

namespace TrackerSQL.Portal
{
    public partial class MyOrders : PortalPageBase
    {
        protected GridView gvOrders;
        protected Literal litNextCoffee;
        protected System.Web.UI.UpdatePanel upOrders;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litNextCoffee.Text = NextCoffeeHtml();
                BindOrders();
            }
        }

        private void BindOrders()
        {
            gvOrders.DataSource = Portal.GetOwnOrdersWithItems();
            gvOrders.DataBind();
        }

        protected void gvOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;
            BindOrders();
        }

        /// <summary>One line per item; falls back to the first-line preview when lines were not loaded.</summary>
        protected static string ItemLinesHtml(ContactOrderSummary order)
        {
            if (order == null)
                return string.Empty;
            string text = order.ItemLines == null || order.ItemLines.Count == 0
                ? order.ItemsDisplay
                : string.Join(", ", order.ItemLines);
            string encoded = HttpUtility.HtmlEncode(text ?? string.Empty);
            return "<span class=\"portal-ellipsis\" title=\"" + encoded + "\">" + encoded + "</span>";
        }
    }
}
