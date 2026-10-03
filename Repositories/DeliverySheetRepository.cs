using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class DeliverySheetRepository
    {
        public RepositoryListResult<ActiveDeliveryDate> GetActiveDeliveryDates()
        {
            const string sql = @"
                SELECT DISTINCT CAST(o.RequiredByDate AS DATE) AS RequiredByDate
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                WHERE o.Done = 0
                  AND o.RequiredByDate IS NOT NULL
                ORDER BY RequiredByDate";

            return ExecuteActiveDeliveryDateQuery(sql, null, "DeliverySheetRepository.GetActiveDeliveryDates");
        }

        public RepositoryListResult<DeliverySheetOrderRow> GetDeliverySheetRows(DateTime requiredByDate, int? deliveryById)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    DataValue = requiredByDate.Date,
                    DataDbType = DbType.Date,
                    ParamName = "@RequiredByDate"
                }
            };

            // Include Done orders so a day's sheet shows what was delivered vs still open
            string sql = BaseDeliverySheetSql() + @"
                WHERE CAST(o.RequiredByDate AS DATE) = @RequiredByDate";

            if (deliveryById.HasValue)
            {
                sql += @"
                AND o.ToBeDeliveredByID = @ToBeDeliveredByID";

                parameters.Add(new DBParameter
                {
                    DataValue = deliveryById.Value,
                    DataDbType = DbType.Int32,
                    ParamName = "@ToBeDeliveredByID"
                });
            }

            sql += @"
                ORDER BY
                    o.Done,
                    o.RequiredByDate,
                    o.ToBeDeliveredByID,
                    NoGeo,
                    GeoSortKey,
                    OffDay,
                    DeliveryOrder,
                    ar.AreaName,
                    c.CompanyName,
                    o.OrderID,
                    i.SortOrder";

            return ExecuteDeliverySheetQuery(sql, parameters, "DeliverySheetRepository.GetDeliverySheetRows");
        }

        /// <summary>
        /// Find contacts on any day that still has open work (RequiredByDate in the
        /// set of dates that have Done = 0 orders). Includes already-delivered rows
        /// on those days; excludes fully closed historical dates.
        /// Matches company name, contact first/last name, and ZZName walk-in label
        /// (Notes text before the first colon — not item descriptions after it).
        /// </summary>
        public RepositoryListResult<DeliverySheetOrderRow> SearchDeliverySheetRowsByContact(string contactName)
        {
            string pattern = "%" + EscapeLike(contactName) + "%";
            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    DataValue = pattern,
                    DataDbType = DbType.String,
                    ParamName = "@SearchText"
                },
                new DBParameter
                {
                    DataValue = (int)SystemConstants.CustomerConstants.SundryCustomerID,
                    DataDbType = DbType.Int32,
                    ParamName = "@SundryContactId"
                }
            };

            string sql = BaseDeliverySheetSql() + @"
                WHERE CAST(o.RequiredByDate AS DATE) IN (
                    SELECT DISTINCT CAST(openOrders.RequiredByDate AS DATE)
                    FROM OrdersTbl openOrders
                    WHERE openOrders.Done = 0
                      AND openOrders.RequiredByDate IS NOT NULL
                )
                  AND (
                        ISNULL(c.CompanyName, '') LIKE @SearchText ESCAPE '\'
                     OR ISNULL(c.ContactFirstName, '') LIKE @SearchText ESCAPE '\'
                     OR ISNULL(c.ContactLastName, '') LIKE @SearchText ESCAPE '\'
                     OR (ISNULL(c.ContactFirstName, '') + ' ' + ISNULL(c.ContactLastName, '')) LIKE @SearchText ESCAPE '\'
                     OR (
                            o.ContactID = @SundryContactId
                        AND (
                                CASE
                                    WHEN CHARINDEX(':', ISNULL(o.Notes, '')) > 0
                                    THEN LEFT(o.Notes, CHARINDEX(':', o.Notes) - 1)
                                    ELSE ISNULL(o.Notes, '')
                                END
                            ) LIKE @SearchText ESCAPE '\'
                     )
                  )
                ORDER BY
                    o.Done,
                    o.RequiredByDate,
                    o.ToBeDeliveredByID,
                    NoGeo,
                    GeoSortKey,
                    OffDay,
                    DeliveryOrder,
                    ar.AreaName,
                    c.CompanyName,
                    o.OrderID,
                    i.SortOrder";

            return ExecuteDeliverySheetQuery(sql, parameters, "DeliverySheetRepository.SearchDeliverySheetRowsByContact");
        }

        private static string EscapeLike(string value)
        {
            string text = (value ?? string.Empty).Trim();
            return text
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_")
                .Replace("[", @"\[");
        }

        public RepositoryListResult<DeliverySheetOrderRow> SearchDeliverySheetRowsByClient(string clientName)
        {
            return SearchDeliverySheetRowsByContact(clientName);
        }

        /// <summary>
        /// One row per order line. Each driver's stops are in geohash order (GeoSortKey: the contact's GPS point, or for a contact
        /// without one yet its area's busiest geohash cell), then PlanRoutes puts them in driving order from the depot. Stops with neither (NoGeo 1) follow in the
        /// old area order: OffDay = 0 when one of the area's prep days (prep day + delay, 1=Sun..7=Sat) delivers on the order's
        /// weekday, then the area's delivery order (Lookups → Delivery Days), then name. The flag is the order's own, or the
        /// contact's account type for orders created before flags were stored on orders.
        /// </summary>
        private static string BaseDeliverySheetSql()
        {
            bool geo = CustomerLocationRepository.TableSeen();
            bool learn = DeliveryVisitRepository.TableSeen();
            bool plan = DeliveryRouteRepository.TableSeen();
            return @"
                SELECT DISTINCT
                    o.OrderID,
                    o.ContactID,
                    c.CompanyName AS ContactName,
                    o.OrderDate,
                    o.PrepDate,
                    o.RequiredByDate,
                    ol.ItemID,
                    i.ItemDesc,
                    i.ItemShortName,
                    ol.QtyOrdered,
                    i.ItemEnabled,
                    i.ReplacementItemID AS ReplacementID,
                    ISNULL(apd.OffDay, 1) AS OffDay,
                    ISNULL(apd.DeliveryOrder, 32767) AS DeliveryOrder,
                    i.SortOrder,
                    o.ToBeDeliveredByID,
                    o.PurchaseOrder,
                    o.Confirmed,
                    o.InvoiceDone,
                    o.Done,
                    o.Notes,
                    ip.ItemPrepDescription AS PackDesc,
                    ip.BGColour,
                    p.Abbreviation AS DeliveryByAbbreviation,
                    ar.AreaName,
                    ar.ShortCode AS AreaShortCode,
                    COALESCE(o.InvoiceTypeID, acc.InvoiceTypeID) AS InvoiceTypeID,
                    " + (geo
                        ? "CASE WHEN " + CustomerLocationRepository.RouteHashSql + " IS NULL THEN 1 ELSE 0 END AS NoGeo, "
                            + CustomerLocationRepository.RouteHashSql + " AS GeoSortKey"
                        : "1 AS NoGeo, CAST(NULL AS VARCHAR(12)) AS GeoSortKey") + @",
                    " + (learn ? "dv.LearnedPos, dv.LearnedVisits" : "CAST(NULL AS FLOAT) AS LearnedPos, 0 AS LearnedVisits") + @",
                    " + (plan ? "dr.PlanSeq, dr.PlanStops" : "CAST(NULL AS INT) AS PlanSeq, 0 AS PlanStops") + @"
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol
                    ON o.OrderID = ol.OrderID
                LEFT JOIN ContactsTbl c
                    ON o.ContactID = c.ContactID
                LEFT JOIN PeopleTbl p
                    ON o.ToBeDeliveredByID = p.PersonID
                LEFT JOIN AreasTbl ar
                    ON ar.AreaID = c.AreaID
                LEFT JOIN ItemPackagingsTbl ip
                    ON ol.PackagingID = ip.ItemPackagingID
                LEFT JOIN ItemsTbl i
                    ON ol.ItemID = i.ItemID" + (geo ? CustomerLocationRepository.RouteJoinSql("c") : string.Empty) + @"
                OUTER APPLY (
                    SELECT TOP 1 a.DeliveryOrder,
                        CASE WHEN ((a.PrepDayOfWeekID - 1 + ISNULL(a.DeliveryDelayDays, 0)) % 7) + 1
                                  = (DATEDIFF(day, '19000107', o.RequiredByDate) % 7) + 1
                             THEN 0 ELSE 1 END AS OffDay
                    FROM AreaPrepDaysTbl a
                    WHERE a.AreaID = c.AreaID
                    ORDER BY OffDay, a.DeliveryOrder
                ) apd
                OUTER APPLY (
                    SELECT TOP 1 ai.InvoiceTypeID
                    FROM ContactsAccInfoTbl ai
                    WHERE ai.ContactID = o.ContactID
                ) acc" + (learn ? DeliveryVisitRepository.LearnedJoinSql("c", "CAST(o.RequiredByDate AS DATE)", "o.ToBeDeliveredByID") : string.Empty)
                + (plan ? DeliveryRouteRepository.PlanJoinSql("c", "CAST(o.RequiredByDate AS DATE)", "o.ToBeDeliveredByID") : string.Empty);
        }

        private RepositoryListResult<ActiveDeliveryDate> ExecuteActiveDeliveryDateQuery(
            string sql,
            List<DBParameter> parameters,
            string context)
        {
            var result = new List<ActiveDeliveryDate>();

            try
            {
                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader(sql, parameters))
                {
                    while (rdr != null && rdr.Read())
                    {
                        result.Add(new ActiveDeliveryDate
                        {
                            RequiredByDate = GetDate(rdr, "RequiredByDate", TimeZoneUtils.Now().Date)
                        });
                    }
                }

                return RepositoryListResult<ActiveDeliveryDate>.Ok(result);
            }
            catch (Exception ex)
            {
                return RepositoryListResult<ActiveDeliveryDate>.Fail(context, ex);
            }
        }

        private RepositoryListResult<DeliverySheetOrderRow> ExecuteDeliverySheetQuery(
            string sql,
            List<DBParameter> parameters,
            string context)
        {
            var result = new List<DeliverySheetOrderRow>();
            var routeHash = new Dictionary<int, string>();
            var learned = new Dictionary<int, Tuple<double, int>>();
            var plans = new Dictionary<int, Tuple<int, int>>();

            try
            {
                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader(sql, parameters))
                {
                    while (rdr != null && rdr.Read())
                    {
                        DeliverySheetOrderRow row = MapDeliverySheetRow(rdr);
                        result.Add(row);
                        if (routeHash.ContainsKey(row.OrderID))
                            continue;
                        routeHash[row.OrderID] = rdr["GeoSortKey"] == DBNull.Value ? null : rdr["GeoSortKey"].ToString();
                        if (rdr["LearnedPos"] != DBNull.Value)
                            learned[row.OrderID] = Tuple.Create(Convert.ToDouble(rdr["LearnedPos"]), Convert.ToInt32(rdr["LearnedVisits"]));
                        if (rdr["PlanSeq"] != DBNull.Value)
                            plans[row.OrderID] = Tuple.Create(Convert.ToInt32(rdr["PlanSeq"]), Convert.ToInt32(rdr["PlanStops"]));
                    }
                }

                return RepositoryListResult<DeliverySheetOrderRow>.Ok(PlanRoutes(result, routeHash, learned, plans));
            }
            catch (Exception ex)
            {
                return RepositoryListResult<DeliverySheetOrderRow>.Fail(context, ex);
            }
        }

        /// <summary>The depot's GPS point (System Preferences → GPS), where every route starts; null when not set.</summary>
        public static void DepotPoint(out double? latitude, out double? longitude)
        {
            latitude = longitude = null;
            try
            {
                GpsSettings settings = new GpsSettingsRepository().Get();
                if (!settings.HasDepotPoint)
                    return;
                latitude = (double)settings.DepotLatitude.Value;
                longitude = (double)settings.DepotLongitude.Value;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Route planning: depot point not read: " + ex.Message);
            }
        }

        /// <summary>
        /// Each driver's orders for a day (open and delivered separately, as the SQL groups them): the order the driver set
        /// in the app for the day, else the order the driver really delivers in, then the driving order from the depot
        /// (RoutePlanner), then the area order; an order's lines stay together.
        /// </summary>
        private static List<DeliverySheetOrderRow> PlanRoutes(List<DeliverySheetOrderRow> rows, Dictionary<int, string> routeHash,
            Dictionary<int, Tuple<double, int>> learned, Dictionary<int, Tuple<int, int>> plans)
        {
            if (rows.Count < 2)
                return rows;
            DepotPoint(out double? depotLat, out double? depotLng);
            Func<IList<string>, double[,]> roadSeconds = RoadTimesManager.ForRouting();

            var planned = new List<DeliverySheetOrderRow>(rows.Count);
            foreach (var run in rows.GroupBy(r => new { r.Done, Day = r.RequiredByDate.Date, r.ToBeDeliveredByID }))
            {
                List<List<DeliverySheetOrderRow>> orders = run.GroupBy(r => r.OrderID).Select(g => g.ToList()).ToList();
                List<List<DeliverySheetOrderRow>> route = RoutePlanner.Order(orders,
                    o => routeHash.TryGetValue(o[0].OrderID, out string hash) ? hash : null, depotLat, depotLng, roadSeconds);
                route = RoutePlanner.ApplyLearned(route, o => o[0].ContactID,
                    o => learned.TryGetValue(o[0].OrderID, out var l) ? l : null);
                route = RoutePlanner.ApplyPlan(route, o => plans.TryGetValue(o[0].OrderID, out var p) ? p : null);
                foreach (List<DeliverySheetOrderRow> order in route)
                    planned.AddRange(order);
            }
            return planned;
        }

        private static DeliverySheetOrderRow MapDeliverySheetRow(IDataRecord rdr)
        {
            return new DeliverySheetOrderRow
            {
                OrderID = GetInt(rdr, "OrderID"),
                ContactID = GetInt(rdr, "ContactID"),
                ContactName = GetString(rdr, "ContactName"),
                OrderDate = GetNullableDate(rdr, "OrderDate"),
                PrepDate = GetNullableDate(rdr, "PrepDate"),
                RequiredByDate = GetDate(rdr, "RequiredByDate", TimeZoneUtils.Now().Date),
                ItemID = GetInt(rdr, "ItemID"),
                ItemDesc = GetString(rdr, "ItemDesc"),
                ItemShortName = GetString(rdr, "ItemShortName"),
                QtyOrdered = GetDouble(rdr, "QtyOrdered"),
                ItemEnabled = GetBool(rdr, "ItemEnabled"),
                ReplacementID = GetNullableInt(rdr, "ReplacementID"),
                DeliveryOrder = GetInt(rdr, "DeliveryOrder"),
                SortOrder = GetInt(rdr, "SortOrder"),
                ToBeDeliveredByID = GetNullableInt(rdr, "ToBeDeliveredByID"),
                PurchaseOrder = GetString(rdr, "PurchaseOrder"),
                Confirmed = GetBool(rdr, "Confirmed"),
                InvoiceDone = GetBool(rdr, "InvoiceDone"),
                Done = GetBool(rdr, "Done"),
                Notes = GetString(rdr, "Notes"),
                PackDesc = GetString(rdr, "PackDesc"),
                BGColour = GetString(rdr, "BGColour"),
                DeliveryByAbbreviation = GetString(rdr, "DeliveryByAbbreviation"),
                AreaName = GetString(rdr, "AreaName"),
                AreaShortCode = GetString(rdr, "AreaShortCode"),
                InvoiceTypeID = GetNullableInt(rdr, "InvoiceTypeID")
            };
        }

        private static string GetString(IDataRecord rdr, string name)
        {
            return rdr[name] == DBNull.Value ? string.Empty : rdr[name].ToString();
        }

        private static int GetInt(IDataRecord rdr, string name)
        {
            return rdr[name] == DBNull.Value ? 0 : Convert.ToInt32(rdr[name]);
        }

        private static int? GetNullableInt(IDataRecord rdr, string name)
        {
            return rdr[name] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr[name]);
        }

        private static double GetDouble(IDataRecord rdr, string name)
        {
            return rdr[name] == DBNull.Value ? 0 : Convert.ToDouble(rdr[name]);
        }

        private static bool GetBool(IDataRecord rdr, string name)
        {
            return rdr[name] != DBNull.Value && Convert.ToBoolean(rdr[name]);
        }

        private static DateTime GetDate(IDataRecord rdr, string name, DateTime fallback)
        {
            return rdr[name] == DBNull.Value ? fallback : Convert.ToDateTime(rdr[name]).Date;
        }

        private static DateTime? GetNullableDate(IDataRecord rdr, string name)
        {
            return rdr[name] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr[name]);
        }
    }
}
