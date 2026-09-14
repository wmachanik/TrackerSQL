using System;

namespace TrackerSQL.Models
{
    /// <summary>One website-promised delivery/dispatch window for an area.</summary>
    public class DeliveryPromiseRule
    {
        public int RuleID { get; set; }
        public string RuleGroup { get; set; }
        public int? AreaID { get; set; }
        public string AreaMatchName { get; set; }
        public int SortOrder { get; set; }
        /// <summary>0=Sun..6=Sat; 255 = any workday (time-of-day only).</summary>
        public byte WindowStartDow { get; set; }
        public short WindowStartMinutes { get; set; }
        public byte WindowEndDow { get; set; }
        public short WindowEndMinutes { get; set; }
        /// <summary>Delivery or Dispatch.</summary>
        public string PromiseKind { get; set; }
        /// <summary>FixedDow, SameWorkday, NextWorkday.</summary>
        public string ResultMode { get; set; }
        public byte? ResultDow { get; set; }
        public bool NoThursdayDispatch { get; set; }
        public bool WedAfterNoonToFriday { get; set; }
        public string Notes { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public class DeliveryPromiseResult
    {
        public DateTime? PromisedDate { get; set; }
        public string PromiseKind { get; set; }
        public string RuleGroup { get; set; }
        public string Notes { get; set; }
        public bool FoundRule { get; set; }
    }
}
