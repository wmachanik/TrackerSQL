namespace TrackerSQL.Models
{
    /// <summary>
    /// How a Woo attribute option map contributes to Tracker line fields.
    /// </summary>
    public static class WooAttributeMapRoles
    {
        public const string Both = "Both";
        public const string QtyOnly = "QtyOnly";
        public const string PackagingOnly = "PackagingOnly";

        public static string Normalize(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return Both;
            string r = role.Trim();
            if (string.Equals(r, QtyOnly, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(r, "Qty only", System.StringComparison.OrdinalIgnoreCase))
                return QtyOnly;
            if (string.Equals(r, PackagingOnly, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(r, "Packaging only", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(r, "Pack only", System.StringComparison.OrdinalIgnoreCase))
                return PackagingOnly;
            return Both;
        }

        public static bool AppliesQty(string role)
        {
            string n = Normalize(role);
            return n == Both || n == QtyOnly;
        }

        public static bool AppliesPackaging(string role)
        {
            string n = Normalize(role);
            return n == Both || n == PackagingOnly;
        }
    }

    /// <summary>
    /// Maps a Woo variation attribute name/option to Tracker qty and/or packaging.
    /// ItemServiceTypeID = 0 means the map applies to all item service types.
    /// </summary>
    public class WooAttributeMap
    {
        public int MapID { get; set; }
        public string AttributeName { get; set; }
        public string AttributeOption { get; set; }
        public double QtyFactor { get; set; } = 1;
        public int? PackagingID { get; set; }
        /// <summary>Both | QtyOnly | PackagingOnly</summary>
        public string MapRole { get; set; } = WooAttributeMapRoles.Both;
        public int ItemServiceTypeID { get; set; }
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; }

        // Display / pull helpers (not persisted unless MapID > 0)
        public string PackagingDesc { get; set; }
        public string ItemServiceTypeName { get; set; }
        public int SampleCount { get; set; }
        public double? SuggestedQtyFactor { get; set; }
        public int? SuggestedPackagingID { get; set; }
        /// <summary>From parent table at resolve time (not a map column).</summary>
        public int ResolvePriority { get; set; } = 100;

        public string RowKey
        {
            get
            {
                return (AttributeName ?? string.Empty).Trim().ToLowerInvariant()
                    + "|" + (AttributeOption ?? string.Empty).Trim().ToLowerInvariant()
                    + "|" + ItemServiceTypeID;
            }
        }
    }
}
