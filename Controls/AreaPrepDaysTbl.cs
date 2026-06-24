// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.AreaPrepDaysTbl
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
    [Obsolete("DO NOT USE Comtrols use Models - MIGRATION IN PROGRESS", true)]
    public class AreaPrepDaysTbl
    {
        private const string CONST_SQL_SELECT = "SELECT AreaPrepDaysID, AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM AreaPrepDaysTbl";
        private const string CONST_SQL_SELECTBYAreaID = "SELECT AreaPrepDaysID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM AreaPrepDaysTbl WHERE AreaID = ? ORDER BY PrepDayOfWeekID";
        private const string CONST_SQL_INSERT = "INSERT INTO AreaPrepDaysTbl (AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) VALUES (  ?   ,      ?         ,        ?         ,      ?)";
        private const string CONST_SQL_UPDATEBYID = "UPDATE AreaPrepDaysTbl SET AreaID = ? , PrepDayOfWeekID = ?, DeliveryDelayDays = ?, DeliveryOrder = ? WHERE (AreaPrepDaysID = ?)";
        private const string CONST_SQL_DELETEBYID = "DELETE FROM AreaPrepDaysTbl WHERE (AreaPrepDaysID = ?)";
        private int _AreaPrepDaysID;
        private int _AreaID;
        private byte _PrepDayOfWeekID;
        private int _DeliveryDelayDays;
        private int _DeliveryOrder;

        public AreaPrepDaysTbl()
        {
            this._AreaPrepDaysID = 0;
            this._AreaID = 0;
            this._PrepDayOfWeekID = (byte)0;
            this._DeliveryDelayDays = 0;
            this._DeliveryOrder = 0;
        }

        public int AreaPrepDaysID
        {
            get => this._AreaPrepDaysID;
            set => this._AreaPrepDaysID = value;
        }

        public int AreaID
        {
            get => this._AreaID;
            set => this._AreaID = value;
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

        public List<AreaPrepDaysTbl> GetAll(string SortBy = "")
        {
            var trackerSQL = new TrackerSQLDb();
            string strSQL = "SELECT AreaPrepDaysID, AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM AreaPrepDaysTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            List<AreaPrepDaysTbl> all = new List<AreaPrepDaysTbl>();
            IDataReader dataReader = trackerSQL.ExecuteReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new AreaPrepDaysTbl()
                    {
                        AreaPrepDaysID = dataReader["AreaPrepDaysID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["AreaPrepDaysID"]),
                        AreaID = dataReader["AreaID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["AreaID"]),
                        PrepDayOfWeekID = dataReader["PrepDayOfWeekID"] == DBNull.Value ? (byte)0 : Convert.ToByte(dataReader["PrepDayOfWeekID"]),
                        DeliveryDelayDays = dataReader["DeliveryDelayDays"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryDelayDays"]),
                        DeliveryOrder = dataReader["DeliveryOrder"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryOrder"])
                    });
                dataReader.Close();
            }
            trackerSQL.Dispose();
            return all;
        }

        public List<AreaPrepDaysTbl> GetAllByAreaId(int pAreaID)
        {
            string strSQL = "SELECT AreaPrepDaysID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM AreaPrepDaysTbl WHERE AreaID = ? ORDER BY PrepDayOfWeekID";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pAreaID, DbType.Int32);
            List<AreaPrepDaysTbl> allByAreaId = new List<AreaPrepDaysTbl>();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allByAreaId.Add(new AreaPrepDaysTbl()
                    {
                        AreaID = pAreaID,
                        AreaPrepDaysID = dataReader["AreaPrepDaysID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["AreaPrepDaysID"]),
                        PrepDayOfWeekID = dataReader["PrepDayOfWeekID"] == DBNull.Value ? (byte)0 : Convert.ToByte(dataReader["PrepDayOfWeekID"]),
                        DeliveryDelayDays = dataReader["DeliveryDelayDays"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryDelayDays"]),
                        DeliveryOrder = dataReader["DeliveryOrder"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryOrder"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return allByAreaId;
        }

        public string InsertAreaPrepDay(AreaPrepDaysTbl objAreaPrepDaysTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)objAreaPrepDaysTbl.AreaID, DbType.Int32);
            trackerDb.AddParams((object)objAreaPrepDaysTbl.PrepDayOfWeekID, DbType.Byte);
            trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryDelayDays, DbType.Int32);
            trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryOrder, DbType.Int32);
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO AreaPrepDaysTbl (AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) VALUES (  ?   ,      ?         ,        ?         ,      ?)");
            trackerDb.Close();
            return str;
        }

        public string UpdateAreaPrepDay(AreaPrepDaysTbl objAreaPrepDaysTbl)
        {
            return this.UpdateAreaPrepDay(objAreaPrepDaysTbl, objAreaPrepDaysTbl.AreaPrepDaysID);
        }

        public string UpdateAreaPrepDay(AreaPrepDaysTbl objAreaPrepDaysTbl, int origAreaPrepDaysID)
        {
            string str = string.Empty;
            if (origAreaPrepDaysID > 0)
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddParams((object)objAreaPrepDaysTbl.AreaID, DbType.Int32);
                trackerDb.AddParams((object)objAreaPrepDaysTbl.PrepDayOfWeekID, DbType.Byte);
                trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryDelayDays, DbType.Int32);
                trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryOrder, DbType.Int32);
                trackerDb.AddWhereParams((object)origAreaPrepDaysID, DbType.Int32);
                str = trackerDb.ExecuteNonQuerySQL("UPDATE AreaPrepDaysTbl SET AreaID = ? , PrepDayOfWeekID = ?, DeliveryDelayDays = ?, DeliveryOrder = ? WHERE (AreaPrepDaysID = ?)");
                trackerDb.Close();
            }
            return str;
        }

        public string DeleteByAreaPrepDayID(int pAreaPrepDayID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pAreaPrepDayID, DbType.Int32, "@AreaPrepDayID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM AreaPrepDaysTbl WHERE (AreaPrepDaysID = ?)");
            trackerDb.Close();
            return str;
        }
    }
}
