# TrackerSQL Migration Guide

## Document Purpose

This guide documents the complete migration strategy from Microsoft Access to SQL Server for the TrackerSQL application. It serves as a reference for developers and AI assistants working on the migration.

---

## Table of Contents

1. [Migration Overview](#migration-overview)
2. [Migration Types](#migration-types)
3. [Migration Process](#migration-process)
4. [Tools and Scripts](#tools-and-scripts)
5. [Naming Convention Changes](#naming-convention-changes)
6. [Data Type Mappings](#data-type-mappings)
7. [Post-Migration Code Updates](#post-migration-code-updates)
8. [Verification Procedures](#verification-procedures)
9. [Rollback Procedures](#rollback-procedures)

---

## Migration Overview

### Business Context

**What:** Migrating TrackerSQL from Microsoft Access backend to SQL Server  
**Why:**  
- Access has scalability and concurrency limitations
- 32-bit OleDb dependencies are problematic for modern deployment
- Need better performance for growing dataset
- Prepare for cloud deployment possibilities

**Scope:**
- 45+ tables
- ~3,000 customer records
- ~300 items
- Historical data going back several years
- Active web application with daily use

### Migration Approach

**Strategy:** Parallel migration with gradual cutover

1. **Schema Migration** (In Progress)
   - Extract Access schema
   - Generate SQL Server DDL
   - Apply schema with improvements

2. **Data Migration** (In Progress)
   - Generate data migration scripts
   - Preserve identity values
   - Validate foreign key integrity

3. **Code Migration** (In Progress)
   - Update table/column references
   - Replace OleDb with SqlClient
   - Refactor data access patterns

4. **Testing** (Ongoing)
   - Page-by-page verification
   - Performance testing
   - User acceptance testing

---

## Migration Types

The migration uses three distinct patterns based on the complexity of changes needed.

### Type 1: Direct Table Copy

**Description:** Source table copied directly with minimal changes

**Characteristics:**
- Same table name in source and target
- Column names may change
- Some columns may be dropped
- Data types converted to SQL Server equivalents

**Example: SysDataTbl**
```
Access: SysDataTbl
  - SysDataID (AutoNumber)
  - CompanyName (Text)
  - LastBackupDate (Date/Time)
  
SQL Server: SysDataTbl
  - SysDataID (INT IDENTITY)
  - CompanyName (NVARCHAR(255))
  - LastBackupDate (DATETIME2)
```

**Other Examples:**
- `OrdersTbl` (with some column renames)
- `PeopleTbl`
- `ItemsTbl` (formerly ItemTypeTbl)
- `SentRemindersLogTbl`

### Type 2: Table Rename

**Description:** Source table copied to differently-named target table

**Characteristics:**
- Reflects terminology standardization
- Column names also updated
- Relationships updated to use new names
- Code requires updates to reference new names

**Example: CustomersTbl ? ContactsTbl**
```
Access: CustomersTbl
  - CustomerID (AutoNumber)
  - CustomerName (Text)
  - CityID (Long Integer)
  - PrepDate (Date/Time)
  
SQL Server: ContactsTbl
  - ContactID (INT IDENTITY)
  - ContactName (NVARCHAR(255))
  - AreaID (INT)
  - PrepDate (DATETIME2)
```

**Major Renames:**
- `CustomersTbl` ? `ContactsTbl`
- `ClientUsageTbl` ? `ContactsItemsPredictedTbl`
- `ClientUsageLinesTbl` ? `ContactsUsageTbl`
- `ItemUsageTbl` ? `ContactsItemUsageTbl`
- `CityTbl` ? `AreasTbl`
- `PackagingTbl` ? `ItemPackagingsTbl`
- `PrepTypesTbl` ? `ItemPrepTypesTbl`
- `MachineConditionsTbl` ? `EquipConditionsTbl`

### Type 3: Table Normalization

**Description:** Source table split into multiple related tables

**Characteristics:**
- Improves relational integrity
- Eliminates data redundancy
- Adds proper foreign key relationships
- Requires complex migration logic

**Example: Orders Normalization**
```
Access: OrdersTbl (flat structure)
  - OrderID
  - CustomerID
  - OrderDate
  - ItemTypeID          } Repeated
  - Quantity            } for each
  - UnitPrice           } line item
  - ItemTypeID2         }
  - Quantity2           }
  - UnitPrice2          }
  
SQL Server: OrdersTbl + OrderLinesTbl
  OrdersTbl:
    - OrderID (PK)
    - ContactID (FK)
    - OrderDate
    
  OrderLinesTbl:
    - OrderLineID (PK)
    - OrderID (FK)
    - ItemID (FK)
    - Quantity
    - UnitPrice
```

**Benefits:**
- Unlimited line items per order
- No NULL columns for unused item slots
- Easier to query and report
- Better data integrity

**Other Examples:**
- Contact usage tracking (split into predicted vs actual)
- Equipment tracking tables

---

## Migration Process

### Phase 1: Schema Extraction (Completed ?)

**Tool:** `MigrationRunner` - `AccessSchemaReader.cs`

**Process:**
1. Connect to Access database via OleDb
2. Read table schemas using `GetOleDbSchemaTable`
3. Extract column definitions, data types, constraints
4. Save to JSON metadata files

**Output:**
- `Migrations\MigrationRunner\Metadata\AccessSchema.json`
- Table definitions with columns, types, keys

**Command:**
```cmd
cd Migrations\MigrationRunner
dotnet run
# Select option 1: Export Access Schema
```

### Phase 2: Bulk Renaming (Completed ?)

**Tool:** `MigrationRunner` - Bulk rename functionality

**Configuration:** `Data\Metadata\BulkRenameRules.json`

**Rules Applied:**
```json
{
  "TableRenames": {
    "CustomersTbl": "ContactsTbl",
    "CityTbl": "AreasTbl",
    "PackagingTbl": "ItemPackagingsTbl"
  },
  "ColumnRenames": {
    "CustomerID": "ContactID",
    "PrepDate": "PrepDate",
    "MachineSN": "EquipmentSN"
  }
}
```

**Process:**
1. Load Access schema
2. Apply rename rules
3. Update foreign key references
4. Save updated metadata

**Command:**
```cmd
dotnet run
# Select option 2: Apply Bulk Renames
```

### Phase 3: DDL Generation (Completed ?)

**Tool:** `MigrationRunner` - `DdlScriptGenerator.cs`

**Process:**
1. Read metadata (post-rename)
2. Convert Access data types to SQL Server types
3. Generate CREATE TABLE statements
4. Include primary keys and identity columns
5. Add column constraints (NOT NULL, defaults)

**Output:** `Data\Metadata\PlanEdits\Sql\CreateTables_LATEST.sql`

**Key Conversions:**
- `AutoNumber` ? `INT IDENTITY(1,1)`
- `Text(n)` ? `NVARCHAR(n)`
- `Memo` ? `NVARCHAR(MAX)`
- `Date/Time` ? `DATETIME2` or `DATETIME`
- `Yes/No` ? `BIT`

**Command:**
```cmd
dotnet run
# Select option 3: Generate DDL Scripts
```

### Phase 4: DML Generation (Completed ?)

**Tool:** `MigrationRunner` - `DmlScriptGenerator.cs`

**Process:**
1. Connect to source Access database
2. For each table, generate INSERT statements
3. Map Access columns to SQL Server columns
4. Handle data type conversions
5. Preserve identity values (SET IDENTITY_INSERT ON)
6. Detect orphaned foreign key references

**Output:** `Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql`

**Special Handling:**
- NULL values properly handled with `NULLIF`
- Boolean conversion (-1/0 to 1/0)
- Date formatting for SQL Server
- String escaping for single quotes
- Identity preservation

**Command:**
```cmd
dotnet run
# Select option 4: Generate DML Scripts
```

### Phase 5: Foreign Key Generation (Completed ?)

**Tool:** `MigrationRunner` - `FkScriptGenerator.cs`

**Configuration:** `Data\Metadata\PlanEdits\PlanConstraints.json`

**Process:**
1. Read foreign key relationships from metadata
2. Verify referenced tables exist
3. Generate ALTER TABLE ADD CONSTRAINT statements
4. Include ON DELETE/UPDATE rules

**Output:** `Data\Metadata\PlanEdits\Sql\AddForeignKeys_LATEST.sql`

**Example:**
```sql
ALTER TABLE ContactsTbl 
ADD CONSTRAINT FK_ContactsTbl_AreasTbl 
FOREIGN KEY (AreaID) REFERENCES AreasTbl(AreaID)
ON DELETE NO ACTION 
ON UPDATE NO ACTION;
```

**Command:**
```cmd
dotnet run
# Select option 5: Generate FK Scripts
```

### Phase 6: Database Creation (Completed ?)

**Process:**
1. Create new SQL Server database
2. Execute `CreateTables_LATEST.sql`
3. Verify table creation
4. Check indexes and primary keys

**Verification:**
```sql
-- Check tables created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Should show ~45 tables
```

### Phase 7: Data Migration Execution (Completed ?)

**Process:**
1. Backup Access database (safety)
2. Execute `DataMigration_LATEST.sql`
3. Monitor for errors
4. Verify row counts

**Verification:**
```sql
-- Compare counts
SELECT 'ContactsTbl' AS TableName, COUNT(*) AS RowCount FROM ContactsTbl
UNION ALL
SELECT 'ItemsTbl', COUNT(*) FROM ItemsTbl
UNION ALL
SELECT 'OrdersTbl', COUNT(*) FROM OrdersTbl
UNION ALL
SELECT 'OrderLinesTbl', COUNT(*) FROM OrderLinesTbl;

-- Expected results:
-- ContactsTbl: ~2,900
-- ItemsTbl: ~279
-- OrdersTbl: ~varies
-- OrderLinesTbl: ~varies
```

### Phase 8: Foreign Key Creation (Completed ?)

**Process:**
1. Verify all data migrated successfully
2. Execute `AddForeignKeys_LATEST.sql`
3. Check for constraint violations

**Verification:**
```sql
-- Check foreign keys
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
ORDER BY TableName, ForeignKeyName;
```

### Phase 9: Code Migration (In Progress ??)

**Approach:** Incremental refactoring

**Steps:**
1. Identify files using old table/column names
2. Update references to new names
3. Test functionality
4. Commit changes

**Search Patterns:**
```powershell
# Find Customer references
Get-ChildItem -Recurse -Include *.cs | Select-String "CustomerID"

# Find Machine references  
Get-ChildItem -Recurse -Include *.cs | Select-String "MachineSN"

# Find Roast references
Get-ChildItem -Recurse -Include *.cs | Select-String "PrepDate"
```

**Update Pattern:**
```csharp
// Before
public long CustomerID { get; set; }
private DateTime _PrepDate;

// After  
public long ContactID { get; set; }
private DateTime _PrepDate;
```

### Phase 10: Connection String Update (Pending ?)

**Current (Access):**
```xml
<connectionStrings>
  <add name="TrackerDB" 
       connectionString="Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Data\Tracker.accdb;" 
       providerName="System.Data.OleDb"/>
</connectionStrings>
```

**Target (SQL Server):**
```xml
<connectionStrings>
  <add name="TrackerDB" 
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=TrackerSQL;Integrated Security=True;" 
       providerName="System.Data.SqlClient"/>
</connectionStrings>
```

**Update Location:** `Web.config`

---

## Tools and Scripts

### MigrationRunner Tool

**Location:** `Migrations\MigrationRunner\`

**Purpose:** Automate schema extraction and script generation

**Main Components:**
- `Program.cs` - Interactive menu
- `AccessSchemaReader.cs` - Reads Access schema
- `DdlScriptGenerator.cs` - Generates CREATE TABLE scripts
- `DmlScriptGenerator.cs` - Generates INSERT scripts
- `FkScriptGenerator.cs` - Generates foreign key scripts

**Usage:**
```cmd
cd Migrations\MigrationRunner
dotnet build
dotnet run

# Menu options:
# 1. Export Access Schema
# 2. Apply Bulk Renames
# 3. Generate DDL Scripts
# 4. Generate DML Scripts
# 5. Generate FK Scripts
# $. Full Pipeline (all steps)
```

### Generated SQL Scripts

**Location:** `Data\Metadata\PlanEdits\Sql\`

**Scripts:**
1. **CreateTables_LATEST.sql**
   - Creates all tables
   - ~1,500 lines
   - Includes primary keys and identity columns

2. **DataMigration_LATEST.sql**
   - Migrates all data
   - ~50,000+ lines
   - Preserves identity values

3. **AddForeignKeys_LATEST.sql**
   - Creates foreign key constraints
   - ~500 lines
   - Defines referential integrity

**Execution Order:**
```sql
-- 1. Create schema
:r CreateTables_LATEST.sql
GO

-- 2. Migrate data
:r DataMigration_LATEST.sql
GO

-- 3. Add constraints
:r AddForeignKeys_LATEST.sql
GO
```

### Configuration Files

**Metadata Files:**

1. **BulkRenameRules.json**
   ```json
   {
     "TableRenames": {
       "OldName": "NewName"
     },
     "ColumnRenames": {
       "OldColumn": "NewColumn"
     }
   }
   ```

2. **PlanColumns.csv**
   - Column-level migration plan
   - Includes: TableName, ColumnName, DataType, IsNullable, IsPrimaryKey

3. **PlanConstraints.json**
   - Primary key definitions
   - Foreign key relationships
   - Check constraints

**Master Reference:**
- `Migrations\Data\TableMigrationReport-10-Mar-26.xlsx`
- Complete migration plan in spreadsheet format

---

## Naming Convention Changes

### Table Naming Conventions

**Pattern:** Standardize terminology across system

| Category | Access Pattern | SQL Server Pattern | Examples |
|----------|----------------|-------------------|----------|
| Customers/Clients | Customer*, Client* | Contact* | ContactsTbl, ContactTypesTbl |
| Location | City* | Area* | AreasTbl |
| Coffee Processing | Roast* | Prep* | PrepDate, NextPreperationDate |
| Equipment | Machine* | Equipment*, Equip* | EquipmentSN, EquipConditionsTbl |
| Items | ItemType* | Item* | ItemsTbl, ItemID |
| Supporting Tables | Various | Item[Type]* | ItemPackagingsTbl, ItemPrepTypesTbl |

### Column Naming Conventions

**Primary Keys:**
- Pattern: `[TableName]ID`
- Example: `ContactsTbl.ContactID`
- Type: `INT IDENTITY(1,1)`

**Foreign Keys:**
- Pattern: Same as referenced PK
- Example: `OrdersTbl.ContactID` references `ContactsTbl.ContactID`
- Type: Matches referenced column type

**Date Columns:**
- Pattern: Descriptive name + Date
- Examples: `PrepDate`, `NextPreperationDate`, `OrderDate`, `DateSentReminder`
- Type: `DATETIME2` (preferred) or `DATETIME`

**Boolean Columns:**
- Pattern: Is*, Has*, [Adjective]
- Examples: `IsActive`, `ReminderSent`, `HadAutoFulfilItem`
- Type: `BIT`

### Complete Rename Mapping

See `Documentation\TABLE_SCHEMA_REFERENCE.md` for complete table-by-table mapping.

---

## Data Type Mappings

### Access to SQL Server Type Conversion

| Access Type | SQL Server Type | Notes |
|-------------|----------------|-------|
| **AutoNumber** | `INT IDENTITY(1,1)` | Primary keys |
| **Text(50)** | `NVARCHAR(50)` | Unicode support |
| **Text(255)** | `NVARCHAR(255)` | Default text field |
| **Memo** | `NVARCHAR(MAX)` | Large text |
| **Number (Long Integer)** | `INT` | 32-bit integer |
| **Number (Integer)** | `SMALLINT` | 16-bit integer |
| **Number (Byte)** | `TINYINT` | 8-bit integer |
| **Number (Double)** | `FLOAT` | Floating point |
| **Number (Decimal)** | `DECIMAL(p,s)` | Fixed precision |
| **Date/Time** | `DATETIME2` or `DATETIME` | Datetime with/without timezone |
| **Yes/No** | `BIT` | Boolean |
| **Currency** | `DECIMAL(19,4)` | Money values |
| **OLE Object** | `VARBINARY(MAX)` | Binary data |

### Special Conversions

**Boolean Values:**
- Access: Yes = -1, No = 0
- SQL Server: True = 1, False = 0
- Conversion: `CASE WHEN [Field] = -1 THEN 1 ELSE 0 END`

**Null Handling:**
- Access: Empty strings often used instead of NULL
- SQL Server: Use proper NULL values
- Conversion: `NULLIF([Field], '')` for text columns

**Date Handling:**
- Access: Date/Time stored with time component
- SQL Server: Use `.Date` property in C# for date-only comparisons
- C# Pattern: `targetDate.Date` when filtering

---

## Post-Migration Code Updates

### Code Update Checklist

#### 1. Update Table Class Names

**Pattern:**
```csharp
// Old
using TrackerSQL.Controls;
CustomersTbl customer = new CustomersTbl();

// New
using TrackerSQL.Controls;
ContactsTbl contact = new ContactsTbl();
```

**Files to Update:**
- All files in `Controls\` folder
- References in `Pages\` folder
- Usage in `Classes\` folder

#### 2. Update Property Names

**Pattern:**
```csharp
// Old
public long CustomerID { get; set; }
public DateTime PrepDate { get; set; }
public string MachineSN { get; set; }

// New
public long ContactID { get; set; }
public DateTime PrepDate { get; set; }
public string EquipmentSN { get; set; }
```

#### 3. Update SQL Queries

**Pattern:**
```csharp
// Old
string sql = "SELECT CustomerID, CustomerName, PrepDate FROM CustomersTbl";

// New
string sql = "SELECT ContactID, ContactName, PrepDate FROM ContactsTbl";
```

#### 4. Update DataReader Column References

**Pattern:**
```csharp
// Old
int custId = Convert.ToInt32(dataReader["CustomerID"]);
DateTime roast = Convert.ToDateTime(dataReader["PrepDate"]);

// New
int contactId = Convert.ToInt32(dataReader["ContactID"]);
DateTime prep = Convert.ToDateTime(dataReader["PrepDate"]);
```

#### 5. Update Foreign Key References

**Pattern:**
```csharp
// Old
order.CustomerID = customer.CustomerID;
item.MachineID = machine.MachineID;

// New
order.ContactID = contact.ContactID;
item.EquipmentID = equipment.EquipmentID;
```

### Testing After Updates

**For Each Modified File:**

1. **Compile Check:**
   ```cmd
   msbuild TrackerSQL.csproj
   ```

2. **Page Load Test:**
   - Navigate to page in browser
   - Verify no errors
   - Check data displays correctly

3. **Functional Test:**
   - Test CRUD operations
   - Verify data saves correctly
   - Check filtering and sorting

4. **Data Integrity:**
   ```sql
   -- Verify FK integrity
   SELECT * FROM ContactsTbl WHERE ContactID NOT IN (SELECT ContactID FROM OrdersTbl);
   ```

---

## Verification Procedures

### Database Verification

**1. Table Count Verification:**
```sql
SELECT COUNT(*) AS TableCount
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
-- Expected: ~45 tables
```

**2. Row Count Verification:**
```sql
-- Create temp table for results
CREATE TABLE #RowCounts (TableName NVARCHAR(255), RowCount INT);

-- Insert counts for all tables
INSERT INTO #RowCounts
EXEC sp_MSForEachTable 'SELECT ''?'' AS TableName, COUNT(*) AS RowCount FROM ?';

-- Display results
SELECT * FROM #RowCounts ORDER BY TableName;

-- Cleanup
DROP TABLE #RowCounts;
```

**3. Primary Key Verification:**
```sql
SELECT 
    TABLE_NAME,
    CONSTRAINT_NAME,
    COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE CONSTRAINT_NAME LIKE 'PK_%'
ORDER BY TABLE_NAME;
```

**4. Foreign Key Verification:**
```sql
SELECT 
    OBJECT_NAME(f.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(f.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc 
    ON f.object_id = fc.constraint_object_id
ORDER BY TableName, ColumnName;
```

**5. Orphaned Record Detection:**
```sql
-- Example: Check for orphaned orders
SELECT o.OrderID, o.ContactID
FROM OrdersTbl o
LEFT JOIN ContactsTbl c ON o.ContactID = c.ContactID
WHERE c.ContactID IS NULL;
-- Should return 0 rows
```

### Application Verification

**1. Connection Test:**
```csharp
TrackerDb db = new TrackerDb();
bool connected = db.TestConnection();
db.Close();
// Should return true
```

**2. Basic CRUD Test:**
```csharp
// Create
SysDataTbl sys = new SysDataTbl();
sys.CompanyName = "Test Company";
string result = sys.InsertSysData(sys);
// result should be empty string (success)

// Read
List<SysDataTbl> all = sys.GetAll("");
// Should contain test record

// Update
sys.CompanyName = "Updated Company";
result = sys.UpdateSysData(sys);
// result should be empty string

// Delete (if supported)
result = sys.DeleteSysData(sys.SysDataID);
```

**3. Page Load Tests:**
- Test each .aspx page
- Verify data grids populate
- Check no JavaScript errors
- Verify forms submit correctly

---

## Rollback Procedures

### Emergency Rollback

If critical issues discovered after migration:

**1. Web Application Rollback:**
```xml
<!-- Web.config: Switch back to Access -->
<connectionStrings>
  <add name="TrackerDB" 
       connectionString="Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Data\Tracker.accdb;" 
       providerName="System.Data.OleDb"/>
</connectionStrings>
```

**2. Code Rollback:**
```cmd
git revert [commit-hash]
git push
```

**3. IIS Reset:**
```cmd
iisreset
```

### Data Rollback

If SQL Server data corrupted:

**1. Drop Database:**
```sql
USE master;
GO
DROP DATABASE TrackerSQL;
GO
```

**2. Restore from Backup:**
```sql
RESTORE DATABASE TrackerSQL
FROM DISK = 'C:\Backups\TrackerSQL_YYYYMMDD.bak'
WITH REPLACE;
GO
```

### Planned Rollback

If migration needs to be postponed:

**1. Document Current State:**
- Note which tables migrated
- List which pages updated
- Record any issues encountered

**2. Preserve SQL Server Database:**
- Keep database for reference
- Don't delete migration scripts
- Maintain metadata files

**3. Continue Using Access:**
- Keep Access database as production
- Use SQL Server for testing
- Plan next migration window

---

## Known Issues and Solutions

### Issue 1: DateTime Conversion Errors (RESOLVED)

**Problem:**
```
Cannot convert nvarchar to datetime
```

**Cause:** Using `NULLIF` on datetime columns

**Solution:** 
```sql
-- Before (broken)
NULLIF([PrepDate], N'') AS PrepDate

-- After (fixed)
[PrepDate] AS PrepDate
```

**Status:** Fixed in `DmlScriptGenerator.cs`

### Issue 2: Boolean Conversion

**Problem:** Access booleans (-1/0) don't match SQL Server (1/0)

**Solution:** Conversion in migration script:
```sql
CASE WHEN [IsActive] = -1 THEN 1 ELSE 0 END AS IsActive
```

**Status:** Handled in `DmlScriptGenerator.cs`

### Issue 3: Identity Column Gaps

**Problem:** Auto-increment values have gaps after migration

**Cause:** Deleted records in Access database

**Solution:** Not an issue - gaps are normal and acceptable

**Status:** Accepted behavior

### Issue 4: Unicode Characters

**Problem:** Special characters in customer names corrupted

**Solution:** Use NVARCHAR instead of VARCHAR in SQL Server

**Status:** Implemented in DDL scripts

---

## Best Practices

### During Migration

1. **Always Backup**
   - Backup Access database before each migration attempt
   - Backup SQL Server database before major changes
   - Keep backups for at least 30 days

2. **Test in Isolation**
   - Use separate test database
   - Don't test in production
   - Verify before applying to production

3. **Verify Counts**
   - Compare row counts after migration
   - Check for orphaned records
   - Validate foreign key relationships

4. **Document Changes**
   - Note any manual corrections
   - Document issues encountered
   - Update migration scripts with fixes

### After Migration

1. **Monitor Performance**
   - Check page load times
   - Monitor SQL Server resource usage
   - Optimize slow queries

2. **Gradual Rollout**
   - Test with small user group first
   - Monitor for errors
   - Have rollback plan ready

3. **Keep Access Available**
   - Don't delete Access database immediately
   - Keep for reference for 30-90 days
   - Archive when stable

---

## Timeline and Milestones

### Completed Milestones

- ? Schema extraction from Access (Oct 2024)
- ? Bulk rename rules defined (Oct 2024)
- ? DDL script generation (Nov 2024)
- ? DML script generation (Nov 2024)
- ? FK script generation (Nov 2024)
- ? SQL Server database created (Dec 2024)
- ? Data migrated to SQL Server (Dec 2024)
- ? Foreign keys established (Jan 2025)
- ? DateTime conversion bug fixed (Mar 2025)

### Current Phase

?? **Code Migration and Testing** (Mar 2025)

### Remaining Milestones

- ? Update all table class files
- ? Update web pages to use new names
- ? Complete regression testing
- ? Update connection string
- ? Production cutover
- ? Access database retirement

---

## Support and Resources

### Documentation Files

- `Documentation\PROJECT_OVERVIEW.md` - Project context
- `Documentation\AI_CONTEXT.md` - AI assistance guidelines
- `Documentation\TABLE_SCHEMA_REFERENCE.md` - Complete schema
- `Migrations\README_MIGRATION.md` - Quick start guide
- `Data\Metadata\README.md` - Naming conventions

### Reference Spreadsheets

- `Migrations\Data\TableMigrationReport-10-Mar-26.xlsx` - Master plan
- Contains: Table mappings, column mappings, data types

### SQL Scripts

- `Data\Metadata\PlanEdits\Sql\CreateTables_LATEST.sql`
- `Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql`
- `Data\Metadata\PlanEdits\Sql\AddForeignKeys_LATEST.sql`

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial comprehensive migration guide created |

---

**For Questions or Issues:**
- Review this document first
- Check `AI_CONTEXT.md` for code patterns
- Reference `TABLE_SCHEMA_REFERENCE.md` for schema details
- Consult Excel spreadsheet for specific table/column mappings
