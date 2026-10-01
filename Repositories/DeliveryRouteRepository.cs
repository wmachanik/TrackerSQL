using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// DeliveryRouteTbl: the order a driver set in the app for a day (one row per day, driver and contact; Seq 1 = first).
    /// It comes before the learned order and the GPS route.
    /// </summary>
    public class DeliveryRouteRepository
    {
        private static bool _tableSeen;

        /// <summary>Whether DeliveryRouteTbl exists; only a positive answer is cached.</summary>
        public static bool TableSeen()
        {
            if (_tableSeen)
                return true;
            try
            {
                using (var db = new TrackerSQLDb())
                    _tableSeen = db.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID(N'dbo.DeliveryRouteTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                _tableSeen = false;
            }
            return _tableSeen;
        }

        /// <summary>Route sorting: dr.PlanSeq (the contact's place in the driver's own order for the day) and dr.PlanStops.</summary>
        public static string PlanJoinSql(string contactAlias, string dayExpr, string driverExpr)
        {
            return @"
                OUTER APPLY (
                    SELECT (SELECT TOP 1 rp.Seq FROM dbo.DeliveryRouteTbl rp
                            WHERE rp.RouteDate = " + dayExpr + @" AND rp.PersonID = " + driverExpr + @"
                              AND rp.ContactID = " + contactAlias + @".ContactID) AS PlanSeq,
                           (SELECT COUNT(*) FROM dbo.DeliveryRouteTbl rc
                            WHERE rc.RouteDate = " + dayExpr + @" AND rc.PersonID = " + driverExpr + @") AS PlanStops
                ) dr";
        }

        /// <summary>Replaces the driver's order for the day (contacts first to last); an empty list goes back to the suggested order.</summary>
        public void Save(DateTime day, int personId, IList<int> contactIds, string setBy)
        {
            List<int> contacts = contactIds.Distinct().ToList();
            var ps = new List<DBParameter>
            {
                P("@Day", day.Date, DbType.Date),
                P("@PersonID", personId, DbType.Int32),
                P("@SetBy", (object)setBy ?? DBNull.Value, DbType.String)
            };
            var values = new List<string>();
            for (int i = 0; i < contacts.Count; i++)
            {
                values.Add("(@Day, @PersonID, @C" + i + ", " + (i + 1) + ", @SetBy)");
                ps.Add(P("@C" + i, contacts[i], DbType.Int32));
            }
            string sql = @"
SET XACT_ABORT ON;
BEGIN TRAN;
DELETE FROM dbo.DeliveryRouteTbl WHERE RouteDate = @Day AND PersonID = @PersonID;"
                + (values.Count > 0
                    ? "\nINSERT INTO dbo.DeliveryRouteTbl (RouteDate, PersonID, ContactID, Seq, SetBy) VALUES " + string.Join(", ", values) + ";"
                    : string.Empty) + @"
COMMIT;";
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(sql, ps);
        }

        /// <summary>Order → contact for the orders that are due on the day.</summary>
        public Dictionary<int, int> ContactsForOrders(DateTime day, IList<int> orderIds)
        {
            var map = new Dictionary<int, int>();
            List<int> ids = orderIds.Where(id => id > 0).Distinct().Take(500).ToList();
            if (ids.Count == 0)
                return map;
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(@"
SELECT DISTINCT o.OrderID, o.ContactID FROM dbo.OrdersTbl o
WHERE CAST(o.RequiredByDate AS DATE) = @Day AND o.ContactID IS NOT NULL AND o.OrderID IN (" + string.Join(",", ids) + ")",
                new List<DBParameter> { P("@Day", day.Date, DbType.Date) }))
            {
                while (r != null && r.Read())
                    map[Convert.ToInt32(r["OrderID"])] = Convert.ToInt32(r["ContactID"]);
            }
            return map;
        }

        /// <summary>The driver's order for the day: contact → place (1 = first), keyed "person:contact".</summary>
        public Dictionary<string, int> GetDay(DateTime day)
        {
            var map = new Dictionary<string, int>();
            if (!TableSeen())
                return map;
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader("SELECT PersonID, ContactID, Seq FROM dbo.DeliveryRouteTbl WHERE RouteDate = @Day",
                new List<DBParameter> { P("@Day", day.Date, DbType.Date) }))
            {
                while (r != null && r.Read())
                    map[Convert.ToInt32(r["PersonID"]) + ":" + Convert.ToInt32(r["ContactID"])] = Convert.ToInt32(r["Seq"]);
            }
            return map;
        }

        private static DBParameter P(string name, object value, DbType type)
        {
            return new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
        }
    }
}
