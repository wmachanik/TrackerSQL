using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class PostalAreaSetupManager
    {
        public const string PresetFileName = "area-place-presets.json";

        private readonly SaPostalCodeRepository _postalRepo = new SaPostalCodeRepository();
        private readonly WooAreaDeliveryDefaultRepository _areaRepo = new WooAreaDeliveryDefaultRepository();
        private readonly WooCommerceAreaMappingManager _areaMapping = new WooCommerceAreaMappingManager();
        private readonly OrderManager _orderManager = new OrderManager();
        private List<AreaPlacePreset> _presets;

        public List<PostalAreaSetupRow> GetGridRows()
        {
            var areas = _areaMapping.GetAreaDeliveryDefaults() ?? new List<WooAreaDeliveryDefault>();
            var presets = LoadPresets();
            var rows = new List<PostalAreaSetupRow>();
            foreach (var area in areas)
            {
                var preset = FindPreset(presets, area.AreaName);
                string suggested = string.Empty;
                int count = 0;
                int bridged = 0;
                if (preset != null && preset.Places != null && preset.Places.Count > 0)
                    suggested = new SaPostalCodeManager().BuildRangeTextForPlaces(preset.Places, out count, out bridged);

                bool mapped = !string.IsNullOrWhiteSpace(area.PostalRanges);
                string status = mapped
                    ? "Mapped"
                    : (count > 0 ? "Suggestion" : "Empty");

                int? personId = area.DefaultPreferredAgentID;
                if (!(personId > 0))
                    personId = SuggestPersonId(preset);

                rows.Add(new PostalAreaSetupRow
                {
                    AreaID = area.AreaID,
                    AreaName = area.AreaName,
                    DefaultPreferredAgentID = personId,
                    PostalRanges = area.PostalRanges,
                    SuggestedRanges = suggested,
                    SuggestedCodeCount = count,
                    DispatchHint = preset != null ? preset.Dispatch : string.Empty,
                    Status = status,
                    FillUnmapped = preset != null && preset.FillUnmapped,
                    SuggestedMatchNote = PostalRangeFormatter.BridgedGapNote(bridged)
                });
            }
            return rows;
        }

        public int SaveGrid(IList<PostalAreaSetupRow> rows, string updatedBy)
        {
            if (rows == null)
                return 0;
            int n = 0;
            foreach (var row in rows)
            {
                if (row == null || row.AreaID <= 0)
                    continue;
                int? personId = row.DefaultPreferredAgentID > 0 ? row.DefaultPreferredAgentID : null;
                _areaRepo.Upsert(row.AreaID, personId, row.PostalRanges, updatedBy);
                n++;
            }
            return n;
        }

        public int FillEmptyFromSuggestions(IList<PostalAreaSetupRow> rows, string updatedBy)
        {
            if (rows == null)
                return 0;
            int n = 0;
            foreach (var row in rows)
            {
                if (row == null || row.AreaID <= 0)
                    continue;
                if (string.IsNullOrWhiteSpace(row.PostalRanges) && !string.IsNullOrWhiteSpace(row.SuggestedRanges))
                {
                    row.PostalRanges = row.SuggestedRanges;
                    n++;
                }
                if (!(row.DefaultPreferredAgentID > 0))
                    row.DefaultPreferredAgentID = SuggestPersonId(FindPreset(LoadPresets(), row.AreaName));
            }
            SaveGrid(rows, updatedBy);
            return n;
        }

        public PostalGapAnalysis AnalyseGaps()
        {
            var result = new PostalGapAnalysis
            {
                Groups = new List<PostalGapGroup>(),
                Conflicts = new List<PostalConflictGroup>()
            };
            var all = _postalRepo.GetAllCodesWithPrimaryPlace();
            result.ReferenceCodes = all.Count;
            if (all.Count == 0)
            {
                result.Summary = "Import the SA postcode CSV first.";
                return result;
            }

            var saCodes = new HashSet<int>(all.Select(r => r.PostalCode));
            var pairs = _postalRepo.GetAllPlaceCodePairs();
            var placesByCode = new Dictionary<int, List<string>>();
            foreach (var grp in pairs.GroupBy(p => p.PostalCode))
            {
                placesByCode[grp.Key] = grp.Select(x => x.PlaceName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            var areas = _areaMapping.GetAreaDeliveryDefaults() ?? new List<WooAreaDeliveryDefault>();
            var covered = new Dictionary<int, List<int>>();
            foreach (var area in areas)
            {
                foreach (var range in WooCommerceAreaMappingManager.ParsePostalRanges(area.PostalRanges))
                {
                    for (int c = range.From; c <= range.To; c++)
                    {
                        if (!saCodes.Contains(c))
                            continue;
                        if (!covered.ContainsKey(c))
                            covered[c] = new List<int>();
                        if (!covered[c].Contains(area.AreaID))
                            covered[c].Add(area.AreaID);
                    }
                }
            }

            result.MappedCodes = all.Count(r => covered.ContainsKey(r.PostalCode));
            result.OverlapCodes = covered.Count(kv => kv.Value.Count > 1);

            var unmapped = all.Where(r => !covered.ContainsKey(r.PostalCode)).ToList();
            result.UnmappedCodes = unmapped.Count;

            int? catchAllId = _areaMapping.GetDefaultImportAreaId();
            int regionalId = catchAllId.HasValue && catchAllId.Value > 0
                ? catchAllId.Value
                : FindAreaIdByNames(areas, "Regional", "RegionalSA");
            int gautengId = FindAreaIdByNames(areas, "Gauteng", "Johannesburg");
            string regionalName = areas.FirstOrDefault(a => a.AreaID == regionalId)?.AreaName ?? "Regional";
            string gautengName = areas.FirstOrDefault(a => a.AreaID == gautengId)?.AreaName ?? "Gauteng";
            bool catchAllSet = catchAllId.HasValue && catchAllId.Value > 0;
            result.CatchAllAreaName = catchAllSet ? regionalName : null;

            var presets = LoadPresets();
            var buckets = new Dictionary<int, List<SaPostalCode>>();
            int catchAllLeftovers = 0;
            foreach (var row in unmapped)
            {
                int areaId = SuggestAreaForUnmapped(row, presets, areas, gautengId, regionalId);
                if (catchAllSet && areaId == regionalId)
                {
                    catchAllLeftovers++;
                    continue;
                }
                if (areaId <= 0)
                    continue;
                if (!buckets.ContainsKey(areaId))
                    buckets[areaId] = new List<SaPostalCode>();
                buckets[areaId].Add(row);
            }
            result.CatchAllLeftovers = catchAllLeftovers;

            foreach (var kv in buckets.OrderByDescending(k => k.Value.Count))
            {
                var codes = kv.Value.Select(x => x.PostalCode).Distinct().OrderBy(c => c).ToList();
                var places = kv.Value.Select(x => x.PlaceName)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .GroupBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(g => g.Count())
                    .Take(6)
                    .Select(g => g.Key);
                string areaName = areas.FirstOrDefault(a => a.AreaID == kv.Key)?.AreaName
                    ?? (kv.Key == gautengId ? gautengName : regionalName);
                string reason = kv.Key == gautengId
                    ? "Looks like Gauteng (JHB / Pretoria band or place name)"
                    : (kv.Key == regionalId
                        ? "Would go to catch-all (not in a Cape / city range)"
                        : "Place name matches this Tracker area");
                int bridged;
                string rangeText = PostalRangeFormatter.CollapseCodesToRangeText(codes, out bridged);
                result.Groups.Add(new PostalGapGroup
                {
                    SuggestedAreaID = kv.Key,
                    SuggestedAreaName = areaName,
                    CodeCount = codes.Count,
                    PlaceSummary = string.Join(", ", places),
                    RangeText = rangeText,
                    Reason = reason,
                    MatchNote = PostalRangeFormatter.BridgedGapNote(bridged)
                });
            }

            var conflictBuckets = covered
                .Where(kv => kv.Value.Count > 1)
                .GroupBy(kv => string.Join("+", kv.Value.OrderBy(id => id)))
                .OrderByDescending(g => g.Count());
            foreach (var g in conflictBuckets)
            {
                var ids = g.First().Value.OrderBy(id => id).ToList();
                var names = ids.Select(id => areas.FirstOrDefault(a => a.AreaID == id)?.AreaName ?? ("#" + id)).ToList();
                var codes = g.Select(x => x.Key).Distinct().OrderBy(c => c).ToList();
                var placeNames = new List<string>();
                foreach (int c in codes)
                {
                    List<string> pl;
                    if (placesByCode.TryGetValue(c, out pl) && pl != null)
                        placeNames.AddRange(pl);
                }
                var samplePlaces = placeNames
                    .GroupBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(x => x.Count())
                    .Take(8)
                    .Select(x => x.Key);
                result.Conflicts.Add(new PostalConflictGroup
                {
                    AreaNames = string.Join(" + ", names),
                    CodeCount = codes.Count,
                    PlaceSummary = string.Join(", ", samplePlaces),
                    RangeText = PostalRangeFormatter.CollapseCodesToRangeText(codes),
                    Kind = ClassifyConflict(ids, codes, areas, placesByCode, presets)
                });
            }

            int onlyOne = result.MappedCodes - result.OverlapCodes;
            if (onlyOne < 0)
                onlyOne = 0;
            result.Summary =
                "SA table: " + result.ReferenceCodes + " postcodes. "
                + onlyOne + " in exactly one area. "
                + result.OverlapCodes + " in two or more areas (conflicts). "
                + result.UnmappedCodes + " in no area"
                + (catchAllLeftovers > 0
                    ? " — those " + catchAllLeftovers + " use catch-all " + regionalName + " and do not need to be listed."
                    : ".")
                + (catchAllSet ? "" : " No catch-all area is set.");
            return result;
        }

        /// <summary>
        /// Shared = different suburbs for the same code (Hout Bay vs Constantia).
        /// Overlap = the same places are claimed by more than one area.
        /// </summary>
        private static string ClassifyConflict(
            List<int> areaIds,
            List<int> codes,
            List<WooAreaDeliveryDefault> areas,
            Dictionary<int, List<string>> placesByCode,
            List<AreaPlacePreset> presets)
        {
            int uniquePlaceWins = 0;
            int sharedPlaceWins = 0;
            foreach (int code in codes)
            {
                List<string> places;
                if (!placesByCode.TryGetValue(code, out places) || places == null || places.Count == 0)
                    continue;
                foreach (string place in places)
                {
                    int bestScore = 0;
                    int winners = 0;
                    foreach (int areaId in areaIds)
                    {
                        string areaName = areas.FirstOrDefault(a => a.AreaID == areaId)?.AreaName;
                        int score = PostalPlaceMatcher.ScoreSuburb(presets, areaName, place);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            winners = 1;
                        }
                        else if (score > 0 && score == bestScore)
                            winners++;
                    }
                    if (bestScore <= 0)
                        continue;
                    if (winners == 1)
                        uniquePlaceWins++;
                    else
                        sharedPlaceWins++;
                }
            }

            if (uniquePlaceWins > 0 && sharedPlaceWins == 0)
                return "Shared postcode — keep both ranges; suburb chooses (e.g. Hout Bay vs Constantia)";
            if (uniquePlaceWins > sharedPlaceWins)
                return "Mostly shared — suburb usually chooses; check leftover overlap";
            return "Overlap — more than one area claims the same places; trim one range";
        }

        public void AssignGapGroup(int areaId, string rangeText, string updatedBy)
        {
            if (areaId <= 0 || string.IsNullOrWhiteSpace(rangeText))
                return;
            var area = _areaRepo.GetAllWithAreas().FirstOrDefault(a => a.AreaID == areaId);
            string existing = area != null ? area.PostalRanges : null;
            string merged = MergeRangeText(existing, rangeText);
            int? personId = area != null && area.DefaultPreferredAgentID > 0
                ? area.DefaultPreferredAgentID
                : SuggestPersonId(FindPreset(LoadPresets(), area?.AreaName));
            _areaRepo.Upsert(areaId, personId, merged, updatedBy);
        }

        public List<Person> GetDeliveryPersons()
        {
            return _orderManager.GetDeliveryPersons() ?? new List<Person>();
        }

        public int SuggestPersonId(AreaPlacePreset preset)
        {
            string dispatch = preset != null ? preset.Dispatch : null;
            if (string.Equals(dispatch, "Pargo", StringComparison.OrdinalIgnoreCase))
                return FindPersonId(SystemConstants.DeliveryConstants.ParcelDispatchID, "Prgo", "Pargo");
            if (string.Equals(dispatch, "Courier", StringComparison.OrdinalIgnoreCase))
            {
                int id = FindPersonId(SystemConstants.DeliveryConstants.CourierDeliveryID, "Cour", "FastWay", "Fastway", "RegionalSA");
                return id > 0 ? id : WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId();
            }
            return WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId();
        }

        private int FindPersonId(int preferredId, params string[] abbrevs)
        {
            var people = GetDeliveryPersons();
            if (people.Any(p => p.PersonID == preferredId))
                return preferredId;
            foreach (string ab in abbrevs)
            {
                var match = people.FirstOrDefault(p =>
                    string.Equals(p.Abbreviation, ab, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(p.PersonName)
                        && p.PersonName.IndexOf(ab, StringComparison.OrdinalIgnoreCase) >= 0));
                if (match != null)
                    return match.PersonID;
            }
            return preferredId;
        }

        private int SuggestAreaForUnmapped(
            SaPostalCode row,
            List<AreaPlacePreset> presets,
            List<WooAreaDeliveryDefault> areas,
            int gautengId,
            int regionalId)
        {
            string place = row.PlaceName ?? string.Empty;
            foreach (var preset in presets)
            {
                if (preset.Places == null)
                    continue;
                foreach (string p in preset.Places)
                {
                    if (place.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        int id = FindAreaIdByNames(areas, preset.AreaNames?.ToArray() ?? new string[0]);
                        if (id > 0)
                            return id;
                    }
                }
            }

            int code = row.PostalCode;
            if (gautengId > 0 && IsGautengBand(code))
                return gautengId;
            return regionalId > 0 ? regionalId : 0;
        }

        /// <summary>Pretoria 0001–0299, East/West Rand and JHB 1400–2199.</summary>
        public static bool IsGautengBand(int code)
        {
            return (code >= 1 && code <= 299) || (code >= 1400 && code <= 2199);
        }

        private static int FindAreaIdByNames(List<WooAreaDeliveryDefault> areas, params string[] names)
        {
            if (areas == null || names == null)
                return 0;
            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                var match = areas.FirstOrDefault(a =>
                    !string.IsNullOrWhiteSpace(a.AreaName)
                    && (string.Equals(a.AreaName.Trim(), name, StringComparison.OrdinalIgnoreCase)
                        || a.AreaName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0));
                if (match != null)
                    return match.AreaID;
            }
            return 0;
        }

        private static AreaPlacePreset FindPreset(List<AreaPlacePreset> presets, string areaName)
        {
            return PostalPlaceMatcher.FindPreset(presets, areaName);
        }

        private static string Normalise(string s)
        {
            return PostalPlaceMatcher.Normalise(s);
        }

        /// <summary>How well a suburb / place name fits this Tracker area (0 = no match).</summary>
        public int ScoreSuburb(string areaName, string suburb)
        {
            return PostalPlaceMatcher.ScoreSuburb(LoadPresets(), areaName, suburb);
        }

        private static string MergeRangeText(string existing, string extra)
        {
            var codes = new HashSet<int>();
            foreach (var r in WooCommerceAreaMappingManager.ParsePostalRanges(existing))
            {
                for (int c = r.From; c <= r.To; c++)
                    codes.Add(c);
            }
            foreach (var r in WooCommerceAreaMappingManager.ParsePostalRanges(extra))
            {
                for (int c = r.From; c <= r.To; c++)
                    codes.Add(c);
            }
            return PostalRangeFormatter.CollapseCodesToRangeText(codes);
        }

        public List<AreaPlacePreset> GetPresetsForMatch()
        {
            return LoadPresets();
        }

        private List<AreaPlacePreset> LoadPresets()
        {
            if (_presets != null)
                return _presets;
            _presets = new List<AreaPlacePreset>();
            string path = ResolvePresetPath();
            if (!File.Exists(path))
                return _presets;
            try
            {
                var root = JsonConvert.DeserializeObject<AreaPlacePresetFile>(File.ReadAllText(path));
                if (root != null && root.Presets != null)
                    _presets = root.Presets;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("system", "Failed to load area-place-presets.json: " + ex.Message);
            }
            return _presets;
        }

        private static string ResolvePresetPath()
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Server.MapPath("~/Data/" + PresetFileName);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", PresetFileName);
        }
    }

    public class AreaPlacePresetFile
    {
        [JsonProperty("presets")]
        public List<AreaPlacePreset> Presets { get; set; }
    }

    public class AreaPlacePreset
    {
        [JsonProperty("areaNames")]
        public List<string> AreaNames { get; set; }
        [JsonProperty("dispatch")]
        public string Dispatch { get; set; }
        [JsonProperty("places")]
        public List<string> Places { get; set; }
        [JsonProperty("fillUnmapped")]
        public bool FillUnmapped { get; set; }
    }

    /// <summary>Suburb / place matching against area-place-presets.json (no DB).</summary>
    public static class PostalPlaceMatcher
    {
        public static AreaPlacePreset FindPreset(List<AreaPlacePreset> presets, string areaName)
        {
            if (presets == null || string.IsNullOrWhiteSpace(areaName))
                return null;
            string norm = Normalise(areaName);
            foreach (var preset in presets)
            {
                if (preset.AreaNames == null)
                    continue;
                foreach (string alias in preset.AreaNames)
                {
                    string a = Normalise(alias);
                    if (norm == a || norm.Contains(a) || a.Contains(norm))
                        return preset;
                }
            }
            return null;
        }

        public static int ScoreSuburb(List<AreaPlacePreset> presets, string areaName, string suburb)
        {
            if (string.IsNullOrWhiteSpace(suburb) || string.IsNullOrWhiteSpace(areaName))
                return 0;
            string sub = Normalise(suburb);
            if (sub.Length == 0)
                return 0;
            string hay = " " + sub + " ";
            int best = 0;

            string areaNorm = Normalise(areaName);
            if (areaNorm.Length >= 4 && (hay.IndexOf(" " + areaNorm + " ", StringComparison.Ordinal) >= 0
                || sub == areaNorm || areaNorm.IndexOf(sub, StringComparison.Ordinal) >= 0))
                best = areaNorm.Length + 20;

            var preset = FindPreset(presets, areaName);
            if (preset != null && preset.Places != null)
            {
                foreach (string p in preset.Places)
                {
                    string n = Normalise(p);
                    if (n.Length < 3)
                        continue;
                    if (hay.IndexOf(" " + n + " ", StringComparison.Ordinal) >= 0 || sub == n)
                        best = Math.Max(best, n.Length);
                }
            }
            return best;
        }

        public static string Normalise(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;
            return s.Trim().ToLowerInvariant()
                .Replace("cape town:", string.Empty)
                .Replace("cpt:", string.Empty)
                .Replace("'", string.Empty)
                .Replace("  ", " ")
                .Trim();
        }
    }
}
