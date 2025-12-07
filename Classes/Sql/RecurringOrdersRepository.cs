using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class RecurringOrdersRepository
    {
        public RecurringOrder GetById(int id)
        {
            string sql = "SELECT RecurringOrderID, ContactID, Enabled, Notes, DeliveryByID FROM RecurringOrdersTbl WHERE RecurringOrderID = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                {
                    var ro = Map(rdr);
                    ro.Items = GetItemsForRecurring(ro.RecurringOrderID);
                    return ro;
                }
            }
            return null;
        }

        public List<RecurringOrder> GetAll()
        {
            var list = new List<RecurringOrder>();
            string sql = "SELECT RecurringOrderID, ContactID, Enabled, Notes, DeliveryByID FROM RecurringOrdersTbl";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                {
                    var ro = Map(rdr);
                    ro.Items = GetItemsForRecurring(ro.RecurringOrderID);
                    list.Add(ro);
                }
            }
            return list;
        }

        private List<RecurringOrderItem> GetItemsForRecurring(int recurringId)
        {
            var list = new List<RecurringOrderItem>();
            string sql = "SELECT RecurringOrderItemID, RecurringOrderID, RecurringTypeID, Value, ItemRequiredID, QtyRequired FROM RecurringOrderItemsTbl WHERE RecurringOrderID = @RecurringOrderID";
            var p = new List<DBParameter> { new DBParameter { DataValue = recurringId, DataDbType = DbType.Int32, ParamName = "@RecurringOrderID" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                while (rdr.Read()) list.Add(new RecurringOrderItem
                {
                    RecurringOrderItemID = rdr["RecurringOrderItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["RecurringOrderItemID"]),
                    RecurringOrderID = rdr["RecurringOrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["RecurringOrderID"]),
                    RecurringTypeID = rdr["RecurringTypeID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["RecurringTypeID"]),
                    ItemRequiredID = rdr["ItemRequiredID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["ItemRequiredID"]),
                    Value = rdr["Value"] == DBNull.Value ? (byte?)null : Convert.ToByte(rdr["Value"]),
                    QtyRequired = rdr["QtyRequired"] == DBNull.Value ? (double?)null : Convert.ToDouble(rdr["QtyRequired"])
                });
            }
            return list;
        }

        private RecurringOrder Map(IDataReader r)
        {
            return new RecurringOrder
            {
                RecurringOrderID = r["RecurringOrderID"] == DBNull.Value ? 0 : Convert.ToInt32(r["RecurringOrderID"]),
                ContactID = r["ContactID"] == DBNull.Value ? 0 : Convert.ToInt32(r["ContactID"]),
                Enabled = r["Enabled"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["Enabled"]),
                Notes = r["Notes"] == DBNull.Value ? string.Empty : r["Notes"].ToString(),
                DeliveryByID = r["DeliveryByID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["DeliveryByID"]) 
            };
        }
    }
}
