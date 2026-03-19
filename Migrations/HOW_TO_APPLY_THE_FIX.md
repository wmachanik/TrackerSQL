# ?? STEP-BY-STEP: How to Apply the Fix

## Current Situation

? **Code is fixed** - `DmlScriptGenerator.cs` has the datetime fix  
? **But you're running OLD SQL** - `DataMigration_LATEST.sql` was generated BEFORE the fix  
? **Result**: Same error keeps happening

---

## Solution: Regenerate and Rerun

You MUST regenerate the SQL script with the fixed code. Here are your options:

### Option A: Quick Regenerate (Recommended)

```cmd
cd C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner
dotnet build
dotnet run
```

**In the menu:**
1. Select: **M** (Generate DataMigration SQL)
2. Wait for it to complete
3. Select: **N** (Apply DataMigration)
4. Check results

### Option B: Full Automated Pipeline (Best)

```cmd
cd C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner
dotnet build
dotnet run
```

**In the menu:**
1. Select: **$** (or **Y** - Full pipeline)
2. Enter connection strings when prompted
3. Say **Y** to drop existing tables (clean slate)
4. Wait for completion

This will:
- ? Drop and recreate all tables
- ? Stage Access data
- ? Auto-import ClientUsageTbl dates
- ? **Generate NEW DataMigration_LATEST.sql** (with fix!)
- ? Apply migration
- ? Verify all data

---

## Why This is Necessary

### Timeline of Events

1. **Earlier**: You ran option "$" with OLD code
   - Generated `DataMigration_LATEST.sql` with bug
   - Migration failed on TempCoffeecheckupCustomerTbl
   - Everything rolled back

2. **Just now**: I fixed the code
   - Updated `DmlScriptGenerator.cs`
   - Build successful ?
   - **But SQL file is still the OLD version!**

3. **Now**: You need to regenerate
   - Generate NEW `DataMigration_LATEST.sql` (with fix)
   - Run the NEW SQL
   - Success! ?

---

## Verification Steps

### Before Regenerating

Check the current DataMigration_LATEST.sql:

```powershell
Select-String -Path "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" -Pattern "TempCoffeecheckupCustomerTbl" -Context 0,5 | Select-Object -First 1
```

You'll see the OLD (broken) version:
```sql
NULLIF([NextCoffee], N'') AS [NextCoffee],  ? BUG!
```

### After Regenerating

Check again:
```powershell
Select-String -Path "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" -Pattern "TempCoffeecheckupCustomerTbl" -Context 0,5 | Select-Object -First 1
```

You should see the NEW (fixed) version:
```sql
[NextCoffee],  ? FIXED!
[NextClean],
[NextFilter],
```

---

## Quick Command Sequence

```cmd
cd C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner

REM Build with latest code
dotnet build

REM Run MigrationRunner
dotnet run

REM In the menu, select: $
REM Or manually: M then N
```

---

## Expected Results After Rerun

### Success Output
```
M) Generate DataMigration rc=0
   ? Generated with datetime fix

N) Apply DATA rc=0
   ? All 45 tables migrated successfully
   ? No datetime conversion errors

V) VERIFICATION
   ? ContactsItemsPredictedTbl: 2,136 rows with dates
   ? TempCoffeecheckupCustomerTbl: 6 rows migrated
   ? Success rate: 100%
```

### Verification Command
```sql
SELECT 
    (SELECT COUNT(*) FROM ContactsTbl) AS Contacts,
    (SELECT COUNT(*) FROM ItemsTbl) AS Items,
    (SELECT COUNT(*) FROM ContactsItemsPredictedTbl) AS Predicted,
    (SELECT COUNT(*) FROM TempCoffeecheckupCustomerTbl) AS TempCheckup
```

Expected:
- Contacts: ~2,900
- Items: ~279
- Predicted: ~2,136
- TempCheckup: 6

---

## Troubleshooting

### If Migration Still Fails

1. **Check the log file**:
   ```powershell
   Get-ChildItem "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Logs\ApplyData_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 50
   ```

2. **Verify the generated SQL**:
   ```powershell
   Select-String -Path "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" -Pattern "NULLIF.*NextCoffee" 
   ```
   
   Should return **NO RESULTS** (datetime columns shouldn't have NULLIF)

3. **Check build timestamp**:
   ```powershell
   Get-Item "C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner\bin\Debug\net48\MigrationRunner.exe" | Select-Object LastWriteTime
   ```
   
   Should be recent (after the fix)

---

## Summary

| Step | Action | Status |
|------|--------|--------|
| 1 | Code fix applied | ? Done |
| 2 | Build successful | ? Done |
| 3 | **Regenerate SQL** | ? **YOU NEED TO DO THIS** |
| 4 | Run migration | ? After step 3 |
| 5 | Verify results | ? After step 4 |

**Next Action**: Run `dotnet run` and select option **$** or **M** to regenerate the SQL!

---

**The fix is ready, but you need to regenerate the SQL script to use it!** ??
