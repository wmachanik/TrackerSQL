using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// POCO model for ItemServiceTypesTbl - Item service types (Coffee, Cleaning, Group, etc.)
    /// </summary>
    public class ItemServiceType
    {
        public int ItemServiceTypeID { get; set; }
        public string ItemServiceTypeName { get; set; }
        public string Description { get; set; }
        public int ItemPackagingID {  get; set; }
        public int ItemPrepTypeID { get; set; }
    }
}
