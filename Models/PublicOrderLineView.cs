using System;

namespace TrackerSQL.Models
{
    public class PublicOrderLineView
    {
        public int OrderID { get; set; }
        public int ItemTypeID { get; set; }
        public double QuantityOrdered { get; set; }
        public int PackagingID { get; set; }
        public DateTime RequiredByDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
