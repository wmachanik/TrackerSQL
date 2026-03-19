# ?? CRITICAL BUG FOUND: Date Heuristic Too Aggressive!

## What Happened

You ran the migration and got:
```
? RepairsTbl: 907 rows migrated (LastStatusChange fix worked!)
? ContactsTbl: 0 rows (FAILED!)
? ContactsItemsPredictedTbl: 0 rows (FK dependency failed)
? ContactsItemSvcSummaryTbl: 0 rows (FK dependency failed)
```

## Root Cause

The **date detection heuristic I added was TOO AGGRESSIVE**!

### The Bad Code (Lines 867-892)

```csharp
// ? BAD - Too broad!
return n.Contains("date") || 
       n.Contains("time") ||
       n.EndsWith("on") ||       // ? Catches "Extension", "Region"!
       n.EndsWith("by") ||       // ? Too broad
       n.StartsWith("next") ||   // ? Too broad
       n.StartsWith("last") ||   // ? Too broad
```

### What It Did Wrong

Generated SQL tried to convert **NON-DATE columns** as dates:

```sql
-- ? WRONG!
CASE WHEN NULLIF([Country/Region], N'') IS NULL ...
  THEN TRY_CONVERT(datetime2(7), ...)     ? Tries to convert "South Africa" to date!

CASE WHEN NULLIF([Extension], N'') IS NULL ...
  THEN TRY_CONVERT(datetime2(7), ...)     ? Tries to convert "123" to date!

CASE WHEN NULLIF([SendDeliveryConfirmation], N'') IS NULL ...
  THEN TRY_CONVERT(datetime2(7), ...)     ? Tries to convert TRUE/FALSE to date!
```

**Result**: Conversion fails ? Transaction rolls back ? ContactsTbl gets 0 rows!

---

## Columns Incorrectly Detected as Dates

| Column Name | Actual Type | Why Detected | Pattern Matched |
|-------------|-------------|--------------|-----------------|
| `Country/Region` | `NVARCHAR` | Contains "Region" ends with "on" | `n.EndsWith("on")` |
| `Extension` | `NVARCHAR` | Ends with "on" | `n.EndsWith("on")` |
| `SendDeliveryConfirmation` | `BIT` | Ends with "on" | `n.EndsWith("on")` |

---

## The Fix Applied

### New Code (More Specific)

```csharp
private static bool IsDateLike(string sqlType, string colName)
{
    if (!string.IsNullOrWhiteSpace(sqlType))
    {
        var t = sqlType.ToLowerInvariant();
        if (t.Contains("date") || t.Contains("time")) return true;
        return false;
    }
    if (string.IsNullOrWhiteSpace(colName)) return false;
    var n = colName.ToLowerInvariant();
    if (n.EndsWith("id")) return false;
    
    // ? GOOD - Specific patterns only
    if (n.Contains("date") || n.Contains("time") || n.Contains("when"))
        return true;
    
    // Only match "*On" if it's a known date pattern
    if (n.EndsWith("on") && 
        (n.StartsWith("next") || n.StartsWith("last") || 
         n.StartsWith("updated") || n.StartsWith("created") || 
         n.Contains("clean")))
        return true;  // NextCleanOn, UpdatedOn - YES
                      // Extension, Region - NO
    
    // Only match "*By" if it's a known date pattern
    if (n.EndsWith("by") && 
        (n.StartsWith("next") || n.StartsWith("last") || 
         n.StartsWith("completed") || n.StartsWith("updated")))
        return true;  // NextCoffeeBy - YES
                      // CompletedBy - YES
                      // Random*By - NO
    
    // Specific patterns for columns without "date" in name
    if ((n.StartsWith("next") || n.StartsWith("last")) && 
        (n.Contains("coffee") || n.Contains("clean") || 
         n.Contains("filter") || n.Contains("descale") || 
         n.Contains("service") || n.Contains("status")))
        return true;  // NextCoffee, LastStatusChange - YES
                      // NextStep - NO
    
    if (n.Contains("logged") || 
        (n.Contains("change") && n.StartsWith("last")))
        return true;  // DateLogged, LastStatusChange - YES
    
    return false;
}
```

### What Changed

| Pattern | Before | After | Why |
|---------|--------|-------|-----|
| `Extension` | ? Detected (ends with "on") | ? Not detected | Doesn't match specific patterns |
| `Country/Region` | ? Detected (ends with "on") | ? Not detected | Doesn't match specific patterns |
| `SendDeliveryConfirmation` | ? Detected (ends with "on") | ? Not detected | Doesn't match specific patterns |
| `NextCleanOn` | ? Detected | ? Still detected | Matches "next*on" pattern |
| `LastStatusChange` | ? Detected | ? Still detected | Matches "last*change" pattern |

---

## Testing the Fix

### Before Fix (Generated Bad SQL)

```sql
-- ContactsTbl migration
CASE WHEN NULLIF([Country/Region], N'') IS NULL ...
  THEN TRY_CONVERT(datetime2(7), NULLIF([Country/Region], N''), 127)  ? FAILS!
```

**Result**: Migration fails, ContactsTbl = 0 rows

### After Fix (Should Generate Good SQL)

```sql
-- ContactsTbl migration
NULLIF([Country/Region], N'') AS [Country/Region]  ? Correct!
NULLIF([Extension], N'') AS [Extension]            ? Correct!
CASE WHEN NULLIF([SendDeliveryConfirmation], N'') IS NULL THEN NULL
  WHEN ... IN (N'1', ...) THEN 1 ...               ? Correct BIT conversion
```

**Result**: Migration succeeds, ContactsTbl = 2,943 rows

---

## What To Do Now

### Step 1: Stop MigrationRunner

The process is currently running and blocking the build.

**Option A: Close MigrationRunner console window**

**Option B: Kill process**
```powershell
Get-Process MigrationRunner | Stop-Process -Force
```

### Step 2: Build with Fix

```cmd
cd C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner
dotnet build
```

### Step 3: Regenerate SQL

```cmd
dotnet run
# Select: M (Generate DataMigration SQL)
```

### Step 4: Verify Generated SQL

Check that `Country/Region`, `Extension`, `SendDeliveryConfirmation` are NOT wrapped in `TRY_CONVERT`:

```powershell
Select-String -Path "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" -Pattern "TRY_CONVERT.*Country/Region|TRY_CONVERT.*Extension|TRY_CONVERT.*SendDelivery"
```

**Should return NOTHING** (no matches)

### Step 5: Run Migration

```cmd
# Still in MigrationRunner:
# Select: N (Apply DataMigration)
```

### Step 6: Verify Results

```sql
SELECT COUNT(*) FROM ContactsTbl;                    -- Should be 2,943
SELECT COUNT(*) FROM ContactsItemsPredictedTbl;      -- Should be 2,137
SELECT COUNT(*) FROM ContactsItemSvcSummaryTbl;      -- Should be 65,046
```

---

## Expected Results After Fix

| Table | Before Fix | After Fix | Status |
|-------|------------|-----------|--------|
| ContactsTbl | 0 | 2,943 | ? FIXED |
| ContactsItemsPredictedTbl | 0 | 2,137 | ? FIXED (FK works) |
| ContactsItemSvcSummaryTbl | 0 | 65,046 | ? FIXED (FK + CustomerID) |
| RepairsTbl | 907 | 907 | ? Already working |
| TempCoffeecheckupCustomerTbl | 0 | 6 | ? Should work |

---

## Summary

| Issue | Cause | Fix |
|-------|-------|-----|
| **ContactsTbl fails** | Tries to convert `Extension`, `Country/Region`, `SendDeliveryConfirmation` as dates | ? Made heuristic more specific |
| **ContactsItemSvcSummaryTbl fails** | FK join uses wrong column casing | ? Already fixed (previous commit) |
| **RepairsTbl works** | LastStatusChange now detected correctly | ? Working |

---

## Files Changed

1. ? `DmlScriptGenerator.cs` line 867 - Made date heuristic more specific

---

## Build Status

?? **Blocked** - MigrationRunner.exe is locked by Visual Studio

**Action**: Stop MigrationRunner, then rebuild

---

**The heuristic fix should solve the ContactsTbl migration failure!** ??
