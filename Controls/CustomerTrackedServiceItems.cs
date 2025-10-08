// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CustomerTrackedServiceItems
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
    public class CustomerTrackedServiceItems
    {
        private const string CONST_SQL_SELECT = "SELECT CustomerTrackedServiceItemsID, CustomerTypeID, ServiceTypeID, Notes FROM CustomerTrackedServiceItemsTbl";
        private const string CONST_SQL_SELECT_FORCUSTOMERTYPE = "SELECT CustomerTrackedServiceItemsID,  ServiceTypeID, Notes FROM CustomerTrackedServiceItemsTbl WHERE CustomerTypeID = ?";

        public List<CustomerTrackedServiceItems.CustomerTrackedServiceItemsData> GetAll(string SortBy)
        {
            List<CustomerTrackedServiceItems.CustomerTrackedServiceItemsData> all = new List<CustomerTrackedServiceItems.CustomerTrackedServiceItemsData>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT CustomerTrackedServiceItemsID, CustomerTypeID, ServiceTypeID, Notes FROM CustomerTrackedServiceItemsTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new CustomerTrackedServiceItems.CustomerTrackedServiceItemsData()
                    {
                        CustomerTrackedServiceItemsID = dataReader["CustomerTrackedServiceItemsID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTrackedServiceItemsID"]),
                        CustomerTypeID = dataReader["CustomerTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTypeID"]),
                        ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public List<CustomerTrackedServiceItems.CustomerTrackedServiceItemsData> GetAllByCustomerTypeID(
          int pCustomerTypeID)
        {
            List<CustomerTrackedServiceItems.CustomerTrackedServiceItemsData> byCustomerTypeId = new List<CustomerTrackedServiceItems.CustomerTrackedServiceItemsData>();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerTypeID, DbType.Int32, "@CustomerTypeID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT CustomerTrackedServiceItemsID,  ServiceTypeID, Notes FROM CustomerTrackedServiceItemsTbl WHERE CustomerTypeID = ?");
            if (dataReader != null)
            {
                while (dataReader.Read())
                    byCustomerTypeId.Add(new CustomerTrackedServiceItems.CustomerTrackedServiceItemsData()
                    {
                        CustomerTrackedServiceItemsID = dataReader["CustomerTrackedServiceItemsID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTrackedServiceItemsID"]),
                        CustomerTypeID = pCustomerTypeID,
                        ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return byCustomerTypeId;
        }

        public class CustomerTrackedServiceItemsData
        {
            private int _CustomerTrackedServiceItemsID;
            private int _CustomerTypeID;
            private int _ServiceTypeID;
            private string _Notes;

            public CustomerTrackedServiceItemsData()
            {
                this._CustomerTrackedServiceItemsID = 0;
                this._CustomerTypeID = 0;
                this._ServiceTypeID = 0;
                this._Notes = string.Empty;
            }

            public int CustomerTrackedServiceItemsID
            {
                get => this._CustomerTrackedServiceItemsID;
                set => this._CustomerTrackedServiceItemsID = value;
            }

            public int CustomerTypeID
            {
                get => this._CustomerTypeID;
                set => this._CustomerTypeID = value;
            }

            public int ServiceTypeID
            {
                get => this._ServiceTypeID;
                set => this._ServiceTypeID = value;
            }

            public string Notes
            {
                get => this._Notes;
                set => this._Notes = value;
            }
        }
    }
}