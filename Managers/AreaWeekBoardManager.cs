using System;
using System.Collections.Generic;
using System.Linq;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Builds Lookups → Delivery Days (areas per delivery weekday) and saves the area delivery order.
    /// Delivery weekday of a prep-day row = prep day + delivery delay, wrapping (1=Sun..7=Sat).
    /// An area has one delivery order, held on every one of its prep-day rows (AreaPrepDaysTbl.DeliveryOrder).
    /// </summary>
    public class AreaWeekBoardManager
    {
        public const short OrderStep = 10;

        private static readonly string[] DayNames = { "", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        private static readonly string[] FullDayNames = { "", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        private static readonly int[] BoardDayOrder = { 2, 3, 4, 5, 6, 7, 1 };

        private readonly AreasRepository _areasRepository = new AreasRepository();
        private readonly AreaPrepDaysRepository _prepDaysRepository = new AreaPrepDaysRepository();
        private readonly DeliveryPromiseRuleRepository _promiseRuleRepository = new DeliveryPromiseRuleRepository();

        public static string DayName(int dayOfWeekId)
        {
            return dayOfWeekId >= 1 && dayOfWeekId <= 7 ? DayNames[dayOfWeekId] : "?";
        }

        public static string FullDayName(int dayOfWeekId)
        {
            return dayOfWeekId >= 1 && dayOfWeekId <= 7 ? FullDayNames[dayOfWeekId] : "?";
        }

        /// <summary>Delivery weekday (1=Sun..7=Sat) for a prep day and delay; null when the prep day is missing.</summary>
        public static int? DeliveryDayOfWeek(int? prepDayOfWeekId, int? deliveryDelayDays)
        {
            if (!prepDayOfWeekId.HasValue || prepDayOfWeekId.Value < 1 || prepDayOfWeekId.Value > 7)
                return null;

            int delay = Math.Max(0, deliveryDelayDays ?? 0);
            return ((prepDayOfWeekId.Value - 1 + delay) % 7) + 1;
        }

        public AreaWeekBoard Build()
        {
            var board = new AreaWeekBoard();
            var areas = _areasRepository.GetAll("AreaName");
            var areaNames = areas.ToDictionary(a => a.AreaID, a => a.AreaName ?? string.Empty);
            var prepDays = _prepDaysRepository.GetAll("AreaPrepDaysID")
                .Where(p => areaNames.ContainsKey(p.AreaID) && DeliveryDayOfWeek(p.PrepDayOfWeekID, p.DeliveryDelayDays).HasValue)
                .ToList();
            var promisesByAreaDay = BuildPromiseLookup(areas);

            var areaOrder = AreaOrder(prepDays, areaNames);
            var deliveryOrderByArea = prepDays
                .GroupBy(p => p.AreaID)
                .ToDictionary(g => g.Key, g => g.Min(p => (int?)p.DeliveryOrder));

            var days = BoardDayOrder.ToDictionary(
                dow => dow,
                dow => new AreaWeekDay { DayOfWeekID = dow, DayName = DayName(dow) });
            board.AllAreas = new AreaWeekDay { DayOfWeekID = 0, DayName = "All" };

            foreach (int areaId in areaOrder)
            {
                var areaPreps = prepDays
                    .Where(p => p.AreaID == areaId)
                    .OrderBy(p => Array.IndexOf(BoardDayOrder, DeliveryDayOfWeek(p.PrepDayOfWeekID, p.DeliveryDelayDays).Value))
                    .Select(p => new
                    {
                        DeliveryDow = DeliveryDayOfWeek(p.PrepDayOfWeekID, p.DeliveryDelayDays).Value,
                        Prep = new AreaWeekPrep { PrepDayOfWeekID = p.PrepDayOfWeekID ?? 0, DeliveryDelayDays = p.DeliveryDelayDays ?? 0 }
                    })
                    .ToList();
                var deliveryDays = areaPreps.Select(p => p.DeliveryDow).Distinct().ToList();

                board.AllAreas.Cards.Add(new AreaWeekCard
                {
                    AreaID = areaId,
                    AreaName = areaNames[areaId],
                    DeliveryOrder = deliveryOrderByArea[areaId],
                    Preps = areaPreps.Select(p => p.Prep).ToList(),
                    DeliveryDays = deliveryDays,
                    PromiseText = string.Empty
                });

                foreach (int dow in deliveryDays)
                {
                    var dayPreps = areaPreps.Where(p => p.DeliveryDow == dow).Select(p => p.Prep).ToList();
                    promisesByAreaDay.TryGetValue(Tuple.Create(areaId, dow), out string promise);
                    days[dow].Cards.Add(new AreaWeekCard
                    {
                        AreaID = areaId,
                        AreaName = areaNames[areaId],
                        DeliveryOrder = deliveryOrderByArea[areaId],
                        Preps = dayPreps,
                        DeliveryDays = deliveryDays,
                        DuplicateOnDay = dayPreps.Count > 1,
                        PromiseText = promise ?? string.Empty
                    });
                }
            }

            foreach (var day in days.Values)
            {
                var deliveredAreaIds = new HashSet<int>(day.Cards.Select(c => c.AreaID));
                day.PromiseGaps = promisesByAreaDay
                    .Where(p => p.Key.Item2 == day.DayOfWeekID && !deliveredAreaIds.Contains(p.Key.Item1))
                    .Select(p => new AreaPromiseGap
                    {
                        AreaID = p.Key.Item1,
                        AreaName = areaNames.TryGetValue(p.Key.Item1, out string name) ? name : "Area " + p.Key.Item1,
                        PromiseText = p.Value
                    })
                    .OrderBy(g => g.AreaName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            board.Days = BoardDayOrder.Select(dow => days[dow]).ToList();

            var areasWithDays = new HashSet<int>(prepDays
                .Where(p => DeliveryDayOfWeek(p.PrepDayOfWeekID, p.DeliveryDelayDays).HasValue)
                .Select(p => p.AreaID));
            board.Unassigned = areas.Where(a => !areasWithDays.Contains(a.AreaID)).ToList();

            return board;
        }

        /// <summary>Area delivery order (lowest DeliveryOrder of the area's prep rows) by AreaID.</summary>
        public Dictionary<int, int?> GetAreaDeliveryOrders()
        {
            return _prepDaysRepository.GetAll(null)
                .GroupBy(p => p.AreaID)
                .ToDictionary(g => g.Key, g => g.Min(p => (int?)p.DeliveryOrder));
        }

        /// <summary>
        /// Saves a new order for the areas of one delivery day (0 = all areas). The day's areas swap places among
        /// themselves in the overall area order; then every area is renumbered 10, 20, 30 ... on all of its prep rows.
        /// Returns the number of the day's areas saved.
        /// </summary>
        public int SaveDayOrder(int dayOfWeekId, IList<int> orderedAreaIds)
        {
            if (dayOfWeekId < 0 || dayOfWeekId > 7 || orderedAreaIds == null || orderedAreaIds.Count == 0)
                return 0;

            var areaNames = _areasRepository.GetAll("AreaName").ToDictionary(a => a.AreaID, a => a.AreaName ?? string.Empty);
            var prepDays = _prepDaysRepository.GetAll(null)
                .Where(p => areaNames.ContainsKey(p.AreaID) && DeliveryDayOfWeek(p.PrepDayOfWeekID, p.DeliveryDelayDays).HasValue)
                .ToList();

            var overall = AreaOrder(prepDays, areaNames);
            var onThisDay = new HashSet<int>(prepDays
                .Where(p => dayOfWeekId == 0 || DeliveryDayOfWeek(p.PrepDayOfWeekID, p.DeliveryDelayDays) == dayOfWeekId)
                .Select(p => p.AreaID));

            var dayOrder = orderedAreaIds.Where(onThisDay.Contains).Distinct().ToList();
            if (dayOrder.Count == 0)
                return 0;
            dayOrder.AddRange(overall.Where(id => onThisDay.Contains(id) && !dayOrder.Contains(id)));

            int next = 0;
            for (int i = 0; i < overall.Count; i++)
            {
                if (onThisDay.Contains(overall[i]))
                    overall[i] = dayOrder[next++];
            }

            var newOrders = new List<KeyValuePair<int, short>>();
            for (int i = 0; i < overall.Count; i++)
            {
                short order = (short)((i + 1) * OrderStep);
                int areaId = overall[i];
                if (prepDays.Any(p => p.AreaID == areaId && p.DeliveryOrder != order))
                    newOrders.Add(new KeyValuePair<int, short>(areaId, order));
            }

            _prepDaysRepository.UpdateAreaDeliveryOrders(newOrders);
            return dayOrder.Count;
        }

        /// <summary>AreaIDs with a delivery day, by area delivery order then name.</summary>
        private static List<int> AreaOrder(IEnumerable<AreaPrepDays> prepDays, IDictionary<int, string> areaNames)
        {
            return prepDays
                .GroupBy(p => p.AreaID)
                .Select(g => new { AreaID = g.Key, Order = g.Min(p => (int?)p.DeliveryOrder) ?? int.MaxValue })
                .OrderBy(a => a.Order)
                .ThenBy(a => areaNames[a.AreaID], StringComparer.OrdinalIgnoreCase)
                .Select(a => a.AreaID)
                .ToList();
        }

        /// <summary>
        /// (AreaID, delivery weekday 1..7) → promise description, from enabled delivery promise rules.
        /// Fixed-day delivery rules land on their result day; courier (dispatch) rules are not day-specific.
        /// </summary>
        private Dictionary<Tuple<int, int>, string> BuildPromiseLookup(List<Area> areas)
        {
            var lookup = new Dictionary<Tuple<int, int>, string>();
            var areaIdsByName = areas
                .Where(a => !string.IsNullOrWhiteSpace(a.AreaName))
                .GroupBy(a => a.AreaName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().AreaID, StringComparer.OrdinalIgnoreCase);

            foreach (var rule in _promiseRuleRepository.GetAll(includeDisabled: false))
            {
                if (!string.Equals(rule.ResultMode, "FixedDow", StringComparison.OrdinalIgnoreCase) || !rule.ResultDow.HasValue || rule.ResultDow.Value > 6)
                    continue;

                int areaId = rule.AreaID ?? 0;
                if (areaId <= 0 && !string.IsNullOrWhiteSpace(rule.AreaMatchName))
                    areaIdsByName.TryGetValue(rule.AreaMatchName.Trim(), out areaId);
                if (areaId <= 0)
                    continue;

                int deliveryDow = rule.ResultDow.Value + 1;
                string text = "orders " + PromiseWindowPoint(rule.WindowStartDow, rule.WindowStartMinutes)
                    + " – " + PromiseWindowPoint(rule.WindowEndDow, rule.WindowEndMinutes);
                var key = Tuple.Create(areaId, deliveryDow);
                lookup[key] = lookup.TryGetValue(key, out string existing) ? existing + "; " + text : text;
            }

            return lookup;
        }

        /// <summary>Promise rules use 0=Sun..6=Sat; 255 = any workday.</summary>
        private static string PromiseDowName(byte promiseDow)
        {
            return promiseDow <= 6 ? DayNames[promiseDow + 1] : "any";
        }

        private static string PromiseWindowPoint(byte promiseDow, short minutes)
        {
            int clamped = Math.Max(0, Math.Min(24 * 60, (int)minutes));
            return PromiseDowName(promiseDow) + " " + (clamped / 60).ToString("00") + ":" + (clamped % 60).ToString("00");
        }
    }
}
