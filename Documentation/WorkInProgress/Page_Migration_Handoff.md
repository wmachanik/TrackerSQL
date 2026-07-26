# Page Migration Handoff

**Date:** 2026-06-15  
**Phase:** Page migration (active)  
**Previous phase:** Models, repositories, and managers — complete for migrated domains

---

## Where to read current status

| Document | Purpose |
|----------|---------|
| [`README.md`](../../README.md) | **Source of truth** — architecture, naming, §1.1 migration status, §11 page rules, §15 workflow, §18 AI prompt |
| [`Controls_Migration_Gap_Analysis.md`](Controls_Migration_Gap_Analysis.md) | Control-by-control inventory; refresh page call sites from here |
| This file | Quick handoff for a new chat/session |

### Code layers (current state)

```text
TrackerSQL.Models          ✅ Data-only POCOs
TrackerSQL.Repositories    ✅ SQL only (RepositoryBase<T>, named DBParameter)
TrackerSQL.Managers        ✅ Migrated — no TrackerSQL.Controls DB calls, no TrackerDb
Pages/*.aspx.cs            ⏳ NEXT — many still on legacy controls + fat code-behind
Tools/*.aspx.cs            ⏳ Same rules as Pages where legacy usage remains
```

Schema reference: **TrackerMigration** project (live SQL Server). Copies under TrackerSQL `Data/Metadata/Sql/` are legacy — do not use as source of truth (Rule #0d).

---

## Next step (in order)

**Active now:** Phase 2 complete; retired track deferred. **Next:** deepen CoffeeCheckupManager layering/naming review, or next page still missing page-tone.

Done: **`SendCoffeeCheckup`** (`page-tone-checkup` + manager façades, 2026-07-16), `SentRemindersSheet` (full shell 2026-07-15), **`Contacts` + `ContactDetails`** (2026-07-15), **`Repairs` + `RepairDetail`** (`page-tone-repairs`, 2026-07-16), **`ContactsAway` + `ContactsAwayDetail`**.  

**UI standard (required):** [`WEBFORMS_UI_STANDARDS.md`](../WEBFORMS_UI_STANDARDS.md) — page-tone shell = `SentRemindersSheet` / `Contacts` / `Repairs` / `SendCoffeeCheckup`; interaction = `HolidayClosureDetail` / `ContactDetails` / `RepairDetail`.

### 1. Gap analysis — pages only

Scan **page code-behind** (and Tools/Administration where relevant) for:

- `using TrackerSQL.Controls`
- `ObjectDataSource` with `TypeName="TrackerSQL.Controls...."`
- `TrackerDb`, `new XxxTbl()`, inline SQL
- **Embedded business logic** (validation, calculations, workflow, multi-repo orchestration, email building)

Produce a markdown report (do not edit code first) classifying each page as:

| Class | Action |
|-------|--------|
| **Thin bind swap** | Replace control with repo/manager call; page stays thin |
| **Manager extraction** | Move logic to existing or new manager; page calls one manager method |
| **New repo method** | Page/manager needs bespoke query not yet on a repository — add to repo only |

Search patterns:

```text
TrackerSQL.Controls
TrackerDb
ObjectDataSource
new \w+Tbl\(
ExecuteSQLGetDataReader
```

Known **high-priority fat pages** (expect manager extraction, not just namespace swap):

- ~~`Pages/OrderDetail.aspx.cs`~~ ✅ done 2026-07-13 (Save/Save&Return, line edit, mobile New Item)
- ~~`Tools/HolidayClosures.aspx` / `HolidayClosureDetail.aspx`~~ ✅ done 2026-07-14
- ~~`Tools/SystemTools.aspx`~~ ✅ hub cleaned 2026-07-14 (Set Client Type deferred)
- ~~`Tools/MessagesEditor.aspx`~~ ✅ verified 2026-07-14
- ~~`Tools/XMLtoSQL.aspx`~~ ✅ cleaned 2026-07-14
- `Pages/SendCoffeeCheckup.aspx.cs`
- `Pages/LogTable.aspx.cs`
- `Pages/GroupItemDetail.aspx.cs`
- `Pages/ViewMyOrder.aspx.cs`

Retired legacy pages/tools (do not migrate unless explicitly reactivated; many excluded from `.csproj` 2026-07-14 — see gap analysis §2.3–2.4):

- `Tools/TestPeople.aspx` (never on menu; Persons CRUD smoke only)
- `Pages/CustomerDetails.aspx.cs` (active customer flow uses `ContactDetails.aspx`)
- `Pages/NewOrder.aspx` / `NewOrderDetail.aspx.cs` (replaced by `OrderDetail.aspx?NewOrder=true`)
- `Tools/MergeCustomersFromQB.aspx.cs` (QuickBooks no longer used)
- Plus orphan list: `ClientList`, `OrdersEdit`, `OrderSheet`, `LogTable`, `SupportTables`, `ContactsTest`, `test/*`, etc.

### 2. Migrate pages

For each page (small batches, build often):

1. **Extract business logic** → `TrackerSQL.Managers` (create or extend manager)
2. **Move any new SQL** → `TrackerSQL.Repositories` (never in page or manager)
3. **Thin code-behind** → events, session/querystring, manager calls, bind controls only
4. **Replace ObjectDataSource** → code-behind binding to `TrackerSQL.Models` or manager view models
5. **Remove manager page-bridge overloads** once the page no longer needs legacy DTOs

### SOLID target for pages

```text
Single responsibility   Page = UI events + binding only
Open/closed             Extend managers/repos; don't grow fat pages
Dependency inversion    Page depends on managers, not TrackerDb or Controls
```

**Not acceptable after migration:**

- SQL or `TrackerDb` in `.aspx.cs`
- Business rules in button handlers / `Page_Load`
- Page coordinating multiple repositories with branching logic
- Reintroducing `TrackerSQL.Controls` in managers

**Acceptable in `.aspx.cs`:**

- Read querystring / session / viewstate
- Call manager method(s)
- Bind grids and dropdowns
- Show messages, toggle visibility

---

## Naming reminders (SQL Server schema)

```text
CustomerID      → ContactID
CustomerTypeID  → ContactTypeID
ItemTypeID      → ItemID
QuantityOrdered → QtyOrdered
RoastDate       → PrepDate
ToBeDeliveredBy → ToBeDeliveredByID
ServiceTypeID   → ItemServiceTypeID (when from ItemServiceTypesTbl)
```

---

## Build note

Full solution build may fail on pre-existing syntax errors in `Controls/CustomersTbl.cs` (unrelated to page migration). Use VS MSBuild, not `dotnet build` (WebApplication targets).

---

## Suggested starter prompt for new chat

Copy into a new Cursor chat:

```text
Read Documentation/WorkInProgress/Page_Migration_Handoff.md and README.md (§1.1, §11, §15, §18).

Managers and repositories are migrated. Next phase: gap-analyse WebForms pages (.aspx.cs), then migrate them.

Rules:
- Pages: UI events, binding, manager calls only (SOLID — thin pages).
- Business logic → TrackerSQL.Managers.
- SQL / data access → TrackerSQL.Repositories only.
- Do not add TrackerSQL.Controls or TrackerDb to managers or pages.
- Swapping Controls for repos in a page is NOT enough if the page has embedded logic — extract to managers first.

Start with a page gap analysis markdown report (no code changes). List each page, legacy usage, and whether it needs thin swap vs manager extraction vs new repo method.
```
