using System;

namespace TrackerSQL.Models
{
    /// <summary>Courier used for parcel dispatch (Pargo, Fastway, Courier Guy, etc.).</summary>
    public class CourierService
    {
        public int CourierServiceID { get; set; }
        /// <summary>Stable code for MERGE/seed: None, Fastway, Pargo, CourierGuy.</summary>
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        /// <summary>Public track-and-trace page URL (customer pastes waybill there).</summary>
        public string TrackingUrl { get; set; }
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int SortOrder { get; set; }

        /// <summary>True when this row is the explicit "none" / not-a-courier option.</summary>
        public bool IsNone =>
            string.Equals(ServiceCode, "None", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ServiceName, "None", StringComparison.OrdinalIgnoreCase);
    }
}
