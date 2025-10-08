using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Controls
{
    public class CustomersAwayTbl
    {
        /// <summary>
        /// Gets a list of away periods, optionally filtered and sorted.
        /// </summary>
        public List<ClientAwayPeriod> GetCustomersAway(string whereFilter = "", string sortBy = "CompanyName")
        {
            var result = new List<ClientAwayPeriod>();
            try
            {
                string sql =
                    "SELECT c.CompanyName, a.AwayPeriodID, a.AwayStartDate, a.AwayEndDate, r.ReasonDesc " +
                    "FROM (ClientAwayPeriodTbl a " +
                    "INNER JOIN CustomersTbl c ON a.ClientID = c.CustomerID) " +
                    "INNER JOIN AwayReasonTbl r ON a.ReasonID = r.AwayReasonID";

                var db = new TrackerDb();

                // Add WHERE clause if needed
                if (!string.IsNullOrWhiteSpace(whereFilter))
                {
                    // NOTE: whereFilter comes from UI/session in this app; left as-is to match existing pattern.
                    sql += " WHERE " + whereFilter;
                }
                else
                {
                    // Default: show all customers whose away period ends today or in the future
                    DateTime yesterday = TimeZoneUtils.Now().Date.AddDays(-1);
                    sql += " WHERE a.AwayEndDate >= ?";
                    db.AddWhereParams(yesterday, DbType.DateTime);
                }

                // Add ORDER BY if specified
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    sql += $" ORDER BY {sortBy}";
                }

                var ds = db.ReturnDataSet(sql);
                db.Close();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        var item = new ClientAwayPeriod
                        {
                            CompanyName = row["CompanyName"]?.ToString(),
                            AwayPeriodID = row["AwayPeriodID"] != DBNull.Value ? Convert.ToInt32(row["AwayPeriodID"]) : 0,
                            AwayStartDate = row["AwayStartDate"] != DBNull.Value ? Convert.ToDateTime(row["AwayStartDate"]) : DateTime.MinValue,
                            AwayEndDate = row["AwayEndDate"] != DBNull.Value ? Convert.ToDateTime(row["AwayEndDate"]) : DateTime.MinValue,
                            ReasonDesc = row["ReasonDesc"]?.ToString()
                        };
                        result.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Common.ErrorGeneric, $"GetCustomersAway error: {ex.Message}")
                );
            }
            return result;
        }

        /// <summary>
        /// Inserts a new away period for a customer.
        /// </summary>
        public string InsertAwayPeriod(int customerId, DateTime startDate, DateTime endDate, int reasonId)
        {
            try
            {
                const string sql = "INSERT INTO ClientAwayPeriodTbl (ClientID, AwayStartDate, AwayEndDate, ReasonID) VALUES (?, ?, ?, ?)";
                var db = new TrackerDb();
                db.AddParams(customerId, DbType.Int32);
                db.AddParams(startDate, DbType.DateTime);
                db.AddParams(endDate, DbType.DateTime);
                db.AddParams(reasonId, DbType.Int32);

                string err = db.ExecuteNonQuerySQL(sql);
                db.Close();
                return err ?? "";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Common.ErrorGeneric, $"InsertAwayPeriod error: {ex.Message}")
                );
                return ex.Message;
            }
        }

        /// <summary>
        /// Updates an existing away period.
        /// </summary>
        public string UpdateAwayPeriod(int awayPeriodId, int customerId, DateTime startDate, DateTime endDate, int reasonId)
        {
            try
            {
                const string sql = "UPDATE ClientAwayPeriodTbl SET ClientID=?, AwayStartDate=?, AwayEndDate=?, ReasonID=? WHERE AwayPeriodID=?";
                var db = new TrackerDb();
                db.AddParams(customerId, DbType.Int32);
                db.AddParams(startDate, DbType.DateTime);
                db.AddParams(endDate, DbType.DateTime);
                db.AddParams(reasonId, DbType.Int32);
                db.AddWhereParams(awayPeriodId, DbType.Int32);

                string err = db.ExecuteNonQuerySQL(sql);
                db.Close();
                return err ?? "";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Common.ErrorGeneric, $"UpdateAwayPeriod error: {ex.Message}")
                );
                return ex.Message;
            }
        }

        /// <summary>
        /// Gets a single away period by its ID.
        /// </summary>
        public ClientAwayPeriod GetAwayPeriodById(int awayPeriodId)
        {
            ClientAwayPeriod result = null;
            try
            {
                var db = new TrackerDb();
                db.AddWhereParams(awayPeriodId, DbType.Int32);

                string sql =
                    "SELECT a.*, c.CompanyName, r.ReasonDesc " +
                    "FROM (ClientAwayPeriodTbl a " +
                    "INNER JOIN CustomersTbl c ON a.ClientID = c.CustomerID) " +
                    "INNER JOIN AwayReasonTbl r ON a.ReasonID = r.AwayReasonID " +
                    "WHERE a.AwayPeriodID = ?";

                var ds = db.ReturnDataSet(sql);
                db.Close();

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    result = new ClientAwayPeriod
                    {
                        AwayPeriodID = Convert.ToInt32(row["AwayPeriodID"]),
                        ClientID = Convert.ToInt32(row["ClientID"]),
                        CompanyName = row["CompanyName"].ToString(),
                        AwayStartDate = Convert.ToDateTime(row["AwayStartDate"]),
                        AwayEndDate = Convert.ToDateTime(row["AwayEndDate"]),
                        ReasonID = Convert.ToInt32(row["ReasonID"]),
                        ReasonDesc = row["ReasonDesc"].ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Common.ErrorGeneric, $"GetAwayPeriodById error: {ex.Message}")
                );
            }
            return result;
        }

        /// <summary>
        /// Gets all away reasons.
        /// </summary>
        public static DataTable GetAllAwayReasons()
        {
            var db = new TrackerDb();
            var dt = db.ReturnDataSet("SELECT AwayReasonID, ReasonDesc FROM AwayReasonTbl ORDER BY ReasonDesc").Tables[0];
            db.Close();
            return dt;
        }

        /// <summary>
        /// Deletes the away period by ID.
        /// </summary>
        public string DeleteAwayPeriod(int awayPeriodId)
        {
            try
            {
                const string sql = "DELETE FROM ClientAwayPeriodTbl WHERE AwayPeriodID = ?";
                var db = new TrackerDb();
                db.AddWhereParams(awayPeriodId, DbType.Int32);
                string err = db.ExecuteNonQuerySQL(sql);
                db.Close();
                return err ?? "";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Common.ErrorGeneric, $"DeleteAwayPeriod error: {ex.Message}")
                );
                return ex.Message;
            }
        }
        public bool IsCustomerAwayOnDate(long customerId, DateTime date)
        {
            try
            {
                const string sql =
                    "SELECT COUNT(*) AS Cnt " +
                    "FROM ClientAwayPeriodTbl a " +
                    "WHERE a.ClientID = ? AND a.AwayStartDate <= ? AND a.AwayEndDate >= ?";

                var db = new TrackerDb();
                db.AddWhereParams(customerId, DbType.Int64);
                db.AddWhereParams(date.Date, DbType.DateTime); // order matters
                db.AddWhereParams(date.Date, DbType.DateTime);

                var ds = db.ReturnDataSet(sql);
                db.Close();

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    int cnt = Convert.ToInt32(ds.Tables[0].Rows[0]["Cnt"]);
                    return cnt > 0;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Common.ErrorGeneric, $"IsCustomerAwayOnDate error: {ex.Message}")
                );
            }
            return false;
        }
        /// <summary>
        /// Returns a set of CustomerIDs with any away period overlapping [windowStart, windowEnd].
        /// Overlap: AwayStart <= windowEnd AND AwayEnd >= windowStart
        /// </summary>
        public HashSet<long> GetAwayCustomerIds(DateTime windowStart, DateTime windowEnd)
        {
            var result = new HashSet<long>();
            try
            {
                string sql =
                    "SELECT DISTINCT a.ClientID " +
                    "FROM ClientAwayPeriodTbl a " +
                    "WHERE a.AwayStartDate <= ? AND a.AwayEndDate >= ?";

                var db = new TrackerDb();
                // Access/Jet: param order matters; bind in SQL order
                db.AddWhereParams(windowEnd, DbType.DateTime);
                db.AddWhereParams(windowStart, DbType.DateTime);

                var ds = db.ReturnDataSet(sql);
                db.Close();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (row["ClientID"] != DBNull.Value)
                            result.Add(Convert.ToInt64(row["ClientID"]));
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Common.ErrorGeneric, $"GetAwayCustomerIds error: {ex.Message}")
                );
            }
            return result;
        }
    }
}