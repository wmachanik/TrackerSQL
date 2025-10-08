using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;
//using TrackerDotNet.Managers;

namespace TrackerDotNet.Managers
{
    /// <summary>
    /// Helper class for Coffee Checkup email operations
    /// </summary>
    public class CoffeeCheckupEmailManager
    {
        private EmailMailKitCls emailClient;
        
        public CoffeeCheckupEmailManager()
        {
            emailClient = new EmailMailKitCls();
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
        public void AddEmailToBatch(ContactToRemindWithItems contact, SendCheckEmailTextsData emailData, string orderType, string subject)
        {
            try
            {
                // Build complete email body
                string emailBody = BuildEmailBody(contact, emailData, orderType);
                
                // Determine recipient email
                string toEmail = !string.IsNullOrEmpty(contact.EmailAddress) ? contact.EmailAddress : contact.AltEmailAddress;
                
                // Add to batch
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
        
        // Add this helper method to the class
        private void AppendLineWithBreak(StringBuilder builder, string content)
        {
            builder.AppendLine(content + "<br/><br/>");
        }

        /// <summary>
        /// Builds the complete email body from template parts
        /// </summary>
        private string BuildEmailBody(ContactToRemindWithItems contact, SendCheckEmailTextsData emailData, string orderType)
        {
            var emailBuilder = new StringBuilder();

            // 1. Add greeting
            emailBuilder.AppendLine(BuildGreeting(contact));
            
            // 2. Add header content (with space cleanup)
            AppendLineWithBreak(emailBuilder, CleanupSpacing(emailData.Header));
            
            // 3. Add main body (with space cleanup)
            AppendLineWithBreak(emailBuilder, CleanupSpacing(emailData.Body));
            
            // 4. Add items table
            AppendLineWithBreak(emailBuilder,BuildItemsList(contact));
            
            // 5. Add footer content
            AppendLineWithBreak(emailBuilder, emailData.Footer);
            
            // 6. Add disable link
            emailBuilder.AppendLine(BuildDisableLink(contact));
            
            // Get the full email text
            string fullEmail = emailBuilder.ToString();
            
            // 7. Replace merge fields
            fullEmail = fullEmail.Replace("[#PREPDATE#]", contact.NextPrepDate.ToString("dddd, dd MMM"));
            fullEmail = fullEmail.Replace("[#DELIVERYDATE#]", contact.NextDeliveryDate.ToString("dddd, dd MMM"));
            
            // 8. Add signature with proper fallback handling - FIXED
            string signature = GetEmailSignatureWithFallback();
            fullEmail += "<br/>" + signature;
            
            return fullEmail;
        }
        
        /// <summary>
        /// Gets email signature with proper fallback handling
        /// </summary>
        private string GetEmailSignatureWithFallback()
        {
            try
            {
                // Try the MessageProvider method first
                string signature = MessageProvider.GetEmailSignature();
                
                // Check if it returned the key name instead of the actual signature
                if (signature == "EmailSignatureTemplate" || signature == "SignatureTemplate" || 
                    signature.StartsWith("MessageKeys.") || signature.Contains("SignatureTemplate"))
                {
                    // Fall back to the existing DefaultEmailSignature
                    signature = MessageProvider.Get("DefaultEmailSignature");
                    
                    // Replace {0} with user name if needed
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
                
                // Final fallback to manual signature
                string userName = GetCurrentUserName();
                return $"<br/>Kind regards,<br/>{userName}<br/><b>a member of the Quaffee Team</b><br/>" +
                       "📧 <a href='mailto:orders@quaffee.co.za'>orders@quaffee.co.za</a><br/>" +
                       "🌐 <a href='http://www.quaffee.co.za'>www.quaffee.co.za</a><br/>";
            }
        }
        
        /// <summary>
        /// Gets the current user name for email signatures
        /// </summary>
        private string GetCurrentUserName()
        {
            try
            {
                if (HttpContext.Current?.User?.Identity?.IsAuthenticated == true)
                {
                    string userName = HttpContext.Current.User.Identity.Name;
                    
                    // Clean up domain prefix if present (e.g., "DOMAIN\username" becomes "username")
                    if (userName.Contains("\\"))
                    {
                        userName = userName.Substring(userName.LastIndexOf("\\") + 1);
                    }
                    
                    // Capitalize first letter
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
            
            // Fallback to generic name
            return MessageProvider.Get(MessageKeys.CoffeeCheckup.GreetingGeneric)
                .Replace("<p>Hi ", "").Replace(",</p>", "").Replace("Coffee Lover", "a member of the Quaffee Team");
        }
        
        /// <summary>
        /// Cleans up spacing issues in email text
        /// </summary>
        private string CleanupSpacing(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            // Fix missing spaces after periods
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\.([A-Z])", ". $1");
            
            // Fix missing spaces after exclamation marks
            text = System.Text.RegularExpressions.Regex.Replace(text, @"!([A-Z])", "! $1");
            
            // Fix missing spaces after question marks
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\?([A-Z])", "? $1");
            
            return text;
        }
        
        /// <summary>
        /// Builds the HTML table of items they last ordered - USING MESSAGE KEYS
        /// </summary>
        private string BuildItemsList(ContactToRemindWithItems contact)
        {
            if (contact.ItemsContactRequires == null || !contact.ItemsContactRequires.Any())
                return string.Empty;
                
            var html = new StringBuilder();
            
            // Use MessageProvider for intro text
            html.AppendLine(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableIntro));
            
            // 1. Start table using existing template
            html.AppendLine(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableStart));
            
            // 2. Add table header row - Company/Contact header with company name
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableHeader),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableCompanyContact), // {0} - "Company/Contact" label
                contact.CompanyName)); // {1} - Actual company name (spans 2 columns)
            
            // 3. Start tbody
            html.AppendLine("<tbody>");
            
            // 4. Add prep date row
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowNormal),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableNextPrepDate), // {0} - "Next estimate prep date"
                contact.NextPrepDate.ToString("dd MMM, ddd"), // {1] - Actual prep date
                "")); // {2} - Third column empty

            // 5. Add dispatch date row
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowAlt),
                "Next estimate dispatch date", // {0} - Direct label
                contact.NextDeliveryDate.ToString("dd MMM, ddd"), // {1} - Actual delivery date  
                "")); // {2} - Third column empty

            // 6. Add order type row
            string orderType = GetOrderTypeForDisplay(contact);
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowNormal),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableType), // {0} - "Type"
                orderType, // {1} - Order type description
                "")); // {2} - Third column empty

            // 7. Add "List of Items" header row with PROPER COLSPAN using message key
            html.AppendLine(string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowColspan),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableListOfItems))); // {0} - "List of Items"

            // 8. Add each item row with better formatting
            for (int i = 0; i < contact.ItemsContactRequires.Count; i++)
            {
                var item = contact.ItemsContactRequires[i];
                string itemDesc = ItemTypeTbl.GetItemTypeDescById(item.ItemID);
                string packaging = item.ItemPackagID > 0 ? new PackagingTbl().GetPackagingDesc(item.ItemPackagID) : "";
                string formattedQty = FormatQuantity(item.ItemQty, item.ItemID);
                
                // Combine quantity and packaging in one column for better readability
                string qtyWithPackaging = string.IsNullOrEmpty(packaging) 
                    ? formattedQty 
                    : $"{formattedQty} ({packaging})";
                
                string rowTemplate = (i + 5) % 2 == 0 ? // +5 to continue alternating from previous rows
                    MessageKeys.CoffeeCheckup.HtmlTableRowNormal : 
                    MessageKeys.CoffeeCheckup.HtmlTableRowAlt;
                    
                html.AppendLine(string.Format(MessageProvider.Get(rowTemplate),
                    itemDesc, // {0} - Item name (clean description)
                    qtyWithPackaging, // {1} - Quantity with packaging info
                    "")); // {2} - Third column empty
            }

            // 9. Close table
            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            
            return html.ToString();
        }
        
        /// <summary>
        /// Gets display-friendly order type text
        /// </summary>
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
        
        /// <summary>
        /// Formats quantity with proper decimal places and units
        /// </summary>
        private string FormatQuantity(double qty, int itemId)
        {
            int dp = SystemConstants.DatabaseConstants.NumDecimalPoints; // 4
            // Round first to ensure consistent midpoint handling
            double rounded = Math.Round(qty, dp, MidpointRounding.AwayFromZero);

            // Build a flexible format: up to dp decimals, suppress trailing zeros
            // Example for 4: "0.####"
            string format = dp > 0 ? ("0." + new string('#', dp)) : "0";
            string formattedQty = rounded.ToString(format);

            string unitOfMeasure = new ItemTypeTbl().GetItemUnitOfMeasure(itemId);

            if (!string.IsNullOrEmpty(unitOfMeasure))
            {
                // Use rounded for plural decision
                bool isPluralWhole = rounded > 1.0 && Math.Abs(rounded - Math.Floor(rounded)) < 0.0000001;
                formattedQty += " " + (isPluralWhole ? unitOfMeasure + "s" : unitOfMeasure);
            }

            return formattedQty;
        }
        
        /// <summary>
        /// Builds personalized greeting using message keys
        /// </summary>
        private string BuildGreeting(ContactToRemindWithItems contact)
        {
            if (!string.IsNullOrEmpty(contact.ContactFirstName))
            {
                return string.Format(MessageProvider.Get(MessageKeys.CoffeeCheckup.GreetingWithName), contact.ContactFirstName);
            }
            
            return MessageProvider.Get(MessageKeys.CoffeeCheckup.GreetingGeneric);
        }
        
        /// <summary>
        /// Builds the disable client link using existing DisableClientManager and message keys
        /// </summary>
        private string BuildDisableLink(ContactToRemindWithItems contact)
        {
            try
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Generating disable link for customer {contact.CustomerID}");
                
                // Generate secure disable link using existing manager
                string disableLink = DisableClientManager.GenerateDisableLink(contact.CustomerID);
                
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Secure disable link generated: {disableLink}");
                
                // Get the message template
                string template = MessageProvider.Get(MessageKeys.CoffeeCheckup.FooterDisableLink);
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Disable link template: {template}");
                
                // Use the proper message template with secure link
                string result = string.Format(template, disableLink);
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Final disable link HTML: {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"DISABLE LINK ERROR for customer {contact.CustomerID}: {ex.Message}");
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"DISABLE LINK STACK TRACE: {ex.StackTrace}");
                
                // FIXED: Better fallback - NO MAILTO
                return $"<br/><br/><p>If you would prefer not to receive these reminders, " +
                       $"<a href='https://tracker.quaffee.co.za/DisableClient.aspx?{SystemConstants.UrlParameterConstants.CustomerID}={contact.CustomerID}'>click here to disable them</a>.</p>";
            }
        }
        /// <summary>
        /// Sends an admin notification email
        /// </summary>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body content</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        public bool SendAdminNotification(string subject, string body)
        {
            try
            {
                var emailSettings = new EmailSettings();
                string adminEmail = ConfigHelper.GetString("SysEmailFrom", SystemConstants.EmailConstants.DefaultAdminEmail);
                emailSettings.SetRecipient(adminEmail);  // system email is the admin address

                var email = new EmailMailKitCls(emailSettings);
                email.SetEmailSubject(subject);
                email.AddFormatToBody(body);
                bool result = email.SendEmail();

                if (result)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"CoffeeCheckupEmailManager: Admin notification sent successfully to {adminEmail}");
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"CoffeeCheckupEmailManager: Failed to send admin notification: {emailClient.LastErrorSummary}");
                }

                return result;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"CoffeeCheckupEmailManager: Error sending admin notification: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Validates system configuration before sending emails
        /// </summary>
        public bool ValidateConfiguration()
        {
            try
            {
                // Test DisableClientManager configuration
                string testLink = DisableClientManager.GenerateDisableLink(999999); // Test customer ID
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"TEST: Disable link generation successful: {testLink}");
                
                // Test message keys exist
                string testGreeting = MessageProvider.Get(MessageKeys.CoffeeCheckup.GreetingGeneric);
                string testSignature = MessageProvider.Get("DefaultEmailSignature");
                
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, "TEST: All message keys accessible");
                
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"TEST: Configuration validation failed: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Sends a direct email to a specific recipient (for goodbye emails, etc.)
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">HTML email body</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        public bool SendDirectEmail(string toEmail, string subject, string htmlBody)
        {
            try
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"CoffeeCheckupEmailManager: Sending direct email to {toEmail} - {subject}");
                
                // Use the existing email infrastructure
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