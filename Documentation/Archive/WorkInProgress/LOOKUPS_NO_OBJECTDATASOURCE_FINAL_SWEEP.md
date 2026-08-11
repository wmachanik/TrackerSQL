# Lookups.aspx Migration Notes (FINAL SWEEP) — No ObjectDataSource / Manual Binding

**Date:** 2026-04-16  
**Scope:** `Pages/Lookups.aspx` + `Pages/Lookups.aspx.cs`  
**Goal:** Enforce current project standard:
- ? No `SqlDataSource` for app data (exception: `sdsUserNames` only)
- ? No `ObjectDataSource` anywhere
- ? Repository pattern for reads
- ? Manual binding in code-behind (`GridView.DataSource = ...; DataBind();`)
- ? Avoid duplicate/implicit binds (`GridView.DataBind()` with no `DataSource`)

---

## 1. ObjectDataSource removals (ASPX)
Removed these from `Lookups.aspx`:
- `odsPeople`
- `odsEquipTypes`
- `odsPackaging`
- `odsInvoiceTypes`
- `odsPaymentTerms`
- `odsPriceLevels`
- `odsAreaDays` (legacy `Controls.AreaPrepDaysTbl`)
- `odsRepairStatuses`

Also removed any `DataSourceID="ods..."` usage from the corresponding `GridView`s.

**Allowed exception remaining:** `sdsUserNames` (ASP.NET membership database) is still present.

---

## 2. Manual binding added (code-behind)
Implemented explicit bind methods in `Lookups.aspx.cs`:
- `BindItemsGrid()` (already existed)
- `BindCitiesGrid()` (already existed)
- `BindPeopleGrid()`
- `BindEquipmentGrid()`
- `BindPackagingGrid()`
- `BindInvoiceTypesGrid()`
- `BindPaymentTermsGrid()`
- `BindPriceLevelsGrid()`
- `BindRepairStatusesGrid()`
- `BindAreaDaysGrid()` (detail grid under Cities)

All of these are invoked on first load (`!IsPostBack`) and from paging/sorting and CRUD handlers.

---

## 3. Paging/Sorting now rebind explicitly
For each converted grid, paging/sorting events do:
- set page index / store sort expression in `ViewState`
- call the relevant `Bind...Grid()`

**Do not** call `GridView.DataBind()` expecting ODS/DS to provide data.

---

## 4. Key runtime binding fixes (POCO alignment)
Packaging grid:
- `Bind("Description")` ? `Bind("ItemPackagingDesc")`
- `Bind("PackagingID")` ? `Bind("ItemPackagingID")`
- `SortExpression="Description"` ? `SortExpression="ItemPackagingDesc"`

`gvPackaging_RowDataBound`:
- cast `e.Row.DataItem` to `ItemPackaging` (not legacy `PackagingTbl`)

Cities prep-days detail grid (`gvAreaDays`):
- Hidden key binding updated:
  - `Bind("AreaPrepDaysID")` ? `Bind("AreaPrepDaysID")`

RepairStatus POCO mismatch fix:
- Property is `EmailContact` (nullable bool), **not** `EmailClient`

---

## 5. Legacy `Controls/*Tbl.cs` usage removed from Lookups
People update previously used legacy `PersonsTbl.UpdatePerson(...)`.
Now:
- build a `Person` POCO
- call `PersonsRepository.Update(person)`
- rebind via `BindPeopleGrid()`

Cities prep-days previously used legacy `Controls.AreaPrepDaysTbl` + `ObjectDataSource`.
Now:
- Uses `AreaPrepDaysTbl` data model (POCO: `AreaPrepDays`)
- Manual binding via `BindAreaDaysGrid()`

---

## 6. IMPORTANT: RepositoryBase currently lacks CRUD
`RepositoryBase<T>` in this codebase provides:
- `GetAll()`, `GetAll(sortBy)`, `GetById()`, etc.

It does **NOT** implement generic `Insert/Update/Delete` at this time.

### What was done in Lookups to keep forward progress
For grids requiring CRUD where repositories do not support it yet:
- Implemented small, parameterized SQL `INSERT/UPDATE/DELETE` via `TrackerSQLDb.ExecuteNonQuery(...)`
- Centralized behind a local helper in `Lookups.aspx.cs`:
  - `ExecNonQuery(string sql, List<TrackerSQL.Classes.DBParameter> parameters)`

This was used for:
- `AreaPrepDaysTbl` CRUD (Cities detail)
- `RepairStatusesTbl` CRUD

**Migration note:** When a proper repository CRUD layer is implemented, these SQL blocks should be moved into the relevant repositories.

---

## 7. Cleanup to prevent duplicate calls in future migrations
To avoid duplication/confusion on other pages:
- Removed unused protected fields in `Lookups.aspx.cs` that referred to deleted ODS/SDS controls (except `sdsUserNames`).
- Removed `using TrackerSQL.Controls;` from `Lookups.aspx.cs`.

---

## 8. Checklist for migrating other pages
When converting another page:
1. Remove all `<asp:ObjectDataSource ...>` and `DataSourceID="ods..."`
2. Replace every `GridView.DataBind()` call that relied on ODS with an explicit `BindXGrid()`
3. Validate all `<%# Bind("...") %>` / `Eval("...")` against the target POCO properties
4. Ensure `RowDataBound` casts match the actual POCO type
5. Add paging/sorting handlers that call the appropriate bind method
6. Leave `sdsUserNames` only if it targets Membership DB (not TrackerDataSQL)

---

## Files changed
- `Pages/Lookups.aspx`
- `Pages/Lookups.aspx.cs`
- `Documentation/HARD_PROJECT_RULES.md`
- `Documentation/PROJECT_OVERVIEW.md`

