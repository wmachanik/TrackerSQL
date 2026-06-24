# Quick Start: Add Missing CRUD to Repositories

**Time:** 2 minutes to add, 2-3 hours to fill in  
**AI Calls:** 0

---

## ?? IMPORTANT: What RepositoryBase Already Provides

**RepositoryBase<T> already includes:**
- ? `GetById(int id)` - inherited from base
- ? `GetAll()` - inherited from base
- ? `GetAll(string SortBy)` - inherited from base
- ? `GetKeyColsById(int id)` - inherited from base
- ? `GetLookupValues()` - inherited from base
- ? `GetLookupList()` - inherited from base
- ? `GetAllEnabled()` - inherited from base

**Scripts only add the missing methods:**
- ? `Insert(T entity)` - not in base, needs adding
- ? `Update(T entity)` - not in base, needs adding
- ? `Delete(int id)` - not in base, needs adding

---

## TL;DR - One Command

```powershell
# Preview what will be added (30 seconds)
.\DevTools\Scripts\Batch-Add-Missing-CRUD.ps1 -DryRun

# Add missing CRUD to all repositories (2 minutes)
.\DevTools\Scripts\Batch-Add-Missing-CRUD.ps1
```

**Result:**
- ? Insert, Update, Delete added (with TODO for Insert/Update)
- ? GetById, GetAll already in RepositoryBase (inherited)
- ? Existing methods unchanged
- ? Custom methods untouched

---

## What Gets Added

**RepositoryBase<T> Already Provides (No Need to Add):**

All repositories inherit these from `RepositoryBase<T>`:

```csharp
// Already available - do NOT add to individual repos
public virtual T GetById(int id) { ... }
public virtual List<T> GetAll() { ... }
public virtual List<T> GetAll(string SortBy) { ... }
public virtual T GetKeyColsById(int id) { ... }
public virtual List<T> GetLookupValues(string sortBy = null) { ... }
public virtual List<T> GetLookupList() { ... }
public virtual List<T> GetAllEnabled(string sortBy = null) { ... }
```

**? These work immediately, no code needed!**

### Methods Script Adds (The Missing Ones)
```csharp
public int Insert(Contact entity)
{
    if (entity == null) throw new ArgumentNullException(nameof(entity));

    // TODO: Customize INSERT statement based on POCO properties
    // Copy from legacy Controls\*Tbl.cs Insert method
    string sql = @"
        INSERT INTO ContactsTbl (
            -- TODO: Add column names here
        )
        VALUES (
            -- TODO: Add parameter names here (@Param1, @Param2, etc.)
        );
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

    var parameters = new List<DBParameter>
    {
        // TODO: Add parameters based on POCO properties
        // Example:
        // new DBParameter { DataValue = entity.PropertyName, DataDbType = DbType.String, ParamName = "@PropertyName" }
    };

    return ExecuteScalar<int>(sql, parameters);
}
```
**Status:** ?? Need to fill in TODOs (10 min)

#### 5. Update(T entity)
```csharp
public bool Update(Contact entity)
{
    if (entity == null) throw new ArgumentNullException(nameof(entity));

    // TODO: Customize UPDATE statement based on POCO properties
    // Copy from legacy Controls\*Tbl.cs Update method
    string sql = @"
        UPDATE ContactsTbl
        SET 
            -- TODO: Add column = @Param pairs here
        WHERE ContactID = @Id";

    var parameters = new List<DBParameter>
    {
        // TODO: Add parameters based on POCO properties
        // Don't forget to add the ID parameter at the end
    };

    int result = ExecuteNonQuery(sql, parameters);
    return result > 0;
}
```
**Status:** ?? Need to fill in TODOs (10 min)

---

## Execution Steps

### Step 1: Add Methods (2 minutes)

```powershell
# Preview (optional)
.\DevTools\Scripts\Batch-Add-Missing-CRUD.ps1 -DryRun

# Execute
.\DevTools\Scripts\Batch-Add-Missing-CRUD.ps1
```

**Output:**
```
Found 25 repositories to check

[1/25] Checking: ContactsRepository
  ? Complete (all CRUD exists)

[2/25] Checking: ItemsRepository
  + Updated (2 methods added)

[3/25] Checking: AreasRepository
  + Updated (5 methods added)

...

Summary:
  ? Already Complete: 5
  + Updated: 20
  Total: 25
```

### Step 2: Fill TODOs (2-3 hours)

For each repository with TODOs:

```powershell
# Open repository and legacy class side-by-side
code Classes\Sql\ContactsRepository.cs Controls\CustomersTbl.cs
```

**Follow:** `DevTools\Documentation\Zero_AI_CRUD_Fill_Guide.md`

**Process per repository:**
1. Find `// TODO` comments (2)
2. Open legacy class
3. Copy INSERT SQL ? fill template (5 min)
4. Copy UPDATE SQL ? fill template (5 min)
5. Build and verify (1 min)

**Total:** 10-15 min per repository

---

## What Gets Skipped

### Methods NOT Added (Intentionally)

The script **does NOT add** if they already exist:
- ? Existing GetAll
- ? Existing GetById
- ? Existing Insert
- ? Existing Update
- ? Existing Delete
- ? Any custom methods (GetByXxx, UpdateXxx, etc.)

**Why?** Avoids overwriting existing implementations!

---

## Example: Before & After

### Before (Minimal Repository)

```csharp
public class ContactsRepository : RepositoryBase<Contact>
{
    protected override string TableName => "ContactsTbl";
    protected override string KeyColumn => "ContactID";

    protected override Contact Map(IDataReader reader)
    {
        return DbMapper.Map<Contact>(reader);
    }

    // Custom method (kept!)
    public int GetReminderCount(int contactId)
    {
        // ... existing implementation
    }
}
```

**Methods:** 1 (custom only)  
**Missing:** GetAll, GetById, Insert, Update, Delete

### After (Full CRUD)

```csharp
public class ContactsRepository : RepositoryBase<Contact>
{
    protected override string TableName => "ContactsTbl";
    protected override string KeyColumn => "ContactID";

    protected override Contact Map(IDataReader reader)
    {
        return DbMapper.Map<Contact>(reader);
    }

    // ? ADDED - Complete
    public List<Contact> GetAll(string sortColumn = null) { ... }

    // ? ADDED - Complete
    public Contact GetById(int id) { ... }

    // ?? ADDED - Need to fill TODO
    public int Insert(Contact entity) { ... }

    // ?? ADDED - Need to fill TODO
    public bool Update(Contact entity) { ... }

    // ? ADDED - Complete
    public bool Delete(int id) { ... }

    // Custom method (kept!)
    public int GetReminderCount(int contactId)
    {
        // ... existing implementation unchanged
    }
}
```

**Methods:** 6 (5 CRUD + 1 custom)  
**Complete:** 3 (GetAll, GetById, Delete)  
**Need Fill:** 2 (Insert, Update)  
**Custom:** 1 (unchanged)

---

## Time Estimates

### Per Repository

| Task | Time | AI |
|---|---|---|
| Script adds methods | 10 sec | 0 |
| Fill INSERT TODO | 5 min | 0 |
| Fill UPDATE TODO | 5 min | 0 |
| Build verify | 1 min | 0 |
| **Total per repo** | **11 min** | **0** |

### For All Repositories

Based on your analysis: **20 repos need updates**

| Scenario | Estimate |
|---|---|
| Script execution | 2 min |
| 20 repos × 11 min | 3.5 hours |
| **Total** | **~4 hours** |

**vs. Manual:** 8-10 hours  
**Savings:** 4-6 hours

---

## Quality Checks

### After Script Runs

```powershell
# Check no syntax errors
dotnet build

# Should compile (even with TODOs)
# TODOs throw NotImplementedException at runtime only
```

### After Filling TODOs

```powershell
# Build
dotnet build

# Run tests
dotnet test

# Manual test one repository
var repo = new ContactsRepository();
var all = repo.GetAll("CompanyName");  // ? works
var one = repo.GetById(1);              // ? works
var id = repo.Insert(new Contact {...}); // ?? verify TODO filled
repo.Update(one);                        // ?? verify TODO filled
repo.Delete(id);                         // ? works
```

---

## Troubleshooting

### Issue: Script says "ERROR: Could not determine POCO type"

**Cause:** Repository doesn't inherit from `RepositoryBase<T>`

**Fix:** Repository must follow pattern:
```csharp
public class SomeRepository : RepositoryBase<SomeType>
{
    protected override string TableName => "SomeTableName";
    protected override string KeyColumn => "SomeID";
    // ...
}
```

### Issue: Method already exists but different signature

**Example:**
- Existing: `GetAll()`
- Script wants: `GetAll(string sortColumn = null)`

**Result:** Script sees it exists, skips adding

**Fix:** None needed! Existing implementation takes precedence.

---

## Next Steps

1. **Execute script** (2 min)
   ```powershell
   .\DevTools\Scripts\Batch-Add-Missing-CRUD.ps1
   ```

2. **Review output** (1 min)
   - Note which repos were updated
   - Note which are already complete

3. **Fill TODOs** (3-4 hours)
   - Use `Zero_AI_CRUD_Fill_Guide.md`
   - Copy-paste pattern (0 AI)

4. **Build & Test** (30 min)
   ```powershell
   dotnet clean
   dotnet build
   dotnet test
   ```

5. **Done!** ?

---

## Reference

**Guide:** `DevTools\Documentation\Zero_AI_CRUD_Fill_Guide.md`  
**Naming:** `DevTools\Documentation\Naming_Convention_Reference.md`  
**Analysis:** `DevTools\Documentation\Legacy_To_Poco_Mapping.csv`

---

**Start now:**

```powershell
.\DevTools\Scripts\Batch-Add-Missing-CRUD.ps1
```
