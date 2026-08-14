using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Minimal WooCommerce REST client (v3). Phase 1: Test Connection only.
    /// </summary>
    public partial class WooCommerceApiClient
    {
        public class ConnectionTestResult
        {
            public bool Succeeded { get; set; }
            public string Detail { get; set; }
            public int? HttpStatus { get; set; }
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
                // Prefer TLS 1.2 for modern Woo hosts.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                // 1) Basic auth + system/status
                // 2) Basic auth + products (status often restricted)
                // 3) Query-string auth (hosts that strip Authorization headers)
                var attempts = new[]
                {
                    new { Url = baseUrl + "/wp-json/wc/v3/system/status", Mode = "basic", Label = "system/status" },
                    new { Url = baseUrl + "/wp-json/wc/v3/products?per_page=1", Mode = "basic", Label = "products" },
                    new
                    {
                        Url = baseUrl + "/wp-json/wc/v3/products?per_page=1"
                            + "&consumer_key=" + Uri.EscapeDataString(consumerKeyPlain)
                            + "&consumer_secret=" + Uri.EscapeDataString(consumerSecretPlain),
                        Mode = "query",
                        Label = "products (query auth)"
                    }
                };

                string lastDetail = null;
                int? lastStatus = null;

                using (var handler = new HttpClientHandler())
                {
                    // Follow redirects; some WP sites bounce http→https or add www.
                    handler.AllowAutoRedirect = true;

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("TrackerSQL-WooClient/3.0");

                        foreach (var attempt in attempts)
                        {
                            using (var request = new HttpRequestMessage(HttpMethod.Get, attempt.Url))
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
                if (string.IsNullOrWhiteSpace(body))
                    return string.Empty;
                body = body.Replace("\r", " ").Replace("\n", " ").Trim();
                if (body.Length > 180)
                    body = body.Substring(0, 180) + "…";
                return body;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
