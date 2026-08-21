using System;

namespace TrackerSQL.Models
{
    /// <summary>Lookup for ItemsTbl.SortOrder (major product categories).</summary>
    public class ItemSortOrder
    {
        public int SortOrderID { get; set; }
        /// <summary>Value stored on ItemsTbl.SortOrder (1 Coffee … 15 Groups).</summary>
        public int SortValue { get; set; }
        public string SortOrderDesc { get; set; }
        public bool? IsEnabled { get; set; } = true;

        public string DisplayText
        {
            get
            {
                string desc = (SortOrderDesc ?? string.Empty).Trim();
                if (desc.Length == 0)
                    return SortValue.ToString();
                return SortValue.ToString() + " - " + desc;
            }
        }
    }
}
