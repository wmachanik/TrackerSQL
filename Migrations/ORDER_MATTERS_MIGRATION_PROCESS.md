# ?? IMPORTANT: Migration Order MATTERS!

## The Problem You Hit

You ran the migration in the **wrong order**, which caused `AccessSrc.ClientUsageTbl` to have NULL dates.

### What Happened (Wrong Order) ?

```
1. Run MigrationRunner option 5 (Generate DML)
   ? This generated correct SQL ?
   
2. Run DataMigration_LATEST.sql
   ? But AccessSrc.ClientUsageTbl was empty/NULL! ?
   ? So it migrated NULL dates
```

### What Should Happen (Correct Order) ?

```
1. Run Import-AccessClientUsageDates.ps1
   ? This populates AccessSrc.ClientUsageTbl with proper datetimes
   
2. Run MigrationRunner option 5 (Generate DML)
   ? This generates DataMigration_LATEST.sql
   
3. Run DataMigration_LATEST.sql
   ? Now AccessSrc has dates, so they get migrated correctly!
```

## Why This Happens

### The Migration Runner's Generation Process

When you run **MigrationRunner option 5**, it:
1. Reads the **schema files** and **constraints**
2. Generates SQL code based on **metadata**
3. **Does NOT look at the actual data** in `AccessSrc`

The generator **assumes** you will have proper data in `AccessSrc` when you run the script.

### The AccessSrc Schema

The `AccessSrc` schema is a **staging area**:
- Standard CSV import ? All columns are TEXT (varchar/nvarchar)
- PowerShell import ? Specific columns are proper DATETIME

The generator now **knows** that `ClientUsageTbl` needs special handling, but the **actual datetime data** must be loaded by running the PowerShell script.

## Complete Migration Process (From Scratch)

### Step 1: Build MigrationRunner
```cmd
cd Migrations\MigrationRunner
dotnet build
```

### Step 2: Export Access Schema (if needed)
```cmd
dotnet run
# Select option 1: Export Access Schema
```

### Step 3: Import ClientUsageTbl Dates ? CRITICAL
```powershell
cd ..\..
.\Migrations\Import-AccessClientUsageDates.ps1
```

**This populates `AccessSrc.ClientUsageTbl` with proper datetime values.**

### Step 4: Generate DML Scripts
```cmd
cd Migrations\MigrationRunner
dotnet run
# Select option 5: Generate DML Scripts
```

**This creates `DataMigration_LATEST.sql` with the fix.**

### Step 5: Run Data Migration
```cmd
sqlcmd -S .\SQLExpress -d OtterDb -U sa -P "password" `
       -i "..\..\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql"
```

**Now it migrates the dates from `AccessSrc` to final tables.**

### Step 6: Verify
```sql
SELECT COUNT(*) AS Total,
       SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS HasDates
FROM ContactsItemsPredictedTbl;
```

Expected: **2,136 rows with dates** ?

## Quick Fix (If You Already Ran Migration)

If you already ran the migration and got NULL dates:

### Option A: Re-import Just This Table
```powershell
# 1. Import dates to AccessSrc
.\Migrations\Import-AccessClientUsageDates.ps1

# 2. Re-migrate just ContactsItemsPredictedTbl
sqlcmd -S .\SQLExpress -d OtterDb -U sa -P "password" `
       -i "Migrations\RemigrateDateColumns.sql"
```

### Option B: Full Re-migration
```powershell
# 1. Import dates to AccessSrc
.\Migrations\Import-AccessClientUsageDates.ps1

# 2. Re-run full data migration
sqlcmd -S .\SQLExpress -d OtterDb -U sa -P "password" `
       -i "Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql"
```

## Why The Generator Can't Do This Automatically

The generator **cannot** run the PowerShell import because:

1. **Separation of concerns**: Generator creates SQL scripts, doesn't execute them
2. **Flexibility**: You might have different data sources
3. **Safety**: The import script clears `AccessSrc.ClientUsageTbl` and re-imports
4. **Order dependencies**: You might want to customize what gets imported

## Visual Summary

```
???????????????????????????????????????????????????????????????
?  CORRECT MIGRATION ORDER                                    ?
???????????????????????????????????????????????????????????????

Access DB
   ?
   ??? Standard CSV Import ? AccessSrc (text columns)
   ?
   ??? PowerShell Import ? AccessSrc.ClientUsageTbl (datetime) ?
          ?
          ??? MigrationRunner reads schemas ? Generates DML
          ?      ?
          ?      DataMigration_LATEST.sql (correct SQL)
          ?
          ??? Run DataMigration_LATEST.sql
                 ?
                 Reads AccessSrc.ClientUsageTbl (has datetimes!)
                 ?
                 ContactsItemsPredictedTbl (dates preserved!) ?
```

```
???????????????????????????????????????????????????????????????
?  WRONG ORDER (What You Did)                                ?
???????????????????????????????????????????????????????????????

Access DB
   ?
   Standard CSV Import ? AccessSrc (text, no special imports)
      ?
      ??? MigrationRunner generates DML ? (SQL is correct)
      ?      ?
      ?      DataMigration_LATEST.sql
      ?
      ??? Run DataMigration_LATEST.sql
             ?
             Reads AccessSrc.ClientUsageTbl (all NULL!) ?
             ?
             ContactsItemsPredictedTbl (NULL dates) ?
```

## TL;DR

**Always run `Import-AccessClientUsageDates.ps1` BEFORE running the data migration!**

The generator creates the correct SQL, but it needs the data to be there when the SQL runs.

---

**Status**: ? Issue understood and documented  
**Fix**: Run PowerShell import before migration  
**Current State**: Database now has correct dates after running RemigrateDateColumns.sql
