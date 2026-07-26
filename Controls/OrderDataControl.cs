//------------------------------------------------------------------------------
// TrackerSQL v3.x — OrderDataControl
// Data access / control type: OrderDataControl.
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class OrderDataControl
    {
        public const string CONST_ORDERUPDATEHEADER_SQL = "UPDATE OrdersTbl SET CustomerID = @CustomerID, OrderDate= @OrderDate, PrepDate= @PrepDate, ToBeDeliveredBy= @ToBeDeliveredBy, RequiredByDate = @RequiredByDate, Confirmed= @Confirmed, Done= @Done, InvoiceDone = @InvoiceDone, PurchaseOrder = @PurchaseOrder, Notes = @Notes";
        private const string CONST_ORDERUPDATEITEMS_SQL = "UPDATE OrdersTbl SET ItemTypeID = @ItemTypeID, QuantityOrdered = @QuantityOrdered, PackagingID = @PackagingID WHERE (OrderId = @OrderId)";
        private const string CONST_ORDERUPDATEALL_SQL = "UPDATE OrdersTbl SET CustomerID = @CustomerID, OrderDate= @OrderDate, PrepDate= @PrepDate, RequiredByDate= @RequiredByDate, ToBeDeliveredBy= @ToBeDeliveredBy, Confirmed= @Confirmed, Done= @Done, InvoiceDone = @InvoiceDone, PurchaseOrder = @PurchaseOrder, Notes = @Notes, ItemTypeID = @ItemTypeID, QuantityOrdered = @QuantityOrdered, PackagingID = @PackagingID WHERE (OrderId = @OrderId)";

        public bool UpdateOrderHeader(OrderHeaderData pOrderHeader, List<string> pOrders)
        {
            string str = "UPDATE OrdersTbl SET CustomerID = @CustomerID, OrderDate= @OrderDate, PrepDate= @PrepDate, ToBeDeliveredBy= @ToBeDeliveredBy, RequiredByDate = @RequiredByDate, Confirmed= @Confirmed, Done= @Done, InvoiceDone = @InvoiceDone, PurchaseOrder = @PurchaseOrder, Notes = @Notes WHERE ";
            for (int index = 0; index < pOrders.Count - 1; ++index)
                str = $"{str} OrderID = {pOrders[index]} OR";
            string strSQL = $"{str} OrderID = {pOrders[pOrders.Count - 1]}";

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pOrderHeader.CustomerID, DataDbType = DbType.Int64, ParamName = "@CustomerID" },
                    new DBParameter { DataValue = pOrderHeader.OrderDate, DataDbType = DbType.Date, ParamName = "@OrderDate" },
                    new DBParameter { DataValue = pOrderHeader.PrepDate, DataDbType = DbType.Date, ParamName = "@PrepDate" },
                    new DBParameter { DataValue = pOrderHeader.ToBeDeliveredBy, DataDbType = DbType.Int64, ParamName = "@ToBeDeliveredBy" },
                    new DBParameter { DataValue = pOrderHeader.RequiredByDate, DataDbType = DbType.Date, ParamName = "@RequiredByDate" },
                    new DBParameter { DataValue = pOrderHeader.Confirmed, DataDbType = DbType.Boolean, ParamName = "@Confirmed" },
                    new DBParameter { DataValue = pOrderHeader.Done, DataDbType = DbType.Boolean, ParamName = "@Done" },
                    new DBParameter { DataValue = pOrderHeader.InvoiceDone, DataDbType = DbType.Boolean, ParamName = "@InvoiceDone" },
                    new DBParameter { DataValue = pOrderHeader.PurchaseOrder, DataDbType = DbType.String, ParamName = "@PurchaseOrder" },
                    new DBParameter { DataValue = pOrderHeader.Notes, DataDbType = DbType.String, ParamName = "@Notes" }
                };

                int result = db.ExecuteNonQuery(strSQL, parameters);
                bool flag = result >= 0;

                if (!flag)
                {
                    AppLogger.WriteLog("OrderDataControl", 
                        $"Failed to update order header for orders: {string.Join(",", pOrders)}");
                }

                return flag;
            }
        }
    }
}
