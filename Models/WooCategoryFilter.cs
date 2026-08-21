namespace TrackerSQL.Models
{
    public class WooCategoryFilter
    {
        public int FilterID { get; set; }
        public long WooCategoryId { get; set; }
        public string CategoryName { get; set; }
        public long ParentWooCategoryId { get; set; }
        public bool IncludeInSync { get; set; } = true;
        /// <summary>ItemsTbl.SortOrder default for new Tracker items in this Woo category. Null = inherit parent.</summary>
        public int? DefaultSortValue { get; set; }

        /// <summary>
        /// Default parent ImportMode for unmapped products in this category
        /// (Variants / ParentItem / ParentNotes / Exclude). Null/blank = inherit parent.
        /// </summary>
        public string DefaultImportMode { get; set; }

        /// <summary>UI-only tree depth after hierarchical sort.</summary>
        public int Depth { get; set; }
    }
}
