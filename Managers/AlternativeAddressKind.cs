using System;
using System.Collections.Generic;
using System.Linq;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>Why this alternative address exists. Types live in Lookups → Alt Address Types.</summary>
    public static class AlternativeAddressKind
    {
        public const string OnBehalf = "OnBehalf";
        public const string Gift = "Gift";
        public const string Home = "Home";
        public const string Work = "Work";
        public const string Friend = "Friend";
        public const string Family = "Family";
        public const string Other = "Other";
        /// <summary>Accounting / ZZName guest sites. Display name is Ad hoc.</summary>
        public const string Sundry = "Sundry";
        /// <summary>Old stored code; <see cref="Normalize"/> maps this to <see cref="Sundry"/>.</summary>
        public const string Casual = "Casual";

        private static readonly object CacheLock = new object();
        private static List<AltAddressKind> _cache;

        private static readonly AltAddressKind[] Fallback =
        {
            new AltAddressKind { KindCode = OnBehalf, KindName = "On their behalf", CountsTowardTracking = false, SortOrder = 10, Enabled = true },
            new AltAddressKind { KindCode = Gift, KindName = "As a gift", CountsTowardTracking = false, SortOrder = 20, Enabled = true },
            new AltAddressKind { KindCode = Home, KindName = "Home", CountsTowardTracking = true, SortOrder = 30, Enabled = true },
            new AltAddressKind { KindCode = Work, KindName = "Work", CountsTowardTracking = true, SortOrder = 40, Enabled = true },
            new AltAddressKind { KindCode = Friend, KindName = "Friend", CountsTowardTracking = true, SortOrder = 50, Enabled = true },
            new AltAddressKind { KindCode = Family, KindName = "Family", CountsTowardTracking = true, SortOrder = 60, Enabled = true },
            new AltAddressKind { KindCode = Other, KindName = "Other", CountsTowardTracking = true, SortOrder = 70, Enabled = true },
            new AltAddressKind { KindCode = Sundry, KindName = "Ad hoc", CountsTowardTracking = false, SortOrder = 80, Enabled = true }
        };

        public static void ResetCache()
        {
            lock (CacheLock)
                _cache = null;
        }

        public static List<AltAddressKind> All()
        {
            lock (CacheLock)
            {
                if (_cache != null)
                    return _cache;
                try
                {
                    if (AltAddressKindsRepository.TableSeen())
                    {
                        List<AltAddressKind> rows = new AltAddressKindsRepository().GetAll();
                        if (rows.Count > 0)
                        {
                            _cache = rows;
                            return _cache;
                        }
                    }
                }
                catch
                {
                    // Use built-in types until the lookup table is available.
                }
                _cache = Fallback.ToList();
                return _cache;
            }
        }

        public static IReadOnlyList<KeyValuePair<string, string>> AllForDropdown()
        {
            return All()
                .Where(k => k.Enabled)
                .OrderBy(k => k.SortOrder)
                .ThenBy(k => k.KindName)
                .Select(k => new KeyValuePair<string, string>(k.KindCode, k.KindName))
                .ToList();
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DefaultCode();
            string v = value.Trim();
            if (string.Equals(v, Casual, StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "Casual (ZZName)", StringComparison.OrdinalIgnoreCase))
                v = Sundry;
            foreach (AltAddressKind kind in All())
            {
                if (string.Equals(kind.KindCode, v, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(kind.KindName, v, StringComparison.OrdinalIgnoreCase))
                    return kind.KindCode;
            }
            return DefaultCode();
        }

        public static string Display(string storedCode)
        {
            if (string.IsNullOrWhiteSpace(storedCode))
                return Display(DefaultCode());
            string code = Normalize(storedCode);
            foreach (AltAddressKind kind in All())
            {
                if (string.Equals(kind.KindCode, code, StringComparison.OrdinalIgnoreCase))
                    return kind.KindName;
            }
            return storedCode.Trim();
        }

        /// <summary>
        /// When the type does not count toward tracking, Order Done must not write usage
        /// or move Next Coffee / filter dates.
        /// </summary>
        public static bool SkipsUsagePrediction(string storedCode)
        {
            string code = Normalize(storedCode);
            foreach (AltAddressKind kind in All())
            {
                if (string.Equals(kind.KindCode, code, StringComparison.OrdinalIgnoreCase))
                    return !kind.CountsTowardTracking;
            }
            return string.Equals(code, Gift, StringComparison.OrdinalIgnoreCase)
                || string.Equals(code, OnBehalf, StringComparison.OrdinalIgnoreCase)
                || string.Equals(code, Sundry, StringComparison.OrdinalIgnoreCase)
                || string.Equals(code, Casual, StringComparison.OrdinalIgnoreCase);
        }

        public static string DefaultCode()
        {
            AltAddressKind first = All().FirstOrDefault(k => k.Enabled);
            return first != null ? first.KindCode : OnBehalf;
        }
    }
}
