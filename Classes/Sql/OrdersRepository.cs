using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class OrdersRepository
    {
        public Order GetById(int id)
        {
            string sql = "SELECT OrderID, ContactID, OrderDate, PrepDate, RequiredByDate, ToBeDeliveredByID, Confirmed, Done, Packed, Notes, PurchaseOrder, InvoiceDone FROM OrdersTbl WHERE OrderID = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                {
                    var o = Map(rdr);
                    // load lines
                    o.Lines = GetLinesForOrder(o.OrderID);
                    return o;
                }
            }
            return null;
        }

        public List<Order> GetAll()
        {
            var list = new List<Order>();
            string sql = "SELECT OrderID, ContactID, OrderDate, PrepDate, RequiredByDate, ToBeDeliveredByID, Confirmed, Done, Packed, Notes, PurchaseOrder, InvoiceDone FROM OrdersTbl";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                {
                    var o = Map(rdr);
                    o.Lines = GetLinesForOrder(o.OrderID);
                    list.Add(o);
                }
            }
            return list;
        }

        private List<OrderLine> GetLinesForOrder(int orderId)
        {
            var lines = new List<OrderLine>();
            string sql = "SELECT OrderLineID, OrderID, ItemID, QtyOrdered, PrepTypeID, PackagingID FROM OrderLinesTbl WHERE OrderID = @OrderID";
            var p = new List<DBParameter> { new DBParameter { DataValue = orderId, DataDbType = DbType.Int32, ParamName = "@OrderID" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                while (rdr.Read()) lines.Add(new OrderLine
                {
                    OrderLineID = rdr["OrderLineID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderLineID"]),
                    OrderID = rdr["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderID"]),
                    ItemID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                    QtyOrdered = rdr["QtyOrdered"] == DBNull.Value ? (double?)null : Convert.ToDouble(rdr["QtyOrdered"]),
                    PrepTypeID = rdr["PrepTypeID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["PrepTypeID"]),
                    PackagingID = rdr["PackagingID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["PackagingID"])
                });
            }
            return lines;
        }

        private Order Map(IDataReader r)
        {
            return new Order
            {
                OrderID = r["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(r["OrderID"]),
                ContactID = r["ContactID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ContactID"]),
                OrderDate = r["OrderDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["OrderDate"]),
                PrepDate = r["PrepDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["PrepDate"]),
                RequiredByDate = r["RequiredByDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["RequiredByDate"]),
                ToBeDeliveredByID = r["ToBeDeliveredByID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ToBeDeliveredByID"]),
                Confirmed = r["Confirmed"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["Confirmed"]),
                Done = r["Done"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["Done"]),
                Packed = r["Packed"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["Packed"]),
                Notes = r["Notes"] == DBNull.Value ? string.Empty : r["Notes"].ToString(),
                PurchaseOrder = r["PurchaseOrder"] == DBNull.Value ? string.Empty : r["PurchaseOrder"].ToString(),
                InvoiceDone = r["InvoiceDone"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["InvoiceDone"])
            };
        }
    }
}
