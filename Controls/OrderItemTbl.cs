// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderItemTbl
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
    public class OrderItemTbl
    {
        private const string CONST_ORDERSLINES_SELECT = "SELECT ItemTypeID, QuantityOrdered, PrepTypeID FROM OrdersTbl WHERE ";
        private const string CONST_ORDERSSUMMARY_SELECT = "SELECT CustomerID, OrderDate, RoastDate, RequiredByDate, ToBeDeliveredBy, Confirmed, Done, InvoiceDone, PurchaseOrder, Notes  FROM OrdersTbl WHERE ";

        public List<OrderHeaderData> LoadOrderSummary(
          long CustomerID,
          DateTime DeliveryDate,
          string Notes,
          int MaximumRows,
          int StartRowIndex)
        {
            List<OrderHeaderData> orderHeaderDataList = new List<OrderHeaderData>();
            string str1 = "SELECT CustomerID, OrderDate, RoastDate, RequiredByDate, ToBeDeliveredBy, Confirmed, Done, InvoiceDone, PurchaseOrder, Notes  FROM OrdersTbl WHERE ";
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
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    if (orderHeaderDataList.Count == 0)
                    {
                        orderHeaderDataList.Add(new OrderHeaderData()
                        {
                            CustomerID = Convert.ToInt32(dataReader[nameof(CustomerID)]),
                            OrderDate = (DateTime)dataReader["OrderDate"],
                            RoastDate = (DateTime)dataReader["RoastDate"],
                            RequiredByDate = (DateTime)dataReader["RequiredByDate"],
                            ToBeDeliveredBy = dataReader["ToBeDeliveredBy"] == DBNull.Value ? 3 : (int)dataReader["ToBeDeliveredBy"],
                            Confirmed = dataReader["Confirmed"] != DBNull.Value && (bool)dataReader["Confirmed"],
                            Done = dataReader["Done"] != DBNull.Value && (bool)dataReader["Done"],
                            InvoiceDone = dataReader["InvoiceDone"] != DBNull.Value && (bool)dataReader["InvoiceDone"],
                            PurchaseOrder = dataReader["PurchaseOrder"] == DBNull.Value ? "" : (string)dataReader["PurchaseOrder"],
                            Notes = dataReader[nameof(Notes)] == DBNull.Value ? "" : (string)dataReader[nameof(Notes)]
                        });
                    }
                    else
                    {
                        long int64 = Convert.ToInt32(dataReader[nameof(CustomerID)].ToString());
                        if (orderHeaderDataList[0].CustomerID.Equals(int64))
                        {
                            string str2 = dataReader[nameof(Notes)] == DBNull.Value ? "" : (string)dataReader[nameof(Notes)];
                            if (!string.IsNullOrEmpty(str2) && !orderHeaderDataList[0].Notes.Contains(str2))
                            {
                                OrderHeaderData orderHeaderData = orderHeaderDataList[0];
                                orderHeaderData.Notes = $"{orderHeaderData.Notes}|{str2}";
                            }
                        }
                    }
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return orderHeaderDataList;
        }

        public bool UpdateOrderDetails(
          long CustomerID,
          DateTime OrderDate,
          DateTime RoastDate,
          int ToBeDeliveredBy,
          DateTime RequiredByDate,
          bool Confirmed,
          bool Done,
          bool InvoiceDone,
          string PurchaseOrder,
          string Notes,
          long OriginalCustomerID,
          DateTime OriginalDeliveryDate,
          string OriginalNotes)
        {
            string str = "UPDATE OrdersTbl SET CustomerID = ?, OrderDate= ?, RoastDate= ?, ToBeDeliveredBy= ?, RequiredByDate = ?, Confirmed= ?, Done= ?, InvoiceDone = ?, PurchaseOrder = ?, Notes = ? WHERE ";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams((object)OrderDate, DbType.Date, "@OrderDate");
            trackerDb.AddParams((object)RoastDate, DbType.Date, "@RoastDate");
            trackerDb.AddParams((object)ToBeDeliveredBy, DbType.Int32, "@ToBeDeliveredBy");
            trackerDb.AddParams((object)RequiredByDate, DbType.Date, "@RequiredByDate");
            trackerDb.AddParams((object)Confirmed, DbType.Boolean, "@Confirmed");
            trackerDb.AddParams((object)Done, DbType.Boolean, "@Done");
            trackerDb.AddParams((object)InvoiceDone, DbType.Boolean, "@InvoiceDone");
            trackerDb.AddParams((object)PurchaseOrder, DbType.String, "@PurchaseOrder");
            trackerDb.AddParams((object)Notes, DbType.String, "@Notes");
            string strSQL;
            if (OriginalCustomerID == 9L)
            {
                strSQL = str + "([CustomerID] = 9) AND ([RequiredByDate] = ?) AND ([Notes] = ?)";
                trackerDb.AddWhereParams((object)OriginalDeliveryDate, DbType.Date, "@RequiredByDate");
                trackerDb.AddWhereParams((object)OriginalNotes, DbType.String, "@Notes");
            }
            else
            {
                strSQL = $"{str}([CustomerID] = {OriginalCustomerID.ToString()}) AND ([RequiredByDate] = ?)";
                trackerDb.AddWhereParams((object)OriginalDeliveryDate, DbType.Date, "@RequiredByDate");
            }
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL(strSQL));
            trackerDb.Close();
            return flag;
        }
    }
}