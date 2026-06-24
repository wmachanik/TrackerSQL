# QUICK START: Minimal-AI Repository Migration

**Time:** 8-10 hours  
**AI Calls:** 5-10 (vs. 100+)

---

## ?? IMPORTANT: Naming Conventions

**The script automatically handles these renames from the migration CSV:**

- `Customer*` ? `Contact*`
- `City*` ? `Area*`
- `ItemType*` ? `Item*`
- `Machine*` ? `Equipment*`/`Equip*`
- `Reoccur*` ? `Recurr*`
- `RoastDate` ? `PrepDate`

**See:** `DevTools/Documentation/Naming_Convention_Reference.md` for complete list.

---

## TL;DR

```powershell
# 1. Analyze (2 min)
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1

# 2. Review CSV
code DevTools\Documentation\Legacy_To_Poco_Mapping.csv

# 3. Generate missing repos (30 min)
# See CSV for which ones are missing

# 4. Fill in CRUD by copy-paste from legacy classes (2-3 hours)
# 5. Copy custom methods (2-3 hours, use AI for complex only)
# 6. Update callers (1-2 hours)
# 7. Test (1 hour)
```

---

## The Problem

? **Old Approach Failed:**
- Automated scripts can't handle database schema changes
- Too many AI calls needed for conversions

? **New Approach:**
- Use existing POCOs
- Generate repository templates (0 AI)
- Copy-paste CRUD from legacy classes (0 AI)
- Use AI only for complex custom logic (5-10 calls)

---

## Step-by-Step

### Step 1: Analyze (2 minutes, 0 AI)

```powershell
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1
```

**Output:** CSV showing:
- Which POCOs exist ?
- Which repos exist ?  
- Which repos are MISSING ?
- Custom methods in legacy classes

### Step 2: Generate Missing Repos (30 minutes, 0 AI)

```powershell
# For each missing repo in CSV:
.\DevTools\Scripts\Generate-Repository.ps1 -PocoName "Contact"
.\DevTools\Scripts\Generate-Repository.ps1 -PocoName "ContactsItemUsage"
# etc.

# Or batch generate:
$missing = Import-Csv "DevTools\Documentation\Legacy_To_Poco_Mapping.csv" | Where-Object { $_.RepoExists -eq "NO" }
foreach ($item in $missing) { 
    .\DevTools\Scripts\Generate-Repository.ps1 -PocoName $item.PocoName 
}
```

**Creates:** Repository templates with TODOs

### Step 3: Fill CRUD (2-3 hours, 0 AI)

For each repository:

1. **Open legacy class** (e.g., `Controls\CustomersTbl.cs`)
2. **Open new repo** (e.g., `Classes\Sql\ContactsRepository.cs`)
3. **Copy INSERT:**
   - Find `INSERT` method in legacy
   - Copy SQL string
   - Copy parameter list
   - Paste into repo template
   - Convert `?` ? `@ParamName`
   - Convert `AddParams` ? `DBParameter`
4. **Copy UPDATE:** Same process
5. **DELETE:** Already done in template

**Example:**

```csharp
// LEGACY (CustomersTbl.cs)
trackerDb.AddParams((object)customer.CompanyName, DbType.String);
string sql = "INSERT INTO CustomersTbl (CompanyName) VALUES (?)";

// NEW (ContactsRepository.cs)
var parameters = new List<DBParameter>
{
    new DBParameter { DataValue = entity.CompanyName, DataDbType = DbType.String, ParamName = "@CompanyName" }
};
string sql = "INSERT INTO ContactsTbl (CompanyName) VALUES (@CompanyName)";
```

**Time:** 10-15 min per repository

### Step 4: Copy Custom Methods (2-3 hours, 5-10 AI)

**Simple methods (copy-paste, 0 AI):**
```csharp
// Simple SELECT - just copy pattern
public int GetReminderCount(int contactId)
{
    string sql = "SELECT ReminderCount FROM ContactsTbl WHERE ContactID = @Id";
    var p = new List<DBParameter> { new DBParameter { DataValue = contactId, DataDbType = DbType.Int32, ParamName = "@Id" } };
    return ExecuteScalar<int>(sql, p);
}
```

**Complex methods (use AI, 1-2 calls each):**
- Multiple table joins
- Subqueries
- Dynamic SQL

**AI Prompt:**
```
Migrate this method from OleDb to SQL Server repository pattern:

[paste legacy method]

Convert to use:
- ExecuteQuery<T> for lists
- ExecuteQuerySingle<T> for single
- ExecuteScalar<T> for value
- List<DBParameter> format
```

**Expected:** 5-10 complex methods across all repos

### Step 5: Update Callers (1-2 hours, 0 AI)

```csharp
// Find all:
new CustomersTbl()

// Replace with:
new ContactsRepository()

// Update method calls to match new names
```

### Step 6: Test (1 hour, 0 AI)

```powershell
dotnet clean
dotnet build
dotnet test
```

Manual testing:
- Login
- CRUD operations
- Custom methods

---

## File Locations

**Scripts:**
- `DevTools\Scripts\Analyze-Legacy-Classes.ps1` ? Run this first
- `DevTools\Scripts\Generate-Repository.ps1` ? Generate repos

**Documentation:**
- `DevTools\Documentation\Minimal_AI_Repository_Migration_Plan.md` ? Full plan
- `DevTools\Documentation\Legacy_To_Poco_Mapping.csv` ? Generated analysis

**Code:**
- `Controls\*Tbl.cs` - Legacy classes (read from these)
- `Classes\Poco\*.cs` - POCO models (already exist)
- `Classes\Sql\*Repository.cs` - New repos (write to these)

---

## Success Criteria

- ? All missing repos created
- ? Standard CRUD complete
- ? Custom methods migrated
- ? Build succeeds (0 errors)
- ? Tests pass
- ? ?10 AI calls used

---

## Common Patterns

### Pattern 1: Convert AddParams

```csharp
// OLD
db.AddParams((object)value, DbType.Int32);

// NEW
new DBParameter { DataValue = value, DataDbType = DbType.Int32, ParamName = "@ParamName" }
```

### Pattern 2: Convert ExecuteNonQuerySQL

```csharp
// OLD
string result = db.ExecuteNonQuerySQL("INSERT INTO...");

// NEW
int result = ExecuteNonQuery(sql, parameters);
```

### Pattern 3: Convert ExecuteSQLGetDataReader

```csharp
// OLD
IDataReader reader = db.ExecuteSQLGetDataReader("SELECT...");
while (reader.Read()) { ... }

// NEW
var list = ExecuteQuery<T>(sql, parameters);
// Or for single:
var item = ExecuteQuerySingle<T>(sql, parameters);
```

---

## Next Steps

1. **Read full plan:** `DevTools\Documentation\Minimal_AI_Repository_Migration_Plan.md`
2. **Run analysis:** `.\DevTools\Scripts\Analyze-Legacy-Classes.ps1`
3. **Review CSV:** See what needs creating
4. **Generate repos:** Use script
5. **Fill CRUD:** Copy-paste pattern
6. **Migrate custom:** Use AI sparingly

**Ready?**

```powershell
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1
```
