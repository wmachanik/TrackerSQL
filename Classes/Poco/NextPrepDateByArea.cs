using System;
namespace TrackerDotNet.Classes.Poco
{
    public class NextPrepDateByArea
    {
        public int NextPrepDayID { get; set; }
        public int AreaID { get; set; }
        public DateTime? PreperationDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public short? DeliveryOrder { get; set; }
        public DateTime? NextPrepDate { get; set; }
        public DateTime? NextDeliveryDate { get; set; }
    }
}
