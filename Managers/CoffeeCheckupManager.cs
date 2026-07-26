using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Central business logic manager for Coffee Checkup operations
    /// Orchestrates the entire coffee checkup process following SOLID principles
    /// ENHANCED WITH QUICK PERFORMANCE OPTIMIZATIONS
    /// </summary>
    public class CoffeeCheckupManager
    {
        private readonly CoffeeCheckupEmailManager _emailManager;
        private readonly ItemsRepository _itemsRepository = new ItemsRepository();
        private readonly ItemPackagingsRepository _itemPackagingsRepository = new ItemPackagingsRepository();
        private readonly SysDataRepository _sysDataRepository = new SysDataRepository();
        private readonly ContactsRepository _contactsRepository = new ContactsRepository();
        private readonly ContactsUsageRepository _contactsUsageRepository = new ContactsUsageRepository();
        private readonly RecurringOrdersRepository _recurringOrdersRepository = new RecurringOrdersRepository();
        private readonly OrdersRepository _ordersRepository = new OrdersRepository();
        private readonly SentRemindersLogRepository _sentRemindersLogRepository = new SentRemindersLogRepository();
        private readonly TempCoffeeCheckupRepository _tempCoffeeCheckupRepository = new TempCoffeeCheckupRepository();
        private readonly CoffeeCheckupRepository _coffeeCheckupRepository = new CoffeeCheckupRepository();
        private readonly ContactTrackedServiceItemsRepository _contactTrackedServiceItemsRepository = new ContactTrackedServiceItemsRepository();
        private readonly ContactsItemUsageRepository _contactsItemUsageRepository = new ContactsItemUsageRepository();
        private readonly AreasRepository _areasRepository = new AreasRepository();

        private sealed class RecurringCheckupContext
        {
            public RecurringOrderSummary Summary { get; set; }
            public DateTime PrepDate { get; set; }
        }

        // Constants moved from code-behind for better organization to system cosntatns
        // private const int CONST_FORCEREMINDERDELAYCOUNT = 4;
        // private const int CONST_MAXREMINDERS = 7;
        // private const int CONST_DEFAULTREMINDERWINDOWDAYS = 7;
        // private const int CONST_DEFAULTMINIMUMMONTHLYRECURRINGDAYS = 20;

        // MISSING: Static caching for frequently accessed lookup data
        private static Dictionary<int, string> _cachedItemDescriptions;
        private static Dictionary<int, string> _cachedItemSKUs;
        private static Dictionary<int, string> _cachedAreaNames;
        private static Dictionary<int, string> _cachedPackagingDescriptions;
        private static List<int> _cachedInternalCustomerIds;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly object _cacheLock = new object();
        private readonly HolidayClosureProvider _holidayProvider = new HolidayClosureProvider();
        private readonly List<string> _lastPrepNotices = new List<string>();

        /// <summary>
        /// User-facing notices from the last Prep Data run (contact names, not IDs).
        /// </summary>
        public IReadOnlyList<string> LastPrepNotices => _lastPrepNotices;

        public CoffeeCheckupManager()
        {
            _emailManager = new CoffeeCheckupEmailManager();
        }
        private static void UpdateCacheExpiry()
        {
            _cacheExpiry = DateTime.Now.AddMinutes(1); // or your preferred duration
        }
        // Add this helper near other private helpers
        /// <summary>
        /// Ensures the TempCoffeeCheckup table has data prepared today.
        /// If empty or oldest stamp predates today, we treat it as stale.
        /// </summary>
        private bool TempDataIsFresh()
        {
            try
            {
                var contacts = _tempCoffeeCheckupRepository.GetAllContacts("CustomerID");
                if (contacts == null || contacts.Count == 0)
                    return false;

                // Heuristic: at least one contact has a NextPreparationDate or NextDeliveryDate == today or later
                var today = TimeZoneUtils.Now().Date;
                bool anyValid = contacts.Any(c =>
                    c.NextPreparationDate.Date >= today ||
                    c.NextDeliveryDate.Date >= today);

                return anyValid;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"CoffeeCheckupManager: TempDataIsFresh check failed: {ex.Message}");
                // Fail safe: force user to prep again
                return false;
            }
        }
        /// <summary>
        /// Main entry point for processing coffee checkup reminders
        /// </summary>
        public BatchSendResult ProcessCoffeeCheckupReminders(SendCheckEmailTexts emailData, bool includeOrdersEmailCc = true)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _emailManager.IncludeConfiguredCc = includeOrdersEmailCc;
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    includeOrdersEmailCc
                        ? "CoffeeCheckupManager: CC to orders email enabled"
                        : "CoffeeCheckupManager: CC to orders email disabled for this send");

                // NEW: Freshness guard ? prevent sending with stale (yesterday/older) prep data
                if (!TempDataIsFresh())
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        "CoffeeCheckupManager: Temp reminder data is stale or empty. Abort send and instruct user to Prep Data first.");
                    return new BatchSendResult
                    {
                        IsSuccess = false,
                        TotalSent = 0,
                        TotalFailed = 0,
                        ErrorMessage = "Reminder data is stale or missing. Click 'Prep Data' before sending."
                    };
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "CoffeeCheckupManager: Starting coffee checkup process");

                // 1. Validate configuration
                if (!_emailManager.ValidateConfiguration())
                {
                    return new BatchSendResult
                    {
                        IsSuccess = false,
                        TotalSent = 0,
                        TotalFailed = 1,
                        ErrorMessage = "Email configuration validation failed"
                    };
                }

                // Pre-warm caches before processing
                WarmupCaches();

                // 2. Get eligible customers
                var eligibleCustomers = GetEligibleCustomers();

                if (!eligibleCustomers.Any())
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "CoffeeCheckupManager: No eligible customers found");
                    return new BatchSendResult
                    {
                        IsSuccess = true,
                        TotalSent = 0,
                        TotalFailed = 0,
                        ErrorMessage = "No customers found requiring reminders"
                    };
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Processing {eligibleCustomers.Count} eligible customers");

                // 3. Process batch reminders
                var result = ProcessRemindersBatch(eligibleCustomers, emailData);

                stopwatch.Stop();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Process completed in {stopwatch.ElapsedMilliseconds}ms - {result.TotalSent} sent, {result.TotalFailed} failed");

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Process failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
                return new BatchSendResult
                {
                    IsSuccess = false,
                    TotalSent = 0,
                    TotalFailed = 1,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Processes reminder batch using the existing logic - ENHANCED WITH PROPER TEST MODE HANDLING
        /// </summary>
        private BatchSendResult ProcessRemindersBatch(List<ContactToRemindWithItems> allContacts, SendCheckEmailTexts emailData)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Processing batch with {allContacts.Count} contacts");

            var totalResult = new BatchSendResult();

            // Check if we're in test mode
            var testEmailClient = new EmailMailKitCls();
            // Group contacts by reminder type
            var recurringContacts = new List<ContactToRemindWithItems>();
            var autoFulfillContacts = new List<ContactToRemindWithItems>();
            var reminderOnlyContacts = new List<ContactToRemindWithItems>();
            var failedContacts = new List<string>();

            // Process each contact and categorize
            int processed = 0;
            foreach (var contact in allContacts)
            {
                try
                {
                    // Validate eligibility
                    if (!ValidateCustomerEligibility(contact))
                    {
                        continue;
                    }

                    processed++;
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Processing contact {processed}: {contact.CompanyName} (ID: {contact.CustomerID})");

                    string orderType = GetOrderType(contact);
                    if (!UpdateCustomerReminderData(contact, out string updateFailureReason))
                    {
                        failedContacts.Add($"{FormatContactDisplayName(contact)} - {updateFailureReason}");
                        continue;
                    }
                    // Categorize for batch processing
                    CategorizeContact(contact, orderType, recurringContacts, autoFulfillContacts, reminderOnlyContacts);
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Error processing contact {contact.CompanyName}: {ex.Message}");
                    failedContacts.Add($"{FormatContactDisplayName(contact)} - Processing error: {ex.Message}");
                }
            }

            // Send batches by type
            totalResult = SendAllBatches(recurringContacts, autoFulfillContacts, reminderOnlyContacts, emailData);

            // Add pre-processing failures (disabled-at-max, DB errors, etc.)
            if (failedContacts.Count > 0)
            {
                totalResult.TotalFailed += failedContacts.Count;
                string failMsg = string.Join("; ", failedContacts);
                totalResult.ErrorMessage = string.IsNullOrEmpty(totalResult.ErrorMessage)
                    ? failMsg
                    : totalResult.ErrorMessage + "; " + failMsg;
                LogFailedCustomers(failedContacts);
            }
            totalResult.IsSuccess = totalResult.TotalFailed == 0;

            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Batch completed - {totalResult.TotalSent} sent, {totalResult.TotalFailed} failed");
            return totalResult;
        }
        /// <summary>
        /// Determines order type for a contact
        /// </summary>
        private string GetOrderType(ContactToRemindWithItems contact)
        {
            bool hasAutoFulfill = contact.ItemsContactRequires.Exists(x => x.AutoFulfill);
            bool hasRecurring = contact.ItemsContactRequires.Exists(x => x.RecurringOrder);

            if (hasRecurring && hasAutoFulfill)
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.OrderTypeCombined);
            else if (hasRecurring)
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.OrderTypeRecurring);
            else if (hasAutoFulfill)
                return MessageProvider.Get(MessageKeys.CoffeeCheckup.OrderTypeAutoFulfill);

            return string.Empty; // Reminder only
        }
        /// <summary>
        /// Updates customer reminder data in database.
        /// When reminder count reaches the system max, disables the contact and returns false.
        /// </summary>
        private bool UpdateCustomerReminderData(ContactToRemindWithItems contact, out string failureReason)
        {
            failureReason = null;
            string displayName = FormatContactDisplayName(contact);
            try
            {
                contact.ReminderCount++;

                if (contact.ReminderCount < SystemConstants.CheckupConstants.MaxReminders)
                {
                    if (contact.ReminderCount >= SystemConstants.CheckupConstants.ForceReminderDelayCount)
                    {
                        int delayDays = 10 * (contact.ReminderCount - SystemConstants.CheckupConstants.ForceReminderDelayCount + 1);
                        _contactsUsageRepository.ForceNextCoffeeDate(
                            (int)contact.CustomerID,
                            contact.NextPreparationDate.AddDays(delayDays));
                    }

                    _contactsRepository.SetSentReminderAndIncrementReminderCount(
                        TimeZoneUtils.Now().Date,
                        (int)contact.CustomerID);

                    return true;
                }

                DisableContactForMaxReminders(contact);
                failureReason =
                    $"disabled — exceeded max reminders ({SystemConstants.CheckupConstants.MaxReminders})";
                return false;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Database update failed for {displayName}: {ex.Message}");
                failureReason = "database update failed";
                return false;
            }
        }

        private void DisableContactForMaxReminders(ContactToRemindWithItems contact)
        {
            string displayName = FormatContactDisplayName(contact);
            _contactsRepository.DisableContact(
                (int)contact.CustomerID,
                $"Disabled on {TimeZoneUtils.Now():d} - exceeded max reminder limit ({SystemConstants.CheckupConstants.MaxReminders})");
            contact.enabled = false;
            string notice = $"Disabled {displayName} — exceeded max reminders ({SystemConstants.CheckupConstants.MaxReminders}).";
            AddPrepNotice(notice);
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: {notice}");
        }

        private static string FormatContactDisplayName(ContactToRemindDetails contact)
        {
            if (contact == null)
                return "Unknown contact";
            if (!string.IsNullOrWhiteSpace(contact.CompanyName))
                return contact.CompanyName.Trim();
            if (!string.IsNullOrWhiteSpace(contact.ContactFirstName))
                return contact.ContactFirstName.Trim();
            return "Unknown contact";
        }

        private void AddPrepNotice(string notice)
        {
            if (string.IsNullOrWhiteSpace(notice))
                return;
            if (!_lastPrepNotices.Contains(notice))
                _lastPrepNotices.Add(notice);
        }

        /// <summary>
        /// Real until dates only — SystemMinDate (1980-01-01) and 2099+ mean forever.
        /// </summary>
        private static bool HasRealRequireUntilDate(DateTime? until)
        {
            if (!until.HasValue)
                return false;
            DateTime date = until.Value.Date;
            return date > SystemConstants.DatabaseConstants.SystemMinDate && date.Year < 2099;
        }

        /// <summary>
        /// Recomputes last-cycle flag from staged recurring item IDs (temp table does not persist the flag).
        /// </summary>
        private void ApplyLastRecurringOrderFlag(ContactToRemindWithItems contact)
        {
            if (contact?.ItemsContactRequires == null)
                return;

            foreach (var item in contact.ItemsContactRequires)
            {
                if (!item.RecurringOrder || item.RecurringOrderItemID <= 0)
                    continue;

                var summary = _recurringOrdersRepository.GetSummaryByRecurringOrderItemId(item.RecurringOrderItemID);
                if (summary == null || !_recurringOrdersRepository.IsFinalOccurrenceBeforeUntil(summary))
                    continue;

                contact.IsLastRecurringOrder = true;
                if (HasRealRequireUntilDate(summary.RequireUntilDate))
                    contact.LastRecurringUntilDate = summary.RequireUntilDate.Value.Date;
                return;
            }
        }
        /// <summary>
        /// Categorizes contact into appropriate batch - FIXED TO PREVENT DUPLICATES
        /// </summary>
        private void CategorizeContact(ContactToRemindWithItems contact, string orderType,
            List<ContactToRemindWithItems> recurringContacts,
            List<ContactToRemindWithItems> autoFulfillContacts,
            List<ContactToRemindWithItems> reminderOnlyContacts)
        {
            // IMPORTANT: Each customer should only go into ONE category to prevent multiple emails
            if (string.IsNullOrWhiteSpace(orderType))
            {
                reminderOnlyContacts.Add(contact);
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Categorized {contact.CompanyName} as REMINDER ONLY");
            }
            else if (orderType.Contains("recurring"))
            {
                recurringContacts.Add(contact);
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Categorized {contact.CompanyName} as RECURRING");
            }
            else if (orderType.Contains("autofulfill") || orderType.Contains("auto"))
            {
                autoFulfillContacts.Add(contact);
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Categorized {contact.CompanyName} as AUTOFULFILL");
            }
            else
            {
                // Default to reminder only if orderType is not recognized
                reminderOnlyContacts.Add(contact);
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Categorized {contact.CompanyName} as REMINDER ONLY (unknown order type: '{orderType}')");
            }
        }

        /// <summary>
        /// Sends all batches and combines results
        /// </summary>
        private BatchSendResult SendAllBatches(
            List<ContactToRemindWithItems> recurringContacts,
            List<ContactToRemindWithItems> autoFulfillContacts,
            List<ContactToRemindWithItems> reminderOnlyContacts,
            SendCheckEmailTexts emailData)
        {
            var totalResult = new BatchSendResult();

            if (recurringContacts.Any())
            {
                var recurringResult = SendReminderBatch(recurringContacts, "recurring", emailData);
                totalResult.Combine(recurringResult);
            }

            if (autoFulfillContacts.Any())
            {
                var autoFulfillResult = SendReminderBatch(autoFulfillContacts, "autofulfill", emailData);
                totalResult.Combine(autoFulfillResult);
            }

            if (reminderOnlyContacts.Any())
            {
                var reminderResult = SendReminderBatch(reminderOnlyContacts, "reminder", emailData);
                totalResult.Combine(reminderResult);
            }

            return totalResult;
        }
        /// <summary>
        /// Prepares customer reminder data for display in UI
        /// </summary>
        public void PrepareCustomerReminderData(int reminderWindowDays)
        {
            try
            {
                _lastPrepNotices.Clear();

                // Ensure roast dates are current
                var trackerTools = new TrackerTools();
                if (!trackerTools.IsNextPreparationDateByAreaTodays())
                {
                    trackerTools.SetNextPreparationDateByArea();
                }

                AreaDeliveryMatrix.EnsureBuilt();

                // Build reminder list and populate temp tables
                SetListOfContactsToSendReminderTo(reminderWindowDays);

                AppLogger.WriteLog(
                    SystemConstants.LogTypes.SendCheckup,
                    $"CoffeeCheckupManager: Customer reminder data preparation completed (window: {reminderWindowDays} days)");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"CoffeeCheckupManager: Error preparing customer data: {ex.Message}");
                throw;
            }
        }
        public int PostAdjustPreparedReminderData(int reminderWindowDays)
        {
            var today = TimeZoneUtils.Now().Date;
            var closureProvider = new HolidayClosureProvider();

            if (!closureProvider.IsThereAHolodayComing(today, reminderWindowDays))
                return -1;  // tell them that there are no holidays coming

            var contacts = _tempCoffeeCheckupRepository.GetAllContacts("CustomerID"); // prepared list

            int updated = 0;
            foreach (var c in contacts)
            {
                var prep = c.NextPreparationDate;
                var del = c.NextDeliveryDate;

                if (!closureProvider.IsClosed(prep, true) && !closureProvider.IsClosed(del, false))
                    continue;

                var adj = closureProvider.AdjustPair(prep, del);
                if (!adj.WasAdjusted) continue;

                UpdateTempContactDates(c.CustomerID, adj.Prep, adj.Delivery);
                updated++;
            }

            if (updated > 0)
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"PostAdjustPreparedReminderData: adjusted {updated} contact(s) for closures.");

            return updated;
        }

        // Minimal DAL update; adjust table/column names if they differ
        private void UpdateTempContactDates(long customerId, DateTime nextPrep, DateTime nextDelivery)
        {
            _tempCoffeeCheckupRepository.UpdateContactDates((int)customerId, nextPrep, nextDelivery);
        }
        // MISSING: All the cache methods
        /// <summary>
        /// QUICK WIN: Cached item descriptions for GridView display
        /// </summary>
        public string GetCachedItemDescription(int itemId)
        {
            if (itemId <= 0) return string.Empty;

            lock (_cacheLock)
            {
                if (_cachedItemDescriptions == null || DateTime.Now > _cacheExpiry)
                {
                    _cachedItemDescriptions = new Dictionary<int, string>();
                    UpdateCacheExpiry();
                }
            }

            if (!_cachedItemDescriptions.ContainsKey(itemId))
            {
                try
                {
                    _cachedItemDescriptions[itemId] = _itemsRepository.GetItemDescById(itemId);
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error caching item description for ID {itemId}: {ex.Message}");
                    _cachedItemDescriptions[itemId] = $"Item {itemId}"; // Fallback
                }
            }

            return _cachedItemDescriptions[itemId];
        }


        /// <summary>
        /// QUICK WIN: Cached item SKUs for GridView display
        /// </summary>
        public string GetCachedItemSKU(int itemId)
        {
            if (itemId <= 0) return string.Empty;

            lock (_cacheLock)
            {
                if (_cachedItemSKUs == null || DateTime.Now > _cacheExpiry)
                {
                    _cachedItemSKUs = new Dictionary<int, string>();
                    UpdateCacheExpiry();
                }

                if (!_cachedItemSKUs.ContainsKey(itemId)
            )
                {
                    try
                    {
                        _cachedItemSKUs[itemId] = _itemsRepository.GetItemSku(itemId);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error caching item SKU for ID {itemId}: {ex.Message}");
                        _cachedItemSKUs[itemId] = $"SKU{itemId}"; // Fallback
                    }
                }

                return _cachedItemSKUs[itemId];
            }
        }

        /// <summary>
        /// QUICK WIN: Cached Area names for GridView display
        /// </summary>
        public string GetCachedAreaName(int AreaId)
        {
            if (AreaId <= 0) return string.Empty;

            lock (_cacheLock)
            {
                if (_cachedAreaNames == null || DateTime.Now > _cacheExpiry)
                {
                    _cachedAreaNames = new Dictionary<int, string>();
                    UpdateCacheExpiry();
                }

                if (!_cachedAreaNames.ContainsKey(AreaId))
                {
                    try
                    {
                        _cachedAreaNames[AreaId] = _areasRepository.GetAreaName(AreaId);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error caching Area name for ID {AreaId}: {ex.Message}");
                        _cachedAreaNames[AreaId] = $"Area {AreaId}"; // Fallback
                    }
                }

                return _cachedAreaNames[AreaId];
            }
        }

        /// <summary>
        /// QUICK WIN: Cached packaging descriptions for GridView display
        /// </summary>
        public string GetCachedPackagingDescription(int packagingId)
        {
            if (packagingId <= 0) return string.Empty;

            lock (_cacheLock)
            {
                if (_cachedPackagingDescriptions == null || DateTime.Now > _cacheExpiry)
                {
                    _cachedPackagingDescriptions = new Dictionary<int, string>();
                    UpdateCacheExpiry();
                }

                if (!_cachedPackagingDescriptions.ContainsKey(packagingId))
                {
                    try
                    {
                        _cachedPackagingDescriptions[packagingId] = _itemPackagingsRepository.GetPackagingDescById(packagingId);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error caching packaging description for ID {packagingId}: {ex.Message}");
                        _cachedPackagingDescriptions[packagingId] = $"Package {packagingId}"; // Fallback
                    }
                }

                return _cachedPackagingDescriptions[packagingId];
            }
        }
        /// <summary>
        /// QUICK WIN: Static cache invalidation method for when lookup data changes
        /// </summary>
        public static void InvalidateCache()
        {
            lock (_cacheLock)
            {
                _cachedItemDescriptions = null;
                _cachedItemSKUs = null;
                _cachedAreaNames = null;
                _cachedPackagingDescriptions = null;
                _cachedInternalCustomerIds = null;
                _cacheExpiry = DateTime.MinValue;
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "CoffeeCheckupManager: Cache invalidated");
            }
        }

        /// <summary>
        /// QUICK WIN: Static cache invalidation method for when lookup data changes
        /// </summary>
        public string GetCachedItemUoM(int itemId)
        {
            if (itemId <= 0) return string.Empty;

            lock (_cacheLock)
            {
                if (_cachedItemSKUs == null || DateTime.Now > _cacheExpiry)
                {
                    _cachedItemSKUs = new Dictionary<int, string>();
                    UpdateCacheExpiry();
                }

                // Use a separate key for UoM to avoid conflicts
                string uomKey = $"UoM_{itemId}";
                int uomKeyHash = uomKey.GetHashCode(); // Simple way to create unique int key

                if (!_cachedItemSKUs.ContainsKey(uomKeyHash))
                {
                    try
                    {
                        _cachedItemSKUs[uomKeyHash] = _itemsRepository.GetItemUnitOfMeasure(itemId);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error caching item UoM for ID {itemId}: {ex.Message}");
                        _cachedItemSKUs[uomKeyHash] = "units"; // Fallback
                    }
                }

                return _cachedItemSKUs[uomKeyHash];
            }
        }
        private bool HasValidEmailAddress(ContactToRemindWithItems contact)
        {
            // QUICK WIN: Use optimized version
            return HasValidEmailAddressOptimized(contact);
        }

        /// <summary>
        /// QUICK WIN: Pre-warm lookup caches to reduce database calls during processing
        /// </summary>
        private void WarmupCaches()
        {
            try
            {
                var warmupStopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Warm up the most commonly used caches
                GetCachedInternalCustomerIds();

                // Pre-cache commonly used item types (coffee service types)
                var coffeeItems = _itemsRepository.GetItemIdsByServiceType(SystemConstants.ServiceTypeConstants.Coffee);
                coffeeItems.AddRange(_itemsRepository.GetItemIdsByServiceType(SystemConstants.ServiceTypeConstants.GroupItem));

                foreach (var itemId in coffeeItems.Take(20)) // Cache top 20 most common items
                {
                    GetCachedItemDescription(itemId);
                    GetCachedItemSKU(itemId);
                }

                warmupStopwatch.Stop();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Cache warmup completed in {warmupStopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Cache warmup failed: {ex.Message}");
                // Don't fail the process if cache warmup fails
            }
        }

        // Helper: pick the effective reminder date your logic uses
        // Helper: pick the effective reminder date your logic uses
        private DateTime GetEffectiveReminderDate(ContactToRemindWithItems c)
        {
            var min = SystemConstants.DatabaseConstants.SystemMinDate;

            if (c.NextDeliveryDate > min)
                return c.NextDeliveryDate.Date;

            if (c.NextCoffee > min)
                return c.NextCoffee.Date;

            // Fallback: today
            return TimeZoneUtils.Now().Date;
        }
        /// <summary>
        /// Validates if a customer is eligible for reminders
        /// </summary>
        /// <param name="customer">Customer to validate</param>
        /// <returns>True if eligible, false otherwise</returns>
        public bool ValidateCustomerEligibility(ContactToRemindWithItems customer)
        {
            try
            {
                // Check if internal customer
                if (IsInternalCustomer((int)customer.CustomerID))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Customer {customer.CompanyName} is internal - skipping");
                    return false;
                }

                // Check if has valid email
                if (!HasValidEmailAddress(customer))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Customer {customer.CompanyName} has no valid email address");
                    return false;
                }

                // Check if within reminder limits
                if (!IsEligibleForReminder(customer))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Customer {customer.CompanyName} not eligible for reminder (disabled or exceeded limits)");
                    return false;
                }

                try
                {
                    var effectiveDate = GetEffectiveReminderDate(customer);
                    var custMgr = new CustomerManager();

                    if (custMgr.IsCustomerAwayOnDate(customer.CustomerID, effectiveDate))
                    {
                        AppLogger.WriteLog(
                            SystemConstants.LogTypes.SendCheckup,
                            $"Excluded {FormatContactDisplayName(customer)} - away on {effectiveDate:yyyy-MM-dd}"
                        );

                        // Mark as NOT SENT in the reminder log (consistent with your failures)
                        string orderType = GetOrderType(customer); // your existing helper
                        LogReminderAttempt(customer, orderType, wasSuccessful: false);

                        return false;
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        $"Away check failed for {FormatContactDisplayName(customer)}: {ex.Message}");
                    // fall through to remaining rules
                }
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Error validating customer {customer.CompanyName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets list of customers eligible for reminders
        /// </summary>
        private List<ContactToRemindWithItems> GetEligibleCustomers()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Get cached internal customer list once
                var internalCustomerIds = GetCachedInternalCustomerIds();

                // Use existing method but with optimizations
                var allContacts = _tempCoffeeCheckupRepository.GetAllContactAndItems();

                // Filter in memory instead of multiple DB calls per customer
                var eligibleContacts = allContacts.Where(contact =>
                    contact.enabled &&
                    contact.ReminderCount < SystemConstants.CheckupConstants.MaxReminders &&
                    !internalCustomerIds.Contains((int)contact.CustomerID) &&
                    HasValidEmailAddressOptimized(contact)
                ).ToList();

                stopwatch.Stop();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Customer filtering completed in {stopwatch.ElapsedMilliseconds}ms - {eligibleContacts.Count} of {allContacts.Count} eligible");

                return eligibleContacts;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Error getting eligible customers: {ex.Message}");
                return new List<ContactToRemindWithItems>();
            }
        }

        /// <summary>
        /// QUICK WIN: Optimized email validation without string concatenation
        /// QUICK WIN: Optimized email validation without string concatenation
        /// </summary>
        private bool HasValidEmailAddressOptimized(ContactToRemindWithItems contact)
        {
            return (!string.IsNullOrWhiteSpace(contact.EmailAddress) && contact.EmailAddress.Contains("@")) ||
               (!string.IsNullOrWhiteSpace(contact.AltEmailAddress) && contact.AltEmailAddress.Contains("@"));
        }

        /// <summary>
        /// QUICK WIN: Cached internal customer IDs to avoid repeated database calls
        /// </summary>
        private List<int> GetCachedInternalCustomerIds()
        {
            lock (_cacheLock)
            {
                if (_cachedInternalCustomerIds == null || DateTime.Now > _cacheExpiry)
                {
                    try
                    {
                        _cachedInternalCustomerIds = _sysDataRepository.GetInternalContactIds();
                        UpdateCacheExpiry();
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Cached {_cachedInternalCustomerIds.Count} internal customer IDs");
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error caching internal customer IDs: {ex.Message}");
                        _cachedInternalCustomerIds = new List<int>(); // Empty list as fallback
                    }
                }
                return _cachedInternalCustomerIds;
            }
        }

        // Helper methods moved from code-behind
        private bool IsInternalCustomer(int customerId)
        {
            try
            {
                // QUICK WIN: Use cached list instead of database call
                var internalCustomerIds = GetCachedInternalCustomerIds();
                return internalCustomerIds.Contains(customerId);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"Error checking internal customer status: {ex.Message}");
                return false;
            }
        }
        private bool IsEligibleForReminder(ContactToRemindWithItems contact)
        {
            return contact.enabled && contact.ReminderCount < SystemConstants.CheckupConstants.MaxReminders;
        }
        /// <summary>
        /// Sets up the list of contacts to send reminders to - FIXED TO PREVENT MIXING RECURRING AND REMINDER CUSTOMERS
        /// </summary>
        private void SetListOfContactsToSendReminderTo(int reminderWindowDays)
        {
            try
            {
                // Step 1: Get all customers with any active reoccurring order
                var recurringCustomerIds = GetAllRecurringOrderCustomerIds();
                //AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Found {recurringCustomerIds.Count} customers with any active recurring orders - they will be excluded from reminder processing");

                // Step 2: Get recurring contacts that are actually due (for display, etc.)
                List<ContactToRemindWithItems> recurringContacts = GetRecurringContactsNeedingReminder(reminderWindowDays);

                // Step 3: Get reminder contacts, excluding all recurring customers
                List<ContactToRemindWithItems> reminderContacts = new List<ContactToRemindWithItems>();
                AddAllContactsToRemind(ref reminderContacts, recurringCustomerIds, reminderWindowDays);

                // Step 4: Combine the lists (recurring + reminder, but no overlap)
                List<ContactToRemindWithItems> allContacts = new List<ContactToRemindWithItems>();
                allContacts.AddRange(recurringContacts);
                allContacts.AddRange(reminderContacts);

                allContacts.Sort((a, b) => string.Compare(a.CompanyName, b.CompanyName));

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Final contact list - {recurringContacts.Count} recurring customers, {reminderContacts.Count} reminder customers, {allContacts.Count} total");

                TempCoffeeCheckupRepository tempCoffeeCheckup = _tempCoffeeCheckupRepository;
                if (!tempCoffeeCheckup.DeleteAllContactRecords() || !tempCoffeeCheckup.DeleteAllContactItems())
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "CoffeeCheckupManager: Error deleting old temp tables");
                    throw new InvalidOperationException("Error deleting old temp tables");
                }

                List<int> idsofServiceType = _itemsRepository.GetItemIdsByServiceType(SystemConstants.ServiceTypeConstants.Coffee);
                idsofServiceType.AddRange(_itemsRepository.GetItemIdsByServiceType(SystemConstants.ServiceTypeConstants.GroupItem));

                bool success = false;
                int insertedContacts = 0;
                int skippedContacts = 0;
                for (int index1 = 0; index1 < allContacts.Count; ++index1)
                {
                    var contact = allContacts[index1];
                    bool hasValidItems = false;
                    for (int index2 = 0; index2 < contact.ItemsContactRequires.Count && !hasValidItems; ++index2)
                        hasValidItems = idsofServiceType.Contains(contact.ItemsContactRequires[index2].ItemID);

                    if (!hasValidItems)
                        continue;

                    if (contact.ReminderCount >= SystemConstants.CheckupConstants.MaxReminders)
                    {
                        if (contact.enabled)
                            DisableContactForMaxReminders(contact);
                        skippedContacts++;
                        continue;
                    }

                    try
                    {
                        if (!tempCoffeeCheckup.InsertContacts((ContactToRemindDetails)contact))
                        {
                            skippedContacts++;
                            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                                $"CoffeeCheckupManager: Failed to stage contact {FormatContactDisplayName(contact)}");
                            continue;
                        }

                        insertedContacts++;
                        success = true;
                        foreach (ItemContactRequires itemsContactRequire in contact.ItemsContactRequires)
                            tempCoffeeCheckup.InsertContactItems(itemsContactRequire);

                        // Only announce last cycle for contacts that actually made it onto the prep list
                        if (contact.IsLastRecurringOrder && contact.LastRecurringUntilDate.HasValue)
                        {
                            AddPrepNotice(
                                $"Last recurring cycle for {FormatContactDisplayName(contact)} (until {contact.LastRecurringUntilDate:yyyy-MM-dd}).");
                        }
                    }
                    catch (Exception insertEx)
                    {
                        skippedContacts++;
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                            $"CoffeeCheckupManager: Error staging contact {FormatContactDisplayName(contact)}: {insertEx.Message}");
                    }
                }

                if (!success)
                {
                    throw new InvalidOperationException("Not all records added to Temp Table");
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"CoffeeCheckupManager: Staged {insertedContacts} contacts for reminder data (skipped {skippedContacts}; candidates {allContacts.Count})");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error in SetListOfContactsToSendReminderTo: {ex.Message}");
                throw;
            }
        }
        private HashSet<long> GetAllRecurringOrderCustomerIds()
        {
            return new HashSet<long>(_recurringOrdersRepository.GetEnabledContactIds().Select(id => (long)id));
        }
        /// <summary>
        /// Enhanced order conflict detection - checks for any existing orders that would conflict
        /// CORRECTED to use available methods
        /// </summary>
        /// <summary>
        /// True if the contact already has a coffee/group order with RequiredByDate in [checkStart, checkEnd].
        /// For recurring checkup, pass the recurring delivery day only — not the whole prep window —
        /// so an earlier weekly delivery does not block the next cycle.
        /// </summary>
        private bool HasConflictingOrders(long customerId, int itemId, DateTime checkStartDate, DateTime checkEndDate)
        {
            try
            {
                var orderCheck = _coffeeCheckupRepository;

                // Check for any coffee orders in the date range
                bool hasConflicts = orderCheck.HasCoffeeOrdersInDateRange(customerId, checkStartDate, checkEndDate);

                if (hasConflicts)
                {
                    var orders = orderCheck.GetCoffeeOrdersInDateRange(customerId, checkStartDate, checkEndDate);
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Customer {customerId} has {orders.Count} coffee orders in date range {checkStartDate:yyyy-MM-dd} to {checkEndDate:yyyy-MM-dd}");
                }

                return hasConflicts;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error checking order conflicts for customer {customerId}: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Fetch recurring orders limited by window end (raw, un-normalized).
        /// </summary>
        private List<RecurringCheckupContext> LoadRawRecurringOrders(DateTime windowEnd)
        {
            return _recurringOrdersRepository.GetEnabledSummariesDueByDate(windowEnd)
                .Select(summary => new RecurringCheckupContext { Summary = summary })
                .ToList();
        }

        /// <summary>
        /// Aligns due/overdue recurring items into the active checkup window.
        /// Overdue NextDateRequired (before today) must still fire — snap into the window.
        /// Does NOT advance from DateLastDone to the next future cycle (that would skip overdue).
        /// </summary>
        private List<RecurringCheckupContext> NormalizeAndFilterRecurringOrders(
            List<RecurringCheckupContext> raw,
            DateTime windowStart,
            DateTime windowEnd)
        {
            var result = new List<RecurringCheckupContext>();
            var dateCalculator = new DateCalculator();

            foreach (var order in raw)
            {
                if (order?.Summary == null || !order.Summary.ContactID.HasValue)
                    continue;

                DateTime storedNext = order.Summary.NextDateRequired?.Date
                    ?? SystemConstants.DatabaseConstants.SystemMinDate;

                if (storedNext == SystemConstants.DatabaseConstants.SystemMinDate || storedNext > windowEnd)
                    continue;

                bool wasOverdue = storedNext < windowStart;
                DateTime deliveryTarget = wasOverdue ? windowStart : storedNext;

                // Snap to area delivery calendar from the due/overdue target — not from DateLastDone.
                var calc = dateCalculator.CalculateOptimalWeeklyDeliveryDates(
                    order.Summary.ContactID.Value,
                    deliveryTarget);

                order.PrepDate = calc.PrepDate;
                DateTime alignedDelivery = calc.DeliveryDate.Date;

                if (alignedDelivery < windowStart)
                    alignedDelivery = windowStart;

                // Area snap can land past the window; overdue/due items must still fire.
                if (alignedDelivery > windowEnd)
                {
                    AreaDeliveryMatrix.EnsureBuilt();
                    int areaId = _areasRepository.GetAreaIdByContactId(order.Summary.ContactID.Value);
                    var closest = AreaDeliveryMatrix.ChooseClosest(areaId, deliveryTarget);
                    if (closest.HasValue && closest.Value.delivery.Date <= windowEnd)
                    {
                        order.PrepDate = closest.Value.prep.Date;
                        alignedDelivery = closest.Value.delivery.Date;
                        if (alignedDelivery < windowStart)
                            alignedDelivery = windowStart;
                    }
                    else
                    {
                        alignedDelivery = windowStart;
                        order.PrepDate = dateCalculator.CalculatePrepDateFromDelivery(alignedDelivery);
                    }

                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        $"RecurringOverduePullIn: OrderItemID={order.Summary.RecurringOrderItemID} Cust={order.Summary.ContactID} " +
                        $"stored={storedNext:yyyy-MM-dd} aligned into window as {alignedDelivery:yyyy-MM-dd}");
                }

                if (wasOverdue)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        $"RecurringOverdue: OrderItemID={order.Summary.RecurringOrderItemID} Cust={order.Summary.ContactID} " +
                        $"NextDateRequired was {storedNext:yyyy-MM-dd}, firing in window as {alignedDelivery:yyyy-MM-dd}");
                }

                order.Summary.NextDateRequired = alignedDelivery;

                // Persist only when we pull overdue into the window or area-align within the window.
                // Never write a next-cycle jump that would clear an overdue due date.
                if (alignedDelivery != storedNext && alignedDelivery <= windowEnd)
                {
                    _recurringOrdersRepository.UpdateItemNextDateRequired(
                        order.Summary.RecurringOrderItemID,
                        alignedDelivery);
                }

                if (alignedDelivery >= windowStart && alignedDelivery <= windowEnd)
                    result.Add(order);
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                $"NormalizeAndFilterRecurringOrders: {result.Count}/{raw.Count} retained in window {windowStart:yyyy-MM-dd}->{windowEnd:yyyy-MM-dd}");

            return result;
        }

        /// <summary>
        /// Builds/updates ContactToRemindWithItems objects from normalized recurring orders.
        /// Handles expiry, conflicts, matrix mapping, closure adjustment.
        /// </summary>
        private List<ContactToRemindWithItems> BuildRecurringContacts(
            List<RecurringCheckupContext> validOrders,
            DateTime windowStart,
            DateTime windowEnd,
            DateTime minReminderDate)
        {
            var contacts = new List<ContactToRemindWithItems>();

            foreach (var order in validOrders)
            {
                try
                {
                    var summary = order.Summary;
                    long contactId = summary.ContactID ?? 0;

                    // 1980-01-01 / null / 2099+ = forever — never treat as a real until date
                    if (HasRealRequireUntilDate(summary.RequireUntilDate)
                        && summary.NextDateRequired.HasValue
                        && summary.NextDateRequired.Value.Date > summary.RequireUntilDate.Value.Date)
                    {
                        string company = !string.IsNullOrWhiteSpace(summary.CompanyName)
                            ? summary.CompanyName.Trim()
                            : "Unknown contact";
                        _recurringOrdersRepository.DisableRecurringOrder(summary.RecurringOrderID);
                        string notice =
                            $"Disabled recurring order for {company} — next date past until date ({summary.RequireUntilDate:yyyy-MM-dd}).";
                        AddPrepNotice(notice);
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                            $"BuildRecurringContacts: {notice}");
                        continue;
                    }

                    var contact = contacts.FirstOrDefault(c => c.CustomerID == contactId);
                    if (contact == null)
                    {
                        contact = _coffeeCheckupRepository.GetCustomerDetails(contactId);
                        if (contact == null)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                                $"BuildRecurringContacts: Missing contact details for ContactID={contactId}");
                            continue;
                        }
                        contacts.Add(contact);
                    }

                    if (contact.ReminderCount >= SystemConstants.CheckupConstants.MaxReminders)
                    {
                        if (contact.enabled)
                            DisableContactForMaxReminders(contact);
                        contacts.Remove(contact);
                        continue;
                    }

                    int areaId = contact.AreaID > 0 ? contact.AreaID : _areasRepository.GetAreaIdByContactId(contactId);
                    var closest = AreaDeliveryMatrix.ChooseClosest(areaId, summary.NextDateRequired ?? windowStart);
                    if (closest.HasValue)
                    {
                        var chosenDelivery = closest.Value.delivery;
                        var chosenPrep = closest.Value.prep;

                        if (chosenDelivery != summary.NextDateRequired?.Date)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                                $"MatrixAdjust: Cust={contactId} OrigDel={summary.NextDateRequired:yyyy-MM-dd} -> {chosenDelivery:yyyy-MM-dd}");
                            summary.NextDateRequired = chosenDelivery;
                        }
                        order.PrepDate = chosenPrep;
                    }

                    // Re-check expiry after matrix alignment (delivery may have moved)
                    if (HasRealRequireUntilDate(summary.RequireUntilDate)
                        && summary.NextDateRequired.HasValue
                        && summary.NextDateRequired.Value.Date > summary.RequireUntilDate.Value.Date)
                    {
                        string company = FormatContactDisplayName(contact);
                        _recurringOrdersRepository.DisableRecurringOrder(summary.RecurringOrderID);
                        string notice =
                            $"Disabled recurring order for {company} — next date past until date ({summary.RequireUntilDate:yyyy-MM-dd}).";
                        AddPrepNotice(notice);
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                            $"BuildRecurringContacts: {notice}");
                        if (contact.ItemsContactRequires.Count == 0)
                            contacts.Remove(contact);
                        continue;
                    }

                    contact.NextDeliveryDate = summary.NextDateRequired?.Date < minReminderDate
                        ? minReminderDate
                        : summary.NextDateRequired?.Date ?? minReminderDate;

                    contact.NextPreparationDate = order.PrepDate < minReminderDate
                        ? minReminderDate
                        : order.PrepDate;

                    ApplyClosureAdjustment(contact);

                    // Conflict only if an order is already booked for THIS recurring delivery day.
                    // Do not treat earlier orders in the checkup window (e.g. this week's delivery)
                    // as blocking next week's recurring cycle.
                    DateTime deliveryDay = contact.NextDeliveryDate.Date;
                    if (deliveryDay < windowStart)
                        deliveryDay = windowStart;

                    bool isLastCycle = _recurringOrdersRepository.IsFinalOccurrenceBeforeUntil(summary);
                    bool hasConflict = HasConflictingOrders(contactId, summary.ItemRequiredID ?? 0, deliveryDay, deliveryDay);

                    if (hasConflict && !isLastCycle)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                            $"BuildRecurringContacts: Conflict skip OrderItemID={summary.RecurringOrderItemID} Cust={contactId} delivery={deliveryDay:yyyy-MM-dd}");
                        // Drop contact if we added it only for this skipped item and it has no items yet
                        if (contact.ItemsContactRequires.Count == 0)
                            contacts.Remove(contact);
                        continue;
                    }

                    if (hasConflict && isLastCycle)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                            $"BuildRecurringContacts: Last cycle for {FormatContactDisplayName(contact)} kept despite existing order on {deliveryDay:yyyy-MM-dd} (email will still send; order create skipped if conflict remains)");
                        contact.SkipOrderCreationDueToConflict = true;
                    }

                    contact.ItemsContactRequires.Add(new ItemContactRequires
                    {
                        CustomerID = contactId,
                        AutoFulfill = false,
                        RecurringOrderItemID = summary.RecurringOrderItemID,
                        RecurringOrder = true,
                        ItemID = summary.ItemRequiredID ?? 0,
                        ItemQty = summary.QtyRequired ?? 0.0,
                        ItemPackagID = summary.ItemPackagingID ?? 0
                    });

                    if (isLastCycle)
                    {
                        contact.IsLastRecurringOrder = true;
                        if (HasRealRequireUntilDate(summary.RequireUntilDate))
                            contact.LastRecurringUntilDate = summary.RequireUntilDate.Value.Date;
                    }

                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        $"BuildRecurringContacts: {FormatContactDisplayName(contact)} Delivery={contact.NextDeliveryDate:yyyy-MM-dd}");
                }
                catch (Exception ex)
                {
                    string company = order.Summary != null && !string.IsNullOrWhiteSpace(order.Summary.CompanyName)
                        ? order.Summary.CompanyName
                        : "Unknown contact";
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        $"BuildRecurringContacts: Error for {company}: {ex.Message}");
                }
            }

            return contacts;
        }
        private List<ContactToRemindWithItems> GetRecurringContactsNeedingReminder(int reminderWindowDays)
        {
            var minReminderDate = _sysDataRepository.GetMinReminderDate();
            var windowStart = TimeZoneUtils.Now().Date;
            var windowEnd = windowStart.AddDays(reminderWindowDays);

            AreaDeliveryMatrix.EnsureBuilt();

            var rawOrders = LoadRawRecurringOrders(windowEnd);
            if (rawOrders.Count == 0)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    "GetRecurringContactsNeedingReminder: No recurring orders raw");
                return new List<ContactToRemindWithItems>();
            }

            var normalized = NormalizeAndFilterRecurringOrders(rawOrders, windowStart, windowEnd);
            var contacts = BuildRecurringContacts(normalized, windowStart, windowEnd, minReminderDate);

            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                $"GetRecurringContactsNeedingReminder: Final recurring contacts {contacts.Count}");
            return contacts;
        }
        // Add near other private helpers
        private bool ApplyClosureAdjustment(ContactToRemindWithItems contact)
        {
            var origPrep = contact.NextPreparationDate;
            var origDel = contact.NextDeliveryDate;

            var adj = _holidayProvider.AdjustPair(origPrep, origDel);
            if (!adj.WasAdjusted) return false;

            contact.NextPreparationDate = adj.Prep;
            contact.NextDeliveryDate = adj.Delivery;

            // Append to contact notes (if not already)
            if (string.IsNullOrEmpty(contact.Notes))
                contact.Notes = adj.Reason;
            else if (!contact.Notes.Contains(adj.Reason))
                contact.Notes += " | " + adj.Reason;

            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                $"Closure adjust (checkup) CustID={contact.CustomerID} Prep {origPrep:yyyy-MM-dd}->{adj.Prep:yyyy-MM-dd} Delivery {origDel:yyyy-MM-dd}->{adj.Delivery:yyyy-MM-dd} {adj.Reason}");

            return true;
        }
        private string BuildClosureNote(ContactToRemindWithItems c)
        {
            try
            {
                var closures = _holidayProvider.GetRange(TimeZoneUtils.Now().Date, c.NextDeliveryDate);
                if (closures == null || closures.Count == 0) return string.Empty;

                var parts = new System.Text.StringBuilder();
                for (int i = 0; i < closures.Count; i++)
                {
                    var h = closures[i];
                    if (i > 0) parts.Append(", ");
                    parts.Append(h.ClosureDate.ToString("dd MMM"));
                    if (!string.IsNullOrEmpty(h.Description))
                        parts.Append(" (" + h.Description + ")");
                }

                return MessageProvider.Format(
                    MessageKeys.CoffeeCheckup.UpcomingClosures,
                    parts.ToString());
            }
            catch { return string.Empty; }
        }
        // --- Helper Methods ---
        //private DateTime CalculateNextDateRequired(ReoccuringOrderExtData order, DeliveryDateCalculator deliveryDateCalculator)
        //{
        //    var recurrenceType = order.ReoccuranceTypeID; // ReoccuranceTypeTbl.GetRecurrenceType(order.ReoccuranceTypeID);
        //    switch (recurrenceType)
        //    {
        //        case ReoccuranceTypeTbl.CONST_WEEKTYPEID:
        //            return order.DateLastDone.AddDays(order.ReoccuranceValue * 7).Date;

        //        case ReoccuranceTypeTbl.CONST_DAYOFMONTHID:
        //            DateTime nextMonth = order.DateLastDone.AddMonths(1);
        //            try
        //            {
        //                return deliveryDateCalculator.CalculateOptimalWeeklyDeliveryDate(
        //                    order.CustomerID,
        //                    DeliveryDateCalculator.WEEKLY_INTERVAL,
        //                    new DateTime(nextMonth.Year, nextMonth.Month, order.ReoccuranceValue).Date);
        //            }
        //            catch (ArgumentOutOfRangeException)
        //            {
        //                int daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        //                int targetDay = Math.Min(order.ReoccuranceValue, daysInMonth);
        //                return new DateTime(nextMonth.Year, nextMonth.Month, targetDay).Date;
        //            }
        //        default:
        //            throw new NotSupportedException($"Unsupported recurrence type {recurrenceType}");
        //    }
        //}
        private bool IsRecentlyProcessed(RecurringCheckupContext order)
        {
            var summary = order?.Summary;
            if (summary == null)
            {
                return false;
            }

            int recurrenceType = summary.RecurringTypeID ?? 0;
            DateTime lastDone = summary.DateLastDone ?? SystemConstants.DatabaseConstants.SystemMinDate;
            int daysSinceLastProcessed = (TimeZoneUtils.Now().Date - lastDone).Days;
            int minimumDays = 0;

            if (recurrenceType == 1)
            {
                minimumDays = (summary.Value ?? 1) * 7;
            }
            else if (recurrenceType == 5)
            {
                minimumDays = GetMinimumRecurringDays();
            }

            bool result = minimumDays > 0 && daysSinceLastProcessed < minimumDays;
            if (result)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Skipping recurring item {summary.RecurringOrderItemID} - processed only {daysSinceLastProcessed} days ago (minimum: {minimumDays})");
            }

            return result;
        }

        /// <summary>
        /// Adds all contacts that may need reminders - ENHANCED TO EXCLUDE RECURRING CUSTOMERS
        /// </summary>
        private void AddAllContactsToRemind(ref List<ContactToRemindWithItems> pContactsToRemind, HashSet<long> excludeCustomerIds, int reminderWindowDays)
        {
            try
            {
                List<ContactMayNeedReminder> thatMayNeedNextWeek = _coffeeCheckupRepository.GetContactsThatMayNeedNextWeek(reminderWindowDays);

                // Initialize exclusion set if not provided
                excludeCustomerIds = excludeCustomerIds ?? new HashSet<long>();

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Processing {thatMayNeedNextWeek.Count} contacts that may need items next week (excluding {excludeCustomerIds.Count} recurring customers)");

                for (int index1 = 0; index1 < thatMayNeedNextWeek.Count; ++index1)
                {
                    try
                    {
                        var candidate = thatMayNeedNextWeek[index1];

                        // BUG FIX: Skip customers that have recurring orders
                        if (excludeCustomerIds.Contains(candidate.ContactID))
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Skipping {candidate.CompanyName} - customer has recurring orders");
                            continue;
                        }

                        List<ContactTrackedServiceItem> byCustomerTypeId =
                            _contactTrackedServiceItemsRepository.GetByContactTypeId(candidate.ContactTypeID);

                        // Build contact info
                        ContactToRemindWithItems toRemindWithItems = new ContactToRemindWithItems
                        {
                            CustomerID = candidate.ContactID,
                            CompanyName = candidate.CompanyName,
                            ContactFirstName = candidate.ContactFirstName,
                            ContactAltFirstName = candidate.ContactAltFirstName,
                            EmailAddress = candidate.EmailAddress,
                            AltEmailAddress = candidate.AltEmailAddress,
                            AreaID = candidate.AreaID,
                            CustomerTypeID = candidate.ContactTypeID,
                            enabled = candidate.Enabled,
                            EquipTypeID = candidate.EquipTypeID,
                            TypicallySecToo = candidate.TypicallySecToo,
                            PreferredAgentID = candidate.PreferredAgentID,
                            SalesAgentID = candidate.SalesAgentID,
                            UsesFilter = candidate.UsesFilter,
                            AlwaysSendChkUp = candidate.AlwaysSendChkUp,
                            RequiresPurchOrder = candidate.RequiresPurchOrder,
                            ReminderCount = candidate.ReminderCount,
                            autofulfill = candidate.AutoFulfill,
                            NextPreparationDate = candidate.PrepDate.Date,
                            NextDeliveryDate = candidate.DeliveryDate.Date,
                            NextCoffee = candidate.NextCoffeeBy.Date,
                            NextClean = candidate.NextCleanOn.Date,
                            NextDescal = candidate.NextDescaleEst.Date,
                            NextFilter = candidate.NextFilterEst.Date,
                            NextService = candidate.NextServiceEst.Date
                        };

                        // Process service items for this customer - ONLY LAST ORDERED ITEMS (no recurring)
                        bool addedAnyItems = false;

                        for (int index2 = 0; index2 < byCustomerTypeId.Count; ++index2)
                        {
                            DateTime serviceDate;
                            switch (byCustomerTypeId[index2].ItemServiceTypeID)
                            {
                                case 1: serviceDate = toRemindWithItems.NextClean; break;
                                case 2: serviceDate = toRemindWithItems.NextCoffee; break;
                                case 4: serviceDate = toRemindWithItems.NextDescal; break;
                                case 5: serviceDate = toRemindWithItems.NextFilter; break;
                                case 10: serviceDate = toRemindWithItems.NextService; break;
                                default: serviceDate = DateTime.MaxValue; break;
                            }

                            // Check if service is due within delivery window
                            if (serviceDate > TimeZoneUtils.Now().AddYears(-1) &&
                                serviceDate <= candidate.DeliveryDate)
                            {
                                List<ContactsItemUsage> lastItemsUsed = _contactsItemUsageRepository.GetLastItemsUsed(
                                    (int)candidate.ContactID,
                                    byCustomerTypeId[index2].ItemServiceTypeID);

                                // Add items this customer typically uses - ONLY LAST ORDERED ITEMS
                                for (int index3 = 0; index3 < lastItemsUsed.Count; ++index3)
                                {
                                    ItemContactRequires itemRequired = new ItemContactRequires
                                    {
                                        CustomerID = candidate.ContactID,
                                        AutoFulfill = candidate.AutoFulfill,
                                        RecurringOrderItemID = 0, // NOT a recurring item
                                        RecurringOrder = false, // NOT a recurring order
                                        ItemID = lastItemsUsed[index3].ItemProvidedID ?? 0,
                                        ItemQty = lastItemsUsed[index3].QtyProvided ?? 0.0,
                                        ItemPackagID = lastItemsUsed[index3].ItemPackagingID ?? 0
                                    };

                                    toRemindWithItems.ItemsContactRequires.Add(itemRequired);
                                    addedAnyItems = true;
                                }
                            }
                        }

                        // Only add customer if they have items
                        if (addedAnyItems)
                        {
                            pContactsToRemind.Add(toRemindWithItems);
                            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Added reminder customer {toRemindWithItems.CompanyName} with {toRemindWithItems.ItemsContactRequires.Count} last-ordered items");
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error processing contact {index1}: {ex.Message}");
                        // Continue with next contact
                    }
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Final reminder contact list has {pContactsToRemind.Count} customers (excluding recurring customers)");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error in AddAllContactsToRemind: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends a batch of reminders for contacts of the same type - moved from code-behind
        /// </summary>
        private BatchSendResult SendReminderBatch(List<ContactToRemindWithItems> contacts, string batchType, SendCheckEmailTexts emailData)
        {
            var result = new BatchSendResult();

            try
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Processing {batchType} batch with {contacts.Count} contacts");

                // Track individual email attempts
                int emailsAdded = 0;
                int emailsFailed = 0;

                // Add all contacts to the batch
                foreach (var contact in contacts)
                {
                    try
                    {
                        var emailTextData = new SendCheckEmailTexts
                        {
                            Header = UrlTextDecoder.DecodePossiblyUrlEncoded(emailData.Header),
                            Body = UrlTextDecoder.DecodePossiblyUrlEncoded(emailData.Body),
                            Footer = UrlTextDecoder.DecodePossiblyUrlEncoded(emailData.Footer)
                        };

                        // Handle order creation for non-reminder contacts
                        string orderType = GetOrderType(contact);
                        if (!string.IsNullOrWhiteSpace(orderType))
                        {
                            var testEmailClient = new EmailMailKitCls();
                            bool isTestMode = testEmailClient.IsTestMode;

                            string orderResult = CreateOrderForContact(contact, orderType, out bool hasAutoFulfill, out bool hasRecurring);
                            if (!string.IsNullOrEmpty(orderResult))
                            {
                                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Order creation failed for {contact.CompanyName}: {orderResult}");
                                // Continue with email even if order creation fails
                            }
                            else
                            {
                                emailTextData.Footer = AppendOrderDetailsToFooter(contact, emailTextData.Footer); 
                                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Order created successfully for {contact.CompanyName}");
                            }
                        }

                        // Add final warning if needed
                        if (contact.ReminderCount == 6)
                        {
                            emailTextData.Body = MessageProvider.Get(MessageKeys.CoffeeCheckup.BodyFinalWarning) + emailTextData.Body;
                        }

                        ApplyLastRecurringOrderFlag(contact);
                        if (contact.IsLastRecurringOrder)
                        {
                            string untilText = contact.LastRecurringUntilDate.HasValue
                                ? contact.LastRecurringUntilDate.Value.ToString("dd MMM yyyy")
                                : "soon";
                            emailTextData.Body =
                                MessageProvider.Format(MessageKeys.CoffeeCheckup.BodyLastRecurringOrder, untilText)
                                + emailTextData.Body;
                        }

                        string emailSubject = _emailManager.GetEmailSubject(orderType);
                        var closureNote = BuildClosureNote(contact);
                        if (!string.IsNullOrEmpty(closureNote))
                        {
                            emailTextData.Footer += "<br/><em>" + closureNote + "</em>";
                        }
                        if (!string.IsNullOrEmpty(contact.Notes) && contact.Notes.IndexOf("Adjusted for closure", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            string adjustedDatesLabel = MessageProvider.Format(
                                MessageKeys.CoffeeCheckup.AdjustedDatesLabel,
                                contact.NextPreparationDate.ToString("yyyy-MM-dd"),
                                contact.NextDeliveryDate.ToString("yyyy-MM-dd"));

                            emailTextData.Footer += "<br/><strong>" + adjustedDatesLabel + "</strong>";
                        }
                        _emailManager.AddEmailToBatch(contact, emailTextData, orderType, emailSubject);
                        emailsAdded++;

                        // Log the reminder attempt
                        LogReminderAttempt(contact, orderType, true);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Failed to add {contact.CompanyName} to batch: {ex.Message}");
                        LogFailedEmail(contact.CompanyName, ex.Message);
                        emailsFailed++;
                        LogReminderAttempt(contact, GetOrderType(contact), false);
                    }
                }

                // Send the entire batch
                if (emailsAdded > 0)
                {
                    var batchResult = _emailManager.SendBatch();

                    if (batchResult.IsSuccess)
                    {
                        result.TotalSent = emailsAdded;
                        result.TotalFailed = emailsFailed;
                    }
                    else
                    {
                        // If batch failed, all emails failed
                        result.TotalSent = 0;
                        result.TotalFailed = emailsAdded + emailsFailed;
                        LogFailedBatch(batchType, batchResult.ErrorMessage);
                    }
                }
                else
                {
                    result.TotalSent = 0;
                    result.TotalFailed = emailsFailed;
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: {batchType} batch result: {result.TotalSent} sent, {result.TotalFailed} failed");

                return result;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error in {batchType} batch processing: {ex.Message}");
                LogFailedBatch(batchType, ex.Message);
                return new BatchSendResult
                {
                    TotalSent = 0,
                    TotalFailed = contacts.Count,
                    ErrorMessage = ex.Message
                };
            }
        }

        private static string AppendOrderDetailsToFooter(ContactToRemindWithItems contact, string emailFooter)
        {
            emailFooter += MessageProvider.Get(MessageKeys.CoffeeCheckup.FooterOrderAdded);

            string baseUrl = DisableClientManager.GetApplicationUrl() ?? string.Empty;

            // Normalize trailing slash once
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            // Build URL robustly
            var ub = new UriBuilder(baseUrl + "Pages/ViewMyOrder.aspx");
            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["CustomerID"] = contact.CustomerID.ToString();
            qs["DeliveryDate"] = contact.NextDeliveryDate.ToString("yyyy-MM-dd");

            // Token (customer + delivery date in UTC) ? encode only the token value
            string token = OrderViewTokenHelper.CreateCustomerDeliveryToken(
                contact.CustomerID,
                contact.NextDeliveryDate.ToUniversalTime());
            qs["t"] = token; // UriBuilder will encode when ToString() is called

            ub.Query = qs.ToString();
            string orderLink = ub.ToString();

            // Inject into footer (MessageProvider template should contain a {0})
            emailFooter += string.Format(
                MessageProvider.Get(MessageKeys.CoffeeCheckup.FooterOrderLink),
                orderLink);
            return emailFooter;
        }

        // Centralised creation so defaults / future changes happen in one place.
        private OrderTblData CreateBaseOrder(ContactToRemindWithItems contact,
            DateTime PrepDate,
            DateTime deliveryDate,
            string orderType)
        {
            var notes = string.Format("{0} - Optimal delivery calculated", orderType);
            if (!string.IsNullOrEmpty(contact.Notes))
            {
                notes = contact.Notes + "; " + notes;
                if (notes.Length > 255) // keep within Access TEXT(255) if that is the column type
                    notes = notes.Substring(0, 255);
            }

            return new OrderTblData
            {
                CustomerID = contact.CustomerID,
                OrderDate = TimeZoneUtils.Now().Date,
                PrepDate = PrepDate,
                RequiredByDate = deliveryDate,
                ToBeDeliveredBy = contact.PreferredAgentID < 0 ? 3 : contact.PreferredAgentID,
                Confirmed = false,
                InvoiceDone = false,
                PurchaseOrder = string.Empty,
                Notes = notes
            };
        }
        /// <summary>
        /// Creates orders for contacts with auto-fulfill or recurring items - ENHANCED WITH DELIVERY DATE CALCULATION
        /// </summary>
        private string CreateOrderForContact(ContactToRemindWithItems pContact, string pOrderType, out bool hasAutoFulfillItem, out bool hasRecurringItems)
        {
            hasAutoFulfillItem = false;
            hasRecurringItems = false;

            try
            {
                bool isRecurringBatch = pOrderType.IndexOf("recurring", StringComparison.OrdinalIgnoreCase) >= 0;

                // NEW: Trust dates already set on the contact (from recurring resolution / matrix)
                DateTime optimalPrepDate = pContact.NextPreparationDate.Date;
                DateTime optimalDeliveryDate = pContact.NextDeliveryDate.Date;

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"CreateOrderForContact: Using pre-assigned dates for {FormatContactDisplayName(pContact)} Prep={optimalPrepDate:yyyy-MM-dd} Delivery={optimalDeliveryDate:yyyy-MM-dd} (IsRecurringBatch={isRecurringBatch})");

                // Identify flags (recurring / autofulfill) without changing dates
                for (int i = 0; i < pContact.ItemsContactRequires.Count; i++)
                {
                    if (pContact.ItemsContactRequires[i].RecurringOrder)
                        hasRecurringItems = true;
                    if (pContact.ItemsContactRequires[i].AutoFulfill)
                        hasAutoFulfillItem = true;
                }

                // Safety: do not create a second coffee order for the same delivery day
                // (last-cycle contacts may still be on the list so they get the final email).
                // Recurring DateLastDone / disable-past-until is owned by Order Done, not Send Checkup.
                if (HasConflictingOrders(pContact.CustomerID, 0, optimalDeliveryDate, optimalDeliveryDate))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        $"CreateOrderForContact: Skipping order create for {FormatContactDisplayName(pContact)} — coffee order already exists on {optimalDeliveryDate:yyyy-MM-dd}");
                    return string.Empty;
                }

                // Build base order object (one object reused per line)
                OrderTblData pOrderData = CreateBaseOrder(pContact, optimalPrepDate, optimalDeliveryDate, pOrderType);

                var testEmailClient = new EmailMailKitCls();
                bool isTestMode = testEmailClient.IsTestMode;

                string errorMessage = string.Empty;

                for (int i = 0; i < pContact.ItemsContactRequires.Count && string.IsNullOrEmpty(errorMessage); i++)
                {
                    var line = pContact.ItemsContactRequires[i];
                    pOrderData.ItemTypeID = line.ItemID;
                    pOrderData.QuantityOrdered = line.ItemQty;
                    pOrderData.PackagingID = line.ItemPackagID;
                    pOrderData.PrepTypeID = line.ItemPrepID;

                    errorMessage = _ordersRepository.InsertNewOrderLine(pOrderData) > 0
                        ? string.Empty
                        : "Failed to insert order line";

                    // Recurring dates / until-disable: OrderDoneManager.SyncRecurringOrderLastDone
                }

                return errorMessage;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"CreateOrderForContact: Error creating order for {pContact.CompanyName} (Cust={pContact.CustomerID}): {ex.Message}");
                return ex.Message;
            }
        }
        /// <summary>
        /// Logs reminder attempt to database - CONDITIONAL TEST MODE LOGGING
        /// </summary>
        private void LogReminderAttempt(ContactToRemindWithItems contact, string orderType, bool wasSuccessful)
        {
            try
            {
                var testEmailClient = new EmailMailKitCls();
                bool isTestMode = testEmailClient.IsTestMode;

                bool hasRecurring = contact.ItemsContactRequires.Any(x => x.RecurringOrder);
                bool hasAutoFulFill = contact.ItemsContactRequires.Any(x => x.AutoFulfill);

                var logEntry = new SentRemindersLog
                {
                    ContactID = (int)contact.CustomerID,
                    DateSentReminder = TimeZoneUtils.Now().Date,
                    NextPreparationDate = contact.NextPreparationDate.Date,
                    ReminderSent = wasSuccessful,
                    HadAutoFulfilItem = hasAutoFulFill,
                    HadRecurringItems = hasRecurring
                };

                string logMode = isTestMode ? "[TEST MODE]" : "[PRODUCTION]";
                _sentRemindersLogRepository.InsertLogItem(logEntry);

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"CoffeeCheckupManager: {logMode} Logged reminder for {FormatContactDisplayName(contact)} Prep={contact.NextPreparationDate:yyyy-MM-dd} Sent={wasSuccessful} Recurring={hasRecurring} AutoFulfill={hasAutoFulFill}");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"CoffeeCheckupManager: Failed to log reminder attempt for {FormatContactDisplayName(contact)}: {ex.Message}");
            }
        }
        /// <summary>
        /// Logs individual email failure
        /// </summary>
        private void LogFailedEmail(string customerName, string errorMessage)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: EMAIL FAILED: {customerName} - {errorMessage}");
        }

        /// <summary>
        /// Logs batch failure
        /// </summary>
        private void LogFailedBatch(string batchType, string errorMessage)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: BATCH FAILED: {batchType} - {errorMessage}");
        }

        /// <summary>
        /// Logs failed customers for display in SentRemindersSheet
        /// </summary>
        public void LogFailedCustomers(List<string> failedContacts)
        {
            if (!failedContacts.Any()) return;

            try
            {
                foreach (string failure in failedContacts)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: FAILED CUSTOMER: {failure}");
                }

                // Store in session for SentRemindersSheet to display
                if (HttpContext.Current?.Session != null)
                {
                    HttpContext.Current.Session["CoffeeCheckupFailures"] = failedContacts;
                    HttpContext.Current.Session["CoffeeCheckupFailureDate"] = TimeZoneUtils.Now().Date;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error logging failed customers: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets eligible customers using database-driven approach via OrderCheckTbl control
        /// </summary>
        private List<ContactToRemindWithItems> GetEligibleCustomersFromDatabase()
        {
            try
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "CoffeeCheckupManager: Getting eligible customers using OrderCheckTbl");

                var databaseCustomers = _coffeeCheckupRepository.GetCustomersWithoutOrderConflicts(SystemConstants.CheckupConstants.MaxReminders);

                var eligibleCustomers = new List<ContactToRemindWithItems>();

                foreach (var dbCustomer in databaseCustomers)
                {
                    // Convert database model to contact model
                    var contact = new ContactToRemindWithItems
                    {
                        CustomerID = dbCustomer.CustomerID,
                        CompanyName = dbCustomer.CompanyName,
                        ContactFirstName = dbCustomer.ContactFirstName,
                        ContactAltFirstName = dbCustomer.ContactAltFirstName,
                        EmailAddress = dbCustomer.EmailAddress,
                        AltEmailAddress = dbCustomer.AltEmailAddress,
                        AreaID = dbCustomer.AreaID,
                        CustomerTypeID = dbCustomer.CustomerTypeID,
                        enabled = dbCustomer.Enabled,
                        EquipTypeID = dbCustomer.EquipTypeID,
                        TypicallySecToo = dbCustomer.TypicallySecToo,
                        PreferredAgentID = dbCustomer.PreferredAgentID,
                        SalesAgentID = dbCustomer.SalesAgentID,
                        UsesFilter = dbCustomer.UsesFilter,
                        AlwaysSendChkUp = dbCustomer.AlwaysSendChkUp,
                        ReminderCount = dbCustomer.ReminderCount,
                        NextPreparationDate = dbCustomer.NextPreparationDate,
                        NextDeliveryDate = dbCustomer.NextDeliveryDate,
                        NextCoffee = dbCustomer.NextCoffee,
                        NextClean = dbCustomer.NextClean,
                        NextDescal = dbCustomer.NextDescal,
                        NextFilter = dbCustomer.NextFilter,
                        NextService = dbCustomer.NextService
                    };

                    // Get typical items for this customer
                    var typicalItems = _coffeeCheckupRepository.GetCustomerTypicalItems(dbCustomer.CustomerID);
                    contact.ItemsContactRequires = typicalItems.Select(item => new ItemContactRequires
                    {
                        CustomerID = dbCustomer.CustomerID,
                        AutoFulfill = dbCustomer.AutoFulfill,
                        RecurringOrderItemID = 0,
                        RecurringOrder = false,
                        ItemID = item.ItemID,
                        ItemQty = item.Quantity,
                        ItemPackagID = item.PackagingID
                    }).ToList();

                    eligibleCustomers.Add(contact);
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Database query returned {eligibleCustomers.Count} eligible customers");
                return eligibleCustomers;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Database query failed: {ex.Message}");
                // Return null to trigger fallback to existing method
                return null;
            }
        }

        /// <summary>
        /// Checks if a recurring order is monthly (day-of-month) type
        /// </summary>
        private bool HasMonthlyRecurrence(int recurringOrderItemId)
        {
            try
            {
                var recurringOrder = _recurringOrdersRepository.GetSummaryByRecurringOrderItemId(recurringOrderItemId);
                return recurringOrder?.RecurringTypeID == 5;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error checking monthly recurrence for {recurringOrderItemId}: {ex.Message}");
                return false;
            }
        }

        private int GetTargetDayOfMonth(int recurringOrderItemId)
        {
            try
            {
                var recurringOrder = _recurringOrdersRepository.GetSummaryByRecurringOrderItemId(recurringOrderItemId);
                return recurringOrder?.Value ?? 0;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"CoffeeCheckupManager: Error getting target day for {recurringOrderItemId}: {ex.Message}");
                return 0;
            }
        }

        public bool IsHolidayComingInWindow(int daysWindow)
        {
            return _holidayProvider.IsThereAHolodayComing(TimeZoneUtils.Now().Date, daysWindow);
        }

        /// <summary>
        /// Clears staging tables used by the SendCoffeeCheckup UI (page façade).
        /// </summary>
        public void ClearTempCheckupData()
        {
            _tempCoffeeCheckupRepository.DeleteAllContactRecords();
            _tempCoffeeCheckupRepository.DeleteAllContactItems();
        }

        public List<ContactToRemindDetails> GetPreparedContacts(string sortBy = "CompanyName")
        {
            return _tempCoffeeCheckupRepository.GetAllContacts(sortBy) ?? new List<ContactToRemindDetails>();
        }

        public List<ItemContactRequires> GetPreparedContactItems(long contactId)
        {
            return _tempCoffeeCheckupRepository.GetContactItems(contactId) ?? new List<ItemContactRequires>();
        }

        /// <summary>
        /// Removes a contact from this checkup run only (temp staging). Does not disable the contact.
        /// </summary>
        public bool ExcludePreparedContactThisTime(long contactId)
        {
            if (contactId <= 0) return false;

            string displayName = null;
            try
            {
                var prepared = GetPreparedContacts("CustomerID");
                var match = prepared?.FirstOrDefault(c => c.CustomerID == contactId);
                if (match != null)
                    displayName = FormatContactDisplayName(match);
            }
            catch { /* best-effort name for status/log */ }

            bool removed = _tempCoffeeCheckupRepository.DeleteContact(contactId);
            if (removed)
            {
                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = _contactsRepository.GetContactNameById((int)contactId);
                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = "contact";

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"CoffeeCheckupManager: Excluded {displayName} from this checkup run");
            }
            return removed;
        }

        /// <summary>
        /// Resolves a prepared contact's display name for UI status messages.
        /// </summary>
        public string GetPreparedContactDisplayName(long contactId)
        {
            if (contactId <= 0) return string.Empty;
            try
            {
                var prepared = GetPreparedContacts("CustomerID");
                var match = prepared?.FirstOrDefault(c => c.CustomerID == contactId);
                if (match != null)
                    return FormatContactDisplayName(match);
            }
            catch { }

            return _contactsRepository.GetContactNameById((int)contactId) ?? string.Empty;
        }

        public int GetPreparedContactCount()
        {
            return GetPreparedContacts("CustomerID").Count;
        }

        public List<ContactToRemindWithItems> GetPreparedContactsWithItems()
        {
            return _tempCoffeeCheckupRepository.GetAllContactAndItems() ?? new List<ContactToRemindWithItems>();
        }

        public int ClearTodaysSentReminderEntries()
        {
            return _sentRemindersLogRepository.DeleteTodaysEntries();
        }

        public SendCheckEmailTexts GetEmailTexts()
        {
            return new SendCheckEmailTextsRepository().GetTexts();
        }

        public string UpdateEmailTexts(SendCheckEmailTexts emailTexts, int originalId)
        {
            return new SendCheckEmailTextsRepository().UpdateTexts(emailTexts, originalId);
        }

        /// <summary>
        /// Stats for redirect to SentRemindersSheet after a send run.
        /// </summary>
        public SentReminderDayStats GetSentReminderDayStats(DateTime sentDate)
        {
            var dayResults = _sentRemindersLogRepository.GetAllByDate(sentDate.Date, "ContactID")
                ?? new List<SentRemindersLog>();

            return new SentReminderDayStats
            {
                SentDate = sentDate.Date,
                TotalReminders = _sentRemindersLogRepository.GetEntriesCountForDate(sentDate.Date),
                UniqueCustomers = dayResults.Select(r => r.ContactID).Distinct().Count(),
                Successful = dayResults.Count(r => r.ReminderSent == true),
                Failed = dayResults.Count(r => r.ReminderSent != true)
            };
        }

        public static int GetReminderWindowDays()
        {
            int days = SystemConstants.CheckupConstants.DefaultReminderWindowDays; // CONST_DEFAULTREMINDERWINDOWDAYS; // default

            // Check session first
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                var sessionVal = HttpContext.Current.Session["CoffeeCheckupReminderWindowDays"] as string;
                if (!string.IsNullOrEmpty(sessionVal) && int.TryParse(sessionVal, out int sessionDays) && sessionDays > 0)
                    return sessionDays;
            }

            return ConfigHelper.GetInt("CoffeeCheckupReminderWindowDays", days);
        }

        /// <summary>
        /// Gets the minimum number of days between monthly recurring orders
        /// </summary>
        public static int GetMinimumRecurringDays()
        {
            // Check app settings first
            return ConfigHelper.GetInt("CoffeeCheckupMinMonthlyRecurringDays", SystemConstants.CheckupConstants.DefaultMinimumMonthlyRecurringDays);
        }
        // Add this handler method anywhere inside the SendCoffeeCheckup partial class (e.g. near other button handlers).

    }
}
