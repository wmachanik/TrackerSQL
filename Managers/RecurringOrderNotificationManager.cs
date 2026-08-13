using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Emails the contact when a recurring order is added, updated, or disabled.
    /// Texts live in Resources/Messages.resx under MessageKeys.RecurringOrder.*;
    /// toggle: SystemConstants.RecurringOrderConstants.NotifyContactOnChange.
    /// </summary>
    public class RecurringOrderNotificationManager
    {
        public enum ChangeKind
        {
            Added,
            Updated,
            Disabled
        }

        /// <summary>
        /// Sends the notification. Returns null on success (or when notifications are off /
        /// skipped); otherwise returns a short error string suitable for status/logging.
        /// Never throws — save/disable must not fail because email failed.
        /// </summary>
        public string NotifyContact(int contactId, int recurringOrderId, ChangeKind kind)
        {
            if (!SystemConstants.RecurringOrderConstants.NotifyContactOnChange)
                return null;

            if (contactId <= 0 || recurringOrderId <= 0)
                return null;

            try
            {
                var contact = new ContactsRepository().GetById(contactId);
                if (contact == null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                        $"RecurringOrder notify skipped: ContactID={contactId} not found.");
                    return null;
                }

                string recipient = !string.IsNullOrWhiteSpace(contact.EmailAddress)
                    ? contact.EmailAddress.Trim()
                    : (contact.AltEmailAddress ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(recipient))
                {
                    string noEmail = MessageProvider.Get(MessageKeys.RecurringOrder.NoEmailAddress);
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                        $"RecurringOrder notify skipped for ContactID={contactId}: {noEmail}");
                    return noEmail;
                }

                var lineSummaries = new RecurringOrdersRepository()
                    .GetSummariesByContactId(contactId)
                    .Where(summary => summary.RecurringOrderID == recurringOrderId)
                    .ToList();

                string contactName = TrackerTools.SafeString(
                    !string.IsNullOrWhiteSpace(contact.ContactFirstName)
                        ? contact.ContactFirstName
                        : contact.CompanyName,
                    SystemConstants.EmailConstants.DefaultContact);
                string companyName = TrackerTools.SafeString(contact.CompanyName, "your account");
                string notes = lineSummaries.Count > 0
                    ? TrackerTools.SafeString(lineSummaries[0].Notes, "none")
                    : "none";
                string itemsHtml = BuildItemsListHtml(lineSummaries);

                string subjectKey;
                string bodyKey;
                switch (kind)
                {
                    case ChangeKind.Added:
                        subjectKey = MessageKeys.RecurringOrder.AddedEmailSubject;
                        bodyKey = MessageKeys.RecurringOrder.AddedEmailBody;
                        break;
                    case ChangeKind.Disabled:
                        subjectKey = MessageKeys.RecurringOrder.DisabledEmailSubject;
                        bodyKey = MessageKeys.RecurringOrder.DisabledEmailBody;
                        break;
                    default:
                        subjectKey = MessageKeys.RecurringOrder.UpdatedEmailSubject;
                        bodyKey = MessageKeys.RecurringOrder.UpdatedEmailBody;
                        break;
                }

                var emailSettings = new EmailSettings();
                emailSettings.SetRecipient(recipient);

                var email = new EmailMailKitCls(emailSettings);
                email.AddSysCCFAddress();
                email.SetEmailSubject(MessageProvider.Get(subjectKey));

                string body = MessageProvider.Format(bodyKey, contactName, companyName, itemsHtml, notes)
                    + MessageProvider.Get(MessageProvider.GetEmailSignature());
                email.AddToBody(body);

                if (!email.SendEmail())
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email, MessageProvider.Format(
                        MessageKeys.Email.SendError, recipient, email.LastErrorSummary));
                    AppLogger.WriteLog(SystemConstants.LogTypes.Recurring,
                        $"RecurringOrder {recurringOrderId} | Contact={contactId} ({companyName}) | Notification email failed | kind={kind}; to={recipient}; {email.LastErrorSummary}");
                    return email.LastErrorSummary;
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"RecurringOrder {kind} notification sent to {recipient} "
                    + $"(ContactID={contactId}, RecurringOrderID={recurringOrderId}).");
                AppLogger.WriteLog(SystemConstants.LogTypes.Recurring,
                    $"RecurringOrder {recurringOrderId} | Contact={contactId} ({companyName}) | Notification email sent | kind={kind}; to={recipient}; status=OK");
                return null;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"RecurringOrder notify failed ContactID={contactId} "
                    + $"RecurringOrderID={recurringOrderId} kind={kind}: {ex.Message}");
                AppLogger.WriteLog(SystemConstants.LogTypes.Recurring,
                    $"RecurringOrder {recurringOrderId} | Contact={contactId} | Notification email failed | kind={kind}; {ex.Message}");
                return ex.Message;
            }
        }

        private static string BuildItemsListHtml(List<RecurringOrderSummary> lineSummaries)
        {
            if (lineSummaries == null || lineSummaries.Count == 0)
                return MessageProvider.Get(MessageKeys.RecurringOrder.ItemsListEmpty);

            var builder = new StringBuilder();
            foreach (var line in lineSummaries)
            {
                string qty = line.QtyRequired.HasValue
                    ? SystemConstants.FormatConstants.FormatQuantity(line.QtyRequired.Value)
                    : "n/a";
                string nextDate = line.NextDateRequired.HasValue
                    ? line.NextDateRequired.Value.ToString("yyyy-MM-dd")
                    : "n/a";
                string pattern = !string.IsNullOrWhiteSpace(line.RecurringPatternDisplay)
                    ? line.RecurringPatternDisplay
                    : TrackerTools.SafeString(line.RecurringTypeDesc);

                builder.Append(MessageProvider.Format(
                    MessageKeys.RecurringOrder.ItemsListItem,
                    TrackerTools.SafeString(line.ItemDesc),
                    qty,
                    pattern,
                    nextDate));
            }

            return builder.ToString();
        }
    }
}
