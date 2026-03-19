# ?? PowerShell Scripts vs C# Code - What's Implemented Where?

## Quick Answer

**NO**, the C# code **DOES NOT** run the PowerShell scripts directly. However, the C# code **DOES IMPLEMENT** the same functionality that the PowerShell scripts provide.

---

## PowerShell Scripts in the Workspace

### 1. `Import-AccessClientUsageDates.ps1` (Standalone Tool)

**Location**: `Migrations\Import-AccessClientUsageDates.ps1`

**Purpose**: Re-imports `ClientUsageTbl` from Access with proper datetime handling

**What it does**:
- Reads ClientUsageTbl from Access (gets proper DateTime objects)
- Backs up AccessSrc.ClientUsageTbl
- Clears AccessSrc.ClientUsageTbl
- Re-inserts with SqlDbType.DateTime parameters
- Verifies dates were imported correctly

**Used by**: Manual user execution (not called by C# code)

**Status**: ?? **OBSOLETE** - The C# code now has this built-in

---

### 2. `RunFix.ps1` (Helper Script - Created by Copilot)

**Location**: `Migrations\RunFix.ps1`

**Purpose**: Automates the process of rebuilding and regenerating the migration

**What it does**:
- Builds MigrationRunner
- Backs up old DataMigration_LATEST.sql
- Runs option M to regenerate SQL
- Verifies the fix was applied

**Used by**: Manual user execution (convenience wrapper)

**Status**: ? **Helper tool** for user convenience

---

## C# Code Implementation

### ? The C# Code HAS the DateTime Import Built-In!

**File**: `Migrations\MigrationRunner\AccessStagingImporter.cs`

**Method**: `ImportClientUsageTableWithDates()` (lines 465-640)

**Called by**: `StageAll()` method (line 162) - **Automatic during staging!**

**What it does** (same as the PowerShell script):
```csharp
// Lines 465-640
private static void ImportClientUsageTableWithDates(OleDbConnection acc, SqlConnection sql, List<string> log)
{
    // 1. Check if ClientUsageTbl exists in Access
    if (!AccessTableExists(acc, tableName)) return;
    
    // 2. Read from Access with proper datetime handling
    var dataTable = new DataTable();
    using (var adapter = new OleDbDataAdapter(cmd))
    {
        adapter.Fill(dataTable);  // Gets DateTime objects
    }
    
    // 3. Backup existing data
    // Creates AccessSrc.ClientUsageTbl_BACKUP
    
    // 4. Clear existing data
    // DELETE FROM AccessSrc.ClientUsageTbl
    
    // 5. Insert with proper datetime types
    cmd.Parameters.Add("@NextCoffeeBy", SqlDbType.DateTime);
    cmd.Parameters.Add("@NextCleanOn", SqlDbType.DateTime);
    // ... insert all rows with proper DateTime parameters
    
    // 6. Verify import
    // Check that dates were imported correctly
}
```

**When it runs**: Automatically during `AccessStagingImporter.StageAll()` (step MS)

**Integration point** (lines 153-170):
```csharp
log.Add($"Summary: ok={ok} skipped={skipped} failed={fail}");

// AUTO-IMPORT: Special handling for ClientUsageTbl with proper datetime columns
Console.WriteLine("AUTO-IMPORTING ClientUsageTbl with proper datetime columns...");

try
{
    ImportClientUsageTableWithDates(acc, sql, log);  // ? C# implements PS1 functionality
    log.Add("SUCCESS: ClientUsageTbl imported with proper datetime columns");
}
catch (Exception ex)
{
    log.Add($"WARNING: Failed to import ClientUsageTbl dates: {ex.Message}");
}
```

---

## Comparison: PowerShell vs C# Implementation

| Feature | PowerShell Script | C# Implementation |
|---------|------------------|-------------------|
| **Reads from Access** | ? Yes (ADO.NET OleDb) | ? Yes (same) |
| **Gets proper DateTime** | ? Yes | ? Yes |
| **Backs up data** | ? Yes | ? Yes |
| **Clears AccessSrc** | ? Yes | ? Yes |
| **Inserts with SqlDbType.DateTime** | ? Yes | ? Yes |
| **Verifies import** | ? Yes | ? Yes |
| **Runs automatically** | ? No (manual) | ? **YES!** (during option $) |
| **Part of pipeline** | ? No | ? **YES!** |

---

## Why Both Exist?

### Timeline of Development

1. **Initially**: PowerShell script was created as a standalone fix
2. **You asked**: "Why must I run a separate script? That defeats the purpose!"
3. **I fixed it**: Integrated the same logic into `AccessStagingImporter.cs`
4. **Now**: The C# code has it built-in, PowerShell script is obsolete

### Current State

**PowerShell Script (`Import-AccessClientUsageDates.ps1`)**:
- ?? **Obsolete** - Can be deleted
- Was useful for testing/debugging
- No longer needed since C# has it built-in

**C# Code (`AccessStagingImporter.ImportClientUsageTableWithDates()`)**:
- ? **Active** - Used automatically
- Runs during step MS (Stage Access)
- Part of the full pipeline (option $)

---

## What Gets Called When You Run Option "$"

```
Option $ (Full Pipeline)
    ?
Step MS: StageAll()  (AccessStagingImporter.cs)
    ?? Standard CSV import for all tables
    ?  ??> ClientUsageTbl imported (as text)
    ?
    ?? ImportClientUsageTableWithDates()  ? C# method (not PowerShell!)
       ?? Read from Access (DateTime objects)
       ?? Backup AccessSrc.ClientUsageTbl
       ?? Clear AccessSrc.ClientUsageTbl
       ?? Insert with SqlDbType.DateTime
       ?? Verify dates imported
```

**No PowerShell scripts are called!** Everything is in C#.

---

## Code Evidence: C# Does NOT Call PowerShell

I searched the entire codebase for PowerShell execution:

```csharp
// Search results for: "Start-Process", "PowerShell", ".ps1"
```

**Result**: The MigrationRunner code **does not** call any PowerShell scripts using:
- `Process.Start()`
- `PowerShell.Create()`
- Or any other process execution

**Conclusion**: The C# code implements the functionality directly, not via external scripts.

---

## Summary

| Question | Answer |
|----------|--------|
| Does C# code run PS1 scripts? | ? **NO** |
| Does C# implement PS1 functionality? | ? **YES!** |
| Where is it implemented? | `AccessStagingImporter.ImportClientUsageTableWithDates()` |
| When does it run? | Automatically during step MS (part of option $) |
| Do I need to run PS1 scripts? | ? **NO** - C# does it automatically |
| Can I delete PS1 scripts? | ?? Optional - They're now redundant |

---

## Recommendations

### Keep These PowerShell Scripts
- ? `RunFix.ps1` - Useful helper for manual regeneration

### Can Delete These (Obsolete)
- ?? `Import-AccessClientUsageDates.ps1` - Now built into C# code

### The C# Code is Self-Contained
Just run:
```cmd
dotnet run
# Select: $ (Full pipeline)
```

Everything happens automatically, including the datetime import that used to require a separate PowerShell script!

---

**Bottom Line**: The C# code is **fully self-contained** and does **NOT** rely on external PowerShell scripts. The PowerShell scripts were created as standalone tools, but their functionality is now **built into the C# code** and runs automatically.
