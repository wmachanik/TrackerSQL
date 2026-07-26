//------------------------------------------------------------------------------
// TrackerSQL v3.x — TestPage
// WebForms page code-behind for TestPage.
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public partial class TestPage : Page
    {
        protected HtmlForm form1;
        protected GridView GridView1;
        protected SqlDataSource SqlDataSource1;

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}
