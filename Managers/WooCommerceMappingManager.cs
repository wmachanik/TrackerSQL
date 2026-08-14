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
        private readonly WooSkuWildcardRuleRepository _wildRepo = new WooSkuWildcardRuleRepository();
        private readonly ItemsRepository _itemsRepo = new ItemsRepository();

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

        public void SaveCategoryRow(int filterId, bool include, string updatedBy)
        {
            _catRepo.UpdateInclude(filterId, include);
            AppLogger.WriteLog("woo", "Category filter " + filterId + " include=" + include, updatedBy);
        }

        private static bool IsUncategorized(string name, long id)
        {
            if (id == 15) // WooCommerce default Uncategorized term id on many installs
                return true;
            return string.Equals((name ?? string.Empty).Trim(), "Uncategorized", StringComparison.OrdinalIgnoreCase);
        }

        public void SaveCategoryMode(string mode, string updatedBy)
        {
            _settings.SaveCategoryFilterMode(mode, updatedBy);
        }

        public List<WooProductMapRow> PullProductsForMapping(string updatedBy)
        {
            WooCommerceApiClient.ApiCredentials creds;
            string error;
            if (!_settings.TryGetApiCredentials(out creds, out error))
                throw new InvalidOperationException(error);

            var settings = _settings.GetSettings();
            var filters = _catRepo.GetAllOrdered();
            var products = _api.GetProductsAndVariations(creds);
            products = ApplyCategoryFilter(products, settings.CategoryFilterMode, filters);

            var items = _itemsRepo.GetAll("SKU") ?? new List<Item>();
            var bySku = items
                .Where(i => !string.IsNullOrWhiteSpace(i.SKU))
                .GroupBy(i => i.SKU.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var wildcards = _wildRepo.GetAllWithItems().Where(w => w.IsActive).ToList();
            var rows = new List<WooProductMapRow>();

            foreach (var p in products)
            {
                var existing = _mapRepo.FindExact(p.Id, p.VariationId);
                int suggested = 0;
                string suggestedDesc = null;
                double qty = 1;

                if (!string.IsNullOrWhiteSpace(p.Sku) && bySku.ContainsKey(p.Sku.Trim()))
                {
                    var item = bySku[p.Sku.Trim()];
                    suggested = item.ItemID;
                    suggestedDesc = item.ItemDesc;
                }
                else
                {
                    var wild = MatchWildcard(p.Sku, wildcards);
                    if (wild != null)
                    {
                        suggested = wild.ItemID;
                        suggestedDesc = wild.ItemDesc;
                        qty = wild.QtyFactor;
                    }
                }

                rows.Add(new WooProductMapRow
                {
                    WooProductId = p.Id,
                    WooVariationId = p.VariationId,
                    Name = p.Name,
                    Sku = p.Sku,
                    Status = p.Status,
                    CategoriesLabel = p.CategoriesLabel,
                    SuggestedItemID = suggested,
                    SuggestedItemDesc = suggestedDesc,
                    MappedItemID = existing != null ? existing.ItemID : suggested,
                    ExistingMappingID = existing != null ? existing.MappingID : 0,
                    QtyFactor = existing != null ? existing.QtyFactor : qty
                });
            }

            settings.LastItemsSyncUtc = DateTime.UtcNow;
            new WooCommerceSettingsRepository().SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo", "Pulled " + rows.Count + " Woo products/variations for mapping", updatedBy);
            return rows;
        }

        public int SaveMapping(long productId, long? variationId, int itemId, double qtyFactor, string sku, string updatedBy)
        {
            if (itemId <= 0)
                throw new ArgumentException("Tracker item is required.");

            var existing = _mapRepo.FindExact(productId, variationId);
            var settings = _settings.GetSettings();
            string scope = settings.DisableScopeDefault ?? "MappedOnly";

            if (existing == null)
            {
                int id = _mapRepo.Insert(new WooItemMapping
                {
                    ItemID = itemId,
                    WooProductId = productId,
                    WooVariationId = variationId,
                    MapType = "Exact",
                    SkuPattern = sku,
                    QtyFactor = qtyFactor <= 0 ? 1 : qtyFactor,
                    DisableScope = scope,
                    IsActive = true
                });
                AppLogger.WriteLog("woo", "Mapping created #" + id + " product=" + productId, updatedBy);
                return id;
            }

            existing.ItemID = itemId;
            existing.QtyFactor = qtyFactor <= 0 ? 1 : qtyFactor;
            existing.SkuPattern = sku;
            existing.IsActive = true;
            _mapRepo.Update(existing);
            AppLogger.WriteLog("woo", "Mapping updated #" + existing.MappingID, updatedBy);
            return existing.MappingID;
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

        public List<WooSkuWildcardRule> GetWildcardRules()
        {
            return _wildRepo.GetAllWithItems();
        }

        public int SaveWildcard(WooSkuWildcardRule rule, string updatedBy)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            if (string.IsNullOrWhiteSpace(rule.SkuPrefixPattern))
                throw new ArgumentException("SKU prefix pattern is required.");
            if (rule.ItemID <= 0)
                throw new ArgumentException("Tracker item is required.");

            rule.SkuPrefixPattern = rule.SkuPrefixPattern.Trim();
            if (rule.RuleID > 0)
            {
                _wildRepo.Update(rule);
                AppLogger.WriteLog("woo", "Wildcard rule updated #" + rule.RuleID, updatedBy);
                return rule.RuleID;
            }
            int id = _wildRepo.Insert(rule);
            AppLogger.WriteLog("woo", "Wildcard rule created #" + id, updatedBy);
            return id;
        }

        public void DeleteWildcard(int ruleId, string updatedBy)
        {
            _wildRepo.DeleteRule(ruleId);
            AppLogger.WriteLog("woo", "Wildcard rule deleted #" + ruleId, updatedBy);
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

            var maps = _mapRepo.GetAllWithItems().Where(m => m.IsActive && m.WooProductId.HasValue).ToList();
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

        private static List<WooProductDto> ApplyCategoryFilter(
            List<WooProductDto> products,
            string mode,
            List<WooCategoryFilter> filters)
        {
            mode = (mode ?? "All").Trim();
            if (string.Equals(mode, "All", StringComparison.OrdinalIgnoreCase) || filters == null || filters.Count == 0)
                return products;

            // IncludeList (and legacy ExcludeList): only products in categories marked Include.
            if (string.Equals(mode, "IncludeList", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "ExcludeList", StringComparison.OrdinalIgnoreCase))
            {
                var include = new HashSet<long>(filters.Where(f => f.IncludeInSync).Select(f => f.WooCategoryId));
                return products.Where(p => p.CategoryIds != null && p.CategoryIds.Any(id => include.Contains(id))).ToList();
            }

            return products;
        }

        private static WooSkuWildcardRule MatchWildcard(string sku, List<WooSkuWildcardRule> rules)
        {
            if (string.IsNullOrWhiteSpace(sku) || rules == null)
                return null;
            string s = sku.Trim();
            foreach (var r in rules)
            {
                string prefix = (r.SkuPrefixPattern ?? string.Empty).TrimEnd('*');
                if (string.IsNullOrEmpty(prefix) || !s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!string.IsNullOrWhiteSpace(r.SuffixToken))
                {
                    if (!s.EndsWith(r.SuffixToken.Trim(), StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                return r;
            }
            return null;
        }

        private static SyncResult Fail(string message)
        {
            return new SyncResult { Succeeded = false, Message = message };
        }
    }
}
