using System;

namespace TrackerDotNet.Classes.Poco
{
    public class HolidayClosure
    {
        public int HolidayClosureID { get; set; }
        public DateTime ClosureDate { get; set; }
        public int? DaysClosed { get; set; }
        public bool? AppliesToPrep { get; set; }
        public bool? AppliesToDelivery { get; set; }
        public string ShiftStrategy { get; set; }
        public string Description { get; set; }
    }
}
