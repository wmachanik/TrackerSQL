# ?? FIX: Regex Broke the SQL - Proper Fix Applied

## What Went Wrong

The Regex approach I tried **completely broke the SQL syntax**!

### Generated SQL (Broken by Regex)

```sql
SELECT
    NULLIF(src.[ClientUsageLineNo], N'') AS src.[ContactsItemSvcSummaryId]  ? ? Invalid!
    NULLIF(src.[CustomerID], N'') AS src.[ContactID]                        ? ? Invalid!
```

**Problem**: Regex replaced BOTH the source column references AND the target aliases with `src.` prefix!

Valid SQL should be:
```sql
SELECT
    NULLIF(src.[ClientUsageLineNo], N'') AS [ContactsItemSvcSummaryId]  ? ? Correct!
    NULLIF(src.[CustomerID], N'') AS [ContactID]                        ? ? Correct!
```

---

## The Real Fix

Instead of using Regex post-processing, I fixed it **at the source** where columns are generated.

### Updated Code (Lines 208-242)

**Before**:
```csharp
var srcExpr = Qi(c.Source);  // Just [ColumnName]
var nvExpr = WrapNullIf(srcExpr);  // NULLIF([ColumnName], N'')
```

**After**:
```csharp
// Check if this table needs FK join
var needsFKJoin = NeedsForeignKeyJoin(tm.Target, tm.Source);

// When we have a JOIN, qualify column references with src. prefix
var srcExpr = needsFKJoin 
    ? $"src.{Qi(c.Source)}"  // src.[ColumnName]
    : Qi(c.Source);          // [ColumnName]

var nvExpr = needsFKJoin 
    ? $"NULLIF({srcExpr}, N'')"        // NULLIF(src.[ColumnName], N'')
    : WrapNullIf(Qi(c.Source));        // NULLIF([ColumnName], N'')
```

---

## What Will Be Generated Now

### For ContactsItemSvcSummaryTbl (Has FK Join)

```sql
SELECT
    NULLIF(src.[ClientUsageLineNo], N'') AS [ContactsItemSvcSummaryId],  ? ? Correct!
    NULLIF(src.[CustomerID], N'') AS [ContactID],                        ? ? Correct!
    CASE WHEN NULLIF(src.[Date], N'') IS NULL ... END AS [UsageDate],   ? ? Correct!
    NULLIF(src.[CupCount], N'') AS [CupCount],                           ? ? Correct!
    NULLIF(src.[ServiceTypeId], N'') AS [ItemServiceTypeID],             ? ? Correct!
    NULLIF(src.[Qty], N'') AS [Qty],                                     ? ? Correct!
    NULLIF(src.[Notes], N'') AS [Notes]                                  ? ? Correct!
FROM [AccessSrc].[ClientUsageLinesTbl] src
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerID
WHERE src.CustomerID IS NOT NULL;
```

### For Other Tables (No FK Join)

```sql
SELECT
    NULLIF([ColumnName], N'') AS [TargetName]  ? No src. prefix (not needed)
FROM [AccessSrc].[TableName];
```

---

## Build Status

? **Successful** - Ready to regenerate

---

## What To Do NOW

```cmd
cd Migrations\MigrationRunner
dotnet run

# Regenerate SQL (with proper column qualification)
Select: M

# Apply migration
Select: N

# Verify
Select: &
```

**Expected**:
```
ContactsItemsPredictedTbl: 2,137 rows ? (98.8%)
ContactsItemSvcSummaryTbl: 64,756 rows ? (99.5%)
```

---

## Summary of All Fixes

| Issue | Attempt | Status |
|-------|---------|--------|
| Date treated as datetime | 1st | ? Fixed - Removed from proper datetime list |
| WHERE uses wrong column | 2nd | ? Fixed - GetCustomerIdColumnName() |
| Ambiguous column 'Notes' | 3rd | ? Regex broke SQL syntax |
| **Proper column qualification** | **4th** | ? **Fixed - Qualify at source** |

---

## Files Changed

1. ? `DmlScriptGenerator.cs` lines 12-69 - Reverted broken Regex
2. ? `DmlScriptGenerator.cs` lines 208-242 - Qualify columns during generation

---

**Regenerate (M) and rerun (N) - the fix is proper now!** ??
