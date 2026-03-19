# ? QUICK FIX - Run This Now!

## Problem
The migration failed because `DataMigration_LATEST.sql` has a bug. The code is fixed, but you need to **regenerate the SQL file**.

## Solution (Choose One)

### Option A: Automated (Easiest) ?
```powershell
cd C:\SRC\ASP.net\TrackerSQL\Migrations
.\RunFix.ps1
```

This script will:
1. Build with fixed code
2. Backup old SQL
3. Regenerate DataMigration_LATEST.sql
4. Verify the fix was applied
5. Offer to run migration

### Option B: Manual (Full Control)
```cmd
cd C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner
dotnet build
dotnet run
# Select: $ (Full pipeline)
# Or: M then N (Generate then Apply)
```

## Why This is Needed

? Code is fixed (DmlScriptGenerator.cs)  
? But SQL file is OLD (generated before fix)  
?? You're running OLD SQL ? same error!

## What The Fix Does

**Before** (broken SQL):
```sql
NULLIF([NextCoffee], N'') AS [NextCoffee]  ? Tries to compare datetime with text
```

**After** (fixed SQL):
```sql
[NextCoffee]  ? Uses datetime directly
```

## Expected Result

```
? All 45 tables migrated
? Success rate: 100% (instead of 6.7%)
? No datetime conversion errors
```

## Verification

After running, check:
```sql
SELECT COUNT(*) FROM ContactsTbl;           -- Should be ~2,900
SELECT COUNT(*) FROM ItemsTbl;              -- Should be ~279
SELECT COUNT(*) FROM ContactsItemsPredictedTbl;  -- Should be ~2,136
```

---

**Just run `.\RunFix.ps1` and follow the prompts!** ??
