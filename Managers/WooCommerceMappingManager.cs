using System;
using System.Collections.Generic;
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
            _catalogCache.ReplaceAll(products);
            products = ApplyCategoryFilter(products, settings.CategoryFilterMode, filters);
            return BuildPullResult(products, parentsScanned, hitCap, updatedBy, touchLastSync: true, categoryFilters: filters);
        }

        private WooProductPullResult BuildPullResult(
            List<WooProductDto> products,
            int parentsScanned,
            bool hitCap,
            string updatedBy,
            bool touchLastSync,
            IList<WooCategoryFilter> categoryFilters = null)
        {
            if (products == null)
                products = new List<WooProductDto>();

            var items = _itemsRepo.GetAll("SKU") ?? new List<Item>();
            var byId = items.ToDictionary(i => i.ItemID);
            var enabledItems = items.Where(i => i.ItemEnabled != false && !string.IsNullOrWhiteSpace(i.SKU)).ToList();
            var enabledBySku = enabledItems
                .GroupBy(i => i.SKU.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            // Longest SKU first for prefix matching (9QRCcoFinc before 9QRC).
            var enabledSkusLongestFirst = enabledBySku.Keys
                .OrderByDescending(s => s.Length)
                .ThenBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var attrMaps = _attrRepo.GetActive();
            var variantNames = _attrParentRepo.GetVariantAttributeNames();
            var priorities = _attrParentRepo.GetPriorityByAttributeName();
            if (variantNames.Count > 0)
            {
                attrMaps = attrMaps.FindAll(m => variantNames.Contains(m.AttributeName ?? string.Empty));
                foreach (var m in attrMaps)
                {
                    int pri;
                    if (priorities.TryGetValue(m.AttributeName ?? string.Empty, out pri))
                        m.ResolvePriority = pri;
                }
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
                        if (TryMatchEnabledItem(p, enabledBySku, enabledSkusLongestFirst, parentItemByProduct, byId,
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
                    if (TryMatchEnabledItem(p, enabledBySku, enabledSkusLongestFirst, parentItemByProduct, byId,
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
                        : BuildVariantAttributesLabel(p.Attributes, priorities, isVariation, p.Name),
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
                    CreateSortOrder = createSort
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
                var settings = _settings.GetSettings();
                settings.LastItemsSyncUtc = DateTime.UtcNow;
                new WooCommerceSettingsRepository().SaveSettings(settings, updatedBy);
                AppLogger.WriteLog("woo",
                    "Pulled " + mappingRows.Count + " mappable / " + missingSkuRows.Count
                    + " missing-SKU (scanned " + parentsScanned + " parents"
                    + (hitCap ? ", HIT CAP" : "") + ")",
                    updatedBy);
            }

            return new WooProductPullResult
            {
                MappingRows = mappingRows,
                MissingSkuRows = missingSkuRows,
                ParentsScanned = parentsScanned,
                HitCatalogCap = hitCap
            };
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
                    if (parent.MappedItemID == WooProductMapRow.DestinationNotesValue
                        || parent.MappedItemID == WooProductMapRow.DestinationCreateParent)
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
                        child.MapToNotes = false;
                        if (string.Equals(child.MatchReason, "Excluded with parent", StringComparison.Ordinal))
                            child.IncludeInImport = WooProductMapRow.IsWooEnabledStatus(child.Status);
                        if (string.Equals(child.MatchReason, "Skipped (parent → notes)", StringComparison.Ordinal)
                            || string.Equals(child.MatchReason, "Variant → order notes", StringComparison.Ordinal)
                            || string.Equals(child.MatchReason, "Excluded (variant → order notes)", StringComparison.Ordinal)
                            || string.Equals(child.MatchReason, "Excluded with parent", StringComparison.Ordinal))
                            child.MatchReason = child.SuggestedItemID > 0 ? "Unmapped" : child.MatchReason;
                    }
                }
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
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Option));
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
        /// Show every attribute on the variation as "Name: Option" so Prep Type vs Packaging
        /// (and other multi-attribute products) are obviously different between rows.
        /// Order by parent ResolvePriority (then name).
        /// </summary>
        private static string BuildVariantAttributesLabel(
            List<WooAttributeValue> attributes,
            Dictionary<string, int> priorities,
            bool isVariation,
            string productName)
        {
            var merged = MergeAttributesFromProductName(attributes, productName, isVariation);
            if (merged.Count == 0)
                return isVariation ? "—" : string.Empty;

            var parts = OrderAttributes(
                    merged.Where(a => a != null && !string.IsNullOrWhiteSpace(a.Option)),
                    priorities)
                .Select(a =>
                {
                    string opt = a.Option.Trim();
                    string name = (a.Name ?? string.Empty).Trim();
                    if (name.Length == 0)
                        return opt;
                    return name + ": " + opt;
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
        /// Creates an enabled Tracker item from this row's Woo SKU (variant SKU, or parent SKU on a group row).
        /// Does not write Woo mappings — caller still Saves maps.
        /// </summary>
        public Item CreateTrackerItemForWooRow(WooProductMapRow row, string updatedBy)
        {
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
                    _itemsRepo.Update(existing);
                    AppLogger.WriteLog("woo", "Re-enabled Tracker item #" + existing.ItemID + " SKU " + sku, updatedBy);
                    return existing;
                }
                throw new InvalidOperationException(
                    MessageProvider.Format(MessageKeys.WooCommerce.MapSkuExistsInItems, sku, existing.ItemID));
            }

            string desc = !string.IsNullOrWhiteSpace(row.Name)
                ? row.Name.Trim()
                : sku;
            // Parent items: drop variation suffixes from Woo names. Variant items keep the full name.
            if (row.IsParentGroup && desc.Contains(" - "))
            {
                int cut = desc.LastIndexOf(" - ", StringComparison.Ordinal);
                if (cut > 0)
                    desc = desc.Substring(0, cut).Trim();
            }

            var item = new Item
            {
                SKU = sku.Trim(),
                ItemDesc = desc,
                ItemEnabled = true,
                SortOrder = row.CreateSortOrder,
                UnitsPerQty = 1
            };
            int id = _itemsRepo.InsertReturningId(item);
            item.ItemID = id;
            AppLogger.WriteLog("woo", "Created Tracker item #" + id + " SKU " + sku + " from Woo", updatedBy);
            return item;
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

            _itemsRepo.Update(item);
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
            match = null;
            reason = null;
            string sku = (p.Sku ?? string.Empty).Trim();
            string parentSku = (p.ParentSku ?? string.Empty).Trim();

            Item item;
            if (!string.IsNullOrEmpty(sku) && enabledBySku.TryGetValue(sku, out item))
            {
                match = item;
                reason = "Exact SKU";
                return true;
            }

            if (!string.IsNullOrEmpty(parentSku) && enabledBySku.TryGetValue(parentSku, out item))
            {
                match = item;
                reason = "Parent SKU";
                return true;
            }

            // Variation SKU starts with parent Woo SKU (9QRCcoFinc250g → parent 9QRCcoFinc).
            if (!string.IsNullOrEmpty(parentSku) && !string.IsNullOrEmpty(sku)
                && sku.StartsWith(parentSku, StringComparison.OrdinalIgnoreCase)
                && enabledBySku.TryGetValue(parentSku, out item))
            {
                match = item;
                reason = "Parent SKU prefix";
                return true;
            }

            // Longest enabled Tracker SKU that is a prefix of the Woo SKU.
            string probe = !string.IsNullOrEmpty(sku) ? sku : parentSku;
            if (!string.IsNullOrEmpty(probe))
            {
                foreach (string candidate in enabledSkusLongestFirst)
                {
                    if (candidate.Length < 4)
                        continue;
                    if (probe.StartsWith(candidate, StringComparison.OrdinalIgnoreCase)
                        && enabledBySku.TryGetValue(candidate, out item))
                    {
                        match = item;
                        reason = "SKU prefix";
                        return true;
                    }
                }
            }

            // Fuzzy: closest enabled SKU to parent (or own) SKU within small edit distance.
            string fuzzyTarget = !string.IsNullOrEmpty(parentSku) ? parentSku : sku;
            if (!string.IsNullOrEmpty(fuzzyTarget) && fuzzyTarget.Length >= 5)
            {
                Item best = null;
                int bestDist = int.MaxValue;
                foreach (var kv in enabledBySku)
                {
                    int dist = LevenshteinDistance(fuzzyTarget, kv.Key);
                    int maxAllowed = fuzzyTarget.Length >= 10 ? 2 : 1;
                    if (dist > 0 && dist <= maxAllowed && dist < bestDist)
                    {
                        bestDist = dist;
                        best = kv.Value;
                    }
                }
                if (best != null)
                {
                    match = best;
                    reason = "Fuzzy SKU (~" + bestDist + ")";
                    return true;
                }
            }

            // Previously saved parent product map (no variation).
            if (p.VariationId.HasValue && p.VariationId.Value > 0
                && parentItemByProduct.ContainsKey(p.Id))
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

            return false;
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

        public SyncResult PullAttributeParents(string updatedBy)
        {
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
            int saved = 0;
            if (selections == null)
                return 0;
            foreach (var sel in selections)
            {
                if (sel == null || sel.ParentID <= 0)
                    continue;
                int pri = sel.ResolvePriority <= 0 ? 100 : sel.ResolvePriority;
                _attrParentRepo.UpdateSelection(sel.ParentID, sel.UseForVariants, pri);
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

            var selected = _attrParentRepo.GetUsedForVariants();
            if (selected.Count == 0)
                throw new InvalidOperationException(MessageProvider.Get(MessageKeys.WooCommerce.MapAttrNeedParents));

            var options = new List<WooAttributeValue>();
            foreach (var parent in selected)
            {
                options.AddRange(_api.GetAttributeTerms(creds, parent.WooAttributeId, parent.AttributeName));
            }

            var packagings = _packRepo.GetAll("ItemPackagingDesc") ?? new List<ItemPackaging>();
            var saved = _attrRepo.GetAllWithLookups()
                .ToDictionary(m => NormalizeKey(m.AttributeName, m.AttributeOption, m.ItemServiceTypeID), StringComparer.OrdinalIgnoreCase);

            var counts = new Dictionary<string, WooAttributeMap>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in options)
            {
                if (a == null || string.IsNullOrWhiteSpace(a.Name) || string.IsNullOrWhiteSpace(a.Option))
                    continue;
                string key = NormalizeKey(a.Name, a.Option, 0);
                WooAttributeMap row;
                if (!counts.TryGetValue(key, out row))
                {
                    row = new WooAttributeMap
                    {
                        AttributeName = a.Name.Trim(),
                        AttributeOption = a.Option.Trim(),
                        ItemServiceTypeID = 0,
                        QtyFactor = 1,
                        MapRole = WooAttributeMapRoles.PackagingOnly,
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
                WooAttributeMap existing;
                if (saved.TryGetValue(NormalizeKey(row.AttributeName, row.AttributeOption, 0), out existing))
                {
                    row.MapID = existing.MapID;
                    row.QtyFactor = existing.QtyFactor;
                    row.PackagingID = existing.PackagingID;
                    row.MapRole = WooAttributeMapRoles.Normalize(existing.MapRole);
                    row.ItemServiceTypeID = existing.ItemServiceTypeID;
                    row.Notes = existing.Notes;
                    row.IsActive = existing.IsActive;
                    row.PackagingDesc = existing.PackagingDesc;
                    row.ItemServiceTypeName = existing.ItemServiceTypeName;

                    // Upgrade legacy Both-on-weight maps so Prep Type can own packaging.
                    double? weightQty = PackagingWeightParser.TryParseQtyFactorFromOption(row.AttributeOption);
                    if (weightQty.HasValue)
                    {
                        row.SuggestedQtyFactor = weightQty;
                        if (row.QtyFactor <= 0)
                            row.QtyFactor = weightQty.Value;
                        if (row.MapRole == WooAttributeMapRoles.Both
                            || row.MapRole == WooAttributeMapRoles.PackagingOnly)
                        {
                            row.MapRole = WooAttributeMapRoles.QtyOnly;
                            row.PackagingID = null;
                            row.PackagingDesc = null;
                        }
                    }
                    else if (!row.PackagingID.HasValue || row.PackagingID.Value <= 0)
                    {
                        int? packId = SuggestPackaging(row.AttributeOption, packagings);
                        if (packId.HasValue)
                        {
                            row.SuggestedPackagingID = packId;
                            row.PackagingID = packId;
                            if (row.MapRole == WooAttributeMapRoles.Both)
                                row.MapRole = WooAttributeMapRoles.PackagingOnly;
                        }
                    }
                }
                else
                {
                    double? suggestedQty = PackagingWeightParser.TryParseQtyFactorFromOption(row.AttributeOption);
                    if (suggestedQty.HasValue)
                    {
                        row.SuggestedQtyFactor = suggestedQty;
                        row.QtyFactor = suggestedQty.Value;
                        // Weight/size drives qty only — prep/grind owns packaging (e.g. GrndPlnger).
                        row.MapRole = WooAttributeMapRoles.QtyOnly;
                    }
                    else
                    {
                        row.MapRole = WooAttributeMapRoles.PackagingOnly;
                        int? packId = SuggestPackaging(row.AttributeOption, packagings);
                        if (packId.HasValue)
                        {
                            row.SuggestedPackagingID = packId;
                            row.PackagingID = packId;
                        }
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
            double qtyFactor, int? packagingId, string sku, string updatedBy, bool exclude = false)
        {
            if (exclude)
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
            string mapType = exclude ? "Exclude" : (mapToNotes ? "Notes" : "Exact");
            int persistItemId = (mapToNotes || exclude) ? 0 : itemId;
            int? persistPack = (mapToNotes || exclude) ? null : packagingId;
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
            _mapRepo.DeleteMapping(existing.MappingID);
            if (wasExclude)
                _mapRepo.SetIncludeInImportForProduct(productId, true);
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

            foreach (var row in rows)
            {
                if (row == null || !row.ApplySelected)
                    continue;
                string sku = (row.NewSku ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(sku))
                {
                    fail++;
                    notes.Add("Missing SKU for product " + row.WooProductId);
                    continue;
                }

                var clash = _itemsRepo.GetBySku(sku);
                if (clash != null)
                {
                    fail++;
                    notes.Add(MessageProvider.Format(MessageKeys.WooCommerce.MapSkuExistsInItems, sku, clash.ItemID));
                    continue;
                }

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
                Succeeded = fail == 0,
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
                .Where(m => m.IsActive && m.WooProductId.HasValue && !m.IsNotesMap && !m.IsExcludeMap && m.ItemID > 0)
                .ToList();
            int ok = 0;
            int fail = 0;
            var notes = new List<string>();

            foreach (var map in maps)
            {
                bool enabled = map.ItemEnabled ?? true;
                string detail;
                if (dryRun)
                {
                    notes.Add((enabled ? "ENABLE" : "DISABLE") + " product " + map.WooProductId + " var " + map.WooVariationId);
                    ok++;
                    continue;
                }

                if (_api.SetCatalogStatus(creds, map.WooProductId.Value, map.WooVariationId, enabled, out detail))
                {
                    map.LastSyncedUtc = DateTime.UtcNow;
                    map.LastWooStatus = detail;
                    _mapRepo.Update(map);
                    ok++;
                }
                else
                {
                    fail++;
                    notes.Add("Failed " + map.WooProductId + ": " + detail);
                }
            }

            AppLogger.WriteLog("woo",
                (dryRun ? "Dry-run " : "") + "Push enabled: ok=" + ok + " fail=" + fail, updatedBy);

            return new SyncResult
            {
                Succeeded = fail == 0,
                Count = ok,
                Message = (dryRun ? "Dry-run: " : "") + ok + " updated, " + fail + " failed."
                    + (notes.Count == 0 ? string.Empty : " " + string.Join("; ", notes.Take(5)))
            };
        }

        private Dictionary<long, int> BuildParentItemLookup()
        {
            var dict = new Dictionary<long, int>();
            foreach (var map in _mapRepo.GetAllWithItems())
            {
                if (!map.IsActive || !map.WooProductId.HasValue)
                    continue;
                if (map.IsExcludeMap || map.IsNotesMap || map.ItemID <= 0)
                    continue;
                if (map.WooVariationId.HasValue && map.WooVariationId.Value > 0)
                    continue;
                dict[map.WooProductId.Value] = map.ItemID;
            }
            return dict;
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
                if (attr == null || string.IsNullOrWhiteSpace(attr.Name) || string.IsNullOrWhiteSpace(attr.Option))
                    continue;
                WooAttributeMap match = FindAttributeMap(maps, attr.Name, attr.Option, itemServiceTypeId);
                if (match != null)
                    matches.Add(match);
            }
            if (matches.Count == 0)
                return;

            var parts = new List<string>();

            WooAttributeMap qtyWinner = matches
                .Where(m => WooAttributeMapRoles.AppliesQty(m.MapRole))
                .OrderBy(m => m.ResolvePriority)
                .ThenBy(m => QtyRoleRank(m.MapRole))
                .ThenBy(m => m.AttributeName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (qtyWinner != null && qtyWinner.QtyFactor > 0)
            {
                qty = qtyWinner.QtyFactor;
                parts.Add(qtyWinner.AttributeName + "=" + qtyWinner.AttributeOption + "→qty " + qtyWinner.QtyFactor);
            }

            var packCandidates = matches
                .Where(m => WooAttributeMapRoles.AppliesPackaging(m.MapRole)
                    && m.PackagingID.HasValue && m.PackagingID.Value > 0)
                .ToList();

            int? comboPack = TryFindComboPackaging(matches, packagings);
            if (comboPack.HasValue)
            {
                packagingId = comboPack;
                parts.Add("combo pack " + comboPack.Value);
            }
            else
            {
                // Prefer Prep Type / PackagingOnly over size options that also carry a pack
                // (e.g. "250g Box" Both). Explicit PackagingID on weight options is still applied
                // when nothing higher-ranked matches — those used to be filtered out entirely.
                WooAttributeMap packWinner = packCandidates
                    .OrderBy(m => m.ResolvePriority)
                    .ThenBy(m => PackRoleRank(m.MapRole))
                    .ThenBy(m => PackagingWeightParser.TryParseQtyFactorFromOption(m.AttributeOption).HasValue ? 1 : 0)
                    .ThenBy(m => m.AttributeName, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (packWinner != null)
                {
                    packagingId = packWinner.PackagingID;
                    parts.Add(packWinner.AttributeName + "=" + packWinner.AttributeOption
                        + "→pack " + packWinner.PackagingID);
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

        /// <summary>Prefer QtyOnly over Both when priorities tie.</summary>
        private static int QtyRoleRank(string role)
        {
            string n = WooAttributeMapRoles.Normalize(role);
            if (n == WooAttributeMapRoles.QtyOnly) return 0;
            if (n == WooAttributeMapRoles.Both) return 1;
            return 2;
        }

        /// <summary>Prefer PackagingOnly over Both when priorities tie.</summary>
        private static int PackRoleRank(string role)
        {
            string n = WooAttributeMapRoles.Normalize(role);
            if (n == WooAttributeMapRoles.PackagingOnly) return 0;
            if (n == WooAttributeMapRoles.Both) return 1;
            return 2;
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
                && string.Equals(m.AttributeName, name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m.AttributeOption, option, StringComparison.OrdinalIgnoreCase));
            if (typed != null)
                return typed;

            return maps.FirstOrDefault(m =>
                m.IsActive
                && m.ItemServiceTypeID == 0
                && string.Equals(m.AttributeName, name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m.AttributeOption, option, StringComparison.OrdinalIgnoreCase));
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
