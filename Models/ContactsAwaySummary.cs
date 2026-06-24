using System;
namespace TrackerSQL.Models
{
    /// <summary>
    /// Summary view for Contacts Away list page - joins ContactsAwayPeriodTbl with ContactsTbl and AwayReasonsTbl
    /// </summary>
    public class ContactsAwaySummary
    {
        public int AwayPeriodID { get; set; }
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public DateTime? AwayStartDate { get; set; }
        public DateTime? AwayEndDate { get; set; }
        public int? ReasonID { get; set; }
        public string ReasonDesc { get; set; }
    }
}
