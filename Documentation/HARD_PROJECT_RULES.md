# HARD PROJECT RULES - MANDATORY COMPLIANCE

**Date:** 2025-03-26  
**Last Updated:** 2026-07-22  
**Status:** ACTIVE - THESE ARE NON-NEGOTIABLE RULES  
**Applies To:** ALL development on this project

---

## Rule #0b: WEBFORMS UI SHELL (Status + UpdatePanel + Page Tone)

**Full standard:** [`WEBFORMS_UI_STANDARDS.md`](WEBFORMS_UI_STANDARDS.md)  
**References:** `Tools/HolidayClosureDetail.aspx` (UpdatePanel / buttons) · `Pages/SentRemindersSheet.aspx` (page-tone shell) · `Pages/ContactDetails.aspx` + `Scripts/unsavedChanges.js` (dirty guard)

### REQUIRED

1. **Status messages at the bottom** of the main form (after action buttons), using `status-message` / `status-success` / `status-error` / `status-info`.
2. **UpdatePanel + UpdateProgress** as the default for interactive pages (async Save; full postback for Save & Return / Back / redirecting Delete).
3. **Back** is an `<asp:Button>`, not a HyperLink.
4. Prefer **Save** and **Save & Return** on detail forms; default return URL is the owning list page.
5. **Page tone panel:** wrap title + toolbar + content in `page-tone-panel` + a `page-tone-*` modifier matching the home dashboard card (`Default.aspx` `home-tone-*`). **One** icon in the header — no duplicate `<h1>` above the panel. CSS: `Styles/Site.css`.
6. **Site.css cache-bust:** whenever `Styles/Site.css` changes, bump `Rev: YYYYMMDD-n` at the top of that file **and** the matching `?v=YYYYMMDD-n` on the `Site.Master` stylesheet link (same-day edits increment `n`). Prevents browsers keeping old CSS.
7. **Return navigation for multi-entry details** (e.g. `ContactDetails`): **Save & Return** / **Back** go to the **referring page** (captured from `UrlReferrer` or optional `?ReturnUrl=`), not always the Contacts list. Fallback when no referrer: owning list (`~/Pages/Contacts.aspx`). Same-site URLs only.
8. **Unsaved-changes:** use shared `Scripts/unsavedChanges.js` (`TrackerUnsaved.init(...)`); do not duplicate `beforeunload` / dirty-tracking blobs in page headers.

### RETROFIT

Apply when creating or editing a page. Bring non-compliant pages into line as they are touched.

---

## Rule #0c: RECURR — NEVER LEGACY "REOCCUR"

**Canonical naming:** use **Recurring** / **HadRecurringItems** / **NextPreparationDate**.

### PROHIBITED in new or touched application code (repos, models, managers, pages)

❌ Do **not** introduce or keep these Access-era / interim forms:

| Forbidden (legacy / interim) | Use instead |
|------------------------------|-------------|
| `HadReoccurItems` | `HadRecurringItems` |
| `HadRecurrItems` | `HadRecurringItems` |
| `Reoccur` / `Reoccuring` / `Reoccurance` in **new** identifiers | `Recurring` / `Recurrence` (match existing DB table names when they still say `RecurranceTypesTbl`) |
| `NextPreperationDate` | `NextPreparationDate` |
| `Preperation` | `Preparation` |

Legacy `Controls/*Tbl.cs` may still contain old names until retired — **do not copy those spellings** into repositories, models, managers, or pages.

See also: [`Docs/MIGRATION_NAMING_ALIGNMENT.md`](../Docs/MIGRATION_NAMING_ALIGNMENT.md)

---

## Rule #0d: DATABASE MIGRATIONS LIVE IN TrackerMigration

SQL schema creation, data migrate scripts, alter/rename scripts, and verification SQL are **not** owned by TrackerSQL.

### REQUIRED

1. Treat **TrackerMigration** as the only source of truth for CreateTables / Migrate_* / Alter_* / Verify_*.
2. Paths under TrackerSQL such as `Data/Metadata/Sql/`, `Migrations/`, root `CleanMigrationTables.sql`, etc. are **legacy leftovers** — do not run them, do not “fix” them as active migration sources, and do not add new migration scripts here.
3. Application code (repos/models) must match the **live SQL Server** schema as defined by TrackerMigration — if a rename is needed, change it in TrackerMigration (and the DB), then update TrackerSQL app code.

### PROHIBITED

❌ Adding new `Migrate_*`, `Alter_*`, or `CreateTables_*` scripts under TrackerSQL  
❌ Using TrackerSQL’s `Data/Metadata/Sql/*.sql` as the current schema reference when it conflicts with TrackerMigration / the live DB

---

## Rule #0e: OPTIONAL FKs — USE `FkOrDbNull` (Access `0` → SQL `NULL`)

Access often stored **`0`** on foreign-key columns to mean “none”. SQL Server foreign keys **reject `0`** unless a matching parent row exists.

### REQUIRED

1. On every repository **INSERT/UPDATE** of an optional/nullable FK (`EquipTypeID`, `AreaID`, `ItemPackagingID`, agent IDs, etc.), pass values through **`DbParamHelpers.FkOrDbNull(...)`** (also exposed as `RepositoryBase<T>.FkOrDbNull` for repos that inherit the base).
2. Prefer the shared helpers — do **not** copy private `FkOrDbNull` methods into each repository.
3. Overloads: `FkOrDbNull(int?)` and `FkOrDbNull(int)` — both treat `null` / `≤ 0` as `DBNull.Value`.

```csharp
using static TrackerSQL.Classes.DbParamHelpers;
// or from a RepositoryBase-derived repo: FkOrDbNull(...)

new DBParameter {
    ParamName = "@EquipTypeID",
    DataValue = FkOrDbNull(contact.EquipTypeID),
    DataDbType = DbType.Int32
};
```
### NOT for

- Required FKs that must always reference a real parent (e.g. `ContactID` on a child row that cannot exist without a contact)
- Non-FK fields (`null`/`0` meaning something else — use normal null coalescing)

### Symptom

`INSERT/UPDATE ... conflicted with the FOREIGN KEY constraint "FK_..."` when the app value is `0` or an Access-era empty lookup.

---

## Rule #1: NO MICROSOFT ACCESS DATABASE

### The Problem

This project is **migrating FROM Microsoft Access TO SQL Server**. Using the Access database defeats the entire purpose of the migration.

### What is PROHIBITED

❌ **ABSOLUTELY FORBIDDEN:**

1. **Connection Strings:**
   - ANY reference to `TrackerDataOleDb`
   - ANY connection string pointing to `.mdb` or `.accdb` files
   - ANY code using Access database paths

2. **OleDb Classes:**
   - `OleDbConnection`
   - `OleDbCommand`
   - `OleDbDataAdapter`
   - `OleDbDataReader`
   - ANY `System.Data.OleDb` namespace usage

3. **Legacy Code:**
   - ANY code that queries Access database
   - ANY code that writes to Access database
   - ANY reference to the physical Access file

### What is REQUIRED

✅ **MUST USE:**

1. **SQL Server Only:**
   - Connection string: `TrackerDataSQL`
   - Database: SQL Server / SQL Server Express
   - All queries against SQL Server

2. **SqlClient Classes:**
   - `SqlConnection`
   - `SqlCommand`
   - `SqlDataAdapter`
   - `SqlDataReader`
   - `System.Data.SqlClient` namespace

3. **Repository Pattern:**
   - Use `TrackerSQLDb` class (wraps SqlConnection)
   - Use Repository classes for all data access
   - NO direct SQL in UI code

### Why This Rule Exists

- Access is 32-bit only (deployment problems)
- Access has scalability limitations
- Access file corruption issues
- Access is not cloud-deployable
- **We spent months migrating - don't undo it!**

### Consequences of Violation

If you use Access database:
- ❌ Migration is reversed
- ❌ All SQL Server work is wasted
- ❌ Project fails its primary objective
- ❌ Code must be rewritten

**DO NOT USE ACCESS DATABASE. PERIOD.**

---

## Rule #2: NO SqlDataSource (Repository Pattern ONLY)

### The Problem

`SqlDataSource` controls cause:
- Code duplication (same SQL in multiple places)
- Maintenance nightmares (SQL scattered in markup)
- Type unsafety (magic strings, runtime errors only)
- Untestable code
- Violation of separation of concerns

### What is PROHIBITED

❌ **ABSOLUTELY FORBIDDEN:**

1. **SqlDataSource Controls:**
   ```aspx
   <!-- NEVER DO THIS -->
   <asp:SqlDataSource ID="sdsWhatever" runat="server" 
       ConnectionString="..." 
       SelectCommand="SELECT ..." />
   ```
   - **Exception:** `sdsUserNames` is allowed (ASP.NET membership views in TrackerDataSQL only)

2. **Legacy ObjectDataSource:**
   ```aspx
   <!-- NEVER DO THIS -->
   <asp:ObjectDataSource ID="odsWhatever" runat="server" 
       TypeName="TrackerSQL.Controls.SomeTableTbl" 
       SelectMethod="GetAll" />
   ```
   - Legacy `*Tbl.cs` table classes are deprecated
   - Use Repository pattern instead

3. **ALL ObjectDataSource Controls (long-term rule):**

   `ObjectDataSource` controls (even when pointing at repository classes) are **not allowed** for application data.
   They cause runtime-only failures during refactors (property renames, method signature changes) and hide data flow.
   
   ✅ Required instead: **manual repository binding in code-behind** (`GridView.DataSource = ...; DataBind();`).

3. **DataSourceID in Controls:**
   ```aspx
   <!-- NEVER DO THIS -->
   <asp:GridView ID="gvWhatever" runat="server" 
       DataSourceID="sdsWhatever" />
   ```

### What is REQUIRED

✅ **MUST USE:**

1. **Repository Pattern:**
   ```csharp
   // In code-behind (Page_Load)
   protected void Page_Load(object sender, EventArgs e)
   {
       if (!IsPostBack)
       {
           BindGrid();
       }
   }
   
   private void BindGrid()
   {
       var repo = new ItemsRepository();
       var items = repo.GetAll("SortOrder");
       gvItems.DataSource = items;
       gvItems.DataBind();
   }
   ```

2. **Repository Classes:**
   - Location: `Classes/Sql/*Repository.cs`
   - Base class: `RepositoryBase<T>`
   - Methods: `GetAll()`, `GetById()`, `Insert()`, `Update()`, `Delete()`

3. **POCO Models:**
   - Location: `Classes/Poco/*.cs`
   - Plain C# classes with properties
   - NO business logic in POCOs

4. **Manual Data Binding:**
   ```csharp
   // Paging
   protected void gvItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
   {
       gvItems.PageIndex = e.NewPageIndex;
       BindGrid();
   }
   
   // Sorting
   protected void gvItems_Sorting(object sender, GridViewSortEventArgs e)
   {
       // Apply sort
       BindGrid();
   }
   
   // Dropdowns in RowDataBound
   protected void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
   {
       if (e.Row.RowType == DataControlRowType.DataRow)
       {
           var ddl = e.Row.FindControl("ddlServiceType") as DropDownList;
           if (ddl != null)
           {
               var repo = new ServiceTypesRepository();
               ddl.DataSource = repo.GetAll("ServiceType");
               ddl.DataTextField = "ServiceTypeName";
               ddl.DataValueField = "ServiceTypeId";
               ddl.DataBind();
           }
       }
   }
   ```

### Why This Rule Exists

**Benefits of Repository Pattern:**
- ✅ DRY - Data access logic in ONE place
- ✅ Maintainable - Changes in one location
- ✅ Testable - Easy to mock repositories
- ✅ Type safe - Compile-time checking
- ✅ Clean architecture - Proper separation of concerns
- ✅ Reusable - Repositories used across pages

**Problems with SqlDataSource:**
- ❌ SQL duplicated in markup across pages
- ❌ Hard to find/update all SQL instances
- ❌ Runtime errors only (no compile-time checking)
- ❌ Can't mock or unit test
- ❌ Mixes data access with UI

### Consequences of Violation

If you use SqlDataSource:
- ❌ Code review will reject it
- ❌ Creates technical debt
- ❌ Violates project architecture
- ❌ Code must be refactored

**DO NOT USE SqlDataSource. USE REPOSITORY PATTERN.**

---

## Rule #3: USE SystemConstants - NEVER Hardcode Magic Numbers/Values

### The Problem

Magic numbers and hardcoded values scattered throughout code are:
- Hard to maintain (where do I change them all?)
- Prone to errors (easy to use wrong value)
- Prevent future preferences database implementation
- Make code intent unclear (what does `== 2` mean?)
- Create duplicate constants across codebase

### What is PROHIBITED

❌ **ABSOLUTELY FORBIDDEN:**

1. **Hardcoded Connection Strings:**
   - NO hardcoded connection strings in code
   - Use `ConfigurationManager.ConnectionStrings`

2. **Magic Numbers:**
   - NO raw numbers in queries, code, or markup
   - Use named constants or enums

3. **Hardcoded File Paths:**
   - NO absolute or relative file paths
   - Use application settings or constants

4. **Inline SQL Queries:**
   - NO SQL queries directly in code or markup
   - Use stored procedures or repository methods

5. **Deprecated Literal Controls:**
   - NO `<asp:Literal>` or `<asp:Label>` for SQL data
   - Use strongly-typed controls or data binding

### What is REQUIRED

✅ **MUST USE SystemConstants:**

1. **SystemConstants Class:**
   - Location: `App_Code/SystemConstants.cs`
   - All constants in ONE place
   - Use `public static` members

2. **Configuration Settings:**
   - Use `web.config` or `appsettings.json` for settings
   - Access via `ConfigurationManager.AppSettings`

3. **Named Parameters in Queries:**
   - Use parameterized queries in repository methods
   - NO inline variable concatenation

4. **Entity Framework or Dapper:**
   - Use ORM for data access (where possible)
   - NO manual SQL string building

5. **Prefer Strongly-Typed Models:**
   - Use `IQueryable<T>` or `IEnumerable<T>` for data collections
   - NO `DataSet`, `DataTable`, or weakly-typed collections

### Why This Rule Exists

**Benefits of Using Constants and Configurations:**
- ✅ Centralized management of values
- ✅ Easier to change and maintain
- ✅ Reduces magic strings/numbers in code
- ✅ Clearer code intent and logic
- ✅ Supports future preferences/settings implementation

**Problems with Hardcoding:**
- ❌ Leads to technical debt
- ❌ Increases maintenance effort
- ❌ Causes confusion and errors
- ❌ Hard to track all usages

### Consequences of Violation

If you hardcode values:
- ❌ Code review will reject it
- ❌ Creates technical debt
- ❌ Violates project architecture
- ❌ Code must be refactored

**USE SystemConstants AND Configuration Settings. AVOID HARD-CODING.**

---

## ObjectDataSource Controls

❌ **Not allowed.**

Even when an `ObjectDataSource` points to a repository class, it still:
- couples markup to method signatures
- fails at runtime on property/method renames
- makes debugging and maintenance harder

✅ Use manual binding in code-behind with repositories.

## How to Check for Violations

### Search for Access Database Usage

```bash
# Search for OleDb references
grep -r "OleDb" --include="*.cs" --include="*.aspx"

# Search for Access connection string
grep -r "TrackerDataOleDb" --include="*.cs" --include="*.aspx" --include="*.config"

# Search for .mdb or .accdb file references
grep -r "\.mdb\|\.accdb" --include="*.cs" --include="*.aspx" --include="*.config"
```

### Search for SqlDataSource Violations

```bash
# Search for SqlDataSource controls
grep -r "SqlDataSource ID=" --include="*.aspx"

# Search for legacy ObjectDataSource
grep -r "TypeName=\"TrackerSQL.Controls" --include="*.aspx"

# Search for DataSourceID attributes
grep -r "DataSourceID=\"sds" --include="*.aspx"
```

---

## Exceptions to These Rules

### Rule #1 (Access Database) - NO EXCEPTIONS

There are **ZERO exceptions** to the Access database rule. SQL Server must be used for everything.

### Rule #2 (SqlDataSource) - ONE EXCEPTION

**Only Exception:** `sdsUserNames`

```aspx
<!-- ALLOWED - ASP.NET membership users (same TrackerDataSQL catalog as app data) -->
<asp:SqlDataSource ID="sdsUserNames" runat="server"
    ConnectionString="<%$ ConnectionStrings:TrackerDataSQL %>"
    SelectCommand="SELECT [UserName] AS SecurityUsername FROM [vw_aspnet_Users]" />
```

**Why Allowed:**
- Reads ASP.NET membership views (`vw_aspnet_Users`) that now live in the TrackerDataSQL catalog
- Standard ASP.NET security feature (not a business-table SqlDataSource)
- Will be refactored to a repository/Membership API in a later phase

**All other SqlDataSource controls are PROHIBITED.**

---

## Summary

### DO NOT

1. ❌ Use Microsoft Access database
2. ❌ Use OleDb classes
3. ❌ Use SqlDataSource controls (except `sdsUserNames`)
4. ❌ Use ObjectDataSource with legacy `*Tbl` classes
5. ❌ Put data access logic in ASPX markup
6. ❌ Hardcode magic numbers/strings

### DO

1. ✅ Use SQL Server database
2. ✅ Use SqlClient classes
3. ✅ Use Repository pattern
4. ✅ Bind data in code-behind
5. ✅ Use POCO models
6. ✅ Use `RepositoryBase<T>` for all repositories
7. ✅ Use `SystemConstants` and configuration settings
8. ✅ Put status messages at the bottom; use UpdatePanel + UpdateProgress on interactive pages

### DO NOT (UI)

1. ❌ Place result status above the form fields
2. ❌ Use HyperLink styled as Back instead of a Button
3. ❌ Add new interactive pages without UpdatePanel/UpdateProgress when async feedback is expected

---

## Where These Rules Are Documented

1. **This File:** `Documentation/HARD_PROJECT_RULES.md` (you are here)
2. **WebForms UI:** `Documentation/WEBFORMS_UI_STANDARDS.md` (status bottom, UpdatePanel, **page-tone panel**)
3. **Repository Standards:** `Documentation/REPOSITORY_STANDARDS.md` (naming conventions & standard methods)
4. **Architecture:** `Documentation/ARCHITECTURE_RULES.md`
5. **Project Overview:** `Documentation/PROJECT_OVERVIEW.md`
6. **Implementation Guide:** `Documentation/WorkInProgress/REPOSITORY_ENFORCEMENT_PHASE1_COMPLETE.md`

---

## Questions?

**Q: Why can't I just use SqlDataSource for a quick fix?**  
A: Because "quick fixes" create technical debt that takes hours to fix later. Doing it right the first time is faster.

**Q: The old code used Access, why can't I?**  
A: The old code is being replaced. We're migrating AWAY from Access. Don't go backwards.

**Q: ObjectDataSource is easier than code-behind binding.**  
A: Only for trivial cases. For real-world scenarios with dropdowns and CRUD, Repository pattern is cleaner and more maintainable.

**Q: What if I need to access Access for migration?**  
A: Use the `MigrationRunner` project. The main TrackerSQL application should NEVER touch Access.

---

**THESE RULES ARE NON-NEGOTIABLE. NO EXCEPTIONS (except `sdsUserNames`).**

**Last Updated:** 2026-07-14  

**Rule Version:** 1.0  
**Status:** ACTIVE
