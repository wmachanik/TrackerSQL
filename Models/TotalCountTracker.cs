using System;
namespace TrackerSQL.Models
{
    public class TotalCountTracker
    {
        public int TotalCounterTrackerID { get; set; }
        public DateTime? CountDate { get; set; }
        public int? TotalCount { get; set; }
        public string Comments { get; set; }
    }
}
