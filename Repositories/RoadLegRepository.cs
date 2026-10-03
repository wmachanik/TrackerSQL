using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TrackerSQL.Classes;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// RoadLegTbl (SQLCommands-DriverGps-06.xml): driving seconds and metres from one geohash point to another, fetched from
    /// the HeiGIT matrix and reused on later days. -1 = HeiGIT found no road between them.
    /// </summary>
    public class RoadLegRepository
    {
        /// <summary>Legs older than this are fetched again (roads change).</summary>
        public const int KeepDays = 180;

        private static readonly Regex HashPattern = new Regex("^[0-9b-hjkmnp-z]{1,12}$", RegexOptions.Compiled);
        private static bool _tableSeen;

        public class Leg
        {
            public string FromHash;
            public string ToHash;
            public int Seconds;
            public int Metres;
        }

        /// <summary>Whether RoadLegTbl exists; only a positive answer is cached.</summary>
        public static bool TableSeen()
        {
            if (_tableSeen)
                return true;
            try
            {
                using (var db = new TrackerSQLDb())
                    _tableSeen = db.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID(N'dbo.RoadLegTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                _tableSeen = false;
            }
            return _tableSeen;
        }

        /// <summary>A geohash safe to put in SQL text (base-32 letters and digits only).</summary>
        public static bool IsValidHash(string hash) => !string.IsNullOrEmpty(hash) && HashPattern.IsMatch(hash);

        /// <summary>The stored legs between any two of <paramref name="hashes"/> that are newer than <see cref="KeepDays"/>.</summary>
        public List<Leg> Between(IEnumerable<string> hashes)
        {
            List<string> list = hashes.Where(IsValidHash).Distinct().ToList();
            var legs = new List<Leg>();
            if (list.Count < 2)
                return legs;
            string inList = string.Join(",", list.Select(h => "'" + h + "'"));
            string sql = "SELECT FromHash, ToHash, Seconds, Metres FROM dbo.RoadLegTbl WHERE FromHash IN (" + inList + ") AND ToHash IN (" + inList
                + ") AND FetchedAt >= DATEADD(DAY, -" + KeepDays + ", GETDATE())";
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql))
            {
                while (r != null && r.Read())
                {
                    legs.Add(new Leg
                    {
                        FromHash = Convert.ToString(r["FromHash"]),
                        ToHash = Convert.ToString(r["ToHash"]),
                        Seconds = Convert.ToInt32(r["Seconds"]),
                        Metres = Convert.ToInt32(r["Metres"])
                    });
                }
            }
            return legs;
        }

        /// <summary>Adds or refreshes the legs (in batches of 500).</summary>
        public void Save(IList<Leg> legs)
        {
            List<Leg> valid = legs.Where(l => IsValidHash(l.FromHash) && IsValidHash(l.ToHash) && l.FromHash != l.ToHash).ToList();
            for (int start = 0; start < valid.Count; start += 500)
            {
                var values = new StringBuilder();
                foreach (Leg leg in valid.Skip(start).Take(500))
                {
                    if (values.Length > 0) values.Append(',');
                    values.Append("('").Append(leg.FromHash).Append("','").Append(leg.ToHash).Append("',")
                        .Append(leg.Seconds).Append(',').Append(leg.Metres).Append(')');
                }
                string sql = @"
MERGE dbo.RoadLegTbl AS t
USING (VALUES " + values + @") AS s (FromHash, ToHash, Seconds, Metres)
    ON t.FromHash = s.FromHash AND t.ToHash = s.ToHash
WHEN MATCHED THEN UPDATE SET Seconds = s.Seconds, Metres = s.Metres, FetchedAt = GETDATE()
WHEN NOT MATCHED THEN INSERT (FromHash, ToHash, Seconds, Metres, FetchedAt) VALUES (s.FromHash, s.ToHash, s.Seconds, s.Metres, GETDATE());";
                using (var db = new TrackerSQLDb())
                    db.ExecuteNonQuery(sql);
            }
        }

        /// <summary>How many legs are stored (for System Preferences).</summary>
        public int Count()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RoadLegTbl");
            }
            catch
            {
                return 0;
            }
        }
    }
}
