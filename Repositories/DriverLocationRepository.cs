using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;

namespace TrackerSQL.Repositories
{
    /// <summary>DriverLocationPingTbl: positions sent by the driver app (and the position of each delivery).</summary>
    public class DriverLocationRepository
    {
        public const string SourceAppOpen = "APP_OPEN";
        public const string SourcePeriodic = "PERIODIC";
        public const string SourceDeliveryComplete = "DELIVERY_COMPLETE";

        public class Ping
        {
            public int PersonID;
            public decimal Latitude;
            public decimal Longitude;
            public decimal? AccuracyM;
            public DateTime PingTime;
            public string Source;
        }

        private static bool _tableSeen;

        /// <summary>Whether DriverLocationPingTbl exists; only a positive answer is cached.</summary>
        public static bool TableSeen()
        {
            if (_tableSeen)
                return true;
            try
            {
                using (var db = new TrackerSQLDb())
                    _tableSeen = db.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID(N'dbo.DriverLocationPingTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                _tableSeen = false;
            }
            return _tableSeen;
        }

        public void Insert(Ping ping, string userName)
        {
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(@"
INSERT INTO dbo.DriverLocationPingTbl (PersonID, Latitude, Longitude, AccuracyM, PingTime, Source, UserName)
VALUES (@PersonID, @Lat, @Lng, @Acc, @At, @Source, @UserName)", new List<DBParameter>
                {
                    P("@PersonID", ping.PersonID, DbType.Int32),
                    P("@Lat", ping.Latitude, DbType.Decimal),
                    P("@Lng", ping.Longitude, DbType.Decimal),
                    P("@Acc", ping.AccuracyM, DbType.Decimal),
                    P("@At", ping.PingTime, DbType.DateTime),
                    P("@Source", ping.Source, DbType.AnsiString),
                    P("@UserName", userName, DbType.String)
                });
        }

        /// <summary>Each driver's latest position on the day (up to <paramref name="upTo"/>).</summary>
        public List<Ping> LatestPerPerson(DateTime day, DateTime upTo)
        {
            const string sql = @"
SELECT PersonID, Latitude, Longitude, AccuracyM, PingTime, Source
FROM (
    SELECT PersonID, Latitude, Longitude, AccuracyM, PingTime, Source,
           ROW_NUMBER() OVER (PARTITION BY PersonID ORDER BY PingTime DESC, PingID DESC) AS rn
    FROM dbo.DriverLocationPingTbl
    WHERE PingTime >= @From AND PingTime < @To AND PingTime <= @UpTo
) x
WHERE rn = 1";
            var list = new List<Ping>();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql, new List<DBParameter>
            {
                P("@From", day.Date, DbType.DateTime),
                P("@To", day.Date.AddDays(1), DbType.DateTime),
                P("@UpTo", upTo, DbType.DateTime)
            }))
            {
                while (r != null && r.Read())
                {
                    list.Add(new Ping
                    {
                        PersonID = Convert.ToInt32(r["PersonID"]),
                        Latitude = Convert.ToDecimal(r["Latitude"]),
                        Longitude = Convert.ToDecimal(r["Longitude"]),
                        AccuracyM = r["AccuracyM"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["AccuracyM"]),
                        PingTime = Convert.ToDateTime(r["PingTime"]),
                        Source = r["Source"].ToString()
                    });
                }
            }
            return list;
        }

        /// <summary>Every position on the day, oldest first.</summary>
        public List<Ping> ForDay(DateTime day)
        {
            var list = new List<Ping>();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(@"
SELECT PersonID, Latitude, Longitude, AccuracyM, PingTime, Source
FROM dbo.DriverLocationPingTbl
WHERE PingTime >= @From AND PingTime < @To
ORDER BY PingTime, PingID", new List<DBParameter>
            {
                P("@From", day.Date, DbType.DateTime),
                P("@To", day.Date.AddDays(1), DbType.DateTime)
            }))
            {
                while (r != null && r.Read())
                {
                    list.Add(new Ping
                    {
                        PersonID = Convert.ToInt32(r["PersonID"]),
                        Latitude = Convert.ToDecimal(r["Latitude"]),
                        Longitude = Convert.ToDecimal(r["Longitude"]),
                        AccuracyM = r["AccuracyM"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["AccuracyM"]),
                        PingTime = Convert.ToDateTime(r["PingTime"]),
                        Source = r["Source"].ToString()
                    });
                }
            }
            return list;
        }

        /// <summary>Deletes positions older than the given number of days. Returns the rows deleted.</summary>
        public int DeleteOlderThan(int days)
        {
            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(
                    "DELETE FROM dbo.DriverLocationPingTbl WHERE PingTime < DATEADD(DAY, -@Days, CAST(GETDATE() AS DATE))",
                    new List<DBParameter> { P("@Days", Math.Max(1, days), DbType.Int32) });
        }

        private static DBParameter P(string name, object value, DbType type)
        {
            return new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
        }
    }
}
