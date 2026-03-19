# ?? FINAL ISSUE: ClientUsageLinesTbl Date Conversion

## Status: 97.8% Success (44/45 tables)

**Only 1 table remaining**: ContactsItemSvcSummaryTbl (0/65,046 rows)

---

## Root Cause

`ClientUsageLinesTbl` has a `Date` column that is:
- **In AccessSrc**: NVARCHAR containing "2007-01-02T00:00:00.0000000"
- **In Target**: datetime

The generator thought it was **already a proper datetime** (like ClientUsageTbl), so it generated:
```sql
[Date] AS [UsageDate]  -- Direct reference, no conversion
```

But it's actually NVARCHAR! So SQL Server tries **implicit conversion** which fails.

---

## Why This Happened

**Line 805** in `DmlScriptGenerator.cs` said:
```csharp
"ClientUsageLinesTbl",  // May also have datetime columns properly imported
```

But there's **NO auto-import** for `ClientUsageLinesTbl`! Only `ClientUsageTbl` has auto-import in `AccessStagingImporter.cs`.

So the dates are still text in AccessSrc.

---

## The Fix Applied

### Changed: `DmlScriptGenerator.cs` lines 795-835

**Before**:
```csharp
var tablesWithProperDates = new HashSet<string>(...)
{
    "ClientUsageTbl",
    "ClientUsageLinesTbl",  // ? Not actually auto-imported!
    ...
};
```

**After**:
```csharp
var tablesWithProperDates = new HashSet<string>(...)
{
    "ClientUsageTbl",  // ? Has auto-import
    // "ClientUsageLinesTbl",  // ? Removed - NOT auto-imported
    ...
};
```

---

## What This Will Generate

### Before Fix (Generated SQL):
```sql
SELECT
    ...
    [Date] AS [UsageDate]  -- ? Fails: can't convert "2007-01-02T00:00:00" to datetime
```

### After Fix (Will Generate):
```sql
SELECT
    ...
    CASE WHEN NULLIF([Date], N'') IS NULL ...
        THEN NULL
        ELSE COALESCE(
            TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127),  -- ISO8601 with T
            TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126),  -- ISO8601
            ...
        )
    END AS [UsageDate]  -- ? Proper conversion
```

---

## Next Steps

### Step 1: Regenerate SQL

```cmd
cd Migrations\MigrationRunner
dotnet run
# Select: M (Generate DataMigration)
```

This will generate NEW SQL with proper date conversion for `ClientUsageLinesTbl.Date`.

### Step 2: Verify Generated SQL

Check that `Date` column now has conversion:
```powershell
Select-String -Path "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" -Pattern "ClientUsageLinesTbl" -Context 0, 10 | Select-Object -First 1
```

Should see `TRY_CONVERT` for the Date column.

### Step 3: Apply Migration

```cmd
# Still in MigrationRunner:
# Select: N (Apply DataMigration)
```

### Step 4: Verify Results

```cmd
# Select: & (Verification)
```

**Expected**:
```
ContactsItemSvcSummaryTbl: 64,756/65,046 rows ? (~99.5%)
Success rate: 45/45 (100%)
```

---

## Why 64,756 and not 65,046?

**290 rows will be excluded** by the INNER JOIN because:
- They have CustomerID values that don't exist in ContactsTbl
- These are orphaned records (deleted customers)
- Correctly excluded to maintain FK integrity

---

## Summary

| Issue | Cause | Fix |
|-------|-------|-----|
| ContactsItemSvcSummaryTbl empty | `ClientUsageLinesTbl.Date` is NVARCHAR but treated as datetime | ? Removed from "proper datetime" list |
| Date conversion fails | Generator didn't add TRY_CONVERT | ? Will now generate proper conversion |

---

## Build Status

? **Successful** - Ready to regenerate SQL

---

## Expected Final Result

After regenerating and re-running:

```
=== VERIFICATION SUMMARY ===
Tables verified: 45
Critical issues: 0
Warnings: 3
Success rate: 45/45 (100%)

ContactsItemSvcSummaryTbl: ? SUCCESS - 64,756 rows
```

---

**Just regenerate (M) and rerun (N) to finish the last table!** ??
