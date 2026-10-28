using System;
using System.Linq;
using TrackerSQL.Managers;

namespace TrackerSQL.Models
{
    /// <summary>Alternative delivery address for a contact (second site, gift, work, etc.).</summary>
    public class ContactDeliverToAddress
    {
        public int DeliverToAddressID { get; set; }
        public int ContactID { get; set; }
        /// <summary>Short name on orders (e.g. "Reception").</summary>
        public string Label { get; set; }
        /// <summary>Who receives and signs on the driver app at this address.</summary>
        public string DeliverToPerson { get; set; }
        /// <summary><see cref="AlternativeAddressKind"/> code (OnBehalf, Gift, Home, …).</summary>
        public string AddressKind { get; set; }
        public string AddressKindDisplay => AlternativeAddressKind.Display(AddressKind);
        public int? ToBeDeliveredByID { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public int? AreaID { get; set; }
        public string StateOrProvince { get; set; }
        public string PostalCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Geohash { get; set; }
        public string GeocodedAddress { get; set; }
        public string SourceAddress { get; set; }
        public string LocationStatus { get; set; }
        public string Source { get; set; }
        public DateTime? LocationUpdatedDate { get; set; }
        public string LocationUpdatedBy { get; set; }
        public int SortOrder { get; set; }
        public bool Enabled { get; set; } = true;

        /// <summary>From AreasTbl when listing.</summary>
        public string AreaName { get; set; }
        /// <summary>From PeopleTbl when listing.</summary>
        public string DeliveryByDisplay { get; set; }

        /// <summary>Short Name for order/dropdown lists — never includes street or GPS text.</summary>
        public string ShortNameForOrders
        {
            get
            {
                string name = (Label ?? string.Empty).Trim();
                if (name.Length == 0)
                    name = (DeliverToPerson ?? string.Empty).Trim();
                int legacySep = name.IndexOf(" — ", StringComparison.Ordinal);
                if (legacySep > 0)
                    name = name.Substring(0, legacySep).Trim();
                return string.IsNullOrEmpty(name) ? "Deliver-to" : name;
            }
        }

        public string AddressSummary
        {
            get
            {
                string a = string.Join(", ", new[] { Address1, Address2, Address3 }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                if (!string.IsNullOrWhiteSpace(PostalCode))
                    a = string.IsNullOrEmpty(a) ? PostalCode : a + ", " + PostalCode;
                return a;
            }
        }

        public bool HasGpsPoint => Latitude.HasValue && Longitude.HasValue;

        public bool GpsNeedsReview =>
            !HasGpsPoint || LocationStatus == CustomerLocation.StatusReviewRequired;

        public bool GpsIsVerified => HasGpsPoint && LocationStatus == CustomerLocation.StatusVerified;

        /// <summary>For the same tick / ? badges as the main contact GPS line.</summary>
        public CustomerLocation ToDisplayLocation()
        {
            if (!HasGpsPoint)
                return null;
            return new CustomerLocation
            {
                Latitude = Latitude.Value,
                Longitude = Longitude.Value,
                Geohash = Geohash,
                GeocodedAddress = GeocodedAddress,
                SourceAddress = SourceAddress,
                LocationStatus = LocationStatus ?? CustomerLocation.StatusApproximate,
                Source = Source,
                UpdatedBy = LocationUpdatedBy,
                UpdatedDate = LocationUpdatedDate ?? DateTime.MinValue
            };
        }
    }
}
