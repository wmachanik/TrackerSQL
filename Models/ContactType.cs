namespace TrackerSQL.Models
{
    public class ContactType
    {
        public int ContactTypeID { get; set; }
        public string ContactTypeDesc { get; set; }
        public string Notes { get; set; }

        /// <summary>Driver route stops: record collection only — no client signature or terms.</summary>
        public bool DriverCollectionOnly { get; set; }
    }
}
