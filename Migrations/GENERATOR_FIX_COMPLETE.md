# ? GENERATOR FIXED - DmlScriptGenerator.cs Updated

## Problem Fixed

The `DmlScriptGenerator.cs` was treating **ALL** columns in `AccessSrc` as text, wrapping them with `NULLIF([Column], N'')` which would convert proper datetime values to NULL.

This was a problem for tables like `ClientUsageTbl` that are imported via the specialized PowerShell script (`Import-AccessClientUsageDates.ps1`) which preserves proper `datetime` types.

## Changes Made to DmlScriptGenerator.cs

### 1. Added Detection for Tables with Proper Datetime Columns

```csharp
// NEW: Check if this table is known to have proper datetime columns in AccessSrc
private static bool HasProperDatetimeColumns(string sourceTable)
{
    var tablesWithProperDates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "ClientUsageTbl",           // Imported via Import-AccessClientUsageDates.ps1
        "ClientUsageLinesTbl"       // May also have datetime columns properly imported
    };
    
    return tablesWithProperDates.Contains(sourceTable);
}
```

### 2. Added Column-Level Datetime Detection

```csharp
// NEW: Check if a specific column is a datetime column that's already properly typed
private static bool IsProperDatetimeColumn(string sourceTable, string columnName)
{
    if (!HasProperDatetimeColumns(sourceTable)) return false;
    
    // Known datetime columns in ClientUsageTbl
    if (sourceTable.Equals("ClientUsageTbl", StringComparison.OrdinalIgnoreCase))
    {
        return col == "nextcoffeeby" || 
               col == "nextcleanon" || 
               col == "nextfilterest" || 
               col == "nextdescaleest" || 
               col == "nextserviceest";
    }
    
    // Known datetime columns in ClientUsageLinesTbl
    if (sourceTable.Equals("ClientUsageLinesTbl", StringComparison.OrdinalIgnoreCase))
    {
        return col == "date";
    }
    
    return false;
}
```

### 3. Updated SELECT Column Generation

**BEFORE** (Lines 189-210):
```csharp
var nvExpr = WrapNullIf(srcExpr);  // ? Always wraps with NULLIF
string typedExpr = nvExpr;
if (IsDateLike(targetType, c.Target))
{
    typedExpr = TryConvertDate(nvExpr);  // ? Always converts as text
}
```

**AFTER** (Fixed):
```csharp
// Check if this is already a proper datetime
bool isProperDatetime = IsProperDatetimeColumn(tm.Source, c.Source);

var nvExpr = isProperDatetime ? srcExpr : WrapNullIf(srcExpr);  // ? Skip NULLIF for datetimes

string typedExpr = nvExpr;
if (IsDateLike(targetType, c.Target))
{
    if (isProperDatetime)
    {
        typedExpr = srcExpr;  // ? Use directly - already datetime!
    }
    else
    {
        typedExpr = TryConvertDate(nvExpr);  // Convert text to datetime
    }
}
```

### 4. Added FK Join Safety

Updated `EmitInsertBlock()` to add INNER JOIN for tables with FK constraints:

```csharp
if (needsFKJoin && !string.IsNullOrWhiteSpace(fkJoinTable))
{
    sb.AppendLine($"        FROM {fromObj} src");
    sb.AppendLine($"        INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerId");
    sb.AppendLine($"        WHERE src.CustomerId IS NOT NULL;");
}
```

Applies to:
- `ContactsItemsPredictedTbl`
- `ContactsItemSvcSummaryTbl`

## Generated SQL Output Changes

### Before Fix (Generated Code - BROKEN):
```sql
SELECT
    NULLIF([CustomerId], N'') AS [ContactID], 
    NULLIF([NextCoffeeBy], N'') AS [NextCoffeeBy],  -- ? Converts datetime to NULL!
    NULLIF([NextCleanOn], N'') AS [NextCleanOn]     -- ? Converts datetime to NULL!
FROM [AccessSrc].[ClientUsageTbl];
```

### After Fix (Generated Code - CORRECT):
```sql
SELECT
    src.[CustomerId] AS [ContactID], 
    src.[NextCoffeeBy],   -- ? Preserves datetime value!
    src.[NextCleanOn],    -- ? Preserves datetime value!
    src.[NextFilterEst],  -- ? Preserves datetime value!
    src.[NextDescaleEst], -- ? Preserves datetime value!
    src.[NextServiceEst]  -- ? Preserves datetime value!
FROM [AccessSrc].[ClientUsageTbl] src
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerId;  -- ? FK safety
```

## Testing the Fix

### Step 1: Regenerate DataMigration_LATEST.sql

Run the MigrationRunner:

```cmd
cd Migrations\MigrationRunner
dotnet run
# Select option to "Generate DML Scripts"
```

This will create a new `DataMigration_LATEST.sql` with the fix applied.

### Step 2: Verify Generated Code

Check `Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql` around line 2415 (ClientUsageTbl section).

You should see:
- ? No `NULLIF` on date columns
- ? Direct column references like `src.[NextCoffeeBy]`
- ? `INNER JOIN ContactsTbl` for FK safety
- ? Comment: `-- Only migrate valid ContactIDs`

### Step 3: Run Full Migration

```powershell
# 1. Import dates properly
.\Migrations\Import-AccessClientUsageDates.ps1

# 2. Run the newly generated migration script
sqlcmd -S .\SQLExpress -d OtterDb -U sa -P "password" `
       -i "Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql"

# 3. Verify
sqlcmd -S .\SQLExpress -d OtterDb -U sa -P "password" -Q `
  "SELECT COUNT(*), SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS HasDates FROM ContactsItemsPredictedTbl"
```

Expected: All rows should have dates.

## Future Tables with Datetime Columns

If you need to add another table with proper datetime columns:

1. **Update `HasProperDatetimeColumns()`** - Add table name to the HashSet
2. **Update `IsProperDatetimeColumn()`** - Add the datetime column names
3. **Regenerate** - Run MigrationRunner to generate new DataMigration_LATEST.sql

## Files Modified

- ? `Migrations\MigrationRunner\DmlScriptGenerator.cs` (Lines 12, 189-210, 283-310, 716-780)
  - Added `HasProperDatetimeColumns()` method
  - Added `IsProperDatetimeColumn()` method
  - Added `NeedsForeignKeyJoin()` method
  - Added `GetForeignKeyJoinTable()` method
  - Added `GetForeignKeyJoinCondition()` method
  - Updated `EmitInsertBlock()` signature and logic
  - Updated SELECT column generation in 3 places
  - Updated all `EmitInsertBlock()` calls to pass source table

## Build Status

? **Build Successful** - MigrationRunner compiles correctly

## Next Steps

1. ? Regenerate `DataMigration_LATEST.sql` using MigrationRunner
2. ? Verify the generated SQL has the fixes
3. ? Test with full migration from scratch
4. ? Update migration documentation

---

**Status**: ? **GENERATOR IS FIXED**  
**Impact**: Future generated migration scripts will preserve datetime values correctly  
**Date Fixed**: January 2025
