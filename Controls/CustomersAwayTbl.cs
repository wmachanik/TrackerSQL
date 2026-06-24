using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;

namespace TrackerSQL.Controls
{
    [Obsolete("DO NOT USE Comtrols use Models - MIGRATION IN PROGRESS", true)]
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
                // SQL Server uses ContactsAwayPeriodTbl and ContactsTbl (not ClientAwayPeriodTbl/CustomersTbl)
                string sql =
                    @"SELECT c.CompanyName, a.AwayPeriodID, a.AwayStartDate, a.AwayEndDate, r.ReasonDesc 
                      FROM ContactsAwayPeriodTbl a 
                      INNER JOIN ContactsTbl c ON a.ContactID = c.ContactID
                      LEFT JOIN AwayReasonsTbl r ON a.ReasonID = r.ReasonID";

                var parameters = new List<DBParameter>();

                // Add WHERE clause if needed
                if (!string.IsNullOrWhiteSpace(whereFilter))
                {
                    // Convert Access-style date literals to SQL Server format
                    string sqlFilter = whereFilter.Replace("#", "'");
                    sql += " WHERE " + sqlFilter;
                }
                else
                {
                    // Default: show all customers whose away period ends today or in the future
                    DateTime yesterday = TimeZoneUtils.Now().Date.AddDays(-1);
                    sql += " WHERE a.AwayEndDate >= @EndDate";
                    parameters.Add(new DBParameter { ParamName = "@EndDate", DataValue = yesterday, DataDbType = DbType.DateTime });
                }

                // Add ORDER BY if specified
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    string orderBy = sortBy;
                    if (sortBy.Equals("CompanyName", StringComparison.OrdinalIgnoreCase))
                        orderBy = "c.CompanyName";
                    sql += $" ORDER BY {orderBy}";
                }

                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader(sql, parameters))
                {
                    while (rdr != null && rdr.Read())
                    {
                        var item = new ClientAwayPeriod
                        {
                            CompanyName = rdr["CompanyName"]?.ToString(),
                            AwayPeriodID = rdr["AwayPeriodID"] != DBNull.Value ? Convert.ToInt32(rdr["AwayPeriodID"]) : 0,
                            AwayStartDate = rdr["AwayStartDate"] != DBNull.Value ? Convert.ToDateTime(rdr["AwayStartDate"]) : DateTime.MinValue,
                            AwayEndDate = rdr["AwayEndDate"] != DBNull.Value ? Convert.ToDateTime(rdr["AwayEndDate"]) : DateTime.MinValue,
                            ReasonDesc = rdr["ReasonDesc"]?.ToString()
                        };
                        result.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    $"GetCustomersAway error: {ex.Message}"
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
                const string sql = "INSERT INTO ContactsAwayPeriodTbl (ContactID, AwayStartDate, AwayEndDate, ReasonID) VALUES (@ContactID, @StartDate, @EndDate, @ReasonID)";
                var parameters = new List<DBParameter>
                {
                    new DBParameter { ParamName = "@ContactID", DataValue = customerId, DataDbType = DbType.Int32 },
                    new DBParameter { ParamName = "@StartDate", DataValue = startDate, DataDbType = DbType.DateTime },
                    new DBParameter { ParamName = "@EndDate", DataValue = endDate, DataDbType = DbType.DateTime },
                    new DBParameter { ParamName = "@ReasonID", DataValue = reasonId, DataDbType = DbType.Int32 }
                };

                using (var db = new TrackerSQLDb())
                {
                    db.ExecuteNonQuery(sql, parameters);
                }
                return "";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    $"InsertAwayPeriod error: {ex.Message}"
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
                const string sql = "UPDATE ContactsAwayPeriodTbl SET ContactID=@ContactID, AwayStartDate=@StartDate, AwayEndDate=@EndDate, ReasonID=@ReasonID WHERE AwayPeriodID=@AwayPeriodID";
                var parameters = new List<DBParameter>
                {
                    new DBParameter { ParamName = "@ContactID", DataValue = customerId, DataDbType = DbType.Int32 },
                    new DBParameter { ParamName = "@StartDate", DataValue = startDate, DataDbType = DbType.DateTime },
                    new DBParameter { ParamName = "@EndDate", DataValue = endDate, DataDbType = DbType.DateTime },
                    new DBParameter { ParamName = "@ReasonID", DataValue = reasonId, DataDbType = DbType.Int32 },
                    new DBParameter { ParamName = "@AwayPeriodID", DataValue = awayPeriodId, DataDbType = DbType.Int32 }
                };

                using (var db = new TrackerSQLDb())
                {
                    db.ExecuteNonQuery(sql, parameters);
                }
                return "";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    $"UpdateAwayPeriod error: {ex.Message}"
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
                string sql =
                    @"SELECT a.AwayPeriodID, a.ContactID AS ClientID, c.CompanyName, a.AwayStartDate, a.AwayEndDate, a.ReasonID, r.ReasonDesc 
                      FROM ContactsAwayPeriodTbl a 
                      INNER JOIN ContactsTbl c ON a.ContactID = c.ContactID
                      LEFT JOIN AwayReasonsTbl r ON a.ReasonID = r.ReasonID 
                      WHERE a.AwayPeriodID = @AwayPeriodID";

                var parameters = new List<DBParameter>
                {
                    new DBParameter { ParamName = "@AwayPeriodID", DataValue = awayPeriodId, DataDbType = DbType.Int32 }
                };

                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader(sql, parameters))
                {
                    if (rdr != null && rdr.Read())
                    {
                        result = new ClientAwayPeriod
                        {
                            AwayPeriodID = Convert.ToInt32(rdr["AwayPeriodID"]),
                            ClientID = Convert.ToInt32(rdr["ClientID"]),
                            CompanyName = rdr["CompanyName"].ToString(),
                            AwayStartDate = Convert.ToDateTime(rdr["AwayStartDate"]),
                            AwayEndDate = Convert.ToDateTime(rdr["AwayEndDate"]),
                            ReasonID = rdr["ReasonID"] != DBNull.Value ? Convert.ToInt32(rdr["ReasonID"]) : 0,
                            ReasonDesc = rdr["ReasonDesc"]?.ToString() ?? ""
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    $"GetAwayPeriodById error: {ex.Message}"
                );
            }
            return result;
        }

        /// <summary>
        /// Gets all away reasons.
        /// </summary>
        public static DataTable GetAllAwayReasons()
        {
            var dt = new DataTable();
            try
            {
                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader("SELECT ReasonID AS AwayReasonID, ReasonDesc FROM AwayReasonsTbl ORDER BY ReasonDesc"))
                {
                    dt.Load(rdr);
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"GetAllAwayReasons error: {ex.Message}");
            }
            return dt;
        }

        /// <summary>
        /// Deletes the away period by ID.
        /// </summary>
        public string DeleteAwayPeriod(int awayPeriodId)
        {
            try
            {
                const string sql = "DELETE FROM ContactsAwayPeriodTbl WHERE AwayPeriodID = @AwayPeriodID";
                var parameters = new List<DBParameter>
                {
                    new DBParameter { ParamName = "@AwayPeriodID", DataValue = awayPeriodId, DataDbType = DbType.Int32 }
                };

                using (var db = new TrackerSQLDb())
                {
                    db.ExecuteNonQuery(sql, parameters);
                }
                return "";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    $"DeleteAwayPeriod error: {ex.Message}"
                );
                return ex.Message;
            }
        }

        public bool IsCustomerAwayOnDate(long customerId, DateTime date)
        {
            try
            {
                const string sql =
                    @"SELECT COUNT(*) AS Cnt 
                      FROM ContactsAwayPeriodTbl a 
                      WHERE a.ContactID = @ContactID AND a.AwayStartDate <= @CheckDate AND a.AwayEndDate >= @CheckDate";

                var parameters = new List<DBParameter>
                {
                    new DBParameter { ParamName = "@ContactID", DataValue = customerId, DataDbType = DbType.Int64 },
                    new DBParameter { ParamName = "@CheckDate", DataValue = date.Date, DataDbType = DbType.DateTime }
                };

                using (var db = new TrackerSQLDb())
                {
                    var result = db.ExecuteScalar(sql, parameters);
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    $"IsCustomerAwayOnDate error: {ex.Message}"
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
                    @"SELECT DISTINCT a.ContactID 
                      FROM ContactsAwayPeriodTbl a 
                      WHERE a.AwayStartDate <= @WindowEnd AND a.AwayEndDate >= @WindowStart";

                var parameters = new List<DBParameter>
                {
                    new DBParameter { ParamName = "@WindowEnd", DataValue = windowEnd, DataDbType = DbType.DateTime },
                    new DBParameter { ParamName = "@WindowStart", DataValue = windowStart, DataDbType = DbType.DateTime }
                };

                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader(sql, parameters))
                {
                    while (rdr != null && rdr.Read())
                    {
                        if (rdr["ContactID"] != DBNull.Value)
                            result.Add(Convert.ToInt64(rdr["ContactID"]));
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Customers,
                    $"GetAwayCustomerIds error: {ex.Message}"
                );
            }
            return result;
        }
    }
}
