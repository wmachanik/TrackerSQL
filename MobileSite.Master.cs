//------------------------------------------------------------------------------
// TrackerSQL v3.x — MobileSiteMaster
// ASP.NET master page code-behind: MobileSiteMaster.
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerSQL
{
    public partial class MobileSiteMaster : MasterPage
    {
        protected ContentPlaceHolder HeadContent;
        protected HtmlForm frmMiniMain;
        protected HyperLink hlHome;
        protected HyperLink hlDeliveries;
        protected HyperLink hlRepairs;
        protected ContentPlaceHolder MainContent;

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}
