using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class OrderDoneResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CustomerUsageDisplay OriginalUsage { get; set; }
        public CustomerUsageDisplay UpdatedUsage { get; set; }
    }

    public class OrderDoneManager
    {
        private readonly ContactsUsageRepository _contactsUsageRepository;
        private readonly TempOrdersLinesRepository _tempOrdersLinesRepository;
        private readonly TempOrdersHeaderRepository _tempOrdersHeaderRepository;
        private readonly OrdersRepository _ordersRepository;
        private readonly ContactsRepository _contactsRepository;
        private readonly ContactsItemUsageRepository _contactsItemUsageRepository;
        private readonly ContactUsageLinesRepository _contactUsageLinesRepository;
        private readonly RecurringOrdersRepository _recurringOrdersRepository;
        private readonly ItemsRepository _itemsRepository;
        private readonly ItemGroupsRepository _itemGroupsRepository;

        public OrderDoneManager()
        {
            _contactsUsageRepository = new ContactsUsageRepository();
            _tempOrdersLinesRepository = new TempOrdersLinesRepository();
            _tempOrdersHeaderRepository = new TempOrdersHeaderRepository();
            _ordersRepository = new OrdersRepository();
            _contactsRepository = new ContactsRepository();
            _contactsItemUsageRepository = new ContactsItemUsageRepository();
            _contactUsageLinesRepository = new ContactUsageLinesRepository();
            _recurringOrdersRepository = new RecurringOrdersRepository();
            _itemsRepository = new ItemsRepository();
            _itemGroupsRepository = new ItemGroupsRepository();
        }

        public static OrderDoneResult CompleteOrder(
            int customerId,
            DateTime deliveryDate,
            string stockText,
            string cupCountText,
            string statusKey,
            string trackingNumber = null)
        {
            return new OrderDoneManager().CompleteOrderInternal(
                customerId, deliveryDate, stockText, cupCountText, statusKey, trackingNumber);
        }

        public static bool RequiresTrackingNumber(int? deliveredByPersonId, string confirmValue)
        {
            bool dispatchContext = string.Equals(confirmValue, "dispatched", StringComparison.OrdinalIgnoreCase)
                || IsDispatchDeliveryPerson(deliveredByPersonId);
            if (!dispatchContext)
                return false;

            try
            {
                var settings = new WooCommerceSettingsManager().GetSettings();
                if (settings != null && !settings.TrackingNumberRequired)
                    return false;
            }
            catch
            {
                // default: require
            }
            return true;
        }

        public static bool IsDispatchDeliveryPerson(int? personId)
        {
            if (!personId.HasValue || personId.Value <= 0)
                return false;

            try
            {
                string ids = new WooCommerceSettingsManager().GetSettings()?.DispatchDeliveryPersonIds;
                if (!string.IsNullOrWhiteSpace(ids))
                {
                    foreach (string part in ids.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int id;
                        if (int.TryParse(part.Trim(), out id) && id == personId.Value)
                            return true;
                    }
                    return false;
                }
            }
            catch
            {
                // fall through to built-in defaults
            }

            // Defaults when Woo settings row has never been saved (Prgo / Cour).
            return personId.Value == SystemConstants.DeliveryConstants.CourierDeliveryID
                || personId.Value == SystemConstants.DeliveryConstants.ParcelDispatchID;
        }

        private OrderDoneResult CompleteOrderInternal(
            int customerId,
            DateTime deliveryDate,
            string stockText,
            string cupCountText,
            string statusKey,
            string trackingNumber)
        {
            var result = new OrderDoneResult();
            if (!TempOrderSession.TryResolve(out int tempHeaderId, out int orderId))
            {
                result.Success = false;
                result.Message = MessageProvider.Get(MessageKeys.Order.NoTempOrder);
                return result;
            }

            var tempHeader = _tempOrdersHeaderRepository.GetById(tempHeaderId);
            if (tempHeader == null || tempHeader.ContactID != customerId)
            {
                result.Success = false;
                result.Message = MessageProvider.Get(MessageKeys.Order.NoTempOrder);
                return result;
            }

            var trackerTools = new TrackerTools();
            trackerTools.SetTrackerSessionErrorString(string.Empty);

            var originalUsage = _contactsUsageRepository.GetByContactId(customerId);
            bool hasCoffee = _tempOrdersLinesRepository.HasCoffeeInTempOrder(tempHeaderId);

            if (!string.IsNullOrEmpty(trackerTools.GetTrackerSessionErrorString()))
            {
                result.Success = false;
                result.Message = trackerTools.GetTrackerSessionErrorString();
                return result;
            }

            if (!string.IsNullOrEmpty(stockText) && double.TryParse(stockText, out double stock) && stock > 50.0)
            {
                result.Success = false;
                result.Message = "The stock quantity appears very high. Please check that you have entered the correct value in kilograms.";
                return result;
            }

            double pStock = string.IsNullOrEmpty(stockText) ? 0.0 : Math.Round(Convert.ToDouble(stockText), SystemConstants.DatabaseConstants.NumDecimalPoints);
            var latestUsageData = GetLatestUsageData(customerId, SystemConstants.ServiceTypeConstants.Coffee);
            int existingLastCupCount = _contactsUsageRepository.GetByContactId(customerId)?.LastCupCount ?? 0;

            bool pIsActual = !string.IsNullOrEmpty(cupCountText);
            int pCupCount = 0;
            if (pIsActual)
            {
                pCupCount = Convert.ToInt32(cupCountText);
            }

            if (pCupCount < 1 || pCupCount < latestUsageData.LastCount)
            {
                int estimated = CalcEstCupCount(customerId, latestUsageData, hasCoffee, tempHeaderId, existingLastCupCount);
                if (estimated >= 1)
                {
                    pCupCount = estimated;
                    pIsActual = false;
                }
                else if (existingLastCupCount >= 1)
                {
                    // Do not wipe a known contact reading when estimate cannot be calculated.
                    pCupCount = existingLastCupCount;
                    pIsActual = false;
                }
            }

            int updatedCupCount = AddItemsToUsageTables(customerId, tempHeaderId, pIsActual, pCupCount, pStock, deliveryDate);

            if (!_contactsUsageRepository.UpdateLastCupCount(customerId, updatedCupCount))
            {
                result.Success = false;
                result.Message =
                    "Could not save the cup count for this contact (no usage/prediction record could be created). " +
                    "Try again, or enter a cup count and retry.";
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Orders,
                    $"OrderDone: UpdateLastCupCount failed for ContactID={customerId}, cupCount={updatedCupCount}");
                return result;
            }

            RecordSystemCupCountTotal(orderId, customerId, updatedCupCount);

            UpdatePredictions(customerId, updatedCupCount);
            if (orderId > 0)
                _ordersRepository.MarkDoneForOrderId(orderId);
            else
                _ordersRepository.MarkDoneForTempOrderHeader(tempHeaderId);
            _contactsRepository.ResetReminderCount(customerId, hasCoffee);

            string companyName = _contactsRepository.GetContactNameById(customerId) ?? string.Empty;
            string orderPart = orderId > 0 ? $"Order {orderId}" : $"TempOrder {tempHeaderId}";
            string contactPart = string.IsNullOrWhiteSpace(companyName)
                ? $"Contact={customerId}"
                : $"Contact={customerId} ({companyName})";
            string countNote = pIsActual
                ? $"cupCount={updatedCupCount} (actual)"
                : $"cupCount={updatedCupCount} (estimate)";
            string stockNote = pStock > 0 ? $"; stockKg={pStock}" : string.Empty;
            // AppLogger prefixes the authenticated user (e.g. [User: warren]).
            AppLogger.WriteLog(
                SystemConstants.LogTypes.Orders,
                $"{orderPart} | {contactPart} | Order marked done | delivery={deliveryDate:yyyy-MM-dd}; {countNote}{stockNote}");

            string sentStatus = null;
            bool emailSent = false;
            if (!string.IsNullOrEmpty(statusKey))
            {
                sentStatus = SendOrderStatusEmail(customerId, statusKey, orderId, trackingNumber);
                emailSent = sentStatus == null;
            }

            string wooNoteStatus = ApplyDispatchTracking(
                orderId, customerId, trackingNumber, !string.IsNullOrEmpty(statusKey), emailSent);

            var recurringNotes = SyncRecurringOrderLastDone(customerId, tempHeaderId, deliveryDate);
            TempOrderSession.CleanupCompletedOrder(orderId, tempHeaderId);

            // Order completion succeeded even if confirmation email failed
            result.Success = true;
            if (sentStatus == null)
                result.Message = "Order completed successfully.";
            else
                result.Message = "Order completed, but confirmation email failed: " + sentStatus;

            if (!string.IsNullOrWhiteSpace(wooNoteStatus))
                result.Message += " " + wooNoteStatus;

            if (recurringNotes != null && recurringNotes.Count > 0)
                result.Message += " " + string.Join(" ", recurringNotes);

            result.OriginalUsage = CustomerUsageDisplay.FromContactsUsage(originalUsage);
            result.UpdatedUsage = CustomerUsageDisplay.FromContactsUsage(_contactsUsageRepository.GetByContactId(customerId));

            return result;
        }

        private int AddItemsToUsageTables(int contactId, int tempHeaderId, bool pIsActual, int pCupCount, double pStock, DateTime pDeliveryDate)
        {
            List<TempOrderUsageLine> all = _tempOrdersLinesRepository.GetUsageLinesForContact(contactId, tempHeaderId);
            int index1 = 0;
            string str = pIsActual ? "actual count" : "estimate count";

            if (pStock > 0.0)
            {
                pCupCount -= Convert.ToInt32(Math.Round(pStock * 100.0, 0));
                str = $"{str}; Stock of: {pCupCount.ToString()}";
            }

            while (all.Count > index1)
            {
                var summaryLine = new ContactUsageLine
                {
                    ContactID = all[index1].ContactID,
                    UsageDate = pDeliveryDate,
                    ItemServiceTypeID = all[index1].ItemServiceTypeID,
                    Qty = 0.0,
                    CupCount = pCupCount,
                    Notes = str
                };

                int serviceTypeId = all[index1].ItemServiceTypeID;
                do
                {
                    summaryLine.Qty += all[index1].Qty * all[index1].UnitsPerQty;
                    _contactsItemUsageRepository.InsertUsageLine(new ContactsItemUsage
                    {
                        ContactID = all[index1].ContactID,
                        DeliveryDate = pDeliveryDate,
                        ItemProvidedID = all[index1].ItemID,
                        QtyProvided = all[index1].Qty,
                        ItemPackagingID = all[index1].ItemPackagingID,
                        Notes = str
                    });
                    ++index1;
                }
                while (all.Count > index1 && serviceTypeId == all[index1].ItemServiceTypeID);

                _contactUsageLinesRepository.InsertUsageLine(summaryLine);
            }

            return pCupCount;
        }

        public static string SendOrderStatusEmail(long customerId, string statusKey, int orderId = 0, string trackingNumber = null)
        {
            if (statusKey == null)
            {
                return "? Status key is null.";
            }

            var customer = new ContactsRepository().GetById((int)customerId);
            if (customer == null)
            {
                return "? Contact not found.";
            }

            string recipient = !string.IsNullOrWhiteSpace(customer.EmailAddress) ? customer.EmailAddress : customer.AltEmailAddress;

            // ZZName / sundry: email lives in order notes as [#address#] (same as Order Detail confirm).
            if (customerId == SystemConstants.CustomerConstants.SundryCustomerID && orderId > 0)
            {
                var header = new OrdersRepository().GetOrderHeaderByOrderId(orderId);
                string fromNotes = new OrderManager().ExtractEmailFromNotes(header?.Notes);
                if (!string.IsNullOrWhiteSpace(fromNotes))
                    recipient = fromNotes.Trim();
            }

            if (string.IsNullOrWhiteSpace(recipient))
            {
                return "? No recipient email address found.";
            }

            var emailSettings = new EmailSettings();
            emailSettings.SetRecipient(recipient);

            var email = new EmailMailKitCls(emailSettings);
            email.AddSysCCFAddress();
            email.SetEmailSubject(MessageProvider.Get(MessageKeys.Order.StatusSubject));

            string contactName = !string.IsNullOrWhiteSpace(customer.ContactFirstName)
                ? customer.ContactFirstName
                : MessageProvider.Get(MessageKeys.Order.StatusDefaultContact);

            string statusMessage = MessageProvider.Get(statusKey);
            string body = MessageProvider.Format(MessageKeys.Order.StatusBody, contactName, statusMessage);
            email.AddToBody(body);
            if (!string.IsNullOrWhiteSpace(trackingNumber))
                email.AddToBody(MessageProvider.Format(MessageKeys.Order.StatusTrackingLine, trackingNumber.Trim()));
            email.AddToBody(MessageProvider.Get(MessageKeys.Order.StatusFooter));
            email.AddToBody(MessageProvider.Get(MessageProvider.GetEmailSignature()));

            bool success = email.SendEmail();
            if (success)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"? Order done message sent to {recipient}");
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"? Failed to send email to {recipient}: {email.myResults.sResult}");
            }

            return success ? null : $"? Failed to send email to {recipient}: {email.myResults.sResult}";
        }

        /// <summary>
        /// Saves the waybill on the Tracker order, emails already sent above,
        /// and posts a Woo customer note (does not complete the Woo order).
        /// </summary>
        private string ApplyDispatchTracking(
            int orderId,
            int customerId,
            string trackingNumber,
            bool emailAttempted,
            bool emailSent)
        {
            if (orderId <= 0 || string.IsNullOrWhiteSpace(trackingNumber))
                return null;

            string track = trackingNumber.Trim();
            var header = _ordersRepository.GetOrderHeaderByOrderId(orderId);
            var settings = new WooCommerceSettingsManager().GetSettings();
            if (header != null && settings != null && settings.AppendTrackingToOrderNotes)
            {
                string notes = header.Notes ?? string.Empty;
                if (notes.IndexOf(track, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    if (notes.Length > 0)
                        notes = notes.TrimEnd() + " | ";
                    notes += "Tracking: " + track;
                    _ordersRepository.UpdateOrderNotes(orderId, notes);
                }
            }

            var wooRepo = new WooOrderInfoRepository();
            var info = wooRepo.GetByTrackerOrderId(orderId);
            long? wooOrderId = info != null && info.WooOrderId > 0 ? info.WooOrderId : (long?)null;

            bool wooNotePosted = false;
            string wooMessage = null;
            if (wooOrderId.HasValue)
            {
                info.TrackingNumber = track;
                wooRepo.Upsert(info);

                WooCommerceApiClient.ApiCredentials creds;
                string credError;
                if (!new WooCommerceSettingsManager().TryGetApiCredentials(out creds, out credError))
                {
                    wooMessage = "Woo tracking note not sent (" + (credError ?? "no credentials") + "). Woo order was not completed.";
                }
                else
                {
                    string note = "Your order has been dispatched. Waybill / tracking number: " + track
                        + ". This number is also on your order in the shop. The order stays open until the parcel is received.";
                    string detail;
                    wooNotePosted = new WooCommerceApiClient().AddOrderNote(
                        creds, wooOrderId.Value, note, customerNote: true, out detail);
                    if (wooNotePosted)
                    {
                        AppLogger.WriteLog("woo",
                            "Order Done posted Woo customer note for Woo #" + (info.WooOrderNumber ?? wooOrderId.Value.ToString())
                            + " waybill=" + track + " (status not completed)");
                        wooMessage = "Waybill saved and sent to WooCommerce as a customer note. The Woo order was left open.";
                    }
                    else
                    {
                        AppLogger.WriteLog("woo", "Order Done Woo customer note failed: " + (detail ?? "unknown"));
                        wooMessage = "Waybill saved on the Tracker order, but the Woo customer note failed: "
                            + (detail ?? "unknown") + " Woo order was not completed.";
                    }
                }
            }

            var waybill = new OrderWaybill
            {
                OrderID = orderId,
                ContactID = customerId > 0 ? customerId : (int?)null,
                WaybillNumber = track,
                Carrier = CarrierLabel(header?.ToBeDeliveredBy ?? 0),
                DispatchStatus = "Dispatched",
                DispatchedAt = TimeZoneUtils.Now(),
                WooOrderId = wooOrderId,
                WooNotePosted = wooNotePosted,
                CustomerEmailSent = emailAttempted && emailSent,
                CreatedBy = HttpContext.Current?.User?.Identity?.Name
            };
            new OrderWaybillRepository().Upsert(waybill);

            if (wooMessage != null)
                return wooMessage;
            return "Waybill " + track + " saved on the order.";
        }

        private static string CarrierLabel(int personId)
        {
            if (personId == SystemConstants.DeliveryConstants.ParcelDispatchID)
                return "Pargo";
            if (personId == SystemConstants.DeliveryConstants.CourierDeliveryID)
                return "Courier";
            return null;
        }

        private static bool IsCoffeeOrConsumableServiceType(int serviceType)
        {
            return serviceType == SystemConstants.ServiceTypeConstants.Coffee;
        }

        private bool IsCoffeeOrConsumable(int itemId)
        {
            return IsCoffeeOrConsumableServiceType(_itemsRepository.GetServiceTypeForItem(itemId));
        }

        private List<string> SyncRecurringOrderLastDone(int customerId, int tempHeaderId, DateTime deliveryDate)
        {
            var notes = new List<string>();
            var reoccurOrders = _recurringOrdersRepository.GetEnabledSummariesByContactId(customerId);
            var deliveredItems = _tempOrdersLinesRepository.GetUsageLinesForContact(customerId, tempHeaderId);

            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                $"SyncRecurringOrderLastDone: Cust={customerId} RecurCnt={reoccurOrders.Count} DeliveredCnt={deliveredItems.Count}");

            var updatedOrderIds = new HashSet<int>();
            int updatedLineCount = 0;

            foreach (var reoccurOrder in reoccurOrders)
            {
                bool matched = false;
                foreach (var item in deliveredItems)
                {
                    if (!OrderMatchesReoccuringOrder(item, reoccurOrder))
                        continue;

                    matched = true;
                    bool wasEnabled = reoccurOrder.Enabled != false;
                    _recurringOrdersRepository.SetRecurringOrderItemDates(
                        deliveryDate,
                        reoccurOrder.RecurringOrderItemID,
                        orderDone: true);
                    updatedLineCount++;

                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"Recurring updated (Cust={customerId}, RecItemID={reoccurOrder.RecurringOrderItemID}, RequiredItem={reoccurOrder.ItemRequiredID}) using delivered ItemID={item.ItemID}");

                    if (wasEnabled && !updatedOrderIds.Contains(reoccurOrder.RecurringOrderID))
                    {
                        updatedOrderIds.Add(reoccurOrder.RecurringOrderID);
                        // Re-read enabled flag after update (SetRecurringOrderItemDates may have disabled)
                        var refreshed = _recurringOrdersRepository.GetById(reoccurOrder.RecurringOrderID);
                        if (refreshed != null && refreshed.Enabled == false)
                        {
                            string name = string.IsNullOrWhiteSpace(reoccurOrder.CompanyName)
                                ? "contact"
                                : reoccurOrder.CompanyName.Trim();
                            notes.Add($"Recurring order for {name} ended (past until date) and was disabled.");
                        }
                    }

                    break;
                }

                if (!matched)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"SyncRecurringOrderLastDone: no delivered match for Cust={customerId} RecItemID={reoccurOrder.RecurringOrderItemID} RequiredItem={reoccurOrder.ItemRequiredID}");
                }
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                $"SyncRecurringOrderLastDone: Cust={customerId} updated {updatedLineCount}/{reoccurOrders.Count} recurring lines");

            return notes;
        }

        private bool IsGroupItem(int itemTypeId)
        {
            return _itemsRepository.GetServiceTypeForItem(itemTypeId) == SystemConstants.ServiceTypeConstants.GroupItem;
        }

        private bool GroupContainsCoffeeOrConsumable(IEnumerable<int> groupItemIds)
        {
            foreach (var id in groupItemIds)
            {
                if (IsCoffeeOrConsumable(id))
                {
                    return true;
                }
            }

            return false;
        }

        private bool OrderMatchesReoccuringOrder(TempOrderUsageLine deliveredItem, RecurringOrderSummary reoccurOrder)
        {
            int itemRequiredId = reoccurOrder.ItemRequiredID ?? 0;

            if (IsGroupItem(itemRequiredId))
            {
                var groupItemIds = _itemGroupsRepository.GetItemIdsForGroup(itemRequiredId);

                if (groupItemIds.Contains(deliveredItem.ItemID))
                {
                    return true;
                }

                if (IsCoffeeOrConsumable(deliveredItem.ItemID) && GroupContainsCoffeeOrConsumable(groupItemIds))
                {
                    return true;
                }

                return false;
            }

            bool requiredIsCoffeeLike = IsCoffeeOrConsumable(itemRequiredId);
            bool deliveredIsCoffeeLike = IsCoffeeOrConsumable(deliveredItem.ItemID);

            if (requiredIsCoffeeLike && deliveredIsCoffeeLike)
            {
                return true;
            }

            return deliveredItem.ItemID == itemRequiredId;
        }

        private struct LineUsageData
        {
            public int LastCount;
            public double LastQty;
            public DateTime UsageDate;
        }

        private LineUsageData GetLatestUsageData(int contactId, int serviceTypeId)
        {
            var line = _contactUsageLinesRepository.GetLatestUsageLine(contactId, serviceTypeId);
            return new LineUsageData
            {
                LastCount = line?.CupCount ?? 0,
                LastQty = line?.Qty ?? 0,
                UsageDate = line?.UsageDate ?? DateTime.MinValue
            };
        }

        private int CalcEstCupCount(
            int contactId,
            LineUsageData usageData,
            bool hasCoffee,
            int tempHeaderId,
            int existingLastCupCount)
        {
            double dailyAverage = _contactsUsageRepository.GetByContactId(contactId)?.DailyConsumption
                ?? SystemConstants.BusinessConstants.TypicalAverageConsumption;
            if (dailyAverage <= 0)
                dailyAverage = SystemConstants.BusinessConstants.TypicalAverageConsumption;

            if (usageData.UsageDate > DateTime.MinValue)
            {
                int daysSince = (TimeZoneUtils.Now().Date - usageData.UsageDate.Date).Days;
                double estimate = !hasCoffee || usageData.LastQty == 0.0
                    ? usageData.LastCount + (daysSince * dailyAverage)
                    : usageData.LastCount + (usageData.LastQty * SystemConstants.BusinessConstants.TypicalCupsPerKg);

                return Convert.ToInt32(Math.Round(estimate));
            }

            // First usable estimate: previous contact reading + coffee on this order (kg × cups/kg).
            if (hasCoffee && tempHeaderId > 0 && contactId > 0)
            {
                double coffeeKg = GetCoffeeKgOnTempOrder(contactId, tempHeaderId);
                if (coffeeKg > 0)
                {
                    int baseCount = Math.Max(existingLastCupCount, usageData.LastCount);
                    return baseCount + Convert.ToInt32(Math.Round(coffeeKg * SystemConstants.BusinessConstants.TypicalCupsPerKg));
                }
            }

            return existingLastCupCount > 0 ? existingLastCupCount : 0;
        }

        private double GetCoffeeKgOnTempOrder(int contactId, int tempHeaderId)
        {
            try
            {
                List<TempOrderUsageLine> lines = _tempOrdersLinesRepository.GetUsageLinesForContact(contactId, tempHeaderId);
                if (lines == null || lines.Count == 0)
                    return 0;

                int coffeeType = SystemConstants.ServiceTypeConstants.Coffee;
                return lines
                    .Where(l => l.ItemServiceTypeID == coffeeType)
                    .Sum(l => l.Qty * l.UnitsPerQty);
            }
            catch
            {
                return 0;
            }
        }

        private void UpdatePredictions(int contactId, int lastCupCount)
        {
            // LastCupCount is already written via UpdateLastCupCount in CompleteOrderInternal.
            // Recalculate NextCoffeeBy / service dates from latest usage (legacy UpdatePredictions).
            if (contactId <= 0 || lastCupCount <= 0)
                return;

            try
            {
                new PredictionManager().UpdatePredictions(contactId, lastCupCount);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"OrderDone: UpdatePredictions failed for ContactID={contactId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Snapshot the live SUM(LastCupCount) into TotalCountTrackerTbl after each Order Done.
        /// Home page uses the live sum; this keeps the tracker history in sync.
        /// </summary>
        private void RecordSystemCupCountTotal(int orderId, int contactId, int contactCupCount)
        {
            try
            {
                long totalCups = _contactsUsageRepository.GetSumOfLastCupCounts();
                if (totalCups > int.MaxValue)
                    totalCups = int.MaxValue;

                string comments = orderId > 0
                    ? string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Order {0} done; Contact {1} cupCount={2}",
                        orderId, contactId, contactCupCount)
                    : string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Order done; Contact {0} cupCount={1}",
                        contactId, contactCupCount);

                new TotalCountTrackerRepository().Add((int)totalCups, comments);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    "OrderDone: TotalCountTracker update failed: " + ex.Message);
            }
        }
    }
}
