using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// DeliveryRouteTbl: the order a driver set in the app for a day (one row per day, driver and stop; Seq 1 = first).
    /// StopKey 0 is the contact's own address. A ZZName order uses the order id, and an alternative address uses that address id,
    /// so two guests or two sites of one contact can sit in different places. It comes before the learned order and the GPS route.
    /// </summary>
    public class DeliveryRouteRepository
    {
        /// <summary>SetBy ends with this when the order was set on the office delivery sheet (not by the driver in the app).</summary>
        public const string OfficeSuffix = " (office)";

        public struct StopRef
        {
            public int ContactId;
            public int StopKey;
        }

        /// <summary>
        /// 0 for the contact's own address. ZZName uses the order id. Any other alternative address uses that address id.
        /// </summary>
        public static int StopKeyFor(int contactId, int? deliverToAddressId, int orderId)
        {
            if (WalkInOrderSite.IsWalkIn(contactId))
                return orderId > 0 ? orderId : 0;
            if (deliverToAddressId.HasValue && deliverToAddressId.Value > 0)
                return deliverToAddressId.Value;
            return 0;
        }

        /// <summary>Groups route stops: one contact's own address stays together; each other site is its own stop.</summary>
        public static long SiteKey(int contactId, int? deliverToAddressId, int orderId)
        {
            return ((long)contactId << 32) | (uint)StopKeyFor(contactId, deliverToAddressId, orderId);
        }

        /// <summary>SQL for <see cref="StopKeyFor"/> on an order row (o / c).</summary>
        public static string StopKeySql(string orderAlias, string contactAlias)
        {
            string zz = SystemConstants.CustomerConstants.SundryCustomerID.ToString();
            if (!OrdersRepository.HasDeliverToAddressIdColumn())
                return "CASE WHEN " + contactAlias + ".ContactID = " + zz + " THEN " + orderAlias + ".OrderID ELSE 0 END";
            return "CASE WHEN " + contactAlias + ".ContactID = " + zz + " THEN " + orderAlias + ".OrderID"
                + " WHEN " + orderAlias + ".DeliverToAddressID IS NOT NULL AND " + orderAlias + ".DeliverToAddressID > 0 THEN " + orderAlias + ".DeliverToAddressID"
                + " ELSE 0 END";
        }

        public class PlanInfo
        {
            public int PersonID;
            public string SetBy;
            public DateTime SetAt;
            public bool ByOffice => (SetBy ?? string.Empty).EndsWith(OfficeSuffix, StringComparison.Ordinal);
            /// <summary>The user name without the office suffix.</summary>
            public string UserName => ByOffice ? SetBy.Substring(0, SetBy.Length - OfficeSuffix.Length) : SetBy;
        }

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
            string stop = StopKeySql("o", contactAlias);
            string stopMatch = ColumnSeen("StopKey") ? " AND ISNULL(rp.StopKey, 0) = (" + stop + ")" : string.Empty;
            return @"
                OUTER APPLY (
                    SELECT (SELECT TOP 1 rp.Seq FROM dbo.DeliveryRouteTbl rp
                            WHERE rp.RouteDate = " + dayExpr + @" AND rp.PersonID = " + driverExpr + @"
                              AND rp.ContactID = " + contactAlias + @".ContactID" + stopMatch + @") AS PlanSeq,
                           (SELECT COUNT(*) FROM dbo.DeliveryRouteTbl rc
                            WHERE rc.RouteDate = " + dayExpr + @" AND rc.PersonID = " + driverExpr + @") AS PlanStops
                ) dr";
        }

        /// <summary>
        /// Adds StopKey and includes it in the primary key when that has not been done yet.
        /// The two steps are separate batches: SQL Server compiles a batch before it runs.
        /// </summary>
        public static bool EnsureStopKey()
        {
            if (_stopKeyReady == true)
                return true;
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    db.ExecuteNonQuery(@"
IF OBJECT_ID(N'dbo.DeliveryRouteTbl', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.DeliveryRouteTbl', N'StopKey') IS NULL
    ALTER TABLE dbo.DeliveryRouteTbl ADD StopKey INT NOT NULL CONSTRAINT DF_DeliveryRoute_StopKey DEFAULT (0);");
                    db.ExecuteNonQuery(@"
IF OBJECT_ID(N'dbo.DeliveryRouteTbl', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.DeliveryRouteTbl', N'StopKey') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.index_columns ic
        INNER JOIN sys.key_constraints kc ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
        INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE kc.parent_object_id = OBJECT_ID(N'dbo.DeliveryRouteTbl') AND kc.type = 'PK' AND c.name = N'StopKey')
BEGIN
    DECLARE @pk sysname;
    SELECT @pk = kc.name FROM sys.key_constraints kc
    WHERE kc.parent_object_id = OBJECT_ID(N'dbo.DeliveryRouteTbl') AND kc.type = 'PK';
    IF @pk IS NOT NULL
        EXEC(N'ALTER TABLE dbo.DeliveryRouteTbl DROP CONSTRAINT ' + QUOTENAME(@pk));
    ALTER TABLE dbo.DeliveryRouteTbl ADD CONSTRAINT PK_DeliveryRouteTbl PRIMARY KEY (RouteDate, PersonID, ContactID, StopKey);
END");
                    _stopKeyReady = db.ExecuteScalar<int>(@"
SELECT CASE WHEN COL_LENGTH(N'dbo.DeliveryRouteTbl', N'StopKey') IS NULL THEN 0
WHEN EXISTS (
    SELECT 1 FROM sys.index_columns ic
    INNER JOIN sys.key_constraints kc ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
    INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE kc.parent_object_id = OBJECT_ID(N'dbo.DeliveryRouteTbl') AND kc.type = 'PK' AND c.name = N'StopKey')
THEN 1 ELSE 0 END") == 1;
                    _stopKeySeen = _stopKeyReady;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Delivery route StopKey was not added: " + ex.Message);
                _stopKeyReady = false;
            }
            return _stopKeyReady == true;
        }

        private static bool? _stopKeyReady;

        private static bool? _stopKeySeen;

        public static bool ColumnSeen(string column)
        {
            if (column == "StopKey" && _stopKeySeen.HasValue)
                return _stopKeySeen.Value;
            bool seen = false;
            try
            {
                using (var db = new TrackerSQLDb())
                    seen = db.ExecuteScalar<int>("SELECT CASE WHEN COL_LENGTH(N'dbo.DeliveryRouteTbl', N'" + column + "') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                seen = false;
            }
            if (column == "StopKey")
                _stopKeySeen = seen;
            return seen;
        }

        /// <summary>Replaces the driver's order for the day (stops first to last); an empty list goes back to the suggested order.</summary>
        public void Save(DateTime day, int personId, IList<StopRef> stops, string setBy)
        {
            var unique = new List<StopRef>();
            var seen = new HashSet<string>();
            if (stops != null)
            {
                foreach (StopRef stop in stops)
                {
                    if (stop.ContactId <= 0)
                        continue;
                    string id = stop.ContactId + ":" + stop.StopKey;
                    if (!seen.Add(id))
                        continue;
                    unique.Add(stop);
                }
            }
            bool stopKey = EnsureStopKey();
            var ps = new List<DBParameter>
            {
                P("@Day", day.Date, DbType.Date),
                P("@PersonID", personId, DbType.Int32),
                P("@SetBy", (object)setBy ?? DBNull.Value, DbType.String),
                P("@SetAt", TimeZoneUtils.Now(), DbType.DateTime)
            };
            var values = new List<string>();
            for (int i = 0; i < unique.Count; i++)
            {
                values.Add(stopKey
                    ? "(@Day, @PersonID, @C" + i + ", @K" + i + ", " + (i + 1) + ", @SetAt, @SetBy)"
                    : "(@Day, @PersonID, @C" + i + ", " + (i + 1) + ", @SetAt, @SetBy)");
                ps.Add(P("@C" + i, unique[i].ContactId, DbType.Int32));
                if (stopKey)
                    ps.Add(P("@K" + i, unique[i].StopKey, DbType.Int32));
            }
            string sql = @"
SET XACT_ABORT ON;
BEGIN TRAN;
DELETE FROM dbo.DeliveryRouteTbl WHERE RouteDate = @Day AND PersonID = @PersonID;"
                + (values.Count > 0
                    ? "\nINSERT INTO dbo.DeliveryRouteTbl (RouteDate, PersonID, ContactID, "
                      + (stopKey ? "StopKey, " : string.Empty)
                      + "Seq, SetAt, SetBy) VALUES " + string.Join(", ", values) + ";"
                    : string.Empty) + @"
COMMIT;";
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(sql, ps);
        }

        /// <summary>Order → stop for the orders that are due on the day.</summary>
        public Dictionary<int, StopRef> StopsForOrders(DateTime day, IList<int> orderIds)
        {
            var map = new Dictionary<int, StopRef>();
            List<int> ids = orderIds.Where(id => id > 0).Distinct().Take(500).ToList();
            if (ids.Count == 0)
                return map;
            bool address = OrdersRepository.HasDeliverToAddressIdColumn();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(@"
SELECT DISTINCT o.OrderID, o.ContactID" + (address ? ", o.DeliverToAddressID" : string.Empty) + @" FROM dbo.OrdersTbl o
WHERE CAST(o.RequiredByDate AS DATE) = @Day AND o.ContactID IS NOT NULL AND o.OrderID IN (" + string.Join(",", ids) + ")",
                new List<DBParameter> { P("@Day", day.Date, DbType.Date) }))
            {
                while (r != null && r.Read())
                {
                    int orderId = Convert.ToInt32(r["OrderID"]);
                    int contactId = Convert.ToInt32(r["ContactID"]);
                    int? addressId = address && r["DeliverToAddressID"] != DBNull.Value ? (int?)Convert.ToInt32(r["DeliverToAddressID"]) : null;
                    map[orderId] = new StopRef { ContactId = contactId, StopKey = StopKeyFor(contactId, addressId, orderId) };
                }
            }
            return map;
        }

        /// <summary>The driver's order for the day: place (1 = first), keyed "person:contact" and "person:contact:stopKey".</summary>
        public Dictionary<string, int> GetDay(DateTime day)
        {
            var map = new Dictionary<string, int>();
            if (!TableSeen())
                return map;
            bool stopKey = ColumnSeen("StopKey");
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader("SELECT PersonID, ContactID, Seq" + (stopKey ? ", StopKey" : ", 0 AS StopKey") + " FROM dbo.DeliveryRouteTbl WHERE RouteDate = @Day",
                new List<DBParameter> { P("@Day", day.Date, DbType.Date) }))
            {
                while (r != null && r.Read())
                {
                    int person = Convert.ToInt32(r["PersonID"]);
                    int contact = Convert.ToInt32(r["ContactID"]);
                    int key = Convert.ToInt32(r["StopKey"]);
                    int seq = Convert.ToInt32(r["Seq"]);
                    map[person + ":" + contact + ":" + key] = seq;
                    if (key == 0)
                        map[person + ":" + contact] = seq;
                }
            }
            return map;
        }

        /// <summary>Who set each driver's order for the day, and when (drivers without one are left out).</summary>
        public Dictionary<int, PlanInfo> GetPlanInfo(DateTime day)
        {
            var map = new Dictionary<int, PlanInfo>();
            if (!TableSeen())
                return map;
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(
                "SELECT PersonID, MAX(SetBy) AS SetBy, MAX(SetAt) AS SetAt FROM dbo.DeliveryRouteTbl WHERE RouteDate = @Day GROUP BY PersonID",
                new List<DBParameter> { P("@Day", day.Date, DbType.Date) }))
            {
                while (r != null && r.Read())
                {
                    int personId = Convert.ToInt32(r["PersonID"]);
                    map[personId] = new PlanInfo
                    {
                        PersonID = personId,
                        SetBy = r["SetBy"] == DBNull.Value ? null : r["SetBy"].ToString(),
                        SetAt = Convert.ToDateTime(r["SetAt"])
                    };
                }
            }
            return map;
        }

        private static DBParameter P(string name, object value, DbType type)
        {
            return new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
        }
    }
}
