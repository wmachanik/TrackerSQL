using System;

namespace TrackerSQL.Models
{
    public class ContactTrackedServiceItem
    {
        public int ContactTrackedServiceItemsID { get; set; }
        public int ContactTypeID { get; set; }
        public int ItemServiceTypeID { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
