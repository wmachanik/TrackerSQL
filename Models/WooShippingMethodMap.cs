namespace TrackerSQL.Models
{
    public class WooShippingMethodMap
    {
        public int MapID { get; set; }
        /// <summary>Woo shipping method title matched case-insensitively (contains).</summary>
        public string MethodMatch { get; set; }
        public int ToBeDeliveredByID { get; set; }
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; }
        public string PersonAbbrev { get; set; }
    }
}
