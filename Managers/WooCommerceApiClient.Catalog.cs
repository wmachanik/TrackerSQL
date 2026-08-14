using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Managers
{
    public partial class WooCommerceApiClient
    {
        public class ApiCredentials
        {
            public string StoreBaseUrl { get; set; }
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
        }

        public class CategoryPullResult
        {
            public List<WooCategoryDto> Categories { get; set; } = new List<WooCategoryDto>();
            /// <summary>Woo X-WP-Total when available; otherwise 0.</summary>
            public int TotalOnWoo { get; set; }
        }

        public CategoryPullResult GetCategories(ApiCredentials creds)
        {
            var result = new CategoryPullResult();
            int page = 1;
            while (page <= 50)
            {
                string path = "/wp-json/wc/v3/products/categories?per_page=100&page=" + page;
                int? wpTotal;
                JToken json = GetJson(creds, path, out wpTotal);
                if (page == 1 && wpTotal.HasValue)
                    result.TotalOnWoo = wpTotal.Value;

                var arr = json as JArray;
                if (arr == null || arr.Count == 0)
                    break;
                foreach (JToken t in arr)
                {
                    result.Categories.Add(new WooCategoryDto
                    {
                        Id = t.Value<long?>("id") ?? 0,
                        Name = t.Value<string>("name"),
                        Parent = t.Value<long?>("parent") ?? 0,
                        Count = t.Value<int?>("count") ?? 0
                    });
                }
                if (arr.Count < 100)
                    break;
                page++;
            }
            if (result.TotalOnWoo <= 0)
                result.TotalOnWoo = result.Categories.Count;
            return result;
        }

        public List<WooProductDto> GetProductsAndVariations(ApiCredentials creds, int maxProducts = 200)
        {
            var results = new List<WooProductDto>();
            int page = 1;
            int loaded = 0;
            while (page <= 50 && loaded < maxProducts)
            {
                string path = "/wp-json/wc/v3/products?per_page=50&page=" + page + "&status=any";
                JToken json = GetJson(creds, path);
                var arr = json as JArray;
                if (arr == null || arr.Count == 0)
                    break;

                foreach (JToken t in arr)
                {
                    if (loaded >= maxProducts)
                        break;

                    var product = ParseProduct(t);
                    loaded++;

                    if (string.Equals(product.Type, "variable", StringComparison.OrdinalIgnoreCase))
                    {
                        var variations = GetVariations(creds, product.Id);
                        if (variations.Count == 0)
                        {
                            results.Add(product);
                        }
                        else
                        {
                            foreach (var v in variations)
                            {
                                v.CategoryIds = product.CategoryIds;
                                v.CategoriesLabel = product.CategoriesLabel;
                                v.ParentId = product.Id;
                                results.Add(v);
                            }
                        }
                    }
                    else
                    {
                        results.Add(product);
                    }
                }

                if (arr.Count < 50)
                    break;
                page++;
            }
            return results;
        }

        public bool SetCatalogStatus(ApiCredentials creds, long productId, long? variationId, bool enabled, out string detail)
        {
            detail = null;
            string status = enabled ? "publish" : "private";
            string path = variationId.HasValue && variationId.Value > 0
                ? "/wp-json/wc/v3/products/" + productId + "/variations/" + variationId.Value
                : "/wp-json/wc/v3/products/" + productId;
            string body = "{\"status\":\"" + status + "\"}";
            try
            {
                JToken json = SendJson(creds, HttpMethod.Put, path, body);
                detail = status;
                return json != null;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                AppLogger.WriteLog("woo", "SetCatalogStatus failed: " + ex.Message);
                return false;
            }
        }

        private List<WooProductDto> GetVariations(ApiCredentials creds, long productId)
        {
            var list = new List<WooProductDto>();
            int page = 1;
            while (page <= 20)
            {
                string path = "/wp-json/wc/v3/products/" + productId + "/variations?per_page=100&page=" + page;
                JToken json;
                try
                {
                    json = GetJson(creds, path);
                }
                catch
                {
                    break;
                }
                var arr = json as JArray;
                if (arr == null || arr.Count == 0)
                    break;
                foreach (JToken t in arr)
                {
                    list.Add(new WooProductDto
                    {
                        Id = productId,
                        VariationId = t.Value<long?>("id"),
                        Name = t.Value<string>("name"),
                        Sku = t.Value<string>("sku"),
                        Status = t.Value<string>("status"),
                        Type = "variation",
                        ParentId = productId
                    });
                }
                if (arr.Count < 100)
                    break;
                page++;
            }
            return list;
        }

        private static WooProductDto ParseProduct(JToken t)
        {
            var dto = new WooProductDto
            {
                Id = t.Value<long?>("id") ?? 0,
                Name = t.Value<string>("name"),
                Sku = t.Value<string>("sku"),
                Status = t.Value<string>("status"),
                Type = t.Value<string>("type")
            };
            var cats = t["categories"] as JArray;
            if (cats != null)
            {
                var names = new List<string>();
                foreach (JToken c in cats)
                {
                    long id = c.Value<long?>("id") ?? 0;
                    if (id > 0)
                        dto.CategoryIds.Add(id);
                    string n = c.Value<string>("name");
                    if (!string.IsNullOrEmpty(n))
                        names.Add(n);
                }
                dto.CategoriesLabel = string.Join(", ", names);
            }
            return dto;
        }

        private JToken GetJson(ApiCredentials creds, string pathAndQuery)
        {
            int? unused;
            return SendJson(creds, HttpMethod.Get, pathAndQuery, null, out unused);
        }

        private JToken GetJson(ApiCredentials creds, string pathAndQuery, out int? wpTotal)
        {
            return SendJson(creds, HttpMethod.Get, pathAndQuery, null, out wpTotal);
        }

        private JToken SendJson(ApiCredentials creds, HttpMethod method, string pathAndQuery, string jsonBody)
        {
            int? unused;
            return SendJson(creds, method, pathAndQuery, jsonBody, out unused);
        }

        private JToken SendJson(ApiCredentials creds, HttpMethod method, string pathAndQuery, string jsonBody, out int? wpTotal)
        {
            wpTotal = null;
            if (creds == null)
                throw new ArgumentNullException(nameof(creds));
            string baseUrl = NormalizeStoreBaseUrl(creds.StoreBaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
                throw new InvalidOperationException("Store URL is required.");

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // Prefer query auth first for catalog (many hosts strip Authorization).
            var modes = new[] { "query", "basic" };
            Exception last = null;
            foreach (string mode in modes)
            {
                try
                {
                    string url = baseUrl + pathAndQuery;
                    if (mode == "query")
                    {
                        string sep = url.Contains("?") ? "&" : "?";
                        url = url + sep
                            + "consumer_key=" + Uri.EscapeDataString(creds.ConsumerKey)
                            + "&consumer_secret=" + Uri.EscapeDataString(creds.ConsumerSecret);
                    }

                    using (var handler = new HttpClientHandler { AllowAutoRedirect = true })
                    using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) })
                    using (var request = new HttpRequestMessage(method, url))
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("TrackerSQL-WooClient/3.0");
                        if (mode == "basic")
                        {
                            var authBytes = Encoding.ASCII.GetBytes(creds.ConsumerKey + ":" + creds.ConsumerSecret);
                            request.Headers.Authorization =
                                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                        }
                        if (jsonBody != null)
                            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                        string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        if (!response.IsSuccessStatusCode)
                            throw new InvalidOperationException("HTTP " + (int)response.StatusCode + " — " + Truncate(body));

                        IEnumerable<string> totalVals;
                        if (response.Headers.TryGetValues("X-WP-Total", out totalVals))
                        {
                            int parsed;
                            if (int.TryParse(totalVals.FirstOrDefault(), out parsed))
                                wpTotal = parsed;
                        }

                        if (string.IsNullOrWhiteSpace(body))
                            return new JObject();
                        return JToken.Parse(body);
                    }
                }
                catch (Exception ex)
                {
                    last = ex;
                }
            }
            throw last ?? new InvalidOperationException("Woo API request failed.");
        }

        private static string Truncate(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= 160 ? s : s.Substring(0, 160) + "…";
        }
    }
}
