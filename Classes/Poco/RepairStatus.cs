namespace TrackerDotNet.Classes.Poco
{
    public class RepairStatus
    {
        public int RepairStatusID { get; set; }
        public string RepairStatusDesc { get; set; }
        public bool? EmailContact { get; set; }
        public int? SortOrder { get; set; }
        public string Notes { get; set; }
        public string StatusNote { get; set; }
    }
}
