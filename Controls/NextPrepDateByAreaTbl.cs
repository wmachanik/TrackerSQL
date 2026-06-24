// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.NextPreperationDateByAreaTbl
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class NextPreperationDateByAreaTbl
    {
        private const string CONST_SELECTBYCustomerID = "SELECT NextPreperationDateByAreaTbl.AreaID,  NextPreperationDateByAreaTbl.PreperationDate, NextPreperationDateByAreaTbl.DeliveryDate, NextPreperationDateByAreaTbl.DeliveryOrder,  NextPreperationDateByAreaTbl.NextPreperationDate,  NextPreperationDateByAreaTbl.NextDeliveryDate  FROM  (NextPreperationDateByAreaTbl RIGHT OUTER JOIN CustomersTbl ON NextPreperationDateByAreaTbl.AreaID = CustomersTbl.AreaID)  WHERE (CustomersTbl.CustomerID = ?) ";
        private const string CONST_SELECTALL = "SELECT NextPreperationDateByAreaTbl.AreaID, NextPreperationDateByAreaTbl.PreperationDate, NextPreperationDateByAreaTbl.DeliveryDate,  NextPreperationDateByAreaTbl.DeliveryOrder, NextPreperationDateByAreaTbl.NextPreperationDate, NextPreperationDateByAreaTbl.NextDeliveryDate  FROM NextPreperationDateByAreaTbl";
        private const string CONST_SELECTALLDELIVERYDATES = "SELECT DISTINCT DeliveryDate FROM NextPreperationDateByAreaTbl ORDER BY DeliveryDate ";
        private const string CONST_SELECTIDBYDELIVERYDATES = "SELECT NextPrepDayID FROM NextPreperationDateByAreaTbl WHERE (DeliveryDate = ?)";
        private const string CONST_UPDATE = "UPDATE NextPreperationDateByAreaTbl SET PreperationDate = ?, DeliveryDate = ?, DeliveryOrder = ?, NextDeliveryDate = ?, NextPreperationDate = ? WHERE AreaID = ?";
        private const string CONST_INSERT = "INSERT INTO NextPreperationDateByAreaTbl (AreaID, PreperationDate, DeliveryDate, DeliveryOrder, NextDeliveryDate, NextPreperationDate) VALUES (?,?,?,?,?,?)";
        private const string CONST_UPDATE_MOVEDELIVERYDATE = "UPDATE NextPreperationDateByAreaTbl SET DeliveryDate = ? WHERE (NextPreperationDateByAreaTbl.DeliveryDate = ?)";
        private const string CONST_UPDATEDELIVERYDATEBYID = "UPDATE NextPreperationDateByAreaTbl SET DeliveryDate = ? WHERE (NextPreperationDateByAreaTbl.NextPrepDayID = ?)";
        private const string CONST_SQL_CUSTOMERSNEXTDELIVERYDATE = "SELECT NextPreperationDateByAreaTbl.DeliveryDate FROM  (CustomersTbl INNER JOIN  NextPreperationDateByAreaTbl ON CustomersTbl.AreaID = NextPreperationDateByAreaTbl.AreaID) WHERE (CustomersTbl.CustomerID = ?)";
        private int _AreaID;
        private DateTime _DeliveryDate;
        private DateTime _PrepDate;
        private int _DeliveryOrder;
        private DateTime _NextDeliveryDate;
        private DateTime _NextPreperationDate;

        public NextPreperationDateByAreaTbl()
        {
            this._AreaID = 0;
            this._DeliveryOrder = 0;
            this._DeliveryDate = this._PrepDate = DateTime.MinValue;
            this._NextDeliveryDate = this._NextPreperationDate = DateTime.MinValue;
        }

        public int AreaID
        {
            get => this._AreaID;
            set => this._AreaID = value;
        }

        public DateTime DeliveryDate
        {
            get => this._DeliveryDate.Date;
            set => this._DeliveryDate = value;
        }

        public DateTime PrepDate
        {
            get => this._PrepDate.Date;
            set => this._PrepDate = value;
        }

        public int DeliveryOrder
        {
            get => this._DeliveryOrder;
            set => this._DeliveryOrder = value;
        }

        public DateTime NextDeliveryDate
        {
            get => this._NextDeliveryDate.Date;
            set => this._NextDeliveryDate = value;
        }

        public DateTime NextPreperationDate
        {
            get => this._NextPreperationDate.Date;
            set => this._NextPreperationDate = value;
        }

        public NextPreperationDateByAreaTbl GetPrepDataForCustomer(long pCustomerID)
        {
            NextPreperationDateByAreaTbl prepDataForCustomer = new NextPreperationDateByAreaTbl();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT NextPreperationDateByAreaTbl.AreaID, NextPreperationDateByAreaTbl.PreperationDate, NextPreperationDateByAreaTbl.DeliveryDate, NextPreperationDateByAreaTbl.DeliveryOrder," +
                " NextPreperationDateByAreaTbl.NextPreperationDate, NextPreperationDateByAreaTbl.NextDeliveryDate FROM " +
                "(NextPreperationDateByAreaTbl RIGHT OUTER JOIN CustomersTbl ON NextPreperationDateByAreaTbl.AreaID = CustomersTbl.AreaID) WHERE (CustomersTbl.CustomerID = ?)";
            try
            {
                // Validate input
                if (pCustomerID <= 0)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Invalid CustomerID: {pCustomerID}");
                    return prepDataForCustomer;
                }

                trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");

                // Use retry logic for critical operations
                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);

                // Check if there was a database error after the call
                TrackerTools trackerTools = new TrackerTools();
                if (trackerTools.IsTrackerSessionErrorString())
                {
                    string errorMsg = trackerTools.GetTrackerSessionErrorString();
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Error in GetPrepDataForCustomer for customer {pCustomerID}: {errorMsg}");
                    trackerTools.ClearTrackerSessionErrorString();
                    return prepDataForCustomer; // Return default object
                }

                if (dataReader != null)
                {
                    try
                    {
                        if (dataReader.Read())
                        {
                            prepDataForCustomer = SafeReadDataFromReader(dataReader);
                        }
                        else
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"No data found for customer {pCustomerID}");
                        }
                    }
                    catch (Exception readEx)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Error reading data for customer {pCustomerID}: {readEx.Message}");
                    }
                    finally
                    {
                        dataReader.Dispose();
                    }
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"DataReader is null for customer {pCustomerID}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Unexpected error in GetPrepDataForCustomer for customer {pCustomerID}: {ex.Message}");
            }
            finally
            {
                try
                {
                    trackerDb.Close();
                }
                catch (Exception closeEx)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Error closing database connection: {closeEx.Message}");
                }
            }

            return prepDataForCustomer;
        }
        private NextPreperationDateByAreaTbl SafeReadDataFromReader(IDataReader dataReader)
        {
            var result = new NextPreperationDateByAreaTbl();

            try
            {
                result.AreaID = SafeGetInt32(dataReader, "AreaID", 0);
                result.PrepDate = SafeGetDateTime(dataReader, "PreperationDate", DateTime.MinValue);
                result.DeliveryDate = SafeGetDateTime(dataReader, "DeliveryDate", DateTime.MinValue);
                result.DeliveryOrder = SafeGetInt32(dataReader, "DeliveryOrder", 0);
                result.NextPreperationDate = SafeGetDateTime(dataReader, "NextPreperationDate", DateTime.MinValue);
                result.NextDeliveryDate = SafeGetDateTime(dataReader, "NextDeliveryDate", DateTime.MinValue);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Error reading individual fields: {ex.Message}");
            }

            return result;
        }
        private int SafeGetInt32(IDataReader reader, string columnName, int defaultValue)
        {
            try
            {
                return reader[columnName] == DBNull.Value ? defaultValue : Convert.ToInt32(reader[columnName]);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Error reading {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        private DateTime SafeGetDateTime(IDataReader reader, string columnName, DateTime defaultValue)
        {
            try
            {
                return reader[columnName] == DBNull.Value ? defaultValue : Convert.ToDateTime(reader[columnName]).Date;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Error reading {columnName}: {ex.Message}");
                return defaultValue;
            }
        }
        public List<NextPreperationDateByAreaTbl> GetAll(string SortBy)
        {
            List<NextPreperationDateByAreaTbl> all = new List<NextPreperationDateByAreaTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT NextPreperationDateByAreaTbl.AreaID, NextPreperationDateByAreaTbl.PreperationDate, NextPreperationDateByAreaTbl.DeliveryDate,  NextPreperationDateByAreaTbl.DeliveryOrder, NextPreperationDateByAreaTbl.NextPreperationDate, NextPreperationDateByAreaTbl.NextDeliveryDate  FROM NextPreperationDateByAreaTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new NextPreperationDateByAreaTbl()
                    {
                        AreaID = dataReader["AreaID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["AreaID"]),
                        PrepDate = dataReader["PreperationDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["PreperationDate"]).Date,
                        DeliveryDate = dataReader["DeliveryDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["DeliveryDate"]).Date,
                        DeliveryOrder = dataReader["DeliveryOrder"] == DBNull.Value ? 100 : Convert.ToInt32(dataReader["DeliveryOrder"]),
                        NextPreperationDate = dataReader["NextPreperationDate"] == DBNull.Value ? TimeZoneUtils.Now().Date.AddDays(7.0).Date : Convert.ToDateTime(dataReader["NextPreperationDate"]).Date,
                        NextDeliveryDate = dataReader["NextDeliveryDate"] == DBNull.Value ? TimeZoneUtils.Now().Date.AddDays(7.0).Date : Convert.ToDateTime(dataReader["NextDeliveryDate"]).Date
                    });
                dataReader.Dispose();
            }
            trackerDb.Close();
            return all;
        }

        public List<DateTime> GetAllDeliveryDates()
        {
            List<DateTime> allDeliveryDates = new List<DateTime>();
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT DISTINCT DeliveryDate FROM NextPreperationDateByAreaTbl ORDER BY DeliveryDate ");
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    DateTime dateTime = dataReader["DeliveryDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["DeliveryDate"]).Date;
                    allDeliveryDates.Add(dateTime);
                }
                dataReader.Dispose();
            }
            trackerDb.Close();
            return allDeliveryDates;
        }

        public List<int> GetAllIDsByDate(DateTime pDeliveryDate)
        {
            List<int> allIdsByDate = new List<int>();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pDeliveryDate, DbType.Date);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT NextPrepDayID FROM NextPreperationDateByAreaTbl WHERE (DeliveryDate = ?)");
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    int int32 = dataReader["NextPrepDayID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["NextPrepDayID"]);
                    allIdsByDate.Add(int32);
                }
                dataReader.Dispose();
            }
            trackerDb.Close();
            return allIdsByDate;
        }

        public string UpdatePrepDataForArea(int pAreaID, NextPreperationDateByAreaTbl pNextPrepAreaTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            string str;
            if (pNextPrepAreaTbl != null)
            {
                trackerDb.AddParams((object)pNextPrepAreaTbl.PrepDate, DbType.Date, "@PreperationDate");
                trackerDb.AddParams((object)pNextPrepAreaTbl.DeliveryDate, DbType.Date, "@DeliveryDate");
                trackerDb.AddParams((object)pNextPrepAreaTbl.DeliveryOrder, DbType.Int16, "@DeliveryOrder");
                trackerDb.AddParams((object)pNextPrepAreaTbl.NextDeliveryDate, DbType.Date, "@NextDeliveryDate");
                trackerDb.AddParams((object)pNextPrepAreaTbl.NextPreperationDate, DbType.Date, "@NextPreperationDate");
                trackerDb.AddWhereParams((object)pNextPrepAreaTbl.AreaID, DbType.Int32, "@AreaID");
                str = trackerDb.ExecuteNonQuerySQL("UPDATE NextPreperationDateByAreaTbl SET PreperationDate = ?, DeliveryDate = ?, DeliveryOrder = ?, NextDeliveryDate = ?, NextPreperationDate = ? WHERE AreaID = ?");
            }
            else
                str = "null data passed-update failed";
            trackerDb.Close();
            return str;
        }

        public string InsertPrepDataForArea(NextPreperationDateByAreaTbl pNextPrepAreaTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            string str;
            if (pNextPrepAreaTbl != null)
            {
                trackerDb.AddParams((object)pNextPrepAreaTbl.AreaID, DbType.Int32);
                trackerDb.AddParams((object)pNextPrepAreaTbl.PrepDate, DbType.Date);
                trackerDb.AddParams((object)pNextPrepAreaTbl.DeliveryDate, DbType.Date);
                trackerDb.AddParams((object)pNextPrepAreaTbl.DeliveryOrder, DbType.Int16);
                trackerDb.AddParams((object)pNextPrepAreaTbl.NextDeliveryDate, DbType.Date);
                trackerDb.AddParams((object)pNextPrepAreaTbl.NextPreperationDate, DbType.Date);
                str = trackerDb.ExecuteNonQuerySQL("INSERT INTO NextPreperationDateByAreaTbl (AreaID, PreperationDate, DeliveryDate, DeliveryOrder, NextDeliveryDate, NextPreperationDate) VALUES (?,?,?,?,?,?)");
            }
            else
                str = "null data passed-insert failed";
            trackerDb.Close();
            return str;
        }

        public string MoveDeliveryDate(
          DateTime pOldDeliveryDate,
          DateTime pNewDeliveryDate,
          ref int pNumRecs)
        {
            string empty = string.Empty;
            string strSQL = "UPDATE NextPreperationDateByAreaTbl SET DeliveryDate = ? WHERE (NextPreperationDateByAreaTbl.DeliveryDate = ?)";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pNewDeliveryDate.Date, DbType.DateTime);
            trackerDb.AddWhereParams((object)pOldDeliveryDate, DbType.Date);
            string str = trackerDb.ExecuteNonQuerySQL(strSQL);
            pNumRecs = trackerDb.numRecs;
            trackerDb.Close();
            return str;
        }

        public string UpdateDeliveryDateByID(int pNextPrepDayID, DateTime pDeliveryDate)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pDeliveryDate.Date, DbType.DateTime);
            trackerDb.AddWhereParams((object)pNextPrepDayID, DbType.Int32);
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE NextPreperationDateByAreaTbl SET DeliveryDate = ? WHERE (NextPreperationDateByAreaTbl.NextPrepDayID = ?)");
            trackerDb.Close();
            return str;
        }

        public DateTime GetNextDeliveryDate(long pCustomerID)
        {
            DateTime nextDeliveryDate = DateTime.MinValue;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT NextPreperationDateByAreaTbl.DeliveryDate FROM  (CustomersTbl INNER JOIN  NextPreperationDateByAreaTbl ON CustomersTbl.AreaID = NextPreperationDateByAreaTbl.AreaID) WHERE (CustomersTbl.CustomerID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    nextDeliveryDate = dataReader["DeliveryDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["DeliveryDate"]).Date;
                dataReader.Dispose();
            }
            trackerDb.Close();
            return nextDeliveryDate;
        }
    }
}
