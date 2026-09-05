using System;

namespace TrackerSQL.Models
{
    public class WooOrderInfo
    {
        public int OrderID { get; set; }
        public long WooOrderId { get; set; }
        public string WooOrderNumber { get; set; }
        public string WooStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public bool PaymentPaid { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? LastSyncedUtc { get; set; }
        public string RawSnapshotJson { get; set; }
        /// <summary>Semicolon-separated import conflicts (also copied into order Notes).</summary>
        public string ImportConflicts { get; set; }
    }
}
