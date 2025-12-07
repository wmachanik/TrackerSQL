namespace TrackerDotNet.Classes.Poco
{
    public class PriceLevel
    {
        public int PriceLevelID { get; set; }
        public string PriceLevelDesc { get; set; }
        public double? PricingFactor { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
    }
}
