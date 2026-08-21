namespace TrackerSQL.Models
{
    /// <summary>
    /// Woo global product attribute (parent). Only those with UseForVariants
    /// have their options mapped to Tracker qty/packaging.
    /// ResolvePriority: lower number wins when multiple maps compete for qty or packaging.
    /// </summary>
    public class WooAttributeParent
    {
        public int ParentID { get; set; }
        public long WooAttributeId { get; set; }
        public string AttributeName { get; set; }
        public string Slug { get; set; }
        public bool UseForVariants { get; set; }
        public int TermCount { get; set; }
        public int ResolvePriority { get; set; } = 100;
    }
}
