using System;

namespace TrackerSQL.Models
{
    public class AreaPrepDateRow
    {
        public string Area { get; set; }
        public DateTime? PreparationDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? NextPreparationDate { get; set; }
        public DateTime? NextDeliveryDate { get; set; }
    }

    public class DeliveryDateLookup
    {
        public DateTime Date { get; set; }
    }
}
