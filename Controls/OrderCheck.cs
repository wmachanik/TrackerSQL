// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderCheck
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
    public class OrderCheck
    {
        public const string CONST_SELECT_ORDERITEMSWITHSAMESERVICETYPEEXISTS = "SELECT  OrdersTbl.OrderID, OrdersTbl.CustomerID, ItemTypeTbl.ItemTypeID,OrdersTbl.RequiredByDate  FROM (OrdersTbl INNER JOIN ItemTypeTbl ON OrdersTbl.ItemTypeID = ItemTypeTbl.ItemTypeID)  WHERE (OrdersTbl.CustomerID = ?) AND (OrdersTbl.Done = false) AND ((OrdersTbl.RequiredByDate > ?) AND (OrdersTbl.RequiredByDate < ?)) AND  (ItemTypeTbl.ServiceTypeId = (SELECT ServiceTypesTbl.ServiceTypeId  FROM (ServiceTypesTbl INNER JOIN ItemTypeTbl ItemTypeTblChkd ON ServiceTypesTbl.ServiceTypeId = ItemTypeTblChkd.ServiceTypeId)  WHERE (ItemTypeTblChkd.ItemTypeID = ?))) ";

        public List<OrderCheckData> GetSimilarItemInOrders(
          long pCustomerID,
          int pItemTypeID,
          DateTime pStartDate,
          DateTime pEndDate)
        {
            List<OrderCheckData> similarItemInOrders = (List<OrderCheckData>)null;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddWhereParams((object)pStartDate, DbType.Date, "@RequiredStartDate");
            trackerDb.AddWhereParams((object)pEndDate, DbType.Date, "@RequiredEndDate");
            trackerDb.AddWhereParams((object)pItemTypeID, DbType.Int32, "@ItemTypeID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT  OrdersTbl.OrderID, OrdersTbl.CustomerID, ItemTypeTbl.ItemTypeID,OrdersTbl.RequiredByDate  FROM (OrdersTbl INNER JOIN ItemTypeTbl ON OrdersTbl.ItemTypeID = ItemTypeTbl.ItemTypeID)  WHERE (OrdersTbl.CustomerID = ?) AND (OrdersTbl.Done = false) AND ((OrdersTbl.RequiredByDate > ?) AND (OrdersTbl.RequiredByDate < ?)) AND  (ItemTypeTbl.ServiceTypeId = (SELECT ServiceTypesTbl.ServiceTypeId  FROM (ServiceTypesTbl INNER JOIN ItemTypeTbl ItemTypeTblChkd ON ServiceTypesTbl.ServiceTypeId = ItemTypeTblChkd.ServiceTypeId)  WHERE (ItemTypeTblChkd.ItemTypeID = ?))) ");
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    OrderCheckData orderCheckData = new OrderCheckData();
                    orderCheckData.OrderID = dataReader["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["OrderID"]);
                    orderCheckData.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    orderCheckData.ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]);
                    orderCheckData.RequiredByDate = dataReader["RequiredByDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["RequiredByDate"]).Date;
                    if (similarItemInOrders == null)
                        similarItemInOrders = new List<OrderCheckData>();
                    similarItemInOrders.Add(orderCheckData);
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return similarItemInOrders;
        }
    }
}