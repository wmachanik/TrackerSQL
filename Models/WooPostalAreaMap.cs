namespace TrackerSQL.Models
{
    /// <summary>Postcode range (+ optional suburb) mapped to a Tracker area for Woo order import.</summary>
    public class WooPostalAreaMap
    {
        public int MapID { get; set; }
        public int PostalFrom { get; set; }
        public int PostalTo { get; set; }
        /// <summary>Null, *, or blank = any suburb within the range. Otherwise case-insensitive substring match.</summary>
        public string SuburbMatch { get; set; }
        public int AreaID { get; set; }
        public int Priority { get; set; } = 100;
        public string Source { get; set; } = "Manual";
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; }

        public string AreaName { get; set; }
    }
}
