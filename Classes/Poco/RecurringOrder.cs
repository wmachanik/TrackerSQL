using System;
using System.Collections.Generic;

namespace TrackerDotNet.Classes.Poco
{
    public class RecurringOrder
    {
        public int RecurringOrderID { get; set; }
        public int ContactID { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
        public int? DeliveryByID { get; set; }
        public List<RecurringOrderItem> Items { get; set; } = new List<RecurringOrderItem>();
    }
}
