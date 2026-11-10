using System;

namespace TrackerSQL.Models
{
    public class WooContactInfo
    {
        public long ContactID { get; set; }
        public long? WooCustomerId { get; set; }
        public string MatchEmail { get; set; }
        public DateTime? LastSyncedUtc { get; set; }
        public int WooOrderCount { get; set; }
        public bool QuaffeeLoversMember { get; set; }
        public DateTime? QuaffeeLoversCheckedUtc { get; set; }
    }
}
