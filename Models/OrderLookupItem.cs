namespace TrackerSQL.Models
{
    /// <summary>
    /// Dropdown row for order pages — property names match legacy ODS bindings.
    /// </summary>
    public class OrderCompanyLookup
    {
        public int CustomerID { get; set; }
        public string CompanyName { get; set; }
    }

    public class OrderItemLookup
    {
        public int ItemTypeID { get; set; }
        public string ItemDesc { get; set; }
        public bool ItemEnabled { get; set; } = true;
    }

    public class OrderPackagingLookup
    {
        public int PackagingID { get; set; }
        public string Description { get; set; }
    }
}
