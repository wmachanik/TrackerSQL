namespace TrackerDotNet.Classes.Poco
{
    public class ItemGroup
    {
        public int ItemGroupID { get; set; }
        public int GroupReferenceItemID { get; set; }
        public int? ItemID { get; set; }
        public int? ItemSortPos { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
    }
}
