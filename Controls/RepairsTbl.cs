// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.RepairsTbl
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;

namespace TrackerSQL.Controls
{
    public class RepairsTbl
    {
        // Status constants moved to RepairStatus enum in Classes/Enums
        private const string CONST_REPAIR_DONESTR = "7";

        // SQL queries
        private const string CONST_SQL_SELECT = "SELECT RepairID, CustomerID, ContactName, ContactEmail, JobCardNumber, DateLogged, LastStatusChange, MachineTypeID, MachineSerialNumber, " +
                                                       "SwopOutMachineID, MachineConditionID, TakenFrother, TakenBeanLid, TakenWaterLid, BrokenFrother, BrokenBeanLid, BrokenWaterLid, " +
                                                       "RepairFaultID, RepairFaultDesc, RepairsTbl.RepairStatusID, RelatedOrderID, Notes FROM RepairsTbl";
        private const string CONST_SQL_SELECTNOTDONE = CONST_SQL_SELECT + " WHERE (RepairsTbl.RepairStatusID <> " + CONST_REPAIR_DONESTR + ")";
        private const string CONST_SQL_SELECTONSTATUS = CONST_SQL_SELECT + " WHERE (RepairsTbl.RepairStatusID = ?)";
        private const string CONST_SQL_SELECTBYREPAIRID = "SELECT CustomerID, ContactName, ContactEmail, JobCardNumber, DateLogged, LastStatusChange, MachineTypeID, MachineSerialNumber, SwopOutMachineID, MachineConditionID, TakenFrother, TakenBeanLid, TakenWaterLid, BrokenFrother, BrokenBeanLid, BrokenWaterLid, RepairFaultID, RepairFaultDesc, RepairsTbl.RepairStatusID, RelatedOrderID, Notes FROM RepairsTbl WHERE (RepairID = ?)";
        private const string CONST_SQL_SELECTLAST = "SELECT TOP 1 RepairID FROM RepairsTbl WHERE (CustomerID = ?) ORDER BY DateLogged DESC";
        private const string CONST_SQL_INSERT = "INSERT INTO RepairsTbl (CustomerID, ContactName, ContactEmail, JobCardNumber, DateLogged, LastStatusChange, MachineTypeID, MachineSerialNumber, SwopOutMachineID, MachineConditionID, TakenFrother, TakenBeanLid, TakenWaterLid, BrokenFrother, BrokenBeanLid, BrokenWaterLid, RepairFaultID, RepairFaultDesc, RepairStatusID, RelatedOrderID, Notes)VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        private const string CONST_SQL_UPDATE = "UPDATE RepairsTbl SET CustomerID = ?, ContactName = ?, ContactEmail = ?, JobCardNumber = ?, DateLogged = ?, LastStatusChange = ?, MachineTypeID = ?, MachineSerialNumber = ?, SwopOutMachineID = ?,  MachineConditionID = ?, TakenFrother = ?, TakenBeanLid = ?, TakenWaterLid = ?, BrokenFrother = ?,  BrokenBeanLid = ?, BrokenWaterLid = ?, RepairFaultID = ?, RepairFaultDesc = ?, RepairStatusID = ?,  RelatedOrderID = ?, Notes = ? WHERE (RepairsTbl.RepairID = ?)";
        private const string CONST_SQL_DELETE = "DELETE FROM RepairsTbl WHERE (RepairsTbl.RepairID = ?)";
        private const string CONST_SQL_SELECTITEMWITHANORDER = "SELECT RepairID, CustomerID, ContactName, ContactEmail, JobCardNumber, DateLogged, LastStatusChange, MachineTypeID, MachineSerialNumber, SwopOutMachineID, MachineConditionID, TakenFrother, TakenBeanLid, TakenWaterLid, BrokenFrother, BrokenBeanLid, BrokenWaterLid, RepairFaultID, RepairFaultDesc, RepairsTbl.RepairStatusID, RelatedOrderID, Notes FROM RepairsTbl WHERE RelatedOrderID > 0";

        // Properties
        public int RepairID { get; set; }
        public long CustomerID { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string JobCardNumber { get; set; } = string.Empty;
        public DateTime DateLogged { get; set; } = DateTime.MinValue;
        public DateTime LastStatusChange { get; set; } = TimeZoneUtils.Now();
        public int MachineTypeID { get; set; }
        public string MachineSerialNumber { get; set; } = string.Empty;
        public int SwopOutMachineID { get; set; }
        public int MachineConditionID { get; set; }
        public bool TakenFrother { get; set; }
        public bool TakenBeanLid { get; set; } = true;
        public bool TakenWaterLid { get; set; } = true;
        public bool BrokenFrother { get; set; }
        public bool BrokenBeanLid { get; set; }
        public bool BrokenWaterLid { get; set; }
        public int RepairFaultID { get; set; }
        public string RepairFaultDesc { get; set; } = string.Empty;
        public int RepairStatusID { get; set; }
        public int RelatedOrderID { get; set; }
        public string Notes { get; set; } = string.Empty;

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public List<RepairsTbl> GetAll(string sortBy)
        {
            return GetAllRepairs(sortBy, CONST_SQL_SELECT, null);
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<RepairsTbl> GetAllNotDone(string sortBy)
        {
            return GetAllRepairs(sortBy, CONST_SQL_SELECTNOTDONE, null);
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public List<RepairsTbl> GetAllRepairsOfStatus(string repairStatus)
        {
            return GetAllRepairsOfStatus("DateLogged DESC", repairStatus);
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public List<RepairsTbl> GetAllRepairsOfStatus(string sortBy, string repairStatus)
        {
            if (!int.TryParse(repairStatus, out int statusId) || repairStatus.Equals("OPEN"))
                return GetAllNotDone(sortBy);

            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = statusId, DataDbType = DbType.Int32 }
            };

            return GetAllRepairs(sortBy, CONST_SQL_SELECTONSTATUS, parameters);
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public List<RepairsTbl> GetRepairsByStatusAndDateRange(string sortBy, string repairStatus, object fromDate, object toDate, string filterBy, string filterText)
        {
            // Convert object parameters to nullable DateTime
            DateTime? fromDateNullable = TrackerTools.ConvertToNullableDateTime(fromDate);
            DateTime? toDateNullable = TrackerTools.ConvertToNullableDateTime(toDate);

            // If no date filters are specified and no additional filters, use the existing status-only method
            if (!fromDateNullable.HasValue && !toDateNullable.HasValue &&
                (string.IsNullOrEmpty(filterText) || filterBy == "DateLogged"))
            {
                return GetAllRepairsOfStatus(sortBy, repairStatus);
            }

            // ✅ Define clean SQL with aliases for JOIN
            const string CONST_SQL_SELECT_WITH_JOIN = "SELECT r.RepairID, r.CustomerID, r.ContactName, r.ContactEmail, r.JobCardNumber, r.DateLogged, r.LastStatusChange, r.MachineTypeID, r.MachineSerialNumber, " +
                                                       "r.SwopOutMachineID, r.MachineConditionID, r.TakenFrother, r.TakenBeanLid, r.TakenWaterLid, r.BrokenFrother, r.BrokenBeanLid, r.BrokenWaterLid, " +
                                                       "r.RepairFaultID, r.RepairFaultDesc, r.RepairStatusID, r.RelatedOrderID, r.Notes " +
                                                       "FROM RepairsTbl AS r INNER JOIN CustomersTbl AS c ON r.CustomerID = c.CustomerID";

            string sql = CONST_SQL_SELECT;
            bool needsJoin = false;

            var parameters = new List<DBParameter>();
            var whereConditions = new List<string>();

            // Add status filter - ✅ Use correct field names based on whether we're using JOIN
            if (!string.IsNullOrEmpty(repairStatus) && int.TryParse(repairStatus, out int statusId) && !repairStatus.Equals("OPEN"))
            {
                whereConditions.Add("RepairsTbl.RepairStatusID = ?"); // ✅ Use full table name for non-JOIN
                parameters.Add(new DBParameter { DataValue = statusId, DataDbType = DbType.Int32 });
            }
            else if (repairStatus == "OPEN" || string.IsNullOrEmpty(repairStatus))
            {
                whereConditions.Add($"RepairsTbl.RepairStatusID <> {CONST_REPAIR_DONESTR}"); // ✅ Use full table name
            }

            // Add date filters - ✅ Use correct field names
            if (fromDateNullable.HasValue)
            {
                whereConditions.Add("RepairsTbl.DateLogged >= ?"); // ✅ Use full table name for non-JOIN
                parameters.Add(new DBParameter { DataValue = fromDateNullable.Value.Date, DataDbType = DbType.DateTime });
            }

            if (toDateNullable.HasValue)
            {
                whereConditions.Add("RepairsTbl.DateLogged < ?"); // ✅ Use full table name for non-JOIN
                parameters.Add(new DBParameter { DataValue = toDateNullable.Value.Date.AddDays(1), DataDbType = DbType.DateTime });
            }

            // Add text-based filters
            if (!string.IsNullOrEmpty(filterText) && !string.IsNullOrEmpty(filterBy))
            {
                switch (filterBy)
                {
                    case "CompanyID":
                        // ✅ Use JOIN SQL and aliases for company search
                        needsJoin = true;
                        string companySearchText = filterText.Contains("%") ? filterText : $"%{filterText}%";
                        whereConditions.Clear(); // ✅ Clear previous conditions and rebuild with aliases
                        parameters.Clear();

                        // Rebuild all conditions with aliases when using JOIN
                        if (!string.IsNullOrEmpty(repairStatus) && int.TryParse(repairStatus, out int joinStatusId) && !repairStatus.Equals("OPEN"))
                        {
                            whereConditions.Add("r.RepairStatusID = ?");
                            parameters.Add(new DBParameter { DataValue = joinStatusId, DataDbType = DbType.Int32 });
                        }
                        else if (repairStatus == "OPEN" || string.IsNullOrEmpty(repairStatus))
                        {
                            whereConditions.Add($"r.RepairStatusID <> {CONST_REPAIR_DONESTR}");
                        }

                        if (fromDateNullable.HasValue)
                        {
                            whereConditions.Add("r.DateLogged >= ?");
                            parameters.Add(new DBParameter { DataValue = fromDateNullable.Value.Date, DataDbType = DbType.DateTime });
                        }

                        if (toDateNullable.HasValue)
                        {
                            whereConditions.Add("r.DateLogged < ?");
                            parameters.Add(new DBParameter { DataValue = toDateNullable.Value.Date.AddDays(1), DataDbType = DbType.DateTime });
                        }

                        whereConditions.Add("c.CompanyName LIKE ?");
                        parameters.Add(new DBParameter { DataValue = companySearchText, DataDbType = DbType.String });
                        break;

                    case "MachineSerialNumber":
                        string serialSearchText = filterText.Contains("%") ? filterText : $"%{filterText}%";
                        whereConditions.Add("RepairsTbl.MachineSerialNumber LIKE ?"); // ✅ Use full table name for non-JOIN
                        parameters.Add(new DBParameter { DataValue = serialSearchText, DataDbType = DbType.String });
                        break;
                }
            }

            // ✅ Use the appropriate SQL based on whether we need a JOIN
            if (needsJoin)
            {
                sql = CONST_SQL_SELECT_WITH_JOIN;
            }

            // Combine conditions
            if (whereConditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", whereConditions);
            }

            return GetAllRepairs(sortBy, sql, parameters);
        }
        /*            AppLogger.WriteLog("debug", "=== DEBUGGING MODE ===");
                    AppLogger.WriteLog("debug", $"Input params - repairStatus: '{repairStatus}', fromDate: '{fromDate}', toDate: '{toDate}', filterBy: '{filterBy}', filterText: '{filterText}'");

                    // Try with just status filter first
                    string sql1 = CONST_SQL_SELECT + " WHERE RepairsTbl.RepairStatusID = ?";
                    var params1 = new List<DBParameter>();
                    params1.Add(new DBParameter
                    {
                        DataValue = 7,
                        DataDbType = DbType.Int32
                    });

                    try
                    {
                        AppLogger.WriteLog("debug", "Testing status-only query...");
                        var result1 = GetAllRepairs(sortBy, sql1, params1);
                        AppLogger.WriteLog("debug", $"Status-only query succeeded, returned {result1.Count} records");
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog("debug", $"Status-only query failed: {ex.Message}");
                        return new List<RepairsTbl>();
                    }

                    // Try with status + date filter (only if fromDate is provided)
                    DateTime? fromDateNullable = TrackerTools.ConvertToNullableDateTime(fromDate);
                    DateTime? toDateNullable = TrackerTools.ConvertToNullableDateTime(toDate);
                    AppLogger.WriteLog("debug", $"Converted fromDate: '{fromDateNullable}', toDate: '{toDateNullable}'");

                    if (fromDateNullable.HasValue)
                    {
                        string sql2 = CONST_SQL_SELECT + " WHERE RepairsTbl.RepairStatusID = ? AND DateLogged >= ?";
                        var params2 = new List<DBParameter>();
                        params2.Add(new DBParameter
                        {
                            DataValue = 7,
                            DataDbType = DbType.Int32
                        });
                        params2.Add(new DBParameter
                        {
                            DataValue = fromDateNullable.Value.Date,
                            DataDbType = DbType.Date
                        });

                        try
                        {
                            AppLogger.WriteLog("debug", "Testing status+date query...");
                            var result2 = GetAllRepairs(sortBy, sql2, params2);
                            AppLogger.WriteLog("debug", $"Status+date query succeeded, returned {result2.Count} records");
                        }
                        catch (Exception ex)
                        {
                            AppLogger.WriteLog("debug", $"Status+date query failed: {ex.Message}");
                            return new List<RepairsTbl>();
                        }
                    }

                    // Try the company subquery separately (only if filterText is provided)
                    if (!string.IsNullOrEmpty(filterText))
                    {
                        string sql3 = "SELECT CustomerID FROM CustomersTbl WHERE CompanyName LIKE ?";
                        var params3 = new List<DBParameter>();
                        params3.Add(new DBParameter
                        {
                            DataValue = $"%{filterText}%",
                            DataDbType = DbType.String
                        });

                        try
                        {
                            AppLogger.WriteLog("debug", "Testing company subquery...");
                            TrackerDb db = new TrackerDb();
                            using (db)
                            {
                                db.WhereParams = params3;
                                IDataReader reader = db.ExecuteSQLGetDataReader(sql3);
                                int count = 0;
                                if (reader != null)
                                {
                                    while (reader.Read())
                                    {
                                        count++;
                                        if (count <= 5) // Only log first 5 to avoid spam
                                        {
                                            AppLogger.WriteLog("debug", $"Found CustomerID: {reader["CustomerID"]}");
                                        }
                                    }
                                    reader.Close();
                                }
                                AppLogger.WriteLog("debug", $"Company subquery succeeded, found {count} customers");
                            }
                        }
                        catch (Exception ex)
                        {
                            AppLogger.WriteLog("debug", $"Company subquery failed: {ex.Message}");
                            return new List<RepairsTbl>();
                        }

                        // NOW TEST THE COMBINED QUERY - this is where it likely fails
                        if (fromDateNullable.HasValue && toDateNullable.HasValue)
                        {
                            string combinedSql = CONST_SQL_SELECT +
                                " WHERE RepairsTbl.RepairStatusID = ? AND DateLogged >= ? AND DateLogged <= ? AND CustomerID IN (SELECT CustomerID FROM CustomersTbl WHERE CompanyName LIKE ?)";

                            var combinedParams = new List<DBParameter>();
                            combinedParams.Add(new DBParameter { DataValue = 7, DataDbType = DbType.Int32 });
                            combinedParams.Add(new DBParameter { DataValue = fromDateNullable.Value.Date, DataDbType = DbType.Date });
                            combinedParams.Add(new DBParameter { DataValue = toDateNullable.Value.Date.AddDays(1).AddSeconds(-1), DataDbType = DbType.DateTime });
                            combinedParams.Add(new DBParameter { DataValue = $"%{filterText}%", DataDbType = DbType.String });

                            try
                            {
                                AppLogger.WriteLog("debug", "Testing COMBINED query...");
                                AppLogger.WriteLog("debug", $"Combined SQL: {combinedSql}");
                                var resultCombined = GetAllRepairs(sortBy, combinedSql, combinedParams);
                                AppLogger.WriteLog("debug", $"Combined query succeeded, returned {resultCombined.Count} records");

                                // If we get here, return the actual results instead of empty list
                                return resultCombined;
                            }
                            catch (Exception ex)
                            {
                                AppLogger.WriteLog("debug", $"COMBINED query failed: {ex.Message}");
                                // This is likely where the error occurs!
                                return new List<RepairsTbl>();
                            }
                        }
                    }

                    AppLogger.WriteLog("debug", "Debug tests completed - returning empty list");
                    return new List<RepairsTbl>();
                }
                /*
                            // Convert object parameters to nullable DateTime
                            DateTime? fromDateNullable = TrackerTools.ConvertToNullableDateTime(fromDate);
                            DateTime? toDateNullable = TrackerTools.ConvertToNullableDateTime(toDate);

                            // DEBUG: Log what we received
                            AppLogger.WriteLog("debug", $"Received params - repairStatus: '{repairStatus}', fromDate: '{fromDate}', toDate: '{toDate}', filterBy: '{filterBy}', filterText: '{filterText}'");
                            AppLogger.WriteLog("debug", $"Converted dates - fromDateNullable: '{fromDateNullable}', toDateNullable: '{toDateNullable}'");

                            // If no date filters are specified and no additional filters, use the existing status-only method
                            // Also check if filterBy is DateLogged (which means no text filter is selected)
                            if (!fromDateNullable.HasValue && !toDateNullable.HasValue &&
                                (string.IsNullOrEmpty(filterText) || filterBy == "DateLogged"))
                            {
                                AppLogger.WriteLog("debug", "Using simple status-only query");
                                return GetAllRepairsOfStatus(sortBy, repairStatus);
                            }

                            AppLogger.WriteLog("debug", "Using complex query with filters");

                            string sql = CONST_SQL_SELECT;
                            var parameters = new List<DBParameter>();
                            var whereConditions = new List<string>();

                            // Add status filter
                            if (!string.IsNullOrEmpty(repairStatus) && int.TryParse(repairStatus, out int statusId) && !repairStatus.Equals("OPEN"))
                            {
                                whereConditions.Add("RepairsTbl.RepairStatusID = ?");
                                parameters.Add(new DBParameter { DataValue = statusId, DataDbType = DbType.Int32 });
                                // DEBUG LOG
                                AppLogger.WriteLog("debug", $"Status filter: {statusId} (Int32)");
                            }
                            else if (repairStatus == "OPEN" || string.IsNullOrEmpty(repairStatus))
                            {
                                whereConditions.Add($"RepairsTbl.RepairStatusID <> {CONST_REPAIR_DONESTR}");
                            }

                            // Add date filters
                            if (fromDateNullable.HasValue)
                            {
                                whereConditions.Add("DateLogged >= ?");
                                parameters.Add(new DBParameter { DataValue = fromDateNullable.Value.Date, DataDbType = DbType.Date });
                                // DEBUG LOG
                                AppLogger.WriteLog("debug", $"Date filter: {fromDateNullable.Value.Date:yyyy-MM-dd} (Date)");
                            }

                            if (toDateNullable.HasValue)
                            {
                                whereConditions.Add("DateLogged <= ?");
                                parameters.Add(new DBParameter { DataValue = toDateNullable.Value.Date.AddDays(1).AddSeconds(-1), DataDbType = DbType.DateTime });
                            }

                            // Add text-based filters
                            if (!string.IsNullOrEmpty(filterText) && !string.IsNullOrEmpty(filterBy))
                            {
                                switch (filterBy)
                                {
                                    case "CompanyID":
                                        // Always use LIKE search - add wildcards if not present
                                        string companySearchText = filterText.Contains("%") ? filterText : $"%{filterText}%";
                                        whereConditions.Add("CustomerID IN (SELECT CustomerID FROM CustomersTbl WHERE CompanyName LIKE ?)");
                                        parameters.Add(new DBParameter { DataValue = companySearchText, DataDbType = DbType.String });
                                        // DEBUG LOG
                                        AppLogger.WriteLog("debug", $"Company filter: '{companySearchText}' (String)");
                                        break;

                                    case "MachineSerialNumber":
                                        // Always use LIKE search - add wildcards if not present  
                                        string serialSearchText = filterText.Contains("%") ? filterText : $"%{filterText}%";
                                        whereConditions.Add("MachineSerialNumber LIKE ?");
                                        parameters.Add(new DBParameter { DataValue = serialSearchText, DataDbType = DbType.String });
                                        break;
                                }
                            }

                            // Combine conditions
                            if (whereConditions.Count > 0)
                            {
                                sql += " WHERE " + string.Join(" AND ", whereConditions);
                            }
                            // DEBUG LOG
                            AppLogger.WriteLog("debug", $"Final SQL: {sql}");
                            AppLogger.WriteLog("debug", $"Parameter count: {parameters.Count}");

                            return GetAllRepairs(sortBy, sql, parameters);
                        }
                */

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public RepairsTbl GetRepairById(int repairId)
        {
            RepairsTbl repairById = null;
            if (repairId <= 0) return null;

            TrackerDb trackerDb = new TrackerDb();
            using (trackerDb)
            {
                trackerDb.AddWhereParams(repairId, DbType.Int32);
                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_SQL_SELECTBYREPAIRID);
                if (dataReader != null)
                {
                    if (dataReader.Read())
                    {
                        repairById = MapDataToRepair(dataReader, repairId);
                    }
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            return repairById;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public bool InsertRepair(RepairsTbl repair)
        {
            TrackerDb db = new TrackerDb();
            using (db)
            {
                AddRepairParameters(db, repair);
                string result = db.ExecuteNonQuerySQL(CONST_SQL_INSERT);
                return string.IsNullOrEmpty(result);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public string UpdateRepair(RepairsTbl repair)
        {
            return UpdateRepair(repair, repair.RepairID);
        }

        public string UpdateRepair(RepairsTbl repair, int origRepairId)
        {
            TrackerDb db = new TrackerDb();
            using (db)
            {
                AddRepairParameters(db, repair);
                db.AddWhereParams(origRepairId, DbType.Int32);
                return db.ExecuteNonQuerySQL(CONST_SQL_UPDATE);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public string DeleteRepair(int repairId)
        {
            TrackerDb db = new TrackerDb();
            using (db)
            {
                db.AddParams(repairId, DbType.Int32);
                return db.ExecuteNonQuerySQL(CONST_SQL_DELETE);
            }
        }

        public int GetLastIDInserted(long customerId)
        {
            TrackerDb db = new TrackerDb();
            using (db)
            {
                db.AddWhereParams(customerId, DbType.Int64);
                IDataReader reader = db.ExecuteSQLGetDataReader(CONST_SQL_SELECTLAST);
                if (reader != null)
                {
                    using (reader)
                    {
                        if (reader.Read())
                        {
                            return reader["RepairID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RepairID"]);
                        }
                    }
                }
                return 0;
            }
        }

        private List<RepairsTbl> GetAllRepairs(string sortBy, string sql, List<DBParameter> whereParams)
        {
            List<RepairsTbl> repairs = new List<RepairsTbl>();
            TrackerDb db = new TrackerDb();
            using (db)
            {
                sql = sql + " ORDER BY " + (string.IsNullOrEmpty(sortBy) ? "DateLogged DESC" : sortBy);

                if (whereParams != null)
                {
                    db.WhereParams = whereParams;
                }

                IDataReader reader = db.ExecuteSQLGetDataReader(sql);
                if (reader != null)
                {
                    using (reader)
                    {
                        while (reader.Read())
                        {
                            repairs.Add(MapDataToRepair(reader));
                        }
                    }
                }
            }
            return repairs;
        }

        private RepairsTbl MapDataToRepair(IDataReader reader, int? repairId = null)
        {
            return new RepairsTbl
            {
                RepairID = repairId ?? (reader["RepairID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RepairID"])),
                CustomerID = reader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CustomerID"]),
                ContactName = reader["ContactName"]?.ToString() ?? string.Empty,
                ContactEmail = reader["ContactEmail"]?.ToString() ?? string.Empty,
                JobCardNumber = reader["JobCardNumber"]?.ToString() ?? string.Empty,
                DateLogged = reader["DateLogged"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(reader["DateLogged"]).Date,
                LastStatusChange = reader["LastStatusChange"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(reader["LastStatusChange"]).Date,
                MachineTypeID = reader["MachineTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MachineTypeID"]),
                MachineSerialNumber = reader["MachineSerialNumber"]?.ToString() ?? string.Empty,
                SwopOutMachineID = reader["SwopOutMachineID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SwopOutMachineID"]),
                MachineConditionID = reader["MachineConditionID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MachineConditionID"]),
                TakenFrother = reader["TakenFrother"] != DBNull.Value && Convert.ToBoolean(reader["TakenFrother"]),
                TakenBeanLid = reader["TakenBeanLid"] != DBNull.Value && Convert.ToBoolean(reader["TakenBeanLid"]),
                TakenWaterLid = reader["TakenWaterLid"] != DBNull.Value && Convert.ToBoolean(reader["TakenWaterLid"]),
                BrokenFrother = reader["BrokenFrother"] != DBNull.Value && Convert.ToBoolean(reader["BrokenFrother"]),
                BrokenBeanLid = reader["BrokenBeanLid"] != DBNull.Value && Convert.ToBoolean(reader["BrokenBeanLid"]),
                BrokenWaterLid = reader["BrokenWaterLid"] != DBNull.Value && Convert.ToBoolean(reader["BrokenWaterLid"]),
                RepairFaultID = reader["RepairFaultID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RepairFaultID"]),
                RepairFaultDesc = reader["RepairFaultDesc"]?.ToString() ?? string.Empty,
                RepairStatusID = reader["RepairStatusID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RepairStatusID"]),
                RelatedOrderID = reader["RelatedOrderID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RelatedOrderID"]),
                Notes = reader["Notes"]?.ToString() ?? string.Empty
            };
        }

        private void AddRepairParameters(TrackerDb db, RepairsTbl repair)
        {
            db.AddParams(repair.CustomerID, DbType.Int64);
            db.AddParams(repair.ContactName, DbType.String);
            db.AddParams(repair.ContactEmail, DbType.String);
            db.AddParams(repair.JobCardNumber, DbType.String);
            db.AddParams(repair.DateLogged, DbType.Date);
            db.AddParams(repair.LastStatusChange, DbType.Date);
            db.AddParams(repair.MachineTypeID, DbType.Int32);
            db.AddParams(repair.MachineSerialNumber, DbType.String);
            db.AddParams(repair.SwopOutMachineID, DbType.Int32);
            db.AddParams(repair.MachineConditionID, DbType.Int32);
            db.AddParams(repair.TakenFrother, DbType.Boolean);
            db.AddParams(repair.TakenBeanLid, DbType.Boolean);
            db.AddParams(repair.TakenWaterLid, DbType.Boolean);
            db.AddParams(repair.BrokenFrother, DbType.Boolean);
            db.AddParams(repair.BrokenBeanLid, DbType.Boolean);
            db.AddParams(repair.BrokenWaterLid, DbType.Boolean);
            db.AddParams(repair.RepairFaultID, DbType.Int32);
            db.AddParams(repair.RepairFaultDesc, DbType.String);
            db.AddParams(repair.RepairStatusID, DbType.Int32);
            db.AddParams(repair.RelatedOrderID, DbType.Int64);
            db.AddParams(repair.Notes, DbType.String);
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<RepairsTbl> GetListOfRelatedTempOrders()
        {
            return GetAllRepairs(null, CONST_SQL_SELECTITEMWITHANORDER, null);
        }

        public static bool UpdateOrderNotesWithRepairStatus(RepairsTbl repair, string status)
        {
            if (repair.JobCardNumber.Equals(string.Empty))
            {
                repair.JobCardNumber = "n/a";
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"Repair with order related id: {repair.RelatedOrderID}, has not Job Card number set!");
            }
            //Get actual repair status string
            //string repairStatusStr = new RepairStatusesTbl().GetRepairStatusDesc(repair.RepairStatusID);
            //if (string.IsNullOrEmpty(repairStatusStr))
            //{
            //    AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"Repair with order related id: {repair.RelatedOrderID}, has invalid repair status id: {repair.RepairStatusID}!");
            //    repairStatusStr = "Unknown Status";
            //}
            // Get the associated order
            var orderTbl = new OrderTbl();
            var order = orderTbl.GetOrderByID(repair.RelatedOrderID);
            if (order == null) {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"Repair with order related id: {repair.RelatedOrderID}, not found in OrdersTbl!");
                return false; // Order not found
            }
            // Update the Notes field using the tag logic
            string startTag = string.Format(SystemConstants.RepairConstants.OrderNotesRepairStatusStartTag);
            int startIdx = order.Notes?.IndexOf(startTag, StringComparison.OrdinalIgnoreCase) ?? -1;
            if (startIdx >= 0)
            {
                int endIdx = order.Notes.IndexOf(SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd, startIdx);
                if (endIdx > startIdx)
                {
                    string before = order.Notes.Substring(0, startIdx);
                    string after = order.Notes.Substring(endIdx + 1);
                    string newBlock = $"{startTag} {status}{SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd}";
                    order.Notes = before + newBlock + after;
                }
            }
            else
            {
                order.Notes += $"{startTag} {status}{SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd}"; ;
            }

            // Save the updated order (implement your own save/update logic)
            bool success = orderTbl.UpdateOrderNotes(repair.RelatedOrderID, order.Notes) == String.Empty;
            if (success) AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"Repair with order related id: {repair.RelatedOrderID}, status changed to {status}.");
            else AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"Repair with order related id: {repair.RelatedOrderID}, status update failed.");

            return true;
        }
    }
}