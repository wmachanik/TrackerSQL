using System;

namespace TrackerDotNet.Classes.Poco
{
    public class ClosureDate
    {
        public int ClosureDateID { get; set; }
        public DateTime DateClosed { get; set; }
        public DateTime? DateReopen { get; set; }
        public DateTime? NextPrepDate { get; set; }
        public string Comments { get; set; }
    }
}
