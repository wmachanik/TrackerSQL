using System;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TrackerDotNet.Classes
{
    public static class TimeZoneUtils
    {
        private static readonly string DefaultTimeZoneId = ConfigHelper.GetString("AppTimeZoneId", "South Africa Standard Time");
        // --- Test date override (cached) ---
        private static readonly bool TestNowEnabled = ConfigHelper.GetBool("TestNow.Enabled", false);
        private static readonly string TestNowRaw = ConfigHelper.GetString("TestNow.Value", "").Trim();
        private static readonly string TestNowInputKind = ConfigHelper.GetString("TestNow.InputKind", "Local").Trim();
        private static readonly Lazy<DateTime?> ParsedTestNow = new Lazy<DateTime?>(ParseTestNow, isThreadSafe: true);

        private static TimeZoneInfo EffectiveTimeZone
        {
            get
            {
                if (HttpContext.Current != null &&
                    HttpContext.Current.Session != null &&
                    HttpContext.Current.Session["UserTimeZoneInfo"] is TimeZoneInfo userZone)
                {
                    return userZone;
                }

                return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
            }
        }
        public static string GetZoneAbbreviation()
        {
            switch (EffectiveTimeZone.Id)
            {
                case "South Africa Standard Time": return "SAST";
                case "GMT Standard Time": return "GMT";
                case "Greenwich Standard Time": return "GMT";
                case "W. Europe Standard Time": return "CET";
                case "Central Europe Standard Time": return "CET";
                case "Romance Standard Time": return "CET";
                case "Central European Standard Time": return "CEST";
                case "Eastern Standard Time": return "EST";
                case "Eastern Daylight Time": return "EDT";
                case "Pacific Standard Time": return "PST";
                case "Pacific Daylight Time": return "PDT";
                case "Mountain Standard Time": return "MST";
                case "Mountain Daylight Time": return "MDT";
                case "Central Standard Time": return "CST";
                case "Central Daylight Time": return "CDT";
                case "India Standard Time": return "IST";
                case "China Standard Time": return "CST";
                case "Tokyo Standard Time": return "JST";
                case "Russian Standard Time": return "MSK";
                case "Arabian Standard Time": return "AST";
                case "AUS Eastern Standard Time": return "AEST";
                case "AUS Central Standard Time": return "ACST";
                case "New Zealand Standard Time": return "NZST";
                case "UTC": return "UTC";
                case "UTC+12": return "UTC+12";
                case "UTC+10": return "UTC+10";
                case "UTC+08": return "UTC+8";
                case "UTC+03": return "UTC+3";
                case "UTC-05": return "UTC-5";
                case "UTC-08": return "UTC-8";
                // Add more as needed
                default:
                    // Fallback: use first letters of each word in the ID
                    return string.Concat(EffectiveTimeZone.Id.Split(' ').Select(w => w[0])).ToUpper();
            }
        }
        public static DateTime Now()
        {
            // If test override enabled and parsed successfully, return it.
            if (TestNowEnabled && ParsedTestNow.Value.HasValue)
                return ParsedTestNow.Value.Value;

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EffectiveTimeZone);
        }

        public static DateTime ConvertUtcToUserZone(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, EffectiveTimeZone);
        }

        public static DateTime ConvertToUtc(DateTime userLocalTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(userLocalTime, EffectiveTimeZone);
        }

        public static string GetZoneId()
        {
            return EffectiveTimeZone.Id;
        }
        // INTERNAL: parsing logic for test date
        private static DateTime? ParseTestNow()
        {
            if (!TestNowEnabled) return null;
            if (string.IsNullOrWhiteSpace(TestNowRaw)) return null;

            var raw = TestNowRaw.Trim();

            // Accept simple date or datetime (ISO-ish)
            // Try exact patterns first
            DateTime dt;
            string[] formats = {
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssK"
            };

            if (DateTime.TryParseExact(raw, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AdjustToUniversal |
                    System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                    out dt))
            {
                // If kind is unspecified and InputKind=UTC treat as UTC
                if (dt.Kind == DateTimeKind.Unspecified && TestNowInputKind.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                {
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
            else if (DateTime.TryParse(raw,
                     System.Globalization.CultureInfo.InvariantCulture,
                     System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                     out dt))
            {
                // fallback parse
            }
            else
            {
                // Could not parse -> ignore override
                return null;
            }

            // Normalize:
            // If value is UTC or specified as UTC -> convert into EffectiveTimeZone
            if (dt.Kind == DateTimeKind.Utc || TestNowInputKind.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            {
                return TimeZoneInfo.ConvertTimeFromUtc(
                    dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc),
                    EffectiveTimeZone);
            }

            // Treat as local in EffectiveTimeZone (if unspecified or Local)
            if (dt.Kind == DateTimeKind.Local || dt.Kind == DateTimeKind.Unspecified)
            {
                // dt represents local wall clock in EffectiveTimeZone already
                return dt;
            }

            // Default fallback
            return dt;
        }
    }
}
