using System;

namespace TrackerDotNet.Controls
{
    public class ClientAwayPeriod
    {
        public int AwayPeriodID { get; set; }
        public int ClientID { get; set; }
        public string CompanyName { get; set; }
        public DateTime AwayStartDate { get; set; }
        public DateTime AwayEndDate { get; set; }
        public int ReasonID { get; set; }
        public string ReasonDesc { get; set; }
    }
}