using System;

namespace TrackerDotNet.Classes.Poco
{
    public class RecurringOrderItem
    {
        public int RecurringOrderItemID { get; set; }
        public int RecurringOrderID { get; set; }
        public int? RecurringTypeID { get; set; }
        public int? ItemRequiredID { get; set; }
        public byte? Value { get; set; }
        public double? QtyRequired { get; set; }
    }
}
