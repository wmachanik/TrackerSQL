using System;

namespace TrackerSQL.Models
{
    public class OrderLine
    {
        public int OrderLineID { get; set; }
        public int OrderID { get; set; }
        public int ItemID { get; set; }
        public double? QtyOrdered { get; set; }
        public int? PrepTypeID { get; set; }
        public int? PackagingID { get; set; }
    }
}
