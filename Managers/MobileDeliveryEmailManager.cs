using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Emails sent when a driver syncs a delivery: the client's proof of delivery (see <see cref="ProofOfDeliveryManager"/>)
    /// and the driver's note to the office. Each returns null when sent, otherwise why not.
    /// </summary>
    public class MobileDeliveryEmailManager
    {
        private readonly MobileApiRepository _repo = new MobileApiRepository();

        /// <summary>The POD PDF to the client, copied to the orders address.</summary>
        public string SendConfirmation(long proofId, string userName)
        {
            return new ProofOfDeliveryManager().Send(proofId, userName, null, out _);
        }

        public string SendNoteToOffice(MobileApiRepository.OrderBrief order, MobileDeliveryProof proof, string driverName)
        {
            var appSettings = MobileApiSettingsManager.Current;
            if (!appSettings.AllowNotesToOffice)
                return "notes to the office are turned off";

            string office = MobileApiSettingsManager.OfficeEmail(appSettings);
            if (string.IsNullOrWhiteSpace(office))
                return "no office email address is set";

            var first = GetOrderRows(order).FirstOrDefault();
            string company = order.ContactID == SystemConstants.CustomerConstants.SundryCustomerID && SundryNotes.Name(first?.Notes).Length > 0
                ? SundryNotes.Name(first?.Notes) + " (walk-in)"
                : string.IsNullOrWhiteSpace(first?.CompanyName) ? "Order " + order.OrderID : first.CompanyName.Trim();
            bool delivered = proof.Outcome == MobileDeliveryManager.OutcomeDelivered;

            var body = new StringBuilder();
            body.AppendFormat("<p><strong>{0}</strong> sent a note from the driver app:</p>", Html(driverName ?? proof.UserName ?? "The driver"));
            body.AppendFormat("<blockquote style=\"border-left:3px solid #ccc;margin:0 0 1em;padding-left:10px\">{0}</blockquote>",
                Html(proof.DriverNote).Replace("\n", "<br />"));
            body.Append("<table cellpadding=\"4\" style=\"border-collapse:collapse\">");
            AppendRow(body, "Client", company);
            AppendRow(body, "Order", order.OrderID.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrWhiteSpace(first?.PurchaseOrder))
                AppendRow(body, "PO", first.PurchaseOrder);
            AppendRow(body, delivered ? "Delivered" : "Not delivered",
                (proof.Reason ?? string.Empty) + " (" + proof.DeliveredAt.ToString("d MMM yyyy HH:mm", CultureInfo.InvariantCulture) + ")");
            if (!string.IsNullOrWhiteSpace(proof.ReceivedByName))
                AppendRow(body, "Received by", proof.ReceivedByName);
            if (!string.IsNullOrWhiteSpace(proof.MissingItems))
                AppendRow(body, "Not handed over", proof.MissingItems);
            body.Append("</table>");

            var settings = new EmailSettings();
            settings.SetRecipient(office);
            var email = new EmailMailKitCls(settings) { IncludeConfiguredCc = false };
            email.SetEmailSubject("Driver note: " + company + " (order " + order.OrderID + ")");
            email.AddToBody(body.ToString());

            if (!email.SendEmail())
                return SendFailed(email);

            AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                "Driver note for order " + order.OrderID + " sent to " + office, proof.UserName);
            return null;
        }

        private List<MobileApiRepository.SheetRow> GetOrderRows(MobileApiRepository.OrderBrief order)
        {
            if (!order.RequiredByDate.HasValue)
                return new List<MobileApiRepository.SheetRow>();
            return _repo.GetSheetRows(order.RequiredByDate.Value.Date, null, order.OrderID);
        }

        private static string SendFailed(EmailMailKitCls email)
        {
            return string.IsNullOrWhiteSpace(email.LastErrorSummary)
                ? "the email could not be sent"
                : "the email could not be sent (" + email.LastErrorSummary + ")";
        }

        private static void AppendRow(StringBuilder body, string label, string value)
        {
            body.AppendFormat("<tr><td style=\"color:#666;padding-right:12px\">{0}</td><td>{1}</td></tr>", Html(label), Html(value));
        }

        private static string Html(string s) => HttpUtility.HtmlEncode((s ?? string.Empty).Trim());
    }
}
