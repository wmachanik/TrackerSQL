using System;
namespace TrackerDotNet.Classes.Poco
{
    public class SysData
    {
        public int ID { get; set; }
        public DateTime? LastReoccurringDate { get; set; }
        public bool? DoReoccuringOrders { get; set; }
        public DateTime? DateLastPrepDateCalcd { get; set; }
        public DateTime? MinReminderDate { get; set; }
        public int? GroupReferenceItemID { get; set; }
        public string InternalContactIDs { get; set; }
    }
}
