using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class RecurringOrdersRepository
    {
        public List<RecurringTypeLookup> GetRecurringTypes()
        {
            var list = new List<RecurringTypeLookup>();
            const string sql = "SELECT RecurringTypeID, RecurringTypeDesc FROM RecurringTypesTbl ORDER BY RecurringTypeDesc";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new RecurringTypeLookup
                    {
                        RecurringTypeID = GetValue<int>(rdr, "RecurringTypeID"),
                        RecurringTypeDesc = GetValue<string>(rdr, "RecurringTypeDesc")
                    });
                }
            }

            return list;
        }

        public List<DeliveryByLookup> GetDeliveryPeople()
        {
            var list = new List<DeliveryByLookup>();
            const string sql = "SELECT PersonID, Person, Abbreviation FROM PeopleTbl WHERE ISNULL(Enabled, 1) = 1 ORDER BY Abbreviation, Person";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new DeliveryByLookup
                    {
                        PersonID = GetValue<int>(rdr, "PersonID"),
                        Person = GetValue<string>(rdr, "Person"),
                        Abbreviation = GetValue<string>(rdr, "Abbreviation")
                    });
                }
            }

            return list;
        }

        public RecurringOrder GetById(int id)
        {
            const string sql = "SELECT RecurringOrderID, ContactID, Enabled, Notes, DeliveryByID FROM RecurringOrdersTbl WHERE RecurringOrderID = @Id";
            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            RecurringOrder recurringOrder = null;
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                    recurringOrder = Map(rdr);
            }

            // Load items after the header reader is closed — nested readers on some setups
            // return no rows while the first reader is still open.
            if (recurringOrder != null)
            {
                recurringOrder.Items = GetItemsForRecurring(recurringOrder.RecurringOrderID);
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    "RecurringOrdersRepository.GetById: RecurringOrderID=" + recurringOrder.RecurringOrderID
                        + " items=" + (recurringOrder.Items == null ? 0 : recurringOrder.Items.Count));
            }

            return recurringOrder;
        }

        public int Insert(RecurringOrder recurringOrder)
        {
            const string sql = @"INSERT INTO RecurringOrdersTbl
                (ContactID, Enabled, Notes, DeliveryByID)
                VALUES
                (@ContactID, @Enabled, @Notes, @DeliveryByID);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using (var db = new TrackerSQLDb())
            {
                var newId = Convert.ToInt32(db.ExecuteScalar(sql, BuildOrderParameters(recurringOrder, false)));
                ReplaceItems(newId, recurringOrder.Items);
                return newId;
            }
        }

        public void Update(RecurringOrder recurringOrder)
        {
            const string sql = @"UPDATE RecurringOrdersTbl
                SET ContactID = @ContactID,
                    Enabled = @Enabled,
                    Notes = @Notes,
                    DeliveryByID = @DeliveryByID
                WHERE RecurringOrderID = @RecurringOrderID";

            using (var db = new TrackerSQLDb())
            {
                db.ExecuteNonQuery(sql, BuildOrderParameters(recurringOrder, true));
            }

            ReplaceItems(recurringOrder.RecurringOrderID, recurringOrder.Items);
        }

        public void Delete(int recurringOrderId)
        {
            if (recurringOrderId <= 0)
                return;

            var itemIds = GetItemsForRecurring(recurringOrderId)
                .Where(item => item != null && item.RecurringOrderItemID > 0)
                .Select(item => item.RecurringOrderItemID)
                .ToList();

            using (var db = new TrackerSQLDb())
            {
                // Same FK cleanup as ReplaceItems — TempCoffeecheckupItemsTbl may reference lines.
                ClearTempCoffeeCheckupRecurringItemReferences(db, itemIds);

                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = recurringOrderId, DataDbType = DbType.Int32, ParamName = "@RecurringOrderID" }
                };

                db.ExecuteNonQuery("DELETE FROM RecurringOrderItemsTbl WHERE RecurringOrderID = @RecurringOrderID", parameters);
                db.ExecuteNonQuery("DELETE FROM RecurringOrdersTbl WHERE RecurringOrderID = @RecurringOrderID", parameters);
            }
        }

        public List<RecurringOrder> GetAll()
        {
            var list = new List<RecurringOrder>();
            const string sql = "SELECT RecurringOrderID, ContactID, Enabled, Notes, DeliveryByID FROM RecurringOrdersTbl";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(Map(rdr));
            }

            foreach (var recurringOrder in list)
                recurringOrder.Items = GetItemsForRecurring(recurringOrder.RecurringOrderID);

            return list;
        }

        public List<RecurringOrderSummary> GetSummaries(string sortBy, string companyNameFilter, int enabledFilter)
        {
            var summaries = new List<RecurringOrderSummary>();
            var parameters = new List<DBParameter>();
            string sql = @"SELECT
                    RecurringOrdersTbl.RecurringOrderID,
                    RecurringOrderItemsTbl.RecurringOrderItemID,
                    RecurringOrdersTbl.ContactID,
                    ContactsTbl.CompanyName,
                    RecurringOrdersTbl.DeliveryByID,
                    COALESCE(NULLIF(PeopleTbl.Abbreviation, ''), PeopleTbl.Person) AS DeliveryByDisplay,
                    RecurringOrderItemsTbl.RecurringTypeID,
                    RecurringTypesTbl.RecurringTypeDesc,
                    RecurringOrderItemsTbl.Value,
                    RecurringOrderItemsTbl.DateLastDone,
                    RecurringOrderItemsTbl.NextDateRequired,
                    RecurringOrderItemsTbl.RequireUntilDate,
                    RecurringOrderItemsTbl.ItemRequiredID,
                    ItemsTbl.ItemDesc,
                    RecurringOrderItemsTbl.QtyRequired,
                    RecurringOrderItemsTbl.ItemPackagingID,
                    ItemPackagingsTbl.ItemPrepDescription AS ItemPackagingDesc,
                    RecurringOrdersTbl.Enabled,
                    RecurringOrdersTbl.Notes
                FROM RecurringOrderItemsTbl
                INNER JOIN RecurringOrdersTbl ON RecurringOrderItemsTbl.RecurringOrderID = RecurringOrdersTbl.RecurringOrderID
                LEFT OUTER JOIN ContactsTbl ON RecurringOrdersTbl.ContactID = ContactsTbl.ContactID
                LEFT OUTER JOIN RecurringTypesTbl ON RecurringOrderItemsTbl.RecurringTypeID = RecurringTypesTbl.RecurringTypeID
                LEFT OUTER JOIN ItemsTbl ON RecurringOrderItemsTbl.ItemRequiredID = ItemsTbl.ItemID
                LEFT OUTER JOIN ItemPackagingsTbl ON RecurringOrderItemsTbl.ItemPackagingID = ItemPackagingsTbl.ItemPackagingID
                LEFT OUTER JOIN PeopleTbl ON RecurringOrdersTbl.DeliveryByID = PeopleTbl.PersonID";

            var filters = new List<string>();
            if (enabledFilter == 0)
            {
                filters.Add("RecurringOrdersTbl.Enabled = @Enabled");
                parameters.Add(new DBParameter { DataValue = false, DataDbType = DbType.Boolean, ParamName = "@Enabled" });
            }
            else if (enabledFilter == 1)
            {
                filters.Add("RecurringOrdersTbl.Enabled = @Enabled");
                parameters.Add(new DBParameter { DataValue = true, DataDbType = DbType.Boolean, ParamName = "@Enabled" });
            }

            if (!string.IsNullOrWhiteSpace(companyNameFilter))
            {
                filters.Add("ContactsTbl.CompanyName LIKE @CompanyName");
                parameters.Add(new DBParameter { DataValue = "%" + companyNameFilter.Trim() + "%", DataDbType = DbType.String, ParamName = "@CompanyName" });
            }

            if (filters.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", filters.ToArray());
            }

            using (var db = new TrackerSQLDb())
            using (var rdr = parameters.Count > 0 ? db.ExecuteReader(sql, parameters) : db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    summaries.Add(MapSummary(rdr));
                }
            }

            summaries = DeduplicateSummaries(summaries);
            return SortSummaries(summaries, sortBy);
        }

        public List<RecurringOrderSummary> GetEnabledSummariesByContactId(int contactId)
        {
            return GetSummaries(string.Empty, null, 1)
                .Where(summary => summary.ContactID == contactId)
                .ToList();
        }

        public List<RecurringOrderSummary> GetEnabledSummariesDueByDate(DateTime windowEnd)
        {
            var summaries = new List<RecurringOrderSummary>();
            const string sql = @"SELECT
                    RecurringOrdersTbl.RecurringOrderID,
                    RecurringOrderItemsTbl.RecurringOrderItemID,
                    RecurringOrdersTbl.ContactID,
                    ContactsTbl.CompanyName,
                    RecurringOrdersTbl.DeliveryByID,
                    COALESCE(NULLIF(PeopleTbl.Abbreviation, ''), PeopleTbl.Person) AS DeliveryByDisplay,
                    RecurringOrderItemsTbl.RecurringTypeID,
                    RecurringTypesTbl.RecurringTypeDesc,
                    RecurringOrderItemsTbl.Value,
                    RecurringOrderItemsTbl.DateLastDone,
                    RecurringOrderItemsTbl.NextDateRequired,
                    RecurringOrderItemsTbl.RequireUntilDate,
                    RecurringOrderItemsTbl.ItemRequiredID,
                    ItemsTbl.ItemDesc,
                    RecurringOrderItemsTbl.QtyRequired,
                    RecurringOrderItemsTbl.ItemPackagingID,
                    ItemPackagingsTbl.ItemPrepDescription AS ItemPackagingDesc,
                    RecurringOrdersTbl.Enabled,
                    RecurringOrdersTbl.Notes
                FROM RecurringOrderItemsTbl
                INNER JOIN RecurringOrdersTbl ON RecurringOrderItemsTbl.RecurringOrderID = RecurringOrdersTbl.RecurringOrderID
                LEFT OUTER JOIN ContactsTbl ON RecurringOrdersTbl.ContactID = ContactsTbl.ContactID
                LEFT OUTER JOIN RecurringTypesTbl ON RecurringOrderItemsTbl.RecurringTypeID = RecurringTypesTbl.RecurringTypeID
                LEFT OUTER JOIN ItemsTbl ON RecurringOrderItemsTbl.ItemRequiredID = ItemsTbl.ItemID
                LEFT OUTER JOIN ItemPackagingsTbl ON RecurringOrderItemsTbl.ItemPackagingID = ItemPackagingsTbl.ItemPackagingID
                LEFT OUTER JOIN PeopleTbl ON RecurringOrdersTbl.DeliveryByID = PeopleTbl.PersonID
                WHERE RecurringOrdersTbl.Enabled = 1
                  AND RecurringOrderItemsTbl.NextDateRequired <= @WindowEnd";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WindowEnd", DataValue = windowEnd.Date, DataDbType = DbType.Date }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    summaries.Add(MapSummary(rdr));
                }
            }

            return DeduplicateSummaries(summaries);
        }

        public HashSet<int> GetEnabledContactIds()
        {
            var contactIds = new HashSet<int>();
            foreach (var summary in GetSummaries(string.Empty, null, 1))
            {
                if (summary.ContactID.HasValue && summary.ContactID.Value > 0)
                {
                    contactIds.Add(summary.ContactID.Value);
                }
            }

            return contactIds;
        }

        public RecurringOrderSummary GetSummaryByRecurringOrderItemId(int recurringOrderItemId)
        {
            foreach (var summary in GetSummaries(string.Empty, null, -1))
            {
                if (summary.RecurringOrderItemID == recurringOrderItemId)
                {
                    return summary;
                }
            }

            return null;
        }

        public bool UpdateItemNextDateRequired(int recurringOrderItemId, DateTime nextDateRequired)
        {
            const string sql = @"
                UPDATE RecurringOrderItemsTbl
                SET NextDateRequired = @NextDateRequired
                WHERE RecurringOrderItemID = @RecurringOrderItemID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@NextDateRequired", DataValue = nextDateRequired.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@RecurringOrderItemID", DataValue = recurringOrderItemId, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters) > 0;
            }
        }

        public bool DisableRecurringOrder(int recurringOrderId)
        {
            const string sql = "UPDATE RecurringOrdersTbl SET Enabled = 0 WHERE RecurringOrderID = @RecurringOrderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RecurringOrderID", DataValue = recurringOrderId, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters) > 0;
            }
        }

        public string SetRecurringOrderItemDates(DateTime orderDate, int recurringOrderItemId, bool orderDone = false)
        {
            try
            {
                var summary = GetSummaryByRecurringOrderItemId(recurringOrderItemId);
                if (summary == null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"RecurringOrdersRepository: Not found RecurringOrderItemID={recurringOrderItemId}");
                    return "Recurring order item not found";
                }

                DateTime? dateLastDone = summary.DateLastDone;
                if (orderDone)
                {
                    dateLastDone = orderDate.Date;
                }

                DateTime? until = NormalizeOptionalDate(summary.RequireUntilDate);

                // Compute next without until-clamp so we can detect end-of-series
                DateTime? nextDateRequired = CalculateNextDateRequired(
                    summary.ContactID,
                    summary.RecurringTypeID,
                    summary.Value,
                    dateLastDone,
                    null);

                bool disableAfterUntil = until.HasValue
                    && nextDateRequired.HasValue
                    && nextDateRequired.Value.Date > until.Value.Date;

                if (disableAfterUntil)
                {
                    // Keep NextDateRequired at the until date; disable the header so it stops firing
                    nextDateRequired = until;
                }

                const string sqlDone = @"
                    UPDATE RecurringOrderItemsTbl
                    SET DateLastDone = @DateLastDone, NextDateRequired = @NextDateRequired
                    WHERE RecurringOrderItemID = @RecurringOrderItemID";

                const string sqlNextOnly = @"
                    UPDATE RecurringOrderItemsTbl
                    SET NextDateRequired = @NextDateRequired
                    WHERE RecurringOrderItemID = @RecurringOrderItemID";

                var parameters = new List<DBParameter>
                {
                    new DBParameter { ParamName = "@NextDateRequired", DataValue = nextDateRequired.HasValue ? (object)nextDateRequired.Value.Date : DBNull.Value, DataDbType = DbType.Date },
                    new DBParameter { ParamName = "@RecurringOrderItemID", DataValue = recurringOrderItemId, DataDbType = DbType.Int32 }
                };

                if (orderDone)
                {
                    parameters.Insert(0, new DBParameter { ParamName = "@DateLastDone", DataValue = dateLastDone.HasValue ? (object)dateLastDone.Value.Date : DBNull.Value, DataDbType = DbType.Date });
                }

                using (var db = new TrackerSQLDb())
                {
                    db.ExecuteNonQuery(orderDone ? sqlDone : sqlNextOnly, parameters);
                }

                if (disableAfterUntil)
                {
                    DisableRecurringOrder(summary.RecurringOrderID);
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"RecurringOrderID={summary.RecurringOrderID} disabled — next cycle past until {until:yyyy-MM-dd} ({summary.CompanyName})");
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"RecurringOrderItemID={recurringOrderItemId} Done={orderDone} Last={dateLastDone:yyyy-MM-dd} Next={nextDateRequired:yyyy-MM-dd} Disabled={disableAfterUntil}");

                string disabledNote = disableAfterUntil ? "; header disabled (past until)" : string.Empty;
                return string.Format(
                    "Updated Last={0}; Next={1}{2}",
                    dateLastDone.HasValue ? dateLastDone.Value.ToString("yyyy-MM-dd") : "(none)",
                    nextDateRequired.HasValue ? nextDateRequired.Value.ToString("yyyy-MM-dd") : "(none)",
                    disabledNote);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"RecurringOrdersRepository: Error RecurringOrderItemID={recurringOrderItemId}: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// All enabled recurring order lines, with optional max matching delivery from ContactsItemUsageTbl.
        /// Matching mirrors Order Done: exact item, coffee↔coffee, or group membership / coffee-in-group.
        /// </summary>
        public List<RecurringLastDateFromUsage> GetEnabledItemsWithUsageForLastDateSync()
        {
            int coffee = SystemConstants.ServiceTypeConstants.Coffee;
            int groupItem = SystemConstants.ServiceTypeConstants.GroupItem;

            string sql = @"
                SELECT
                    ROI.RecurringOrderItemID,
                    ROI.RecurringOrderID,
                    RO.ContactID,
                    C.CompanyName,
                    I.ItemDesc,
                    ROI.DateLastDone AS PreviousDateLastDone,
                    MAX(CIU.DeliveryDate) AS LastUsageDate
                FROM RecurringOrderItemsTbl AS ROI
                INNER JOIN RecurringOrdersTbl AS RO
                    ON RO.RecurringOrderID = ROI.RecurringOrderID
                INNER JOIN ContactsTbl AS C
                    ON C.ContactID = RO.ContactID
                LEFT JOIN ItemsTbl AS I
                    ON I.ItemID = ROI.ItemRequiredID
                LEFT JOIN ContactsItemUsageTbl AS CIU
                    ON CIU.ContactID = RO.ContactID
                   AND CIU.DeliveryDate IS NOT NULL
                   AND ROI.ItemRequiredID IS NOT NULL
                   AND ROI.ItemRequiredID > 0
                   AND (
                        CIU.ItemProvidedID = ROI.ItemRequiredID
                        OR (
                            I.ItemServiceTypeID = " + coffee + @"
                            AND EXISTS (
                                SELECT 1
                                FROM ItemsTbl AS Provided
                                WHERE Provided.ItemID = CIU.ItemProvidedID
                                  AND Provided.ItemServiceTypeID = " + coffee + @")
                        )
                        OR (
                            I.ItemServiceTypeID = " + groupItem + @"
                            AND (
                                EXISTS (
                                    SELECT 1
                                    FROM ItemGroupsTbl AS G
                                    WHERE G.GroupItemServiceTypeID = ROI.ItemRequiredID
                                      AND G.ItemID = CIU.ItemProvidedID)
                                OR (
                                    EXISTS (
                                        SELECT 1
                                        FROM ItemsTbl AS Provided
                                        WHERE Provided.ItemID = CIU.ItemProvidedID
                                          AND Provided.ItemServiceTypeID = " + coffee + @")
                                    AND EXISTS (
                                        SELECT 1
                                        FROM ItemGroupsTbl AS G
                                        INNER JOIN ItemsTbl AS GI ON GI.ItemID = G.ItemID
                                        WHERE G.GroupItemServiceTypeID = ROI.ItemRequiredID
                                          AND GI.ItemServiceTypeID = " + coffee + @")
                                )
                            )
                        )
                   )
                WHERE ISNULL(RO.Enabled, 0) = 1
                GROUP BY
                    ROI.RecurringOrderItemID,
                    ROI.RecurringOrderID,
                    RO.ContactID,
                    C.CompanyName,
                    I.ItemDesc,
                    ROI.DateLastDone
                ORDER BY C.CompanyName, I.ItemDesc";

            var list = new List<RecurringLastDateFromUsage>();
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    DateTime? lastUsage = NormalizeOptionalDate(GetValue<DateTime?>(rdr, "LastUsageDate"));
                    list.Add(new RecurringLastDateFromUsage
                    {
                        RecurringOrderItemID = GetValue<int>(rdr, "RecurringOrderItemID"),
                        RecurringOrderID = GetValue<int>(rdr, "RecurringOrderID"),
                        ContactID = GetValue<int>(rdr, "ContactID"),
                        CompanyName = GetValue<string>(rdr, "CompanyName") ?? string.Empty,
                        ItemDesc = GetValue<string>(rdr, "ItemDesc") ?? string.Empty,
                        PreviousDateLastDone = NormalizeOptionalDate(GetValue<DateTime?>(rdr, "PreviousDateLastDone")),
                        LastUsageDate = lastUsage
                    });
                }
            }

            return list;
        }

        /// <summary>
        /// System Tools: for every enabled recurring line, set DateLastDone from matching item usage
        /// when available (never regress), then recalculate NextDateRequired via SetRecurringOrderItemDates.
        /// Lines with no usable usage still refresh Next from the existing DateLastDone.
        /// </summary>
        public List<RecurringLastDateFromUsage> SetLastRecurringOrderDate()
        {
            var candidates = GetEnabledItemsWithUsageForLastDateSync();
            foreach (var candidate in candidates)
            {
                DateTime? usage = candidate.LastUsageDate.HasValue
                    ? candidate.LastUsageDate.Value.Date
                    : (DateTime?)null;
                DateTime? previous = candidate.PreviousDateLastDone.HasValue
                    ? candidate.PreviousDateLastDone.Value.Date
                    : (DateTime?)null;

                if (usage.HasValue && (!previous.HasValue || usage.Value >= previous.Value))
                {
                    candidate.AppliedLastDate = usage;
                    candidate.UpdateResult = SetRecurringOrderItemDates(
                        usage.Value,
                        candidate.RecurringOrderItemID,
                        orderDone: true);
                    if (previous.HasValue && usage.Value == previous.Value)
                    {
                        candidate.UpdateResult += " (usage matched existing last; Next refreshed)";
                    }
                }
                else if (previous.HasValue)
                {
                    candidate.AppliedLastDate = previous;
                    candidate.UpdateResult = SetRecurringOrderItemDates(
                        previous.Value,
                        candidate.RecurringOrderItemID,
                        orderDone: true);
                    if (!usage.HasValue)
                    {
                        candidate.UpdateResult += " (no matching usage; Next refreshed)";
                    }
                    else
                    {
                        candidate.UpdateResult += " (usage older than DateLastDone; Next refreshed)";
                    }
                }
                else
                {
                    candidate.AppliedLastDate = null;
                    candidate.UpdateResult = "Skipped — no DateLastDone and no matching usage";
                }
            }

            return candidates;
        }

        public DateCalculator.PrepDeliveryPair CalculatePrepDeliveryDates(RecurringOrderSummary summary)
        {
            if (summary == null || !summary.ContactID.HasValue)
            {
                return new DateCalculator.PrepDeliveryPair(TimeZoneUtils.Now().Date, TimeZoneUtils.Now().Date);
            }

            var calculator = new DateCalculator();
            int contactId = summary.ContactID.Value;
            DateTime lastDone = summary.DateLastDone ?? SystemConstants.DatabaseConstants.SystemMinDate;
            bool isFirstTime = lastDone <= SystemConstants.DatabaseConstants.SystemMinDate;
            DateTime today = TimeZoneUtils.Now().Date;
            int recurrenceType = summary.RecurringTypeID ?? 0;
            int recurrenceValue = summary.Value ?? 0;

            switch (recurrenceType)
            {
                case 1:
                    {
                        int weeks = recurrenceValue > 0 ? recurrenceValue : 1;
                        int intervalDays = weeks * 7;
                        DateTime anchor = isFirstTime
                            ? today
                            : lastDone.AddDays(intervalDays);

                        while (!isFirstTime && anchor < today)
                        {
                            anchor = anchor.AddDays(intervalDays);
                        }

                        return calculator.CalculateOptimalWeeklyDeliveryDates(contactId, anchor);
                    }
                case 5:
                    return calculator.CalculateOptimalMonthlyDeliveryDates(contactId, recurrenceValue > 0 ? recurrenceValue : 1, lastDone);
                default:
                    {
                        DateTime delivery = isFirstTime ? today : lastDone.AddDays(7).Date;
                        return calculator.CalculateOptimalWeeklyDeliveryDates(contactId, delivery);
                    }
            }
        }

        public int RecalculateNextDatesRequiredForEnabledHeaders()
        {
            var recurringOrderIds = GetEnabledRecurringOrderIdsForNextDateCalculation();
            if (recurringOrderIds.Count == 0)
            {
                return 0;
            }

            int updatedCount = 0;
            using (var db = new TrackerSQLDb())
            {
                foreach (var recurringOrderId in recurringOrderIds)
                {
                    var recurringOrder = GetById(recurringOrderId);
                    if (recurringOrder == null || recurringOrder.Enabled != true)
                    {
                        continue;
                    }

                    AutoCalculateNextDatesForItems(recurringOrder);

                    foreach (var item in recurringOrder.Items.Where(item => item != null && item.RecurringOrderItemID > 0))
                    {
                        var parameters = new List<DBParameter>
                        {
                            new DBParameter { DataValue = NormalizeOptionalDate(item.NextDateRequired).HasValue ? (object)NormalizeOptionalDate(item.NextDateRequired).Value : DBNull.Value, DataDbType = DbType.DateTime, ParamName = "@NextDateRequired" },
                            new DBParameter { DataValue = item.RecurringOrderItemID, DataDbType = DbType.Int32, ParamName = "@RecurringOrderItemID" }
                        };

                        db.ExecuteNonQuery(
                            "UPDATE RecurringOrderItemsTbl SET NextDateRequired = @NextDateRequired WHERE RecurringOrderItemID = @RecurringOrderItemID",
                            parameters);

                        updatedCount++;
                    }
                }
            }

            return updatedCount;
        }

        public int AutoCalculateNextDatesForItems(RecurringOrder recurringOrder)
        {
            if (recurringOrder?.Items == null || !recurringOrder.ContactID.HasValue)
            {
                return 0;
            }

            int calculatedCount = 0;
            foreach (var item in recurringOrder.Items.Where(item => item != null))
            {
                DateTime? nextDateRequired = CalculateNextDateRequired(
                    recurringOrder.ContactID,
                    item.RecurringTypeID,
                    item.Value,
                    item.DateLastDone,
                    item.RequireUntilDate);

                item.NextDateRequired = nextDateRequired;
                if (nextDateRequired.HasValue)
                {
                    calculatedCount++;
                }
            }

            return calculatedCount;
        }

        private void ReplaceItems(int recurringOrderId, List<RecurringOrderItem> items)
        {
            var incomingItems = (items ?? new List<RecurringOrderItem>())
                .Where(item => item != null)
                .ToList();

            var existingItemIds = new HashSet<int>(GetItemsForRecurring(recurringOrderId)
                .Where(item => item != null && item.RecurringOrderItemID > 0)
                .Select(item => item.RecurringOrderItemID));

            using (var db = new TrackerSQLDb())
            {
                var retainedItemIds = new HashSet<int>();

                foreach (var item in incomingItems)
                {
                    var normalizedDateLastDone = NormalizeOptionalDate(item.DateLastDone);
                    var normalizedNextDateRequired = NormalizeOptionalDate(item.NextDateRequired);
                    var normalizedRequireUntilDate = NormalizeOptionalDate(item.RequireUntilDate);

                    if (item.RecurringOrderItemID > 0 && existingItemIds.Contains(item.RecurringOrderItemID))
                    {
                        var updateParameters = BuildItemParameters(recurringOrderId, item, normalizedDateLastDone, normalizedNextDateRequired, normalizedRequireUntilDate);
                        updateParameters.Add(new DBParameter { DataValue = item.RecurringOrderItemID, DataDbType = DbType.Int32, ParamName = "@RecurringOrderItemID" });

                        db.ExecuteNonQuery(
                            @"UPDATE RecurringOrderItemsTbl
                              SET RecurringTypeID = @RecurringTypeID,
                                  Value = @Value,
                                  ItemRequiredID = @ItemRequiredID,
                                  QtyRequired = @QtyRequired,
                                  DateLastDone = @DateLastDone,
                                  NextDateRequired = @NextDateRequired,
                                  RequireUntilDate = @RequireUntilDate,
                                  ItemPackagingID = @ItemPackagingID
                              WHERE RecurringOrderID = @RecurringOrderID
                                AND RecurringOrderItemID = @RecurringOrderItemID",
                            updateParameters);

                        retainedItemIds.Add(item.RecurringOrderItemID);
                        continue;
                    }

                    var insertParameters = BuildItemParameters(recurringOrderId, item, normalizedDateLastDone, normalizedNextDateRequired, normalizedRequireUntilDate);

                    db.ExecuteNonQuery(
                        @"INSERT INTO RecurringOrderItemsTbl
                            (RecurringOrderID, RecurringTypeID, Value, ItemRequiredID, QtyRequired, DateLastDone, NextDateRequired, RequireUntilDate, ItemPackagingID)
                          VALUES
                            (@RecurringOrderID, @RecurringTypeID, @Value, @ItemRequiredID, @QtyRequired, @DateLastDone, @NextDateRequired, @RequireUntilDate, @ItemPackagingID)",
                        insertParameters);
                }

                var removedItemIds = existingItemIds.Except(retainedItemIds).ToList();
                if (removedItemIds.Count > 0)
                {
                    ClearTempCoffeeCheckupRecurringItemReferences(db, removedItemIds);
                    DeleteRecurringItems(db, recurringOrderId, removedItemIds);
                }
            }
        }

        private static List<DBParameter> BuildItemParameters(
            int recurringOrderId,
            RecurringOrderItem item,
            DateTime? normalizedDateLastDone,
            DateTime? normalizedNextDateRequired,
            DateTime? normalizedRequireUntilDate)
        {
            return new List<DBParameter>
            {
                new DBParameter { DataValue = recurringOrderId, DataDbType = DbType.Int32, ParamName = "@RecurringOrderID" },
                new DBParameter { DataValue = item.RecurringTypeID.HasValue && item.RecurringTypeID.Value > 0 ? (object)item.RecurringTypeID.Value : DBNull.Value, DataDbType = DbType.Int32, ParamName = "@RecurringTypeID" },
                new DBParameter { DataValue = item.Value.HasValue ? (object)item.Value.Value : DBNull.Value, DataDbType = DbType.Int32, ParamName = "@Value" },
                new DBParameter { DataValue = item.ItemRequiredID.HasValue && item.ItemRequiredID.Value > 0 ? (object)item.ItemRequiredID.Value : DBNull.Value, DataDbType = DbType.Int32, ParamName = "@ItemRequiredID" },
                new DBParameter { DataValue = item.QtyRequired.HasValue ? (object)item.QtyRequired.Value : DBNull.Value, DataDbType = DbType.Double, ParamName = "@QtyRequired" },
                new DBParameter { DataValue = normalizedDateLastDone.HasValue ? (object)normalizedDateLastDone.Value : DBNull.Value, DataDbType = DbType.DateTime, ParamName = "@DateLastDone" },
                new DBParameter { DataValue = normalizedNextDateRequired.HasValue ? (object)normalizedNextDateRequired.Value : DBNull.Value, DataDbType = DbType.DateTime, ParamName = "@NextDateRequired" },
                new DBParameter { DataValue = normalizedRequireUntilDate.HasValue ? (object)normalizedRequireUntilDate.Value : DBNull.Value, DataDbType = DbType.DateTime, ParamName = "@RequireUntilDate" },
                new DBParameter { DataValue = item.ItemPackagingID.HasValue && item.ItemPackagingID.Value > 0 ? (object)item.ItemPackagingID.Value : DBNull.Value, DataDbType = DbType.Int32, ParamName = "@ItemPackagingID" }
            };
        }

        private static void ClearTempCoffeeCheckupRecurringItemReferences(TrackerSQLDb db, List<int> recurringOrderItemIds)
        {
            if (db == null || recurringOrderItemIds == null || recurringOrderItemIds.Count == 0)
            {
                return;
            }

            var parameters = new List<DBParameter>();
            var parameterNames = new List<string>();
            for (int index = 0; index < recurringOrderItemIds.Count; index++)
            {
                string parameterName = "@RecurringOrderItemID" + index;
                parameterNames.Add(parameterName);
                parameters.Add(new DBParameter
                {
                    DataValue = recurringOrderItemIds[index],
                    DataDbType = DbType.Int32,
                    ParamName = parameterName
                });
            }

            db.ExecuteNonQuery(
                "UPDATE TempCoffeecheckupItemsTbl SET RecurringOrderItemID = NULL WHERE RecurringOrderItemID IN (" + string.Join(", ", parameterNames) + ")",
                parameters);
        }

        private static void DeleteRecurringItems(TrackerSQLDb db, int recurringOrderId, List<int> recurringOrderItemIds)
        {
            if (db == null || recurringOrderItemIds == null || recurringOrderItemIds.Count == 0)
            {
                return;
            }

            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = recurringOrderId, DataDbType = DbType.Int32, ParamName = "@RecurringOrderID" }
            };

            var parameterNames = new List<string>();
            for (int index = 0; index < recurringOrderItemIds.Count; index++)
            {
                string parameterName = "@DeleteRecurringOrderItemID" + index;
                parameterNames.Add(parameterName);
                parameters.Add(new DBParameter
                {
                    DataValue = recurringOrderItemIds[index],
                    DataDbType = DbType.Int32,
                    ParamName = parameterName
                });
            }

            db.ExecuteNonQuery(
                "DELETE FROM RecurringOrderItemsTbl WHERE RecurringOrderID = @RecurringOrderID AND RecurringOrderItemID IN (" + string.Join(", ", parameterNames) + ")",
                parameters);
        }

        private List<DBParameter> BuildOrderParameters(RecurringOrder recurringOrder, bool includeKey)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = recurringOrder.ContactID.HasValue && recurringOrder.ContactID.Value > 0 ? (object)recurringOrder.ContactID.Value : DBNull.Value, DataDbType = DbType.Int32, ParamName = "@ContactID" },
                new DBParameter { DataValue = recurringOrder.Enabled.HasValue ? (object)recurringOrder.Enabled.Value : DBNull.Value, DataDbType = DbType.Boolean, ParamName = "@Enabled" },
                new DBParameter { DataValue = string.IsNullOrWhiteSpace(recurringOrder.Notes) ? DBNull.Value : (object)recurringOrder.Notes.Trim(), DataDbType = DbType.String, ParamName = "@Notes" },
                new DBParameter { DataValue = recurringOrder.DeliveryByID.HasValue && recurringOrder.DeliveryByID.Value > 0 ? (object)recurringOrder.DeliveryByID.Value : DBNull.Value, DataDbType = DbType.Int32, ParamName = "@DeliveryByID" }
            };

            if (includeKey)
            {
                parameters.Add(new DBParameter { DataValue = recurringOrder.RecurringOrderID, DataDbType = DbType.Int32, ParamName = "@RecurringOrderID" });
            }

            return parameters;
        }

        private List<RecurringOrderItem> GetItemsForRecurring(int recurringOrderId)
        {
            var list = new List<RecurringOrderItem>();
            const string sql = @"SELECT
                    RecurringOrderItemsTbl.RecurringOrderItemID,
                    RecurringOrderItemsTbl.RecurringOrderID,
                    RecurringOrderItemsTbl.RecurringTypeID,
                    RecurringOrderItemsTbl.Value,
                    RecurringOrderItemsTbl.ItemRequiredID,
                    RecurringOrderItemsTbl.QtyRequired,
                    RecurringOrderItemsTbl.DateLastDone,
                    RecurringOrderItemsTbl.NextDateRequired,
                    RecurringOrderItemsTbl.RequireUntilDate,
                    RecurringOrderItemsTbl.ItemPackagingID,
                    RecurringTypesTbl.RecurringTypeDesc,
                    ItemsTbl.ItemDesc,
                    ItemPackagingsTbl.ItemPrepDescription AS ItemPackagingDesc
                FROM RecurringOrderItemsTbl
                LEFT OUTER JOIN RecurringTypesTbl ON RecurringOrderItemsTbl.RecurringTypeID = RecurringTypesTbl.RecurringTypeID
                LEFT OUTER JOIN ItemsTbl ON RecurringOrderItemsTbl.ItemRequiredID = ItemsTbl.ItemID
                LEFT OUTER JOIN ItemPackagingsTbl ON RecurringOrderItemsTbl.ItemPackagingID = ItemPackagingsTbl.ItemPackagingID
                WHERE RecurringOrderItemsTbl.RecurringOrderID = @RecurringOrderID
                ORDER BY RecurringOrderItemsTbl.NextDateRequired, ItemsTbl.ItemDesc, RecurringOrderItemsTbl.RecurringOrderItemID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = recurringOrderId, DataDbType = DbType.Int32, ParamName = "@RecurringOrderID" }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new RecurringOrderItem
                    {
                        RecurringOrderItemID = GetValue<int>(rdr, "RecurringOrderItemID"),
                        RecurringOrderID = GetValue<int>(rdr, "RecurringOrderID"),
                        RecurringTypeID = GetValue<int?>(rdr, "RecurringTypeID"),
                        Value = GetValue<int?>(rdr, "Value"),
                        ItemRequiredID = GetValue<int?>(rdr, "ItemRequiredID"),
                        QtyRequired = GetValue<double?>(rdr, "QtyRequired"),
                        DateLastDone = NormalizeOptionalDate(GetValue<DateTime?>(rdr, "DateLastDone")),
                        NextDateRequired = NormalizeOptionalDate(GetValue<DateTime?>(rdr, "NextDateRequired")),
                        RequireUntilDate = NormalizeOptionalDate(GetValue<DateTime?>(rdr, "RequireUntilDate")),
                        ItemPackagingID = GetValue<int?>(rdr, "ItemPackagingID"),
                        RecurringTypeDesc = GetValue<string>(rdr, "RecurringTypeDesc"),
                        ItemDesc = GetValue<string>(rdr, "ItemDesc"),
                        ItemPackagingDesc = GetValue<string>(rdr, "ItemPackagingDesc")
                    });
                }
            }

            return list;
        }

        private List<int> GetEnabledRecurringOrderIdsForNextDateCalculation()
        {
            var list = new List<int>();
            const string sql = @"SELECT RecurringOrderID
                FROM RecurringOrdersTbl
                WHERE Enabled = 1
                    AND ContactID IS NOT NULL";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    int recurringOrderId = GetValue<int>(rdr, "RecurringOrderID");
                    if (recurringOrderId > 0)
                    {
                        list.Add(recurringOrderId);
                    }
                }
            }

            return list;
        }

        private DateTime? CalculateNextDateRequired(
            int? contactId,
            int? recurringTypeId,
            int? value,
            DateTime? dateLastDone,
            DateTime? requireUntilDate)
        {
            if (!contactId.HasValue || !recurringTypeId.HasValue)
            {
                return null;
            }

            DateTime today = TimeZoneUtils.Now().Date;
            DateTime lastDoneDate = NormalizeOptionalDate(dateLastDone) ?? SystemConstants.DatabaseConstants.SystemMinDate;
            DateTime? normalizedRequireUntilDate = NormalizeOptionalDate(requireUntilDate);
            bool isFirstTime = lastDoneDate <= SystemConstants.DatabaseConstants.SystemMinDate;

            DateTime nextDateRequired;
            switch (recurringTypeId.Value)
            {
                case 1:
                    int weeks = value.HasValue && value.Value > 0 ? value.Value : 1;
                    int intervalDays = weeks * 7;

                    nextDateRequired = isFirstTime
                        ? today
                        : lastDoneDate.AddDays(intervalDays).Date;
                    break;

                case 5:
                    int targetDay = value.HasValue && value.Value > 0 ? value.Value : 1;
                    DateTime targetDate;
                    if (isFirstTime)
                    {
                        int daysInThisMonth = DateTime.DaysInMonth(today.Year, today.Month);
                        int thisMonthTargetDay = Math.Min(targetDay, daysInThisMonth);
                        targetDate = new DateTime(today.Year, today.Month, thisMonthTargetDay);

                        if (targetDate < today)
                        {
                            DateTime nextMonth = today.AddMonths(1);
                            int daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                            int nextMonthTargetDay = Math.Min(targetDay, daysInNextMonth);
                            targetDate = new DateTime(nextMonth.Year, nextMonth.Month, nextMonthTargetDay);
                        }
                    }
                    else
                    {
                        DateTime cycleMonth = lastDoneDate.AddMonths(1);
                        targetDate = BuildMonthlyTargetDate(cycleMonth, targetDay);

                        if (AreInSameMondayWeek(lastDoneDate, targetDate))
                        {
                            targetDate = BuildMonthlyTargetDate(cycleMonth.AddMonths(1), targetDay);
                        }
                    }

                    nextDateRequired = CalculateClosestMonthlyDeliveryDate(contactId.Value, targetDate);
                    break;

                default:
                    nextDateRequired = isFirstTime
                        ? today
                        : lastDoneDate.AddDays(7).Date;
                    break;
            }

            if (normalizedRequireUntilDate.HasValue && nextDateRequired.Date > normalizedRequireUntilDate.Value.Date)
            {
                nextDateRequired = normalizedRequireUntilDate.Value.Date;
            }

            return nextDateRequired.Date;
        }

        private static DateTime BuildMonthlyTargetDate(DateTime anyDayInTargetMonth, int targetDayOfMonth)
        {
            int daysInMonth = DateTime.DaysInMonth(anyDayInTargetMonth.Year, anyDayInTargetMonth.Month);
            int day = Math.Min(Math.Max(1, targetDayOfMonth), daysInMonth);
            return new DateTime(anyDayInTargetMonth.Year, anyDayInTargetMonth.Month, day);
        }

        private DateTime CalculateClosestMonthlyDeliveryDate(int contactId, DateTime targetDate)
        {
            var prepRules = GetPrepRulesForContact(contactId);
            DateTime today = TimeZoneUtils.Now().Date;
            targetDate = targetDate.Date;

            if (prepRules.Count == 0)
            {
                return targetDate < today ? today : targetDate;
            }

            const int searchRadius = 21;
            bool found = false;
            DateTime bestDelivery = DateTime.MaxValue;
            DateTime bestPrep = DateTime.MaxValue;
            double bestMetric = double.MaxValue;

            for (int radius = 0; radius <= searchRadius; radius++)
            {
                int[] offsets = radius == 0 ? new[] { 0 } : new[] { radius, -radius };

                foreach (int offset in offsets)
                {
                    DateTime candidatePrep = targetDate.AddDays(offset).Date;
                    int dayOfWeek = (int)candidatePrep.DayOfWeek;

                    foreach (var prepRule in prepRules.Where(rule => rule.PrepDayOfWeekID == dayOfWeek))
                    {
                        DateTime candidateDelivery = candidatePrep.AddDays(prepRule.DeliveryDelayDays).Date;
                        if (candidateDelivery < today)
                        {
                            continue;
                        }

                        double metric = Math.Abs((candidateDelivery - targetDate).TotalDays);
                        if (!found
                            || metric < bestMetric
                            || (metric == bestMetric && candidateDelivery < bestDelivery)
                            || (metric == bestMetric && candidateDelivery == bestDelivery && candidatePrep < bestPrep))
                        {
                            bestMetric = metric;
                            bestDelivery = candidateDelivery;
                            bestPrep = candidatePrep;
                            found = true;
                        }
                    }
                }
            }

            return found ? bestDelivery : (targetDate < today ? today : targetDate);
        }
            
        private List<PrepRule> GetPrepRulesForContact(int contactId)
        {
            var areasRepository = new AreasRepository();
            return areasRepository.GetPrepRulesForContact(contactId);
        }

        private static bool AreInSameMondayWeek(DateTime fiPreperationDate, DateTime secondDate)
        {
            return StartOfWeekMonday(fiPreperationDate) == StartOfWeekMonday(secondDate);
        }

        private static DateTime StartOfWeekMonday(DateTime value)
        {
            int diff = (7 + (value.DayOfWeek - DayOfWeek.Monday)) % 7;
            return value.AddDays(-diff).Date;
        }

        private RecurringOrder Map(IDataReader reader)
        {
            return new RecurringOrder
            {
                RecurringOrderID = GetValue<int>(reader, "RecurringOrderID"),
                ContactID = GetValue<int?>(reader, "ContactID"),
                Enabled = GetValue<bool?>(reader, "Enabled"),
                Notes = GetValue<string>(reader, "Notes"),
                DeliveryByID = GetValue<int?>(reader, "DeliveryByID")
            };
        }

        private RecurringOrderSummary MapSummary(IDataReader reader)
        {
            return new RecurringOrderSummary
            {
                RecurringOrderID = GetValue<int>(reader, "RecurringOrderID"),
                RecurringOrderItemID = GetValue<int>(reader, "RecurringOrderItemID"),
                ContactID = GetValue<int?>(reader, "ContactID"),
                CompanyName = GetValue<string>(reader, "CompanyName"),
                DeliveryByID = GetValue<int?>(reader, "DeliveryByID"),
                DeliveryByDisplay = GetValue<string>(reader, "DeliveryByDisplay"),
                RecurringTypeID = GetValue<int?>(reader, "RecurringTypeID"),
                RecurringTypeDesc = GetValue<string>(reader, "RecurringTypeDesc"),
                Value = GetValue<int?>(reader, "Value"),
                DateLastDone = NormalizeOptionalDate(GetValue<DateTime?>(reader, "DateLastDone")),
                NextDateRequired = NormalizeOptionalDate(GetValue<DateTime?>(reader, "NextDateRequired")),
                RequireUntilDate = NormalizeOptionalDate(GetValue<DateTime?>(reader, "RequireUntilDate")),
                ItemRequiredID = GetValue<int?>(reader, "ItemRequiredID"),
                ItemDesc = GetValue<string>(reader, "ItemDesc"),
                QtyRequired = GetValue<double?>(reader, "QtyRequired"),
                ItemPackagingID = GetValue<int?>(reader, "ItemPackagingID"),
                ItemPackagingDesc = GetValue<string>(reader, "ItemPackagingDesc"),
                Enabled = GetValue<bool?>(reader, "Enabled"),
                Notes = GetValue<string>(reader, "Notes")
            };
        }

        private List<RecurringOrderSummary> SortSummaries(List<RecurringOrderSummary> summaries, string sortBy)
        {
            switch (sortBy)
            {
                case "RecurringOrderID":
                    return summaries.OrderBy(summary => summary.RecurringOrderID).ThenBy(summary => summary.RecurringOrderItemID).ToList();
                case "RecurringTypeDesc":
                    return summaries.OrderBy(summary => summary.RecurringPatternDisplay).ThenBy(summary => summary.CompanyName).ThenBy(summary => summary.ItemsDisplay).ToList();
                case "DateLastDone":
                    return summaries.OrderBy(summary => summary.DateLastDone ?? DateTime.MaxValue).ThenBy(summary => summary.CompanyName).ThenBy(summary => summary.ItemsDisplay).ToList();
                case "NextDateRequired":
                    return summaries.OrderBy(summary => summary.NextDateRequired ?? DateTime.MaxValue).ThenBy(summary => summary.CompanyName).ThenBy(summary => summary.ItemsDisplay).ToList();
                case "RequireUntilDate":
                    return summaries.OrderBy(summary => summary.RequireUntilDate ?? DateTime.MaxValue).ThenBy(summary => summary.CompanyName).ThenBy(summary => summary.ItemsDisplay).ToList();
                case "Enabled":
                    return summaries.OrderBy(summary => summary.Enabled ?? false).ThenBy(summary => summary.CompanyName).ThenBy(summary => summary.ItemsDisplay).ToList();
                case "Notes":
                    return summaries.OrderBy(summary => summary.Notes ?? string.Empty).ThenBy(summary => summary.CompanyName).ThenBy(summary => summary.ItemsDisplay).ToList();
                case "CompanyName":
                default:
                    return summaries.OrderBy(summary => summary.CompanyName ?? string.Empty).ThenBy(summary => summary.NextDateRequired ?? DateTime.MaxValue).ThenBy(summary => summary.RecurringOrderID).ThenBy(summary => summary.RecurringOrderItemID).ToList();
            }
        }

        private static List<RecurringOrderSummary> DeduplicateSummaries(List<RecurringOrderSummary> summaries)
        {
            return (summaries ?? new List<RecurringOrderSummary>())
                .GroupBy(summary => new
                {
                    ItemKey = summary.RecurringOrderItemID > 0 ? summary.RecurringOrderItemID : 0,
                    summary.RecurringOrderID,
                    summary.ContactID,
                    summary.RecurringTypeID,
                    summary.ItemRequiredID,
                    summary.QtyRequired,
                    summary.ItemPackagingID,
                    summary.DateLastDone,
                    summary.NextDateRequired,
                    summary.RequireUntilDate,
                    Enabled = summary.Enabled ?? false,
                    Notes = summary.Notes ?? string.Empty
                })
                .Select(group => group.First())
                .ToList();
        }

        private static DateTime? NormalizeOptionalDate(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            DateTime date = value.Value.Date;
            // SystemMinDate (1980-01-01) and far-future sentinels mean "forever" / no until date
            if (date <= SystemConstants.DatabaseConstants.SystemMinDate || date.Year >= 2099)
                return null;

            return date;
        }

        /// <summary>
        /// True when this occurrence is the last one before RequireUntilDate
        /// (next cycle after completing on NextDateRequired would fall past until).
        /// </summary>
        public bool IsFinalOccurrenceBeforeUntil(RecurringOrderSummary summary)
        {
            if (summary == null)
                return false;

            DateTime? until = NormalizeOptionalDate(summary.RequireUntilDate);
            if (!until.HasValue || !summary.NextDateRequired.HasValue)
                return false;

            DateTime next = summary.NextDateRequired.Value.Date;
            if (next > until.Value.Date)
                return false;
            if (next == until.Value.Date)
                return true;

            // Peek following date with no until clamp (pass null) so we can detect expiry
            DateTime? following = CalculateNextDateRequired(
                summary.ContactID,
                summary.RecurringTypeID,
                summary.Value,
                next,
                null);

            return following.HasValue && following.Value.Date > until.Value.Date;
        }

        private static T GetValue<T>(IDataRecord record, string name)
        {
            if (!HasColumn(record, name))
            {
                return default(T);
            }

            object value = record[name];
            if (value == null || value == DBNull.Value)
            {
                return default(T);
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)Convert.ToString(value);
            }

            return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
        }

        private static bool HasColumn(IDataRecord record, string name)
        {
            for (int index = 0; index < record.FieldCount; index++)
            {
                if (string.Equals(record.GetName(index), name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class RecurringTypeLookup
    {
        public int RecurringTypeID { get; set; }
        public string RecurringTypeDesc { get; set; }
    }

    public class DeliveryByLookup
    {
        public int PersonID { get; set; }
        public string Person { get; set; }
        public string Abbreviation { get; set; }

        public string DisplayName
        {
            get
            {
                return string.IsNullOrWhiteSpace(Abbreviation)
                    ? Person ?? string.Empty
                    : Abbreviation + (string.IsNullOrWhiteSpace(Person) ? string.Empty : " - " + Person);
            }
        }
    }
}
