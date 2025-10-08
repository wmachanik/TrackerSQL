using System;
using System.Collections.Generic;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

namespace TrackerDotNet.Classes
{
    public class CityDeliveryMatrixRow
    {
        public int CityID { get; set; }
        public DateTime PrepDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime NextPrepDate { get; set; }
        public DateTime NextDeliveryDate { get; set; }
    }

    public static class CityDeliveryMatrix
    {
        private static readonly object _lock = new object();
        private static Dictionary<int, CityDeliveryMatrixRow> _rows = new Dictionary<int, CityDeliveryMatrixRow>();
        private static DateTime _builtAtUtc = DateTime.MinValue;
        private static TimeSpan _ttl = TimeSpan.FromMinutes(
            ConfigHelper.GetInt("CityDeliveryMatrixTTLMinutes", 10));

        public static IReadOnlyDictionary<int, CityDeliveryMatrixRow> Rows
        {
            get { lock (_lock) return new Dictionary<int, CityDeliveryMatrixRow>(_rows); }
        }

        public static void EnsureBuilt()
        {
            lock (_lock)
            {
                if (_rows.Count == 0 || DateTime.UtcNow - _builtAtUtc > _ttl)
                    Build();
            }
        }

        public static CityDeliveryMatrixRow Get(int cityId)
        {
            EnsureBuilt();
            lock (_lock)
            {
                CityDeliveryMatrixRow row;
                return _rows.TryGetValue(cityId, out row) ? row : null;
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _rows.Clear();
                _builtAtUtc = DateTime.MinValue;
            }
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "CityDeliveryMatrix: Cleared");
        }

        private static void Build()
        {
            try
            {
                var tools = new TrackerTools();
                tools.SetNextRoastDateByCity();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    "CityDeliveryMatrix: SetNextRoastDateByCity failed: " + ex.Message);
            }

            var dal = new NextRoastDateByCityTbl();
            var list = dal.GetAll("CityID");
            var dict = new Dictionary<int, CityDeliveryMatrixRow>();

            foreach (var r in list)
            {
                dict[r.CityID] = new CityDeliveryMatrixRow
                {
                    CityID = r.CityID,
                    PrepDate = r.PrepDate,
                    DeliveryDate = r.DeliveryDate,
                    NextPrepDate = r.NextPrepDate,
                    NextDeliveryDate = r.NextDeliveryDate
                };
            }

            _rows = dict;
            _builtAtUtc = DateTime.UtcNow;

            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                $"CityDeliveryMatrix: Built {dict.Count} city rows (TTL={_ttl.TotalMinutes}m)");
        }

        public static (DateTime prep, DateTime delivery)? ChooseClosest(int cityId, DateTime targetDelivery)
        {
            var row = Get(cityId);
            if (row == null) return null;

            targetDelivery = targetDelivery.Date;
            var c1 = row.DeliveryDate;
            var c2 = row.NextDeliveryDate;

            var candidates = new List<DateTime>();
            if (c1 > DateTime.MinValue) candidates.Add(c1);
            if (c2 > DateTime.MinValue && c2 != c1) candidates.Add(c2);
            if (candidates.Count == 0) return null;

            DateTime chosen = candidates[0];
            double best = Math.Abs((chosen - targetDelivery).TotalDays);
            for (int i = 1; i < candidates.Count; i++)
            {
                double diff = Math.Abs((candidates[i] - targetDelivery).TotalDays);
                if (diff < best || (diff == best && candidates[i] < chosen))
                {
                    chosen = candidates[i];
                    best = diff;
                }
            }

            DateTime prep = chosen == row.DeliveryDate ? row.PrepDate : row.NextPrepDate;
            return (prep.Date, chosen.Date);
        }

        /// <summary>
        /// Returns a copy of the current cached rows (what the scheduling logic is using).
        /// No DB hit; call Clear() first if you need a rebuild before inspecting.
        /// </summary>
        public static List<CityDeliveryMatrixRow> GetSnapshot()
        {
            EnsureBuilt();
            lock (_lock)
            {
                var list = new List<CityDeliveryMatrixRow>(_rows.Count);
                foreach (var kvp in _rows)
                    list.Add(kvp.Value);
                list.Sort((a, b) => a.CityID.CompareTo(b.CityID));
                return list;
            }
        }
    }
}