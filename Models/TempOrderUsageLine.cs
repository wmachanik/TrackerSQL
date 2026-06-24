namespace TrackerSQL.Models
{
    public class TempOrderUsageLine
    {
        public int ContactID { get; set; }
        public int ItemID { get; set; }
        public int ItemServiceTypeID { get; set; }
        public double Qty { get; set; }
        public double UnitsPerQty { get; set; }
        public int ItemPackagingID { get; set; }
    }
}
