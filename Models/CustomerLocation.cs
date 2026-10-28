using System;
using System.Globalization;
using System.Web;

namespace TrackerSQL.Models
{
    /// <summary>CustomerLocationTbl: one GPS point per contact delivered by one of our own drivers.</summary>
    public class CustomerLocation
    {
        public const string StatusVerified = "VERIFIED";
        public const string StatusApproximate = "APPROXIMATE";
        public const string StatusReviewRequired = "REVIEW_REQUIRED";

        public const string SourceGeocoder = "GEOCODER";
        public const string SourceAzureMaps = "AZURE_MAPS";
        public const string SourcePostcode = "POSTCODE";
        public const string SourceManual = "MANUAL";
        /// <summary>Where our driver's phone was when the order was delivered (ours to keep).</summary>
        public const string SourceDriver = "DRIVER";

        public int ContactID { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Geohash { get; set; }
        public int? MapAreaID { get; set; }
        /// <summary>What the geocoder says it found (its label).</summary>
        public string GeocodedAddress { get; set; }
        /// <summary>The address text that was looked up; a different contact address means look up again.</summary>
        public string SourceAddress { get; set; }
        public string LocationStatus { get; set; } = StatusApproximate;
        public string Source { get; set; }
        public decimal? Confidence { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        /// <summary>Azure Maps points only: must be looked up again (or removed) by this date.</summary>
        public DateTime? ExpiresDate { get; set; }
        /// <summary>Deliveries whose phone position agreed with this point.</summary>
        public int DriverFixes { get; set; }
        public DateTime? LastDriverFixAt { get; set; }

        public bool IsVerified => LocationStatus == StatusVerified;
        public bool NeedsReview => LocationStatus == StatusReviewRequired;
        public bool IsFromDriver => Source == SourceDriver;
        public bool IsManual => Source == SourceManual;
        /// <summary>An Azure Maps point close to its keep date: look it up again on the next order or batch.</summary>
        public bool RefreshDue => ExpiresDate.HasValue && ExpiresDate.Value <= DateTime.Now.AddDays(GpsSettings.AzureMapsRefreshDaysBefore);

        /// <summary>E.g. "from the driver's phone, 3 deliveries" or "from Azure Maps, kept until 2027-03-29".</summary>
        public string OriginText =>
            "from " + (Source == SourceDriver ? "the " : string.Empty) + SourceDisplay
            + (DriverFixes > 0 ? ", " + DriverFixes + (DriverFixes == 1 ? " delivery" : " deliveries") : string.Empty)
            + (ExpiresDate.HasValue ? ", kept until " + ExpiresDate.Value.ToString("yyyy-MM-dd") : string.Empty);

        public string SourceDisplay
        {
            get
            {
                switch (Source)
                {
                    case SourceDriver: return "driver's phone";
                    case SourceManual: return "placed by hand";
                    case SourceAzureMaps: return "Azure Maps";
                    case SourcePostcode: return "postcode";
                    case SourceGeocoder: return "OpenStreetMap";
                    default: return Source;
                }
            }
        }

        public string StatusDisplay
        {
            get
            {
                switch (LocationStatus)
                {
                    case StatusVerified: return "Checked";
                    case StatusReviewRequired: return "Needs checking";
                    default: return Source == SourceDriver ? "Delivered here" : "Looked up";
                }
            }
        }

        public string CoordinatesText =>
            Latitude.ToString("0.00000", CultureInfo.InvariantCulture) + ", "
            + Longitude.ToString("0.00000", CultureInfo.InvariantCulture);

        /// <summary>
        /// Small CSS-drawn status badge (no icon font): filled tick = checked, outlined tick = looked up,
        /// "?" = needs checking or no point (<paramref name="loc"/> null).
        /// </summary>
        public static string BadgeHtml(CustomerLocation loc)
        {
            return BadgeHtml(loc, null);
        }

        /// <param name="tooltip">When set, used as the badge title (e.g. coordinates for alternative addresses).</param>
        public static string BadgeHtml(CustomerLocation loc, string tooltip)
        {
            string title = tooltip;
            if (string.IsNullOrEmpty(title))
            {
                if (loc == null)
                    title = "No GPS point yet";
                else
                    title = loc.CoordinatesText + " — " + loc.StatusDisplay;
            }

            string titleAttr = string.IsNullOrEmpty(title)
                ? string.Empty
                : " title=\"" + HttpUtility.HtmlAttributeEncode(title) + "\"";

            if (loc == null || loc.NeedsReview)
                return "<span class=\"gps-badge gps-badge-unsure\" aria-hidden=\"true\"" + titleAttr + "></span>";
            return loc.IsVerified
                ? "<span class=\"gps-badge gps-badge-ok\" aria-hidden=\"true\"" + titleAttr + "></span>"
                : "<span class=\"gps-badge gps-badge-found\" aria-hidden=\"true\"" + titleAttr + "></span>";
        }
    }
}
