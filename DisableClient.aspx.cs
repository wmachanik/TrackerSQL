using System;
using System.Configuration;
using System.Web.UI;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Repositories;

namespace TrackerSQL
{
    public partial class DisableClient : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BindMessages();

            if (IsPostBack)
                return;

            ShowConfirmation();
            LoadContact();
        }

        protected void btnConfirmDisable_Click(object sender, EventArgs e)
        {
            if (!TryGetContactId(out var contactId) || !IsValidToken(contactId))
            {
                ShowInvalidRequest(MessageProvider.Get(MessageKeys.DisableClient.ErrorInvalidToken));
                return;
            }

            var contact = new ContactsRepository().GetById(contactId);
            if (contact == null)
            {
                ShowInvalidRequest(MessageProvider.Get(MessageKeys.DisableClient.ErrorCustomerNotFound));
                return;
            }

            bool disableAll = rbDisableAll != null && rbDisableAll.Checked;
            if (!DisableClientManager.DisableFromEmailLink(contactId, disableAll))
            {
                ShowInvalidRequest(MessageProvider.Get(MessageKeys.DisableClient.ErrorGeneral));
                return;
            }

            CompanyNameSuccessLabel.Text = Server.HtmlEncode(contact.CompanyName ?? string.Empty);
            ltrlSuccessDetails.Text = disableAll
                ? MessageProvider.Get(MessageKeys.DisableClient.SuccessDetailsAll)
                : MessageProvider.Get(MessageKeys.DisableClient.SuccessDetails);
            ShowSuccess();
        }

        private void BindMessages()
        {
            string contactEmail = ConfigurationManager.AppSettings["SysEmailFrom"] ?? "orders@quaffee.co.za";
            string encodedEmail = Server.HtmlEncode(contactEmail);
            string mailTo = "<a href=\"mailto:" + encodedEmail + "\">" + encodedEmail + "</a>";

            ltrlPageHeading.Text = "Coffee Checkup Reminder Settings";
            ltrlConfirmationHeader.Text = MessageProvider.Get(MessageKeys.DisableClient.ConfirmationHeader);
            ltrlConfirmationMessage.Text = MessageProvider.Get(MessageKeys.DisableClient.ConfirmationMessage);
            ltrlWarningMessage.Text = MessageProvider.Get(MessageKeys.DisableClient.WarningMessage);

            lblRemindersOnly.Text = MessageProvider.Get(MessageKeys.DisableClient.OptionReminders);
            ltrlRemindersHelp.Text = MessageProvider.Get(MessageKeys.DisableClient.OptionRemindersHelp);
            lblDisableAll.Text = MessageProvider.Get(MessageKeys.DisableClient.OptionAll);
            ltrlAllHelp.Text = MessageProvider.Get(MessageKeys.DisableClient.OptionAllHelp);
            ltrlOptionsHelp.Text = MessageProvider.Get(MessageKeys.DisableClient.OptionsHelp);

            btnConfirmDisable.Text = MessageProvider.Get(MessageKeys.DisableClient.ButtonConfirm);
            ltrlCancelText.Text = MessageProvider.Get(MessageKeys.DisableClient.ButtonCancel)
                .Replace("&amp;", "&");

            ltrlHelpMessage.Text = MessageProvider.Get(MessageKeys.DisableClient.HelpMessage) + " ";
            ltrlContactEmail.Text = mailTo;

            ltrlSuccessHeader.Text = MessageProvider.Get(MessageKeys.DisableClient.SuccessHeader);
            ltrlSuccessMessage.Text = MessageProvider.Get(MessageKeys.DisableClient.SuccessMessage);
            if (string.IsNullOrEmpty(ltrlSuccessDetails.Text))
                ltrlSuccessDetails.Text = MessageProvider.Get(MessageKeys.DisableClient.SuccessDetails);

            ltrlReenableMessage.Text = MessageProvider.Format(
                MessageKeys.DisableClient.ReenableMessage, mailTo);

            ltrlErrorHeader.Text = MessageProvider.Get(MessageKeys.DisableClient.ErrorHeader);
            ltrlReturnHomeText.Text = MessageProvider.Get(MessageKeys.DisableClient.ReturnHome);
        }

        private void LoadContact()
        {
            if (!TryGetContactId(out var contactId) || !IsValidToken(contactId))
            {
                ShowInvalidRequest(MessageProvider.Get(MessageKeys.DisableClient.ErrorInvalidParams));
                return;
            }

            var contact = new ContactsRepository().GetById(contactId);
            if (contact == null)
            {
                ShowInvalidRequest(MessageProvider.Get(MessageKeys.DisableClient.ErrorCustomerNotFound));
                return;
            }

            CompanyNameLabel.Text = Server.HtmlEncode(contact.CompanyName ?? string.Empty);
            CompanyNameSuccessLabel.Text = CompanyNameLabel.Text;
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
            errorSection.Style["display"] = "none";
        }

        private void ShowSuccess()
        {
            confirmationSection.Style["display"] = "none";
            successSection.Style["display"] = "block";
            errorSection.Style["display"] = "none";
            btnConfirmDisable.Enabled = false;
        }

        private void ShowInvalidRequest(string message)
        {
            // Message is the headline so "Customer not found" is clear; no preference form.
            ltrlErrorHeader.Text = Server.HtmlEncode(message ?? string.Empty);
            ltrlErrorMessage.Text = MessageProvider.Get(MessageKeys.DisableClient.ErrorReturnHint);
            confirmationSection.Style["display"] = "none";
            successSection.Style["display"] = "none";
            errorSection.Style["display"] = "block";
            btnConfirmDisable.Enabled = false;
        }
    }
}
