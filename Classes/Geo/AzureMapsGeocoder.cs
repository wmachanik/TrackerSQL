using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace TrackerSQL.Classes.Geo
{
    /// <summary>
    /// Azure Maps Get Geocoding (api-version 2025-01-01, TomTom and other data). The subscription key goes in the
    /// subscription-key header. Terms: geocodes stored outside Azure may be kept up to 6 months (see GpsSettings.AzureMapsKeepDays);
    /// only the co-ordinates are kept, and they may be shown on our OpenStreetMap map.
    /// </summary>
    public class AzureMapsGeocoder : IGeocoder
    {
        private const int TimeoutMs = 10000;
        private const string Endpoint = "https://atlas.microsoft.com/geocode?api-version=2025-01-01";
        private readonly string _apiKey;

        public AzureMapsGeocoder(string apiKey)
        {
            _apiKey = apiKey?.Trim();
        }

        public GeocodeResult Search(string addressText, double? focusLatitude, double? focusLongitude, double? radiusKm = null)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return GeocodeResult.Fail(GeocodeFailure.NotConfigured, "No Azure Maps key: add it on System Preferences → GPS & Drivers.");
            if (string.IsNullOrWhiteSpace(addressText))
                return GeocodeResult.Fail(GeocodeFailure.NotFound, "No address to look up.");

            string url = Endpoint + "&top=1&query=" + Uri.EscapeDataString(addressText.Trim());
            if (focusLatitude.HasValue && focusLongitude.HasValue)
            {
                url += "&coordinates=" + F(focusLongitude.Value) + "," + F(focusLatitude.Value);
                if (radiusKm.HasValue && radiusKm.Value > 0)
                {
                    double dLat = radiusKm.Value / 111.32;
                    double dLng = radiusKm.Value / (111.32 * Math.Max(0.1, Math.Cos(focusLatitude.Value * Math.PI / 180)));
                    url += "&bbox=" + F(focusLongitude.Value - dLng) + "," + F(focusLatitude.Value - dLat)
                        + "," + F(focusLongitude.Value + dLng) + "," + F(focusLatitude.Value + dLat);
                }
            }

            string body;
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "application/geo+json, application/json";
                request.UserAgent = "TrackerSQL/1.0 (Quaffee delivery tracker)";
                request.Headers["subscription-key"] = _apiKey;
                request.Timeout = TimeoutMs;
                request.ReadWriteTimeout = TimeoutMs;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                    body = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                var http = ex.Response as HttpWebResponse;
                int status = http != null ? (int)http.StatusCode : 0;
                if (status == 401 || status == 403)
                    return GeocodeResult.Fail(GeocodeFailure.NotConfigured, "Azure Maps refused the key (HTTP " + status + "); check it on System Preferences → GPS & Drivers.");
                if (status == 429)
                    return GeocodeResult.Fail(GeocodeFailure.PerMinuteLimitReached, "Azure Maps says too many lookups; try again shortly.");
                return GeocodeResult.Fail(GeocodeFailure.Error,
                    status > 0 ? "Azure Maps error (HTTP " + status + ")." : "Could not reach Azure Maps: " + ex.Status + ".");
            }
            catch (Exception ex)
            {
                return GeocodeResult.Fail(GeocodeFailure.Error, "Azure Maps error: " + ex.GetType().Name + ".");
            }

            try
            {
                JObject json = JObject.Parse(body);
                var features = json["features"] as JArray;
                if (features == null || features.Count == 0)
                    return GeocodeResult.Fail(GeocodeFailure.NotFound, "Azure Maps could not find this address.");

                JToken feature = features[0];
                var coords = feature["geometry"]?["coordinates"] as JArray;
                if (coords == null || coords.Count < 2)
                    return GeocodeResult.Fail(GeocodeFailure.NotFound, "Azure Maps returned no point for this address.");

                JToken props = feature["properties"];
                string layer = LayerFor(props?["type"]?.Value<string>());
                if (GeocodeResult.IsCoarseLayer(layer))
                    return GeocodeResult.Fail(GeocodeFailure.NotFound, "Azure Maps only matched the province or country, not the address.");

                var matchCodes = (props?["matchCodes"] as JArray)?.Select(t => t.Value<string>()).ToList();
                return new GeocodeResult
                {
                    Failure = GeocodeFailure.None,
                    Longitude = coords[0].Value<double>(),
                    Latitude = coords[1].Value<double>(),
                    Label = props?["address"]?["formattedAddress"]?.Value<string>(),
                    Layer = layer,
                    Confidence = ConfidenceFor(props?["confidence"]?.Value<string>(), matchCodes)
                };
            }
            catch (Exception)
            {
                return GeocodeResult.Fail(GeocodeFailure.Error, "The Azure Maps answer could not be read.");
            }
        }

        /// <summary>Azure entity type → the Pelias-style layer names GeocodeResult understands.</summary>
        private static string LayerFor(string type)
        {
            switch ((type ?? string.Empty).Trim())
            {
                case "Address": return "address";
                case "RoadBlock":
                case "RoadIntersection": return "street";
                case "Neighborhood": return "neighbourhood";
                case "PopulatedPlace": return "locality";
                case "Postcode1": return "postalcode";
                case "AdminDivision2": return "county";
                case "AdminDivision1": return "region";
                case "CountryRegion": return "country";
                default: return string.IsNullOrWhiteSpace(type) ? null : type.ToLowerInvariant();
            }
        }

        private static double? ConfidenceFor(string confidence, System.Collections.Generic.List<string> matchCodes)
        {
            double value;
            switch ((confidence ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "high": value = 0.9; break;
                case "medium": value = 0.6; break;
                case "low": value = 0.3; break;
                default: return null;
            }
            bool unsure = matchCodes != null && matchCodes.Any(c =>
                string.Equals(c, "Ambiguous", StringComparison.OrdinalIgnoreCase) || string.Equals(c, "UpHierarchy", StringComparison.OrdinalIgnoreCase));
            return unsure ? Math.Min(value, 0.6) : value;
        }

        private static string F(double value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }
    }
}
