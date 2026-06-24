using System;

namespace TrackerSQL.Models
{
    public class OrderDoneHeaderView
    {
        public string CompanyName { get; set; }
        public int CustomerID { get; set; }
        public DateTime RequiredByDate { get; set; }
    }

    public class OrderDoneLineView
    {
        public int TOLineID { get; set; }
        public int ItemID { get; set; }
        public float Qty { get; set; }
        public int PackagingID { get; set; }
    }
}
