# Page Migration TODO List - Ordered by Complexity

**Created:** 2025-03-26  
**Last Updated:** 2026-07-15  
**Purpose:** Migrate pages from legacy Controls/OleDb/ObjectDataSource to Repository pattern

---

## WebForms UI retrofit (ongoing)

**Standard:** [`Documentation/WEBFORMS_UI_STANDARDS.md`](../WEBFORMS_UI_STANDARDS.md)  
**Refs:** `HolidayClosureDetail.aspx` (UpdatePanel/buttons) · `SentRemindersSheet.aspx` (**page-tone panel** — canonical from 2026-07-15)

When touching any interactive page, bring it into compliance if needed:

- [x] `Pages/SendCoffeeCheckup.aspx` — 2026-07-14/16 (`page-tone-checkup`; page → `CoffeeCheckupManager` façades only; no direct repos/ODS)
- [x] `Pages/SentRemindersSheet.aspx` — 2026-07-14/15 (UpdatePanel + **page-tone-reminders** shell; single icon; title/filter inside accent bar)
- [x] `Pages/Contacts.aspx` — 2026-07-15 (page-tone-contacts list shell; status bottom; Back → home)
- [x] `Pages/ContactDetails.aspx` — 2026-07-15 (page-tone-contacts; Save / Save & Return / Back; status in panel)
- [x] `Pages/Repairs.aspx` — 2026-07-16 (page-tone-repairs list shell; single UpdatePanel; status bottom; Back → home)
- [x] `Pages/RepairDetail.aspx` — 2026-07-16 (page-tone-repairs; Save / Save & Return / Back; TrackerUnsaved; status bottom)
- [x] `Pages/ContactsAway.aspx` — 2026-07-16 (page-tone-contacts list shell; status bottom; Back → Contacts)
- [x] `Pages/ContactsAwayDetail.aspx` — 2026-07-16 (page-tone-contacts; Save / Save & Return / Delete / Back)
- [x] `Pages/OrderEntry.aspx` — 2026-07-22 (page-tone-orders; View/Edit Orders list; ODS → repos; home card link fixed)
- [x] `Pages/Lookups.aspx` — 2026-07-22 (page-tone-lookups; header/toolbar/status shell)
- [ ] Status / result message at **bottom** of form (not above fields)
- [ ] `ScriptManager` + `UpdatePanel` + `UpdateProgress`
- [ ] Async **Save**; full postback for **Save & Return** / **Back** / redirecting **Delete**
- [ ] **Back** is a Button; prefer Save + Save & Return on detail forms
- [ ] **`page-tone-panel` + `page-tone-*`** — one icon; no duplicate h1 outside panel; tone matches home card

**Next UI retrofit:** pick next interactive page still missing page-tone (or deepen CoffeeCheckupManager naming/layering review). OrderEntry View/Edit list done 2026-07-22.

---

## 🧪 TESTING CHECKLIST - Recent Changes


### Session 10 (2026-05-14) - ItemsRequired Page Refactored to Repository Pattern

#### ItemsRequired.aspx - COMPLETED ✓

`Pages/ItemsRequired.aspx` has been successfully migrated from legacy `SqlDataSource` controls to Repository pattern:

**Completed work:**
- ✓ Removed both `SqlDataSource` controls (`sdsRequiredByRoastingDay` and `sdsCoffeeRequiredCalc`)
- ✓ Created new POCO model: `Classes/Poco/ItemsRequiredSummary.cs` for strong typing
- ✓ Added repository methods to `OrdersRepository`:
  - `GetItemsRequiredByPrepDate(DateTime? fromDate, DateTime? toDate)` - items grouped by prep date
  - `GetItemsRequiredByDeliveryDate(DateTime? fromDate, DateTime? toDate)` - items grouped by delivery date/person
- ✓ Updated terminology: "Roasting Day" → "Preparation Day" throughout
- ✓ Renamed GridView IDs and control names for consistency
- ✓ Added date range filtering UI with From/To date pickers
- ✓ Added "Apply Filter" and "Clear Filter" buttons with validation
- ✓ Default filter shows last 30 days of data
- ✓ Filter status label shows active date range and timestamp
- ✓ Updated Site.Master: fixed CSS typos (`opacity` → `opacity`) and navigation menu links

**Files modified:**
- `Pages/ItemsRequired.aspx` - removed SqlDataSource, added date filter UI
- `Pages/ItemsRequired.aspx.cs` - added date filtering logic and button handlers
- `Classes/Sql/OrdersRepository.cs` - added two new overloaded methods with date parameters
- `Classes/Poco/ItemsRequiredSummary.cs` - new POCO for report data binding
- `Site.Master` - fixed CSS bugs and updated navigation

**Standards compliance:**
- ✓ Repository pattern only (no SqlDataSource, no ObjectDataSource)
- ✓ SQL Server only (no OleDb)
- ✓ Strong typing with POCO models
- ✓ Manual data binding in code-behind
- ✓ Proper error handling

**Page status:** Ready for testing

---

### Session 8 (2026-04-27) - Recurring Pages Almost Finished, Migration Return Focus = Delivery Person

### Session 9 (2026-04-30) - Contacts Tested, ContactDetails Under Test, Predictive Items Suspect

#### Current Resume Point (Read This Before Touching Contacts Again)

`Pages/Contacts.aspx`
- has now been smoke-tested
- contact list is loading
- area naming was corrected further so the page should use `AreaName`, not legacy `Area`

`Pages/ContactDetails.aspx`
- is currently under active testing
- the delivery area selection issue exposed a repository naming problem that was corrected
- repository output should follow SQL/project-standard names only

#### New issue found while testing ContactDetails

There now appears to be a likely **data migration / data-shape issue** around contact predictive items / predicted usage data.

Current working interpretation:
- the web-page binding path is close enough to test
- but some predictive-item data shown from the contact screen may not match expected migrated SQL content
- before doing more UI cleanup on `ContactDetails.aspx`, verify whether the issue is:
  - bad migration output
  - incomplete mapping
  - wrong repository query shape
  - or missing expected target data

#### Important naming / alias rule re-confirmed during Contacts testing

Do **not** reintroduce legacy naming aliases in repositories just to satisfy old pages.

Specifically:
- do **not** return `<TableName>ID AS ID`
- do **not** alias `AreaName` or `Area` as `Area`
- do **not** use legacy Area terminology in new SQL repository paths when the project-standard term is `Area`

Required direction:
- repository result shapes should use the real SQL/project-standard names
- page code and POCOs should be updated to those names instead of carrying legacy aliases forward

#### Next safest action from here

1. Continue testing `Pages/ContactDetails.aspx`
2. Investigate the predictive-items / predicted-usage data problem
3. Verify repository query shape versus live migrated SQL data
4. Prefer fixing data/repository naming at the source instead of adding more legacy aliases

#### Current Resume Point (Read This Before Touching Recurring Again)

The SQL-backed recurring web pages are now **almost finished for layout and behavior testing**.

#### What is now in good shape on the web side

`Pages/RecurringOrders.aspx`
- builds and runs from SQL/repository code only
- grouped recurring-header layout is readable and much closer to the legacy screen
- enabled-only filtering now follows current live SQL header behavior
- `Calc Next Required` has been restored on the page
- mobile column priorities were added for the nested recurring line grid
- async progress / completion feedback for `Calc Next Required` now works
- page is now acceptable for current SQL-backed testing

`Pages/RecurringOrderDetails.aspx`
- remains SQL-backed and repository-based
- recurring line editing is working in the current SQL header/line model
- item dropdown handling was adjusted so a selected disabled item can still appear in the editor
- item dropdown ordering / disabled-item display now follows the standard lookup formatting approach
- page is now acceptable for current SQL-backed testing

#### Current recurring web conclusion

For the current live SQL schema and remigrated data, the recurring web pages should now be treated as **done enough for current testing**:

- `Pages/RecurringOrders.aspx`
- `Pages/RecurringOrderDetails.aspx`
- `Classes/Sql/RecurringOrdersRepository.cs`

Any further recurring work should be driven by:
- migration/schema changes
- business-rule changes
- broader shared cleanup / repository reuse work

The main recurring-page code path is no longer the active blocker.

#### Important current unresolved data/migration issue

The next return point is now **not mainly a web-page problem**.

Direct live SQL verification indicates the recurring **delivery person** did not migrate as expected.

Current observed state:
- source staging table: `AccessSrc.ReoccuringOrderTbl`
- normalized target header table: `RecurringOrdersTbl`
- source staging currently shows `DeliveryByID` blank/null for effectively all recurring rows checked in live SQL
- target recurring headers also have `DeliveryByID` null except for rows that were manually edited later in the web UI

Verified live SQL interpretation:
- the recurring page/repository already supports `DeliveryByID`
- the recurring normalizer appears to carry header fields including `DeliveryByID`
- but the live staged source currently does not contain the recurring delivery-person values needed for migration

#### Working conclusion right now

Before changing the recurring pages further, move back to the migration project and determine **why recurring `DeliveryByID` is not arriving in SQL staging / normalized output**.

Possible causes to verify:
1. Access staging import is not actually bringing over recurring `DeliveryByID`
2. recurring normalization is not grouping/copying `DeliveryByID` into `RecurringOrdersTbl`
3. legacy source data may never have persisted recurring `DeliveryByID` correctly
4. only post-migration manual edits may be setting `RecurringOrdersTbl.DeliveryByID`

#### Verified follow-up conclusion (2026-04-27)

- direct Access query has now confirmed `ReoccuringOrderTbl.DeliveryByID` is null for all `204` source rows in the current `.mdb`
- this means the recurring delivery-person problem is **not** primarily a staging-import or normalize-plan bug
- legacy code also shows why this is plausible:
  - `Controls\ReoccuringOrderDAL.cs` legacy recurring `INSERT`/`UPDATE` statements do not write `DeliveryByID`
  - `Controls\ReoccuringOrderTbl.cs` has no `DeliveryByID` property at all
- safest interpretation: legacy recurring delivery person was likely never actually persisted in the source write path
- current recurring normalization already does the safest fallback now:
  - use source `DeliveryByID` if it exists
  - otherwise fall back/backfill from `ContactsTbl.PreferedAgentID`
- so the next recurring migration step is verification, not redesign of staging metadata, unless a second legacy source for recurring delivery-person is discovered

#### Files to reference first when resuming

Documentation / rules:
- `Documentation/HARD_PROJECT_RULES.md`
- `Documentation/PROJECT_OVERVIEW.md`
- `Documentation/AI/MigrationRunner/STATUS_BOOKMARK.md`
- `Documentation/WorkInProgress/MIGRATION_TODO.md`

MigrationRunner code / metadata:
- `Migrations/MigrationRunner/CustomNormalizeRunner.cs`
- `Migrations/MigrationRunner/AccessStagingImporter.cs`
- `Migrations/MigrationRunner/AccessSchemaExporter.cs`
- `Migrations/MigrationRunner/PlanHumanReviewImporter.cs`
- `Data/Metadata/AccessSchema/ReoccuringOrderTbl.schema.json`
- `Data/Metadata/PlanEdits/PlanConstraints.json`
- latest generated SQL under `Data/Metadata/PlanEdits/Sql`

Recurring web references only for expected behavior context:
- `Pages/RecurringOrders.aspx`
- `Pages/RecurringOrders.aspx.cs`
- `Pages/RecurringOrderDetails.aspx`
- `Pages/RecurringOrderDetails.aspx.cs`
- `Classes/Sql/RecurringOrdersRepository.cs`

#### Engagement / architecture reminders for the next GPT session

Pay special attention to:
- `Documentation/PROJECT_OVERVIEW.md`
- `Documentation/HARD_PROJECT_RULES.md`

Non-negotiable rules still apply:
- **no Access usage in the web application**
- **no OleDb in the web application**
- **repository pattern only for application data**
- **no `SqlDataSource`**
- **no `ObjectDataSource`**
- **no `TrackerDb` for the recurring SQL pages**
- use current SQL names only

#### Updated recurring checklist

- [x] Rework `RecurringOrders.aspx` into a readable SQL-backed grouped layout
- [x] Rework `RecurringOrderDetails.aspx` into SQL/repository-backed editing
- [x] Restore `Calc Next Required` on the recurring list page
- [x] Fix enabled-only filtering to match live recurring header behavior
- [x] Confirm `Cuth Bland` and `Ana Corrochano` display correctly against remigrated SQL behavior
- [x] Bring recurring pages close to acceptance for current testing
- [x] Verify why recurring `DeliveryByID` did not migrate into SQL staging / normalized headers
- [x] Determine that current legacy recurring source does not actually persist `DeliveryByID`; fallback/backfill from `ContactsTbl.PreferedAgentID` is the current safe behavior
- [ ] Re-run recurring migration safely if recurring tables are rebuilt again and re-verify fallback/backfill behavior
- [x] Re-test `RecurringOrders.aspx` and `RecurringOrderDetails.aspx` after delivery-person migration is corrected
- [x] Finish current SQL-backed recurring page pass for list/details behavior
- [ ] Fold recurring-specific dropdown ordering / display logic into shared repository methods where practical

#### Reuse note for future page migrations

When a page needs a standard lookup list (especially `ItemsTbl` dropdowns), do **not** keep re-implementing ordering / disabled-display behavior in each page.

Preferred direction:
- put standard lookup ordering in the table-specific repository (for example `Classes/Sql/ItemsRepository.cs`)
- or in one shared repository/helper path used by multiple pages
- keep page code focused on binding, not on recreating the same lookup ordering rules repeatedly

Current recurring work exposed this specifically for:
- item dropdown ordering by enabled state + `SortOrder`
- disabled item display formatting (underscore prefix / standard display text)

### Session 5 (2026-04-24) - Recurring Orders Paused for Schema Redesign

### Session 6 (2026-04-24) - Recurring Orders SQL Verification After Redesign

#### Current Resume Point (Read This Before Touching `RecurringOrders.aspx` Again)

The SQL-backed recurring page was updated to the redesigned header/line schema and now builds, but a live SQL check confirmed that the migration output is still wrong.

**Confirmed by direct SQL comparison:**

`Cuth Bland`
- legacy `ReoccuringOrderTbl`: `4` rows
- new normalized target: `1` recurring line
- the four `0.1` rows appear to have been merged into one `0.4` row

`Ana Corrochano`
- legacy table: `2` rows
- one legacy row was enabled and one was disabled
- new normalized target has both lines under one enabled header
- this means the old row-level enabled behavior was not preserved

**What this means:**
- the ugly list layout was a page issue and was partially cleaned up
- disabled `ItemsTbl` rows were temporarily filtered out of `RecurringOrders.aspx`
- at that point the missing / merged recurring lines were confirmed as a migration issue, not a page issue

**Important blocker at that point:**
- if the redesigned schema truly keeps `Enabled` at header level only, mixed enabled/disabled legacy recurring rows cannot be represented unless migration splits them into separate headers

**Do not continue polishing `RecurringOrders.aspx` until this is resolved in migration.**

### Session 7 (2026-04-26) - Recurring Migration Rechecked After Fix

#### Current Resume Point (Read This Before Touching `RecurringOrders.aspx` Again)

The recurring migration has now been re-run and the specific row-collapse bug appears fixed.

**Verified by live SQL:**

`Cuth Bland`
- legacy `ReoccuringOrderTbl`: `4` rows
- normalized live target: `4` rows
- each normalized row still has `QtyRequired = 0.1`
- the old bad merge into one `0.4` row appears fixed

`Ana Corrochano`
- legacy table: `2` rows
- normalized live target: `2` rows
- current live normalized shape represents mixed enabled state by splitting into:
  - one enabled header
  - one disabled header

**Current important caveat:**
- live SQL still has only `RecurringOrdersTbl.Enabled`
- live `RecurringOrderItemsTbl` does **not** currently have a line-level `Enabled` column
- so the current behavior is split-header enabled handling, not line-level enabled handling

#### What remains before the recurring page is considered done

1. Re-check `RecurringOrders.aspx` against the remigrated live data
2. Confirm the enabled-only list now behaves correctly for:
   - `Ana Corrochano`
   - `Cuth Bland`
3. Decide whether the accepted business model is now:
   - split headers for mixed enabled rows
   - or a future schema revision with line-level `Enabled`
4. Update the recurring pages only to match the now-accepted live SQL design

#### Resume prompt for recurring web page follow-up

> Read `Documentation/PROJECT_OVERVIEW.md`, `Documentation/WorkInProgress/MIGRATION_TODO.md`, and `Documentation/AI/MigrationRunner/STATUS_BOOKMARK.md` first. The recurring migration has been rerun and live SQL now shows that `Cuth Bland` has 4 normalized recurring lines again and `Ana Corrochano` has 2 normalized recurring records represented as separate enabled/disabled headers. Re-verify the live SQL data first, then inspect `Pages/RecurringOrders.aspx`, `Pages/RecurringOrders.aspx.cs`, `Classes/Sql/RecurringOrdersRepository.cs`, `Pages/RecurringOrderDetails.aspx`, and `Pages/RecurringOrderDetails.aspx.cs` to determine what changes are still required so the recurring list/details pages match the remigrated SQL behavior, remain readable, and filter enabled-only results correctly. Keep everything SQL-only, repository-based, no `SqlDataSource`, no `ObjectDataSource`, no `TrackerDb`, and use only SQL names.`

#### Exact web-page return point after migration is fixed

Once recurring migration is corrected and rerun, come back to:
- `Pages/RecurringOrders.aspx`
- `Pages/RecurringOrders.aspx.cs`
- `Classes/Sql/RecurringOrdersRepository.cs`

and verify:
1. `Cuth Bland` shows `4` recurring lines again
2. `Ana Corrochano` does not show the disabled line in the enabled-only list
3. grouped contact display is readable and close to the legacy layout
4. list filtering matches the intended business rule for enabled/disabled recurring rows
5. details page still shows associated lines correctly for editing

#### Resume prompt for after migration fix

> Read `Documentation/PROJECT_OVERVIEW.md`, `Documentation/WorkInProgress/MIGRATION_TODO.md`, and `Documentation/AI/MigrationRunner/STATUS_BOOKMARK.md` first. The recurring page already builds against the redesigned SQL schema, but direct SQL verification showed the migration output was still wrong: `Cuth Bland` was collapsed from 4 legacy recurring rows to 1 normalized line, and `Ana Corrochano` lost mixed enabled/disabled row behavior under the new header-only enabled model. Assume the migration has now been fixed and rerun. Re-verify `AccessSrc.ReoccuringOrderTbl` vs `RecurringOrdersTbl` and `RecurringOrderItemsTbl` for `Cuth Bland` and `Ana Corrochano`, then finish the `RecurringOrders.aspx` pass so the list is readable, excludes rows that should not appear in enabled-only mode, and accurately reflects the corrected normalized recurring data. Keep everything SQL-only, repository-based, no `SqlDataSource`, no `ObjectDataSource`, no `TrackerDb`, and use only SQL names.`

#### Current Resume Point (Read This Before Touching Recurring Orders Again)

The recurring-order page work is now **paused on purpose**.

The current SQL design for recurring orders is now considered **incorrect for the business rules** and must be redesigned in the migration project before more page work is done.

**Why it is paused:**
- The current normalized SQL shape still keeps recurrence-driving fields at the header level.
- That makes one recurring header try to represent multiple different item schedules.
- Real data needs item-level recurrence, dates, and related schedule state.
- The current UI can be made prettier, but it will still be wrong until the schema is corrected and the data is re-migrated.

**Current incorrect SQL structure in code today:**

`RecurringOrdersTbl` currently behaves like this:
- `RecurringOrderID`
- `ContactID`
- `RecurringTypeID`
- `Value`
- `DateLastDone`
- `NextDateRequired`
- `RequireUntilDate`
- `Enabled`
- `Notes`

`RecurringOrderItemsTbl` currently behaves like this:
- `RecurringOrderItemID`
- `RecurringOrderID`
- `ItemRequiredID`
- `QtyRequired`
- `ItemPackagingID`

**Why that is wrong:**
- `RecurringTypeID`
- `Value`
- `DateLastDone`
- `NextDateRequired`
- `RequireUntilDate`
- likely some enable/notes behavior depending on final rules

These are schedule-driving fields and do **not** belong at the header level if one contact/header can have multiple item lines with different recurrence patterns.

**Expected direction of the corrected design:**

`RecurringOrdersTbl` should likely become mostly contact/header-level data only, for example:
- `RecurringOrderID`
- `ContactID`
- header-only fields that truly apply to the whole group

`RecurringOrderItemsTbl` should likely become the line-level schedule table, for example:
- `RecurringOrderItemID`
- `RecurringOrderID`
- `ItemRequiredID`
- `QtyRequired`
- `ItemPackagingID`
- `RecurringTypeID`
- `Value`
- `DateLastDone`
- `NextDateRequired`
- `RequireUntilDate`
- line-level `Enabled` if required by business rules
- line-level `Notes` if required by business rules

**Files currently affected by this pause:**
- `Pages/RecurringOrders.aspx`
- `Pages/RecurringOrders.aspx.cs`
- `Pages/RecurringOrderDetails.aspx`
- `Pages/RecurringOrderDetails.aspx.cs`
- `Classes/Sql/RecurringOrdersRepository.cs`
- `Classes/Poco/RecurringOrder.cs`
- `Classes/Poco/RecurringOrderItem.cs`
- `Classes/Poco/RecurringOrderSummary.cs`
- migration output and metadata used by `Migrations/MigrationRunner`

**UI status at pause point:**
- `RecurringOrders.aspx` is SQL-backed and builds successfully
- the list page was reworked to display grouped recurring rows by contact
- this page should now be treated as provisional only
- after the schema redesign, the recurring list page, POCOs, repository, and details page will all need another pass

**Data/schema inputs needed before resuming code changes:**
1. Exact **current SQL schema** for:
   - `RecurringOrdersTbl`
   - `RecurringOrderItemsTbl`
2. Exact **new intended SQL schema** for both tables after redesign
3. Clear field movement list:
   - what moves from header to line
   - what stays on header
   - what is deleted
   - what is newly added
4. One or two concrete business examples, such as:
   - one contact with 3 recurring item lines, each with different recurrence
5. If possible, one mapping example from:
   - old `AccessSrc.ReoccuringOrderTbl`
   - to new `RecurringOrdersTbl`
   - and new `RecurringOrderItemsTbl`

**Resume prompt for next time:**

> Read `Documentation/PROJECT_OVERVIEW.md` and `Documentation/WorkInProgress/MIGRATION_TODO.md` first. The recurring-order work is paused because the current SQL normalization is wrong. Use the new post-redesign schema for `RecurringOrdersTbl` and `RecurringOrderItemsTbl` that I provide, then update the recurring-order POCOs, repository, summaries, and pages to match the new design. Rework `Classes/Poco/RecurringOrder.cs`, `Classes/Poco/RecurringOrderItem.cs`, `Classes/Poco/RecurringOrderSummary.cs`, `Classes/Sql/RecurringOrdersRepository.cs`, `Pages/RecurringOrders.aspx`, `Pages/RecurringOrders.aspx.cs`, `Pages/RecurringOrderDetails.aspx`, and `Pages/RecurringOrderDetails.aspx.cs` so they reflect the corrected header/line split. Assume the migration has already been rerun from `Migrations/MigrationRunner`. Keep everything SQL-only, repository-based, no `SqlDataSource`, no `ObjectDataSource`, no `TrackerDb`, and use only the SQL names.

**Do not resume recurring-order code work until the remigrated schema and sample data are available.**

### Session 4 (2026-04-23) - Recurring Orders Migration Started

#### Current Resume Point (Read This Before Touching Recurring Orders Again)

The recurring-order migration has **started but is not complete**.

**Pages currently in play:**
- `Pages/RecurringOrders.aspx`
- `Pages/RecurringOrders.aspx.cs`
- `Pages/RecurringOrderDetails.aspx`
- `Pages/RecurringOrderDetails.aspx.cs`
- `Classes/Sql/RecurringOrdersRepository.cs`
- `Classes/Poco/RecurringOrder.cs`
- `Classes/Poco/RecurringOrderItem.cs`
- `Classes/Poco/RecurringOrderSummary.cs`

**What was completed in this session:**
- Removed malformed markup issues on the recurring pages
- Converted `RecurringOrders.aspx` list page to bind via `RecurringOrdersRepository`
- Converted `RecurringOrderDetails.aspx` away from `SqlDataSource` / `ObjectDataSource`
- Replaced Access-backed page usage with SQL-backed repository usage on the current recurring pages
- Fixed legacy SQL joins in the recurring orders grid from old names to SQL names (`ItemsTbl`, `ItemPackagingsTbl`)
- Fixed `DisableClient.aspx` so it inherits from `System.Web.UI.Page` again and works via SQL only

**What is NOT complete yet:**
- `RecurringOrderDetails.aspx` is **not done**
- The `Recurring Orders` page is now blocked by schema redesign and remigration
- The recurring item migration appears to be incomplete / incorrect in the database migration output
- Item names are missing in the recurring list because the normalized item rows were not migrated correctly

**Confirmed blocker:**
- The recurring-order normalization in the migration project appears incomplete.
- Old source: `AccessSrc.ReoccuringOrderTbl`
- Target tables: `RecurringOrdersTbl` and `RecurringOrderItemsTbl`
- Before finishing the page migration, fix the migration project and re-validate that item rows are present and joined correctly.

**How to resume next time using only this TODO + `Documentation/PROJECT_OVERVIEW.md`:**
1. Re-read the hard rules in `PROJECT_OVERVIEW.md`:
   - no Access usage
   - no `TrackerDb`
   - no `SqlDataSource` / `ObjectDataSource`
   - repository pattern only
2. Re-check the migration output for recurring orders:
   - compare `AccessSrc.ReoccuringOrderTbl`
   - against `RecurringOrdersTbl`
   - and `RecurringOrderItemsTbl`
3. Once migration data is corrected, return to the recurring pages and verify:
   - item descriptions display in the grid
   - quantities and packaging still display correctly
   - details page load / update / insert / delete all work against SQL only
4. Keep all future recurring-order work aligned to the SQL names documented in `PROJECT_OVERVIEW.md`:
   - `ItemsTbl` not `ItemTypeTbl`
   - `ItemPackagingsTbl` not `PackagingTbl`
   - `RecurranceTypesTbl` / `RecurringOrdersTbl` / `RecurringOrderItemsTbl`

#### Remaining Work Checklist - Recurring Orders

- [x] Remove malformed markup issues from recurring pages
- [x] Remove page-level `SqlDataSource` / `ObjectDataSource` usage from recurring details page
- [x] Introduce SQL repository-backed recurring page binding
- [x] Fix legacy grid query joins to SQL table names
- [x] Bookmark current recurring-order UI/code state before redesign
- [ ] Redesign recurring-order header/line schema in migration project
- [ ] Re-run migration with corrected recurring-order normalization
- [ ] Update recurring-order POCOs to match new schema
- [ ] Update recurring-order repository to match new schema
- [ ] Update recurring-order summary/query shape to match new schema
- [ ] Fix migration project so normalized recurring item rows populate correctly
- [ ] Re-test `RecurringOrders.aspx` grid after data migration fix
- [ ] Confirm `ItemsDisplay` shows item names, qty, and packaging
- [ ] Finish `RecurringOrderDetails.aspx` migration and validation
- [ ] Verify insert / update / delete behavior end-to-end against SQL only
- [ ] Remove any remaining legacy recurring-order code paths once replacement is confirmed

### Session 3 (2025-03-28) - ContactsAway Pages + SupportTables

#### Changes Made (Latest):

1. **ContactsAwayPeriodRepository.cs - SQL Join Fix**
   - Fixed wrong table name: `AwayReasonsTbl` → `AwayReasonTbl` (singular)
   - Fixed wrong join column: `r.ReasonID` → `r.AwayReasonID`
   - This was causing empty results on the list page

2. **CustomersAway.aspx + CustomersAwayDetail.aspx - Terminology Update**
   - Renamed all "Customer" text to "Contact" per project naming conventions
   - Updated page titles: "Contacts Away", "Contact Away Detail"
   - Updated labels: "Customer" → "Contact"
   - Updated empty data message: "No contacts are currently away"

3. **CustomersAwayDetail.aspx - Fixed Access DB Usage**
   - Removed all ObjectDataSource controls
   - Replaced `CustomersAwayTbl` (Access) with `ContactsAwayPeriodRepository` (SQL Server)
   - Added `ContactsRepository.GetAllCompanyNames()` for dropdown
   - Added `AwayReasonRepository.GetAll()` for dropdown
   - Added JavaScript to auto-set end date when start date changes

4. **SupportTables.aspx - Major Enhancement**
   - Expanded from 5 tables to 10 lookup tables
   - Added image buttons (EditItem.gif, UpdateItem.gif, CancelItem.gif)
   - Tables now supported:
     - ✅ Areas, Area Prep Days, Equipment Types, Invoice Types
     - ✅ Item Packaging, Items, Payment Terms, People / Delivery By
     - ✅ Price Levels, Repair Statuses

5. **Project File Fix (TrackerSQL.csproj)**
   - Changed `<RootNamespace>TrackerDotNet</RootNamespace>` → `<RootNamespace>TrackerSQL</RootNamespace>`
   - Changed `<AssemblyName>TrackerDotNet</AssemblyName>` → `<AssemblyName>TrackerSQL</AssemblyName>`

### Pages Modified in TIER 1 Migration

| Page | Test Status | Test Notes |
|------|-------------|------------|
| `Pages/Contacts.aspx` | ✅ Mostly Done | Grid loads, Filter works, Reset button fixed |
| `Tools/TestPeople.aspx` | ✅ Done | Grid loads, Edit/Update works |
| `Pages/SupportTables.aspx` | ✅ **DONE** | 10 tables, image buttons, edit/update works |
| `Pages/CustomersAway.aspx` | 🧪 **DONE** | SQL fix applied, terminology updated |
| `Pages/CustomersAwayDetail.aspx` | 🧪 **DONE** | Fully converted to SQL Server |

#### ContactsAway + ContactsAwayDetail Test Checklist:
- [ ] List page loads without error
- [ ] Grid displays currently away contacts by default
- [ ] "All Periods" shows all away periods
- [ ] "Custom Range" date filter works
- [ ] Company name filter works
- [ ] Reset clears all filters
- [ ] **Add** new away period works
- [ ] **Edit** existing away period works (click Edit link)
- [ ] **Update** saves changes correctly
- [ ] **Delete** away period works
- [ ] Cancel button returns to list
- [ ] Confirmation email sends (if configured)

### Issues Fixed (2025-03-27 Session 2):

1. **SupportTables.aspx - AreaPrepDays not working**
   - **Cause:** `AreaPrepDays.cs` POCO was missing namespace declaration
   - **Fix:** Recreated the file with proper namespace

2. **CustomersAwayDetail.aspx - Crash when adding record**
   - **Cause:** `CustomersAwayTbl.cs` was using old `TrackerDb` (Access) instead of `TrackerSQLDb` (SQL Server)
   - **Fix:** Rewrote entire `CustomersAwayTbl.cs` to use `TrackerSQLDb` with parameterized queries
   - **Tables renamed:** 
     - `ClientAwayPeriodTbl` → `ContactsAwayPeriodTbl`
     - `CustomersTbl` → `ContactsTbl`  
     - `AwayReasonTbl` → `AwayReasonsTbl`
     - `ClientID` → `ContactID`

3. **SupportTables.aspx - Sort column error when switching tables**
   - **Cause:** ViewState retained sort column from previous table
   - **Fix:** Reset sort expression when table selection changes

---

## 📋 PAGES STILL TO VERIFY (Post Namespace Change)

These pages should be smoke-tested to ensure they work after the TrackerDotNet → TrackerSQL namespace change:

| Page | Priority | Test Status | Notes |
|------|----------|-------------|-------|
| `Default.aspx` | HIGH | ⬜ Pending | Home page - must work |
| `Lookups.aspx` | HIGH | ⬜ Pending | Full lookup editor - many repos |
| `Pages/Contacts.aspx` | HIGH | ⬜ Pending | Contact list |
| `Pages/ContactDetails.aspx` | HIGH | 🧪 Testing | Contact editing; predictive-item data may still expose migration/query issues |
| `Pages/RecurringOrders.aspx` | HIGH | 🧪 WIP | Grid loads from SQL repo but recurring item migration data is incomplete |
| `Pages/RecurringOrderDetails.aspx` | HIGH | 🧪 WIP | Partial SQL migration; not finished |
| `Pages/NewOrder.aspx` | MEDIUM | ⬜ Pending | Order creation |
| `Pages/OrderDetail.aspx` | HIGH | ✅ Done | Manual Save / Save & Return; line edit save; mobile New Item; 2026-07-13 |
| `Pages/NewOrderDetail.aspx` | LOW | 💤 Legacy/Retired | Replaced by `OrderDetail.aspx?NewOrder=true`; keep only for legacy bookmarks if needed |
| `Account/Login.aspx` | HIGH | ⬜ Pending | Must work for any testing |

### Quick Smoke Test Checklist

1. [ ] **Close and reopen Visual Studio** (to pick up .csproj changes)
2. [ ] Clean and Rebuild solution
3. [ ] Application starts without assembly load errors
4. [ ] Login page works
5. [ ] Default.aspx loads (home page)
6. [ ] Contacts list loads
7. [ ] SupportTables page works (select each table)
8. [ ] Lookups page loads all tabs

---

## ✅ COMPLETED PAGES (Fully Migrated)

| Page | Completion Date | Notes |
|------|-----------------|-------|
| `Pages/OrderEntry.aspx` | 2026-07-22 | UI retrofit: page-tone-orders View/Edit list; home card fixed. See COMPLETED_TASKS.md |
| `Pages/PreperationSummary.aspx` | 2026-07-21 | UI retrofit: page-tone-summary, stacked date filters. See COMPLETED_TASKS.md |
| `Pages/RecurringOrders.aspx` + `RecurringOrderDetails.aspx` | 2026-07-20 | UI retrofit: page-tone-recurring, image-button, grouping-table. See COMPLETED_TASKS.md + WEBFORMS_UI_STANDARDS §3a |
| `Tools/XMLtoSQL.aspx` | 2026-07-14 | TrackerSQLDb + UI standards + SQLCommands_Test_SQLServer.xml. See COMPLETED_TASKS.md |
| `Tools/MessagesEditor.aspx` | 2026-07-14 | Manager already; status bottom + UpdateProgress. See COMPLETED_TASKS.md |
| `Tools/SystemTools.aspx` | 2026-07-14 | Hub: repo bind prep grid; removed ODS + Set Client Type card. See COMPLETED_TASKS.md |
| `Tools/HolidayClosures.aspx` + `HolidayClosureDetail.aspx` | 2026-07-14 | Repo + manager; UI standards; unique start date + range overlap. See COMPLETED_TASKS.md |
| `Pages/OrderDetail.aspx` | 2026-07-13 | OrderID model + repos; Save/Save&Return; line edit; mobile New Item. See COMPLETED_TASKS.md |
| `Pages/SupportTables.aspx` | 2025-03-28 | 10 lookup tables, image buttons, full CRUD |
| `Pages/Contacts.aspx` | 2025-03-27 | ContactSummariesRepository, manual binding |
| `Tools/TestPeople.aspx` | 2025-03-27 | PersonsRepository, manual binding |
| `Pages/CustomersAway.aspx` | 2025-03-27 | ContactsAwayPeriodRepository |
| `Pages/CustomersAwayDetail.aspx` | 2025-03-27 | CustomersAwayTbl rewritten for SQL Server |

---

## 🚨 CRITICAL RULES

1. **NO Access Database** - We do NOT connect to Access. All pages must use SQL Server via `TrackerSQLDb`
2. **NO ObjectDataSource** - Remove ALL ObjectDataSource controls, use manual binding
3. **NO SqlDataSource** (except `sdsUserNames` for ASP.NET Membership)
4. **NO TrackerDb** - Use `TrackerSQLDb` via Repository pattern only
5. **Each page is ALL or NOTHING** - Either fully migrated or not started

---

## 📊 Migration Status Summary

| Phase | Pages | Migrated | Remaining |
|-------|-------|----------|-----------|
| **TIER 0: No DB** | 19 | ✅ 19 | 0 |
| **TIER 1: Simple** | 11 | ✅ 6 | 5 |
| **TIER 2: Medium** | 10 | 0 | 10 |
| **TIER 3: Complex** | 11 | 0 | 11 |
| **TIER 4: Critical** | 6 | 0 | 6 |
| **TOTAL** | **57** | **25** | **32** |

---

## ✅ TIER 0: Pages with NO Database Access (Already Done!)

These pages have NO database calls - they're already migrated by definition.

| Page | Status | Notes |
|------|--------|-------|
| `Tools/About.aspx` | ✅ Done | Static content only |
| `Tools/GenMachineKeys.aspx` | ✅ Done | Static content |
| `Tools/AutoClassMaker.aspx` | ✅ Done | Dev tool, static |
| `Pages/LeaveApp.aspx` | ✅ Done | Embedded iframe only |
| `Pages/Print.aspx` | ✅ Done | Print styling only |
| `Pages/QuaffeeCoffeeTastingSheet.aspx` | ✅ Done | Static content |
| `Pages/OrderBuiten2Vineyard.aspx` | ✅ Done | Static/redirect |
| `Pages/OrderVineyard2Buiten.aspx` | ✅ Done | Static/redirect |
| `Pages/SummaryOFCoffeeRequired.aspx` | ✅ Done | Static content |
| `Pages/LoadSendCoffeeCheckup.aspx` | ✅ Done | Redirect only |
| `Tools/MessagesEditor.aspx` | ✅ Done | File-based, no DB |
| `Tools/LogViewer.aspx` | ✅ Done | File-based logs |
| `Account/Register.aspx` | ✅ Done | ASP.NET Membership |
| `Account/RecoverPassword.aspx` | ✅ Done | ASP.NET Membership |
| `Account/ChangePasswordSuccess.aspx` | ✅ Done | Static content |
| `Administration/ManageRoles.aspx` | ✅ Done | ASP.NET Membership |
| `Administration/ManageUsers.aspx` | ✅ Done | ASP.NET Membership |
| `Administration/UserInformation.aspx` | ✅ Done | ASP.NET Membership |
| `Administration/ResetPasswordHelper.aspx` | ✅ Done | ASP.NET Membership |

**Also Done:**
| Page | Status | Notes |
|------|--------|-------|
| `Default.aspx` | ✅ Done | Uses `TotalCountTrackerRepository` |
| `HttpErrorPage.aspx` | ✅ Done | No database access |
| `Lookups.aspx` | ✅ Done | Fully repository-based (29 repo refs) |

---

## 🟢 TIER 1: Simple Pages (Score 0-10)

**Priority: START HERE** - Low complexity, easy wins.

### 1.1 Pages with Login/Account Controls Only ✅ DONE

| Page | SqlDS | ObjDS | Controls | Status |
|------|-------|-------|----------|--------|
| `Account/ChangePassword.aspx` | 0 | 0 | 1 | ✅ ASP.NET Membership only |
| `Account/Login.aspx` | 0 | 0 | 1 | ✅ ASP.NET Membership only |

---

### 1.2 Pages with Simple Repository Needs ✅ DONE

| Page | Status | Notes |
|------|--------|-------|
| `Pages/Contacts.aspx` | ✅ Done | Uses `ContactSummariesRepository`, manual binding |
| `Tools/TestPeople.aspx` | ✅ Done | Uses `PersonsRepository`, manual binding |
| `Pages/SupportTables.aspx` | ✅ Done | Uses multiple repos (Items, People, Equipment, Packaging) |
| `Pages/CustomersAway.aspx` | ✅ Done | Uses `ContactsAwayPeriodRepository.GetAwaySummaries()` |

**Migration Date:** 2025-03-27

**Changes Made:**
- Removed all ObjectDataSource controls
- Added manual data binding in code-behind
- Created `ContactsAwaySummary` POCO for list view
- Enhanced `ContactsAwayPeriodRepository` with `GetAwaySummaries()` method
- Created designer files where missing

---

### 1.3 Remaining TIER 1 Pages

| Page | ObjDS Count | Current TypeName | Action Needed |
|------|-------------|------------------|---------------|
| `Pages/ClientList.aspx` | 2 | `CustomersDataSetTableAdapters` | Needs `ContactSummariesRepository` |
| `Pages/ThisWeeksOrder.aspx` | 2 | `OpenOrdersDataSetTableAdapters` | Needs `OrdersRepository` |
| `Pages/RecurringOrders.aspx` | 2 | `ReoccuringOrderDAL` | **WIP** - partially migrated to `RecurringOrdersRepository`; revisit after recurring item migration fix |

**Status:** 🧪 In Progress  
**Est. Time:** 1-2 hours each

---

### 1.2 Pages with Simple Repository Needs

| Page | SqlDS | ObjDS | OleDb | Controls Refs | Current TypeName |
|------|-------|-------|-------|---------------|------------------|
| `Pages/Contacts.aspx` | 0 | 2 | 0 | 0 | `ContactSummariesRepository` ✅ |
| `Pages/ContactsTest.aspx` | 0 | 0 | 0 | 3 | Already uses repos in code-behind |

**1.2.1 Contacts.aspx**
- **Current:** Uses `ObjectDataSource` with `ContactSummariesRepository`
- **Action:** Remove ODS, manual bind in code-behind
- **Repo Exists:** ✅ `ContactSummariesRepository`
- **Status:** ❌ Not Started
- **Est. Time:** 1 hour

**1.2.2 ContactsTest.aspx**
- **Current:** Uses repositories directly in code-behind
- **Action:** Verify no legacy Controls refs
- **Status:** ❌ Not Started  
- **Est. Time:** 30 min

---

### 1.3 Pages with ObjectDataSource Only (No Legacy Controls)

| Page | ObjDS Count | Current TypeName | Action |
|------|-------------|------------------|--------|
| `Pages/ClientList.aspx` | 2 | `CustomersDataSetTableAdapters` | Needs `ContactSummariesRepository` |
| `Pages/ThisWeeksOrder.aspx` | 2 | `OpenOrdersDataSetTableAdapters` | Needs `OrdersRepository` |
| `Pages/RecurringOrders.aspx` | 2 | `ReoccuringOrderDAL` | **WIP** - SQL repo in place, GridView still needs work after migration-data fix |
| `Pages/CustomersAway.aspx` | 2 | `CustomersAwayTbl` | Needs `ContactsAwayPeriodRepository` |
| `Tools/TestPeople.aspx` | 2 | `PersonsTbl` | Use `PersonsRepository` ✅ |
| `Pages/SupportTables.aspx` | 2 | `ItemTypeTbl` | Use `ItemsRepository` ✅ |

**Status:** 🧪 Partially Complete  
**Est. Time:** 1-2 hours each

---

### 1.4 Pages with SqlDataSource Only

| Page | SqlDS Count | Action |
|------|-------------|--------|
| _(none outstanding)_ | | |

~~`Pages/PreperationSummary.aspx`~~ — completed 2026-07-21 (repo + UI retrofit). See COMPLETED_TASKS.md.

**Status:** ✅ Complete  
**Est. Time:** —

---

## 🟡 TIER 2: Medium Pages (Score 10-20)

**Priority: After Tier 1** - Some complexity, multiple data sources.

### 2.1 ObjectDataSource with Single Legacy Type

| Page | ObjDS | TypeName(s) | Repos Needed |
|------|-------|-------------|--------------|
| `Pages/LogTable.aspx` | 2 | `LogTbl` | Create `LogRepository` |
| `Tools/HolidayClosures.aspx` | 0 | (code-behind refs) | Use `HolidayClosuresRepository` ✅ |
| `Tools/EmailDiagnostics.aspx` | 0 | (code-behind refs) | Review - may be file-based |

**Status:** ❌ Not Started  
**Est. Time:** 2 hours each

---

### 2.2 Mixed SqlDataSource + ObjectDataSource

| Page | SqlDS | ObjDS | TypeNames | Action |
|------|-------|-------|-----------|--------|
| `Pages/DeleteOrderLine.aspx` | 2 | 0 | - | Convert SqlDS to `OrdersRepository` |
| `Tools/MoveDeliveryDate.aspx` | 2 | 2 | `NextPreperationDateByAreaTbl` | Create `AreaPrepDaysRepository` or use existing |
| `Pages/TestPage.aspx` | 4 | 0 | - | Convert all SqlDS |
| `Pages/CoffeeRequired.aspx` | 4 | 0 | - | Convert all SqlDS |

**Status:** ❌ Not Started  
**Est. Time:** 2-3 hours each

---

### 2.3 ObjectDataSource with Multiple Types

| Page | ObjDS | TypeNames |
|------|-------|-----------|
| `Pages/Repairs.aspx` | 4 | `RepairsTbl`, `RepairStatusesTbl` |
| `Pages/RepairStatusChange.aspx` | 4 | `EquipTypeTbl`, `RepairStatusesTbl` |
| `Pages/SendCoffeeCheckup.aspx` | 4 | `ContactToRemindDetails`, `TempCoffeeCheckup` |
| `Pages/SentRemindersSheet.aspx` | 4 | `SentRemindersLogTbl` |
| `Pages/OrderSheet.aspx` | 4 | `PersonsLkupTableAdapter`, `OrderSheetTableAdapter` |

**Repos Needed:**
- `RepairsRepository` (check if exists)
- `SentRemindersLogRepository` (create)
- Create adapters for order sheets

**Status:** ❌ Not Started  
**Est. Time:** 2-3 hours each

---

## 🟠 TIER 3: Complex Pages (Score 20-35)

**Priority: After Tier 2** - Multiple data sources, business logic.

### 3.1 Medium-High Complexity

| Page | SqlDS | ObjDS | Controls Refs | Key TypeNames |
|------|-------|-------|---------------|---------------|
| `Pages/CustomersAwayDetail.aspx` | 0 | 2 | 6 | `CustomersAwayTbl`, `CustomersTbl` |
| `Pages/DeliverySheet.aspx` | 0 | 2 | 0 | `ActiveDeliveryData` |
| `Tools/XMLtoSQL.aspx` | 0 | 0 | 3 | Various legacy |
| `Pages/ItemGroups.aspx` | 0 | 7 | 0 | `ItemGroupTbl`, `ItemTypeTbl` |

**Status:** ❌ Not Started  
**Est. Time:** 3-4 hours each

---

### 3.2 High Complexity

| Page | SqlDS | ObjDS | Controls Refs | Key TypeNames |
|------|-------|-------|---------------|---------------|
| `Pages/RecurringOrderDetails.aspx` | 2 | 6 | 0 | `ItemTypeTbl`, `PackagingTbl`, `ReoccuranceTypeTbl` → **WIP:** moved to SQL repo binding but page is not finished |
| `Pages/NewOrder.aspx` | 0 | 8 | 0 | Multiple LookUp adapters |
| `Pages/OrdersEdit.aspx` | 0 | 10 | 0 | `OrdersDataSet` adapters |
| `Pages/OrderEntry.aspx` | 2 | 8 | 0 | `CompanyNames`, `ItemTypeTbl`, `OrderData` |
| `Tools/SystemTools.aspx` | 2 | 2 | 2 | `CustomerTypeTbl` |
| `Tools/SystemData.aspx` | 0 | 4 | 0 | `ItemTypeTbl`, `SysDataTbl` |

**Status:** 🧪 In Progress  
**Est. Time:** 4-6 hours each

---

## 🔴 TIER 4: Critical/Most Complex Pages (Score 35+)

**Priority: LAST** - Core business functionality, highest risk.

### 4.1 Repair Management

| Page | SqlDS | ObjDS | Controls Refs | Key TypeNames |
|------|-------|-------|---------------|---------------|
| `Pages/RepairDetail.aspx` | 0 | 12 | 0 | Multiple repair types |
| `Pages/RepairDetailOld.aspx` | 0 | 14 | 0 | Legacy - consider deletion |

**Status:** ❌ Not Started  
**Est. Time:** 6-8 hours

---

### 4.2 Order Management

| Page | SqlDS | ObjDS | OleDb | Controls | Notes |
|------|-------|-------|-------|----------|-------|
| `Pages/NewOrderDetail.aspx` | — | — | — | — | 💤 Legacy/Retired → `OrderDetail.aspx?NewOrder=true` |
| `Pages/OrderDetail.aspx` | 0 | 0 | 0 | 0 | ✅ **Done 2026-07-13** — manager/repos; manual Save; line edit; mobile New Item |
| `Pages/OrderDone.aspx` | 8 | 0 | 3 | 0 | Still listed historically; verify OleDb cleared vs OrderDoneManager path |
| `Pages/ViewMyOrder.aspx` | 0 | 0 | 0 | 0 | Migrated via repos (Phase 1G); smoke-test if needed |

**Status:** ✅ `OrderDetail` complete; `NewOrderDetail` retired; confirm `OrderDone` / `ViewMyOrder` smoke tests  
**Est. Time (remaining):** smoke-test only unless OleDb found on OrderDone

---

### 4.3 Contact Management

| Page | SqlDS | ObjDS | Controls | Notes |
|------|-------|-------|----------|-------|
| `Pages/ContactDetails.aspx` | 0 | 18 | 0 | Uses modern repos in ODS |
| `Pages/CustomerDetails.aspx` | 0 | 22 | 0 | **Legacy version** - 11 legacy TypeNames |
| `Tools/MergeCustomersFromQB.aspx` | 0 | 0 | 0 | Code-behind uses 5+ legacy Tbls |
| `TrackerSQL/DisableClient.aspx` | 0 | 0 | 4 | Uses `CustomersTbl`, `ItemTypeTbl` |

**Status:** ❌ Not Started  
**Est. Time:** 10-15 hours each
### Column Renaming Patterns Discovered

| Legacy (Access) | SQL Server | Notes |
|---|---|---|
| PrepDate | PrepDate | System moving to "prep" terminology (not coffee-specific) |
| ReqDate UI | RequiredByDate | UI shorthand vs full column name |
| ... | ... | ... |

**Action:** Check migration CSV for ALL pages - this pattern likely affects Orders, OrderDetail, and other prep-related pages.
---

## 📋 Repository Availability Matrix

### ✅ Repositories That Exist (Ready to Use)

| Repository | POCO | Notes |
|------------|------|-------|
| `AreasRepository` | `Area` | |
| `AwayReasonRepository` | `AwayReason` | |
| `ClosureDatesRepository` | `ClosureDate` | |
| `ContactsRepository` | `Contact` | |
| `ContactSummariesRepository` | `ContactSummary` | |
| `ContactTypesRepository` | `ContactType` | |
| `ContactsAwayPeriodRepository` | `ContactsAwayPeriod` | |
| `EquipConditionsRepository` | `EquipCondition` | |
| `EquipTypesRepository` | `EquipType` | |
| `HolidayClosuresRepository` | `HolidayClosure` | |
| `InvoiceTypesRepository` | `InvoiceType` | |
| `ItemGroupsRepository` | `ItemGroup` | |
| `ItemPackagingsRepository` | `ItemPackaging` | |
| `ItemPrepTypesRepository` | `ItemPrepType` | |
| `ItemsRepository` | `Item` | |
| `ItemUnitsRepository` | `ItemUnit` | |
| `OrdersRepository` | `Order`, `OrderLine` | |
| `PaymentTermsRepository` | `PaymentTerm` | |
| `PersonsRepository` | `Person` | |
| `PriceLevelsRepository` | `PriceLevel` | |
| `RecurringOrdersRepository` | `RecurringOrder` | |
| `RepairStatusesRepository` | `RepairStatus` | |
| `ServiceTypesRepository` | `ServiceType` | |
| `TotalCountTrackerRepository` | `TotalCountTracker` | |

### ❌ Repositories Needed (Must Create)

| Needed Repository | POCO Exists? | Legacy Class | Used By |
|-------------------|--------------|--------------|---------|
| `LogRepository` | ❌ Need `Log.cs` | `LogTbl` | `LogTable.aspx` |
| `SentRemindersLogRepository` | ✅ `SentRemindersLog` | `SentRemindersLogTbl` | `SentRemindersSheet.aspx` |
| `RepairsRepository` | ✅ `Repair` | `RepairsTbl` | `Repairs.aspx`, `RepairDetail.aspx` |
| `RepairFaultsRepository` | ✅ `RepairFault` | `RepairFaultsTbl` | `RepairDetail.aspx` |
| `SysDataRepository` | ✅ `SysData` | `SysDataTbl` | `SystemData.aspx` |
| `SectionTypesRepository` | ✅ `SectionType` | `SectionTypesTbl` | `LogTable.aspx` |
| `TransactionTypesRepository` | ✅ `TransactionType` | `TransactionTypesTbl` | `LogTable.aspx` |
| `TempOrdersRepository` | ✅ `TempOrder*` | `TempOrdersHeaderTbl` | `OrderDone.aspx` |
| `ActiveDeliveryDataRepository` | ❌ Need POCO | `ActiveDeliveryData` | `DeliverySheet.aspx` |

---

## 🔧 Migration Pattern (For Each Page)

### Step 1: Analyze
```powershell
# Check what the page uses
Get-Content "Pages\YourPage.aspx" -Raw | Select-String "ObjectDataSource|SqlDataSource" -AllMatches
Get-Content "Pages\YourPage.aspx.cs" -Raw | Select-String "new \w+Tbl|TrackerDb" -AllMatches
```

### Step 2: Create Missing Repository (if needed)
```csharp
// Classes/Sql/YourRepository.cs
public class YourRepository : RepositoryBase<YourPoco>
{
    protected override string TableName => "YourTable";
    protected override string KeyColumn => "YourID";

    // Add CRUD methods as needed
}
```

### Step 3: Update Code-Behind
```csharp
// BEFORE (Legacy)
var tbl = new SomethingTbl();
var data = tbl.GetAll();
gvData.DataSource = data;

// AFTER (Repository)
var repo = new SomethingRepository();
var data = repo.GetAll();
gvData.DataSource = data;
gvData.DataBind();
```

### Step 4: Update ASPX
```aspx
<!-- REMOVE ObjectDataSource -->
<!-- <asp:ObjectDataSource ID="odsData" TypeName="..." /> -->

<!-- REMOVE DataSourceID from GridView -->
<asp:GridView ID="gvData" runat="server">
    <!-- Remove: DataSourceID="odsData" -->
</asp:GridView>
```

### Step 5: Build & Test
```powershell
# Build solution
msbuild TrackerSQL.sln /t:Build

# Manual test the page
```

---

## 📅 Recommended Migration Order

### Week 1: Foundation
1. ✅ Verify TIER 0 pages (already done)
2. Create missing repositories for TIER 1
3. Migrate TIER 1 pages (9 pages)

### Week 2: Build Momentum  
4. Create repositories for TIER 2
5. Migrate TIER 2 pages (10 pages)

### Week 3: Core Pages
6. Create repositories for TIER 3
7. Migrate TIER 3 pages (11 pages)

### Week 4: Critical & Cleanup
8. Migrate TIER 4 pages (6 pages)
9. Delete unused Controls/*.cs files
10. Final testing

---

## ⚠️ High-Risk Pages (OleDb Still Present!)

**CRITICAL:** These pages still have OleDb references and MUST be migrated:

| Page | OleDb Count | Priority |
|------|-------------|----------|
| `Pages/OrderDone.aspx.cs` | 3 | **HIGHEST** |

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial TODO created |
| 2.0 | 2025-03-27 | **Complete rewrite** - PAGE-BASED migration (not table-based) |
| 3.0 | 2026-05-13 | Revised migration plan; detailed UX step order; prep date terminology updates |
| 4.0 | 2026-07-13 | Marked `OrderDetail.aspx` complete (Save UX, line edit, mobile New Item) |
| 4.1 | 2026-07-13 | Marked `HolidayClosures` / `HolidayClosureDetail` complete (repo + manager) |
| 4.3 | 2026-07-14 | XMLtoSQL done (CRUD smoke + XmlReader fix); **Phase 2 complete** |
| 4.4 | 2026-07-14 | Reachability scan: TestPeople + orphans → retired track; excluded from `.csproj` |

---

**Questions?** Check:
- [AI_CONTEXT.md](../AI_CONTEXT.md) - Code patterns
- [PROJECT_OVERVIEW.md](../PROJECT_OVERVIEW.md) - Project context
- [TABLE_SCHEMA_REFERENCE.md](../TABLE_SCHEMA_REFERENCE.md) - Name mappings
- `Pages/RepairDetailOld.aspx` (odsRepairStatuses, odsMachineConditions, odsRepairFaults)
- `Pages/SendCoffeeCheckup.aspx` (odsContactsToSendCheckup, odsContactToBeRemindedItems)
- `Pages/SentRemindersSheet.aspx` (odsSentRemindersSummarys, odsDatesSentReminder)
- `Pages/ThisWeeksOrder.aspx` (odsOpenOrders)

**Tools/**

**P0 (legacy `Controls/*Tbl.cs` likely)**
- `Tools/SystemData.aspx` (odsItemTypes)

**P1 (ODS still present; target type to be verified during migration)**
- `Tools/MoveDeliveryDate.aspx` (odsAreaDeliveryDates)
- `Tools/SystemData.aspx` (odsSystemData)
- `Tools/SystemTools.aspx` (odsCustomerTypes)
- ~~`Tools/TestPeople.aspx` (odsPeople)~~ — **retired 2026-07-14** (not on menu; excluded from `.csproj`)

**Notes:**
- Some of the above ODS controls point directly to legacy `Controls/*Tbl.cs` classes; those pages are high-priority to refactor.
- Use `Lookups.aspx` as the reference implementation for removing ODS and rebinding manually.

### Visual Studio Find

1. **Find All References** (Ctrl+K, R) on class name
2. Check **Find Results** window
3. Document each consumer before starting

---

## ⚠️ Known Issues to Address

### Issue 1: View Models for Normalized Tables

**Problem:** Some tables were normalized (Orders → OrdersTbl + OrderLinesTbl)

**Solution:**
- Create ViewModels in `Classes\ViewModels\` folder
- Use for complex displays that need denormalized data
- Keep repositories focused on single entity

**Examples Needed:**
- `OrderViewModel` (Order + Lines + Items)
- `ContactUsageViewModel` (Usage + Items)

### Issue 2: ContactSummariesRepository Pattern

**Good News:** `ContactSummariesRepository` already exists and works!

**Pattern to Follow:**
- For complex queries, create dedicated repository
- Don't force everything into entity repository
- Example: `ContactSummariesRepository` vs `ContactsRepository`

### Issue 3: ObjectDataSource in ASPX

**Problem:** Many pages use ObjectDataSource pointing to old Table Classes

**Solution:**
```aspx
<!-- Old -->
<asp:ObjectDataSource ID="odsCustomers" runat="server"
    TypeName="TrackerSQL.Controls.CustomerTypeTbl"
    SelectMethod="GetAll" />

<!-- New -->
<asp:ObjectDataSource ID="odsContacts" runat="server"
    TypeName="TrackerDotNet.Classes.Sql.ContactTypesRepository"
    SelectMethod="GetAll" />
```

Or better: Remove ObjectDataSource, bind in code-behind.

---

## 📝 Commit Message Template

```
Refactor [TableName] to use Repository pattern

- Removed legacy Controls\[TableClass].cs
- Updated [PageName].aspx.cs to use [Repository]
- Verified POCO and Repository exist
- Tested [specific functionality]
- No more references to old Table Class

Phase: [1-5]
Task: [X.Y]
Est: [X hours], Actual: [Y hours]
```

---

## 🎓 Learning Resources

### Before Starting Phase 1

Read these sections:
1. **AI_CONTEXT.md** - Code patterns
2. **TABLE_SCHEMA_REFERENCE.md** - Verify names
3. Review existing Repository pattern in `Classes\Sql\`

### Repository Pattern Examples

Look at these working examples:
- `Classes\Sql\ContactTypesRepository.cs` - Simple lookup
- `Classes\Sql\ItemsRepository.cs` - Medium complexity
- `Classes\Sql\ContactsRepository.cs` - Complex entity
- `Classes\Sql\OrdersRepository.cs` - Normalized tables

---

## ✅ Quick Start Checklist

**Ready to begin Phase 1?**

- [ ] Read this entire TODO
- [ ] Read AI_CONTEXT.md patterns section
- [ ] Review existing Repository examples
- [ ] Create feature branch: `feature/refactor-phase1`
- [ ] Start with task 1.1 (ContactTypesTbl)
- [ ] Follow the pattern for each subsequent task

**Let's go!** 🚀

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial TODO - renamed/renamed approach |
| 2.0 | 2025-03-26 | **Complete rewrite** - Architectural modernization approach |
| 2.1 | 2025-05-12 | Updated with actual progress, added detailed steps for recurring orders |
| 3.0 | 2026-05-13 | Revised migration plan; detailed UX step order; prep date terminology updates |

---

**Questions?** Check:
- [AI_CONTEXT.md](../AI_CONTEXT.md) - Code patterns
- [TABLE_SCHEMA_REFERENCE.md](../TABLE_SCHEMA_REFERENCE.md) - Name mappings
- [CURRENT_ISSUES.md](CURRENT_ISSUES.md) - Known problems

## **FINAL RECOMMENDATION**

### 🎯 **Start with `Tools/SystemData.aspx`**

**Reasoning:**
1. **Lowest complexity** - Simple CRUD on single settings table
2. **Highest success rate** - 99% confidence of completion in 1-1.5 hours
3. **Creates reference** - Other CRUD pages can copy this pattern
4. **Builds momentum** - Quick win before tackling harder pages
5. **Zero risk** - System data changes rarely affect operations

**Estimated Time:** 1-1.5 hours  
**Difficulty:** ⭐ LOW  
**Impact:** HIGH (enables pattern reuse for 3+ pages)

---

Would you like me to:
1. ✅ CONTRIBUTING.md updated
2. ✅ MIGRATION_TODO.md section updarted 
3. **Generate complete code for `SysDataRepository.cs`**?
4. **Show line-by-line changes for `SystemData.aspx` markup and code-behind**?

All are ready whenever you want them!
