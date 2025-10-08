using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using TrackerDotNet.Controls;

namespace TrackerDotNet.Classes
{
    /// <summary>
    /// Represents one prep/delivery schedule row for a city.
    /// </summary>
    public class DateMatrixRow
    {
        public int CityID { get; set; }
        public DateTime PrepDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int PrepDayOfWeek { get; set; } // 0..6
    }

    public enum DateMatchKind
    {
        Exact,
        PlusOne,
        MinusOne,
        FutureForward,
        AbsoluteClosest
    }

    public struct DateMatrixMatch
    {
        public DateTime Delivery;
        public DateTime Prep;
        public DateMatchKind MatchKind;
    }

    /// <summary>
    /// Caching builder for city prep/delivery matrix.
    /// Features:
    ///  - Expand-in-place for larger horizons
    ///  - TTL-based expiration
    ///  - Optional invalidation when City prep data recalculated
    ///  - Optional SQL-based build (Numbers table variant)
    /// </summary>
    public static class DateMatrixBuilder
    {
        private static readonly object _lock = new object();

        private static List<DateMatrixRow> _rows = new List<DateMatrixRow>();
        private static Dictionary<int, List<DateMatrixRow>> _rowsByCity = new Dictionary<int, List<DateMatrixRow>>();

        private static DateTime _builtAtUtc = DateTime.MinValue;
        private static DateTime _horizonEnd = DateTime.MinValue;
        private static DateTime _horizonStart = DateTime.MinValue;
        private static DateTime _sourceCalcDate = DateTime.MinValue; // SysDataTbl.DateLastPrepDateCalcd snapshot

        // Settings
        private static TimeSpan _ttl = TimeSpan.FromMinutes(GetSettingInt("CoffeeCheckupDateMatrixTTLMinutes", 10));
        private static int _bufferDays = GetSettingInt("CoffeeCheckupDateMatrixBufferDays", 5);
        private static bool _useSqlMatrix = GetSettingBool("CoffeeCheckupUseSqlMatrix", false);
        private static bool _rebuildOnSourceChange = GetSettingBool("CoffeeCheckupRebuildIfSourceChanges", true);

        public static IReadOnlyList<DateMatrixRow> Rows
        {
            get { lock (_lock) return _rows.AsReadOnly(); }
        }

        public static void EnsureBuilt(int windowDays)
        {
            if (windowDays < 0) windowDays = 0;
            var today = TimeZoneUtils.Now().Date;
            var desiredStart = today; // always from "today"
            var desiredEnd = today.AddDays(windowDays + _bufferDays);

            lock (_lock)
            {
                bool needRebuild = false;
                bool needExtend = false;

                // Source calc date used to detect changes in City prep plan
                DateTime latestSourceCalc = GetSysPrepCalcDate();
                if (_rebuildOnSourceChange && latestSourceCalc > _sourceCalcDate && _sourceCalcDate != DateTime.MinValue)
                {
                    needRebuild = true;
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        $"DateMatrixBuilder: Invalidate due to source change. OldCalc={_sourceCalcDate:yyyy-MM-dd HH:mm:ss}, NewCalc={latestSourceCalc:yyyy-MM-dd HH:mm:ss}");
                }

                if (_rows.Count == 0) needRebuild = true;
                else if (DateTime.UtcNow - _builtAtUtc > _ttl) needRebuild = true;
                else
                {
                    // If requested horizon extends beyond current horizon, only extend
                    if (desiredEnd > _horizonEnd) needExtend = true;
                }

                if (needRebuild)
                {
                    BuildNew(desiredStart, desiredEnd);
                }
                else if (needExtend)
                {
                    Extend(desiredEnd);
                }
            }
        }
        public static void Clear()
        {
            lock (_lock)
            {
                _rows.Clear();
                _rowsByCity.Clear();
                _builtAtUtc = DateTime.MinValue;
                _horizonEnd = DateTime.MinValue;
                _horizonStart = DateTime.MinValue;
                _sourceCalcDate = DateTime.MinValue;
            }
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "DateMatrixBuilder: Cleared matrix");
        }

        public static DateMatrixMatch? FindBestForCity(int cityId, DateTime target)
        {
            target = target.Date;
            var today = TimeZoneUtils.Now().Date;

            List<DateMatrixRow> cityRows;
            lock (_lock)
            {
                if (!_rowsByCity.TryGetValue(cityId, out cityRows) || cityRows.Count == 0)
                    return null;
            }

            DateMatrixMatch? TryMatch(DateTime d, DateMatchKind kind)
            {
                var row = cityRows.FirstOrDefault(r => r.DeliveryDate == d);
                if (row == null) return null;
                return new DateMatrixMatch { Delivery = row.DeliveryDate, Prep = row.PrepDate, MatchKind = kind };
            }

            // 1 exact
            var m = TryMatch(target, DateMatchKind.Exact);
            if (m != null) return m;
            // 2 +1
            m = TryMatch(target.AddDays(1), DateMatchKind.PlusOne);
            if (m != null) return m;
            // 3 -1
            if (target.AddDays(-1) >= today)
            {
                m = TryMatch(target.AddDays(-1), DateMatchKind.MinusOne);
                if (m != null) return m;
            }
            // 4 first future >= target
            var future = cityRows
                .Where(r => r.DeliveryDate >= target)
                .OrderBy(r => r.DeliveryDate)
                .FirstOrDefault();
            if (future != null)
            {
                return new DateMatrixMatch
                {
                    Delivery = future.DeliveryDate,
                    Prep = future.PrepDate,
                    MatchKind = DateMatchKind.FutureForward
                };
            }
            // 5 absolute closest
            var closest = cityRows
                .OrderBy(r => Math.Abs((r.DeliveryDate - target).TotalDays))
                .ThenBy(r => r.DeliveryDate)
                .First();
            return new DateMatrixMatch
            {
                Delivery = closest.DeliveryDate,
                Prep = closest.PrepDate,
                MatchKind = DateMatchKind.AbsoluteClosest
            };
        }

        // ----- Internal Build Methods -----
        private static void BuildNew(DateTime start, DateTime end)
        {
            _rows.Clear();
            _rowsByCity.Clear();

            _sourceCalcDate = GetSysPrepCalcDate();

            if (_useSqlMatrix)
            {
                if (!BuildViaSql(start, end))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        "DateMatrixBuilder: SQL build failed, using in-memory fallback.");
                    BuildInMemory(start, end);
                }
            }
            else
            {
                BuildInMemory(start, end);
            }

            IndexRows();
            _horizonStart = start;
            _horizonEnd = end;
            _builtAtUtc = DateTime.UtcNow;

            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                $"DateMatrixBuilder: BuildNew complete Rows={_rows.Count} Horizon={_horizonStart:yyyy-MM-dd}->{_horizonEnd:yyyy-MM-dd} SourceCalc={_sourceCalcDate:yyyy-MM-dd HH:mm:ss}");
        }

        private static void Extend(DateTime newEnd)
        {
            // Extend from current _horizonEnd+1 to newEnd
            var appendStart = _horizonEnd.AddDays(1);
            if (appendStart > newEnd) return;

            var added = BuildSegment(appendStart, newEnd);
            if (added > 0)
                IndexAppend(appendStart, newEnd);

            _horizonEnd = newEnd;
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                $"DateMatrixBuilder: Extended matrix by {added} rows to {newEnd:yyyy-MM-dd}");
        }

        private static void BuildInMemory(DateTime startDate, DateTime endDate)
        {
            var rules = new CityPrepDaysTbl().GetAll();
            if (rules == null || rules.Count == 0)
                return;

            var byCity = rules.GroupBy(r => r.CityID).ToDictionary(g => g.Key, g => g.ToList());
            var today = TimeZoneUtils.Now().Date;

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var dow = (int)day.DayOfWeek;
                foreach (var kv in byCity)
                {
                    foreach (var rule in kv.Value)
                    {
                        // Adjust if DB uses 1..7: if ((rule.PrepDayOfWeekID - 1) == dow)
                        if (rule.PrepDayOfWeekID == dow)
                        {
                            var delivery = day.AddDays(rule.DeliveryDelayDays).Date;
                            if (delivery < today) continue;
                            _rows.Add(new DateMatrixRow
                            {
                                CityID = kv.Key,
                                PrepDate = day,
                                DeliveryDate = delivery,
                                PrepDayOfWeek = dow
                            });
                        }
                    }
                }
            }
        }
        private static bool BuildViaSql(DateTime startDate, DateTime endDate)
        {
            try
            {
                int windowDays = (int)(endDate - startDate).TotalDays;
                if (windowDays < 0) windowDays = 0;

                string sql = @"
SELECT 
    cp.CityID,
    DateAdd('d', n.N, ?) AS PrepDate,
    DateAdd('d', n.N + cp.DeliveryDelayDays, ?) AS DeliveryDate,
    (Weekday(DateAdd('d', n.N, ?), 1) - 1) AS PrepDayOfWeek,
    cp.PrepDayOfWeekID
FROM CityPrepDaysTbl cp
INNER JOIN Numbers n ON n.N BETWEEN 0 AND ?
WHERE (Weekday(DateAdd('d', n.N, ?), 1) - 1) = cp.PrepDayOfWeekID
ORDER BY cp.CityID, PrepDate";

                using (var db = new TrackerDb())
                {
                    db.AddWhereParams(startDate, DbType.Date);
                    db.AddWhereParams(startDate, DbType.Date);
                    db.AddWhereParams(startDate, DbType.Date);
                    db.AddWhereParams(windowDays, DbType.Int32);
                    db.AddWhereParams(startDate, DbType.Date);

                    using (var rdr = db.ExecuteSQLGetDataReader(sql))  //uses standard and where params
                    {
                        var today = TimeZoneUtils.Now().Date;
                        while (rdr != null && rdr.Read())
                        {
                            DateTime prep = Convert.ToDateTime(rdr["PrepDate"]);
                            DateTime delivery = Convert.ToDateTime(rdr["DeliveryDate"]);
                            if (delivery < today) continue;

                            _rows.Add(new DateMatrixRow
                            {
                                CityID = Convert.ToInt32(rdr["CityID"]),
                                PrepDate = prep.Date,
                                DeliveryDate = delivery.Date,
                                PrepDayOfWeek = Convert.ToInt32(rdr["PrepDayOfWeek"])
                            });
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"DateMatrixBuilder: SQL build error: {ex.Message}");
                return false;
            }
        }

        private static int BuildSegment(DateTime startInclusive, DateTime endInclusive)
        {
            int before = _rows.Count;
            BuildInMemory(startInclusive, endInclusive);
            return _rows.Count - before;
        }

        private static void IndexRows()
        {
            _rowsByCity = _rows
                .GroupBy(r => r.CityID)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DeliveryDate).ToList());
        }

        private static void IndexAppend(DateTime start, DateTime end)
        {
            // Re-index only appended subset
            var appended = _rows.Where(r => r.DeliveryDate >= start && r.DeliveryDate <= end).ToList();
            foreach (var g in appended.GroupBy(r => r.CityID))
            {
                if (!_rowsByCity.TryGetValue(g.Key, out var list))
                {
                    _rowsByCity[g.Key] = g.OrderBy(x => x.DeliveryDate).ToList();
                }
                else
                {
                    list.AddRange(g);
                    list.Sort((a, b) => a.DeliveryDate.CompareTo(b.DeliveryDate));
                }
            }
        }

        // ----- Source Change Detection -----
        private static DateTime GetSysPrepCalcDate()
        {
            try
            {
                using (var db = new TrackerDb())
                {
                    var rdr = db.ExecuteSQLGetDataReader("SELECT DateLastPrepDateCalcd FROM SysDataTbl WHERE ID=1");
                    if (rdr != null && rdr.Read())
                    {
                        var valObj = rdr["DateLastPrepDateCalcd"];
                        if (valObj != DBNull.Value)
                        {
                            var d = Convert.ToDateTime(valObj);
                            rdr.Close();
                            return d;
                        }
                        rdr.Close();
                    }
                }
            }
            catch { }
            return DateTime.MinValue;
        }

        // ----- Helpers for settings -----
        private static int GetSettingInt(string key, int defaultVal)
        {
            try
            {
                var s = ConfigurationManager.AppSettings[key];
                if (int.TryParse(s, out int v) && v >= 0) return v;
            }
            catch { }
            return defaultVal;
        }

        private static bool GetSettingBool(string key, bool defaultVal)
        {
            try
            {
                var s = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(s)) return defaultVal;
                return s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                       s.Equals("1") ||
                       s.Equals("yes", StringComparison.OrdinalIgnoreCase);
            }
            catch { return defaultVal; }
        }
    }
}