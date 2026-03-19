using System;

namespace TrackerDotNet.Classes.Poco
{
    public class ContactUsageLine
    {
        public int ContactsItemSvcSummaryId { get; set; }
        public int ContactID { get; set; }
        public DateTime? UsageDate { get; set; }
        public int? CupCount { get; set; }
        public int? ItemServiceTypeID { get; set; }
        public double? Qty { get; set; }
        public string Notes { get; set; }
    }
}
