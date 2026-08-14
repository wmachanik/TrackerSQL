using System.Collections.Generic;

namespace TrackerSQL.Models
{
    public class WooCategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long Parent { get; set; }
        public int Count { get; set; }
    }

    public class WooProductDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public long? ParentId { get; set; }
        public long? VariationId { get; set; }
        public List<long> CategoryIds { get; set; } = new List<long>();
        public string CategoriesLabel { get; set; }
    }

    /// <summary>Row for the mapping pull grid (session-backed).</summary>
    public class WooProductMapRow
    {
        public long WooProductId { get; set; }
        public long? WooVariationId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string Status { get; set; }
        public string CategoriesLabel { get; set; }
        public int SuggestedItemID { get; set; }
        public string SuggestedItemDesc { get; set; }
        public int MappedItemID { get; set; }
        public int ExistingMappingID { get; set; }
        public double QtyFactor { get; set; } = 1;
        public string RowKey
        {
            get { return WooProductId + ":" + (WooVariationId ?? 0); }
        }
    }
}
