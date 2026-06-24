using System;
namespace TrackerSQL.Models
{
    public class NextPreperationDateByArea
    {
        public int NextPrepDayID { get; set; }
        public int AreaID { get; set; }
        public DateTime? PreperationDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public short? DeliveryOrder { get; set; }
        public DateTime? NextPreperationDate { get; set; }
        public DateTime? NextDeliveryDate { get; set; }
    }
}
