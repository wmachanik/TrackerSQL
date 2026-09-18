using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class DisableClientManager
    {
        /// <summary>
        /// Disables a contact from the token-validated email link flow.
        /// Appends a contact note and notifies orders@ of the preference change.
        /// </summary>
        public static bool DisableFromEmailLink(int contactId, bool disableAll)
        {
            var repo = new ContactsRepository();
            if (!repo.ApplyEmailDisableChoice(contactId, disableAll))
                return false;

            try
            {
                Contact contact = repo.GetById(contactId);
                NotifyOrdersOfSelfServiceDisable(contact, disableAll);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "DisableClient admin notify failed for contact " + contactId + ": " + ex.Message);
            }

            return true;
        }

        /// <summary>
        /// Emails orders@ (SysCCEmailAddress / SysEmailFrom) that the contact changed preferences.
        /// </summary>
        private static void NotifyOrdersOfSelfServiceDisable(Contact contact, bool disableAll)
        {
            if (contact == null || contact.ContactID <= 0)
                return;

            string to = ConfigHelper.GetString("SysCCEmailAddress", string.Empty);
            if (string.IsNullOrWhiteSpace(to))
                to = ConfigHelper.GetString("SysEmailFrom", SystemConstants.EmailConstants.DefaultAdminEmail);
            if (string.IsNullOrWhiteSpace(to))
                to = SystemConstants.EmailConstants.DefaultAdminEmail;

            string company = string.IsNullOrWhiteSpace(contact.CompanyName)
                ? ("Contact #" + contact.ContactID)
                : contact.CompanyName.Trim();
            string choice = disableAll
                ? MessageProvider.Get(MessageKeys.DisableClient.AdminChoiceAll)
                : MessageProvider.Get(MessageKeys.DisableClient.AdminChoiceReminders);
            string contactEmail = !string.IsNullOrWhiteSpace(contact.EmailAddress)
                ? contact.EmailAddress.Trim()
                : (contact.AltEmailAddress ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(contactEmail))
                contactEmail = "(no email on file)";

            string subject = MessageProvider.Format(
                MessageKeys.DisableClient.AdminSubjectTemplate, company);
            string body = MessageProvider.Format(
                    MessageKeys.DisableClient.AdminBodyHeader,
                    contact.ContactID.ToString(),
                    company,
                    TimeZoneUtils.Now().ToString("yyyy-MM-dd HH:mm"),
                    choice,
                    contactEmail)
                + MessageProvider.Get(MessageKeys.DisableClient.AdminFooter);

            var settings = new EmailSettings();
            settings.SetRecipient(to);
            var email = new EmailMailKitCls(settings);
            email.SetEmailSubject(subject);
            email.AddFormatToBody(
                "<pre style=\"font-family:Consolas,monospace; white-space:pre-wrap;\">"
                + System.Web.HttpUtility.HtmlEncode(body)
                + "</pre>");

            if (email.SendEmail())
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "DisableClient admin notify sent to " + to
                    + " contact=" + contact.ContactID
                    + " disableAll=" + disableAll);
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "DisableClient admin notify failed to " + to + ": "
                    + (!string.IsNullOrWhiteSpace(email.LastErrorSummary)
                        ? email.LastErrorSummary
                        : (email.myResults.sResult ?? "unknown")));
            }
        }

        private const int MIN_SECRET_LENGTH = 32;
        private const int TOKEN_VALIDITY_HOURS = 24;

        /// <summary>
        /// Generates a secure link for disabling a client
        /// </summary>
        public static string GenerateDisableLink(long customerId)
        {
            ValidateSecret();
            string token = GenerateToken(customerId);
            string baseUrl = GetApplicationUrl();
            return $"{baseUrl}/DisableClient.aspx?{SystemConstants.UrlParameterConstants.CustomerID}={customerId}&token={token}";
        }

        /// <summary>
        /// Validates a disable request token
        /// </summary>
        public static bool ValidateToken(string customerId, string token)
        {
            if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(token))
                return false;

            string expectedToken = GenerateToken(long.Parse(customerId));
            return token.Equals(expectedToken, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Generates a new secure secret key for the application
        /// </summary>
        public static string GenerateNewSecret()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[48]; // 384 bits
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        private static string GenerateToken(long customerId)
        {
            string secret = ConfigurationManager.AppSettings["DisableClientSecret"];
            string dateString = TimeZoneUtils.Now().Date.ToString("yyyyMMdd");
            string input = $"{customerId}:{dateString}:{secret}";

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private static void ValidateSecret()
        {
            string secret = ConfigurationManager.AppSettings["DisableClientSecret"];
            if (string.IsNullOrEmpty(secret) || secret.Length < MIN_SECRET_LENGTH)
            {
                AppLogger.WriteLog("security", MessageProvider.Get(MessageKeys.Security.InvalidSecret));
                throw new ConfigurationErrorsException(MessageProvider.Get(MessageKeys.Security.InvalidSecretConfig));
            }
        }

        public static string GetApplicationUrl()
        {
            string baseUrl = ConfigHelper.GetString("ApplicationBaseUrl","");
            
            if (string.IsNullOrEmpty(baseUrl))
            {
                var context = System.Web.HttpContext.Current;
                if (context != null)
                {
                    var request = context.Request;
                    string appPath = request.ApplicationPath;
                    if (appPath == "/") appPath = "";
                    
                    baseUrl = string.Format("{0}://{1}{2}",
                        request.Url.Scheme,
                        request.Url.Authority,
                        appPath);
                }
                else
                {
                    AppLogger.WriteLog("error", MessageProvider.Get(MessageKeys.Security.NoHttpContext));
                    throw new InvalidOperationException(MessageProvider.Get(MessageKeys.Security.NoHttpContext));
                }
            }

            return baseUrl.TrimEnd('/');
        }
    }
}
