namespace TrackerDotNet.Classes.Poco
{
    public class ItemServiceType
    {
        public int ItemServiceTypeID { get; set; }
        public string ServiceTypeName { get; set; }
        public string Description { get; set; }
        public int? ItemPackagingID { get; set; }
        public int? ItemPrepTypeID { get; set; }
    }
}
