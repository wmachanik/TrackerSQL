using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>SQL for the mobile REST API: device tokens, delivery sheet, delivery proof, sync refs, repairs, lookups.</summary>
    public class MobileApiRepository
    {
        // ---------------- Device tokens ----------------

        public long InsertToken(string tokenHash, string userName, int? personId, string deviceId, string deviceName,
            string appVersion, DateTime expiresAt)
        {
            const string sql = @"
                INSERT INTO MobileDeviceTokensTbl (TokenHash, UserName, PersonID, DeviceId, DeviceName, AppVersion, CreatedAt, LastUsedAt, ExpiresAt)
                VALUES (@TokenHash, @UserName, @PersonID, @DeviceId, @DeviceName, @AppVersion, @Now, @Now, @ExpiresAt);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
            DateTime now = TimeZoneUtils.Now();
            return Scalar<long>(sql,
                P("@TokenHash", tokenHash),
                P("@UserName", userName),
                P("@PersonID", personId, DbType.Int32),
                P("@DeviceId", Trunc(deviceId, 128)),
                P("@DeviceName", Trunc(deviceName, 128)),
                P("@AppVersion", Trunc(appVersion, 32)),
                P("@Now", now, DbType.DateTime),
                P("@ExpiresAt", expiresAt, DbType.DateTime));
        }

        public MobileDeviceToken FindActiveToken(string tokenHash)
        {
            const string sql = @"
                SELECT TokenID, UserName, PersonID, DeviceId, DeviceName, AppVersion, CreatedAt, LastUsedAt, ExpiresAt, RevokedAt
                FROM MobileDeviceTokensTbl
                WHERE TokenHash = @TokenHash AND RevokedAt IS NULL AND ExpiresAt > @Now";
            var list = Query(sql, MapToken, P("@TokenHash", tokenHash), P("@Now", TimeZoneUtils.Now(), DbType.DateTime));
            return list.Count > 0 ? list[0] : null;
        }

        public void TouchToken(long tokenId)
        {
            Exec("UPDATE MobileDeviceTokensTbl SET LastUsedAt = @Now WHERE TokenID = @TokenID",
                P("@Now", TimeZoneUtils.Now(), DbType.DateTime), P("@TokenID", tokenId, DbType.Int64));
        }

        public int RevokeToken(long tokenId, string revokedBy)
        {
            return Exec(@"UPDATE MobileDeviceTokensTbl SET RevokedAt = @Now, RevokedBy = @By
                          WHERE TokenID = @TokenID AND RevokedAt IS NULL",
                P("@Now", TimeZoneUtils.Now(), DbType.DateTime), P("@By", revokedBy), P("@TokenID", tokenId, DbType.Int64));
        }

        /// <summary>Signing in again on the same phone replaces that phone's earlier token.</summary>
        public int RevokeDeviceTokens(string userName, string deviceId, string revokedBy)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return 0;
            return Exec(@"UPDATE MobileDeviceTokensTbl SET RevokedAt = @Now, RevokedBy = @By
                          WHERE UserName = @UserName AND DeviceId = @DeviceId AND RevokedAt IS NULL",
                P("@Now", TimeZoneUtils.Now(), DbType.DateTime), P("@By", revokedBy),
                P("@UserName", userName), P("@DeviceId", Trunc(deviceId, 128)));
        }

        public List<MobileDeviceToken> ListTokens(int max = 200)
        {
            string sql = @"
                SELECT TOP (" + Math.Max(1, Math.Min(max, 1000)) + @") TokenID, UserName, PersonID, DeviceId, DeviceName, AppVersion,
                       CreatedAt, LastUsedAt, ExpiresAt, RevokedAt
                FROM MobileDeviceTokensTbl
                ORDER BY CASE WHEN RevokedAt IS NULL THEN 0 ELSE 1 END, ISNULL(LastUsedAt, CreatedAt) DESC";
            return Query(sql, MapToken);
        }

        private static MobileDeviceToken MapToken(IDataRecord r)
        {
            return new MobileDeviceToken
            {
                TokenID = Convert.ToInt64(r["TokenID"]),
                UserName = Str(r, "UserName"),
                PersonID = NInt(r, "PersonID"),
                DeviceId = Str(r, "DeviceId"),
                DeviceName = Str(r, "DeviceName"),
                AppVersion = Str(r, "AppVersion"),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                LastUsedAt = NDate(r, "LastUsedAt"),
                ExpiresAt = Convert.ToDateTime(r["ExpiresAt"]),
                RevokedAt = NDate(r, "RevokedAt")
            };
        }

        // ---------------- Delivery sheet ----------------

        /// <summary>Dates that still have open orders (as the web Delivery Sheet), plus today.</summary>
        public List<MobileDeliveryDate> GetDeliveryDates(DateTime today, int? personId)
        {
            const string sql = @"
                SELECT CAST(o.RequiredByDate AS DATE) AS DeliveryDate,
                       COUNT(*) AS Orders,
                       SUM(CASE WHEN o.Done = 0 THEN 1 ELSE 0 END) AS OpenOrders,
                       SUM(CASE WHEN o.ToBeDeliveredByID = @PersonID THEN 1 ELSE 0 END) AS Mine
                FROM OrdersTbl o
                WHERE o.RequiredByDate IS NOT NULL
                  AND EXISTS (SELECT 1 FROM OrderLinesTbl ol WHERE ol.OrderID = o.OrderID)
                  AND (CAST(o.RequiredByDate AS DATE) = @Today
                       OR CAST(o.RequiredByDate AS DATE) IN (
                           SELECT DISTINCT CAST(x.RequiredByDate AS DATE) FROM OrdersTbl x
                           WHERE x.Done = 0 AND x.RequiredByDate IS NOT NULL
                             AND EXISTS (SELECT 1 FROM OrderLinesTbl xl WHERE xl.OrderID = x.OrderID)))
                GROUP BY CAST(o.RequiredByDate AS DATE)
                ORDER BY DeliveryDate";
            return Query(sql, r => new MobileDeliveryDate
            {
                Date = Convert.ToDateTime(r["DeliveryDate"]).ToString("yyyy-MM-dd"),
                Orders = Convert.ToInt32(r["Orders"]),
                Open = Convert.ToInt32(r["OpenOrders"]),
                Mine = personId.HasValue ? Convert.ToInt32(r["Mine"]) : (int?)null
            }, P("@Today", today.Date, DbType.Date), P("@PersonID", personId ?? -1, DbType.Int32));
        }

        public class SheetRow
        {
            public int OrderID;
            public int ContactID;
            public string CompanyName;
            public string FirstName;
            public string LastName;
            public string Phone;
            public string Cell;
            public string Address;
            public string Province;
            public string PostalCode;
            public string AreaName;
            public string Notes;
            public string PurchaseOrder;
            public bool Done;
            public bool Confirmed;
            public int DeliveryOrder;
            public int? DeliveryById;
            public string DeliveryBy;
            public int OrderLineID;
            public int? ItemID;
            public int? ItemServiceTypeID;
            public double Qty;
            public string ItemDesc;
            public string PackDesc;
            public long? ProofID;
            public string ProofOutcome;
            public string ProofReason;
            public string ProofReceivedBy;
            public string ProofMissingItems;
            public DateTime? ProofDeliveredAt;
            public string ProofStatus;
            public bool ProofHasSignature;
        }

        /// <summary>One row per order line for the date (optionally one driver), with contact and latest proof.</summary>
        public List<SheetRow> GetSheetRows(DateTime date, int? personId, int? orderId = null)
        {
            string sql = @"
                SELECT o.OrderID, o.ContactID, c.CompanyName, c.ContactFirstName, c.ContactLastName,
                       c.PhoneNumber, c.CellNumber, c.BillingAddress, c.StateOrProvince, c.PostalCode,
                       a.AreaName, o.Notes, o.PurchaseOrder, o.Done, o.Confirmed,
                       ISNULL(apd.DeliveryOrder, 999) AS DeliveryOrder,
                       o.ToBeDeliveredByID, p.Abbreviation AS DeliveryBy,
                       ol.OrderLineID, ol.ItemID, i.ItemServiceTypeID,
                       ol.QtyOrdered, i.ItemDesc, ip.ItemPrepDescription AS PackDesc,
                       pr.ProofID, pr.Outcome AS ProofOutcome, pr.Reason AS ProofReason, pr.ReceivedByName AS ProofReceivedBy,
                       pr.MissingItems AS ProofMissingItems,
                       pr.DeliveredAt AS ProofDeliveredAt, pr.Status AS ProofStatus, pr.HasSignature AS ProofHasSignature
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol ON ol.OrderID = o.OrderID
                LEFT JOIN ContactsTbl c ON c.ContactID = o.ContactID
                LEFT JOIN AreasTbl a ON a.AreaID = c.AreaID
                LEFT JOIN PeopleTbl p ON p.PersonID = o.ToBeDeliveredByID
                LEFT JOIN ItemsTbl i ON i.ItemID = ol.ItemID
                LEFT JOIN ItemPackagingsTbl ip ON ip.ItemPackagingID = ol.PackagingID
                OUTER APPLY (SELECT MIN(d.DeliveryOrder) AS DeliveryOrder FROM AreaPrepDaysTbl d WHERE d.AreaID = c.AreaID) apd
                OUTER APPLY (SELECT TOP 1 m.ProofID, m.Outcome, m.Reason, m.ReceivedByName, m.MissingItems, m.DeliveredAt, m.Status,
                                    CAST(CASE WHEN m.SignatureImage IS NULL THEN 0 ELSE 1 END AS BIT) AS HasSignature
                             FROM MobileDeliveryProofTbl m WHERE m.OrderID = o.OrderID ORDER BY m.ProofID DESC) pr
                WHERE CAST(o.RequiredByDate AS DATE) = @Date";
            var ps = new List<DBParameter> { P("@Date", date.Date, DbType.Date) };
            if (personId.HasValue)
            {
                sql += " AND o.ToBeDeliveredByID = @PersonID";
                ps.Add(P("@PersonID", personId.Value, DbType.Int32));
            }
            if (orderId.HasValue)
            {
                sql += " AND o.OrderID = @OrderID";
                ps.Add(P("@OrderID", orderId.Value, DbType.Int32));
            }
            sql += " ORDER BY o.Done, ISNULL(apd.DeliveryOrder, 999), c.CompanyName, o.OrderID, i.SortOrder, ol.OrderLineID";

            return Query(sql, r => new SheetRow
            {
                OrderID = Convert.ToInt32(r["OrderID"]),
                ContactID = NInt(r, "ContactID") ?? 0,
                CompanyName = Str(r, "CompanyName"),
                FirstName = Str(r, "ContactFirstName"),
                LastName = Str(r, "ContactLastName"),
                Phone = Str(r, "PhoneNumber"),
                Cell = Str(r, "CellNumber"),
                Address = Str(r, "BillingAddress"),
                Province = Str(r, "StateOrProvince"),
                PostalCode = Str(r, "PostalCode"),
                AreaName = Str(r, "AreaName"),
                Notes = Str(r, "Notes"),
                PurchaseOrder = Str(r, "PurchaseOrder"),
                Done = Bool(r, "Done"),
                Confirmed = Bool(r, "Confirmed"),
                DeliveryOrder = NInt(r, "DeliveryOrder") ?? 999,
                DeliveryById = NInt(r, "ToBeDeliveredByID"),
                DeliveryBy = Str(r, "DeliveryBy"),
                OrderLineID = Convert.ToInt32(r["OrderLineID"]),
                ItemID = NInt(r, "ItemID"),
                ItemServiceTypeID = NInt(r, "ItemServiceTypeID"),
                Qty = r["QtyOrdered"] == DBNull.Value ? 0 : Convert.ToDouble(r["QtyOrdered"]),
                ItemDesc = Str(r, "ItemDesc"),
                PackDesc = Str(r, "PackDesc"),
                ProofID = r["ProofID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["ProofID"]),
                ProofOutcome = Str(r, "ProofOutcome"),
                ProofReason = Str(r, "ProofReason"),
                ProofReceivedBy = Str(r, "ProofReceivedBy"),
                ProofMissingItems = Str(r, "ProofMissingItems"),
                ProofDeliveredAt = NDate(r, "ProofDeliveredAt"),
                ProofStatus = Str(r, "ProofStatus"),
                ProofHasSignature = Bool(r, "ProofHasSignature")
            }, ps.ToArray());
        }

        /// <summary>Repairs whose related order line is on an order due on the date (optionally one driver), keyed by OrderID.</summary>
        public List<KeyValuePair<int, MobileStopRepair>> GetSheetRepairs(DateTime date, int? personId)
        {
            string sql = @"
                SELECT ol.OrderID, r.RepairID, r.JobCardNumber, e.EquipTypeName, r.EquipSerialNumber,
                       ISNULL(NULLIF(r.RepairFaultDesc, ''), f.RepairFaultDesc) AS Fault, r.RepairStatusID, s.RepairStatusDesc
                FROM RepairsTbl r
                INNER JOIN OrderLinesTbl ol ON ol.OrderLineID = r.RelatedOrderLineID
                INNER JOIN OrdersTbl o ON o.OrderID = ol.OrderID
                LEFT JOIN EquipTypesTbl e ON e.EquipTypeID = r.EquipTypeID
                LEFT JOIN RepairFaultsTbl f ON f.RepairFaultID = r.RepairFaultID
                LEFT JOIN RepairStatusesTbl s ON s.RepairStatusID = r.RepairStatusID
                WHERE CAST(o.RequiredByDate AS DATE) = @Date";
            var ps = new List<DBParameter> { P("@Date", date.Date, DbType.Date) };
            if (personId.HasValue)
            {
                sql += " AND o.ToBeDeliveredByID = @PersonID";
                ps.Add(P("@PersonID", personId.Value, DbType.Int32));
            }
            sql += " ORDER BY ol.OrderID, r.RepairID";

            return Query(sql, r => new KeyValuePair<int, MobileStopRepair>(Convert.ToInt32(r["OrderID"]), new MobileStopRepair
            {
                Id = Convert.ToInt32(r["RepairID"]),
                JobCard = NullIfEmpty(Str(r, "JobCardNumber")),
                EquipType = NullIfEmpty(Str(r, "EquipTypeName")),
                Serial = NullIfEmpty(Str(r, "EquipSerialNumber")),
                Fault = NullIfEmpty(Str(r, "Fault")),
                StatusId = NInt(r, "RepairStatusID"),
                Status = NullIfEmpty(Str(r, "RepairStatusDesc"))
            }), ps.ToArray());
        }

        public class OrderBrief
        {
            public int OrderID;
            public int ContactID;
            public bool Done;
            public int? DeliveryById;
            public DateTime? RequiredByDate;
        }

        public OrderBrief GetOrderBrief(int orderId)
        {
            var list = Query("SELECT OrderID, ContactID, Done, ToBeDeliveredByID, RequiredByDate FROM OrdersTbl WHERE OrderID = @OrderID",
                r => new OrderBrief
                {
                    OrderID = Convert.ToInt32(r["OrderID"]),
                    ContactID = NInt(r, "ContactID") ?? 0,
                    Done = Bool(r, "Done"),
                    DeliveryById = NInt(r, "ToBeDeliveredByID"),
                    RequiredByDate = NDate(r, "RequiredByDate")
                }, P("@OrderID", orderId, DbType.Int32));
            return list.Count > 0 ? list[0] : null;
        }

        // ---------------- Delivery proof ----------------

        public long InsertProof(MobileDeliveryProof proof, byte[] signature)
        {
            const string sql = @"
                INSERT INTO MobileDeliveryProofTbl
                    (ClientRef, OrderID, ContactID, PersonID, UserName, Outcome, Reason, ReceivedByName, SignatureImage, SignatureContentType,
                     DriverNote, MissingItems, DeliveredAt, Latitude, Longitude, Status, StatusMessage, ReceivedAt)
                VALUES
                    (@ClientRef, @OrderID, @ContactID, @PersonID, @UserName, @Outcome, @Reason, @ReceivedByName, @SignatureImage, @SignatureContentType,
                     @DriverNote, @MissingItems, @DeliveredAt, @Latitude, @Longitude, @Status, @StatusMessage, @ReceivedAt);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
            return Scalar<long>(sql,
                P("@ClientRef", proof.ClientRef),
                P("@OrderID", proof.OrderID, DbType.Int32),
                P("@ContactID", proof.ContactID, DbType.Int32),
                P("@PersonID", proof.PersonID, DbType.Int32),
                P("@UserName", proof.UserName),
                P("@Outcome", proof.Outcome),
                P("@Reason", Trunc(proof.Reason, 100)),
                P("@ReceivedByName", Trunc(proof.ReceivedByName, 128)),
                P("@SignatureImage", signature, DbType.Binary),
                P("@SignatureContentType", proof.SignatureContentType),
                P("@DriverNote", Trunc(proof.DriverNote, 1000)),
                P("@MissingItems", Trunc(proof.MissingItems, 1000)),
                P("@DeliveredAt", proof.DeliveredAt, DbType.DateTime),
                P("@Latitude", proof.Latitude, DbType.Decimal),
                P("@Longitude", proof.Longitude, DbType.Decimal),
                P("@Status", proof.Status),
                P("@StatusMessage", Trunc(proof.StatusMessage, 1000)),
                P("@ReceivedAt", TimeZoneUtils.Now(), DbType.DateTime));
        }

        public void UpdateProofStatus(long proofId, string status, string message, string completedBy)
        {
            const string sql = @"
                UPDATE MobileDeliveryProofTbl
                SET Status = @Status, StatusMessage = @Message,
                    CompletedAt = CASE WHEN @Status = 'Completed' THEN @Now ELSE CompletedAt END,
                    CompletedBy = CASE WHEN @Status = 'Completed' THEN @By ELSE CompletedBy END
                WHERE ProofID = @ProofID";
            Exec(sql, P("@Status", status), P("@Message", Trunc(message, 1000)), P("@Now", TimeZoneUtils.Now(), DbType.DateTime),
                P("@By", completedBy), P("@ProofID", proofId, DbType.Int64));
        }

        private const string ProofColumns = @"
            m.ProofID, m.ClientRef, m.OrderID, m.ContactID, m.PersonID, m.UserName, m.Outcome, m.Reason, m.ReceivedByName,
            m.SignatureContentType, CAST(CASE WHEN m.SignatureImage IS NULL THEN 0 ELSE 1 END AS BIT) AS HasSignature,
            m.DriverNote, m.MissingItems, m.DeliveredAt, m.Latitude, m.Longitude, m.Status, m.StatusMessage, m.ReceivedAt,
            m.CompletedAt, m.CompletedBy, c.CompanyName";

        public MobileDeliveryProof GetProofByClientRef(string clientRef)
        {
            var list = Query("SELECT " + ProofColumns + @" FROM MobileDeliveryProofTbl m
                              LEFT JOIN ContactsTbl c ON c.ContactID = m.ContactID WHERE m.ClientRef = @ClientRef",
                MapProof, P("@ClientRef", clientRef));
            return list.Count > 0 ? list[0] : null;
        }

        public MobileDeliveryProof GetProof(long proofId)
        {
            var list = Query("SELECT " + ProofColumns + @" FROM MobileDeliveryProofTbl m
                              LEFT JOIN ContactsTbl c ON c.ContactID = m.ContactID WHERE m.ProofID = @ProofID",
                MapProof, P("@ProofID", proofId, DbType.Int64));
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>Proof captured for orders due on the date, or (when orderId is given) for that order.</summary>
        public List<MobileDeliveryProof> ListProofs(DateTime? date, int? orderId, int max = 500)
        {
            string sql = "SELECT TOP (" + Math.Max(1, Math.Min(max, 2000)) + ") " + ProofColumns + @"
                FROM MobileDeliveryProofTbl m
                LEFT JOIN ContactsTbl c ON c.ContactID = m.ContactID
                LEFT JOIN OrdersTbl o ON o.OrderID = m.OrderID
                WHERE 1 = 1";
            var ps = new List<DBParameter>();
            if (date.HasValue)
            {
                sql += " AND (CAST(o.RequiredByDate AS DATE) = @Date OR CAST(m.DeliveredAt AS DATE) = @Date)";
                ps.Add(P("@Date", date.Value.Date, DbType.Date));
            }
            if (orderId.HasValue)
            {
                sql += " AND m.OrderID = @OrderID";
                ps.Add(P("@OrderID", orderId.Value, DbType.Int32));
            }
            sql += " ORDER BY m.DeliveredAt DESC, m.ProofID DESC";
            return Query(sql, MapProof, ps.ToArray());
        }

        public bool TryGetSignature(long proofId, out byte[] image, out string contentType)
        {
            image = null;
            contentType = null;
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(
                "SELECT SignatureImage, SignatureContentType FROM MobileDeliveryProofTbl WHERE ProofID = @ProofID",
                new List<DBParameter> { P("@ProofID", proofId, DbType.Int64) }))
            {
                if (rdr == null || !rdr.Read() || rdr["SignatureImage"] == DBNull.Value)
                    return false;
                image = (byte[])rdr["SignatureImage"];
                contentType = Str(rdr, "SignatureContentType");
                return true;
            }
        }

        private static MobileDeliveryProof MapProof(IDataRecord r)
        {
            return new MobileDeliveryProof
            {
                ProofID = Convert.ToInt64(r["ProofID"]),
                ClientRef = Str(r, "ClientRef"),
                OrderID = Convert.ToInt32(r["OrderID"]),
                ContactID = NInt(r, "ContactID"),
                PersonID = NInt(r, "PersonID"),
                UserName = Str(r, "UserName"),
                Outcome = Str(r, "Outcome"),
                Reason = Str(r, "Reason"),
                ReceivedByName = Str(r, "ReceivedByName"),
                SignatureContentType = Str(r, "SignatureContentType"),
                HasSignature = Bool(r, "HasSignature"),
                DriverNote = Str(r, "DriverNote"),
                MissingItems = Str(r, "MissingItems"),
                DeliveredAt = Convert.ToDateTime(r["DeliveredAt"]),
                Latitude = r["Latitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Latitude"]),
                Longitude = r["Longitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Longitude"]),
                Status = Str(r, "Status"),
                StatusMessage = Str(r, "StatusMessage"),
                ReceivedAt = Convert.ToDateTime(r["ReceivedAt"]),
                CompletedAt = NDate(r, "CompletedAt"),
                CompletedBy = Str(r, "CompletedBy"),
                CompanyName = Str(r, "CompanyName")
            };
        }

        // ---------------- Offline sync de-duplication ----------------

        public int? GetSyncEntityId(string clientRef, string kind)
        {
            object value = ScalarObject("SELECT EntityID FROM MobileSyncRefTbl WHERE ClientRef = @ClientRef AND Kind = @Kind",
                P("@ClientRef", clientRef), P("@Kind", kind));
            return value == null ? (int?)null : Convert.ToInt32(value);
        }

        public void InsertSyncRef(string clientRef, string kind, int entityId, string userName)
        {
            Exec(@"IF NOT EXISTS (SELECT 1 FROM MobileSyncRefTbl WHERE ClientRef = @ClientRef)
                   INSERT INTO MobileSyncRefTbl (ClientRef, Kind, EntityID, UserName, CreatedAt)
                   VALUES (@ClientRef, @Kind, @EntityID, @UserName, @Now)",
                P("@ClientRef", clientRef), P("@Kind", kind), P("@EntityID", entityId, DbType.Int32),
                P("@UserName", userName), P("@Now", TimeZoneUtils.Now(), DbType.DateTime));
        }

        // ---------------- Repairs ----------------

        private const string RepairSelect = @"
            SELECT {TOP} r.RepairID, r.ContactID, c.CompanyName, r.ContactName, r.ContactEmail, r.JobCardNumber,
                   r.DateLogged, r.LastStatusChange, r.EquipTypeID, e.EquipTypeName, r.EquipSerialNumber,
                   r.SwopOutMachineID, r.EquipConditionID, r.TakenFrother, r.TakenBeanLid, r.TakenWaterLid,
                   r.BrokenFrother, r.BrokenBeanLid, r.BrokenWaterLid, r.RepairFaultID, f.RepairFaultDesc AS FaultName,
                   r.RepairFaultDesc, r.RepairStatusID, s.RepairStatusDesc, ol.OrderID AS RelatedOrderID, r.Notes
            FROM RepairsTbl r
            LEFT JOIN ContactsTbl c ON c.ContactID = r.ContactID
            LEFT JOIN EquipTypesTbl e ON e.EquipTypeID = r.EquipTypeID
            LEFT JOIN RepairFaultsTbl f ON f.RepairFaultID = r.RepairFaultID
            LEFT JOIN RepairStatusesTbl s ON s.RepairStatusID = r.RepairStatusID
            LEFT JOIN OrderLinesTbl ol ON ol.OrderLineID = r.RelatedOrderLineID";

        /// <summary>Repairs newest first. openOnly excludes Done; since returns only repairs changed after that time.</summary>
        public List<MobileRepair> ListRepairs(bool openOnly, int? contactId, DateTime? since, int max)
        {
            string sql = RepairSelect.Replace("{TOP}", "TOP (" + Math.Max(1, Math.Min(max, 500)) + ")") + " WHERE 1 = 1";
            var ps = new List<DBParameter>();
            if (openOnly)
                sql += " AND ISNULL(r.RepairStatusID, 0) <> " + RepairsRepository.DoneStatusId;
            if (contactId.HasValue)
            {
                sql += " AND r.ContactID = @ContactID";
                ps.Add(P("@ContactID", contactId.Value, DbType.Int32));
            }
            if (since.HasValue)
            {
                sql += " AND r.LastStatusChange > @Since";
                ps.Add(P("@Since", since.Value, DbType.DateTime));
            }
            sql += " ORDER BY r.DateLogged DESC, r.RepairID DESC";
            return Query(sql, MapRepair, ps.ToArray());
        }

        public MobileRepair GetRepair(int repairId)
        {
            var list = Query(RepairSelect.Replace("{TOP}", string.Empty) + " WHERE r.RepairID = @RepairID",
                MapRepair, P("@RepairID", repairId, DbType.Int32));
            return list.Count > 0 ? list[0] : null;
        }

        private static MobileRepair MapRepair(IDataRecord r)
        {
            return new MobileRepair
            {
                Id = Convert.ToInt32(r["RepairID"]),
                ContactId = NInt(r, "ContactID") ?? 0,
                Company = NullIfEmpty(Str(r, "CompanyName")),
                ContactName = NullIfEmpty(Str(r, "ContactName")),
                ContactEmail = NullIfEmpty(Str(r, "ContactEmail")),
                JobCard = NullIfEmpty(Str(r, "JobCardNumber")),
                Logged = NDate(r, "DateLogged")?.ToString("yyyy-MM-dd"),
                LastChange = NDate(r, "LastStatusChange")?.ToString("yyyy-MM-ddTHH:mm:ss"),
                EquipTypeId = NInt(r, "EquipTypeID"),
                EquipType = NullIfEmpty(Str(r, "EquipTypeName")),
                Serial = NullIfEmpty(Str(r, "EquipSerialNumber")),
                SwopOutId = NInt(r, "SwopOutMachineID"),
                ConditionId = NInt(r, "EquipConditionID"),
                TakenFrother = Bool(r, "TakenFrother"),
                TakenBeanLid = Bool(r, "TakenBeanLid"),
                TakenWaterLid = Bool(r, "TakenWaterLid"),
                BrokenFrother = Bool(r, "BrokenFrother"),
                BrokenBeanLid = Bool(r, "BrokenBeanLid"),
                BrokenWaterLid = Bool(r, "BrokenWaterLid"),
                FaultId = NInt(r, "RepairFaultID"),
                Fault = NullIfEmpty(Str(r, "FaultName")),
                FaultDesc = NullIfEmpty(Str(r, "RepairFaultDesc")),
                StatusId = NInt(r, "RepairStatusID"),
                Status = NullIfEmpty(Str(r, "RepairStatusDesc")),
                RelatedOrderId = NInt(r, "RelatedOrderID"),
                Notes = NullIfEmpty(Str(r, "Notes"))
            };
        }

        // ---------------- Lookups and contacts ----------------

        public MobileLookups GetLookups()
        {
            return new MobileLookups
            {
                RepairStatuses = Lookup("SELECT RepairStatusID AS Id, RepairStatusDesc AS Name, NULL AS Code FROM RepairStatusesTbl ORDER BY SortOrder, RepairStatusID"),
                Faults = Lookup("SELECT RepairFaultID AS Id, RepairFaultDesc AS Name, NULL AS Code FROM RepairFaultsTbl ORDER BY SortOrder, RepairFaultDesc"),
                EquipTypes = Lookup("SELECT EquipTypeID AS Id, EquipTypeName AS Name, NULL AS Code FROM EquipTypesTbl ORDER BY EquipTypeName"),
                Conditions = Lookup("SELECT EquipConditionID AS Id, ConditionDesc AS Name, NULL AS Code FROM EquipConditionsTbl ORDER BY SortOrder, ConditionDesc"),
                People = Lookup("SELECT PersonID AS Id, Person AS Name, Abbreviation AS Code FROM PeopleTbl WHERE Enabled = 1 ORDER BY Person"),
                DeliveryOptions = GetDeliveryOptions(enabledOnly: true),
                Settings = Managers.MobileApiSettingsManager.ForApp()
            };
        }

        // ---------------- Delivery options (Lookups > Driver options) ----------------

        public List<MobileDeliveryOption> GetDeliveryOptions(bool enabledOnly)
        {
            return Query(@"SELECT OptionID, Outcome, OptionText, NeedsName, SortOrder, IsEnabled
                           FROM MobileDeliveryOptionsTbl"
                           + (enabledOnly ? " WHERE IsEnabled = 1" : string.Empty)
                           + " ORDER BY CASE WHEN Outcome = 'Delivered' THEN 0 ELSE 1 END, SortOrder, OptionText",
                r => new MobileDeliveryOption
                {
                    Id = Convert.ToInt32(r["OptionID"]),
                    Outcome = Str(r, "Outcome"),
                    Text = Str(r, "OptionText"),
                    NeedsName = Bool(r, "NeedsName"),
                    SortOrder = Convert.ToInt32(r["SortOrder"]),
                    Enabled = Bool(r, "IsEnabled")
                });
        }

        public void InsertDeliveryOption(MobileDeliveryOption option)
        {
            Exec(@"INSERT INTO MobileDeliveryOptionsTbl (Outcome, OptionText, NeedsName, SortOrder, IsEnabled)
                   VALUES (@Outcome, @Text, @NeedsName, @SortOrder, @Enabled)",
                P("@Outcome", option.Outcome), P("@Text", Trunc(option.Text, 100)),
                P("@NeedsName", option.NeedsName, DbType.Boolean), P("@SortOrder", option.SortOrder, DbType.Int32),
                P("@Enabled", option.Enabled, DbType.Boolean));
        }

        public void UpdateDeliveryOption(MobileDeliveryOption option)
        {
            Exec(@"UPDATE MobileDeliveryOptionsTbl
                   SET Outcome = @Outcome, OptionText = @Text, NeedsName = @NeedsName, SortOrder = @SortOrder, IsEnabled = @Enabled
                   WHERE OptionID = @Id",
                P("@Outcome", option.Outcome), P("@Text", Trunc(option.Text, 100)),
                P("@NeedsName", option.NeedsName, DbType.Boolean), P("@SortOrder", option.SortOrder, DbType.Int32),
                P("@Enabled", option.Enabled, DbType.Boolean), P("@Id", option.Id, DbType.Int32));
        }

        public void DeleteDeliveryOption(int optionId)
        {
            Exec("DELETE FROM MobileDeliveryOptionsTbl WHERE OptionID = @Id", P("@Id", optionId, DbType.Int32));
        }

        private List<MobileLookupItem> Lookup(string sql)
        {
            return Query(sql, r => new MobileLookupItem
            {
                Id = Convert.ToInt32(r["Id"]),
                Name = Str(r, "Name"),
                Code = NullIfEmpty(Str(r, "Code"))
            });
        }

        public List<MobileContact> SearchContacts(string text, bool includeDisabled, int max)
        {
            string sql = @"
                SELECT TOP (" + Math.Max(1, Math.Min(max, 100)) + @") ContactID, CompanyName, ContactFirstName, ContactLastName,
                       PhoneNumber, CellNumber, EmailAddress, BillingAddress, PostalCode, Enabled, EquipTypeID, EquipentSN
                FROM ContactsTbl
                WHERE (ISNULL(CompanyName, '') LIKE @Like ESCAPE '\'
                       OR (ISNULL(ContactFirstName, '') + ' ' + ISNULL(ContactLastName, '')) LIKE @Like ESCAPE '\'
                       OR ISNULL(CellNumber, '') LIKE @Like ESCAPE '\'
                       OR ISNULL(PhoneNumber, '') LIKE @Like ESCAPE '\'
                       OR ContactID = @Id)";
            if (!includeDisabled)
                sql += " AND Enabled = 1";
            sql += " ORDER BY CASE WHEN CompanyName LIKE @Starts ESCAPE '\\' THEN 0 ELSE 1 END, CompanyName";

            string t = (text ?? string.Empty).Trim();
            int.TryParse(t, out int id);
            return Query(sql, MapContact,
                P("@Like", "%" + EscapeLike(t) + "%"),
                P("@Starts", EscapeLike(t) + "%"),
                P("@Id", id, DbType.Int32));
        }

        public MobileContact GetContact(int contactId)
        {
            var list = Query(@"SELECT ContactID, CompanyName, ContactFirstName, ContactLastName, PhoneNumber, CellNumber,
                                      EmailAddress, BillingAddress, PostalCode, Enabled, EquipTypeID, EquipentSN
                               FROM ContactsTbl WHERE ContactID = @ContactID",
                MapContact, P("@ContactID", contactId, DbType.Int32));
            return list.Count > 0 ? list[0] : null;
        }

        private static MobileContact MapContact(IDataRecord r)
        {
            return new MobileContact
            {
                Id = Convert.ToInt32(r["ContactID"]),
                Company = Str(r, "CompanyName"),
                ContactName = NullIfEmpty((Str(r, "ContactFirstName") + " " + Str(r, "ContactLastName")).Trim()),
                Phone = NullIfEmpty(Str(r, "PhoneNumber")),
                Cell = NullIfEmpty(Str(r, "CellNumber")),
                Email = NullIfEmpty(Str(r, "EmailAddress")),
                Address = NullIfEmpty(Str(r, "BillingAddress")),
                PostalCode = NullIfEmpty(Str(r, "PostalCode")),
                Enabled = Bool(r, "Enabled"),
                EquipTypeId = NInt(r, "EquipTypeID"),
                Serial = NullIfEmpty(Str(r, "EquipentSN"))
            };
        }

        // ---------------- Request log ----------------

        public void InsertRequestLog(MobileApiLogEntry e)
        {
            Exec(@"INSERT INTO MobileApiRequestLogTbl
                       (LoggedAt, Method, Path, StatusCode, DurationMs, RequestBytes, ResponseBytes, UserName, TokenID, ClientIp, UserAgent, Note)
                   VALUES (@At, @Method, @Path, @Status, @Ms, @ReqBytes, @ResBytes, @User, @TokenID, @Ip, @Agent, @Note)",
                P("@At", e.At, DbType.DateTime),
                P("@Method", Trunc(e.Method, 10)),
                P("@Path", Trunc(e.Path, 400)),
                P("@Status", e.Status, DbType.Int32),
                P("@Ms", e.Ms, DbType.Int32),
                P("@ReqBytes", e.RequestBytes, DbType.Int32),
                P("@ResBytes", e.ResponseBytes, DbType.Int32),
                P("@User", Trunc(e.User, 256)),
                P("@TokenID", e.TokenId, DbType.Int64),
                P("@Ip", Trunc(e.Ip, 45)),
                P("@Agent", Trunc(e.Agent, 256)),
                P("@Note", Trunc(e.Note, 500)));
        }

        /// <summary>Newest first. problemsOnly = status 400 and above.</summary>
        public List<MobileApiLogEntry> ListRequestLog(int max, string userName, bool problemsOnly)
        {
            string sql = @"
                SELECT TOP (" + Math.Max(1, Math.Min(max, 1000)) + @") LogID, LoggedAt, Method, Path, StatusCode, DurationMs,
                       RequestBytes, ResponseBytes, UserName, TokenID, ClientIp, UserAgent, Note
                FROM MobileApiRequestLogTbl
                WHERE (@User IS NULL OR UserName = @User)
                  AND (@Problems = 0 OR StatusCode >= 400)
                ORDER BY LogID DESC";
            return Query(sql, r => new MobileApiLogEntry
            {
                Id = Convert.ToInt64(r["LogID"]),
                At = Convert.ToDateTime(r["LoggedAt"]),
                Method = Str(r, "Method"),
                Path = Str(r, "Path"),
                Status = Convert.ToInt32(r["StatusCode"]),
                Ms = Convert.ToInt32(r["DurationMs"]),
                RequestBytes = NInt(r, "RequestBytes"),
                ResponseBytes = NInt(r, "ResponseBytes"),
                User = NullIfEmpty(Str(r, "UserName")),
                TokenId = r["TokenID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["TokenID"]),
                Ip = NullIfEmpty(Str(r, "ClientIp")),
                Agent = NullIfEmpty(Str(r, "UserAgent")),
                Note = NullIfEmpty(Str(r, "Note"))
            },
            P("@User", NullIfEmpty(userName?.Trim())),
            P("@Problems", problemsOnly ? 1 : 0, DbType.Int32));
        }

        public int PurgeRequestLog(DateTime before)
        {
            return Exec("DELETE FROM MobileApiRequestLogTbl WHERE LoggedAt < @Before", P("@Before", before, DbType.DateTime));
        }

        // ---------------- Helpers ----------------

        private static DBParameter P(string name, object value, DbType type = DbType.String)
        {
            return new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
        }

        private static List<T> Query<T>(string sql, Func<IDataRecord, T> map, params DBParameter[] ps)
        {
            var list = new List<T>();
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, new List<DBParameter>(ps)))
            {
                while (rdr != null && rdr.Read())
                    list.Add(map(rdr));
            }
            return list;
        }

        private static int Exec(string sql, params DBParameter[] ps)
        {
            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(sql, new List<DBParameter>(ps));
        }

        private static T Scalar<T>(string sql, params DBParameter[] ps)
        {
            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<T>(sql, new List<DBParameter>(ps));
        }

        private static object ScalarObject(string sql, params DBParameter[] ps)
        {
            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar(sql, new List<DBParameter>(ps));
        }

        private static string Str(IDataRecord r, string name)
        {
            object v = r[name];
            return v == DBNull.Value ? string.Empty : Convert.ToString(v).Trim();
        }

        private static int? NInt(IDataRecord r, string name)
        {
            object v = r[name];
            return v == DBNull.Value ? (int?)null : Convert.ToInt32(v);
        }

        private static DateTime? NDate(IDataRecord r, string name)
        {
            object v = r[name];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v);
        }

        private static bool Bool(IDataRecord r, string name)
        {
            object v = r[name];
            return v != DBNull.Value && Convert.ToBoolean(v);
        }

        private static string NullIfEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        private static string Trunc(string s, int max)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            s = s.Trim();
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private static string EscapeLike(string value)
        {
            return (value ?? string.Empty)
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_")
                .Replace("[", @"\[");
        }
    }
}
