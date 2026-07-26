//------------------------------------------------------------------------------
// TrackerSQL v3.x — Register
// Membership / account page code-behind for Register.
//------------------------------------------------------------------------------

using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerSQL.Account
{
    public partial class Register : Page
    {
        protected CreateUserWizard RegisterUser;
        protected CreateUserWizardStep RegisterUserWizardStep;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.RegisterUser.ContinueDestinationPageUrl = this.Request.QueryString["ReturnUrl"];
        }

        protected void RegisterUser_CreatedUser(object sender, EventArgs e)
        {
            FormsAuthentication.SetAuthCookie(this.RegisterUser.UserName, false);
            string url = this.RegisterUser.ContinueDestinationPageUrl;
            if (string.IsNullOrEmpty(url))
                url = "~/";
            this.Response.Redirect(url);
        }
    }
}
