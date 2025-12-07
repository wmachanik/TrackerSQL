using System;
using System.Collections.Generic;
using TrackerSQL.Controls;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Calculates optimal delivery dates based on city delivery schedules and recurring order requirements
    /// Follows project standards with MessageProvider logging and proper error handling
    /// </summary>
    public class DateCalculator
    {
        private readonly TrackerTools _trackerTools;
        private readonly CityTblDAL _cityTblDAL;

        public const int WEEKLY_INTERVAL = 1;
        public const int BIWEEKLY_INTERVAL = 2;
        public const int TRI_WEEKLY_INTERVAL = 3;
        // --- Added enum for selection strategy ---
        private enum DeliverySelectionMode
        {
            ClosestToTargetDate,   // Absolute difference to a target recurrence date
            ClosestToToday         // Minimal non-negative distance from today
        }
        public struct PrepDeliveryPair
        {
            public DateTime PrepDate;
            public DateTime DeliveryDate;
            public PrepDeliveryPair(DateTime prep, DateTime delivery)
            {
                PrepDate = prep.Date;
                DeliveryDate = delivery.Date;
            }
        }
        public DateCalculator()
        {
            _trackerTools = new TrackerTools();
            _cityTblDAL = new CityTblDAL();
        }
        private static bool IsDeliveryValidForRules(DateTime deliveryDate, List<CityPrepDaysTbl> rules)
        {
            foreach (var rule in rules)
            {
                var candidatePrep = deliveryDate.AddDays(-rule.DeliveryDelayDays).Date;
                if ((int)candidatePrep.DayOfWeek == rule.PrepDayOfWeekID)
                    return true;
            }
            return false;
        }
        public PrepDeliveryPair CalculateOptimalMonthlyDeliveryDates(long customerId, int targetDayOfMonth, DateTime lastOrderDate)
        {
            try
            {
                var today = TimeZoneUtils.Now().Date;

                // 1. Anchor (recurrence target) – this must remain stable so filtering/window logic still works
                DateTime targetDate = CalculateNextMonthlyOccurrence(targetDayOfMonth, lastOrderDate).Date;
                if (targetDate < today) targetDate = today; // never schedule in past

                // 2. Get city prep rules (if missing, just return target unchanged)
                int cityId = _cityTblDAL.GetCityIdByCustomerId(customerId);
                var rules = (cityId == 0) ? null : _cityTblDAL.GetPrepRulesForCity(cityId);
                if (rules == null || rules.Count == 0)
                {
                    var prepFallback = CalculateRoastDateFromDelivery(targetDate);
                    return new PrepDeliveryPair(prepFallback, targetDate);
                }

                // 3. Build candidate list: target, +1, -1 (if -1 >= today)
                var candidates = new List<DateTime> { targetDate, targetDate.AddDays(1) };
                var backOne = targetDate.AddDays(-1);
                if (backOne >= today) candidates.Add(backOne);

                // 4. Try in priority order; pick first valid
                foreach (var delivery in candidates)
                {
                    if (IsDeliveryValidForRules(delivery, rules))
                    {
                        // derive matching prep
                        foreach (var rule in rules)
                        {
                            var prep = delivery.AddDays(-rule.DeliveryDelayDays).Date;
                            if ((int)prep.DayOfWeek == rule.PrepDayOfWeekID)
                            {
                                if (prep < today) prep = today;
                                return new PrepDeliveryPair(prep, delivery);
                            }
                        }
                    }
                }

                // 5. None matched: return target unmodified (do NOT widen search here to avoid pushing outside window)
                var fallbackPrepDate = CalculateRoastDateFromDelivery(targetDate);
                return new PrepDeliveryPair(fallbackPrepDate, targetDate);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"DateCalculator: Monthly calc failed for customer {customerId}: {ex.Message}");
                var delivery = TimeZoneUtils.Now().Date;
                var prep = CalculateRoastDateFromDelivery(delivery);
                return new PrepDeliveryPair(prep, delivery);
            }
        }
        /// <summary>
        /// Calculates the optimal delivery date for a weekly recurring order:
        /// chooses the delivery date (from city schedule) closest to today (>= today).
        /// The recurrence engine (outside) should have already ensured we are in/near
        /// the appropriate cycle; this method focuses on "closest to now".
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="lastOrderDate">Date of last order</param>
        /// <returns>Optimal delivery date</returns>
        public PrepDeliveryPair CalculateOptimalWeeklyDeliveryDates(long customerId, DateTime calculatedDeliveryDate)
        {
            try
            {
                var anchor = calculatedDeliveryDate.Date;
                var today = TimeZoneUtils.Now().Date;
                if (anchor < today) anchor = today; // safety

                var pair = FindBestCityDeliveryDate(
                    customerId,
                    centerDate: anchor,
                    mode: DeliverySelectionMode.ClosestToTargetDate,
                    referenceDate: anchor,
                    minDeliveryDate: anchor,
                    requireFuturePrep: true);

                return pair;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"DateCalculator: Weekly calc failed for customer {customerId}: {ex.Message}");
                var delivery = TimeZoneUtils.Now().Date.AddDays(1);
                var prep = CalculateRoastDateFromDelivery(delivery);
                return new PrepDeliveryPair(prep, delivery);
            }
        }
        /// <summary>
        /// Calculates the roast date from a delivery date (typically 1-2 days before)
        /// </summary>
        public DateTime CalculateRoastDateFromDelivery(DateTime deliveryDate)
        {
            try
            {
                DateTime roastDate = deliveryDate.AddDays(-1);
                DateTime today = TimeZoneUtils.Now().Date;
                if (roastDate < today)
                    roastDate = today;

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    MessageProvider.Format(MessageKeys.DeliveryCalculation.RoastDateCalculated,
                        deliveryDate, roastDate));

                return roastDate;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    MessageProvider.Format(MessageKeys.DeliveryCalculation.ErrorCalculatingRoastDate,
                        deliveryDate, ex.Message));
                return deliveryDate.AddDays(-1);
            }
        }
        /// <summary>
        /// Calculates the next monthly occurrence.
        /// Rules (revised):
        /// - If lastOrderDate is "first time" (system min) OR older than one month ago,  then IGNORE lastOrderDate and use THIS month's target 
        ///   day (clamped to month length). If that day is already past this month, we clamp forward to today (so we don't schedule in the past).
        /// - Otherwise (normal cycle), take lastOrderDate + 1 month and build the target day. If that computed date is still not in the 
        ///   future (<= today), keep advancing by whole months until we are strictly >= today.
        /// </summary>
        private DateTime CalculateNextMonthlyOccurrence(int targetDayOfMonth, DateTime lastOrderDate)
        {
            DateTime today = TimeZoneUtils.Now().Date;
            bool isFirstTime = lastOrderDate <= SystemConstants.DatabaseConstants.SystemMinDate;
            bool isOlderThanOneMonth = lastOrderDate < today.AddMonths(-1);

            if (isFirstTime || isOlderThanOneMonth)
            {
                int daysInThisMonth = DateTime.DaysInMonth(today.Year, today.Month);
                int day = Math.Min(Math.Max(1, targetDayOfMonth), daysInThisMonth);
                DateTime candidate = new DateTime(today.Year, today.Month, day);
                if (candidate < today)
                    candidate = today;

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    MessageProvider.Format(MessageKeys.DeliveryCalculation.NextOccurrenceCalculated,
                        targetDayOfMonth, candidate));
                return candidate;
            }

            // Base month (one month after last order)
            DateTime cycleMonth = lastOrderDate.AddMonths(1);
            DateTime nextOccurrence = BuildMonthlyTargetDate(cycleMonth, targetDayOfMonth);

            // If the target day in that month is earlier than the day-of-month of the last order
            // (e.g. last=29 Sep, target day=1 → gives 1 Oct only 2 days later),
            // skip ahead one more month to enforce a full-cycle gap (result: 1 Nov).
            if (targetDayOfMonth < lastOrderDate.Day)
            {
                cycleMonth = cycleMonth.AddMonths(1);
                nextOccurrence = BuildMonthlyTargetDate(cycleMonth, targetDayOfMonth);
            }

            // Still ensure we never return a past date relative to today
            while (nextOccurrence < today)
            {
                cycleMonth = cycleMonth.AddMonths(1);
                nextOccurrence = BuildMonthlyTargetDate(cycleMonth, targetDayOfMonth);
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                MessageProvider.Format(MessageKeys.DeliveryCalculation.NextOccurrenceCalculated,
                    targetDayOfMonth, nextOccurrence));

            return nextOccurrence;
        }
        /// <summary>
        /// Helper: builds a target day within the bounds of a given month.
        /// </summary>
        private DateTime BuildMonthlyTargetDate(DateTime anyDayInTargetMonth, int targetDayOfMonth)
        {
            int days = DateTime.DaysInMonth(anyDayInTargetMonth.Year, anyDayInTargetMonth.Month);
            int day = Math.Min(Math.Max(1, targetDayOfMonth), days);
            return new DateTime(anyDayInTargetMonth.Year, anyDayInTargetMonth.Month, day);
        }
        /// <summary>
        /// Finds the optimal delivery date based on customer's city delivery schedule
        /// </summary>
        private DateTime FindOptimalCityDeliveryDate(long customerId, DateTime targetDate)
        {
            try
            {
                // Get customer's city delivery schedule using existing infrastructure
                DateTime dummyDate = DateTime.MinValue;
                DateTime customerRoastDate = _trackerTools.GetNextRoastDateByCustomerID(customerId, ref dummyDate);

                if (customerRoastDate == DateTime.MinValue || customerRoastDate < TimeZoneUtils.Now().Date.AddDays(-30))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, MessageProvider.Format(
                        MessageKeys.DeliveryCalculation.InvalidRoastDate,
                        customerId));

                    return targetDate; // Fallback to target date
                }

                // Find the delivery date closest to our target
                DateTime optimalDate = FindClosestAvailableDeliveryDate(customerRoastDate, targetDate);

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, MessageProvider.Format(
                    MessageKeys.DeliveryCalculation.OptimalCityDateFound,
                    customerRoastDate, optimalDate));

                return optimalDate;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, MessageProvider.Format(
                    MessageKeys.DeliveryCalculation.ErrorFindingCityDate,
                    customerId, ex.Message));

                return targetDate; // Fallback to target date
            }
        }

        /// <summary>
        /// Finds the closest available delivery date to the target date
        /// </summary>
        private DateTime FindClosestAvailableDeliveryDate(DateTime cityDeliveryDate, DateTime targetDate)
        {
            try
            {
                // If city delivery date is within 7 days of target, use it
                double daysDifference = Math.Abs((cityDeliveryDate - targetDate).TotalDays);

                if (daysDifference <= 7)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, MessageProvider.Format(
                        MessageKeys.DeliveryCalculation.UsingCityDeliveryDate,
                        cityDeliveryDate, daysDifference));

                    return cityDeliveryDate;
                }

                // Otherwise, use GetClosestNextRoastDate to find next available date
                DateTime searchDate = targetDate;
                int maxSearchDays = 14; // Search up to 2 weeks ahead

                for (int i = 0; i < maxSearchDays; i++)
                {
                    try
                    {
                        // Use existing TrackerTools method
                        DateTime testRoastDate = _trackerTools.GetClosestNextRoastDate(searchDate);

                        if (testRoastDate > searchDate && testRoastDate != DateTime.MinValue)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, MessageProvider.Format(
                                MessageKeys.DeliveryCalculation.FoundNextDeliveryCycle,
                                testRoastDate, i));

                            return testRoastDate;
                        }
                    }
                    catch (Exception)
                    {
                        // Continue searching if this date fails
                    }

                    searchDate = searchDate.AddDays(1);
                }

                // Final fallback: use target date
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, MessageProvider.Format(
                    MessageKeys.DeliveryCalculation.UsingFallbackDate,
                    targetDate));

                return targetDate;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, MessageProvider.Format(
                    MessageKeys.DeliveryCalculation.ErrorFindingClosestDate,
                    ex.Message));

                return targetDate;
            }
        }
        /// <summary>
        /// New unified search:
        /// Enumerates candidate prep days within ±SEARCH_RADIUS of centerDate.
        /// For each matching prep rule, calculates delivery = prep + delay.
        /// Filters out delivery dates in the past (relative to today).
        /// Selection modes:
        /// - requireFuturePrep: if true, candidate prep must be >= today
        /// - minDeliveryDate: delivery must be >= this (e.g. recurrence anchor)
        /// Selection mode determines metric.
        /// </summary>
        private PrepDeliveryPair FindBestCityDeliveryDate(
            long customerId,
            DateTime centerDate,
            DeliverySelectionMode mode,
            DateTime referenceDate,
            DateTime? minDeliveryDate,
            bool requireFuturePrep)
        {
            int cityId = _cityTblDAL.GetCityIdByCustomerId(customerId);
            var today = TimeZoneUtils.Now().Date;
            centerDate = centerDate.Date;

            if (cityId == 0)
                return new PrepDeliveryPair(centerDate.AddDays(-1) < today ? today : centerDate.AddDays(-1), centerDate);

            var prepRules = _cityTblDAL.GetPrepRulesForCity(cityId);
            if (prepRules == null || prepRules.Count == 0)
                return new PrepDeliveryPair(CalculateRoastDateFromDelivery(centerDate), centerDate);

            var byDow = new Dictionary<int, List<CityPrepDaysTbl>>();
            foreach (var r in prepRules)
            {
                if (!byDow.ContainsKey(r.PrepDayOfWeekID))
                    byDow[r.PrepDayOfWeekID] = new List<CityPrepDaysTbl>();
                byDow[r.PrepDayOfWeekID].Add(r);
            }

            const int SEARCH_RADIUS = 21;
            bool found = false;
            DateTime bestDelivery = DateTime.MaxValue;
            DateTime bestPrep = DateTime.MaxValue;
            double bestMetric = double.MaxValue;
            DateTime minDel = (minDeliveryDate ?? today).Date;

            for (int radius = 0; radius <= SEARCH_RADIUS; radius++)
            {
                int[] offsets = radius == 0 ? new[] { 0 } : new[] { radius, -radius };

                foreach (var offset in offsets)
                {
                    DateTime candidatePrep = centerDate.AddDays(offset).Date;
                    int dow = (int)candidatePrep.DayOfWeek;

                    List<CityPrepDaysTbl> rulesForDay;
                    if (!byDow.TryGetValue(dow, out rulesForDay))
                        continue;

                    foreach (var rule in rulesForDay)
                    {
                        DateTime candidateDelivery = candidatePrep.AddDays(rule.DeliveryDelayDays).Date;

                        // Enforce no past prep (if required)
                        if (requireFuturePrep && candidatePrep < today)
                            continue;

                        // Delivery cannot be in past and must respect minDeliveryDate (anchor)
                        if (candidateDelivery < today) continue;
                        if (candidateDelivery < minDel) continue;

                        double metric;
                        switch (mode)
                        {
                            case DeliverySelectionMode.ClosestToTargetDate:
                                metric = Math.Abs((candidateDelivery - referenceDate).TotalDays);
                                break;
                            case DeliverySelectionMode.ClosestToToday:
                                {
                                    double delta = (candidateDelivery - today).TotalDays;
                                    if (delta < 0) continue;
                                    metric = delta;
                                    break;
                                }
                            default:
                                continue;
                        }

                        if (!found ||
                            metric < bestMetric ||
                            (metric == bestMetric && candidateDelivery < bestDelivery) ||
                            (metric == bestMetric && candidateDelivery == bestDelivery && candidatePrep < bestPrep))
                        {
                            bestMetric = metric;
                            bestDelivery = candidateDelivery;
                            bestPrep = candidatePrep;
                            found = true;

                            if (metric == 0) goto DONE; // perfect match
                        }
                    }
                }
            }

        DONE:
            if (found)
            {
                // Guarantee prep not past today (final safety)
                if (bestPrep < today)
                    bestPrep = today;

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"DateCalculator: Selected delivery {bestDelivery:yyyy-MM-dd} prep {bestPrep:yyyy-MM-dd} (mode={mode}, metric={bestMetric:0.##}) for customer {customerId}");
                return new PrepDeliveryPair(bestPrep, bestDelivery);
            }

            // Fallback
            var fallbackDelivery = centerDate < minDel ? minDel : centerDate;
            if (fallbackDelivery < today) fallbackDelivery = today;
            var fallbackPrep = CalculateRoastDateFromDelivery(fallbackDelivery);
            return new PrepDeliveryPair(fallbackPrep, fallbackDelivery);
        }
        /// <summary>
        /// Fast version: returns the first acceptable delivery date expanding outward:
        /// target (0), +1, -1, +2, -2, ... up to a search radius.
        /// Accepts only delivery dates >= targetDate and >= minDate.
        /// Falls back to targetDate if none found.
        /// </summary>
        private PrepDeliveryPair old_FindClosestCityDeliveryDate(long customerId, DateTime targetDate, DateTime minDate)
        {
            int cityId = _cityTblDAL.GetCityIdByCustomerId(customerId);
            targetDate = targetDate.Date;
            minDate = minDate.Date;

            if (cityId == 0)
                return new PrepDeliveryPair(targetDate.AddDays(-1), targetDate);

            var prepDays = _cityTblDAL.GetPrepRulesForCity(cityId);
            if (prepDays == null || prepDays.Count == 0)
                return new PrepDeliveryPair(targetDate.AddDays(-1), targetDate);

            var byDow = new Dictionary<int, List<CityPrepDaysTbl>>();
            foreach (var p in prepDays)
            {
                int key = p.PrepDayOfWeekID;
                if (!byDow.ContainsKey(key))
                    byDow[key] = new List<CityPrepDaysTbl>();
                byDow[key].Add(p);
            }

            const int searchRadius = 14;
            DateTime bestDelivery = DateTime.MaxValue;
            DateTime bestPrep = DateTime.MaxValue;
            bool found = false;

            // Offsets searched in “balanced” order: 0, +1, -1, +2, -2, ...
            for (int radius = 0; radius <= searchRadius; radius++)
            {
                int[] offsets;
                if (radius == 0)
                    offsets = new[] { 0 };
                else
                    offsets = new[] { radius, -radius };

                foreach (int offset in offsets)
                {
                    DateTime candidatePrep = targetDate.AddDays(offset).Date;
                    int dow = (int)candidatePrep.DayOfWeek;

                    List<CityPrepDaysTbl> rulesForDow;
                    if (!byDow.TryGetValue(dow, out rulesForDow))
                        continue;

                    foreach (var rule in rulesForDow)
                    {
                        DateTime candidateDelivery = candidatePrep.AddDays(rule.DeliveryDelayDays).Date;

                        if (candidateDelivery < targetDate) continue;
                        if (candidateDelivery < minDate) continue;

                        // Select earliest delivery; tie-breaker earliest prep
                        if (!found ||
                            candidateDelivery < bestDelivery ||
                            (candidateDelivery == bestDelivery && candidatePrep < bestPrep))
                        {
                            bestDelivery = candidateDelivery;
                            bestPrep = candidatePrep;
                            found = true;

                            if (bestDelivery == targetDate)
                            {
                                // Can't get earlier than target; early exit
                                AppLogger.WriteLog(
                                    SystemConstants.LogTypes.Orders,
                                    $"DateCalculator: Exact target delivery {bestDelivery:yyyy-MM-dd} selected (offset {offset}) for customer {customerId}");
                                return new PrepDeliveryPair(bestPrep, bestDelivery);
                            }
                        }
                    }
                }
            }

            if (found)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Orders,
                    $"DateCalculator: Chose delivery {bestDelivery:yyyy-MM-dd} with prep {bestPrep:yyyy-MM-dd} for customer {customerId}");
                return new PrepDeliveryPair(bestPrep, bestDelivery);
            }

            // Fallback
            DateTime fallbackPrep = targetDate.AddDays(-1);
            if (fallbackPrep < minDate) fallbackPrep = minDate;
            AppLogger.WriteLog(
                SystemConstants.LogTypes.Orders,
                $"DateCalculator: Fallback delivery {targetDate:yyyy-MM-dd} (no schedule match) for customer {customerId}");
            return new PrepDeliveryPair(fallbackPrep, targetDate);
        }
        /// <summary>
        /// Finds the closest delivery date for a customer based on their city's prep/delivery schedule and a target date.
        /// </summary>
        //private DateTime FindClosestCityDeliveryDate(long customerId, DateTime targetDate, DateTime minDate)
        //{
        //    int cityId = _cityTblDAL.GetCityIdByCustomerId(customerId);
        //    if (cityId == 0)
        //        return targetDate;

        //    List<CityPrepDaysTbl> prepDays = _cityTblDAL.GetPrepRulesForCity(cityId);
        //    if (prepDays == null || prepDays.Count == 0)
        //        return targetDate;

        //    DateTime earliestDelivery = DateTime.MaxValue;

        //    // Search a window of prep dates around the target date (e.g., ±7 days)
        //    for (int offset = -7; offset <= 7; offset++)
        //    {
        //        DateTime candidatePrep = targetDate.AddDays(offset);

        //        foreach (var prep in prepDays)
        //        {
        //            if ((int)candidatePrep.DayOfWeek != prep.PrepDayOfWeekID)
        //                continue;

        //            DateTime candidateDelivery = candidatePrep.AddDays(prep.DeliveryDelayDays);

        //            // Only consider delivery dates on or after targetDate and minDate
        //            if (candidateDelivery >= targetDate && candidateDelivery >= minDate && candidateDelivery < earliestDelivery)
        //            {
        //                earliestDelivery = candidateDelivery;
        //            }
        //        }
        //    }

        //    return earliestDelivery == DateTime.MaxValue ? targetDate : earliestDelivery;
        //}
        public List<ReoccuringOrderExtData> FilterAndUpdateRecurringOrdersDates(List<ReoccuringOrderExtData> reoccuringOrders,
            DateTime? windowStart = null, DateTime? windowEnd = null)
        {
            var validOrders = new List<ReoccuringOrderExtData>();
            var reoccuringOrderDal = new ReoccuringOrderDAL();

            foreach (var order in reoccuringOrders)
            {
                // Always recalculate next required date using LastDone date and recurrence type
                var calculatedNextDates = reoccuringOrderDal.CalculateNextDatesRequired(order);

                order.PrepDate = calculatedNextDates.PrepDate;
                // Update legacy/broken records if needed
                if (order.NextDateRequired != calculatedNextDates.DeliveryDate)
                {
                    order.NextDateRequired = calculatedNextDates.DeliveryDate;
                    reoccuringOrderDal.UpdateReoccuringOrder(order, order.ReoccuringOrderID, false);
                }

                // Only keep records where next required date is <= windowEnd and not min date
                bool isValid = order.NextDateRequired != SystemConstants.DatabaseConstants.SystemMinDate;
                if (windowEnd.HasValue)
                    isValid &= order.NextDateRequired <= windowEnd.Value;
                if (windowStart.HasValue)
                    isValid &= order.NextDateRequired >= windowStart.Value;

                if (isValid)
                    validOrders.Add(order);
            }
            return validOrders;
        }
        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}