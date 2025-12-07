using System;
namespace TrackerDotNet.Classes.Poco
{
    public class UsedItemGroup
    {
        public int UsedItemGroupID { get; set; }
        public int ContactID { get; set; }
        public int? GroupReferenceItemID { get; set; }
        public int? LastItemID { get; set; }
        public int? LastItemSortPos { get; set; }
        public DateTime? LastItemDateChanged { get; set; }
        public string Notes { get; set; }
    }
}
