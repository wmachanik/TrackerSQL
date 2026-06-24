# TrackerSQL Migration README

**Created:** 2025-03-26  
**Last Updated:** 2026-06-15 (page migration phase documented)  
**Purpose:** Current migration guide for replacing legacy `TrackerSQL.Controls`, `TrackerDb`, OleDb/ObjectDataSource patterns, and old Access naming with the new SQL Server repository/model/manager/page structure.

> **Important:** This file supersedes older migration notes that referred to `Classes/Sql`, `Classes/Poco`, `Customer*`, `Client*`, `Company*`, `TrackerDb`, or ObjectDataSource-based patterns. Those older notes may remain useful as historical context, but they are **not** the current target architecture.

---

## 1. Current Target Architecture

The project is being migrated aggressively to this structure:

```text
TrackerSQL.Models          Data-only POCO models
TrackerSQL.Repositories    SQL Server data access only
TrackerSQL.Managers        Business rules, grouping, totals, display preparation
.aspx / .aspx.cs           WebForms events, state, binding and rendering only
TrackerSQL.Classes         Shared support classes only
```

### Models

Models live in:

```csharp
namespace TrackerSQL.Models
```

Models must be data-only. They must not contain SQL, database access, WebForms controls, session/request state, or business/display formatting.

### Repositories

Repositories live in:

```csharp
namespace TrackerSQL.Repositories
```

Repositories contain SQL/data access only. They should use `RepositoryBase<T>` and `TrackerSQLDb` helper methods through the base class where possible.

### Managers

Managers live in:

```csharp
namespace TrackerSQL.Managers
```

Managers contain business rules, grouping, totals, calculations, and display/view preparation. Managers must not contain SQL or instantiate database classes.

### Pages

`.aspx` and `.aspx.cs` files handle UI only:

- WebForms events
- querystring/session/viewstate reading
- calling managers/repositories
- binding controls
- rendering controls

Pages must not contain SQL or direct database access.

### 1.1 Current migration status

**As of 2026-06-15**, the **repository and manager layers are in place** for the areas already migrated:

| Layer | Status |
|-------|--------|
| `TrackerSQL.Models` | Active — data-only POCOs for migrated domains |
| `TrackerSQL.Repositories` | Active — SQL Server access; bespoke legacy queries added where managers needed them |
| `TrackerSQL.Managers` | **Migrated** — no legacy `TrackerSQL.Controls` DB calls; no `TrackerDb` / `new XxxTbl()` usage |
| `.aspx` / `.aspx.cs` | **Next phase** — many pages still on legacy controls and fat code-behind |

**Next step: migrate WebForms pages** (and related `Tools/` / `Administration/` code-behind where they still use legacy patterns).

Pages still to review typically:

- import or instantiate `TrackerSQL.Controls` types
- use `ObjectDataSource` with `TypeName="TrackerSQL.Controls...."`
- call `TrackerDb`, `new XxxTbl()`, or embed SQL in event handlers
- duplicate or embed business logic that already belongs in a manager (or that should be extracted into one)

See also: `Documentation/WorkInProgress/Controls_Migration_Gap_Analysis.md` for control-by-control inventory (refresh after manager work as needed).

**Start here for a new session:** `Documentation/WorkInProgress/Page_Migration_Handoff.md`

Managers may still expose **page bridge overloads** or legacy-shaped DTOs (e.g. grid types from old controls) so existing pages keep compiling — remove those bridges when the corresponding page is migrated to `TrackerSQL.Models`.

---

## 2. Old Namespaces Are Deprecated

Do not create new migrated files in these namespaces:

```csharp
TrackerSQL.Controls
TrackerSQL.Classes.Poco
TrackerSQL.Classes.Sql
TrackerSQL.Classes.POCO
TrackerSQL.Classes.SQL
```

Older notes may mention files such as:

```text
Classes/Sql/SomeRepository.cs
Classes/Poco/SomeModel.cs
```

Those paths are historical and should now be interpreted as:

```text
Repositories/SomeRepository.cs
Models/SomeModel.cs
```

---

## 3. Current Naming Standard

Active migrated code must use the new SQL Server naming standard.

### Contact Naming

```text
Customer              -> Contact
Client                -> Contact
Company               -> Contact
CustomerID            -> ContactID
ClientID              -> ContactID
CompanyName           -> ContactName
CoName                -> ContactName
ContactCompany        -> ContactName
CustomerTypeID        -> ContactTypeID
CustomerTracked...    -> ContactTracked...
```

### Item and Service Naming

```text
ItemType              -> Item
ItemTypeID            -> ItemID
ItemTypeDesc          -> ItemDesc or ItemName, depending on actual schema
ServiceTypeID         -> ItemServiceTypeID, when it references ItemServiceTypesTbl
ItemServiceType       -> ItemServiceTypeName, when it is the name/description field
ItemPrepType          -> ItemPrepTypeDesc, where that is the actual SQL column
QuantityOrdered       -> QtyOrdered
Quantity              -> Qty, where appropriate
TotalsQty             -> TotalQty
```

### Order and Delivery Naming

```text
RoastDate             -> PrepDate
ReqDate               -> RequiredByDate, where referring to the actual order required-by date
ToBeDeliveredBy       -> ToBeDeliveredByID
```

### Area Naming

Use the actual migrated SQL/project-standard column names. Do not add legacy aliases simply to satisfy old pages.

If the database column is `AreaName`, use `AreaName` in models and bindings. Do not alias it back to old or ambiguous names unless there is a temporary, documented schema mismatch.

---

## 4. Database/Model Alignment Rule

The preferred final state is:

```text
SQL column name == Model property name == Repository result column name
```

Do not add aliases just to keep old pages working.

Temporary aliases are acceptable only when the database has not yet been renamed and the mismatch is explicitly temporary.

Example temporary alias:

```csharp
protected override string CoreColumns =>
    "PersonID, Person AS PersonName, Abbreviation, Enabled";
```

Preferred final state after schema/model alignment:

```csharp
protected override string CoreColumns =>
    "PersonID, PersonName, Abbreviation, Enabled";
```

---

## 5. Repository Rules

### Thin CRUD/Lookup Repositories

If a repository only performs generic CRUD and the model matches the database columns, reduce it to a thin class.

Example:

```csharp
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ItemUnitsRepository : RepositoryBase<ItemUnit>
    {
        protected override string TableName => "ItemUnitsTbl";
        protected override string KeyColumn => "ItemUnitID";

        protected override string CoreColumns =>
            "ItemUnitID, ItemUnitDesc";

        protected override string LookupColumns =>
            "ItemUnitID, ItemUnitDesc";
    }
}
```

Do not duplicate generic methods already provided by `RepositoryBase<T>`:

```text
GetById
GetAll
Insert
Update
Delete
```

### When to Add Bespoke Repository Methods

Add custom repository methods only for real table-specific logic, such as:

- filtered lookups
- methods from old controls that used a business key
- joins/projections
- aggregate database queries
- soft-delete or non-standard insert/update behaviour
- required legacy behaviour that is still used by managers/pages

Example: old control method:

```csharp
GetAllByCustomerTypeID(int customerTypeId)
```

Migrated repository method:

```csharp
GetAllByContactTypeId(int contactTypeId)
```

This type of method should be preserved and migrated, not deleted.

---

## 6. Legacy Controls Replacement Rule

Old `TrackerSQL.Controls` classes are legacy. They often contain a mixture of:

- generated CRUD wrappers
- ObjectDataSource methods
- old Access/OleDb data access
- real bespoke queries
- nested DTO classes

When reviewing old controls, classify every public method:

```text
Generic CRUD                 -> do not migrate if RepositoryBase<T> handles it
Lookup/list only             -> thin repository or RepositoryBase lookup method
Filtered query               -> bespoke repository method
Join/projection query         -> bespoke repository method or summary model
Grouping/totals/calculation   -> manager
Display formatting            -> manager or page binding logic
ObjectDataSource wrapper      -> remove and replace with explicit binding
```

Do not assume a thin repository is complete until old controls have been checked for bespoke methods.

---

## 7. Example: ContactTrackedServiceItems Bespoke Method

Legacy decompiled control:

```text
TrackerSQL.Controls.CustomerTrackedServiceItems
CustomerTrackedServiceItemsTbl
CustomerTypeID
ServiceTypeID
GetAllByCustomerTypeID(int pCustomerTypeID)
```

Migrated naming:

```text
ContactTrackedServiceItemsRepository
ContactTrackedServiceItemsTbl
ContactTypeID
ItemServiceTypeID
GetAllByContactTypeId(int contactTypeId)
```

Correct repository pattern:

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactTrackedServiceItemsRepository : RepositoryBase<ContactTrackedServiceItem>
    {
        protected override string TableName => "ContactTrackedServiceItemsTbl";
        protected override string KeyColumn => "ContactTrackedServiceItemsID";

        protected override string CoreColumns =>
            "ContactTrackedServiceItemsID, ContactTypeID, ItemServiceTypeID, Notes";

        public List<ContactTrackedServiceItem> GetAllByContactTypeId(
            int contactTypeId,
            string sortBy = "ItemServiceTypeID")
        {
            string sql = $@"
                SELECT {CoreColumns}
                FROM {TableName}
                WHERE ContactTypeID = @ContactTypeID
                ORDER BY {MapSortColumnOrDefault(sortBy)}";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@ContactTypeID",
                    DataValue = contactTypeId,
                    DataDbType = DbType.Int32
                }
            };

            var list = new List<ContactTrackedServiceItem>();

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(DbMapper.Map<ContactTrackedServiceItem>(rdr));
                }
            }

            return list;
        }

        private static string MapSortColumnOrDefault(string sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return "ItemServiceTypeID";
            }

            string trimmed = sortBy.Trim();

            if (trimmed.Equals("ContactTrackedServiceItemsID", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("ContactTrackedServiceItemsID ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "ContactTrackedServiceItemsID ASC";
            }

            if (trimmed.Equals("ContactTrackedServiceItemsID DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "ContactTrackedServiceItemsID DESC";
            }

            if (trimmed.Equals("ContactTypeID", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("ContactTypeID ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "ContactTypeID ASC";
            }

            if (trimmed.Equals("ContactTypeID DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "ContactTypeID DESC";
            }

            if (trimmed.Equals("ItemServiceTypeID", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("ItemServiceTypeID ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "ItemServiceTypeID ASC";
            }

            if (trimmed.Equals("ItemServiceTypeID DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "ItemServiceTypeID DESC";
            }

            return "ItemServiceTypeID";
        }
    }
}
```

---

## 8. SQL/Data Access Rules

Do not use old Access/OleDb patterns:

```csharp
TrackerDb trackerDb = new TrackerDb();
trackerDb.AddParams(...);
trackerDb.AddWhereParams(...);
trackerDb.ExecuteSQLGetDataReader(...);
trackerDb.ExecuteNonQuerySQL(...);
trackerDb.Close();
```

Do not use positional parameters:

```sql
WHERE ContactTypeID = ?
```

Use named SQL Server parameters:

```sql
WHERE ContactTypeID = @ContactTypeID
```

Use `DBParameter`:

```csharp
var parameters = new List<DBParameter>
{
    new DBParameter
    {
        ParamName = "@ContactTypeID",
        DataValue = contactTypeId,
        DataDbType = DbType.Int32
    }
};
```

Use repository base helpers:

```csharp
using (var rdr = ExecReader(sql, parameters))
{
    while (rdr != null && rdr.Read())
    {
        list.Add(DbMapper.Map<MyModel>(rdr));
    }
}
```

---

## 9. Sort and Filter Rules

Never concatenate arbitrary user-provided sort or filter strings into SQL.

Incorrect:

```csharp
sql += " ORDER BY " + sortBy;
```

Correct:

```csharp
sql += " ORDER BY " + MapSortColumnOrDefault(sortBy);
```

Sort mappers must whitelist allowed columns and directions. Unknown values must fall back to a safe default.

Raw `whereFilter` strings are not allowed. Replace them with explicit repository methods such as:

```csharp
GetAllByContactTypeId(int contactTypeId)
GetByContactId(int contactId)
GetActiveForDate(DateTime date)
SearchByContactName(string contactName)
```

---

## 10. Manager Migration Rules

> **Status (2026-06-15):** Manager migration is complete for the current scope. Do not add new `TrackerSQL.Controls` usage in managers. New business logic belongs in managers and calls repositories only.

Managers should replace old control calls with repositories or other managers.

Old pattern:

```csharp
var ctl = new TrackerSQL.Controls.CustomerTrackedServiceItems();
var rows = ctl.GetAllByCustomerTypeID(customerTypeId);
```

New pattern:

```csharp
var repo = new ContactTrackedServiceItemsRepository();
var rows = repo.GetAllByContactTypeId(contactTypeId);
```

Managers must not contain SQL. If a manager needs data, add or use a repository method.

Managers may transform repository models into display/view models.

---

## 11. WebForms Page Migration Rules

> **Status (2026-06-15):** This is the **active migration phase**. Repositories and managers are ready; remaining legacy usage is concentrated in page code-behind, ObjectDataSource markup, and some `Tools/` pages.

### Target: thin pages only

After migration, a page code-behind should only:

- handle WebForms events and postbacks
- read querystring, session, and viewstate
- call **one manager** (or a small number of focused manager methods)
- bind controls to manager-prepared or model data
- display messages and control visibility

### Known legacy anti-patterns (fix during migration, not after)

Many `.aspx.cs` files pre-date the repository/manager split. **Swapping `new CustomersTbl()` for `new ContactsRepository()` in the page is not sufficient.** During page migration you must also:

1. **Extract embedded business logic** — validation, calculations, workflow (status changes, order completion, checkup prep/send), email body building, and multi-table updates belong in `TrackerSQL.Managers`, not in `Page_Load`, button handlers, or grid events.
2. **Remove SQL and direct data access from pages** — no `TrackerDb`, no inline SQL, no `ExecuteSQLGetDataReader` in code-behind. SQL belongs only in repositories.
3. **Avoid orchestrating repositories in the page** — if code-behind coordinates several repositories with branching logic, move that orchestration into a manager (or extend an existing one).
4. **Replace ObjectDataSource** — stop binding grids and dropdowns to legacy control types; bind from code-behind using models or manager-prepared view data.
5. **Do not grow new fat pages** — if a handler needs more than binding and a manager call, the handler is too fat.

**High-priority pages** (heavy legacy logic — expect manager extraction, not just a control swap):

| Page | Typical issues |
|------|----------------|
| `Pages/CustomerDetails.aspx.cs` | Legacy page (bookmark-only); active customer flow uses `ContactDetails.aspx` |
| `Pages/NewOrderDetail.aspx.cs` | Legacy/orphan page; replaced by `OrderDetail.aspx?NewOrder=true` |
| `Pages/OrderDetail.aspx.cs` | Order/line logic in code-behind |
| `Pages/SendCoffeeCheckup.aspx.cs` | Checkup UI + legacy DTOs; manager bridges in place |
| `Pages/LogTable.aspx.cs` | Direct DB / control usage |
| `Pages/GroupItemDetail.aspx.cs` | Item/group logic in page |
| `Pages/ViewMyOrder.aspx.cs` | Mixed access patterns |
| `Tools/MergeCustomersFromQB.aspx.cs` | Legacy QB import tool (retired; QB no longer used) |

Other pages under `Pages/` and `Tools/` still reference `TrackerSQL.Controls` or `TrackerDb` and should be migrated using the same rules, unless explicitly retired legacy pages.

Remove old ObjectDataSource usage where it points at legacy controls.

Old pattern:

```aspx
<asp:ObjectDataSource
    ID="odsTrackedServiceItems"
    TypeName="TrackerSQL.Controls.CustomerTrackedServiceItems"
    SelectMethod="GetAllByCustomerTypeID" />
```

New pattern in code-behind:

```csharp
private void BindTrackedServiceItems()
{
    int contactTypeId = GetContactTypeIdFromPageState();

    var repo = new ContactTrackedServiceItemsRepository();
    var data = repo.GetAllByContactTypeId(contactTypeId);

    gvTrackedServiceItems.DataSource = data;
    gvTrackedServiceItems.DataBind();
}
```

Pages must not contain SQL.

---

## 12. Corrected Notes From Older README Content

### Older note: `Classes/Sql/PreperationSummaryRepository.cs`

Correct current path/namespace should be:

```text
Repositories/PreperationSummaryRepository.cs
namespace TrackerSQL.Repositories
```

### Older note: `Classes/Poco/PreperationSummaryItem.cs`

Correct current path/namespace should be:

```text
Models/PreperationSummaryItem.cs
namespace TrackerSQL.Models
```

### Older note: `CustomerConstants.SundryCustomerID`

Prefer current naming:

```text
ContactConstants.SundryContactID
```

If the old constant still exists, treat that as a migration item. Do not introduce new references to `CustomerConstants` in migrated code.

### Older note: `ServiceTypeConstants.CoffeeStr`

Use only if that constant still exists and is intentionally retained. Where the underlying meaning is item service type, prefer the current naming standard around `ItemServiceType` constants.

### Older note: SysDataTbl actual database names

Older notes listed:

```text
DoReoccuringOrders
GroupItemTypeID
InternalCustomerIds
```

Current migration direction should be to align database/model/repository names with the new standard where possible:

```text
DoRecurringOrders
GroupItemServiceTypeID
InternalContactIDs
```

If the live database still contains old names, treat that as a schema migration issue or a temporary alias/override, not as the target standard.

---

## 13. RoastDate to PrepDate Migration

This remains valid and important.

All active migrated code should use:

```text
PrepDate
```

not:

```text
RoastDate
```

Update:

- SQL column references
- model properties
- repository result shapes
- GridView `DataField`, `SortExpression`, and `Bind(...)`
- code-behind strings
- variable names where practical
- log messages
- UI labels from `Roast Date` to `Prep Date`

Do not create aliases such as:

```sql
PrepDate AS RoastDate
```

Update consumers instead.

---

## 14. Orders Normalisation Rules

The SQL Server schema uses separate order header and line tables.

### OrdersTbl header fields

Use:

```text
OrderID
ContactID
OrderDate
PrepDate
RequiredByDate
ToBeDeliveredByID
Confirmed
Done
Packed
Notes
PurchaseOrder
InvoiceDone
```

Do not use:

```text
CustomerID
RoastDate
ToBeDeliveredBy
```

### OrderLinesTbl fields

Use:

```text
OrderLineID
OrderID
ItemID
QtyOrdered
PrepTypeID
PackagingID
```

Do not use:

```text
ItemTypeID
QuantityOrdered
```

---

## 15. Current Gap-Analysis Workflow

At this stage, models, repositories, and managers for the migrated domains are largely complete. **The primary remaining work is page code-behind** (and extracting any business logic still embedded there into managers).

Use this workflow:

1. Scan old `TrackerSQL.Controls` classes (or use `Documentation/WorkInProgress/Controls_Migration_Gap_Analysis.md`).
2. Identify public methods still called from **pages** (not managers — manager call sites should already be clear).
3. Confirm equivalent repository/manager methods exist; add only missing bespoke repository methods if a page truly needs data not yet exposed.
4. **Migrate the page:** remove `TrackerSQL.Controls`, ObjectDataSource, and `TrackerDb` usage.
5. **Refactor fat code-behind:** move business logic and multi-step orchestration from the page into the appropriate manager.
6. Bind UI to `TrackerSQL.Models` (or manager view models); remove manager page-bridge overloads once unused.
7. Build and fix compile errors by preserving the architecture, not by restoring old patterns.

### Cursor/GitHub Copilot instruction (page phase)

Use this instruction when asking an AI tool to help with **page migration**:

```text
Manager and repository migration is largely complete. Focus on WebForms pages (.aspx / .aspx.cs).

Task:
Migrate page code-behind off TrackerSQL.Controls, TrackerDb, and ObjectDataSource.

Rules:
- Do not add SQL or TrackerDb to pages.
- Do not put business logic in code-behind — extract to TrackerSQL.Managers and call repositories from there.
- Do not reintroduce TrackerSQL.Controls in managers.
- Pages: events, state, binding, manager calls only.
- Use TrackerSQL.Models types for binding where possible.
- Replace Customer/Client naming with Contact naming in UI-facing code.
- Use named DBParameter in repositories only; never in pages.

If a page handler is doing validation, calculation, or multi-repository coordination, create or extend a manager first, then thin the page to call it.
```

### Cursor/GitHub Copilot instruction (gap analysis — still valid for pages)

Use this instruction when asking an AI tool to help with **gap analysis**:

```text
This is not a greenfield conversion.
Models already exist in TrackerSQL.Models.
Repositories already exist in TrackerSQL.Repositories.
Managers are migrated — do not add legacy Controls usage to managers.
Do not recreate models.
Do not bulk rewrite repositories.
Do not add duplicate generic CRUD methods.
Only add missing bespoke legacy behaviour that is still called from pages or still required.
Scan old TrackerSQL.Controls classes and produce a gap analysis first (page call sites).
```

---

## 16. Search Checklist

Search for old controls and data access:

```text
TrackerSQL.Controls
TrackerDb
AddParams
AddWhereParams
ExecuteSQLGetDataReader
ExecuteNonQuerySQL
ObjectDataSource
DataSourceID=
WHERE ... ?
```

Search for old naming:

```text
CustomerID
CustomerTypeID
CustomerTracked
CustomerTrackedServiceItemsTbl
CompanyName
ClientID
CoName
ContactCompany
ItemTypeID
ServiceTypeID
QuantityOrdered
RoastDate
ToBeDeliveredBy
InternalCustomerIds
```

Search for generated placeholders:

```text
TODO: Fill in column names
TODO: Add parameter names
SELECT * FROM
string sql = `$"
```

Each hit should be classified as:

```text
already migrated / historical note / active code requiring migration
```

---

## 17. Build Checklist

After each batch:

1. Build the solution.
2. Fix missing method errors by adding migrated bespoke repository methods only where needed.
3. Fix property errors by aligning model/page/manager property names with SQL-standard names.
4. Do not add legacy aliases just to fix page bindings.
5. Do not reintroduce `TrackerSQL.Controls`.
6. Do not reintroduce `TrackerDb`.
7. Commit small batches.

Suggested commit sequence (page phase):

```text
migration: thin CustomerDetails page — move logic to CustomerManager
migration: replace ObjectDataSource on OrderDetail page
migration: remove SendCoffeeCheckup legacy DTO bridge
migration: bind Repairs grid via RepairManager only
```

---

## 18. Standard Prompt For Future Work

```text
Use the TrackerSQL Migration README as the source of truth.

Current project state (2026-06-15):
- Models exist in TrackerSQL.Models.
- Repositories exist in TrackerSQL.Repositories.
- Managers are migrated — no legacy Controls DB usage in TrackerSQL.Managers.
- NEXT: WebForms pages (.aspx.cs) and some Tools/Administration code-behind.
- Many pages still embed business logic and direct data access — extract to managers during page migration.

Task (page phase):
Migrate page code-behind off TrackerSQL.Controls, TrackerDb, and ObjectDataSource.
Extract embedded business logic from pages into managers.

Rules:
- Do not recreate models.
- Do not bulk rewrite repositories unless a page exposes a missing bespoke query.
- Do not add duplicate GetById/GetAll/Insert/Update/Delete methods.
- Do not add SQL, TrackerDb, or business rules to .aspx.cs files.
- Do not reintroduce TrackerSQL.Controls in managers.
- Replace Customer/Client/Company naming with Contact naming.
- Replace CustomerTypeID with ContactTypeID.
- Replace ServiceTypeID with ItemServiceTypeID when it references ItemServiceTypesTbl.
- Replace ItemTypeID with ItemID.
- Replace QuantityOrdered with QtyOrdered.
- Replace RoastDate with PrepDate.
- Replace ToBeDeliveredBy with ToBeDeliveredByID.
- Use named DBParameter parameters, never `?` placeholders (repositories only).
- Use RepositoryBase<T> helpers such as ExecReader, ExecNonQuery, and ExecuteScalar<T>.
- SQL belongs only in repositories.
- Managers contain business/display logic only.
- Pages contain WebForms binding/event/state and manager calls only.

For gap analysis on a specific page, list Controls/TrackerDb usage and classify: bind-only swap vs manager extraction required.
```

---

## 19. Final Principle

Do not ask:

```text
How do I make the old code compile?
```

Ask:

```text
What responsibility does this code have, and where should that responsibility live now?
```

Then apply the mapping:

```text
SQL query                 -> Repository
Reader-to-object mapping  -> Repository
Business/display rules    -> Manager
Totals/grouping           -> Manager
WebForms rendering        -> .aspx.cs
Data-only properties      -> Models
```
