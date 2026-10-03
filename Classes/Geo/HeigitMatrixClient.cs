using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TrackerSQL.Classes.Geo
{
    /// <summary>
    /// HeiGIT openrouteservice matrix (driving-car): driving seconds and metres between every pair of points in one call.
    /// Standard plan: 500 calls a day, 40 a minute, at most 3500 pairs a call. HTTP 403 = daily quota used up (or the key
    /// was refused), 429 = per-minute limit. The key goes in the Authorization header.
    /// </summary>
    public class HeigitMatrixClient
    {
        /// <summary>59 × 59 = 3481 pairs, just inside the 3500 a call.</summary>
        public const int MaxLocations = 59;
        private const int TimeoutMs = 8000;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public enum Failure { None, NotConfigured, DailyLimitReached, PerMinuteLimitReached, TooMany, Error }

        public class Result
        {
            public Failure Failure;
            public string Message;
            /// <summary>[from, to] driving seconds; -1 = no road found.</summary>
            public int[,] Seconds;
            /// <summary>[from, to] driving metres; -1 = no road found.</summary>
            public int[,] Metres;
            public bool Ok => Failure == Failure.None;

            public static Result Fail(Failure failure, string message) => new Result { Failure = failure, Message = message };
        }

        public HeigitMatrixClient(string baseUrl, string apiKey)
        {
            _baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.heigit.org" : baseUrl.Trim().TrimEnd('/');
            _apiKey = apiKey?.Trim();
        }

        /// <param name="points">Latitude, longitude of each point.</param>
        public Result Fetch(IList<Tuple<double, double>> points)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return Result.Fail(Failure.NotConfigured, "No HeiGIT key: add it on System Preferences → GPS & Drivers.");
            if (points == null || points.Count < 2)
                return Result.Fail(Failure.Error, "Fewer than two points.");
            if (points.Count > MaxLocations)
                return Result.Fail(Failure.TooMany, "More than " + MaxLocations + " points in one route.");

            var body = new StringBuilder("{\"locations\":[");
            for (int i = 0; i < points.Count; i++)
            {
                if (i > 0) body.Append(',');
                body.Append('[')
                    .Append(points[i].Item2.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                    .Append(points[i].Item1.ToString("0.######", CultureInfo.InvariantCulture)).Append(']');
            }
            body.Append("],\"metrics\":[\"duration\",\"distance\"],\"units\":\"m\"}");

            string json;
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var request = (HttpWebRequest)WebRequest.Create(_baseUrl + "/openrouteservice/v2/matrix/driving-car");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json";
                request.UserAgent = "TrackerSQL/1.0 (Quaffee delivery tracker)";
                request.Headers[HttpRequestHeader.Authorization] = _apiKey;
                request.Timeout = TimeoutMs;
                request.ReadWriteTimeout = TimeoutMs;
                byte[] bytes = Encoding.UTF8.GetBytes(body.ToString());
                request.ContentLength = bytes.Length;
                using (Stream stream = request.GetRequestStream())
                    stream.Write(bytes, 0, bytes.Length);

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                    json = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                var http = ex.Response as HttpWebResponse;
                int status = http != null ? (int)http.StatusCode : 0;
                if (status == 403)
                    return Result.Fail(Failure.DailyLimitReached, "HeiGIT says today's road time calls are used up (or the key was refused).");
                if (status == 429)
                    return Result.Fail(Failure.PerMinuteLimitReached, "Too many road time calls this minute.");
                if (status == 401)
                    return Result.Fail(Failure.NotConfigured, "The HeiGIT key was refused; check it on System Preferences → GPS & Drivers.");
                return Result.Fail(Failure.Error,
                    status > 0 ? "HeiGIT road times error (HTTP " + status + ")." : "Could not reach HeiGIT for road times: " + ex.Status + ".");
            }
            catch (Exception ex)
            {
                return Result.Fail(Failure.Error, "HeiGIT road times error: " + ex.GetType().Name + ".");
            }

            try
            {
                JObject parsed = JObject.Parse(json);
                int n = points.Count;
                int[,] seconds = Read(parsed["durations"] as JArray, n);
                int[,] metres = Read(parsed["distances"] as JArray, n);
                if (seconds == null || metres == null)
                    return Result.Fail(Failure.Error, "The HeiGIT road times answer could not be read.");
                return new Result { Failure = Failure.None, Seconds = seconds, Metres = metres };
            }
            catch (Exception)
            {
                return Result.Fail(Failure.Error, "The HeiGIT road times answer could not be read.");
            }
        }

        private static int[,] Read(JArray rows, int n)
        {
            if (rows == null || rows.Count != n)
                return null;
            var values = new int[n, n];
            for (int a = 0; a < n; a++)
            {
                var row = rows[a] as JArray;
                if (row == null || row.Count != n)
                    return null;
                for (int b = 0; b < n; b++)
                {
                    JToken v = row[b];
                    values[a, b] = v == null || v.Type == JTokenType.Null ? -1 : (int)Math.Round(v.Value<double>());
                }
            }
            return values;
        }
    }
}
