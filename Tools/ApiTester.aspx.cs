using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Managers;

namespace TrackerSQL.Tools
{
    public partial class ApiTester : Page
    {
        protected Panel pnlAccessDenied;
        protected Panel pnlMain;
        protected Literal litMessage;
        protected Literal litAutoComplete;
        protected Literal litHttps;
        protected Literal litSyncNote;
        protected Literal litLogDays;
        protected Literal litTokenScript;
        protected Button btnWebToken;
        protected Button btnEnsureSchema;

        protected string ApiBase => ResolveUrl("~/api/v1");

        protected int TokenDays => MobileAuthManager.TokenDays;

        protected string LogDaysText => MobileAuthManager.LogDays > 0 ? MobileAuthManager.LogDays + " days" : "nothing (logging is off)";

        protected string IdleText => MobileApiSecurity.TokenIdleDays > 0 ? MobileApiSecurity.TokenIdleDays + " days" : "never";

        protected int LoginFailuresPerIp => MobileApiSecurity.LoginFailuresPerIp;

        protected string RequestsPerMinuteText => MobileApiSecurity.RequestsPerMinute > 0 ? MobileApiSecurity.RequestsPerMinute + " calls a minute" : "no limit";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SecurityManager.IsAdmin())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = true;
                return;
            }

            bool autoComplete = MobileAuthManager.AutoCompleteDeliveries;
            litAutoComplete.Text = autoComplete
                ? "complete the order straight away (Order Done, which emails the contact)"
                : "store the name and signature and leave the order for the office to complete";
            litHttps.Text = MobileAuthManager.RequireHttps ? "yes" : "no";
            litLogDays.Text = MobileAuthManager.LogDays.ToString();
            litSyncNote.Text = autoComplete
                ? " <strong>Auto-complete is on: sending a Delivered order also runs Order Done and emails the contact.</strong>"
                : " The name and signature are stored against the order; the order itself is not changed (the office completes it).";
        }

        protected void btnWebToken_Click(object sender, EventArgs e)
        {
            string userName = Membership.GetUser()?.UserName ?? User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                ShowMessage("Could not tell who is signed in.", true);
                return;
            }

            try
            {
                var response = new MobileAuthManager().IssueTokenForWebUser(userName, "API tester (web sign-in)");
                litTokenScript.Text = "<script type=\"text/javascript\">document.addEventListener('DOMContentLoaded',function(){apiTester.setToken("
                    + HttpUtility.JavaScriptStringEncode(response.Token, true) + ");});</script>";
                ShowMessage("You are now testing as " + HttpUtility.HtmlEncode(userName) + ".", false);
            }
            catch (Exception ex)
            {
                ShowMessage("Could not issue a token: " + HttpUtility.HtmlEncode(ex.Message), true);
            }
        }

        protected void btnEnsureSchema_Click(object sender, EventArgs e)
        {
            string error = MobileApiSchemaInstaller.Run();
            ShowMessage(error == null ? "Mobile API tables are ready." : "Could not create the tables: " + HttpUtility.HtmlEncode(error), error != null);
        }

        private void ShowMessage(string html, bool isError)
        {
            litMessage.Text = "<div class=\"status-message " + (isError ? "status-error" : "status-success") + "\">" + html + "</div>";
        }
    }
}
