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
        private static readonly TextInfo TitleCaser = CultureInfo.CurrentCulture.TextInfo;

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

        private static readonly HashSet<string> CapeTownTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CAPE TOWN", "CAPETOWN", "CPT"
        };

        public static WooImportAddressConfig FromSettings(WooCommerceSettings settings)
        {
            if (settings == null)
                return new WooImportAddressConfig();

            return new WooImportAddressConfig
            {
                IncludeProvince = settings.ImportAddressIncludeProvince,
                IncludeCountry = settings.ImportAddressIncludeCountry,
                DeduplicateSuburb = settings.ImportAddressDeduplicateSuburb,
                StripCapeTown = settings.ImportAddressStripCapeTown,
                TitleCase = settings.ImportAddressTitleCase,
                ReplacePlus27WithZero = settings.ImportPhoneReplacePlus27,
                FormatSaPhone = settings.ImportPhoneFormatSa
            };
        }

        public static string FormatBillingAddress(WooAddressDto ship, WooImportAddressConfig config, string areaName = null, string careOfCompany = null)
        {
            if (ship == null)
                return string.Empty;

            config = config ?? new WooImportAddressConfig();
            var raw = new List<string>();
            if (!string.IsNullOrWhiteSpace(careOfCompany))
                raw.Add("c/o " + careOfCompany.Trim());
            AddExpandedParts(raw, ship.Address1);
            AddExpandedParts(raw, ship.Address2);
            if (!CityRedundantWithArea(ship.City, areaName))
                AddExpandedParts(raw, ship.City);
            if (config.IncludeProvince)
                AddExpandedParts(raw, ship.State);
            if (config.IncludeCountry)
                AddExpandedParts(raw, ship.Country);

            return FinalizeAddressParts(raw, config, areaName);
        }

        public static string NormalizeStoredBillingAddress(string billingAddress, WooImportAddressConfig config, string areaName = null)
        {
            if (string.IsNullOrWhiteSpace(billingAddress))
                return string.Empty;

            config = config ?? new WooImportAddressConfig();
            var raw = ExpandAddressSegments(billingAddress);

            if (!config.IncludeCountry)
                raw.RemoveAll(IsCountryToken);
            if (!config.IncludeProvince)
                raw.RemoveAll(IsProvinceToken);
            if (!string.IsNullOrWhiteSpace(areaName))
                raw.RemoveAll(p => CityRedundantWithArea(p, areaName));

            return FinalizeAddressParts(raw, config, areaName);
        }

        public static bool BillingAddressesMatch(string storedBilling, WooAddressDto ship, WooImportAddressConfig config, string areaName = null)
        {
            return BillingAddressesMatch(storedBilling, ship, config, areaName, null, null);
        }

        /// <param name="wooAreaName">Area name from Woo postcode resolve (preferred for Woo formatting).</param>
        /// <param name="careOfCompany">When set, Woo side is formatted with a leading c/o company segment.</param>
        public static bool BillingAddressesMatch(
            string storedBilling,
            WooAddressDto ship,
            WooImportAddressConfig config,
            string contactAreaName,
            string wooAreaName,
            string careOfCompany = null)
        {
            config = config ?? new WooImportAddressConfig();
            string wooArea = !string.IsNullOrWhiteSpace(wooAreaName) ? wooAreaName : contactAreaName;
            string woo = NormalizeCompare(FormatBillingAddress(ship, config, wooArea, careOfCompany));

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
            string wooRaw = NormalizeCompare(FormatBillingAddress(ship, config, null, careOfCompany));
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

        public static bool IsCapeTownDeliveryArea(string areaName)
        {
            if (string.IsNullOrWhiteSpace(areaName))
                return false;

            string name = areaName.Trim();
            return name.IndexOf("Cape Town", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("CapeTown", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FinalizeAddressParts(List<string> raw, WooImportAddressConfig config, string areaName)
        {
            var parts = raw
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();

            if (config.StripCapeTown && IsCapeTownDeliveryArea(areaName))
                parts.RemoveAll(IsCapeTownToken);

            if (config.DeduplicateSuburb)
                parts = DeduplicateParts(parts);

            if (!string.IsNullOrWhiteSpace(areaName))
                parts.RemoveAll(p => CityRedundantWithArea(p, areaName) && !LooksLikeStreetLine(p));

            if (config.TitleCase)
                parts = parts.Select(ApplyTitleCasePreserveCareOf).ToList();

            return string.Join("; ", parts);
        }

        private static List<string> DeduplicateParts(List<string> parts)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string part in parts)
            {
                string key = NormalizeCompare(part);
                if (string.IsNullOrEmpty(key) || seen.Contains(key))
                    continue;
                seen.Add(key);
                result.Add(part);
            }
            return result;
        }

        private static void AddExpandedParts(List<string> parts, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            parts.AddRange(ExpandAddressSegments(value));
        }

        private static List<string> ExpandAddressSegments(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();
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

        /// <summary>Keep street-like lines even when area name overlaps a suburb word.</summary>
        private static bool LooksLikeStreetLine(string part)
        {
            if (string.IsNullOrWhiteSpace(part))
                return false;
            // Digits usually mean street number / unit.
            return part.Any(char.IsDigit);
        }

        private static bool IsCapeTownToken(string part)
        {
            if (string.IsNullOrWhiteSpace(part))
                return false;
            return CapeTownTokens.Contains(part.Trim());
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

        private static string ApplyTitleCasePreserveCareOf(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value ?? string.Empty;

            string trimmed = value.Trim();
            if (trimmed.StartsWith("c/o ", StringComparison.OrdinalIgnoreCase))
            {
                string rest = trimmed.Substring(4).Trim();
                return "c/o " + TitleCaseWords(rest);
            }

            return TitleCaseWords(trimmed);
        }

        private static string TitleCaseWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value ?? string.Empty;
            return TitleCaser.ToTitleCase(value.Trim().ToLower(CultureInfo.CurrentCulture));
        }

        public static bool NormEquals(string a, string b)
        {
            return string.Equals(NormalizeCompare(a), NormalizeCompare(b), StringComparison.OrdinalIgnoreCase);
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
        /// <summary>Drop repeated suburb/city tokens (e.g. Claremont; Claremont).</summary>
        public bool DeduplicateSuburb { get; set; } = true;
        /// <summary>When delivery area is Cape Town*, remove standalone Cape Town tokens.</summary>
        public bool StripCapeTown { get; set; } = true;
        /// <summary>Title-case address lines (not ALL CAPS / all lowercase).</summary>
        public bool TitleCase { get; set; } = true;
        public bool ReplacePlus27WithZero { get; set; } = true;
        public bool FormatSaPhone { get; set; } = true;
    }
}
