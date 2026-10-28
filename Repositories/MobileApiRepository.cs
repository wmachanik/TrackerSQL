using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
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

        /// <summary>Orders delivered by a "Collect (shop)" person: collected by the client at the depot.</summary>
        private static string CollectionsOnlySql()
        {
            return "o.ToBeDeliveredByID IN (SELECT cp.PersonID FROM PeopleTbl cp WHERE cp.DeliveryType = '" + DeliveryTypes.Collect + "')";
        }

        /// <summary>Dates that still have open orders (as the web Delivery Sheet), plus today.</summary>
        /// <param name="withCollections">Also count each date's depot collections (people who hand them over).</param>
        public List<MobileDeliveryDate> GetDeliveryDates(DateTime today, int? personId, bool withCollections = false)
        {
            string sql = @"
                SELECT CAST(o.RequiredByDate AS DATE) AS DeliveryDate,
                       COUNT(*) AS Orders,
                       SUM(CASE WHEN o.Done = 0 THEN 1 ELSE 0 END) AS OpenOrders,
                       SUM(CASE WHEN o.ToBeDeliveredByID = @PersonID THEN 1 ELSE 0 END) AS Mine,
                       {COLLECTIONS} AS Collections
                FROM OrdersTbl o{BY_JOIN}
                WHERE o.RequiredByDate IS NOT NULL
                  AND EXISTS (SELECT 1 FROM OrderLinesTbl ol WHERE ol.OrderID = o.OrderID)
                  AND (CAST(o.RequiredByDate AS DATE) = @Today
                       OR CAST(o.RequiredByDate AS DATE) IN (
                           SELECT DISTINCT CAST(x.RequiredByDate AS DATE) FROM OrdersTbl x
                           WHERE x.Done = 0 AND x.RequiredByDate IS NOT NULL
                             AND EXISTS (SELECT 1 FROM OrderLinesTbl xl WHERE xl.OrderID = x.OrderID)))
                GROUP BY CAST(o.RequiredByDate AS DATE)
                ORDER BY DeliveryDate";
            // A subquery is not allowed inside SUM, so the Delivered by person is joined.
            bool collections = withCollections && PersonsRepository.HasDeliveryTypeColumn();
            sql = sql.Replace("{BY_JOIN}", collections ? "\n                LEFT JOIN PeopleTbl bp ON bp.PersonID = o.ToBeDeliveredByID" : string.Empty)
                     .Replace("{COLLECTIONS}", collections
                         ? "SUM(CASE WHEN bp.DeliveryType = '" + DeliveryTypes.Collect + "' THEN 1 ELSE 0 END)"
                         : "0");
            return Query(sql, r => new MobileDeliveryDate
            {
                Date = Convert.ToDateTime(r["DeliveryDate"]).ToString("yyyy-MM-dd"),
                Orders = Convert.ToInt32(r["Orders"]),
                Open = Convert.ToInt32(r["OpenOrders"]),
                Mine = personId.HasValue ? Convert.ToInt32(r["Mine"]) : (int?)null,
                Collections = collections ? Convert.ToInt32(r["Collections"]) : (int?)null
            }, P("@Today", today.Date, DbType.Date), P("@PersonID", personId ?? -1, DbType.Int32));
        }

        public class SheetRow
        {
            public int OrderID;
            public int ContactID;
            public string CompanyName;
            public string FirstName;
            public string LastName;
            public string AltFirstName;
            public string AltLastName;
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
            public bool ProofNotAtClient;
            /// <summary>The contact's geohash, or its area's busiest cell when it has no point yet.</summary>
            public string RouteHash;
            /// <summary>Where drivers really delivered this contact (0 = first stop .. 1 = last) over <see cref="LearnedVisits"/> earlier days.</summary>
            public double? LearnedPos;
            public int LearnedVisits;
            public string PackColour;
            /// <summary>Where the driver put this contact in the app for the day (1 = first), of <see cref="PlanStops"/>.</summary>
            public int? PlanSeq;
            public int PlanStops;
            public double? GpsLat;
            public double? GpsLng;
            public string GpsSource;
            public string GpsStatus;
            /// <summary>The contact's usual delivery person collects or is a courier.</summary>
            public bool CollectsOrCourier;
            /// <summary>The order's Delivered by person is "Collect (shop)": collected by the client at the depot.</summary>
            public bool Collection;
            public string DeliverToPerson;
            public string DeliverToLabel;
            public int? DeliverToAddressId;
            public string DeliverToAddressKind;
        }

        /// <summary>
        /// One row per order line for the date (optionally one driver), with contact and latest proof. Rows are in route order
        /// per driver: geohash (the contact's point or its area's cell), then the old area order for stops with neither.
        /// </summary>
        public List<SheetRow> GetSheetRows(DateTime date, int? personId, int? orderId = null, bool collectionsOnly = false)
        {
            bool deliverTo = DeliveryOnBehalfManager.IsActive;
            string sql = @"
                SELECT o.OrderID, o.ContactID, c.CompanyName, c.ContactFirstName, c.ContactLastName,
                       c.ContactAltFirstName, c.ContactAltLastName, {EFFECTIVE_PHONE} AS PhoneNumber, {EFFECTIVE_CELL} AS CellNumber,
                       {BILLING_ADDR} AS BillingAddress, {PROVINCE_COL} AS StateOrProvince, {POSTAL_COL} AS PostalCode,
                       a.AreaName, o.Notes, o.PurchaseOrder, o.Done, o.Confirmed,
                       CASE WHEN apd.OffDay = 0 THEN apd.DeliveryOrder ELSE 10000 + ISNULL(apd.DeliveryOrder, 9999) END AS DeliveryOrder,
                       {EFFECTIVE_BY} AS ToBeDeliveredByID, p.Abbreviation AS DeliveryBy,
                       ol.OrderLineID, ol.ItemID, i.ItemServiceTypeID,
                       ol.QtyOrdered, i.ItemDesc, ip.ItemPrepDescription AS PackDesc, ip.BGColour AS PackColour,
                       pr.ProofID, pr.Outcome AS ProofOutcome, pr.Reason AS ProofReason, pr.ReceivedByName AS ProofReceivedBy,
                       pr.MissingItems AS ProofMissingItems,
                       pr.DeliveredAt AS ProofDeliveredAt, pr.Status AS ProofStatus, pr.HasSignature AS ProofHasSignature,
                       pr.NotAtClient AS ProofNotAtClient,
                       {GEO_HASH} AS RouteHash, {LEARN_COLS}, {PLAN_COLS}, {GPS_COLS}, {AGENT_COL}, {BY_TYPE} AS DeliveryByType
                       {DOB_EXTRA_COLS}
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol ON ol.OrderID = o.OrderID
                LEFT JOIN ContactsTbl c ON c.ContactID = o.ContactID
                {DOB_JOIN}
                {AREA_JOIN}
                LEFT JOIN PeopleTbl p ON p.PersonID = {EFFECTIVE_BY}
                LEFT JOIN ItemsTbl i ON i.ItemID = ol.ItemID
                LEFT JOIN ItemPackagingsTbl ip ON ip.ItemPackagingID = ol.PackagingID{GEO_JOIN}
                OUTER APPLY (SELECT TOP 1 d.DeliveryOrder,
                                    CASE WHEN ((d.PrepDayOfWeekID - 1 + ISNULL(d.DeliveryDelayDays, 0)) % 7) + 1
                                              = (DATEDIFF(day, '19000107', o.RequiredByDate) % 7) + 1
                                         THEN 0 ELSE 1 END AS OffDay
                             FROM AreaPrepDaysTbl d WHERE d.AreaID = c.AreaID
                             ORDER BY OffDay, d.DeliveryOrder) apd
                OUTER APPLY (SELECT TOP 1 m.ProofID, m.Outcome, m.Reason, m.ReceivedByName, m.MissingItems, m.DeliveredAt, m.Status,
                                    CAST(CASE WHEN m.SignatureImage IS NULL THEN 0 ELSE 1 END AS BIT) AS HasSignature,
                                    {PROOF_AWAY} AS NotAtClient
                             FROM MobileDeliveryProofTbl m WHERE m.OrderID = o.OrderID ORDER BY m.ProofID DESC) pr{LEARN_JOIN}{PLAN_JOIN}
                WHERE CAST(o.RequiredByDate AS DATE) = @Date";
            string effectiveBy = DeliveryOnBehalfManager.EffectiveDeliveredByIdSql();
            sql = sql.Replace("{PROOF_AWAY}", ProofHasNotAtClientColumn() ? "ISNULL(m.NotAtClient, 0)" : "CAST(0 AS BIT)");
            sql = sql.Replace("{EFFECTIVE_BY}", effectiveBy);
            sql = sql.Replace("{BILLING_ADDR}", DeliveryOnBehalfManager.EffectiveBillingAddressSql("c"))
                     .Replace("{PROVINCE_COL}", DeliveryOnBehalfManager.EffectiveProvinceSql("c"))
                     .Replace("{POSTAL_COL}", DeliveryOnBehalfManager.EffectivePostalCodeSql("c"))
                     .Replace("{EFFECTIVE_PHONE}", DeliveryOnBehalfManager.EffectivePhoneSql("c"))
                     .Replace("{EFFECTIVE_CELL}", DeliveryOnBehalfManager.EffectiveCellSql("c"))
                     .Replace("{AREA_JOIN}", DeliveryOnBehalfManager.AreaJoinOnEffectiveArea("c", "a"))
                     .Replace("{DOB_JOIN}", deliverTo ? DeliveryOnBehalfManager.JoinClause : string.Empty)
                     .Replace("{DOB_EXTRA_COLS}", deliverTo
                         ? ", dob.DeliverToPerson, dob.Label AS DeliverToLabel"
                           + (ContactDeliverToAddressRepository.HasAddressKindColumn() ? ", dob.AddressKind AS DeliverToAddressKind" : string.Empty)
                           + (OrdersRepository.HasDeliverToAddressIdColumn() ? ", o.DeliverToAddressID" : string.Empty)
                         : (OrdersRepository.HasDeliverToAddressIdColumn() ? ", o.DeliverToAddressID" : string.Empty));
            bool plan = DeliveryRouteRepository.TableSeen();
            sql = sql.Replace("{PLAN_JOIN}", plan ? DeliveryRouteRepository.PlanJoinSql("c", "CAST(o.RequiredByDate AS DATE)", effectiveBy) : string.Empty)
                     .Replace("{PLAN_COLS}", plan ? "dr.PlanSeq, dr.PlanStops" : "CAST(NULL AS INT) AS PlanSeq, 0 AS PlanStops");
            bool geo = CustomerLocationRepository.TableSeen();
            string routeHashExpr = geo ? CustomerLocationRepository.RouteHashSql : "CAST(NULL AS VARCHAR(12))";
            if (geo && deliverTo)
                routeHashExpr = DeliveryOnBehalfManager.EffectiveRouteHashSql(CustomerLocationRepository.RouteHashSql);
            sql = sql.Replace("{GEO_JOIN}", geo ? CustomerLocationRepository.RouteJoinSql("c") : string.Empty)
                     .Replace("{GEO_HASH}", routeHashExpr)
                     .Replace("{GPS_COLS}", geo
                         ? (deliverTo
                             ? DeliveryOnBehalfManager.EffectiveGpsLatSql() + " AS GpsLat, "
                               + DeliveryOnBehalfManager.EffectiveGpsLngSql() + " AS GpsLng, gl.Source AS GpsSource, gl.LocationStatus AS GpsStatus"
                             : "gl.Latitude AS GpsLat, gl.Longitude AS GpsLng, gl.Source AS GpsSource, gl.LocationStatus AS GpsStatus")
                         : "CAST(NULL AS FLOAT) AS GpsLat, CAST(NULL AS FLOAT) AS GpsLng, CAST(NULL AS VARCHAR(20)) AS GpsSource, CAST(NULL AS VARCHAR(20)) AS GpsStatus");
            sql = sql.Replace("{AGENT_COL}", PersonsRepository.HasDeliveryTypeColumn()
                ? "(SELECT TOP 1 ag.DeliveryType FROM PeopleTbl ag WHERE ag.PersonID = c.PreferredAgentID) AS AgentDeliveryType"
                : "CAST(NULL AS VARCHAR(20)) AS AgentDeliveryType")
                .Replace("{BY_TYPE}", PersonsRepository.HasDeliveryTypeColumn() ? "p.DeliveryType" : "CAST(NULL AS VARCHAR(10))");
            bool learn = DeliveryVisitRepository.TableSeen();
            sql = sql.Replace("{LEARN_JOIN}", learn ? DeliveryVisitRepository.LearnedJoinSql("c", "CAST(o.RequiredByDate AS DATE)", effectiveBy) : string.Empty)
                     .Replace("{LEARN_COLS}", learn ? "dv.LearnedPos, dv.LearnedVisits" : "CAST(NULL AS FLOAT) AS LearnedPos, 0 AS LearnedVisits");
            var ps = new List<DBParameter> { P("@Date", date.Date, DbType.Date) };
            if (personId.HasValue)
            {
                sql += " AND " + effectiveBy + " = @PersonID";
                ps.Add(P("@PersonID", personId.Value, DbType.Int32));
            }
            if (collectionsOnly && PersonsRepository.HasDeliveryTypeColumn())
                sql += " AND " + CollectionsOnlySql();
            if (orderId.HasValue)
            {
                sql += " AND o.OrderID = @OrderID";
                ps.Add(P("@OrderID", orderId.Value, DbType.Int32));
            }
            // Delivered stops keep their place so the route numbers do not shift during the day.
            sql += " ORDER BY " + effectiveBy + ","
                 + (geo ? " CASE WHEN " + routeHashExpr + " IS NULL THEN 1 ELSE 0 END, "
                        + routeHashExpr + "," : string.Empty)
                 + " CASE WHEN apd.OffDay = 0 THEN apd.DeliveryOrder ELSE 10000 + ISNULL(apd.DeliveryOrder, 9999) END,"
                 + " a.AreaName, c.CompanyName, o.OrderID, i.SortOrder, ol.OrderLineID";

            return Query(sql, r => new SheetRow
            {
                OrderID = Convert.ToInt32(r["OrderID"]),
                ContactID = NInt(r, "ContactID") ?? 0,
                CompanyName = Str(r, "CompanyName"),
                FirstName = Str(r, "ContactFirstName"),
                LastName = Str(r, "ContactLastName"),
                AltFirstName = Str(r, "ContactAltFirstName"),
                AltLastName = Str(r, "ContactAltLastName"),
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
                ProofHasSignature = Bool(r, "ProofHasSignature"),
                ProofNotAtClient = Bool(r, "ProofNotAtClient"),
                RouteHash = Str(r, "RouteHash"),
                LearnedPos = r["LearnedPos"] == DBNull.Value ? (double?)null : Convert.ToDouble(r["LearnedPos"]),
                LearnedVisits = NInt(r, "LearnedVisits") ?? 0,
                PackColour = Str(r, "PackColour"),
                PlanSeq = NInt(r, "PlanSeq"),
                PlanStops = NInt(r, "PlanStops") ?? 0,
                GpsLat = r["GpsLat"] == DBNull.Value ? (double?)null : Convert.ToDouble(r["GpsLat"]),
                GpsLng = r["GpsLng"] == DBNull.Value ? (double?)null : Convert.ToDouble(r["GpsLng"]),
                GpsSource = Str(r, "GpsSource"),
                GpsStatus = Str(r, "GpsStatus"),
                CollectsOrCourier = Str(r, "AgentDeliveryType") == DeliveryTypes.Courier || Str(r, "AgentDeliveryType") == DeliveryTypes.Collect,
                Collection = Str(r, "DeliveryByType") == DeliveryTypes.Collect,
                DeliverToPerson = deliverTo ? Str(r, "DeliverToPerson") : null,
                DeliverToLabel = deliverTo ? Str(r, "DeliverToLabel") : null,
                DeliverToAddressId = DbMapper.HasColumn(r, "DeliverToAddressID") ? NInt(r, "DeliverToAddressID") : null,
                DeliverToAddressKind = DbMapper.HasColumn(r, "DeliverToAddressKind") ? Str(r, "DeliverToAddressKind") : null
            }, ps.ToArray());
        }

        /// <summary>Repairs whose related order line is on an order due on the date (optionally one driver), keyed by OrderID.</summary>
        public List<KeyValuePair<int, MobileStopRepair>> GetSheetRepairs(DateTime date, int? personId, bool collectionsOnly = false)
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
            if (collectionsOnly && PersonsRepository.HasDeliveryTypeColumn())
                sql += " AND " + CollectionsOnlySql();
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

        /// <summary>
        /// Sets the contact's alternative person when both its names are blank and the name is not the main contact's.
        /// Returns false when nothing was changed.
        /// </summary>
        public bool SetAltContactIfBlank(int contactId, string name, string firstName, string lastName)
        {
            const string sql = @"
                UPDATE ContactsTbl SET ContactAltFirstName = @First, ContactAltLastName = @Last
                WHERE ContactID = @ContactID
                  AND LTRIM(RTRIM(ISNULL(ContactAltFirstName, N'') + ISNULL(ContactAltLastName, N''))) = N''
                  AND LTRIM(RTRIM(ISNULL(ContactFirstName, N'') + N' ' + ISNULL(ContactLastName, N''))) <> @Name";
            return Exec(sql,
                P("@ContactID", contactId, DbType.Int32),
                P("@Name", name, DbType.String),
                P("@First", firstName, DbType.String),
                P("@Last", (object)lastName ?? DBNull.Value, DbType.String)) > 0;
        }

        // ---------------- Delivery proof ----------------

        private static bool _proofAccuracySeen;

        /// <summary>Whether the DriverGps-02 pack has added MobileDeliveryProofTbl.AccuracyM (only a yes is cached).</summary>
        private bool ProofHasAccuracyColumn()
        {
            if (!_proofAccuracySeen)
            {
                try
                {
                    _proofAccuracySeen = Scalar<int>(
                        "SELECT CASE WHEN COL_LENGTH('dbo.MobileDeliveryProofTbl', 'AccuracyM') IS NULL THEN 0 ELSE 1 END") == 1;
                }
                catch
                {
                    _proofAccuracySeen = false;
                }
            }
            return _proofAccuracySeen;
        }

        private static bool _proofNotAtClientSeen;
        private static bool _optionNotAtClientSeen;

        /// <summary>Whether the DriverGps-04 pack has added MobileDeliveryProofTbl.NotAtClient (only a yes is cached).</summary>
        private bool ProofHasNotAtClientColumn()
        {
            if (!_proofNotAtClientSeen)
                _proofNotAtClientSeen = HasColumn("MobileDeliveryProofTbl", "NotAtClient");
            return _proofNotAtClientSeen;
        }

        /// <summary>Whether the DriverGps-04 pack has added MobileDeliveryOptionsTbl.NotAtClient (only a yes is cached).</summary>
        private bool OptionHasNotAtClientColumn()
        {
            if (!_optionNotAtClientSeen)
                _optionNotAtClientSeen = HasColumn("MobileDeliveryOptionsTbl", "NotAtClient");
            return _optionNotAtClientSeen;
        }

        private static bool _proofPodSeen;

        /// <summary>Whether the DriverGps-05 pack has added TermsAccepted, TermsText, HandedOver, PodSentAt and PodSentTo (only a yes is cached).</summary>
        public bool ProofHasPodColumns()
        {
            if (!_proofPodSeen)
                _proofPodSeen = HasColumn("MobileDeliveryProofTbl", "TermsAccepted")
                    && HasColumn("MobileDeliveryProofTbl", "HandedOver")
                    && HasColumn("MobileDeliveryProofTbl", "PodSentAt");
            return _proofPodSeen;
        }

        private bool HasColumn(string table, string column)
        {
            try
            {
                return Scalar<int>("SELECT CASE WHEN COL_LENGTH('dbo." + table + "', '" + column + "') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                return false;
            }
        }

        public long InsertProof(MobileDeliveryProof proof, byte[] signature)
        {
            bool accuracy = ProofHasAccuracyColumn();
            bool away = ProofHasNotAtClientColumn();
            bool pod = ProofHasPodColumns();
            string sql = @"
                INSERT INTO MobileDeliveryProofTbl
                    (ClientRef, OrderID, ContactID, PersonID, UserName, Outcome, Reason, ReceivedByName, SignatureImage, SignatureContentType,
                     DriverNote, MissingItems, DeliveredAt, Latitude, Longitude, " + (accuracy ? "AccuracyM, " : "") + (away ? "NotAtClient, " : "")
                     + (pod ? "TermsAccepted, TermsText, HandedOver, " : "") + @"Status, StatusMessage, ReceivedAt)
                VALUES
                    (@ClientRef, @OrderID, @ContactID, @PersonID, @UserName, @Outcome, @Reason, @ReceivedByName, @SignatureImage, @SignatureContentType,
                     @DriverNote, @MissingItems, @DeliveredAt, @Latitude, @Longitude, " + (accuracy ? "@AccuracyM, " : "") + (away ? "@NotAtClient, " : "")
                     + (pod ? "@TermsAccepted, @TermsText, @HandedOver, " : "") + @"@Status, @StatusMessage, @ReceivedAt);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
            return Scalar<long>(sql,
                P("@AccuracyM", proof.AccuracyM, DbType.Decimal),
                P("@NotAtClient", proof.NotAtClient, DbType.Boolean),
                P("@TermsAccepted", proof.TermsAccepted, DbType.Boolean),
                P("@TermsText", Trunc(proof.TermsText, 1000)),
                P("@HandedOver", Trunc(proof.HandedOver, 2000)),
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

        private string ProofColumns => @"
            m.ProofID, m.ClientRef, m.OrderID, m.ContactID, m.PersonID, m.UserName, m.Outcome, m.Reason, m.ReceivedByName,
            m.SignatureContentType, CAST(CASE WHEN m.SignatureImage IS NULL THEN 0 ELSE 1 END AS BIT) AS HasSignature,
            m.DriverNote, m.MissingItems, m.DeliveredAt, m.Latitude, m.Longitude, m.Status, m.StatusMessage, m.ReceivedAt,
            m.CompletedAt, m.CompletedBy, c.CompanyName, "
            + (ProofHasAccuracyColumn() ? "m.AccuracyM" : "CAST(NULL AS DECIMAL(9,1)) AS AccuracyM") + ", "
            + (ProofHasNotAtClientColumn() ? "ISNULL(m.NotAtClient, 0) AS NotAtClient" : "CAST(0 AS BIT) AS NotAtClient") + ", "
            + (ProofHasPodColumns()
                ? "m.TermsAccepted, m.TermsText, m.HandedOver, m.PodSentAt, m.PodSentTo"
                : "CAST(NULL AS BIT) AS TermsAccepted, CAST(NULL AS NVARCHAR(1)) AS TermsText, CAST(NULL AS NVARCHAR(1)) AS HandedOver, "
                  + "CAST(NULL AS DATETIME) AS PodSentAt, CAST(NULL AS NVARCHAR(1)) AS PodSentTo");

        /// <summary>The POD PDF was emailed (Contact Details or the delivery sync).</summary>
        public void MarkPodSent(long proofId, string sentTo)
        {
            if (!ProofHasPodColumns())
                return;
            Exec("UPDATE MobileDeliveryProofTbl SET PodSentAt = @Now, PodSentTo = @To WHERE ProofID = @ProofID",
                P("@Now", TimeZoneUtils.Now(), DbType.DateTime), P("@To", Trunc(sentTo, 200)), P("@ProofID", proofId, DbType.Int64));
        }

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

        /// <summary>The oldest proof waiting for the background Order Done (status Pending), or null.</summary>
        public MobileDeliveryProof NextPendingProof()
        {
            var list = Query("SELECT TOP (1) " + ProofColumns + @" FROM MobileDeliveryProofTbl m
                              LEFT JOIN ContactsTbl c ON c.ContactID = m.ContactID
                              WHERE m.Status = 'Pending' ORDER BY m.ProofID",
                MapProof);
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// Driver deliveries whose order is still not done in Tracker (received, waiting or failed), newest proof per
        /// order, oldest delivery first. With the driver's name in UserName.
        /// </summary>
        public List<MobileDeliveryProof> ListAwaitingDone(int max = 200)
        {
            string sql = "SELECT TOP (" + Math.Max(1, Math.Min(max, 1000)) + ") * FROM (SELECT " + ProofColumns + @",
                    COALESCE(p.Person, m.UserName) AS Driver,
                    ROW_NUMBER() OVER (PARTITION BY m.OrderID ORDER BY m.ProofID DESC) AS Nth
                FROM MobileDeliveryProofTbl m
                JOIN OrdersTbl o ON o.OrderID = m.OrderID
                LEFT JOIN ContactsTbl c ON c.ContactID = COALESCE(m.ContactID, o.ContactID)
                LEFT JOIN PeopleTbl p ON p.PersonID = m.PersonID
                WHERE m.Outcome = 'Delivered' AND ISNULL(o.Done, 0) = 0
                  AND m.Status IN ('Received', 'Pending', 'CompleteFailed')) x
                WHERE x.Nth = 1
                ORDER BY x.DeliveredAt, x.ProofID";
            return Query(sql, r =>
            {
                var proof = MapProof(r);
                proof.UserName = Str(r, "Driver");
                return proof;
            });
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

        /// <summary>Contact Details: the contact's proofs of delivery, newest first, with signatures.</summary>
        public List<ContactProofRow> ListProofsForContact(int contactId, int max = 100)
        {
            string sql = "SELECT TOP (" + Math.Max(1, Math.Min(max, 500)) + @")
                    m.ProofID, m.OrderID, m.DeliveredAt, m.Outcome, m.Reason, m.ReceivedByName, m.DriverNote, m.MissingItems,
                    m.Status, m.Latitude, m.Longitude, " + (ProofHasAccuracyColumn() ? "m.AccuracyM" : "CAST(NULL AS DECIMAL(9,1)) AS AccuracyM") + @",
                    " + (ProofHasNotAtClientColumn() ? "m.NotAtClient" : "CAST(0 AS BIT) AS NotAtClient") + @",
                    " + (ProofHasPodColumns() ? "m.TermsAccepted, m.PodSentAt, m.PodSentTo"
                        : "CAST(NULL AS BIT) AS TermsAccepted, CAST(NULL AS DATETIME) AS PodSentAt, CAST(NULL AS NVARCHAR(1)) AS PodSentTo") + @",
                    m.SignatureImage, m.SignatureContentType, o.Notes AS OrderNotes,
                    COALESCE(p.Person, m.UserName) AS Driver
                FROM MobileDeliveryProofTbl m
                LEFT JOIN OrdersTbl o ON o.OrderID = m.OrderID
                LEFT JOIN PeopleTbl p ON p.PersonID = m.PersonID
                WHERE m.ContactID = @ContactID OR (m.ContactID IS NULL AND o.ContactID = @ContactID)
                ORDER BY m.DeliveredAt DESC, m.ProofID DESC";
            return Query(sql, r => new ContactProofRow
            {
                ProofID = Convert.ToInt64(r["ProofID"]),
                OrderID = Convert.ToInt32(r["OrderID"]),
                DeliveredAt = Convert.ToDateTime(r["DeliveredAt"]),
                Outcome = Str(r, "Outcome"),
                Reason = Str(r, "Reason"),
                ReceivedByName = Str(r, "ReceivedByName"),
                DriverNote = Str(r, "DriverNote"),
                MissingItems = Str(r, "MissingItems"),
                Status = Str(r, "Status"),
                Driver = Str(r, "Driver"),
                Latitude = r["Latitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Latitude"]),
                Longitude = r["Longitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Longitude"]),
                AccuracyM = r["AccuracyM"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["AccuracyM"]),
                NotAtClient = Bool(r, "NotAtClient"),
                TermsAccepted = r["TermsAccepted"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["TermsAccepted"]),
                PodSentAt = NDate(r, "PodSentAt"),
                PodSentTo = NullIfEmpty(Str(r, "PodSentTo")),
                OrderNotes = Str(r, "OrderNotes"),
                SignatureImage = r["SignatureImage"] == DBNull.Value ? null : (byte[])r["SignatureImage"],
                SignatureContentType = Str(r, "SignatureContentType")
            }, P("@ContactID", contactId, DbType.Int32));
        }

        /// <summary>Contact Portal: the contact's delivered proofs (no signatures), newest first.</summary>
        public List<ContactProofRow> ListDeliveredProofsForContact(int contactId, int max = 500)
        {
            string sql = "SELECT TOP (" + Math.Max(1, Math.Min(max, 2000)) + @")
                    m.ProofID, m.OrderID, m.DeliveredAt, m.Outcome, m.Reason, m.ReceivedByName
                FROM MobileDeliveryProofTbl m
                LEFT JOIN OrdersTbl o ON o.OrderID = m.OrderID
                WHERE (m.ContactID = @ContactID OR (m.ContactID IS NULL AND o.ContactID = @ContactID))
                  AND ISNULL(m.Outcome, '') <> 'NotDelivered'
                ORDER BY m.DeliveredAt DESC, m.ProofID DESC";
            return Query(sql, r => new ContactProofRow
            {
                ProofID = Convert.ToInt64(r["ProofID"]),
                OrderID = Convert.ToInt32(r["OrderID"]),
                DeliveredAt = Convert.ToDateTime(r["DeliveredAt"]),
                Outcome = Str(r, "Outcome"),
                Reason = Str(r, "Reason"),
                ReceivedByName = Str(r, "ReceivedByName")
            }, P("@ContactID", contactId, DbType.Int32));
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
                CompanyName = Str(r, "CompanyName"),
                AccuracyM = r["AccuracyM"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["AccuracyM"]),
                NotAtClient = Bool(r, "NotAtClient"),
                TermsAccepted = r["TermsAccepted"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["TermsAccepted"]),
                TermsText = NullIfEmpty(Str(r, "TermsText")),
                HandedOver = NullIfEmpty(Str(r, "HandedOver")),
                PodSentAt = NDate(r, "PodSentAt"),
                PodSentTo = NullIfEmpty(Str(r, "PodSentTo"))
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

        // ---------------- Delivery options (Lookups > Delivery Outcomes) ----------------

        public List<MobileDeliveryOption> GetDeliveryOptions(bool enabledOnly)
        {
            return Query(@"SELECT OptionID, Outcome, OptionText, NeedsName, SortOrder, IsEnabled, "
                           + (OptionHasNotAtClientColumn() ? "NotAtClient" : "CAST(0 AS BIT) AS NotAtClient") + @"
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
                    Enabled = Bool(r, "IsEnabled"),
                    NotAtClient = Bool(r, "NotAtClient")
                });
        }

        public void InsertDeliveryOption(MobileDeliveryOption option)
        {
            bool away = OptionHasNotAtClientColumn();
            Exec(@"INSERT INTO MobileDeliveryOptionsTbl (Outcome, OptionText, NeedsName, SortOrder, IsEnabled" + (away ? ", NotAtClient" : "") + @")
                   VALUES (@Outcome, @Text, @NeedsName, @SortOrder, @Enabled" + (away ? ", @NotAtClient" : "") + ")",
                P("@Outcome", option.Outcome), P("@Text", Trunc(option.Text, 100)),
                P("@NeedsName", option.NeedsName, DbType.Boolean), P("@SortOrder", option.SortOrder, DbType.Int32),
                P("@Enabled", option.Enabled, DbType.Boolean), P("@NotAtClient", option.NotAtClient, DbType.Boolean));
        }

        public void UpdateDeliveryOption(MobileDeliveryOption option)
        {
            Exec(@"UPDATE MobileDeliveryOptionsTbl
                   SET Outcome = @Outcome, OptionText = @Text, NeedsName = @NeedsName, SortOrder = @SortOrder, IsEnabled = @Enabled"
                   + (OptionHasNotAtClientColumn() ? ", NotAtClient = @NotAtClient" : "") + @"
                   WHERE OptionID = @Id",
                P("@Outcome", option.Outcome), P("@Text", Trunc(option.Text, 100)),
                P("@NeedsName", option.NeedsName, DbType.Boolean), P("@SortOrder", option.SortOrder, DbType.Int32),
                P("@Enabled", option.Enabled, DbType.Boolean), P("@NotAtClient", option.NotAtClient, DbType.Boolean),
                P("@Id", option.Id, DbType.Int32));
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

        /// <summary>Alternative delivery addresses on orders due on the date (for the driver app offline cache).</summary>
        public List<MobileDeliverToSite> ListDeliverToSitesForDate(DateTime date, int? personId)
        {
            if (!DeliveryOnBehalfManager.IsActive || !OrdersRepository.HasDeliverToAddressIdColumn())
                return new List<MobileDeliverToSite>();

            string effectiveBy = DeliveryOnBehalfManager.EffectiveDeliveredByIdSql();
            string phoneCols = ContactDeliverToAddressRepository.HasPhoneColumns()
                ? @"COALESCE(NULLIF(LTRIM(RTRIM(dob.CellNumber)), N''), NULLIF(LTRIM(RTRIM(dob.PhoneNumber)), N'')) AS CellNumber"
                : "CAST(NULL AS NVARCHAR(30)) AS CellNumber";
            string kindCol = ContactDeliverToAddressRepository.HasAddressKindColumn()
                ? "dob.AddressKind"
                : "CAST(NULL AS NVARCHAR(20)) AS AddressKind";
            string sql = @"
SELECT DISTINCT dob.DeliverToAddressID, o.ContactID, c.CompanyName, dob.Label, dob.DeliverToPerson, "
                + kindCol + @" AS AddressKind, " + phoneCols + @",
    LTRIM(RTRIM(
        ISNULL(dob.Address1, N'')
        + CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(dob.Address2, N''))), N'') IS NOT NULL THEN N', ' + dob.Address2 ELSE N'' END
        + CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(dob.Address3, N''))), N'') IS NOT NULL THEN N', ' + dob.Address3 ELSE N'' END
    )) AS AddressLine,
    dob.PostalCode, a.AreaName
FROM OrdersTbl o
INNER JOIN ContactDeliverToAddressTbl dob ON dob.DeliverToAddressID = o.DeliverToAddressID AND dob.Enabled = 1
LEFT JOIN ContactsTbl c ON c.ContactID = o.ContactID
LEFT JOIN AreasTbl a ON a.AreaID = COALESCE(dob.AreaID, c.AreaID)
WHERE CAST(o.RequiredByDate AS DATE) = @Date AND o.DeliverToAddressID IS NOT NULL";
            var ps = new List<DBParameter> { P("@Date", date.Date, DbType.Date) };
            if (personId.HasValue)
            {
                sql += " AND " + effectiveBy + " = @PersonID";
                ps.Add(P("@PersonID", personId.Value, DbType.Int32));
            }

            return Query(sql, r => new MobileDeliverToSite
            {
                Id = Convert.ToInt32(r["DeliverToAddressID"]),
                ContactId = Convert.ToInt32(r["ContactID"]),
                AccountCompany = Str(r, "CompanyName"),
                Label = Str(r, "Label"),
                DeliverToPerson = NullIfEmpty(Str(r, "DeliverToPerson")),
                AddressKind = AlternativeAddressKind.Display(Str(r, "AddressKind")),
                Cell = NullIfEmpty(Str(r, "CellNumber")),
                Phone = null,
                Address = NullIfEmpty(Str(r, "AddressLine")),
                PostalCode = NullIfEmpty(Str(r, "PostalCode")),
                Area = NullIfEmpty(Str(r, "AreaName"))
            }, ps.ToArray());
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
