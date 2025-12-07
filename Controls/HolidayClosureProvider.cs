using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using System.Configuration;

namespace TrackerSQL.Controls
{
    public class HolidayClosure
    {
        public int ID;
        public DateTime ClosureDate;
        public int DaysClosed;
        public bool AppliesToPrep;
        public bool AppliesToDelivery;
        public string ShiftStrategy;
        public string Description;
    }

    public class PrepDeliveryAdjustment
    {
        public DateTime Prep;
        public DateTime Delivery;
        public bool WasAdjusted;
        public string Reason;
        public PrepDeliveryAdjustment(DateTime prep, DateTime delivery, bool wasAdjusted, string reason)
        {
            Prep = prep;
            Delivery = delivery;
            WasAdjusted = wasAdjusted;
            Reason = reason;
        }
    }

    public class HolidayClosureProvider
    {
        private static List<HolidayClosure> _cache = new List<HolidayClosure>();
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly object _sync = new object();
        private const int CACHE_MINUTES = 2;
        private int ImminentWindowDays
        {
            get
            {
                int d = ConfigHelper.GetInt(SystemConstants.HolidayClosureConstants.ImminentWindowDaysSettingKey,
                    SystemConstants.HolidayClosureConstants.DefaultImminentWindowDays);
                if (d > 0)
                {
                    return d;
                }
                return SystemConstants.HolidayClosureConstants.DefaultImminentWindowDays;
            }
        }

        private bool ShouldForceForward(HolidayClosure closure)
        {
            if (closure == null) return false;
            DateTime today = TimeZoneUtils.Now().Date;
            if (closure.ClosureDate < today) return false;
            return (closure.ClosureDate - today).TotalDays <= ImminentWindowDays;
        }
        private void GetHolidayDateWithCache()
        {
            lock (_sync)
            {
                if (DateTime.Now <= _cacheExpiry && _cache.Count > 0)
                    return;

                var list = new List<HolidayClosure>();

                const string sql = "SELECT ID, ClosureDate, DaysClosed, AppliesToPrep, AppliesToDelivery, ShiftStrategy, Description FROM HolidayClosureTbl";
                using (var db = new TrackerDb())
                {
                    using (var rdr = db.ExecuteSQLGetDataReader(sql))
                    {
                        if (rdr != null)
                        {
                            while (rdr.Read())
                            {
                                var hc = new HolidayClosure
                                {
                                    ID = rdr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ID"]),
                                    ClosureDate = rdr["ClosureDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["ClosureDate"]).Date,
                                    DaysClosed = rdr["DaysClosed"] == DBNull.Value ? 1 : Convert.ToInt32(rdr["DaysClosed"]),
                                    AppliesToPrep = rdr["AppliesToPrep"] != DBNull.Value && Convert.ToBoolean(rdr["AppliesToPrep"]),
                                    AppliesToDelivery = rdr["AppliesToDelivery"] != DBNull.Value && Convert.ToBoolean(rdr["AppliesToDelivery"]),
                                    ShiftStrategy = rdr["ShiftStrategy"] == DBNull.Value ? "Forward" : Convert.ToString(rdr["ShiftStrategy"]),
                                    Description = rdr["Description"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["Description"])
                                };
                                if (hc.DaysClosed < 1) hc.DaysClosed = 1;
                                list.Add(hc);
                            }
                            rdr.Close();
                        }
                    }
                }

                _cache = list;
                _cacheExpiry = DateTime.Now.AddMinutes(CACHE_MINUTES);

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    "HolidayClosureProvider: cache loaded " + _cache.Count + " closures");
            }
        }

        private HolidayClosure GetClosureForDate(DateTime targetDate)
        {
            GetHolidayDateWithCache();
            targetDate = targetDate.Date;
            return _cache.FirstOrDefault(h => h.ClosureDate == targetDate);
        }

        private IEnumerable<HolidayClosure> MatchExactDate(DateTime targetDate)
        {
            GetHolidayDateWithCache();
            targetDate = targetDate.Date;
            return _cache.Where(h => h.ClosureDate == targetDate);
        }

        // Helper to compute end date:
        private static DateTime GetEndDate(HolidayClosure h) => h.ClosureDate.AddDays(h.DaysClosed - 1);

        public bool IsClosed(DateTime targetDate, bool forPrep)
        {
            GetHolidayDateWithCache();
            targetDate = targetDate.Date;
            foreach (var h in _cache)
            {
                if (targetDate >= h.ClosureDate && targetDate <= GetEndDate(h))
                {
                    if (forPrep && h.AppliesToPrep) return true;
                    if (!forPrep && h.AppliesToDelivery) return true;
                }
            }
            return false;
        }
        public IList<HolidayClosure> GetRange(DateTime rangeStart, DateTime rangeEnd)
        {
            GetHolidayDateWithCache();
            rangeStart = rangeStart.Date; rangeEnd = rangeEnd.Date;
            var list = new List<HolidayClosure>();
            foreach (var h in _cache)
            {
                var end = GetEndDate(h);
                if (h.ClosureDate <= rangeEnd && end >= rangeStart)
                    list.Add(h);
            }
            list.Sort((a, b) => a.ClosureDate.CompareTo(b.ClosureDate));
            return list;
        }
        public DateTime AdjustDate(DateTime candidateDate, bool forPrep)
        {
            if (!IsClosed(candidateDate, forPrep))
                return candidateDate;

            var closure = _cache.FirstOrDefault(h =>
                candidateDate >= h.ClosureDate &&
                candidateDate <= GetEndDate(h) &&
               (forPrep ? h.AppliesToPrep : h.AppliesToDelivery));

            string strategy = (closure != null && !string.IsNullOrEmpty(closure.ShiftStrategy))
                ? closure.ShiftStrategy
                : "Forward";
            strategy = strategy.ToLowerInvariant();

            if (strategy == "skip")
                return candidateDate;

            // NEW: force forward if imminent
            if (strategy == "backward" && ShouldForceForward(closure))
            {
                strategy = "forward";
            }

            DateTime adjusted = candidateDate.Date;
            int guard = 0;

            if (strategy == "backward")
            {
                while (IsClosed(adjusted, forPrep) && guard++ < 14)
                    adjusted = adjusted.AddDays(-1);
                return adjusted;
            }

            // default forward (including forcedForward)
            while (IsClosed(adjusted, forPrep) && guard++ < 14)
                adjusted = adjusted.AddDays(1);

            return adjusted;
        }
        public PrepDeliveryAdjustment AdjustPair(DateTime prepDate, DateTime deliveryDate)
        {
            bool changed = false;
            string reason = string.Empty;

            DateTime adjPrep = prepDate;
            DateTime adjDelivery = deliveryDate;

            // Capture closures for annotation (before adjustment)
            var prepClosure = _cache.FirstOrDefault(h =>
                prepDate >= h.ClosureDate &&
                prepDate <= GetEndDate(h) &&
                h.AppliesToPrep);

            var deliveryClosure = _cache.FirstOrDefault(h =>
                deliveryDate >= h.ClosureDate &&
                deliveryDate <= GetEndDate(h) &&
                h.AppliesToDelivery);

            if (IsClosed(adjPrep, true))
            {
                DateTime newPrep = AdjustDate(adjPrep, true);
                if (newPrep != adjPrep)
                {
                    changed = true;
                    reason += "Prep moved from " + adjPrep.ToShortDateString() + "; ";
                    adjPrep = newPrep;
                    if (prepClosure != null && prepClosure.ShiftStrategy.Equals("Backward", StringComparison.OrdinalIgnoreCase)
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
                    if (deliveryClosure != null && deliveryClosure.ShiftStrategy.Equals("Backward", StringComparison.OrdinalIgnoreCase)
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
        public static void Invalidate()
        {
            lock (_sync)
            {
                _cacheExpiry = DateTime.MinValue;
                _cache.Clear();
            }
        }

        public bool Insert(DateTime closureDate,
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
                using (var db = new TrackerDb())
                {
                    const string sql = "INSERT INTO HolidayClosureTbl (ClosureDate, DaysClosed, AppliesToPrep, AppliesToDelivery, ShiftStrategy, Description) VALUES (?,?,?,?,?,?)";

                    db.AddParams(closureDate.Date, DbType.Date);
                    db.AddParams(daysClosed < 1 ? 1 : daysClosed, DbType.Int32);
                    db.AddParams(appliesToPrep, DbType.Boolean);
                    db.AddParams(appliesToDelivery, DbType.Boolean);
                    db.AddParams(shiftStrategy ?? "Forward", DbType.String);
                    db.AddParams(string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description, DbType.String);

                    string err = db.ExecuteNonQuerySQLWithParams(sql, db.Params);
                    if (!string.IsNullOrEmpty(err))
                    {
                        error = err;
                        return false;
                    }
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
                using (var db = new TrackerDb())
                {
                    const string sql = "DELETE FROM HolidayClosureTbl WHERE ID = ?";
                    db.AddWhereParams(id, DbType.Int32);
                    string err = db.ExecuteNonQuerySQLWithParams(sql, null, db.WhereParams);
                    if (!string.IsNullOrEmpty(err))
                    {
                        error = err;
                        return false;
                    }
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
            GetHolidayDateWithCache();
            var start = startDate.Date;
            var end = start.AddDays(daysWindow);
            foreach (var h in _cache)
            {
                var hEnd = GetEndDate(h);
                if (h.ClosureDate <= end && hEnd >= start)
                    return true;
            }
            return false;
        }

        public bool IsThereAHolodayComing(DateTime startDate)
        {
            int days = 9; // default
            int.TryParse(ConfigurationManager.AppSettings["CoffeeCheckupReminderWindowDays"], out days);
            if (days <= 0) days = 9;
            return IsThereAHolodayComing(startDate, days);
        }
    }
}