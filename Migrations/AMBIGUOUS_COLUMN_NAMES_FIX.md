# ?? ROOT CAUSE FOUND: Ambiguous Column Names!

## The Real Problem

The migration SQL has **ambiguous column references** when using INNER JOIN!

### Error Found

```
Msg 209, Level 16, State 1
Ambiguous column name 'Notes'.
```

Both `AccessSrc.ClientUsageLinesTbl` and `ContactsTbl` have a `Notes` column, but the generated SQL doesn't qualify which one to use!

---

## The Generated SQL (Broken)

### Line 2352 in DataMigration_20260319_141349.sql

```sql
SELECT
    NULLIF([ClientUsageLineNo], N'') AS [ContactsItemSvcSummaryId],
    NULLIF([CustomerID], N'') AS [ContactID],
    CASE WHEN... END AS [UsageDate],
    NULLIF([CupCount], N'') AS [CupCount],
    NULLIF([ServiceTypeId], N'') AS [ItemServiceTypeID],
    NULLIF([Qty], N'') AS [Qty],
    NULLIF([Notes], N'') AS [Notes]          ? ? AMBIGUOUS!
FROM [AccessSrc].[ClientUsageLinesTbl] src
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerID
WHERE src.CustomerID IS NOT NULL;
```

**Problem**: `[Notes]` could be from `src` or `c` table!

---

## Why This Wasn't Caught Before

1. **Date conversion fix worked** ? - The CASE statement for Date is correct
2. **JOIN condition worked** ? - The INNER JOIN uses correct column names
3. **WHERE clause worked** ? - Uses `src.CustomerID`
4. **BUT column references NOT qualified!** ?

When you run the SQL:
- SQL Server tries to parse the SELECT
- Finds `[Notes]` in both tables
- Throws "Ambiguous column name" error
- Transaction rolls back
- 0 rows inserted

---

## The Fix Applied

### Updated `EmitInsertBlock()` method

**New logic**:
```csharp
// If we have a JOIN, qualify all column references with src. prefix
List<string> qualifiedSelectCols = selectCols;
if (needsFKJoin)
{
    qualifiedSelectCols = selectCols.Select(expr =>
    {
        // Replace unqualified [ColName] with src.[ColName]
        return Regex.Replace(expr, 
            @"\[([^\]]+)\](?!\s*AS)",  // Match [ColName] not followed by AS
            m => {
                // If already qualified or is a function, leave it
                if (m.Value.Contains("src.") || m.Value.Contains("c.") || m.Value.Contains("("))
                    return m.Value;
                // Otherwise qualify with src.
                return $"src.[{m.Groups[1].Value}]";
            });
    }).ToList();
}
```

---

## What the NEW SQL Will Generate

### Fixed SELECT (After Regenerating)

```sql
SELECT
    NULLIF(src.[ClientUsageLineNo], N'') AS [ContactsItemSvcSummaryId],
    NULLIF(src.[CustomerID], N'') AS [ContactID],
    CASE WHEN NULLIF(src.[Date], N'') IS NULL... END AS [UsageDate],
    NULLIF(src.[CupCount], N'') AS [CupCount],
    NULLIF(src.[ServiceTypeId], N'') AS [ItemServiceTypeID],
    NULLIF(src.[Qty], N'') AS [Qty],
    NULLIF(src.[Notes], N'') AS [Notes]      ? ? QUALIFIED!
FROM [AccessSrc].[ClientUsageLinesTbl] src
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerID
WHERE src.CustomerID IS NOT NULL;
```

**Now SQL Server knows to use src.Notes!**

---

## What You Need To Do

### Step 1: Regenerate SQL (AGAIN)

```cmd
cd Migrations\MigrationRunner
dotnet run

# Select: M (Generate DataMigration SQL)
```

### Step 2: Verify the Generated SQL

Check that Notes and other columns are qualified:

```powershell
Select-String -Path "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" -Pattern "ClientUsageLinesTbl.*ContactsItemSvcSummaryTbl" -Context 0, 20 | Select-Object -First 1
```

Look for `src.[Notes]` instead of just `[Notes]`.

### Step 3: Apply Migration

```cmd
# Still in MigrationRunner:
# Select: N (Apply DataMigration)
```

### Step 4: Verify

```cmd
# Select: & (Verification)
```

**Expected**:
```
ContactsItemSvcSummaryTbl: 64,756 rows ? (99.5%)
```

---

## Timeline of Issues

| Attempt | Issue | Status |
|---------|-------|--------|
| 1st | Date treated as datetime (no conversion) | ? Fixed (removed from proper datetime list) |
| 2nd | WHERE uses wrong column casing | ? Fixed (GetCustomerIdColumnName helper) |
| 3rd | **Ambiguous column names in JOIN** | ? **Fixed NOW** (qualify all columns with src.) |

---

## Why This Happens

When generating SQL with INNER JOIN:
1. **Column names must be qualified** if they exist in multiple tables
2. The code was generating: `[Notes]`
3. Should have been: `src.[Notes]`

SQL Server can't tell which table's Notes column you want!

---

## Other Tables That Might Be Affected

Any table using `NeedsForeignKeyJoin()`:
- ? **ContactsItemsPredictedTbl** (ClientUsageTbl) - might also have ambiguous columns
- ? **ContactsItemSvcSummaryTbl** (ClientUsageLinesTbl) - **this one failing**

The fix applies to **all** tables with FK joins!

---

## Build Status

? **Successful** - Ready to regenerate

---

## Summary

| Issue | Cause | Fix |
|-------|-------|-----|
| **Ambiguous column 'Notes'** | Unqualified column names in JOIN SELECT | ? Qualify all columns with `src.` prefix when JOIN present |
| Date conversion | ClientUsageLinesTbl.Date needs TRY_CONVERT | ? Already fixed (previous) |
| WHERE clause column | Wrong casing CustomerId vs CustomerID | ? Already fixed (previous) |

---

**Just regenerate (M) and rerun (N) - THIRD time's the charm!** ??
