using System;

namespace TrackerSQL.Models
{
    public class NextPreparationDateByArea
    {
        public int NextPrepDayID { get; set; }
        public int AreaID { get; set; }
        public DateTime? PreparationDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public short? DeliveryOrder { get; set; }
        public DateTime? NextPreparationDate { get; set; }
        public DateTime? NextDeliveryDate { get; set; }
    }
}
