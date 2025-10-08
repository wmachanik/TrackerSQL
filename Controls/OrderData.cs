// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderData
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
    public class OrderData
    {
        private const string CONST_SELECTDISTINCTORDERS = "SELECT DISTINCT OrdersTbl.OrderID, CustomersTbl.CompanyName, OrdersTbl.CustomerID As CustomerID, OrdersTbl.OrderDate,  OrdersTbl.RoastDate, OrdersTbl.RequiredByDate,OrdersTbl.ToBeDeliveredBy, PersonsTbl.Person, OrdersTbl.Confirmed,  OrdersTbl.Done, OrdersTbl.Notes, OrdersTbl.ItemTypeID, OrdersTbl.QuantityOrdered  FROM ((OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) LEFT OUTER JOIN CustomersTbl ON OrdersTbl.CustomerID = CustomersTbl.CustomerID) WHERE (OrdersTbl.Done = ?)";
        private const string CONST_UPDATEORDER = "UPDATE OrdersTbl SET CustomerID = ?, OrderDate = ?, RoastDate = ?, RequiredByDate = ?, ToBeDeliveredBy = ?,  ItemTypeID = ?, QuantityOrdered = ?, Confirmed = ?, Done = ?, Notes = ?  WHERE OrderID = ?";
        private const string CONST_UPDATEORDERDATES = "UPDATE OrdersTbl SET RoastDate = ? WHERE CustomerID = ? AND OrderDate = ?";
        private string _CompanyName;
        private int _OrderID;
        private long _CustomerID;
        private DateTime _OrderDate;
        private DateTime _RoastDate;
        private DateTime _RequiredByDate;
        private int _ToBeDeliveredBy;
        private int _ItemTypeID;
        private double _QuantityOrdered;
        private string _Person;
        private bool _Confirmed;
        private bool _Done;
        private string _Notes;

        public OrderData()
        {
            this._CompanyName = string.Empty;
            this._CustomerID = 0;
            this._OrderDate = this._RoastDate = this._RequiredByDate = DateTime.MinValue;
            this._Person = string.Empty;
            this._Confirmed = this._Done = false;
            this._ItemTypeID = 0;
            this._QuantityOrdered = 0.0;
            this._Notes = string.Empty;
        }

        public int OrderID
        {
            get => this._OrderID;
            set => this._OrderID = value;
        }

        public string CompanyName
        {
            get => this._CompanyName;
            set => this._CompanyName = value;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public DateTime OrderDate
        {
            get => this._OrderDate;
            set => this._OrderDate = value;
        }

        public DateTime RoastDate
        {
            get => this._RoastDate;
            set => this._RoastDate = value;
        }

        public DateTime RequiredByDate
        {
            get => this._RequiredByDate;
            set => this._RequiredByDate = value;
        }

        public int ToBeDeliveredBy
        {
            get => this._ToBeDeliveredBy;
            set => this._ToBeDeliveredBy = value;
        }

        public int ItemTypeID
        {
            get => this._ItemTypeID;
            set => this._ItemTypeID = value;
        }

        public double QuantityOrdered
        {
            get => this._QuantityOrdered;
            set => this._QuantityOrdered = value;
        }

        public string Person
        {
            get => this._Person;
            set => this._Person = value;
        }

        public bool Confirmed
        {
            get => this._Confirmed;
            set => this._Confirmed = value;
        }

        public bool Done
        {
            get => this._Done;
            set => this._Done = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        private void VerifySortColumns(string sortColumns)
        {
            if (sortColumns.ToLowerInvariant().EndsWith(" desc"))
                sortColumns = sortColumns.Substring(0, sortColumns.Length - 5);
            string str1 = sortColumns;
            char[] chArray = new char[1] { ',' };
            foreach (string str2 in str1.Split(chArray))
            {
                switch (str2.Trim().ToLowerInvariant())
                {
                    case "":
                        continue;
                    default:
                        throw new ArgumentException("SortColumns contains an invalid column name.");
                }
            }
        }

        public void Initialize()
        {
        }

        public List<OrderData> GetDistinctOrders(bool pOrderDone, string pSearchFor, string pSearchValue)
        {
            List<OrderData> distinctOrders = new List<OrderData>();
            string strSQL = "SELECT DISTINCT OrdersTbl.OrderID, CustomersTbl.CompanyName, OrdersTbl.CustomerID As CustomerID, OrdersTbl.OrderDate,  OrdersTbl.RoastDate, OrdersTbl.RequiredByDate,OrdersTbl.ToBeDeliveredBy, PersonsTbl.Person, OrdersTbl.Confirmed,  OrdersTbl.Done, OrdersTbl.Notes, OrdersTbl.ItemTypeID, OrdersTbl.QuantityOrdered  FROM ((OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) LEFT OUTER JOIN CustomersTbl ON OrdersTbl.CustomerID = CustomersTbl.CustomerID) WHERE (OrdersTbl.Done = ?)";
            if (pSearchFor != "none" && !string.IsNullOrEmpty(pSearchFor))
            {
                switch (pSearchFor)
                {
                    case "Company":
                        strSQL = $"{strSQL} AND CustomersTbl.CompanyName LIKE '%{pSearchValue}%'";
                        break;
                    case "PrepDate":
                        strSQL = $"{strSQL} AND OrdersTbl.RoastDate= #{pSearchValue}#";
                        break;
                }
            }
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pOrderDone, DbType.Boolean, "@Done");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    distinctOrders.Add(new OrderData()
                    {
                        OrderID = dataReader["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["OrderID"]),
                        CompanyName = dataReader["CompanyName"] == DBNull.Value ? string.Empty : dataReader["CompanyName"].ToString(),
                        CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]),
                        OrderDate = dataReader["OrderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["OrderDate"]).Date,
                        RoastDate = dataReader["RoastDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["RoastDate"]).Date,
                        RequiredByDate = dataReader["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["RequiredByDate"]).Date,
                        ToBeDeliveredBy = dataReader["ToBeDeliveredBy"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ToBeDeliveredBy"].ToString()),
                        ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"].ToString()),
                        QuantityOrdered = dataReader["QuantityOrdered"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["QuantityOrdered"].ToString()),
                        Person = dataReader["Person"] == DBNull.Value ? string.Empty : dataReader["Person"].ToString(),
                        Confirmed = dataReader["Confirmed"] != DBNull.Value && Convert.ToBoolean(dataReader["Confirmed"]),
                        Done = dataReader["Done"] != DBNull.Value && Convert.ToBoolean(dataReader["Done"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return distinctOrders;
        }

        public bool UpdateOrderData(OrderData NewOrderData)
        {
            return this.UpdateOrderData(NewOrderData, NewOrderData.OrderID);
        }

        public bool UpdateOrderData(OrderData NewOrderData, long orig_OrderID)
        {
            if (!new TrackerTools().ChangeItemIfGroupToNextItemInGroup(NewOrderData.CustomerID, NewOrderData.ItemTypeID, NewOrderData.RequiredByDate).Equals(NewOrderData.ItemTypeID))
                NewOrderData.Notes += $"{(NewOrderData.Notes != null || !NewOrderData.Notes.Equals(string.Empty) ? (object)" " : (object)"")}group item added";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)NewOrderData.CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams((object)NewOrderData.OrderDate, DbType.Date, "@OrderDate");
            trackerDb.AddParams((object)NewOrderData.RoastDate, DbType.Date, "@RoastDate");
            trackerDb.AddParams((object)NewOrderData.RequiredByDate, DbType.Date, "@RequiredByDate");
            trackerDb.AddParams((object)NewOrderData.ToBeDeliveredBy, DbType.Int32, "@ToBeDeliveredBy");
            trackerDb.AddParams((object)NewOrderData.ItemTypeID, DbType.Int32, "@ItemTypeID");
            trackerDb.AddParams((object)Math.Round(NewOrderData.QuantityOrdered, SystemConstants.DatabaseConstants.NumDecimalPoints), DbType.Double, "@QuantityOrdered");
            trackerDb.AddParams((object)NewOrderData.Confirmed, DbType.Boolean, "@Confirmed");
            trackerDb.AddParams((object)NewOrderData.Done, DbType.Boolean, "@Done");
            trackerDb.AddParams(NewOrderData.Notes == null ? (object)string.Empty : (object)NewOrderData.Notes, DbType.String, "@Notes");
            trackerDb.AddWhereParams((object)orig_OrderID, DbType.Int64, "@OrderID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("UPDATE OrdersTbl SET CustomerID = ?, OrderDate = ?, RoastDate = ?, RequiredByDate = ?, ToBeDeliveredBy = ?,  ItemTypeID = ?, QuantityOrdered = ?, Confirmed = ?, Done = ?, Notes = ?  WHERE OrderID = ?"));
            trackerDb.Close();
            return flag;
        }
    }
}