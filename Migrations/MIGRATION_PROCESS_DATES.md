# Migration Process Update: ClientUsageTbl Date Handling

## Critical Issue Fixed

**Problem**: When migrating from Access to SQL Server, date columns in `ClientUsageTbl` were being lost, resulting in NULL values in `ContactsItemsPredictedTbl`.

**Root Cause**: The standard Access staging import was treating datetime columns as text, and the migration script was using `NULLIF(column, N'')` which would convert proper dates to NULL.

## Solution Implemented

### 1. Pre-Migration Step: Import Dates Properly

Before running `DataMigration_LATEST.sql`, you must run the specialized PowerShell import script:

```powershell
# Run from TrackerSQL directory
.\Migrations\Import-AccessClientUsageDates.ps1
```

This script:
- Connects directly to Access using ADO.NET with proper DateTime types
- Reads dates as DateTime objects (not strings)
- Inserts into `AccessSrc.ClientUsageTbl` using parameterized SQL with SqlDbType.DateTime
- Validates that dates are preserved

### 2. Updated Migration Script

`DataMigration_LATEST.sql` has been updated (lines 2397-2452) to:

**Before (BROKEN)**:
```sql
SELECT
    NULLIF([CustomerId], N'') AS [ContactID], 
    NULLIF([NextCoffeeBy], N'') AS [NextCoffeeBy],  -- WRONG: Converts datetime to NULL
    NULLIF([NextCleanOn], N'') AS [NextCleanOn]     -- WRONG: Converts datetime to NULL
FROM [AccessSrc].[ClientUsageTbl];
```

**After (FIXED)**:
```sql
SELECT
    src.[CustomerId] AS [ContactID], 
    src.[NextCoffeeBy],   -- CORRECT: Preserves datetime value
    src.[NextCleanOn],    -- CORRECT: Preserves datetime value
    src.[NextFilterEst],  -- CORRECT: Preserves datetime value
    src.[NextDescaleEst], -- CORRECT: Preserves datetime value
    src.[NextServiceEst]  -- CORRECT: Preserves datetime value
FROM [AccessSrc].[ClientUsageTbl] src
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerId  -- Prevents FK violations
WHERE src.CustomerId IS NOT NULL;
```

## Complete Migration Process (Updated)

### Step 0: Prerequisites
```cmd
cd Migrations\MigrationRunner
dotnet build
```

### Step 1: Run Standard Migration (Up to Schema/DML generation)
```cmd
dotnet run
# Follow menu to export schema and generate scripts
```

### Step 2: **NEW** - Import ClientUsageTbl with Dates
```powershell
# This is the critical new step!
.\Migrations\Import-AccessClientUsageDates.ps1
```

**What this does**:
- Reads `ClientUsageTbl` from Access: `C:\SRC\Data\QuaffeeTracker08.mdb`
- Properly handles DateTime columns
- Imports to `AccessSrc.ClientUsageTbl` in SQL Server
- Creates backup: `AccessSrc.ClientUsageTbl_BACKUP`

**Verification**:
```sql
-- Should show dates, not NULLs
SELECT TOP 5 CustomerId, NextCoffeeBy, NextCleanOn 
FROM AccessSrc.ClientUsageTbl 
WHERE NextCoffeeBy IS NOT NULL;
```

### Step 3: Run Data Migration Script
```cmd
sqlcmd -S .\SQLExpress -d OtterDb -U sa -P "your_password" ^
       -i "Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql"
```

**Note**: The updated script (see above) now properly handles dates.

### Step 4: Verify ContactsItemsPredictedTbl
```sql
SELECT 
    COUNT(*) AS Total,
    SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS HasDates
FROM ContactsItemsPredictedTbl;
-- Should show 2,136 rows with dates
```

## Why This Approach?

### Option 1: Fix the CSV Import (Rejected)
- The standard CSV import from Access doesn't preserve datetime types
- Would require rewriting the entire staging import logic
- Complex and error-prone

### Option 2: Pre-Import Dates with PowerShell (CHOSEN ?)
- Simple, focused script that handles just this one table
- Uses proper ADO.NET types for datetime
- Easy to verify and test
- Can be re-run if needed
- Doesn't affect other migration logic

## Files Changed

### Updated Files
1. **`Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql`**
   - Lines 2397-2452: Updated ClientUsageTbl ? ContactsItemsPredictedTbl migration
   - Removed `NULLIF` on date columns
   - Added `INNER JOIN` for FK safety
   - Added comments explaining the fix

### New Files Created
1. **`Migrations\Import-AccessClientUsageDates.ps1`**
   - PowerShell script to properly import dates from Access
   - Main solution to the date loss problem

2. **`Migrations\RemigrateDateColumns.sql`**
   - Helper script to re-migrate just this table
   - Useful for testing without full migration run

3. **`Migrations\CheckAccessDates.ps1`**
   - Diagnostic tool to verify dates exist in Access
   - Helps troubleshoot import issues

### Documentation Files
1. **`Migrations\FIX_NULL_DATES_IN_ACCESSSRC.md`** - Detailed troubleshooting guide
2. **`Migrations\DATES_NULL_SUMMARY.md`** - Executive summary
3. **`Migrations\MIGRATION_COMPLETE.md`** - What was done
4. **`Migrations\QUICK_REFERENCE.md`** - Quick reference card
5. **`Migrations\MIGRATION_PROCESS_DATES.md`** (this file) - Updated process documentation

## Troubleshooting

### If dates are still NULL after migration

1. **Check Access database has dates**:
   ```powershell
   .\Migrations\CheckAccessDates.ps1
   ```

2. **Verify AccessSrc.ClientUsageTbl has dates**:
   ```sql
   SELECT TOP 5 CustomerId, NextCoffeeBy 
   FROM AccessSrc.ClientUsageTbl 
   WHERE NextCoffeeBy IS NOT NULL;
   ```
   
   If NULL here, re-run the import script:
   ```powershell
   .\Migrations\Import-AccessClientUsageDates.ps1
   ```

3. **Check foreign keys**:
   ```sql
   -- Check for orphan ContactIDs
   SELECT COUNT(*) 
   FROM AccessSrc.ClientUsageTbl src
   WHERE NOT EXISTS (
       SELECT 1 FROM ContactsTbl c WHERE c.ContactID = src.CustomerId
   );
   ```

4. **Re-migrate just ContactsItemsPredictedTbl**:
   ```sql
   DELETE FROM ContactsItemsPredictedTbl;
   -- Then run RemigrateDateColumns.sql
   ```

## Prevention: Future Migrations

For any future tables with datetime columns from Access:

1. **Check if dates are being preserved** in AccessSrc schema
2. If not, create a similar PowerShell import script
3. Update the DML migration script to **NOT** use `NULLIF` on datetime columns
4. Always use `INNER JOIN` to prevent FK violations

## Summary Checklist

Before running a full migration from scratch:

- [ ] Access database accessible at `C:\SRC\Data\QuaffeeTracker08.mdb`
- [ ] SQL Server running with OtterDb database created
- [ ] MigrationRunner built successfully
- [ ] Run standard migration up to DML script generation
- [ ] **Run `Import-AccessClientUsageDates.ps1`** ? CRITICAL NEW STEP
- [ ] Run `DataMigration_LATEST.sql` (now has the fix)
- [ ] Verify `ContactsItemsPredictedTbl` has dates
- [ ] Test ContactDetails.aspx "Predicted Items" tab

---

**Last Updated**: $(Get-Date -Format "yyyy-MM-dd")  
**Status**: ? Issue fixed and documented
