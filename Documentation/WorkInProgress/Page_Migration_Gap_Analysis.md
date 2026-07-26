# WebForms Page Migration Gap Analysis

**Date:** 2026-06-15 (nav phases); **reachability scan 2026-07-14**  
**Primary source of truth:** [`README.md`](../../README.md) (§1.1, §11, §15, §18)  
**Handoff:** [`Page_Migration_Handoff.md`](Page_Migration_Handoff.md)  
**Control inventory:** [`Controls_Migration_Gap_Analysis.md`](Controls_Migration_Gap_Analysis.md)  
**Scope:** All `.aspx` / `.aspx.cs` under `Pages/`, `Tools/`, `Administration/`, `Account/`, repo root, and `test/`  
**Status:** Active inventory — Phase 2 complete; Phase 3/4 = **when we have time / may not be needed**.

> **Phasing rule:** Migrate pages the app actually navigates to. **Phase 1** = `Site.Master` menu (+ workflow children reached from those pages). **Phase 2** = pages opened from `Tools/SystemTools.aspx` not already in Phase 1. **Phase 3 / 4 (retired track)** = direct-URL-only, test, or superseded pages — do **not** schedule migration unless a live link or bookmark need is confirmed.

---

## 1. Classification legend

Each page is assigned a **primary** migration action. Secondary actions appear in the Notes column.

| Class | Meaning | Page work |
|-------|---------|-----------|
| **Thin bind swap** | Mostly replace legacy `ObjectDataSource` / `new XxxTbl()` with repo calls and code-behind binding; little embedded business logic | Swap controls → repos; move ODS to `BindXxx()` methods |
| **Manager extraction** | Code-behind orchestrates validation, workflow, multi-table updates, email, or prefs/dates — must move to `TrackerSQL.Managers` first | Extract logic → manager; thin page to manager calls |
| **New repo method** | Page (or manager after extraction) needs bespoke SQL/query not yet on a repository | Add method to existing repo only; no SQL in page |
| **Already migrated** | Repos/managers dominate; no `TrackerSQL.Controls` / `TrackerDb` in active path | Minor cleanup only (ODS → code-behind bind optional) |
| **Out of scope** | Auth/membership, static shells, diagnostics, schema tools, one-off migration utilities | Defer or exclude from production migration |

**Rules applied (from README §11):**

- Swapping `new CustomersTbl()` for `new ContactsRepository()` in the page is **not sufficient** when handlers contain business logic.
- All `ObjectDataSource` with `TypeName="TrackerSQL.Controls...."` must move to code-behind binding via repos or manager-prepared data.
- No `TrackerDb`, inline SQL, or `TrackerSQL.Controls` in pages after migration.
- Managers already migrated — do **not** reintroduce Controls into managers.

---

## 2. Executive summary

| Metric | Count |
|--------|------:|
| `.aspx.cs` code-behind files scanned | 57 |
| `.aspx` pages total (in scope folders) | 72 |
| **Phase 1** pages (menu + workflow children) | **32** |
| **Phase 2** pages (SystemTools-only additions) | **4** (+ `SystemTools.aspx` hub) |
| **Retired track** (Phase 3+4 — when we have time / may not need) | **~25** (excluded from `.csproj` where listed) |
| Pages referencing `TrackerSQL.Controls` in code-behind | 22 |
| Pages using `TrackerDb` / `ExecuteSQLGetDataReader` in code-behind | 5 |
| `.aspx` markup still using legacy Controls `ObjectDataSource` | 14 |

### Phase overview

| Phase | Source | Pages needing migration work | Already done / minimal |
|-------|--------|------------------------------|------------------------|
| **1** | `Site.Master` menu + workflow children | **19** | **13** |
| **2** | `SystemTools.aspx` dashboard (unique links) | **4** (+ hub) | **1** (`MessagesEditor`) |
| **3** | `test/` direct URL | **1–2** | **2–3** (minimal) |
| **4** | Not linked from menu or SystemTools | TBD if needed | — |

Auth pages (`Account/*`, `Administration/*` in User menu) use Membership only — **no repo migration** in any phase.

---

## 2.1 Phase 1 — `Site.Master` menu (+ workflow children)

**Navigation source:** `Site.Master` lines 156–217 (desktop menu + mobile footer links 254–262).

### 2.1.1 Menu direct links

| Page | Menu path | Migration class | Status |
|------|-----------|-----------------|--------|
| `Default.aspx` | Home | Already migrated | Done |
| `Pages/Contacts.aspx` | Contacts | Already migrated | Done |
| `Pages/ContactDetails.aspx` | Contacts → New Contact | Already migrated | ODS → code-behind bind optional |
| `Pages/ContactsAway.aspx` | Contacts → Contacts Away | Already migrated | Done |
| `Pages/SendCoffeeCheckup.aspx` | Contacts → Send Checkup | **Mostly done** | Page → manager façades + `page-tone-checkup` (2026-07-16); deepen manager internals optional |
| `Pages/SentRemindersSheet.aspx` | Contacts → View reminders sent | **Thin bind swap** | |
| `Pages/OrderDetail.aspx` | Orders → New Order | **Manager extraction** | P0 — largest page |
| `Pages/OrderEntry.aspx` | Orders → View/Edit Orders | **Thin bind swap** | Redirects to `OrderDetail` |
| `Pages/RecurringOrders.aspx` | Orders → Recurring Orders | Already migrated + **UI retrofit 2026-07-20** (`page-tone-recurring`, `grouping-table`) | Done |
| `Pages/Repairs.aspx` | Repairs → Repairs | **Thin bind swap** | |
| `Pages/RepairDetail.aspx` | Repairs → New Repair | **Manager extraction** | |
| `Pages/ItemsRequired.aspx` | Preperation → Required Sheet | Already migrated | Minor cleanup |
| `Pages/DeliverySheet.aspx` | Preperation → Delivery Sheet | **Manager extraction** | `DeliverySheetManager` mostly done |
| `Pages/PreperationSummary.aspx` | Preperation → Weekly Summary | Already migrated + **UI retrofit 2026-07-21** (`page-tone-summary`) | Done |
| `Pages/ItemGroups.aspx` | System → Item Groups | **Thin bind swap** | |
| `Pages/Lookups.aspx` | System → Lookup Tables | Already migrated | Reference pattern |
| `Tools/LogViewer.aspx` | System → Log Viewer | Minimal / none | Reads log files only |
| `Tools/SystemTools.aspx` | System → System Tools | **Manager extraction** | Also Phase 2 hub |
| `Tools/MoveDeliveryDate.aspx` | System → Move Del. Dt | **New repo method** + thin swap | Also in SystemTools dashboard |
| `Tools/SystemData.aspx` | System → System Data | Already migrated | ODS cleanup optional |
| `Tools/EmailDiagnostics.aspx` | System → Email Diag. | Minimal / none | SMTP test harness |
| `Pages/LeaveApp.aspx` | System → Forms → Leave App | Empty shell | No migration |

**User menu (no repo migration):** `Account/Login`, `RecoverPassword`, `ChangePassword`, `Register`; `Administration/ManageUsers`, `ManageRoles`.

### 2.1.2 Workflow children (not in menu — required by Phase 1 pages)

These pages are **in Phase 1** because live navigation from menu pages reaches them.

| Page | Reached from | Migration class | Notes |
|------|--------------|-----------------|-------|
| `Pages/ContactsAwayDetail.aspx` | `ContactsAway.aspx` | Already migrated | Done |
| `Pages/RecurringOrderDetails.aspx` | `RecurringOrders.aspx` | UI retrofit 2026-07-20; optional manager extraction later | Done (UI) |
| `Pages/RepairStatusChange.aspx` | `Repairs.aspx` | **Manager extraction** | Status-change links |
| `Pages/GroupItemDetail.aspx` | `ItemGroups.aspx` | **New repo method** | Add/edit group items |
| `Pages/OrderDone.aspx` | `OrderDetail.aspx` (order complete) | **Manager extraction** | `OrderDoneManager` partial |
| **`Pages/CustomerDetails.aspx`** | Legacy bookmark/direct URL only | **Retire (preferred)** | Active customer navigation uses `ContactDetails.aspx?ID=` |
| `DisableClient.aspx` | Checkup emails (`CoffeeCheckupEmailManager`) | **New repo method** | Token disable flow — production |
| `Pages/ViewMyOrder.aspx` | Public URL / order emails (`Web.config` rewrite) | **New repo method** | Public read-only order view |

### 2.1.3 Phase 1 suggested batch order

| Batch | Pages | Rationale |
|-------|-------|-----------|
| **1A — Done / verify** | `Default`, `Contacts`, `ContactDetails`, `ContactsAway*`, `Lookups`, `PreperationSummary`, `ItemsRequired`, `RecurringOrders` | Confirm build; optional ODS → code-behind on `ContactDetails` |
| **1B — Coffee checkup** | `SendCoffeeCheckup`, `SentRemindersSheet`, `DisableClient` | Single workflow; managers exist |
| **1C — Orders** | `OrderEntry` → `OrderDetail` → `OrderDone` | Core daily use |
| **1D — Prep / delivery** | `DeliverySheet` (+ fix `CustomerDetails` links → `ContactDetails` or migrate) | Delivery sheet is P0 operational |
| **1E — Repairs** | `Repairs` → `RepairDetail` → `RepairStatusChange` | `RepairManager` started |
| **1F — Recurring** | `RecurringOrderDetails` | Child of menu page |
| **1G — System (menu)** | `ItemGroups` → `GroupItemDetail`; `MoveDeliveryDate`; `SystemTools`; `ViewMyOrder` | Admin + scheduling |
| **1H — Contact legacy** | `CustomerDetails` **or** redirect all links to `ContactDetails` | Unblocks repair/delivery/checkup grids |

---

## 2.2 Phase 2 — `SystemTools.aspx` (unique pages)

**Navigation source:** `Tools/SystemTools.aspx` dashboard buttons (`PostBackUrl` / `OnClick`).

Pages **already in Phase 1** (also linked from menu or SystemTools submenu): `MoveDeliveryDate`, `SystemData`, `LogViewer`, `EmailDiagnostics`, and **`SystemTools.aspx` itself** (inline batch operations).

### Phase 2 additions (SystemTools dashboard only)

| Page | SystemTools button | Migration class | Notes |
|------|-------------------|-----------------|-------|
| `Tools/XMLtoSQL.aspx` | XML file to SQL | **Done 2026-07-14** | `TrackerSQLDb` + UI standards + CRUD smoke XML |

| `Tools/HolidayClosures.aspx` | Holiday / Closure Dates | **New repo method** | → `HolidayClosuresRepository` |
| `Tools/HolidayClosureDetail.aspx` | *(child of HolidayClosures)* | **New repo method** | Insert/update/delete |
| `Tools/MessagesEditor.aspx` | Messages Editor | Already migrated | Done |

### Phase 2 — `SystemTools.aspx` hub (inline, not separate pages)

| Operation | Legacy usage | Migration class |
|-----------|--------------|-----------------|
| Reset Prep/Delivery Date (`btnResetPrepDates`) | `TrackerTools.SetNextPreperationDateByArea`, `TrackerSQLDb` grid | **Manager extraction** |
| Set Last Order Date (`btnSetLastOrderDate`) | `ReoccuringOrderDAL` | **Manager extraction** |
| Disable Inactive Clients (`btnDisableInactiveClients`) | `ContactsRepository` + manager path | Mostly migrated |
| Set Client Type (`btnSetClientType`) | `CustomerTypeTbl` ODS | **Thin bind swap** |

### Phase 2 suggested batch order

1. `SystemTools.aspx` hub — **done 2026-07-14** (Set Client Type deferred)
2. `HolidayClosures` → `HolidayClosureDetail` — **done 2026-07-14**
3. `MessagesEditor` — **done 2026-07-14** (verify + UI standards)
4. `XMLtoSQL` — **done 2026-07-14**

---

## 2.3–2.4 Retired track — when we have time / may not be needed

**Reachability scan (2026-07-14):** filename references across `.aspx` / `.cs` / `.master` / config (excluding `bin`, docs noise). Seeds: `Site.Master`, `Default.aspx`, `Tools/SystemTools.aspx`.

### Keep (not retired)

| Page | Why kept |
|------|----------|
| Menu + SystemTools list | Linked from nav / hub |
| Drill-downs: `ContactDetails`, `ContactsAwayDetail`, `GroupItemDetail`, `RecurringOrderDetails`, `HolidayClosureDetail`, `RepairStatusChange`, `OrderDone` | Opened from live pages |
| `Pages/ViewMyOrder.aspx`, `DisableClient.aspx` | Email / URL rewrite entry points |
| `HttpErrorPage.aspx` | `Global.asax` |

### `Tools/TestPeople.aspx`

**Not linked** from `Site.Master`, `Default.aspx`, or `SystemTools.aspx` — direct URL only. Already on `PersonsRepository`; **no migration work scheduled**. Excluded from `TrackerSQL.csproj` 2026-07-14; file kept on disk.

### Superseded / orphan (do not migrate)

| Page | Why retired | Project status |
|------|-------------|----------------|
| `Pages/NewOrderDetail.aspx` | Replaced by `OrderDetail.aspx?NewOrder=true` | Never in `.csproj` |
| `Pages/NewOrder.aspx` | Same | Never in `.csproj` |
| `Pages/DeleteOrderLine.aspx` | Only linked from `NewOrderDetail` | Excluded 2026-07-14 |
| `Pages/CustomerDetails.aspx` | Superseded by `ContactDetails` | Never in `.csproj` |
| `Pages/RepairDetailOld.aspx` | Superseded by `RepairDetail` | Never in `.csproj` |
| `Pages/ClientList.aspx` | Not in menu; broken inherit | Excluded |
| `Pages/ContactsTest.aspx`, `TestPage.aspx` | Dev/test only | Excluded |
| `Pages/OrderSheet.aspx`, `OrdersEdit.aspx`, `ThisWeeksOrder.aspx` | Not in menu | Excluded |
| `Pages/LogTable.aspx`, `SupportTables.aspx` | Superseded by `LogViewer` / `Lookups` | Excluded |
| `Pages/SummaryOFCoffeeRequired.aspx`, `QuaffeeCoffeeTastingSheet.aspx` | Orphan shells | Excluded |
| `Pages/OrderBuiten2Vineyard.aspx`, `OrderVineyard2Buiten.aspx` | Form variants; not linked | Excluded |
| `Pages/Print.aspx`, `LoadSendCoffeeCheckup.aspx` | Orphan | Never in `.csproj` |
| `Tools/MergeCustomersFromQB.aspx` | QB unused; not in hub | Never in `.csproj` |
| `Tools/About.aspx`, `GenMachineKeys.aspx`, `AutoClassMaker.aspx` | Dev utilities | Excluded |
| `Tools/TestPeople.aspx` | Dev CRUD test | Excluded |
| `test/*`, `smpttester/*` | Test harness | Excluded |

**Do not migrate unless a live bookmark/business need is confirmed.** Re-add to `.csproj` only if reactivating.

---

## 3. Pages — `Pages/`

### 3.1 Already migrated

| Page | Lines | Legacy usage | ObjectDataSource (`.aspx`) | Repos / managers in code-behind | Class | Notes |
|------|------:|--------------|----------------------------|----------------------------------|-------|-------|
| `Contacts.aspx` | ~146 | None | None (manual bind) | `ContactSummariesRepository` | **Already migrated** | Filter builds dynamic WHERE in session — consider explicit repo method |
| `ContactDetails.aspx` | ~309 | None in code-behind | **Repos** (9 lookup ODS) | `ContactsRepository`, `ContactsAccInfoRepository`, `ContactsUsageRepository`, `ContactsItemUsageRepository`, + lookup repos | **Already migrated** | ODS point at repos; finish by moving ODS → code-behind bind |
| `ContactsAway.aspx` | ~222 | None | None | `ContactsAwayPeriodRepository` | **Already migrated** | |
| `ContactsAwayDetail.aspx` | ~217 | None | Removed (comment in markup) | `ContactsRepository`, `AwayReasonRepository`, `ContactsAwayPeriodRepository`, `CustomerManager` | **Already migrated** | |
| `Lookups.aspx` | ~1615 | None | Legacy ODS removed from markup | 10+ repos (items, persons, areas, prep days, etc.) | **Already migrated** | Reference pattern for admin CRUD pages |
| `SupportTables.aspx` | ~463 | None | None | Generic bind over 10+ repos | **Already migrated** | Mirror of Lookups |
| `PreperationSummary.aspx` | ~164 | None | None | `PreperationSummaryRepository` | **Already migrated** | Week grouping in RowDataBound is display-only |
| `RecurringOrders.aspx` | ~352 | None | None | `RecurringOrdersRepository` | **Already migrated** | Group/sort UI in page is acceptable |
| `ItemsRequired.aspx` | ~339 | Orphan inline SQL helpers at file bottom | None | `OrdersRepository` | **Already migrated** | Remove dead SQL helpers; consolidate into repo if still needed |
| `LoadSendCoffeeCheckup.aspx` | ~24 | None | None | None | **Already migrated** | Timer/shell only |

### 3.2 Manager extraction (high priority)

| Page | Lines | Legacy usage | ObjectDataSource (`.aspx`) | Repos / managers | Class | Notes |
|------|------:|--------------|----------------------------|------------------|-------|-------|
| **`CustomerDetails.aspx`** | ~701 | **Heavy:** `CustomersTbl`, `CustomersAccInfoTbl`, `ClientUsageTbl`, `ItemUsageTbl`, `PackagingTbl`; `TrackerTools` (dates); `GeneralTrackerDbTools.CalcAndSaveNextRequiredDates` | 11× Controls: `ItemUsageTbl`, `CityTblDAL`, `ItemTypeTbl`, `EquipTypeTbl`, `CustomerTypeTbl`, `PersonsTbl`, `PackagingTbl`, `InvoiceTypeTbl`, `PaymentTermsTbl`, `PriceLevelsTbl`, `ClientUsageTbl` | None in code-behind | **Manager extraction** | Legacy parallel to migrated `ContactDetails`; acc-info CRUD, usage grids, force-next/recalc buttons → `CustomerManager` / `ContactUsageManager` |
| **`OrderDetail.aspx`** | ~2136 | **Mixed:** `OrderTbl`, `ItemTypeTbl`, `PackagingTbl`, `UsedItemGroupTbl`; `TrackerTools` (prefs/dates, session errors) | `OrderItemTbl`, `CustomersTbl`, `OrderDetailDAL`, `ItemTypeTbl` | `OrderManager`, `OrderDetailManager` | **Manager extraction** | Largest page; managers exist but ODS + Tbl calls remain; finish extraction then thin page |
| **`NewOrderDetail.aspx`** | ~742 | **Heavy:** `CustomersTbl`, `ItemUsageTbl`, `OrderTbl`, `PersonsTbl`, `PackagingTbl`, `UsedItemGroupTbl`; `TrackerDb`+`ExecuteSQLGetDataReader`; `TrackerTools` (prefs/dates) | `OrderDetailDAL` | None | **Manager extraction** | Temp-order flow; needs `OrderManager`/`OrderDetailManager` parity with `OrderDetail` |
| **`OrderDone.aspx`** | ~243 | `TempOrdersHeaderTbl`; inline SQL on temp header; `SqlDataSource` for header/lines | SqlDataSource (not ODS): temp order + item/packaging lookups | `OrderDoneManager`, `ContactsUsageRepository` | **Manager extraction** | Completion logic partially migrated; binding layer still legacy Tbl + SqlDataSource |
| **`SendCoffeeCheckup.aspx`** | ~400 | — (via manager) | — | `CoffeeCheckupManager`, `CoffeeCheckupEmailManager` | **Mostly done** | UI + thin page (2026-07-16); staging still used inside manager/repos |
| **`DeliverySheet.aspx`** | ~728 | `CustomersAccInfoTbl` (invoice type delegate to manager) | Commented `ActiveDeliveryData` | `DeliverySheetRepository`, `DeliverySheetManager` | **Manager extraction** | Sheet build migrated; replace `CustomersAccInfoTbl` callback with `ContactsAccInfoRepository` |
| **`RecurringOrderDetails.aspx`** | ~536 | — | — | `RecurringOrdersRepository`, … | **UI done 2026-07-20**; optional manager extraction | See COMPLETED_TASKS; WEBFORMS_UI_STANDARDS §3a |
| **`RepairDetail.aspx`** | ~256 | `RepairsTbl`, `CustomersTbl` | `CompanyNames`, `EquipTypeTbl`, `RepairFaultsTbl`, `RepairStatusesTbl`, `MachineConditionsTbl` | `RepairManager` | **Manager extraction** | Manual load/save + legacy ODS lookups |
| **`RepairStatusChange.aspx`** | ~123 | `RepairsTbl`, `EquipTypeTbl`, `CompanyNames` | `EquipTypeTbl`, `RepairStatusesTbl` | `RepairManager` | **Manager extraction** | Status update flow |

### 3.3 Thin bind swap

| Page | Lines | Legacy usage | ObjectDataSource (`.aspx`) | Repos / managers | Class | Notes |
|------|------:|--------------|----------------------------|------------------|-------|-------|
| `OrderEntry.aspx` | ~180 | — | `OrderEntryDataSource`, `OrderLookupDataSource` | `OrdersRepository`, `ItemsRepository` | **UI done 2026-07-22** | View/Edit Orders list; page-tone-orders; home card → OrderEntry |
| `Repairs.aspx` | ~368 | `EquipTypeTbl`, `RepairFaultsTbl`, `RepairStatusesTbl` (display helpers) | `RepairsTbl`, `RepairStatusesTbl` | `RepairManager` (filter/update) | **Thin bind swap** | ODS grid + lookup desc helpers → repos |
| `SentRemindersSheet.aspx` | ~358 | `SentRemindersLogTbl` | `SentRemindersLogTbl` (×2) | None | **Thin bind swap** | Failed-email UI logic → `CoffeeCheckupManager` or thin page helper |
| `LogTable.aspx` | ~186 | Display helpers: `LogTbl`, `CompanyNames`, `EquipTypeTbl`, `SectionTypesTbl`, `TransactionTypesTbl`, `PersonsTbl`, `CustomersTbl` | `LogTbl` | None | **Thin bind swap** | Grid ODS + template lookup methods → repos |
| `ItemGroups.aspx` | ~137 | `ItemGroupTbl`, `ItemTypeTbl` | `ItemTypeTbl`, `ItemGroupTbl` (×3) | None | **Thin bind swap** | Add/remove group membership via Tbl → `ItemGroupsRepository` / `ItemsRepository` |
| `NewOrder.aspx` | ~176 | `TrackerTools` (prefs/dates only) | Typed DataSets: `OrdersTableAdapter`, `CustomersLkupTableAdapter`, `ItemTypeLkupTableAdapter`, `PersonsLkupTableAdapter` | None | **Thin bind swap** + partial manager | Replace TableAdapter ODS; prefs → `OrderManager` |
| `ThisWeeksOrder.aspx` | ~32 | None in code | `OpenOrdersDataSetTableAdapters.OrdersToDoQryTableAdapter` | None | **Thin bind swap** | Non-standard `ThisWeeksOrder.cs` filename; replace TableAdapter |

### 3.4 New repo method

| Page | Lines | Legacy usage | ObjectDataSource (`.aspx`) | Repos / managers | Class | Notes |
|------|------:|--------------|----------------------------|------------------|-------|-------|
| **`ViewMyOrder.aspx`** | ~376 | `TrackerDb` (3× raw SELECT); `ItemTypeTbl`, `PackagingTbl` | None | `OrderChangeRequestLoggingService` only | **New repo method** + manager | Public token order view; SQL → `OrdersRepository`/`OrderLinesRepository`; consider `PublicOrderViewManager` for data (currently UI-only) |
| **`DeleteOrderLine.aspx`** | ~58 | `TrackerDb.ExecuteNonQuerySQL` (raw DELETE) | SqlDataSource `sdsOrderLine` | None | **New repo method** | Add `OrderLinesRepository.DeleteById`; remove inline SQL |
| **`GroupItemDetail.aspx`** | ~112 | `ItemTypeTbl`, `SysDataTbl` | None | None | **New repo method** + manager | Create/edit group item types → `ItemsRepository` bespoke + `ItemManager` |

### 3.5 Out of scope / minimal

| Page | Lines | Legacy | Class | Notes |
|------|------:|--------|-------|-------|
| `ContactsTest.aspx` | ~126 | `TrackerSQLDb` diagnostic | **Out of scope (test)** | |
| `Print.aspx` | ~20 | `PrintHelper` (Classes) | **Out of scope** | Session print utility |
| `LeaveApp.aspx` | ~19 | None | **Out of scope** | Empty shell; also inherited by `OrderBuiten2Vineyard.aspx` |
| `OrderBuiten2Vineyard.aspx` | — | Inherits `LeaveApp` | **Out of scope** | No dedicated code-behind |
| `OrderVineyard2Buiten.aspx` | — | *(check if linked)* | **Out of scope** | |
| `QuaffeeCoffeeTastingSheet.aspx` | ~19 | None | **Out of scope** | Empty shell |
| `TestPage.aspx` | ~25 | SqlDataSource in markup | **Out of scope (test)** | |

### 3.6 Missing code-behind (Phase 4 — defer unless needed)

These `.aspx` files reference code-behind classes that **do not exist** in the repo. **Not linked from menu or SystemTools** — treat as Phase 4.

| Page | ObjectDataSource / data source | Class | Notes |
|------|-------------------------------|-------|-------|
| **`OrderSheet.aspx`** | `OrderSheetTableAdapter`, `PersonsLkupTableAdapter` | **Thin bind swap** + new code-behind | `OrderSheet.aspx.cs` **missing** |
| **`OrdersEdit.aspx`** | `OrdersTableAdapter` (×2), `CustomersLkupTableAdapter`, `ItemTypeLkupTableAdapter`, `PersonsLkupTableAdapter` | **Manager extraction** + new code-behind | `OrdersEdit.aspx.cs` **missing**; full order edit workflow |
| **`SummaryOFCoffeeRequired.aspx`** | *(designer only)* | **TBD** | `SummaryOFCoffeeRequired.aspx.cs` **missing** |
| **`RepairDetailOld.aspx`** | Full legacy repair ODS set | **Defer / retire** | Superseded by `RepairDetail`; `RepairDetailOld.aspx.cs` **missing** |
| **`ClientList.aspx`** | `CustomersDataSetTableAdapters.CustomersTableAdapter` | **Broken** | `CodeBehind="ItemsRequired.aspx.cs"` / `Inherits="ClientListForm"` — **class does not exist**; needs dedicated code-behind + repo migration |

---

## 4. Tools — `Tools/`

> **Phase note:** Pages marked **Phase 1** are also in `Site.Master` menu. **Phase 2** pages are reached only from `SystemTools.aspx` dashboard (§2.2).

| Page | Phase | Lines | Legacy usage | ObjectDataSource (`.aspx`) | Repos / managers | Class | Notes |
|------|-------|------:|--------------|----------------------------|------------------|-------|-------|
| `SystemData.aspx` | **1** | ~150 | None | `TrackerSQL.Tools.SystemData`, `ItemServiceTypesRepository` | `SysDataRepository`, `ItemServiceTypesRepository` | **Already migrated** | Move remaining ODS → code-behind bind |
| `LogViewer.aspx` | **1** | ~250 | None | None | None | **Minimal** | Reads `App_Data/*.log` files |
| `EmailDiagnostics.aspx` | **1** | ~395 | None | None | `EmailSettings`, MailKit | **Minimal** | SMTP test harness |
| `MoveDeliveryDate.aspx` | **1** | ~76 | None in code | `NextPreperationDateByAreaTbl` | `TrackerSQLDb` inline UPDATE in `btnMove_Click` | **New repo method** + thin swap | ODS → `NextPrepDateByAreaRepository`; UPDATE → repo |
| **`SystemTools.aspx`** | **1 + 2** | ~354 | `ReoccuringOrderDAL`; `TrackerTools.SetNextPreperationDateByArea` | `CustomerTypeTbl` | `ContactsRepository`; `TrackerSQLDb` for prep grid | **Manager extraction** | Menu + dashboard hub |
| `MessagesEditor.aspx` | **2** | ~457 | None | None | `MessagesResourceManager` | **Already migrated** | |
| **`HolidayClosures.aspx`** | **2** | ~298 | — | `HolidayClosuresRepository` | `HolidayClosureManager` | **Done 2026-07-14** | List/filter/inline add/copy year; unique date + overlap |
| **`HolidayClosureDetail.aspx`** | **2** | ~120 | — | `HolidayClosuresRepository` | `HolidayClosureManager` | **Done 2026-07-14** | UpdatePanel UI; Save / Save & Return; status bottom |
| `XMLtoSQL.aspx` | **2** | ~450 | — | `TrackerSQLDb` | — | **Done 2026-07-14** | SQL Server smoke XML; Access-era command files may still fail |
| `MergeCustomersFromQB.aspx` | **4** | ~934 | **Heavy:** 6 Tbl types + `AreaTblDAL`; `TrackerTools` | None | None | **Manager extraction** | Not in menu or SystemTools |
| `TestPeople.aspx` | **3** | ~94 | None | None | `PersonsRepository` | **Phase 3 test** | CRUD test grid |

### Tools — missing code-behind

| Page | Notes |
|------|-------|
| `About.aspx` | Static; `About.aspx.cs` missing |
| `AutoClassMaker.aspx` | `AutoClassMaker.aspx.cs` missing (test copy under `test/`) |
| `GenMachineKeys.aspx` | Inline script — no code-behind |

---

## 5. Root — repo root

| Page | Phase | Lines | Legacy usage | Repos / managers | Class | Notes |
|------|-------|------:|--------------|------------------|-------|-------|
| `Default.aspx` | **1** | ~49 | None | `TotalCountTrackerRepository` | **Already migrated** | Home cup count |
| **`DisableClient.aspx`** | **1** | ~127 | Inline UPDATE via `TrackerSQLDb` | `ContactsRepository`, `DisableClientManager` | **New repo method** | Checkup email workflow; move UPDATE to repo |
| `HttpErrorPage.aspx` | **4** | ~65 | None | None | **Defer** | Error display |

---

## 6. Account — `Account/`

All pages use ASP.NET Membership / auth only. **Out of scope** for repository migration.

| Page | Lines | Notes |
|------|------:|-------|
| `Login.aspx` | ~75 | Membership + `UserPreferencesHelper` (TZ) |
| `Register.aspx` | ~34 | `CreateUserWizard` |
| `RecoverPassword.aspx` | ~24 | Empty shell |
| `ChangePassword.aspx` | ~25 | Logging only |
| `ChangePasswordSuccess.aspx` | ~19 | Empty shell |

---

## 7. Administration — `Administration/`

All pages use Membership / Roles API. **Out of scope**.

| Page | Lines | Notes |
|------|------:|-------|
| `ManageUsers.aspx` | ~35 | |
| `ManageRoles.aspx` | ~90 | |
| `UserInformation.aspx` | ~150 | Roles + TZ prefs |
| `ResetPasswordHelper.aspx` | ~56 | Admin password reset |

---

## 8. test — `test/` (Phase 3)

| Page | Lines | Legacy | Class | Notes |
|------|------:|--------|-------|-------|
| `ShowTableStruct.aspx` | ~266 | **Heavy:** `TrackerDb`, `ExecuteSQLGetDataReader` | **Phase 3 — defer** | Schema introspection (ADOX + SQL) |

---

## 9. Legacy usage matrix (code-behind)

Quick reference — pages with **`using TrackerSQL.Controls`** or direct **`TrackerDb`**:

| Page | Controls types / patterns | TrackerDb / inline SQL |
|------|---------------------------|------------------------|
| `CustomerDetails` | 11 ODS + 8 Tbl types + `GeneralTrackerDbTools` | — |
| `OrderDetail` | `OrderTbl`, `ItemTypeTbl`, `PackagingTbl`, `UsedItemGroupTbl` + 4 ODS | — |
| `NewOrderDetail` | 6 Tbl types + ODS | `ExecuteSQLGetDataReader` |
| `OrderDone` | `TempOrdersHeaderTbl` | Inline SELECT on temp header |
| `DeleteOrderLine` | — | `ExecuteNonQuerySQL` DELETE |
| `ViewMyOrder` | `ItemTypeTbl`, `PackagingTbl` | 3× raw SELECT |
| `DeliverySheet` | `CustomersAccInfoTbl` | — |
| `SendCoffeeCheckup` | `TempCoffeeCheckup`, `SentRemindersLogTbl` + ODS | — |
| `SentRemindersSheet` | `SentRemindersLogTbl` + ODS | — |
| `LogTable` | 7 Tbl types + ODS | — |
| `ItemGroups` | `ItemGroupTbl`, `ItemTypeTbl` + ODS | — |
| `GroupItemDetail` | `ItemTypeTbl`, `SysDataTbl` | — |
| `Repairs` / `RepairDetail` / `RepairStatusChange` | `RepairsTbl` + lookup Tbls + ODS | — |
| `RecurringOrderDetails` | `ReoccuringOrderTbl`, `ReoccuringOrderDAL` | — |
| `OrderEntry` | ODS only (+ static Tbl helper) | — |
| `MergeCustomersFromQB` | 6 Tbl types + `AreaTblDAL` | — |
| `HolidayClosures` / `HolidayClosureDetail` | `HolidayClosureProvider` | — |
| `SystemTools` | `ReoccuringOrderDAL` + ODS | `TrackerSQLDb` prep grid |
| `MoveDeliveryDate` | ODS (`NextPreperationDateByAreaTbl`) | Inline UPDATE |
| `DisableClient` | — | Inline UPDATE via `TrackerSQLDb` |
| `XMLtoSQL` | — | Done — `TrackerSQLDb` (no TrackerDb in page) |
| `ShowTableStruct` | — | Full `TrackerDb` usage |

---

## 10. ObjectDataSource inventory (`.aspx` markup)

### 10.1 Legacy `TrackerSQL.Controls` (must remove)

| Page | TypeName values |
|------|-----------------|
| `CustomerDetails` | `ItemUsageTbl`, `CityTblDAL`, `ItemTypeTbl`, `EquipTypeTbl`, `CustomerTypeTbl`, `PersonsTbl`, `PackagingTbl`, `InvoiceTypeTbl`, `PaymentTermsTbl`, `PriceLevelsTbl`, `ClientUsageTbl` |
| `OrderDetail` | `OrderItemTbl`, `CustomersTbl`, `OrderDetailDAL`, `ItemTypeTbl` |
| `NewOrderDetail` | `OrderDetailDAL` |
| `OrderEntry` | `OrderData`, `CompanyNames`, `PersonsTbl`, `ItemTypeTbl` |
| `Repairs` | `RepairsTbl`, `RepairStatusesTbl` |
| `RepairDetail` | `CompanyNames`, `EquipTypeTbl`, `RepairFaultsTbl`, `RepairStatusesTbl`, `MachineConditionsTbl` |
| `RepairStatusChange` | `EquipTypeTbl`, `RepairStatusesTbl` |
| `RepairDetailOld` | Full repair ODS set *(retire)* |
| `ItemGroups` | `ItemTypeTbl`, `ItemGroupTbl` |
| `LogTable` | `LogTbl` |
| `SendCoffeeCheckup` | `TempCoffeeCheckup` (×2) |
| `SentRemindersSheet` | `SentRemindersLogTbl` (×2) |
| `MoveDeliveryDate` | `NextPreperationDateByAreaTbl` |
| `SystemTools` | `CustomerTypeTbl` |

### 10.2 Already on repositories (finish by moving to code-behind)

| Page | TypeName values |
|------|-----------------|
| `ContactDetails` | `AreasRepository`, `ItemsRepository`, `EquipTypesRepository`, `ContactTypesRepository`, `PersonsRepository`, `ItemPackagingsRepository`, `InvoiceTypesRepository`, `PaymentTermsRepository`, `PriceLevelsRepository` |
| `SystemData` | `ItemServiceTypesRepository` (+ page class for SysData) |

### 10.3 Typed DataSet `TableAdapter` (separate migration track)

| Page | TypeName values |
|------|-----------------|
| `NewOrder` | `OrdersTableAdapter`, `CustomersLkupTableAdapter`, `ItemTypeLkupTableAdapter`, `PersonsLkupTableAdapter` |
| `OrderSheet` | `OrderSheetTableAdapter`, `PersonsLkupTableAdapter` |
| `OrdersEdit` | `OrdersTableAdapter` (×2), `CustomersLkupTableAdapter`, `ItemTypeLkupTableAdapter`, `PersonsLkupTableAdapter` |
| `ClientList` | `CustomersTableAdapter` |
| `ThisWeeksOrder` | `OrdersToDoQryTableAdapter` |

---

## 11. Manager readiness vs page gaps

Managers exist for several fat pages but pages still call Controls directly:

| Manager | Page(s) still using Controls / TrackerDb | Gap |
|---------|------------------------------------------|-----|
| `OrderManager` / `OrderDetailManager` | `OrderDetail`, `NewOrderDetail`, `OrderDone`, `DeleteOrderLine` | Finish page wiring; remove ODS |
| `OrderDoneManager` | `OrderDone` | Replace `TempOrdersHeaderTbl` + SqlDataSource binding |
| `CoffeeCheckupManager` / `CoffeeCheckupEmailManager` | `SendCoffeeCheckup`, `SentRemindersSheet` | Remove `TempCoffeeCheckup` ODS; bind from manager |
| `DeliverySheetManager` | `DeliverySheet` | Replace `CustomersAccInfoTbl` delegate with repo |
| `RepairManager` | `Repairs`, `RepairDetail`, `RepairStatusChange` | Swap ODS + lookup Tbl → repos |
| `CustomerManager` | `CustomerDetails`, `ContactsAwayDetail` *(done)* | Full `CustomerDetails` extraction |
| `DisableClientManager` | `DisableClient` | Move inline SQL to repo |
| `PublicOrderViewManager` | `ViewMyOrder` *(UI only today)* | Extend for data load or add `PublicOrderViewRepository` methods |

**No manager yet (likely needed):**

| Area | Page(s) | Suggested manager |
|------|---------|-------------------|
| QB import | `MergeCustomersFromQB` | `QuickBooksImportManager` |
| Recurring order dates | `RecurringOrderDetails`, `SystemTools` | Extend `RecurringOrdersManager` |
| Item groups | `ItemGroups`, `GroupItemDetail` | `ItemGroupManager` |
| Holiday closures | `HolidayClosures`, `HolidayClosureDetail` | `HolidayClosureManager` (thin; repo does SQL) |

---

## 12. Per-page migration checklist (template)

For each page in §3–§8:

1. **Extract** — Move validation, workflow, calculations, email from handlers → manager.
2. **Repo** — Add bespoke query methods only if manager cannot use existing repo API.
3. **Remove ODS** — Replace markup `ObjectDataSource` with `BindXxx()` in code-behind.
4. **Swap** — Replace `new XxxTbl()` with repo or manager call.
5. **Remove bridges** — Delete manager page-bridge overloads once page binds to `TrackerSQL.Models`.
6. **Build** — VS MSBuild (not `dotnet build`); see handoff build note.

---

## 13. Known blockers / debt (from Controls gap analysis)

These affect page migration even when swapping Controls:

| Issue | Affects pages |
|-------|---------------|
| No `RepairsRepository` (stub/broken) | Repair cluster |
| `NextPrepDateByAreaRepository` stub — bespoke methods missing | `MoveDeliveryDate`, `SystemTools`, delivery scheduling |
| `UsedItemGroupTbl` stub repo | `OrderDetail`, `NewOrderDetail` |
| `OrdersRepository.UpdateOrderHeader` legacy column names | Order pages on repo path |
| `ContactSummariesRepository` raw `whereFilter` strings | `Contacts` filter UI |
| Broken auto-gen repo stubs (9 files) | Any page calling those repos |

---

## 14. Phased migration todo

Track progress here and in Cursor todos. Mark `[x]` when page is thin (no Controls, no TrackerDb, no business logic in code-behind).

### Phase 1 — Menu + workflow children

#### 1A — Verify already migrated
- [x] `Default.aspx`
- [x] `Pages/Contacts.aspx`
- [x] `Pages/ContactDetails.aspx` — optional: move repo ODS → code-behind bind
- [x] `Pages/ContactsAway.aspx` + `ContactsAwayDetail.aspx`
- [x] `Pages/Lookups.aspx`
- [x] `Pages/PreperationSummary.aspx`
- [x] `Pages/ItemsRequired.aspx` — removed orphan SQL helpers at file bottom
- [x] `Pages/RecurringOrders.aspx`

#### 1B — Coffee checkup workflow
- [x] `Pages/SendCoffeeCheckup.aspx` — repos + code-behind bind; `SendCheckEmailTextsData` still Controls (email text CRUD)
- [x] `Pages/SentRemindersSheet.aspx` — ODS removed; `SentRemindersLogRepository` + `ContactDetails` links
- [x] `DisableClient.aspx` — `ContactsRepository.ApplyEmailDisableChoice` + `DisableClientManager.DisableFromEmailLink`

#### 1C — Orders workflow
- [x] `Pages/OrderEntry.aspx` — grid + lookup ODS via `OrderEntryDataSource` / `OrderLookupDataSource`; **UI retrofit 2026-07-22** (`page-tone-orders`; home View/Edit → OrderEntry)
- [x] `Pages/OrderDetail.aspx` — `OrderID` model; repos for lookups, group items, packaging; `ContactEmailDetails` via `ContactsRepository`; **UX complete 2026-07-13** (manual Save / Save & Return, line edit save, mobile New Item stack). Log: `COMPLETED_TASKS.md`
- [x] `Pages/OrderDone.aspx` — `OrderDoneDataSource` + `OrderDoneManager`; session-scoped temp order via `TempOrderSession`

#### 1D — Prep / delivery
- [x] `Pages/DeliverySheet.aspx` — `ContactsAccInfoRepository`; contact links use `ContactDetails.aspx`
- [x] Fix hyperlinks: `Repairs`, `OrderDetail`, `ClientList`, `NewOrderDetail`, `OrdersEdit` → `ContactDetails.aspx`

#### 1E — Repairs workflow
- [x] `Pages/Repairs.aspx` — `RepairManager` + `RepairLookupDataSource`
- [x] `Pages/RepairDetail.aspx` — `RepairManager` + `RepairFormData` + `RepairLookupDataSource`
- [x] `Pages/RepairStatusChange.aspx` — `RepairManager` + `RepairLookupDataSource`

#### 1F — Recurring orders child
- [x] `Pages/RecurringOrderDetails.aspx` — `RecurringOrdersRepository.AutoCalculateNextDatesForItems`; dead `ReoccuringOrderDAL` removed

#### 1G — System menu pages
- [x] `Pages/ItemGroups.aspx` + `GroupItemDetail.aspx` — `ItemGroupDataSource` + repos
- [x] `Tools/MoveDeliveryDate.aspx` — `NextPrepDateDataSource` + `NextPrepDateByAreaRepository`
- [x] `Tools/SystemTools.aspx` — `RecurringOrdersRepository` for Set Last Order Date; `ContactTypeDataSource` ODS
- [x] `Pages/ViewMyOrder.aspx` — `OrdersRepository.GetPublicOrderLines*` + repos

#### 1H — Contact legacy decision
- [ ] **Option A:** Migrate `CustomerDetails.aspx` to managers (heavy) — deferred
- [x] **Option B (preferred):** Phase 1 workflow links point to `ContactDetails.aspx?ID=`; `CustomerDetails.aspx` retained for legacy bookmarks only

**Phase 1 exit criteria:** All menu pages + workflow children compile and run without `TrackerSQL.Controls` or `TrackerDb` in code-behind.

---

### Phase 2 — SystemTools-only pages

- [x] `Tools/SystemTools.aspx` — hub cleaned 2026-07-14 (no page SQL/ODS; Set Client Type deferred/removed from UI)
- [x] `Tools/HolidayClosures.aspx` → `HolidayClosureDetail.aspx` — done 2026-07-14
- [x] `Tools/MessagesEditor.aspx` — verified 2026-07-14 (`MessagesResourceManager`; status at bottom; UpdateProgress)
- [x] `Tools/XMLtoSQL.aspx` — cleaned 2026-07-14 (`TrackerSQLDb` + UI standards + `SQLCommands_Test_SQLServer.xml`)

**Phase 2 exit criteria:** Every `SystemTools.aspx` dashboard button opens a migrated page (or documented deferral). ✅ Met 2026-07-14 (Set Client Type deferred/removed from hub).

---

### Retired track (when we have time / may not be needed) — 2026-07-14

- [x] Reachability scan: menu / home / SystemTools + drill-downs vs direct-URL orphans
- [x] Mark retired set; exclude from `TrackerSQL.csproj` (files remain on disk)
- [x] `Tools/TestPeople.aspx` — **not** next work (unlinked); excluded from project
- [ ] Optional later: delete retired files from disk after confirming no bookmarks
- [ ] Optional later: reactivate any page only if a real link/need appears

---

### Cross-phase blockers (resolve when touching affected pages)

- [ ] `RepairsRepository` — repair pages use `RepairManager` + dedicated repos (Phase 1E complete)
- [x] `NextPrepDateByAreaRepository` — `MoveDeliveryDate` + grid bind (Phase 1G)
- [x] `UsedItemGroupTbl` repo gap — `UsedItemGroupRepository.UpdateIfGroupItem` (Phase 1C)
- [x] `OrdersRepository.UpdateOrderHeader` — `UpdateOrderHeaderByOrderId` (Phase 1C)

---

*Generated for page migration phase. Refresh after each migrated page batch.*
