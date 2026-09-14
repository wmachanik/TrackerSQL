using System;
using System.Collections.Generic;
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
            email.AddFormatAndNewLineToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationIntro), contactName);

            email.AddFormatAndNewLineToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationHeader));

            AppendOrderItemsToEmailBody(email, orderLines, notes);

            AppendDeliveryExplanation(email, contactName, header);

            email.AddStrAndNewLineToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationFooter));

            email.AddFormatToBody(MessageProvider.Get(MessageKeys.Order.EmailFooter));
            email.AddToBody(MessageProvider.Get(MessageProvider.GetEmailSignature()));

            bool success = email.SendEmail();
            if (success)
                TryWriteExpectedDeliveryToWoo(header);

            statusMessage = success
                ? $"Email sent to {contactName}"
                : $"Error sending email: {email.myResults.sResult}";
            return success;
        }

        private void AppendDeliveryExplanation(EmailMailKitCls email, string contactName, OrderHeaderData header)
        {
            if (header == null)
                return;

            string dateText = header.RequiredByDate.ToString("dd MMM, ddd, yyyy");
            int? areaId = ResolveContactAreaId(header.CustomerID);
            DateTime now = TimeZoneUtils.Now();
            bool orderToday = header.OrderDate.Date == now.Date;
            bool soonerException = orderToday
                && _promiseManager.IsSoonerThanPromiseException(areaId, header.OrderDate, header.RequiredByDate, now);

            if (soonerException)
            {
                email.AddFormatAndNewLineToBody(
                    MessageProvider.Get(MessageKeys.Order.ConfirmationDeliverySoonerException),
                    dateText);
                return;
            }

            if (orderToday)
            {
                string statusKey = GetStatusKeyFromDeliveryPersonId(header.ToBeDeliveredBy);
                string statusText = string.IsNullOrEmpty(statusKey)
                    ? MessageProvider.Get(MessageKeys.Order.StatusPendingDelivery)
                    : MessageProvider.Get(statusKey);
                email.AddFormatAndNewLineToBody(
                    MessageProvider.Get(MessageKeys.Order.ConfirmationDeliveryStandard),
                    statusText,
                    dateText);
                return;
            }

            // Existing orders (not placed today): keep prior wording.
            string legacyStatusKey = GetStatusKeyFromDeliveryPersonId(header.ToBeDeliveredBy);
            if (!string.IsNullOrEmpty(legacyStatusKey))
            {
                string statusText = MessageProvider.Get(legacyStatusKey);
                string preDeliveryFormat = MessageProvider.Get(MessageKeys.Order.StatusPreDeliveryBody);
                if (!string.IsNullOrEmpty(preDeliveryFormat))
                {
                    email.AddFormatToBody(preDeliveryFormat, contactName, statusText, dateText);
                    return;
                }
            }

            email.AddFormatAndNewLineToBody(
                MessageProvider.Get(MessageKeys.Order.ConfirmationDeliveryDate),
                dateText);
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

        private void AppendOrderItemsToEmailBody(EmailMailKitCls email, List<OrderLineData> orderLines, string notes)
        {
            email.AddToBody("<ul>");
            foreach (var line in orderLines)
            {
                int sortOrder = _itemsRepository.GetItemSortOrder(line.ItemID);

                if (sortOrder == SystemConstants.ItemConstants.NotesSortOrder)
                {
                    string cleanedNotes = EmailUtils.CleanNoteText(notes);
                    email.AddFormatToBody("<li>{0}</li>", cleanedNotes);
                }
                else
                {
                    if (string.IsNullOrEmpty(line.PackagingName) || line.PackagingID == 0)
                        email.AddFormatToBody(MessageProvider.Get(MessageKeys.Order.ItemFormatBasic), line.ItemName, line.Qty);
                    else
                        email.AddFormatToBody(MessageProvider.Get(MessageKeys.Order.ItemFormatWithPrep), line.ItemName, line.Qty, line.PackagingName);
                }
            }
            email.AddToBody("</ul>");
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
