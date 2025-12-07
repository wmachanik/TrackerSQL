// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.OrderDetailDAL
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
    public class OrderDetailDAL
    {
        public List<OrderDetailData> LoadOrderDetailData(
          long CustomerID,
          DateTime DeliveryDate,
          string Notes,
          int MaximumRows,
          int StartRowIndex)
        {
            List<OrderDetailData> orderDetailDataList = new List<OrderDetailData>();
            TrackerDb trackerDb = new TrackerDb();
            string str = "SELECT [ItemTypeID], [QuantityOrdered], [PackagingID], [OrderID] FROM [OrdersTbl] WHERE ";
            string strSQL;
            if (CustomerID == SystemConstants.CustomerConstants.SundryCustomerID)
            {
                strSQL = str + $"([CustomerID] = {SystemConstants.CustomerConstants.SundryCustomerIDStr})  AND ([RequiredByDate] = ?) AND ([Notes] = ?)";
                trackerDb.AddWhereParams((object)DeliveryDate, DbType.Date, "@RequiredByDate");
                trackerDb.AddWhereParams((object)Notes, DbType.String, "@Notes");
            }
            else
            {
                strSQL = str + "([CustomerID] = ?) AND ([RequiredByDate] = ?)";
                trackerDb.AddWhereParams((object)CustomerID, DbType.Int64, "@CustomerID");
                trackerDb.AddWhereParams((object)DeliveryDate, DbType.Date, "@RequiredByDate");
            }
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    orderDetailDataList.Add(new OrderDetailData()
                    {
                        ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : (int)dataReader["ItemTypeID"],
                        PackagingID = dataReader["PackagingID"] == DBNull.Value ? 0 : (int)dataReader["PackagingID"],
                        OrderID = dataReader["OrderId"] == DBNull.Value ? 0 : (int)dataReader["OrderId"],
                        QuantityOrdered = dataReader["QuantityOrdered"] == DBNull.Value ? 1.0 : Math.Round(Convert.ToDouble(dataReader["QuantityOrdered"]), SystemConstants.DatabaseConstants.NumDecimalPoints)
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return orderDetailDataList;
        }

        public bool UpdateOrderDetails(
          long OrderID,
          long CustomerID,
          int ItemTypeID,
          DateTime DeliveryDate,
          double QuantityOrdered,
          int PackagingID)
        {
            string strSQL = "UPDATE OrdersTbl SET ItemTypeID = ?, QuantityOrdered = ?, PackagingID = ? WHERE (OrderId = ?)";
            TrackerDb trackerDb = new TrackerDb();
            ItemTypeID = new TrackerTools().ChangeItemIfGroupToNextItemInGroup(CustomerID, ItemTypeID, DeliveryDate);
            trackerDb.AddParams((object)ItemTypeID, DbType.Int32, "@ItemTypeID");
            trackerDb.AddParams((object)Math.Round(QuantityOrdered, SystemConstants.DatabaseConstants.NumDecimalPoints), DbType.Double, "@QuantityOrdered");
            trackerDb.AddParams((object)PackagingID, DbType.Int32, "@PackagingID");
            trackerDb.AddWhereParams((object)OrderID, DbType.Int64, "@OrderID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL(strSQL));
            trackerDb.Close();
            return flag;
        }

        public bool InsertOrderDetails(
          long CustomerID,
          DateTime OrderDate,
          DateTime RoastDate,
          int ToBeDeliveredBy,
          DateTime RequiredByDate,
          bool Confirmed,
          bool Done,
          string Notes,
          double QuantityOrdered,
          int PackagingID,
          int ItemTypeID)
        {
            string strSQL = "INSERT INTO OrdersTbl (CustomerID, OrderDate, RoastDate, RequiredByDate, ToBeDeliveredBy, Confirmed, Done, Notes,  ItemTypeID, QuantityOrdered, PackagingID) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            ItemTypeID = new TrackerTools().ChangeItemIfGroupToNextItemInGroup(CustomerID, ItemTypeID, RequiredByDate);
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams((object)OrderDate, DbType.Date, "@OrderDate");
            trackerDb.AddParams((object)RoastDate, DbType.Date, "@RoastDate");
            trackerDb.AddParams((object)RequiredByDate, DbType.Date, "@RequiredByDate");
            trackerDb.AddParams((object)ToBeDeliveredBy, DbType.Int32, "@ToBeDeliveredBy");
            trackerDb.AddParams((object)Confirmed, DbType.Boolean, "@Confirmed");
            trackerDb.AddParams((object)Done, DbType.Boolean, "@Done");
            trackerDb.AddParams((object)Notes, DbType.String, "@Notes");
            trackerDb.AddParams((object)ItemTypeID, DbType.Int32, "@ItemTypeID");
            trackerDb.AddParams((object)Math.Round(QuantityOrdered, SystemConstants.DatabaseConstants.NumDecimalPoints), DbType.Double, "@QuantityOrdered");
            trackerDb.AddParams((object)PackagingID, DbType.Int32, "@PackagingID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL(strSQL));
            trackerDb.Close();
            return flag;
        }

        public bool DeleteOrderDetails(string OrderID)
        {
            string strSQL = "DELETE FROM OrdersTbl WHERE (OrderID = ?)";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)OrderID, DbType.String, "@OrderID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL(strSQL));
            trackerDb.Close();
            return flag;
        }
    }
}