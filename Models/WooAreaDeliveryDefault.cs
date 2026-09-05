using System;

namespace TrackerSQL.Models
{
    [Serializable]
    public class WooAreaDeliveryDefault
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public int? DefaultPreferredAgentID { get; set; }
        public string DefaultPersonAbbrev { get; set; }
        /// <summary>Semicolon-separated postcodes: 7806 or 7800...7806 (Woo-style range).</summary>
        public string PostalRanges { get; set; }
    }
}

