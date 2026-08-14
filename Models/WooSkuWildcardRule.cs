namespace TrackerSQL.Models
{
    public class WooSkuWildcardRule
    {
        public int RuleID { get; set; }
        public string SkuPrefixPattern { get; set; }
        public string SuffixToken { get; set; }
        public double QtyFactor { get; set; }
        public int? PackagingID { get; set; }
        public int ItemID { get; set; }
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; }

        public string ItemDesc { get; set; }
    }
}
