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
    /// Emails sent when a driver syncs a delivery: the client's delivery confirmation (who received it,
    /// signature attached) and the driver's note to the office. Each returns null when sent, otherwise why not.
    /// </summary>
    public class MobileDeliveryEmailManager
    {
        private readonly MobileApiRepository _repo = new MobileApiRepository();

        public string SendConfirmation(MobileApiRepository.OrderBrief order, MobileDeliveryProof proof,
            byte[] signature, string signatureType, List<MobileDeliveredLine> lines)
        {
            if (order.ContactID <= 0)
                return "the order has no client";

            var customer = new ContactsRepository().GetById(order.ContactID);
            if (customer == null)
                return "the client was not found";

            string recipient = !string.IsNullOrWhiteSpace(customer.EmailAddress) ? customer.EmailAddress : customer.AltEmailAddress;
            var rows = GetOrderRows(order);
            var first = rows.FirstOrDefault();

            // ZZName / sundry: the walk-in's email lives in the order notes as [#address#].
            if (order.ContactID == SystemConstants.CustomerConstants.SundryCustomerID)
                recipient = new OrderManager().ExtractEmailFromNotes(first?.Notes);

            if (string.IsNullOrWhiteSpace(recipient))
                return "the client has no email address";

            string name = order.ContactID == SystemConstants.CustomerConstants.SundryCustomerID
                ? null
                : (!string.IsNullOrWhiteSpace(customer.ContactFirstName) ? customer.ContactFirstName.Trim() : null);

            var body = new StringBuilder();
            body.AppendFormat("<p>Hi {0},</p>", Html(name ?? "there"));
            body.AppendFormat("<p>Your delivery{0} was received on {1} at {2}.</p>",
                string.IsNullOrWhiteSpace(first?.PurchaseOrder) ? string.Empty : " for PO " + Html(first.PurchaseOrder.Trim()),
                proof.DeliveredAt.ToString("dddd d MMMM yyyy", CultureInfo.InvariantCulture),
                proof.DeliveredAt.ToString("HH:mm", CultureInfo.InvariantCulture));

            body.Append("<table cellpadding=\"4\" style=\"border-collapse:collapse\">");
            if (!string.IsNullOrWhiteSpace(proof.ReceivedByName))
                AppendRow(body, "Received by", proof.ReceivedByName);
            if (!string.IsNullOrWhiteSpace(proof.Reason))
                AppendRow(body, "Delivered", proof.Reason);
            AppendRow(body, "Order", order.OrderID.ToString(CultureInfo.InvariantCulture));
            body.Append("</table>");

            var handedOver = HandedOver(rows, lines);
            if (handedOver.Count > 0)
            {
                body.Append("<p><strong>Delivered</strong></p><ul>");
                foreach (string line in handedOver)
                    body.AppendFormat("<li>{0}</li>", Html(line));
                body.Append("</ul>");
            }
            if (!string.IsNullOrWhiteSpace(proof.MissingItems))
                body.AppendFormat("<p><strong>Still to follow:</strong> {0}</p>", Html(proof.MissingItems));

            bool attach = signature != null && signature.Length > 0;
            if (attach)
                body.Append("<p>The signature is attached.</p>");
            body.Append(MessageProvider.GetEmailSignature());

            var settings = new EmailSettings();
            settings.SetRecipient(recipient.Trim());
            var email = new EmailMailKitCls(settings);
            email.AddSysCCFAddress();
            email.SetEmailSubject("Delivery confirmation" + (string.IsNullOrWhiteSpace(first?.CompanyName) ? string.Empty : " - " + first.CompanyName.Trim()));
            email.AddToBody(body.ToString());
            if (attach)
                email.AddAttachment("Signature-Order" + order.OrderID + Extension(signatureType), signature, signatureType);

            if (!email.SendEmail())
                return SendFailed(email);

            AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                "Delivery confirmation for order " + order.OrderID + " sent to " + recipient, proof.UserName);
            return null;
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
            string company = string.IsNullOrWhiteSpace(first?.CompanyName) ? "Order " + order.OrderID : first.CompanyName.Trim();
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

        /// <summary>"2 × Kenya AA (1kg)" per line actually handed over; repair lines are left out.</summary>
        private static List<string> HandedOver(List<MobileApiRepository.SheetRow> rows, List<MobileDeliveredLine> lines)
        {
            var sent = (lines ?? new List<MobileDeliveredLine>())
                .GroupBy(l => l.LineId)
                .ToDictionary(g => g.Key, g => g.Last().Qty);

            var result = new List<string>();
            foreach (var row in rows)
            {
                if (row.ItemServiceTypeID == SystemConstants.ServiceTypeConstants.Service
                    || row.ItemID == SystemConstants.ItemConstants.RepairCheckItemID)
                    continue;

                double qty = sent.TryGetValue(row.OrderLineID, out double given) ? Math.Min(given, row.Qty) : row.Qty;
                qty = Math.Round(qty, SystemConstants.DatabaseConstants.NumDecimalPoints);
                if (qty <= 0)
                    continue;

                result.Add(qty.ToString("0.###", CultureInfo.InvariantCulture) + " × " + (row.ItemDesc ?? "Item").Trim()
                    + (string.IsNullOrWhiteSpace(row.PackDesc) ? string.Empty : " (" + row.PackDesc.Trim() + ")"));
            }
            return result;
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

        private static string Extension(string contentType)
        {
            switch ((contentType ?? string.Empty).ToLowerInvariant())
            {
                case "image/jpeg": return ".jpg";
                case "image/webp": return ".webp";
                default: return ".png";
            }
        }

        private static string Html(string s) => HttpUtility.HtmlEncode((s ?? string.Empty).Trim());
    }
}
