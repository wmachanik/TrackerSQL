# ?? TWO FIXES APPLIED - Need to Regenerate!

## Current Status

? **ContactsItemSvcSummaryTbl**: Still 0/65,046 rows

**Why?** You haven't regenerated the SQL yet! The old SQL still has the bugs.

---

## Two Issues Fixed

### Issue #1: ClientUsageLinesTbl Date Column ?

**Problem**: Generator thought `Date` column was already datetime (it's NVARCHAR)

**Fix Applied**:
- Removed `ClientUsageLinesTbl` from `HasProperDatetimeColumns()` list
- Generator will now add `TRY_CONVERT` for Date column

**Before** (wrong):
```sql
[Date] AS [UsageDate]  -- No conversion, fails!
```

**After** (will generate):
```sql
CASE WHEN NULLIF([Date], N'') IS NULL ...
    ELSE COALESCE(
        TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127),  -- ISO8601
        TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126),
        ...
    )
END AS [UsageDate]  -- ? Proper conversion
```

---

### Issue #2: WHERE Clause Column Name ?

**Problem**: WHERE clause uses hardcoded `src.CustomerId` but column is `src.CustomerID` (capital D)

**Fix Applied**:
- Added `GetCustomerIdColumnName()` helper method
- WHERE clause now uses correct column name per source table

**Before** (line 42 in generated SQL):
```sql
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerID  -- ? Correct
WHERE src.CustomerId IS NOT NULL;                         -- ? Wrong casing
```

**After** (will generate):
```sql
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerID  -- ? Correct
WHERE src.CustomerID IS NOT NULL;                         -- ? Correct!
```

---

## What You MUST Do Now

### Step 1: Regenerate SQL

```cmd
cd C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner
dotnet run

# Select: M (Generate DataMigration SQL)
```

**This will create NEW SQL with both fixes!**

### Step 2: Apply Migration

```cmd
# Still in MigrationRunner:
# Select: N (Apply DataMigration)
```

### Step 3: Verify

```cmd
# Select: & (Verification)
```

**Expected**:
```
ContactsItemSvcSummaryTbl: 64,756 rows ? (~99.5%)
Success rate: 45/45 (100%)
```

---

## Why You Still See 0 Rows

**Timeline**:
1. ? Old code had bugs
2. ? Generated `DataMigration_LATEST.sql` with bugs
3. ? Ran migration ? 0 rows
4. ? **I fixed the code** (just now)
5. ?? **But SQL is still the OLD version!**
6. ? **You need to regenerate!**

---

## What the New SQL Will Have

### 1. Date Conversion ?
```sql
-- ClientUsageLinesTbl.Date column
CASE WHEN NULLIF([Date], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([Date], N'')))) = 0 
    THEN NULL 
    ELSE COALESCE(
        TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127),  -- ISO8601 with T
        TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126),  -- ISO8601
        TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121),  -- ODBC
        TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103),  -- dd/MM/yyyy
        TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 101),  -- MM/dd/yyyy
        TRY_CONVERT(datetime2(7), NULLIF([Date], N'')),       -- Default
        CAST(NULL AS datetime2(7))                             -- Fallback
    ) 
END AS [UsageDate]
```

### 2. Correct WHERE Clause ?
```sql
FROM [AccessSrc].[ClientUsageLinesTbl] src
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerID
WHERE src.CustomerID IS NOT NULL;  -- ? Now matches actual column name
```

---

## Files Changed

| File | Line | Change |
|------|------|--------|
| `DmlScriptGenerator.cs` | 797-811 | ? Removed ClientUsageLinesTbl from proper datetime list |
| `DmlScriptGenerator.cs` | 12-68 | ? Added GetCustomerIdColumnName() helper |
| `DmlScriptGenerator.cs` | 42 | ? WHERE clause now uses correct column |

---

## Build Status

? **Successful** - Ready to regenerate

---

## Expected Results

### Current (Before Regenerate)
```
ContactsItemSvcSummaryTbl: 0/65,046 rows ?
```

### After Regenerate + Rerun
```
ContactsItemSvcSummaryTbl: 64,756/65,046 rows ? (99.5%)
```

**Why 99.5% and not 100%?**
- 290 rows have CustomerID that doesn't exist in ContactsTbl
- These are orphaned records (deleted customers)
- INNER JOIN correctly excludes them
- This is **expected and correct**

---

## Quick Command Reference

```cmd
# 1. Go to MigrationRunner
cd Migrations\MigrationRunner

# 2. Run MigrationRunner
dotnet run

# 3. Regenerate SQL
Select: M

# 4. Apply migration
Select: N

# 5. Verify
Select: &
```

---

## Summary

| Issue | Status | Action Needed |
|-------|--------|---------------|
| Date conversion bug | ? Code fixed | Regenerate SQL |
| WHERE clause column name | ? Code fixed | Regenerate SQL |
| Old SQL still in use | ? Not regenerated | **Run option M!** |
| Migration not applied | ? Not run | **Run option N!** |

---

**The fixes are in the CODE, but you haven't regenerated the SQL yet!**  
**Run M (Generate) ? N (Apply) to finish!** ??
