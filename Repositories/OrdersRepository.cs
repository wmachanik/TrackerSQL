using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
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

        /// <summary>
        /// Gets items required by preparation (roasting) date with optional date filtering
        /// </summary>
        /// <param name="fromDate">Optional start date filter</param>
        /// <param name="toDate">Optional end date filter</param>
        /// <returns>List of items grouped by PrepDate showing quantities required</returns>
        public List<ItemsRequiredSummary> GetItemsRequiredByPrepDate(DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<ItemsRequiredSummary>();
            var parameters = new List<DBParameter>();
            
            string sql = @"
                SELECT OrdersTbl.PrepDate, ItemsTbl.ItemDesc, ROUND(SUM(OrderLinesTbl.QtyOrdered), 2) AS Qty
                FROM ((OrdersTbl 
                    INNER JOIN OrderLinesTbl ON OrdersTbl.OrderID = OrderLinesTbl.OrderID)
                    INNER JOIN ItemsTbl ON OrderLinesTbl.ItemID = ItemsTbl.ItemID)
                WHERE OrdersTbl.Done = 0";

            // Add date filters if provided
            if (fromDate.HasValue)
            {
                sql += " AND OrdersTbl.PrepDate >= @FromDate";
                parameters.Add(new DBParameter 
                { 
                    DataValue = fromDate.Value, 
                    DataDbType = DbType.DateTime, 
                    ParamName = "@FromDate" 
                });
            }

            if (toDate.HasValue)
            {
                sql += " AND OrdersTbl.PrepDate <= @ToDate";
                parameters.Add(new DBParameter 
                { 
                    DataValue = toDate.Value, 
                    DataDbType = DbType.DateTime, 
                    ParamName = "@ToDate" 
                });
            }

            sql += " GROUP BY OrdersTbl.PrepDate, ItemsTbl.ItemDesc ORDER BY OrdersTbl.PrepDate ASC, ItemsTbl.ItemDesc ASC";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters.Count > 0 ? parameters : null))
            {
                while (rdr.Read())
                {
                    list.Add(new ItemsRequiredSummary
                    {
                        PrepDate = rdr["PrepDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["PrepDate"]),
                        ItemDesc = rdr["ItemDesc"] == DBNull.Value ? string.Empty : rdr["ItemDesc"].ToString(),
                        Qty = rdr["Qty"] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr["Qty"])
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// Gets items required by delivery date with optional date filtering, grouped by delivery person
        /// </summary>
        /// <param name="fromDate">Optional start date filter</param>
        /// <param name="toDate">Optional end date filter</param>
        /// <returns>List of items grouped by RequiredByDate and delivery person showing quantities</returns>
        public List<ItemsRequiredSummary> GetItemsRequiredByDeliveryDate(DateTime? fromDate, DateTime? toDate)
        {
            System.Diagnostics.Debug.WriteLine($"GetItemsRequiredByDeliveryDate called: fromDate={fromDate:yyyy-MM-dd HH:mm:ss}, toDate={toDate:yyyy-MM-dd HH:mm:ss}");
            
            var list = new List<ItemsRequiredSummary>();
            var parameters = new List<DBParameter>();

            string sql = @"
                SELECT OrdersTbl.RequiredByDate, ISNULL(PeopleTbl.Abbreviation, 'Unassigned') AS Abbreviation,
                       ItemsTbl.ItemDesc, ROUND(SUM(OrderLinesTbl.QtyOrdered), 2) AS Qty
                FROM ((OrdersTbl 
                    INNER JOIN OrderLinesTbl ON OrdersTbl.OrderID = OrderLinesTbl.OrderID)
                    INNER JOIN ItemsTbl ON OrderLinesTbl.ItemID = ItemsTbl.ItemID)
                    LEFT OUTER JOIN PeopleTbl ON OrdersTbl.ToBeDeliveredByID = PeopleTbl.PersonID
                WHERE OrdersTbl.Done = 0 AND OrdersTbl.RequiredByDate IS NOT NULL";

            // Add date filters if provided
            if (fromDate.HasValue)
            {
                sql += " AND OrdersTbl.RequiredByDate >= @FromDate";
                parameters.Add(new DBParameter 
                { 
                    DataValue = fromDate.Value, 
                    DataDbType = DbType.DateTime, 
                    ParamName = "@FromDate" 
                });
            }

            if (toDate.HasValue)
            {
                sql += " AND OrdersTbl.RequiredByDate <= @ToDate";
                parameters.Add(new DBParameter 
                { 
                    DataValue = toDate.Value, 
                    DataDbType = DbType.DateTime, 
                    ParamName = "@ToDate" 
                });
            }

            sql += " GROUP BY OrdersTbl.RequiredByDate, ISNULL(PeopleTbl.Abbreviation, 'Unassigned'), ItemsTbl.ItemDesc ORDER BY OrdersTbl.RequiredByDate ASC, Abbreviation ASC, ItemsTbl.ItemDesc ASC";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters.Count > 0 ? parameters : null))
            {
                while (rdr.Read())
                {
                    list.Add(new ItemsRequiredSummary
                    {
                        RequiredByDate = rdr["RequiredByDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["RequiredByDate"]),
                        Abbreviation = rdr["Abbreviation"] == DBNull.Value ? "Unassigned" : rdr["Abbreviation"].ToString(),
                        ItemDesc = rdr["ItemDesc"] == DBNull.Value ? string.Empty : rdr["ItemDesc"].ToString(),
                        Qty = rdr["Qty"] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr["Qty"])
                    });
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
        public OrderHeaderData GetOrderHeaderByOrderId(int orderId)
        {
            var order = GetById(orderId);
            if (order == null) return null;

            return new OrderHeaderData
            {
                OrderID = order.OrderID,
                CustomerID = order.ContactID ?? 0,
                OrderDate = order.OrderDate ?? DateTime.MinValue,
                PrepDate = order.PrepDate ?? DateTime.MinValue,
                RequiredByDate = order.RequiredByDate ?? DateTime.MinValue,
                ToBeDeliveredBy = order.ToBeDeliveredByID ?? 0,
                Confirmed = order.Confirmed ?? false,
                Done = order.Done ?? false,
                InvoiceDone = order.InvoiceDone ?? false,
                PurchaseOrder = order.PurchaseOrder ?? string.Empty,
                Notes = order.Notes ?? string.Empty
            };
        }

        public int GetOrderLineCount(int orderId)
        {
            if (orderId <= 0)
                return 0;

            const string sql = "SELECT COUNT(*) FROM OrderLinesTbl WHERE OrderID = @OrderID";
            return ExecuteScalar<int>(sql, OrderIdParam(orderId));
        }

        public List<OrderDetailData> LoadOrderDetailDataByOrderId(int orderId)
        {
            var list = new List<OrderDetailData>();
            const string sql = @"
                SELECT OrderLineID, ItemID, QtyOrdered, PackagingID, OrderID
                FROM OrderLinesTbl
                WHERE OrderID = @OrderID
                ORDER BY OrderLineID";

            using (var rdr = ExecReader(sql, OrderIdParam(orderId)))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new OrderDetailData
                    {
                        OrderLineID = rdr["OrderLineID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderLineID"]),
                        ItemTypeID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                        PackagingID = rdr["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PackagingID"]),
                        OrderID = rdr["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderID"]),
                        QuantityOrdered = rdr["QtyOrdered"] == DBNull.Value ? 1.0 :
                            Math.Round(Convert.ToDouble(rdr["QtyOrdered"]), SystemConstants.DatabaseConstants.NumDecimalPoints)
                    });
                }
            }

            return list;
        }

        public int? FindOrderIdByRequiredByDate(long contactId, DateTime requiredByDate, string notes)
        {
            string sql;
            var parameters = new List<DBParameter>();

            if (contactId == SystemConstants.CustomerConstants.SundryCustomerID)
            {
                sql = @"
                    SELECT TOP 1 OrderID FROM OrdersTbl
                    WHERE ContactID = @ContactID AND RequiredByDate = @RequiredByDate AND Notes = @Notes
                    ORDER BY OrderID DESC";
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = SystemConstants.CustomerConstants.SundryCustomerID, DataDbType = DbType.Int64 });
                parameters.Add(new DBParameter { ParamName = "@RequiredByDate", DataValue = requiredByDate.Date, DataDbType = DbType.Date });
                parameters.Add(new DBParameter { ParamName = "@Notes", DataValue = notes ?? string.Empty, DataDbType = DbType.String });
            }
            else
            {
                sql = @"
                    SELECT TOP 1 OrderID FROM OrdersTbl
                    WHERE ContactID = @ContactID AND RequiredByDate = @RequiredByDate
                    ORDER BY OrderID DESC";
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 });
                parameters.Add(new DBParameter { ParamName = "@RequiredByDate", DataValue = requiredByDate.Date, DataDbType = DbType.Date });
            }

            int orderId = ExecuteScalar<int>(sql, parameters);
            return orderId > 0 ? orderId : (int?)null;
        }

        public int? FindOrderIdByPrepDate(long contactId, DateTime prepDate)
        {
            const string sql = @"
                SELECT TOP 1 OrderID FROM OrdersTbl
                WHERE ContactID = @ContactID AND PrepDate = @PrepDate
                ORDER BY OrderID DESC";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@PrepDate", DataValue = prepDate.Date, DataDbType = DbType.Date }
            };

            int orderId = ExecuteScalar<int>(sql, parameters);
            return orderId > 0 ? orderId : (int?)null;
        }

        public bool UpdateOrderHeaderByOrderId(int orderId, OrderHeaderData header)
        {
            return UpdateOrderHeader(header, new List<string> { orderId.ToString() });
        }

        public bool UpdateSetInvoicedByOrderId(bool markInvoiced, int orderId)
        {
            const string sql = "UPDATE OrdersTbl SET InvoiceDone = @InvoiceDone WHERE OrderID = @OrderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@InvoiceDone", DataValue = markInvoiced, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int32 }
            };
            return ExecNonQuery(sql, parameters) >= 0;
        }

        public int AddLineToExistingOrder(int orderId, OrderTblData orderData)
        {
            return InsertOrderLine(orderId, orderData.ItemTypeID, orderData.QuantityOrdered,
                orderData.PrepTypeID, orderData.PackagingID);
        }

        public bool DeleteOrderLineById(int orderLineId)
        {
            const string sql = "DELETE FROM OrderLinesTbl WHERE OrderLineID = @OrderLineID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderLineID", DataValue = orderLineId, DataDbType = DbType.Int32 }
            };
            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool UpdateOrderHeader(OrderHeaderData header, List<string> orderIds)
        {
            var ids = ParseOrderIds(orderIds);
            if (ids.Count == 0) return false;

            var inParams = new List<string>();
            var parameters = BuildHeaderUpdateParameters(header);
            for (int i = 0; i < ids.Count; i++)
            {
                string paramName = "@OrderId" + i;
                inParams.Add(paramName);
                parameters.Add(new DBParameter { ParamName = paramName, DataValue = ids[i], DataDbType = DbType.Int32 });
            }

            string sql = $@"
                UPDATE OrdersTbl SET
                    ContactID = @ContactID, OrderDate = @OrderDate, PrepDate = @PrepDate,
                    ToBeDeliveredByID = @ToBeDeliveredByID, RequiredByDate = @RequiredByDate,
                    Confirmed = @Confirmed, Done = @Done, InvoiceDone = @InvoiceDone,
                    PurchaseOrder = @PurchaseOrder, Notes = @Notes
                WHERE OrderID IN ({string.Join(", ", inParams)})";

            int result = ExecNonQuery(sql, parameters);
            if (result < 0)
            {
                AppLogger.WriteLog("OrdersRepository",
                    $"Failed to update order header for orders: {string.Join(",", ids)}");
                return false;
            }

            if (result == 0)
            {
                AppLogger.WriteLog("OrdersRepository",
                    $"Order header update affected 0 rows for orders: {string.Join(",", ids)}");
            }

            return result > 0;
        }

        public List<OrderHeaderData> LoadOrderHeader(long contactId, DateTime prepDate)
        {
            var list = new List<OrderHeaderData>();
            const string sql = @"
                SELECT o.ContactID, o.OrderDate, o.PrepDate, o.RequiredByDate, p.PersonID,
                       o.Confirmed, o.Done, o.InvoiceDone, o.PurchaseOrder, o.Notes
                FROM OrdersTbl o
                LEFT OUTER JOIN PeopleTbl p ON o.ToBeDeliveredByID = p.PersonID
                WHERE o.ContactID = @ContactID AND o.PrepDate = @PrepDate";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@PrepDate", DataValue = prepDate.Date, DataDbType = DbType.Date }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new OrderHeaderData
                    {
                        CustomerID = rdr["ContactID"] == DBNull.Value ? 0 : Convert.ToInt64(rdr["ContactID"]),
                        ToBeDeliveredBy = rdr["PersonID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PersonID"]),
                        OrderDate = rdr["OrderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["OrderDate"]).Date,
                        PrepDate = rdr["PrepDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["PrepDate"]).Date,
                        RequiredByDate = rdr["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["RequiredByDate"]).Date,
                        Confirmed = rdr["Confirmed"] != DBNull.Value && Convert.ToBoolean(rdr["Confirmed"]),
                        Done = rdr["Done"] != DBNull.Value && Convert.ToBoolean(rdr["Done"]),
                        InvoiceDone = rdr["InvoiceDone"] != DBNull.Value && Convert.ToBoolean(rdr["InvoiceDone"]),
                        PurchaseOrder = rdr["PurchaseOrder"] == DBNull.Value ? string.Empty : rdr["PurchaseOrder"].ToString(),
                        Notes = rdr["Notes"] == DBNull.Value ? string.Empty : rdr["Notes"].ToString()
                    });
                }
            }

            return list;
        }

        public OrderTblData GetOrderTblDataById(int orderId)
        {
            const string sql = @"
                SELECT o.OrderID, o.ContactID, o.OrderDate, o.PrepDate, o.RequiredByDate,
                       o.ToBeDeliveredByID, o.Confirmed, o.Done, o.Packed, o.InvoiceDone,
                       o.PurchaseOrder, o.Notes,
                       ol.OrderLineID, ol.ItemID, ol.QtyOrdered, ol.PrepTypeID, ol.PackagingID
                FROM OrdersTbl o
                LEFT JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                WHERE o.OrderID = @OrderID
                ORDER BY ol.OrderLineID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return MapOrderTblData(rdr);
                }
            }

            return null;
        }

        public int InsertNewOrderLine(OrderTblData orderData)
        {
            int orderId = InsertOrderHeader(orderData);
            if (orderId <= 0) return 0;

            int lineId = InsertOrderLine(orderId, orderData.ItemTypeID, orderData.QuantityOrdered,
                orderData.PrepTypeID, orderData.PackagingID);
            return lineId > 0 ? orderId : 0;
        }

        public int InsertOrderHeader(OrderTblData orderData)
        {
            const string sql = @"
                INSERT INTO OrdersTbl
                (ContactID, OrderDate, PrepDate, ToBeDeliveredByID, RequiredByDate, Confirmed, Done, Packed,
                 InvoiceDone, PurchaseOrder, Notes)
                VALUES
                (@ContactID, @OrderDate, @PrepDate, @ToBeDeliveredByID, @RequiredByDate, @Confirmed, @Done, @Packed,
                 @InvoiceDone, @PurchaseOrder, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = orderData.CustomerID, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@OrderDate", DataValue = orderData.OrderDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@PrepDate", DataValue = orderData.PrepDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ToBeDeliveredByID", DataValue = orderData.ToBeDeliveredBy, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@RequiredByDate", DataValue = orderData.RequiredByDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@Confirmed", DataValue = orderData.Confirmed, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Done", DataValue = orderData.Done, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Packed", DataValue = orderData.Packed, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@InvoiceDone", DataValue = orderData.InvoiceDone, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@PurchaseOrder", DataValue = orderData.PurchaseOrder ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Notes", DataValue = orderData.Notes ?? string.Empty, DataDbType = DbType.String }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public int InsertOrderLine(int orderId, int itemId, double quantityOrdered, int prepTypeId, int packagingId)
        {
            const string sql = @"
                INSERT INTO OrderLinesTbl (OrderID, ItemID, QtyOrdered, PrepTypeID, PackagingID)
                VALUES (@OrderID, @ItemID, @QtyOrdered, @PrepTypeID, @PackagingID);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemID", DataValue = itemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@QtyOrdered", DataValue = Math.Round(quantityOrdered, SystemConstants.DatabaseConstants.NumDecimalPoints), DataDbType = DbType.Double },
                new DBParameter { ParamName = "@PrepTypeID", DataValue = prepTypeId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PackagingID", DataValue = packagingId, DataDbType = DbType.Int32 }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public int GetLastOrderAdded(long contactId, DateTime orderDate, int itemId)
        {
            const string sql = @"
                SELECT TOP 1 o.OrderID
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                WHERE o.ContactID = @ContactID AND o.OrderDate = @OrderDate AND ol.ItemID = @ItemID
                ORDER BY o.OrderID DESC";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@OrderDate", DataValue = orderDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ItemID", DataValue = itemId, DataDbType = DbType.Int32 }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public bool UpdateIncDeliveryDateBy7(long orderId)
        {
            const string sql = "UPDATE OrdersTbl SET RequiredByDate = DATEADD(day, 7, RequiredByDate) WHERE OrderID = @OrderID";
            return ExecNonQuery(sql, OrderIdParam(orderId)) >= 0;
        }

        public bool UpdateOrderDeliveryDate(DateTime newDate, long orderId)
        {
            const string sql = "UPDATE OrdersTbl SET RequiredByDate = @RequiredByDate WHERE OrderID = @OrderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RequiredByDate", DataValue = newDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int64 }
            };
            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool UpdateSetDoneById(bool done, long orderId)
        {
            const string sql = "UPDATE OrdersTbl SET Done = @Done WHERE OrderID = @OrderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Done", DataValue = done, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int64 }
            };
            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool UpdateSetInvoiced(bool markInvoiced, long contactId, DateTime deliveryDate, string notes)
        {
            string sql;
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@InvoiceDone", DataValue = markInvoiced, DataDbType = DbType.Boolean }
            };

            if (contactId == SystemConstants.CustomerConstants.SundryCustomerID)
            {
                sql = "UPDATE OrdersTbl SET InvoiceDone = @InvoiceDone WHERE ContactID = @ContactID AND RequiredByDate = @RequiredByDate AND Notes = @Notes";
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = SystemConstants.CustomerConstants.SundryCustomerID, DataDbType = DbType.Int64 });
                parameters.Add(new DBParameter { ParamName = "@RequiredByDate", DataValue = deliveryDate.Date, DataDbType = DbType.Date });
                parameters.Add(new DBParameter { ParamName = "@Notes", DataValue = notes ?? string.Empty, DataDbType = DbType.String });
            }
            else
            {
                sql = "UPDATE OrdersTbl SET InvoiceDone = @InvoiceDone WHERE ContactID = @ContactID AND RequiredByDate = @RequiredByDate";
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 });
                parameters.Add(new DBParameter { ParamName = "@RequiredByDate", DataValue = deliveryDate.Date, DataDbType = DbType.Date });
            }

            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool DeleteOrderById(long orderId)
        {
            DeleteLinesForOrder((int)orderId);
            return ExecNonQuery("DELETE FROM OrdersTbl WHERE OrderID = @OrderID", OrderIdParam(orderId)) >= 0;
        }

        public bool UpdateOrderNotes(long orderId, string notes)
        {
            const string sql = "UPDATE OrdersTbl SET Notes = @Notes WHERE OrderID = @OrderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Notes", DataValue = notes ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int64 }
            };
            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool MarkDoneForTempOrderOriginalIds()
        {
            const string sql = @"
                UPDATE OrdersTbl SET Done = 1
                WHERE EXISTS (
                    SELECT 1 FROM TempOrdersLinesTbl
                    WHERE TempOrdersLinesTbl.OriginalOrderID = OrdersTbl.OrderID)";
            return ExecNonQuery(sql) >= 0;
        }

        public bool MarkDoneForOrderId(int orderId)
        {
            if (orderId <= 0) return false;

            const string sql = "UPDATE OrdersTbl SET Done = 1 WHERE OrderID = @OrderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool MarkDoneForTempOrderHeader(int tempHeaderId)
        {
            if (tempHeaderId <= 0) return false;

            const string sql = @"
                UPDATE OrdersTbl SET Done = 1
                WHERE EXISTS (
                    SELECT 1 FROM TempOrdersLinesTbl
                    WHERE TempOrdersLinesTbl.TOHeaderID = @TOHeaderID
                      AND TempOrdersLinesTbl.OriginalOrderID = OrdersTbl.OrderID)";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = tempHeaderId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) >= 0;
        }

        public List<OrderDetailData> LoadOrderDetailData(long contactId, DateTime deliveryDate, string notes)
        {
            var list = new List<OrderDetailData>();
            string sql;
            var parameters = new List<DBParameter>();

            if (contactId == SystemConstants.CustomerConstants.SundryCustomerID)
            {
                sql = @"
                    SELECT ol.OrderLineID, ol.ItemID, ol.QtyOrdered, ol.PackagingID, ol.OrderID
                    FROM OrderLinesTbl ol
                    INNER JOIN OrdersTbl o ON ol.OrderID = o.OrderID
                    WHERE o.ContactID = @ContactID AND o.RequiredByDate = @RequiredByDate AND o.Notes = @Notes";
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = SystemConstants.CustomerConstants.SundryCustomerID, DataDbType = DbType.Int64 });
                parameters.Add(new DBParameter { ParamName = "@RequiredByDate", DataValue = deliveryDate.Date, DataDbType = DbType.Date });
                parameters.Add(new DBParameter { ParamName = "@Notes", DataValue = notes ?? string.Empty, DataDbType = DbType.String });
            }
            else
            {
                sql = @"
                    SELECT ol.OrderLineID, ol.ItemID, ol.QtyOrdered, ol.PackagingID, ol.OrderID
                    FROM OrderLinesTbl ol
                    INNER JOIN OrdersTbl o ON ol.OrderID = o.OrderID
                    WHERE o.ContactID = @ContactID AND o.RequiredByDate = @RequiredByDate";
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 });
                parameters.Add(new DBParameter { ParamName = "@RequiredByDate", DataValue = deliveryDate.Date, DataDbType = DbType.Date });
            }

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new OrderDetailData
                    {
                        OrderLineID = rdr["OrderLineID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderLineID"]),
                        ItemTypeID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                        PackagingID = rdr["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PackagingID"]),
                        OrderID = rdr["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderID"]),
                        QuantityOrdered = rdr["QtyOrdered"] == DBNull.Value ? 1.0 :
                            Math.Round(Convert.ToDouble(rdr["QtyOrdered"]), SystemConstants.DatabaseConstants.NumDecimalPoints)
                    });
                }
            }

            return list;
        }

        public List<PublicOrderLineView> GetPublicOrderLines(long contactId, DateTime requiredDate, string notes)
        {
            var list = new List<PublicOrderLineView>();
            string sql;
            var parameters = new List<DBParameter>();

            if (contactId == SystemConstants.CustomerConstants.SundryCustomerID)
            {
                sql = @"
                    SELECT o.OrderID, ol.ItemID AS ItemTypeID, ol.QtyOrdered AS QuantityOrdered,
                           ISNULL(ol.PackagingID, 0) AS PackagingID, o.RequiredByDate, o.Notes
                    FROM OrdersTbl o
                    INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                    WHERE o.ContactID = @ContactID AND o.RequiredByDate = @RequiredByDate AND o.Notes = @Notes
                    ORDER BY o.OrderID";
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = SystemConstants.CustomerConstants.SundryCustomerID, DataDbType = DbType.Int64 });
                parameters.Add(new DBParameter { ParamName = "@RequiredByDate", DataValue = requiredDate.Date, DataDbType = DbType.Date });
                parameters.Add(new DBParameter { ParamName = "@Notes", DataValue = notes ?? string.Empty, DataDbType = DbType.String });
            }
            else
            {
                sql = @"
                    SELECT o.OrderID, ol.ItemID AS ItemTypeID, ol.QtyOrdered AS QuantityOrdered,
                           ISNULL(ol.PackagingID, 0) AS PackagingID, o.RequiredByDate, o.Notes
                    FROM OrdersTbl o
                    INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                    WHERE o.ContactID = @ContactID AND o.RequiredByDate = @RequiredByDate
                    ORDER BY o.OrderID";
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 });
                parameters.Add(new DBParameter { ParamName = "@RequiredByDate", DataValue = requiredDate.Date, DataDbType = DbType.Date });
            }

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(MapPublicOrderLine(rdr));
                }
            }

            return list;
        }

        public List<PublicOrderLineView> GetPublicOrderLinesInRange(long contactId, DateTime fromDate, DateTime toDate)
        {
            var list = new List<PublicOrderLineView>();
            const string sql = @"
                SELECT o.OrderID, ol.ItemID AS ItemTypeID, ol.QtyOrdered AS QuantityOrdered,
                       ISNULL(ol.PackagingID, 0) AS PackagingID, o.RequiredByDate, o.Notes
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                WHERE o.ContactID = @ContactID AND o.RequiredByDate BETWEEN @FromDate AND @ToDate
                ORDER BY o.RequiredByDate, o.OrderID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@FromDate", DataValue = fromDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ToDate", DataValue = toDate.Date, DataDbType = DbType.Date }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(MapPublicOrderLine(rdr));
                }
            }

            return list;
        }

        private static PublicOrderLineView MapPublicOrderLine(IDataReader rdr)
        {
            return new PublicOrderLineView
            {
                OrderID = rdr["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderID"]),
                ItemTypeID = rdr["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemTypeID"]),
                QuantityOrdered = rdr["QuantityOrdered"] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr["QuantityOrdered"]),
                PackagingID = rdr["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PackagingID"]),
                RequiredByDate = rdr["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["RequiredByDate"]).Date,
                Notes = rdr["Notes"] == DBNull.Value ? string.Empty : rdr["Notes"].ToString()
            };
        }

        public bool UpdateOrderLine(long orderLineId, int itemId, double quantityOrdered, int packagingId)
        {
            const string sql = @"
                UPDATE OrderLinesTbl
                SET ItemID = @ItemID, QtyOrdered = @QtyOrdered, PackagingID = @PackagingID
                WHERE OrderLineID = @OrderLineID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemID", DataValue = itemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@QtyOrdered", DataValue = Math.Round(quantityOrdered, SystemConstants.DatabaseConstants.NumDecimalPoints), DataDbType = DbType.Double },
                new DBParameter { ParamName = "@PackagingID", DataValue = packagingId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@OrderLineID", DataValue = orderLineId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool UpdateOrderDetails(long orderId, int itemId, double quantityOrdered, int packagingId)
        {
            int? lineId = GetFirstOrderLineId((int)orderId);
            if (!lineId.HasValue) return false;
            return UpdateOrderLine(lineId.Value, itemId, quantityOrdered, packagingId);
        }

        public bool InsertOrderDetails(
            long contactId, DateTime orderDate, DateTime prepDate, int toBeDeliveredById,
            DateTime requiredByDate, bool confirmed, bool done, string notes,
            double quantityOrdered, int packagingId, int itemId, int prepTypeId = 0)
        {
            var header = new OrderTblData
            {
                CustomerID = contactId,
                OrderDate = orderDate,
                PrepDate = prepDate,
                ToBeDeliveredBy = toBeDeliveredById,
                RequiredByDate = requiredByDate,
                Confirmed = confirmed,
                Done = done,
                Notes = notes ?? string.Empty,
                ItemTypeID = itemId,
                QuantityOrdered = quantityOrdered,
                PackagingID = packagingId,
                PrepTypeID = prepTypeId
            };

            return InsertNewOrderLine(header) > 0;
        }

        public bool DeleteOrderDetails(string orderId)
        {
            if (!int.TryParse(orderId, out int id)) return false;
            return DeleteOrderById(id);
        }

        public bool DeleteLinesForOrder(int orderId)
        {
            return ExecNonQuery("DELETE FROM OrderLinesTbl WHERE OrderID = @OrderID", OrderIdParam(orderId)) >= 0;
        }

        private int? GetFirstOrderLineId(int orderId)
        {
            const string sql = "SELECT TOP 1 OrderLineID FROM OrderLinesTbl WHERE OrderID = @OrderID ORDER BY OrderLineID";
            int lineId = ExecuteScalar<int>(sql, OrderIdParam(orderId));
            return lineId > 0 ? lineId : (int?)null;
        }

        private static OrderTblData MapOrderTblData(IDataReader rdr)
        {
            return new OrderTblData
            {
                OrderID = rdr["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderID"]),
                CustomerID = rdr["ContactID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ContactID"]),
                OrderDate = rdr["OrderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["OrderDate"]).Date,
                PrepDate = rdr["PrepDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["PrepDate"]).Date,
                RequiredByDate = rdr["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["RequiredByDate"]).Date,
                ItemTypeID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                QuantityOrdered = rdr["QtyOrdered"] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr["QtyOrdered"]),
                PrepTypeID = rdr["PrepTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PrepTypeID"]),
                PackagingID = rdr["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PackagingID"]),
                ToBeDeliveredBy = rdr["ToBeDeliveredByID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ToBeDeliveredByID"]),
                Confirmed = rdr["Confirmed"] != DBNull.Value && Convert.ToBoolean(rdr["Confirmed"]),
                Done = rdr["Done"] != DBNull.Value && Convert.ToBoolean(rdr["Done"]),
                Packed = rdr["Packed"] != DBNull.Value && Convert.ToBoolean(rdr["Packed"]),
                InvoiceDone = rdr["InvoiceDone"] != DBNull.Value && Convert.ToBoolean(rdr["InvoiceDone"]),
                PurchaseOrder = rdr["PurchaseOrder"] == DBNull.Value ? string.Empty : rdr["PurchaseOrder"].ToString(),
                Notes = rdr["Notes"] == DBNull.Value ? string.Empty : rdr["Notes"].ToString()
            };
        }

        private static List<DBParameter> BuildHeaderUpdateParameters(OrderHeaderData header)
        {
            return new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = header.CustomerID, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@OrderDate", DataValue = header.OrderDate, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@PrepDate", DataValue = header.PrepDate, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ToBeDeliveredByID", DataValue = header.ToBeDeliveredBy, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@RequiredByDate", DataValue = header.RequiredByDate, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@Confirmed", DataValue = header.Confirmed, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Done", DataValue = header.Done, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@InvoiceDone", DataValue = header.InvoiceDone, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@PurchaseOrder", DataValue = header.PurchaseOrder ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Notes", DataValue = header.Notes ?? string.Empty, DataDbType = DbType.String }
            };
        }

        public List<OrderEntryListItem> GetOrderEntryList(bool orderDone, string searchFor, string searchValue)
        {
            var list = new List<OrderEntryListItem>();
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Done", DataValue = orderDone, DataDbType = DbType.Boolean }
            };

            string sql = @"
                SELECT
                    o.OrderID,
                    ISNULL(c.ContactName, '') AS CompanyName,
                    o.ContactID AS CustomerID,
                    o.OrderDate,
                    o.PrepDate,
                    o.RequiredByDate,
                    ISNULL(o.ToBeDeliveredByID, 0) AS ToBeDeliveredBy,
                    ISNULL(p.Abbreviation, '') AS Person,
                    o.Confirmed,
                    o.Done,
                    ISNULL(o.Notes, '') AS Notes,
                    ISNULL(firstLine.ItemID, 0) AS ItemTypeID,
                    ISNULL(firstLine.QtyOrdered, 0) AS QuantityOrdered
                FROM OrdersTbl o
                LEFT JOIN ContactsTbl c ON o.ContactID = c.ContactID
                LEFT JOIN PeopleTbl p ON o.ToBeDeliveredByID = p.PersonID
                OUTER APPLY (
                    SELECT TOP 1 ol.ItemID, ol.QtyOrdered
                    FROM OrderLinesTbl ol
                    WHERE ol.OrderID = o.OrderID
                    ORDER BY ol.OrderLineID
                ) firstLine
                WHERE o.Done = @Done";

            if (!string.IsNullOrEmpty(searchFor) && searchFor != "none")
            {
                if (searchFor == "Company")
                {
                    sql += " AND c.ContactName LIKE @SearchValue";
                    parameters.Add(new DBParameter
                    {
                        ParamName = "@SearchValue",
                        DataValue = "%" + (searchValue ?? string.Empty).Trim() + "%",
                        DataDbType = DbType.String
                    });
                }
                else if (searchFor == "PrepDate" && DateTime.TryParse(searchValue, out DateTime prepDate))
                {
                    sql += " AND o.PrepDate = @PrepDate";
                    parameters.Add(new DBParameter
                    {
                        ParamName = "@PrepDate",
                        DataValue = prepDate.Date,
                        DataDbType = DbType.Date
                    });
                }
            }

            sql += " ORDER BY o.PrepDate DESC, o.OrderID DESC";

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new OrderEntryListItem
                    {
                        OrderID = rdr["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderID"]),
                        CompanyName = rdr["CompanyName"] == DBNull.Value ? string.Empty : rdr["CompanyName"].ToString(),
                        CustomerID = rdr["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt64(rdr["CustomerID"]),
                        OrderDate = rdr["OrderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["OrderDate"]).Date,
                        PrepDate = rdr["PrepDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["PrepDate"]).Date,
                        RequiredByDate = rdr["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["RequiredByDate"]).Date,
                        ToBeDeliveredBy = rdr["ToBeDeliveredBy"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ToBeDeliveredBy"]),
                        Person = rdr["Person"] == DBNull.Value ? string.Empty : rdr["Person"].ToString(),
                        Confirmed = rdr["Confirmed"] != DBNull.Value && Convert.ToBoolean(rdr["Confirmed"]),
                        Done = rdr["Done"] != DBNull.Value && Convert.ToBoolean(rdr["Done"]),
                        Notes = rdr["Notes"] == DBNull.Value ? string.Empty : rdr["Notes"].ToString(),
                        ItemTypeID = rdr["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemTypeID"]),
                        QuantityOrdered = rdr["QuantityOrdered"] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr["QuantityOrdered"])
                    });
                }
            }

            return list;
        }

        public bool UpdateOrderEntry(OrderEntryListItem item, int originalOrderId)
        {
            if (item == null || originalOrderId <= 0)
                return false;

            var header = new OrderHeaderData
            {
                CustomerID = item.CustomerID,
                OrderDate = item.OrderDate,
                PrepDate = item.PrepDate,
                RequiredByDate = item.RequiredByDate,
                ToBeDeliveredBy = item.ToBeDeliveredBy,
                Confirmed = item.Confirmed,
                Done = item.Done,
                Notes = item.Notes ?? string.Empty
            };

            if (!UpdateOrderHeaderByOrderId(originalOrderId, header))
                return false;

            int? lineId = GetFirstOrderLineId(originalOrderId);
            if (lineId.HasValue && item.ItemTypeID > 0)
            {
                return UpdateOrderLine(
                    lineId.Value,
                    item.ItemTypeID,
                    item.QuantityOrdered,
                    0);
            }

            return true;
        }

        private static List<int> ParseOrderIds(List<string> orderIds)
        {
            var ids = new List<int>();
            if (orderIds == null) return ids;
            foreach (var idStr in orderIds)
            {
                if (int.TryParse(idStr, out int id)) ids.Add(id);
            }
            return ids;
        }

        private static List<DBParameter> OrderIdParam(long orderId)
        {
            return new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int64 }
            };
        }

        private int ExecNonQuery(string sql, List<DBParameter> parameters = null)
        {
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }

        private T ExecuteScalar<T>(string sql, List<DBParameter> parameters = null)
        {
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteScalar<T>(sql, parameters);
            }
        }

        private IDataReader ExecReader(string sql, List<DBParameter> parameters = null)
        {
            var db = new TrackerSQLDb();
            return db.ExecuteReader(sql, parameters);
        }
    }
}
