using System;
using TrackerSQL.Classes;

namespace TrackerSQL.Models
{
    [Serializable]
    public class OrderHeaderData
    {
        public int OrderID { get; set; }
        public long CustomerID { get; set; }
        public int ToBeDeliveredBy { get; set; }
        public DateTime OrderDate { get; set; } = TimeZoneUtils.Now().Date;
        public DateTime PrepDate { get; set; } = TimeZoneUtils.Now().Date;
        public DateTime RequiredByDate { get; set; } = TimeZoneUtils.Now().Date;
        public bool Confirmed { get; set; } = true;
        public bool Done { get; set; }
        public bool InvoiceDone { get; set; }
        public string PurchaseOrder { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        /// <summary>Order flag (InvoiceTypesTbl). Null on update = keep the stored flag.</summary>
        public int? InvoiceTypeID { get; set; }
    }
}
