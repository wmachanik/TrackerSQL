using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// The proof of delivery (POD) for a delivery captured on the driver app: a PDF with the client, the order, what was handed
    /// over, who received it, the terms they accepted and their signature, and the email that sends it to the client
    /// (after a delivery syncs when System → Driver App says so, or from Contact Details → Proof of Delivery).
    /// </summary>
    public class ProofOfDeliveryManager
    {
        private const string LogoPath = "~/images/logo/QuaffeeLogoSmallest.jpg";
        private const float Left = 40f;
        private const float Right = SimplePdf.PageWidth - 40f;
        private const float ContentWidth = Right - Left;
        private const float Bottom = SimplePdf.PageHeight - 60f;

        private readonly MobileApiRepository _repo = new MobileApiRepository();

        public class PodDocument
        {
            public MobileDeliveryProof Proof;
            public bool WalkIn;
            public string Client;
            /// <summary>Billing account when <see cref="OnBehalfOf"/> is set (delivery on behalf).</summary>
            public string OnBehalfOf;
            /// <summary>First name for "Hi …" (none for walk-ins).</summary>
            public string Greeting;
            public string Address;
            public string Email;
            public string PurchaseOrder;
            public string Driver;
            public List<string> Items = new List<string>();
            /// <summary>The proof predates the stored hand-over list and something was short: the items are the order, not what was delivered.</summary>
            public bool ItemsAreOrdered;
            public byte[] Signature;
            public bool ShowLocation;
            public string TermsText;
            public string TermsUrl;
        }

        /// <summary>Everything the POD shows; null (with the reason) for a missing proof or a delivery that was not made.</summary>
        public PodDocument Load(long proofId, out string error)
        {
            return Load(proofId, out error, true);
        }

        /// <summary>Loads a delivered record for a confirmation email without requiring a signature.</summary>
        public PodDocument LoadForDeliveryConfirmation(long proofId, out string error)
        {
            return Load(proofId, out error, false);
        }

        private PodDocument Load(long proofId, out string error, bool requireSignature)
        {
            error = null;
            MobileApiSchemaInstaller.EnsureReady();
            MobileDeliveryProof proof = _repo.GetProof(proofId);
            if (proof == null)
            {
                error = "Proof " + proofId + " was not found.";
                return null;
            }
            if (!string.Equals(proof.Outcome, MobileDeliveryManager.OutcomeDelivered, StringComparison.OrdinalIgnoreCase))
            {
                error = "That delivery was not made, so there is no proof of delivery.";
                return null;
            }
            if (requireSignature && !proof.HasSignature)
            {
                error = "That delivery was not signed, so there is no proof of delivery.";
                return null;
            }

            MobileApiSettings settings = MobileApiSettingsManager.Current;
            var doc = new PodDocument
            {
                Proof = proof,
                TermsText = proof.TermsText ?? MobileApiSettingsManager.DeliveryTerms(settings),
                TermsUrl = MobileApiSettingsManager.TermsUrl(settings)
            };

            MobileApiRepository.OrderBrief order = _repo.GetOrderBrief(proof.OrderID);
            List<MobileApiRepository.SheetRow> rows = order?.RequiredByDate != null
                ? _repo.GetSheetRows(order.RequiredByDate.Value.Date, null, order.OrderID)
                : new List<MobileApiRepository.SheetRow>();
            MobileApiRepository.SheetRow first = rows.FirstOrDefault();
            int contactId = proof.ContactID ?? order?.ContactID ?? 0;

            doc.WalkIn = contactId == SystemConstants.CustomerConstants.SundryCustomerID;
            if (doc.WalkIn)
            {
                string notes = first?.Notes;
                doc.Client = SundryNotes.Name(notes);
                if (doc.Client.Length == 0)
                    doc.Client = "Walk-in client";
                doc.Address = NullIfEmpty(SundryNotes.Details(notes));
                doc.Email = NullIfEmpty(SundryNotes.Email(notes));
            }
            else
            {
                Contact customer = contactId > 0 ? new ContactsRepository().GetById(contactId) : null;
                string accountCompany = NullIfEmpty(first?.CompanyName) ?? NullIfEmpty(customer?.CompanyName) ?? NullIfEmpty(proof.CompanyName);
                ApplyDeliverToClient(doc, first, accountCompany);
                doc.Greeting = NullIfEmpty(customer?.ContactFirstName);
                doc.Address = FormatPostalAddress(
                    NullIfEmpty(first?.Address) ?? NullIfEmpty(customer?.BillingAddress),
                    NullIfEmpty(first?.PostalCode) ?? NullIfEmpty(customer?.PostalCode));
                doc.Email = NullIfEmpty(customer?.EmailAddress) ?? NullIfEmpty(customer?.AltEmailAddress);
            }
            doc.PurchaseOrder = NullIfEmpty(first?.PurchaseOrder);

            string driver = proof.PersonID.HasValue ? new PersonsRepository().GetPersonNameById(proof.PersonID.Value) : null;
            doc.Driver = NullIfEmpty(driver) ?? NullIfEmpty(proof.UserName);

            if (proof.HandedOver != null)
                doc.Items = proof.HandedOver.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
            else
            {
                doc.Items = HandedOverLines(rows, null);
                doc.ItemsAreOrdered = proof.MissingItems != null;
            }

            if (_repo.TryGetSignature(proofId, out byte[] signature, out _))
                doc.Signature = signature;

            doc.ShowLocation = ShouldShowLocation(proof, contactId, first);
            return doc;
        }

        /// <summary>Same titles as the delivery sheet and driver app: deliver-to site, optional "on behalf of" account.</summary>
        private static void ApplyDeliverToClient(PodDocument doc, MobileApiRepository.SheetRow sheetRow, string accountCompany)
        {
            string deliverTo = DeliveryOnBehalfManager.DeliverToDisplayName(sheetRow?.DeliverToLabel, sheetRow?.DeliverToPerson);
            bool hasDeliverTo = !string.IsNullOrWhiteSpace(deliverTo);
            if (hasDeliverTo)
            {
                doc.Client = DeliveryOnBehalfManager.ListDisplayName(deliverTo, accountCompany);
                doc.OnBehalfOf = accountCompany;
            }
            else
                doc.Client = accountCompany ?? "Client";
        }

        private static string FormatPostalAddress(string address, string postal)
        {
            if (address == null)
                return postal;
            if (postal == null || address.Contains(postal))
                return address;
            return address + ", " + postal;
        }

        /// <summary>"2 × Kenya AA (1kg)" per order line handed over (all of it unless <paramref name="lines"/> says less); repair lines are left out.</summary>
        public static List<string> HandedOverLines(List<MobileApiRepository.SheetRow> rows, List<MobileDeliveredLine> lines)
        {
            var sent = (lines ?? new List<MobileDeliveredLine>())
                .GroupBy(l => l.LineId)
                .ToDictionary(g => g.Key, g => g.Last().Qty);

            var result = new List<string>();
            foreach (var row in rows ?? new List<MobileApiRepository.SheetRow>())
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

        public static string FileName(PodDocument doc)
        {
            return "POD-Order" + doc.Proof.OrderID.ToString(CultureInfo.InvariantCulture) + ".pdf";
        }

        public byte[] BuildPdf(PodDocument doc)
        {
            MobileDeliveryProof proof = doc.Proof;
            var pdf = new SimplePdf();
            const int ink = 0x222222;
            const int muted = 0x6F6F6F;
            const int rule = 0xD9D4CC;
            const int accent = 0x5A3E2B;

            string logoFile = HostingEnvironment.MapPath(LogoPath);
            if (logoFile != null && File.Exists(logoFile))
            {
                byte[] logo = SimplePdf.ToJpeg(File.ReadAllBytes(logoFile), 600, out int w, out int h);
                if (logo != null)
                {
                    const float logoHeight = 54f;
                    pdf.Image(pdf.AddJpeg(logo, w, h), Left, 34f, logoHeight * w / h, logoHeight);
                }
            }
            pdf.TextRight(Right, 58f, "Proof of Delivery", 18f, true, accent);
            pdf.TextRight(Right, 76f, "Order " + proof.OrderID.ToString(CultureInfo.InvariantCulture)
                + "    POD " + proof.ProofID.ToString(CultureInfo.InvariantCulture), 10f, false, muted);
            pdf.Line(Left, 100f, Right, 100f, 1.2f, accent);

            float y = 122f;
            Action<float> ensure = height =>
            {
                if (y + height <= Bottom)
                    return;
                pdf.NewPage();
                y = 50f;
            };
            Action<string, string, bool> row = (label, value, bold) =>
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                List<string> lines = SimplePdf.Wrap(value.Trim(), 10.5f, bold, ContentWidth - 110f);
                ensure(lines.Count * 14f + 4f);
                pdf.Text(Left, y, label, 9f, false, muted);
                foreach (string line in lines)
                {
                    pdf.Text(Left + 110f, y, line, 10.5f, bold, ink);
                    y += 14f;
                }
                y += 4f;
            };

            row("Delivered to", doc.Client, true);
            if (!string.IsNullOrWhiteSpace(doc.OnBehalfOf))
                row("On behalf of", doc.OnBehalfOf, false);
            row(doc.WalkIn ? "Details" : "Address", doc.Address, false);
            row("Order", proof.OrderID.ToString(CultureInfo.InvariantCulture), false);
            row("Your PO", doc.PurchaseOrder, false);
            row("Delivered", proof.DeliveredAt.ToString("dddd d MMMM yyyy 'at' HH:mm", CultureInfo.InvariantCulture), false);
            row("How", (proof.Reason ?? string.Empty) + (proof.NotAtClient ? " (not at the client's address)" : string.Empty), false);
            row("Received by", string.IsNullOrWhiteSpace(proof.ReceivedByName) ? "(no name given)" : proof.ReceivedByName, true);
            row("Driver", doc.Driver, false);
            if (doc.ShowLocation && proof.Latitude.HasValue && proof.Longitude.HasValue)
                row("Location", proof.Latitude.Value.ToString("0.00000", CultureInfo.InvariantCulture) + ", "
                    + proof.Longitude.Value.ToString("0.00000", CultureInfo.InvariantCulture)
                    + (proof.AccuracyM.HasValue ? "  (within " + proof.AccuracyM.Value.ToString("0", CultureInfo.InvariantCulture) + " m)" : string.Empty)
                    + "  where the driver's phone was when it was signed", false);

            y += 10f;
            ensure(40f);
            pdf.FillRect(Left, y, ContentWidth, 18f, 0xEFEBE5);
            pdf.Text(Left + 6f, y + 12.5f, "Qty", 9f, true, ink);
            pdf.Text(Left + 60f, y + 12.5f, doc.ItemsAreOrdered ? "Ordered" : "Delivered", 9f, true, ink);
            y += 18f;
            if (doc.Items.Count == 0)
            {
                pdf.Text(Left + 60f, y + 13f, "No items listed", 10f, false, muted);
                y += 20f;
            }
            foreach (string item in doc.Items)
            {
                int split = item.IndexOf(" × ", StringComparison.Ordinal);
                string qty = split > 0 ? item.Substring(0, split) : string.Empty;
                string text = split > 0 ? item.Substring(split + 3) : item;
                List<string> lines = SimplePdf.Wrap(text, 10f, false, ContentWidth - 66f);
                ensure(lines.Count * 13f + 6f);
                pdf.Text(Left + 6f, y + 13f, qty, 10f, false, ink);
                float lineY = y + 13f;
                foreach (string line in lines)
                {
                    pdf.Text(Left + 60f, lineY, line, 10f, false, ink);
                    lineY += 13f;
                }
                y = lineY - 13f + 7f;
                pdf.Line(Left, y, Right, y, 0.4f, rule);
            }
            if (!string.IsNullOrWhiteSpace(proof.MissingItems))
            {
                y += 8f;
                List<string> lines = SimplePdf.Wrap("Still to follow: " + proof.MissingItems.Trim(), 10f, false, ContentWidth);
                ensure(lines.Count * 13f + 6f);
                foreach (string line in lines)
                {
                    y += 13f;
                    pdf.Text(Left, y, line, 10f, false, 0x9A5B00);
                }
            }

            y += 20f;
            if (proof.TermsAccepted.HasValue)
            {
                List<string> lines = SimplePdf.Wrap(doc.TermsText, 9.5f, false, ContentWidth - 44f);
                float boxHeight = 16f + lines.Count * 12.5f + 34f;
                ensure(boxHeight);
                pdf.StrokeRect(Left, y, ContentWidth, boxHeight, 0.6f, 0xBBB3A8);
                pdf.StrokeRect(Left + 12f, y + 11f, 11f, 11f, 0.9f, ink);
                if (proof.TermsAccepted.Value)
                {
                    pdf.Line(Left + 14.2f, y + 16.8f, Left + 16.9f, y + 19.8f, 1.6f, 0x2E7D32);
                    pdf.Line(Left + 16.9f, y + 19.8f, Left + 21.6f, y + 12.6f, 1.6f, 0x2E7D32);
                }
                float lineY = y + 20f;
                foreach (string line in lines)
                {
                    pdf.Text(Left + 32f, lineY, line, 9.5f, false, ink);
                    lineY += 12.5f;
                }
                lineY += 4f;
                pdf.Text(Left + 32f, lineY, proof.TermsAccepted.Value
                    ? "Ticked by " + (string.IsNullOrWhiteSpace(proof.ReceivedByName) ? "the person receiving the delivery" : proof.ReceivedByName.Trim())
                      + " on the driver's phone before signing."
                    : "The terms box was not ticked.", 9f, true, proof.TermsAccepted.Value ? 0x2E7D32 : 0xB00020);
                pdf.Text(Left + 32f, lineY + 13f, "Full terms and conditions: " + doc.TermsUrl, 8.5f, false, muted);
                y += boxHeight;
            }
            else
            {
                List<string> lines = SimplePdf.Wrap("This delivery is subject to our standard terms and conditions: " + doc.TermsUrl, 9.5f, false, ContentWidth);
                ensure(lines.Count * 12.5f + 4f);
                foreach (string line in lines)
                {
                    y += 12.5f;
                    pdf.Text(Left, y, line, 9.5f, false, ink);
                }
            }

            y += 22f;
            ensure(150f);
            pdf.Text(Left, y, "Signature", 9f, false, muted);
            y += 8f;
            const float boxWidth = 250f, boxHeightSig = 100f;
            pdf.StrokeRect(Left, y, boxWidth, boxHeightSig, 0.6f, 0xBBB3A8);
            byte[] jpeg = SimplePdf.ToJpeg(doc.Signature, 900, out int sw, out int sh);
            if (jpeg != null)
            {
                float scale = Math.Min((boxWidth - 10f) / sw, (boxHeightSig - 10f) / sh);
                float drawW = sw * scale, drawH = sh * scale;
                pdf.Image(pdf.AddJpeg(jpeg, sw, sh), Left + (boxWidth - drawW) / 2f, y + (boxHeightSig - drawH) / 2f, drawW, drawH);
            }
            else
            {
                string none = doc.Signature == null ? "No signature captured" : "Signature on file in Tracker";
                pdf.Text(Left + (boxWidth - SimplePdf.Width(none, 10f)) / 2f, y + boxHeightSig / 2f + 4f, none, 10f, false, muted);
            }
            float sideX = Left + boxWidth + 20f;
            pdf.Text(sideX, y + 14f, string.IsNullOrWhiteSpace(proof.ReceivedByName) ? "Received" : proof.ReceivedByName.Trim(), 10.5f, true, ink);
            pdf.Text(sideX, y + 30f, proof.DeliveredAt.ToString("d MMMM yyyy, HH:mm", CultureInfo.InvariantCulture), 10f, false, ink);
            pdf.Text(sideX, y + 46f, "Captured on the Quaffee Driver app", 9f, false, muted);
            pdf.Text(sideX, y + 60f, "Received by Tracker " + proof.ReceivedAt.ToString("d MMM yyyy HH:mm", CultureInfo.InvariantCulture), 9f, false, muted);

            string generated = TimeZoneUtils.Now().ToString("d MMM yyyy HH:mm", CultureInfo.InvariantCulture);
            int pages = pdf.PageCount;
            for (int i = 0; i < pages; i++)
            {
                pdf.SelectPage(i);
                const float footerY = SimplePdf.PageHeight - 34f;
                pdf.Line(Left, footerY - 12f, Right, footerY - 12f, 0.4f, rule);
                pdf.Text(Left, footerY, "Quaffee  |  Proof of delivery for order " + proof.OrderID.ToString(CultureInfo.InvariantCulture)
                    + "  |  printed " + generated, 8f, false, muted);
                if (pages > 1)
                    pdf.TextRight(Right, footerY, "Page " + (i + 1) + " of " + pages, 8f, false, muted);
            }

            return pdf.Save("Proof of delivery - order " + proof.OrderID.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Emails the POD PDF to the client (or <paramref name="toOverride"/>), copied to the orders address.
        /// Returns null when sent (<paramref name="sentTo"/> says where), otherwise why not.
        /// </summary>
        public string Send(long proofId, string sentBy, string toOverride, out string sentTo)
        {
            sentTo = null;
            PodDocument doc = Load(proofId, out string error);
            if (doc == null)
                return error;

            string recipient = string.IsNullOrWhiteSpace(toOverride) ? doc.Email : toOverride.Trim();
            if (string.IsNullOrWhiteSpace(recipient))
                return doc.WalkIn
                    ? "the walk-in order has no email in its notes ([#name@example.com#])"
                    : "the client has no email address";
            if (!IsEmail(recipient))
                return "'" + recipient + "' is not a valid email address";

            MobileDeliveryProof proof = doc.Proof;
            byte[] pdf = BuildPdf(doc);

            var body = new StringBuilder();
            body.AppendFormat("<p>Hi {0},</p>", Html(doc.Greeting ?? "there"));
            body.AppendFormat("<p>Please find attached the proof of delivery for order {0}{1}, delivered on {2} at {3}.</p>",
                proof.OrderID.ToString(CultureInfo.InvariantCulture),
                doc.PurchaseOrder == null ? string.Empty : " (PO " + Html(doc.PurchaseOrder) + ")",
                proof.DeliveredAt.ToString("dddd d MMMM yyyy", CultureInfo.InvariantCulture),
                proof.DeliveredAt.ToString("HH:mm", CultureInfo.InvariantCulture));
            body.Append("<table cellpadding=\"4\" style=\"border-collapse:collapse\">");
            if (!string.IsNullOrWhiteSpace(doc.OnBehalfOf))
                AppendRow(body, "On behalf of", doc.OnBehalfOf);
            if (!string.IsNullOrWhiteSpace(doc.Address))
                AppendRow(body, "Address", doc.Address);
            if (!string.IsNullOrWhiteSpace(proof.ReceivedByName))
                AppendRow(body, "Received by", proof.ReceivedByName);
            if (!string.IsNullOrWhiteSpace(proof.Reason))
                AppendRow(body, "Delivered", proof.Reason);
            AppendRow(body, "Order", proof.OrderID.ToString(CultureInfo.InvariantCulture));
            body.Append("</table>");
            if (doc.Items.Count > 0)
            {
                body.AppendFormat("<p><strong>{0}</strong></p><ul>", doc.ItemsAreOrdered ? "Ordered" : "Delivered");
                foreach (string line in doc.Items)
                    body.AppendFormat("<li>{0}</li>", Html(line));
                body.Append("</ul>");
            }
            if (!string.IsNullOrWhiteSpace(proof.MissingItems))
                body.AppendFormat("<p><strong>Still to follow:</strong> {0}</p>", Html(proof.MissingItems));
            body.AppendFormat("<p style=\"color:#666;font-size:smaller\">Deliveries are subject to our standard terms and conditions: <a href=\"{0}\">{1}</a></p>",
                HttpUtility.HtmlAttributeEncode(doc.TermsUrl), Html(doc.TermsUrl));
            body.Append(MessageProvider.GetEmailSignature());

            var settings = new EmailSettings();
            settings.SetRecipient(recipient);
            var email = new EmailMailKitCls(settings);
            email.AddSysCCFAddress();
            email.SetEmailSubject("Proof of delivery - " + doc.Client + " - order " + proof.OrderID.ToString(CultureInfo.InvariantCulture));
            email.AddToBody(body.ToString());
            email.AddAttachment(FileName(doc), pdf, "application/pdf");

            if (!email.SendEmail())
                return string.IsNullOrWhiteSpace(email.LastErrorSummary)
                    ? "the email could not be sent"
                    : "the email could not be sent (" + email.LastErrorSummary + ")";

            sentTo = recipient;
            _repo.MarkPodSent(proofId, recipient);
            AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                "Proof of delivery for order " + proof.OrderID + " (POD " + proofId + ") sent to " + recipient, sentBy);
            return null;
        }

        /// <summary>Emails an unsigned delivered record as a confirmation, without calling it proof of delivery.</summary>
        public string SendDeliveryConfirmation(long proofId, string sentBy, out string sentTo)
        {
            sentTo = null;
            PodDocument doc = LoadForDeliveryConfirmation(proofId, out string error);
            if (doc == null)
                return error;

            string recipient = doc.Email;
            if (string.IsNullOrWhiteSpace(recipient))
                return doc.WalkIn
                    ? "the walk-in order has no email in its notes ([#name@example.com#])"
                    : "the client has no email address";
            if (!IsEmail(recipient))
                return "'" + recipient + "' is not a valid email address";

            MobileDeliveryProof proof = doc.Proof;
            var body = new StringBuilder();
            body.AppendFormat("<p>Hi {0},</p>", Html(doc.Greeting ?? "there"));
            body.AppendFormat("<p>This is a confirmation that order {0}{1} was delivered on {2} at {3}.</p>",
                proof.OrderID.ToString(CultureInfo.InvariantCulture),
                doc.PurchaseOrder == null ? string.Empty : " (PO " + Html(doc.PurchaseOrder) + ")",
                proof.DeliveredAt.ToString("dddd d MMMM yyyy", CultureInfo.InvariantCulture),
                proof.DeliveredAt.ToString("HH:mm", CultureInfo.InvariantCulture));
            body.Append("<table cellpadding=\"4\" style=\"border-collapse:collapse\">");
            if (!string.IsNullOrWhiteSpace(doc.OnBehalfOf))
                AppendRow(body, "On behalf of", doc.OnBehalfOf);
            if (!string.IsNullOrWhiteSpace(doc.Address))
                AppendRow(body, "Address", doc.Address);
            if (!string.IsNullOrWhiteSpace(proof.ReceivedByName))
                AppendRow(body, "Received by", proof.ReceivedByName);
            if (!string.IsNullOrWhiteSpace(proof.Reason))
                AppendRow(body, "Delivered", proof.Reason);
            AppendRow(body, "Order", proof.OrderID.ToString(CultureInfo.InvariantCulture));
            body.Append("</table>");
            if (doc.Items.Count > 0)
            {
                body.Append("<p><strong>Delivered</strong></p><ul>");
                foreach (string line in doc.Items)
                    body.AppendFormat("<li>{0}</li>", Html(line));
                body.Append("</ul>");
            }
            if (!string.IsNullOrWhiteSpace(proof.MissingItems))
                body.AppendFormat("<p><strong>Still to follow:</strong> {0}</p>", Html(proof.MissingItems));
            body.Append(MessageProvider.GetEmailSignature());

            var settings = new EmailSettings();
            settings.SetRecipient(recipient);
            var email = new EmailMailKitCls(settings);
            email.AddSysCCFAddress();
            email.SetEmailSubject("Delivery confirmation - " + doc.Client + " - order " + proof.OrderID.ToString(CultureInfo.InvariantCulture));
            email.AddToBody(body.ToString());

            if (!email.SendEmail())
                return string.IsNullOrWhiteSpace(email.LastErrorSummary)
                    ? "the email could not be sent"
                    : "the email could not be sent (" + email.LastErrorSummary + ")";

            sentTo = recipient;
            AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                "Delivery confirmation for order " + proof.OrderID + " (unsigned proof " + proofId + ") sent to " + recipient, sentBy);
            return null;
        }

        private bool ShouldShowLocation(MobileDeliveryProof proof, int contactId, MobileApiRepository.SheetRow sheetRow)
        {
            if (proof == null || proof.NotAtClient || !proof.Latitude.HasValue || !proof.Longitude.HasValue)
                return false;
            if (proof.AccuracyM.HasValue && proof.AccuracyM.Value > 500m)
                return false;

            if (contactId <= 0 || contactId == SystemConstants.CustomerConstants.SundryCustomerID)
                return true;

            double? expectedLat = null, expectedLng = null;
            if (sheetRow?.DeliverToAddressId is int deliverToId && deliverToId > 0)
            {
                ContactDeliverToAddress deliverTo = new ContactDeliverToAddressRepository().GetById(deliverToId);
                if (deliverTo?.Latitude is decimal dLat && deliverTo.Longitude is decimal dLng)
                {
                    expectedLat = (double)dLat;
                    expectedLng = (double)dLng;
                }
            }

            if (!expectedLat.HasValue)
            {
                CustomerLocation expected = new CustomerLocationRepository().GetByContactId(contactId);
                if (expected == null)
                    return true;
                expectedLat = (double)expected.Latitude;
                expectedLng = (double)expected.Longitude;
            }

            double distanceKm = DistanceKm((double)proof.Latitude.Value, (double)proof.Longitude.Value,
                expectedLat.Value, expectedLng.Value);
            return distanceKm <= 2.0;
        }

        private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0088;
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static bool IsEmail(string value)
        {
            try
            {
                return new MailAddress(value).Address.Equals(value, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static void AppendRow(StringBuilder body, string label, string value)
        {
            body.AppendFormat("<tr><td style=\"color:#666;padding-right:12px\">{0}</td><td>{1}</td></tr>", Html(label), Html(value));
        }

        private static string Html(string s) => HttpUtility.HtmlEncode((s ?? string.Empty).Trim());

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
