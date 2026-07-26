# DATETIME → DATE migration checklist

Use this when updating the **TrackerMigration** SQL scripts so business dates are stored as `DATE`, not `DATETIME` (legacy Access behaviour).

## Script location

All scripts to edit live here:

```
C:\SRC\ASP.net\TrackerMigration\MigrationRunner\Metadata\PlanEdits\Sql
```

## Files you edit

| File | What to do |
|------|------------|
| **`CreateTables_LATEST_FIXED.sql`** | Change each listed column from `DATETIME NULL` → `DATE NULL` |
| **`Migrate_<TableName>.sql`** | Where listed below, wrap date conversions: `CAST(dbo.SafeDateConvert([...]) AS DATE)` |
| Older `CreateTables_202605*.sql` | Optional — only `CreateTables_LATEST_FIXED.sql` is canonical; archive or ignore the rest |

**Do not** change `Verify_*.sql` unless a verify script compares literal types.

## Rules

1. **All calendar-day fields → `DATE`** (orders, prep, delivery, closures, reminders, usage, repairs, etc.).
2. **No `DATETIME` columns are required** in the current migrated schema — there are no audit columns like `CreatedAt` / `ModifiedAt`.
3. **C# can stay `DateTime?`** — SqlClient maps `DATE` fine; the app already uses `.Date` and `DbType.Date` in most places.
4. **`LogTbl`** is ignored by migration — not in scope.

### CreateTables change

```sql
-- before
[RequiredByDate] DATETIME NULL,

-- after
[RequiredByDate] DATE NULL,
```

### Migrate_* change (where `SafeDateConvert` is used)

```sql
-- before
dbo.SafeDateConvert([RequiredByDate]) AS [RequiredByDate]

-- after
CAST(dbo.SafeDateConvert([RequiredByDate]) AS DATE) AS [RequiredByDate]
```

---

## Tables to change (24 tables, 53 columns)

Work through this list. For each table: update **CreateTables** first, then **Migrate_*** if one exists.

| # | SQL table | Columns → `DATE` | Migrate script |
|---|-----------|------------------|----------------|
| 1 | **ClosureDatesTbl** | `DateClosed`, `DateReopen`, `NextPreperationDate` | `Migrate_ClosureDatesTbl.sql` |
| 2 | **ContactsAwayPeriodTbl** | `AwayStartDate`, `AwayEndDate` | `Migrate_ContactsAwayPeriodTbl.sql` |
| 3 | **ContactsItemsPredictedTbl** | `NextCoffeeBy`, `NextCleanOn`, `NextFilterEst`, `NextDescaleEst`, `NextServiceEst` | `Migrate_ContactsItemsPredictedTbl.sql` |
| 4 | **ContactsItemSvcSummaryTbl** | `UsageDate` | `Migrate_ContactsItemSvcSummaryTbl.sql` |
| 5 | **ContactsItemUsageTbl** | `DeliveryDate` | `Migrate_ContactsItemUsageTbl.sql` |
| 6 | **ContactsTbl** | `LastDateSentReminder` | `Migrate_ContactsTbl.sql` |
| 7 | **HolidayClosuresTbl** | `ClosureDate` | `Migrate_HolidayClosuresTbl.sql` |
| 8 | **NextPreperationDateByAreasTbl** | `PreperationDate`, `DeliveryDate`, `NextPreperationDate`, `NextDeliveryDate` | `Migrate_NextPrepDateByAreasTbl.sql` |
| 9 | **OrderList** | `Time` *(legacy pivot table; day-level, not clock time)* | *(none — see note below)* |
| 10 | **OrdersTbl** | `OrderDate`, `PrepDate`, `RequiredByDate` | *(none — see note below)* |
| 11 | **PredictedOrdersTbl** | `PrepDate`, `DeliveryDate` | *(none — see note below)* |
| 12 | **RecurringOrderItemsTbl** | `DateLastDone`, `NextDateRequired`, `RequireUntilDate` | *(none — see note below)* |
| 13 | **RepairsTbl** | `DateLogged`, `LastStatusChange` | `Migrate_RepairsTbl.sql` |
| 14 | **SendCheckupEmailTextsTbl** | `DateLastChange` | `Migrate_SendCheckupEmailTextsTbl.sql` |
| 15 | **SentRemindersLogTbl** | `DateSentReminder`, `NextPreperationDate` | `Migrate_SentRemindersLogTbl.sql` |
| 16 | **SysDataTbl** | `LastRecurringDate`, `DateLastPrepDateCalcd`, `MinReminderDate` | `Migrate_SysDataTbl.sql` |
| 17 | **TempCoffeecheckupCustomerTbl** | `NextPreperationDate`, `NextDeliveryDate`, `NextCoffee`, `NextClean`, `NextFilter`, `NextDescal`, `NextService` | `Migrate_TempCoffeecheckupCustomerTbl.sql` |
| 18 | **TempCoffeecheckupItemsTbl** | `NextDateRequired` | `Migrate_TempCoffeecheckupItemsTbl.sql` |
| 19 | **TempOrdersHeaderTbl** | `OrderDate`, `RoastDate`, `RequiredByDate` | `Migrate_TempOrdersHeaderTbl.sql` |
| 20 | **TempOrdersTbl** | `OrderDate`, `RoastDate`, `RequiredByDate` | `Migrate_TempOrdersTbl.sql` |
| 21 | **tmpOrdersReplyTbl** | `NextCoffeeBy` | *(none — see note below)* |
| 22 | **TotalCountTrackerTbl** | `CountDate` | `Migrate_TotalCountTrackerTbl.sql` |
| 23 | **UsedItemGroupsTbl** | `LastItemDateChanged` | `Migrate_UsedItemGroupsTbl.sql` |
| 24 | **VisitLogTbl** | `VisitDate` | *(none — see note below)* |

### Tables with CreateTables-only changes (no `Migrate_*.sql`)

These have `DATE` columns in `CreateTables_LATEST_FIXED.sql` but **no** per-table `Migrate_<name>.sql` in the Sql folder. Data load is handled by **MigrationRunner** generated scripts:

- `OrdersTbl` / `OrderLinesTbl` → `DataMigration_NORMALIZED.sql` (menu **N** / normalized migration)
- `RecurringOrdersTbl` / `RecurringOrderItemsTbl` → same normalized bundle
- `OrderList`, `PredictedOrdersTbl`, `tmpOrdersReplyTbl`, `VisitLogTbl` → `DataMigration_UNNORMALIZED.sql` (menu **!** / unnormalized)

After changing CreateTables, regenerate data migration from MigrationRunner and confirm generated `INSERT` expressions use `CAST(... AS DATE)` or target `DATE` columns (may require a small change in `DmlScriptGenerator.cs` later).

---

## Per-table Migrate scripts (17 files)

Check off each `Migrate_*.sql` as you update `SafeDateConvert` calls:

- [ ] `Migrate_ClosureDatesTbl.sql`
- [ ] `Migrate_ContactsAwayPeriodTbl.sql`
- [ ] `Migrate_ContactsItemsPredictedTbl.sql`
- [ ] `Migrate_ContactsItemSvcSummaryTbl.sql`
- [ ] `Migrate_ContactsItemUsageTbl.sql`
- [ ] `Migrate_ContactsTbl.sql`
- [ ] `Migrate_HolidayClosuresTbl.sql`
- [ ] `Migrate_NextPrepDateByAreasTbl.sql`
- [ ] `Migrate_RepairsTbl.sql`
- [ ] `Migrate_SendCheckupEmailTextsTbl.sql`
- [ ] `Migrate_SentRemindersLogTbl.sql`
- [ ] `Migrate_SysDataTbl.sql`
- [ ] `Migrate_TempCoffeecheckupCustomerTbl.sql`
- [ ] `Migrate_TempCoffeecheckupItemsTbl.sql`
- [ ] `Migrate_TempOrdersHeaderTbl.sql`
- [ ] `Migrate_TempOrdersTbl.sql`
- [ ] `Migrate_TotalCountTrackerTbl.sql`
- [ ] `Migrate_UsedItemGroupsTbl.sql`

---

## Migrate scripts with no date columns (no change)

These `Migrate_*.sql` files have **no** `SafeDateConvert` / date fields — skip them:

`Migrate_AreaPrepDaysTbl.sql`, `Migrate_AreasTbl.sql`, `Migrate_AwayReasonTbl.sql`, `Migrate_ContactTrackedServiceItemsTbl.sql`, `Migrate_ContactTypesTbl.sql`, `Migrate_ContactsAccInfoTbl.sql`, `Migrate_EquipConditionsTbl.sql`, `Migrate_EquipTypesTbl.sql`, `Migrate_InvoiceTypesTbl.sql`, `Migrate_ItemGroupsTbl.sql`, `Migrate_ItemPackagingsTbl.sql`, `Migrate_ItemPrepTypesTbl.sql`, `Migrate_ItemServiceTypesTbl.sql`, `Migrate_ItemsTbl.sql`, `Migrate_ItemUnitsTbl.sql`, `Migrate_PaymentTermsTbl.sql`, `Migrate_PeopleTbl.sql`, `Migrate_PriceLevelsTbl.sql`, `Migrate_RecurranceTypesTbl.sql`, `Migrate_RepairFaultsTbl.sql`, `Migrate_RepairStatusesTbl.sql`, `Migrate_SectionTypesTbl.sql`, `Migrate_TempOrdersLinesTbl.sql`, `Migrate_TrackedServiceItemsTbl.sql`, `Migrate_TransactionTypesTbl.sql`

---

## Existing database (OtterDb) without full remigrate

If you already have a live database and only want to fix column types in place (no drop/recreate), run:

```
TrackerSQL\Data\Metadata\Sql\Alter_DateColumns_DatetimeToDate.sql
```

*(Back up first.)* Re-sync a copy of that script into TrackerMigration if you keep migration assets in one place.

---

## Suggested workflow

1. Edit `CreateTables_LATEST_FIXED.sql` — replace all 53 `DATETIME NULL` with `DATE NULL` (or search/replace ` DATETIME NULL` → ` DATE NULL` in that file only).
2. Update the 17 `Migrate_*.sql` files listed above.
3. Rebuild MigrationRunner; run create + migrate on a test database.
4. Regenerate `DataMigration_NORMALIZED.sql` / `DataMigration_UNNORMALIZED.sql`.
5. Smoke-test: Delivery Sheet, Order Detail, coffee checkup, repairs, recurring orders.

---

## Column reference (copy/paste)

```
ClosureDatesTbl:              DateClosed, DateReopen, NextPreperationDate
ContactsAwayPeriodTbl:        AwayStartDate, AwayEndDate
ContactsItemsPredictedTbl:    NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst
ContactsItemSvcSummaryTbl:    UsageDate
ContactsItemUsageTbl:         DeliveryDate
ContactsTbl:                  LastDateSentReminder
HolidayClosuresTbl:           ClosureDate
NextPreperationDateByAreasTbl: PreperationDate, DeliveryDate, NextPreperationDate, NextDeliveryDate
OrderList:                    Time
OrdersTbl:                    OrderDate, PrepDate, RequiredByDate
PredictedOrdersTbl:           PrepDate, DeliveryDate
RecurringOrderItemsTbl:       DateLastDone, NextDateRequired, RequireUntilDate
RepairsTbl:                   DateLogged, LastStatusChange
SendCheckupEmailTextsTbl:     DateLastChange
SentRemindersLogTbl:          DateSentReminder, NextPreperationDate
SysDataTbl:                   LastRecurringDate, DateLastPrepDateCalcd, MinReminderDate
TempCoffeecheckupCustomerTbl: NextPreperationDate, NextDeliveryDate, NextCoffee, NextClean, NextFilter, NextDescal, NextService
TempCoffeecheckupItemsTbl:    NextDateRequired
TempOrdersHeaderTbl:          OrderDate, RoastDate, RequiredByDate
TempOrdersTbl:                OrderDate, RoastDate, RequiredByDate
tmpOrdersReplyTbl:            NextCoffeeBy
TotalCountTrackerTbl:         CountDate
UsedItemGroupsTbl:            LastItemDateChanged
VisitLogTbl:                  VisitDate
```

*Source: `CreateTables_LATEST_FIXED.sql` in TrackerMigration (53 `DATETIME` columns across 24 tables).*
