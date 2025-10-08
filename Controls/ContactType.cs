// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ContactType
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
    public class ContactType
    {
        private const string CONST_SQL_SELECT = "SELECT CustomerID, CompanyName, CustomerTypeID, enabled, PredictionDisabled FROM CustomersTbl";
        private const string CONST_SQL_UPDATE = "UPDATE CustomersTbl SET CustomerTypeID = ?, PredictionDisabled = ? WHERE CustomerID = ?";
        private const string CONST_SQL_UPDATETYPEONLY = "UPDATE CustomersTbl SET CustomerTypeID = ? WHERE CustomerID = ? AND CustomerTypeID = ?";
        private long _CustomerID;
        private string _CompanyName;
        private int _CustomerTypeID;
        private bool _IsEnabled;
        private bool _PredictionDisabled;

        public ContactType()
        {
            this._CustomerID = 0;
            this._CompanyName = string.Empty;
            this._CustomerTypeID = 0;
            this._IsEnabled = false;
            this._PredictionDisabled = false;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public string CompanyName
        {
            get => this._CompanyName;
            set => this._CompanyName = value;
        }

        public int CustomerTypeID
        {
            get => this._CustomerTypeID;
            set => this._CustomerTypeID = value;
        }

        public bool IsEnabled
        {
            get => this._IsEnabled;
            set => this._IsEnabled = value;
        }

        public bool PredictionDisabled
        {
            get => this._PredictionDisabled;
            set => this._PredictionDisabled = value;
        }

        public List<ContactType> GetAllContacts(string SortBy)
        {
            List<ContactType> allContacts = new List<ContactType>();
            string strSQL = "SELECT CustomerID, CompanyName, CustomerTypeID, enabled, PredictionDisabled FROM CustomersTbl";
            TrackerDb trackerDb = new TrackerDb();
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allContacts.Add(new ContactType()
                    {
                        CustomerID = Convert.ToInt32(dataReader["CustomerID"]),
                        CompanyName = dataReader["CompanyName"] == DBNull.Value ? "" : dataReader["CompanyName"].ToString(),
                        CustomerTypeID = dataReader["CustomerTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTypeID"]),
                        IsEnabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"]),
                        PredictionDisabled = dataReader["PredictionDisabled"] != DBNull.Value && Convert.ToBoolean(dataReader["PredictionDisabled"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return allContacts;
        }

        public string UpdateContact(ContactType pContact)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pContact.CustomerTypeID, DbType.Int32);
            trackerDb.AddParams((object)pContact.PredictionDisabled, DbType.Boolean);
            trackerDb.AddWhereParams((object)pContact.CustomerID, DbType.Int64);
            string str = trackerDb.ExecuteNonQuerySQLWithParams("UPDATE CustomersTbl SET CustomerTypeID = ?, PredictionDisabled = ? WHERE CustomerID = ?", trackerDb.Params, trackerDb.WhereParams);
            trackerDb.Close();
            return str;
        }

        public string UpdateContactTypeIfInfoOnly(long pCustomerID, int pCustomerTypeID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pCustomerTypeID, DbType.Int32);
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            trackerDb.AddWhereParams((object)9, DbType.Int32);
            string str = trackerDb.ExecuteNonQuerySQLWithParams("UPDATE CustomersTbl SET CustomerTypeID = ? WHERE CustomerID = ? AND CustomerTypeID = ?", trackerDb.Params, trackerDb.WhereParams);
            trackerDb.Close();
            return str;
        }
    }
}