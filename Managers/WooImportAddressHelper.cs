using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TrackerSQL.Models;

namespace TrackerSQL.Managers
{
    /// <summary>Formats Woo shipping address/phone for Tracker contacts (order import).</summary>
    public static class WooImportAddressHelper
    {
        private static readonly HashSet<string> CountryTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ZA", "ZAF", "RSA", "SOUTH AFRICA"
        };

        private static readonly HashSet<string> ProvinceTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "EC", "FS", "GP", "KZN", "LP", "MP", "NC", "NW", "WC",
            "EASTERN CAPE", "FREE STATE", "GAUTENG", "KWAZULU-NATAL", "KWAZULU NATAL",
            "LIMPOPO", "MPUMALANGA", "NORTHERN CAPE", "NORTH WEST", "WESTERN CAPE"
        };

        public static WooImportAddressConfig FromSettings(WooCommerceSettings settings)
        {
            if (settings == null)
                return new WooImportAddressConfig();

            return new WooImportAddressConfig
            {
                IncludeProvince = settings.ImportAddressIncludeProvince,
                IncludeCountry = settings.ImportAddressIncludeCountry,
                ReplacePlus27WithZero = settings.ImportPhoneReplacePlus27,
                FormatSaPhone = settings.ImportPhoneFormatSa
            };
        }

        public static string FormatBillingAddress(WooAddressDto ship, WooImportAddressConfig config, string areaName = null)
        {
            if (ship == null)
                return string.Empty;

            config = config ?? new WooImportAddressConfig();
            var parts = new List<string>();
            AddPart(parts, ship.Address1);
            AddPart(parts, ship.Address2);
            if (!CityRedundantWithArea(ship.City, areaName))
                AddPart(parts, ship.City);
            if (config.IncludeProvince)
                AddPart(parts, ship.State);
            if (config.IncludeCountry)
                AddPart(parts, ship.Country);
            return string.Join("; ", parts);
        }

        public static string NormalizeStoredBillingAddress(string billingAddress, WooImportAddressConfig config, string areaName = null)
        {
            if (string.IsNullOrWhiteSpace(billingAddress))
                return string.Empty;

            config = config ?? new WooImportAddressConfig();
            string[] rawParts = billingAddress.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var parts = rawParts.Select(p => p.Trim()).Where(p => p.Length > 0).ToList();

            if (!config.IncludeCountry)
                parts.RemoveAll(IsCountryToken);
            if (!config.IncludeProvince)
                parts.RemoveAll(IsProvinceToken);
            if (!string.IsNullOrWhiteSpace(areaName))
                parts.RemoveAll(p => CityRedundantWithArea(p, areaName));

            return string.Join("; ", parts);
        }

        public static bool BillingAddressesMatch(string storedBilling, WooAddressDto ship, WooImportAddressConfig config, string areaName = null)
        {
            return BillingAddressesMatch(storedBilling, ship, config, areaName, null);
        }

        /// <param name="wooAreaName">Area name from Woo postcode resolve (preferred for Woo formatting).</param>
        public static bool BillingAddressesMatch(
            string storedBilling,
            WooAddressDto ship,
            WooImportAddressConfig config,
            string contactAreaName,
            string wooAreaName)
        {
            config = config ?? new WooImportAddressConfig();
            string wooArea = !string.IsNullOrWhiteSpace(wooAreaName) ? wooAreaName : contactAreaName;
            string woo = NormalizeCompare(FormatBillingAddress(ship, config, wooArea));

            // Stored may have been formatted with contact area or Woo area — accept either.
            string storedContact = NormalizeCompare(NormalizeStoredBillingAddress(storedBilling, config, contactAreaName));
            if (string.Equals(storedContact, woo, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!string.IsNullOrWhiteSpace(wooArea)
                && !string.Equals(wooArea, contactAreaName ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                string storedWoo = NormalizeCompare(NormalizeStoredBillingAddress(storedBilling, config, wooArea));
                if (string.Equals(storedWoo, woo, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Last resort: ignore area-based city stripping on stored side.
            string storedRaw = NormalizeCompare(NormalizeStoredBillingAddress(storedBilling, config, null));
            string wooRaw = NormalizeCompare(FormatBillingAddress(ship, config, null));
            return string.Equals(storedRaw, wooRaw, StringComparison.OrdinalIgnoreCase);
        }

        public static string FormatPhone(string phone, WooImportAddressConfig config)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            config = config ?? new WooImportAddressConfig();
            string value = phone.Trim();

            if (config.ReplacePlus27WithZero)
                value = ReplacePlus27WithZero(value);

            if (config.FormatSaPhone)
                value = FormatSaPhoneNumber(value);

            return value;
        }

        public static bool PhonesMatch(string storedPhone, string wooPhone, WooImportAddressConfig config)
        {
            string stored = NormalizeCompare(FormatPhone(storedPhone, config));
            string woo = NormalizeCompare(FormatPhone(wooPhone, config));
            if (string.IsNullOrEmpty(stored) && string.IsNullOrEmpty(woo))
                return true;
            return string.Equals(stored, woo, StringComparison.OrdinalIgnoreCase);
        }

        public static string ReplacePlus27WithZero(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone ?? string.Empty;

            string trimmed = phone.Trim();
            if (trimmed.StartsWith("+27", StringComparison.OrdinalIgnoreCase))
                return "0" + trimmed.Substring(3).TrimStart();

            string digits = Regex.Replace(trimmed, @"[^\d]", string.Empty);
            if (digits.StartsWith("27", StringComparison.Ordinal) && digits.Length >= 11)
                return "0" + digits.Substring(2);

            return trimmed;
        }

        /// <summary>SA national format: aaa bbb-cccc (e.g. 021 555-1234, 083 687-3887).</summary>
        public static string FormatSaPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone ?? string.Empty;

            string digits = Regex.Replace(phone.Trim(), @"[^\d]", string.Empty);
            if (digits.StartsWith("27", StringComparison.Ordinal) && digits.Length >= 11)
                digits = "0" + digits.Substring(2);
            if (digits.Length == 9)
                digits = "0" + digits;

            if (digits.Length != 10 || digits[0] != '0')
                return phone.Trim();

            return digits.Substring(0, 3) + " " + digits.Substring(3, 3) + "-" + digits.Substring(6, 4);
        }

        private static bool CityRedundantWithArea(string cityOrPart, string areaName)
        {
            if (string.IsNullOrWhiteSpace(cityOrPart) || string.IsNullOrWhiteSpace(areaName))
                return false;

            string city = cityOrPart.Trim();
            string area = areaName.Trim();
            return string.Equals(city, area, StringComparison.OrdinalIgnoreCase)
                || area.IndexOf(city, StringComparison.OrdinalIgnoreCase) >= 0
                || city.IndexOf(area, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void AddPart(List<string> parts, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                parts.Add(value.Trim());
        }

        private static bool IsCountryToken(string part)
        {
            return !string.IsNullOrWhiteSpace(part) && CountryTokens.Contains(part.Trim());
        }

        private static bool IsProvinceToken(string part)
        {
            if (string.IsNullOrWhiteSpace(part))
                return false;

            string trimmed = part.Trim();
            if (ProvinceTokens.Contains(trimmed))
                return true;

            int comma = trimmed.IndexOf(',');
            if (comma >= 0)
            {
                string tail = trimmed.Substring(comma + 1).Trim();
                if (ProvinceTokens.Contains(tail))
                    return true;
            }

            return false;
        }

        private static string NormalizeCompare(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return Regex.Replace(value.Trim(), @"\s+", " ");
        }
    }

    public class WooImportAddressConfig
    {
        public bool IncludeProvince { get; set; }
        public bool IncludeCountry { get; set; }
        public bool ReplacePlus27WithZero { get; set; } = true;
        public bool FormatSaPhone { get; set; } = true;
    }
}
