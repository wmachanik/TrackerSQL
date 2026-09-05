namespace TrackerSQL.Models
{
    public class WooAreaResolveResult
    {
        public int? AreaID { get; set; }
        public string AreaName { get; set; }
        public int? DefaultPreferredAgentID { get; set; }
        public bool UsedDefaultArea { get; set; }
        public bool IsAmbiguous { get; set; }
        public string Reason { get; set; }
    }
}
