using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
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
                    apd.DeliveryOrder,
                    c.CompanyName,
                    i.SortOrder";

            return ExecuteDeliverySheetQuery(sql, parameters, "DeliverySheetRepository.GetDeliverySheetRows");
        }

        public RepositoryListResult<DeliverySheetOrderRow> SearchDeliverySheetRowsByContact(string contactName)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    DataValue = "%" + (contactName ?? string.Empty).Trim() + "%",
                    DataDbType = DbType.String,
                    ParamName = "@CompanyName"
                }
            };

            string sql = BaseDeliverySheetSql() + @"
                WHERE c.CompanyName LIKE @CompanyName
                ORDER BY
                    o.Done,
                    o.RequiredByDate,
                    o.ToBeDeliveredByID,
                    apd.DeliveryOrder,
                    c.CompanyName,
                    i.SortOrder";

            return ExecuteDeliverySheetQuery(sql, parameters, "DeliverySheetRepository.SearchDeliverySheetRowsByContact");
        }

        public RepositoryListResult<DeliverySheetOrderRow> SearchDeliverySheetRowsByClient(string clientName)
        {
            return SearchDeliverySheetRowsByContact(clientName);
        }

        private static string BaseDeliverySheetSql()
        {
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
                    apd.DeliveryOrder,
                    i.SortOrder,
                    o.ToBeDeliveredByID,
                    o.PurchaseOrder,
                    o.Confirmed,
                    o.InvoiceDone,
                    o.Done,
                    o.Notes,
                    ip.ItemPrepDescription AS PackDesc,
                    ip.BGColour,
                    p.Abbreviation AS DeliveryByAbbreviation
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol
                    ON o.OrderID = ol.OrderID
                LEFT JOIN ContactsTbl c
                    ON o.ContactID = c.ContactID
                LEFT JOIN PeopleTbl p
                    ON o.ToBeDeliveredByID = p.PersonID
                LEFT JOIN ItemPackagingsTbl ip
                    ON ol.PackagingID = ip.ItemPackagingID
                LEFT JOIN ItemsTbl i
                    ON ol.ItemID = i.ItemID
                LEFT JOIN AreaPrepDaysTbl apd
                    ON apd.AreaID = c.AreaID";
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

            try
            {
                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader(sql, parameters))
                {
                    while (rdr != null && rdr.Read())
                    {
                        result.Add(MapDeliverySheetRow(rdr));
                    }
                }

                return RepositoryListResult<DeliverySheetOrderRow>.Ok(result);
            }
            catch (Exception ex)
            {
                return RepositoryListResult<DeliverySheetOrderRow>.Fail(context, ex);
            }
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
                DeliveryByAbbreviation = GetString(rdr, "DeliveryByAbbreviation")
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
