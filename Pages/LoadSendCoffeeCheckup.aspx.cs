//------------------------------------------------------------------------------
// TrackerSQL v3.x — LoadSendCoffeeCheckup
// WebForms page code-behind for LoadSendCoffeeCheckup.
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public partial class LoadSendCoffeeCheckup : Page
    {
        private const string CONST_LOADTIMERFIRE = "LoadTimerFier";
        protected HtmlGenericControl DivLoading;

        protected void Page_Load(object sender, EventArgs e)
        {
            int num = this.IsPostBack ? 1 : 0;
        }
    }
}
