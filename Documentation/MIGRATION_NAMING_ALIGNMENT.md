# Migration naming alignment scan

Tracks alignment between **TrackerMigration** (canonical schema + migrate scripts) and **TrackerSQL** application code.

> **Note:** Copies of SQL under TrackerSQL `Data/Metadata/Sql/` and `Migrations/` are **legacy** and excluded from the web project. Do not use them as the living schema. See `Documentation/HARD_PROJECT_RULES.md` Rule #0d.

## Database names (canonical after your migration fix)

| Old (Access / early SQL) | New (SQL Server) |
|--------------------------|------------------|
| `NextPreperationDateByAreasTbl` | `NextPreparationDateByAreasTbl` |
| `PreperationDate` | `PreparationDate` |
| `NextPreperationDate` | `NextPreparationDate` |
| `DoReoccuringOrders` / `DoRecurringOrders` | `DoReccuringOrders` *(DB column — note double `c`)* |
| `GroupItemTypeID` / `GroupItemServiceTypeID` | `GroupReferenceItemID` |
| `HadReoccurItems` / `HadRecurrItems` | `HadRecurringItems` |
| `ReoccurOrderID` (temp items) | `RecurringOrderItemID` |
| `RecurranceTypesTbl` | Still `RecurranceTypesTbl` *(table name unchanged; columns use `RecurringTypeID`)* |
| `RepairsTbl.RelatedOrderID` (OrdersTbl.OrderID) | `RepairsTbl.RelatedOrderLineID` (OrderLinesTbl.OrderLineID) — run `Alter_RepairsTbl_RelatedOrderLineID.sql` |

**Hard rule:** Application code must use **`HadRecurringItems` / Recurring*** — never `HadReoccurItems`, `HadRecurrItems`, or other `Reoccur*` forms in repos, models, managers, or pages. See `Documentation/HARD_PROJECT_RULES.md` Rule #0c. Column renames belong in **TrackerMigration**, not under TrackerSQL.

All business date columns should be **`DATE`**, not `DATETIME`.

---

## Fixed in this pass (repos, models, managers + supporting code)

### Models
- [x] `NextPreparationDateByArea` — was `NextPreperationDateByArea`; properties `PreparationDate`, `NextPreparationDate`
- [x] `PreparationSummaryItem` — was `PreperationSummaryItem`
- [x] `ClosureDate`, `SentRemindersLog`, `TempCoffeecheckupCustomer`, `AreaPrepDateRow`, `CoffeeCheckupModels` — `NextPreparationDate`
- [x] `CoffeeCheckupModels.ItemContactRequires` — `RecurringOrder`, `RecurringOrderItemID`
- [x] `SysData` — `DoReccuringOrders`, `GroupReferenceItemID`

### Repositories
- [x] `NextPrepDateByAreaRepository` — table/columns/SQL
- [x] `CoffeeCheckupRepository` — joins and column reads
- [x] `SentRemindersLogRepository` — `NextPreparationDate`
- [x] `TempCoffeeCheckupRepository` — `NextPreparationDate`, recurring item fields
- [x] `SysDataRepository` — `DoReccuringOrders`, `GroupReferenceItemID`; `GetGroupReferenceItemId()`
- [x] `RecurranceTypeRepository` — table `RecurranceTypesTbl` (was wrong `RecurranceTypeTbl`)
- [x] `PreparationSummaryRepository` — was `PreperationSummaryRepository`

### Managers
- [x] `CoffeeCheckupManager` — `NextPreparationDate`, `RecurringOrder` / `RecurringOrderItemID`, `TrackerTools` method names
- [x] `CoffeeCheckupEmailManager` — `NextPreparationDate`
- [x] `OrderDoneManager` — `SyncRecurringOrderLastDone`
- [x] `OrderManager`, `RepairManager` — `GetNextPreparationDateByCustomerID`

### Also updated (required for build)
- [x] `Classes/TrackerTools.cs` — SQL table/columns; method names `SetNextPreparationDateByArea`, `GetNextPreparationDateByCustomerID`, etc.
- [x] `Classes/AreaDeliveryMatrix.cs`, `Classes/DateCalculator.cs`
- [x] `Pages/PreperationSummary.aspx.cs`, `SentRemindersSheet.aspx.cs`, `OrderDetail.aspx.cs`
- [x] `Tools/SystemTools.aspx.cs` — prep grid SQL

**Build:** `TrackerSQL.csproj` compiles after these changes.

---

## Still using old names (not updated — next pass)

These **Controls** and legacy paths still reference old table/column names or `Reoccur*` properties. They will fail at **runtime** against the new database until migrated:

| Area | Files |
|------|--------|
| Legacy prep-by-area control | `Controls/NextPrepDateByAreaTbl.cs` |
| Coffee checkup temp tables | `Controls/TempCoffeeCheckup.cs`, `Controls/ItemContactRequires.cs` |
| Contact reminder SQL | `Controls/ContactToRemindWithItems.cs`, `Controls/ContactToRemindDetails.cs` |
| Usage / may-need queries | `Controls/ContactsThatMayNeedNextWeek.cs`, `Controls/CustomersWithDatesAndUsageTbl.cs`, `Controls/OrderCheckTbl.cs` |
| Sent reminders legacy | `Controls/SentRemindersLogTbl.cs` |
| Page UI labels only | `Pages/PreperationSummary.aspx` (title still says "Preperation") |
| Recurring UI text | `Pages/RecurringOrderDetails.aspx`, `Pages/SendCoffeeCheckup.aspx` (`ReoccurOrder` grid field) |

`Repositories/RecurringOrdersRepository.cs` already uses `RecurranceTypesTbl` (matches DB).  
**Live OtterDb (2026-07-16):** `ItemGroupsTbl` / `UsedItemGroupsTbl` still use column **`GroupItemServiceTypeID`** (not renamed to `GroupReferenceItemID`). Repos must match the live column. `SysDataTbl.GroupReferenceItemID` is the group service-type id stored in system data.

---

## Suggested next steps

1. Replace or retire `Controls/NextPrepDateByAreaTbl.cs` — repo `NextPrepDateByAreaRepository` is already correct.
2. Update `Controls/TempCoffeeCheckup.cs` to `NextPreparationDate`, `RecurringOrderItemID`, `NextPreparationDateByAreasTbl`.
3. Run app smoke tests: **Delivery Sheet**, **Order Detail**, **Coffee Checkup**, **System Tools** prep grid, **Recurring Orders**.
4. Optional: rename UI page `PreperationSummary.aspx` → `PreparationSummary.aspx` (requires route/menu updates).

---

## Quick grep (find stragglers)

```powershell
rg -i "Preperation|NextPreperationDateByArea|ReoccurOrder|ReoccurID|DoReoccuring|GroupItemServiceTypeID" --glob "*.cs" c:\SRC\ASP.net\TrackerSQL
```
