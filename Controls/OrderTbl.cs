// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderTbl
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
    public class OrderTbl
    {
        private const string CONST_ORDERSHEADER_SELECT = "SELECT CustomersTbl.CompanyName, OrdersTbl.CustomerID As CustId, OrdersTbl.OrderDate, OrdersTbl.RoastDate,  OrdersTbl.RequiredByDate, PersonsTbl.Abreviation, PersonsTbl.PersonID,  OrdersTbl.Confirmed,  OrdersTbl.Done, OrdersTbl.InvoiceDone, OrdersTbl.PurchaseOrder, OrdersTbl.Notes  FROM ((OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) LEFT OUTER JOIN CustomersTbl ON OrdersTbl.CustomerID = CustomersTbl.CustomerID) WHERE ([CustomerID] = ?) AND ([RoastDate] = ?)";
        private const string CONST_SELECTBYORDERIR = "SELECT CustomerID, OrderDate, RoastDate, ItemTypeID, QuantityOrdered, RequiredByDate, PrepTypeID, PackagingID, ToBeDeliveredBy, Confirmed, Done, Packed, InvoiceDone, PurchaseOrder, Notes FROM OrdersTbl WHERE (OrderID = ?)";
        private const string CONST_UPDATE_INCREQUIRDERBYDATEBY7 = "UPDATE OrdersTbl SET RequiredByDate = DateAdd('d', 7, RequiredByDate) WHERE (OrderID = ?)";
        private const string CONST_ORDERSLINES_SELECT = "SELECT ItemTypeID, QuantityOrdered, PrepTypeID FROM OrdersTbl WHERE ([OrdersTbl.CustomerID] = ?) AND ([RoastDate] = ?)";
        private const string CONST_SELECTLASTITEM = "SELECT Top 1 OrderID FROM OrdersTbl WHERE (CustomerID = ?) AND (OrderDate = ?) AND (ItemTypeID = ?) ORDER BY OrderID DESC";
        private const string CONST_UPDATEORDERDELIVERDATE = "UPDATE OrdersTbl SET RequiredByDate = ? WHERE (OrderID = ?)";
        private const string CONST_UPDATESETDONEBYID = "UPDATE OrdersTbl SET Done = ? WHERE (OrderID = ?)";
        private const string CONST_UPDATESETINVOICED = "UPDATE OrdersTbl SET InvoiceDone = ? WHERE ";

        public List<OrderHeaderData> LoadOrderHeader(long pCustomerID, DateTime pPrepDate)
        {
            List<OrderHeaderData> orderHeaderDataList = new List<OrderHeaderData>();
            string strSQL = "SELECT CustomersTbl.CompanyName, OrdersTbl.CustomerID As CustId, OrdersTbl.OrderDate, OrdersTbl.RoastDate,  OrdersTbl.RequiredByDate, PersonsTbl.Abreviation, PersonsTbl.PersonID,  OrdersTbl.Confirmed,  OrdersTbl.Done, OrdersTbl.InvoiceDone, OrdersTbl.PurchaseOrder, OrdersTbl.Notes  FROM ((OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) LEFT OUTER JOIN CustomersTbl ON OrdersTbl.CustomerID = CustomersTbl.CustomerID) WHERE ([CustomerID] = ?) AND ([RoastDate] = ?)";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddWhereParams((object)pPrepDate, DbType.Date, "@RoastDate");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    orderHeaderDataList.Add(new OrderHeaderData()
                    {
                        CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : (long)dataReader["CustID"],
                        ToBeDeliveredBy = dataReader["PersonsID"] == DBNull.Value ? 0 : (int)dataReader["PersonsID"],
                        OrderDate = dataReader["OrderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["OrderDate"]).Date,
                        RoastDate = dataReader["RoastDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["RoastDate"]).Date,
                        RequiredByDate = dataReader["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["RequiredByDate"]).Date,
                        Confirmed = dataReader["Confirmed"] != DBNull.Value && (bool)dataReader["Confirmed"],
                        Done = dataReader["Done"] != DBNull.Value && (bool)dataReader["Done"],
                        InvoiceDone = dataReader["InvoiceDone"] != DBNull.Value && (bool)dataReader["InvoiceDone"],
                        PurchaseOrder = dataReader["PurchaseOrder"] == DBNull.Value ? "" : (string)dataReader["PurchaseOrder"],
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return orderHeaderDataList;
        }

        public string InsertNewOrderLine(OrderTblData pOrderData)
        {
            string empty = string.Empty;
            string strSQL = "INSERT INTO OrdersTbl (CustomerID, OrderDate, RoastDate, ToBeDeliveredBy, RequiredByDate, Confirmed, Done, Packed,  InvoiceDone, PurchaseOrder, Notes, ItemTypeID, QuantityOrdered, PrepTypeID, PackagingID)  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            TrackerDb trackerDb = new TrackerDb();
            TrackerTools trackerTools = new TrackerTools();
            pOrderData.ItemTypeID = trackerTools.ChangeItemIfGroupToNextItemInGroup(pOrderData.CustomerID, pOrderData.ItemTypeID, pOrderData.RequiredByDate);
            trackerDb.AddParams((object)pOrderData.CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams((object)pOrderData.OrderDate.Date, DbType.Date, "@OrderDate");
            trackerDb.AddParams((object)pOrderData.RoastDate.Date, DbType.Date, "@RoastDate");
            trackerDb.AddParams((object)pOrderData.ToBeDeliveredBy, DbType.Int32, "@ToBeDeliveredBy");
            trackerDb.AddParams((object)pOrderData.RequiredByDate.Date, DbType.Date, "@RequiredByDate");
            trackerDb.AddParams((object)pOrderData.Confirmed, DbType.Boolean, "@Confirmed");
            trackerDb.AddParams((object)pOrderData.Done, DbType.Boolean, "@Done");
            trackerDb.AddParams((object)pOrderData.Packed, DbType.Boolean, "@Packed");
            trackerDb.AddParams((object)pOrderData.InvoiceDone, DbType.Boolean, "@InvoiceDone");
            trackerDb.AddParams((object)pOrderData.PurchaseOrder, DbType.String, "@PurchaseOrder");
            trackerDb.AddParams((object)pOrderData.Notes, DbType.String, "@Notes");
            trackerDb.AddParams((object)pOrderData.ItemTypeID, DbType.Int32, "@ItemTypeID");
            trackerDb.AddParams((object)Math.Round(pOrderData.QuantityOrdered, SystemConstants.DatabaseConstants.NumDecimalPoints), DbType.Double, "@QuantityOrdered");
            trackerDb.AddParams((object)pOrderData.PrepTypeID, DbType.Int32, "@PrepTypeID");
            trackerDb.AddParams((object)pOrderData.PackagingID, DbType.Int32, "@PackagingID");
            return trackerDb.ExecuteNonQuerySQL(strSQL);
        }

        public int GetLastOrderAdded(long pCustomerID, DateTime pOrderDate, int pItemTypeID)
        {
            int lastOrderAdded = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            trackerDb.AddWhereParams((object)pOrderDate, DbType.Date);
            trackerDb.AddWhereParams((object)pItemTypeID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT Top 1 OrderID FROM OrdersTbl WHERE (CustomerID = ?) AND (OrderDate = ?) AND (ItemTypeID = ?) ORDER BY OrderID DESC");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    lastOrderAdded = dataReader["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["OrderID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return lastOrderAdded;
        }

        public OrderTblData GetOrderByID(int pOrderID)
        {
            OrderTblData orderById = (OrderTblData)null;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pOrderID, DbType.Int64);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT CustomerID, OrderDate, RoastDate, ItemTypeID, QuantityOrdered, RequiredByDate, PrepTypeID, PackagingID, ToBeDeliveredBy, Confirmed, Done, Packed, InvoiceDone, PurchaseOrder, Notes FROM OrdersTbl WHERE (OrderID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    orderById = new OrderTblData();
                    orderById.OrderID = pOrderID; 
                    orderById.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    orderById.OrderDate = dataReader["OrderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["OrderDate"]).Date;
                    orderById.RoastDate = dataReader["RoastDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["RoastDate"]).Date;
                    orderById.RequiredByDate = dataReader["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["RequiredByDate"]).Date;
                    orderById.ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]);
                    orderById.QuantityOrdered = dataReader["QuantityOrdered"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["QuantityOrdered"]);
                    orderById.PrepTypeID = dataReader["PrepTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PrepTypeID"]);
                    orderById.PackagingID = dataReader["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PackagingID"]);
                    orderById.ToBeDeliveredBy = dataReader["ToBeDeliveredBy"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ToBeDeliveredBy"]);
                    orderById.Confirmed = dataReader["Confirmed"] != DBNull.Value && Convert.ToBoolean(dataReader["Confirmed"]);
                    orderById.Done = dataReader["Done"] != DBNull.Value && Convert.ToBoolean(dataReader["Done"]);
                    orderById.Packed = dataReader["Packed"] != DBNull.Value && Convert.ToBoolean(dataReader["Packed"]);
                    orderById.InvoiceDone = dataReader["InvoiceDone"] != DBNull.Value && (bool)dataReader["InvoiceDone"];
                    orderById.PurchaseOrder = dataReader["PurchaseOrder"] == DBNull.Value ? "" : (string)dataReader["PurchaseOrder"];
                    orderById.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return orderById;
        }

        public string UpdateIncDeliveryDateBy7(long pOrderID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pOrderID, DbType.Int64);
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE OrdersTbl SET RequiredByDate = DateAdd('d', 7, RequiredByDate) WHERE (OrderID = ?)");
            trackerDb.Close();
            return str;
        }

        public string UpdateOrderDeliveryDate(DateTime pNewDate, long pOrderID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pNewDate, DbType.Date);
            trackerDb.AddWhereParams((object)pOrderID, DbType.Int64);
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE OrdersTbl SET RequiredByDate = ? WHERE (OrderID = ?)");
            trackerDb.Close();
            return str;
        }

        public string UpdateSetDoneByID(bool pDone, long pOrderID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pDone, DbType.Boolean);
            trackerDb.AddWhereParams((object)pOrderID, DbType.Int64);
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE OrdersTbl SET Done = ? WHERE (OrderID = ?)");
            trackerDb.Close();
            return str;
        }

        public string UpdateSetInvoiced(
          bool pMarkInvoiced,
          long CustomerID,
          DateTime DeliveryDate,
          string Notes)
        {
            string empty = string.Empty;
            string str1 = "UPDATE OrdersTbl SET InvoiceDone = ? WHERE ";
            TrackerDb trackerDb = new TrackerDb();
            string strSQL;
            if (CustomerID == 9L)
            {
                strSQL = str1 + "([CustomerID] = 9) AND ([RequiredByDate] = ?) AND ([Notes] = ?)";
                trackerDb.AddWhereParams((object)DeliveryDate, DbType.Date, "@RequiredByDate");
                trackerDb.AddWhereParams((object)Notes, DbType.String, "@Notes");
            }
            else
            {
                strSQL = str1 + "([CustomerID] = ?) AND ([RequiredByDate] = ?)";
                trackerDb.AddWhereParams((object)CustomerID, DbType.Int64, "@CustomerID");
                trackerDb.AddWhereParams((object)DeliveryDate, DbType.Date, "@RequiredByDate");
            }
            trackerDb.AddParams((object)pMarkInvoiced, DbType.Boolean);
            string str2 = trackerDb.ExecuteNonQuerySQL(strSQL);
            trackerDb.Close();
            return str2;
        }

        public string DeleteOrderById(long pOrderID)
        {
            string strSQL = "DELETE FROM OrdersTbl WHERE (OrderId = ?)";
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pOrderID, DbType.Int64, "@OrderID");
            string str = trackerDb.ExecuteNonQuerySQL(strSQL);
            trackerDb.Close();
            return str;
        }

        public string UpdateOrderNotes(long orderId, string notes)
        {
            const string sql = "UPDATE OrdersTbl SET Notes = ? WHERE OrderID = ?";
            using (var db = new TrackerDb())
            {
                db.AddParams(notes, DbType.String);
                db.AddWhereParams(orderId, DbType.Int64);
                return db.ExecuteNonQuerySQL(sql);
            }
        }
    }
}