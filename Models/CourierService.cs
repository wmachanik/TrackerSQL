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
        /// <summary>Public track-and-trace page URL (base site).</summary>
        public string TrackingUrl { get; set; }
        /// <summary>
        /// Optional suffix to open tracking with the waybill filled in, e.g. Fastway "?l=".
        /// When set, emails link the waybill number to TrackingUrl + this + encoded waybill.
        /// Leave blank until the courier's query/path pattern is known.
        /// </summary>
        public string TrackingUrlParam { get; set; }
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int SortOrder { get; set; }

        /// <summary>True when this row is the explicit "none" / not-a-courier option.</summary>
        public bool IsNone =>
            string.Equals(ServiceCode, "None", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ServiceName, "None", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Builds a deep tracking link when TrackingUrlParam is set; otherwise returns the base TrackingUrl.
        /// </summary>
        public string BuildTrackingUrl(string waybillNumber)
        {
            if (string.IsNullOrWhiteSpace(TrackingUrl))
                return null;

            string baseUrl = TrackingUrl.Trim();
            if (string.IsNullOrWhiteSpace(TrackingUrlParam) || string.IsNullOrWhiteSpace(waybillNumber))
                return baseUrl;

            string param = TrackingUrlParam.Trim();
            string encoded = Uri.EscapeDataString(waybillNumber.Trim());

            if (param.IndexOf("{0}", StringComparison.Ordinal) >= 0)
                return baseUrl + string.Format(param, encoded);

            // If base already has a query and param starts with '?', use '&' instead.
            if (param.StartsWith("?", StringComparison.Ordinal) && baseUrl.IndexOf('?') >= 0)
                param = "&" + param.Substring(1);

            return baseUrl + param + encoded;
        }

        /// <summary>True when a deep link (base + param + waybill) can be built.</summary>
        public bool CanDeepLinkTracking(string waybillNumber)
        {
            return !string.IsNullOrWhiteSpace(TrackingUrl)
                && !string.IsNullOrWhiteSpace(TrackingUrlParam)
                && !string.IsNullOrWhiteSpace(waybillNumber);
        }
    }
}
