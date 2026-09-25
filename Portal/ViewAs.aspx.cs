using System;
using System.Web;
using System.Web.UI;
using TrackerSQL.Managers;

namespace TrackerSQL.Portal
{
    /// <summary>
    /// Administrator read-only preview of the Contact Portal as a contact.
    /// ?ContactID=n starts it (then Home); ?exit=1 ends it and returns to that contact's details.
    /// </summary>
    public partial class ViewAs : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SecurityManager.IsAdmin())
            {
                Response.Redirect("~/Default.aspx", true);
                return;
            }

            if (string.Equals(Request.QueryString["exit"], "1", StringComparison.Ordinal))
            {
                int previewed = ContactPortalManager.StopViewAs();
                Response.Redirect(previewed > 0 ? "~/Pages/ContactDetails.aspx?ID=" + previewed : "~/Default.aspx", true);
                return;
            }

            int.TryParse(Request.QueryString["ContactID"], out int contactId);
            var portal = new ContactPortalManager();
            portal.EnsureReady();
            if (!portal.StartViewAs(contactId, out string message))
            {
                Response.ContentType = "text/plain";
                Response.Write(message);
                Response.End();
                return;
            }
            Response.Redirect("~/Portal/Home.aspx", true);
        }
    }
}
