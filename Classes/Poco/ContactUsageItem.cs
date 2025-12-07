namespace TrackerDotNet.Classes.Poco
{
    public class ContactUsageItem
    {
        public int ContactUsageLineNo { get; set; }
        public int ContactID { get; set; }
        public System.DateTime? DeliveryDate { get; set; }
        public int? ItemProvidedID { get; set; }
        public double? QtyProvided { get; set; }
        public int? ItemPrepTypeID { get; set; }
        public int? ItemPackagingID { get; set; }
        public string Notes { get; set; }
    }
}
