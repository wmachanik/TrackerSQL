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
        public const string NotesOnly = "NotesOnly";

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
                || string.Equals(r, "Pack only", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(r, "PrepOnly", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(r, "Prep only", System.StringComparison.OrdinalIgnoreCase))
                return PackagingOnly;
            if (string.Equals(r, NotesOnly, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(r, "Order notes", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(r, "Notes", System.StringComparison.OrdinalIgnoreCase))
                return NotesOnly;
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

        public static bool IsNotesOnly(string role)
        {
            return Normalize(role) == NotesOnly;
        }

        public static string DisplayLabel(string role)
        {
            string n = Normalize(role);
            if (n == QtyOnly) return "Qty";
            if (n == PackagingOnly) return "Pack";
            if (n == NotesOnly) return "Notes";
            return "Qty + pack";
        }
    }

    /// <summary>
    /// Maps a Woo variation attribute option to Tracker qty and/or packaging (and notes).
    /// Item (SKU) is not set here — that comes from product/variation mapping.
    /// ItemServiceTypeID = 0 means the map applies to all item service types.
    /// Ranks are stamped from Attribute parents at resolve time (not DB columns).
    /// </summary>
    public class WooAttributeMap
    {
        public int MapID { get; set; }
        public string AttributeName { get; set; }
        public string AttributeOption { get; set; }
        public double QtyFactor { get; set; } = 1;
        public int? PackagingID { get; set; }
        /// <summary>Both | QtyOnly | PackagingOnly | NotesOnly — derived from parent ranks on pull.</summary>
        public string MapRole { get; set; } = WooAttributeMapRoles.Both;
        public int ItemServiceTypeID { get; set; }
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; }

        public string PackagingDesc { get; set; }
        public string ItemServiceTypeName { get; set; }
        public int SampleCount { get; set; }
        public double? SuggestedQtyFactor { get; set; }
        public int? SuggestedPackagingID { get; set; }

        /// <summary>From parent: 0 = this attr does not set qty.</summary>
        public int QtyRank { get; set; }
        /// <summary>From parent: 0 = this attr does not set packaging.</summary>
        public int PackRank { get; set; }
        /// <summary>From parent: 0 = not appended to notes.</summary>
        public int NoteRank { get; set; }

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
