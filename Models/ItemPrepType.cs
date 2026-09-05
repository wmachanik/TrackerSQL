namespace TrackerSQL.Models
{
    public class ItemPrepType
    {
        public int ItemPrepID { get; set; }
        /// <summary>Display name. Table column is ItemPrepType (cannot match the class name in C#).</summary>
        public string ItemPrepTypeDesc { get; set; }
        public string IdentifyingChar { get; set; }
    }
}
