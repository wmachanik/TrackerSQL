using System;

namespace TrackerDotNet.Classes.Poco
{
    public class Item
    {
        public int ItemID { get; set; }
        public string SKU { get; set; }
        public string ItemDesc { get; set; }
        public bool? ItemEnabled { get; set; }
        public string ItemDetail { get; set; }
        public int? ItemServiceTypeID { get; set; }
        public int? ReplacementItemID { get; set; }
        public int? ItemUnitID { get; set; }
        public double? BasePrice { get; set; }
    }
}
