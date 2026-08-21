namespace TrackerSQL.Models
{
    /// <summary>
    /// Companion link: which packaging options apply to which Tracker item service type.
    /// Empty table = all packaging allowed for all types.
    /// </summary>
    public class WooPackagingServiceType
    {
        public int PackagingServiceTypeID { get; set; }
        public int ItemPackagingID { get; set; }
        public int ItemServiceTypeID { get; set; }
    }
}
