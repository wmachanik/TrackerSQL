using System;

namespace TrackerDotNet.Classes
{
    public class ChangeRequestSubmission
    {
        public long CustomerID { get; set; }
        public DateTime DeliveryDateUtc { get; set; }
        public string RequestTextRaw { get; set; }
        public string SourceIp { get; set; }
        public string UserAgent { get; set; }
        public string TokenRef { get; set; } // optional if you later pass a token hash
    }
}