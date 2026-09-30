using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// <summary>Automatic contact lookups only accept matches this close to the depot (when the depot point is set).</summary>
        private const double AutoLookupRadiusKm = 150;

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
            string address = AddressLines(contact.BillingAddress);
            if (address.Length > 0) parts.Add(address);
            string postal = (contact.PostalCode ?? string.Empty).Trim();
            if (postal.Length > 0 && address.IndexOf(postal, StringComparison.OrdinalIgnoreCase) < 0) parts.Add(postal);
            if (parts.Count > 0) parts.Add("South Africa");
            return string.Join(", ", parts);
        }

        /// <summary>Address lines (new lines or ";") joined with ", ".</summary>
        private static string AddressLines(string billingAddress)
        {
            return Regex.Replace(billingAddress ?? string.Empty, @"\s*(?:[\r\n]+|;)\s*", ", ").Trim(' ', ',');
        }

        private static string Norm(string s) => Regex.Replace((s ?? string.Empty).ToLowerInvariant(), @"[^a-z0-9]+", " ").Trim();

        private static bool SameAddress(string a, string b)
        {
            return Norm(a) == Norm(b);
        }

        /// <summary>
        /// The contact's delivery area as a suburb for the lookup, or null when there is none or the address already
        /// names it. "Constantia &amp; Surrounds" → "Constantia", "Rondebosch / Newlands" → "Rondebosch".
        /// </summary>
        public static string AreaSuburbForLookup(Contact contact)
        {
            if (contact?.AreaID == null || contact.AreaID.Value <= 0)
                return null;
            string name;
            try
            {
                name = new AreasRepository().GetAreaName(contact.AreaID.Value);
            }
            catch
            {
                return null;
            }

            name = Regex.Replace(name ?? string.Empty, @"\([^)]*\)", " ");
            name = Regex.Split(name, @"\s*(?:/|&|,|\+|\band\b)\s*", RegexOptions.IgnoreCase)[0];
            name = Regex.Replace(name, @"\b(surrounds|surrounding|area|areas|region)\b", " ", RegexOptions.IgnoreCase).Trim();
            if (name.Length < 3)
                return null;

            string address = " " + Norm(contact.BillingAddress) + " ";
            return address.Contains(" " + Norm(name) + " ") ? null : name;
        }

        private const string StreetWords = @"road|rd|street|str|st|avenue|ave|av|drive|dr|crescent|cres|close|cl|lane|ln|way|boulevard|blvd"
            + @"|circle|terrace|tce|walk|square|sq|highway|hwy|link|grove|rise|loop|pass|path|place|pl";
        private const string ComplexWords = @"estate|est|complex|village|court|crt|ct|mansions|lodge|manor|residence|residences"
            + @"|towers?|block|building|bldg|flats|apartments|house|centre|center|mall|office park|business park|chambers";

        /// <summary>"3 Kingfisher Close Kenrock Estate Tokai" → "3 Kingfisher Close Tokai", only when a street name is left.</summary>
        private static string WithoutInlineEstate(string line)
        {
            string stripped = Regex.Replace(line,
                @"\b[\w']+(\s+(mountain|vineyard|marina|residential|business|office|golf|country|eco|wine|security|lifestyle))*\s+(estate|est|complex)\b\.?",
                " ", RegexOptions.IgnoreCase);
            return stripped != line && Regex.IsMatch(stripped, @"\b(" + StreetWords + @")\b", RegexOptions.IgnoreCase) ? stripped : line;
        }

        /// <summary>
        /// Address lines the geocoder can match: drops "c/o …", shop / unit / flat numbers and, when a street line is present,
        /// estate / complex / building names ("Shop 1, Cascades 4, Tyger Waterfront" → "Cascades 4, Tyger Waterfront";
        /// "33 Darter Rd, Blue Water Est, Kommetjie" → "33 Darter Rd, Kommetjie"; "No 12 Main Rd" → "12 Main Rd").
        /// </summary>
        public static string CleanAddressForLookup(string billingAddress)
        {
            var lines = new List<string>();
            foreach (string raw in AddressLines(billingAddress).Split(','))
            {
                string line = raw.Trim();
                if (line.Length == 0 || Regex.IsMatch(line, @"^(c/o|care of|attn|attention|p\.?\s*o\.?\s*box|postnet)\b", RegexOptions.IgnoreCase))
                    continue;
                line = Regex.Replace(line, @"^.*?\b(\(pty\)\s*ltd|pty\s*ltd|ltd|\(pty\))\.?(?=\s|$)", " ", RegexOptions.IgnoreCase);
                line = Regex.Replace(line, @"\b(ground|first|second|third|fourth|fifth|\d+(st|nd|rd|th))\s+(floor|level)\b", " ", RegexOptions.IgnoreCase);
                line = Regex.Replace(line,
                    @"\b(shop|unit|flat|suite|office|apt|apartment|room|door|stand|erf|section|floor|level)\s*(no\.?|nr\.?|#)?\s*[a-z]?\d+[a-z]?(\s*(&|and|-|/)\s*[a-z]?\d+[a-z]?)*\b",
                    " ", RegexOptions.IgnoreCase);
                line = Regex.Replace(line, @"^\s*(no|nr|number)\.?\s*(?=\d)", string.Empty, RegexOptions.IgnoreCase);
                line = Regex.Replace(line, @"^\s*#\s*\d+\w*\s*", string.Empty);
                line = WithoutInlineEstate(line);
                line = Regex.Replace(line, @"\s{2,}", " ").Trim(' ', '-', ',', '&');
                if (line.Length > 0)
                    lines.Add(line);
            }

            bool IsComplex(string l) => Regex.IsMatch(l, @"\b(" + ComplexWords + @")\b", RegexOptions.IgnoreCase);
            bool IsStreet(string l) => Regex.IsMatch(l, @"\b(" + StreetWords + @")\b", RegexOptions.IgnoreCase)
                || (Regex.IsMatch(l, @"^\d+[a-z]?\s+\S") && !IsComplex(l));
            if (lines.Any(IsStreet))
                lines = lines.Where(l => IsStreet(l) || !IsComplex(l)).ToList();
            return string.Join(", ", lines);
        }

        /// <summary>Address (cleaned unless <paramref name="clean"/> is false), the area suburb when given, then postcode and country.</summary>
        public static string BuildLookupText(Contact contact, string suburb, bool clean = true)
        {
            if (contact == null) return string.Empty;
            var parts = new List<string>();
            string address = clean ? CleanAddressForLookup(contact.BillingAddress) : AddressLines(contact.BillingAddress);
            if (address.Length > 0) parts.Add(address);
            if (!string.IsNullOrWhiteSpace(suburb)) parts.Add(suburb.Trim());
            string postal = (contact.PostalCode ?? string.Empty).Trim();
            if (postal.Length > 0 && address.IndexOf(postal, StringComparison.OrdinalIgnoreCase) < 0) parts.Add(postal);
            if (parts.Count > 0) parts.Add("South Africa");
            return string.Join(", ", parts);
        }

        /// <summary>
        /// Tries, until one matches the address itself: cleaned address + area suburb (when the address lacks it),
        /// cleaned address, then the address as typed. Keeps the first rough match when none is exact (at most 3 lookups).
        /// </summary>
        private GeocodeResult GeocodeContact(Contact contact, GpsSettings settings, out bool usedLookup)
        {
            usedLookup = false;
            string suburb = AreaSuburbForLookup(contact);
            var queries = new List<string>();
            foreach (string q in new[]
            {
                suburb == null ? null : BuildLookupText(contact, suburb),
                BuildLookupText(contact, null),
                BuildLookupText(contact, null, clean: false)
            })
            {
                if (!string.IsNullOrWhiteSpace(q) && !queries.Any(x => Norm(x) == Norm(q)))
                    queries.Add(q);
            }

            GeocodeResult best = null;
            foreach (string query in queries)
            {
                GeocodeResult result = Geocode(query, settings, out bool used, radiusKm: AutoLookupRadiusKm);
                usedLookup |= used;
                if (!result.Success && result.Failure != GeocodeFailure.NotFound)
                    return best ?? result;
                if (result.Success && result.IsPrecise)
                    return result;
                if (best == null || (!best.Success && result.Success))
                    best = result;
            }
            return best ?? GeocodeResult.Fail(GeocodeFailure.NotFound, "No address to look up.");
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

            PurgeExpiredOncePerDay();
            CustomerLocation existing = _locations.GetByContactId(contactId);
            if (!force && existing != null && SameAddress(existing.SourceAddress, addressText) && !existing.RefreshDue)
                return new LocationLookupOutcome { Success = true, Skipped = true, Location = existing };

            GpsSettings settings = _settingsRepo.Get();
            GeocodeResult result = GeocodeContact(contact, settings, out bool usedLookup);
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
                bool azure = result.Provider == GpsSettings.ProviderAzureMaps;
                loc.Source = azure ? CustomerLocation.SourceAzureMaps : CustomerLocation.SourceGeocoder;
                loc.ExpiresDate = azure ? DateTime.Now.AddDays(GpsSettings.AzureMapsKeepDays) : (DateTime?)null;
                loc.Confidence = result.Confidence.HasValue ? (decimal?)Convert.ToDecimal(result.Confidence.Value) : null;
                loc.LocationStatus = result.IsPrecise ? CustomerLocation.StatusApproximate : CustomerLocation.StatusReviewRequired;
            }
            else if (!TryPostcodeCentroid(contact.PostalCode, loc))
            {
                bool dropRough = existing != null && existing.NeedsReview && !existing.IsManual && !existing.IsFromDriver;
                if (dropRough)
                    _locations.Delete(contactId);
                return new LocationLookupOutcome
                {
                    UsedLookup = usedLookup,
                    Location = dropRough ? null : existing,
                    Message = "Address not found and no postcode point to fall back on; set the pin by hand."
                        + (dropRough ? " The old rough point was removed." : string.Empty)
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

        /// <summary>Phone fixes less sure than this (metres) are ignored.</summary>
        public const decimal DriverMaxAccuracyM = 75;
        /// <summary>A delivery this close to our driver point agrees with it.</summary>
        public const double DriverMatchMetres = 60;
        /// <summary>A hand-placed pin is only questioned when deliveries are further away than this.</summary>
        public const double ManualMismatchMetres = 150;
        /// <summary>Deliveries closer together in time than this are the same visit (several orders at one stop).</summary>
        private const double SameVisitHours = 6;
        /// <summary>Older fixes carry at most this weight when averaging, so a point can still drift to where drivers really stop.</summary>
        private const int DriverAverageCap = 5;

        /// <summary>
        /// Called after a Delivered proof with a phone position is saved. The driver's position replaces a geocoder,
        /// Azure Maps or postcode point; a second delivery on another visit within <see cref="DriverMatchMetres"/> marks it checked.
        /// Hand-placed pins are never moved; two visits in a row far from the pin flag it for review.
        /// </summary>
        public void LearnFromDelivery(int contactId, long proofId, DateTime deliveredAt, decimal latitude, decimal longitude,
            decimal? accuracyM, string userName)
        {
            string who = "GPS learn: contact " + contactId + " (proof " + proofId + ")";
            if (contactId == SystemConstants.CustomerConstants.SundryCustomerID)
                return;
            if (latitude == 0 && longitude == 0)
                return;
            if (accuracyM.HasValue && accuracyM.Value > DriverMaxAccuracyM)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    who + ": phone position too rough (+/-" + accuracyM.Value.ToString("0", CultureInfo.InvariantCulture) + " m), not used", userName);
                return;
            }
            if (!IsReady)
                return;

            Contact contact = new ContactsRepository().GetById(contactId);
            if (contact == null
                || (contact.CompanyName ?? string.Empty).StartsWith(SystemConstants.CustomerConstants.SundryCustomerNamePrefix, StringComparison.OrdinalIgnoreCase))
                return;

            CustomerLocation existing = _locations.GetByContactId(contactId);
            bool sameVisit = existing?.LastDriverFixAt != null
                && Math.Abs((deliveredAt - existing.LastDriverFixAt.Value).TotalHours) < SameVisitHours;
            double distance = existing == null ? 0 : GeoDistance.Metres(existing.Latitude, existing.Longitude, latitude, longitude);
            string away = distance.ToString("0", CultureInfo.InvariantCulture) + " m";

            if (existing != null && existing.IsManual)
            {
                if (distance <= ManualMismatchMetres)
                {
                    if (!sameVisit || existing.NeedsReview)
                    {
                        _locations.TouchDriverFix(contactId, existing.DriverFixes + (sameVisit ? 0 : 1), deliveredAt,
                            existing.NeedsReview ? CustomerLocation.StatusVerified : null);
                        AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                            who + ": delivery agrees with the hand-placed pin (" + away + ")"
                            + (existing.NeedsReview ? "; review flag cleared" : string.Empty), userName);
                    }
                    return;
                }

                Tuple<decimal, decimal> previous = _locations.GetPreviousDeliveryFix(contactId, proofId,
                    deliveredAt.AddHours(-SameVisitHours), DriverMaxAccuracyM);
                bool previousAlsoFar = previous != null
                    && GeoDistance.Metres(existing.Latitude, existing.Longitude, previous.Item1, previous.Item2) > ManualMismatchMetres;
                if (previousAlsoFar && !existing.NeedsReview)
                {
                    _locations.TouchDriverFix(contactId, existing.DriverFixes, existing.LastDriverFixAt ?? deliveredAt, CustomerLocation.StatusReviewRequired);
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        who + ": two visits in a row were far from the hand-placed pin (now " + away + "); flagged for review", userName);
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        who + ": delivery was " + away + " from the hand-placed pin; pin kept", userName);
                }
                return;
            }

            var loc = new CustomerLocation
            {
                ContactID = contactId,
                MapAreaID = existing?.MapAreaID,
                SourceAddress = BuildAddressText(contact),
                Source = CustomerLocation.SourceDriver,
                Confidence = null,
                GeocodedAddress = null,
                ExpiresDate = null,
                LastDriverFixAt = deliveredAt,
                UpdatedBy = userName
            };

            string message;
            if (existing != null && existing.IsFromDriver && distance <= DriverMatchMetres)
            {
                if (sameVisit)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        who + ": same visit as the last delivery (" + away + " from the driver point); nothing changed", userName);
                    return;
                }
                int weight = Math.Min(Math.Max(existing.DriverFixes, 1), DriverAverageCap);
                loc.Latitude = (existing.Latitude * weight + latitude) / (weight + 1);
                loc.Longitude = (existing.Longitude * weight + longitude) / (weight + 1);
                loc.DriverFixes = existing.DriverFixes + 1;
                loc.LocationStatus = loc.DriverFixes >= 2 ? CustomerLocation.StatusVerified : CustomerLocation.StatusApproximate;
                message = "delivery agrees with the driver point (" + away + "), " + loc.DriverFixes + " visits"
                    + (loc.IsVerified && !existing.IsVerified ? "; now checked" : string.Empty);
            }
            else
            {
                loc.Latitude = latitude;
                loc.Longitude = longitude;
                loc.DriverFixes = 1;
                loc.LocationStatus = CustomerLocation.StatusApproximate;
                message = existing == null
                    ? "no point yet; set from the delivery"
                    : existing.IsFromDriver
                        ? "delivery was " + away + " from the driver point; moved to the new position"
                        : "replaced the " + existing.SourceDisplay + " point (" + away + " away) with the delivery position";
            }

            loc.Latitude = Math.Round(loc.Latitude, 6);
            loc.Longitude = Math.Round(loc.Longitude, 6);
            loc.Geohash = Geohash.Encode((double)loc.Latitude, (double)loc.Longitude);
            _locations.Upsert(loc);
            AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                who + ": " + message + (accuracyM.HasValue ? " (+/-" + accuracyM.Value.ToString("0", CultureInfo.InvariantCulture) + " m)" : string.Empty), userName);
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

            GpsSettings settings = _settingsRepo.Get();
            string cleaned = CleanAddressForLookup(query);
            if (cleaned.Length == 0 || Norm(cleaned) == Norm(query))
                return Geocode(query, settings, out _);

            GeocodeResult first = Geocode(cleaned, settings, out _);
            if ((first.Success && first.IsPrecise) || (!first.Success && first.Failure != GeocodeFailure.NotFound))
                return first;
            GeocodeResult second = Geocode(query, settings, out _);
            return second.Success && (second.IsPrecise || !first.Success) ? second : first;
        }

        public int LookupsUsedToday()
        {
            return _settingsRepo.GetCallsToday(GpsSettingsRepository.ServiceFor(_settingsRepo.Get()));
        }

        private GeocodeResult Geocode(string address, GpsSettings settings, out bool usedLookup, bool useFocus = true, double? radiusKm = null)
        {
            usedLookup = false;
            if (!settings.HasApiKey)
                return GeocodeResult.Fail(GeocodeFailure.NotConfigured,
                    "No " + settings.ProviderDisplay + " key: add it on System Preferences → GPS & Drivers.");

            if (!TryTakeMinuteSlot(settings.GeocodePerMinute))
                return GeocodeResult.Fail(GeocodeFailure.PerMinuteLimitReached, "Per-minute lookup limit reached; try again in a minute.");

            string service = GpsSettingsRepository.ServiceFor(settings);
            if (!_settingsRepo.TryCountCall(service, settings.GeocodeDailyLimit))
                return GeocodeResult.Fail(GeocodeFailure.DailyLimitReached,
                    "Today's lookup limit (" + settings.GeocodeDailyLimit + ") is used up; the rest will be done tomorrow.");

            usedLookup = true;
            IGeocoder geocoder = settings.IsAzureMaps
                ? (IGeocoder)new AzureMapsGeocoder(settings.AzureMapsKey)
                : new HeigitGeocoder(settings.GeocodeBaseUrl, settings.GeocodeApiKey);
            bool focus = useFocus && settings.HasDepotPoint;
            GeocodeResult result = geocoder.Search(address,
                focus ? (double?)settings.DepotLatitude.Value : null,
                focus ? (double?)settings.DepotLongitude.Value : null,
                radiusKm);
            result.Provider = settings.IsAzureMaps ? GpsSettings.ProviderAzureMaps : GpsSettings.ProviderHeigit;

            if (result.Success && focus && radiusKm.HasValue
                && GeoDistance.Metres(result.Latitude, result.Longitude, (double)settings.DepotLatitude.Value, (double)settings.DepotLongitude.Value) > radiusKm.Value * 1000)
            {
                result = GeocodeResult.Fail(GeocodeFailure.NotFound, "The only match was more than " + radiusKm.Value + " km from the depot.");
            }

            if (result.Failure == GeocodeFailure.DailyLimitReached)
                _settingsRepo.MarkDailyLimitReached(service, settings.GeocodeDailyLimit);
            if (!result.Success && result.Failure != GeocodeFailure.NotFound)
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "Geocode failed (" + settings.ProviderDisplay + "): " + result.Failure + " " + result.Message);
            return result;
        }

        private static DateTime _lastPurge = DateTime.MinValue;

        /// <summary>Azure Maps points past their keep date are removed (terms); runs at most once a day.</summary>
        public void PurgeExpiredOncePerDay()
        {
            if (_lastPurge.Date == DateTime.Today)
                return;
            _lastPurge = DateTime.Today;
            try
            {
                int removed = _locations.DeleteExpired();
                if (removed > 0)
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, "GPS: removed " + removed + " expired Azure Maps point(s)");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "GPS purge expired: " + ex.Message);
            }
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
