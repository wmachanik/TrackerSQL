using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooOrderInfoRepository
    {
        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'WooOrderInfoTbl'") > 0;
            }
            catch
            {
                return false;
            }
        }

        public WooOrderInfo GetByWooOrderId(long wooOrderId)
        {
            if (!TableExists() || wooOrderId <= 0)
                return null;
            const string sql = "SELECT * FROM WooOrderInfoTbl WHERE WooOrderId = @WooOrderId";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooOrderId", DataValue = wooOrderId, DataDbType = DbType.Int64 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr.Read())
                    return Map(rdr);
            }
            return null;
        }

        /// <summary>
        /// Woo import link only when the Tracker order still exists.
        /// Orphaned rows (order deleted) are removed so the Woo order can be imported again.
        /// </summary>
        public WooOrderInfo GetLiveByWooOrderId(long wooOrderId)
        {
            if (!TableExists() || wooOrderId <= 0)
                return null;

            const string sql = @"
SELECT w.*
FROM WooOrderInfoTbl w
INNER JOIN OrdersTbl o ON o.OrderID = w.OrderID
WHERE w.WooOrderId = @WooOrderId";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooOrderId", DataValue = wooOrderId, DataDbType = DbType.Int64 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                    return Map(rdr);
            }

            // Stale link — Tracker order gone.
            DeleteByWooOrderId(wooOrderId);
            return null;
        }

        public int DeleteByWooOrderId(long wooOrderId)
        {
            if (!TableExists() || wooOrderId <= 0)
                return 0;
            const string sql = "DELETE FROM WooOrderInfoTbl WHERE WooOrderId = @WooOrderId";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooOrderId", DataValue = wooOrderId, DataDbType = DbType.Int64 }
            };
            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(sql, p);
        }

        public int DeleteByOrderId(int orderId)
        {
            if (!TableExists() || orderId <= 0)
                return 0;
            const string sql = "DELETE FROM WooOrderInfoTbl WHERE OrderID = @OrderID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(sql, p);
        }

        public WooOrderInfo GetByTrackerOrderId(int orderId)
        {
            if (!TableExists() || orderId <= 0)
                return null;
            const string sql = "SELECT * FROM WooOrderInfoTbl WHERE OrderID = @OrderID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr.Read())
                    return Map(rdr);
            }
            return null;
        }

        public List<WooOrderImportConflictRow> GetOrdersWithConflicts(int maxRows = 200)
        {
            var list = new List<WooOrderImportConflictRow>();
            if (!TableExists() || !HasImportConflictsColumn())
                return list;

            string sql = @"
SELECT TOP (" + maxRows.ToString(System.Globalization.CultureInfo.InvariantCulture) + @")
    w.OrderID,
    w.WooOrderNumber,
    w.ImportConflicts,
    w.LastSyncedUtc,
    o.OrderDate,
    o.Notes,
    o.PurchaseOrder
FROM WooOrderInfoTbl w
INNER JOIN OrdersTbl o ON o.OrderID = w.OrderID
WHERE w.ImportConflicts IS NOT NULL AND LTRIM(RTRIM(w.ImportConflicts)) <> N''
ORDER BY w.LastSyncedUtc DESC, w.OrderID DESC";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new WooOrderImportConflictRow
                    {
                        OrderID = Convert.ToInt32(rdr["OrderID"]),
                        WooOrderNumber = rdr["WooOrderNumber"] as string,
                        ImportConflicts = rdr["ImportConflicts"] as string,
                        LastSyncedUtc = rdr["LastSyncedUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["LastSyncedUtc"]),
                        OrderDate = rdr["OrderDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["OrderDate"]).Date,
                        Notes = rdr["Notes"] as string,
                        PurchaseOrder = rdr["PurchaseOrder"] as string
                    });
                }
            }
            return list;
        }

        public void Upsert(WooOrderInfo row)
        {
            if (!TableExists() || row == null || row.OrderID <= 0 || row.WooOrderId <= 0)
                return;

            EnsureImportConflictsColumn();
            bool hasConflicts = HasImportConflictsColumn();

            // Drop any stale WooOrderId → deleted OrderID links, and keep one row per Woo order.
            DeleteByWooOrderIdExceptOrder(row.WooOrderId, row.OrderID);

            string sql = hasConflicts
                ? @"
IF EXISTS (SELECT 1 FROM WooOrderInfoTbl WHERE OrderID = @OrderID)
    UPDATE WooOrderInfoTbl SET
        WooOrderId = @WooOrderId,
        WooOrderNumber = @WooOrderNumber,
        WooStatus = @WooStatus,
        PaymentMethod = @PaymentMethod,
        PaymentStatus = @PaymentStatus,
        PaymentPaid = @PaymentPaid,
        TrackingNumber = @TrackingNumber,
        LastSyncedUtc = @LastSyncedUtc,
        RawSnapshotJson = @RawSnapshotJson,
        ImportConflicts = @ImportConflicts
    WHERE OrderID = @OrderID
ELSE IF EXISTS (SELECT 1 FROM WooOrderInfoTbl WHERE WooOrderId = @WooOrderId)
    UPDATE WooOrderInfoTbl SET
        OrderID = @OrderID,
        WooOrderNumber = @WooOrderNumber,
        WooStatus = @WooStatus,
        PaymentMethod = @PaymentMethod,
        PaymentStatus = @PaymentStatus,
        PaymentPaid = @PaymentPaid,
        TrackingNumber = @TrackingNumber,
        LastSyncedUtc = @LastSyncedUtc,
        RawSnapshotJson = @RawSnapshotJson,
        ImportConflicts = @ImportConflicts
    WHERE WooOrderId = @WooOrderId
ELSE
    INSERT INTO WooOrderInfoTbl
    (OrderID, WooOrderId, WooOrderNumber, WooStatus, PaymentMethod, PaymentStatus, PaymentPaid,
     TrackingNumber, LastSyncedUtc, RawSnapshotJson, ImportConflicts)
    VALUES
    (@OrderID, @WooOrderId, @WooOrderNumber, @WooStatus, @PaymentMethod, @PaymentStatus, @PaymentPaid,
     @TrackingNumber, @LastSyncedUtc, @RawSnapshotJson, @ImportConflicts);"
                : @"
IF EXISTS (SELECT 1 FROM WooOrderInfoTbl WHERE OrderID = @OrderID)
    UPDATE WooOrderInfoTbl SET
        WooOrderId = @WooOrderId,
        WooOrderNumber = @WooOrderNumber,
        WooStatus = @WooStatus,
        PaymentMethod = @PaymentMethod,
        PaymentStatus = @PaymentStatus,
        PaymentPaid = @PaymentPaid,
        TrackingNumber = @TrackingNumber,
        LastSyncedUtc = @LastSyncedUtc,
        RawSnapshotJson = @RawSnapshotJson
    WHERE OrderID = @OrderID
ELSE IF EXISTS (SELECT 1 FROM WooOrderInfoTbl WHERE WooOrderId = @WooOrderId)
    UPDATE WooOrderInfoTbl SET
        OrderID = @OrderID,
        WooOrderNumber = @WooOrderNumber,
        WooStatus = @WooStatus,
        PaymentMethod = @PaymentMethod,
        PaymentStatus = @PaymentStatus,
        PaymentPaid = @PaymentPaid,
        TrackingNumber = @TrackingNumber,
        LastSyncedUtc = @LastSyncedUtc,
        RawSnapshotJson = @RawSnapshotJson
    WHERE WooOrderId = @WooOrderId
ELSE
    INSERT INTO WooOrderInfoTbl
    (OrderID, WooOrderId, WooOrderNumber, WooStatus, PaymentMethod, PaymentStatus, PaymentPaid,
     TrackingNumber, LastSyncedUtc, RawSnapshotJson)
    VALUES
    (@OrderID, @WooOrderId, @WooOrderNumber, @WooStatus, @PaymentMethod, @PaymentStatus, @PaymentPaid,
     @TrackingNumber, @LastSyncedUtc, @RawSnapshotJson);";

            var p = BuildParams(row, includeConflicts: hasConflicts);
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(sql, p);
        }

        private int DeleteByWooOrderIdExceptOrder(long wooOrderId, int keepOrderId)
        {
            if (!TableExists() || wooOrderId <= 0)
                return 0;
            const string sql = @"
DELETE FROM WooOrderInfoTbl
WHERE WooOrderId = @WooOrderId
  AND OrderID <> @KeepOrderID
  AND NOT EXISTS (SELECT 1 FROM OrdersTbl o WHERE o.OrderID = WooOrderInfoTbl.OrderID)";
            // Also remove other live rows for same Woo id (should be at most one).
            const string sqlDup = @"
DELETE FROM WooOrderInfoTbl
WHERE WooOrderId = @WooOrderId AND OrderID <> @KeepOrderID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooOrderId", DataValue = wooOrderId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@KeepOrderID", DataValue = keepOrderId, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            {
                db.ExecuteNonQuery(sql, p);
                return db.ExecuteNonQuery(sqlDup, p);
            }
        }

        private static bool HasImportConflictsColumn()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(@"
SELECT COUNT(*)
FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.WooOrderInfoTbl') AND name = N'ImportConflicts'") > 0;
            }
            catch
            {
                return false;
            }
        }

        private static void EnsureImportConflictsColumn()
        {
            if (HasImportConflictsColumn())
                return;
            try
            {
                using (var db = new TrackerSQLDb())
                    db.ExecuteNonQuery(@"
IF OBJECT_ID(N'dbo.WooOrderInfoTbl', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.WooOrderInfoTbl', N'ImportConflicts') IS NULL
    ALTER TABLE dbo.WooOrderInfoTbl ADD ImportConflicts NVARCHAR(MAX) NULL;");
            }
            catch
            {
                // schema ensure via XMLtoSQL preferred
            }
        }

        private static List<DBParameter> BuildParams(WooOrderInfo row, bool includeConflicts)
        {
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = row.OrderID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@WooOrderId", DataValue = row.WooOrderId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@WooOrderNumber", DataValue = (object)row.WooOrderNumber ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@WooStatus", DataValue = (object)row.WooStatus ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PaymentMethod", DataValue = (object)row.PaymentMethod ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PaymentStatus", DataValue = (object)row.PaymentStatus ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PaymentPaid", DataValue = row.PaymentPaid, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@TrackingNumber", DataValue = (object)row.TrackingNumber ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@LastSyncedUtc", DataValue = row.LastSyncedUtc.HasValue ? (object)row.LastSyncedUtc.Value : DBNull.Value, DataDbType = DbType.DateTime2 },
                new DBParameter { ParamName = "@RawSnapshotJson", DataValue = (object)row.RawSnapshotJson ?? DBNull.Value, DataDbType = DbType.String }
            };
            if (includeConflicts)
                p.Add(new DBParameter { ParamName = "@ImportConflicts", DataValue = (object)row.ImportConflicts ?? DBNull.Value, DataDbType = DbType.String });
            return p;
        }

        private static WooOrderInfo Map(IDataReader rdr)
        {
            var info = new WooOrderInfo
            {
                OrderID = rdr["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderID"]),
                WooOrderId = rdr["WooOrderId"] == DBNull.Value ? 0 : Convert.ToInt64(rdr["WooOrderId"]),
                WooOrderNumber = rdr["WooOrderNumber"] as string,
                WooStatus = rdr["WooStatus"] as string,
                PaymentMethod = rdr["PaymentMethod"] as string,
                PaymentStatus = rdr["PaymentStatus"] as string,
                PaymentPaid = rdr["PaymentPaid"] != DBNull.Value && Convert.ToBoolean(rdr["PaymentPaid"]),
                TrackingNumber = rdr["TrackingNumber"] as string,
                LastSyncedUtc = rdr["LastSyncedUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["LastSyncedUtc"]),
                RawSnapshotJson = rdr["RawSnapshotJson"] as string
            };
            if (HasColumn(rdr, "ImportConflicts"))
                info.ImportConflicts = rdr["ImportConflicts"] as string;
            return info;
        }

        private static bool HasColumn(IDataRecord rdr, string name)
        {
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                if (string.Equals(rdr.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
