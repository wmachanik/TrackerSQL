using System;
using System.Text;
using System.Web;
using System.Web.Security;

namespace TrackerDotNet.Classes
{
    public static class OrderViewTokenHelper
    {
        private const string Purpose = "OrderViewLink_v1";
        private const int DefaultDaysValid = 7;

        public static string CreateToken(int orderId, int customerId, TimeSpan? lifetime = null)
        {
            var expiry = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromDays(DefaultDaysValid)).Ticks;
            var nonce = Guid.NewGuid().ToString("N"); // mitigates replay pattern analysis
            string payload = $"{orderId}|{customerId}|{expiry}|{nonce}";
            byte[] protectedBytes = MachineKey.Protect(Encoding.UTF8.GetBytes(payload), Purpose);
            return HttpServerUtility.UrlTokenEncode(protectedBytes);
        }

        public static bool TryValidate(string token, out int orderId, out int customerId)
        {
            orderId = 0;
            customerId = 0;
            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                byte[] data = HttpServerUtility.UrlTokenDecode(token);
                if (data == null) return false;

                byte[] unprotected = MachineKey.Unprotect(data, Purpose);
                if (unprotected == null) return false;

                string payload = Encoding.UTF8.GetString(unprotected);
                // orderId|custId|expiry|nonce
                var parts = payload.Split('|');
                if (parts.Length != 4) return false;

                if (!int.TryParse(parts[0], out orderId)) return false;
                if (!int.TryParse(parts[1], out customerId)) return false;
                if (!long.TryParse(parts[2], out long ticks)) return false;

                if (new DateTime(ticks, DateTimeKind.Utc) < DateTime.UtcNow) return false;

                // Optionally: add a replay / revocation check (DB or cache) here if you later store nonce
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string CreateCustomerDeliveryToken(long customerId, DateTime deliveryDateUtcDateOnly, TimeSpan? lifetime = null)
        {
            var expiry = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromDays(DefaultDaysValid)).Ticks;
            // Normalize date to date-only UTC ticks to avoid mismatch
            var deliveryDate = deliveryDateUtcDateOnly.Date.ToUniversalTime();
            string payload = $"CD|{customerId}|{deliveryDate:yyyyMMdd}|{expiry}|{Guid.NewGuid():N}";
            byte[] protectedBytes = MachineKey.Protect(Encoding.UTF8.GetBytes(payload), Purpose);
            return HttpServerUtility.UrlTokenEncode(protectedBytes);
        }
        public static bool TryValidateCustomerDeliveryToken(
                    string token,
                    out long customerId,
                    out DateTime deliveryDateUtcDateOnly)
        {
            customerId = 0;
            deliveryDateUtcDateOnly = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                byte[] raw = HttpServerUtility.UrlTokenDecode(token);
                if (raw == null) return false;

                byte[] unprotected = MachineKey.Unprotect(raw, "OrderViewLink_v1");
                if (unprotected == null) return false;

                string payload = Encoding.UTF8.GetString(unprotected);
                // Expected: CD|{customerId}|{yyyyMMdd}|{expiryTicks}|{nonce}
                var parts = payload.Split('|');
                if (parts.Length != 5) return false;
                if (parts[0] != "CD") return false;
                if (!long.TryParse(parts[1], out customerId)) return false;
                if (parts[2].Length != 8) return false;
                if (!long.TryParse(parts[3], out long expiryTicks)) return false;

                var expiryUtc = new DateTime(expiryTicks, DateTimeKind.Utc);
                if (DateTime.UtcNow > expiryUtc) return false;

                // Parse date (yyyyMMdd)
                string ds = parts[2];
                int y = int.Parse(ds.Substring(0, 4));
                int m = int.Parse(ds.Substring(4, 2));
                int d = int.Parse(ds.Substring(6, 2));
                deliveryDateUtcDateOnly = new DateTime(y, m, d, 0, 0, 0, DateTimeKind.Utc);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}