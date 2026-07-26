//------------------------------------------------------------------------------
// TrackerSQL v3.x — ChangePassword
// Membership / account page code-behind for ChangePassword.
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using TrackerSQL.Classes;

////- only form later versions #nullable disable
namespace TrackerSQL.Account
{

    public partial class ChangePassword : Page
    {
        protected System.Web.UI.WebControls.ChangePassword ChangeUserPassword;

        protected void Page_Load(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "User change password page entered.");

        }
    }
}
