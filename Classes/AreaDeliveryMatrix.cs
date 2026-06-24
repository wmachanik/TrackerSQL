using System;
using System.Collections.Generic;
using TrackerSQL.Repositories;

namespace TrackerSQL.Classes
{
    public class AreaDeliveryMatrixRow
    {
        public int AreaID { get; set; }
        public DateTime PrepDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime NextPreperationDate { get; set; }
        public DateTime NextDeliveryDate { get; set; }
    }

    public static class AreaDeliveryMatrix
    {
        private static readonly object _lock = new object();
        private static Dictionary<int, AreaDeliveryMatrixRow> _rows = new Dictionary<int, AreaDeliveryMatrixRow>();
        private static DateTime _builtAtUtc = DateTime.MinValue;
        private static TimeSpan _ttl = TimeSpan.FromMinutes(
            ConfigHelper.GetInt("AreaDeliveryMatrixTTLMinutes", 10));

        public static IReadOnlyDictionary<int, AreaDeliveryMatrixRow> Rows
        {
            get { lock (_lock) return new Dictionary<int, AreaDeliveryMatrixRow>(_rows); }
        }

        public static void EnsureBuilt()
        {
            lock (_lock)
            {
                if (_rows.Count == 0 || DateTime.UtcNow - _builtAtUtc > _ttl)
                    Build();
            }
        }

        public static AreaDeliveryMatrixRow Get(int AreaId)
        {
            EnsureBuilt();
            lock (_lock)
            {
                AreaDeliveryMatrixRow row;
                return _rows.TryGetValue(AreaId, out row) ? row : null;
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _rows.Clear();
                _builtAtUtc = DateTime.MinValue;
            }
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "AreaDeliveryMatrix: Cleared");
        }

        private static void Build()
        {
            try
            {
                var tools = new TrackerTools();
                tools.SetNextPreperationDateByArea();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    "AreaDeliveryMatrix: SetNextPreperationDateByArea failed: " + ex.Message);
            }

            var repo = new NextPrepDateByAreaRepository();
            var list = repo.GetAll("AreaID");
            var dict = new Dictionary<int, AreaDeliveryMatrixRow>();

            foreach (var r in list)
            {
                dict[r.AreaID] = new AreaDeliveryMatrixRow
                {
                    AreaID = r.AreaID,
                    PrepDate = r.PreperationDate ?? DateTime.MinValue,
                    DeliveryDate = r.DeliveryDate ?? DateTime.MinValue,
                    NextPreperationDate = r.NextPreperationDate ?? DateTime.MinValue,
                    NextDeliveryDate = r.NextDeliveryDate ?? DateTime.MinValue
                };
            }

            _rows = dict;
            _builtAtUtc = DateTime.UtcNow;

            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                $"AreaDeliveryMatrix: Built {dict.Count} Area rows (TTL={_ttl.TotalMinutes}m)");
        }

        public static (DateTime prep, DateTime delivery)? ChooseClosest(int AreaId, DateTime targetDelivery)
        {
            var row = Get(AreaId);
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

            DateTime prep = chosen == row.DeliveryDate ? row.PrepDate : row.NextPreperationDate;
            return (prep.Date, chosen.Date);
        }

        /// <summary>
        /// Returns a copy of the current cached rows (what the scheduling logic is using).
        /// No DB hit; call Clear() first if you need a rebuild before inspecting.
        /// </summary>
        public static List<AreaDeliveryMatrixRow> GetSnapshot()
        {
            EnsureBuilt();
            lock (_lock)
            {
                var list = new List<AreaDeliveryMatrixRow>(_rows.Count);
                foreach (var _row in _rows)
                    list.Add(_row.Value);
                list.Sort((a, b) => a.AreaID.CompareTo(b.AreaID));
                return list;
            }
        }
    }
}
