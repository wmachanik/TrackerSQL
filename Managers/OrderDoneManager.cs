using System;
using System.Collections.Generic;
using System.Linq;
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
            string statusKey)
        {
            return new OrderDoneManager().CompleteOrderInternal(customerId, deliveryDate, stockText, cupCountText, statusKey);
        }

        private OrderDoneResult CompleteOrderInternal(
            int customerId,
            DateTime deliveryDate,
            string stockText,
            string cupCountText,
            string statusKey)
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
            var latestUsageData = GetLatestUsageData(customerId, 2);

            bool pIsActual = !string.IsNullOrEmpty(cupCountText);
            int pCupCount = 0;
            if (pIsActual)
            {
                pCupCount = Convert.ToInt32(cupCountText);
            }

            if (pCupCount < 1 || pCupCount < latestUsageData.LastCount)
            {
                pCupCount = CalcEstCupCount(customerId, latestUsageData, hasCoffee);
                pIsActual = false;
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
            if (!string.IsNullOrEmpty(statusKey))
            {
                sentStatus = SendOrderStatusEmail(customerId, statusKey);
            }

            var recurringNotes = SyncRecurringOrderLastDone(customerId, tempHeaderId, deliveryDate);
            TempOrderSession.CleanupCompletedOrder(orderId, tempHeaderId);

            // Order completion succeeded even if confirmation email failed
            result.Success = true;
            if (sentStatus == null)
                result.Message = "Order completed successfully.";
            else
                result.Message = "Order completed, but confirmation email failed: " + sentStatus;

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

        public static string SendOrderStatusEmail(long customerId, string statusKey)
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

            foreach (var reoccurOrder in reoccurOrders)
            {
                foreach (var item in deliveredItems)
                {
                    if (!IsCoffeeOrConsumable(item.ItemID))
                    {
                        continue;
                    }

                    if (OrderMatchesReoccuringOrder(item, reoccurOrder))
                    {
                        bool wasEnabled = reoccurOrder.Enabled != false;
                        _recurringOrdersRepository.SetRecurringOrderItemDates(
                            deliveryDate,
                            reoccurOrder.RecurringOrderItemID,
                            orderDone: true);

                        AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                            $"Recurring updated (Cust={customerId}, RecItemID={reoccurOrder.RecurringOrderItemID}) using delivered ItemID={item.ItemID}");

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
                }
            }

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

        private int CalcEstCupCount(int contactId, LineUsageData usageData, bool hasCoffee)
        {
            if (usageData.UsageDate <= DateTime.MinValue)
            {
                return 0;
            }

            double dailyAverage = _contactsUsageRepository.GetByContactId(contactId)?.DailyConsumption
                ?? SystemConstants.BusinessConstants.TypicalAverageConsumption;
            if (dailyAverage <= 0)
            {
                dailyAverage = SystemConstants.BusinessConstants.TypicalAverageConsumption;
            }

            int daysSince = (TimeZoneUtils.Now().Date - usageData.UsageDate.Date).Days;
            double estimate = !hasCoffee || usageData.LastQty == 0.0
                ? usageData.LastCount + (daysSince * dailyAverage)
                : usageData.LastCount + (usageData.LastQty * 100.0);

            return Convert.ToInt32(Math.Round(estimate));
        }

        private void UpdatePredictions(int contactId, int lastCupCount)
        {
            // LastCupCount is already written via UpdateLastCupCount in CompleteOrderInternal.
            // Do NOT call RepositoryBase.Update here — it binds parameters as DbType.Object (sql_variant)
            // and SQL Server rejects implicit conversion to int/date columns.
            if (contactId <= 0 || lastCupCount <= 0)
                return;

            _contactsUsageRepository.UpdateLastCupCount(contactId, lastCupCount);
        }
    }
}
