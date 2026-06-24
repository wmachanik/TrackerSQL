using System;

namespace TrackerSQL.Models
{
    public class OrderEntryListItem
    {
        public int OrderID { get; set; }
        public string CompanyName { get; set; }
        public long CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime PrepDate { get; set; }
        public DateTime RequiredByDate { get; set; }
        public int ToBeDeliveredBy { get; set; }
        public int ItemTypeID { get; set; }
        public double QuantityOrdered { get; set; }
        public string Person { get; set; }
        public bool Confirmed { get; set; }
        public bool Done { get; set; }
        public string Notes { get; set; }

        public OrderEntryListItem()
        {
            CompanyName = string.Empty;
            Person = string.Empty;
            Notes = string.Empty;
        }
    }
}
