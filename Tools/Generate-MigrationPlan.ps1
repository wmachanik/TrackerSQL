# Creates Docs\MigrationPlan_TrackerDotNet_to_TrackerSQL.md
$outDir = Join-Path $PSScriptRoot "..\Docs"
$outFile = Join-Path $outDir "MigrationPlan_TrackerDotNet_to_TrackerSQL.md"
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

$plan = @'
# Migration Plan: TrackerSQL (Access) → TrackerSQL (SQL Server / SQL Express)

Version: 1.0
Target Runtime: .NET Framework 4.8
Date: (fill in)

0. Executive Summary
We fork `TrackerSQL` → `TrackerSQL`, migrate schema/data to SQL Server, refactor away Access (OleDb), keep old project for hotfixes until cutover. Extend `test/ShowTableStruct.aspx` to extract schema → JSON + T-SQL.

1. Objectives
- Replace Access (32‑bit) with SQL Server / SQL Express
- Preserve UI behavior
- Centralize DAL via ITrackerDb
- Support dual or unified DB (Security/Data)
- Repeatable migration + rollback
- Remove Jet/Ace dependency

2. Scope (IN) project clone, schema/data migration, query refactor, DAL abstraction, connection strategy, tests, deployment docs. (OUT) full ORM / architecture rewrite (phase 1).

3. Phases (P1..P10)
P1 Clone & inventory
P2 Schema conversion (Schema.sql)
P3 Data migration (DataMigration.ps1)
P4 DAL abstraction
P5 Query refactor (remove Access idioms)
P6 Update SqlDataSources / move to ObjectDataSource
P7 Connection strategy (dual/unified)
P8 Testing
P9 Deployment + rollback
P10 Cleanup & hardening

4. Project Strategy
- Branch `version/2.x-access` (maintenance)
- New branch `feature/sql-migration`
- Copy project → rename to TrackerSQL (csproj rename + namespace replace)
- Keep shared utils identical until switch

5. Extend ShowTableStruct.aspx
Add export modes:
  ?format=json  (tables, columns, types, nullability, PK)
  ?format=tsql  (draft CREATE TABLE scripts)
Add Access→SQL type mapping + optional rowversion.

6. Type Mapping
COUNTER→INT IDENTITY, YES/NO→BIT, TEXT(n)→NVARCHAR(n), MEMO→NVARCHAR(MAX), CURRENCY→DECIMAL(19,4), SINGLE→REAL (or DECIMAL), DOUBLE→FLOAT, DATETIME→DATETIME2, OLEOBJECT→VARBINARY(MAX).

7. Schema Conversion
Generate draft → add PK/FK, unique, nonclustered indexes (SortOrder, ItemDesc, City, etc.), optional rowversion columns. Store in Database/Schema.sql + patches folder.

8. Data Migration
Use SSMA or CSV + BULK INSERT staging → final with IDENTITY_INSERT. Script: Database/DataMigration.ps1. Output row counts + checksums to Docs/Reports/MigrationValidation.md.

9. Connection Strings (Web.config)
TrackerData & TrackerSecurity (System.Data.SqlClient). Add appSetting UseUnifiedDatabase=true/false. If single DB host: point both names to same DB.

10. DAL Abstraction
ITrackerDb + TrackerDbSql (ExecuteReader/ExecuteNonQuery/ExecuteScalar/FillTable). Incrementally refactor DAL classes; remove OleDb.

11. Query Transform (Access → T-SQL)
iif(...)→IIF or CASE; IsNull(x,y)→ISNULL(x,y); Date()→CAST(GETDATE() AS date); Now()→GETDATE(); #date#→'date'; &→+; positional ? params→@Named; LIKE ?→LIKE @Search; TRUE/FALSE→1/0.

12. SqlDataSource Refactor Example (Items)
Access: iif(IsNull(ReplacementID),0,ReplacementID), LIKE ?.
SQL: ISNULL(ReplacementID,0) Replacement, WHERE ItemDesc LIKE @Search. Rename all parameters.

13. Refactor Order
sdsItems → sdsCities → sdsServiceTypes → any remaining `<asp:SqlDataSource>`.

14. Concurrency
Phase 1: simple PK updates. Optionally add rowversion later for optimistic concurrency in WHERE.

15. Testing
- Schema: table/PK/FK presence
- CRUD each GridView (Items, People, Equipment, Cities, Packaging, InvoiceTypes, PaymentTerms, PriceLevels, RepairStatuses)
- Auth (membership lookups)
- Data parity sample vs Access
- Performance smoke (page load)
- Slow query & exception logging

16. Deployment Runbook
Backup Access → run Schema.sql → run DataMigration.ps1 → deploy TrackerSQL staging → smoke test → freeze writes on Access → (optional delta) → swap IIS binding → monitor → finalize or rollback.

17. Rollback
Repoint IIS to Access site, restore Access DB, analyze cause, retry after fix.

18. Risks (sample)
Missed Access idioms, truncation, perf regression, membership break, single DB limitation, divergence. Mitigate with scans, audits, indexes, early auth test, unified fallback, branch isolation.

19. Acceptance Criteria
All CRUD pass; no OleDb refs; Access idiom scan empty; row counts match; auth ok; rollback tested.

20. Example Timeline (10 days)
1 Clone + schema export
2 Finalize schema
3 Data migration rehearsal
4 DAL + first refactors
5 Major SqlDataSource conversions
6 Remaining queries
7 Testing
8 Staging deploy
9 Production migration
10 Cleanup

21. Search Checklist
Patterns: iif(, IsNull(, Date(), Now(), LIKE ?, '= ?', ', ?', OleDb, ACE.OLEDB, #[date]#.

22. Folder Layout
/Docs (plan, reports, generated schema)
/Database (Schema.sql, DataMigration.ps1, Patches)
/DataAccess (ITrackerDb*, TrackerDbSql*)

23. Work Items (sample)
1 ShowTableStruct extension
2 Schema finalize
3 Data migration script
4 DAL abstraction
5 Items data source refactor
6 Remaining SqlDataSources
7 Access idiom cleanup
8 Dual/unified config
9 Test execution
10 Deployment rehearsal
11 Production cutover
12 Cleanup/docs

24. Pre-Go-Live Checklist
[ ] Schema deployed
[ ] Data migrated & validated
[ ] Access idioms removed
[ ] DAL abstraction used
[ ] Auth tested
[ ] Rollback tested
[ ] Staging logs clean
[ ] Sign-off

25. Post-Migration Enhancements
Caching lookups, health endpoint, Dapper, integration tests (LocalDB), .NET upgrade path.

26. Single vs Dual DB
Single: membership tables inside TrackerData. Dual: cleaner isolation.

27. Quick Reference
iif→CASE/IIF, IsNull→ISNULL, Date()→CAST(GETDATE() AS date), Now()→GETDATE(), #d#→'d', &→+, LIKE ?→LIKE @P, True/False→1/0.

End of Document
'@

Set-Content -Path $outFile -Value $plan -Encoding UTF8
Write-Host "Created $outFile"