// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.LogTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class LogTbl
    {
        private const string CONST_SQL_SELECT = "SELECT LogID, DateAdded, UserID, SectionID, TranactionTypeID, CustomerID, Details, Notes FROM LogTbl";
        private const string CONST_SQL_INSERT = "INSERT INTO LogTbl (DateAdded, UserID, SectionID, TranactionTypeID, CustomerID, Details, Notes) VALUES (?,?,?,?,?,?,?)";
        private int _LogID;
        private DateTime _DateAdded;
        private int _UserID;
        private int _SectionID;
        private int _TranactionTypeID;
        private long _CustomerID;
        private string _Details;
        private string _Notes;

        public LogTbl()
        {
            this._LogID = 0;
            this._DateAdded = DateTime.MinValue;
            this._UserID = 0;
            this._SectionID = 0;
            this._TranactionTypeID = 0;
            this._CustomerID = 0;
            this._Details = string.Empty;
            this._Notes = string.Empty;
        }

        public int LogID
        {
            get => this._LogID;
            set => this._LogID = value;
        }

        public DateTime DateAdded
        {
            get => this._DateAdded;
            set => this._DateAdded = value;
        }

        public int UserID
        {
            get => this._UserID;
            set => this._UserID = value;
        }

        public int SectionID
        {
            get => this._SectionID;
            set => this._SectionID = value;
        }

        public int TranactionTypeID
        {
            get => this._TranactionTypeID;
            set => this._TranactionTypeID = value;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public string Details
        {
            get => this._Details;
            set => this._Details = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value == null ? string.Empty : value;
        }

        public List<LogTbl> GetAll(string SortBy)
        {
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT LogID, DateAdded, UserID, SectionID, TranactionTypeID, CustomerID, Details, Notes FROM LogTbl" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY DateAdded");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            List<LogTbl> all = new List<LogTbl>();
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new LogTbl()
                    {
                        LogID = dataReader["LogID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LogID"]),
                        DateAdded = dataReader["DateAdded"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["DateAdded"]).Date,
                        UserID = dataReader["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["UserID"]),
                        SectionID = dataReader["SectionID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SectionID"]),
                        TranactionTypeID = dataReader["TranactionTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["TranactionTypeID"]),
                        CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]),
                        Details = dataReader["Details"] == DBNull.Value ? string.Empty : dataReader["Details"].ToString(),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public bool InsertLogItem(LogTbl objLog)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)TimeZoneUtils.Now(), DbType.DateTime);
            trackerDb.AddParams((object)objLog.UserID, DbType.Int32);
            trackerDb.AddParams((object)objLog.SectionID, DbType.Int32);
            trackerDb.AddParams((object)objLog.TranactionTypeID, DbType.Int32);
            trackerDb.AddParams((object)objLog.CustomerID, DbType.Int64);
            trackerDb.AddParams((object)objLog.Details);
            trackerDb.AddParams((object)objLog.Notes);
            bool flag = string.IsNullOrWhiteSpace(trackerDb.ExecuteNonQuerySQL("INSERT INTO LogTbl (DateAdded, UserID, SectionID, TranactionTypeID, CustomerID, Details, Notes) VALUES (?,?,?,?,?,?,?)"));
            trackerDb.Close();
            return flag;
        }

        public bool InsertLogItem(
          string pSecurityUserName,
          int pSectionID,
          int pTransactionTypeID,
          long pCustomerID,
          string pDetails,
          string pNotes)
        {
            int pDataValue = new PersonsTbl().PersonsIDoFSecurityUsers(pSecurityUserName);
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)TimeZoneUtils.Now(), DbType.Date);
            trackerDb.AddParams((object)pDataValue, DbType.Int32);
            trackerDb.AddParams((object)pSectionID, DbType.Int32);
            trackerDb.AddParams((object)pTransactionTypeID, DbType.Int32);
            trackerDb.AddParams((object)pCustomerID, DbType.Int64);
            trackerDb.AddParams((object)pDetails);
            trackerDb.AddParams((object)pNotes);
            bool flag = string.IsNullOrWhiteSpace(trackerDb.ExecuteNonQuerySQL("INSERT INTO LogTbl (DateAdded, UserID, SectionID, TranactionTypeID, CustomerID, Details, Notes) VALUES (?,?,?,?,?,?,?)"));
            trackerDb.Close();
            return flag;
        }

        public void AddToWhatsChanged(
          string pItemName,
          string pOrig,
          string pNew,
          ref string pWhatChanged)
        {
            if (pOrig.Equals(pNew))
                return;
            if (!string.IsNullOrWhiteSpace(pWhatChanged))
                pWhatChanged += " - ";
            ref string local = ref pWhatChanged;
            local = $"{local}{pItemName} changed from: {pOrig} to {pNew}";
        }
    }
}