# Minimal-AI Repository Migration Plan

**Created:** 2026-05-15  
**Purpose:** Migrate legacy Controls\*Tbl.cs to POCO+Repository WITHOUT excessive AI usage  
**Estimated Time:** 6-8 hours  
**Estimated AI Calls:** 5-10 (vs. 100+ for manual approach)

---

## ?? Strategy Overview

### What We Have
- ? POCO classes (in `Classes/Poco/`)
- ? Some repositories (in `Classes/Sql/`)
- ? Legacy classes with business logic (in `Controls/*Tbl.cs`)

### What We Need
- Missing repositories for POCOs
- Custom business logic migrated from legacy classes
- Legacy classes deprecated

### How We'll Do It (Minimal AI)
1. **Automated Analysis** (0 AI calls) - Script identifies gaps
2. **Automated Generation** (0 AI calls) - Script creates repo templates
3. **Manual Copy-Paste** (0 AI calls) - Copy custom methods
4. **AI Review** (5-10 calls) - Only for complex custom logic

---

## Phase 1: Analysis (10 minutes, 0 AI calls)

### Step 1.1: Analyze Legacy Classes

```powershell
cd C:\SRC\ASP.net\TrackerSQL
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1
```

**Output:** `DevTools/Documentation/Legacy_To_Poco_Mapping.csv`

**What It Shows:**
- Which POCOs exist
- Which repositories exist
- Which repositories are missing
- Custom methods in each legacy class

### Step 1.2: Review Analysis

```powershell
# Open the CSV
code DevTools\Documentation\Legacy_To_Poco_Mapping.csv

# Or view in Excel
start DevTools\Documentation\Legacy_To_Poco_Mapping.csv
```

**Prioritize:**
- **HIGH:** Missing repositories (create first)
- **MEDIUM:** Repos exist but custom logic in legacy class
- **LOW:** Repos exist with all logic migrated

---

## Phase 2: Generate Missing Repositories (30 minutes, 0 AI calls)

### Step 2.1: Generate Repository Template

For each missing repository:

```powershell
# Example: Generate CustomerRepository
.\DevTools\Scripts\Generate-Repository.ps1 `
    -PocoName "Contact" `
    -TableName "ContactsTbl" `
    -PrimaryKeyColumn "ContactID"

# Example: Generate ItemUsageRepository  
.\DevTools\Scripts\Generate-Repository.ps1 `
    -PocoName "ContactsItemUsage" `
    -TableName "ContactsItemUsageTbl" `
    -PrimaryKeyColumn "ContactItemUsageLineNo"
```

**What It Creates:**
- Repository class inheriting from `RepositoryBase<T>`
- Standard CRUD methods (GetAll, GetById, Insert, Update, Delete)
- TODO comments for customization
- Placeholders for custom methods

### Step 2.2: Batch Generation

```powershell
# Generate all missing repos at once
$missing = Import-Csv "DevTools\Documentation\Legacy_To_Poco_Mapping.csv" | 
    Where-Object { $_.RepoExists -eq "NO" }

foreach ($item in $missing) {
    Write-Host "Generating $($item.RepoName)..." -ForegroundColor Cyan

    .\DevTools\Scripts\Generate-Repository.ps1 `
        -PocoName $item.PocoName `
        -TableName "$($item.PocoName)Tbl"

    Write-Host ""
}
```

---

## Phase 3: Fill in CRUD Methods (2-3 hours, 0 AI calls)

### Step 3.1: Pattern for INSERT

**Template (in generated repo):**
```csharp
public int Insert(Contact entity)
{
    string sql = @"
        INSERT INTO ContactsTbl (
            -- ADD COLUMNS HERE
        )
        VALUES (
            -- ADD @PARAMS HERE
        );
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

    var parameters = new List<DBParameter>
    {
        // ADD PARAMETERS HERE
    };

    return ExecuteScalar<int>(sql, parameters);
}
```

**Where to Get Column Names:**
1. Open legacy `Controls\CustomersTbl.cs`
2. Find existing `Insert` method
3. Copy column names and parameter list

**Example - CustomersTbl.cs:**
```csharp
// OLD (in CustomersTbl.cs)
string sql = "INSERT INTO CustomersTbl (CompanyName, ContactFirstName, ...) VALUES (?, ?, ...)";
trackerDb.AddParams((object)customer.CompanyName, DbType.String);
trackerDb.AddParams((object)customer.ContactFirstName, DbType.String);
```

**NEW (in ContactsRepository.cs):**
```csharp
public int Insert(Contact entity)
{
    string sql = @"
        INSERT INTO ContactsTbl (CompanyName, ContactFirstName, ...)
        VALUES (@CompanyName, @ContactFirstName, ...);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = entity.CompanyName, DataDbType = DbType.String, ParamName = "@CompanyName" },
        new DBParameter { DataValue = entity.ContactFirstName, DataDbType = DbType.String, ParamName = "@ContactFirstName" },
        // ... etc
    };

    return ExecuteScalar<int>(sql, parameters);
}
```

### Step 3.2: Pattern for UPDATE

**Same process:**
1. Find UPDATE in legacy class
2. Copy SQL and parameters
3. Paste into repo template
4. Convert `?` to `@ParamName`
5. Convert `AddParams` to `DBParameter`

### Step 3.3: Use Migration Template

For each repository, follow this checklist:

```markdown
## Migrating [PocoName]Repository

- [ ] Open `Controls\[Legacy]Tbl.cs`
- [ ] Open `Classes\Sql\[PocoName]Repository.cs`
- [ ] Copy INSERT SQL and parameters
- [ ] Copy UPDATE SQL and parameters
- [ ] DELETE is already done (template)
- [ ] Copy any GetBy* methods
- [ ] Copy custom business methods
- [ ] Build and test

Time: 10-15 minutes per repository
```

---

## Phase 4: Migrate Custom Methods (2-3 hours, 5-10 AI calls)

### Step 4.1: Identify Custom Methods

From `Legacy_To_Poco_Mapping.csv`, check `CustomMethodNames` column.

**Example (CustomersTbl.cs):**
- `GetReminderCount()` - Custom business logic
- `GetActiveCustomersForArea(int areaId)` - Custom query
- `UpdateLastReminderDate(int customerId, DateTime date)` - Custom update

### Step 4.2: Categorize Custom Methods

**Simple (Copy-Paste, 0 AI calls):**
- Methods with simple SELECT
- Methods with single WHERE clause
- No complex joins

**Complex (Use AI, 1-2 calls each):**
- Multiple table joins
- Subqueries
- Dynamic SQL
- Complex business logic

### Step 4.3: Migrate Simple Custom Methods

**Pattern:**
```csharp
// OLD (CustomersTbl.cs)
public int GetReminderCount(int customerId)
{
    TrackerDb db = new TrackerDb();
    db.AddParams((object)customerId, DbType.Int32);
    IDataReader reader = db.ExecuteSQLGetDataReader("SELECT ReminderCount FROM CustomersTbl WHERE CustomerID = ?");
    int count = 0;
    if (reader.Read())
        count = Convert.ToInt32(reader["ReminderCount"]);
    reader.Close();
    db.Close();
    return count;
}

// NEW (ContactsRepository.cs)
public int GetReminderCount(int contactId)
{
    string sql = "SELECT ReminderCount FROM ContactsTbl WHERE ContactID = @Id";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = contactId, DataDbType = DbType.Int32, ParamName = "@Id" }
    };

    var result = ExecuteScalar<int>(sql, parameters);
    return result;
}
```

**Time:** 2-3 minutes per method

### Step 4.4: Use AI for Complex Methods

**Only use AI when:**
- Complex joins across 3+ tables
- Subqueries
- Dynamic SQL building
- Unclear business logic

**AI Prompt Template:**
```
Context:
- Migrating from OleDb TrackerDb to SQL Server TrackerSQLDb
- Legacy class: Controls\CustomersTbl.cs
- New repository: Classes\Sql\ContactsRepository.cs
- Table renamed: CustomersTbl -> ContactsTbl

Legacy Method:
[paste full method code]

Please convert to repository pattern using:
- ExecuteQuery<T> for lists
- ExecuteQuerySingle<T> for single object
- ExecuteScalar<T> for single value
- ExecuteNonQuery for updates/deletes
- List<DBParameter> format
- SQL Server table names

Provide:
1. Converted method code
2. Any SQL changes needed
3. Parameter list
```

**Expected AI Calls:** 1-2 per complex method × 5-10 complex methods = 5-20 calls

---

## Phase 5: Update Callers (1-2 hours, 0 AI calls)

### Step 5.1: Find Usage of Legacy Classes

```powershell
# Find all references to legacy CustomersTbl
Get-ChildItem -Path "." -Filter "*.cs" -Recurse | 
    Select-String "new CustomersTbl\(\)" |
    Select-Object Path, LineNumber

# Or use Visual Studio "Find All References"
```

### Step 5.2: Update Each Caller

**Pattern:**
```csharp
// OLD
var customerTbl = new CustomersTbl();
var customer = customerTbl.GetCustomerByID(customerId);
int count = customerTbl.GetReminderCount(customerId);

// NEW
var repo = new ContactsRepository();
var contact = repo.GetById(contactId);
int count = repo.GetReminderCount(contactId);
```

**Time:** 1-2 minutes per file

---

## Phase 6: Testing (1 hour, 0 AI calls)

### Step 6.1: Unit Tests

Create test for each repository:

```csharp
[TestMethod]
public void Test_ContactsRepository_GetById()
{
    var repo = new ContactsRepository();
    var contact = repo.GetById(1);
    Assert.IsNotNull(contact);
    Assert.AreEqual("Expected Name", contact.CompanyName);
}
```

### Step 6.2: Integration Tests

Test key workflows:
- Create contact
- Update contact  
- Delete contact
- Custom business methods

### Step 6.3: Smoke Tests

- [ ] Application starts
- [ ] Login works
- [ ] Main pages load
- [ ] CRUD operations work

---

## Execution Timeline

| Phase | Time | AI Calls | Cumulative |
|---|---|---|---|
| 1. Analysis | 10 min | 0 | 10 min |
| 2. Generate Repos | 30 min | 0 | 40 min |
| 3. Fill CRUD | 2-3 hours | 0 | 3h 40min |
| 4. Custom Methods | 2-3 hours | 5-10 | 6h 40min |
| 5. Update Callers | 1-2 hours | 0 | 8h 40min |
| 6. Testing | 1 hour | 0 | 9h 40min |
| **TOTAL** | **~8-10 hours** | **5-10** | |

**vs. Manual Migration:**
- Time: 40+ hours
- AI Calls: 100+ calls

**Savings:**
- 30+ hours saved
- 90+ AI calls saved

---

## Quick Reference Commands

```powershell
# 1. Analyze
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1

# 2. Generate single repo
.\DevTools\Scripts\Generate-Repository.ps1 -PocoName "Contact"

# 3. Generate all missing repos
$missing = Import-Csv "DevTools\Documentation\Legacy_To_Poco_Mapping.csv" | Where-Object { $_.RepoExists -eq "NO" }
foreach ($item in $missing) { .\DevTools\Scripts\Generate-Repository.ps1 -PocoName $item.PocoName }

# 4. Build
dotnet clean
dotnet build

# 5. Test
dotnet test
```

---

## Success Metrics

| Metric | Target |
|---|---|
| Missing Repos Created | 100% |
| Standard CRUD Complete | 100% |
| Custom Methods Migrated | 100% |
| Build Errors | 0 |
| Test Pass Rate | 100% |
| AI Calls Used | ?10 |
| Time Spent | ?10 hours |

---

## Rollback Plan

```powershell
# Restore from Git
git reset --hard backup-before-migration

# Or restore specific file
git checkout HEAD -- "Classes\Sql\ContactsRepository.cs"
```

---

## Next Steps

1. **Read this plan** (10 minutes)
2. **Run analysis** (2 minutes)
3. **Generate repos** (30 minutes)
4. **Fill CRUD** (follow pattern, minimal AI)
5. **Migrate custom logic** (use AI sparingly)

**Ready to start?**

```powershell
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1
```
