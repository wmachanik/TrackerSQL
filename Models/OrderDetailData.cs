namespace TrackerSQL.Models
{
    public class OrderDetailData
    {
        public int OrderLineID { get; set; }
        public int ItemTypeID { get; set; }
        public int PackagingID { get; set; }
        public int OrderID { get; set; }
        public double QuantityOrdered { get; set; }
    }
}
