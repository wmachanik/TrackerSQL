using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackerSQL.Managers
{
    /// <summary>Why this alternative address exists (Contact Details → Alternative Addresses).</summary>
    public static class AlternativeAddressKind
    {
        public const string OnBehalf = "OnBehalf";
        public const string Gift = "Gift";
        public const string Home = "Home";
        public const string Work = "Work";
        public const string Friend = "Friend";
        public const string Family = "Family";
        public const string Other = "Other";

        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { OnBehalf, "On their behalf" },
            { Gift, "As a gift" },
            { Home, "Home" },
            { Work, "Work" },
            { Friend, "Friend" },
            { Family, "Family" },
            { Other, "Other" }
        };

        public static IReadOnlyList<KeyValuePair<string, string>> AllForDropdown()
        {
            return DisplayNames.Select(kv => kv).ToList();
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return OnBehalf;
            string v = value.Trim();
            foreach (var kv in DisplayNames)
            {
                if (string.Equals(kv.Key, v, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(kv.Value, v, StringComparison.OrdinalIgnoreCase))
                    return kv.Key;
            }
            return OnBehalf;
        }

        public static string Display(string storedCode)
        {
            if (string.IsNullOrWhiteSpace(storedCode))
                return DisplayNames[OnBehalf];
            return DisplayNames.TryGetValue(storedCode.Trim(), out string text) ? text : storedCode.Trim();
        }
    }
}
