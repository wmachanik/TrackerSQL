namespace TrackerDotNet.Classes.Poco
{
    public class TempOrdersLine
    {
        public int TOLineID { get; set; }
        public int? TOHeaderID { get; set; }
        public int? ItemID { get; set; }
        public int? ItemServiceTypeID { get; set; }
        public double? Qty { get; set; }
        public int? ItemPackagingID { get; set; }
        public int? OriginalOrderID { get; set; }
    }
}
