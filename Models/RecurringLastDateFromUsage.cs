using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Candidate / result for System Tools "Set Last Order Date"
    /// (DateLastDone pulled from ContactsItemUsageTbl when matching usage exists).
    /// </summary>
    public class RecurringLastDateFromUsage
    {
        public int RecurringOrderItemID { get; set; }
        public int RecurringOrderID { get; set; }
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public string ItemDesc { get; set; }
        public DateTime? PreviousDateLastDone { get; set; }
        /// <summary>Max matching delivery from ContactsItemUsageTbl; null when none.</summary>
        public DateTime? LastUsageDate { get; set; }
        /// <summary>Date applied to DateLastDone (usage or existing last).</summary>
        public DateTime? AppliedLastDate { get; set; }
        public string UpdateResult { get; set; }
    }
}
