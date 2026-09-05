using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrackerSQL.Models;

namespace TrackerSQL.Managers
{
    public partial class WooCommerceApiClient
    {
        public WooOrderDto GetOrder(ApiCredentials creds, long orderId)
        {
            if (orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId));
            string path = "/wp-json/wc/v3/orders/" + orderId.ToString(CultureInfo.InvariantCulture);
            JToken json = GetJson(creds, path);
            return ParseOrder(json);
        }

        /// <summary>
        /// Adds a Woo order note. When customerNote is true, Woo emails it to the customer.
        /// Does not change order status (Pargo/courier stay open until received).
        /// </summary>
        public bool AddOrderNote(ApiCredentials creds, long orderId, string note, bool customerNote, out string detail)
        {
            detail = null;
            if (orderId <= 0)
            {
                detail = "Invalid Woo order id.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(note))
            {
                detail = "Note is empty.";
                return false;
            }

            string path = "/wp-json/wc/v3/orders/" + orderId.ToString(CultureInfo.InvariantCulture) + "/notes";
            string body = new JObject
            {
                ["note"] = note.Trim(),
                ["customer_note"] = customerNote
            }.ToString(Formatting.None);

            try
            {
                SendJson(creds, HttpMethod.Post, path, body);
                detail = null;
                return true;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return false;
            }
        }

        public List<WooOrderDto> GetLatestOrders(ApiCredentials creds, int count = 1)
        {
            int take = Math.Max(1, Math.Min(count, 100));
            string path = "/wp-json/wc/v3/orders?per_page=" + take.ToString(CultureInfo.InvariantCulture)
                + "&orderby=date&order=desc";
            return ParseOrderList(GetJson(creds, path));
        }

        public List<WooOrderDto> GetOrdersSince(ApiCredentials creds, DateTime sinceUtc, int maxPages = 10)
        {
            return GetOrdersFiltered(creds, sinceUtc, null, maxPages);
        }

        public List<WooOrderDto> GetOrdersInRange(ApiCredentials creds, DateTime fromUtc, DateTime toUtc, int maxPages = 20)
        {
            if (toUtc < fromUtc)
            {
                DateTime swap = fromUtc;
                fromUtc = toUtc;
                toUtc = swap;
            }
            return GetOrdersFiltered(creds, fromUtc, toUtc, maxPages);
        }

        private List<WooOrderDto> GetOrdersFiltered(ApiCredentials creds, DateTime? afterUtc, DateTime? beforeUtc, int maxPages)
        {
            var results = new List<WooOrderDto>();
            int page = 1;
            while (page <= maxPages)
            {
                string path = "/wp-json/wc/v3/orders?per_page=100&page=" + page.ToString(CultureInfo.InvariantCulture)
                    + "&orderby=date&order=asc";
                if (afterUtc.HasValue)
                    path += "&after=" + Uri.EscapeDataString(ToWooIso(afterUtc.Value));
                if (beforeUtc.HasValue)
                    path += "&before=" + Uri.EscapeDataString(ToWooIso(beforeUtc.Value));

                JToken json = GetJson(creds, path);
                var batch = ParseOrderList(json);
                if (batch.Count == 0)
                    break;
                results.AddRange(batch);
                if (batch.Count < 100)
                    break;
                page++;
            }
            return results;
        }

        private static string ToWooIso(DateTime utc)
        {
            if (utc.Kind == DateTimeKind.Unspecified)
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            return utc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static List<WooOrderDto> ParseOrderList(JToken json)
        {
            var list = new List<WooOrderDto>();
            var arr = json as JArray;
            if (arr == null)
                return list;
            foreach (JToken t in arr)
                list.Add(ParseOrder(t));
            return list;
        }

        private static WooOrderDto ParseOrder(JToken t)
        {
            if (t == null)
                return null;

            var billing = ParseAddress(t["billing"]);
            var shipping = ParseAddress(t["shipping"]);
            if (string.IsNullOrWhiteSpace(shipping.Email))
                shipping.Email = billing.Email;
            if (string.IsNullOrWhiteSpace(shipping.Phone))
                shipping.Phone = billing.Phone;

            var order = new WooOrderDto
            {
                Id = t.Value<long?>("id") ?? 0,
                Number = t.Value<string>("number") ?? t.Value<long?>("id")?.ToString(CultureInfo.InvariantCulture),
                Status = t.Value<string>("status"),
                DateCreated = ParseWooDate(t.Value<string>("date_created")),
                DateModified = ParseWooDate(t.Value<string>("date_modified")),
                DatePaid = ParseWooDate(t.Value<string>("date_paid")),
                PaymentMethod = t.Value<string>("payment_method"),
                PaymentMethodTitle = t.Value<string>("payment_method_title"),
                TransactionId = t.Value<string>("transaction_id"),
                CustomerId = t.Value<long?>("customer_id") ?? 0,
                CustomerNote = t.Value<string>("customer_note"),
                Billing = billing,
                Shipping = shipping,
                RawJson = t.ToString(Newtonsoft.Json.Formatting.None)
            };

            var lines = t["line_items"] as JArray;
            if (lines != null)
            {
                foreach (JToken li in lines)
                {
                    order.LineItems.Add(new WooOrderLineDto
                    {
                        Id = li.Value<long?>("id") ?? 0,
                        Name = li.Value<string>("name"),
                        Sku = li.Value<string>("sku"),
                        ProductId = li.Value<long?>("product_id") ?? 0,
                        VariationId = li.Value<long?>("variation_id") ?? 0,
                        Quantity = li.Value<double?>("quantity") ?? 0,
                        MetaData = ParseMeta(li["meta_data"])
                    });
                }
            }

            var ships = t["shipping_lines"] as JArray;
            if (ships != null)
            {
                foreach (JToken sl in ships)
                {
                    order.ShippingLines.Add(new WooShippingLineDto
                    {
                        MethodTitle = sl.Value<string>("method_title"),
                        MethodId = sl.Value<string>("method_id")
                    });
                }
            }

            order.MetaData = ParseMeta(t["meta_data"]);

            return order;
        }

        private static WooAddressDto ParseAddress(JToken a)
        {
            if (a == null)
                return new WooAddressDto();
            return new WooAddressDto
            {
                FirstName = a.Value<string>("first_name"),
                LastName = a.Value<string>("last_name"),
                Company = a.Value<string>("company"),
                Address1 = a.Value<string>("address_1"),
                Address2 = a.Value<string>("address_2"),
                City = a.Value<string>("city"),
                State = a.Value<string>("state"),
                Postcode = a.Value<string>("postcode"),
                Country = a.Value<string>("country"),
                Email = a.Value<string>("email"),
                Phone = a.Value<string>("phone")
            };
        }

        private static List<WooMetaDto> ParseMeta(JToken meta)
        {
            var list = new List<WooMetaDto>();
            var arr = meta as JArray;
            if (arr == null)
                return list;
            foreach (JToken m in arr)
            {
                JToken valueToken = m["value"];
                JToken displayToken = m["display_value"];
                list.Add(new WooMetaDto
                {
                    Key = ReadScalarString(m["key"]),
                    DisplayKey = ReadScalarString(m["display_key"]),
                    Value = FormatMetaValue(valueToken),
                    DisplayValue = ReadScalarString(displayToken) ?? FormatMetaValue(valueToken)
                });
            }
            return list;
        }

        /// <summary>Woo line meta values are often objects/arrays — never use JToken.Value&lt;string&gt;(key) on those.</summary>
        private static string ReadScalarString(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;
            if (token.Type == JTokenType.String)
                return token.Value<string>();
            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float
                || token.Type == JTokenType.Boolean || token.Type == JTokenType.Date)
                return token.ToString();
            return null;
        }

        private static string FormatMetaValue(JToken token)
        {
            string scalar = ReadScalarString(token);
            if (scalar != null)
                return scalar;
            if (token == null || token.Type == JTokenType.Null)
                return null;
            return token.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static DateTime? ParseWooDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime dt))
                return dt;
            return null;
        }
    }
}
