# TrackerSQL Controls → Repositories/Managers Gap Analysis

**Date:** 2026-06-15 (refreshed)  
**Primary source of truth:** [`README.md`](../../README.md) (root migration guide, last updated 2026-06-15)  
**Supplementary (database renames only):** [`Migrations/README_MIGRATION.md`](../../Migrations/README_MIGRATION.md)  
**Scope:** All 73 files in `TrackerSQL.Controls`  
**Status:** Analysis only — no code changes in this pass.

> Older notes (`DevTools/Documentation/README_TrackerDb_Migration.md`, `Classes/Sql`, `Classes/Poco`, ObjectDataSource patterns) are **historical context only**. They are not the target architecture.

---

## 1. Target Architecture (from root README)

```text
TrackerSQL.Models          Data-only POCO models
TrackerSQL.Repositories    SQL Server data access only (RepositoryBase<T>, TrackerSQLDb)
TrackerSQL.Managers        Business rules, grouping, totals, display preparation — no SQL
.aspx / .aspx.cs           WebForms events, state, binding — no SQL
```

**Responsibility mapping:**

| Legacy Controls responsibility | New home |
|------------------------------|----------|
| SQL query / reader mapping | Repository |
| Filtered lookup, join, aggregate | Bespoke repository method |
| Grouping, totals, calculations, email orchestration | Manager |
| Grid binding, querystring/session | Page code-behind |
| Data properties | Model |

**This is not greenfield.** Models and repositories already exist. Only add **missing bespoke** legacy behaviour still called by managers/pages. Do not recreate models, bulk-rewrite thin repos, or duplicate generic CRUD.

---

## 2. Executive Summary

| Metric | Count |
|--------|------:|
| Controls files scanned | 73 |
| Files with SQL/data-access logic | 58 |
| DTO/projection-only (no migration target) | 15 |
| Repository files in `Repositories/` | 47 |
| Managers | 14 |
| Active files still referencing legacy Controls | ~50+ (pages, managers, classes, aspx) |

### Key findings (unchanged in substance, tightened by README)

1. **Models largely exist** in `TrackerSQL.Models` — do not recreate.
2. **Thin `RepositoryBase<T>` repos are intentional** — extend only for bespoke legacy methods.
3. **Largest gaps remain domain workflows**, not lookup CRUD:
   - Orders + order lines + temp orders (`OrderTbl`, `OrderDetailDAL`, `TempOrdersDAL`)
   - Coffee checkup pipeline (`OrderCheckTbl`, `ContactsThatMayNeedNextWeek`, `TempCoffeeCheckup`, `SentRemindersLogTbl`)
   - Contact lifecycle (`CustomersTbl` → `ContactsRepository` incomplete)
   - Repairs (**no `RepairsRepository`**)
   - Scheduling (`NextPrepDateByAreaTbl` — stub repo, all bespoke methods missing)
   - Item rotation (`UsedItemGroupTbl` — stub repo)
4. **Nine repository files are broken auto-gen stubs** — wrong namespace (`TrackerSQL.Classes.Sql`), syntax errors, invalid `Map` override, literal `` `$"..."` `` SQL (README §16 search checklist).
5. **Cross-cutting technical debt** beyond missing methods:
   - `RepositoryBase.GetAll(string SortBy)` concatenates sort strings (README §9 — forbidden)
   - `ItemsRepository.GetAll(string SortBy)` same issue
   - `ContactSummariesRepository` accepts raw `whereFilter` strings (README §9 — replace with explicit methods)
   - `OrdersRepository.UpdateOrderHeader` uses legacy column names (`CustomerID`, `ToBeDeliveredBy`) — violates README §14
   - Several models/repos still use legacy property names (e.g. `Contact.CompanyName`) — schema alignment debt

### Risk priority

| Priority | Area | Risk |
|----------|------|------|
| P0 | Orders + lines + temp orders + fix `UpdateOrderHeader` | **High** |
| P0 | Coffee checkup pipeline | **High** |
| P0 | NextPrepDateByArea / delivery scheduling | **High** |
| P0 | Fix broken repo stubs (namespace + compile) | **High** |
| P1 | Contact reminder & disable workflows | **High** |
| P1 | Repairs | **High** |
| P1 | Contact usage / prediction lines | **Medium** |
| P2 | Item group sort/rotation | **Medium** |
| P2 | Recurring orders (partially migrated) | **Medium** |
| P2 | Sort/filter whitelist on touched repos | **Medium** |
| P3 | Thin lookup desc-by-id helpers | **Low** |

---

## 3. Naming & Schema Standards

### 3.1 Contact / item / order naming (README §3, §13, §14)

| Legacy (Controls) | Target (SQL / models / repos) |
|-------------------|-------------------------------|
| Customer / Client / Company | **Contact** |
| CustomerID / ClientID | **ContactID** |
| CompanyName / CoName / ContactCompany | **ContactName** |
| CustomerTypeID | **ContactTypeID** |
| CustomerTracked… | **ContactTracked…** |
| ItemType / ItemTypeID | **Item / ItemID** |
| ItemTypeDesc | **ItemDesc** or **ItemName** (match schema) |
| ServiceTypeID (→ ItemServiceTypesTbl) | **ItemServiceTypeID** |
| QuantityOrdered | **QtyOrdered** |
| RoastDate / PreperationDate | **PrepDate** |
| ToBeDeliveredBy | **ToBeDeliveredByID** |
| InternalCustomerIds | **InternalContactIDs** |
| GroupItemTypeID | **GroupItemServiceTypeID** |
| DoReoccuringOrders | **DoRecurringOrders** |
| CustomerConstants.SundryCustomerID | **ContactConstants.SundryContactID** |

### 3.2 Database table renames (Migrations/README_MIGRATION.md)

| Legacy table | Migrated table | Model |
|--------------|----------------|-------|
| `ClientUsageTbl` | `ContactsItemsPredictedTbl` | `ContactsUsage` |
| `ClientUsageLinesTbl` | `ContactsUsageTbl` | `ContactUsageLine` |
| `ItemUsageTbl` | `ContactsItemUsageTbl` | `ContactsItemUsage` |
| `CustomersTbl` | `ContactsTbl` | `Contact` |
| `CustomerTrackedServiceItemsTbl` | `ContactTrackedServiceItemsTbl` | `ContactTrackedServiceItem` |
| `PersonsTbl` | `PeopleTbl` | `Person` |
| `PackagingTbl` | `ItemPackagingsTbl` | `ItemPackaging` |
| `ReoccuringOrderTbl` | `RecurringOrdersTbl` + `RecurringOrderItemsTbl` | `RecurringOrder` |

### 3.3 Alignment rule (README §4)

**Target:** `SQL column == model property == repository projection`

Temporary `AS` aliases are acceptable only while schema rename is incomplete. Do not add aliases to keep old pages compiling — update consumers instead.

**Known alignment debt:**

| Location | Issue | Action |
|----------|-------|--------|
| `Models/Contact.cs` | Property `CompanyName` | Rename to `ContactName` when DB column confirmed |
| `ContactsRepository` | Maps/selects `CompanyName` | Align with `ContactName` |
| `ContactSummariesRepository` | Column map key `CompanyName` | Rename to `ContactName` |
| `OrdersRepository.UpdateOrderHeader` | SQL uses `CustomerID`, `ToBeDeliveredBy` | Fix to §14 names |
| `ContactUsageLinesRepository` | `TableName = ContactsItemSvcSummaryTbl` | Verify vs `ContactsUsageTbl` |

### 3.4 Orders normalisation (README §14)

**OrdersTbl header:** `OrderID`, `ContactID`, `OrderDate`, `PrepDate`, `RequiredByDate`, `ToBeDeliveredByID`, `Confirmed`, `Done`, `Packed`, `Notes`, `PurchaseOrder`, `InvoiceDone`

**OrderLinesTbl:** `OrderLineID`, `OrderID`, `ItemID`, `QtyOrdered`, `PrepTypeID`, `PackagingID`

Do **not** use in new SQL: `CustomerID`, `RoastDate`, `ToBeDeliveredBy`, `ItemTypeID`, `QuantityOrdered`.

---

## 4. Deprecated Namespaces & Paths (README §2)

Do **not** create or retain migrated code in:

```text
TrackerSQL.Controls
TrackerSQL.Classes.Poco
TrackerSQL.Classes.Sql
```

Historical path `Classes/Sql/SomeRepository.cs` → **`Repositories/SomeRepository.cs`** with `namespace TrackerSQL.Repositories`.

**Files in `Repositories/` still using wrong namespace** (must fix before use):

| File | Current namespace | Required |
|------|-------------------|----------|
| `NextPrepDateByAreaRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |
| `UsedItemGroupRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |
| `SentRemindersLogRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |
| `OrderCheckRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |
| `TempOrdersHeaderRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |
| `TrackedServiceItemRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |
| `DeliveryItemsRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |
| `LogRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |
| `OrderItemRepository.cs` | `TrackerSQL.Classes.Sql` | `TrackerSQL.Repositories` |

---

## 5. Cross-Cutting Technical Debt

| Issue | Where | README rule | Remediation |
|-------|-------|-------------|-------------|
| Raw sort concatenation | `RepositoryBase.GetAll/GetLookupValues/GetAllEnabled`, `ItemsRepository.GetAll` | §9 | Add `MapSortColumnOrDefault` whitelist per repo when touched |
| Raw `whereFilter` | `ContactSummariesRepository.GetAllContactSummaries`, legacy `CustomersAwayTbl` | §9 | Replace with explicit methods (`SearchByContactName`, etc.) |
| Order ID string concat in SQL | `OrdersRepository.UpdateOrderHeader` | §8, §14 | Table-valued param or `IN (@Id1,@Id2…)` with bound params |
| `OrderCheckRepository` maps to fake table | Stub assumes `OrderCheckTbl` | §6 | Delete table mapping; use query repo or `OrdersRepository` methods |
| Managers instantiate Controls | `CustomerManager`, `RepairManager`, `OrderManager`, `CoffeeCheckupManager`, etc. | §10 | Wire to repositories after bespoke methods exist |
| ObjectDataSource → Controls | Various `.aspx` (CustomerDetails, ItemGroups, SendCoffeeCheckup, etc.) | §11 | Explicit code-behind binding |
| Auto-gen TODO placeholders | Multiple stub repos | §16 | Fix structure, implement bespoke methods from Controls |

---

## 6. Method Classification Legend

| Class | Meaning | Migrate to |
|-------|---------|------------|
| **CRUD** | GetById/GetAll/Insert/Update/Delete | `RepositoryBase<T>` — do not duplicate |
| **Lookup** | Desc by id, name by id | Bespoke repo method if still called |
| **Filtered** | WHERE/LIKE/enabled | Bespoke repo with **named params**, no raw filter strings |
| **Joined** | Multi-table projection | Bespoke repo or summary model |
| **Aggregate** | COUNT/SUM/MIN/MAX | Bespoke repo method |
| **Business** | Email, date math, rotation logic | Manager (calls repo) |
| **Obsolete** | ObjectDataSource-only wrapper | Remove; bind from page/manager |

---

## 7. DTO / Projection Classes (no repository migration)

Map to existing models; do not recreate Controls-equivalent classes.

| Old Class | Intended Model(s) |
|-----------|-------------------|
| `AreaTblData` | `Area` |
| `ClientAwayPeriod` | `ContactsAwaySummary` |
| `ContactToRemindDetails` | Composite — manager builds from repos |
| `ContactsThayMayNeedData` | `ContactsWithDatesAndUsage` |
| `CustomerData` | `Contact` |
| `CustomerSummary` | `ContactSummary` |
| `DeliveryItemsTbl` | `DeliverySheetModels` |
| `ItemContactRequires` | Checkup line DTO |
| `OrderCheckData` | Conflict-check result (not a table) |
| `OrderCls` | `Order` + lines |
| `OrderDetailData` | `OrderLine` |
| `OrderHeaderData` | Order header fields — **retire** after `Order` model used everywhere |
| `OrderTblData` | `Order` / `OrderLine` — legacy combined row |
| `ReoccuringOrderTbl` | `RecurringOrder` |
| `ReoccuringOrderExtData` | `RecurringOrderSummary` |
| `TempOrdersData` | `TempOrdersHeader` + `TempOrdersLine` |

---

## 8. Detailed Gap Analysis by Legacy Control Class

---

### CustomersTbl → ContactsTbl

| Field | Value |
|-------|-------|
| **Old table(s)** | `CustomersTbl` → `ContactsTbl` |
| **New model** | `Contact` (note: `CompanyName` property → align to `ContactName`) |
| **New repository** | `ContactsRepository` (partial) |
| **Manager** | `CustomerManager` → should call `ContactsRepository`, not Controls |
| **Overall risk** | **High** |

| Public Method | Classification | Existing Equivalent | Missing | Callers | Recommended Change | Risk |
|---------------|----------------|---------------------|---------|---------|-------------------|------|
| `GetAllCustomers(SortBy)` | CRUD | `GetAll()` | Whitelisted sort | Legacy ODS | `GetAll(sortBy)` with mapper | Low |
| `GetAllCustomerNames()` / `(sortBy)` | Lookup | `GetAllCompanyNames()` | Rename to contact lookups; disabled `_` prefix | Dropdowns | `GetContactLookups()` + manager formatting | Low |
| `GetCustomerByCustomerID(id)` | Lookup | `GetById()` | — | CustomerDetails, CustomerManager, RepairDetail, OrderDoneManager, LogTable | Wire to repo | Low |
| `InsertCustomer` / `UpdateCustomer` | CRUD | — | **Insert/Update** | CustomerDetails, register | Add to `ContactsRepository` or extend `RepositoryBase` | Medium |
| `GetReminderCount` | Lookup | — | **Yes** | Checkup | `GetReminderCount(contactId)` | Medium |
| `SetEquipDetailsIfEmpty` | Filtered update | — | **Yes** | CustomerManager, RepairManager | Repo method | Medium |
| `IncrementReminderCount` | Update | — | **Yes** | CustomerDetails | Repo method | Medium |
| `SetSentReminderAndIncrementReminderCount` | Update | — | **Yes** | CoffeeCheckupManager | Repo method | Medium |
| `ResetReminderCount(id, forceEnable)` | Business | — | **Yes** | CustomerDetails, GeneralTrackerDbTools, CustomerManager | Repo + manager | Medium |
| `DisableCustomer` / `DisableCustomerReminders` / `DisableCustomerIfReminderToHigh` | Business | `DisableInactiveContacts` (different) | **Yes** | CustomerManager, checkup | `DisableContact`, `DisableReminders`, etc. | **High** |
| `GetCustomerByName` | Lookup | — | **Yes** | Merge tool | `GetByContactName` | Low |
| `GetAllCustomerWithNameLIKE` / `EmailLIKE` | Filtered | — | **Yes** | CustomerManager, OrderManager, MergeCustomersFromQB | `SearchByContactNameLike`, `SearchByEmailLike` | Medium |
| `GetCustomerNameById` static | Lookup | Partial | **Yes** | Various | `GetContactNameById` | Low |

---

### CustomerDAL

| Public Method | Classification | Existing | Missing | Recommended | Risk |
|---------------|----------------|----------|---------|-------------|------|
| `GetAllCustomers(SortBy)` | Joined → `CustomerData` | `ContactsRepository.GetAll()` | Use `Contact` directly | Retire DAL | Low |

---

### CustomerSummaryDAL → ContactSummariesRepository

| Public Method | Classification | Existing | Gap | Recommended | Risk |
|---------------|----------------|----------|-----|-------------|------|
| `GetAllCustomerSummarys(...)` | Joined/filtered | `GetAllContactSummaries` | `whereFilter` string is legacy anti-pattern | Replace filter with explicit repo methods; whitelist sort | Low–Medium |

**Example migrated pattern already done:** joined summary with enabled filter. **Remaining debt:** raw `whereFilter` parsing.

---

### CompanyNames

| Public Method | Classification | Existing | Missing | Callers | Recommended | Risk |
|---------------|----------------|----------|---------|---------|-------------|------|
| `GetAll()` | Lookup | `GetAllCompanyNames()` | ContactName alignment | Legacy | `GetEnabledContactLookups()` | Low |
| `GetAllDemo()` | Filtered | — | **Yes** | Demo pages | `GetDemoContacts()` | Low |
| `GetCompanyNameByCompanyID(id)` | Lookup | — | **Yes** | RepairStatusChange | `GetContactNameById` | Low |

---

### ContactType (Controls class — not ContactTypesRepository)

| Public Method | Classification | Existing | Missing | Callers | Recommended | Risk |
|---------------|----------------|----------|---------|---------|-------------|------|
| `GetAllContacts(SortBy)` | Filtered projection | — | **Yes** | Admin | Thin query on ContactsTbl | Low |
| `UpdateContact` | CRUD | No Update on ContactsRepository | **Yes** | Admin | Add Update | Medium |
| `UpdateContactTypeIfInfoOnly` | Business update | — | **Yes** | GeneralTrackerDbTools | Repo method | Medium |

---

### CustomersAccInfoTbl → ContactsAccInfoTbl

| Field | Value |
|-------|-------|
| **New model** | `ContactsAccInfo` |
| **New repository** | `ContactsAccInfoRepository` (**empty file**) |
| **Overall risk** | **High** |

| Public Method | Classification | Missing | Callers | Recommended | Risk |
|---------------|----------------|---------|---------|-------------|------|
| `GetAll` | CRUD | **All** | Support tables | `RepositoryBase<ContactsAccInfo>` | Medium |
| `GetByCustomerID` | Lookup | **Yes** | DeliverySheet | `GetByContactId` | **High** |
| `GetByPaymentTypeIDByCustomerID` | Lookup | **Yes** | — | `GetPaymentTermIdByContactId` | Low |
| `GetCustomersInvoiceType` | Lookup | **Yes** | — | `GetInvoiceTypeIdByContactId` | Low |
| Insert/Update/Delete | CRUD | **Yes** | DeliverySheet | Base CRUD | Medium |

---

### CustomersAwayTbl → ContactsAwayPeriodRepository

| Public Method | Classification | Existing | Missing | Callers | Recommended | Risk |
|---------------|----------------|----------|---------|---------|-------------|------|
| `GetCustomersAway(whereFilter, sortBy)` | Joined | `GetAwaySummaries` | Legacy `whereFilter` | Away pages | Use `GetAwaySummaries` + explicit filters only | Medium |
| `InsertAwayPeriod` / `UpdateAwayPeriod` / `DeleteAwayPeriod` | CRUD | Base CRUD | Wire callers | Away pages | Wire | Low |
| `GetAwayPeriodById` | Lookup | `GetById` | — | — | Wire | Low |
| `GetAllAwayReasons` | Lookup | `AwayReasonRepository.GetAll` | — | Away pages | Wire | Low |
| `IsCustomerAwayOnDate` | Filtered | — | **Yes** | CoffeeCheckupManager | `IsContactAwayOnDate` | Medium |
| `GetAwayCustomerIds` | Filtered | — | **Yes** | Checkup | `GetAwayContactIdsInRange` | Medium |

---

### CustomersWithDatesAndUsageTbl

| Public Method | Classification | Existing | Missing | Recommended | Risk |
|---------------|----------------|----------|---------|-------------|------|
| `GetCustomerWithDatesAndUsage(id)` | Joined | — | **Yes** | Manager compositing Contacts + NextPrep + ContactsUsage | Medium |

---

### ClientUsageTbl → ContactsItemsPredictedTbl

| Public Method | Classification | Existing | Missing | Callers | Recommended | Risk |
|---------------|----------------|----------|---------|---------|-------------|------|
| `GetUsageData` | Lookup | `ContactsUsageRepository.GetByContactId` | — | OrderDone | Wire | Low |
| `UsageDataExists` | Lookup | — | **Yes** | GeneralTrackerDbTools | `ExistsByContactId` | Medium |
| `GetAverageConsumption` | Aggregate | — | From model | GeneralTrackerDbTools | Manager reads repo | Low |
| `UpdateUsageCupCount` / `Update` / `ForceNextCoffeeDate` | Update | Base Update partial | **Yes** | Usage tools | Bespoke update methods | Medium |

---

### ClientUsageLinesTbl → ContactsUsageTbl

| Field | Value |
|-------|-------|
| **New repository** | `ContactUsageLinesRepository` — **verify TableName** vs schema |
| **Overall risk** | **High** |

| Public Method | Classification | Missing | Callers | Recommended | Risk |
|---------------|----------------|---------|---------|-------------|------|
| `GetAllClientUsageLinesTbl` | Filtered | **Yes** | Usage UI | `GetByContactId` + sort whitelist | Medium |
| `InsertItemsUsed` / `UpdateItemsUsed` | CRUD | Verify table | Usage UI | Fix TableName, use base | Medium |
| `GetCustomerInstallDate` | Aggregate | **Yes** | GeneralTrackerDbTools | `GetInstallDate` | Medium |
| `GetAllCustomerServiceLines` | Filtered | **Yes** | Prediction | `GetByContactAndItemServiceType` | Medium |
| `GetLast10UsageLines` / `GetLatestUsageData` | Filtered | **Yes** | GeneralTrackerDbTools | Recent-line queries | Medium |

---

### ItemUsageTbl → ContactsItemUsageTbl

| Public Method | Classification | Existing | Missing | Callers | Recommended | Risk |
|---------------|----------------|----------|---------|---------|-------------|------|
| `GetAllItemsUsed` | Filtered | `GetByContactId` | — | Usage UI | Wire | Low |
| `GetLastItemsUsed(id, itemServiceTypeId)` | Filtered | — | **Yes** | OrderManager, NewOrderDetail | `GetLastByItemServiceType` | Medium |
| `GetLastMaintenanceItem` | Filtered | — | **Yes** | Maintenance | Service-type query | Low |
| Insert/Update/Delete | CRUD | Base | — | Usage UI | Wire | Low |

---

### ItemTypeTbl → ItemsTbl

| Public Method | Classification | Existing | Missing | Callers | Recommended | Risk |
|---------------|----------------|----------|---------|---------|-------------|------|
| `GetAll` / `GetAllItemDesc` | CRUD/Lookup | `GetAll()` | Lookup list formatting | Many | `GetLookupList` pattern + whitelist sort | Low |
| `GetAllItemsNotInItemGroup` | Joined | — | **Yes** | ItemGroups | `ItemsRepository.GetNotInGroup` | Medium |
| `GetItemTypeDescById` | Lookup | — | **Yes** | Many | `GetItemDescById` | Medium |
| `GetItemTypeSKU` / `GetItemSortOrder` | Lookup | — | **Yes** | — | Scalar methods | Low |
| `GetServiceID` / `GetServiceTypeForItem` | Lookup | — | **Yes** | OrderManager | `GetItemServiceTypeId` | Medium |
| `GetItemUnitOfMeasure` | Join join | — | **Yes** | CoffeeCheckupEmailManager, OrderDetail | Items + ItemUnits join | Medium |
| `GetAllItemIDsofServiceType` / `GetAllItemsofServiceType` | Filtered | — | **Yes** | Orders | Filter by `ItemServiceTypeID` | Medium |
| `GetAllGroupTypeItems` | Filtered | — | **Yes** | Group orders | SysDataRepository + ItemsRepository | Medium |
| CRUD | CRUD | `Insert/Update/Delete` | — | Item admin | Wire | Low |
| `GroupOfThisNameExists` | Lookup | — | **Yes** | ItemGroups | Exists query | Low |

---

### ItemGroupTbl → ItemGroupsRepository (thin)

| Public Method | Classification | Missing | Callers | Risk |
|---------------|----------------|---------|---------|------|
| `GetAllByGroupItemTypeID` | Filtered | **Yes** | ItemGroups.aspx | Medium |
| `DeleteGroupItemFromGroup` | Filtered delete | **Yes** | ItemGroups.aspx | Medium |
| `UpdateItemsSortPos` | Update | **Yes** | GroupItemDetail | Medium |
| Navigation (`GetFirst/Prev/Next/Last…`) | Lookup | **Yes** | GroupItemDetail | Medium |
| `IncItemSortPos` / `DecItemSortPos` | Business | **Yes** | GroupItemDetail | Medium |
| `GetItemIdsForGroup` | Lookup | **Yes** | Order logic | Medium |
| Generic CRUD | CRUD | Use base | ItemGroups | Low |

---

### UsedItemGroupTbl → UsedItemGroupRepository (broken stub)

| Public Method | Classification | Missing | Callers | Risk |
|---------------|----------------|---------|---------|------|
| Fix stub (namespace, compile) | Infrastructure | **Yes** | — | **High** |
| CRUD | CRUD | **Yes** | — | Medium |
| `ContactLastGroupItem` | Lookup | **Yes** | OrderDetail, NewOrderDetail | **High** |
| `GetNextGroupItem` | Business/joined | **Yes** | Order flows | **High** |
| `UpdateIfGroupItem` / `GetLastUsedItemID` / `ChangeItemIDToGroupIfItWas` | Lookup/update | **Yes** | Order insert | Medium |

---

### OrderTbl + OrderDetailDAL + OrderData + OrderItemTbl

| Field | Value |
|-------|-------|
| **New model** | `Order`, `OrderLine` |
| **New repository** | `OrdersRepository` (partial), `OrderItemRepository` (stub) |
| **Overall risk** | **High** |

| Public Method | Source | Classification | Existing | Missing | Callers | Risk |
|---------------|--------|----------------|----------|---------|---------|------|
| `LoadOrderHeader` | OrderTbl | Joined | — | **Yes** | OrderDetail | High |
| `InsertNewOrderLine` | OrderTbl | Insert line | — | **Yes** | OrderManager, pages | High |
| `GetLastOrderAdded` | OrderTbl | Lookup | — | **Yes** | Order flows | Medium |
| `GetOrderByID` | OrderTbl | Lookup | `GetById` partial | — | Pages | Medium |
| `UpdateIncDeliveryDateBy7` | OrderTbl | Update | — | **Yes** | RepairManager | Medium |
| `UpdateOrderDeliveryDate` | OrderTbl | Update | — | **Yes** | RepairManager, OrderManager | Medium |
| `UpdateSetDoneByID` | OrderTbl | Update | — | **Yes** | OrderManager, OrderDone | High |
| `UpdateSetInvoiced` | OrderTbl | Update | — | **Yes** | OrderManager | High |
| `DeleteOrderById` | OrderTbl | Delete | — | **Yes** | OrderManager, pages | High |
| `UpdateOrderNotes` | OrderTbl | Update | — | **Yes** | Repairs | Medium |
| `LoadOrderDetailData` | OrderDetailDAL | Joined | — | **Yes** | OrderDetailManager | High |
| Line Insert/Update/Delete | OrderDetailDAL | CRUD | — | **Yes** | OrderDetail | High |
| `GetDistinctOrders` | OrderData | Joined/filtered | — | **Yes** | Order list | Medium |
| `LoadOrderSummary` / bulk header update | OrderItemTbl | Joined/update | Partial | **Yes** | — | Medium |

All new SQL must use **ContactID**, **ItemID**, **QtyOrdered**, **PrepDate**, **ToBeDeliveredByID** (README §14).

---

### OrderDataControl → OrdersRepository

| Public Method | Classification | Existing | Gap | Recommended | Risk |
|---------------|----------------|----------|-----|-------------|------|
| `UpdateOrderHeader` | Bulk update | Method exists | Legacy columns; order IDs concatenated into SQL | Rewrite with §14 columns + parameterized ID list | **High** |

---

### OrderCheck + OrderCheckTbl

**Do not use stub `OrderCheckRepository` as a table repo** — legacy logic is multi-table queries, not CRUD on `OrderCheckTbl`.

| Public Method | Source | Classification | Missing | Callers | Recommended | Risk |
|---------------|--------|----------------|---------|---------|-------------|------|
| `GetSimilarItemInOrders` | OrderCheck | Joined | **Yes** | OrderDetail | `OrdersRepository` or `CoffeeCheckupQueriesRepository` | High |
| `GetCustomersWithoutOrderConflicts` | OrderCheckTbl | Joined | **Yes** | CoffeeCheckupManager | New query repo | High |
| `GetCustomerTypicalItems` | OrderCheckTbl | Joined | **Yes** | CoffeeCheckupManager | Same | High |
| `HasCoffeeOrdersInDateRange` / `GetCoffeeOrdersInDateRange` | OrderCheckTbl | Aggregate/joined | **Yes** | CoffeeCheckupManager | Same | Medium |

---

### TempOrdersDAL + TempOrdersHeaderTbl + TempOrdersLinesTbl

| Public Method | Classification | Missing | Callers | Risk |
|---------------|----------------|---------|---------|------|
| Fix `TempOrdersHeaderRepository` stub | Infrastructure | **Yes** | — | High |
| Create `TempOrdersLinesRepository` | Infrastructure | **Yes** | OrderManager | High |
| `TempOrdersDAL.Insert` | Business | **Yes** | OrderManager | High |
| `HasCoffeeInTempOrder` | Aggregate | **Yes** | OrderDone | Medium |
| `MarkTempOrdersItemsAsDone` | Bulk update | **Yes** | OrderDoneManager | High |
| `KillTempOrdersData` | Delete all | **Yes** | OrderManager | Medium |
| Header/lines CRUD + `DeleteByOriginalID` | CRUD | **Yes** | OrderDone, OrderManager | High |

---

### ReoccuringOrderDAL → RecurringOrdersRepository

| Public Method | Classification | Existing | Missing | Callers | Risk |
|---------------|----------------|----------|---------|---------|------|
| `GetAll` overloads | Joined/filtered | `GetSummaries` | Filter parity | RecurringOrderDetails | Medium |
| `GetByReoccuringOrderByID` | Lookup | `GetById` | — | RecurringOrderDetails | Low |
| Date calculation methods | Business | Partial | Manager owns calc | DAL | Low–Medium |
| CRUD | CRUD | Done | — | Pages | Low |
| `SetReoccuringItemsLastDate` | Bulk update | — | **Yes** | Batch | Medium |
| `SetReoccuringOrderDates` | Update | Partial | **Yes** | Fulfillment | Medium |

---

### NextPrepDateByAreaTbl → NextPrepDateByAreaRepository (broken stub)

| Public Method | Classification | Missing | Callers | Risk |
|---------------|----------------|---------|---------|------|
| Fix stub | Infrastructure | **Yes** | — | **High** |
| `GetPrepDataForCustomer` | Joined | **Yes** | Checkup, customer | High |
| `GetAll` | CRUD | Fix base | DateMatrixBuilder | High |
| `GetAllDeliveryDates` / `GetAllIDsByDate` | Lookup/filtered | **Yes** | Admin | Medium |
| `UpdatePrepDataForArea` / `InsertPrepDataForArea` | CRUD | **Yes** | Prep admin | High |
| `MoveDeliveryDate` / `UpdateDeliveryDateByID` | Bulk/update | **Yes** | Admin | High |
| `GetNextDeliveryDate(contactId)` | Joined lookup | **Yes** | **RepairManager** | **High** |

---

### AreaTblDAL + AreaPrepDaysTbl

| Public Method | Source | Existing | Missing | Risk |
|---------------|--------|----------|---------|------|
| Area lookups | AreaTblDAL | `AreasRepository` | `GetByName` | Low |
| `GetAreaIdByCustomerId` | AreaTblDAL | Partial | `GetAreaIdByContactId` | Low |
| `GetPrepRulesForArea` | AreaTblDAL | **Done** | — | Low |
| `GetAllByAreaId` | AreaPrepDaysTbl | — | **Yes** | Medium |
| AreaPrepDays CRUD | AreaPrepDaysTbl | **Done** | — | Low |

---

### ActiveDeliveryData → DeliveryRepository / DeliverySheetRepository

**Status: largely migrated.** Wire remaining callers; retire Control.

---

### HolidayClosureProvider → HolidayClosuresRepository + new Manager

| Public Method | Classification | Target | Missing | Callers | Risk |
|---------------|----------------|--------|---------|---------|------|
| `IsClosed` / `GetRange` / `AdjustDate` / `AdjustPair` | Business | `HolidayClosureManager` | **All** | OrderDetail, CoffeeCheckupManager | Medium |
| `Insert` / `Delete` | CRUD | `HolidayClosuresRepository` | Wire; remove TrackerDb from Control | Holiday tools | Medium |
| `IsThereAHolodayComing` | Lookup | Manager | **Yes** | Checkup | Low |

---

### Coffee Checkup Cluster

| Legacy class | Key method(s) | Target | Missing | Callers | Risk |
|--------------|---------------|--------|---------|---------|------|
| `ContactsThatMayNeedNextWeek` | `GetContactsThatMayNeedNextWeek` | `CoffeeCheckupQueriesRepository` + manager | **Yes** | SendCoffeeCheckup | **High** |
| `TempCoffeeCheckup` | Staging CRUD | `TempCoffeeCheckupRepository` (new) | **All** | CoffeeCheckupManager | **High** |
| `ContactToRemindWithItems` / `ContactEmailDetails` | Joined lookups | ContactsRepository methods | **Yes** | CoffeeCheckupEmailManager | Medium |
| `SendCheckEmailTextsData` | Get/Update texts | New or existing text repo | **Yes** | SendCoffeeCheckup | Medium |
| `SentRemindersLogTbl` | CRUD + aggregates | Fix `SentRemindersLogRepository` + bespoke | **Most** | CoffeeCheckupManager, SentRemindersSheet | **High** |
| `CustomerTrackedServiceItems` | `GetAllByCustomerTypeID` | `ContactTrackedServiceItemsRepository.GetAllByContactTypeId` | **Done** | Checkup | Low |

**Reference example (README §7):** `CustomerTrackedServiceItems.GetAllByCustomerTypeID` → `ContactTrackedServiceItemsRepository.GetAllByContactTypeId` — **already implemented**. Pattern to follow for other bespoke methods.

---

### RepairsTbl → RepairsRepository (missing)

| Public Method | Classification | Missing | Callers | Risk |
|---------------|----------------|---------|---------|------|
| `GetAll` / `GetAllNotDone` / `GetAllRepairsOfStatus` | Filtered | **All** | Repairs.aspx | High |
| `GetRepairsByStatusAndDateRange` | Joined/filtered | **Yes** | Repairs list | High |
| `GetRepairById` | Lookup | **Yes** | RepairDetail, RepairStatusChange | High |
| Insert/Update/Delete | CRUD | **All** | Repair pages | High |
| `GetLastIDInserted` / `GetListOfRelatedTempOrders` | Lookup | **Yes** | — | Low |
| `UpdateOrderNotesWithRepairStatus` static | Business | **Yes** | RepairManager | Move to manager + OrdersRepository | Medium |

---

### PersonsTbl → PersonsRepository (thin)

| Public Method | Classification | Missing | Callers | Risk |
|---------------|----------------|---------|---------|------|
| CRUD | CRUD | Use base | Admin | Low |
| `IsNormalDeliveryDoW` | Lookup | **Yes** | OrderManager, OrderDetail | Medium |
| `PersonsIDoFSecurityUsers` | Lookup | **Yes** | Security | Medium |
| `PersonsIDFromAbbreviation` / `PersonsNameFromID` | Lookup | **Yes** | Various | Low |
| `SecurityUsersNotInPeopleTbl` | Joined | **Yes** | Admin | Low |

---

### SysDataTbl → SysDataRepository

| Public Method | Classification | Existing | Missing | Recommended | Risk |
|---------------|----------------|----------|---------|-------------|------|
| `GetAll` | CRUD | `GetSystemData()` | — | Wire | Low |
| `GetMinReminderDate` | Lookup | Model property | — | Manager | Low |
| `GetInternalCustomerIdsList` | Lookup | — | **Yes** | CoffeeCheckupManager | `GetInternalContactIds()` | Medium |
| `Update` | CRUD | `UpdateSystemData` | — | Wire | Low |
| `GetGroupItemTypeID` | Lookup | Model | Use `GroupItemServiceTypeID` | ItemTypeTbl callers | Low |

---

### ClientUsageFromTempOrder / LogTbl

| Class | Repository | Gap | Risk |
|-------|------------|-----|------|
| `ClientUsageFromTempOrder.GetAll` | Temp orders query | **Yes** — joined temp lines | Medium |
| `LogTbl` CRUD + insert | `LogRepository` stub | Fix stub + Insert | Low |

---

## 9. Thin Lookup Tables — Consolidated

Add **only** missing desc-by-id (or id-by-desc) helpers when callers still exist. Do not bulk-rewrite thin repos.

| Old Class | Repository | Missing bespoke | Callers | Risk |
|-----------|------------|-----------------|---------|------|
| `CustomerTypeTbl` | `ContactTypesRepository` | — | Lookups | Done |
| `EquipTypeTbl` | `EquipTypesRepository` | `GetEquipTypeNameById` | RepairStatusChange | Low |
| `InvoiceTypeTbl` | `InvoiceTypesRepository` | — (verify Insert override) | SupportTables | Low |
| `PackagingTbl` | `ItemPackagingsRepository` | `GetItemPackagingDescById` | OrderManager, pages | Medium |
| `PaymentTermsTbl` | `PaymentTermsRepository` | `GetIdByDescription` | MergeCustomersFromQB | Low |
| `PriceLevelsTbl` | `PriceLevelsRepository` | `GetIdByDescription` | MergeCustomersFromQB | Low |
| `RepairFaultsTbl` | **None** | Entire thin repo | Repair UI | Low |
| `RepairStatusesTbl` | `RepairStatusesRepository` | `GetDescById`, `GetStatusNote` | RepairManager | Medium |
| `SectionTypesTbl` | **None** | Repo + `InsertDefaultSections` | — | Low |
| `ServiceTypeTbl` | `ItemServiceTypesRepository` | `GetDescById`; static coffee helpers → manager/constants | Various | Low |
| `TransactionTypesTbl` | **None** | Repo + seed | — | Low |
| `ItemUnitsTbl` | `ItemUnitsRepository` | — | Lookups | Done |
| `MachineConditionsTbl` | `EquipConditionsRepository` | — | Lookups | Done |
| `ReoccuranceTypeTbl` | `RecurranceTypeRepository` | Static helpers → manager | Recurring | Done |
| `TrackedServiceItemTbl` | Stub | Fix stub | — | Low |
| `DeliveryItemsTbl` | Stub (projection) | Fix or use DeliverySheetRepository | DeliverySheet | Low |

---

## 10. Active Code Still Calling Legacy Controls

| Consumer | Legacy Controls | Migration target |
|----------|-----------------|------------------|
| `CoffeeCheckupManager` | TempCoffeeCheckup, OrderCheckTbl, CustomersTbl, SentRemindersLogTbl, HolidayClosureProvider, CustomersAwayTbl, SysDataTbl, ItemTypeTbl | Repos + manager (no SQL in manager) |
| `CoffeeCheckupEmailManager` | PackagingTbl, ItemTypeTbl | ItemPackagingsRepository, ItemsRepository |
| `OrderManager` | OrderTbl, TempOrdersDAL, CustomersTbl, ItemTypeTbl, PackagingTbl, TempOrdersLinesTbl, PersonsTbl | OrdersRepository, temp repos, ContactsRepository, ItemsRepository |
| `OrderDoneManager` | TempOrdersHeaderTbl, ClientUsageTbl, CustomersTbl, TempOrdersDAL | Temp + ContactsUsage repos |
| `CustomerManager` | CustomersTbl | ContactsRepository (extend) — **manager must not new Controls** |
| `RepairManager` | RepairsTbl, OrderTbl, CustomersTbl, NextPrepDateByAreaTbl, RepairStatusesTbl | RepairsRepository, OrdersRepository, ContactsRepository, NextPrepDateByAreaRepository |
| `GeneralTrackerDbTools` | ClientUsageLinesTbl, ClientUsageTbl, CustomersTbl, ContactType | Usage + Contacts repos |
| `DateMatrixBuilder` | AreaPrepDaysTbl | AreaPrepDaysRepository |
| `OrderDetail.aspx.cs` / `NewOrderDetail.aspx.cs` | OrderTbl, UsedItemGroupTbl, HolidayClosureProvider, ItemTypeTbl, PackagingTbl | Orders + UsedItemGroup + Holiday manager + Items/ItemPackagings |
| `CustomerDetails.aspx.cs` | CustomersTbl | ContactsRepository |
| `DeliverySheet.aspx.cs` | CustomersAccInfoTbl | ContactsAccInfoRepository |
| `ItemGroups.aspx.cs` / `GroupItemDetail.aspx.cs` | ItemGroupTbl, ItemTypeTbl, SysDataTbl | ItemGroupsRepository, ItemsRepository, SysDataRepository |
| `RepairDetail.aspx.cs` / `Repairs.aspx.cs` | RepairsTbl, CustomersTbl | RepairsRepository, ContactsRepository |
| `SendCoffeeCheckup.aspx.cs` + `.aspx` | ObjectDataSource + checkup controls | Explicit binding via CoffeeCheckupManager |
| `MergeCustomersFromQB.aspx.cs` | Multiple Controls | Multiple repos |
| `HolidayClosureDetail.aspx.cs` | HolidayClosureProvider | HolidayClosureManager + HolidayClosuresRepository |

---

## 11. Repository Infrastructure Issues

### 11.1 Broken stubs (fix first — README §16)

Files in `Repositories/` with wrong namespace, syntax errors, orphaned duplicate methods, invalid `Map` override, `` `$"..."` `` literals, TODO placeholders:

- `NextPrepDateByAreaRepository.cs`
- `UsedItemGroupRepository.cs`
- `SentRemindersLogRepository.cs`
- `OrderCheckRepository.cs` — **repurpose as query helper or delete table mapping**
- `TempOrdersHeaderRepository.cs`
- `TrackedServiceItemRepository.cs`
- `DeliveryItemsRepository.cs`
- `LogRepository.cs`
- `OrderItemRepository.cs`

### 11.2 Empty stubs

- `ContactsAccInfoRepository.cs` — **empty file**
- `ServiceTypesRepository.cs` — **empty file** (use `ItemServiceTypesRepository` instead; delete or implement)

### 11.3 Missing repositories

- `RepairsRepository`
- `RepairFaultsRepository`
- `SectionTypesRepository`
- `TransactionTypesRepository`
- `TempOrdersLinesRepository`
- `SendCheckupEmailTextRepository` (or map to existing model)
- `TempCoffeeCheckupRepository`
- `CoffeeCheckupQueriesRepository` (proposed — multi-table checkup SQL)

### 11.4 Confirmed defects in otherwise-real repos

| Repo | Issue |
|------|-------|
| `OrdersRepository.UpdateOrderHeader` | Legacy column names; concatenated order IDs |
| `ContactUsageLinesRepository` | TableName may not match migrated schema |
| `RepositoryBase` | Unsafe sort concatenation in `GetAll(string)` |
| `ItemsRepository` | Unsafe sort concatenation |
| `ContactSummariesRepository` | Raw `whereFilter` string |

---

## 12. Recommended Migration Sequence

Per README §15–§17 — small batches, build after each:

| Step | Work | Rationale |
|------|------|-----------|
| 1 | Fix broken stub repos: namespace → `TrackerSQL.Repositories`, compile, remove invalid `Map` | Unblocks all downstream |
| 2 | Fix `OrdersRepository.UpdateOrderHeader` (§14 columns, parameterized IDs) | Active defect + security |
| 3 | `ContactsRepository` bespoke: disable, reminders, search, Insert/Update | Unblocks CustomerManager, checkup |
| 4 | `OrdersRepository` line CRUD + status methods | Unblocks OrderManager, pages |
| 5 | `NextPrepDateByAreaRepository` bespoke scheduling | Unblocks RepairManager, checkup, DateMatrixBuilder |
| 6 | Implement `ContactsAccInfoRepository` | Unblocks DeliverySheet |
| 7 | Create `RepairsRepository` | Unblocks RepairManager |
| 8 | `UsedItemGroupRepository` rotation methods | Unblocks order entry |
| 9 | Coffee checkup query + temp staging repos | Unblocks CoffeeCheckupManager |
| 10 | Temp orders header/lines repos | Unblocks OrderDoneManager |
| 11 | Wire managers/pages off Controls | README §10–§11 |
| 12 | Add thin lookup helpers + sort whitelists **only where callers remain** | README §9 |

Suggested commit messages (README §17):

```text
migration: fix stub repository namespaces and compile errors
migration: add OrdersRepository line CRUD and fix UpdateOrderHeader columns
migration: add ContactsRepository reminder and disable methods
migration: replace CustomersTbl calls in CustomerManager
```

---

## 13. Appendix: Classification Summary by Control File

| File | Has SQL | Primary gap | New home | Severity |
|------|---------|-------------|----------|----------|
| AreaTblDAL | Yes | Minor lookups | AreasRepository | Low |
| AreaPrepDaysTbl | Yes | `GetAllByAreaId` | AreaPrepDaysRepository | Low |
| ActiveDeliveryData | Yes | — | DeliveryRepository | **Done** |
| ClientUsageFromTempOrder | Yes | Join query | Temp orders repo | Medium |
| ClientUsageLinesTbl | Yes | Most bespoke | ContactUsageLinesRepository | High |
| ClientUsageTbl | Yes | Updates/exists | ContactsUsageRepository | Medium |
| CompanyNames | Yes | ContactName lookups | ContactsRepository | Low |
| ContactEmailDetails / ContactToRemindWithItems | Yes | Joined | ContactsRepository / manager | Medium |
| ContactType | Yes | Update methods | ContactsRepository | Medium |
| ContactsThatMayNeedNextWeek | Yes | Entire query | CoffeeCheckupQueries (new) | **High** |
| CustomerDAL | Yes | — | ContactsRepository | **Done** |
| CustomersAccInfoTbl | Yes | Entire repo empty | ContactsAccInfoRepository | **High** |
| CustomersAwayTbl | Yes | Away-on-date queries | ContactsAwayPeriodRepository | Medium |
| CustomersTbl | Yes | Reminder/disable/search | ContactsRepository | **High** |
| CustomerSummaryDAL | Yes | whereFilter debt | ContactSummariesRepository | Low |
| CustomersWithDatesAndUsageTbl | Yes | Composite query | Manager + repos | Medium |
| CustomerTrackedServiceItems | Yes | — | ContactTrackedServiceItemsRepository | **Done** |
| CustomerTypeTbl | Yes | — | ContactTypesRepository | **Done** |
| EquipTypeTbl | Yes | Name lookup | EquipTypesRepository | Low |
| HolidayClosureProvider | Yes | Business logic | HolidayClosureManager | Medium |
| InvoiceTypeTbl | Yes | — | InvoiceTypesRepository | Low |
| ItemGroupTbl | Yes | Group navigation | ItemGroupsRepository | Medium |
| ItemTypeTbl | Yes | Many lookups | ItemsRepository | **High** |
| ItemUsageTbl | Yes | Last-by-service | ContactsItemUsageRepository | Medium |
| LogTbl | Yes | Stub | LogRepository | Low |
| NextPrepDateByAreaTbl | Yes | All bespoke | NextPrepDateByAreaRepository | **High** |
| OrderCheck / OrderCheckTbl | Yes | All queries | CoffeeCheckupQueries / OrdersRepository | **High** |
| OrderData / OrderDetailDAL / OrderItemTbl / OrderTbl | Yes | Lines + status | OrdersRepository | **High** |
| OrderDataControl | Yes | Fix existing method | OrdersRepository | **High** |
| PackagingTbl | Yes | Desc lookup | ItemPackagingsRepository | Medium |
| PersonsTbl | Yes | Delivery DoW, security | PersonsRepository | Medium |
| ReoccuringOrderDAL | Yes | Partial | RecurringOrdersRepository | Medium |
| RepairsTbl | Yes | Entire repo missing | **RepairsRepository (new)** | **High** |
| SendCheckEmailTextsData / SentRemindersLogTbl / TempCoffeeCheckup | Yes | Staging + aggregates | New/fix repos | **High** |
| TempOrders* / TempOrdersDAL | Yes | Entire workflow | Temp repos | **High** |
| UsedItemGroupTbl | Yes | Rotation | UsedItemGroupRepository | **High** |
| Thin lookup *Tbl (§9) | Yes | Desc helpers only | Respective thin repos | Low |

---

*End of refreshed gap analysis. No code was modified. Primary reference: [`README.md`](../../README.md).*
