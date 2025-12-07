using System;
namespace TrackerDotNet.Classes.Poco
{
    public class TempOrder
    {
        public int TempOrderID { get; set; }
        public int OrderID { get; set; }
        public int? ContactID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? RoastDate { get; set; }
        public int? ItemID { get; set; }
        public int? ItemServiceTypeID { get; set; }
        public int? ItemPrepTypeID { get; set; }
        public int? ItemPackagingID { get; set; }
        public double? QtyOrdered { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public bool? Delivered { get; set; }
        public string Notes { get; set; }
    }
}
