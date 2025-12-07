using System.Collections.Generic;
using TrackerSQL.Classes;
using TrackerSQL.Controls;

namespace TrackerSQL.Managers
{
    public class OrderDetailManager
    {
        public bool SendOrderConfirmation(ContactEmailDetails contact, OrderHeaderData header,List<OrderLineData> orderLines,
            string notes, out string statusMessage)
        {
            // Build recipient
            string recipientEmail = !string.IsNullOrWhiteSpace(contact.EmailAddress)
                ? contact.EmailAddress
                : contact.altEmailAddress;

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                statusMessage = "No email address found.";
                return false;
            }

            // Build email
            var emailSettings = new EmailSettings();
            emailSettings.SetRecipient(recipientEmail);

            var email = new EmailMailKitCls(emailSettings);
            email.AddSysCCFAddress();
            email.SetEmailSubject(MessageProvider.Get(MessageKeys.Order.ConfirmatonSubject));

            // 1. Greeting
            string contactName = EmailUtils.GetFriendlyContactName(contact);
            email.AddFormatAndNewLineToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationIntro ), contactName);

            // 2. Order header
            email.AddFormatAndNewLineToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationHeader));

            // 3. Order items
            AppendOrderItemsToEmailBody(email, orderLines, notes);

            // 3.5 Pre-delivery status line (use resource string, not the literal key)
            // Use MessageProvider.Get(...) so the resource value is retrieved; avoid passing the key itself.
            string statusKey = GetStatusKeyFromDeliveryPersonId(header?.ToBeDeliveredBy ?? 0);
            bool wrotePreDeliveryLine = false;
            if (!string.IsNullOrEmpty(statusKey))
            {
                string statusText = MessageProvider.Get(statusKey);
                string preDeliveryFormat = MessageProvider.Get(MessageKeys.Order.StatusPreDeliveryBody);
                if (!string.IsNullOrEmpty(preDeliveryFormat))
                {
                    // Use AddFormatToBody (no automatic extra line break) because the resource already contains HTML line breaks.
                    email.AddFormatToBody(preDeliveryFormat, contactName, statusText, header.RequiredByDate.ToString("dd MMM, ddd, yyyy"));
                    wrotePreDeliveryLine = true;
                }
            }

            // 4. Delivery date
            // Avoid duplicating the delivery-date sentence when the pre-delivery body already included the date.
            if (!wrotePreDeliveryLine)
            {
                email.AddFormatAndNewLineToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationDeliveryDate),
                    header.RequiredByDate.ToString("dd MMM, ddd, yyyy"));
            }

            // 5. Footer
            email.AddStrAndNewLineToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationFooter));

            // 6. Signature     
            email.AddFormatToBody(MessageProvider.Get(MessageKeys.Order.EmailFooter));
            email.AddToBody(MessageProvider.Get(MessageProvider.GetEmailSignature()));

            // Send
            bool success = email.SendEmail();
            statusMessage = success
                ? $"Email sent to {contactName}"
                : $"Error sending email: {email.myResults.sResult}";
            return success;
        }

        private void AppendOrderItemsToEmailBody(EmailMailKitCls email, List<OrderLineData> orderLines, string notes)
        {
            email.AddToBody("<ul>");
            foreach (var line in orderLines)
            {
                // If you have a method to get the sort order for the item:
                int sortOrder = new ItemTypeTbl().GetItemSortOrder(line.ItemID);

                if (sortOrder == ItemTypeTbl.ItemTypeConstants.NotesSortOrder)
                {
                    // This is a "notes" line
                    string cleanedNotes = EmailUtils.CleanNoteText(notes);
                    email.AddFormatToBody("<li>{0}</li>", cleanedNotes);
                }
                else
                {
                    if (string.IsNullOrEmpty(line.PackagingName) || line.PackagingID == 0)
                    {
                        // No packaging specified
                        email.AddFormatToBody(MessageProvider.Get(MessageKeys.Order.ItemFormatBasic), line.ItemName, line.Qty);
                    }
                    else
                    {
                        // Packaging specified
                        email.AddFormatToBody(MessageProvider.Get(MessageKeys.Order.ItemFormatWithPrep), line.ItemName, line.Qty, line.PackagingName);
                    }
                }
            }
            email.AddToBody("</ul>");
        }

        private void AppendOrderDetails(EmailMailKitCls email, OrderHeaderData header)
        {
            // Add Purchase Order info if present
            if (!string.IsNullOrWhiteSpace(header.PurchaseOrder))
            {
                if (header.PurchaseOrder.EndsWith(SystemConstants.UIConstants.PORequiredText))
                {
                    email.AddStrAndNewLineToBody(MessageProvider.Get(MessageKeys.Order.ConfirmationPORequired));
                }
                else
                {
                    email.AddStrAndNewLineToBody(
                        MessageProvider.Format(MessageKeys.Order.ConfirmationPOReceived, header.PurchaseOrder)
                    );
                }
            }

            // Add Delivery Date
            email.AddFormatToBody(
                MessageProvider.Get(MessageKeys.Order.ConfirmationDeliveryDate),
                header.RequiredByDate.ToString("dd MMM, ddd, yyyy")
            );
        }

        // Map delivery person (ToBeDeliveredBy) id to an Order.Status* message key for pre-delivery emails.
        private string GetStatusKeyFromDeliveryPersonId(int deliveryPersonId)
        {
            switch (deliveryPersonId)
            {
                case SystemConstants.DeliveryConstants.CourierDeliveryID:
                case SystemConstants.DeliveryConstants.ParcelDispatchID:
                    // Courier and ParcelDispatch ("Prgo") -> "dispatched"
                    return MessageKeys.Order.StatusDispatched;
                case SystemConstants.DeliveryConstants.CollectionID:
                    // Collection -> "ready for collection"
                    return MessageKeys.Order.StatusReadyForCollection;
                default:
                    // Default -> pending delivery wording
                    return MessageKeys.Order.StatusPendingDelivery;
            }
        }
    }

    // You can reuse or adapt OrderLineData from OrderManager
    public class OrderLineData
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public double Qty { get; set; }
        public int PackagingID { get; set; }
        public string PackagingName { get; set; }
        // Add more fields as needed
    }
}