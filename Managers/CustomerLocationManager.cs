using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class LocationLookupOutcome
    {
        public bool Success { get; set; }
        /// <summary>Nothing to do (not one of our drivers, no address, already looked up).</summary>
        public bool Skipped { get; set; }
        /// <summary>Stop a batch: daily cap, per-minute limit, or no key.</summary>
        public bool StopBatch { get; set; }
        public bool UsedLookup { get; set; }
        public CustomerLocation Location { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Customer GPS points: geocode once, keep while the address is unchanged, let a person check / move the pin.
    /// Only the address, postcode and "South Africa" are sent to the geocoder (no names, phones or e-mails).
    /// </summary>
    public class CustomerLocationManager
    {
        private static readonly Queue<DateTime> RecentCalls = new Queue<DateTime>();
        private static readonly object RecentCallsLock = new object();

        private readonly CustomerLocationRepository _locations = new CustomerLocationRepository();
        private readonly GpsSettingsRepository _settingsRepo = new GpsSettingsRepository();

        public bool IsReady => _locations.TableExists();

        public CustomerLocation Get(int contactId)
        {
            return IsReady ? _locations.GetByContactId(contactId) : null;
        }

        public static string BuildAddressText(Contact contact)
        {
            if (contact == null) return string.Empty;
            var parts = new List<string>();
            string address = Regex.Replace(contact.BillingAddress ?? string.Empty, @"\s*[\r\n]+\s*", ", ").Trim(' ', ',');
            if (address.Length > 0) parts.Add(address);
            string postal = (contact.PostalCode ?? string.Empty).Trim();
            if (postal.Length > 0 && address.IndexOf(postal, StringComparison.OrdinalIgnoreCase) < 0) parts.Add(postal);
            if (parts.Count > 0) parts.Add("South Africa");
            return string.Join(", ", parts);
        }

        private static bool SameAddress(string a, string b)
        {
            string Norm(string s) => Regex.Replace((s ?? string.Empty).ToLowerInvariant(), @"[^a-z0-9]+", " ").Trim();
            return Norm(a) == Norm(b);
        }

        /// <summary>
        /// Called on contact save / order capture / the one-time batch. Looks up only when the contact is delivered
        /// by one of our drivers and has no point yet, or the address changed. <paramref name="force"/> = "look up again".
        /// </summary>
        public LocationLookupOutcome EnsureForContact(int contactId, string userName, bool force = false, int? deliverPersonId = null)
        {
            if (!IsReady)
                return new LocationLookupOutcome { Skipped = true, Message = "GPS tables are not installed yet." };

            Contact contact = new ContactsRepository().GetById(contactId);
            if (contact == null)
                return new LocationLookupOutcome { Skipped = true, Message = "Contact not found." };

            if (!force && !new PersonsRepository().IsDeliverPerson(deliverPersonId ?? contact.PreferredAgentID))
                return new LocationLookupOutcome { Skipped = true, Message = "Not delivered by one of our drivers." };

            string addressText = BuildAddressText(contact);
            if (string.IsNullOrWhiteSpace(addressText))
                return new LocationLookupOutcome { Skipped = true, Message = "The contact has no address." };

            CustomerLocation existing = _locations.GetByContactId(contactId);
            if (!force && existing != null && SameAddress(existing.SourceAddress, addressText))
                return new LocationLookupOutcome { Success = true, Skipped = true, Location = existing };

            GpsSettings settings = _settingsRepo.Get();
            GeocodeResult result = Geocode(addressText, settings, out bool usedLookup);
            if (!result.Success && result.Failure != GeocodeFailure.NotFound)
            {
                return new LocationLookupOutcome
                {
                    StopBatch = result.Failure == GeocodeFailure.DailyLimitReached
                        || result.Failure == GeocodeFailure.PerMinuteLimitReached
                        || result.Failure == GeocodeFailure.NotConfigured,
                    UsedLookup = usedLookup,
                    Location = existing,
                    Message = result.Message
                };
            }

            var loc = new CustomerLocation
            {
                ContactID = contactId,
                SourceAddress = addressText,
                MapAreaID = existing?.MapAreaID,
                UpdatedBy = userName
            };

            if (result.Success)
            {
                loc.Latitude = (decimal)result.Latitude;
                loc.Longitude = (decimal)result.Longitude;
                loc.GeocodedAddress = result.Label;
                loc.Source = CustomerLocation.SourceGeocoder;
                loc.Confidence = result.Confidence.HasValue ? (decimal?)Convert.ToDecimal(result.Confidence.Value) : null;
                loc.LocationStatus = result.IsPrecise ? CustomerLocation.StatusApproximate : CustomerLocation.StatusReviewRequired;
            }
            else if (!TryPostcodeCentroid(contact.PostalCode, loc))
            {
                return new LocationLookupOutcome
                {
                    UsedLookup = usedLookup,
                    Location = existing,
                    Message = "Address not found and no postcode point to fall back on; set the pin by hand."
                };
            }

            loc.Geohash = Geohash.Encode((double)loc.Latitude, (double)loc.Longitude);
            _locations.Upsert(loc);

            return new LocationLookupOutcome
            {
                Success = true,
                UsedLookup = usedLookup,
                Location = loc,
                Message = loc.NeedsReview
                    ? "Location found, but only roughly; please check the pin."
                    : "Location found."
            };
        }

        /// <summary>A person placed or confirmed the pin on the map.</summary>
        public CustomerLocation SetManual(int contactId, decimal latitude, decimal longitude, string userName, string matchedAddress = null)
        {
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(latitude), "Not a valid GPS point.");

            Contact contact = new ContactsRepository().GetById(contactId);
            CustomerLocation existing = _locations.GetByContactId(contactId);
            var loc = new CustomerLocation
            {
                ContactID = contactId,
                Latitude = latitude,
                Longitude = longitude,
                Geohash = Geohash.Encode((double)latitude, (double)longitude),
                MapAreaID = existing?.MapAreaID,
                GeocodedAddress = string.IsNullOrWhiteSpace(matchedAddress) ? existing?.GeocodedAddress : matchedAddress,
                SourceAddress = BuildAddressText(contact),
                LocationStatus = CustomerLocation.StatusVerified,
                Source = CustomerLocation.SourceManual,
                Confidence = null,
                UpdatedBy = userName
            };
            _locations.Upsert(loc);
            return loc;
        }

        /// <summary>Looks up the depot address (used as the focus point and the start of every route).</summary>
        public LocationLookupOutcome LookupDepot(string userName)
        {
            GpsSettings settings = _settingsRepo.Get();
            string address = (settings.DepotAddress ?? string.Empty).Replace(';', ',');
            if (address.IndexOf("South Africa", StringComparison.OrdinalIgnoreCase) < 0)
                address += ", South Africa";

            GeocodeResult result = Geocode(address, settings, out bool usedLookup, useFocus: false);
            if (!result.Success)
                return new LocationLookupOutcome { UsedLookup = usedLookup, Message = result.Message };

            _settingsRepo.SaveDepotPoint(Math.Round((decimal)result.Latitude, 6), Math.Round((decimal)result.Longitude, 6));
            return new LocationLookupOutcome
            {
                Success = true,
                UsedLookup = usedLookup,
                Message = "Depot found: " + (result.Label ?? "") + " (" +
                    result.Latitude.ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
                    result.Longitude.ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture) + ")."
            };
        }

        /// <summary>Looks up any typed address (uses one lookup) without saving; the Check popup moves the pin there.</summary>
        public GeocodeResult SearchAddress(string text)
        {
            string query = (text ?? string.Empty).Trim();
            if (query.Length == 0)
                return GeocodeResult.Fail(GeocodeFailure.NotFound, "Type an address to search for.");
            if (query.IndexOf("South Africa", StringComparison.OrdinalIgnoreCase) < 0)
                query += ", South Africa";
            return Geocode(query, _settingsRepo.Get(), out _);
        }

        public int LookupsUsedToday()
        {
            return _settingsRepo.GetCallsToday(GpsSettingsRepository.GeocodeService);
        }

        private GeocodeResult Geocode(string address, GpsSettings settings, out bool usedLookup, bool useFocus = true)
        {
            usedLookup = false;
            if (!settings.HasApiKey)
                return GeocodeResult.Fail(GeocodeFailure.NotConfigured, "No geocoding key: add it on System Preferences → GPS & Drivers.");

            if (!TryTakeMinuteSlot(settings.GeocodePerMinute))
                return GeocodeResult.Fail(GeocodeFailure.PerMinuteLimitReached, "Per-minute lookup limit reached; try again in a minute.");

            if (!_settingsRepo.TryCountCall(GpsSettingsRepository.GeocodeService, settings.GeocodeDailyLimit))
                return GeocodeResult.Fail(GeocodeFailure.DailyLimitReached,
                    "Today's lookup limit (" + settings.GeocodeDailyLimit + ") is used up; the rest will be done tomorrow.");

            usedLookup = true;
            IGeocoder geocoder = new HeigitGeocoder(settings.GeocodeBaseUrl, settings.GeocodeApiKey);
            GeocodeResult result = geocoder.Search(address,
                useFocus && settings.HasDepotPoint ? (double?)settings.DepotLatitude.Value : null,
                useFocus && settings.HasDepotPoint ? (double?)settings.DepotLongitude.Value : null);

            if (result.Failure == GeocodeFailure.DailyLimitReached)
                _settingsRepo.MarkDailyLimitReached(GpsSettingsRepository.GeocodeService, settings.GeocodeDailyLimit);
            if (!result.Success && result.Failure != GeocodeFailure.NotFound)
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "Geocode failed: " + result.Failure + " " + result.Message);
            return result;
        }

        private static bool TryTakeMinuteSlot(int perMinute)
        {
            if (perMinute <= 0) return true;
            lock (RecentCallsLock)
            {
                DateTime now = DateTime.UtcNow;
                while (RecentCalls.Count > 0 && (now - RecentCalls.Peek()).TotalSeconds >= 60)
                    RecentCalls.Dequeue();
                if (RecentCalls.Count >= perMinute)
                    return false;
                RecentCalls.Enqueue(now);
                return true;
            }
        }

        private static bool TryPostcodeCentroid(string postalCode, CustomerLocation loc)
        {
            if (!int.TryParse((postalCode ?? string.Empty).Trim(), out int code) || code <= 0)
                return false;
            try
            {
                SaPostalCode place = new SaPostalCodeRepository().GetPlacesForPostalCode(code, 20)
                    .FirstOrDefault(p => p.Latitude.HasValue && p.Longitude.HasValue);
                if (place == null)
                    return false;
                loc.Latitude = place.Latitude.Value;
                loc.Longitude = place.Longitude.Value;
                loc.GeocodedAddress = "Postcode " + code.ToString("0000") + " (" + place.PlaceName + ")";
                loc.Source = CustomerLocation.SourcePostcode;
                loc.LocationStatus = CustomerLocation.StatusReviewRequired;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
