using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Order header + line data used when inserting a new order line.
    /// </summary>
    public class OrderTblData
    {
        public int OrderID { get; set; }
        public long CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime PrepDate { get; set; }
        public int ToBeDeliveredBy { get; set; }
        public DateTime RequiredByDate { get; set; }
        public bool Confirmed { get; set; }
        public bool Done { get; set; }
        public bool Packed { get; set; }
        public bool InvoiceDone { get; set; }
        public string PurchaseOrder { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int ItemTypeID { get; set; }
        public double QuantityOrdered { get; set; }
        public int PrepTypeID { get; set; }
        public int PackagingID { get; set; }
    }
}
