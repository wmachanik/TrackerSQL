using System;
using System.Collections.Generic;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// PeopleTbl.DeliveryType: how the "Delivery By" person gets the order to the customer.
    /// Only DELIVER (our own drivers) needs a GPS location, a map area and a route.
    /// </summary>
    public static class DeliveryTypes
    {
        public const string Deliver = "DELIVER";
        public const string Courier = "COURIER";
        public const string Collect = "COLLECT";
        public const string None = "NONE";

        /// <summary>Value → label, in dropdown order.</summary>
        public static readonly IList<KeyValuePair<string, string>> All = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(Deliver, "Does deliveries"),
            new KeyValuePair<string, string>(Courier, "Courier (waybill)"),
            new KeyValuePair<string, string>(Collect, "Collect (shop)"),
            new KeyValuePair<string, string>(None, "None")
        };

        public static string Normalise(string value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant();
            return v == Deliver || v == Courier || v == Collect ? v : None;
        }

        public static string Label(string value)
        {
            string v = Normalise(value);
            foreach (var item in All)
            {
                if (item.Key == v)
                    return item.Value;
            }
            return "None";
        }
    }
}
