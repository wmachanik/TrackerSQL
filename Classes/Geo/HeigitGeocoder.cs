using System;
using System.Globalization;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace TrackerSQL.Classes.Geo
{
    /// <summary>
    /// HeiGIT openrouteservice geocoder (Pelias search). Standard plan: 3000 lookups a day, 100 a minute.
    /// HTTP 403 = daily quota used up, HTTP 429 = per-minute limit. The key goes in the Authorization header.
    /// </summary>
    public class HeigitGeocoder : IGeocoder
    {
        private const int TimeoutMs = 10000;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public HeigitGeocoder(string baseUrl, string apiKey)
        {
            _baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.heigit.org" : baseUrl.Trim().TrimEnd('/');
            _apiKey = apiKey?.Trim();
        }

        public GeocodeResult Search(string addressText, double? focusLatitude, double? focusLongitude, double? radiusKm = null)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return GeocodeResult.Fail(GeocodeFailure.NotConfigured, "No geocoding key: add it on System Preferences → GPS & Drivers.");
            if (string.IsNullOrWhiteSpace(addressText))
                return GeocodeResult.Fail(GeocodeFailure.NotFound, "No address to look up.");

            string url = _baseUrl + "/pelias/v1/search?text=" + Uri.EscapeDataString(addressText.Trim())
                + "&boundary.country=ZA&size=1";
            if (focusLatitude.HasValue && focusLongitude.HasValue)
            {
                string lat = focusLatitude.Value.ToString("0.######", CultureInfo.InvariantCulture);
                string lon = focusLongitude.Value.ToString("0.######", CultureInfo.InvariantCulture);
                url += "&focus.point.lat=" + lat + "&focus.point.lon=" + lon;
                if (radiusKm.HasValue && radiusKm.Value > 0)
                {
                    url += "&boundary.circle.lat=" + lat + "&boundary.circle.lon=" + lon
                        + "&boundary.circle.radius=" + radiusKm.Value.ToString("0.#", CultureInfo.InvariantCulture);
                }
            }

            string body;
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "application/json";
                request.UserAgent = "TrackerSQL/1.0 (Quaffee delivery tracker)";
                request.Headers[HttpRequestHeader.Authorization] = _apiKey;
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
                if (status == 403)
                    return GeocodeResult.Fail(GeocodeFailure.DailyLimitReached, "The geocoder says today's lookups are used up (or the key was refused).");
                if (status == 429)
                    return GeocodeResult.Fail(GeocodeFailure.PerMinuteLimitReached, "Too many lookups this minute; try again shortly.");
                if (status == 401)
                    return GeocodeResult.Fail(GeocodeFailure.NotConfigured, "The geocoding key was refused; check it on System Preferences → GPS & Drivers.");
                return GeocodeResult.Fail(GeocodeFailure.Error,
                    status > 0 ? "Geocoder error (HTTP " + status + ")." : "Could not reach the geocoder: " + ex.Status + ".");
            }
            catch (Exception ex)
            {
                return GeocodeResult.Fail(GeocodeFailure.Error, "Geocoder error: " + ex.GetType().Name + ".");
            }

            try
            {
                JObject json = JObject.Parse(body);
                var features = json["features"] as JArray;
                if (features == null || features.Count == 0)
                    return GeocodeResult.Fail(GeocodeFailure.NotFound, "The geocoder could not find this address.");

                JToken feature = features[0];
                var coords = feature["geometry"]?["coordinates"] as JArray;
                if (coords == null || coords.Count < 2)
                    return GeocodeResult.Fail(GeocodeFailure.NotFound, "The geocoder returned no point for this address.");

                JToken props = feature["properties"];
                string layer = props?["layer"]?.Value<string>();
                if (GeocodeResult.IsCoarseLayer(layer))
                    return GeocodeResult.Fail(GeocodeFailure.NotFound, "The geocoder only matched the province or country, not the address.");
                return new GeocodeResult
                {
                    Failure = GeocodeFailure.None,
                    Longitude = coords[0].Value<double>(),
                    Latitude = coords[1].Value<double>(),
                    Label = props?["label"]?.Value<string>(),
                    Layer = layer,
                    Confidence = props?["confidence"]?.Type == JTokenType.Float || props?["confidence"]?.Type == JTokenType.Integer
                        ? props["confidence"].Value<double>()
                        : (double?)null
                };
            }
            catch (Exception)
            {
                return GeocodeResult.Fail(GeocodeFailure.Error, "The geocoder answer could not be read.");
            }
        }
    }
}
