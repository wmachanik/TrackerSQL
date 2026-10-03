using System;
using System.Collections.Generic;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Road driving times between route points for <see cref="RoutePlanner"/>: from memory, then RoadLegTbl, and only for
    /// pairs not known yet one HeiGIT matrix call for the whole set (counted against <see cref="GpsSettingsRepository.RoadMatrixDailyLimit"/>).
    /// When HeiGIT can't be used (no key, switched off, limit reached, error) the route falls back to straight lines; after an
    /// error no call is made for a while so pages and the driver app are never held up twice.
    /// </summary>
    public static class RoadTimesManager
    {
        private const int PauseAfterErrorMinutes = 15;
        private const int PauseAfterMinuteLimitMinutes = 2;
        /// <summary>The same set of points is not fetched again within this time (e.g. when saving the legs failed).</summary>
        private const int RetrySameSetMinutes = 30;
        private const int MaxMemoryLegs = 200000;
        /// <summary>For a pair with no road: straight line × 1.3 at 30 km/h, in seconds.</summary>
        private const double NoRoadSecondsPerMetre = 1.3 / (30000.0 / 3600.0);

        private static readonly object Sync = new object();
        private static readonly object FetchSync = new object();
        private static readonly Dictionary<string, int> Memory = new Dictionary<string, int>(StringComparer.Ordinal);
        private static readonly Dictionary<string, DateTime> RecentSets = new Dictionary<string, DateTime>(StringComparer.Ordinal);
        private static DateTime _pausedUntil = DateTime.MinValue;

        /// <summary>
        /// The road time source for one sheet (settings read once), or null when road times are off, the HeiGIT key is
        /// missing or the DriverGps-06 pack has not run.
        /// </summary>
        public static Func<IList<string>, double[,]> ForRouting()
        {
            try
            {
                GpsSettings settings = new GpsSettingsRepository().Get();
                if (!settings.RoadTimesAvailable || !RoadLegRepository.TableSeen())
                    return null;
                return hashes => Seconds(hashes, settings);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Road times: settings not read: " + ex.Message);
                return null;
            }
        }

        /// <summary>When road times are paused after an error or the daily limit (null = not paused).</summary>
        public static DateTime? PausedUntil()
        {
            lock (Sync)
                return _pausedUntil > DateTime.Now ? _pausedUntil : (DateTime?)null;
        }

        /// <summary>[from, to] driving seconds between the points, or null when they can't all be had.</summary>
        public static double[,] Seconds(IList<string> hashes, GpsSettings settings)
        {
            try
            {
                return Lookup(hashes, settings);
            }
            catch (Exception ex)
            {
                Pause(DateTime.Now.AddMinutes(PauseAfterErrorMinutes), ex.GetType().Name + ": " + ex.Message);
                return null;
            }
        }

        private static double[,] Lookup(IList<string> hashes, GpsSettings settings)
        {
            int n = hashes?.Count ?? 0;
            if (n < 2 || hashes.Any(h => !RoadLegRepository.IsValidHash(h)))
                return null;

            if (TryFill(hashes, out double[,] matrix))
                return matrix;

            List<RoadLegRepository.Leg> stored;
            try
            {
                stored = new RoadLegRepository().Between(hashes);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Road times: stored legs not read: " + ex.Message);
                return null;
            }
            Remember(stored);
            if (TryFill(hashes, out matrix))
                return matrix;

            lock (FetchSync)
            {
                if (TryFill(hashes, out matrix))
                    return matrix;
                return Fetch(hashes, settings) && TryFill(hashes, out matrix) ? matrix : null;
            }
        }

        private static bool Fetch(IList<string> hashes, GpsSettings settings)
        {
            List<string> points = hashes.Distinct().ToList();
            if (points.Count > HeigitMatrixClient.MaxLocations || PausedUntil().HasValue)
                return false;
            string set = string.Join(",", points.OrderBy(h => h, StringComparer.Ordinal));
            lock (Sync)
            {
                if (RecentSets.TryGetValue(set, out DateTime at) && at > DateTime.Now.AddMinutes(-RetrySameSetMinutes))
                    return false;
                foreach (string old in RecentSets.Where(kv => kv.Value <= DateTime.Now.AddMinutes(-RetrySameSetMinutes)).Select(kv => kv.Key).ToList())
                    RecentSets.Remove(old);
                RecentSets[set] = DateTime.Now;
            }

            var repo = new GpsSettingsRepository();
            if (!repo.TryCountCall(GpsSettingsRepository.RoadMatrixService, GpsSettingsRepository.RoadMatrixDailyLimit))
            {
                Pause(DateTime.Today.AddDays(1), "today's " + GpsSettingsRepository.RoadMatrixDailyLimit + " road time calls are used up");
                return false;
            }

            var coords = new List<Tuple<double, double>>(points.Count);
            foreach (string hash in points)
            {
                Geohash.TryDecode(hash, out double lat, out double lng);
                coords.Add(Tuple.Create(lat, lng));
            }
            HeigitMatrixClient.Result result = new HeigitMatrixClient(settings.GeocodeBaseUrl, settings.GeocodeApiKey).Fetch(coords);
            if (!result.Ok)
            {
                switch (result.Failure)
                {
                    case HeigitMatrixClient.Failure.DailyLimitReached:
                        repo.MarkDailyLimitReached(GpsSettingsRepository.RoadMatrixService, GpsSettingsRepository.RoadMatrixDailyLimit);
                        Pause(DateTime.Today.AddDays(1), result.Message);
                        break;
                    case HeigitMatrixClient.Failure.PerMinuteLimitReached:
                        Pause(DateTime.Now.AddMinutes(PauseAfterMinuteLimitMinutes), result.Message);
                        break;
                    default:
                        Pause(DateTime.Now.AddMinutes(PauseAfterErrorMinutes), result.Message);
                        break;
                }
                return false;
            }

            var legs = new List<RoadLegRepository.Leg>(points.Count * points.Count);
            for (int a = 0; a < points.Count; a++)
            {
                for (int b = 0; b < points.Count; b++)
                {
                    if (a != b)
                        legs.Add(new RoadLegRepository.Leg { FromHash = points[a], ToHash = points[b], Seconds = result.Seconds[a, b], Metres = result.Metres[a, b] });
                }
            }
            Remember(legs);
            try
            {
                new RoadLegRepository().Save(legs);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Road times: legs not saved (kept in memory): " + ex.Message);
            }
            int noRoad = legs.Count(l => l.Seconds < 0);
            AppLogger.WriteLog(SystemConstants.LogTypes.Delivery,
                "Road times: HeiGIT matrix for " + points.Count + " points (" + legs.Count + " legs"
                + (noRoad > 0 ? ", " + noRoad + " with no road" : string.Empty) + "), call "
                + repo.GetCallsToday(GpsSettingsRepository.RoadMatrixService) + " of " + GpsSettingsRepository.RoadMatrixDailyLimit + " today");
            return true;
        }

        private static void Pause(DateTime until, string reason)
        {
            lock (Sync)
                _pausedUntil = until;
            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "Road times paused until " + until.ToString("yyyy-MM-dd HH:mm") + " (straight lines meanwhile): " + reason);
        }

        private static void Remember(IEnumerable<RoadLegRepository.Leg> legs)
        {
            lock (Sync)
            {
                if (Memory.Count > MaxMemoryLegs)
                    Memory.Clear();
                foreach (RoadLegRepository.Leg leg in legs)
                    Memory[leg.FromHash + "|" + leg.ToHash] = leg.Seconds;
            }
        }

        /// <summary>The matrix from memory; false when any pair is not known yet.</summary>
        private static bool TryFill(IList<string> hashes, out double[,] matrix)
        {
            int n = hashes.Count;
            matrix = new double[n, n];
            lock (Sync)
            {
                for (int a = 0; a < n; a++)
                {
                    for (int b = 0; b < n; b++)
                    {
                        if (a == b || hashes[a] == hashes[b])
                            continue;
                        if (!Memory.TryGetValue(hashes[a] + "|" + hashes[b], out int seconds))
                            return false;
                        matrix[a, b] = seconds >= 0 ? seconds : NoRoadSeconds(hashes[a], hashes[b]);
                    }
                }
            }
            return true;
        }

        private static double NoRoadSeconds(string fromHash, string toHash)
        {
            Geohash.TryDecode(fromHash, out double lat1, out double lng1);
            Geohash.TryDecode(toHash, out double lat2, out double lng2);
            return GeoDistance.Metres(lat1, lng1, lat2, lng2) * NoRoadSecondsPerMetre;
        }
    }
}
