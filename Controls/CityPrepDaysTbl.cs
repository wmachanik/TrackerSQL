// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.CityPrepDaysTbl
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
    public class CityPrepDaysTbl
    {
        private const string CONST_SQL_SELECT = "SELECT CityPrepDaysID, CityID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM CityPrepDaysTbl";
        private const string CONST_SQL_SELECTBYCITYID = "SELECT CityPrepDaysID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM CityPrepDaysTbl WHERE CityID = ? ORDER BY PrepDayOfWeekID";
        private const string CONST_SQL_INSERT = "INSERT INTO CityPrepDaysTbl (CityID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) VALUES (  ?   ,      ?         ,        ?         ,      ?)";
        private const string CONST_SQL_UPDATEBYID = "UPDATE CityPrepDaysTbl SET CityID = ? , PrepDayOfWeekID = ?, DeliveryDelayDays = ?, DeliveryOrder = ? WHERE (CityPrepDaysID = ?)";
        private const string CONST_SQL_DELETEBYID = "DELETE FROM CityPrepDaysTbl WHERE (CityPrepDaysID = ?)";
        private int _CityPrepDaysID;
        private int _CityID;
        private byte _PrepDayOfWeekID;
        private int _DeliveryDelayDays;
        private int _DeliveryOrder;

        public CityPrepDaysTbl()
        {
            this._CityPrepDaysID = 0;
            this._CityID = 0;
            this._PrepDayOfWeekID = (byte)0;
            this._DeliveryDelayDays = 0;
            this._DeliveryOrder = 0;
        }

        public int CityPrepDaysID
        {
            get => this._CityPrepDaysID;
            set => this._CityPrepDaysID = value;
        }

        public int CityID
        {
            get => this._CityID;
            set => this._CityID = value;
        }

        public byte PrepDayOfWeekID
        {
            get => this._PrepDayOfWeekID;
            set => this._PrepDayOfWeekID = value;
        }

        public int DeliveryDelayDays
        {
            get => this._DeliveryDelayDays;
            set => this._DeliveryDelayDays = value;
        }

        public int DeliveryOrder
        {
            get => this._DeliveryOrder;
            set => this._DeliveryOrder = value;
        }

        public List<CityPrepDaysTbl> GetAll(string SortBy = "")
        {
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT CityPrepDaysID, CityID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM CityPrepDaysTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            List<CityPrepDaysTbl> all = new List<CityPrepDaysTbl>();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new CityPrepDaysTbl()
                    {
                        CityPrepDaysID = dataReader["CityPrepDaysID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CityPrepDaysID"]),
                        CityID = dataReader["CityID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CityID"]),
                        PrepDayOfWeekID = dataReader["PrepDayOfWeekID"] == DBNull.Value ? (byte)0 : Convert.ToByte(dataReader["PrepDayOfWeekID"]),
                        DeliveryDelayDays = dataReader["DeliveryDelayDays"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryDelayDays"]),
                        DeliveryOrder = dataReader["DeliveryOrder"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryOrder"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public List<CityPrepDaysTbl> GetAllByCityId(int pCityID)
        {
            string strSQL = "SELECT CityPrepDaysID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM CityPrepDaysTbl WHERE CityID = ? ORDER BY PrepDayOfWeekID";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCityID, DbType.Int32);
            List<CityPrepDaysTbl> allByCityId = new List<CityPrepDaysTbl>();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allByCityId.Add(new CityPrepDaysTbl()
                    {
                        CityID = pCityID,
                        CityPrepDaysID = dataReader["CityPrepDaysID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CityPrepDaysID"]),
                        PrepDayOfWeekID = dataReader["PrepDayOfWeekID"] == DBNull.Value ? (byte)0 : Convert.ToByte(dataReader["PrepDayOfWeekID"]),
                        DeliveryDelayDays = dataReader["DeliveryDelayDays"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryDelayDays"]),
                        DeliveryOrder = dataReader["DeliveryOrder"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryOrder"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return allByCityId;
        }

        public string InsertCityPrepDay(CityPrepDaysTbl objCityPrepDaysTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)objCityPrepDaysTbl.CityID, DbType.Int32);
            trackerDb.AddParams((object)objCityPrepDaysTbl.PrepDayOfWeekID, DbType.Byte);
            trackerDb.AddParams((object)objCityPrepDaysTbl.DeliveryDelayDays, DbType.Int32);
            trackerDb.AddParams((object)objCityPrepDaysTbl.DeliveryOrder, DbType.Int32);
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO CityPrepDaysTbl (CityID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) VALUES (  ?   ,      ?         ,        ?         ,      ?)");
            trackerDb.Close();
            return str;
        }

        public string UpdateCityPrepDay(CityPrepDaysTbl objCityPrepDaysTbl)
        {
            return this.UpdateCityPrepDay(objCityPrepDaysTbl, objCityPrepDaysTbl.CityPrepDaysID);
        }

        public string UpdateCityPrepDay(CityPrepDaysTbl objCityPrepDaysTbl, int origCityPrepDaysID)
        {
            string str = string.Empty;
            if (origCityPrepDaysID > 0)
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddParams((object)objCityPrepDaysTbl.CityID, DbType.Int32);
                trackerDb.AddParams((object)objCityPrepDaysTbl.PrepDayOfWeekID, DbType.Byte);
                trackerDb.AddParams((object)objCityPrepDaysTbl.DeliveryDelayDays, DbType.Int32);
                trackerDb.AddParams((object)objCityPrepDaysTbl.DeliveryOrder, DbType.Int32);
                trackerDb.AddWhereParams((object)origCityPrepDaysID, DbType.Int32);
                str = trackerDb.ExecuteNonQuerySQL("UPDATE CityPrepDaysTbl SET CityID = ? , PrepDayOfWeekID = ?, DeliveryDelayDays = ?, DeliveryOrder = ? WHERE (CityPrepDaysID = ?)");
                trackerDb.Close();
            }
            return str;
        }

        public string DeleteByCityPrepDayID(int pCityPrepDayID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCityPrepDayID, DbType.Int32, "@CityPrepDayID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM CityPrepDaysTbl WHERE (CityPrepDaysID = ?)");
            trackerDb.Close();
            return str;
        }
    }
}