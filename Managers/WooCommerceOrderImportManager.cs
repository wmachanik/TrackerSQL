using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Pulls Woo orders and creates/updates Tracker orders + WooOrderInfoTbl companion rows.
    /// </summary>
    public class WooCommerceOrderImportManager
    {
        private readonly WooCommerceSettingsManager _settingsManager = new WooCommerceSettingsManager();
        private readonly WooCommerceSettingsRepository _settingsRepo = new WooCommerceSettingsRepository();
        private readonly WooCommerceApiClient _api = new WooCommerceApiClient();
        private readonly WooCommerceAreaMappingManager _areaManager = new WooCommerceAreaMappingManager();
        private readonly WooOrderInfoRepository _wooOrderRepo = new WooOrderInfoRepository();
        private readonly WooItemMappingRepository _itemMapRepo = new WooItemMappingRepository();
        private readonly WooPaymentMethodMapRepository _paymentMapRepo = new WooPaymentMethodMapRepository();
        private readonly WooCatalogCacheRepository _catalogRepo = new WooCatalogCacheRepository();
        private readonly WooCategoryFilterRepository _categoryRepo = new WooCategoryFilterRepository();
        private readonly ContactsRepository _contactsRepo = new ContactsRepository();
        private readonly OrderManager _orderManager = new OrderManager();
        private readonly OrdersRepository _ordersRepo = new OrdersRepository();
        private readonly ItemsRepository _itemsRepo = new ItemsRepository();
        private readonly ItemPackagingsRepository _packRepo = new ItemPackagingsRepository();
        private readonly ContactsAccInfoRepository _accInfoRepo = new ContactsAccInfoRepository();
        private readonly AreasRepository _areasRepo = new AreasRepository();
        private readonly WooCommerceMappingManager _mappingManager = new WooCommerceMappingManager();

        private HashSet<long> _gearCategoryIds;
        private List<WooCategoryFilter> _categoryFilters;
        private string _categoryFilterMode;
        private WooImportAddressConfig _addressConfig;

        private const string NoteNoItemMap = "No item map";
        private const string NoteCategoryNotImported = "Category not imported";

        private WooImportAddressConfig AddressConfig =>
            _addressConfig ?? (_addressConfig = WooImportAddressHelper.FromSettings(_settingsRepo.GetSettings()));

        public List<WooOrderImportPreviewRow> PullPreview(
            WooOrderImportMode mode,
            long specificOrderId,
            DateTime? rangeFrom,
            DateTime? rangeTo,
            out string error)
        {
            error = null;
            if (!_settingsManager.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out error))
                return new List<WooOrderImportPreviewRow>();

            List<WooOrderDto> orders = FetchOrders(creds, mode, specificOrderId, rangeFrom, rangeTo);
            _gearCategoryIds = LoadGearCategoryIds();

            var previews = orders
                .Select(o => BuildPreview(o))
                .OrderByDescending(r => r.OrderDate ?? DateTime.MinValue)
                .ToList();

            WooCommerceUserLog.Write(
                "Order import pull preview",
                string.Format(CultureInfo.InvariantCulture, "mode={0}, orders={1}", mode, previews.Count));

            return previews;
        }

        public WooOrderImportPreviewRow GetPreviewForOrder(long wooOrderId, out string error)
        {
            error = null;
            if (wooOrderId <= 0)
            {
                error = "Invalid Woo order.";
                return null;
            }

            if (!_settingsManager.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out error))
                return null;

            _gearCategoryIds = LoadGearCategoryIds();
            WooOrderDto order = _api.GetOrder(creds, wooOrderId);
            if (order == null)
            {
                error = "Woo order not found.";
                return null;
            }

            return BuildPreview(order);
        }

        public WooOrderImportBatchResult ImportOne(long wooOrderId, bool updateExisting, string updatedBy)
        {
            if (wooOrderId <= 0)
            {
                return new WooOrderImportBatchResult
                {
                    Failed = 1,
                    Messages = { "Invalid Woo order." }
                };
            }

            return ImportSelected(
                new List<WooOrderImportPreviewRow>
                {
                    new WooOrderImportPreviewRow { WooOrderId = wooOrderId, Selected = true }
                },
                updateExisting,
                updatedBy);
        }

        /// <summary>Create a Tracker contact from Woo shipping/billing, then caller opens Contact Details.</summary>
        public int CreateContactFromWooOrder(long wooOrderId, string updatedBy, out string error)
        {
            error = null;
            if (wooOrderId <= 0)
            {
                error = "Invalid Woo order.";
                return 0;
            }

            if (!_settingsManager.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out error))
                return 0;

            _gearCategoryIds = LoadGearCategoryIds();
            WooOrderDto order = _api.GetOrder(creds, wooOrderId);
            if (order == null)
            {
                error = "Woo order not found.";
                return 0;
            }

            var preview = BuildPreview(order);
            if (preview.UseZzName)
            {
                error = "Gear-only (ZZName) orders do not create contacts.";
                return 0;
            }

            if (preview.MatchedContactId.HasValue && preview.MatchedContactId.Value > 0)
            {
                error = "Contact already exists — use Update contact if shipping details changed.";
                return 0;
            }

            WooAddressDto ship = order.Shipping ?? new WooAddressDto();
            Contact contact = BuildContactFromWoo(order, preview, ship);
            int newId = _contactsRepo.Insert(contact);
            if (newId <= 0)
            {
                error = "Could not save contact.";
                return 0;
            }

            contact.ContactID = newId;
            WooContactBootstrap.EnsureAccInfo(_accInfoRepo, contact, order, newId);

            WooCommerceUserLog.Write(
                string.Format(CultureInfo.InvariantCulture, "Created contact from Woo #{0}", preview.WooOrderNumber),
                FormatContactLogName(contact),
                updatedBy);

            return newId;
        }

        /// <summary>Update matched contact billing address / area / phone from Woo shipping.</summary>
        public int UpdateContactFromWooOrder(long wooOrderId, string updatedBy, out string error)
        {
            error = null;
            if (wooOrderId <= 0)
            {
                error = "Invalid Woo order.";
                return 0;
            }

            if (!_settingsManager.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out error))
                return 0;

            _gearCategoryIds = LoadGearCategoryIds();
            WooOrderDto order = _api.GetOrder(creds, wooOrderId);
            if (order == null)
            {
                error = "Woo order not found.";
                return 0;
            }

            var preview = BuildPreview(order);
            if (preview.UseZzName)
            {
                error = "Gear-only (ZZName) orders do not update contacts.";
                return 0;
            }

            if (!preview.MatchedContactId.HasValue || preview.MatchedContactId.Value <= 0)
            {
                error = "No matched contact — use Add contact first.";
                return 0;
            }

            if (!preview.ContactHasShippingChanges)
            {
                error = "Shipping address, area, and phone already match the contact.";
                return preview.MatchedContactId.Value;
            }

            WooAddressDto ship = order.Shipping ?? new WooAddressDto();
            Contact contact = _contactsRepo.GetById(preview.MatchedContactId.Value);
            if (contact == null)
            {
                error = "Contact not found.";
                return 0;
            }

            var areaForDiff = _areaManager.ResolveArea(ship.Postcode, ship.Suburb, ship.State);
            var changeParts = DescribeShippingChanges(contact, order, ship, areaForDiff?.AreaID);

            ApplyWooShippingToContact(contact, order, ship);
            if (!_contactsRepo.Update(contact))
            {
                error = "Could not update contact.";
                return 0;
            }

            WooContactBootstrap.UpdateAccInfoAddresses(_accInfoRepo, contact, order, contact.ContactID);

            string changeDetail = changeParts.Count > 0
                ? string.Join("; ", changeParts)
                : "shipping details";
            _contactsRepo.AppendSystemNote(contact.ContactID,
                string.Format(CultureInfo.InvariantCulture,
                    "Updated from Woo order #{0}: {1}.",
                    preview.WooOrderNumber,
                    changeDetail));

            WooCommerceUserLog.Write(
                string.Format(CultureInfo.InvariantCulture, "Updated contact from Woo #{0}", preview.WooOrderNumber),
                FormatContactLogName(contact) + " — " + changeDetail,
                updatedBy);

            return contact.ContactID;
        }

        public WooOrderImportBatchResult ImportSelected(
            IList<WooOrderImportPreviewRow> rows,
            bool updateExisting,
            string updatedBy)
        {
            var result = new WooOrderImportBatchResult();
            if (rows == null || rows.Count == 0)
            {
                result.Messages.Add("No orders selected.");
                return result;
            }

            if (!_settingsManager.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out string credError))
            {
                result.Messages.Add(credError ?? "Could not load Woo credentials.");
                return result;
            }

            _gearCategoryIds = LoadGearCategoryIds();
            DateTime? maxImportedDate = null;

            foreach (var row in rows.Where(r => r != null && r.Selected))
            {
                try
                {
                    WooOrderDto order = _api.GetOrder(creds, row.WooOrderId);
                    if (order == null)
                    {
                        result.Failed++;
                        result.Messages.Add("Failed Woo #" + row.WooOrderNumber + " — Woo order not found.");
                        continue;
                    }

                    var preview = BuildPreview(order);
                    preview.Selected = true;

                    if (preview.AlreadyImported && !updateExisting)
                    {
                        result.Skipped++;
                        result.Messages.Add(string.Format(CultureInfo.InvariantCulture,
                            "Skipped Woo #{0} — already imported as Tracker order #{1}.",
                            preview.WooOrderNumber, preview.ExistingTrackerOrderId));
                        continue;
                    }

                    if (!preview.CanImport)
                    {
                        result.Failed++;
                        result.Messages.Add(string.Format(CultureInfo.InvariantCulture,
                            "Failed Woo #{0} — {1}",
                            preview.WooOrderNumber,
                            preview.Warnings.Count > 0 ? preview.Warnings[0] : "cannot import"));
                        continue;
                    }

                    bool wasUpdate = preview.AlreadyImported && updateExisting;
                    int? existingId = wasUpdate ? preview.ExistingTrackerOrderId : null;
                    int orderId = CommitOrder(order, preview, existingId, updatedBy);
                    if (orderId <= 0)
                    {
                        result.Failed++;
                        result.Messages.Add("Failed Woo #" + preview.WooOrderNumber + " — could not save Tracker order.");
                        continue;
                    }

                    if (wasUpdate)
                        result.Updated++;
                    else
                        result.Imported++;

                    result.LastTrackerOrderId = orderId;

                    if (preview.OrderDate.HasValue)
                    {
                        if (!maxImportedDate.HasValue || preview.OrderDate.Value > maxImportedDate.Value)
                            maxImportedDate = preview.OrderDate.Value;
                    }

                    result.Messages.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0} Woo #{1} → Tracker order #{2}.",
                        wasUpdate ? "Updated" : "Imported",
                        preview.WooOrderNumber,
                        orderId));
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Messages.Add("Failed Woo #" + row.WooOrderNumber + " — " + ex.Message);
                    AppLogger.WriteLog("woo", "Order import error Woo #" + row.WooOrderNumber + ": " + ex.Message, updatedBy);
                }
            }

            if (maxImportedDate.HasValue)
                TouchOrdersSyncCursor(maxImportedDate.Value, updatedBy);

            WooCommerceUserLog.Write(
                "Order import batch",
                string.Format(CultureInfo.InvariantCulture,
                    "imported={0}, updated={1}, skipped={2}, failed={3}",
                    result.Imported, result.Updated, result.Skipped, result.Failed),
                updatedBy);

            return result;
        }

        private List<WooOrderDto> FetchOrders(
            WooCommerceApiClient.ApiCredentials creds,
            WooOrderImportMode mode,
            long specificOrderId,
            DateTime? rangeFrom,
            DateTime? rangeTo)
        {
            switch (mode)
            {
                case WooOrderImportMode.Specific:
                    if (specificOrderId <= 0)
                        throw new InvalidOperationException("Enter a Woo order number.");
                    var one = _api.GetOrder(creds, specificOrderId);
                    return one != null ? new List<WooOrderDto> { one } : new List<WooOrderDto>();

                case WooOrderImportMode.Last:
                    return _api.GetLatestOrders(creds, 1);

                case WooOrderImportMode.SinceLastSync:
                    var settings = _settingsManager.GetSettings();
                    DateTime since = settings.LastOrdersSyncUtc ?? DateTime.UtcNow.AddDays(-30);
                    return _api.GetOrdersSince(creds, since);

                case WooOrderImportMode.DateRange:
                    if (!rangeFrom.HasValue || !rangeTo.HasValue)
                        throw new InvalidOperationException("Enter both from and to dates.");
                    return _api.GetOrdersInRange(creds, rangeFrom.Value.ToUniversalTime(), rangeTo.Value.ToUniversalTime());

                default:
                    throw new InvalidOperationException("Unknown import mode.");
            }
        }

        private WooOrderImportPreviewRow BuildPreview(WooOrderDto order)
        {
            if (order == null)
                return null;

            var preview = new WooOrderImportPreviewRow
            {
                WooOrderId = order.Id,
                WooOrderNumber = order.Number ?? order.Id.ToString(CultureInfo.InvariantCulture),
                WooStatus = order.Status,
                OrderDate = order.DateCreated,
                PaymentAbbrev = _paymentMapRepo.ResolveAbbrev(order.PaymentMethod, order.PaymentMethodTitle),
                ShippingMethod = order.ShippingLines.FirstOrDefault()?.MethodTitle ?? string.Empty,
                RawJson = order.RawJson
            };

            var existing = ResolveExistingImportLink(order);
            if (existing != null)
            {
                preview.AlreadyImported = true;
                preview.ExistingTrackerOrderId = existing.OrderID;
            }

            WooAddressDto ship = order.Shipping ?? new WooAddressDto();
            preview.ShippingSummary = ship.FormattedAddress;
            preview.Lines = order.LineItems.Select(li => BuildLinePreview(li)).ToList();

            preview.IsGearOnly = preview.Lines.Count > 0 && preview.Lines.All(l => l.IsGear);
            preview.UseZzName = preview.IsGearOnly;

            ResolveContactPreview(order, preview, ship);
            ResolveDeliveryPreview(order, preview, ship);
            CollectConflicts(order, preview, ship);

            preview.LinesSummary = string.Join("; ", preview.Lines.Select(l =>
            {
                if (l.IsUnmappedForNotes
                    || string.Equals(l.MapType, "Notes", StringComparison.OrdinalIgnoreCase))
                    return string.Format(CultureInfo.InvariantCulture, "{0}× order notes ({1})", l.WooQty, l.Sku ?? l.Name);
                if (l.TrackerItemId.HasValue && l.TrackerItemId.Value > 0)
                    return string.Format(CultureInfo.InvariantCulture, "{0}× {1}", l.TrackerQty, l.TrackerSku ?? l.Sku);
                return (l.Sku ?? l.Name) + " (" + (l.Note ?? "?") + ")";
            }));

            preview.CanImport = preview.Lines.Any(l => l.CanImport)
                && preview.Lines.Any(l => l.CanImport && (
                    l.IsUnmappedForNotes
                    || string.Equals(l.MapType, "Notes", StringComparison.OrdinalIgnoreCase)
                    || (l.TrackerItemId.HasValue && l.TrackerItemId.Value > 0)));
            if (!preview.CanImport && preview.Lines.Count == 0)
                preview.Warnings.Add("Order has no line items.");
            else if (!preview.CanImport)
                preview.Warnings.Add("No importable mapped lines — check item maps or Notes mode.");

            return preview;
        }

        /// <summary>
        /// Live Tracker order linked to this Woo order: WooOrderInfo join, else PurchaseOrder = Woo #.
        /// Orphaned WooOrderInfo rows (Tracker order deleted) are cleared so re-import is allowed.
        /// </summary>
        private WooOrderInfo ResolveExistingImportLink(WooOrderDto order)
        {
            if (order == null || order.Id <= 0)
                return null;

            string wooNumber = (order.Number ?? order.Id.ToString(CultureInfo.InvariantCulture)).Trim();

            WooOrderInfo live = _wooOrderRepo.GetLiveByWooOrderId(order.Id);
            if (live != null)
            {
                if (_ordersRepo.OrderExists(live.OrderID))
                    return live;

                _wooOrderRepo.DeleteByWooOrderId(order.Id);
                _wooOrderRepo.DeleteByOrderId(live.OrderID);
            }
            else
            {
                // Clear any orphaned WooOrderInfo rows that no longer join to OrdersTbl.
                WooOrderInfo orphan = _wooOrderRepo.GetByWooOrderId(order.Id);
                if (orphan != null)
                {
                    _wooOrderRepo.DeleteByWooOrderId(order.Id);
                    if (orphan.OrderID > 0 && !_ordersRepo.OrderExists(orphan.OrderID))
                        _wooOrderRepo.DeleteByOrderId(orphan.OrderID);
                }
            }

            int? byPo = _ordersRepo.FindOrderIdByPurchaseOrder(wooNumber);
            if (!byPo.HasValue || byPo.Value <= 0 || !_ordersRepo.OrderExists(byPo.Value))
                return null;

            // Confirm this PO really is the Woo order number (avoid loose matches).
            var header = _ordersRepo.GetOrderHeaderByOrderId(byPo.Value);
            if (header == null
                || !string.Equals((header.PurchaseOrder ?? string.Empty).Trim(), wooNumber, StringComparison.OrdinalIgnoreCase))
                return null;

            return new WooOrderInfo
            {
                OrderID = byPo.Value,
                WooOrderId = order.Id,
                WooOrderNumber = wooNumber
            };
        }

        private WooOrderLinePreview BuildLinePreview(WooOrderLineDto li)
        {
            var line = new WooOrderLinePreview
            {
                Sku = li.Sku,
                Name = li.Name,
                WooQty = li.Quantity,
                IsGear = IsGearProduct(li.ProductId, li.VariationId)
            };

            bool variantParentItemNotes;
            WooItemMapping map = ResolveLineMap(li, out variantParentItemNotes);
            if (variantParentItemNotes)
            {
                line.MapType = "Notes";
                line.CanImport = true;
                line.Note = "→ order notes (variant)";
                return line;
            }

            if (map == null)
            {
                TryResolveUnmappedLine(li, line);
                return line;
            }

            line.MapType = map.MapType;
            if (map.IsExcludeMap || !map.IncludeInImport)
            {
                line.CanImport = false;
                line.Note = map.IsExcludeMap ? "Excluded" : "Not included in import";
                return line;
            }

            if (map.IsNotesMap)
            {
                line.CanImport = true;
                line.Note = "→ order notes";
                return line;
            }

            if (map.ItemID <= 0)
            {
                line.CanImport = false;
                line.Note = "Notes map — no Tracker item";
                return line;
            }

            var item = _itemsRepo.GetById(map.ItemID);
            line.TrackerItemId = map.ItemID;
            line.TrackerSku = item?.SKU;
            double qtyFactor = map.QtyFactor > 0 ? map.QtyFactor : 1;
            int? packagingId = map.PackagingID.HasValue && map.PackagingID.Value > 0
                ? map.PackagingID
                : null;

            int serviceTypeId = item?.ItemServiceTypeID ?? 0;
            WooLineAttributeResolveResult attr = _mappingManager.ResolveOrderLineAttributes(
                li.MetaData,
                serviceTypeId,
                qtyFactor,
                packagingId);
            if (attr != null)
            {
                if (attr.QtyFactor > 0)
                    qtyFactor = attr.QtyFactor;
                if (attr.PackagingId.HasValue && attr.PackagingId.Value > 0)
                    packagingId = attr.PackagingId;
                if (attr.NoteParts != null && attr.NoteParts.Count > 0)
                    line.AttributeNoteParts = attr.NoteParts;
                if (!string.IsNullOrWhiteSpace(attr.Reason))
                    line.Note = attr.Reason;
            }

            line.TrackerQty = Math.Round(qtyFactor * li.Quantity, SystemConstants.DatabaseConstants.NumDecimalPoints);
            line.PackagingId = packagingId;
            line.CanImport = true;
            return line;
        }

        /// <summary>
        /// Variation lines inherit parent maps for ParentNotes / Exclude; ParentItem variants → notes.
        /// Variants mode requires a per-variation map.
        /// </summary>
        private WooItemMapping ResolveLineMap(WooOrderLineDto li, out bool variantParentItemNotes)
        {
            variantParentItemNotes = false;
            long variationId = li.VariationId > 0 ? li.VariationId : 0;

            WooItemMapping map = null;
            if (variationId > 0)
                map = _itemMapRepo.FindExact(li.ProductId, variationId);

            if (map != null)
                return map;

            WooItemMapping parentMap = _itemMapRepo.FindExact(li.ProductId, null);
            if (parentMap == null)
                return null;

            if (variationId <= 0)
                return parentMap;

            if (parentMap.IsNotesMap || parentMap.IsExcludeMap)
                return parentMap;

            if (parentMap.IsVariantsMap)
                return null;

            if (parentMap.ItemID > 0)
            {
                variantParentItemNotes = true;
                return null;
            }

            return null;
        }

        private void ResolveContactPreview(WooOrderDto order, WooOrderImportPreviewRow preview, WooAddressDto ship)
        {
            if (preview.UseZzName)
            {
                preview.IsNewContact = false;
                preview.CanAddContact = false;
                preview.CanUpdateContact = false;
                preview.ContactHasShippingChanges = false;
                preview.ContactStatusLabel = string.Empty;
                string email = FirstNonEmpty(ship.Email, order.Billing?.Email);
                preview.ContactDisplayName = GetWooContactName(order, ship);
                preview.ContactSummary = "ZZName — " + BuildZzNameNotePrefix(ship.Company, ship.FullName, email);
                return;
            }

            Contact matched = FindMatchedContact(order, preview, ship);
            var areaResult = _areaManager.ResolveArea(ship.Postcode, ship.Suburb, ship.State);
            int? resolvedAreaId = areaResult?.AreaID;

            if (matched != null)
            {
                preview.MatchedContactId = matched.ContactID;
                preview.IsNewContact = false;
                preview.ContactStatusLabel = "Found";
                preview.ContactDisplayName = FormatStoredContactName(matched);
                preview.ContactHasShippingChanges = HasShippingChanges(matched, order, ship, resolvedAreaId);
                preview.CanAddContact = false;
                preview.CanUpdateContact = preview.ContactHasShippingChanges;
                preview.ContactSummary = string.Empty;
            }
            else
            {
                preview.MatchedContactId = null;
                preview.IsNewContact = true;
                preview.ContactStatusLabel = "New";
                preview.ContactDisplayName = GetWooContactName(order, ship);
                preview.ContactHasShippingChanges = false;
                preview.CanAddContact = true;
                preview.CanUpdateContact = false;
                string email = FirstNonEmpty(ship.Email, order.Billing?.Email);
                preview.ContactSummary = preview.ContactDisplayName
                    + (string.IsNullOrWhiteSpace(email) ? string.Empty : " (" + email + ")");
            }
        }

        private Contact FindMatchedContact(WooOrderDto order, WooOrderImportPreviewRow preview, WooAddressDto ship)
        {
            string email = FirstNonEmpty(ship.Email, order.Billing?.Email);
            EmailMatchResolution emailResolution = ResolveEmailMatch(email, order, ship);
            preview.ContactEmailConflict = emailResolution.ConflictNote;
            if (emailResolution.Contact != null)
                return emailResolution.Contact;

            Contact byName = FindContactByName(order, ship);
            if (byName != null)
                return byName;

            if (preview.AlreadyImported && preview.ExistingTrackerOrderId.HasValue && preview.ExistingTrackerOrderId.Value > 0)
            {
                var trackerOrder = _ordersRepo.GetById(preview.ExistingTrackerOrderId.Value);
                int? contactId = trackerOrder?.ContactID;
                if (contactId.HasValue
                    && contactId.Value > 0
                    && contactId.Value != SystemConstants.CustomerConstants.SundryCustomerID)
                {
                    Contact byOrder = _contactsRepo.GetById(contactId.Value);
                    if (byOrder != null && byOrder.Enabled != false)
                        return byOrder;
                }
            }

            return null;
        }

        private sealed class EmailMatchResolution
        {
            public Contact Contact { get; set; }
            public string ConflictNote { get; set; }
        }

        private EmailMatchResolution ResolveEmailMatch(string email, WooOrderDto order, WooAddressDto ship)
        {
            var empty = new EmailMatchResolution();
            if (string.IsNullOrWhiteSpace(email))
                return empty;

            string normalized = email.Trim();
            var hits = _contactsRepo.FindByEmailExact(normalized)
                .Where(c => c.Enabled != false
                    && (EmailMatches(c.EmailAddress, normalized) || EmailMatches(c.AltEmailAddress, normalized)))
                .ToList();

            if (hits.Count == 0)
                return empty;

            if (hits.Count == 1)
                return new EmailMatchResolution { Contact = hits[0] };

            var primaryMatches = hits
                .Where(c => EmailMatches(c.EmailAddress, normalized))
                .ToList();

            List<Contact> pool = hits;
            string reason = null;

            if (primaryMatches.Count == 1)
            {
                pool = primaryMatches;
                reason = "primary EmailAddress match";
            }
            else if (primaryMatches.Count > 1)
            {
                pool = primaryMatches;
            }

            Contact picked = null;
            if (pool.Count > 1)
            {
                picked = PickContactByOrderName(pool, order, ship);
                if (picked != null)
                {
                    reason = string.IsNullOrEmpty(reason)
                        ? "order name match"
                        : reason + ", order name match";
                }
            }

            if (picked == null && pool.Count == 1)
                picked = pool[0];
            else if (picked == null)
                picked = pool.OrderBy(c => c.ContactID).First();

            if (string.IsNullOrEmpty(reason))
            {
                reason = hits.Count == pool.Count
                    ? hits.Count + " matches — lowest ContactID"
                    : "lowest ContactID among " + pool.Count + " primary-email match(es)";
            }

            string note = string.Format(CultureInfo.InvariantCulture,
                "Multiple contacts match email {0} — using {1} ({2})",
                normalized,
                FormatStoredContactName(picked),
                reason);

            return new EmailMatchResolution { Contact = picked, ConflictNote = note };
        }

        private Contact PickContactByOrderName(IList<Contact> candidates, WooOrderDto order, WooAddressDto ship)
        {
            if (candidates == null || candidates.Count == 0)
                return null;
            if (candidates.Count == 1)
                return candidates[0];

            string wooName = GetWooContactName(order, ship);

            var scored = candidates
                .Select(c => new { Contact = c, Score = ScoreOrderNameMatch(c, wooName, ship, order?.Billing) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Contact.ContactID)
                .ToList();

            if (scored.Count == 0)
                return null;
            if (scored.Count == 1)
                return scored[0].Contact;
            if (scored[0].Score > scored[1].Score)
                return scored[0].Contact;

            return null;
        }

        private static int ScoreOrderNameMatch(Contact contact, string wooName, WooAddressDto ship, WooAddressDto billing)
        {
            if (contact == null)
                return 0;

            int score = 0;
            if (!string.IsNullOrWhiteSpace(wooName))
            {
                string woo = wooName.Trim();
                if (!string.IsNullOrWhiteSpace(contact.CompanyName)
                    && string.Equals(contact.CompanyName.Trim(), woo, StringComparison.OrdinalIgnoreCase))
                    score += 100;
                else if (string.Equals(FormatStoredContactName(contact), woo, StringComparison.OrdinalIgnoreCase))
                    score += 90;
                else if (!string.IsNullOrWhiteSpace(contact.CompanyName)
                    && contact.CompanyName.IndexOf(woo, StringComparison.OrdinalIgnoreCase) >= 0)
                    score += 40;
            }

            if (ship != null)
            {
                if (!string.IsNullOrWhiteSpace(ship.FullName)
                    && string.Equals(FormatStoredContactName(contact), ship.FullName.Trim(), StringComparison.OrdinalIgnoreCase))
                    score += 90;
                if (PersonNameMatches(contact, ship.FirstName, ship.LastName))
                    score += 80;
            }

            if (billing != null && PersonNameMatches(contact, billing.FirstName, billing.LastName))
                score += 80;

            return score;
        }

        private Contact FindContactByName(WooOrderDto order, WooAddressDto ship)
        {
            string wooName = GetWooContactName(order, ship);
            if (!string.IsNullOrWhiteSpace(wooName))
            {
                Contact byCompany = _contactsRepo.GetByContactNamePreferEnabled(wooName.Trim());
                if (byCompany != null && byCompany.Enabled != false)
                    return byCompany;

                var companyHits = _contactsRepo.SearchByContactNameLike(wooName.Trim())
                    .Where(c => c.Enabled != false
                        && string.Equals(c.CompanyName?.Trim(), wooName.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (companyHits.Count == 1)
                    return companyHits[0];
            }

            Contact byShipPerson = FindContactByPersonName(ship.FirstName, ship.LastName);
            if (byShipPerson != null)
                return byShipPerson;

            if (order?.Billing != null)
            {
                Contact byBillPerson = FindContactByPersonName(order.Billing.FirstName, order.Billing.LastName);
                if (byBillPerson != null)
                    return byBillPerson;
            }

            return null;
        }

        private Contact FindContactByPersonName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return null;

            string fullName = string.Join(" ",
                new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                Contact byFullName = _contactsRepo.GetByContactNamePreferEnabled(fullName);
                if (byFullName != null && byFullName.Enabled != false)
                    return byFullName;
            }

            var hits = _contactsRepo.SearchByLastName(lastName.Trim())
                .Where(c => c.Enabled != false)
                .ToList();
            if (hits.Count == 0)
                return null;

            if (string.IsNullOrWhiteSpace(firstName))
                return hits.Count == 1 ? hits[0] : null;

            var matched = hits.Where(c => PersonNameMatches(c, firstName, lastName)).ToList();
            if (matched.Count == 1)
                return matched[0];

            return null;
        }

        private static bool PersonNameMatches(Contact contact, string firstName, string lastName)
        {
            if (contact == null)
                return false;

            if (LastNameMatches(contact.ContactLastName, lastName) && FirstNameMatches(contact.ContactFirstName, firstName))
                return true;
            if (LastNameMatches(contact.ContactAltLastName, lastName) && FirstNameMatches(contact.ContactAltFirstName, firstName))
                return true;
            if (CompanyNameMatchesPerson(contact.CompanyName, firstName, lastName))
                return true;

            return false;
        }

        private static bool CompanyNameMatchesPerson(string companyName, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(lastName))
                return false;

            string company = NormalizeCompare(companyName);
            string last = lastName.Trim();
            if (!company.EndsWith(last, StringComparison.OrdinalIgnoreCase)
                && company.IndexOf(" " + last, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return string.IsNullOrWhiteSpace(firstName) || FirstNameMatches(company, firstName);
        }

        private static bool LastNameMatches(string stored, string probe)
        {
            if (string.IsNullOrWhiteSpace(stored) || string.IsNullOrWhiteSpace(probe))
                return false;

            return string.Equals(stored.Trim(), probe.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool FirstNameMatches(string stored, string probe)
        {
            if (string.IsNullOrWhiteSpace(stored) || string.IsNullOrWhiteSpace(probe))
                return false;

            stored = stored.Trim();
            probe = probe.Trim();
            if (string.Equals(stored, probe, StringComparison.OrdinalIgnoreCase))
                return true;

            string storedFirst = stored.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? stored;
            string probeFirst = probe.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? probe;

            if (string.Equals(storedFirst, probeFirst, StringComparison.OrdinalIgnoreCase))
                return true;

            if (storedFirst.Length >= 3 && probeFirst.Length >= 3)
            {
                if (storedFirst.StartsWith(probeFirst, StringComparison.OrdinalIgnoreCase)
                    || probeFirst.StartsWith(storedFirst, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool EmailMatches(string stored, string probe)
        {
            if (string.IsNullOrWhiteSpace(stored) || string.IsNullOrWhiteSpace(probe))
                return false;
            return string.Equals(stored.Trim(), probe.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string GetWooContactName(WooOrderDto order, WooAddressDto ship)
        {
            string company = FirstNonEmpty(ship.Company, order.Billing?.Company);
            if (!string.IsNullOrWhiteSpace(company))
                return company.Trim();

            string vat = ExtractVatNumber(order);
            if (!string.IsNullOrWhiteSpace(vat))
                return vat.Trim();

            return ship.FullName ?? string.Empty;
        }

        private static string FormatStoredContactName(Contact contact)
        {
            if (contact == null)
                return string.Empty;
            if (!string.IsNullOrWhiteSpace(contact.CompanyName))
                return contact.CompanyName.Trim();

            return string.Join(" ",
                new[] { contact.ContactFirstName, contact.ContactLastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
        }

        private static string FormatContactLogName(Contact contact)
        {
            string name = FormatStoredContactName(contact);
            return string.IsNullOrWhiteSpace(name) ? "Contact" : name;
        }

        private static string ExtractVatNumber(WooOrderDto order)
        {
            return WooContactBootstrap.ExtractVatNumber(order);
        }

        private bool HasShippingChanges(Contact contact, WooOrderDto order, WooAddressDto ship, int? wooResolvedAreaId)
        {
            return DescribeShippingChanges(contact, order, ship, wooResolvedAreaId).Count > 0;
        }

        /// <summary>Human-readable list of contact fields that differ from Woo shipping (before apply).</summary>
        private List<string> DescribeShippingChanges(
            Contact contact,
            WooOrderDto order,
            WooAddressDto ship,
            int? wooResolvedAreaId)
        {
            var parts = new List<string>();
            if (contact == null)
                return parts;

            WooImportAddressConfig config = AddressConfig;
            string areaName = GetContactAreaName(contact);
            string wooAreaName = null;
            if (wooResolvedAreaId.HasValue && wooResolvedAreaId.Value > 0)
                wooAreaName = _areasRepo.GetAreaName(wooResolvedAreaId.Value);

            if (!WooImportAddressHelper.BillingAddressesMatch(contact.BillingAddress, ship, config, areaName, wooAreaName))
            {
                string from = TruncateNoteValue(contact.BillingAddress, 40);
                string to = TruncateNoteValue(
                    WooImportAddressHelper.FormatBillingAddress(ship, config, wooAreaName ?? areaName),
                    40);
                parts.Add(string.IsNullOrEmpty(from)
                    ? "address → " + to
                    : "address " + from + " → " + to);
            }

            string wooPostcode = NormalizePostcode(ship.Postcode);
            string oldPost = (contact.PostalCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(oldPost) && !string.IsNullOrWhiteSpace(wooPostcode))
                parts.Add("postcode → " + wooPostcode);
            else if (!string.IsNullOrWhiteSpace(oldPost)
                && !string.IsNullOrWhiteSpace(wooPostcode)
                && !NormEquals(oldPost, wooPostcode))
                parts.Add("postcode " + oldPost + " → " + wooPostcode);

            string wooPhoneRaw = FirstNonEmpty(ship.Phone, order.Billing?.Phone);
            if (!WooImportAddressHelper.PhonesMatch(contact.PhoneNumber, wooPhoneRaw, config))
            {
                string oldPhone = TruncateNoteValue(contact.PhoneNumber, 24);
                string newPhone = TruncateNoteValue(
                    string.IsNullOrWhiteSpace(wooPhoneRaw)
                        ? string.Empty
                        : WooImportAddressHelper.FormatPhone(wooPhoneRaw, config),
                    24);
                parts.Add(string.IsNullOrEmpty(oldPhone)
                    ? "phone → " + newPhone
                    : "phone " + oldPhone + " → " + newPhone);
            }

            bool contactMissingArea = !contact.AreaID.HasValue || contact.AreaID.Value <= 0;
            bool wooHasArea = wooResolvedAreaId.HasValue && wooResolvedAreaId.Value > 0;
            // Only nudge area when the contact has none — do not keep re-offering when
            // Tracker already has a different area than the postcode default.
            if (contactMissingArea && wooHasArea)
            {
                parts.Add("area → " + (string.IsNullOrWhiteSpace(wooAreaName)
                    ? ("#" + wooResolvedAreaId.Value.ToString(CultureInfo.InvariantCulture))
                    : wooAreaName.Trim()));
            }

            return parts;
        }

        private static string TruncateNoteValue(string value, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            string s = NormalizeCompare(value);
            if (s.Length <= maxLen)
                return s;
            return s.Substring(0, Math.Max(0, maxLen - 1)) + "…";
        }

        private static bool NormEquals(string a, string b)
        {
            return string.Equals(NormalizeCompare(a), NormalizeCompare(b), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeCompare(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(value.Trim(), @"\s+", " ");
        }

        private void ApplyWooShippingToContact(Contact contact, WooOrderDto order, WooAddressDto ship)
        {
            WooImportAddressConfig config = AddressConfig;
            var areaResult = _areaManager.ResolveArea(ship.Postcode, ship.Suburb, ship.State);

            // Prefer keeping an existing Tracker area; only fill area when missing.
            // Format billing with the area we keep so the next preview compare is clean.
            string areaName = GetContactAreaName(contact);
            if ((!contact.AreaID.HasValue || contact.AreaID.Value <= 0) && areaResult?.AreaID != null)
            {
                contact.AreaID = areaResult.AreaID;
                contact.PreferredAgentID = ResolveDeliveryPersonForArea(areaResult);
                areaName = areaResult.AreaName;
            }
            else if (string.IsNullOrWhiteSpace(areaName) && areaResult != null)
                areaName = areaResult.AreaName;

            contact.BillingAddress = WooImportAddressHelper.FormatBillingAddress(ship, config, areaName);
            contact.PostalCode = NormalizePostcode(ship.Postcode);
            contact.StateOrProvince = ship.State ?? contact.StateOrProvince;
            if (!string.IsNullOrWhiteSpace(ship.Country))
                contact.CountryOrRegion = ship.Country;

            string phone = FirstNonEmpty(ship.Phone, order.Billing?.Phone);
            if (!string.IsNullOrWhiteSpace(phone))
                contact.PhoneNumber = WooImportAddressHelper.FormatPhone(phone, config);
        }

        private string GetContactAreaName(Contact contact)
        {
            if (contact?.AreaID == null || contact.AreaID.Value <= 0)
                return null;
            return _areasRepo.GetAreaName(contact.AreaID.Value);
        }

        private void ResolveDeliveryPreview(WooOrderDto order, WooOrderImportPreviewRow preview, WooAddressDto ship)
        {
            if (preview.MatchedContactId.HasValue && preview.MatchedContactId.Value > 0)
            {
                Contact matched = _contactsRepo.GetById(preview.MatchedContactId.Value);
                if (matched?.AreaID != null && matched.AreaID.Value > 0)
                {
                    preview.ResolvedAreaId = matched.AreaID;
                    preview.ResolvedAreaName = _areasRepo.GetAreaName(matched.AreaID.Value);
                    if (matched.PreferredAgentID.HasValue && matched.PreferredAgentID.Value > 0)
                        preview.DeliveryPersonId = matched.PreferredAgentID.Value;
                    else
                    {
                        int personId = _areaManager.ResolveDeliveryPersonForArea(matched.AreaID.Value);
                        preview.DeliveryPersonId = personId > 0
                            ? personId
                            : WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId();
                    }
                    return;
                }
            }

            var areaResult = _areaManager.ResolveArea(ship.Postcode, ship.Suburb, ship.State);
            if (areaResult != null && areaResult.AreaID.HasValue)
            {
                preview.ResolvedAreaId = areaResult.AreaID;
                preview.ResolvedAreaName = areaResult.AreaName;
            }

            int? shipPerson = _areaManager.ResolveDeliveryPersonForShippingMethod(preview.ShippingMethod);
            if (shipPerson.HasValue && shipPerson.Value > 0)
                preview.DeliveryPersonId = shipPerson.Value;
            else if (areaResult?.AreaID != null)
                preview.DeliveryPersonId = _areaManager.ResolveDeliveryPersonForArea(areaResult.AreaID.Value);
            else
                preview.DeliveryPersonId = WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId();
        }

        private void CollectConflicts(WooOrderDto order, WooOrderImportPreviewRow preview, WooAddressDto ship)
        {
            foreach (var line in preview.Lines.Where(l => l.IsUnmappedForNotes))
            {
                string sku = line.Sku ?? line.Name;
                if (line.Note != null && line.Note.StartsWith(NoteCategoryNotImported, StringComparison.OrdinalIgnoreCase))
                    preview.Conflicts.Add(line.Note + " — " + sku);
                else
                    preview.Conflicts.Add("Unmapped SKU: " + sku);
            }

            var areaResult = _areaManager.ResolveArea(ship.Postcode, ship.Suburb, ship.State);
            bool matchedContactHasArea = preview.MatchedContactId.HasValue
                && preview.MatchedContactId.Value > 0
                && _contactsRepo.GetById(preview.MatchedContactId.Value)?.AreaID is int matchedAreaId
                && matchedAreaId > 0;

            if (areaResult != null && !matchedContactHasArea)
            {
                if (areaResult.IsAmbiguous)
                    preview.Conflicts.Add("Area: " + (areaResult.Reason ?? "ambiguous postcode"));
                else if (areaResult.UsedDefaultArea)
                    preview.Conflicts.Add("Area: used catch-all — " + (areaResult.Reason ?? string.Empty));
            }

            if (preview.AlreadyImported)
                preview.Conflicts.Add(
                    "Order #"
                    + (preview.ExistingTrackerOrderId.HasValue
                        ? preview.ExistingTrackerOrderId.Value.ToString(CultureInfo.InvariantCulture)
                        : "?"));

            if (preview.Lines.Any(LineGoesToOrderNotes) && ResolveImportNotesItemId() <= 0)
                preview.Conflicts.Add("Notes SKUs will go to order notes, but no Notes item is configured (Woo Mapping → Mappings).");

            if (!preview.UseZzName)
            {
                if (!string.IsNullOrWhiteSpace(preview.ContactEmailConflict))
                    preview.Conflicts.Add(preview.ContactEmailConflict);

                if (preview.MatchedContactId.HasValue
                    && preview.MatchedContactId.Value > 0)
                {
                    DateTime orderDate = TimeZoneUtils.Now().Date;
                    DateTime prepDate;
                    DateTime deliveryDate;
                    ResolveImportScheduleDates(preview.MatchedContactId.Value, orderDate, out prepDate, out deliveryDate);
                    var probe = new OrderHeaderData
                    {
                        CustomerID = preview.MatchedContactId.Value,
                        RequiredByDate = deliveryDate
                    };
                    int? dup = _orderManager.FindExistingOrderForHeader(probe);
                    if (dup.HasValue && dup.Value > 0
                        && (!preview.ExistingTrackerOrderId.HasValue || preview.ExistingTrackerOrderId.Value != dup.Value))
                    {
                        preview.Conflicts.Add("An order already exists for this contact on delivery date "
                            + deliveryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    }
                }
            }

            foreach (string c in preview.Conflicts)
            {
                if (!preview.Warnings.Contains(c))
                    preview.Warnings.Add(c);
            }
        }

        public List<WooPaymentMethodMap> GetPaymentMethodMaps()
        {
            return _paymentMapRepo.GetAllOrdered();
        }

        public void SavePaymentMethodMaps(IList<WooPaymentMethodMap> maps, string updatedBy)
        {
            if (maps == null)
                return;
            foreach (var map in maps)
            {
                if (map == null || string.IsNullOrWhiteSpace(map.MethodMatch))
                    continue;
                if (map.MapID > 0)
                    _paymentMapRepo.UpdateMap(map, updatedBy);
                else
                    _paymentMapRepo.InsertMap(map, updatedBy);
            }
        }

        public void DeletePaymentMethodMap(int mapId, string updatedBy)
        {
            if (mapId > 0)
                _paymentMapRepo.DeleteMap(mapId);
        }

        public List<WooOrderImportConflictRow> GetImportConflicts(int maxRows = 200)
        {
            return _wooOrderRepo.GetOrdersWithConflicts(maxRows);
        }

        private int CommitOrder(WooOrderDto order, WooOrderImportPreviewRow preview, int? existingOrderId, string updatedBy)
        {
            WooAddressDto ship = order.Shipping ?? new WooAddressDto();
            long contactId = ResolveContactId(order, preview, ship, updatedBy);
            if (contactId <= 0)
                return 0;

            DateTime orderDate;
            DateTime prepDate;
            DateTime deliveryDate;
            if (existingOrderId.HasValue && existingOrderId.Value > 0)
            {
                // Refreshing: keep existing schedule dates; only replace lines/notes.
                var existingHeader = _ordersRepo.GetOrderHeaderByOrderId(existingOrderId.Value);
                if (existingHeader != null)
                {
                    orderDate = existingHeader.OrderDate.Date;
                    prepDate = existingHeader.PrepDate.Date;
                    deliveryDate = existingHeader.RequiredByDate.Date;
                }
                else
                {
                    orderDate = TimeZoneUtils.Now().Date;
                    ResolveImportScheduleDates(contactId, orderDate, out prepDate, out deliveryDate);
                }
            }
            else
            {
                // New import: OrderDate = today; Prep/Delivery from contact's area.
                orderDate = TimeZoneUtils.Now().Date;
                ResolveImportScheduleDates(contactId, orderDate, out prepDate, out deliveryDate);
            }

            var header = new OrderHeaderData
            {
                CustomerID = contactId,
                OrderDate = orderDate,
                PrepDate = prepDate,
                RequiredByDate = deliveryDate,
                ToBeDeliveredBy = preview.DeliveryPersonId ?? WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId(),
                Confirmed = order.DatePaid.HasValue
                    || string.Equals(order.Status, "processing", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(order.Status, "completed", StringComparison.OrdinalIgnoreCase),
                PurchaseOrder = preview.WooOrderNumber,
                Notes = BuildOrderNotes(order, preview, ship,
                    existingOrderId.HasValue && existingOrderId.Value > 0)
            };

            string conflictText = preview.Conflicts != null && preview.Conflicts.Count > 0
                ? string.Join("; ", preview.Conflicts)
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(conflictText))
            {
                header.Notes = (header.Notes ?? string.Empty).Trim();
                if (header.Notes.Length > 0)
                    header.Notes += " | ";
                header.Notes += "Conflict: " + conflictText;
            }

            int orderId;
            if (existingOrderId.HasValue && existingOrderId.Value > 0)
            {
                orderId = existingOrderId.Value;
                header.OrderID = orderId;
                if (!_orderManager.UpdateOrderHeader(orderId, header))
                    return 0;
                _ordersRepo.DeleteLinesForOrder(orderId);
            }
            else
            {
                var ensure = _orderManager.EnsureOrderHeader(header, forceNewOrder: true);
                if (ensure.OrderId <= 0)
                    throw new InvalidOperationException(ensure.Error ?? "Could not create order header.");
                orderId = ensure.OrderId;
            }

            bool inferPackagingFromPriorOrders = ShouldInferPackagingFromPriorOrders(preview, contactId);
            var packagingNoteLines = new List<string>();
            var packagingLogLines = new List<string>();

            foreach (var line in preview.Lines)
            {
                if (!line.CanImport)
                    continue;
                if (string.Equals(line.MapType, "Notes", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!line.TrackerItemId.HasValue || line.TrackerItemId.Value <= 0)
                    continue;

                int packagingId = line.PackagingId ?? 0;
                if (inferPackagingFromPriorOrders && packagingId <= 0)
                {
                    string inferredNote;
                    string inferredLog;
                    packagingId = TryInferPackagingFromPriorOrder(
                        contactId,
                        line,
                        orderId,
                        out inferredNote,
                        out inferredLog);
                    if (packagingId > 0)
                    {
                        line.PackagingId = packagingId;
                        if (!string.IsNullOrWhiteSpace(inferredNote))
                            packagingNoteLines.Add(inferredNote);
                        if (!string.IsNullOrWhiteSpace(inferredLog))
                            packagingLogLines.Add(inferredLog);
                    }
                }

                var add = _orderManager.AddOrderLineToOrder(orderId, line.TrackerItemId.Value, line.TrackerQty, packagingId);
                if (!string.IsNullOrEmpty(add.Error))
                    throw new InvalidOperationException(add.Error);
            }

            TryAddNotesItemLine(orderId, preview);

            if (packagingNoteLines.Count > 0)
            {
                header.Notes = (header.Notes ?? string.Empty).Trim();
                if (header.Notes.Length > 0)
                    header.Notes += " | ";
                header.Notes += string.Join("; ", packagingNoteLines);
                header.OrderID = orderId;
                _orderManager.UpdateOrderHeader(orderId, header);
            }

            _wooOrderRepo.Upsert(new WooOrderInfo
            {
                OrderID = orderId,
                WooOrderId = order.Id,
                WooOrderNumber = preview.WooOrderNumber,
                WooStatus = order.Status,
                PaymentMethod = order.PaymentMethodTitle ?? order.PaymentMethod,
                PaymentStatus = order.Status,
                PaymentPaid = order.DatePaid.HasValue,
                LastSyncedUtc = DateTime.UtcNow,
                RawSnapshotJson = order.RawJson,
                ImportConflicts = conflictText
            });

            AppLogger.WriteLog("woo",
                string.Format(CultureInfo.InvariantCulture, "Imported Woo #{0} → Order #{1}", preview.WooOrderNumber, orderId)
                + (packagingLogLines.Count > 0 ? "; " + string.Join("; ", packagingLogLines) : string.Empty),
                updatedBy);
            WooCommerceUserLog.Write(
                string.Format(CultureInfo.InvariantCulture, "Imported Woo #{0}", preview.WooOrderNumber),
                "Tracker order #" + orderId
                + (packagingLogLines.Count > 0 ? "; " + string.Join("; ", packagingLogLines) : string.Empty),
                updatedBy);

            return orderId;
        }

        private static bool LineGoesToOrderNotes(WooOrderLinePreview line)
        {
            if (line == null || !line.CanImport)
                return false;
            return line.IsUnmappedForNotes
                || string.Equals(line.MapType, "Notes", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// When SKUs are written to order notes, also add the configured Tracker Notes item
        /// so Order Detail / emails treat the header notes as a notes line.
        /// </summary>
        private void TryAddNotesItemLine(int orderId, WooOrderImportPreviewRow preview)
        {
            if (orderId <= 0 || preview == null)
                return;
            if (!preview.Lines.Any(LineGoesToOrderNotes))
                return;

            int notesItemId = ResolveImportNotesItemId();
            if (notesItemId <= 0)
                return;

            if (preview.Lines.Any(l => l.TrackerItemId.HasValue && l.TrackerItemId.Value == notesItemId))
                return;

            var add = _orderManager.AddOrderLineToOrder(orderId, notesItemId, 1, 0);
            if (!string.IsNullOrEmpty(add.Error))
            {
                AppLogger.WriteLog("woo",
                    "Notes item line skipped for order " + orderId + ": " + add.Error);
            }
        }

        private int ResolveImportNotesItemId()
        {
            var settings = _settingsRepo.GetSettings();
            int configured = settings?.ImportNotesItemID ?? 0;
            if (configured > 0 && _itemsRepo.GetById(configured) != null)
                return configured;

            int legacyId = SystemConstants.ItemConstants.NoteItemTimeID;
            if (legacyId > 0 && _itemsRepo.GetById(legacyId) != null)
                return legacyId;

            var byName = _itemsRepo.FindFirstByDescription("Notes");
            return byName != null && byName.ItemID > 0 ? byName.ItemID : 0;
        }

        private static bool ShouldInferPackagingFromPriorOrders(WooOrderImportPreviewRow preview, long contactId)
        {
            if (preview == null || preview.UseZzName || preview.IsNewContact)
                return false;
            if (contactId <= 0 || contactId == SystemConstants.CustomerConstants.SundryCustomerID)
                return false;
            return true;
        }

        private int TryInferPackagingFromPriorOrder(
            long contactId,
            WooOrderLinePreview line,
            int currentOrderId,
            out string orderNote,
            out string logDetail)
        {
            orderNote = null;
            logDetail = null;
            if (line == null || !line.TrackerItemId.HasValue || line.TrackerItemId.Value <= 0)
                return 0;

            int sortOrder = _itemsRepo.GetItemSortOrder(line.TrackerItemId.Value);
            if (sortOrder <= 0)
                return 0;

            PriorOrderPackagingHint hint = _ordersRepo.GetRecentPackagingForContactSortGroup(
                contactId,
                sortOrder,
                currentOrderId);
            if (hint == null || hint.PackagingId <= 0)
                return 0;

            string packLabel = GetPackagingLabel(hint.PackagingId);
            string lineSku = !string.IsNullOrWhiteSpace(line.TrackerSku)
                ? line.TrackerSku
                : (!string.IsNullOrWhiteSpace(line.Sku) ? line.Sku : line.Name);
            string sourceItem = !string.IsNullOrWhiteSpace(hint.SourceItemSku)
                ? hint.SourceItemSku.Trim()
                : (hint.SourceItemDesc ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(sourceItem))
                sourceItem = "item #" + hint.SourceItemId.ToString(CultureInfo.InvariantCulture);

            orderNote = string.Format(CultureInfo.InvariantCulture,
                "Packaging for {0}: {1} (from order #{2}, same sort group as {3})",
                lineSku ?? "line",
                packLabel,
                hint.SourceOrderId,
                sourceItem);

            logDetail = string.Format(CultureInfo.InvariantCulture,
                "packaging for {0} from order #{1} ({2})",
                lineSku ?? "line",
                hint.SourceOrderId,
                packLabel);

            return hint.PackagingId;
        }

        /// <summary>
        /// OrderDate = today. Prep/Delivery from the contact's area schedule (same as Order Detail),
        /// not from the WooCommerce created date (which looked random vs area prep days).
        /// </summary>
        private void ResolveImportScheduleDates(long contactId, DateTime orderDate, out DateTime prepDate, out DateTime deliveryDate)
        {
            prepDate = orderDate;
            deliveryDate = orderDate;

            if (contactId > 0)
            {
                try
                {
                    DateTime delivery = orderDate;
                    DateTime prep = new TrackerTools().GetNextPreparationDateByCustomerID(contactId, ref delivery);
                    if (prep > DateTime.MinValue && delivery > DateTime.MinValue)
                    {
                        prepDate = prep.Date;
                        deliveryDate = delivery.Date;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("ResolveImportScheduleDates: " + ex.Message);
                }
            }

            var fallback = _orderManager.CalculateOrderDates(orderDate);
            prepDate = fallback.PrepDate.Date;
            deliveryDate = fallback.deliveryDate.Date;
        }

        private string GetPackagingLabel(int packagingId)
        {
            if (packagingId <= 0)
                return string.Empty;

            var pack = _packRepo.GetById(packagingId);
            if (pack != null && !string.IsNullOrWhiteSpace(pack.Symbol))
                return pack.Symbol.Trim();
            return _packRepo.GetPackagingDescById(packagingId);
        }

        private long ResolveContactId(WooOrderDto order, WooOrderImportPreviewRow preview, WooAddressDto ship, string updatedBy)
        {
            if (preview.UseZzName)
                return SystemConstants.CustomerConstants.SundryCustomerID;

            Contact contact = FindMatchedContact(order, preview, ship);

            if (contact == null)
            {
                contact = BuildContactFromWoo(order, preview, ship);
                int newId = _contactsRepo.Insert(contact);
                if (newId <= 0)
                    return 0;
                contact.ContactID = newId;
                WooContactBootstrap.EnsureAccInfo(_accInfoRepo, contact, order, newId);
                return newId;
            }

            ApplyWooShippingToContact(contact, order, ship);
            if (!string.IsNullOrWhiteSpace(ship.Company))
                contact.CompanyName = GetWooContactName(order, ship);
            if (!string.IsNullOrWhiteSpace(ship.FirstName))
                contact.ContactFirstName = ship.FirstName;
            if (!string.IsNullOrWhiteSpace(ship.LastName))
                contact.ContactLastName = ship.LastName;
            _contactsRepo.Update(contact);
            return contact.ContactID;
        }

        private Contact BuildContactFromWoo(WooOrderDto order, WooOrderImportPreviewRow preview, WooAddressDto ship)
        {
            string email = FirstNonEmpty(ship.Email, order.Billing?.Email);
            var areaResult = _areaManager.ResolveArea(ship.Postcode, ship.Suburb, ship.State);
            int? deliveryPersonId = ResolveDeliveryPersonForArea(areaResult);
            int salesAgentId = PersonDefaults.GetDefaultSalesAgentId();
            string contactName = GetWooContactName(order, ship);

            var contact = new Contact
            {
                ContactFirstName = ship.FirstName ?? string.Empty,
                ContactLastName = ship.LastName ?? string.Empty,
                CompanyName = contactName,
                BillingAddress = WooImportAddressHelper.FormatBillingAddress(ship, AddressConfig, areaResult?.AreaName),
                PostalCode = NormalizePostcode(ship.Postcode),
                StateOrProvince = ship.State ?? string.Empty,
                CountryOrRegion = ship.Country ?? string.Empty,
                EmailAddress = email ?? string.Empty,
                PhoneNumber = WooImportAddressHelper.FormatPhone(
                    FirstNonEmpty(ship.Phone, order.Billing?.Phone) ?? string.Empty,
                    AddressConfig),
                AreaID = areaResult?.AreaID,
                PreferredAgentID = deliveryPersonId,
                SalesAgentID = salesAgentId > 0 ? salesAgentId : (int?)null,
                Enabled = true,
                Notes = string.Format(CultureInfo.InvariantCulture,
                    "{0:yyyy-MM-dd}: Created from Woo order #{1}.\n",
                    TimeZoneUtils.Now(),
                    preview.WooOrderNumber)
            };

            WooContactBootstrap.ApplyCoffeePreferences(contact, preview, _itemsRepo);
            WooContactBootstrap.ApplyCapitalization(contact);
            return contact;
        }

        private int? ResolveDeliveryPersonForArea(WooAreaResolveResult areaResult)
        {
            if (areaResult?.AreaID != null && areaResult.AreaID.Value > 0)
                return _areaManager.ResolveDeliveryPersonForArea(areaResult.AreaID.Value);
            if (areaResult?.DefaultPreferredAgentID != null && areaResult.DefaultPreferredAgentID.Value > 0)
                return areaResult.DefaultPreferredAgentID;
            return WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId();
        }

        private string BuildOrderNotes(WooOrderDto order, WooOrderImportPreviewRow preview, WooAddressDto ship, bool isUpdate)
        {
            var sb = new StringBuilder();
            sb.Append(isUpdate ? "Updated by Woo import" : "Added by Woo import");
            if (!string.IsNullOrWhiteSpace(preview.WooOrderNumber))
                sb.Append(" #").Append(preview.WooOrderNumber.Trim());
            sb.Append(" — ");
            sb.Append('[').Append(preview.PaymentAbbrev).Append("] ");

            if (preview.UseZzName)
            {
                // Put [#email#] early so Order Done / confirm can find it even if notes are truncated.
                string email = FirstNonEmpty(ship.Email, order.Billing?.Email);
                if (!string.IsNullOrWhiteSpace(email))
                    sb.Append("[#").Append(email.Trim()).Append("#] ");
                sb.Append(BuildZzNameNamePrefix(ship.Company, ship.FullName));
                sb.Append(ship.FormattedAddress);
            }
            else if (!string.IsNullOrWhiteSpace(order.CustomerNote))
            {
                sb.Append(order.CustomerNote.Trim());
            }

            foreach (var line in preview.Lines.Where(l => l.CanImport && (
                l.IsUnmappedForNotes
                || string.Equals(l.MapType, "Notes", StringComparison.OrdinalIgnoreCase))))
            {
                if (sb.Length > 0)
                    sb.Append(" | ");
                sb.Append(FormatNoteLine(line));
            }

            foreach (var line in preview.Lines.Where(l => l.CanImport && l.AttributeNoteParts != null && l.AttributeNoteParts.Count > 0))
            {
                foreach (string part in line.AttributeNoteParts)
                {
                    if (string.IsNullOrWhiteSpace(part))
                        continue;
                    if (sb.Length > 0)
                        sb.Append(" | ");
                    sb.Append(part.Trim());
                }
            }

            return sb.ToString().Trim();
        }

        private static string FormatNoteLine(WooOrderLinePreview line)
        {
            string label = !string.IsNullOrWhiteSpace(line.Sku) ? line.Sku : line.Name;
            if (line.IsUnmappedForNotes && line.WooQty > 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}× {1}", line.WooQty, label);
            }

            return label ?? string.Empty;
        }

        private static string BuildZzNameNotePrefix(string company, string contactName, string email)
        {
            string prefix = BuildZzNameNamePrefix(company, contactName);
            if (!string.IsNullOrWhiteSpace(email))
                prefix += "[#" + email.Trim() + "#] ";
            return prefix;
        }

        private static string BuildZzNameNamePrefix(string company, string contactName)
        {
            if (string.IsNullOrWhiteSpace(company))
                return (contactName ?? string.Empty).Trim() + ": ";
            return company.Trim() + ", " + (contactName ?? string.Empty).Trim() + ": ";
        }

        private bool IsGearProduct(long productId, long variationId)
        {
            if (_gearCategoryIds == null || _gearCategoryIds.Count == 0)
                return IsGearFromCacheLabel(productId, variationId);

            var cats = GetProductCategoryIds(productId, variationId);
            return cats.Any(id => _gearCategoryIds.Contains(id));
        }

        private bool IsGearFromCacheLabel(long productId, long variationId)
        {
            string label = GetCategoriesLabel(productId, variationId);
            return !string.IsNullOrWhiteSpace(label)
                && label.IndexOf("Gear", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private HashSet<long> LoadGearCategoryIds()
        {
            var set = new HashSet<long>();
            var cats = _categoryRepo.GetAllOrdered() ?? new List<WooCategoryFilter>();
            foreach (var c in cats)
            {
                if (c == null || c.WooCategoryId <= 0)
                    continue;
                if (!string.IsNullOrWhiteSpace(c.CategoryName)
                    && c.CategoryName.IndexOf("Gear", StringComparison.OrdinalIgnoreCase) >= 0)
                    set.Add(c.WooCategoryId);
            }
            // Include direct children of Gear categories.
            foreach (var c in cats)
            {
                if (c == null || c.WooCategoryId <= 0)
                    continue;
                if (c.ParentWooCategoryId > 0 && set.Contains(c.ParentWooCategoryId))
                    set.Add(c.WooCategoryId);
            }
            return set;
        }

        private void TryResolveUnmappedLine(WooOrderLineDto li, WooOrderLinePreview line)
        {
            Item bySku = TryMatchTrackerSku(li.Sku);
            if (bySku != null)
            {
                line.MapType = "SkuMatch";
                line.TrackerItemId = bySku.ItemID;
                line.TrackerSku = bySku.SKU;
                double qtyFactor = 1;
                int? packagingId = null;
                WooLineAttributeResolveResult attr = _mappingManager.ResolveOrderLineAttributes(
                    li.MetaData,
                    bySku.ItemServiceTypeID ?? 0,
                    qtyFactor,
                    packagingId);
                if (attr != null)
                {
                    if (attr.QtyFactor > 0)
                        qtyFactor = attr.QtyFactor;
                    if (attr.PackagingId.HasValue && attr.PackagingId.Value > 0)
                        packagingId = attr.PackagingId;
                    if (attr.NoteParts != null && attr.NoteParts.Count > 0)
                        line.AttributeNoteParts = attr.NoteParts;
                    if (!string.IsNullOrWhiteSpace(attr.Reason))
                        line.Note = "Matched Tracker SKU + " + attr.Reason;
                    else
                        line.Note = "Matched Tracker SKU";
                }
                else
                    line.Note = "Matched Tracker SKU";

                line.TrackerQty = Math.Round(qtyFactor * li.Quantity, SystemConstants.DatabaseConstants.NumDecimalPoints);
                line.PackagingId = packagingId;
                line.CanImport = true;
                return;
            }

            string catNote = GetExcludedCategoryNote(li.ProductId, li.VariationId);
            line.MapType = "Notes";
            line.CanImport = true;
            line.IsUnmappedForNotes = true;
            if (!string.IsNullOrWhiteSpace(catNote))
                line.Note = catNote + " → order notes";
            else
                line.Note = "Unmapped → order notes";
        }

        private Item TryMatchTrackerSku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return null;
            return _itemsRepo.GetBySku(sku.Trim());
        }

        private void EnsureCategoryFilterContext()
        {
            if (_categoryFilters != null)
                return;
            _categoryFilters = _categoryRepo.GetAllOrdered() ?? new List<WooCategoryFilter>();
            _categoryFilterMode = _settingsRepo.GetSettings()?.CategoryFilterMode ?? "All";
        }

        private string GetExcludedCategoryNote(long productId, long variationId)
        {
            var catIds = GetProductCategoryIds(productId, variationId);
            if (catIds.Count == 0)
                return null;

            EnsureCategoryFilterContext();
            if (_categoryFilters.Count == 0)
                return null;

            if (ProductPassesCategoryFilter(catIds, _categoryFilterMode, _categoryFilters))
                return null;

            string label = GetCategoriesLabel(productId, variationId);
            if (string.IsNullOrWhiteSpace(label))
                label = FormatCategoryNames(catIds, _categoryFilters);
            return NoteCategoryNotImported + ": " + label;
        }

        private static string FormatCategoryNames(IList<long> categoryIds, IList<WooCategoryFilter> filters)
        {
            if (categoryIds == null || categoryIds.Count == 0 || filters == null)
                return string.Empty;

            var byId = filters
                .Where(f => f != null && f.WooCategoryId > 0)
                .ToDictionary(f => f.WooCategoryId, f => f.CategoryName ?? string.Empty);

            var names = new List<string>();
            foreach (long id in categoryIds)
            {
                string name;
                if (byId.TryGetValue(id, out name) && !string.IsNullOrWhiteSpace(name))
                    names.Add(name.Trim());
            }

            return names.Count > 0
                ? string.Join(", ", names.Distinct(StringComparer.OrdinalIgnoreCase))
                : string.Empty;
        }

        private static bool ProductPassesCategoryFilter(
            IList<long> categoryIds,
            string mode,
            List<WooCategoryFilter> filters)
        {
            if (filters == null || filters.Count == 0)
                return true;
            if (categoryIds == null || categoryIds.Count == 0)
                return true;

            mode = (mode ?? "All").Trim();
            bool anyUnticked = filters.Any(f => f != null && !f.IncludeInSync);
            bool useIncludeList = string.Equals(mode, "IncludeList", StringComparison.OrdinalIgnoreCase)
                || (string.Equals(mode, "All", StringComparison.OrdinalIgnoreCase) && anyUnticked);

            if (string.Equals(mode, "ExcludeList", StringComparison.OrdinalIgnoreCase))
            {
                var exclude = new HashSet<long>(filters.Where(f => f != null && !f.IncludeInSync).Select(f => f.WooCategoryId));
                if (exclude.Count == 0)
                    return true;
                return !categoryIds.Any(id => exclude.Contains(id));
            }

            if (!useIncludeList)
                return true;

            var include = BuildEffectiveIncludeCategoryIds(filters);
            if (include.Count == 0)
                return false;

            return categoryIds.Any(id => include.Contains(id));
        }

        private static HashSet<long> BuildEffectiveIncludeCategoryIds(List<WooCategoryFilter> filters)
        {
            var unticked = new HashSet<long>(filters.Where(f => f != null && !f.IncludeInSync).Select(f => f.WooCategoryId));
            var include = new HashSet<long>(filters.Where(f => f != null && f.IncludeInSync).Select(f => f.WooCategoryId));
            var childrenOf = filters.Where(f => f != null).ToLookup(f => f.ParentWooCategoryId);

            var queue = new Queue<long>(include);
            while (queue.Count > 0)
            {
                long id = queue.Dequeue();
                foreach (var child in childrenOf[id])
                {
                    if (unticked.Contains(child.WooCategoryId))
                        continue;
                    if (include.Add(child.WooCategoryId))
                        queue.Enqueue(child.WooCategoryId);
                }
            }

            include.ExceptWith(unticked);
            return include;
        }

        private List<long> GetProductCategoryIds(long productId, long variationId)
        {
            string raw = LookupCacheColumn(productId, variationId, "CategoryIds");
            var list = new List<long>();
            if (string.IsNullOrWhiteSpace(raw))
                return list;
            foreach (string part in raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                long id;
                if (long.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id) && id > 0)
                    list.Add(id);
            }
            return list;
        }

        private string GetCategoriesLabel(long productId, long variationId)
        {
            return LookupCacheColumn(productId, variationId, "CategoriesLabel");
        }

        private string LookupCacheColumn(long productId, long variationId, string column)
        {
            if (!_catalogRepo.TableExists() || productId <= 0)
                return string.Empty;

            long varId = variationId > 0 ? variationId : 0;
            string sql = @"
SELECT TOP 1 " + column + @" FROM WooCatalogCacheTbl
WHERE WooProductId = @P AND (WooVariationId = @V OR (@V > 0 AND WooVariationId = 0))
ORDER BY CASE WHEN WooVariationId = @V THEN 0 ELSE 1 END";

            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@P", DataValue = productId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@V", DataValue = varId, DataDbType = DbType.Int64 }
            };
            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<string>(sql, p) ?? string.Empty;
        }

        private void TouchOrdersSyncCursor(DateTime latestOrderUtc, string updatedBy)
        {
            var settings = _settingsManager.GetSettings();
            if (!settings.LastOrdersSyncUtc.HasValue || latestOrderUtc > settings.LastOrdersSyncUtc.Value)
                settings.LastOrdersSyncUtc = latestOrderUtc;
            _settingsRepo.SaveSettings(settings, updatedBy);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return null;
            foreach (string v in values)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }
            return null;
        }

        private static string NormalizePostcode(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
                return string.Empty;
            string digits = new string(postcode.Where(char.IsDigit).ToArray());
            return digits.Length > 0 ? digits.PadLeft(4, '0') : postcode.Trim();
        }
    }
}
