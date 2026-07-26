using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class PrepDeliveryAdjustment
    {
        public DateTime Prep { get; }
        public DateTime Delivery { get; }
        public bool WasAdjusted { get; }
        public string Reason { get; }

        public PrepDeliveryAdjustment(DateTime prep, DateTime delivery, bool wasAdjusted, string reason)
        {
            Prep = prep;
            Delivery = delivery;
            WasAdjusted = wasAdjusted;
            Reason = reason;
        }
    }

    /// <summary>
    /// Holiday / closure date business rules. All SQL goes through <see cref="HolidayClosuresRepository"/>.
    /// </summary>
    public class HolidayClosureManager
    {
        private readonly HolidayClosuresRepository _repository = new HolidayClosuresRepository();

        private static List<HolidayClosure> _cache = new List<HolidayClosure>();
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly object SyncRoot = new object();
        private const int CacheMinutes = 2;

        private int ImminentWindowDays
        {
            get
            {
                int days = ConfigHelper.GetInt(
                    SystemConstants.HolidayClosureConstants.ImminentWindowDaysSettingKey,
                    SystemConstants.HolidayClosureConstants.DefaultImminentWindowDays);
                return days > 0 ? days : SystemConstants.HolidayClosureConstants.DefaultImminentWindowDays;
            }
        }

        public static int GetDaysClosed(HolidayClosure closure)
        {
            if (closure == null || !closure.DaysClosed.HasValue || closure.DaysClosed.Value < 1)
                return 1;
            return closure.DaysClosed.Value;
        }

        public static DateTime GetEndDate(HolidayClosure closure)
        {
            if (closure == null)
                return DateTime.MinValue;
            return closure.ClosureDate.Date.AddDays(GetDaysClosed(closure) - 1);
        }

        public static bool AppliesPrep(HolidayClosure closure) =>
            closure != null && (closure.AppliesToPrep ?? false);

        public static bool AppliesDelivery(HolidayClosure closure) =>
            closure != null && (closure.AppliesToDelivery ?? false);

        private bool ShouldForceForward(HolidayClosure closure)
        {
            if (closure == null)
                return false;

            DateTime today = TimeZoneUtils.Now().Date;
            if (closure.ClosureDate.Date < today)
                return false;

            return (closure.ClosureDate.Date - today).TotalDays <= ImminentWindowDays;
        }

        private void EnsureCache()
        {
            lock (SyncRoot)
            {
                if (DateTime.Now <= _cacheExpiry && _cache.Count > 0)
                    return;

                try
                {
                    _cache = _repository.GetAllOrdered() ?? new List<HolidayClosure>();
                    _cacheExpiry = DateTime.Now.AddMinutes(CacheMinutes);
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        "HolidayClosureManager: cache loaded " + _cache.Count + " closures");
                }
                catch (Exception ex)
                {
                    _cache = new List<HolidayClosure>();
                    _cacheExpiry = DateTime.MinValue;
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database,
                        "HolidayClosureManager: cache load failed: " + ex.Message);
                }
            }
        }

        public static void Invalidate()
        {
            lock (SyncRoot)
            {
                _cacheExpiry = DateTime.MinValue;
                _cache.Clear();
            }
        }

        public bool IsClosed(DateTime targetDate, bool forPrep)
        {
            EnsureCache();
            targetDate = targetDate.Date;
            foreach (var closure in _cache)
            {
                if (targetDate < closure.ClosureDate.Date || targetDate > GetEndDate(closure))
                    continue;

                if (forPrep && AppliesPrep(closure))
                    return true;
                if (!forPrep && AppliesDelivery(closure))
                    return true;
            }

            return false;
        }

        public IList<HolidayClosure> GetRange(DateTime rangeStart, DateTime rangeEnd)
        {
            EnsureCache();
            rangeStart = rangeStart.Date;
            rangeEnd = rangeEnd.Date;

            var list = new List<HolidayClosure>();
            foreach (var closure in _cache)
            {
                DateTime end = GetEndDate(closure);
                if (closure.ClosureDate.Date <= rangeEnd && end >= rangeStart)
                    list.Add(closure);
            }

            list.Sort((a, b) => a.ClosureDate.CompareTo(b.ClosureDate));
            return list;
        }

        public HolidayClosure GetById(int id)
        {
            if (id <= 0)
                return null;

            EnsureCache();
            var cached = _cache.FirstOrDefault(h => h.HolidayClosureID == id);
            if (cached != null)
                return cached;

            return _repository.GetById(id);
        }

        public DateTime AdjustDate(DateTime candidateDate, bool forPrep)
        {
            if (!IsClosed(candidateDate, forPrep))
                return candidateDate;

            var closure = _cache.FirstOrDefault(h =>
                candidateDate.Date >= h.ClosureDate.Date
                && candidateDate.Date <= GetEndDate(h)
                && (forPrep ? AppliesPrep(h) : AppliesDelivery(h)));

            string strategy = string.IsNullOrEmpty(closure?.ShiftStrategy)
                ? "Forward"
                : closure.ShiftStrategy;
            strategy = strategy.ToLowerInvariant();

            if (strategy == "skip")
                return candidateDate;

            if (strategy == "backward" && ShouldForceForward(closure))
                strategy = "forward";

            DateTime adjusted = candidateDate.Date;
            int guard = 0;

            if (strategy == "backward")
            {
                while (IsClosed(adjusted, forPrep) && guard++ < 14)
                    adjusted = adjusted.AddDays(-1);
                return adjusted;
            }

            while (IsClosed(adjusted, forPrep) && guard++ < 14)
                adjusted = adjusted.AddDays(1);

            return adjusted;
        }

        public PrepDeliveryAdjustment AdjustPair(DateTime prepDate, DateTime deliveryDate)
        {
            EnsureCache();

            bool changed = false;
            string reason = string.Empty;
            DateTime adjPrep = prepDate;
            DateTime adjDelivery = deliveryDate;

            var prepClosure = _cache.FirstOrDefault(h =>
                prepDate.Date >= h.ClosureDate.Date
                && prepDate.Date <= GetEndDate(h)
                && AppliesPrep(h));

            var deliveryClosure = _cache.FirstOrDefault(h =>
                deliveryDate.Date >= h.ClosureDate.Date
                && deliveryDate.Date <= GetEndDate(h)
                && AppliesDelivery(h));

            if (IsClosed(adjPrep, true))
            {
                DateTime newPrep = AdjustDate(adjPrep, true);
                if (newPrep != adjPrep)
                {
                    changed = true;
                    reason += "Prep moved from " + adjPrep.ToShortDateString() + "; ";
                    adjPrep = newPrep;
                    if (prepClosure != null
                        && string.Equals(prepClosure.ShiftStrategy, "Backward", StringComparison.OrdinalIgnoreCase)
                        && ShouldForceForward(prepClosure))
                    {
                        reason += "(prep forced forward); ";
                    }
                }
            }

            if (IsClosed(adjDelivery, false))
            {
                DateTime newDelivery = AdjustDate(adjDelivery, false);
                if (newDelivery != adjDelivery)
                {
                    changed = true;
                    reason += "Delivery moved from " + adjDelivery.ToShortDateString() + "; ";
                    if (deliveryClosure != null
                        && string.Equals(deliveryClosure.ShiftStrategy, "Backward", StringComparison.OrdinalIgnoreCase)
                        && ShouldForceForward(deliveryClosure))
                    {
                        reason += "(delivery forced forward); ";
                    }
                    adjDelivery = newDelivery;
                }
            }

            if (adjPrep > adjDelivery)
            {
                adjPrep = adjDelivery.AddDays(-1);
                changed = true;
                reason += "Prep re-aligned; ";
            }

            return new PrepDeliveryAdjustment(adjPrep, adjDelivery, changed, reason.Trim());
        }

        /// <summary>
        /// Validates that the proposed closure does not reuse an existing start date
        /// and does not overlap any other closure range. Returns false with a user-facing message.
        /// </summary>
        public bool ValidateUniqueDateRange(DateTime closureDate, int daysClosed, int excludeId, out string error)
        {
            error = null;
            closureDate = closureDate.Date;
            if (daysClosed < 1)
                daysClosed = 1;

            DateTime endDate = closureDate.AddDays(daysClosed - 1);

            if (_repository.ExistsWithStartDate(closureDate, excludeId))
            {
                error = "A holiday/closure already starts on "
                    + closureDate.ToString("yyyy-MM-dd")
                    + ". Each start date must be unique.";
                return false;
            }

            var overlaps = _repository.GetOverlappingRange(closureDate, endDate, excludeId);
            if (overlaps == null || overlaps.Count == 0)
                return true;

            HolidayClosure conflict = overlaps[0];
            DateTime conflictEnd = GetEndDate(conflict);
            string desc = string.IsNullOrWhiteSpace(conflict.Description)
                ? "(no description)"
                : conflict.Description.Trim();

            error = "This date range overlaps an existing closure: "
                + conflict.ClosureDate.ToString("yyyy-MM-dd")
                + " to "
                + conflictEnd.ToString("yyyy-MM-dd")
                + " — "
                + desc
                + ".";
            return false;
        }

        /// <summary>Inserts a closure. Returns true if a new row was created.</summary>
        public bool Insert(
            DateTime closureDate,
            int daysClosed,
            bool appliesToPrep,
            bool appliesToDelivery,
            string shiftStrategy,
            string description,
            out string error)
        {
            return InsertReturningId(
                closureDate, daysClosed, appliesToPrep, appliesToDelivery, shiftStrategy, description, out error) > 0;
        }

        /// <summary>Inserts a closure and returns the new HolidayClosureID, or -1 on failure.</summary>
        public int InsertReturningId(
            DateTime closureDate,
            int daysClosed,
            bool appliesToPrep,
            bool appliesToDelivery,
            string shiftStrategy,
            string description,
            out string error)
        {
            error = null;
            try
            {
                if (daysClosed < 1)
                    daysClosed = 1;

                if (!ValidateUniqueDateRange(closureDate, daysClosed, excludeId: 0, out error))
                    return -1;

                var entity = new HolidayClosure
                {
                    ClosureDate = closureDate.Date,
                    DaysClosed = daysClosed,
                    AppliesToPrep = appliesToPrep,
                    AppliesToDelivery = appliesToDelivery,
                    ShiftStrategy = shiftStrategy,
                    Description = description
                };

                int newId = _repository.Insert(entity);
                if (newId <= 0)
                {
                    error = "Insert failed";
                    return -1;
                }

                Invalidate();
                return newId;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return -1;
            }
        }

        public bool Update(
            int id,
            DateTime closureDate,
            int daysClosed,
            bool appliesToPrep,
            bool appliesToDelivery,
            string shiftStrategy,
            string description,
            out string error)
        {
            error = null;
            if (id <= 0)
            {
                error = "Invalid ID";
                return false;
            }

            try
            {
                if (daysClosed < 1)
                    daysClosed = 1;

                if (!ValidateUniqueDateRange(closureDate, daysClosed, excludeId: id, out error))
                    return false;

                var entity = new HolidayClosure
                {
                    HolidayClosureID = id,
                    ClosureDate = closureDate.Date,
                    DaysClosed = daysClosed,
                    AppliesToPrep = appliesToPrep,
                    AppliesToDelivery = appliesToDelivery,
                    ShiftStrategy = shiftStrategy,
                    Description = description
                };

                if (_repository.Update(entity) <= 0)
                {
                    error = "Update failed";
                    return false;
                }

                Invalidate();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool Delete(int id, out string error)
        {
            error = null;
            try
            {
                if (!_repository.Delete(id))
                {
                    error = "Delete failed";
                    return false;
                }

                Invalidate();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool IsThereAHolodayComing(DateTime startDate, int daysWindow)
        {
            EnsureCache();
            DateTime start = startDate.Date;
            DateTime end = start.AddDays(daysWindow);
            foreach (var closure in _cache)
            {
                DateTime closureEnd = GetEndDate(closure);
                if (closure.ClosureDate.Date <= end && closureEnd >= start)
                    return true;
            }

            return false;
        }

        public bool IsThereAHolodayComing(DateTime startDate)
        {
            int days = 9;
            int.TryParse(ConfigurationManager.AppSettings["CoffeeCheckupReminderWindowDays"], out days);
            if (days <= 0)
                days = 9;
            return IsThereAHolodayComing(startDate, days);
        }
    }

    /// <summary>Backward-compatible alias for existing call sites.</summary>
    public class HolidayClosureProvider : HolidayClosureManager
    {
    }
}
