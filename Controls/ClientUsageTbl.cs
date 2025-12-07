// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.ClientUsageTbl
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{

    public class ClientUsageTbl
    {
        //private const string CONST_CONSTRING = "Tracker08ConnectionString";
        private const string CONST_SQL_SELECT = "SELECT TOP 1 LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount  FROM ClientUsageTbl WHERE CustomerID = ?";
        private const string CONST_SQL_ISARECORD = "SELECT TOP 1 LastCupCount FROM ClientUsageTbl WHERE CustomerID = ?";
        private const string CONST_SQL_SELECTDAILYCONSUMPTION = "SELECT TOP 1 DailyConsumption FROM ClientUsageTbl WHERE CustomerID = ?";
        private const string CONST_SQL_UPDATE = "UPDATE ClientUsageTbl SET LastCupCount = ?, NextCoffeeBy= ?, NextCleanOn= ?, NextFilterEst= ?, NextDescaleEst= ?, NextServiceEst= ?,DailyConsumption= ?, CleanAveCount = ?, FilterAveCount = ?, DescaleAveCount = ?, ServiceAveCount = ?  WHERE CustomerID = ?";
        private const string CONST_SQL_FORCENEXCOFFEETBY = "UPDATE ClientUsageTbl SET NextCoffeeBy = ? WHERE CustomerID = ?";
        private long _CustomerID;
        private int _LastCupCount;
        private DateTime _NextCoffeeBy;
        private DateTime _NextCleanOn;
        private DateTime _NextFilterEst;
        private DateTime _NextDescaleEst;
        private DateTime _NextServiceEst;
        private double _DailyConsumption;
        private double _FilterAveCount;
        private double _DescaleAveCount;
        private double _ServiceAveCount;
        private double _CleanAveCount;

        public ClientUsageTbl()
        {
            this._CustomerID = 0;
            this._LastCupCount = 0;
            this._NextCoffeeBy = TimeZoneUtils.Now().Date;
            this._NextCleanOn = TimeZoneUtils.Now().Date;
            this._NextFilterEst = TimeZoneUtils.Now().Date;
            this._NextDescaleEst = TimeZoneUtils.Now().Date;
            this._NextServiceEst = TimeZoneUtils.Now().Date;
            this._DailyConsumption = 0.0;
            this._FilterAveCount = 0.0;
            this._DescaleAveCount = 0.0;
            this._ServiceAveCount = 0.0;
            this._CleanAveCount = 0.0;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public int LastCupCount
        {
            get => this._LastCupCount;
            set => this._LastCupCount = value;
        }

        public DateTime NextCoffeeBy
        {
            get => this._NextCoffeeBy;
            set => this._NextCoffeeBy = value;
        }

        public DateTime NextCleanOn
        {
            get => this._NextCleanOn;
            set => this._NextCleanOn = value;
        }

        public DateTime NextFilterEst
        {
            get => this._NextFilterEst;
            set => this._NextFilterEst = value;
        }

        public DateTime NextDescaleEst
        {
            get => this._NextDescaleEst;
            set => this._NextDescaleEst = value;
        }

        public DateTime NextServiceEst
        {
            get => this._NextServiceEst;
            set => this._NextServiceEst = value;
        }

        public double DailyConsumption
        {
            get => this._DailyConsumption;
            set => this._DailyConsumption = value;
        }

        public double FilterAveCount
        {
            get => this._FilterAveCount;
            set => this._FilterAveCount = value;
        }

        public double DescaleAveCount
        {
            get => this._DescaleAveCount;
            set => this._DescaleAveCount = value;
        }

        public double ServiceAveCount
        {
            get => this._ServiceAveCount;
            set => this._ServiceAveCount = value;
        }

        public double CleanAveCount
        {
            get => this._CleanAveCount;
            set => this._CleanAveCount = value;
        }

        public ClientUsageTbl GetUsageData(long pCustomerID)
        {
            ClientUsageTbl usageData = new ClientUsageTbl();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT TOP 1 LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount  FROM ClientUsageTbl WHERE CustomerID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    usageData.CustomerID = pCustomerID;
                    usageData.LastCupCount = dataReader["LastCupCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LastCupCount"]);
                    usageData.NextCoffeeBy = dataReader["NextCoffeeBy"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextCoffeeBy"]).Date;
                    usageData.NextCleanOn = dataReader["NextCleanOn"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextCleanOn"]).Date;
                    usageData.NextFilterEst = dataReader["NextFilterEst"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextFilterEst"]).Date;
                    usageData.NextDescaleEst = dataReader["NextDescaleEst"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextDescaleEst"]).Date;
                    usageData.NextServiceEst = dataReader["NextServiceEst"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextServiceEst"]).Date;
                    usageData.DailyConsumption = dataReader["DailyConsumption"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["DailyConsumption"]);
                    usageData.FilterAveCount = dataReader["FilterAveCount"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["FilterAveCount"]);
                    usageData.DescaleAveCount = dataReader["DescaleAveCount"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["DescaleAveCount"]);
                    usageData.ServiceAveCount = dataReader["ServiceAveCount"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["ServiceAveCount"]);
                    usageData.CleanAveCount = dataReader["CleanAveCount"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["CleanAveCount"]);
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return usageData;
        }

        public bool UsageDataExists(long pCustomerID)
        {
            bool flag = false;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT TOP 1 LastCupCount FROM ClientUsageTbl WHERE CustomerID = ?");
            if (dataReader != null)
            {
                flag = dataReader.Read();
                dataReader.Close();
            }
            trackerDb.Close();
            return flag;
        }

        public double GetAverageConsumption(long pCustomerID)
        {
            double averageConsumption = 5.0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT TOP 1 LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount  FROM ClientUsageTbl WHERE CustomerID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    averageConsumption = dataReader["DailyConsumption"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["DailyConsumption"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return averageConsumption;
        }

        public bool UpdateUsageCupCount(long pCustomerID, long pLastCupCount)
        {
            string strSQL = "UPDATE ClientUsageTbl SET LastCupCount = ? WHERE CustomerID = ?";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pLastCupCount, DbType.Int64, "@LastCupCount");
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL(strSQL));
            trackerDb.Close();
            return flag;
        }

        public bool Update(ClientUsageTbl pClientUsage)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pClientUsage.LastCupCount, DbType.Int64, "@LastCupCount");
            trackerDb.AddParams((object)pClientUsage.NextCoffeeBy, DbType.Date, "@NextCoffeeBy");
            trackerDb.AddParams((object)pClientUsage.NextCleanOn, DbType.Date, "@NextCleanOn");
            trackerDb.AddParams((object)pClientUsage.NextFilterEst, DbType.Date, "@NextFilterEst");
            trackerDb.AddParams((object)pClientUsage.NextDescaleEst, DbType.Date, "@NextDescaleEst");
            trackerDb.AddParams((object)pClientUsage.DailyConsumption, DbType.Double, "@DailyConsumption");
            trackerDb.AddParams((object)pClientUsage.CleanAveCount, DbType.Double, "@CleanAveCount");
            trackerDb.AddParams((object)pClientUsage.FilterAveCount, DbType.Double, "@FilterAveCount");
            trackerDb.AddParams((object)pClientUsage.DescaleAveCount, DbType.Double, "@DescaleAveCount");
            trackerDb.AddParams((object)pClientUsage.ServiceAveCount, DbType.Double, "@ServiceAveCount");
            trackerDb.AddWhereParams((object)pClientUsage.CustomerID, DbType.Int64, "@CustomerID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("UPDATE ClientUsageTbl SET LastCupCount = ?, NextCoffeeBy= ?, NextCleanOn= ?, NextFilterEst= ?, NextDescaleEst= ?, NextServiceEst= ?,DailyConsumption= ?, CleanAveCount = ?, FilterAveCount = ?, DescaleAveCount = ?, ServiceAveCount = ?  WHERE CustomerID = ?"));
            trackerDb.Close();
            return flag;
        }

        public bool ForceNextCoffeeDate(DateTime pNextDate, long pCustomerID)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pNextDate, DbType.Date, "@NextCoffeeBy");
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("UPDATE ClientUsageTbl SET NextCoffeeBy = ? WHERE CustomerID = ?"));
            trackerDb.Close();
            return flag;
        }
    }
}