using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class TempOrdersLinesRepository : RepositoryBase<TempOrdersLine>
    {
        private const string SelectColumns =
            "TOLineID, TOHeaderID, ItemID, ItemServiceTypeID, Qty, ItemPackagingID, OriginalOrderID";

        protected override string TableName => "TempOrdersLinesTbl";
        protected override string KeyColumn => "TOLineID";

        protected override string CoreColumns => SelectColumns;

        public bool HasCoffeeInTempOrder(int headerId)
        {
            if (headerId <= 0) return false;

            const string sql = @"
                SELECT TOP 1 ItemServiceTypeID
                FROM TempOrdersLinesTbl
                WHERE TOHeaderID = @TOHeaderID AND ItemServiceTypeID = @CoffeeServiceType";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = headerId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@CoffeeServiceType", DataValue = SystemConstants.ServiceTypeConstants.Coffee, DataDbType = DbType.Int32 }
            };

            int result = ExecuteScalar<int>(sql, parameters);
            return result > 0;
        }

        public bool DeleteByHeaderId(int headerId)
        {
            if (headerId <= 0) return false;

            const string sql = "DELETE FROM TempOrdersLinesTbl WHERE TOHeaderID = @TOHeaderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = headerId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool InsertLine(TempOrdersLine line)
        {
            return Insert(line) > 0;
        }

        public bool DeleteAllRecords()
        {
            return ExecNonQuery("DELETE FROM TempOrdersLinesTbl") >= 0;
        }

        public bool DeleteByOriginalOrderId(long originalOrderId)
        {
            const string sql = "DELETE FROM TempOrdersLinesTbl WHERE OriginalOrderID = @OriginalOrderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OriginalOrderID", DataValue = originalOrderId, DataDbType = DbType.Int64 }
            };

            return ExecNonQuery(sql, parameters) >= 0;
        }

        public int? GetHeaderIdByOriginalOrderId(int orderId)
        {
            if (orderId <= 0) return null;

            const string sql = @"
                SELECT TOP 1 TOHeaderID
                FROM TempOrdersLinesTbl
                WHERE OriginalOrderID = @OriginalOrderID
                ORDER BY TOHeaderID DESC";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OriginalOrderID", DataValue = orderId, DataDbType = DbType.Int32 }
            };

            int headerId = ExecuteScalar<int>(sql, parameters);
            return headerId > 0 ? headerId : (int?)null;
        }

        public List<int> GetOriginalOrderIdsForHeader(int headerId)
        {
            var orderIds = new List<int>();
            if (headerId <= 0) return orderIds;

            const string sql = @"
                SELECT DISTINCT OriginalOrderID
                FROM TempOrdersLinesTbl
                WHERE TOHeaderID = @TOHeaderID AND OriginalOrderID IS NOT NULL";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = headerId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    if (rdr["OriginalOrderID"] != DBNull.Value)
                    {
                        int orderId = Convert.ToInt32(rdr["OriginalOrderID"]);
                        if (orderId > 0)
                            orderIds.Add(orderId);
                    }
                }
            }

            return orderIds;
        }

        public override int Insert(TempOrdersLine entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO TempOrdersLinesTbl
                (TOHeaderID, ItemID, ItemServiceTypeID, Qty, ItemPackagingID, OriginalOrderID)
                VALUES
                (@TOHeaderID, @ItemID, @ItemServiceTypeID, @Qty, @ItemPackagingID, @OriginalOrderID);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return ExecuteScalar<int>(sql, BuildParameters(entity));
        }

        public List<OrderDoneLineView> GetOrderDoneLines(int headerId)
        {
            var list = new List<OrderDoneLineView>();
            if (headerId <= 0) return list;

            const string sql = @"
                SELECT TOLineID, ItemID, Qty, ISNULL(ItemPackagingID, 0) AS PackagingID
                FROM TempOrdersLinesTbl
                WHERE TOHeaderID = @TOHeaderID
                ORDER BY TOLineID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = headerId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new OrderDoneLineView
                    {
                        TOLineID = rdr["TOLineID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TOLineID"]),
                        ItemID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                        Qty = rdr["Qty"] == DBNull.Value ? 0f : Convert.ToSingle(rdr["Qty"]),
                        PackagingID = rdr["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PackagingID"])
                    });
                }
            }

            return list;
        }

        public bool UpdateOrderDoneLine(int toLineId, int itemId, float qty, int packagingId)
        {
            if (toLineId <= 0) return false;

            const string sql = @"
                UPDATE TempOrdersLinesTbl
                SET ItemID = @ItemID, Qty = @Qty, ItemPackagingID = @ItemPackagingID
                WHERE TOLineID = @TOLineID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemID", DataValue = itemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Qty", DataValue = qty, DataDbType = DbType.Single },
                new DBParameter { ParamName = "@ItemPackagingID", DataValue = packagingId > 0 ? (object)packagingId : DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@TOLineID", DataValue = toLineId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool DeleteOrderDoneLine(int toLineId)
        {
            if (toLineId <= 0) return false;

            const string sql = "DELETE FROM TempOrdersLinesTbl WHERE TOLineID = @TOLineID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOLineID", DataValue = toLineId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public List<TempOrderUsageLine> GetUsageLinesForContact(int contactId, int headerId)
        {
            const string sql = @"
                SELECT
                    TempOrdersHeaderTbl.ContactID,
                    TempOrdersLinesTbl.ItemID,
                    TempOrdersLinesTbl.ItemServiceTypeID,
                    TempOrdersLinesTbl.Qty,
                    ISNULL(ItemsTbl.UnitsPerQty, 1) AS UnitsPerQty,
                    ISNULL(TempOrdersLinesTbl.ItemPackagingID, 0) AS ItemPackagingID
                FROM TempOrdersHeaderTbl
                INNER JOIN TempOrdersLinesTbl ON TempOrdersHeaderTbl.TOHeaderID = TempOrdersLinesTbl.TOHeaderID
                LEFT OUTER JOIN ItemsTbl ON TempOrdersLinesTbl.ItemID = ItemsTbl.ItemID
                WHERE TempOrdersHeaderTbl.TOHeaderID = @TOHeaderID
                  AND TempOrdersHeaderTbl.ContactID = @ContactID
                  AND TempOrdersLinesTbl.ItemServiceTypeID <> @NotApplicableServiceType
                ORDER BY TempOrdersLinesTbl.ItemServiceTypeID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = headerId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@NotApplicableServiceType", DataValue = SystemConstants.ServiceTypeConstants.NotApplicable, DataDbType = DbType.Int32 }
            };

            var list = new List<TempOrderUsageLine>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new TempOrderUsageLine
                    {
                        ContactID = rdr["ContactID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ContactID"]),
                        ItemID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                        ItemServiceTypeID = rdr["ItemServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemServiceTypeID"]),
                        Qty = rdr["Qty"] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr["Qty"]),
                        UnitsPerQty = rdr["UnitsPerQty"] == DBNull.Value ? 1.0 : Convert.ToDouble(rdr["UnitsPerQty"]),
                        ItemPackagingID = rdr["ItemPackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemPackagingID"])
                    });
                }
            }

            return list;
        }

        private static List<DBParameter> BuildParameters(TempOrdersLine entity)
        {
            return new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = entity.TOHeaderID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemID", DataValue = entity.ItemID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemServiceTypeID", DataValue = entity.ItemServiceTypeID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Qty", DataValue = entity.Qty ?? (object)DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@ItemPackagingID", DataValue = entity.ItemPackagingID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@OriginalOrderID", DataValue = entity.OriginalOrderID ?? (object)DBNull.Value, DataDbType = DbType.Int32 }
            };
        }
    }
}
