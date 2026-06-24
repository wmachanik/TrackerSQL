using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// POCO class for ItemUsage
    /// Generated from ItemUsageTbl
    /// </summary>
    public class ItemUsage
    {
        public int ClientUsageLineNo { get; set; }
        public long CustomerID { get; set; }
        public DateTime ItemDate { get; set; }
        public int ItemProvidedID { get; set; }
        public double AmountProvided { get; set; }
        public int PrepTypeID { get; set; }
        public int PackagingID { get; set; }
        public string Notes { get; set; }
    }
}
