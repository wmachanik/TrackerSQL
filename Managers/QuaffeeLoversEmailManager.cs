using System;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Managers
{
    internal class QuaffeeLoversEmailManager
    {
        public bool SendWelcome(Contact contact, int wooOrderCount, out string error)
        {
            error = null;
            if (contact == null)
            {
                error = "Contact not found.";
                return false;
            }

            string to = PickEmail(contact);
            if (string.IsNullOrWhiteSpace(to))
            {
                error = "This contact has no email address.";
                return false;
            }

            string first = !string.IsNullOrWhiteSpace(contact.ContactFirstName)
                ? contact.ContactFirstName.Trim()
                : (!string.IsNullOrWhiteSpace(contact.CompanyName) ? contact.CompanyName.Trim() : "there");

            string subject = "Welcome to The Quaffee Lovers group";
            string body = "<p>Dear " + System.Web.HttpUtility.HtmlEncode(first) + ",</p>"
                + "<p>Thank you for your continued support. You have now qualified as a member of "
                + "<strong>The Quaffee Lovers</strong> group.</p>"
                + "<p>When you use our online shop you will automatically see member prices on all coffees. "
                + "This is our way of thanking you for your continued support.</p>"
                + "<p>With thanks,<br/>The Quaffee Team</p>";

            string portalBlurb = ContactPortalManager.BuildEmailPortalBlurbHtml((int)contact.ContactID);
            if (!string.IsNullOrWhiteSpace(portalBlurb))
                body += portalBlurb;
            body += MessageProvider.Get(MessageKeys.Order.EmailFooter);
            string signature = MessageProvider.GetEmailSignature();
            if (!string.IsNullOrWhiteSpace(signature))
                body += signature;

            try
            {
                var settings = new EmailSettings();
                settings.SetRecipient(to);
                var email = new EmailMailKitCls(settings) { IncludeConfiguredCc = false };
                email.SetEmailSubject(subject);
                email.AddToBody(body);
                if (!email.SendEmail())
                {
                    error = email.LastErrorSummary ?? "Email could not be sent.";
                    return false;
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "Quaffee Lovers welcome sent to contact " + contact.ContactID + " (" + to + ")");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "Quaffee Lovers welcome failed for contact " + contact.ContactID + ": " + ex.Message);
                return false;
            }
        }

        private static string PickEmail(Contact contact)
        {
            if (!string.IsNullOrWhiteSpace(contact.EmailAddress))
                return contact.EmailAddress.Trim();
            if (!string.IsNullOrWhiteSpace(contact.AltEmailAddress))
                return contact.AltEmailAddress.Trim();
            return null;
        }
    }
}
