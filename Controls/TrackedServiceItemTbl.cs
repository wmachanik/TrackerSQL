// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.TrackedServiceItemTbl
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
    public class TrackedServiceItemTbl
    {
        private const string CONST_SQL_SELECT = "SELECT TrackerServiceItemID, ServiceTypeID, TypicalAvePerItem, UsageDateFieldName, UsageAveFieldName, ThisItemSetsDailyAverage, Notes FROM TrackedServiceItemTbl";
        private int _TrackerServiceItemID;
        private int _ServiceTypeID;
        private double _TypicalAvePerItem;
        private string _UsageDateFieldName;
        private string _UsageAveFieldName;
        private bool _ThisItemSetsDailyAverage;
        private string _Notes;

        public TrackedServiceItemTbl()
        {
            this._TrackerServiceItemID = 0;
            this._ServiceTypeID = 0;
            this._TypicalAvePerItem = 0.0;
            this._UsageDateFieldName = string.Empty;
            this._UsageAveFieldName = string.Empty;
            this._ThisItemSetsDailyAverage = false;
            this._Notes = string.Empty;
        }

        public int TrackerServiceItemID
        {
            get => this._TrackerServiceItemID;
            set => this._TrackerServiceItemID = value;
        }

        public int ServiceTypeID
        {
            get => this._ServiceTypeID;
            set => this._ServiceTypeID = value;
        }

        public double TypicalAvePerItem
        {
            get => this._TypicalAvePerItem;
            set => this._TypicalAvePerItem = value;
        }

        public string UsageDateFieldName
        {
            get => this._UsageDateFieldName;
            set => this._UsageDateFieldName = value;
        }

        public string UsageAveFieldName
        {
            get => this._UsageAveFieldName;
            set => this._UsageAveFieldName = value;
        }

        public bool ThisItemSetsDailyAverage
        {
            get => this._ThisItemSetsDailyAverage;
            set => this._ThisItemSetsDailyAverage = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        public List<TrackedServiceItemTbl> GetAll(string SortBy)
        {
            List<TrackedServiceItemTbl> all = new List<TrackedServiceItemTbl>();
            string strSQL = "SELECT TrackerServiceItemID, ServiceTypeID, TypicalAvePerItem, UsageDateFieldName, UsageAveFieldName, ThisItemSetsDailyAverage, Notes FROM TrackedServiceItemTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new TrackedServiceItemTbl()
                    {
                        TrackerServiceItemID = dataReader["TrackerServiceItemID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["TrackerServiceItemID"]),
                        ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]),
                        TypicalAvePerItem = dataReader["TypicalAvePerItem"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["TypicalAvePerItem"]),
                        UsageDateFieldName = dataReader["UsageDateFieldName"] == DBNull.Value ? string.Empty : dataReader["UsageDateFieldName"].ToString(),
                        UsageAveFieldName = dataReader["UsageAveFieldName"] == DBNull.Value ? string.Empty : dataReader["UsageAveFieldName"].ToString(),
                        ThisItemSetsDailyAverage = dataReader["ThisItemSetsDailyAverage"] != DBNull.Value && Convert.ToBoolean(dataReader["ThisItemSetsDailyAverage"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }
    }
}