using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// DeliveryVisitTbl: the order drivers really deliver in, one row per day, driver and contact (several orders at one
    /// stop are one visit). Position = the nth stop that day by the first proof's time; Stops = the driver's stops that day.
    /// </summary>
    public class DeliveryVisitRepository
    {
        /// <summary>Visits used for a contact's learned place in the route (most recent first, the route's driver first).</summary>
        public const int LearnFromVisits = 10;
        /// <summary>Days with fewer stops say little about the order and are not learnt from.</summary>
        public const int MinStopsToLearn = 3;

        public class Visit
        {
            public DateTime VisitDate;
            public int PersonID;
            public string DriverAbbreviation;
            public int ContactID;
            public string CompanyName;
            public string AreaName;
            public DateTime FirstAt;
            public DateTime LastAt;
            public int? Position;
            public int Stops;
        }

        private static bool _tableSeen;

        /// <summary>Whether DeliveryVisitTbl exists; only a positive answer is cached.</summary>
        public static bool TableSeen()
        {
            if (_tableSeen)
                return true;
            try
            {
                using (var db = new TrackerSQLDb())
                    _tableSeen = db.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID(N'dbo.DeliveryVisitTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                _tableSeen = false;
            }
            return _tableSeen;
        }

        /// <summary>
        /// Route sorting: the contact's learned place in the route (dv.LearnedPos, 0 = first stop .. 1 = last) from its last
        /// visits before the sheet's day, and how many visits that is (dv.LearnedVisits).
        /// </summary>
        public static string LearnedJoinSql(string contactAlias, string dayExpr, string driverExpr)
        {
            return @"
                OUTER APPLY (
                    SELECT COUNT(*) AS LearnedVisits,
                           AVG(CAST(lv.Position - 1 AS FLOAT) / (lv.Stops - 1)) AS LearnedPos
                    FROM (SELECT TOP (" + LearnFromVisits + @") v.Position, v.Stops
                          FROM dbo.DeliveryVisitTbl v
                          WHERE v.ContactID = " + contactAlias + @".ContactID
                            AND v.VisitDate < " + dayExpr + @"
                            AND v.Position IS NOT NULL AND v.Stops >= " + MinStopsToLearn + @"
                          ORDER BY CASE WHEN v.PersonID = " + driverExpr + @" THEN 0 ELSE 1 END, v.VisitDate DESC) lv
                ) dv";
        }

        /// <summary>
        /// Records (or moves earlier) the visit, then renumbers the driver's stops for the day by time and sets Stops to the
        /// larger of the stops on the sheet and the visits so far. Returns the visit's position and the day's stop count.
        /// </summary>
        public Tuple<int, int> RecordVisit(DateTime visitDate, int personId, int contactId, DateTime at)
        {
            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;
IF EXISTS (SELECT 1 FROM dbo.DeliveryVisitTbl WITH (UPDLOCK, HOLDLOCK)
           WHERE VisitDate = @VisitDate AND PersonID = @PersonID AND ContactID = @ContactID)
    UPDATE dbo.DeliveryVisitTbl
    SET FirstAt = CASE WHEN @At < FirstAt THEN @At ELSE FirstAt END,
        LastAt = CASE WHEN @At > LastAt THEN @At ELSE LastAt END
    WHERE VisitDate = @VisitDate AND PersonID = @PersonID AND ContactID = @ContactID;
ELSE
    INSERT INTO dbo.DeliveryVisitTbl (VisitDate, PersonID, ContactID, FirstAt, LastAt, Stops)
    VALUES (@VisitDate, @PersonID, @ContactID, @At, @At, 0);

;WITH r AS (
    SELECT Position, ROW_NUMBER() OVER (ORDER BY FirstAt, ContactID) AS rn
    FROM dbo.DeliveryVisitTbl WHERE VisitDate = @VisitDate AND PersonID = @PersonID)
UPDATE r SET Position = rn;

DECLARE @Visits INT = (SELECT COUNT(*) FROM dbo.DeliveryVisitTbl WHERE VisitDate = @VisitDate AND PersonID = @PersonID);
DECLARE @Planned INT = (SELECT COUNT(DISTINCT o.ContactID) FROM dbo.OrdersTbl o
                        WHERE o.ToBeDeliveredByID = @PersonID AND CAST(o.RequiredByDate AS DATE) = @VisitDate);
UPDATE dbo.DeliveryVisitTbl SET Stops = CASE WHEN @Planned > @Visits THEN @Planned ELSE @Visits END
WHERE VisitDate = @VisitDate AND PersonID = @PersonID;
COMMIT;

SELECT Position, Stops FROM dbo.DeliveryVisitTbl
WHERE VisitDate = @VisitDate AND PersonID = @PersonID AND ContactID = @ContactID;";

            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql, new List<DBParameter>
            {
                P("@VisitDate", visitDate.Date, DbType.Date),
                P("@PersonID", personId, DbType.Int32),
                P("@ContactID", contactId, DbType.Int32),
                P("@At", at, DbType.DateTime)
            }))
            {
                if (r == null || !r.Read())
                    return Tuple.Create(0, 0);
                return Tuple.Create(r["Position"] == DBNull.Value ? 0 : Convert.ToInt32(r["Position"]), Convert.ToInt32(r["Stops"]));
            }
        }

        /// <summary>Days with visits, newest first.</summary>
        public List<DateTime> ListDates(int max)
        {
            var dates = new List<DateTime>();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader("SELECT DISTINCT TOP (" + Math.Max(1, max) + ") VisitDate FROM dbo.DeliveryVisitTbl ORDER BY VisitDate DESC"))
            {
                while (r != null && r.Read())
                    dates.Add(Convert.ToDateTime(r["VisitDate"]).Date);
            }
            return dates;
        }

        /// <summary>The day's visits in the order they were made (optionally one driver).</summary>
        public List<Visit> ListDay(DateTime visitDate, int? personId)
        {
            string sql = @"
SELECT v.VisitDate, v.PersonID, p.Abbreviation, v.ContactID, c.CompanyName, a.AreaName,
       v.FirstAt, v.LastAt, v.Position, v.Stops
FROM dbo.DeliveryVisitTbl v
LEFT JOIN dbo.PeopleTbl p ON p.PersonID = v.PersonID
LEFT JOIN dbo.ContactsTbl c ON c.ContactID = v.ContactID
LEFT JOIN dbo.AreasTbl a ON a.AreaID = c.AreaID
WHERE v.VisitDate = @VisitDate" + (personId.HasValue ? " AND v.PersonID = @PersonID" : string.Empty) + @"
ORDER BY p.Abbreviation, v.PersonID, v.Position, v.FirstAt";
            var ps = new List<DBParameter> { P("@VisitDate", visitDate.Date, DbType.Date) };
            if (personId.HasValue)
                ps.Add(P("@PersonID", personId.Value, DbType.Int32));

            var list = new List<Visit>();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql, ps))
            {
                while (r != null && r.Read())
                {
                    list.Add(new Visit
                    {
                        VisitDate = Convert.ToDateTime(r["VisitDate"]).Date,
                        PersonID = Convert.ToInt32(r["PersonID"]),
                        DriverAbbreviation = r["Abbreviation"] == DBNull.Value ? null : r["Abbreviation"].ToString(),
                        ContactID = Convert.ToInt32(r["ContactID"]),
                        CompanyName = r["CompanyName"] == DBNull.Value ? null : r["CompanyName"].ToString(),
                        AreaName = r["AreaName"] == DBNull.Value ? null : r["AreaName"].ToString(),
                        FirstAt = Convert.ToDateTime(r["FirstAt"]),
                        LastAt = Convert.ToDateTime(r["LastAt"]),
                        Position = r["Position"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["Position"]),
                        Stops = Convert.ToInt32(r["Stops"])
                    });
                }
            }
            return list;
        }

        private static DBParameter P(string name, object value, DbType type)
        {
            return new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
        }
    }
}
