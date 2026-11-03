using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class ClientMailManager
    {
        private readonly ClientMailRepository _repository = new ClientMailRepository();

        public List<ClientMailRecipient> Search(ClientMailFilter filter)
        {
            return _repository.Search(filter);
        }

        public string BuildBody(ClientMailRecipient contact, string headerHtml, string bodyHtml, string footerHtml, bool includeGreeting)
        {
            var builder = new StringBuilder();
            if (includeGreeting)
                builder.Append(BuildGreeting(contact));

            AppendSection(builder, ReplacePlaceholders(headerHtml, contact));
            AppendSection(builder, ReplacePlaceholders(bodyHtml, contact));
            AppendSection(builder, ReplacePlaceholders(footerHtml, contact));

            string signature = MessageProvider.GetEmailSignature();
            if (!string.IsNullOrWhiteSpace(signature) && !signature.StartsWith("MessageKeys.") && signature != "Email.SignatureTemplate")
                builder.Append(signature);

            return builder.ToString();
        }

        public BatchSendResult Send(
            IList<ClientMailRecipient> recipients,
            string subject,
            string headerHtml,
            string bodyHtml,
            string footerHtml,
            bool includeGreeting,
            bool includeCc,
            int groupSize,
            int delayMilliseconds)
        {
            var result = new BatchSendResult { IsSuccess = true };
            if (recipients == null || recipients.Count == 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "No recipients.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Subject is required.";
                return result;
            }

            int size = groupSize <= 0 ? 20 : groupSize;

            for (int i = 0; i < recipients.Count; i += size)
            {
                var email = new EmailMailKitCls { IncludeConfiguredCc = includeCc };
                var group = recipients.Skip(i).Take(size).ToList();
                int queued = 0;
                foreach (var contact in group)
                {
                    string to = contact.ToEmail;
                    if (string.IsNullOrWhiteSpace(to))
                    {
                        result.TotalFailed++;
                        result.IsSuccess = false;
                        result.ErrorMessage = AppendError(result.ErrorMessage, contact.CompanyName + ": no email");
                        continue;
                    }

                    string html = BuildBody(contact, headerHtml, bodyHtml, footerHtml, includeGreeting);
                    if (!email.AddToBatch(subject, html, null, to))
                    {
                        result.TotalFailed++;
                        result.IsSuccess = false;
                        result.ErrorMessage = AppendError(result.ErrorMessage, contact.CompanyName + ": could not queue");
                    }
                    else
                    {
                        queued++;
                    }
                }

                if (queued == 0)
                    continue;

                if (!email.SendEmailBatch(delayMilliseconds))
                {
                    result.TotalFailed += queued;
                    result.IsSuccess = false;
                    result.ErrorMessage = AppendError(result.ErrorMessage, email.LastErrorSummary);
                    AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail,
                        "ClientMail send group failed: " + email.LastErrorSummary);
                }
                else
                {
                    result.TotalSent += queued;
                    AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail,
                        $"ClientMail sent group of {queued} (running total {result.TotalSent}).");
                }
            }

            return result;
        }

        public BatchSendResult SendTest(
            string testTo,
            ClientMailRecipient sample,
            string subject,
            string headerHtml,
            string bodyHtml,
            string footerHtml,
            bool includeGreeting)
        {
            var result = new BatchSendResult();
            if (string.IsNullOrWhiteSpace(testTo))
            {
                result.ErrorMessage = "Test address is required.";
                return result;
            }

            var contact = sample ?? new ClientMailRecipient
            {
                CompanyName = "Test Company",
                ContactFirstName = "Coffee",
                ContactLastName = "Lover"
            };

            string html = BuildBody(contact, headerHtml, bodyHtml, footerHtml, includeGreeting);
            html = "<p><i>Test mail — would normally go to "
                 + System.Web.HttpUtility.HtmlEncode(sample?.ToEmail ?? "(no recipient yet)")
                 + " / " + System.Web.HttpUtility.HtmlEncode(contact.CompanyName ?? "")
                 + "</i></p>" + html;

            var email = new EmailMailKitCls { IncludeConfiguredCc = false };
            if (!email.AddToBatch("[TEST] " + subject, html, null, testTo.Trim()) || !email.SendEmailBatch())
            {
                result.TotalFailed = 1;
                result.ErrorMessage = email.LastErrorSummary;
                return result;
            }

            result.IsSuccess = true;
            result.TotalSent = 1;
            return result;
        }

        private static string BuildGreeting(ClientMailRecipient contact)
        {
            string name = contact?.DisplayName;
            if (!string.IsNullOrWhiteSpace(name))
                return string.Format(MessageProvider.Get(MessageKeys.ClientMail.GreetingWithName), name);
            return MessageProvider.Get(MessageKeys.ClientMail.GreetingGeneric);
        }

        private static string ReplacePlaceholders(string html, ClientMailRecipient contact)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            string first = contact?.ContactFirstName ?? string.Empty;
            string last = contact?.ContactLastName ?? string.Empty;
            string company = contact?.CompanyName ?? string.Empty;
            string greeting = BuildGreeting(contact);

            return html
                .Replace("[#FIRSTNAME#]", first)
                .Replace("[#LASTNAME#]", last)
                .Replace("[#COMPANY#]", company)
                .Replace("[#GREETING#]", greeting);
        }

        private static void AppendSection(StringBuilder builder, string content)
        {
            if (builder == null || string.IsNullOrWhiteSpace(content))
                return;
            builder.Append(content.Trim());
        }

        private static string AppendError(string existing, string add)
        {
            if (string.IsNullOrWhiteSpace(add))
                return existing;
            if (string.IsNullOrWhiteSpace(existing))
                return add;
            return existing + "; " + add;
        }
    }
}
