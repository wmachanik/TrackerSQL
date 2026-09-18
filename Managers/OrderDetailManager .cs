using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class OrderDetailManager
    {
        private readonly ItemsRepository _itemsRepository;
        private readonly ContactsRepository _contactsRepository = new ContactsRepository();
        private readonly DeliveryPromiseManager _promiseManager = new DeliveryPromiseManager();
        private readonly WooCommerceSettingsManager _wooSettings = new WooCommerceSettingsManager();
        private readonly WooOrderInfoRepository _wooOrderInfo = new WooOrderInfoRepository();
        private readonly WooCommerceApiClient _wooApi = new WooCommerceApiClient();

        public OrderDetailManager()
        {
            _itemsRepository = new ItemsRepository();
        }

        public bool SendOrderConfirmation(ContactEmailDetails contact, OrderHeaderData header, List<OrderLineData> orderLines,
            string notes, out string statusMessage)
        {
            string recipientEmail = !string.IsNullOrWhiteSpace(contact.EmailAddress)
                ? contact.EmailAddress
                : contact.altEmailAddress;

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                statusMessage = "No email address found.";
                return false;
            }

            var emailSettings = new EmailSettings();
            emailSettings.SetRecipient(recipientEmail);

            var email = new EmailMailKitCls(emailSettings);
            email.AddSysCCFAddress();
            email.SetEmailSubject(MessageProvider.Get(MessageKeys.Order.ConfirmatonSubject));

            string contactName = EmailUtils.GetFriendlyContactName(contact);
            string companyName = !string.IsNullOrWhiteSpace(contact.CompanyName)
                ? contact.CompanyName.Trim()
                : contactName;

            email.AddToBody(MessageProvider.Format(MessageKeys.Order.ConfirmationIntro, contactName));
            email.AddToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationHeader));
            email.AddToBody(BuildConfirmationTableHtml(companyName, header, orderLines, notes));

            string deliveryNote = BuildDeliveryExplanationHtml(header);
            if (!string.IsNullOrWhiteSpace(deliveryNote))
                email.AddToBody(deliveryNote);

            if (!string.IsNullOrWhiteSpace(header?.PurchaseOrder))
            {
                string po = header.PurchaseOrder.Trim();
                if (string.Equals(po, SystemConstants.UIConstants.PORequiredText, StringComparison.OrdinalIgnoreCase))
                    email.AddToBody("<p style=\"margin:8px 0 0 0;\">" + MessageProvider.Get(MessageKeys.Order.ConfirmationPORequired) + "</p>");
                else
                    email.AddToBody("<p style=\"margin:8px 0 0 0;\">"
                        + MessageProvider.Format(MessageKeys.Order.ConfirmationPOReceived, HttpUtility.HtmlEncode(po))
                        + "</p>");
            }

            email.AddToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationFooter));
            email.AddToBody(MessageProvider.Get(MessageKeys.Order.EmailFooter));
            email.AddToBody(MessageProvider.GetEmailSignature());

            bool success = email.SendEmail();
            if (success)
                TryWriteExpectedDeliveryToWoo(header);

            statusMessage = success
                ? $"Email sent to {contactName}"
                : $"Error sending email: {email.myResults.sResult}";
            return success;
        }

        private string BuildConfirmationTableHtml(
            string companyName,
            OrderHeaderData header,
            List<OrderLineData> orderLines,
            string notes)
        {
            var html = new StringBuilder();
            html.Append(MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableStart));
            html.Append(string.Format(
                MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableHeader),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableCompanyContact),
                HttpUtility.HtmlEncode(companyName ?? string.Empty)));
            html.Append("<tbody>");

            string deliveryLabel = MessageProvider.Get(MessageKeys.Order.ConfirmationTableDeliveryLabel);
            string deliveryValue = header != null
                ? header.RequiredByDate.ToString("dd MMM, ddd, yyyy")
                : string.Empty;
            html.Append(string.Format(
                MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowNormal),
                deliveryLabel,
                HttpUtility.HtmlEncode(deliveryValue),
                ""));

            if (header != null && header.OrderID > 0)
            {
                html.Append(string.Format(
                    MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowAlt),
                    MessageProvider.Get(MessageKeys.Order.ConfirmationTableOrderLabel),
                    header.OrderID.ToString(),
                    ""));
            }

            html.Append(string.Format(
                MessageProvider.Get(MessageKeys.CoffeeCheckup.HtmlTableRowColspan),
                MessageProvider.Get(MessageKeys.CoffeeCheckup.TableListOfItems)));

            int itemIndex = 0;
            if (orderLines != null)
            {
                foreach (var line in orderLines)
                {
                    int sortOrder = _itemsRepository.GetItemSortOrder(line.ItemID);
                    string left;
                    string right;

                    if (sortOrder == SystemConstants.ItemConstants.NotesSortOrder)
                    {
                        left = "Notes";
                        right = HttpUtility.HtmlEncode(EmailUtils.CleanNoteText(notes) ?? string.Empty);
                    }
                    else
                    {
                        left = HttpUtility.HtmlEncode(line.ItemName ?? string.Empty);
                        if (string.IsNullOrEmpty(line.PackagingName) || line.PackagingID == 0)
                            right = HttpUtility.HtmlEncode(FormatQty(line.Qty));
                        else
                            right = HttpUtility.HtmlEncode(FormatQty(line.Qty) + " (" + line.PackagingName + ")");
                    }

                    string rowTemplate = itemIndex % 2 == 0
                        ? MessageKeys.CoffeeCheckup.HtmlTableRowNormal
                        : MessageKeys.CoffeeCheckup.HtmlTableRowAlt;
                    html.Append(string.Format(MessageProvider.Get(rowTemplate), left, right, ""));
                    itemIndex++;
                }
            }

            html.Append("</tbody></table>");
            return html.ToString();
        }

        private static string FormatQty(double qty)
        {
            if (Math.Abs(qty - Math.Round(qty)) < 0.0001)
                return ((int)Math.Round(qty)).ToString();
            return qty.ToString("0.##");
        }

        private string BuildDeliveryExplanationHtml(OrderHeaderData header)
        {
            if (header == null)
                return null;

            string dateText = header.RequiredByDate.ToString("dd MMM, ddd, yyyy");
            int? areaId = ResolveContactAreaId(header.CustomerID);
            DateTime now = TimeZoneUtils.Now();
            bool orderToday = header.OrderDate.Date == now.Date;
            bool soonerException = orderToday
                && _promiseManager.IsSoonerThanPromiseException(areaId, header.OrderDate, header.RequiredByDate, now);

            string inner;
            if (soonerException)
            {
                inner = MessageProvider.Format(
                    MessageKeys.Order.ConfirmationDeliverySoonerException,
                    dateText);
            }
            else if (orderToday)
            {
                string statusKey = GetStatusKeyFromDeliveryPersonId(header.ToBeDeliveredBy);
                string statusText = string.IsNullOrEmpty(statusKey)
                    ? MessageProvider.Get(MessageKeys.Order.StatusPendingDelivery)
                    : MessageProvider.Get(statusKey);
                inner = MessageProvider.Format(
                    MessageKeys.Order.ConfirmationDeliveryStandard,
                    statusText,
                    dateText);
            }
            else
            {
                // Avoid StatusPreDeliveryBody here — it repeats "Dear …" after ConfirmationIntro.
                string legacyStatusKey = GetStatusKeyFromDeliveryPersonId(header.ToBeDeliveredBy);
                if (!string.IsNullOrEmpty(legacyStatusKey))
                {
                    string statusText = MessageProvider.Get(legacyStatusKey);
                    inner = MessageProvider.Format(
                        MessageKeys.Order.ConfirmationDeliveryStandard,
                        statusText,
                        dateText);
                }
                else
                {
                    inner = MessageProvider.Format(
                        MessageKeys.Order.ConfirmationDeliveryDate,
                        dateText);
                }
            }

            if (string.IsNullOrWhiteSpace(inner))
                return null;

            // Strip leading <br /> from older templates so spacing stays tight.
            inner = inner.Trim();
            while (inner.StartsWith("<br", StringComparison.OrdinalIgnoreCase))
            {
                int gt = inner.IndexOf('>');
                if (gt < 0)
                    break;
                inner = inner.Substring(gt + 1).TrimStart();
            }

            return "<p style=\"margin:8px 0 0 0;\">" + inner + "</p>";
        }

        private int? ResolveContactAreaId(long customerId)
        {
            if (customerId <= 0 || customerId > int.MaxValue)
                return null;
            Contact contact = _contactsRepository.GetById((int)customerId);
            return contact?.AreaID;
        }

        private void TryWriteExpectedDeliveryToWoo(OrderHeaderData header)
        {
            try
            {
                _wooSettings.EnsureSchemaOnce();
                var settings = _wooSettings.GetSettings();
                if (settings == null || !settings.WriteExpectedDeliveryToWoo)
                    return;
                if (header == null || header.OrderID <= 0)
                    return;

                WooOrderInfo link = _wooOrderInfo.GetByTrackerOrderId(header.OrderID);
                if (link == null || link.WooOrderId <= 0)
                    return;

                if (!_wooSettings.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out _))
                    return;

                string note = "Tracker expected delivery/dispatch: "
                    + header.RequiredByDate.ToString("yyyy-MM-dd");
                _wooApi.AddOrderNote(creds, link.WooOrderId, note, customerNote: false, out _);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("woo", "WriteExpectedDeliveryToWoo failed: " + ex.Message);
            }
        }

        private string GetStatusKeyFromDeliveryPersonId(int deliveryPersonId)
        {
            switch (deliveryPersonId)
            {
                case SystemConstants.DeliveryConstants.CourierDeliveryID:
                case SystemConstants.DeliveryConstants.ParcelDispatchID:
                    return MessageKeys.Order.StatusDispatched;
                case SystemConstants.DeliveryConstants.CollectionID:
                    return MessageKeys.Order.StatusReadyForCollection;
                default:
                    return MessageKeys.Order.StatusPendingDelivery;
            }
        }
    }

    public class OrderLineData
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public double Qty { get; set; }
        public int PackagingID { get; set; }
        public string PackagingName { get; set; }
    }
}
