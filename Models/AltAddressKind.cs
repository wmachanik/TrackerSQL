namespace TrackerSQL.Models
{
    /// <summary>Lookups → Alt Address Types. KindCode is stored on alternative addresses.</summary>
    public class AltAddressKind
    {
        public int AltAddressKindID { get; set; }
        public string KindCode { get; set; }
        public string KindName { get; set; }
        /// <summary>When false, Order Done does not write usage or move Next Coffee / filter dates.</summary>
        public bool CountsTowardTracking { get; set; } = true;
        public int SortOrder { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
