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

        /// <summary>
        /// Stage 1: global product attributes only (no term bodies). TermCount from X-WP-Total.
        /// </summary>
        public List<WooGlobalAttributeDto> GetGlobalAttributes(ApiCredentials creds)
        {
            var results = new List<WooGlobalAttributeDto>();
            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            int page = 1;
            while (page <= 20)
            {
                string path = "/wp-json/wc/v3/products/attributes?per_page=100&page=" + page;
                JToken json = GetJson(creds, path);
                var arr = json as JArray;
                if (arr == null || arr.Count == 0)
                    break;

                foreach (JToken a in arr)
                {
                    long id = a.Value<long?>("id") ?? 0;
                    string name = a.Value<string>("name");
                    if (id <= 0 || string.IsNullOrWhiteSpace(name))
                        continue;

                    int termCount = 0;
                    try
                    {
                        int? wpTotal;
                        GetJson(creds, "/wp-json/wc/v3/products/attributes/" + id + "/terms?per_page=1", out wpTotal);
                        termCount = wpTotal ?? 0;
                    }
                    catch
                    {
                        termCount = 0;
                    }

                    results.Add(new WooGlobalAttributeDto
                    {
                        Id = id,
                        Name = name.Trim(),
                        Slug = a.Value<string>("slug"),
                        TermCount = termCount
                    });
                }

                if (arr.Count < 100)
                    break;
                page++;
            }

            return results;
        }

        /// <summary>
        /// Stage 2: terms (options) for one global attribute. Term count is Woo product usage.
        /// </summary>
        public List<WooAttributeValue> GetAttributeTerms(ApiCredentials creds, long attributeId, string attributeName)
        {
            var results = new List<WooAttributeValue>();
            if (creds == null)
                throw new ArgumentNullException(nameof(creds));
            if (attributeId <= 0)
                return results;

            string name = (attributeName ?? string.Empty).Trim();
            int termPage = 1;
            while (termPage <= 50)
            {
                string termPath = "/wp-json/wc/v3/products/attributes/" + attributeId
                    + "/terms?per_page=100&page=" + termPage;
                JToken termJson;
                try
                {
                    termJson = GetJson(creds, termPath);
                }
                catch
                {
                    break;
                }
                var terms = termJson as JArray;
                if (terms == null || terms.Count == 0)
                    break;

                foreach (JToken t in terms)
                {
                    string option = t.Value<string>("name");
                    if (string.IsNullOrWhiteSpace(option))
                        continue;
                    results.Add(new WooAttributeValue
                    {
                        Name = name,
                        Option = option.Trim(),
                        SampleCount = t.Value<int?>("count") ?? 0
                    });
                }

                if (terms.Count < 100)
                    break;
                termPage++;
            }

            return results;
        }

        public List<WooProductDto> GetProductsAndVariations(ApiCredentials creds, int maxProducts = 2000)
        {
            int unusedScanned;
            bool unusedCap;
            return GetProductsAndVariations(creds, out unusedScanned, out unusedCap, maxProducts);
        }

        public List<WooProductDto> GetProductsAndVariations(
            ApiCredentials creds, out int parentsScanned, out bool hitCatalogCap, int maxProducts = 2000)
        {
            var results = new List<WooProductDto>();
            parentsScanned = 0;
            hitCatalogCap = false;
            var variableParents = new List<WooProductDto>();
            int page = 1;
            while (page <= 100 && parentsScanned < maxProducts)
            {
                string path = "/wp-json/wc/v3/products?per_page=50&page=" + page + "&status=publish";
                JToken json = GetJson(creds, path);
                var arr = json as JArray;
                if (arr == null || arr.Count == 0)
                    break;

                foreach (JToken t in arr)
                {
                    if (parentsScanned >= maxProducts)
                    {
                        hitCatalogCap = true;
                        break;
                    }

                    var product = ParseProduct(t);
                    parentsScanned++;

                    if (string.Equals(product.Type, "variable", StringComparison.OrdinalIgnoreCase))
                        variableParents.Add(product);
                    else if (IsCatalogEligible(product))
                        results.Add(product);
                }

                if (parentsScanned >= maxProducts)
                {
                    hitCatalogCap = true;
                    break;
                }
                if (arr.Count < 50)
                    break;
                page++;
            }

            AppendVariableProductsParallel(creds, variableParents, results);
            return results;
        }

        /// <summary>Fetch variations for variable parents with bounded concurrency (shared HttpClient).</summary>
        private void AppendVariableProductsParallel(
            ApiCredentials creds,
            List<WooProductDto> variableParents,
            List<WooProductDto> results)
        {
            if (variableParents == null || variableParents.Count == 0)
                return;

            const int maxDegree = 6;
            var bag = new System.Collections.Concurrent.ConcurrentBag<VariablePullResult>();
            var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            System.Threading.Tasks.Parallel.ForEach(
                variableParents,
                new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = maxDegree },
                parent =>
                {
                    try
                    {
                        var variations = GetVariations(creds, parent.Id);
                        bag.Add(new VariablePullResult
                        {
                            Parent = parent,
                            Keep = variations.Where(IsCatalogEligible).ToList(),
                            Total = variations.Count
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                });

            if (!errors.IsEmpty)
                throw errors.First();

            foreach (var entry in bag.OrderBy(e => e.Parent.Id))
            {
                if (entry.Keep.Count == 0)
                    continue;

                var product = entry.Parent;
                product.IsParentGroup = true;
                product.VariationTotalCount = entry.Total;
                product.VariationInStockCount = entry.Keep.Count;
                results.Add(product);
                foreach (var v in entry.Keep)
                {
                    v.CategoryIds = product.CategoryIds;
                    v.CategoriesLabel = product.CategoriesLabel;
                    v.ParentId = product.Id;
                    v.ParentSku = product.Sku;
                    v.ParentName = product.Name;
                    if (string.IsNullOrWhiteSpace(v.Name))
                        v.Name = product.Name;
                    results.Add(v);
                }
            }
        }

        private sealed class VariablePullResult
        {
            public WooProductDto Parent { get; set; }
            public List<WooProductDto> Keep { get; set; }
            public int Total { get; set; }
        }

        /// <summary>
        /// Publish and not out of stock. Out of stock is treated like disabled for mapping.
        /// (Woo often returns manage_stock=parent on variations; stock_status still applies.)
        /// </summary>
        private static bool IsCatalogEligible(WooProductDto p)
        {
            if (p == null || !WooProductMapRow.IsWooEnabledStatus(p.Status))
                return false;
            string stock = (p.StockStatus ?? string.Empty).Trim();
            if (string.Equals(stock, "outofstock", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
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

        public bool UpdateCatalogSku(ApiCredentials creds, long productId, long? variationId, string sku, out string detail)
        {
            detail = null;
            if (string.IsNullOrWhiteSpace(sku))
            {
                detail = "SKU is required.";
                return false;
            }
            string path = variationId.HasValue && variationId.Value > 0
                ? "/wp-json/wc/v3/products/" + productId + "/variations/" + variationId.Value
                : "/wp-json/wc/v3/products/" + productId;
            string escaped = sku.Trim().Replace("\\", "\\\\").Replace("\"", "\\\"");
            string body = "{\"sku\":\"" + escaped + "\"}";
            try
            {
                JToken json = SendJson(creds, HttpMethod.Put, path, body);
                detail = sku.Trim();
                return json != null;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                AppLogger.WriteLog("woo", "UpdateCatalogSku failed: " + ex.Message);
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
                        Status = t.Value<string>("status") ?? "publish",
                        Type = "variation",
                        ParentId = productId,
                        StockStatus = t.Value<string>("stock_status"),
                        ManageStock = ReadFlexibleBool(t["manage_stock"]),
                        Attributes = ParseAttributes(t["attributes"] as JArray)
                    });
                    EnrichVariationAttributesFromName(list[list.Count - 1]);
                }
                if (arr.Count < 100)
                    break;
                page++;
            }
            return list;
        }

        /// <summary>
        /// Woo variation names are often "Product - 1kg Packet, Whole beans".
        /// When the attributes array only has one entry, recover the rest from the name.
        /// </summary>
        private static void EnrichVariationAttributesFromName(WooProductDto v)
        {
            if (v == null)
                return;
            if (v.Attributes == null)
                v.Attributes = new List<WooAttributeValue>();
            if (v.Attributes.Count >= 2)
                return;
            string name = v.Name ?? string.Empty;
            int dash = name.LastIndexOf(" - ", StringComparison.Ordinal);
            if (dash < 0)
                return;
            string tail = name.Substring(dash + 3).Trim();
            if (tail.Length == 0)
                return;

            var existing = new HashSet<string>(
                v.Attributes.Select(a => (a.Option ?? string.Empty).Trim())
                    .Where(o => o.Length > 0),
                StringComparer.OrdinalIgnoreCase);
            foreach (string bit in tail.Split(new[] { ',', '·', '|', '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string opt = bit.Trim();
                if (opt.Length == 0 || existing.Contains(opt))
                    continue;
                string attrName = PackagingWeightParser.TryParseQtyFactorFromOption(opt).HasValue
                    ? "Packaging"
                    : "Prep Type";
                v.Attributes.Add(new WooAttributeValue { Name = attrName, Option = opt });
                existing.Add(opt);
            }
        }

        private static WooProductDto ParseProduct(JToken t)
        {
            var dto = new WooProductDto
            {
                Id = t.Value<long?>("id") ?? 0,
                Name = t.Value<string>("name"),
                Sku = t.Value<string>("sku"),
                Status = t.Value<string>("status"),
                Type = t.Value<string>("type"),
                StockStatus = t.Value<string>("stock_status"),
                ManageStock = ReadFlexibleBool(t["manage_stock"])
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
            dto.Attributes = ParseAttributes(t["attributes"] as JArray);
            return dto;
        }

        /// <summary>
        /// Woo returns manage_stock as bool, or the string "parent" on variations.
        /// Newtonsoft Value&lt;bool?&gt; throws FormatException on "parent"/empty.
        /// </summary>
        private static bool ReadFlexibleBool(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
                return false;
            if (token.Type == JTokenType.Boolean)
                return token.Value<bool>();
            if (token.Type == JTokenType.Integer)
                return token.Value<long>() != 0;

            string s = (token.Type == JTokenType.String ? token.Value<string>() : token.ToString()) ?? string.Empty;
            s = s.Trim();
            if (s.Length == 0)
                return false;
            if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)
                || s == "1"
                || string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase))
                return true;
            // "parent", "false", "0", "no", etc. → not actively managing stock on this row
            return false;
        }

        private static List<WooAttributeValue> ParseAttributes(JArray attrs)
        {
            var list = new List<WooAttributeValue>();
            if (attrs == null)
                return list;

            foreach (JToken a in attrs)
            {
                string name = a.Value<string>("name");
                if (string.IsNullOrWhiteSpace(name))
                    name = a.Value<string>("slug");
                if (!string.IsNullOrWhiteSpace(name) && name.StartsWith("pa_", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(3).Replace('-', ' ');
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                // Variation attributes use "option"; parent product attributes use "options[]".
                string option = a.Value<string>("option");
                if (string.IsNullOrWhiteSpace(option))
                {
                    var optionToken = a["option"];
                    if (optionToken != null && optionToken.Type != JTokenType.Null)
                        option = optionToken.ToString();
                }
                if (!string.IsNullOrWhiteSpace(option))
                {
                    list.Add(new WooAttributeValue { Name = name.Trim(), Option = option.Trim() });
                    continue;
                }

                var options = a["options"] as JArray;
                if (options != null && options.Count > 0)
                {
                    foreach (JToken opt in options)
                    {
                        string optText = opt.Type == JTokenType.String
                            ? opt.Value<string>()
                            : opt.ToString();
                        if (string.IsNullOrWhiteSpace(optText))
                            continue;
                        list.Add(new WooAttributeValue { Name = name.Trim(), Option = optText.Trim() });
                    }
                    continue;
                }

                // Variation "Any Prep Type…" / empty option — keep the attribute so mapping can show it.
                list.Add(new WooAttributeValue { Name = name.Trim(), Option = "(any)" });
            }
            return list;
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

            // Sticky auth: try remembered mode first; default query then basic (many hosts strip Authorization).
            string preferred = GetPreferredAuthMode(baseUrl);
            string[] modes;
            if (string.Equals(preferred, "basic", StringComparison.OrdinalIgnoreCase))
                modes = new[] { "basic", "query" };
            else if (string.Equals(preferred, "query", StringComparison.OrdinalIgnoreCase))
                modes = new[] { "query", "basic" };
            else
                modes = new[] { "query", "basic" };

            HttpClient client = SharedClient;
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

                    using (var request = new HttpRequestMessage(method, url))
                    {
                        if (mode == "basic")
                        {
                            var authBytes = Encoding.ASCII.GetBytes(creds.ConsumerKey + ":" + creds.ConsumerSecret);
                            request.Headers.Authorization =
                                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                        }
                        if (jsonBody != null)
                            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                        using (HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult())
                        {
                            string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            if (!response.IsSuccessStatusCode)
                                throw new InvalidOperationException(
                                    "HTTP " + (int)response.StatusCode + " — " + FormatErrorBody(body));

                            IEnumerable<string> totalVals;
                            if (response.Headers.TryGetValues("X-WP-Total", out totalVals))
                            {
                                int parsed;
                                if (int.TryParse(totalVals.FirstOrDefault(), out parsed))
                                    wpTotal = parsed;
                            }

                            RememberAuthMode(baseUrl, mode);

                            if (string.IsNullOrWhiteSpace(body))
                                return new JObject();
                            return JToken.Parse(body);
                        }
                    }
                }
                catch (Exception ex)
                {
                    last = ex;
                }
            }
            throw last ?? new InvalidOperationException("Woo API request failed.");
        }

        private static string FormatErrorBody(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.Replace("\r", " ").Replace("\n", " ").Trim();
            if (s.StartsWith("{", StringComparison.Ordinal))
            {
                try
                {
                    var token = JToken.Parse(s);
                    var msg = token["message"];
                    if (msg != null && !string.IsNullOrWhiteSpace(msg.ToString()))
                    {
                        string text = msg.ToString().Trim();
                        var code = token["code"];
                        if (code != null && !string.IsNullOrWhiteSpace(code.ToString()))
                            text = code.ToString().Trim() + ": " + text;
                        return Truncate(text);
                    }
                }
                catch
                {
                    // fall through to truncate raw body
                }
            }
            return Truncate(s);
        }

        private static string Truncate(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= 160 ? s : s.Substring(0, 160) + "…";
        }
    }
}
