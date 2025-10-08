// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderDataControl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class OrderDataControl
    {
        public const string CONST_ORDERUPDATEHEADER_SQL = "UPDATE OrdersTbl SET CustomerID = ?, OrderDate= ?, RoastDate= ?, ToBeDeliveredBy= ?, RequiredByDate = ?, Confirmed= ?, Done= ?, InvoiceDone = ?, PurchaseOrder = ?, Notes = ?";
        private const string CONST_ORDERUPDATEITEMS_SQL = "UPDATE OrdersTbl SET ItemTypeID = ?, QuantityOrdered = ?, PackagingID = ? WHERE (OrderId = ?)";
        private const string CONST_ORDERUPDATEALL_SQL = "UPDATE OrdersTbl SET CustomerID = ?, OrderDate= ?, RoastDate= ?, RequiredByDate= ?, ToBeDeliveredBy= ?, Confirmed= ?, Done= ?, InvoiceDone = ?, PurchaseOrder = ?, Notes = ?, ItemTypeID = ?, QuantityOrdered = ?, PackagingID = ? WHERE (OrderId = ?)";

        public bool UpdateOrderHeader(OrderHeaderData pOrderHeader, List<string> pOrders)
        {
            string str = "UPDATE OrdersTbl SET CustomerID = ?, OrderDate= ?, RoastDate= ?, ToBeDeliveredBy= ?, RequiredByDate = ?, Confirmed= ?, Done= ?, InvoiceDone = ?, PurchaseOrder = ?, Notes = ? WHERE ";
            for (int index = 0; index < pOrders.Count - 1; ++index)
                str = $"{str} OrderID = {pOrders[index]} OR";
            string strSQL = $"{str} OrderID = {pOrders[pOrders.Count - 1]}";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pOrderHeader.CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams((object)pOrderHeader.OrderDate, DbType.Date, "@OrderDate");
            trackerDb.AddParams((object)pOrderHeader.RoastDate, DbType.Date, "@RoastDate");
            trackerDb.AddParams((object)pOrderHeader.ToBeDeliveredBy, DbType.Int64, "@ToBeDeliveredBy");
            trackerDb.AddParams((object)pOrderHeader.RequiredByDate, DbType.Date, "@RequiredByDate");
            trackerDb.AddParams((object)pOrderHeader.Confirmed, DbType.Boolean, "@Confirmed");
            trackerDb.AddParams((object)pOrderHeader.Done, DbType.Boolean, "@Done");
            trackerDb.AddParams((object)pOrderHeader.InvoiceDone, DbType.Boolean, "@InvoiceDone");
            trackerDb.AddParams((object)pOrderHeader.PurchaseOrder, DbType.String, "@PurchaseOrder");
            trackerDb.AddParams((object)pOrderHeader.Notes, DbType.String, "@Notes");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL(strSQL));
            trackerDb.Close();
            return flag;
        }
    }
}