// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ReoccuringOrderDAL
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;
using static TrackerDotNet.Classes.DateCalculator;
using static TrackerDotNet.Classes.MessageKeys;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class ReoccuringOrderDAL
    {
        private const string CONST_SQL_SELECT = "SELECT ReoccuringOrderTbl.ID, CustomersTbl.CustomerID, ReoccuranceType, [Value], ItemRequiredID, QtyRequired, DateLastDone, NextDateRequired, RequireUntilDate, PackagingID, ReoccuringOrderTbl.Enabled, ReoccuringOrderTbl.Notes, CustomersTbl.CompanyName, ItemTypeTbl.ItemDesc AS ItemTypeDesc, ReoccuranceTypeTbl.Type AS ReoccuranceTypeDesc  FROM (((ReoccuringOrderTbl LEFT OUTER JOIN ItemTypeTbl ON ReoccuringOrderTbl.ItemRequiredID = ItemTypeTbl.ItemTypeID)   LEFT OUTER JOIN CustomersTbl ON ReoccuringOrderTbl.CustomerID = CustomersTbl.CustomerID)   LEFT OUTER JOIN ReoccuranceTypeTbl ON ReoccuringOrderTbl.ReoccuranceType = ReoccuranceTypeTbl.ID)";
        private const string CONST_SQL_SELECTBYREOCCURINGID = "SELECT CustomerID, ReoccuranceType, [Value], ItemRequiredID, QtyRequired, DateLastDone, NextDateRequired, RequireUntilDate, PackagingID, DeliveryByID, Enabled, Notes  FROM ReoccuringOrderTbl WHERE ID = ?";
        private const string CONST_SQL_UPDATE = "UPDATE ReoccuringOrderTbl SET CustomerID = ?, ReoccuranceType = ?, [Value]= ?,  ItemRequiredID = ?, QtyRequired= ?, DateLastDone= ?, NextDateRequired= ?, RequireUntilDate = ?, PackagingID = ?, Enabled = ?, Notes = ? WHERE ReoccuringOrderTbl.ID = ?";
        private const string CONST_SQL_INSERT = "INSERT INTO ReoccuringOrderTbl (CustomerID, ReoccuranceType, [Value], ItemRequiredID, QtyRequired,  DateLastDone, NextDateRequired, RequireUntilDate, PackagingID, Enabled, Notes)  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        private const string CONST_SQL_DELETE = "DELETE FROM ReoccuringOrderTbl WHERE ReoccuringOrderTbl.ID = ?";
        private const string CONST_SQL_GETITEMSLASTDATE = " SELECT CustomerID, ItemRequiredID, MAX(LastDate) AS LastDatePerItem FROM (SELECT ReoccuringOrderTbl.CustomerID, ReoccuringOrderTbl.ItemRequiredID, ClientUsageLinesTbl.[Date] AS LastDate FROM  ((ClientUsageLinesTbl INNER JOIN ReoccuringOrderTbl ON ClientUsageLinesTbl.CustomerID = ReoccuringOrderTbl.CustomerID AND ClientUsageLinesTbl.[Date] > ReoccuringOrderTbl.DateLastDone) INNER JOIN ItemTypeTbl ON ReoccuringOrderTbl.ItemRequiredID = ItemTypeTbl.ItemTypeID)) ListOfOrdersRequired GROUP BY CustomerID, ItemRequiredID";
        private const string CONST_UPDATE_ITEMSLASTDATE = "UPDATE ReoccuringOrderTbl SET DateLastDone = ? WHERE (CustomerID = ?) AND (ItemRequiredID = ?)";
        private const string CONST_UPDATE_SETREOCCURLASTDATE = "UPDATE ReoccuringOrderTbl SET DateLastDone = ? WHERE (ID = ?)";
        public const int CONST_EITHERENABLEDORDISABLED = -1;
        public const int CONST_DISABLEDONLY = 0;
        public const int CONST_ENABLEDONLY = 1;

        public List<ReoccuringOrderExtData> GetAll() => this.GetAll(-1, string.Empty);

        public List<ReoccuringOrderExtData> GetAll(string SortBy)
        {
            return this.GetAll(-1, SortBy, string.Empty);
        }

        public List<ReoccuringOrderExtData> GetAll(int IsEnabled)
        {
            return this.GetAll(IsEnabled, string.Empty, string.Empty);
        }

        public List<ReoccuringOrderExtData> GetAll(int IsEnabled, string SortBy)
        {
            return this.GetAll(IsEnabled, SortBy, string.Empty);
        }

        public List<ReoccuringOrderExtData> GetAll(int IsEnabled, string SortBy, string WhereFilter)
        {

            List<ReoccuringOrderExtData> all = new List<ReoccuringOrderExtData>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT ReoccuringOrderTbl.ID, CustomersTbl.CustomerID, ReoccuranceType, [Value], ItemRequiredID, QtyRequired, DateLastDone, NextDateRequired, RequireUntilDate, PackagingID, " +
                "ReoccuringOrderTbl.Enabled, ReoccuringOrderTbl.Notes, CustomersTbl.CompanyName, ItemTypeTbl.ItemDesc AS ItemTypeDesc, ReoccuranceTypeTbl.Type AS ReoccuranceTypeDesc  " +
                "FROM (((ReoccuringOrderTbl LEFT OUTER JOIN ItemTypeTbl ON ReoccuringOrderTbl.ItemRequiredID = ItemTypeTbl.ItemTypeID)" +
                "   LEFT OUTER JOIN CustomersTbl ON ReoccuringOrderTbl.CustomerID = CustomersTbl.CustomerID)" +
                "   LEFT OUTER JOIN ReoccuranceTypeTbl ON ReoccuringOrderTbl.ReoccuranceType = ReoccuranceTypeTbl.ID)";
            string str = "";
            switch (IsEnabled)
            {
                case 0:
                    str = " WHERE ReoccuringOrderTbl.enabled = false";
                    break;
                case 1:
                    str += " WHERE ReoccuringOrderTbl.enabled = true";
                    break;
            }
            if (!string.IsNullOrWhiteSpace(WhereFilter))
                str = (!string.IsNullOrWhiteSpace(str) ? str + " AND " : " WHERE ") + WhereFilter;
            if (!string.IsNullOrWhiteSpace(str))
                strSQL += str;
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    ReoccuringOrderExtData reoccuringOrderExtData = new ReoccuringOrderExtData();
                    reoccuringOrderExtData.ReoccuringOrderID = dataReader["ID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ID"]);
                    reoccuringOrderExtData.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    reoccuringOrderExtData.ReoccuranceTypeID = dataReader["ReoccuranceType"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReoccuranceType"]);
                    reoccuringOrderExtData.ReoccuranceValue = dataReader["Value"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["Value"]);
                    reoccuringOrderExtData.ItemRequiredID = dataReader["ItemRequiredID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemRequiredID"]);
                    reoccuringOrderExtData.QtyRequired = dataReader["QtyRequired"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["QtyRequired"]);
                    reoccuringOrderExtData.DateLastDone = dataReader["DateLastDone"] == DBNull.Value ? SystemConstants.DatabaseConstants.SystemMinDate : Convert.ToDateTime(dataReader["DateLastDone"]).Date;
                    reoccuringOrderExtData.NextDateRequired = dataReader["NextDateRequired"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextDateRequired"]).Date;
                    reoccuringOrderExtData.RequireUntilDate = dataReader["RequireUntilDate"] == DBNull.Value ? SystemConstants.DatabaseConstants.SystemMinDate : Convert.ToDateTime(dataReader["RequireUntilDate"]).Date;
                    reoccuringOrderExtData.PackagingID = dataReader["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PackagingID"]);
                    reoccuringOrderExtData.Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]);
                    reoccuringOrderExtData.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                    reoccuringOrderExtData.CompanyName = dataReader["CompanyName"] == DBNull.Value ? string.Empty : dataReader["CompanyName"].ToString();
                    reoccuringOrderExtData.ItemTypeDesc = dataReader["ItemTypeDesc"] == DBNull.Value ? string.Empty : dataReader["ItemTypeDesc"].ToString();
                    reoccuringOrderExtData.ReoccuranceTypeDesc = dataReader["ReoccuranceTypeDesc"] == DBNull.Value ? string.Empty : dataReader["ReoccuranceTypeDesc"].ToString();
                    all.Add(reoccuringOrderExtData);
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public ReoccuringOrderTbl GetByReoccuringOrderByID(int pReoccuringID)
        {
            ReoccuringOrderTbl reoccuringOrderById = (ReoccuringOrderTbl)null;
            string strSQL = "SELECT CustomerID, ReoccuranceType, [Value], ItemRequiredID, QtyRequired, DateLastDone, NextDateRequired, RequireUntilDate, " +
                "                   PackagingID, DeliveryByID, Enabled, Notes FROM ReoccuringOrderTbl WHERE ID = ?";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pReoccuringID, DbType.Int32, "@RecoccuringID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    reoccuringOrderById = new ReoccuringOrderTbl();
                    reoccuringOrderById.ReoccuringOrderID = pReoccuringID;
                    reoccuringOrderById.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    reoccuringOrderById.ReoccuranceTypeID = dataReader["ReoccuranceType"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReoccuranceType"]);
                    reoccuringOrderById.ReoccuranceValue = dataReader["Value"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["Value"]);
                    reoccuringOrderById.ItemRequiredID = dataReader["ItemRequiredID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemRequiredID"]);
                    reoccuringOrderById.QtyRequired = dataReader["QtyRequired"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["QtyRequired"]);
                    reoccuringOrderById.DateLastDone = dataReader["DateLastDone"] == DBNull.Value ? SystemConstants.DatabaseConstants.SystemMinDate : Convert.ToDateTime(dataReader["DateLastDone"]).Date;
                    reoccuringOrderById.NextDateRequired = dataReader["NextDateRequired"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextDateRequired"]).Date;
                    reoccuringOrderById.RequireUntilDate = dataReader["RequireUntilDate"] == DBNull.Value ? SystemConstants.DatabaseConstants.SystemMinDate : Convert.ToDateTime(dataReader["RequireUntilDate"]).Date;
                    reoccuringOrderById.PackagingID = dataReader["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PackagingID"]);
                    reoccuringOrderById.Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]);
                    reoccuringOrderById.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return reoccuringOrderById;
        }

        private DateTime GetMonday(DateTime pDate)
        {
            int num = (int)(1 - pDate.DayOfWeek);
            return pDate.AddDays((double)num);
        }
        //public static DateTime CalculateNextDateRequired(DateTime dateLastDone, int recurrenceTypeId, int recurrenceValue)
        //{
        //    var recurrenceType = ReoccuranceTypeTbl.GetRecurrenceType(recurrenceTypeId);
        //    switch (recurrenceType)
        //    {
        //        case ReoccuranceTypeTbl.RecurrenceType.Weekly:
        //            return dateLastDone.AddDays(recurrenceValue * 7);
        //        case ReoccuranceTypeTbl.RecurrenceType.Monthly:
        //            var nextMonth = dateLastDone.AddMonths(1);
        //            int day = Math.Min(recurrenceValue, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
        //            return new DateTime(nextMonth.Year, nextMonth.Month, day);
        //        default:
        //            return dateLastDone;
        //    }
        //}
        public DateTime CalculateNextDeliveryDateRequired(ReoccuringOrderTbl reoccuranceOrder)
        {
            DateTime nextDateRequired = CalculateNextDatesRequired(reoccuranceOrder).DeliveryDate;
            return nextDateRequired;
        }
        public PrepDeliveryPair CalculateNextDatesRequired(ReoccuringOrderTbl reoccuranceOrder)
        {
            DateCalculator deliveryDateCalculator = new DateCalculator();
            int recurrenceType = reoccuranceOrder.ReoccuranceTypeID;
            DateTime today = TimeZoneUtils.Now().Date;

            bool isFirstTime = reoccuranceOrder.DateLastDone <= SystemConstants.DatabaseConstants.SystemMinDate;

            switch (recurrenceType)
            {
                case ReoccuranceTypeTbl.CONST_WEEKTYPEID:
                    {
                        int weeks = reoccuranceOrder.ReoccuranceValue > 0 ? reoccuranceOrder.ReoccuranceValue : 1;
                        int intervalDays = weeks * 7;
                        DateTime anchor;

                        if (isFirstTime)
                        {
                            // First-time: schedule from today (not today + full interval)
                            anchor = today;
                        }
                        else
                        {
                            anchor = reoccuranceOrder.DateLastDone.AddDays(intervalDays);
                            while (anchor < today)
                                anchor = anchor.AddDays(intervalDays);
                        }

                        return deliveryDateCalculator.CalculateOptimalWeeklyDeliveryDates(
                            reoccuranceOrder.CustomerID,
                            anchor);
                    }

                case ReoccuranceTypeTbl.CONST_DAYOFMONTHID:
                    {
                        int targetDay = reoccuranceOrder.ReoccuranceValue;
                        if (targetDay <= 0) targetDay = 1;

                        DateTime lastDone = reoccuranceOrder.DateLastDone;
                        DateTime baseForCalc;

                        if (isFirstTime)
                        {
                            // For first-time monthly: aim for this month’s target if still upcoming, else next month.
                            int daysInThisMonth = DateTime.DaysInMonth(today.Year, today.Month);
                            int thisMonthTargetDay = Math.Min(targetDay, daysInThisMonth);
                            DateTime candidate = new DateTime(today.Year, today.Month, thisMonthTargetDay);

                            if (candidate < today)
                            {
                                DateTime nextMonth = today.AddMonths(1);
                                int daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                                int nextMonthTargetDay = Math.Min(targetDay, daysInNextMonth);
                                candidate = new DateTime(nextMonth.Year, nextMonth.Month, nextMonthTargetDay);
                            }

                            // Trick: pass a synthetic lastDone = candidate.AddMonths(-1) so the monthly calculator targets 'candidate'
                            baseForCalc = candidate.AddMonths(-1);
                        }
                        else
                        {
                            baseForCalc = lastDone;
                        }

                        return deliveryDateCalculator.CalculateOptimalMonthlyDeliveryDates(
                            reoccuranceOrder.CustomerID,
                            targetDay,
                            baseForCalc);
                    }

                default:
                    {
                        // Fallback treat as weekly “immediate”
                        DateTime anchor = isFirstTime ? today : reoccuranceOrder.DateLastDone.AddDays(7);
                        if (anchor < today) anchor = today;
                        return deliveryDateCalculator.CalculateOptimalWeeklyDeliveryDates(
                            reoccuranceOrder.CustomerID,
                            anchor);
                    }
            }
        }
        public string UpdateReoccuringOrder(ReoccuringOrderTbl reoccuranceOrder, int origReoccuringIDToUpdate, bool recalcNextDateRequired = true)
        {
            // since we are updating the record, also update next date required based on last done, type and value
            if (recalcNextDateRequired)
                reoccuranceOrder.NextDateRequired = CalculateNextDeliveryDateRequired(reoccuranceOrder);
            // no update the data
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)reoccuranceOrder.CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams((object)reoccuranceOrder.ReoccuranceTypeID, DbType.Int32, "@ReoccuranceTypeID");
            trackerDb.AddParams((object)reoccuranceOrder.ReoccuranceValue, DbType.Int32, "@ReoccuranceValue");
            trackerDb.AddParams((object)reoccuranceOrder.ItemRequiredID, DbType.Int32, "@ItemRequiredID");
            trackerDb.AddParams((object)reoccuranceOrder.QtyRequired, DbType.Double, "@QtyRequired");
            trackerDb.AddParams((object)this.GetMonday(reoccuranceOrder.DateLastDone), DbType.DateTime, "@DateLastDone");
            trackerDb.AddParams((object)reoccuranceOrder.NextDateRequired, DbType.DateTime, "@NextDateRequired");
            trackerDb.AddParams((object)reoccuranceOrder.RequireUntilDate, DbType.DateTime, "@RequireUntilDate");
            trackerDb.AddParams((object)reoccuranceOrder.PackagingID, DbType.Int32, "@PackagingID");
            trackerDb.AddParams((object)reoccuranceOrder.Enabled, DbType.Boolean, "@Enabled");
            trackerDb.AddParams((object)reoccuranceOrder.Notes, DbType.String, "@Notes");
            trackerDb.AddWhereParams((object)origReoccuringIDToUpdate, DbType.Int32, "@ReoccuringID");
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE ReoccuringOrderTbl SET CustomerID = ?, ReoccuranceType = ?, [Value]= ?,  ItemRequiredID = ?, QtyRequired= ?, DateLastDone= ?, NextDateRequired= ?, RequireUntilDate = ?, PackagingID = ?, Enabled = ?, Notes = ? WHERE ReoccuringOrderTbl.ID = ?");
            trackerDb.Close();
            return str;
        }

        // Centralized initialization / normalization logic for a new recurring order.
        // Keeps InsertReoccuringOrder lean and makes it easier to adjust defaulting rules.
        private ReoccuringOrderTbl InitializeNewOrderDefaults(ReoccuringOrderTbl newOrder, bool recalcNextDateRequired)
        {
            if (newOrder == null) throw new ArgumentNullException(nameof(newOrder));
            var today = TimeZoneUtils.Now().Date;

            // Normalize first-time MONTHLY recurrence so that the first NextDateRequired points to
            // THIS month (not automatically a month ahead) when DateLastDone has never been set.
            bool isFirstTime = newOrder.DateLastDone <= SystemConstants.DatabaseConstants.SystemMinDate;
            bool isMonthly = newOrder.ReoccuranceTypeID == ReoccuranceTypeTbl.CONST_DAYOFMONTHID;
            if (isFirstTime && isMonthly)
            {
                int targetDay = newOrder.ReoccuranceValue <= 0 ? 1 : newOrder.ReoccuranceValue;
                int daysInThisMonth = DateTime.DaysInMonth(today.Year, today.Month);
                int actualDay = Math.Min(targetDay, daysInThisMonth);
                DateTime thisMonthTarget = new DateTime(today.Year, today.Month, actualDay);

                // Only adjust if the target day is still today or in the future this month.
                // (If already past, we leave DateLastDone as min so existing logic will advance correctly.)
                if (thisMonthTarget >= today)
                {
                    // Set synthetic last-done to previous month’s target so downstream
                    // logic that does AddMonths(1) yields thisMonthTarget.
                    DateTime prevMonth = thisMonthTarget.AddMonths(-1);
                    newOrder.DateLastDone = prevMonth;
                }
            }

            if (newOrder.ReoccuranceTypeID == ReoccuranceTypeTbl.CONST_WEEKTYPEID &&
                newOrder.DateLastDone > SystemConstants.DatabaseConstants.SystemMinDate)
            {
                newOrder.DateLastDone = GetMonday(newOrder.DateLastDone);
            }

            if (newOrder.RequireUntilDate == DateTime.MinValue)
                newOrder.RequireUntilDate = SystemConstants.DatabaseConstants.SystemMinDate;

            if (recalcNextDateRequired)
            {
                try
                {
                    newOrder.NextDateRequired = CalculateNextDeliveryDateRequired(newOrder);

                    // For monthly first-time we intentionally allow "today" if it is the target;
                    // only bump if it somehow calculated into the past.
                    if (newOrder.NextDateRequired < today)
                        newOrder.NextDateRequired = today.AddDays(1);
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"ReoccuringOrderDAL: Insert recalc failed (Cust={newOrder.CustomerID}): {ex.Message}");
                    newOrder.NextDateRequired = today.AddDays(1);
                }
            }
            else if (newOrder.NextDateRequired <= SystemConstants.DatabaseConstants.SystemMinDate)
            {
                newOrder.NextDateRequired = today.AddDays(1);
            }
            return newOrder;
        }

        public string InsertReoccuringOrder(ReoccuringOrderTbl newOrder, bool recalcNextDateRequired = true)
        {
            if (newOrder == null) throw new ArgumentNullException(nameof(newOrder));

            // All default / normalization logic moved here.
            newOrder = InitializeNewOrderDefaults(newOrder, recalcNextDateRequired);

            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams(newOrder.CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams(newOrder.ReoccuranceTypeID, DbType.Int32, "@ReoccuranceTypeID");
            trackerDb.AddParams(newOrder.ReoccuranceValue, DbType.Int32, "@ReoccuranceValue");
            trackerDb.AddParams(newOrder.ItemRequiredID, DbType.Int32, "@ItemRequiredID");
            trackerDb.AddParams(newOrder.QtyRequired, DbType.Double, "@QtyRequired");
            trackerDb.AddParams(newOrder.DateLastDone, DbType.DateTime, "@DateLastDone");
            trackerDb.AddParams(newOrder.NextDateRequired, DbType.DateTime, "@NextDateRequired");
            trackerDb.AddParams(newOrder.RequireUntilDate, DbType.DateTime, "@RequireUntilDate");
            trackerDb.AddParams(newOrder.PackagingID, DbType.Int32, "@PackagingID");
            trackerDb.AddParams(newOrder.Enabled, DbType.Boolean, "@Enabled");
            trackerDb.AddParams(newOrder.Notes ?? string.Empty, DbType.String, "@Notes");

            string result = trackerDb.ExecuteNonQuerySQL(
                "INSERT INTO ReoccuringOrderTbl (CustomerID, ReoccuranceType, [Value], ItemRequiredID, QtyRequired, DateLastDone, NextDateRequired, RequireUntilDate, PackagingID, Enabled, Notes) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
            trackerDb.Close();

            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                $"ReoccuringOrderDAL: Inserted recurring order (Cust={newOrder.CustomerID}) FirstNextDate={newOrder.NextDateRequired:yyyy-MM-dd}");

            return result;
        }

        public string DeleteReoccuringOrder(long pReoccuringIDToDelete)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pReoccuringIDToDelete, DbType.Int64, "@ReoccuringID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM ReoccuringOrderTbl WHERE ReoccuringOrderTbl.ID = ?");
            trackerDb.Close();
            return str;
        }

        public bool SetReoccuringItemsLastDate()
        {
            List<ReoccuringOrderTbl> reoccuringOrders = new List<ReoccuringOrderTbl>();
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT CustomerID, ItemRequiredID, MAX(LastDate) AS LastDatePerItem" +
                " FROM (SELECT ReoccuringOrderTbl.CustomerID, ReoccuringOrderTbl.ItemRequiredID, ClientUsageLinesTbl.[Date] AS LastDate" +
                " FROM ((ClientUsageLinesTbl INNER JOIN ReoccuringOrderTbl ON ClientUsageLinesTbl.CustomerID = ReoccuringOrderTbl.CustomerID" +
                    " AND ClientUsageLinesTbl.[Date] > ReoccuringOrderTbl.DateLastDone) INNER JOIN ItemTypeTbl ON ReoccuringOrderTbl.ItemRequiredID = ItemTypeTbl.ItemTypeID))" +
                    " ListOfOrdersRequired GROUP BY CustomerID, ItemRequiredID");
            bool flag = dataReader != null;
            if (flag)
            {
                while (dataReader.Read())
                    reoccuringOrders.Add(new ReoccuringOrderTbl()
                    {
                        CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]),
                        ItemRequiredID = dataReader["ItemRequiredID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemRequiredID"]),
                        DateLastDone = dataReader["LastDatePerItem"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["LastDatePerItem"]).Date
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            for (int index = 0; index < reoccuringOrders.Count; ++index)
            {
                trackerDb.Open();
                trackerDb.AddParams((object)this.GetMonday(reoccuringOrders[index].DateLastDone), DbType.Date, "@DateLastDone");
                trackerDb.AddWhereParams((object)reoccuringOrders[index].CustomerID, DbType.Int64, "@CustomerID");
                trackerDb.AddWhereParams((object)reoccuringOrders[index].ItemRequiredID, DbType.Int32, "@ItemRequiredID");
                flag = flag || string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("UPDATE ReoccuringOrderTbl SET DateLastDone = ? WHERE (CustomerID = ?) AND (ItemRequiredID = ?)"));
                trackerDb.Close();
            }
            return flag;
        }

        /// <summary>
        /// Calculates the appropriate DateLastDone value for recurring orders
        /// Ensures consistency across all parts of the system
        /// </summary>
        public DateTime CalculateRecurringLastDate(DateTime baseDate, ReoccuranceTypeTbl.RecurrenceType recurrenceType, int targetValue)
        {
            switch (recurrenceType)
            {
                case ReoccuranceTypeTbl.RecurrenceType.Weekly:
                    // For weekly orders, use Monday of the week containing the base date
                    return GetMonday(baseDate);

                case ReoccuranceTypeTbl.RecurrenceType.Monthly:
                    // For monthly orders, use the actual target day of month
                    // This prevents the "duplicate send" issue you identified
                    try
                    {
                        return new DateTime(baseDate.Year, baseDate.Month, targetValue).Date;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Handle months with fewer days
                        int daysInMonth = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
                        return new DateTime(baseDate.Year, baseDate.Month, Math.Min(targetValue, daysInMonth)).Date;
                    }

                default:
                    // Fallback to Monday normalization
                    return GetMonday(baseDate);
            }
        }

        /// <summary>
        /// Updates the order date (if it is done and the next order date 
        /// </summary>
        public string SetReoccuringOrderDates(DateTime orderDate, long reoccuringOrderId, bool orderDone = false)
        {
            try
            {
                var recurringOrder = GetByReoccuringOrderByID((int)reoccuringOrderId);
                if (recurringOrder == null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"ReoccuringOrderDAL: Not found RecID={reoccuringOrderId}");
                    return "Recurring order not found";
                }

                var recurrenceType = ReoccuranceTypeTbl.GetRecurrenceType(recurringOrder.ReoccuranceTypeID);

                if (orderDone)
                {
                    // Store actual delivered date
                    recurringOrder.DateLastDone = orderDate.Date;
                }

                // Compute next date with normalization for MONTHLY so cadence stays aligned
                DateTime nextDate;
                //if (recurrenceType == ReoccuranceTypeTbl.RecurrenceType.Monthly)
                //{
                //    int targetDay = recurringOrder.ReoccuranceValue <= 0 ? 1 : recurringOrder.ReoccuranceValue;
                //    var daysInMonth = DateTime.DaysInMonth(orderDate.Year, orderDate.Month);
                //    // Anchor at intended target day for current (delivered) month
                //    var normalizedAnchor = new DateTime(orderDate.Year, orderDate.Month, Math.Min(targetDay, daysInMonth));

                //    // If you delivered before the target day, treat the pattern as still fulfilled for THIS month:
                //    // so next should be target day NEXT month.
                //    if (orderDate.Date < normalizedAnchor.Date)
                //        normalizedAnchor = normalizedAnchor.AddMonths(1);

                //    // Temporarily swap for calculation
                //    var originalActual = recurringOrder.DateLastDone;
                //    var tempForCalc = new ReoccuringOrderTbl
                //    {
                //        ReoccuringOrderID = recurringOrder.ReoccuringOrderID,
                //        CustomerID = recurringOrder.CustomerID,
                //        ReoccuranceTypeID = recurringOrder.ReoccuranceTypeID,
                //        ReoccuranceValue = recurringOrder.ReoccuranceValue,
                //        ItemRequiredID = recurringOrder.ItemRequiredID,
                //        DateLastDone = normalizedAnchor, // synthetic
                //        QtyRequired = recurringOrder.QtyRequired,
                //        PackagingID = recurringOrder.PackagingID,
                //        Enabled = recurringOrder.Enabled,
                //        Notes = recurringOrder.Notes
                //    };
                //    nextDate = CalculateNextDeliveryDateRequired(tempForCalc);
                //}
                //else
                //{
                    nextDate = CalculateNextDeliveryDateRequired(recurringOrder);
                //}
                recurringOrder.NextDateRequired = nextDate;

                string sql;
                var trackerDb = new TrackerDb();
                if (orderDone)
                {
                    sql = "UPDATE ReoccuringOrderTbl SET DateLastDone = ?, NextDateRequired = ? WHERE (ID = ?)";
                    trackerDb.AddParams(recurringOrder.DateLastDone, DbType.Date, "@DateLastDone");
                    trackerDb.AddParams(recurringOrder.NextDateRequired, DbType.Date, "@NextDateRequired");
                }
                else
                {
                    sql = "UPDATE ReoccuringOrderTbl SET NextDateRequired = ? WHERE (ID = ?)";
                    trackerDb.AddParams(recurringOrder.NextDateRequired, DbType.Date, "@NextDateRequired");
                }
                trackerDb.AddWhereParams(reoccuringOrderId, DbType.Int32, "@ID");
                var result = trackerDb.ExecuteNonQuerySQL(sql);
                trackerDb.Close();

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"RecID={reoccuringOrderId} Done={orderDone} Last={recurringOrder.DateLastDone:yyyy-MM-dd} Next={recurringOrder.NextDateRequired:yyyy-MM-dd}");

                return result;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"ReoccuringOrderDAL: Error RecID={reoccuringOrderId}: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}