using System;
using System.Collections.Generic;
using System.Linq;
using TrackerDotNet.Controls;

namespace TrackerDotNet.Classes
{
    /// <summary>
    /// Caches per-city prep/delivery pairs for a rolling horizon to allow
    /// very fast "closest delivery date" lookups without recalculating rules per customer.
    /// </summary>
    public static class DeliveryDateMatrixCache
    {
        private class Entry
        {
            public int CityId;
            public DateTime PrepDate;
            public DateTime DeliveryDate;
            public int PrepDayOfWeek; // 0..6
            public override string ToString() =>
                $"City:{CityId} Prep:{PrepDate:yyyy-MM-dd} Del:{DeliveryDate:yyyy-MM-dd}";
        }

        private static readonly object _lock = new object();
        private static List<Entry> _entries = new List<Entry>();
        private static DateTime _builtAtUtc = DateTime.MinValue;
        private static TimeSpan _ttl = TimeSpan.FromMinutes(5);
        private static DateTime _horizonEnd = DateTime.MinValue;

        /// <summary>
        /// Build (or rebuild) the matrix for (today .. today + window + bufferDays).
        /// Safe to call multiple times; will no-op if fresh and horizon is sufficient.
        /// </summary>
        public static void Initialize(int windowDays, int bufferDays = 5)
        {
            var today = TimeZoneUtils.Now().Date;
            var desiredEnd = today.AddDays(windowDays + bufferDays);

            lock (_lock)
            {
                if (_entries.Count > 0 &&
                    DateTime.UtcNow - _builtAtUtc < _ttl &&
                    desiredEnd <= _horizonEnd)
                {
                    return; // still valid
                }

                _entries.Clear();

                // Load all city prep rules grouped by City
                var allRules = new CityPrepDaysTbl().GetAll("CityID, PrepDayOfWeekID");
                if (allRules == null || allRules.Count == 0)
                {
                    _builtAtUtc = DateTime.UtcNow;
                    _horizonEnd = desiredEnd;
                    return;
                }

                var byCity = allRules
                    .GroupBy(r => r.CityID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Iterate each day in horizon and build entries
                for (var day = today; day <= desiredEnd; day = day.AddDays(1))
                {
                    foreach (var kv in byCity)
                    {
                        var cityId = kv.Key;
                        var rules = kv.Value;

                        // For any rule whose prep DOW matches this day, create an entry.
                        foreach (var rule in rules)
                        {
                            // rule.PrepDayOfWeekID is assumed 0..6 (Sunday..Saturday). If 1..7 adjust here.
                            if ((int)day.DayOfWeek == rule.PrepDayOfWeekID)
                            {
                                var delivery = day.AddDays(rule.DeliveryDelayDays).Date;
                                if (delivery < today) continue; // never go into the past
                                _entries.Add(new Entry
                                {
                                    CityId = cityId,
                                    PrepDate = day,
                                    DeliveryDate = delivery,
                                    PrepDayOfWeek = (int)day.DayOfWeek
                                });
                            }
                        }
                    }
                }

                _builtAtUtc = DateTime.UtcNow;
                _horizonEnd = desiredEnd;

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"DeliveryDateMatrixCache: Built {_entries.Count} entries for horizon end {desiredEnd:yyyy-MM-dd}");
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
                _builtAtUtc = DateTime.MinValue;
                _horizonEnd = DateTime.MinValue;
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "DeliveryDateMatrixCache: Cleared");
            }
        }

        /// <summary>
        /// Finds the closest valid delivery for a city according to rule:
        /// 1. Exact target
        /// 2. target + 1
        /// 3. target - 1 (if not before today)
        /// If all fail, best absolute future (then absolute nearest overall).
        /// Returns null if city has no entries at all.
        /// </summary>
        public static (DateTime Delivery, DateTime Prep)? FindClosestDelivery(int cityId, DateTime targetDelivery)
        {
            var today = TimeZoneUtils.Now().Date;
            targetDelivery = targetDelivery.Date;

            List<Entry> cityEntries;
            lock (_lock)
            {
                cityEntries = _entries.Where(e => e.CityId == cityId).ToList();
            }
            if (cityEntries.Count == 0)
                return null;

            // Helper local evaluator
            (DateTime Delivery, DateTime Prep)? TryExact(DateTime probe)
            {
                var hit = cityEntries.FirstOrDefault(e => e.DeliveryDate == probe);
                if (hit == null) return null;
                return (hit.DeliveryDate, hit.PrepDate);
            }

            // Priority sequence
            var exact = TryExact(targetDelivery);
            if (exact != null) return exact;

            var plus1 = TryExact(targetDelivery.AddDays(1));
            if (plus1 != null) return plus1;

            if (targetDelivery.AddDays(-1) >= today)
            {
                var minus1 = TryExact(targetDelivery.AddDays(-1));
                if (minus1 != null) return minus1;
            }

            // Fallback: earliest future >= target
            var future = cityEntries
                .Where(e => e.DeliveryDate >= targetDelivery)
                .OrderBy(e => e.DeliveryDate)
                .FirstOrDefault();

            if (future != null)
                return (future.DeliveryDate, future.PrepDate);

            // Last resort: absolute closest (both past and future)
            var closest = cityEntries
                .OrderBy(e => Math.Abs((e.DeliveryDate - targetDelivery).TotalDays))
                .ThenBy(e => e.DeliveryDate)
                .First();

            return (closest.DeliveryDate, closest.PrepDate);
        }
    }
}