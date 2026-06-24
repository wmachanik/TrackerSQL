# AI Assistant Context & Refactoring Guide for TrackerSQL

## 🔴 CRITICAL: City → Area Refactoring (2026-05-11)

**STATUS:** Complete systematic refactoring across entire codebase
**SCOPE:** All classes, tables, repositories, and SQL queries
**IMPACT:** Breaking change - all legacy "City*" patterns are now deprecated

### What Changed - Complete Reference

#### SQL Server Table Names
| **OLD** | **NEW** | **Location** |
|--------|--------|-------------|
| `CityTbl` | `AreasTbl` | SQL Server database |
| `CityPrepDaysTbl` | `AreaPrepDaysTbl` | SQL Server database |

#### C# Class Names (Controls/)
| **OLD** | **NEW** | **Status** | **Use** |
|--------|--------|-----------|--------|
| `CityTblDAL` | **Deprecated** | DO NOT USE | Use `AreasRepository` instead |
| `CityPrepDaysTbl` | `AreaPrepDaysTbl` | ✅ Updated | Still used as legacy POCO |
| `CityTblData` | `AreaTblData` | ✅ Updated | Data container class |

#### New SQL Repository (Classes/Sql/)
| **Class** | **Purpose** | **Key Methods** |
|----------|-----------|-----------------|
| `AreasRepository` | SQL-based replacement for `CityTblDAL` | `GetPrepRulesForArea(int areaId)` `GetPrepRulesForContact(int contactId)` |

#### New POCO Classes (Classes/Poco/)
| **Class** | **Purpose** | **Properties** |
|----------|-----------|----------------|
| `Area.cs` | Delivery area model | `AreaID`, `AreaName`, `PrepDayOfWeekID`, `DeliveryDelay` |
| `PrepRule.cs` | Prep/delivery rule | `PrepDayOfWeekID`, `DeliveryDelayDays`, `DeliveryOrder` |

### Code Patterns - Before & After

#### Pattern #1: Getting Prep Rules

❌ **LEGACY (Do Not Use)**
````````markdown
SELECT PrepDayOfWeekID, DeliveryDelay, DeliveryOrder 
FROM CityPrepDaysTbl 
WHERE CityID = ?
ORDER BY DeliveryOrder
````````

✔️ **NEW** - Use `AreasRepository`
````````markdown
var rules = areasRepo.GetPrepRulesForArea(areaId);
````````

---

# AI Assistant Context Guide for TrackerSQL

## Purpose of This Document

This document provides **specific guidance for AI assistants** (GitHub Copilot, Claude, ChatGPT, etc.) working on the TrackerSQL project. It contains patterns, conventions, and context that should inform code suggestions and assistance.

---

## Quick Context Injection Prompt

When starting a new AI conversation about this codebase, use this summary:

```
This is a .NET Framework 4.8 ASP.NET Web Forms application being migrated from 
Microsoft Access to SQL Server. The project uses C# 7.3 syntax only. There are 
two components: (1) the main web application (TrackerSQL.csproj) and (2) a 
migration utility (MigrationRunner). The migration involves renaming tables and 
columns (Customer* ? Contact*, Machine* ? Equipment*, etc.). Always check 
Documentation\TABLE_SCHEMA_REFERENCE.md for current column and table names.
```

## MigrationRunner AI Documents

For work in `Migrations\MigrationRunner`, read these first:

- `Documentation\AI\MigrationRunner\README.md`
- `Documentation\AI\MigrationRunner\Migration-Workflow.md`
- `Documentation\AI\MigrationRunner\Migration-Rules.md`

---

## Code Patterns and Conventions

### 1. Database Access Pattern

**Standard Pattern:**
```csharp
public string InsertItem(ItemTbl pItem)
{
    string empty = string.Empty;
    TrackerDb trackerDb = new TrackerDb();
    
    trackerDb.AddParams((object)pItem.ItemID, DbType.Int64);
    trackerDb.AddParams((object)pItem.ItemName, DbType.String);
    trackerDb.AddParams((object)pItem.PrepDate, DbType.Date);
    
    string str = trackerDb.ExecuteNonQuerySQL(
        "INSERT INTO ItemsTbl (ItemID, ItemName, PrepDate) VALUES (?, ?, ?)"
    );
    
    trackerDb.Close();
    return str;
}
```

**Key Points:**
- Always use `TrackerDb` class for database access
- Parameters use positional `?` placeholders
- Always call `trackerDb.Close()`
- Return error string (empty string = success)
- Use `DbType` enumeration for type safety

### 2. Date/Time Handling

**ALWAYS use `TimeZoneUtils.Now()` instead of `DateTime.Now`:**

? Wrong:
```csharp
DateTime today = DateTime.Now;
DateTime defaultDate = DateTime.Now.Date;
```

? Correct:
```csharp
DateTime today = TimeZoneUtils.Now();
DateTime defaultDate = TimeZoneUtils.Now().Date;
```

**Reason:** Application handles multiple time zones for deliveries.

### 3. Logging Pattern

**Standard logging:**
```csharp
AppLogger.WriteLog(
    SystemConstants.LogTypes.Email, 
    $"SentRemindersLogTbl: Deleted {count} entries for {targetDate:yyyy-MM-dd}"
);
```

**Available Log Types (from SystemConstants):**
- `SystemConstants.LogTypes.Email`
- `SystemConstants.LogTypes.SendCheckup`
- `SystemConstants.LogTypes.General`
- `SystemConstants.LogTypes.Error`

**Format:**
- Start with class name or context
- Use structured logging format
- Include relevant data (counts, dates, IDs)
- Use interpolated strings with proper formatting

### 4. Null Handling Pattern

**Reading from DataReader:**
```csharp
// For value types
int id = dataReader["ContactID"] == DBNull.Value 
    ? 0 
    : Convert.ToInt32(dataReader["ContactID"]);

// For DateTime
DateTime date = dataReader["PrepDate"] == DBNull.Value 
    ? TimeZoneUtils.Now().Date 
    : Convert.ToDateTime(dataReader["PrepDate"]).Date;

// For Boolean
bool isActive = dataReader["IsActive"] != DBNull.Value 
    && Convert.ToBoolean(dataReader["IsActive"]);
```

**Key Points:**
- Always check for `DBNull.Value` before converting
- Provide sensible defaults
- For dates, use `.Date` to strip time component
- For booleans, use `!= DBNull.Value &&` pattern

### 5. SQL Query Construction

**Parameterized queries:**
```csharp
// Correct - uses parameters
TrackerDb db = new TrackerDb();
db.AddWhereParams((object)contactId, DbType.Int64);
db.AddWhereParams((object)date.Date, DbType.Date);
string sql = "SELECT * FROM ContactsTbl WHERE ContactID = ? AND PrepDate = ?";
IDataReader reader = db.ExecuteSQLGetDataReader(sql);
```

**Never use string interpolation in SQL:**
? Wrong:
```csharp
string sql = $"SELECT * FROM ContactsTbl WHERE ContactID = {contactId}";
```

---

## Naming Conventions Reference

### Access ? SQL Server Mappings

**Always use the SQL Server names in new/modified code:**

#### Table Names
| Access Name | SQL Server Name | Usage |
|-------------|-----------------|-------|
| `CustomersTbl` | `ContactsTbl` | Customer master data |
| `ClientUsageTbl` | `ContactsItemsPredictedTbl` | Usage predictions |
| `ClientUsageLinesTbl` | `ContactsUsageTbl` | Usage tracking |
| `ItemUsageTbl` | `ContactsItemUsageTbl` | Item usage details |
| `CityTbl` | `AreasTbl` | Delivery areas |
| `ItemTypeTbl` | `ItemsTbl` | Coffee items/products |
| `PackagingTbl` | `ItemPackagingsTbl` | Packaging types |
| `PrepTypesTbl` | `ItemPrepTypesTbl` | Preparation methods |
| `ServiceTypesTbl` | `ItemServiceTypesTbl` | Service types |
| `MachineConditionsTbl` | `EquipConditionsTbl` | Equipment conditions |
| `PersonsTbl` | `PeopleTbl` | People |
| `CustomerTypeTbl` | `ContactTypesTbl` | Contact types |

#### Column Names
| Old Name | New Name | Context |
|----------|----------|---------|
| `CustomerID` | `ContactID` | Primary key for contacts |
| `PrepDate` | `PrepDate` | Preparation date |
| `NextPreperationDate` | `NextPreperationDate` | Next preparation date |
| `MachineSN` | `EquipmentSN` | Equipment serial number |
| `Abreviation` | `Abbreviation` | Spelling correction |
| `PackagingID` | `ItemPackagingID` | Packaging reference |
| `PrepID` | `ItemPrepID` | Prep type reference |

### Terminology Guidelines

**Use these terms consistently:**
- **Contact** (not Customer or Client)
- **Equipment** or **Equip** (not Machine)
- **Prep** (not Roast)
- **Area** (not City)  - did a bulk repalce of City to Area, this is the new standard.
- **Item** (not ItemType)

---

## Common Code Scenarios

### Scenario 1: Creating a New Table Class

**Template:**
```csharp
using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;

namespace TrackerSQL.Controls
{
    public class MyNewTbl
    {
        private const string CONST_SQL_SELECT = "SELECT * FROM MyNewTbl";
        
        // Private fields
        private int _MyID;
        private string _MyName;
        private DateTime _CreatedDate;
        
        // Constructor
        public MyNewTbl()
        {
            this._MyID = 0;
            this._MyName = string.Empty;
            this._CreatedDate = TimeZoneUtils.Now().Date;
        }
        
        // Properties
        public int MyID
        {
            get => this._MyID;
            set => this._MyID = value;
        }
        
        public string MyName
        {
            get => this._MyName;
            set => this._MyName = value;
        }
        
        public DateTime CreatedDate
        {
            get => this._CreatedDate;
            set => this._CreatedDate = value;
        }
        
        // Methods
        public List<MyNewTbl> GetAll(string SortBy)
        {
            List<MyNewTbl> all = new List<MyNewTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = CONST_SQL_SELECT;
            
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
                
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new MyNewTbl()
                    {
                        MyID = dataReader["MyID"] == DBNull.Value 
                            ? 0 
                            : Convert.ToInt32(dataReader["MyID"]),
                        MyName = dataReader["MyName"] == DBNull.Value 
                            ? string.Empty 
                            : dataReader["MyName"].ToString(),
                        CreatedDate = dataReader["CreatedDate"] == DBNull.Value 
                            ? TimeZoneUtils.Now().Date 
                            : Convert.ToDateTime(dataReader["CreatedDate"]).Date
                    });
                    
                dataReader.Close();
            }
            
            trackerDb.Close();
            return all;
        }
    }
}
```

### Scenario 2: Adding a New Query Method

**Pattern:**
```csharp
public List<ContactsTbl> GetActiveContactsForArea(int areaId, DateTime fromDate)
{
    List<ContactsTbl> contacts = new List<ContactsTbl>();
    TrackerDb trackerDb = new TrackerDb();
    
    // Build parameterized query
    string sql = @"SELECT ContactID, ContactName, AreaID, NextPreperationDate 
                   FROM ContactsTbl 
                   WHERE AreaID = ? 
                     AND NextPreperationDate >= ? 
                     AND IsActive = ?
                   ORDER BY NextPreperationDate";
    
    // Add parameters in order
    trackerDb.AddWhereParams((object)areaId, DbType.Int32);
    trackerDb.AddWhereParams((object)fromDate.Date, DbType.Date);
    trackerDb.AddWhereParams((object)true, DbType.Boolean);
    
    IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(sql);
    
    if (dataReader != null)
    {
        while (dataReader.Read())
        {
            contacts.Add(new ContactsTbl()
            {
                ContactID = Convert.ToInt32(dataReader["ContactID"]),
                ContactName = dataReader["ContactName"].ToString(),
                AreaID = Convert.ToInt32(dataReader["AreaID"]),
                NextPreperationDate = Convert.ToDateTime(dataReader["NextPreperationDate"]).Date
            });
        }
        dataReader.Close();
    }
    
    trackerDb.Close();
    
    AppLogger.WriteLog(
        SystemConstants.LogTypes.General,
        $"ContactsTbl: Retrieved {contacts.Count} active contacts for area {areaId}"
    );
    
    return contacts;
}
```

### Scenario 3: Updating Legacy Code References

**When you see Access-era code:**

? Old:
```csharp
public long CustomerID { get; set; }
private DateTime _PrepDate;
string machine = item.MachineSN;
```

? Update to:
```csharp
public long ContactID { get; set; }
private DateTime _PrepDate;
string equipment = item.EquipmentSN;
```

**Search patterns to find legacy code:**
- Search for `Customer` (might need to be `Contact`)
- Search for `Roast` (might need to be `Prep`)
- Search for `Machine` (might need to be `Equipment` or `Equip`)
- Search for `City` (might need to be `Area`)

---

## C# 7.3 Language Constraints

### ? Allowed Features

```csharp
// Expression-bodied members
public int MyProperty => _myField;
public string GetName() => _name;

// Pattern matching (basic)
if (obj is string str)
{
    Console.WriteLine(str);
}

// Tuple syntax
(string name, int age) = GetPerson();
var person = (Name: "John", Age: 30);

// Local functions
void LocalFunction()
{
    // Can access outer scope
}

// Throw expressions
public string Name => _name ?? throw new InvalidOperationException();

// Default literal
int x = default;
string s = default;
```

### ? NOT Allowed (C# 8.0+)

```csharp
// NO nullable reference types
string? nullableString = null;  // ? Not available

// NO switch expressions
var result = value switch { ... };  // ? Not available

// NO indices and ranges
var item = array[^1];  // ? Not available
var slice = array[1..5];  // ? Not available

// NO async streams
IAsyncEnumerable<int> GetNumbers() { ... }  // ? Not available

// NO enhanced pattern matching
if (obj is { Property: value }) { ... }  // ? Not available
```

### Safe Alternatives

Instead of nullable reference types:
```csharp
// Use null checking
if (myString == null)
    throw new ArgumentNullException(nameof(myString));
```

Instead of switch expressions:
```csharp
// Use traditional switch statements
switch (value)
{
    case 1:
        return "One";
    case 2:
        return "Two";
    default:
        return "Other";
}
```

---

## Error Handling Patterns

### Standard Try-Catch Pattern

```csharp
public int DeleteEntriesForDate(DateTime targetDate)
{
    int deletedCount = 0;
    
    try
    {
        // First get count to return
        deletedCount = GetEntriesCountForDate(targetDate);
        
        // Perform operation
        TrackerDb trackerDb = new TrackerDb();
        trackerDb.AddWhereParams((object)targetDate.Date, DbType.Date);
        
        string deleteSql = "DELETE FROM SentRemindersLogTbl WHERE DateSentReminder = ?";
        string result = trackerDb.ExecuteNonQuerySQL(deleteSql);
        
        trackerDb.Close();
        
        // Check for errors
        if (!string.IsNullOrEmpty(result))
        {
            AppLogger.WriteLog(
                SystemConstants.LogTypes.Error,
                $"MyClass: Error in operation: {result}"
            );
            throw new Exception($"Operation failed: {result}");
        }
        
        AppLogger.WriteLog(
            SystemConstants.LogTypes.General,
            $"MyClass: Successfully completed operation for {targetDate:yyyy-MM-dd}"
        );
    }
    catch (Exception ex)
    {
        AppLogger.WriteLog(
            SystemConstants.LogTypes.Error,
            $"MyClass: Exception in operation: {ex.Message}"
        );
        throw new Exception($"Operation failed: {ex.Message}", ex);
    }
    
    return deletedCount;
}
```

**Key Points:**
- Always log errors before throwing
- Include context in error messages
- Re-throw with additional context
- Clean up resources (call `.Close()`)

---

## Testing Patterns

### Manual Testing Checklist

When modifying data access code, verify:

1. **Null Safety**: Test with NULL database values
2. **Empty Results**: Test queries that return no rows
3. **Date Handling**: Test with dates at boundaries (min/max)
4. **Parameter Types**: Verify DbType matches column type
5. **Resource Cleanup**: Ensure `trackerDb.Close()` is always called

### Verification Queries

```sql
-- Check if data exists
SELECT COUNT(*) FROM ContactsTbl;

-- Verify specific record
SELECT * FROM ContactsTbl WHERE ContactID = 123;

-- Check date ranges
SELECT * FROM SentRemindersLogTbl 
WHERE DateSentReminder BETWEEN '2025-01-01' AND '2025-12-31';

-- Verify foreign keys
SELECT c.ContactName, a.AreaName
FROM ContactsTbl c
LEFT JOIN AreasTbl a ON c.AreaID = a.AreaID
WHERE c.AreaID IS NOT NULL;
```

---

## Common Migration Issues

### Issue 1: Column Name Mismatches

**Symptom:** `Invalid column name 'CustomerID'`

**Cause:** Code still uses old Access column names

**Fix:** Update to SQL Server names
```csharp
// Before
int custId = Convert.ToInt32(dataReader["CustomerID"]);

// After
int contactId = Convert.ToInt32(dataReader["ContactID"]);
```

### Issue 2: DateTime Comparison Issues

**Symptom:** Unexpected results when filtering by date

**Cause:** Time component in DateTime comparisons

**Fix:** Always use `.Date` for date-only comparisons
```csharp
// Before
trackerDb.AddWhereParams((object)targetDate, DbType.Date);

// After  
trackerDb.AddWhereParams((object)targetDate.Date, DbType.Date);
```

### Issue 3: Boolean Conversion

**Symptom:** Exception when reading boolean from database

**Cause:** Access uses -1/0, SQL Server uses 1/0

**Fix:** Use proper boolean conversion pattern
```csharp
// Correct pattern
bool isActive = dataReader["IsActive"] != DBNull.Value 
    && Convert.ToBoolean(dataReader["IsActive"]);
```

---

## File Organization Patterns

### Namespace Conventions

```csharp
// Data access classes (table classes)
namespace TrackerSQL.Controls { }

// Business logic
namespace TrackerSQL.Classes { }

// Data models
namespace TrackerSQL.DataSets { }

// Migration tools
namespace MigrationRunner { }
```

### Class Naming

```csharp
// Table classes end with "Tbl"
public class ContactsTbl { }
public class OrderLinesTbl { }

// Utility classes are descriptive
public class TimeZoneUtils { }
public class AppLogger { }

// Constants class
public class SystemConstants { }
```

---

## SQL Server Specific Considerations

### Identity Columns

**When inserting with IDENTITY:**
```sql
SET IDENTITY_INSERT ContactsTbl ON;
INSERT INTO ContactsTbl (ContactID, ContactName) VALUES (1, 'Test');
SET IDENTITY_INSERT ContactsTbl OFF;
```

**Let SQL Server generate ID:**
```sql
INSERT INTO ContactsTbl (ContactName) VALUES ('Test');
SELECT SCOPE_IDENTITY();  -- Get generated ID
```

### Date Types

**SQL Server date types used:**
- `DATETIME` - Access compatibility
- `DATETIME2` - Higher precision (preferred for new columns)
- `DATE` - Date only (no time)

**Always use `.Date` in C# when comparing:**
```csharp
DateTime compareDate = someDate.Date;  // Strip time component
```

---

## Resources and References

### Key Documentation Files

1. `Documentation\PROJECT_OVERVIEW.md` - High-level project context
2. `Documentation\TABLE_SCHEMA_REFERENCE.md` - Complete schema mappings
3. `Documentation\MIGRATION_GUIDE.md` - Migration procedures
4. `Migrations\Data\TableMigrationReport-10-Mar-26.xlsx` - Master migration data
5. `Data\Metadata\README.md` - Naming conventions

### When in Doubt

1. **Check existing code patterns** in `Controls\*Tbl.cs` files
2. **Reference migration Excel** for table/column names
3. **Use TimeZoneUtils.Now()** for all date/time operations
4. **Log extensively** using `AppLogger`
5. **Ask user for clarification** on business logic

---

## AI Assistant Best Practices

### Before Suggesting Code

? **DO:**
- Check TABLE_SCHEMA_REFERENCE.md for correct column names
- Verify C# 7.3 compatibility
- Use existing patterns from Controls\ folder
- Include error handling and logging
- Test for null safety

? **DON'T:**
- Assume Access-era column names
- Use C# 8+ features
- Skip error handling
- Forget to close database connections
- Use DateTime.Now (use TimeZoneUtils.Now())

### When Uncertain

If you're unsure about:
- **Column names** ? Reference `TABLE_SCHEMA_REFERENCE.md`
- **Business logic** ? Ask the user
- **Existing patterns** ? Search `Controls\` folder
- **SQL syntax** ? Check generated scripts in `Data\Metadata\PlanEdits\Sql\`

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-03-26 | AI Documentation Initiative | Initial creation |

---

**Remember:** This is a migration-in-progress. Always verify table and column names against the latest documentation, and maintain backward compatibility where possible.
