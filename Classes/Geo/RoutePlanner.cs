using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackerSQL.Classes.Geo
{
    /// <summary>
    /// Puts a driver's stops in the quickest driving order from the start (the depot), as an open route (no return).
    /// Costs are road driving seconds when a road time source is given and has every pair (HeiGIT, see RoadTimesManager),
    /// else straight-line metres. Up to <see cref="ExactUpTo"/> points the best order is found exactly (Held-Karp); above
    /// that: nearest stop not visited yet, then 2-opt (reverse a stretch) and Or-opt (move 1-3 stops) until nothing improves.
    /// Stops are positioned by their geohash; stops at the same point stay together; ties keep the incoming (geohash) order.
    /// Stops without a position keep their order at the end.
    /// <see cref="ApplyLearned"/> then moves stops towards where drivers really deliver them (DeliveryVisitTbl).
    /// </summary>
    public static class RoutePlanner
    {
        public const int ExactUpTo = 13;
        private const int MaxImprovementPasses = 50;
        private const double MinGain = 0.5;

        /// <summary>
        /// Reorders <paramref name="stops"/>; <paramref name="geohashOf"/> gives the stop's geohash (null/empty = no position).
        /// <paramref name="roadSeconds"/> (optional) gives [from, to] driving seconds for a list of geohashes (the first is the
        /// start), or null when it can't.
        /// </summary>
        public static List<T> Order<T>(IList<T> stops, Func<T, string> geohashOf, double? startLatitude, double? startLongitude,
            Func<IList<string>, double[,]> roadSeconds = null)
        {
            var hashes = new List<string>();
            var lat = new List<double>();
            var lng = new List<double>();
            var members = new List<List<int>>();
            var pointOf = new Dictionary<string, int>(StringComparer.Ordinal);
            var unlocated = new List<int>();
            for (int i = 0; i < stops.Count; i++)
            {
                string hash = (geohashOf(stops[i]) ?? string.Empty).Trim().ToLowerInvariant();
                if (!Geohash.TryDecode(hash, out double la, out double ln))
                {
                    unlocated.Add(i);
                    continue;
                }
                if (pointOf.TryGetValue(hash, out int point))
                {
                    members[point].Add(i);
                    continue;
                }
                pointOf[hash] = hashes.Count;
                hashes.Add(hash);
                lat.Add(la);
                lng.Add(ln);
                members.Add(new List<int> { i });
            }

            int m = hashes.Count;
            List<int> path;
            if (m < 2)
            {
                path = Enumerable.Range(1, m).ToList();
            }
            else
            {
                bool hasStart = startLatitude.HasValue && startLongitude.HasValue;
                double startLat = hasStart ? startLatitude.Value : lat[0];
                double startLng = hasStart ? startLongitude.Value : lng[0];
                double[,] cost = null;
                if (roadSeconds != null)
                {
                    var points = new List<string>(m + 1) { hasStart ? Geohash.Encode(startLat, startLng, 9) : hashes[0] };
                    points.AddRange(hashes);
                    try
                    {
                        cost = roadSeconds(points);
                    }
                    catch (Exception)
                    {
                        cost = null;
                    }
                    if (cost != null && (cost.GetLength(0) != m + 1 || cost.GetLength(1) != m + 1))
                        cost = null;
                }
                if (cost == null)
                    cost = StraightLine(startLat, startLng, lat, lng);
                path = Solve(cost, m);
            }

            return path.SelectMany(node => members[node - 1])
                .Concat(unlocated)
                .Select(i => stops[i])
                .ToList();
        }

        /// <summary>Straight-line metres; node 0 is the start, node k is point k - 1.</summary>
        private static double[,] StraightLine(double startLat, double startLng, IList<double> lat, IList<double> lng)
        {
            int nodes = lat.Count + 1;
            var dist = new double[nodes, nodes];
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
            return dist;
        }

        /// <summary>The visiting order of nodes 1..<paramref name="m"/> from node 0 with the lowest total cost (open route).</summary>
        public static List<int> Solve(double[,] cost, int m)
        {
            if (m <= 1)
                return Enumerable.Range(1, m).ToList();
            if (m <= ExactUpTo)
                return Exact(cost, m);
            List<int> path = Nearest(cost, m);
            Improve(cost, path);
            return path;
        }

        /// <summary>Total cost of the open route from node 0 along <paramref name="path"/>.</summary>
        public static double PathCost(double[,] cost, IList<int> path)
        {
            double total = 0;
            int previous = 0;
            foreach (int node in path)
            {
                total += cost[previous, node];
                previous = node;
            }
            return total;
        }

        /// <summary>Held-Karp: the best open route over every subset of points (2^m × m² steps; 13 points ≈ 1.4 million).</summary>
        private static List<int> Exact(double[,] cost, int m)
        {
            int full = 1 << m;
            var best = new double[full, m];
            var from = new int[full, m];
            for (int mask = 0; mask < full; mask++)
            {
                for (int j = 0; j < m; j++)
                    best[mask, j] = double.PositiveInfinity;
            }
            for (int j = 0; j < m; j++)
            {
                best[1 << j, j] = cost[0, j + 1];
                from[1 << j, j] = -1;
            }
            for (int mask = 1; mask < full; mask++)
            {
                for (int j = 0; j < m; j++)
                {
                    if ((mask & (1 << j)) == 0)
                        continue;
                    double here = best[mask, j];
                    if (double.IsPositiveInfinity(here))
                        continue;
                    for (int k = 0; k < m; k++)
                    {
                        if ((mask & (1 << k)) != 0)
                            continue;
                        int next = mask | (1 << k);
                        double value = here + cost[j + 1, k + 1];
                        if (value < best[next, k] - 1e-9)
                        {
                            best[next, k] = value;
                            from[next, k] = j;
                        }
                    }
                }
            }

            int all = full - 1;
            int last = 0;
            for (int j = 1; j < m; j++)
            {
                if (best[all, j] < best[all, last] - 1e-9)
                    last = j;
            }
            var path = new List<int>(m);
            int at = last, set = all;
            while (at >= 0)
            {
                path.Add(at + 1);
                int previous = from[set, at];
                set &= ~(1 << at);
                at = previous;
            }
            path.Reverse();
            return path;
        }

        private static List<int> Nearest(double[,] cost, int m)
        {
            var path = new List<int>(m);
            var visited = new bool[m + 1];
            int current = 0;
            for (int step = 0; step < m; step++)
            {
                int best = -1;
                for (int c = 1; c <= m; c++)
                {
                    if (!visited[c] && (best < 0 || cost[current, c] < cost[current, best]))
                        best = c;
                }
                visited[best] = true;
                path.Add(best);
                current = best;
            }
            return path;
        }

        /// <summary>2-opt and Or-opt on the whole route cost (road times are not the same both ways).</summary>
        private static void Improve(double[,] cost, List<int> path)
        {
            int n = path.Count;
            double current = PathCost(cost, path);
            for (int pass = 0; pass < MaxImprovementPasses; pass++)
            {
                bool improved = false;

                for (int i = 0; i < n - 1; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        path.Reverse(i, j - i + 1);
                        double candidate = PathCost(cost, path);
                        if (candidate + MinGain < current)
                        {
                            current = candidate;
                            improved = true;
                        }
                        else
                        {
                            path.Reverse(i, j - i + 1);
                        }
                    }
                }

                for (int length = 1; length <= 3 && length < n; length++)
                {
                    for (int i = 0; i + length <= n; i++)
                    {
                        List<int> segment = path.GetRange(i, length);
                        path.RemoveRange(i, length);
                        int bestAt = i;
                        double bestCost = current;
                        for (int at = 0; at <= path.Count; at++)
                        {
                            if (at == i)
                                continue;
                            path.InsertRange(at, segment);
                            double candidate = PathCost(cost, path);
                            path.RemoveRange(at, length);
                            if (candidate + MinGain < bestCost)
                            {
                                bestCost = candidate;
                                bestAt = at;
                            }
                        }
                        path.InsertRange(bestAt, segment);
                        if (bestAt != i)
                        {
                            current = bestCost;
                            improved = true;
                        }
                    }
                }

                if (!improved)
                    break;
            }
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
