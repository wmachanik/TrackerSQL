using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    [Serializable]
    public class RecurringOrder
    {
        public int RecurringOrderID { get; set; }
        public int? ContactID { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
        public int? DeliveryByID { get; set; }
        /// <summary>Order flag (InvoiceTypesTbl) copied onto every order this recurring order creates.</summary>
        public int? InvoiceTypeID { get; set; }
        public List<RecurringOrderItem> Items { get; set; } = new List<RecurringOrderItem>();
    }
}
