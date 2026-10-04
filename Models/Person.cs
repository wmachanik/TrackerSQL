using System;

namespace TrackerSQL.Models
{
    public class Person
    {
        public int PersonID { get; set; }
        public string PersonName { get; set; }
        public string Abbreviation { get; set; }
        public bool? Enabled { get; set; }
        public int? NormalDeliveryDoW { get; set; }
        public string SecurityUsername { get; set; }
        /// <summary>
        /// When true, Order Done treats this Delivered-by person as parcel dispatch
        /// (waybill / courier panel). Source of truth — not Woo settings.
        /// </summary>
        public bool IsDispatched { get; set; }

        /// <summary>DELIVER, COURIER, COLLECT or NONE (DeliveryTypes). COURIER and IsDispatched are kept equal.</summary>
        public string DeliveryType { get; set; } = "NONE";

        /// <summary>Hands over depot collections: their driver app lists orders delivered by a "Collect (shop)" person.</summary>
        public bool HandsOverCollections { get; set; }
    }
}
