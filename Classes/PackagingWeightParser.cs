using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Derives pack weight (kg) from packaging descriptions such as "250g bx", "275g bag", "1kg bag".
    /// Blank / unparseable packaging is treated as 1kg — no DB schema change required.
    /// </summary>
    public static class PackagingWeightParser
    {
        private static readonly Regex KgPattern = new Regex(
            @"(\d+(?:[.,]\d+)?)\s*kg\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex GramsPattern = new Regex(
            @"(\d+(?:[.,]\d+)?)\s*g(?:rams?)?\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Pack size in kilograms. Default 1kg when missing or not parseable.
        /// </summary>
        public static double ParsePackWeightKg(string packagingDesc)
        {
            if (string.IsNullOrWhiteSpace(packagingDesc))
                return 1.0;

            string text = packagingDesc.Trim();

            Match kgMatch = KgPattern.Match(text);
            if (kgMatch.Success)
            {
                double kg;
                if (TryParseNumber(kgMatch.Groups[1].Value, out kg) && kg > 0)
                    return kg;
            }

            Match gMatch = GramsPattern.Match(text);
            if (gMatch.Success)
            {
                double grams;
                if (TryParseNumber(gMatch.Groups[1].Value, out grams) && grams > 0)
                    return grams / 1000.0;
            }

            return 1.0;
        }

        /// <summary>
        /// Order qty is in kg; packaging size converts to number of packs
        /// (e.g. 0.5 kg with 250g packaging → 2 packs).
        /// </summary>
        public static double ComputePackQty(double qtyKg, string packagingDesc)
        {
            double packKg = ParsePackWeightKg(packagingDesc);
            if (packKg <= 0)
                packKg = 1.0;
            // One decimal is enough for pack counts (e.g. 1.3 packs of 250g).
            return Math.Round(qtyKg / packKg, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// If the text looks like a pack weight (250g, 1kg), returns that weight in kilograms
        /// for use as a Woo qty factor. Otherwise null.
        /// </summary>
        public static double? TryParseQtyFactorFromOption(string optionText)
        {
            if (string.IsNullOrWhiteSpace(optionText))
                return null;

            string text = optionText.Trim();
            if (!KgPattern.IsMatch(text) && !GramsPattern.IsMatch(text))
                return null;

            double kg = ParsePackWeightKg(text);
            return kg > 0 ? (double?)kg : null;
        }

        private static bool TryParseNumber(string value, out double number)
        {
            string normalized = (value ?? string.Empty).Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }
    }
}
