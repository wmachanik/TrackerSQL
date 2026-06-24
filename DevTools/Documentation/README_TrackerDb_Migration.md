# TrackerDb to TrackerSQLDb Migration - Summary & Index

**Created:** 2025-05-14  
**Purpose:** Master index for all TrackerDb migration documentation  
**Status:** Ready for Migration

---

## ?? Quick Links

| Document | Purpose | When to Use |
|----------|---------|-------------|
| **[Migration Plan](TrackerDb_to_TrackerSQLDb_Migration_Plan.md)** | Complete migration strategy | Planning, tracking progress |
| **[Quick Reference](Quick_Reference_TrackerDb_Migration.md)** | Fast lookup of patterns | During coding |
| **[Migration Template](Migration_Template.md)** | File-by-file template | When starting a new file |
| **[Example Migration](AreaPrepDaysTbl_Migration_Example.md)** | Worked example | Reference for how-to |
| **PowerShell Script** | Find all TrackerDb usage | Initial analysis |

---

## ?? Getting Started (First Time)

### Step 1: Understand the Problem
Read: [Migration Plan - Overview Section](TrackerDb_to_TrackerSQLDb_Migration_Plan.md#overview)

**Key Takeaway:** We're moving from OleDb/Access to SqlClient/SQL Server

### Step 2: Run Analysis
```powershell
cd C:\SRC\ASP.net\TrackerSQL
.\DevTools\Scripts\Find-TrackerDbUsages.ps1
```

This generates: `DevTools\Documentation\TrackerDb_Usage_Report.txt`

### Step 3: Review the Example
Read: [AreaPrepDaysTbl Migration Example](AreaPrepDaysTbl_Migration_Example.md)

**Key Takeaway:** See complete before/after for all method types

### Step 4: Pick a File to Migrate
Start with: **Phase 1 - Critical Path** files from Migration Plan

### Step 5: Use the Template
Copy: [Migration Template](Migration_Template.md) and fill it in

### Step 6: Refer to Quick Reference
Keep open: [Quick Reference](Quick_Reference_TrackerDb_Migration.md) while coding

---

## ?? Migration Process Overview

```
???????????????????
?  Find TrackerDb ?  ? PowerShell script
?     Usages      ?
???????????????????
         ?
         v
???????????????????
?  Pick a File    ?  ? Start with Phase 1
???????????????????
         ?
         v
???????????????????
?  Copy Template  ?  ? Use Migration_Template.md
???????????????????
         ?
         v
???????????????????
?  Migrate Code   ?  ? Use Quick_Reference.md
?   Method by     ?     and Example
?     Method      ?
???????????????????
         ?
         v
???????????????????
?  Test Each      ?  ? Unit + Manual testing
?    Method       ?
???????????????????
         ?
         v
???????????????????
?  Commit When    ?  ? Git commit per file
?   Complete      ?
???????????????????
         ?
         v
???????????????????
?  Update Plan    ?  ? Mark as COMPLETED
?     Status      ?
???????????????????
```

---

## ?? Document Descriptions

### 1. TrackerDb_to_TrackerSQLDb_Migration_Plan.md
**~370 lines**

**Contains:**
- Complete migration strategy
- All 32+ files that need migration
- Priority order (4 phases)
- Testing strategy
- Progress tracking

**Use when:**
- Planning overall migration
- Tracking which files are done
- Understanding priorities
- Checking status

---

### 2. Quick_Reference_TrackerDb_Migration.md
**~250 lines**

**Contains:**
- Side-by-side comparison tables
- Common code transformations
- Parameter creation shortcuts
- DbType reference
- Common mistakes to avoid

**Use when:**
- Actively coding a migration
- Need quick pattern lookup
- Forgot a DbType
- Debugging migration issues

---

### 3. Migration_Template.md
**~400 lines**

**Contains:**
- Complete method-by-method template
- Before/after sections for each method
- Testing checklist
- Data type mapping table
- Common patterns reference

**Use when:**
- Starting migration of a new file
- Want structured approach
- Need to document migration steps
- Creating PR for review

---

### 4. AreaPrepDaysTbl_Migration_Example.md
**~350 lines**

**Contains:**
- Real working example
- All 4 CRUD operations
- Complete before/after code
- Detailed annotations
- Testing checklist
- Common issues & solutions

**Use when:**
- First time migrating
- Need concrete example
- Stuck on a specific pattern
- Want to see full file migration

---

### 5. Find-TrackerDbUsages.ps1
**~150 lines**

**Contains:**
- PowerShell analysis script
- Scans all .cs files
- Generates usage report
- Categorizes by file type
- Recommends priority order

**Use when:**
- Starting migration project
- Need to see scope
- Want to prioritize files
- Creating migration plan

---

## ?? Quick Pattern Reference

### INSERT
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "INSERT INTO Table (Col1, Col2) VALUES (@Col1, @Col2)";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = val1, DataDbType = DbType.String, ParamName = "@Col1" },
        new DBParameter { DataValue = val2, DataDbType = DbType.Int32, ParamName = "@Col2" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
}
```

### SELECT
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "SELECT * FROM Table WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };

    using (IDataReader reader = db.ExecuteReader(sql, parameters))
    {
        if (reader != null && reader.Read()) { /* process */ }
    }
}
```

### UPDATE
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "UPDATE Table SET Field = @Field WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = val, DataDbType = DbType.String, ParamName = "@Field" },
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
}
```

### DELETE
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "DELETE FROM Table WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
}
```

---

## ?? Migration Status Overview

| Phase | Files | Status | Notes |
|-------|-------|--------|-------|
| **Phase 1** | 4 files | ?? In Planning | Critical path - start here |
| **Phase 2** | 5 files | ? Not Started | High impact |
| **Phase 3** | 11 files | ? Not Started | Supporting tables |
| **Phase 4** | 12 files | ? Not Started | Lookup tables |

**Total:** 32+ files to migrate

---

## ? Checklist for Each File

### Pre-Migration
- [ ] Read the file completely
- [ ] List all methods that use TrackerDb
- [ ] Copy Migration Template
- [ ] Review Example if needed

### During Migration
- [ ] Update one method at a time
- [ ] Test each method immediately
- [ ] Add error handling and logging
- [ ] Use `using` statements

### Post-Migration
- [ ] All methods migrated
- [ ] All tests pass
- [ ] Code compiles
- [ ] Commit to Git
- [ ] Update Migration Plan status

---

## ?? Common Mistakes to Avoid

1. ? **Forgetting to change `?` to `@ParamName` in SQL**
2. ? **Not wrapping in `using` statements**
3. ? **Parameter names don't match SQL placeholders**
4. ? **Forgetting to pass parameters to ExecuteNonQuery/ExecuteReader**
5. ? **Not adding error handling and logging**

---

## ?? Git Workflow

### Branch Naming
```
feature/migrate-trackerdb-[filename]
```

Example: `feature/migrate-trackerdb-areprepdaystbl`

### Commit Messages
```
Migrated [FileName] from TrackerDb to TrackerSQLDb

- Replaced all TrackerDb usage with TrackerSQLDb
- Updated [X] methods: [list]
- Added error handling and logging
- All tests passing

Closes #[issue-number]
```

### Pull Request Template
```markdown
## Description
Migrated [FileName] from legacy TrackerDb (OleDb/Access) to TrackerSQLDb (SQL Server)

## Methods Migrated
- [ ] Method1 (INSERT)
- [ ] Method2 (SELECT)
- [ ] Method3 (UPDATE)
- [ ] Method4 (DELETE)

## Testing Done
- [ ] Unit tests pass
- [ ] Manual testing complete
- [ ] No regression in functionality
- [ ] Error handling verified
- [ ] Logging verified

## Checklist
- [ ] Code compiles without errors
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Migration Plan status updated
```

---

## ?? Finding Files to Migrate

### Run PowerShell Script
```powershell
.\DevTools\Scripts\Find-TrackerDbUsages.ps1
```

### Manual Search in Visual Studio
1. Ctrl+Shift+F (Find in Files)
2. Search for: `new TrackerDb\(\)`
3. Use Regular Expressions: ON
4. Look in: Entire Solution

### Recommended Order
1. Start with `Controls\*Tbl.cs` files
2. Then `Controls\*DAL.cs` files
3. Then any code-behind files
4. Finally, utility classes

---

## ?? Support Resources

### Documentation Files
- [Migration Plan](TrackerDb_to_TrackerSQLDb_Migration_Plan.md)
- [Quick Reference](Quick_Reference_TrackerDb_Migration.md)
- [Migration Template](Migration_Template.md)
- [Example Migration](AreaPrepDaysTbl_Migration_Example.md)

### Project Documentation
- [HARD_PROJECT_RULES.md](../../Documentation/HARD_PROJECT_RULES.md)
- [PROJECT_OVERVIEW.md](../../Documentation/PROJECT_OVERVIEW.md)
- [MIGRATION_TODO.md](../../Documentation/WorkInProgress/MIGRATION_TODO.md)

### PowerShell Tools
- `DevTools\Scripts\Find-TrackerDbUsages.ps1`

---

## ?? Training Path

### Day 1: Understanding (2 hours)
1. Read Migration Plan Overview (30 min)
2. Read AreaPrepDaysTbl Example (45 min)
3. Run PowerShell script (15 min)
4. Review generated report (30 min)

### Day 2: First Migration (4 hours)
1. Pick simple file from Phase 4 (30 min)
2. Copy Migration Template (15 min)
3. Migrate first method (1 hour)
4. Test thoroughly (1 hour)
5. Complete remaining methods (1 hour)
6. Final testing & commit (15 min)

### Day 3: Ramp Up (4 hours)
1. Pick Phase 3 file (2 hours)
2. Pick another Phase 3 file (2 hours)

### Week 2: Critical Path
1. Phase 2 files (2-3 hours each)
2. Phase 1 files (3-4 hours each)

---

## ?? Progress Tracking

Create a simple tracker:

```markdown
## Migration Progress

### Phase 1 (Critical)
- [ ] CustomersTbl.cs (0/15 methods)
- [ ] OrdersTbl.cs (0/10 methods)
- [ ] ItemTypeTbl.cs (0/12 methods)
- [ ] OrderDataControl.cs (0/5 methods)

### Phase 2 (High Impact)
- [ ] ReoccuringOrderDAL.cs (0/10 methods)
- [ ] ItemUsageTbl.cs (0/5 methods)
- [ ] PersonsTbl.cs (0/5 methods)
- [ ] OrderDetailDAL.cs (0/5 methods)
- [ ] ClientUsageTbl.cs (0/5 methods)

... etc
```

---

## ?? Success Criteria

Migration is complete when:
- [ ] All 32+ files migrated
- [ ] All tests passing
- [ ] No TrackerDb references in Controls folder
- [ ] No TrackerDb references in Pages folder
- [ ] Migration Plan shows 100% complete
- [ ] Full regression testing passed
- [ ] Production deployment successful

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-05-14 | Initial summary and index created |

---

**Ready to start?** Begin with [Migration Plan](TrackerDb_to_TrackerSQLDb_Migration_Plan.md) and [Example Migration](AreaPrepDaysTbl_Migration_Example.md)!

**Good luck!** ??
