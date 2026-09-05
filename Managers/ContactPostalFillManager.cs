using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class ContactPostalFillManager
    {
        private readonly ContactsRepository _contacts = new ContactsRepository();
        private readonly SaPostalCodeRepository _postal = new SaPostalCodeRepository();
        private readonly WooAreaDeliveryDefaultRepository _areas = new WooAreaDeliveryDefaultRepository();

        public List<ContactPostalSuggestion> ScanMissingPostalCodes(bool enabledOnly = true)
        {
            var contacts = _contacts.GetMissingPostalCodes(enabledOnly);
            var places = _postal.GetAllPlaceCodePairs();
            var areaRows = _areas.GetAllWithAreas() ?? new List<WooAreaDeliveryDefault>();
            var areaById = areaRows.ToDictionary(a => a.AreaID, a => a);
            var areaCodeSets = BuildAreaCodeSets(areaRows);

            var results = new List<ContactPostalSuggestion>();
            int skippedBlank = 0;
            int skippedCollect = 0;
            foreach (var c in contacts)
            {
                string areaName = null;
                if (c.AreaID.HasValue && areaById.TryGetValue(c.AreaID.Value, out var area))
                    areaName = area.AreaName;

                if (IsCollectArea(areaName))
                {
                    skippedCollect++;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(c.BillingAddress))
                {
                    skippedBlank++;
                    continue;
                }

                var suggestion = SuggestForContact(c, places, areaCodeSets);
                results.Add(new ContactPostalSuggestion
                {
                    ContactID = c.ContactID,
                    CompanyName = c.CompanyName,
                    BillingAddress = c.BillingAddress,
                    AreaID = c.AreaID,
                    AreaName = areaName,
                    SuggestedPostalCode = suggestion.Code,
                    MatchPlace = suggestion.Place,
                    Reason = suggestion.Reason,
                    Confidence = suggestion.Confidence,
                    Selected = !string.IsNullOrWhiteSpace(suggestion.Code)
                });
            }

            skippedBlankAddress = skippedBlank;
            skippedCollectArea = skippedCollect;

            return results
                .OrderByDescending(r => r.Confidence)
                .ThenBy(r => r.CompanyName)
                .ToList();
        }

        public static bool IsCollectArea(string areaName)
        {
            if (string.IsNullOrWhiteSpace(areaName))
                return false;
            string n = areaName.Trim();
            return n.IndexOf("collect", StringComparison.OrdinalIgnoreCase) >= 0
                || string.Equals(n, "Cllct", StringComparison.OrdinalIgnoreCase);
        }

        public ContactPostalSuggestion SuggestOne(Contact contact)
        {
            if (contact == null)
                return null;
            return SuggestOneInternal(contact);
        }

        public int SkippedBlankAddress { get { return skippedBlankAddress; } }
        public int SkippedCollectArea { get { return skippedCollectArea; } }

        private int skippedBlankAddress;
        private int skippedCollectArea;

        private ContactPostalSuggestion SuggestOneInternal(Contact contact)
        {
            var places = _postal.GetAllPlaceCodePairs();
            var areaRows = _areas.GetAllWithAreas() ?? new List<WooAreaDeliveryDefault>();
            var areaCodeSets = BuildAreaCodeSets(areaRows);
            string areaName = null;
            if (contact.AreaID.HasValue)
            {
                var area = areaRows.FirstOrDefault(a => a.AreaID == contact.AreaID.Value);
                areaName = area?.AreaName;
            }
            var suggestion = SuggestForContact(contact, places, areaCodeSets);
            return new ContactPostalSuggestion
            {
                ContactID = contact.ContactID,
                CompanyName = contact.CompanyName,
                BillingAddress = contact.BillingAddress,
                AreaID = contact.AreaID,
                AreaName = areaName,
                SuggestedPostalCode = suggestion.Code,
                MatchPlace = suggestion.Place,
                Reason = suggestion.Reason,
                Confidence = suggestion.Confidence
            };
        }

        public int ApplySuggestions(IEnumerable<ContactPostalSuggestion> rows)
        {
            if (rows == null)
                return 0;
            int n = 0;
            foreach (var row in rows)
            {
                if (row == null || !row.Selected || row.ContactID <= 0)
                    continue;
                string code = NormalizeCode(row.SuggestedPostalCode);
                if (string.IsNullOrEmpty(code))
                    continue;
                string note = "Postal code " + code + " assigned from Contact postal fill"
                    + (!string.IsNullOrWhiteSpace(row.MatchPlace) ? " (place: " + row.MatchPlace + ")" : "");
                if (_contacts.UpdatePostalCode(row.ContactID, code, note))
                    n++;
            }
            return n;
        }

        private static Dictionary<int, HashSet<int>> BuildAreaCodeSets(List<WooAreaDeliveryDefault> areas)
        {
            var map = new Dictionary<int, HashSet<int>>();
            foreach (var area in areas)
            {
                if (area == null || area.AreaID <= 0 || string.IsNullOrWhiteSpace(area.PostalRanges))
                    continue;
                var set = new HashSet<int>();
                foreach (var range in WooCommerceAreaMappingManager.ParsePostalRanges(area.PostalRanges))
                {
                    for (int c = range.From; c <= range.To; c++)
                        set.Add(c);
                }
                if (set.Count > 0)
                    map[area.AreaID] = set;
            }
            return map;
        }

        private static Suggestion SuggestForContact(
            Contact contact,
            List<SaPostalCode> places,
            Dictionary<int, HashSet<int>> areaCodeSets)
        {
            string address = contact.BillingAddress ?? string.Empty;
            if (string.IsNullOrWhiteSpace(address) && !contact.AreaID.HasValue)
                return Suggestion.Empty("No address or area to match");

            var embedded = Regex.Matches(address, @"\b(\d{4})\b");
            if (embedded.Count > 0)
            {
                string code = embedded[embedded.Count - 1].Groups[1].Value;
                return new Suggestion
                {
                    Code = code,
                    Place = null,
                    Reason = "4-digit code found in address",
                    Confidence = 3
                };
            }

            if (places == null || places.Count == 0)
                return Suggestion.Empty("Import SA postcode CSV first");

            string hay = NormalizePlace(address);
            if (hay.Length < 4)
                return Suggestion.Empty("No suburb/place match in address");

            string compactHay = Compact(hay);
            hay = ExpandKnownSuburbAliases(hay, compactHay);
            compactHay = Compact(hay);
            HashSet<int> areaCodes = null;
            if (contact.AreaID.HasValue)
                areaCodeSets.TryGetValue(contact.AreaID.Value, out areaCodes);

            SaPostalCode best = null;
            int bestScore = 0;
            int bestNameLen = 0;
            foreach (var place in places)
            {
                string pn = NormalizePlace(place.PlaceName);
                if (pn.Length < 4)
                    continue;
                int score = PlaceHitScore(hay, compactHay, pn);
                if (score <= 0)
                    continue;
                if (areaCodes != null && areaCodes.Contains(place.PostalCode))
                    score += 50;
                if (score > bestScore || (score == bestScore && pn.Length > bestNameLen))
                {
                    bestScore = score;
                    bestNameLen = pn.Length;
                    best = place;
                }
            }

            if (best != null)
            {
                bool inArea = areaCodes != null && areaCodes.Contains(best.PostalCode);
                return new Suggestion
                {
                    Code = best.PostalCode.ToString("0000", CultureInfo.InvariantCulture),
                    Place = best.PlaceName,
                    Reason = "Matched SA place \"" + best.PlaceName + "\" in address"
                        + (inArea ? " (in this contact's area ranges)" : ""),
                    Confidence = inArea ? 3 : 2
                };
            }

            if (contact.AreaID.HasValue && areaCodes != null && areaCodes.Count == 1)
            {
                int only = areaCodes.First();
                return new Suggestion
                {
                    Code = only.ToString("0000", CultureInfo.InvariantCulture),
                    Place = null,
                    Reason = "Only one postcode in the contact's area ranges",
                    Confidence = 1
                };
            }

            return Suggestion.Empty("No suburb/place in the SA table matched this address");
        }

        /// <summary>Letters/digits only, single spaces. Simon'S Town → simons town.</summary>
        private static string NormalizePlace(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var chars = new char[text.Length];
            int n = 0;
            bool space = true;
            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c))
                {
                    chars[n++] = char.ToLowerInvariant(c);
                    space = false;
                }
                else if (!space)
                {
                    chars[n++] = ' ';
                    space = true;
                }
            }
            if (n > 0 && chars[n - 1] == ' ')
                n--;
            return n == 0 ? string.Empty : new string(chars, 0, n);
        }

        private static string Compact(string normalized)
        {
            return (normalized ?? string.Empty).Replace(" ", string.Empty);
        }

        private static int PlaceHitScore(string hay, string compactHay, string placeNorm)
        {
            string padded = " " + hay + " ";
            if (padded.IndexOf(" " + placeNorm + " ", StringComparison.Ordinal) >= 0)
                return 20 + placeNorm.Length;

            string compactPlace = Compact(placeNorm);
            if (compactPlace.Length >= 6 && compactHay.IndexOf(compactPlace, StringComparison.Ordinal) >= 0)
                return 15 + compactPlace.Length;

            return 0;
        }

        /// <summary>
        /// Spellings / suburbs the geo file omits or names differently, mapped to a Place Name that exists in SaPostalCodeTbl.
        /// </summary>
        private static string ExpandKnownSuburbAliases(string hay, string compactHay)
        {
            string extra = string.Empty;
            AppendAlias(ref extra, compactHay, "simonstown", "simons town");
            AppendAlias(ref extra, compactHay, "kenilworth", "claremont");
            AppendAlias(ref extra, compactHay, "kalkbay", "lakeside");
            AppendAlias(ref extra, compactHay, "glencairn", "simons town");
            AppendAlias(ref extra, compactHay, "killarney", "milnerton");
            AppendAlias(ref extra, compactHay, "murdochvalley", "simons town");
            if (extra.Length == 0)
                return hay;
            return hay + " " + extra.Trim();
        }

        private static void AppendAlias(ref string extra, string compactHay, string compactKey, string tablePlaceNorm)
        {
            if (compactHay.IndexOf(compactKey, StringComparison.Ordinal) >= 0)
                extra += " " + tablePlaceNorm;
        }

        private static string NormalizeCode(string text)
        {
            int n = WooCommerceAreaMappingManager.ParsePostalCode(text);
            return n > 0 ? n.ToString("0000", CultureInfo.InvariantCulture) : null;
        }

        private sealed class Suggestion
        {
            public string Code { get; set; }
            public string Place { get; set; }
            public string Reason { get; set; }
            public int Confidence { get; set; }

            public static Suggestion Empty(string reason)
            {
                return new Suggestion { Reason = reason, Confidence = 0 };
            }
        }
    }
}
