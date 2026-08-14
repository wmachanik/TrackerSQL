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

        // UI helpers (not DB columns — ignored if Insert maps all props; override repos)
        public string ItemDesc { get; set; }
        public string ItemSku { get; set; }
        public bool? ItemEnabled { get; set; }
    }
}
