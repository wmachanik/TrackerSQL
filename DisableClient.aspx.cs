using System;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;
using TrackerSQL.Managers;

namespace TrackerSQL
{
    public partial class DisableClient : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetContactEmail();

            if (IsPostBack)
                return;

            ShowConfirmation();
            LoadContact();
        }

        protected void btnConfirmDisable_Click(object sender, EventArgs e)
        {
            if (!TryGetContactId(out var contactId) || !IsValidToken(contactId))
            {
                ShowInvalidRequest("Invalid or expired disable link.");
                return;
            }

            var contact = new ContactsRepository().GetById(contactId);
            if (contact == null)
            {
                ShowInvalidRequest("Contact not found.");
                return;
            }

            bool disableAll = rblDisableChoice.SelectedValue == "disable_all";
            if (!DisableClientManager.DisableFromEmailLink(contactId, disableAll))
            {
                ShowInvalidRequest("Unable to update contact. Please try again or contact us.");
                return;
            }

            CompanyNameSuccessLabel.Text = Server.HtmlEncode(contact.CompanyName ?? string.Empty);
            ShowSuccess();
        }

        private void LoadContact()
        {
            if (!TryGetContactId(out var contactId) || !IsValidToken(contactId))
            {
                ShowInvalidRequest("Invalid or expired disable link.");
                return;
            }

            var contact = new ContactsRepository().GetById(contactId);
            if (contact == null)
            {
                ShowInvalidRequest("Contact not found.");
                return;
            }

            CompanyNameLabel.Text = Server.HtmlEncode(contact.CompanyName ?? string.Empty);
            CompanyNameSuccessLabel.Text = CompanyNameLabel.Text;
        }

        private void SetContactEmail()
        {
            string email = ConfigurationManager.AppSettings["SysEmailFrom"] ?? "orders@quaffee.co.za";
            string encodedEmail = Server.HtmlEncode(email);
            string mailTo = "<a href=\"mailto:" + encodedEmail + "\">" + encodedEmail + "</a>";
            ltrlContactEmail.Text = mailTo;
            ltrlContactEmailSuccess.Text = mailTo;
        }

        private bool TryGetContactId(out int contactId)
        {
            return int.TryParse(Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID], out contactId);
        }

        private bool IsValidToken(int contactId)
        {
            return DisableClientManager.ValidateToken(contactId.ToString(), Request.QueryString["token"]);
        }

        private void ShowConfirmation()
        {
            confirmationSection.Style["display"] = "block";
            successSection.Style["display"] = "none";
        }

        private void ShowSuccess()
        {
            confirmationSection.Style["display"] = "none";
            successSection.Style["display"] = "block";
            btnConfirmDisable.Enabled = false;
        }

        private void ShowInvalidRequest(string message)
        {
            CompanyNameLabel.Text = Server.HtmlEncode(message);
            CompanyNameSuccessLabel.Text = Server.HtmlEncode(message);
            btnConfirmDisable.Enabled = false;
            rblDisableChoice.Enabled = false;
            ShowConfirmation();
        }
    }
}
