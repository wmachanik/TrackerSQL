using System;
namespace TrackerDotNet.Classes.Poco
{
    public class TempCoffeecheckupItem
    {
        public int TCIID { get; set; }
        public int ContactID { get; set; }
        public int? ItemID { get; set; }
        public double? ItemQty { get; set; }
        public int? ItemPrepID { get; set; }
        public int? ItemPackagingID { get; set; }
        public bool? AutoFulfill { get; set; }
        public DateTime? NextDateRequired { get; set; }
        public int? RecurringOrderItemID { get; set; }
    }
}
