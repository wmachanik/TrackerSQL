namespace TrackerDotNet.Classes.Poco
{
    public class TrackedServiceItem
    {
        public int TrackerServiceItemID { get; set; }
        public int? ItemServiceTypeID { get; set; }
        public double? TypicalAvePerItem { get; set; }
        public string UsageDateFieldName { get; set; }
        public string UsageAveFieldName { get; set; }
        public bool? ThisItemSetsDailyAverage { get; set; }
        public string Notes { get; set; }
    }
}
