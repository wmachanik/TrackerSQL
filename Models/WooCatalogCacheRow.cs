using System;

namespace TrackerSQL.Models
{
    /// <summary>Persisted Woo catalog snapshot (filled only on Sync products).</summary>
    public class WooCatalogCacheRow
    {
        public int CacheID { get; set; }
        public long WooProductId { get; set; }
        public long WooVariationId { get; set; }
        public bool IsParentGroup { get; set; }
        public bool IsVariation { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string ParentSku { get; set; }
        public string ParentName { get; set; }
        public string Status { get; set; }
        public string StockStatus { get; set; }
        public string ProductType { get; set; }
        public string CategoriesLabel { get; set; }
        public string CategoryIds { get; set; }
        public string AttributesJson { get; set; }
        public int VariationTotalCount { get; set; }
        public int VariationInStockCount { get; set; }
        public DateTime PulledUtc { get; set; }
    }
}
