using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Logging-only implementation (no DB). Writes structured line via AppLogger.
    /// Swap later with a DB-backed version without changing page code.
    /// </summary>
    public class OrderChangeRequestLoggingService : IOrderChangeRequestService
    {
        private const int MaxLen = 800;          // hard cap
        private const int MinLen = 5;            // reject trivial noise
        private static readonly Regex MultiWhitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);
        private static readonly Regex ControlChars = new Regex(@"[\u0000-\u001F]", RegexOptions.Compiled);

        public ChangeRequestResult Submit(ChangeRequestSubmission submission)
        {
            if (submission == null)
                return Fail("Invalid submission");

            string cleaned = Clean(submission.RequestTextRaw);
            if (cleaned.Length < MinLen)
                return Fail("Too short");
            if (cleaned.Length > MaxLen)
                cleaned = cleaned.Substring(0, MaxLen);

            string refId = GenerateRef();
            string line = BuildLogLine(refId, submission, cleaned);

            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, line);

            // Optional: email notification (config driven)
            if (ConfigHelper.GetBool("RequestChanges.NotifyEmail", true))
            {
                TrySendEmail(refId, submission, cleaned);
            }

            return new ChangeRequestResult
            {
                Success = true,
                Reference = refId,
                Message = $"Change request submitted. Reference: {refId}"
            };
        }

        private string Clean(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            string s = raw.Replace("\r", " ").Replace("\n", " ");
            s = ControlChars.Replace(s, string.Empty);
            s = MultiWhitespace.Replace(s.Trim(), " ");
            return s;
        }

        private string GenerateRef()
        {
            // Timestamp + 4 random hex
            return DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ") + "_" +
                   Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
        }

        private string BuildLogLine(string refId, ChangeRequestSubmission sub, string cleaned)
        {
            var sb = new StringBuilder();
            sb.Append("CRQ|Ref=").Append(refId);
            sb.Append("|Cust=").Append(sub.CustomerID);
            sb.Append("|Deliv=").Append(sub.DeliveryDateUtc.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(sub.TokenRef))
                sb.Append("|TokRef=").Append(sub.TokenRef);
            sb.Append("|Ip=").Append(sub.SourceIp ?? "-");
            sb.Append("|UA=").Append(Truncate(sub.UserAgent, 64));
            sb.Append("|Len=").Append(cleaned.Length);
            sb.Append("|Text=").Append(cleaned);
            return sb.ToString();
        }

        private void TrySendEmail(string refId, ChangeRequestSubmission sub, string cleaned)
        {
            try
            {
                string to = ConfigHelper.GetString("OrdersEmail", SystemConstants.EmailConstants.DefaultAdminEmail);
                var email = new EmailMailKitCls();

                // Set recipient (leave From as configured)
                email.SetEmailFromTo(null, to);

                email.SetEmailSubject($"Change Request {refId} Cust:{sub.CustomerID}");
                email.AddToBody(
                    $"Ref: {HttpUtility.HtmlEncode(refId)}<br/>" +
                    $"CustomerID: {sub.CustomerID}<br/>" +
                    $"DeliveryDate (UTC): {sub.DeliveryDateUtc:yyyy-MM-dd}<br/>" +
                    $"IP: {HttpUtility.HtmlEncode(sub.SourceIp)}<br/>" +
                    $"UA: {HttpUtility.HtmlEncode(Truncate(sub.UserAgent, 128))}<br/><br/>" +
                    $"Request:<br/>{HttpUtility.HtmlEncode(cleaned)}");

                if (!email.SendEmail())
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                        $"CRQ email send failed: {email.LastErrorSummary}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, "CRQ email fail: " + ex.Message);
            }
        }

        private string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private ChangeRequestResult Fail(string msg) =>
            new ChangeRequestResult { Success = false, Reference = null, Message = msg };
    }
}