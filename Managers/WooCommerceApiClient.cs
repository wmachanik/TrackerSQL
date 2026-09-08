using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Minimal WooCommerce REST client (v3). Shared HttpClient + sticky auth per store.
    /// </summary>
    public partial class WooCommerceApiClient
    {
        private static readonly object SharedClientLock = new object();
        private static HttpClient _sharedClient;

        /// <summary>Preferred auth mode per normalized store base URL ("query" or "basic").</summary>
        private static readonly ConcurrentDictionary<string, string> PreferredAuthByStore =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public class ApiCredentials
        {
            public string StoreBaseUrl { get; set; }
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
        }

        public class ConnectionTestResult
        {
            public bool Succeeded { get; set; }
            public string Detail { get; set; }
            public int? HttpStatus { get; set; }
        }

        private static HttpClient SharedClient
        {
            get
            {
                if (_sharedClient != null)
                    return _sharedClient;
                lock (SharedClientLock)
                {
                    if (_sharedClient != null)
                        return _sharedClient;
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    var handler = new HttpClientHandler { AllowAutoRedirect = true };
                    var client = new HttpClient(handler)
                    {
                        Timeout = TimeSpan.FromSeconds(60)
                    };
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("TrackerSQL-WooClient/3.0");
                    _sharedClient = client;
                    return _sharedClient;
                }
            }
        }

        internal static void RememberAuthMode(string storeBaseUrl, string mode)
        {
            string key = NormalizeStoreBaseUrl(storeBaseUrl);
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(mode))
                return;
            PreferredAuthByStore[key] = mode.Trim().ToLowerInvariant();
        }

        internal static string GetPreferredAuthMode(string storeBaseUrl)
        {
            string key = NormalizeStoreBaseUrl(storeBaseUrl);
            string mode;
            if (!string.IsNullOrEmpty(key) && PreferredAuthByStore.TryGetValue(key, out mode))
                return mode;
            return null;
        }

        public ConnectionTestResult TestConnection(WooCommerceSettings settings, string consumerKeyPlain, string consumerSecretPlain)
        {
            try
            {
                if (settings == null || string.IsNullOrWhiteSpace(settings.StoreBaseUrl))
                {
                    return new ConnectionTestResult
                    {
                        Succeeded = false,
                        Detail = "Store URL is required. Enter the store base URL and Save, or enter it before testing."
                    };
                }

                if (string.IsNullOrWhiteSpace(consumerKeyPlain) || string.IsNullOrWhiteSpace(consumerSecretPlain))
                {
                    return new ConnectionTestResult
                    {
                        Succeeded = false,
                        Detail = "Consumer key and secret are required (enter them, or Save first so the stored secrets can be used)."
                    };
                }

                string baseUrl = NormalizeStoreBaseUrl(settings.StoreBaseUrl);
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var attempts = new[]
                {
                    new { Path = "/wp-json/wc/v3/system/status", Mode = "basic", Label = "system/status" },
                    new { Path = "/wp-json/wc/v3/products?per_page=1", Mode = "basic", Label = "products" },
                    new { Path = "/wp-json/wc/v3/products?per_page=1", Mode = "query", Label = "products (query auth)" }
                };

                string preferred = GetPreferredAuthMode(baseUrl);
                if (!string.IsNullOrEmpty(preferred))
                {
                    attempts = new[]
                    {
                        new { Path = "/wp-json/wc/v3/products?per_page=1", Mode = preferred, Label = "products (" + preferred + ")" },
                        new { Path = "/wp-json/wc/v3/system/status", Mode = preferred == "basic" ? "basic" : "query", Label = "system/status (" + preferred + ")" },
                        new { Path = "/wp-json/wc/v3/products?per_page=1", Mode = preferred == "basic" ? "query" : "basic", Label = "products (fallback)" }
                    };
                }

                string lastDetail = null;
                int? lastStatus = null;
                HttpClient client = SharedClient;

                foreach (var attempt in attempts)
                {
                    string url = baseUrl + attempt.Path;
                    if (attempt.Mode == "query")
                    {
                        string sep = url.Contains("?") ? "&" : "?";
                        url = url + sep
                            + "consumer_key=" + Uri.EscapeDataString(consumerKeyPlain)
                            + "&consumer_secret=" + Uri.EscapeDataString(consumerSecretPlain);
                    }

                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        if (attempt.Mode == "basic")
                        {
                            var authBytes = Encoding.ASCII.GetBytes(consumerKeyPlain + ":" + consumerSecretPlain);
                            request.Headers.Authorization =
                                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                        }

                        HttpResponseMessage response;
                        try
                        {
                            response = client.SendAsync(request).GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            lastDetail = attempt.Label + ": " + ex.Message;
                            AppLogger.WriteLog("woo", "Test attempt failed — " + lastDetail);
                            continue;
                        }

                        using (response)
                        {
                            int code = (int)response.StatusCode;
                            lastStatus = code;
                            if (response.IsSuccessStatusCode)
                            {
                                RememberAuthMode(baseUrl, attempt.Mode);
                                string ok = "OK via " + attempt.Label + " — HTTP " + code + " @ " + baseUrl;
                                AppLogger.WriteLog("woo", "Test connection " + ok);
                                return new ConnectionTestResult
                                {
                                    Succeeded = true,
                                    HttpStatus = code,
                                    Detail = ok
                                };
                            }

                            string body = SafeReadBody(response);
                            lastDetail = attempt.Label + " HTTP " + code
                                + " @ " + baseUrl
                                + (string.IsNullOrEmpty(body) ? string.Empty : " — " + body);
                            AppLogger.WriteLog("woo", "Test attempt failed — " + lastDetail);
                        }
                    }
                }

                return new ConnectionTestResult
                {
                    Succeeded = false,
                    HttpStatus = lastStatus,
                    Detail = lastDetail ?? ("No response from " + baseUrl)
                };
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("woo", "Test connection exception: " + ex.Message);
                return new ConnectionTestResult
                {
                    Succeeded = false,
                    Detail = ex.Message
                };
            }
        }

        public static string NormalizeStoreBaseUrl(string storeBaseUrl)
        {
            string url = (storeBaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (url.EndsWith("/wp-admin", StringComparison.OrdinalIgnoreCase))
                url = url.Substring(0, url.Length - "/wp-admin".Length).TrimEnd('/');
            if (url.EndsWith("/wp-json", StringComparison.OrdinalIgnoreCase))
                url = url.Substring(0, url.Length - "/wp-json".Length).TrimEnd('/');
            return url;
        }

        public static string SuggestAdminUrl(string storeBaseUrl)
        {
            string baseUrl = NormalizeStoreBaseUrl(storeBaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
                return string.Empty;
            return baseUrl + "/wp-admin";
        }

        private static string SafeReadBody(HttpResponseMessage response)
        {
            try
            {
                string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(body))
                    return string.Empty;
                body = body.Replace("\r", " ").Replace("\n", " ").Trim();
                return body.Length > 200 ? body.Substring(0, 200) + "…" : body;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
