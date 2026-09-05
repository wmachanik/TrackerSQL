using System;

namespace TrackerSQL.Models
{
    public class WooItemMapping
    {
        public int MappingID { get; set; }
        public int ItemID { get; set; }
        public long? WooProductId { get; set; }
        public long? WooVariationId { get; set; }
        public string MapType { get; set; } = "Exact";
        public string SkuPattern { get; set; }
        public double QtyFactor { get; set; } = 1;
        public int? PackagingID { get; set; }
        public string DisableScope { get; set; }
        public DateTime? LastSyncedUtc { get; set; }
        public string LastWooStatus { get; set; }
        public bool IsActive { get; set; } = true;
        /// <summary>When false, order import skips this Woo line.</summary>
        public bool IncludeInImport { get; set; } = true;

        // UI helpers (not DB columns — ignored if Insert maps all props; override repos)
        public string ItemDesc { get; set; }
        public string ItemSku { get; set; }
        public bool? ItemEnabled { get; set; }
        /// <summary>Display label for PackagingID (Symbol or short desc).</summary>
        public string PackagingDesc { get; set; }
        /// <summary>Woo product name from catalog cache (UI).</summary>
        public string WooProductLabel { get; set; }
        /// <summary>Woo variation name/attrs from catalog cache (UI).</summary>
        public string WooVariationLabel { get; set; }

        public bool IsNotesMap
        {
            get { return string.Equals(MapType, "Notes", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsExcludeMap
        {
            get { return string.Equals(MapType, "Exclude", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>Parent map that forces Import variants (overrides category default).</summary>
        public bool IsVariantsMap
        {
            get { return string.Equals(MapType, "Variants", StringComparison.OrdinalIgnoreCase); }
        }
    }
}
