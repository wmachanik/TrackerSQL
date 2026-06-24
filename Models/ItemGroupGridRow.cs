namespace TrackerSQL.Models
{
    /// <summary>
    /// Grid row for ItemGroups page — legacy property names for ODS binding.
    /// </summary>
    public class ItemGroupGridRow
    {
        public int ItemGroupID { get; set; }
        public int GroupItemTypeID { get; set; }
        public int ItemTypeID { get; set; }
        public int ItemTypeSortPos { get; set; }
        public bool Enabled { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;
    }
}
