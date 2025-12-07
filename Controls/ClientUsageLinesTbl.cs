// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.ClientUsageLinesTbl
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
    public class ClientUsageLinesTbl
    {
        private const string CONST_SQL_SELECT = "SELECT ClientUsageLineNo, [Date] AS LineDate, CupCount, ServiceTypeID, Qty, Notes FROM ClientUsageLinesTbl ";
        private const string CONST_SQL_UPDATE = "UPDATE ClientUsageLinesTbl SET CustomerID = ?, [Date] = ?, CupCount = ?, ServiceTypeID = ? , Qty = ?, Notes = ? WHERE ClientUsageLineNo = ? ";
        private const string CONST_SQL_INSERT = "INSERT INTO ClientUsageLinesTbl (CustomerID, [Date], CupCount, ServiceTypeID, Qty, Notes)  VALUES (?, ?, ?, ?, ?, ?)";
        private const string CONST_SQL_SELECTSERVICELINES = "SELECT ClientUsageLineNo, CustomerID, [Date] AS LineDate, CupCount, ServiceTypeID, Qty, Notes  FROM ClientUsageLinesTbl WHERE ClientUsageLinesTbl.CustomerID = ? AND ClientUsageLinesTbl.ServiceTypeID = ?";
        private const string CONST_SQL_LASTESTUSAGEDATA = "SELECT TOP 1 ClientUsageLineNo, CustomerID, [Date] As LineDate, CupCount, ServiceTypeID, Qty  FROM ClientUsageLinesTbl WHERE CustomerID = ? ";
        private int _ClientUsageLineNo;
        private long _CustomerID;   //issues with this beiun glong in the 32 bit verion, but have kept it as long for compatibility with other parts of the code
        private DateTime _LineDate;
        private int _CupCount;   // change from long for this implemention
        private int _ServiceTypeID;
        private double _Qty;
        private string _Notes;

        public ClientUsageLinesTbl()
        {
            this._ClientUsageLineNo = 0;
            this._CustomerID = 0;
            this._LineDate = DateTime.MinValue;
            this._CupCount = 0;
            this._ServiceTypeID = 0;
            this._Qty = 0.0;
            this._Notes = string.Empty;
        }

        public int ClientUsageLineNo
        {
            get => this._ClientUsageLineNo;
            set => this._ClientUsageLineNo = value;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public DateTime LineDate
        {
            get => this._LineDate;
            set => this._LineDate = value;
        }

        public int CupCount
        {
            get => this._CupCount;
            set => this._CupCount = value;
        }

        public int ServiceTypeID
        {
            get => this._ServiceTypeID;
            set => this._ServiceTypeID = value;
        }

        public double Qty
        {
            get => this._Qty;
            set => this._Qty = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        public List<ClientUsageLinesTbl> GetAllClientUsageLinesTbl(long pCustomerID, string SortBy)
        {
            List<ClientUsageLinesTbl> clientUsageLinesTbl = new List<ClientUsageLinesTbl>();
            string strSQL = "SELECT ClientUsageLineNo, [Date] AS LineDate, CupCount, ServiceTypeID, Qty, Notes FROM ClientUsageLinesTbl  WHERE CustomerID = " + pCustomerID.ToString();
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    clientUsageLinesTbl.Add(new ClientUsageLinesTbl()
                    {
                        ClientUsageLineNo = dataReader["ClientUsageLineNo"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ClientUsageLineNo"]),
                        CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]),
                        LineDate = dataReader["LineDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["LineDate"]).Date,
                        CupCount = dataReader["CupCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CupCount"]),
                        ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]),
                        Qty = dataReader["Qty"] == DBNull.Value ? 0.0 : Math.Round(Convert.ToDouble(dataReader["Qty"]), SystemConstants.DatabaseConstants.NumDecimalPoints),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return clientUsageLinesTbl;
        }

        public bool InsertItemsUsed(ClientUsageLinesTbl pClientUsageLine)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pClientUsageLine.CustomerID, DbType.Int64);
            trackerDb.AddParams((object)pClientUsageLine.LineDate, DbType.Date);
            trackerDb.AddParams((object)pClientUsageLine.CupCount, DbType.Int64);
            trackerDb.AddParams((object)pClientUsageLine.ServiceTypeID, DbType.Int32);
            trackerDb.AddParams((object)Math.Round(pClientUsageLine.Qty, SystemConstants.DatabaseConstants.NumDecimalPoints), DbType.Double);
            trackerDb.AddParams((object)pClientUsageLine.Notes, DbType.String);
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQLWithParams("INSERT INTO ClientUsageLinesTbl (CustomerID, [Date], CupCount, ServiceTypeID, Qty, Notes)  VALUES (?, ?, ?, ?, ?, ?)", trackerDb.Params));
            trackerDb.Close();
            return flag;
        }

        public bool UpdateItemsUsed(ClientUsageLinesTbl pClientUsageLine, long OriginalClientUsageLineNo)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pClientUsageLine.CustomerID, DbType.Int64);
            trackerDb.AddParams((object)pClientUsageLine.LineDate, DbType.Date);
            trackerDb.AddParams((object)pClientUsageLine.CupCount, DbType.Int64);
            trackerDb.AddParams((object)pClientUsageLine.ServiceTypeID, DbType.Int32);
            trackerDb.AddParams((object)Math.Round(pClientUsageLine.Qty, SystemConstants.DatabaseConstants.NumDecimalPoints), DbType.Double);
            trackerDb.AddParams((object)pClientUsageLine.Notes, DbType.String);
            trackerDb.AddWhereParams((object)OriginalClientUsageLineNo, DbType.Int64);
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQLWithParams("UPDATE ClientUsageLinesTbl SET CustomerID = ?, [Date] = ?, CupCount = ?, ServiceTypeID = ? , Qty = ?, Notes = ? WHERE ClientUsageLineNo = ? ", trackerDb.Params, trackerDb.WhereParams));
            trackerDb.Close();
            return flag;
        }

        public DateTime GetCustomerInstallDate(long pCustomerID)
        {
            DateTime customerInstallDate = DateTime.MinValue;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.WhereParams.Add(new DBParameter()
            {
                DataValue = (object)pCustomerID,
                DataDbType = DbType.Int32
            });
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT MIN(ClientUsageLinesTbl.[Date]) AS MinDate FROM ClientUsageLinesTbl WHERE ClientUsageLinesTbl.CustomerID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    customerInstallDate = dataReader["MinDate"] == DBNull.Value ? DateTime.MinValue : (DateTime)dataReader["MinDate"];
                dataReader.Close();
            }
            trackerDb.Close();
            return customerInstallDate;
        }

        public List<ClientUsageLinesTbl> GetAllCustomerServiceLines(int pCustomerID)
        {
            return this.GetAllCustomerServiceLines(pCustomerID, 2, "ClientUsageLinesTbl.Date DESC");
        }

        public List<ClientUsageLinesTbl> GetAllCustomerServiceLines(int pCustomerID, int pServiceTypeID)
        {
            return this.GetAllCustomerServiceLines(pCustomerID, pServiceTypeID, "ClientUsageLinesTbl.Date DESC");
        }

        public List<ClientUsageLinesTbl> GetAllCustomerServiceLines(
          int pCustomerID,
          int pServiceTypeID,
          string pSortBy)
        {
            List<ClientUsageLinesTbl> customerServiceLines = new List<ClientUsageLinesTbl>();
            string strSQL = "SELECT ClientUsageLineNo, CustomerID, [Date] AS LineDate, CupCount, ServiceTypeID, Qty, Notes  FROM ClientUsageLinesTbl WHERE ClientUsageLinesTbl.CustomerID = ? AND ClientUsageLinesTbl.ServiceTypeID = ?";
            if (!string.IsNullOrEmpty(pSortBy))
                strSQL = $"{strSQL} ORDER BY {pSortBy}";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            trackerDb.AddWhereParams((object)pServiceTypeID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    ClientUsageLinesTbl clientUsageLinesTbl = new ClientUsageLinesTbl()
                    {
                        CustomerID = pCustomerID,
                        ClientUsageLineNo = dataReader["ClientUsageLineNo"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ClientUsageLineNo"])
                    };
                    clientUsageLinesTbl.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    clientUsageLinesTbl.LineDate = dataReader["LineDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["LineDate"]).Date;
                    clientUsageLinesTbl.CupCount = dataReader["CupCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CupCount"]);
                    clientUsageLinesTbl.ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]);
                    clientUsageLinesTbl.Qty = dataReader["Qty"] == DBNull.Value ? 0.0 : Math.Round(Convert.ToDouble(dataReader["Qty"]), SystemConstants.DatabaseConstants.NumDecimalPoints);
                    clientUsageLinesTbl.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                    customerServiceLines.Add(clientUsageLinesTbl);
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return customerServiceLines;
        }

        public List<ClientUsageLinesTbl> GetLast10UsageLines(long pCustomerID)
        {
            return this.GetLast10UsageLines(pCustomerID, 0);
        }

        public List<ClientUsageLinesTbl> GetLast10UsageLines(long pCustomerID, int pServiceTypeID)
        {
            List<ClientUsageLinesTbl> last10UsageLines = new List<ClientUsageLinesTbl>();
            string str = "SELECT TOP 10 ClientUsageLineNo, CustomerID, [Date] AS LineDate, CupCount, ServiceTypeID, Qty FROM ClientUsageLinesTbl WHERE ClientUsageLinesTbl.CustomerID = ? ";
            if (pServiceTypeID > 0)
                str += " AND ClientUsageLinesTbl.ServiceTypeID = ?";
            string strSQL = str + " ORDER BY ClientUsageLinesTbl.Date DESC";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            if (pServiceTypeID > 0)
                trackerDb.AddWhereParams((object)pServiceTypeID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    ClientUsageLinesTbl clientUsageLinesTbl = new ClientUsageLinesTbl()
                    {
                        CustomerID = pCustomerID,
                        ClientUsageLineNo = dataReader["ClientUsageLineNo"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ClientUsageLineNo"])
                    };
                    clientUsageLinesTbl.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    clientUsageLinesTbl.LineDate = dataReader["LineDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["LineDate"]).Date;
                    clientUsageLinesTbl.CupCount = dataReader["CupCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CupCount"]);
                    clientUsageLinesTbl.ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]);
                    clientUsageLinesTbl.Qty = dataReader["Qty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["Qty"]);
                    last10UsageLines.Add(clientUsageLinesTbl);
                }
                dataReader.Dispose();
            }
            trackerDb.Close();
            return last10UsageLines;
        }

        public ClientUsageLinesTbl GetLatestUsageData(long pCustomerID, int pServiceTypeID)
        {
            ClientUsageLinesTbl latestUsageData = (ClientUsageLinesTbl)null;
            TrackerDb trackerDb = new TrackerDb();
            string str = "SELECT TOP 1 ClientUsageLineNo, CustomerID, [Date] As LineDate, CupCount, ServiceTypeID, Qty  FROM ClientUsageLinesTbl WHERE CustomerID = ? ";
            if (pServiceTypeID > 0)
                str += "AND ServiceTypeID = ?";
            string strSQL = str + " ORDER BY [Date] DESC";
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            if (pServiceTypeID > 0)
                trackerDb.AddWhereParams((object)pServiceTypeID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    latestUsageData = new ClientUsageLinesTbl()
                    {
                        CustomerID = pCustomerID,
                        ClientUsageLineNo = dataReader["ClientUsageLineNo"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ClientUsageLineNo"])
                    };
                    latestUsageData.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    latestUsageData.LineDate = dataReader["LineDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["LineDate"]).Date;
                    latestUsageData.CupCount = dataReader["CupCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CupCount"]);
                    latestUsageData.ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]);
                    latestUsageData.Qty = dataReader["Qty"] == DBNull.Value ? 0.0 : Math.Round(Convert.ToDouble(dataReader["Qty"]), SystemConstants.DatabaseConstants.NumDecimalPoints);
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return latestUsageData;
        }
    }
}