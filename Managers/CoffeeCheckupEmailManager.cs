using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Helper class for Coffee Checkup email operations
    /// </summary>
    public class CoffeeCheckupEmailManager
    {
        private readonly EmailMailKitCls emailClient;
        private readonly ItemsRepository _itemsRepository;
        private readonly ItemPackagingsRepository _itemPackagingsRepository;

        public CoffeeCheckupEmailManager()
        {
            emailClient = new EmailMailKitCls();
            _itemsRepository = new ItemsRepository();
            _itemPackagingsRepository = new ItemPackagingsRepository();
        }

        /// <summary>
        /// Gets appropriate email subject based on order type
        /// </summary>
        public string GetEmailSubject(string orderType)
        {
            if (string.IsNullOrWhiteSpace(orderType))
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.SubjectReminderOnly);

            if (orderType.Contains("combination"))
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.SubjectCombined);
            else if (orderType.Contains("recurring"))
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.SubjectRecurring);
            else if (orderType.Contains("autofulfill"))
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.SubjectAutoFulfill);

            return MessageProvider.Get(MessageKeys.CoffeeCheckup.SubjectReminderOnly);
        }

        /// <summary>
        /// Adds an email to the batch for sending
        /// </summary>
        public void AddEmailToBatch(ContactToRemindWithItems contact, SendCheckEmailTexts emailData, string orderType, string subject)
        {
            try
            {
                string emailBody = BuildEmailBody(contact, emailData, orderType);
                string toEmail = !string.IsNullOrEmpty(contact.EmailAddress) ? contact.EmailAddress : contact.AltEmailAddress;
                emailClient.AddToBatch(subject, emailBody, null, toEmail);
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Added {contact.CompanyName} to email batch");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Failed to add {contact.CompanyName} to batch: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends all emails in the batch
        /// </summary>
        public BatchSendResult SendBatch()
        {
            try
            {
                bool success = emailClient.SendEmailBatch();

                return new BatchSendResult
                {
                    IsSuccess = success,
                    TotalSent = success ? 1 : 0,
                    TotalFailed = success ? 0 : 1,
                    ErrorMessage = success ? "" : emailClient.LastErrorSummary
                };
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Batch send failed: {ex.Message}");
                return new BatchSendResult
                {
                    IsSuccess = false,
                    TotalSent = 0,
                    TotalFailed = 1,
                    ErrorMessage = ex.Message
                };
            }
        }

        private void AppendLineWithBreak(StringBuilder builder, string content)
        {
            builder.AppendLine(content + "<br/><br/>");
        }

        private string BuildEmailBody(ContactToRemindWithItems contact, SendCheckEmailTexts emailData, string orderType)
        {
            var emailBuilder = new StringBuilder();

            emailBuilder.AppendLine(BuildGreeting(contact));
            AppendLineWithBreak(emailBuilder, CleanupSpacing(emailData.Header));
            AppendLineWithBreak(emailBuilder, CleanupSpacing(emailData.Body));
            AppendLineWithBreak(emailBuilder, BuildItemsList(contact));
            AppendLineWithBreak(emailBuilder, emailData.Footer);
            emailBuilder.AppendLine(BuildDisableLink(contact));

            string fullEmail = emailBuilder.ToString();
            fullEmail = fullEmail.Replace("[#PREPDATE#]", contact.NextPreperationDate.ToString("dddd, dd MMM"));
            fullEmail = fullEmail.Replace("[#DELIVERYDATE#]", contact.NextDeliveryDate.ToString("dddd, dd MMM"));

            string signature = GetEmailSignatureWithFallback();
            fullEmail += "<br/>" + signature;

            return fullEmail;
        }

        private string GetEmailSignatureWithFallback()
        {
            try
            {
                string signature = MessageProvider.GetEmailSignature();

                if (signature == "EmailSignatureTemplate" || signature == "SignatureTemplate" ||
                    signature.StartsWith("MessageKeys.") || signature.Contains("SignatureTemplate"))
                {
                    signature = MessageProvider.Get("DefaultEmailSignature");

                    if (signature.Contains("{0}"))
                    {
                        string userName = GetCurrentUserName();
                        signature = signature.Replace("{0}", userName);
                    }
                }

                return signature;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Error getting email signature: {ex.Message}");
                string userName = GetCurrentUserName();
                return $"<br/>Kind regards,<br/>{userName}<br/><b>a member of the Quaffee Team</b><br/>" +
                       "?? <a href='mailto:orders@quaffee.co.za'>orders@quaffee.co.za</a><br/>" +
                       "?? <a href='http://www.quaffee.co.za'>www.quaffee.co.za</a><br/>";
            }
        }

        private string GetCurrentUserName()
        {
            try
            {
                if (HttpContext.Current?.User?.Identity?.IsAuthenticated == true)
                {
                    string userName = HttpContext.Current.User.Identity.Name;

                    if (userName.Contains("\\"))
                    {
                        userName = userName.Substring(userName.LastIndexOf("\\") + 1);
                    }

                    if (!string.IsNullOrEmpty(userName))
                    {
                        return char.ToUpper(userName[0]) + userName.Substring(1).ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Error getting current user name: {ex.Message}");
            }

            return MessageProvider.Get(MessageKeys.CoffeeCheckup.GreetingGeneric)
                .Replace("<p>Hi ", "").Replace(",</p>", "").Replace("Coffee Lover", "a member of the Quaffee Team");
        }

        private string CleanupSpacing(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = System.Text.RegularExpressions.Regex.Replace(text, @"\.([A-Z])", ". $1");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"!([A-Z])", "! $1");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\?([A-Z])", "? $1");

            return text;
        }

        private string BuildItemsList(ContactToRemindWithItems contact)
        {
            if (contact.ItemsContactRequires == null || !contact.ItemsContactRequires.Any())
                return string.Empty;

            var html = new StringBuilder();
            html.AppendLine(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableIntro));
            html.AppendLine(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableStart));
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableHeader),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableCompanyContact),
                contact.CompanyName));
            html.AppendLine("<tbody>");
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowNormal),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableNextPreperationDate),
                contact.NextPreperationDate.ToString("dd MMM, ddd"),
                ""));
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowAlt),
                "Next estimate dispatch date",
                contact.NextDeliveryDate.ToString("dd MMM, ddd"),
                ""));
            string orderType = GetOrderTypeForDisplay(contact);
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowNormal),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableType),
                orderType,
                ""));
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowColspan),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableListOfItems)));

            for (int i = 0; i < contact.ItemsContactRequires.Count; i++)
            {
                var item = contact.ItemsContactRequires[i];
                string itemDesc = _itemsRepository.GetItemDescById(item.ItemID);
                string packaging = item.ItemPackagID > 0 ? _itemPackagingsRepository.GetPackagingDescById(item.ItemPackagID) : "";
                string formattedQty = FormatQuantity(item.ItemQty, item.ItemID);

                string qtyWithPackaging = string.IsNullOrEmpty(packaging)
                    ? formattedQty
                    : $"{formattedQty} ({packaging})";

                string rowTemplate = (i + 5) % 2 == 0
                    ? MessageKeys.CoffeeCheckup.HtmlTableRowNormal
                    : MessageKeys.CoffeeCheckup.HtmlTableRowAlt;

                html.AppendLine(string.Format(MessageProvider.Get(rowTemplate),
                    itemDesc,
                    qtyWithPackaging,
                    ""));
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            return html.ToString();
        }

        private string GetOrderTypeForDisplay(ContactToRemindWithItems contact)
        {
            bool hasAutoFulfill = contact.ItemsContactRequires.Exists(x => x.AutoFulfill);
            bool hasRecurring = contact.ItemsContactRequires.Exists(x => x.ReoccurOrder);

            if (hasRecurring && hasAutoFulfill)
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.OrderTypeCombined);
            else if (hasRecurring)
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.OrderTypeRecurring);
            else if (hasAutoFulfill)
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.OrderTypeAutoFulfill);

            return MessageProvider.Get(MessageKeys.CoffeeCheckup.OrderTypeReminderOnly);
        }

        private string FormatQuantity(double qty, int itemId)
        {
            int dp = SystemConstants.DatabaseConstants.NumDecimalPoints;
            double rounded = Math.Round(qty, dp, MidpointRounding.AwayFromZero);
            string format = dp > 0 ? ("0." + new string('#', dp)) : "0";
            string formattedQty = rounded.ToString(format);
            string unitOfMeasure = _itemsRepository.GetItemUnitOfMeasure(itemId);

            if (!string.IsNullOrEmpty(unitOfMeasure))
            {
                bool isPluralWhole = rounded > 1.0 && Math.Abs(rounded - Math.Floor(rounded)) < 0.0000001;
                formattedQty += " " + (isPluralWhole ? unitOfMeasure + "s" : unitOfMeasure);
            }

            return formattedQty;
        }

        private string BuildGreeting(ContactToRemindWithItems contact)
        {
            if (!string.IsNullOrEmpty(contact.ContactFirstName))
            {
                return string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.GreetingWithName), contact.ContactFirstName);
            }

            return MessageProvider.Get(MessageKeys.CoffeeCheckup.GreetingGeneric);
        }

        private string BuildDisableLink(ContactToRemindWithItems contact)
        {
            try
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Generating disable link for customer {contact.CustomerID}");
                string disableLink = DisableClientManager.GenerateDisableLink(contact.CustomerID);
                string template = MessageProvider.Get(MessageKeys.CoffeeCheckup.FooterDisableLink);
                return string.Format(template, disableLink);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"DISABLE LINK ERROR for customer {contact.CustomerID}: {ex.Message}");
                return $"<br/><br/><p>If you would prefer not to receive these reminders, " +
                       $"<a href='https://tracker.quaffee.co.za/DisableClient.aspx?{SystemConstants.UrlParameterConstants.CustomerID}={contact.CustomerID}'>click here to disable them</a>.</p>";
            }
        }

        public bool SendAdminNotification(string subject, string body)
        {
            try
            {
                string adminEmail = ConfigHelper.GetString("SysEmailFrom", SystemConstants.EmailConstants.DefaultAdminEmail);
                string smtpLogin = ConfigHelper.GetString("EMailLogIn", string.Empty);

                string htmlBody;
                {
                    var sb = new StringBuilder();
                    var lines = body.Replace("\r", "").Split('\n');
                    bool inList = false;

                    foreach (var raw in lines)
                    {
                        var line = raw?.Trim();
                        if (string.IsNullOrEmpty(line))
                        {
                            if (inList)
                            {
                                sb.AppendLine("</ul>");
                                inList = false;
                            }
                            sb.AppendLine("<br/>");
                            continue;
                        }

                        if (line.StartsWith("- ") || line.StartsWith(" -"))
                        {
                            if (!inList)
                            {
                                sb.AppendLine("<ul>");
                                inList = true;
                            }
                            var itemText = line.TrimStart('-', ' ').Trim();
                            sb.AppendLine($"<li>{HttpUtility.HtmlEncode(itemText)}</li>");
                            continue;
                        }

                        if (line.StartsWith("??") || line == line.ToUpperInvariant() && line.Length < 80)
                        {
                            if (inList)
                            {
                                sb.AppendLine("</ul>");
                                inList = false;
                            }
                            sb.AppendLine($"<p><strong>{HttpUtility.HtmlEncode(line)}</strong></p>");
                            continue;
                        }

                        if (inList)
                        {
                            sb.AppendLine("</ul>");
                            inList = false;
                        }
                        sb.AppendLine($"<p>{HttpUtility.HtmlEncode(line)}</p>");
                    }

                    if (inList)
                        sb.AppendLine("</ul>");

                    htmlBody = sb.ToString();
                }

                var email = new EmailMailKitCls();
                email.IsTestMode = false;

                if (!string.IsNullOrEmpty(smtpLogin))
                    email.SetEmailFromTo(smtpLogin, adminEmail);
                else
                    email.SetEmailFromTo(null, adminEmail);

                email.SetEmailSubject(subject);
                email.AddFormatToBody(htmlBody);

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupEmailManager: Attempting admin notification From={(string.IsNullOrEmpty(smtpLogin) ? "(default)" : smtpLogin)} To={adminEmail} Subject={subject}");
                bool result = email.SendEmail();

                if (result)
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupEmailManager: Admin notification sent successfully to {adminEmail}");
                else
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupEmailManager: Failed to send admin notification: {email.LastErrorSummary}");

                return result;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupEmailManager: Error sending admin notification: {ex.Message}");
                return false;
            }
        }

        public bool ValidateConfiguration()
        {
            try
            {
                string testLink = DisableClientManager.GenerateDisableLink(999999);
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"TEST: Disable link generation successful: {testLink}");
                MessageProvider.Get(MessageKeys.CoffeeCheckup.GreetingGeneric);
                MessageProvider.Get("DefaultEmailSignature");
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, "TEST: All message keys accessible");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"TEST: Configuration validation failed: {ex.Message}");
                return false;
            }
        }

        public bool SendDirectEmail(string toEmail, string subject, string htmlBody)
        {
            try
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"CoffeeCheckupEmailManager: Sending direct email to {toEmail} - {subject}");
                bool result = emailClient.AddToBatch(subject, htmlBody, null, toEmail);

                if (result)
                {
                    result = emailClient.SendEmailBatch();
                }

                if (result)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"CoffeeCheckupEmailManager: Direct email sent successfully to {toEmail}");
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"CoffeeCheckupEmailManager: Failed to send direct email to {toEmail}: {emailClient.LastErrorSummary}");
                }

                return result;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"CoffeeCheckupEmailManager: Error sending direct email to {toEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
