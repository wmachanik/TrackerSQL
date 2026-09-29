using System;
using System.Collections.Generic;
using System.Linq;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Builds the Areas week board and saves the per-day delivery order.
    /// Delivery weekday of a prep-day row = prep day + delivery delay, wrapping (1=Sun..7=Sat).
    /// </summary>
    public class AreaWeekBoardManager
    {
        public const short OrderStep = 10;

        private static readonly string[] DayNames = { "", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        private static readonly int[] BoardDayOrder = { 2, 3, 4, 5, 6, 7, 1 };

        private readonly AreasRepository _areasRepository = new AreasRepository();
        private readonly AreaPrepDaysRepository _prepDaysRepository = new AreaPrepDaysRepository();
        private readonly DeliveryPromiseRuleRepository _promiseRuleRepository = new DeliveryPromiseRuleRepository();

        public static string DayName(int dayOfWeekId)
        {
            return dayOfWeekId >= 1 && dayOfWeekId <= 7 ? DayNames[dayOfWeekId] : "?";
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
            var prepDays = _prepDaysRepository.GetAll("DeliveryOrder, AreaPrepDaysID");
            var promisesByAreaDay = BuildPromiseLookup(areas);

            var days = BoardDayOrder.ToDictionary(
                dow => dow,
                dow => new AreaWeekDay { DayOfWeekID = dow, DayName = DayName(dow) });

            foreach (var prepDay in prepDays)
            {
                int? deliveryDow = DeliveryDayOfWeek(prepDay.PrepDayOfWeekID, prepDay.DeliveryDelayDays);
                if (!deliveryDow.HasValue || !areaNames.ContainsKey(prepDay.AreaID))
                    continue;

                promisesByAreaDay.TryGetValue(Tuple.Create(prepDay.AreaID, deliveryDow.Value), out string promise);
                days[deliveryDow.Value].Cards.Add(new AreaWeekCard
                {
                    AreaPrepDaysID = prepDay.AreaPrepDaysID,
                    AreaID = prepDay.AreaID,
                    AreaName = areaNames[prepDay.AreaID],
                    PrepDayOfWeekID = prepDay.PrepDayOfWeekID ?? 0,
                    DeliveryDelayDays = prepDay.DeliveryDelayDays ?? 0,
                    DeliveryOrder = prepDay.DeliveryOrder,
                    PromiseText = promise ?? string.Empty
                });
            }

            foreach (var day in days.Values)
            {
                day.Cards = day.Cards
                    .OrderBy(c => c.DeliveryOrder ?? short.MaxValue)
                    .ThenBy(c => c.AreaName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var tie in day.Cards.Where(c => c.DeliveryOrder.HasValue).GroupBy(c => c.DeliveryOrder).Where(g => g.Count() > 1))
                    foreach (var card in tie)
                        card.OrderTied = true;

                foreach (var duplicate in day.Cards.GroupBy(c => c.AreaID).Where(g => g.Count() > 1))
                    foreach (var card in duplicate)
                        card.DuplicateOnDay = true;

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

        /// <summary>
        /// Saves the order of one delivery day: the listed prep-day rows get 10, 20, 30 ...
        /// Rows that do not deliver on that day are ignored. Returns the number of rows saved.
        /// </summary>
        public int SaveDayOrder(int dayOfWeekId, IList<int> orderedAreaPrepDaysIds)
        {
            if (dayOfWeekId < 1 || dayOfWeekId > 7 || orderedAreaPrepDaysIds == null || orderedAreaPrepDaysIds.Count == 0)
                return 0;

            var onThisDay = new HashSet<int>(_prepDaysRepository.GetAll(null)
                .Where(p => DeliveryDayOfWeek(p.PrepDayOfWeekID, p.DeliveryDelayDays) == dayOfWeekId)
                .Select(p => p.AreaPrepDaysID));

            var ordered = orderedAreaPrepDaysIds.Where(onThisDay.Contains).Distinct().ToList();
            var newOrders = new List<KeyValuePair<int, short>>();
            for (int i = 0; i < ordered.Count; i++)
                newOrders.Add(new KeyValuePair<int, short>(ordered[i], (short)((i + 1) * OrderStep)));

            return _prepDaysRepository.UpdateDeliveryOrders(newOrders);
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
                string text = "ordered " + PromiseDowName(rule.WindowStartDow) + "–" + PromiseDowName(rule.WindowEndDow);
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
    }
}
