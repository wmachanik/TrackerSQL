using System;
using System.Web.UI.WebControls;
using TrackerSQL.Models;

namespace TrackerSQL.Portal
{
    public partial class Home : PortalPageBase
    {
        protected Literal litName, litNextCoffee;
        protected LinkButton lnkSignOutCard;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            Contact c = Portal.GetOwnContact();
            litName.Text = System.Web.HttpUtility.HtmlEncode(
                !string.IsNullOrWhiteSpace(c?.CompanyName)
                    ? c.CompanyName
                    : (c?.ContactFirstName ?? "there"));
            litNextCoffee.Text = NextCoffeeHtml();
        }

        protected void lnkSignOutCard_Click(object sender, EventArgs e)
        {
            SignOutAndRedirect();
        }
    }
}
