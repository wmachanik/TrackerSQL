using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// ZZName (contact 9) guest deliveries use alternative addresses, not the shared contact pin.
    /// Walk-in collect-at-depot is a different case (collection driver).
    /// </summary>
    public static class ZzNameDeliverTo
    {
        public const string NewAddressValue = "__new__";
        public const string StandardValue = "0";

        public static bool IsZzName(long contactId) =>
            contactId == SystemConstants.CustomerConstants.SundryCustomerID;

        /// <summary>
        /// Order Detail Recipient row: ZZName always, otherwise only when Contact Details
        /// has "This contact has alternative delivery addresses" checked.
        /// </summary>
        public static bool ShowChoice(int contactId)
        {
            if (contactId <= 0 || !ContactDeliverToAddressRepository.TableSeen())
                return false;
            if (IsZzName(contactId))
                return true;
            if (!ContactsRepository.HasDeliveryOnBehalfColumn())
                return false;
            Contact contact = new ContactsRepository().GetById(contactId);
            return contact != null && (contact.DeliveryOnBehalf ?? false);
        }

        public static string NormalizeKey(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            string t = text.Trim().ToLowerInvariant();
            t = Regex.Replace(t, @"[^a-z0-9]+", " ");
            return Regex.Replace(t, @"\s+", " ").Trim();
        }

        public static ContactDeliverToAddress FindMatch(int contactId, string personName, string address)
        {
            string nameKey = NormalizeKey(personName);
            string addrKey = NormalizeKey(address);
            if (contactId <= 0 || (nameKey.Length == 0 && addrKey.Length == 0))
                return null;

            foreach (ContactDeliverToAddress row in new ContactDeliverToAddressRepository().ListForContact(contactId, enabledOnly: true))
            {
                bool nameOk = nameKey.Length == 0
                    || NormalizeKey(row.DeliverToPerson) == nameKey
                    || NormalizeKey(row.Label) == nameKey;
                bool addrOk = addrKey.Length == 0
                    || NormalizeKey(row.AddressSummary) == addrKey
                    || NormalizeKey((row.Address1 ?? string.Empty) + " " + (row.Address2 ?? string.Empty)) == addrKey;
                if (nameOk && addrOk)
                    return row;
                if (addrKey.Length > 8 && addrOk)
                    return row;
            }
            return null;
        }

        public static int CountOrders(int deliverToAddressId)
        {
            if (deliverToAddressId <= 0)
                return 0;
            using (var db = new TrackerSQLDb())
            {
                object n = db.ExecuteScalar(
                    "SELECT COUNT(*) FROM dbo.OrdersTbl WHERE DeliverToAddressID = @Id",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@Id", DataValue = deliverToAddressId, DataDbType = System.Data.DbType.Int32 }
                    });
                return n == null || n == DBNull.Value ? 0 : Convert.ToInt32(n);
            }
        }

        public static void TouchLastUsed(int deliverToAddressId)
        {
            if (deliverToAddressId <= 0 || !ContactDeliverToAddressRepository.HasUseDatesColumn())
                return;
            using (var db = new TrackerSQLDb())
            {
                db.ExecuteNonQuery(
                    "UPDATE dbo.ContactDeliverToAddressTbl SET LastUsedDate = GETDATE() WHERE DeliverToAddressID = @Id",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@Id", DataValue = deliverToAddressId, DataDbType = System.Data.DbType.Int32 }
                    });
            }
        }

        public static ContactDeliverToAddress BuildFromGuest(
            int contactId,
            string personName,
            string addressLine,
            string cell,
            int? areaId,
            int? deliveredById,
            string postalCode,
            string province)
        {
            string name = (personName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
                name = "Guest";
            var parts = (addressLine ?? string.Empty).Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
            return new ContactDeliverToAddress
            {
                ContactID = contactId,
                Label = name,
                DeliverToPerson = name,
                CellNumber = string.IsNullOrWhiteSpace(cell) ? null : cell.Trim(),
                AddressKind = AlternativeAddressKind.Sundry,
                Address1 = parts.Count > 0 ? parts[0] : addressLine,
                Address2 = parts.Count > 1 ? parts[1] : null,
                Address3 = parts.Count > 2 ? string.Join(", ", parts.Skip(2)) : null,
                AreaID = areaId,
                PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim(),
                StateOrProvince = string.IsNullOrWhiteSpace(province) ? null : province.Trim(),
                ToBeDeliveredByID = deliveredById,
                Enabled = true
            };
        }

        public static void TryGeocode(ContactDeliverToAddress row, string userName)
        {
            if (row == null || row.Latitude.HasValue)
                return;
            string address = row.AddressSummary;
            if (string.IsNullOrWhiteSpace(address))
                return;
            try
            {
                GeocodeResult result = new CustomerLocationManager().SearchAddress(address);
                if (!result.Success)
                    return;
                row.Latitude = Math.Round((decimal)result.Latitude, 6);
                row.Longitude = Math.Round((decimal)result.Longitude, 6);
                row.Source = CustomerLocation.SourceGeocoder;
                row.LocationStatus = CustomerLocation.StatusApproximate;
                row.SourceAddress = address;
                row.GeocodedAddress = result.Label;
                row.LocationUpdatedBy = userName;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "ZZName alt GPS: " + ex.Message, userName);
            }
        }

        public static int Save(ContactDeliverToAddress row, string userName)
        {
            if (row == null)
                return 0;
            TryGeocode(row, userName);
            var repo = new ContactDeliverToAddressRepository();
            if (row.DeliverToAddressID > 0)
            {
                repo.Update(row);
                if (row.Latitude.HasValue)
                    repo.UpdateGps(row);
                return row.DeliverToAddressID;
            }
            int id = repo.Insert(row);
            row.DeliverToAddressID = id;
            if (id > 0 && row.Latitude.HasValue)
                repo.UpdateGps(row);
            return id;
        }

        public static int PromoteToContact(int deliverToAddressId, string userName, out string error)
        {
            error = null;
            var repo = new ContactDeliverToAddressRepository();
            ContactDeliverToAddress alt = repo.GetById(deliverToAddressId);
            if (alt == null || !IsZzName(alt.ContactID))
            {
                error = "This address is not a ZZName alternative.";
                return 0;
            }

            string full = (alt.DeliverToPerson ?? alt.Label ?? string.Empty).Trim();
            if (full.Length == 0)
            {
                error = "The alternative address has no person name.";
                return 0;
            }

            string first = full;
            string last = string.Empty;
            int space = full.IndexOf(' ');
            if (space > 0)
            {
                first = full.Substring(0, space).Trim();
                last = full.Substring(space + 1).Trim();
            }

            var contact = new Contact
            {
                CompanyName = full,
                ContactFirstName = first,
                ContactLastName = last,
                BillingAddress = alt.AddressSummary,
                AreaID = alt.AreaID,
                StateOrProvince = alt.StateOrProvince,
                PostalCode = alt.PostalCode,
                CellNumber = alt.CellNumber,
                PhoneNumber = alt.PhoneNumber,
                PreferredAgentID = alt.ToBeDeliveredByID,
                Enabled = true,
                DeliveryOnBehalf = false
            };

            int newId = new ContactsRepository().Insert(contact);
            if (newId <= 0)
            {
                error = "Could not create the contact.";
                return 0;
            }

            using (var db = new TrackerSQLDb())
            {
                db.ExecuteNonQuery(
                    "UPDATE dbo.OrdersTbl SET ContactID = @NewId, DeliverToAddressID = NULL WHERE DeliverToAddressID = @AltId",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@NewId", DataValue = newId, DataDbType = System.Data.DbType.Int32 },
                        new DBParameter { ParamName = "@AltId", DataValue = deliverToAddressId, DataDbType = System.Data.DbType.Int32 }
                    });
            }

            int usageLines = ImportTrackedUsageFromAdHocOrders(newId);
            RecalcPredictionFromImportedUsage(newId);

            alt.Enabled = false;
            repo.Update(alt);
            ContactChangeLogManager.LogSummary(
                newId,
                ContactChangeLogManager.SourceContactDetails,
                "Created from ZZName alternative address " + deliverToAddressId.ToString(CultureInfo.InvariantCulture)
                    + (usageLines > 0 ? "; imported " + usageLines.ToString(CultureInfo.InvariantCulture) + " usage line(s) from Ad hoc orders." : "."),
                userName);
            return newId;
        }

        /// <summary>
        /// Ad hoc ZZName orders skip usage on Order Done so contact 9 is not predicted.
        /// When the guest becomes a real contact, copy tracked-item lines from those done orders.
        /// </summary>
        private static int ImportTrackedUsageFromAdHocOrders(int contactId)
        {
            if (contactId <= 0)
                return 0;

            HashSet<int> trackedTypes = TrackedServiceTypeIds();
            if (trackedTypes.Count == 0)
                return 0;

            const string sql = @"
SELECT o.OrderID,
       CAST(ISNULL(o.RequiredByDate, o.OrderDate) AS DATE) AS DeliveryDate,
       ol.ItemID,
       ol.QtyOrdered,
       ol.PackagingID,
       i.ItemServiceTypeID,
       ISNULL(i.UnitsPerQty, 1) AS UnitsPerQty
FROM dbo.OrdersTbl o
INNER JOIN dbo.OrderLinesTbl ol ON ol.OrderID = o.OrderID
INNER JOIN dbo.ItemsTbl i ON i.ItemID = ol.ItemID
WHERE o.ContactID = @ContactID
  AND o.Done = 1
  AND i.ItemServiceTypeID IS NOT NULL
  AND i.ItemServiceTypeID <> @NotApplicable
ORDER BY o.RequiredByDate, o.OrderID, i.ItemServiceTypeID, ol.OrderLineID";

            var rows = new List<ImportedUsageRow>();
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = System.Data.DbType.Int32 },
                new DBParameter { ParamName = "@NotApplicable", DataValue = SystemConstants.ServiceTypeConstants.NotApplicable, DataDbType = System.Data.DbType.Int32 }
            }))
            {
                while (rdr != null && rdr.Read())
                {
                    int serviceType = rdr["ItemServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemServiceTypeID"]);
                    if (!trackedTypes.Contains(serviceType))
                        continue;
                    rows.Add(new ImportedUsageRow
                    {
                        OrderID = Convert.ToInt32(rdr["OrderID"]),
                        DeliveryDate = rdr["DeliveryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["DeliveryDate"]),
                        ItemID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                        Qty = rdr["QtyOrdered"] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr["QtyOrdered"]),
                        PackagingID = rdr["PackagingID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["PackagingID"]),
                        ServiceTypeID = serviceType,
                        UnitsPerQty = rdr["UnitsPerQty"] == DBNull.Value ? 1.0 : Convert.ToDouble(rdr["UnitsPerQty"])
                    });
                }
            }

            if (rows.Count == 0)
                return 0;

            var itemUsage = new ContactsItemUsageRepository();
            var summaries = new ContactUsageLinesRepository();
            int written = 0;
            foreach (var group in rows.GroupBy(r => new { r.OrderID, r.ServiceTypeID, r.DeliveryDate }))
            {
                var summary = new ContactUsageLine
                {
                    ContactID = contactId,
                    UsageDate = group.Key.DeliveryDate,
                    ItemServiceTypeID = group.Key.ServiceTypeID,
                    Qty = 0,
                    Notes = "from Ad hoc order " + group.Key.OrderID.ToString(CultureInfo.InvariantCulture)
                };
                foreach (ImportedUsageRow line in group)
                {
                    if (line.ItemID <= 0)
                        continue;
                    itemUsage.InsertUsageLine(new ContactsItemUsage
                    {
                        ContactID = contactId,
                        DeliveryDate = line.DeliveryDate,
                        ItemProvidedID = line.ItemID,
                        QtyProvided = line.Qty,
                        ItemPackagingID = line.PackagingID,
                        Notes = summary.Notes
                    });
                    summary.Qty += line.Qty * (line.UnitsPerQty <= 0 ? 1.0 : line.UnitsPerQty);
                    written++;
                }
                summaries.InsertUsageLine(summary);
            }

            return written;
        }

        private static HashSet<int> TrackedServiceTypeIds()
        {
            var ids = new HashSet<int>();
            try
            {
                List<TrackedServiceItem> tracked = new TrackedServiceItemRepository().GetAllForPrediction();
                if (tracked == null)
                    return ids;
                foreach (TrackedServiceItem item in tracked)
                {
                    if (item.ItemServiceTypeID.HasValue && item.ItemServiceTypeID.Value > 0
                        && item.ItemServiceTypeID.Value != SystemConstants.ServiceTypeConstants.NotApplicable)
                        ids.Add(item.ItemServiceTypeID.Value);
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "ZZName promote: tracked types: " + ex.Message);
            }
            return ids;
        }

        private static void RecalcPredictionFromImportedUsage(int contactId)
        {
            try
            {
                new ContactsUsageRepository().UpdateLastCupCount(contactId, 1);
                new PredictionManager().CalcAndSaveNextRequiredDates(contactId);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "ZZName promote: prediction: " + ex.Message);
            }
        }

        private sealed class ImportedUsageRow
        {
            public int OrderID { get; set; }
            public DateTime? DeliveryDate { get; set; }
            public int ItemID { get; set; }
            public double Qty { get; set; }
            public int? PackagingID { get; set; }
            public int ServiceTypeID { get; set; }
            public double UnitsPerQty { get; set; }
        }

        /// <summary>Deletes unused ZZName casual alts not used since <paramref name="olderThan"/> and with no orders.</summary>
        public static int PurgeUnused(DateTime olderThan)
        {
            if (!ContactDeliverToAddressRepository.TableSeen())
                return 0;
            string datePred = ContactDeliverToAddressRepository.HasUseDatesColumn()
                ? "AND ISNULL(d.LastUsedDate, d.CreatedDate) < @Cut"
                : "AND 1 = 0";
            string sql = @"
DELETE d
FROM dbo.ContactDeliverToAddressTbl d
WHERE d.ContactID = @Zz
  AND ISNULL(d.AddressKind, N'') IN (N'Sundry', N'Casual', N'OnBehalf')
  AND NOT EXISTS (SELECT 1 FROM dbo.OrdersTbl o WHERE o.DeliverToAddressID = d.DeliverToAddressID)
  " + datePred;
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Zz", DataValue = (int)SystemConstants.CustomerConstants.SundryCustomerID, DataDbType = System.Data.DbType.Int32 },
                    new DBParameter { ParamName = "@Cut", DataValue = olderThan, DataDbType = System.Data.DbType.DateTime }
                });
            }
        }
    }
}
