namespace TrackerSQL.Models
{
    public class WooCategoryFilter
    {
        public int FilterID { get; set; }
        public long WooCategoryId { get; set; }
        public string CategoryName { get; set; }
        public long ParentWooCategoryId { get; set; }
        public bool IncludeInSync { get; set; } = true;

        /// <summary>UI-only tree depth after hierarchical sort.</summary>
        public int Depth { get; set; }
    }
}
