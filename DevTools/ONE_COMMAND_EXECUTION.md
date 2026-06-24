# ONE-COMMAND Migration Execution Plan

**Total Time:** 8-10 hours  
**Total AI Calls:** 5-10 (vs. 100+)  
**Status:** Ready to Execute

---

## ?? Execute Now (3 Commands)

```powershell
# Step 1: Preview what will be created (2 min)
.\DevTools\Scripts\Master-Generate-All.ps1 -DryRun

# Step 2: Create all POCOs and Repositories (5 min)
.\DevTools\Scripts\Master-Generate-All.ps1

# Step 3: Open CSV to see what was created
code DevTools\Documentation\Legacy_To_Poco_Mapping.csv
```

**Result:**
- ? All missing POCOs created
- ? All missing Repositories created with templates
- ? Zero AI calls used

---

## ?? What Gets Created

### POCOs (3 missing)
Based on your current analysis, these POCOs don't exist:

1. ? `DeliveryItems` - will be created from `DeliveryItemsTbl.cs`
2. ? `ContactsWithDatesAndUsage` - will be created from `CustomersWithDatesAndUsageTbl.cs`
3. ? `OrderItem` - will be created from `OrderItemTbl.cs`

**Action:** Script auto-creates with properties from legacy classes

### Repositories (20 missing)
These repositories will be created with standard CRUD templates:

**High Priority (have custom methods):**
1. `UsedItemGroupRepository` (2 custom methods)
2. `ContactsItemsPredictedRepository` (2 custom methods)
3. `LogRepository` (1 custom method)
4. `NextPrepDateByAreaRepository` (1 custom method)
5. `OrderCheckRepository` (1 custom method)

**Medium Priority (standard CRUD only):**
6-20. See CSV for complete list

**Action:** Script auto-creates with TODO placeholders for CRUD

---

## ?? Fill-In Work (Manual, 0 AI)

### Phase 1: Fill CRUD (2-3 hours, 0 AI)

**For each repository:**

```
Time: 10-15 minutes per repo
Process:
  1. Open legacy class (Controls\*Tbl.cs)
  2. Open new repo (Classes\Sql\*Repository.cs)
  3. Copy INSERT SQL ? fill template
  4. Copy UPDATE SQL ? fill template
  5. DELETE already done
  6. Build
```

**Guide:** `DevTools\Documentation\Zero_AI_CRUD_Fill_Guide.md`

**Example workflow:**
```powershell
# Repo 1: ContactsItemsPredictedRepository
code Controls\ClientUsageTbl.cs Classes\Sql\ContactsItemsPredictedRepository.cs
# Copy INSERT/UPDATE, fill template (10 min)

# Repo 2: UsedItemGroupRepository
code Controls\UsedItemGroupTbl.cs Classes\Sql\UsedItemGroupRepository.cs
# Copy INSERT/UPDATE, fill template (10 min)

# Repo 3: LogRepository
code Controls\LogTbl.cs Classes\Sql\LogRepository.cs
# Copy INSERT/UPDATE, fill template (10 min)

# ... continue for all 20 repos
```

**Total time:** 20 repos × 10-15 min = 3-5 hours

### Phase 2: Copy Custom Methods (2-3 hours, 5-10 AI)

**Simple methods (copy-paste, 0 AI):**
```csharp
// Example: GetReminderCount from CustomersTbl
public int GetReminderCount(int contactId)
{
    string sql = "SELECT ReminderCount FROM ContactsTbl WHERE ContactID = @Id";
    var p = new List<DBParameter> { new DBParameter { DataValue = contactId, DataDbType = DbType.Int32, ParamName = "@Id" } };
    return ExecuteScalar<int>(sql, p);
}
```

**Complex methods (use AI, 1-2 calls each):**
- Methods with joins across 3+ tables
- Subqueries
- Dynamic SQL

**From CSV, these have custom methods:**
- `ContactsRepository`: 9 custom methods
- `PersonsRepository`: 4 custom methods
- `ItemGroupsRepository`: 4 custom methods
- `ContactsAwayPeriodRepository`: 1 custom method
- `ItemsRepository`: 1 custom method

**Estimate:**
- Simple: 15 methods × 2 min = 30 min (0 AI)
- Complex: 5 methods × 30 min = 2.5 hours (5-10 AI calls)

---

## ?? Timeline Breakdown

| Phase | Time | AI Calls | What You Do |
|---|---|---|---|
| **Execute Scripts** | 10 min | 0 | Run 2 commands |
| **Fill CRUD** | 3-5 hours | 0 | Copy-paste SQL, fill templates |
| **Copy Simple Custom** | 30 min | 0 | Copy-paste simple methods |
| **Copy Complex Custom** | 2-3 hours | 5-10 | Use AI for complex methods |
| **Update Callers** | 1-2 hours | 0 | Find/replace class names |
| **Testing** | 1 hour | 0 | Build, test, smoke test |
| **TOTAL** | **8-12 hours** | **5-10** | |

**vs. Manual:** 40+ hours, 100+ AI calls

---

## ? Execution Checklist

### Prerequisites
- [x] Git branch created
- [x] Backup committed
- [x] Analysis run

### Execution
- [ ] Run dry-run preview
- [ ] Execute generation scripts
- [ ] Review created files
- [ ] Fill CRUD methods (use guide)
- [ ] Copy custom methods (simple first)
- [ ] Use AI for complex methods only
- [ ] Update callers
- [ ] Build
- [ ] Test

---

## ?? Reference Documents

**Execution:**
- `DevTools\Documentation\Zero_AI_CRUD_Fill_Guide.md` ? Copy-paste patterns
- `DevTools\Documentation\Minimal_AI_Repository_Migration_Plan.md` ? Full plan
- `DevTools\QUICK_START_REPOSITORY_MIGRATION.md` ? Quick reference

**Reference:**
- `DevTools\Documentation\Naming_Convention_Reference.md` - Table/column mappings
- `DevTools\Documentation\Legacy_To_Poco_Mapping.csv` - Analysis results

---

## ?? Success Metrics

| Metric | Target | How to Check |
|---|---|---|
| POCOs Created | 3 | Check Classes\Poco\ |
| Repos Created | 20 | Check Classes\Sql\ |
| Build Errors | 0 | dotnet build |
| CRUD Complete | 100% | Manual review |
| Custom Methods | 100% | Check CSV |
| AI Calls | ?10 | Manual count |

---

## ?? START NOW

```powershell
# Execute this ONE command:
.\DevTools\Scripts\Master-Generate-All.ps1

# Then follow the Zero-AI CRUD Fill Guide
code DevTools\Documentation\Zero_AI_CRUD_Fill_Guide.md
```

**Estimated completion:** Tomorrow if you start now! ??
