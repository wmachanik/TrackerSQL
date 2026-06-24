using System;



namespace TrackerSQL.Models
{
    public class Item : ILookupEntity
    {
        public int ItemID { get; set; }
        public string SKU { get; set; }
        public string ItemDesc { get; set; }
        public bool? ItemEnabled { get; set; }
        public string ItemsCharacteritics { get; set; }  // Note: Typo in database column name
        public string ItemDetail { get; set; }
        public int? ItemServiceTypeID { get; set; }
        public int? ReplacementItemID { get; set; }
        public int? ItemUnitID { get; set; }
        public double? BasePrice { get; set; }
        public string ItemShortName { get; set; }
        public int? SortOrder { get; set; }
        public double? UnitsPerQty { get; set; }

        // ILookupEntity implementation
        public int GetId() => ItemID;
        
        public string GetDisplayText() => ItemDesc ?? string.Empty;
        
        public bool? IsEnabled() => ItemEnabled;

        /// <summary>
        /// Gets formatted display text for dropdowns (disabled items prefixed with "_")
        /// </summary>
        public string FormattedDisplayText => LookupFormatter.FormatLookupText(ItemDesc, ItemEnabled);
    }
}





