# CRITICAL FIX: Identity Column DROP Action Not Working

## Root Cause Identified ?

The "Drop" action for identity columns **was working correctly** to exclude columns from the INSERT list, **BUT** there was a separate bug in the logic that decides whether to use `SET IDENTITY_INSERT ON`.

### The Bug

In `DmlScriptGenerator.cs` line 182, the code was checking the **raw column list** (`tm.Columns`) instead of the **filtered column list** (`cols`):

```csharp
// BEFORE (BUGGY):
var includeIdentityInInsert = !string.IsNullOrWhiteSpace(chosenIdentity) &&
                              tm.Columns.Any(c => c.Target != null && c.Target.Equals(chosenIdentity, StringComparison.OrdinalIgnoreCase));
```

This meant:
1. ? The identity column WAS excluded from the INSERT column list (correct!)
2. ? BUT `useIdentityInsert` was still set to TRUE (because the column was in `tm.Columns`)
3. ? So the generated SQL had `SET IDENTITY_INSERT ON` without the identity column in the INSERT
4. ? Result: Duplicate PK errors because SQL Server was auto-generating IDs but also trying to use IDENTITY_INSERT

### The Fix

Changed line 182 to use `cols` (the filtered list after "Drop" columns are removed):

```csharp
// AFTER (FIXED):
var includeIdentityInInsert = !string.IsNullOrWhiteSpace(chosenIdentity) &&
                              cols.Any(c => c.Target != null && c.Target.Equals(chosenIdentity, StringComparison.OrdinalIgnoreCase));
```

Now:
1. ? Identity column is excluded from INSERT column list
2. ? `useIdentityInsert` is FALSE (because column is not in `cols`)
3. ? Generated SQL will NOT have `SET IDENTITY_INSERT ON`
4. ? SQL Server auto-generates new unique IDs ? No duplicate PK errors!

---

## Files Changed

### 1. `Migrations/MigrationRunner/DmlScriptGenerator.cs` (Line 182)
**Before:**
```csharp
var includeIdentityInInsert = !string.IsNullOrWhiteSpace(chosenIdentity) &&
                              tm.Columns.Any(c => c.Target != null && c.Target.Equals(chosenIdentity, StringComparison.OrdinalIgnoreCase));
```

**After:**
```csharp
var includeIdentityInInsert = !string.IsNullOrWhiteSpace(chosenIdentity) &&
                              cols.Any(c => c.Target != null && c.Target.Equals(chosenIdentity, StringComparison.OrdinalIgnoreCase));
```

### 2. Schema Files (Already Fixed Earlier)
- `Data/Metadata/AccessSchema/ItemUsageTbl.schema.json` - ClientUsageLineNo marked as "Drop"
- `Data/Metadata/AccessSchema/ClientUsageLinesTbl.schema.json` - ClientUsageLineNo marked as "Drop"

---

## Next Steps

### 1. Rebuild the Project ?
Already done - build successful!

### 2. Regenerate the Migration Script
Run **Option M** in MigrationRunner to regenerate `DataMigration_LATEST.sql`.

**You should see:**
```
SKIPPING dropped column: ClientUsageLineNo (Action: Drop)
SKIPPING dropped column: ClientUsageLineNo (Action: Drop)
```

### 3. Verify the Generated SQL

Check `Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql` for these tables:

**ContactsItemUsageTbl should look like:**
```sql
-- ItemUsageTbl -> ContactsItemUsageTbl
IF OBJECT_ID(N'AccessSrc.ItemUsageTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';  -- ? Should be OFF now!
    PRINT N'INSERT INTO [ContactsItemUsageTbl] ([ContactID], [DeliveryDate], ...';
    BEGIN TRY
        BEGIN TRAN;
        -- NO SET IDENTITY_INSERT ON here!  ? This line should be GONE!
        INSERT INTO [ContactsItemUsageTbl]
        (
            [ContactID], [DeliveryDate], [ItemProvidedID], [QtyProvided], [ItemPrepTypeID], [ItemPackagingID], [Notes]
            -- NOTE: ContactItemUsageLineNo is NOT in this list!
        )
        SELECT
            NULLIF([CustomerID], N'') AS [ContactID],
            ...
        FROM [AccessSrc].[ItemUsageTbl];
        -- NO SET IDENTITY_INSERT OFF here!  ? This line should be GONE!
        -- NO DBCC CHECKIDENT here!  ? This line should be GONE!
        COMMIT;
        PRINT 'OK migrate [ContactsItemUsageTbl] from ' + N'[AccessSrc].[ItemUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        -- NO identity insert cleanup here!  ? These lines should be GONE!
        PRINT 'ERROR migrate [ContactsItemUsageTbl] from ' + N'[AccessSrc].[ItemUsageTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
```

### 4. Run the Migration
Run **Option N** to apply the migration.

**Expected Results:**
? All purges succeed  
? All INSERTs succeed  
? **NO duplicate PK errors** for ContactsItemUsageTbl and ContactUsageLinesTbl  
? SQL Server auto-generates new sequential IDs (1, 2, 3, ...)  
? Row counts match between Access and SQL Server

---

## Why This Happened

The issue was subtle:

1. The "Drop" action correctly excluded columns from `tm.Columns` during mapping construction
2. The `cols` list was correctly filtered to only include valid columns
3. BUT the `useIdentityInsert` flag was calculated using the **unfiltered** `tm.Columns`
4. This caused a mismatch: **no identity column in INSERT**, but **IDENTITY_INSERT still ON**

This is like telling SQL Server:
> "Hey, I'm going to manually specify identity values (IDENTITY_INSERT ON), but actually I'm not giving you any identity values in my INSERT statement."

SQL Server then auto-generates IDs (because they're not in the INSERT), but the transaction context still has IDENTITY_INSERT ON, which can cause weird behavior and constraint violations.

---

## Testing Strategy

After regenerating and running the migration:

### 1. Check for "SKIPPING dropped column" messages
```
Run Option M, look for:
  SKIPPING dropped column: ClientUsageLineNo (Action: Drop)
```

### 2. Verify IDENTITY_INSERT is OFF
```sql
-- Check the generated SQL file
Get-Content "Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" | 
    Select-String -Pattern "ContactsItemUsageTbl" -Context 5,15
```

Look for **"IdentityInsert=OFF"** in the PRINT statement.

### 3. Verify Row Counts
```sql
-- After migration succeeds
SELECT COUNT(*) FROM AccessSrc.ItemUsageTbl;          -- Should match
SELECT COUNT(*) FROM ContactsItemUsageTbl;            -- Should match

SELECT COUNT(*) FROM AccessSrc.ClientUsageLinesTbl;   -- Should match
SELECT COUNT(*) FROM ContactUsageLinesTbl;            -- Should match
```

### 4. Verify New IDs Are Sequential
```sql
SELECT 
    MIN(ContactItemUsageLineNo) AS MinID,
    MAX(ContactItemUsageLineNo) AS MaxID,
    COUNT(*) AS RowCount
FROM ContactsItemUsageTbl;
-- Should see: MinID=1, MaxID=RowCount

SELECT 
    MIN(ContactUsageLineNo) AS MinID,
    MAX(ContactUsageLineNo) AS MaxID,
    COUNT(*) AS RowCount
FROM ContactUsageLinesTbl;
-- Should see: MinID=1, MaxID=RowCount
```

---

## Summary

| Issue | Status |
|-------|--------|
| Schema files marked columns as "Drop" | ? Fixed (earlier) |
| Purge logic simplified | ? Fixed (earlier) |
| **IDENTITY_INSERT logic bug** | ? **FIXED NOW** |
| Build successful | ? Yes |
| Ready to regenerate migration | ? Yes |

**Action Required:** Run **Option M** to regenerate, then run **Option N** to migrate!

This should finally work! ??
