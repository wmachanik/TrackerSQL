using System;
using System.Collections.Generic;

namespace TrackerDotNet.Classes.Poco
{
    public class Order
    {
        public int OrderID { get; set; }
        public int? ContactID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? PrepDate { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public int? ToBeDeliveredByID { get; set; }
        public bool? Confirmed { get; set; }
        public bool? Done { get; set; }
        public bool? Packed { get; set; }
        public string Notes { get; set; }
        public string PurchaseOrder { get; set; }
        public bool? InvoiceDone { get; set; }
        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
    }
}
