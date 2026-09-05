using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class WooCommerceAreaMappingManager
    {
        private readonly WooAreaDeliveryDefaultRepository _areaDefaultRepo = new WooAreaDeliveryDefaultRepository();
        private readonly WooShippingMethodMapRepository _shipRepo = new WooShippingMethodMapRepository();
        private readonly WooCommerceSettingsRepository _settingsRepo = new WooCommerceSettingsRepository();
        private readonly AreasRepository _areasRepo = new AreasRepository();

        public List<WooAreaDeliveryDefault> GetAreaDeliveryDefaults()
        {
            var list = _areaDefaultRepo.GetAllWithAreas();
            MigrateLegacyPostalMapsIfNeeded(list);
            return list;
        }

        public List<WooShippingMethodMap> GetShippingMethodMaps(bool includeInactive = true)
        {
            return _shipRepo.GetAllOrdered(includeInactive);
        }

        public int? GetDefaultImportAreaId()
        {
            return _settingsRepo.GetSettings().DefaultImportAreaID;
        }

        public void SaveDefaultImportArea(int? areaId, string updatedBy)
        {
            var s = _settingsRepo.GetSettings();
            s.DefaultImportAreaID = areaId > 0 ? areaId : null;
            _settingsRepo.SaveSettings(s, updatedBy);
        }

        public int SaveAreaDeliveryDefaults(IList<WooAreaDeliveryDefault> rows, string updatedBy)
        {
            if (rows == null)
                return 0;
            int n = 0;
            foreach (var row in rows)
            {
                if (row == null || row.AreaID <= 0)
                    continue;
                int? personId = row.DefaultPreferredAgentID > 0 ? row.DefaultPreferredAgentID : null;
                _areaDefaultRepo.Upsert(row.AreaID, personId, row.PostalRanges, updatedBy);
                n++;
            }
            return n;
        }

        public int SaveShippingMethodMaps(IList<WooShippingMethodMap> rows, string updatedBy)
        {
            if (rows == null)
                return 0;
            int n = 0;
            foreach (var row in rows)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.MethodMatch) || row.ToBeDeliveredByID <= 0)
                    continue;
                row.MethodMatch = row.MethodMatch.Trim();
                if (row.MapID > 0)
                {
                    _shipRepo.UpdateMap(row, updatedBy);
                    n++;
                }
                else
                {
                    _shipRepo.InsertMap(row, updatedBy);
                    n++;
                }
            }
            return n;
        }

        public void DeleteShippingMethodMap(int mapId, string updatedBy)
        {
            if (mapId <= 0)
                return;
            _shipRepo.DeleteMap(mapId);
            AppLogger.WriteLog("woo", "Deleted shipping method map #" + mapId, updatedBy);
        }

        /// <summary>Resolve Tracker area from SA postcode + shipping suburb (order import).</summary>
        public WooAreaResolveResult ResolveArea(string postalCode, string suburb)
        {
            return ResolveArea(postalCode, suburb, null);
        }

        public WooAreaResolveResult ResolveArea(string postalCode, string suburb, string provinceOrState)
        {
            int code = ParsePostalCode(postalCode);
            if (code <= 0)
                return FallbackDefault("Invalid or missing postcode");

            var fromRanges = ResolveAreaFromConfiguredRanges(code, suburb);
            if (fromRanges != null && fromRanges.AreaID.HasValue && !fromRanges.IsAmbiguous)
                return fromRanges;

            var fromSa = TryResolveAreaFromSaPostal(code, suburb, provinceOrState);
            if (fromSa != null && fromSa.AreaID.HasValue && !fromSa.IsAmbiguous)
                return fromSa;

            if (fromRanges != null)
                return fromRanges;

            return fromSa ?? FallbackDefault("Postcode not in any configured range");
        }

        private WooAreaResolveResult ResolveAreaFromConfiguredRanges(int code, string suburb)
        {
            var areas = GetAreaDeliveryDefaults();
            var matches = new List<AreaRangeMatch>();
            foreach (var area in areas)
            {
                if (area == null || string.IsNullOrWhiteSpace(area.PostalRanges))
                    continue;
                foreach (var range in ParsePostalRanges(area.PostalRanges))
                {
                    if (code >= range.From && code <= range.To)
                    {
                        matches.Add(new AreaRangeMatch
                        {
                            Area = area,
                            Range = range
                        });
                    }
                }
            }

            if (matches.Count == 0)
                return null;

            var distinctAreas = matches
                .GroupBy(m => m.Area.AreaID)
                .ToList();

            if (distinctAreas.Count > 1)
            {
                var presets = new PostalAreaSetupManager().GetPresetsForMatch();
                var scored = distinctAreas
                    .Select(g =>
                    {
                        var area = g.First().Area;
                        var range = g.OrderBy(m => m.Range.To - m.Range.From).First().Range;
                        return new
                        {
                            Area = area,
                            Range = range,
                            Score = PostalPlaceMatcher.ScoreSuburb(presets, area.AreaName, suburb)
                        };
                    })
                    .ToList();

                var withScore = scored.Where(s => s.Score > 0)
                    .OrderByDescending(s => s.Score)
                    .ThenBy(s => s.Range.To - s.Range.From)
                    .ToList();

                if (withScore.Count > 0 && (withScore.Count == 1 || withScore[0].Score > withScore[1].Score))
                {
                    var win = withScore[0];
                    return BuildResult(
                        win.Area.AreaID,
                        win.Area.AreaName,
                        false,
                        "Postcode " + code.ToString("0000", CultureInfo.InvariantCulture)
                        + " is shared; suburb matched " + win.Area.AreaName);
                }

                string names = string.Join(", ", distinctAreas.Select(g => g.First().Area.AreaName).Distinct());
                return new WooAreaResolveResult
                {
                    IsAmbiguous = true,
                    Reason = "Postcode " + code.ToString("0000", CultureInfo.InvariantCulture)
                        + " is used by more than one area (" + names
                        + "). Add the suburb (e.g. Hout Bay vs Constantia) to choose."
                };
            }

            var winner = matches
                .OrderBy(m => m.Range.To - m.Range.From)
                .ThenBy(m => m.Area.AreaID)
                .First();

            return BuildResult(
                winner.Area.AreaID,
                winner.Area.AreaName,
                false,
                "Matched " + FormatRange(winner.Range));
        }

        /// <summary>When postcode ranges fail or are ambiguous, match shipping city/suburb via SA postal places.</summary>
        private WooAreaResolveResult TryResolveAreaFromSaPostal(int code, string suburb, string provinceOrState)
        {
            var postalRepo = new SaPostalCodeRepository();
            if (!postalRepo.TableExists() || postalRepo.GetRowCount() <= 0)
                return null;

            var places = postalRepo.GetPlacesForPostalCode(code);
            if (places == null || places.Count == 0)
                return null;

            SaPostalCode place = PickSaPlace(places, suburb, provinceOrState);
            string probe = FirstNonEmpty(suburb, place?.PlaceName);
            if (string.IsNullOrWhiteSpace(probe))
                return null;

            var presets = new PostalAreaSetupManager().GetPresetsForMatch();
            var candidates = BuildAreaMatchCandidates();
            var scored = candidates
                .Select(a => new
                {
                    AreaId = a.AreaID,
                    AreaName = a.AreaName,
                    Score = PostalPlaceMatcher.ScoreSuburb(presets, a.AreaName, probe)
                        + (place != null ? PostalPlaceMatcher.ScoreSuburb(presets, a.AreaName, place.PlaceName) : 0)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.AreaId)
                .ToList();

            if (scored.Count == 0)
                return null;
            if (scored.Count > 1 && scored[0].Score == scored[1].Score)
                return null;

            var win = scored[0];
            string placeNote = place != null ? " (" + place.PlaceName + ")" : string.Empty;
            return BuildResult(
                win.AreaId,
                win.AreaName,
                false,
                "Matched shipping address" + placeNote + " via SA postcode " + code.ToString("0000", CultureInfo.InvariantCulture));
        }

        private List<Area> BuildAreaMatchCandidates()
        {
            var seen = new HashSet<int>();
            var list = new List<Area>();
            foreach (var row in GetAreaDeliveryDefaults())
            {
                if (row == null || row.AreaID <= 0 || !seen.Add(row.AreaID))
                    continue;
                list.Add(new Area { AreaID = row.AreaID, AreaName = row.AreaName });
            }
            foreach (var area in _areasRepo.GetAll("AreaName"))
            {
                if (area == null || area.AreaID <= 0 || !seen.Add(area.AreaID))
                    continue;
                list.Add(area);
            }
            return list;
        }

        private static SaPostalCode PickSaPlace(List<SaPostalCode> places, string suburb, string provinceOrState)
        {
            if (places == null || places.Count == 0)
                return null;
            if (places.Count == 1)
                return places[0];

            string sub = PostalPlaceMatcher.Normalise(suburb);
            string prov = PostalPlaceMatcher.Normalise(provinceOrState);
            SaPostalCode best = null;
            int bestScore = 0;

            foreach (var p in places)
            {
                int score = 0;
                string pn = PostalPlaceMatcher.Normalise(p.PlaceName);
                if (!string.IsNullOrEmpty(sub) && pn.Length >= 3)
                {
                    if (sub == pn || sub.Contains(pn) || pn.Contains(sub))
                        score += pn.Length + 10;
                }
                if (!string.IsNullOrEmpty(prov) && !string.IsNullOrWhiteSpace(p.Province))
                {
                    string pv = PostalPlaceMatcher.Normalise(p.Province);
                    if (prov == pv || pv.Contains(prov) || prov.Contains(pv))
                        score += 5;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    best = p;
                }
            }
            return best ?? places[0];
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return null;
            foreach (string v in values)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }
            return null;
        }

        public int? ResolveDeliveryPersonForShippingMethod(string methodTitle)
        {
            if (string.IsNullOrWhiteSpace(methodTitle))
                return null;
            string probe = methodTitle.Trim();
            foreach (var m in _shipRepo.GetAllOrdered(includeInactive: false))
            {
                if (m == null || string.IsNullOrWhiteSpace(m.MethodMatch))
                    continue;
                if (probe.IndexOf(m.MethodMatch.Trim(), StringComparison.OrdinalIgnoreCase) >= 0
                    || m.MethodMatch.Trim().IndexOf(probe, StringComparison.OrdinalIgnoreCase) >= 0)
                    return m.ToBeDeliveredByID;
            }
            return null;
        }

        public static int GetSystemDefaultDeliveryPersonId()
        {
            return SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;
        }

        public int ResolveDeliveryPersonForArea(int areaId)
        {
            int? personId = _areaDefaultRepo.GetDefaultPersonForArea(areaId);
            if (personId.HasValue && personId.Value > 0)
                return personId.Value;
            return GetSystemDefaultDeliveryPersonId();
        }

        /// <summary>
        /// Parse Woo-style ranges: 7806 or 7800...7806, separated by ; or newline.
        /// Spaces around the ellipsis are optional. Also accepts two dots (7800..7806)
        /// and a Unicode ellipsis.
        /// </summary>
        public static List<PostalRange> ParsePostalRanges(string text)
        {
            var list = new List<PostalRange>();
            if (string.IsNullOrWhiteSpace(text))
                return list;

            foreach (string part in text.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string seg = part.Trim();
                if (seg.Length == 0)
                    continue;

                int skip;
                int dots = FindRangeSeparator(seg, out skip);
                if (dots >= 0)
                {
                    int from = ParsePostalCode(seg.Substring(0, dots));
                    int to = ParsePostalCode(seg.Substring(dots + skip));
                    if (from <= 0 || to <= 0)
                        continue;
                    if (from > to)
                    {
                        int swap = from;
                        from = to;
                        to = swap;
                    }
                    list.Add(new PostalRange { From = from, To = to });
                }
                else
                {
                    int single = ParsePostalCode(seg);
                    if (single > 0)
                        list.Add(new PostalRange { From = single, To = single });
                }
            }
            return list;
        }

        /// <summary>Prefer three dots, then Unicode ellipsis, then two dots (so ... is not split as ..).</summary>
        private static int FindRangeSeparator(string seg, out int skip)
        {
            int i = seg.IndexOf("...", StringComparison.Ordinal);
            if (i >= 0)
            {
                skip = 3;
                return i;
            }
            i = seg.IndexOf("…", StringComparison.Ordinal);
            if (i >= 0)
            {
                skip = 1;
                return i;
            }
            i = seg.IndexOf("..", StringComparison.Ordinal);
            if (i >= 0)
            {
                skip = 2;
                return i;
            }
            skip = 0;
            return -1;
        }

        public static string FormatRange(PostalRange range)
        {
            if (range.From == range.To)
                return range.From.ToString("0000", CultureInfo.InvariantCulture);
            return range.From.ToString("0000", CultureInfo.InvariantCulture)
                + "..."
                + range.To.ToString("0000", CultureInfo.InvariantCulture);
        }

        private WooAreaResolveResult FallbackDefault(string reason)
        {
            int? areaId = GetDefaultImportAreaId();
            if (!areaId.HasValue || areaId.Value <= 0)
            {
                return new WooAreaResolveResult
                {
                    Reason = reason + " — set a catch-all area on Postal Area Setup"
                };
            }
            var area = _areasRepo.GetById(areaId.Value);
            return BuildResult(areaId.Value, area?.AreaName, true, reason + " — using default area");
        }

        private WooAreaResolveResult BuildResult(int areaId, string areaName, bool usedDefault, string reason)
        {
            int personId = ResolveDeliveryPersonForArea(areaId);
            return new WooAreaResolveResult
            {
                AreaID = areaId,
                AreaName = areaName,
                DefaultPreferredAgentID = personId,
                UsedDefaultArea = usedDefault,
                Reason = reason
            };
        }

        public static int ParsePostalCode(string postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                return 0;
            var digits = Regex.Replace(postalCode.Trim(), @"[^\d]", string.Empty);
            if (digits.Length < 4)
                return 0;
            if (digits.Length > 4)
                digits = digits.Substring(0, 4);
            int code;
            return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out code) ? code : 0;
        }

        /// <summary>One-time merge from legacy WooPostalAreaMapTbl if present and new column empty.</summary>
        private void MigrateLegacyPostalMapsIfNeeded(List<WooAreaDeliveryDefault> areas)
        {
            if (areas == null || areas.Count == 0)
                return;
            if (areas.Any(a => !string.IsNullOrWhiteSpace(a.PostalRanges)))
                return;

            var legacyRepo = new WooPostalAreaMapRepository();
            List<WooPostalAreaMap> maps;
            try
            {
                maps = legacyRepo.GetAllOrdered(includeInactive: false);
            }
            catch
            {
                return;
            }
            if (maps == null || maps.Count == 0)
                return;

            foreach (var group in maps.GroupBy(m => m.AreaID))
            {
                var area = areas.FirstOrDefault(a => a.AreaID == group.Key);
                if (area == null)
                    continue;
                var parts = group
                    .OrderBy(m => m.PostalFrom)
                    .ThenBy(m => m.PostalTo)
                    .Select(m => m.PostalFrom == m.PostalTo
                        ? m.PostalFrom.ToString("0000", CultureInfo.InvariantCulture)
                        : m.PostalFrom.ToString("0000", CultureInfo.InvariantCulture)
                          + "..."
                          + m.PostalTo.ToString("0000", CultureInfo.InvariantCulture));
                area.PostalRanges = string.Join(";", parts);
            }
        }

        private sealed class AreaRangeMatch
        {
            public WooAreaDeliveryDefault Area { get; set; }
            public PostalRange Range { get; set; }
        }
    }

    public struct PostalRange
    {
        public int From;
        public int To;
    }
}
