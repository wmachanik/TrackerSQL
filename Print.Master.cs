//------------------------------------------------------------------------------
// TrackerSQL v3.x — PrintMasterPage
// ASP.NET master page code-behind: PrintMasterPage.
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace TrackerSQL
{
    public partial class PrintMasterPage : MasterPage
    {
        protected HtmlHead Head1;
        protected ContentPlaceHolder HeadContent;
        protected Image imgQuaffeeLogo;
        protected HtmlForm frmMain;
        protected ContentPlaceHolder MainContent;

        protected void Page_Load(object sender, EventArgs e) => this.Response.Write("  ");

        protected void btnClose_Click(object sender, EventArgs e)
        {
        }
    }
}
