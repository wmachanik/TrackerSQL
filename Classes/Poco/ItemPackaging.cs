namespace TrackerDotNet.Classes.Poco
{
    public class ItemPackaging
    {
        // Match PackagingTbl columns
        public int ItemPackagingID { get; set; }
        public string ItemPackagingDesc { get; set; }
        public string AdditionalNotes { get; set; }
        public string Symbol { get; set; }
        public int? Colour { get; set; }
        public string BGColour { get; set; }

    }
}
