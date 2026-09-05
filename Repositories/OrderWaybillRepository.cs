using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class OrderWaybillRepository
    {
        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'OrderWaybillTbl'") > 0;
            }
            catch
            {
                return false;
            }
        }

        public void EnsureTable()
        {
            if (TableExists())
                return;
            try
            {
                using (var db = new TrackerSQLDb())
                    db.ExecuteNonQuery(@"
IF OBJECT_ID(N'dbo.OrderWaybillTbl', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderWaybillTbl
    (
        WaybillID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderWaybillTbl PRIMARY KEY,
        OrderID INT NOT NULL,
        ContactID INT NULL,
        WaybillNumber NVARCHAR(100) NOT NULL,
        Carrier NVARCHAR(50) NULL,
        DispatchStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_OrderWaybill_Status DEFAULT (N'Dispatched'),
        DispatchedAt DATETIME2 NOT NULL CONSTRAINT DF_OrderWaybill_At DEFAULT (SYSUTCDATETIME()),
        WooOrderId BIGINT NULL,
        WooNotePosted BIT NOT NULL CONSTRAINT DF_OrderWaybill_WooNote DEFAULT (0),
        CustomerEmailSent BIT NOT NULL CONSTRAINT DF_OrderWaybill_Email DEFAULT (0),
        CreatedBy NVARCHAR(100) NULL,
        CONSTRAINT UQ_OrderWaybill_OrderID UNIQUE (OrderID)
    );
    CREATE INDEX IX_OrderWaybill_ContactID ON dbo.OrderWaybillTbl (ContactID);
END");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("woo", "OrderWaybillTbl ensure failed: " + ex.Message);
            }
        }

        public OrderWaybill GetByOrderId(int orderId)
        {
            EnsureTable();
            if (!TableExists() || orderId <= 0)
                return null;
            const string sql = "SELECT * FROM OrderWaybillTbl WHERE OrderID = @OrderID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = orderId, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                    return Map(rdr);
            }
            return null;
        }

        public List<OrderWaybill> GetByContactId(int contactId)
        {
            var list = new List<OrderWaybill>();
            EnsureTable();
            if (!TableExists() || contactId <= 0)
                return list;

            const string sql = @"
SELECT *
FROM OrderWaybillTbl
WHERE ContactID = @ContactID
ORDER BY DispatchedAt DESC, WaybillID DESC";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                while (rdr != null && rdr.Read())
                    list.Add(Map(rdr));
            }
            return list;
        }

        public void Upsert(OrderWaybill row)
        {
            if (row == null || row.OrderID <= 0 || string.IsNullOrWhiteSpace(row.WaybillNumber))
                return;
            EnsureTable();
            if (!TableExists())
                return;

            const string sql = @"
IF EXISTS (SELECT 1 FROM OrderWaybillTbl WHERE OrderID = @OrderID)
    UPDATE OrderWaybillTbl SET
        ContactID = @ContactID,
        WaybillNumber = @WaybillNumber,
        Carrier = @Carrier,
        DispatchStatus = @DispatchStatus,
        DispatchedAt = @DispatchedAt,
        WooOrderId = @WooOrderId,
        WooNotePosted = @WooNotePosted,
        CustomerEmailSent = @CustomerEmailSent,
        CreatedBy = @CreatedBy
    WHERE OrderID = @OrderID
ELSE
    INSERT INTO OrderWaybillTbl
    (OrderID, ContactID, WaybillNumber, Carrier, DispatchStatus, DispatchedAt,
     WooOrderId, WooNotePosted, CustomerEmailSent, CreatedBy)
    VALUES
    (@OrderID, @ContactID, @WaybillNumber, @Carrier, @DispatchStatus, @DispatchedAt,
     @WooOrderId, @WooNotePosted, @CustomerEmailSent, @CreatedBy);";

            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@OrderID", DataValue = row.OrderID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = row.ContactID.HasValue ? (object)row.ContactID.Value : DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@WaybillNumber", DataValue = row.WaybillNumber.Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@Carrier", DataValue = (object)row.Carrier ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@DispatchStatus", DataValue = string.IsNullOrWhiteSpace(row.DispatchStatus) ? "Dispatched" : row.DispatchStatus, DataDbType = DbType.String },
                new DBParameter { ParamName = "@DispatchedAt", DataValue = row.DispatchedAt, DataDbType = DbType.DateTime2 },
                new DBParameter { ParamName = "@WooOrderId", DataValue = row.WooOrderId.HasValue ? (object)row.WooOrderId.Value : DBNull.Value, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@WooNotePosted", DataValue = row.WooNotePosted, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@CustomerEmailSent", DataValue = row.CustomerEmailSent, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@CreatedBy", DataValue = (object)row.CreatedBy ?? DBNull.Value, DataDbType = DbType.String }
            };
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(sql, p);
        }

        private static OrderWaybill Map(IDataRecord rdr)
        {
            return new OrderWaybill
            {
                WaybillID = rdr["WaybillID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["WaybillID"]),
                OrderID = rdr["OrderID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["OrderID"]),
                ContactID = rdr["ContactID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["ContactID"]),
                WaybillNumber = rdr["WaybillNumber"] as string,
                Carrier = rdr["Carrier"] as string,
                DispatchStatus = rdr["DispatchStatus"] as string,
                DispatchedAt = rdr["DispatchedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["DispatchedAt"]),
                WooOrderId = rdr["WooOrderId"] == DBNull.Value ? (long?)null : Convert.ToInt64(rdr["WooOrderId"]),
                WooNotePosted = rdr["WooNotePosted"] != DBNull.Value && Convert.ToBoolean(rdr["WooNotePosted"]),
                CustomerEmailSent = rdr["CustomerEmailSent"] != DBNull.Value && Convert.ToBoolean(rdr["CustomerEmailSent"]),
                CreatedBy = rdr["CreatedBy"] as string
            };
        }
    }
}
