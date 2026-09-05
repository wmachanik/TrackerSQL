namespace TrackerSQL.Models

{

    /// <summary>SA postal reference row (imported from geo CSV).</summary>

    public class SaPostalCode

    {

        public int PostalCodeID { get; set; }

        public int PostalCode { get; set; }

        public string PlaceName { get; set; }

        public string Province { get; set; }

        public string Municipality { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

    }

}

