# Migration Troubleshooting Guide

## Current Issues

### Issue 1: Duplicate Primary Key Errors (PARTIALLY FIXED - Needs Regeneration)
? **Fixed in schema files** - identity columns marked as "Drop"  
? **Not yet regenerated** - old DataMigration_LATEST.sql still has the identity columns

**Solution:** Run **Option M** to regenerate the migration script.

---

### Issue 2: Invalid Column Name Errors (CRITICAL)
The migration script is trying to INSERT into columns that don't exist in your target tables:

**Missing Columns Detected:**
- `PrepDate`
- `ItemID`
- `QtyOrdered`
- `PrepTypeID`
- `PackagingID`
- `ItemRequiredID`
- `QtyRequired`
- `ItemPackagingID`

This means:
1. Your **AccessSchema JSON files** have column mappings
2. But the **actual SQL Server tables** don't have those columns
3. OR the columns have been renamed in SQL Server but not in the schema plan

---

## Quick Fix: Regenerate Everything

### Option A: Full Pipeline (Recommended)
1. **Drop all tables manually** (see SQL script below)
2. **Run Option A** - Generate CREATE TABLE DDL
3. **Run Option B** - Generate FK constraints DDL
4. **Run Option C** - Apply DDL to database
5. **Run Option M** - Generate data migration script (will now respect "Drop" actions)
6. **Run Option MS** - Stage Access data
7. **Run Option N** - Apply data migration

### Option B: Just Regenerate Migration (If tables are correct)
1. **Run Option M** - Generate data migration script
2. **Check the generated SQL** - verify identity columns are excluded
3. **Run Option N** - Apply migration

---

## Manual Table Drop Script

If Option X is failing, run this SQL manually:

```sql
-- Run this in SQL Server Management Studio or sqlcmd
USE TrackerSQL;
GO

PRINT 'Starting table drop process...';
GO

-- Step 1: Disable all foreign keys
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + '.' + QUOTENAME(t.name) + 
              ' NOCHECK CONSTRAINT ' + QUOTENAME(fk.name) + ';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id = fk.parent_object_id;

IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql;
    PRINT 'Disabled ' + CAST((SELECT COUNT(*) FROM sys.foreign_keys) AS VARCHAR) + ' foreign keys';
END
ELSE
    PRINT 'No foreign keys found';
GO

-- Step 2: Drop all tables in dbo schema
DECLARE @sql2 NVARCHAR(MAX) = '';
DECLARE @count INT = 0;
SELECT @sql2 = @sql2 + 'DROP TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + '.' + QUOTENAME(t.name) + ';' + CHAR(13),
       @count = @count + 1
FROM sys.tables t
WHERE SCHEMA_NAME(t.schema_id) = 'dbo'
ORDER BY t.name;

IF LEN(@sql2) > 0
BEGIN
    EXEC sp_executesql @sql2;
    PRINT 'Dropped ' + CAST(@count AS VARCHAR) + ' tables from dbo schema';
END
ELSE
    PRINT 'No tables found in dbo schema';
GO

-- Step 3: Drop AccessSrc schema tables if they exist
IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AccessSrc')
BEGIN
    DECLARE @sql3 NVARCHAR(MAX) = '';
    DECLARE @count3 INT = 0;
    SELECT @sql3 = @sql3 + 'DROP TABLE [AccessSrc].' + QUOTENAME(t.name) + ';' + CHAR(13),
           @count3 = @count3 + 1
    FROM sys.tables t
    WHERE SCHEMA_NAME(t.schema_id) = 'AccessSrc'
    ORDER BY t.name;
    
    IF LEN(@sql3) > 0
    BEGIN
        EXEC sp_executesql @sql3;
        PRINT 'Dropped ' + CAST(@count3 AS VARCHAR) + ' tables from AccessSrc schema';
    END
END
ELSE
    PRINT 'AccessSrc schema does not exist';
GO

PRINT 'Table drop process completed successfully!';
GO

-- Verify: Should return 0 rows
SELECT COUNT(*) AS RemainingTables FROM sys.tables 
WHERE SCHEMA_NAME(schema_id) IN ('dbo', 'AccessSrc');
GO
```

**Save as:** `Data\Metadata\PlanEdits\Sql\ManualDropAllTables.sql`

---

## Verify Schema Alignment

After recreating tables, verify the columns match:

```sql
-- Check what columns exist in ContactsItemUsageTbl
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ContactsItemUsageTbl'
ORDER BY ORDINAL_POSITION;

-- Check what columns exist in ContactUsageLinesTbl
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ContactUsageLinesTbl'
ORDER BY ORDINAL_POSITION;

-- Check for tables with "Invalid column name" issues
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('OrdersTbl', 'RecurringOrdersTbl')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
```

---

## Understanding the "Drop" Action

The schema files now have:
```json
{
  "Source": "ClientUsageLineNo",
  "Target": "ContactUsageLineNo",
  "Action": "Drop",
  "Expression": "Let SQL Server auto-generate new identity values"
}
```

This tells the DML generator to:
1. **Skip this column in the INSERT statement**
2. **Let SQL Server auto-generate** the identity value
3. **Avoid duplicate PK errors** from Access composite keys

The generated SQL should look like:
```sql
INSERT INTO [ContactsItemUsageTbl]
(
    [ContactID], [DeliveryDate], [ItemProvidedID], ...
    -- NOTE: ContactItemUsageLineNo is NOT here!
)
SELECT
    NULLIF([CustomerID], N'') AS [ContactID],
    -- ... other columns ...
FROM [AccessSrc].[ItemUsageTbl];
```

---

## Next Steps After Fresh Start

1. ? **Drop all tables** (manual SQL or fixed Option X)
2. ? **Run full pipeline** (Options A ? B ? C ? M ? MS ? N)
3. ? **Verify no duplicate PK errors** (identity columns auto-generated)
4. ? **Verify no "Invalid column name" errors** (schema matches tables)
5. ? **Check row counts** match between Access and SQL Server

---

## If "Invalid Column Name" Errors Persist

This means your schema plans are outdated. You need to:

### Step 1: Re-export Access Schema
Run **Option 1** to regenerate all AccessSchema JSON files from the current Access database.

### Step 2: Review Column Mappings
Check these files for outdated column names:
- `Data\Metadata\AccessSchema\OrdersTbl.schema.json`
- `Data\Metadata\AccessSchema\RecurringOrdersTbl.schema.json`
- Any other tables mentioned in "Invalid column name" errors

### Step 3: Fix Column Actions
Look for column mappings like:
```json
{
  "Source": "OldColumnName",
  "Target": "NonExistentColumn",
  "Action": "Rename"
}
```

Change to either:
- `"Action": "Drop"` if the column shouldn't be migrated
- Update `"Target"` to match the actual SQL Server column name
- Add the column to the CREATE TABLE DDL if it's missing

---

## Common Pitfalls

### ? Forgetting to Regenerate After Schema Changes
**Problem:** You edit JSON files but don't run Option M  
**Solution:** Always run Option M after editing schema files

### ? Table Structure Mismatch
**Problem:** SQL tables don't match the schema plan  
**Solution:** Run Options A?B?C to recreate tables from scratch

### ? Access Database Has Duplicates
**Problem:** Access data itself has PK violations  
**Solution:** Clean Access data first or use "Drop" action for identity columns

### ? Stale AccessSrc Schema
**Problem:** AccessSrc staging tables have old structure  
**Solution:** Run Option MS to re-stage Access data after recreating tables
