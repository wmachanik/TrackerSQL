using System;
namespace TrackerDotNet.Classes.Poco
{
    public class ContactsAwayPeriod
    {
        public int AwayPeriodID { get; set; }
        public int ContactID { get; set; }
        public DateTime AwayStartDate { get; set; }
        public DateTime AwayEndDate { get; set; }
        public int? ReasonID { get; set; }
    }
}
