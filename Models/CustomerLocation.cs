using System;
using System.Globalization;

namespace TrackerSQL.Models
{
    /// <summary>CustomerLocationTbl: one GPS point per contact delivered by one of our own drivers.</summary>
    public class CustomerLocation
    {
        public const string StatusVerified = "VERIFIED";
        public const string StatusApproximate = "APPROXIMATE";
        public const string StatusReviewRequired = "REVIEW_REQUIRED";

        public const string SourceGeocoder = "GEOCODER";
        public const string SourcePostcode = "POSTCODE";
        public const string SourceManual = "MANUAL";

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

        public bool IsVerified => LocationStatus == StatusVerified;
        public bool NeedsReview => LocationStatus == StatusReviewRequired;

        public string StatusDisplay
        {
            get
            {
                switch (LocationStatus)
                {
                    case StatusVerified: return "Checked";
                    case StatusReviewRequired: return "Needs checking";
                    default: return "Looked up";
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
            if (loc == null || loc.NeedsReview)
                return "<span class=\"gps-badge gps-badge-unsure\" aria-hidden=\"true\">?</span>";
            return loc.IsVerified
                ? "<span class=\"gps-badge gps-badge-ok\" aria-hidden=\"true\"></span>"
                : "<span class=\"gps-badge gps-badge-found\" aria-hidden=\"true\"></span>";
        }
    }
}
