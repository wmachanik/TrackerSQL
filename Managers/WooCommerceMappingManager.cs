using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class WooCommerceMappingManager
    {
        private readonly WooCommerceSettingsManager _settings = new WooCommerceSettingsManager();
        private readonly WooCommerceApiClient _api = new WooCommerceApiClient();
        private readonly WooCategoryFilterRepository _catRepo = new WooCategoryFilterRepository();
        private readonly WooItemMappingRepository _mapRepo = new WooItemMappingRepository();
        private readonly WooAttributeMapRepository _attrRepo = new WooAttributeMapRepository();
        private readonly WooAttributeParentRepository _attrParentRepo = new WooAttributeParentRepository();
        private readonly WooPackagingServiceTypeRepository _packSvcRepo = new WooPackagingServiceTypeRepository();
        private readonly ItemsRepository _itemsRepo = new ItemsRepository();
        private readonly ItemPackagingsRepository _packRepo = new ItemPackagingsRepository();
        private readonly WooCatalogCacheRepository _catalogCache = new WooCatalogCacheRepository();

        public class SyncResult
        {
            public bool Succeeded { get; set; }
            public string Message { get; set; }
            public int Count { get; set; }
            public int FailCount { get; set; }
            public int UnchangedCount { get; set; }
            public List<WooEnabledPushPreviewRow> Rows { get; set; } = new List<WooEnabledPushPreviewRow>();
        }

        public List<WooCategoryFilter> GetCategoryFilters()
        {
            return _catRepo.GetAllOrdered();
        }

        public SyncResult PullCategories(string updatedBy)
        {
            WooCommerceApiClient.ApiCredentials creds;
            string error;
            if (!_settings.TryGetApiCredentials(out creds, out error))
                return Fail(error);

            try
            {
                var pull = _api.GetCategories(creds);
                var cats = pull.Categories ?? new List<WooCategoryDto>();
                bool onlyUncategorized = cats.Count > 0 &&
                    cats.All(c => IsUncategorized(c.Name, c.Id));

                foreach (var c in cats)
                {
                    bool defaultInclude = true;
                    if (IsUncategorized(c.Name, c.Id) && !onlyUncategorized)
                        defaultInclude = false;
                    _catRepo.UpsertFromWoo(c.Id, c.Name, c.Parent, defaultInclude);
                }

                var s = _settings.GetSettings();
                s.LastItemsSyncUtc = DateTime.UtcNow;
                new WooCommerceSettingsRepository().SaveSettings(s, updatedBy);

                int pulled = cats.Count;
                int total = pull.TotalOnWoo > 0 ? pull.TotalOnWoo : pulled;
                AppLogger.WriteLog("woo", "Pulled " + pulled + " of " + total + " categories", updatedBy);
                return new SyncResult
                {
                    Succeeded = true,
                    Count = pulled,
                    Message = MessageProvider.Format(MessageKeys.WooCommerce.MapPullCategoriesOk, pulled, total)
                };
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("woo", "PullCategories failed: " + ex.Message, updatedBy);
                return Fail(ex.Message);
            }
        }

        public void SaveCategoryRow(int filterId, bool include, int? defaultSortValue, string updatedBy, string defaultImportMode = null)
        {
            _catRepo.UpdateIncludeAndSort(filterId, include, defaultSortValue, defaultImportMode);
            AppLogger.WriteLog("woo", "Category filter " + filterId + " include=" + include
                + " sort=" + (defaultSortValue.HasValue ? defaultSortValue.Value.ToString() : "inherit")
                + " import=" + (string.IsNullOrEmpty(defaultImportMode) ? "inherit" : defaultImportMode), updatedBy);
        }

        private static bool IsUncategorized(string name, long id)
        {
            if (id == 15)
                return true;
            return string.Equals((name ?? string.Empty).Trim(), "Uncategorized", StringComparison.OrdinalIgnoreCase);
        }

        public void SaveCategoryMode(string mode, string updatedBy)
        {
            _settings.SaveCategoryFilterMode(mode, updatedBy);
        }

        public bool HasCachedCatalog()
        {
            try
            {
                _settings.EnsureSchema();
                return _catalogCache.CountRows() > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Rebuild mapping grids from the last saved Woo catalog (no API call).</summary>
        public WooProductPullResult LoadCachedProductsForMapping(IList<WooCategoryFilter> categoryFilters = null)
        {
            _settings.EnsureSchema();
            var filters = categoryFilters != null
                ? categoryFilters.ToList()
                : (_catRepo.GetAllOrdered() ?? new List<WooCategoryFilter>());
            var products = _catalogCache.GetAllAsDtos();
            var settings = _settings.GetSettings();
            products = ApplyCategoryFilter(products, settings.CategoryFilterMode, filters);
            int parents = products.Count(p => p != null && (p.IsParentGroup || !p.VariationId.HasValue || p.VariationId.Value <= 0));
            return BuildPullResult(products, parents, hitCap: false, updatedBy: null, touchLastSync: false, categoryFilters: filters);
        }

        public void ResetCatalogCache(string updatedBy)
        {
            _settings.EnsureSchema();
            _catalogCache.ClearAll();
            AppLogger.WriteLog("woo", "Cleared Woo catalog cache", updatedBy);
        }

        /// <summary>Download catalog from Woo, replace local cache, then map against Tracker.</summary>
        public WooProductPullResult PullProductsForMapping(string updatedBy)
        {
            WooCommerceApiClient.ApiCredentials creds;
            string error;
            if (!_settings.TryGetApiCredentials(out creds, out error))
                throw new InvalidOperationException(error);

            _settings.EnsureSchema();
            var settings = _settings.GetSettings();
            var filters = _catRepo.GetAllOrdered();
            int parentsScanned;
            bool hitCap;
            var products = _api.GetProductsAndVariations(creds, out parentsScanned, out hitCap);
            DateTime pullStarted = DateTime.UtcNow;
            _catalogCache.UpsertAll(products, pullStarted);
            products = _catalogCache.GetAllAsDtos();
            products = ApplyCategoryFilter(products, settings.CategoryFilterMode, filters);
            return BuildPullResult(products, parentsScanned, hitCap, updatedBy, touchLastSync: true,
                categoryFilters: filters, catalogSyncCutoff: pullStarted);
        }

        private WooProductPullResult BuildPullResult(
            List<WooProductDto> products,
            int parentsScanned,
            bool hitCap,
            string updatedBy,
            bool touchLastSync,
            IList<WooCategoryFilter> categoryFilters = null,
            DateTime? catalogSyncCutoff = null)
        {
            if (products == null)
                products = new List<WooProductDto>();

            var settings = _settings.GetSettings();
            DateTime? newSinceCutoff = catalogSyncCutoff ?? settings.LastItemsSyncUtc;

            var items = _itemsRepo.GetAll("SKU") ?? new List<Item>();
            var byId = items.ToDictionary(i => i.ItemID);
            var bySku = items
                .Where(i => !string.IsNullOrWhiteSpace(i.SKU))
                .GroupBy(i => i.SKU.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.ItemEnabled != false).First(),
                    StringComparer.OrdinalIgnoreCase);
            var enabledSkusLongestFirst = bySku.Keys
                .OrderByDescending(s => s.Length)
                .ThenBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var attrMaps = _attrRepo.GetActive();
            var variantNames = _attrParentRepo.GetVariantAttributeNames();
            var lineNames = _attrParentRepo.GetLineAttributeNames();
            var notesNames = _attrParentRepo.GetNotesAttributeNames();
            var parentByName = _attrParentRepo.GetUsedByAttributeName();
            var priorities = _attrParentRepo.GetDisplaySortByAttributeName();
            var packSourceNames = parentByName.Values
                .Where(p => p != null && p.ContributesPack)
                .OrderBy(p => p.PackRank)
                .ThenBy(p => p.AttributeName, StringComparer.OrdinalIgnoreCase)
                .Select(p => (p.AttributeName ?? string.Empty).Trim())
                .Where(n => n.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (lineNames.Count > 0)
            {
                attrMaps = attrMaps.FindAll(m => lineNames.Contains(m.AttributeName ?? string.Empty));
                StampParentRanks(attrMaps, parentByName);
            }
            else
                attrMaps = new List<WooAttributeMap>();

            var packagings = GetPackagingsForServiceType(null);
            var parentItemByProduct = BuildParentItemLookup();
            if (categoryFilters == null)
                categoryFilters = _catRepo.GetAllOrdered() ?? new List<WooCategoryFilter>();
            List<ItemSortOrder> sortLookups;
            try
            {
                sortLookups = new ItemSortOrdersRepository().GetAll("SortValue") ?? new List<ItemSortOrder>();
            }
            catch
            {
                sortLookups = ItemSortOrdersRepository.BuiltInRows();
            }
            var allRows = new List<WooProductMapRow>();
            var categoryLockedImportModes = new HashSet<long>();

            foreach (var p in products)
            {
                bool isParentGroup = p.IsParentGroup;
                bool isVariation = !isParentGroup && p.VariationId.HasValue && p.VariationId.Value > 0;
                long? variationId = isParentGroup ? null : p.VariationId;

                var existing = _mapRepo.FindExact(p.Id, variationId);
                int suggested = 0;
                string suggestedDesc = null;
                double qty = 1;
                int? packagingId = null;
                string reason = "Unmapped";
                bool mapToNotes = false;
                bool includeInImport = WooProductMapRow.IsWooEnabledStatus(p.Status);
                string importMode = WooProductMapRow.ImportModeVariants;
                bool usesCategoryImportDefault = false;

                if (isParentGroup)
                {
                    // Parent header: default map children; if a notes map exists on the parent, prefer that mode.
                    if (existing != null && existing.IsNotesMap)
                    {
                        importMode = WooProductMapRow.ImportModeParentNotes;
                        mapToNotes = true;
                        includeInImport = existing.IncludeInImport;
                        reason = "Parent → order notes";
                        qty = existing.QtyFactor;
                    }
                    else if (existing != null && existing.IsExcludeMap)
                    {
                        importMode = WooProductMapRow.ImportModeExclude;
                        includeInImport = false;
                        reason = "Do not import";
                    }
                    else if (existing != null && existing.IsVariantsMap)
                    {
                        importMode = WooProductMapRow.ImportModeVariants;
                        includeInImport = false;
                        reason = "Import variants";
                    }
                    else if (existing != null && existing.ItemID > 0)
                    {
                        importMode = WooProductMapRow.ImportModeParentItem;
                        suggested = existing.ItemID;
                        includeInImport = existing.IncludeInImport;
                        reason = "Parent SKU only";
                        qty = existing.QtyFactor;
                        packagingId = existing.PackagingID;
                        Item existingItem;
                        if (byId.TryGetValue(existing.ItemID, out existingItem))
                            suggestedDesc = existingItem.ItemDesc;
                    }
                    else
                    {
                        string catMode = WooCategoryFilterRepository.ResolveDefaultImportMode(p.CategoryIds, categoryFilters);
                        if (!string.IsNullOrEmpty(catMode))
                        {
                            importMode = catMode;
                            usesCategoryImportDefault = true;
                            categoryLockedImportModes.Add(p.Id);
                            includeInImport = false;
                            if (string.Equals(catMode, WooProductMapRow.ImportModeParentNotes, StringComparison.OrdinalIgnoreCase))
                            {
                                mapToNotes = true;
                                reason = "Parent → order notes (category default)";
                            }
                            else if (string.Equals(catMode, WooProductMapRow.ImportModeParentItem, StringComparison.OrdinalIgnoreCase))
                                reason = "Parent SKU only (category default)";
                            else if (string.Equals(catMode, WooProductMapRow.ImportModeExclude, StringComparison.OrdinalIgnoreCase))
                                reason = "Do not import (category default)";
                            else
                                reason = "Import variants (category default)";
                        }
                        else
                        {
                            includeInImport = false;
                            reason = "Import variants";
                        }
                        Item parentMatch;
                        string parentMatchReason;
                        if (TryMatchEnabledItem(p, bySku, enabledSkusLongestFirst, parentItemByProduct, byId,
                            out parentMatch, out parentMatchReason))
                        {
                            suggested = parentMatch.ItemID;
                            suggestedDesc = parentMatch.ItemDesc;
                        }
                    }
                }
                else if (existing != null)
                {
                    includeInImport = existing.IncludeInImport;
                    mapToNotes = existing.IsNotesMap;
                    if (mapToNotes)
                    {
                        suggested = 0;
                        qty = existing.QtyFactor;
                        packagingId = existing.PackagingID;
                        reason = "Order notes";
                    }
                    else
                    {
                        suggested = existing.ItemID;
                        qty = existing.QtyFactor;
                        packagingId = existing.PackagingID;
                        reason = "Saved mapping";
                        Item existingItem;
                        if (byId.TryGetValue(existing.ItemID, out existingItem))
                            suggestedDesc = existingItem.ItemDesc;
                    }
                }
                else if (!isParentGroup)
                {
                    Item match;
                    string matchReason;
                    if (TryMatchEnabledItem(p, bySku, enabledSkusLongestFirst, parentItemByProduct, byId,
                        out match, out matchReason))
                    {
                        suggested = match.ItemID;
                        suggestedDesc = match.ItemDesc;
                        reason = matchReason;
                    }
                    else if (WooProductMapRow.IsWooEnabledStatus(p.Status))
                    {
                        reason = "Needs Tracker item";
                    }
                }

                int serviceTypeId = 0;
                Item mappedItem;
                if (suggested > 0 && byId.TryGetValue(suggested, out mappedItem) && mappedItem.ItemServiceTypeID.HasValue)
                    serviceTypeId = mappedItem.ItemServiceTypeID.Value;

                if (!mapToNotes && !isParentGroup)
                    ApplyAttributeMaps(
                        MergeAttributesFromProductName(p.Attributes, p.Name, isVariation),
                        attrMaps, packagings, serviceTypeId, ref qty, ref packagingId, ref reason);

                bool applySelected;
                if (isParentGroup)
                {
                    // Only auto-tick Apply for modes that need a first-time parent map — not already-saved rows.
                    bool needsParentMap = string.Equals(importMode, WooProductMapRow.ImportModeParentNotes, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(importMode, WooProductMapRow.ImportModeParentItem, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(importMode, WooProductMapRow.ImportModeExclude, StringComparison.OrdinalIgnoreCase);
                    applySelected = needsParentMap && existing == null;
                }
                else
                    applySelected = mapToNotes || suggested > 0 || (existing != null && !mapToNotes && existing.ItemID > 0);

                string createSku = !string.IsNullOrWhiteSpace(p.Sku) ? p.Sku.Trim() : null;
                int? categorySort = WooCategoryFilterRepository.ResolveDefaultSortValue(p.CategoryIds, categoryFilters);
                if (!categorySort.HasValue)
                    categorySort = ItemSortOrdersRepository.MatchFromCategories(p.CategoriesLabel, sortLookups);
                int createSort = categorySort.HasValue && categorySort.Value > 0 ? categorySort.Value : 1;
                Item skuSource;
                if (suggested > 0 && byId.TryGetValue(suggested, out skuSource))
                {
                    // Item SKU box defaults to Woo SKU when creating. Tracker SKU is only
                    // copied for already-mapped rows (rename), not for a new Create.
                    if (existing != null && !string.IsNullOrWhiteSpace(skuSource.SKU))
                        createSku = skuSource.SKU.Trim();
                    if (existing != null && skuSource.SortOrder.HasValue && skuSource.SortOrder.Value > 0)
                        createSort = skuSource.SortOrder.Value;
                }
                if (string.IsNullOrWhiteSpace(createSku) && isParentGroup && !string.IsNullOrWhiteSpace(p.ParentSku))
                    createSku = p.ParentSku.Trim();

                var row = new WooProductMapRow
                {
                    WooProductId = p.Id,
                    WooVariationId = variationId,
                    Name = string.IsNullOrWhiteSpace(p.Name) ? p.ParentName : p.Name,
                    Sku = p.Sku,
                    ParentSku = p.ParentSku,
                    ParentName = p.ParentName,
                    Status = p.Status,
                    CategoriesLabel = p.CategoriesLabel,
                    AttributesLabel = isParentGroup
                        ? BuildParentGroupAttributesLabel(p.Attributes, variantNames, priorities)
                        : BuildVariantAttributesLabel(p.Attributes, priorities, isVariation, p.Name, notesNames),
                    NotesAttributeSummary = BuildCheckoutExtrasSummary(
                        MergeAttributesFromProductName(p.Attributes, p.Name, isVariation),
                        notesNames,
                        packSourceNames,
                        parentByName,
                        isParentGroup),
                    MatchReason = reason,
                    SuggestedItemID = suggested,
                    SuggestedItemDesc = suggestedDesc,
                    MappedItemID = mapToNotes ? WooProductMapRow.DestinationNotesValue : (existing != null && !isParentGroup ? existing.ItemID : suggested),
                    ExistingMappingID = existing != null ? existing.MappingID : 0,
                    QtyFactor = existing != null ? existing.QtyFactor : qty,
                    PackagingID = existing != null ? existing.PackagingID : packagingId,
                    SuggestedPackagingID = packagingId,
                    IsVariation = isVariation,
                    IsParentGroup = isParentGroup,
                    ImportMode = importMode,
                    UsesCategoryImportDefault = usesCategoryImportDefault,
                    ImportModeUserSet = false,
                    Depth = isVariation ? 1 : 0,
                    IncludeInImport = includeInImport,
                    MapToNotes = mapToNotes,
                    ApplySelected = applySelected,
                    VariationTotalCount = p.VariationTotalCount,
                    VariationInStockCount = p.VariationInStockCount,
                    CreateSku = createSku,
                    CreateSortOrder = createSort,
                    IsNewSinceLastSync = IsNewSinceSync(p.FirstSeenUtc, newSinceCutoff)
                };

                allRows.Add(row);
            }

            // Sync parent ImportMode onto children; build parent attribute summary from children when empty.
            SuggestParentItemModeForSingleInStock(allRows, categoryLockedImportModes);
            ApplyParentImportModes(allRows);
            MarkParentSavedFromChildren(allRows);
            foreach (var row in allRows)
            {
                if (row != null && row.HasSavedMapping)
                    row.CaptureSavedSnapshot();
            }

            List<WooProductMapRow> mappingRows;
            List<WooProductMapRow> missingSkuRows;
            SplitMappingAndMissingSku(allRows, out mappingRows, out missingSkuRows);

            mappingRows = mappingRows
                .OrderBy(r => r.FamilySortKey, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.WooProductId)
                .ThenBy(r => r.IsParentGroup ? 0 : 1)
                .ThenBy(r => r.AttributesLabel ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.WooVariationId ?? 0)
                .ThenBy(r => r.DisplaySku, StringComparer.OrdinalIgnoreCase)
                .ToList();
            missingSkuRows = missingSkuRows
                .OrderBy(r => r.FamilySortKey, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.WooProductId)
                .ThenBy(r => r.IsParentGroup ? 0 : 1)
                .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.WooVariationId ?? 0)
                .ToList();

            if (touchLastSync)
            {
                settings.LastItemsSyncUtc = catalogSyncCutoff ?? DateTime.UtcNow;
                new WooCommerceSettingsRepository().SaveSettings(settings, updatedBy);
                int newCount = allRows.Count(r => r != null && r.IsNewSinceLastSync);
                AppLogger.WriteLog("woo",
                    "Pulled " + mappingRows.Count + " mappable / " + missingSkuRows.Count
                    + " missing-SKU (scanned " + parentsScanned + " parents"
                    + (hitCap ? ", HIT CAP" : "")
                    + (newCount > 0 ? ", " + newCount + " new since previous pull" : "")
                    + ")",
                    updatedBy);

                return new WooProductPullResult
                {
                    MappingRows = mappingRows,
                    MissingSkuRows = missingSkuRows,
                    ParentsScanned = parentsScanned,
                    HitCatalogCap = hitCap,
                    NewSinceLastSyncCount = newCount
                };
            }

            return new WooProductPullResult
            {
                MappingRows = mappingRows,
                MissingSkuRows = missingSkuRows,
                ParentsScanned = parentsScanned,
                HitCatalogCap = hitCap,
                NewSinceLastSyncCount = allRows.Count(r => r != null && r.IsNewSinceLastSync)
            };
        }

        private static bool IsNewSinceSync(DateTime? firstSeenUtc, DateTime? syncCutoffUtc)
        {
            if (!firstSeenUtc.HasValue || !syncCutoffUtc.HasValue)
                return false;
            return firstSeenUtc.Value >= syncCutoffUtc.Value;
        }

        /// <summary>
        /// Parents without a SKU go to Missing SKUs (even if variants have SKUs).
        /// A parent with no mappable variants is kept off the Mappings grid.
        /// </summary>
        private static void SplitMappingAndMissingSku(
            List<WooProductMapRow> allRows,
            out List<WooProductMapRow> mappingRows,
            out List<WooProductMapRow> missingSkuRows)
        {
            mappingRows = new List<WooProductMapRow>();
            missingSkuRows = new List<WooProductMapRow>();
            if (allRows == null || allRows.Count == 0)
                return;

            var children = allRows.Where(r => !r.IsParentGroup).ToLookup(r => r.WooProductId);
            foreach (var row in allRows)
            {
                if (row.IsParentGroup)
                {
                    var kids = children[row.WooProductId].ToList();
                    var kidsWithSku = kids.Where(k => k.HasOwnSku).ToList();
                    if (!row.HasOwnSku)
                    {
                        missingSkuRows.Add(CloneAsMissingParent(row));
                        row.MatchReason = kidsWithSku.Count > 0
                            ? "Parent has no SKU — variants below"
                            : "Parent has no SKU";
                    }

                    if (kidsWithSku.Count == 0)
                        continue;

                    mappingRows.Add(row);
                    continue;
                }

                if (row.HasOwnSku)
                    mappingRows.Add(row);
                else
                {
                    if (string.IsNullOrWhiteSpace(row.MatchReason)
                        || string.Equals(row.MatchReason, "Unmapped", StringComparison.OrdinalIgnoreCase))
                        row.MatchReason = "Variation has no SKU";
                    missingSkuRows.Add(row);
                }
            }
        }

        private static WooProductMapRow CloneAsMissingParent(WooProductMapRow row)
        {
            return new WooProductMapRow
            {
                WooProductId = row.WooProductId,
                WooVariationId = null,
                Name = row.DisplayName,
                Sku = row.Sku,
                ParentSku = row.ParentSku,
                ParentName = row.ParentName ?? row.Name,
                Status = row.Status,
                CategoriesLabel = row.CategoriesLabel,
                AttributesLabel = "parent (no SKU)",
                MatchReason = "Parent has no SKU",
                IsParentGroup = true,
                IsVariation = false,
                Depth = 0,
                ApplySelected = false,
                IncludeInImport = false
            };
        }

        /// <summary>
        /// One in-stock variant usually means the rest are out of stock — map/create the parent SKU
        /// instead of treating that single variation as the product.
        /// </summary>
        private static void SuggestParentItemModeForSingleInStock(List<WooProductMapRow> rows, HashSet<long> categoryLockedImportModes = null)
        {
            if (rows == null || rows.Count == 0)
                return;

            var children = rows.Where(r => !r.IsParentGroup).ToLookup(r => r.WooProductId);
            foreach (var parent in rows.Where(r => r.IsParentGroup))
            {
                if (parent.ExistingMappingID > 0)
                    continue;
                if (categoryLockedImportModes != null && categoryLockedImportModes.Contains(parent.WooProductId))
                    continue;
                if (parent.ImportParentAsNotes || parent.ImportParentAsItem || parent.ImportParentExcluded)
                    continue;
                if (string.IsNullOrWhiteSpace(parent.DisplaySku)
                    || string.Equals(parent.DisplaySku, "(parent)", StringComparison.Ordinal))
                    continue;
                var kids = children[parent.WooProductId].ToList();
                int inStock = parent.VariationInStockCount > 0 ? parent.VariationInStockCount : kids.Count;
                if (inStock != 1 || kids.Count != 1)
                    continue;
                if (kids.Any(c => c.ExistingMappingID > 0))
                    continue;
                parent.ImportMode = WooProductMapRow.ImportModeParentItem;
                parent.UsesCategoryImportDefault = false;
                parent.ApplySelected = true;
                if (parent.MappedItemID <= 0 && parent.SuggestedItemID <= 0)
                    parent.MappedItemID = WooProductMapRow.DestinationCreateParent;
                var only = kids[0];
                if (parent.QtyFactor <= 1 && only.QtyFactor > 0)
                    parent.QtyFactor = only.QtyFactor;
                if (!parent.PackagingID.HasValue)
                    parent.PackagingID = only.PackagingID ?? only.SuggestedPackagingID;
            }
        }

        /// <summary>Variants parents show Saved when any variation map exists.</summary>
        private static void MarkParentSavedFromChildren(List<WooProductMapRow> rows)
        {
            if (rows == null || rows.Count == 0)
                return;
            var savedProducts = new HashSet<long>(
                rows.Where(r => r != null && !r.IsParentGroup && r.ExistingMappingID > 0)
                    .Select(r => r.WooProductId));
            foreach (var parent in rows.Where(r => r != null && r.IsParentGroup))
                parent.ChildrenHaveSavedMapping = savedProducts.Contains(parent.WooProductId);
        }

        /// <summary>Propagate parent ImportMode to variant children (notes mode disables variant apply/import).</summary>
        public static void ApplyParentImportModes(List<WooProductMapRow> rows)
        {
            if (rows == null || rows.Count == 0)
                return;

            var parents = rows.Where(r => r.IsParentGroup).ToDictionary(r => r.WooProductId);
            foreach (var parent in parents.Values)
            {
                var children = rows.Where(r => !r.IsParentGroup && r.WooProductId == parent.WooProductId).ToList();
                if (string.IsNullOrWhiteSpace(parent.Sku))
                {
                    string fromChild = children
                        .Select(c => c.ParentSku)
                        .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    if (!string.IsNullOrWhiteSpace(fromChild))
                        parent.Sku = fromChild.Trim();
                }
                if (string.IsNullOrWhiteSpace(parent.ParentSku) && !string.IsNullOrWhiteSpace(parent.Sku))
                    parent.ParentSku = parent.Sku;
                foreach (var child in children)
                {
                    if (string.IsNullOrWhiteSpace(child.ParentSku) && !string.IsNullOrWhiteSpace(parent.Sku))
                        child.ParentSku = parent.Sku;
                    if (string.IsNullOrWhiteSpace(child.ParentName))
                        child.ParentName = parent.DisplayName;
                }

                if (children.Count > 0 && (string.IsNullOrWhiteSpace(parent.AttributesLabel) || parent.AttributesLabel == "—"))
                {
                    var opts = children
                        .Select(c => c.AttributesLabel)
                        .Where(a => !string.IsNullOrWhiteSpace(a) && a != "—")
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(8)
                        .ToList();
                    if (opts.Count > 0)
                        parent.AttributesLabel = string.Join(" · ", opts);
                }

                bool notesMode = parent.ImportParentAsNotes;
                bool parentItemMode = parent.ImportParentAsItem;
                bool excludeMode = parent.ImportParentExcluded;
                int inStock = parent.VariationInStockCount > 0 ? parent.VariationInStockCount : children.Count;
                int total = parent.VariationTotalCount > inStock ? parent.VariationTotalCount : inStock;
                string stockLabel = WooProductMapRow.FormatInStockReason(inStock, total);
                if (excludeMode)
                {
                    parent.MapToNotes = false;
                    parent.IncludeInImport = false;
                    if (parent.ExistingMappingID == 0)
                        parent.ApplySelected = true;
                    parent.MatchReason = children.Count == 0
                        ? "Do not import"
                        : ("Do not import (" + stockLabel + ")");
                    foreach (var child in children)
                    {
                        child.ImportMode = WooProductMapRow.ImportModeExclude;
                        child.IncludeInImport = false;
                        child.ApplySelected = false;
                        child.MatchReason = "Excluded with parent";
                    }
                }
                else if (notesMode)
                {
                    parent.MapToNotes = true;
                    parent.MappedItemID = WooProductMapRow.DestinationNotesValue;
                    // Keep IncludeInImport from DB / category default — do not force true (that marked every Notes row dirty).
                    if (parent.ExistingMappingID == 0 && !parent.UsesCategoryImportDefault)
                        parent.IncludeInImport = true;
                    if (parent.ExistingMappingID == 0)
                        parent.ApplySelected = true;
                    parent.MatchReason = parent.UsesCategoryImportDefault && !parent.ImportModeUserSet
                        ? "Parent → order notes (category default)"
                        : "Parent → order notes";
                    foreach (var child in children)
                    {
                        child.ImportMode = WooProductMapRow.ImportModeParentNotes;
                        child.IncludeInImport = false;
                        child.ApplySelected = false;
                        child.MatchReason = "Skipped (parent → notes)";
                    }
                }
                else if (parentItemMode)
                {
                    parent.MapToNotes = false;
                    parent.IncludeInImport = true;
                    if (parent.MappedItemID != WooProductMapRow.DestinationCreateParent)
                    {
                        if (parent.MappedItemID <= 0 && parent.SuggestedItemID > 0)
                            parent.MappedItemID = parent.SuggestedItemID;
                        else if (parent.MappedItemID <= 0)
                            parent.MappedItemID = WooProductMapRow.DestinationCreateParent;
                    }
                    if (parent.ExistingMappingID == 0)
                        parent.ApplySelected = true;
                    if (parent.MappedItemID == WooProductMapRow.DestinationCreateParent)
                    {
                        if (string.IsNullOrWhiteSpace(parent.CreateSku)
                            && !string.IsNullOrWhiteSpace(parent.WooCreateSkuDefault))
                            parent.CreateSku = parent.WooCreateSkuDefault;
                    }
                    else if (string.IsNullOrWhiteSpace(parent.CreateSku)
                        && !string.IsNullOrWhiteSpace(parent.WooCreateSkuDefault))
                        parent.CreateSku = parent.WooCreateSkuDefault;
                    parent.MatchReason = children.Count == 0
                        ? "Parent SKU only"
                        : ("Parent SKU only; " + stockLabel + " → notes");
                    foreach (var child in children)
                    {
                        child.ImportMode = WooProductMapRow.ImportModeParentItem;
                        child.IncludeInImport = false;
                        child.ApplySelected = false;
                        child.MapToNotes = true;
                        child.MatchReason = "Variant → order notes";
                    }
                }
                else
                {
                    parent.MapToNotes = false;
                    // UI forces Destination "(variants)" (value 0). Keep MappedItemID in sync or every
                    // parent with a SuggestedItemID looks dirty forever and Save skips it.
                    parent.MappedItemID = 0;
                    parent.IncludeInImport = false;
                    // Do not clear Apply when the user explicitly chose Variants after editing Mode.
                    if (!parent.ImportModeUserSet)
                        parent.ApplySelected = parent.ExistingMappingID > 0;
                    parent.MatchReason = children.Count == 0
                        ? "Import variants"
                        : stockLabel;
                    foreach (var child in children)
                    {
                        child.ImportMode = WooProductMapRow.ImportModeVariants;
                        // Do not leave Notes destination id with MapToNotes=false — Save used to
                        // treat MappedItemID==-1 as Notes and rewrite Exact rows incorrectly.
                        if (child.MappedItemID == WooProductMapRow.DestinationNotesValue && !child.MapToNotes)
                        {
                            if (child.ExistingMappingID > 0 && child.SuggestedItemID > 0)
                                child.MappedItemID = child.SuggestedItemID;
                            else if (child.SuggestedItemID > 0)
                                child.MappedItemID = child.SuggestedItemID;
                            else
                                child.MappedItemID = 0;
                        }
                        child.MapToNotes = child.MappedItemID == WooProductMapRow.DestinationNotesValue;
                        if (string.Equals(child.MatchReason, "Excluded with parent", StringComparison.Ordinal))
                            child.IncludeInImport = WooProductMapRow.IsWooEnabledStatus(child.Status);
                        if (string.Equals(child.MatchReason, "Skipped (parent → notes)", StringComparison.Ordinal)
                            || string.Equals(child.MatchReason, "Variant → order notes", StringComparison.Ordinal)
                            || string.Equals(child.MatchReason, "Excluded (variant → order notes)", StringComparison.Ordinal)
                            || string.Equals(child.MatchReason, "Excluded with parent", StringComparison.Ordinal))
                        {
                            child.MatchReason = child.SuggestedItemID > 0 ? "Matched SKU" : "Unmapped";
                            if (child.SuggestedItemID > 0 && child.MappedItemID <= 0)
                                child.MappedItemID = child.SuggestedItemID;
                        }
                    }
                }
            }
        }

        /// <summary>Re-match Tracker items for variant rows after switching a parent to Import variants.</summary>
        public void RematchUnmappedChildren(IList<WooProductMapRow> rows, long productId)
        {
            if (rows == null || rows.Count == 0)
                return;
            var items = _itemsRepo.GetAll("SKU") ?? new List<Item>();
            var bySku = items
                .Where(i => !string.IsNullOrWhiteSpace(i.SKU))
                .GroupBy(i => i.SKU.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.ItemEnabled != false).First(),
                    StringComparer.OrdinalIgnoreCase);
            var skusLongestFirst = bySku.Keys
                .OrderByDescending(s => s.Length)
                .ThenBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var child in rows.Where(r => r != null && !r.IsParentGroup && r.WooProductId == productId))
            {
                if (child.ExistingMappingID > 0 && child.MappedItemID > 0)
                    continue;
                if (child.MapToNotes)
                    continue;
                Item match;
                string reason;
                if (!TryMatchSku(child.Sku, child.ParentSku, bySku, skusLongestFirst, out match, out reason))
                    continue;
                child.SuggestedItemID = match.ItemID;
                child.SuggestedItemDesc = match.ItemDesc;
                if (child.MappedItemID <= 0 || child.MappedItemID == WooProductMapRow.DestinationNotesValue)
                    child.MappedItemID = match.ItemID;
                child.MatchReason = reason;
                child.MapToNotes = false;
            }
        }

        private static string BuildParentGroupAttributesLabel(
            List<WooAttributeValue> attributes,
            HashSet<string> variantNames,
            Dictionary<string, int> priorities)
        {
            if (attributes == null || attributes.Count == 0)
                return string.Empty;

            IEnumerable<WooAttributeValue> filtered = attributes
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Option) && !a.IsAnyOption);
            if (variantNames != null && variantNames.Count > 0)
            {
                filtered = filtered.Where(a =>
                    !string.IsNullOrWhiteSpace(a.Name)
                    && variantNames.Contains(a.Name.Trim()));
            }

            var options = OrderAttributes(filtered, priorities)
                .Select(a => a.Option.Trim())
                .Where(o => o.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToList();
            return options.Count == 0 ? string.Empty : string.Join(" · ", options);
        }

        /// <summary>
        /// Show every attribute on the variation as "Name: Option".
        /// Order by parent display sort (qty/pack/note ranks).
        /// </summary>
        private static string BuildVariantAttributesLabel(
            List<WooAttributeValue> attributes,
            Dictionary<string, int> priorities,
            bool isVariation,
            string productName,
            HashSet<string> notesNames)
        {
            var merged = MergeAttributesFromProductName(attributes, productName, isVariation);
            if (merged.Count == 0)
                return isVariation ? "—" : string.Empty;

            var parts = OrderAttributes(
                    merged.Where(a => a != null && (!string.IsNullOrWhiteSpace(a.Option) || a.IsAnyOption)),
                    priorities)
                .Select(a =>
                {
                    string opt = a.IsAnyOption ? "(at checkout)" : a.DisplayOption;
                    string name = (a.Name ?? string.Empty).Trim();
                    if (name.Length == 0)
                        return opt;
                    string label = name + ": " + opt;
                    if (notesNames != null && notesNames.Contains(name))
                        label += " → notes";
                    return label;
                })
                .Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (parts.Count == 0)
                return isVariation ? "—" : string.Empty;
            return string.Join(" · ", parts);
        }

        private static List<WooAttributeValue> MergeAttributesFromProductName(
            List<WooAttributeValue> attributes,
            string productName,
            bool isVariation)
        {
            var list = attributes == null
                ? new List<WooAttributeValue>()
                : new List<WooAttributeValue>(attributes);
            if (!isVariation || list.Count >= 2)
                return list;
            string name = productName ?? string.Empty;
            int dash = name.LastIndexOf(" - ", StringComparison.Ordinal);
            if (dash < 0)
                return list;
            string tail = name.Substring(dash + 3).Trim();
            if (tail.Length == 0)
                return list;
            var existing = new HashSet<string>(
                list.Select(a => (a.Option ?? string.Empty).Trim()).Where(o => o.Length > 0),
                StringComparer.OrdinalIgnoreCase);
            foreach (string bit in tail.Split(new[] { ',', '·', '|', '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string opt = bit.Trim();
                if (opt.Length == 0 || existing.Contains(opt))
                    continue;
                string attrName = PackagingWeightParser.TryParseQtyFactorFromOption(opt).HasValue
                    ? "Packaging"
                    : "Prep Type";
                list.Add(new WooAttributeValue { Name = attrName, Option = opt });
                existing.Add(opt);
            }
            return list;
        }

        /// <summary>Prep Type (and other Notes-channel attributes) for the Tracker order note.</summary>
        public static string FormatNotesAttributes(List<WooAttributeValue> attributes, HashSet<string> notesNames)
        {
            if (attributes == null || attributes.Count == 0 || notesNames == null || notesNames.Count == 0)
                return string.Empty;
            var parts = new List<string>();
            foreach (var a in attributes)
            {
                if (a == null || string.IsNullOrWhiteSpace(a.Name) || !notesNames.Contains(a.Name.Trim()))
                    continue;
                if (a.IsAnyOption)
                    continue;
                string opt = a.DisplayOption;
                if (string.IsNullOrWhiteSpace(opt) || opt == "(any)")
                    continue;
                parts.Add(a.Name.Trim() + ": " + opt);
            }
            return parts.Count == 0 ? string.Empty : string.Join("; ", parts);
        }

        /// <summary>
        /// Shows pack-cascade attrs (e.g. Prep Type at checkout → pack #1) and note-cascade attrs
        /// so Mappings makes clear grind/pack still applies even when the Woo variation is Any.
        /// </summary>
        private static string BuildCheckoutExtrasSummary(
            List<WooAttributeValue> attributes,
            HashSet<string> notesNames,
            List<string> packSourceNames,
            Dictionary<string, WooAttributeParent> parentByName,
            bool isParentGroup)
        {
            var parts = new List<string>();
            var attrs = attributes ?? new List<WooAttributeValue>();

            if (packSourceNames != null)
            {
                foreach (string packName in packSourceNames)
                {
                    if (string.IsNullOrWhiteSpace(packName))
                        continue;
                    int rank = 1;
                    WooAttributeParent parent;
                    if (parentByName != null && parentByName.TryGetValue(packName, out parent) && parent != null)
                        rank = parent.PackRank;

                    var match = attrs.FirstOrDefault(a =>
                        a != null
                        && string.Equals((a.Name ?? string.Empty).Trim(), packName, StringComparison.OrdinalIgnoreCase));

                    if (match != null && !match.IsAnyOption
                        && !string.IsNullOrWhiteSpace(match.DisplayOption)
                        && match.DisplayOption != "(any)")
                    {
                        parts.Add(packName + ": " + match.DisplayOption.Trim() + " → pack #" + rank);
                    }
                    else if (isParentGroup)
                    {
                        parts.Add(packName + " → pack #" + rank + " (customer choice)");
                    }
                    else
                    {
                        parts.Add(packName + ": (at checkout) → pack #" + rank);
                    }
                }
            }

            string notesConcrete = FormatNotesAttributes(attrs, notesNames);
            if (!string.IsNullOrWhiteSpace(notesConcrete))
            {
                parts.Add(notesConcrete + " → notes");
            }
            else if (notesNames != null && notesNames.Count > 0)
            {
                // Avoid duplicating names already listed as pack sources.
                var noteOnly = notesNames
                    .Where(n => packSourceNames == null
                        || !packSourceNames.Any(p => string.Equals(p, n, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                if (noteOnly.Count > 0)
                {
                    if (isParentGroup)
                        parts.Add(string.Join(" · ", noteOnly) + " → notes (customer choice)");
                    else
                        parts.Add(string.Join(" · ", noteOnly) + ": (at checkout) → notes");
                }
            }

            return parts.Count == 0 ? string.Empty : string.Join(" · ", parts);
        }

        private static string BuildNotesAttributeSummary(
            List<WooAttributeValue> attributes,
            HashSet<string> notesNames,
            bool isParentGroup)
        {
            return BuildCheckoutExtrasSummary(
                attributes, notesNames, null, null, isParentGroup);
        }

        private static IEnumerable<WooAttributeValue> OrderAttributes(
            IEnumerable<WooAttributeValue> attributes,
            Dictionary<string, int> priorities)
        {
            return attributes
                .OrderBy(a =>
                {
                    int pri = 100;
                    string name = (a.Name ?? string.Empty).Trim();
                    if (priorities != null && name.Length > 0 && priorities.TryGetValue(name, out pri))
                        return pri;
                    return 100;
                })
                .ThenBy(a => a.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates an enabled Tracker item from this row's Woo SKU, or returns the existing
        /// item when that SKU is already in Tracker (so Save can map instead of failing).
        /// </summary>
        public Item CreateTrackerItemForWooRow(WooProductMapRow row, string updatedBy)
        {
            bool created;
            return CreateTrackerItemForWooRow(row, updatedBy, out created);
        }

        /// <summary>
        /// Creates an enabled Tracker item from this row's Woo SKU (variant SKU, or parent SKU on a group row).
        /// If the SKU already exists, returns that item (re-enables if disabled) instead of throwing.
        /// Does not write Woo mappings — caller still Saves maps.
        /// </summary>
        public Item CreateTrackerItemForWooRow(WooProductMapRow row, string updatedBy, out bool created)
        {
            created = false;
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            string sku = !string.IsNullOrWhiteSpace(row.CreateSku)
                ? row.CreateSku.Trim()
                : (row.IsParentGroup
                    ? ResolveBaseSku(row.ParentSku, row.Sku)
                    : (string.IsNullOrWhiteSpace(row.Sku) ? ResolveBaseSku(row.ParentSku, row.Sku) : row.Sku.Trim()));
            if (string.IsNullOrWhiteSpace(sku))
                throw new InvalidOperationException("Cannot create a Tracker item without a SKU.");

            var existing = _itemsRepo.GetBySku(sku);
            if (existing != null)
            {
                if (existing.ItemEnabled == false)
                {
                    existing.ItemEnabled = true;
                    _itemsRepo.SetEnabled(existing.ItemID, true);
                    AppLogger.WriteLog("woo", "Re-enabled Tracker item #" + existing.ItemID + " SKU " + sku, updatedBy);
                }
                else
                {
                    AppLogger.WriteLog("woo",
                        "Linked Woo row to existing Tracker item #" + existing.ItemID + " SKU " + sku,
                        updatedBy);
                }
                return existing;
            }

            string desc = ResolveCreateItemDesc(row, sku);
            string abbr = BuildAbbreviationFromSku(sku);

            var item = new Item
            {
                SKU = sku.Trim(),
                ItemDesc = Truncate(desc, 50),
                ItemShortName = Truncate(abbr, 6),
                ItemEnabled = true,
                SortOrder = row.CreateSortOrder,
                UnitsPerQty = 1
            };
            int id = _itemsRepo.InsertReturningId(item);
            item.ItemID = id;
            created = true;
            AppLogger.WriteLog("woo",
                "Created Tracker item #" + id + " SKU " + sku
                + " desc=\"" + item.ItemDesc + "\" abrv=\"" + item.ItemShortName + "\" from Woo",
                updatedBy);
            return item;
        }

        /// <summary>
        /// Tracker ItemDesc: variant rows use the Woo Variant column (attribute options),
        /// not the parent product title. Parent/simple rows use the product name.
        /// </summary>
        private static string ResolveCreateItemDesc(WooProductMapRow row, string sku)
        {
            if (row == null)
                return sku ?? string.Empty;

            if (!row.IsParentGroup)
            {
                string fromVariant = VariantOptionsForItemDesc(row.AttributesLabel);
                if (!string.IsNullOrWhiteSpace(fromVariant))
                    return fromVariant;

                string name = (row.Name ?? string.Empty).Trim();
                string parent = (row.ParentName ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(name) && name.Contains(" - "))
                {
                    int cut = name.LastIndexOf(" - ", StringComparison.Ordinal);
                    string tail = name.Substring(cut + 3).Trim();
                    if (tail.Length > 0)
                        return tail;
                }
                if (!string.IsNullOrEmpty(name)
                    && !string.Equals(name, parent, StringComparison.OrdinalIgnoreCase))
                    return name;
                if (!string.IsNullOrEmpty(parent))
                    return parent;
                return string.IsNullOrWhiteSpace(sku) ? name : sku;
            }

            string parentDesc = !string.IsNullOrWhiteSpace(row.Name)
                ? row.Name.Trim()
                : (!string.IsNullOrWhiteSpace(row.ParentName) ? row.ParentName.Trim() : sku);
            if (parentDesc != null && parentDesc.Contains(" - "))
            {
                int cut = parentDesc.LastIndexOf(" - ", StringComparison.Ordinal);
                if (cut > 0)
                    parentDesc = parentDesc.Substring(0, cut).Trim();
            }
            return string.IsNullOrWhiteSpace(parentDesc) ? (sku ?? string.Empty) : parentDesc;
        }

        /// <summary>
        /// AttributesLabel is "Packaging: bottle · Prep Type: (at checkout) → notes".
        /// Item names want the concrete options only (e.g. "bottle").
        /// </summary>
        private static string VariantOptionsForItemDesc(string attributesLabel)
        {
            if (string.IsNullOrWhiteSpace(attributesLabel)
                || string.Equals(attributesLabel.Trim(), "—", StringComparison.Ordinal))
                return null;

            var options = new List<string>();
            foreach (string part in attributesLabel.Split(new[] { '·' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string bit = part.Trim();
                if (bit.Length == 0)
                    continue;
                int arrow = bit.IndexOf(" → ", StringComparison.Ordinal);
                if (arrow >= 0)
                    bit = bit.Substring(0, arrow).Trim();
                int colon = bit.IndexOf(':');
                string opt = colon >= 0 && colon < bit.Length - 1
                    ? bit.Substring(colon + 1).Trim()
                    : bit;
                if (opt.Length == 0
                    || string.Equals(opt, "(any)", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(opt, "(at checkout)", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(opt, "—", StringComparison.Ordinal))
                    continue;
                if (!options.Contains(opt))
                    options.Add(opt);
            }
            return options.Count == 0 ? null : string.Join(" · ", options);
        }

        /// <summary>
        /// Abrv from SKU: drop digits and vowels, keep letters, max 6 (e.g. 8JuraDecT36 → JrDcT).
        /// </summary>
        internal static string BuildAbbreviationFromSku(string sku, int maxLen = 6)
        {
            if (string.IsNullOrWhiteSpace(sku) || maxLen <= 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder(maxLen);
            foreach (char c in sku.Trim())
            {
                if (!char.IsLetter(c))
                    continue;
                char upper = char.ToUpperInvariant(c);
                if (upper == 'A' || upper == 'E' || upper == 'I' || upper == 'O' || upper == 'U')
                    continue;
                sb.Append(upper);
                if (sb.Length >= maxLen)
                    break;
            }
            if (sb.Length > 0)
                return sb.ToString();

            // Fallback when SKU is only digits/vowels: first letters, no digits.
            foreach (char c in sku.Trim())
            {
                if (!char.IsLetter(c))
                    continue;
                sb.Append(char.ToUpperInvariant(c));
                if (sb.Length >= maxLen)
                    break;
            }
            return sb.ToString();
        }

        private static string Truncate(string value, int maxLen)
        {
            if (string.IsNullOrEmpty(value) || maxLen <= 0)
                return value ?? string.Empty;
            string t = value.Trim();
            return t.Length <= maxLen ? t : t.Substring(0, maxLen);
        }

        /// <summary>Renames Tracker SKU and/or sort order when the mapping row values differ.</summary>
        public void ApplySkuAndSort(int itemId, string sku, int sortOrder, string updatedBy)
        {
            if (itemId <= 0)
                return;
            var item = _itemsRepo.GetById(itemId);
            if (item == null)
                return;

            bool skuChanged = false;
            if (!string.IsNullOrWhiteSpace(sku))
            {
                string next = sku.Trim();
                if (!string.Equals(item.SKU ?? string.Empty, next, StringComparison.OrdinalIgnoreCase))
                {
                    var clash = _itemsRepo.GetBySku(next);
                    if (clash != null && clash.ItemID != itemId)
                        throw new InvalidOperationException(
                            MessageProvider.Format(MessageKeys.WooCommerce.MapSkuExistsInItems, next, clash.ItemID));
                    item.SKU = next;
                    skuChanged = true;
                }
            }

            int nextSort = sortOrder;
            bool sortChanged = (item.SortOrder ?? 1) != nextSort;
            if (sortChanged)
                item.SortOrder = nextSort;

            if (!skuChanged && !sortChanged)
                return;

            _itemsRepo.UpdateSkuAndSort(itemId, item.SKU, nextSort);
            AppLogger.WriteLog("woo",
                "Updated Tracker item #" + itemId
                + (skuChanged ? " SKU=" + item.SKU : "")
                + (sortChanged ? " sort=" + nextSort : ""),
                updatedBy);
        }

        private static string ResolveBaseSku(string parentSku, string sku)
        {
            if (!string.IsNullOrWhiteSpace(parentSku))
                return parentSku.Trim();
            return string.IsNullOrWhiteSpace(sku) ? null : sku.Trim();
        }

        private static bool TryMatchEnabledItem(
            WooProductDto p,
            Dictionary<string, Item> enabledBySku,
            List<string> enabledSkusLongestFirst,
            Dictionary<long, int> parentItemByProduct,
            Dictionary<int, Item> byId,
            out Item match,
            out string reason)
        {
            bool isVariation = p != null && p.VariationId.HasValue && p.VariationId.Value > 0;
            string sku = p == null ? string.Empty : (p.Sku ?? string.Empty).Trim();
            string parentSku = p == null ? string.Empty : (p.ParentSku ?? string.Empty).Trim();
            if (TryMatchSku(sku, parentSku, enabledBySku, enabledSkusLongestFirst, out match, out reason))
                return true;

            if (p != null && isVariation && parentItemByProduct.ContainsKey(p.Id))
            {
                int parentItemId = parentItemByProduct[p.Id];
                Item parentItem;
                if (byId.TryGetValue(parentItemId, out parentItem) && parentItem.ItemEnabled != false)
                {
                    match = parentItem;
                    reason = "Parent product map";
                    return true;
                }
            }

            match = null;
            reason = null;
            return false;
        }

        private static bool TryMatchSku(
            string sku,
            string parentSku,
            Dictionary<string, Item> bySku,
            List<string> skusLongestFirst,
            out Item match,
            out string reason)
        {
            match = null;
            reason = null;
            sku = (sku ?? string.Empty).Trim();
            parentSku = (parentSku ?? string.Empty).Trim();

            Item item;
            if (!string.IsNullOrEmpty(sku) && bySku.TryGetValue(sku, out item))
            {
                match = item;
                reason = "Exact SKU";
                return true;
            }

            if (!string.IsNullOrEmpty(parentSku) && bySku.TryGetValue(parentSku, out item))
            {
                match = item;
                reason = "Parent SKU";
                return true;
            }

            if (!string.IsNullOrEmpty(parentSku) && !string.IsNullOrEmpty(sku)
                && sku.StartsWith(parentSku, StringComparison.OrdinalIgnoreCase)
                && bySku.TryGetValue(parentSku, out item))
            {
                match = item;
                reason = "Parent SKU prefix";
                return true;
            }

            string probe = !string.IsNullOrEmpty(sku) ? sku : parentSku;
            if (!string.IsNullOrEmpty(probe) && skusLongestFirst != null)
            {
                foreach (string candidate in skusLongestFirst)
                {
                    if (candidate.Length < 4)
                        continue;
                    if (probe.StartsWith(candidate, StringComparison.OrdinalIgnoreCase)
                        && bySku.TryGetValue(candidate, out item))
                    {
                        match = item;
                        reason = "SKU prefix";
                        return true;
                    }
                }
            }

            // Own SKU first so 8JuraProfiF wins over parent 8JuraProfiX.
            if (TryFuzzySku(sku, bySku, out match, out reason))
                return true;
            if (!string.Equals(parentSku, sku, StringComparison.OrdinalIgnoreCase)
                && TryFuzzySku(parentSku, bySku, out match, out reason))
                return true;

            if (TryStemSku(probe, bySku, out match, out reason))
                return true;

            return false;
        }

        private static bool TryFuzzySku(string target, Dictionary<string, Item> bySku, out Item match, out string reason)
        {
            match = null;
            reason = null;
            if (string.IsNullOrEmpty(target) || target.Length < 5 || bySku == null)
                return false;
            Item best = null;
            int bestDist = int.MaxValue;
            foreach (var kv in bySku)
            {
                int dist = LevenshteinDistance(target, kv.Key);
                int maxAllowed = target.Length >= 10 ? 2 : 1;
                if (dist > 0 && dist <= maxAllowed && dist < bestDist)
                {
                    bestDist = dist;
                    best = kv.Value;
                }
            }
            if (best == null)
                return false;
            match = best;
            reason = "Fuzzy SKU (~" + bestDist + ")";
            return true;
        }

        /// <summary>8JuraProfiX ↔ 8JuraProfiF: long shared stem, last character(s) differ.</summary>
        private static bool TryStemSku(string probe, Dictionary<string, Item> bySku, out Item match, out string reason)
        {
            match = null;
            reason = null;
            if (string.IsNullOrEmpty(probe) || probe.Length < 6 || bySku == null)
                return false;
            Item best = null;
            int bestPrefix = 0;
            int bestExtra = int.MaxValue;
            foreach (var kv in bySku)
            {
                int prefix = CommonPrefixLength(probe, kv.Key);
                if (prefix < 6)
                    continue;
                if (prefix < probe.Length * 0.7 && prefix < kv.Key.Length * 0.7)
                    continue;
                int extra = Math.Abs(probe.Length - kv.Key.Length) + (probe.Length - prefix) + (kv.Key.Length - prefix);
                if (prefix > bestPrefix || (prefix == bestPrefix && extra < bestExtra))
                {
                    bestPrefix = prefix;
                    bestExtra = extra;
                    best = kv.Value;
                }
            }
            if (best == null || bestPrefix < 6)
                return false;
            match = best;
            reason = "SKU stem";
            return true;
        }

        private static int CommonPrefixLength(string a, string b)
        {
            if (a == null || b == null)
                return 0;
            int n = Math.Min(a.Length, b.Length);
            int i = 0;
            while (i < n && char.ToUpperInvariant(a[i]) == char.ToUpperInvariant(b[i]))
                i++;
            return i;
        }

        private static int LevenshteinDistance(string a, string b)
        {
            if (a == null) a = string.Empty;
            if (b == null) b = string.Empty;
            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();
            int n = a.Length;
            int m = b.Length;
            if (n == 0) return m;
            if (m == 0) return n;
            var d = new int[n + 1, m + 1];
            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        public List<WooAttributeParent> GetAttributeParents()
        {
            return _attrParentRepo.GetAllOrdered();
        }

        /// <summary>
        /// Resolve Qty / Packaging / Notes from Woo order-line meta using Attribute parents PackRank cascade
        /// (e.g. Prep Type #1 primary, Packaging #2 if missing).
        /// </summary>
        public WooLineAttributeResolveResult ResolveOrderLineAttributes(
            IList<WooMetaDto> meta,
            int itemServiceTypeId,
            double baseQtyFactor,
            int? basePackagingId)
        {
            var result = new WooLineAttributeResolveResult
            {
                QtyFactor = baseQtyFactor > 0 ? baseQtyFactor : 1,
                PackagingId = basePackagingId.HasValue && basePackagingId.Value > 0 ? basePackagingId : null
            };

            List<WooAttributeValue> attributes = MetaToAttributeValues(meta);
            if (attributes.Count == 0)
                return result;

            var attrMaps = _attrRepo.GetActive() ?? new List<WooAttributeMap>();
            var parentByName = new Dictionary<string, WooAttributeParent>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in _attrParentRepo.GetAllOrdered() ?? new List<WooAttributeParent>())
            {
                if (p == null || !p.ContributesAnything)
                    continue;
                string name = (p.AttributeName ?? string.Empty).Trim();
                if (name.Length == 0 || parentByName.ContainsKey(name))
                    continue;
                parentByName[name] = p;
            }
            StampParentRanks(attrMaps, parentByName);

            attrMaps = attrMaps
                .Where(m => m != null && (m.QtyRank > 0 || m.PackRank > 0 || m.NoteRank > 0))
                .ToList();
            if (attrMaps.Count == 0)
                return result;

            var packagings = GetPackagingsForServiceType(
                itemServiceTypeId > 0 ? (int?)itemServiceTypeId : null);
            double qty = result.QtyFactor;
            int? packagingId = result.PackagingId;
            string reason = string.Empty;
            ApplyAttributeMaps(attributes, attrMaps, packagings, itemServiceTypeId, ref qty, ref packagingId, ref reason);

            result.QtyFactor = qty > 0 ? qty : result.QtyFactor;
            result.PackagingId = packagingId;
            result.Reason = reason;
            result.Applied = !string.IsNullOrWhiteSpace(reason)
                || (packagingId.HasValue && packagingId.Value > 0 && packagingId != basePackagingId)
                || Math.Abs(qty - (baseQtyFactor > 0 ? baseQtyFactor : 1)) > 0.0001;

            var notesNames = new HashSet<string>(
                parentByName.Values
                    .Where(p => p != null && p.ContributesNote)
                    .Select(p => (p.AttributeName ?? string.Empty).Trim())
                    .Where(n => n.Length > 0),
                StringComparer.OrdinalIgnoreCase);
            string notesSummary = FormatNotesAttributes(attributes, notesNames);
            if (!string.IsNullOrWhiteSpace(notesSummary))
            {
                foreach (string part in notesSummary.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!string.IsNullOrWhiteSpace(part))
                        result.NoteParts.Add(part.Trim());
                }
            }

            return result;
        }

        private static List<WooAttributeValue> MetaToAttributeValues(IList<WooMetaDto> meta)
        {
            var list = new List<WooAttributeValue>();
            if (meta == null)
                return list;

            foreach (WooMetaDto m in meta)
            {
                if (m == null)
                    continue;
                string key = (m.DisplayKey ?? m.Key ?? string.Empty).Trim();
                if (key.Length == 0)
                    continue;
                if (string.IsNullOrWhiteSpace(m.DisplayKey)
                    && key.StartsWith("_", StringComparison.Ordinal))
                    continue;

                string option = !string.IsNullOrWhiteSpace(m.DisplayValue)
                    ? m.DisplayValue.Trim()
                    : (m.Value ?? string.Empty).Trim();
                if (option.Length == 0)
                    continue;

                string name = key;
                if (name.StartsWith("pa_", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(3).Replace('-', ' ');

                list.Add(new WooAttributeValue
                {
                    Name = name,
                    Option = option
                });
            }

            return list;
        }

        public SyncResult PullAttributeParents(string updatedBy)
        {
            _settings.EnsureSchema();
            WooCommerceApiClient.ApiCredentials creds;
            string error;
            if (!_settings.TryGetApiCredentials(out creds, out error))
                return Fail(error);

            try
            {
                var parents = _api.GetGlobalAttributes(creds) ?? new List<WooGlobalAttributeDto>();
                foreach (var p in parents)
                {
                    _attrParentRepo.UpsertFromWoo(
                        p.Id,
                        p.Name,
                        p.Slug,
                        p.TermCount,
                        defaultUseForVariants: false);
                }

                AppLogger.WriteLog("woo", "Pulled " + parents.Count + " Woo attribute parents", updatedBy);
                return new SyncResult
                {
                    Succeeded = true,
                    Count = parents.Count,
                    Message = MessageProvider.Format(MessageKeys.WooCommerce.MapPullAttrParentsOk, parents.Count)
                };
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("woo", "PullAttributeParents failed: " + ex.Message, updatedBy);
                return Fail(ex.Message);
            }
        }

        public int SaveAttributeParentVariants(IEnumerable<WooAttributeParent> selections, string updatedBy)
        {
            _settings.EnsureSchema();
            int saved = 0;
            if (selections == null)
                return 0;
            foreach (var sel in selections)
            {
                if (sel == null || sel.ParentID <= 0)
                    continue;
                _attrParentRepo.UpdateSelection(
                    sel.ParentID,
                    sel.UseForVariants,
                    sel.QtyRank,
                    sel.PackRank,
                    sel.NoteRank);
                saved++;
            }
            AppLogger.WriteLog("woo", "Saved selection on " + saved + " attribute parents", updatedBy);
            return saved;
        }

        /// <summary>
        /// Stage 2: pull options only for parents marked UseForVariants.
        /// </summary>
        public List<WooAttributeMap> PullDistinctAttributes(string updatedBy)
        {
            WooCommerceApiClient.ApiCredentials creds;
            string error;
            if (!_settings.TryGetApiCredentials(out creds, out error))
                throw new InvalidOperationException(error);

            var used = _attrParentRepo.GetUsedForVariants() ?? new List<WooAttributeParent>();
            if (used.Count == 0)
                throw new InvalidOperationException(MessageProvider.Get(MessageKeys.WooCommerce.MapAttrNeedParents));
            var lineParents = used.Where(p => p != null && p.ContributesLine).ToList();
            if (lineParents.Count == 0)
                throw new InvalidOperationException(MessageProvider.Get(MessageKeys.WooCommerce.MapAttrNeedLineParents));

            var parentByName = used
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.AttributeName))
                .GroupBy(p => p.AttributeName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var options = new List<WooAttributeValue>();
            foreach (var parent in used)
            {
                options.AddRange(_api.GetAttributeTerms(creds, parent.WooAttributeId, parent.AttributeName));
            }

            var packagings = _packRepo.GetAll("ItemPackagingDesc") ?? new List<ItemPackaging>();
            // Prefer (all types) saved maps; ignore Coffee twin rows when suggesting.
            var saved = _attrRepo.GetAllWithLookups()
                .Where(m => m != null && m.ItemServiceTypeID == 0)
                .GroupBy(m => NormalizeKey(m.AttributeName, m.AttributeOption, 0), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var counts = new Dictionary<string, WooAttributeMap>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in options)
            {
                if (a == null || string.IsNullOrWhiteSpace(a.Name) || string.IsNullOrWhiteSpace(a.Option))
                    continue;
                string attrName = a.Name.Trim();
                WooAttributeParent parent;
                if (!parentByName.TryGetValue(attrName, out parent) || parent == null || !parent.ContributesAnything)
                    continue;

                string key = NormalizeKey(attrName, a.Option, 0);
                WooAttributeMap row;
                if (!counts.TryGetValue(key, out row))
                {
                    row = new WooAttributeMap
                    {
                        AttributeName = attrName,
                        AttributeOption = a.Option.Trim(),
                        ItemServiceTypeID = 0,
                        QtyFactor = 1,
                        MapRole = parent.DefaultMapRole(),
                        QtyRank = parent.QtyRank,
                        PackRank = parent.PackRank,
                        NoteRank = parent.NoteRank,
                        IsActive = true,
                        SampleCount = 0
                    };
                    counts[key] = row;
                }
                if (a.SampleCount > row.SampleCount)
                    row.SampleCount = a.SampleCount;
            }

            var result = new List<WooAttributeMap>();
            foreach (var row in counts.Values.OrderBy(r => r.AttributeName).ThenBy(r => r.AttributeOption))
            {
                WooAttributeParent parent;
                parentByName.TryGetValue(row.AttributeName ?? string.Empty, out parent);
                string role = parent != null ? parent.DefaultMapRole() : WooAttributeMapRoles.Both;
                row.MapRole = role;
                if (parent != null)
                {
                    row.QtyRank = parent.QtyRank;
                    row.PackRank = parent.PackRank;
                    row.NoteRank = parent.NoteRank;
                }

                WooAttributeMap existing;
                if (saved.TryGetValue(NormalizeKey(row.AttributeName, row.AttributeOption, 0), out existing))
                {
                    row.MapID = existing.MapID;
                    row.QtyFactor = existing.QtyFactor;
                    row.PackagingID = existing.PackagingID;
                    row.Notes = existing.Notes;
                    row.IsActive = existing.IsActive;
                    row.PackagingDesc = existing.PackagingDesc;
                    row.ItemServiceTypeName = existing.ItemServiceTypeName;
                    row.ItemServiceTypeID = 0;
                }

                if (WooAttributeMapRoles.IsNotesOnly(role))
                {
                    row.PackagingID = null;
                    row.PackagingDesc = null;
                }
                else
                {
                    if (WooAttributeMapRoles.AppliesQty(role))
                    {
                        double? weightQty = PackagingWeightParser.TryParseQtyFactorFromOption(row.AttributeOption);
                        if (weightQty.HasValue)
                        {
                            row.SuggestedQtyFactor = weightQty;
                            if (row.MapID <= 0 || row.QtyFactor <= 0 || Math.Abs(row.QtyFactor - 1.0) < 0.000001)
                                row.QtyFactor = weightQty.Value;
                        }
                    }
                    if (WooAttributeMapRoles.AppliesPackaging(role)
                        && (!row.PackagingID.HasValue || row.PackagingID.Value <= 0))
                    {
                        int? packId = SuggestPackaging(row.AttributeOption, packagings);
                        if (packId.HasValue)
                        {
                            row.SuggestedPackagingID = packId;
                            row.PackagingID = packId;
                        }
                    }
                    if (!WooAttributeMapRoles.AppliesPackaging(role))
                    {
                        row.PackagingID = null;
                        row.PackagingDesc = null;
                    }
                }

                result.Add(row);
            }

            foreach (var row in result)
            {
                if (row.MapID > 0)
                    continue;
                try
                {
                    row.MapID = SaveAttributeMap(row, updatedBy);
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog("woo", "Persist pulled attribute option failed: " + ex.Message, updatedBy);
                }
            }

            AppLogger.WriteLog("woo", "Pulled " + result.Count + " Woo attribute options (variant parents only)", updatedBy);
            return result;
        }

        public List<WooAttributeMap> GetAttributeMaps()
        {
            return _attrRepo.GetAllWithLookups();
        }

        public int SaveAttributeMap(WooAttributeMap map, string updatedBy)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (string.IsNullOrWhiteSpace(map.AttributeName) || string.IsNullOrWhiteSpace(map.AttributeOption))
                throw new ArgumentException("Attribute name and option are required.");

            map.AttributeName = map.AttributeName.Trim();
            map.AttributeOption = map.AttributeOption.Trim();
            map.MapRole = WooAttributeMapRoles.Normalize(map.MapRole);
            if (WooAttributeMapRoles.IsNotesOnly(map.MapRole))
                map.PackagingID = null;
            if (map.ItemServiceTypeID < 0)
                map.ItemServiceTypeID = 0;
            if (map.QtyFactor <= 0)
                map.QtyFactor = 1;

            if (map.MapID > 0)
            {
                var clash = _attrRepo.FindExact(map.AttributeName, map.AttributeOption, map.ItemServiceTypeID);
                if (clash != null && clash.MapID != map.MapID)
                {
                    int oldId = map.MapID;
                    map.MapID = clash.MapID;
                    _attrRepo.Update(map);
                    _attrRepo.DeleteMap(oldId);
                    AppLogger.WriteLog("woo", "Attribute map merged onto #" + map.MapID + ", removed #" + oldId, updatedBy);
                    return map.MapID;
                }
                _attrRepo.Update(map);
                AppLogger.WriteLog("woo", "Attribute map updated #" + map.MapID, updatedBy);
                return map.MapID;
            }

            var existing = _attrRepo.FindExact(map.AttributeName, map.AttributeOption, map.ItemServiceTypeID);
            if (existing != null)
            {
                map.MapID = existing.MapID;
                _attrRepo.Update(map);
                AppLogger.WriteLog("woo", "Attribute map upserted #" + map.MapID, updatedBy);
                return map.MapID;
            }

            int id = _attrRepo.Insert(map);
            AppLogger.WriteLog("woo", "Attribute map created #" + id, updatedBy);
            return id;
        }

        public void DeleteAttributeMap(int mapId, string updatedBy)
        {
            _attrRepo.DeleteMap(mapId);
            AppLogger.WriteLog("woo", "Attribute map deleted #" + mapId, updatedBy);
        }

        public List<ItemPackaging> GetPackagingsForServiceType(int? itemServiceTypeId)
        {
            var all = _packRepo.GetAll("ItemPackagingDesc") ?? new List<ItemPackaging>();
            if (!itemServiceTypeId.HasValue || itemServiceTypeId.Value <= 0)
                return all;

            var allowed = _packSvcRepo.GetAllowedPackagingIds(itemServiceTypeId.Value);
            if (allowed.Count == 0)
                return all;
            return all.Where(p => allowed.Contains(p.ItemPackagingID)).ToList();
        }

        public int SaveMapping(long productId, long? variationId, int itemId, bool mapToNotes, bool includeInImport,
            double qtyFactor, int? packagingId, string sku, string updatedBy, bool exclude = false,
            bool variantsParent = false)
        {
            if (variantsParent)
            {
                mapToNotes = false;
                itemId = 0;
                includeInImport = false;
                exclude = false;
            }
            else if (exclude)
            {
                mapToNotes = false;
                itemId = 0;
                includeInImport = false;
            }
            else if (!mapToNotes && itemId <= 0)
                throw new ArgumentException("Tracker item is required (or choose order notes).");

            var existing = _mapRepo.FindExact(productId, variationId);
            var settings = _settings.GetSettings();
            string scope = settings.DisableScopeDefault ?? "MappedOnly";
            string mapType = variantsParent ? "Variants" : (exclude ? "Exclude" : (mapToNotes ? "Notes" : "Exact"));
            int persistItemId = (mapToNotes || exclude || variantsParent) ? 0 : itemId;
            int? persistPack = (mapToNotes || exclude || variantsParent) ? null : packagingId;
            double persistQty = qtyFactor <= 0 ? 1 : qtyFactor;

            if (existing == null)
            {
                int id = _mapRepo.Insert(new WooItemMapping
                {
                    ItemID = persistItemId,
                    WooProductId = productId,
                    WooVariationId = variationId,
                    MapType = mapType,
                    SkuPattern = sku,
                    QtyFactor = persistQty,
                    PackagingID = persistPack,
                    DisableScope = scope,
                    IsActive = true,
                    IncludeInImport = includeInImport
                });
                AppLogger.WriteLog("woo", "Mapping created #" + id + " product=" + productId + " type=" + mapType, updatedBy);
                return id;
            }

            existing.ItemID = persistItemId;
            existing.MapType = mapType;
            existing.QtyFactor = persistQty;
            existing.PackagingID = persistPack;
            existing.SkuPattern = sku;
            existing.IsActive = true;
            existing.IncludeInImport = includeInImport;
            _mapRepo.Update(existing);
            AppLogger.WriteLog("woo", "Mapping updated #" + existing.MappingID + " type=" + mapType, updatedBy);
            return existing.MappingID;
        }

        public void ExcludeProductFromImport(long productId)
        {
            _mapRepo.SetIncludeInImportForProduct(productId, false);
        }

        /// <summary>Removes a parent-level Notes/Exclude/Exact map so variants mode can persist after Save.</summary>
        public void ClearParentProductMapping(long productId)
        {
            var existing = _mapRepo.FindExact(productId, null);
            if (existing == null)
                return;
            bool wasExclude = existing.IsExcludeMap;
            int id = existing.MappingID;
            _mapRepo.DeleteMapping(existing.MappingID);
            if (wasExclude)
                _mapRepo.SetIncludeInImportForProduct(productId, true);
            AppLogger.WriteLog("woo", "Parent mapping cleared #" + id + " product=" + productId, "system");
        }

        public List<WooItemMapping> GetMappings()
        {
            return _mapRepo.GetAllWithItems();
        }

        public void DeleteMapping(int mappingId, string updatedBy)
        {
            _mapRepo.DeleteMapping(mappingId);
            AppLogger.WriteLog("woo", "Mapping deleted #" + mappingId, updatedBy);
        }

        /// <summary>Writes SKUs back to Woo for missing-SKU rows. Returns success/fail counts.</summary>
        public SyncResult WriteMissingSkusToWoo(IEnumerable<WooProductMapRow> rows, string updatedBy)
        {
            WooCommerceApiClient.ApiCredentials creds;
            string error;
            if (!_settings.TryGetApiCredentials(out creds, out error))
                return Fail(error);

            int ok = 0;
            int fail = 0;
            var notes = new List<string>();
            if (rows == null)
                return new SyncResult { Succeeded = true, Count = 0, Message = "No SKUs to write." };

            var candidates = rows
                .Where(r => r != null
                    && (r.ApplySelected || !string.IsNullOrWhiteSpace(r.NewSku)))
                .ToList();
            if (candidates.Count == 0)
            {
                return new SyncResult
                {
                    Succeeded = false,
                    Count = 0,
                    Message = MessageProvider.Get(MessageKeys.WooCommerce.MapMissingSkuNothingToWrite)
                };
            }

            foreach (var row in candidates)
            {
                string sku = (row.NewSku ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(sku))
                {
                    fail++;
                    notes.Add("Missing SKU for product " + row.WooProductId);
                    continue;
                }

                // Matching an existing Tracker item SKU is expected (link Woo blank SKU to Tracker).
                // Only reject if Woo already has a different product using this SKU (API will say so).

                string detail;
                if (_api.UpdateCatalogSku(creds, row.WooProductId, row.WooVariationId, sku, out detail))
                {
                    row.Sku = sku;
                    try
                    {
                        _settings.EnsureSchema();
                        _catalogCache.UpdateSku(row.WooProductId, row.WooVariationId, sku);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog("woo", "Catalog cache SKU update failed: " + ex.Message, updatedBy);
                    }
                    ok++;
                    AppLogger.WriteLog("woo",
                        "Wrote SKU " + sku + " to product " + row.WooProductId + " var " + row.WooVariationId,
                        updatedBy);
                }
                else
                {
                    fail++;
                    string wooMsg = FormatWooSkuWriteError(sku, row.WooProductId, detail);
                    notes.Add(wooMsg);
                }
            }

            return new SyncResult
            {
                Succeeded = fail == 0 && ok > 0,
                Count = ok,
                Message = fail == 0
                    ? MessageProvider.Format(MessageKeys.WooCommerce.MapMissingSkuWriteOk, ok)
                    : MessageProvider.Format(MessageKeys.WooCommerce.MapMissingSkuWritePartial, ok, fail)
                      + (notes.Count > 0 ? " " + string.Join("; ", notes.Take(5)) : string.Empty)
            };
        }

        private static string FormatWooSkuWriteError(string sku, long productId, string detail)
        {
            string msg = ExtractWooApiMessage(detail);
            if (string.IsNullOrWhiteSpace(msg))
                msg = "Woo rejected the SKU update.";
            return MessageProvider.Format(MessageKeys.WooCommerce.MapSkuWooWriteFailed, sku, productId, msg);
        }

        /// <summary>Pull a readable message from Woo REST error bodies (JSON or HTTP … — {json}).</summary>
        private static string ExtractWooApiMessage(string detail)
        {
            if (string.IsNullOrWhiteSpace(detail))
                return null;
            string raw = detail.Trim();
            int dash = raw.IndexOf(" — ", StringComparison.Ordinal);
            if (dash >= 0 && dash + 3 < raw.Length)
                raw = raw.Substring(dash + 3).Trim();
            if (raw.StartsWith("{", StringComparison.Ordinal))
            {
                try
                {
                    var token = Newtonsoft.Json.Linq.JToken.Parse(raw);
                    var msg = token["message"];
                    if (msg != null && !string.IsNullOrWhiteSpace(msg.ToString()))
                        return msg.ToString().Trim();
                    var code = token["code"];
                    if (code != null && !string.IsNullOrWhiteSpace(code.ToString()))
                        return code.ToString().Trim();
                }
                catch
                {
                    // fall through
                }
            }
            return detail.Trim();
        }

        public SyncResult PushEnabledState(string updatedBy, bool dryRun)
        {
            WooCommerceApiClient.ApiCredentials creds;
            string error;
            if (!_settings.TryGetApiCredentials(out creds, out error))
                return Fail(error);

            var settings = _settings.GetSettings();
            if (!settings.PushEnabledStateToWoo)
                return Fail("Push enabled state is turned off in settings.");

            var maps = _mapRepo.GetAllWithItems()
                .Where(m => m.IsActive && m.WooProductId.HasValue && !m.IsNotesMap && !m.IsExcludeMap
                    && !m.IsVariantsMap && m.ItemID > 0)
                .ToList();
            var mapById = maps.ToDictionary(m => m.MappingID);
            var catalogStatus = BuildCatalogStatusLookup();
            var preview = BuildEnabledPushPreviewFromMaps(maps, catalogStatus);
            int ok = 0;
            int fail = 0;
            int unchanged = preview.Count(r => !r.NeedsChange);

            foreach (var row in preview)
            {
                if (dryRun)
                {
                    if (!row.NeedsChange && row.CurrentWooStatus.StartsWith("(unknown", StringComparison.Ordinal))
                        row.Result = "Pull products to compare";
                    else
                        row.Result = row.NeedsChange ? "Will update" : "Already matches";
                    if (row.NeedsChange)
                        ok++;
                    continue;
                }

                if (!row.NeedsChange)
                {
                    row.Result = row.CurrentWooStatus.StartsWith("(unknown", StringComparison.Ordinal)
                        ? "Skipped (unknown Woo status)"
                        : "Already matches";
                    continue;
                }

                string detail;
                if (_api.SetCatalogStatus(creds, row.WooProductId, row.WooVariationId, row.TrackerEnabled, out detail))
                {
                    WooItemMapping map;
                    if (mapById.TryGetValue(row.MappingID, out map))
                    {
                        map.LastSyncedUtc = DateTime.UtcNow;
                        map.LastWooStatus = detail;
                        _mapRepo.Update(map);
                    }
                    row.LastWooStatus = detail;
                    row.CurrentWooStatus = detail;
                    row.NeedsChange = false;
                    row.Result = "OK";
                    ok++;
                }
                else
                {
                    row.Result = "Failed: " + (detail ?? "unknown");
                    fail++;
                }
            }

            AppLogger.WriteLog("woo",
                (dryRun ? "Dry-run " : "") + "Push enabled: ok=" + ok + " fail=" + fail + " unchanged=" + unchanged,
                updatedBy);

            string prefix = dryRun ? "Dry-run: " : string.Empty;
            string msg = prefix + preview.Count + " mapped product(s). ";
            if (dryRun)
            {
                int willPublish = preview.Count(r => r.NeedsChange && r.TrackerEnabled);
                int willPrivate = preview.Count(r => r.NeedsChange && !r.TrackerEnabled);
                msg += willPublish + " → publish, " + willPrivate + " → private";
                if (unchanged > 0)
                    msg += ", " + unchanged + " already match Woo";
            }
            else
            {
                msg += ok + " updated";
                if (fail > 0)
                    msg += ", " + fail + " failed";
                if (unchanged > 0)
                    msg += ", " + unchanged + " unchanged";
            }

            return new SyncResult
            {
                Succeeded = fail == 0,
                Count = dryRun ? preview.Count(r => r.NeedsChange) : ok,
                FailCount = fail,
                UnchangedCount = unchanged,
                Rows = preview,
                Message = msg
            };
        }

        private Dictionary<string, string> BuildCatalogStatusLookup()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in _catalogCache.GetAllAsDtos())
            {
                if (p == null || string.IsNullOrWhiteSpace(p.Status))
                    continue;
                dict[CatalogStatusKey(p.Id, p.VariationId)] = p.Status.Trim();
            }
            return dict;
        }

        private static string CatalogStatusKey(long productId, long? variationId)
        {
            long varId = variationId.HasValue && variationId.Value > 0 ? variationId.Value : 0;
            return productId.ToString(CultureInfo.InvariantCulture) + ":" + varId.ToString(CultureInfo.InvariantCulture);
        }

        private static List<WooEnabledPushPreviewRow> BuildEnabledPushPreviewFromMaps(
            List<WooItemMapping> maps,
            Dictionary<string, string> catalogStatus)
        {
            if (maps == null || maps.Count == 0)
                return new List<WooEnabledPushPreviewRow>();

            if (catalogStatus == null)
                catalogStatus = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            return maps
                .Select(m =>
                {
                    bool enabled = m.ItemEnabled ?? true;
                    string newStatus = enabled ? "publish" : "private";
                    string lastPushed = (m.LastWooStatus ?? string.Empty).Trim();

                    string cacheStatus;
                    catalogStatus.TryGetValue(
                        CatalogStatusKey(m.WooProductId.Value, m.WooVariationId),
                        out cacheStatus);
                    cacheStatus = (cacheStatus ?? string.Empty).Trim();

                    string currentWoo = !string.IsNullOrEmpty(cacheStatus)
                        ? cacheStatus
                        : lastPushed;
                    bool canCompare = !string.IsNullOrEmpty(currentWoo);
                    bool needsChange = canCompare
                        && !string.Equals(currentWoo, newStatus, StringComparison.OrdinalIgnoreCase);

                    return new WooEnabledPushPreviewRow
                    {
                        MappingID = m.MappingID,
                        ItemID = m.ItemID,
                        ItemDesc = m.ItemDesc ?? string.Empty,
                        ItemSku = m.ItemSku ?? string.Empty,
                        TrackerEnabled = enabled,
                        WooProductId = m.WooProductId.Value,
                        WooVariationId = m.WooVariationId,
                        LastWooStatus = string.IsNullOrEmpty(lastPushed) ? string.Empty : lastPushed,
                        CurrentWooStatus = canCompare
                            ? currentWoo
                            : "(unknown — pull products on Mappings tab)",
                        NewWooStatus = newStatus,
                        NeedsChange = needsChange
                    };
                })
                .OrderBy(r => r.ItemDesc, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.WooProductId)
                .ThenBy(r => r.WooVariationId ?? 0)
                .ToList();
        }

        private Dictionary<long, int> BuildParentItemLookup()
        {
            var dict = new Dictionary<long, int>();
            foreach (var map in _mapRepo.GetAllWithItems())
            {
                if (!map.IsActive || !map.WooProductId.HasValue)
                    continue;
                if (map.IsExcludeMap || map.IsNotesMap || map.IsVariantsMap || map.ItemID <= 0)
                    continue;
                if (map.WooVariationId.HasValue && map.WooVariationId.Value > 0)
                    continue;
                dict[map.WooProductId.Value] = map.ItemID;
            }
            return dict;
        }

        private static void StampParentRanks(
            List<WooAttributeMap> maps,
            Dictionary<string, WooAttributeParent> parentByName)
        {
            if (maps == null || parentByName == null)
                return;
            foreach (var m in maps)
            {
                if (m == null)
                    continue;
                WooAttributeParent parent;
                if (!parentByName.TryGetValue(m.AttributeName ?? string.Empty, out parent) || parent == null)
                    continue;
                m.QtyRank = parent.QtyRank;
                m.PackRank = parent.PackRank;
                m.NoteRank = parent.NoteRank;
                m.MapRole = parent.DefaultMapRole();
            }
        }

        private static void ApplyAttributeMaps(
            List<WooAttributeValue> attributes,
            List<WooAttributeMap> maps,
            List<ItemPackaging> packagings,
            int itemServiceTypeId,
            ref double qty,
            ref int? packagingId,
            ref string reason)
        {
            if (attributes == null || attributes.Count == 0 || maps == null || maps.Count == 0)
                return;

            var matches = new List<WooAttributeMap>();
            foreach (var attr in attributes)
            {
                if (attr == null || string.IsNullOrWhiteSpace(attr.Name) || attr.IsAnyOption)
                    continue;
                WooAttributeMap match = FindAttributeMap(maps, attr.Name, attr.Option, itemServiceTypeId);
                if (match != null)
                    matches.Add(match);
            }
            if (matches.Count == 0)
                return;

            var parts = new List<string>();

            WooAttributeMap qtyWinner = matches
                .Where(m => m.QtyRank > 0
                    && WooAttributeMapRoles.AppliesQty(m.MapRole)
                    && m.QtyFactor > 0)
                .OrderBy(m => m.QtyRank)
                .ThenBy(m => m.AttributeName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (qtyWinner != null)
            {
                qty = qtyWinner.QtyFactor;
                parts.Add(qtyWinner.AttributeName + "=" + qtyWinner.AttributeOption
                    + "→qty " + qtyWinner.QtyFactor + " [#" + qtyWinner.QtyRank + "]");
            }

            int? comboPack = TryFindComboPackaging(matches, packagings);
            if (comboPack.HasValue)
            {
                packagingId = comboPack;
                parts.Add("combo pack " + comboPack.Value);
            }
            else
            {
                // PackRank cascade: #1 primary, #2 only if primary missing, etc.
                bool packSet = false;
                foreach (var packWinner in matches
                    .Where(m => m.PackRank > 0 && WooAttributeMapRoles.AppliesPackaging(m.MapRole))
                    .OrderBy(m => m.PackRank)
                    .ThenBy(m => m.AttributeName, StringComparer.OrdinalIgnoreCase))
                {
                    int? resolved = packWinner.PackagingID.HasValue && packWinner.PackagingID.Value > 0
                        ? packWinner.PackagingID
                        : SuggestPackaging(packWinner.AttributeOption, packagings);
                    if (!resolved.HasValue || resolved.Value <= 0)
                        continue;

                    packagingId = resolved;
                    parts.Add(packWinner.AttributeName + "=" + packWinner.AttributeOption
                        + "→pack " + resolved.Value + " [#" + packWinner.PackRank + "]");
                    packSet = true;
                    break;
                }

                if (!packSet)
                {
                    // No ranked pack map hit — still try option text against packaging lookup.
                    foreach (var m in matches.Where(x => WooAttributeMapRoles.AppliesPackaging(x.MapRole)))
                    {
                        int? suggested = SuggestPackaging(m.AttributeOption, packagings);
                        if (!suggested.HasValue || suggested.Value <= 0)
                            continue;
                        packagingId = suggested;
                        parts.Add(m.AttributeName + "=" + m.AttributeOption
                            + "→pack " + suggested.Value + " (suggested)");
                        break;
                    }
                }
            }

            if (parts.Count > 0)
            {
                string attrReason = "Attributes: " + string.Join("; ", parts);
                reason = string.Equals(reason, "Unmapped", StringComparison.OrdinalIgnoreCase)
                    ? attrReason
                    : reason + " + " + attrReason;
            }
        }

        /// <summary>
        /// When 2+ packaging-driving options match, prefer a packaging whose desc/symbol
        /// contains every option text (e.g. Size 500g + Grind Plunger → "500g GrndPlnger").
        /// </summary>
        private static int? TryFindComboPackaging(
            List<WooAttributeMap> matches,
            List<ItemPackaging> packagings)
        {
            if (packagings == null || packagings.Count == 0 || matches == null)
                return null;

            var optionTexts = matches
                .Where(m => WooAttributeMapRoles.AppliesPackaging(m.MapRole)
                    || PackagingWeightParser.TryParseQtyFactorFromOption(m.AttributeOption).HasValue)
                .Select(m => (m.AttributeOption ?? string.Empty).Trim())
                .Where(o => o.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (optionTexts.Count < 2)
                return null;

            int bestId = 0;
            int bestHits = 0;
            foreach (var p in packagings)
            {
                string desc = p.ItemPackagingDesc ?? string.Empty;
                string symbol = p.Symbol ?? string.Empty;
                string haystack = desc + " " + symbol;
                if (string.IsNullOrWhiteSpace(haystack))
                    continue;

                int hits = 0;
                foreach (string opt in optionTexts)
                {
                    if (OptionHitsPackaging(opt, desc, symbol))
                        hits++;
                }
                if (hits > bestHits)
                {
                    bestHits = hits;
                    bestId = p.ItemPackagingID;
                }
            }

            return bestHits >= 2 ? (int?)bestId : null;
        }

        private static bool OptionHitsPackaging(string option, string desc, string symbol)
        {
            if (string.IsNullOrWhiteSpace(option))
                return false;
            // Weight fragment (1kg / 250g) against packaging name.
            double? kg = PackagingWeightParser.TryParseQtyFactorFromOption(option);
            if (kg.HasValue)
            {
                string hay = (desc + " " + symbol).ToLowerInvariant();
                if (kg.Value >= 1.0)
                {
                    string kgText = kg.Value == Math.Floor(kg.Value)
                        ? ((int)kg.Value).ToString() + "kg"
                        : kg.Value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "kg";
                    if (hay.IndexOf(kgText, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                    if (hay.IndexOf(((int)kg.Value).ToString() + " kg", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                else
                {
                    int grams = (int)Math.Round(kg.Value * 1000.0);
                    if (hay.IndexOf(grams + "g", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }

            return PackagingOptionMatcher.SuggestPackagingId(option,
                new List<ItemPackaging>
                {
                    new ItemPackaging { ItemPackagingID = 1, ItemPackagingDesc = desc, Symbol = symbol }
                }).HasValue
                || (!string.IsNullOrEmpty(desc) && desc.IndexOf(option, StringComparison.OrdinalIgnoreCase) >= 0)
                || (!string.IsNullOrEmpty(symbol) && symbol.IndexOf(option, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static WooAttributeMap FindAttributeMap(
            List<WooAttributeMap> maps,
            string name,
            string option,
            int itemServiceTypeId)
        {
            WooAttributeMap typed = maps.FirstOrDefault(m =>
                m.IsActive
                && m.ItemServiceTypeID == itemServiceTypeId
                && itemServiceTypeId > 0
                && AttributeNameMatches(m.AttributeName, name)
                && AttributeOptionMatches(m.AttributeOption, option));
            if (typed != null)
                return typed;

            return maps.FirstOrDefault(m =>
                m.IsActive
                && m.ItemServiceTypeID == 0
                && AttributeNameMatches(m.AttributeName, name)
                && AttributeOptionMatches(m.AttributeOption, option));
        }

        private static bool AttributeNameMatches(string stored, string probe)
        {
            if (string.IsNullOrWhiteSpace(stored) || string.IsNullOrWhiteSpace(probe))
                return false;
            if (string.Equals(stored.Trim(), probe.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;
            string a = NormalizeAttrToken(stored);
            string b = NormalizeAttrToken(probe);
            return a.Length > 0 && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static bool AttributeOptionMatches(string stored, string probe)
        {
            if (string.IsNullOrWhiteSpace(stored) || string.IsNullOrWhiteSpace(probe))
                return false;
            if (string.Equals(stored.Trim(), probe.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;
            string a = NormalizeAttrToken(stored);
            string b = NormalizeAttrToken(probe);
            return a.Length > 0 && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeAttrToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            var chars = value.Trim().ToLowerInvariant()
                .Where(c => char.IsLetterOrDigit(c))
                .ToArray();
            return new string(chars);
        }

        private static int? SuggestPackaging(string option, List<ItemPackaging> packagings)
        {
            return PackagingOptionMatcher.SuggestPackagingId(option, packagings);
        }

        private static string NormalizeKey(string name, string option, int serviceTypeId)
        {
            return (name ?? string.Empty).Trim().ToLowerInvariant()
                + "|" + (option ?? string.Empty).Trim().ToLowerInvariant()
                + "|" + serviceTypeId;
        }

        private static List<WooProductDto> ApplyCategoryFilter(
            List<WooProductDto> products,
            string mode,
            List<WooCategoryFilter> filters)
        {
            mode = (mode ?? "All").Trim();
            if (filters == null || filters.Count == 0)
                return products;

            // If any category is unticked, honour ticks even when mode is still "All"
            // (users expect unticked Referral etc. to stay out of Mappings / Missing SKUs).
            bool anyUnticked = filters.Any(f => !f.IncludeInSync);
            bool useIncludeList = string.Equals(mode, "IncludeList", StringComparison.OrdinalIgnoreCase)
                || (string.Equals(mode, "All", StringComparison.OrdinalIgnoreCase) && anyUnticked);

            if (string.Equals(mode, "ExcludeList", StringComparison.OrdinalIgnoreCase))
            {
                var exclude = new HashSet<long>(filters.Where(f => !f.IncludeInSync).Select(f => f.WooCategoryId));
                if (exclude.Count == 0)
                    return products;
                return products.Where(p =>
                    p.CategoryIds == null
                    || p.CategoryIds.Count == 0
                    || !p.CategoryIds.Any(id => exclude.Contains(id))).ToList();
            }

            if (!useIncludeList)
                return products;

            var include = BuildEffectiveIncludeCategoryIds(filters);
            if (include.Count == 0)
                return new List<WooProductDto>();

            return products.Where(p =>
                p.CategoryIds != null
                && p.CategoryIds.Count > 0
                && p.CategoryIds.Any(id => include.Contains(id))).ToList();
        }

        /// <summary>
        /// Ticked categories, plus descendants that are not themselves unticked.
        /// Unticking Referral keeps Referral out even when a parent is ticked.
        /// </summary>
        private static HashSet<long> BuildEffectiveIncludeCategoryIds(List<WooCategoryFilter> filters)
        {
            var unticked = new HashSet<long>(filters.Where(f => !f.IncludeInSync).Select(f => f.WooCategoryId));
            var include = new HashSet<long>(filters.Where(f => f.IncludeInSync).Select(f => f.WooCategoryId));
            var childrenOf = filters.ToLookup(f => f.ParentWooCategoryId);

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

            // Explicitly unticked always win.
            include.ExceptWith(unticked);
            return include;
        }

        private static SyncResult Fail(string message)
        {
            return new SyncResult { Succeeded = false, Message = message };
        }
    }
}
