// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.ActiveDeliveryData
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class ActiveDeliveryData
    {
        //private const string CONST_CONSTRING = "Tracker08ConnectionString";  SystemConstants.DatabaseConstants.ConnectionStringName
        private const string CONST_SQL_SELECT_ACTIVEDELIVERIES = "SELECT DISTINCT OrdersTbl.RequiredByDate, PersonsTbl.Person, PersonsTbl.PersonID  FROM (OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) WHERE (OrdersTbl.Done = false)";
        private const string CONST_SQL_SELECT_DISTINTDELIVERYDATES = "SELECT DISTINCT OrdersTbl.RequiredByDate FROM OrdersTbl WHERE (OrdersTbl.Done = false) ORDER BY RequiredByDate";
        private DateTime _RequiredByDate;
        private string _Person;
        private int _PersonID;

        public ActiveDeliveryData()
        {
            this._RequiredByDate = DateTime.MinValue;
            this._Person = string.Empty;
            this._PersonID = 0;
        }

        public DateTime RequiredByDate
        {
            get => this._RequiredByDate;
            set => this._RequiredByDate = value;
        }

        public string Person
        {
            get => this._Person;
            set => this._Person = value;
        }

        public int PersonID
        {
            get => this._PersonID;
            set => this._PersonID = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<ActiveDeliveryData> GetActiveDeliveryDateWithDeliveryPerson(string SortBy)
        {
            List<ActiveDeliveryData> withDeliveryPerson = new List<ActiveDeliveryData>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT DISTINCT OrdersTbl.RequiredByDate, PersonsTbl.Person, PersonsTbl.PersonID  FROM (OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) WHERE (OrdersTbl.Done = false)";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    withDeliveryPerson.Add(new ActiveDeliveryData()
                    {
                        RequiredByDate = dataReader["RequiredByDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["RequiredByDate"]).Date,
                        Person = dataReader["Person"] == DBNull.Value ? string.Empty : dataReader["Person"].ToString(),
                        PersonID = dataReader["PersonID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PersonID"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return withDeliveryPerson;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public List<ActiveDeliveryData.ActiveDeliveryDate> GetActiveDeliveryDates()
        {
            List<ActiveDeliveryData.ActiveDeliveryDate> activeDeliveryDates = new List<ActiveDeliveryData.ActiveDeliveryDate>();
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT DISTINCT OrdersTbl.RequiredByDate FROM OrdersTbl WHERE (OrdersTbl.Done = false) ORDER BY RequiredByDate");
            if (dataReader != null)
            {
                while (dataReader.Read())
                    activeDeliveryDates.Add(new ActiveDeliveryData.ActiveDeliveryDate()
                    {
                        RequiredByDate = dataReader["RequiredByDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["RequiredByDate"]).Date
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return activeDeliveryDates;
        }

        public class ActiveDeliveryDate
        {
            private DateTime _RequiredByDate;

            public ActiveDeliveryDate() => this._RequiredByDate = DateTime.MinValue;

            public DateTime RequiredByDate
            {
                get => this._RequiredByDate;
                set => this._RequiredByDate = value;
            }
        }
    }
}