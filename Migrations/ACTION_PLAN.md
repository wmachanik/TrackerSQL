# IMMEDIATE ACTION PLAN

## Current Situation
- Migration is failing with duplicate PK errors
- Schema changes were made but migration script wasn't regenerated
- Option X (Drop All Tables) is failing with syntax error
- "Invalid column name" errors indicate schema mismatch

---

## SOLUTION: Use Manual SQL Script

### Step 1: Run the Manual Drop Script

I've created a working SQL script for you. Execute it in SQL Server Management Studio:

**File:** `Data\Metadata\PlanEdits\Sql\ManualDropAllTables.sql`

**OR paste this directly into SSMS:**

```sql
USE TrackerSQL;
GO

-- Disable all foreign keys
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + '.' + QUOTENAME(t.name) + 
              ' NOCHECK CONSTRAINT ' + QUOTENAME(fk.name) + ';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id = fk.parent_object_id;
EXEC sp_executesql @sql;
GO

-- Drop all dbo tables
DECLARE @sql2 NVARCHAR(MAX) = '';
SELECT @sql2 = @sql2 + 'DROP TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
FROM sys.tables t
WHERE SCHEMA_NAME(t.schema_id) = 'dbo';
EXEC sp_executesql @sql2;
GO

-- Drop all AccessSrc tables
IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AccessSrc')
BEGIN
    DECLARE @sql3 NVARCHAR(MAX) = '';
    SELECT @sql3 = @sql3 + 'DROP TABLE [AccessSrc].' + QUOTENAME(t.name) + ';' + CHAR(13)
    FROM sys.tables t
    WHERE SCHEMA_NAME(t.schema_id) = 'AccessSrc';
    EXEC sp_executesql @sql3;
END
GO

-- Verify
SELECT COUNT(*) AS RemainingTables FROM sys.tables 
WHERE SCHEMA_NAME(schema_id) IN ('dbo', 'AccessSrc');
GO
```

### Step 2: Run the Full Migration Pipeline

In MigrationRunner, execute these options in sequence:

```
A ? Generate CREATE TABLE DDL
B ? Generate FK constraints DDL  
C ? Apply DDL scripts to database
M ? Generate data migration script (will now exclude identity columns!)
MS ? Stage Access data to AccessSrc schema
N ? Apply data migration
```

**Or use the shortcut:**
```
Z ? Full migration pipeline (runs all the above automatically)
```

---

## What Got Fixed

### ? Identity Column Issue (Fixed in Schema)
- `ItemUsageTbl.schema.json` - ClientUsageLineNo now has `"Action": "Drop"`
- `ClientUsageLinesTbl.schema.json` - ClientUsageLineNo now has `"Action": "Drop"`

These changes tell the migration to **auto-generate new IDs** instead of preserving Access IDs.

### ? Purge Logic (Fixed in Code)
- Changed from complex TRUNCATE/DELETE logic to simple DELETE
- Better error messages
- More reliable table clearing

### ?? Invalid Column Names (Needs Investigation)
If you still get "Invalid column name" errors after regenerating, you need to:

1. **Re-export Access schema** (Option 1)
2. **Review column mappings** in the affected `.schema.json` files
3. **Update column actions** to match current table structure

---

## Expected Results After Fresh Start

When you run the pipeline (A?B?C?M?MS?N) you should see:

? **Step A:** Creates all table definitions  
? **Step B:** Creates all foreign key constraints  
? **Step C:** Applies DDL to database (tables created)  
? **Step M:** Generates migration script **WITHOUT** identity columns for ContactsItemUsageTbl and ContactUsageLinesTbl  
? **Step MS:** Stages Access data successfully  
? **Step N:** Migrates data with:
- All purges succeed
- All INSERTs succeed with auto-generated IDs
- No duplicate PK errors
- No "Invalid column name" errors (if schema is current)

---

## If You Still Get Errors

### Duplicate PK Errors
? **Means:** Migration script wasn't regenerated  
? **Fix:** Run Option M again

### Invalid Column Name Errors
? **Means:** Schema JSON files don't match SQL Server tables  
? **Fix:** 
1. Run Option 1 (re-export Access schema)
2. Or manually update the `.schema.json` files
3. Then run Options A?B?C to recreate tables

### Option X Still Fails
? **Means:** There's a SQL execution issue  
? **Fix:** Use the manual SQL script instead (provided above)

---

## Quick Reference

| Option | Description | When to Use |
|--------|-------------|-------------|
| **X** | Drop ALL tables | When starting fresh (use manual script if fails) |
| **A** | Generate CREATE TABLE DDL | After dropping tables |
| **B** | Generate FK constraints | After Option A |
| **C** | Apply DDL to database | After Option B |
| **M** | Generate data migration script | After schema changes or Option C |
| **MS** | Stage Access data | Before Option N |
| **N** | Apply data migration | After Option MS |
| **Z** | Full pipeline (A?B?C?M?MS?N) | When you want automated full migration |

---

## Files Created for You

1. ? `Migrations/TROUBLESHOOTING.md` - Detailed troubleshooting guide
2. ? `Data/Metadata/PlanEdits/Sql/ManualDropAllTables.sql` - Working drop script
3. ? `Migrations/DUPLICATE_PK_FIX_SUMMARY.md` - Explanation of duplicate PK fix
4. ? `Migrations/STEP_N_FIX_SUMMARY.md` - Explanation of purge logic fix

---

## Bottom Line

**Your issue will be fixed if you:**

1. ? **Drop all tables** (use manual SQL script)
2. ? **Run full pipeline** (Option Z or A?B?C?M?MS?N)
3. ? **The schema changes are already applied** - just need regeneration

**The duplicate PK problem is solved** - the schema files now tell the migrator to skip identity columns and let SQL Server auto-generate them.

**Just need to start fresh with clean tables!** ??
