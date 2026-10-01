using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackerSQL.Classes.Geo
{
    /// <summary>
    /// Puts a driver's stops in driving order without a routing service: from the start (the depot) always go to the
    /// nearest stop not visited yet, then undo any crossings (2-opt) in the open route. Stops are positioned by their
    /// geohash; ties keep the incoming (geohash) order. Stops without a position keep their order at the end.
    /// <see cref="ApplyLearned"/> then moves stops towards where drivers really deliver them (DeliveryVisitTbl).
    /// </summary>
    public static class RoutePlanner
    {
        private const int MaxImprovementPasses = 50;

        /// <summary>Reorders <paramref name="stops"/>; <paramref name="geohashOf"/> gives the stop's geohash (null/empty = no position).</summary>
        public static List<T> Order<T>(IList<T> stops, Func<T, string> geohashOf, double? startLatitude, double? startLongitude)
        {
            var located = new List<int>();
            var lat = new List<double>();
            var lng = new List<double>();
            var unlocated = new List<int>();
            for (int i = 0; i < stops.Count; i++)
            {
                if (Geohash.TryDecode(geohashOf(stops[i]), out double la, out double ln))
                {
                    located.Add(i);
                    lat.Add(la);
                    lng.Add(ln);
                }
                else
                {
                    unlocated.Add(i);
                }
            }
            if (located.Count < 2)
                return located.Concat(unlocated).Select(i => stops[i]).ToList();

            bool hasStart = startLatitude.HasValue && startLongitude.HasValue;
            int n = located.Count;
            // Node 0 is the start (the depot, or the first stop in geohash order when there is no depot point).
            int nodes = n + 1;
            var dist = new double[nodes, nodes];
            double startLat = hasStart ? startLatitude.Value : lat[0];
            double startLng = hasStart ? startLongitude.Value : lng[0];
            for (int a = 0; a < nodes; a++)
            {
                for (int b = a + 1; b < nodes; b++)
                {
                    double d = GeoDistance.Metres(
                        a == 0 ? startLat : lat[a - 1], a == 0 ? startLng : lng[a - 1],
                        b == 0 ? startLat : lat[b - 1], b == 0 ? startLng : lng[b - 1]);
                    dist[a, b] = d;
                    dist[b, a] = d;
                }
            }

            var path = new List<int>(n);
            var visited = new bool[nodes];
            int current = 0;
            for (int step = 0; step < n; step++)
            {
                int best = -1;
                for (int c = 1; c < nodes; c++)
                {
                    if (!visited[c] && (best < 0 || dist[current, c] < dist[current, best]))
                        best = c;
                }
                visited[best] = true;
                path.Add(best);
                current = best;
            }

            for (int pass = 0; pass < MaxImprovementPasses; pass++)
            {
                bool improved = false;
                for (int i = 0; i < n - 1; i++)
                {
                    int before = i == 0 ? 0 : path[i - 1];
                    for (int j = i + 1; j < n; j++)
                    {
                        double removed = dist[before, path[i]] + (j + 1 < n ? dist[path[j], path[j + 1]] : 0);
                        double added = dist[before, path[j]] + (j + 1 < n ? dist[path[i], path[j + 1]] : 0);
                        if (added + 0.5 < removed)
                        {
                            path.Reverse(i, j - i + 1);
                            improved = true;
                        }
                    }
                }
                if (!improved)
                    break;
            }

            return path.Select(node => stops[located[node - 1]])
                .Concat(unlocated.Select(i => stops[i]))
                .ToList();
        }

        /// <summary>
        /// Puts the driver's own habits first: each stop's place in <paramref name="route"/> (0 = first .. 1 = last) is blended
        /// with where the driver really delivered it (<paramref name="learnedOf"/>: average place 0..1 and how many visits),
        /// weighting the learned place by its visits (1 visit = half, 4 visits = 80%). Stops never delivered with the app keep
        /// their route place. A contact's stops share the place of its first one so they stay together.
        /// </summary>
        public static List<T> ApplyLearned<T>(IList<T> route, Func<T, int> contactOf, Func<T, Tuple<double, int>> learnedOf)
        {
            int n = route.Count;
            if (n < 2)
                return route.ToList();

            var firstIndex = new Dictionary<int, int>();
            var keyed = new List<Tuple<T, int, double>>(n);
            for (int i = 0; i < n; i++)
            {
                int contact = contactOf(route[i]);
                if (!firstIndex.TryGetValue(contact, out int first))
                    firstIndex[contact] = first = i;
                double place = (double)first / (n - 1);
                Tuple<double, int> learned = learnedOf(route[i]);
                if (learned != null && learned.Item2 > 0)
                    place = (learned.Item2 * Math.Max(0, Math.Min(1, learned.Item1)) + place) / (learned.Item2 + 1);
                keyed.Add(Tuple.Create(route[i], i, place));
            }
            return keyed.OrderBy(k => k.Item3).ThenBy(k => k.Item2).Select(k => k.Item1).ToList();
        }

        /// <summary>
        /// The order the driver set in the app comes first: stops in the plan (<paramref name="planOf"/>: place 1..stops) go
        /// in that order; stops added after the plan was made keep their place in <paramref name="route"/> between them.
        /// </summary>
        public static List<T> ApplyPlan<T>(IList<T> route, Func<T, Tuple<int, int>> planOf)
        {
            int n = route.Count;
            if (n < 2)
                return route.ToList();

            var keyed = new List<Tuple<T, int, double>>(n);
            for (int i = 0; i < n; i++)
            {
                Tuple<int, int> plan = planOf(route[i]);
                double place = plan != null
                    ? (plan.Item2 > 1 ? (double)(plan.Item1 - 1) / (plan.Item2 - 1) : 0)
                    : (double)i / (n - 1);
                keyed.Add(Tuple.Create(route[i], i, place));
            }
            return keyed.OrderBy(k => k.Item3).ThenBy(k => k.Item2).Select(k => k.Item1).ToList();
        }
    }
}
