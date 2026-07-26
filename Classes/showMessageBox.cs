//------------------------------------------------------------------------------
// TrackerSQL v3.x — showMessageBox
// Shared infrastructure / utility: showMessageBox.
//------------------------------------------------------------------------------

using System.Web.UI;

//- only form later versions #nullable disable
namespace TrackerSQL.Classes
{
    public class showMessageBox
    {
        public showMessageBox(Page pPage, string pTitle, string pMessage)
        {
            if (pPage == null)
                return;
            // Encode so newlines/quotes in the message do not break the script (and block later redirects).
            string script = "showAppMessage(" + System.Web.HttpUtility.JavaScriptStringEncode(pMessage ?? string.Empty, true) + ");";
            ScriptManager.RegisterStartupScript(pPage, pPage.GetType(), pTitle, script, true);
        }
    }
}
