namespace TrackerSQL.Models
{
    public class ItemPackaging
    {
        // Match ItemPackagingsTbl columns
        public int ItemPackagingID { get; set; }
        public string ItemPackagingDesc { get; set; }
        public string AdditionalNotes { get; set; }
        public string Symbol { get; set; }
        /// <summary>Foreground colour as HTML hex (#RRGGBB), same format as BGColour.</summary>
        public string Colour { get; set; }
        /// <summary>Background colour as HTML hex (#RRGGBB) — used by DeliverySheet item spans.</summary>
        public string BGColour { get; set; }

    }
}
