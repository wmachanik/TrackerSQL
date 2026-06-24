# TrackerDb Migration Scripts - README

**Location:** `DevTools\Scripts\`  
**Purpose:** Automation tools for TrackerDb to TrackerSQLDb migration  
**Created:** 2025-05-14

---

## ?? Available Scripts

### 1. Find-TrackerDbUsages.ps1
**Purpose:** Analyze all TrackerDb usage in the codebase

**Usage:**
```powershell
.\DevTools\Scripts\Find-TrackerDbUsages.ps1
```

**Output:**
- Console report of all files with TrackerDb usage
- Detailed report saved to `DevTools\Documentation\TrackerDb_Usage_Report.txt`
- Categorizes files by type (Controls, Pages, Classes, etc.)
- Recommends migration priority

**When to use:** 
- At project start to understand scope
- Periodically to track progress
- To identify remaining files

---

### 2. Generate-PocoClasses.ps1
**Purpose:** Create missing POCO classes for table classes

**Usage:**
```powershell
# Analyze what's missing
.\DevTools\Scripts\Generate-PocoClasses.ps1

# Generate missing POCOs
.\DevTools\Scripts\Generate-PocoClasses.ps1 -GenerateFiles
```

**Parameters:**
- `-GenerateFiles` - Actually create the POCO files
- `-RootPath` - Override root path (default: current solution)
- `-ControlsPath` - Override controls folder (default: "Controls")
- `-PocoPath` - Override POCO folder (default: "Classes\Poco")

**Output:**
- List of missing POCO classes
- Generated POCO .cs files if `-GenerateFiles` used

**When to use:**
- Before starting migration
- When creating new repository classes
- To ensure all table classes have POCOs

---

### 3. Migrate-TrackerDbFile.ps1
**Purpose:** Migrate a single file from TrackerDb to TrackerSQLDb

**Usage:**
```powershell
# Dry run (preview changes)
.\DevTools\Scripts\Migrate-TrackerDbFile.ps1 -FilePath "Controls\AreaPrepDaysTbl.cs" -DryRun

# Execute migration
.\DevTools\Scripts\Migrate-TrackerDbFile.ps1 -FilePath "Controls\AreaPrepDaysTbl.cs"

# Skip backup
.\DevTools\Scripts\Migrate-TrackerDbFile.ps1 -FilePath "Controls\AreaPrepDaysTbl.cs" -CreateBackup:$false
```

**Parameters:**
- `-FilePath` (required) - Full or relative path to file
- `-DryRun` - Preview changes without modifying file
- `-CreateBackup` - Create backup before modifying (default: true)

**What it does:**
- ? Replaces `new TrackerDb()` with `using (var db = new TrackerSQLDb())`
- ? Replaces `.Close()` with `}`
- ? Marks parameter conversion spots with TODO
- ? Creates timestamped backup file

**What it does NOT do (manual steps required):**
- ? Convert SQL placeholders (? ? @ParamName)
- ? Create parameter lists (AddParams ? List<DBParameter>)
- ? Update method calls (ExecuteNonQuerySQL ? ExecuteNonQuery)
- ? Add error handling/logging

**When to use:**
- For individual file migration
- For testing automation on a single file
- When you want fine control

---

### 4. Migrate-AllTrackerDbFiles.ps1
**Purpose:** Batch migrate multiple files by phase

**Usage:**
```powershell
# Dry run Phase 1
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only -DryRun

# Execute Phase 1 migration
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only

# Execute Phase 2 migration
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase2Only

# Execute Phase 3 migration
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase3Only

# Execute Phase 4 migration
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase4Only

# Migrate ALL files at once (advanced)
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1
```

**Parameters:**
- `-Phase1Only` - Migrate only Phase 1 files (3 critical files)
- `-Phase2Only` - Migrate only Phase 2 files (5 high-impact files)
- `-Phase3Only` - Migrate only Phase 3 files (20 supporting files)
- `-Phase4Only` - Migrate only Phase 4 files (20+ lookup files)
- `-DryRun` - Preview changes without modifying files
- `-RootPath` - Override root path
- `-OutputLog` - Override log file path

**Output:**
- Summary of files processed
- Success/failure count
- Detailed log saved to `DevTools\Documentation\Migration_Log.txt`

**When to use:**
- For phase-by-phase migration
- When ready to migrate multiple files
- To track batch migration progress

---

### 5. Verify-Migration.ps1
**Purpose:** Check migration completeness and status

**Usage:**
```powershell
# Basic verification
.\DevTools\Scripts\Verify-Migration.ps1

# Detailed report
.\DevTools\Scripts\Verify-Migration.ps1 -DetailedReport
```

**Parameters:**
- `-DetailedReport` - Show all files in each category
- `-RootPath` - Override root path

**Output:**
- Overall migration percentage
- Count of files: Fully Migrated, In Progress, Not Started
- Categorized file lists (with -DetailedReport)

**When to use:**
- After each phase to verify progress
- To identify remaining work
- Before final testing
- To confirm 100% completion

---

## ?? Recommended Workflow

### Complete Migration Workflow

```powershell
# Step 1: Analyze current state
.\DevTools\Scripts\Find-TrackerDbUsages.ps1

# Step 2: Generate missing POCOs
.\DevTools\Scripts\Generate-PocoClasses.ps1 -GenerateFiles

# Step 3: Test Phase 1 migration (dry run)
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only -DryRun

# Step 4: Execute Phase 1 migration
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only

# Step 5: Manual parameter updates (see TODO comments in files)
#         - Update SQL placeholders (? ? @ParamName)
#         - Create parameter lists
#         - Update method calls
#         - Add error handling

# Step 6: Verify Phase 1
.\DevTools\Scripts\Verify-Migration.ps1 -DetailedReport

# Step 7: Test Phase 1 files
# - Build solution
# - Run unit tests
# - Manual testing

# Step 8: Commit Phase 1
git add .
git commit -m "Phase 1 migration complete"

# Step 9: Repeat for Phases 2, 3, 4
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase2Only
# ... manual updates, verify, test, commit

# Step 10: Final verification
.\DevTools\Scripts\Verify-Migration.ps1 -DetailedReport
```

---

## ?? Important Notes

### Automation Limitations

These scripts provide **semi-automated** migration. They handle:
- ? File backups
- ? Basic pattern replacements
- ? Progress tracking
- ? Verification

They do NOT handle (requires manual work):
- ? SQL query updates (? ? @ParamName)
- ? Parameter list creation
- ? Error handling
- ? Logging
- ? Business logic changes

**You MUST manually update files after running automation!**

---

### Safety Features

1. **Backups:** Automatic timestamped backups created
2. **Dry Run:** Test changes before applying
3. **Verification:** Check migration status anytime
4. **Logging:** Detailed logs of all operations

---

### Common Issues

**Issue:** Script won't run  
**Solution:** Check execution policy:
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

**Issue:** File not found  
**Solution:** Use full path or ensure working directory is correct:
```powershell
cd C:\SRC\ASP.net\TrackerSQL
```

**Issue:** Changes not as expected  
**Solution:** Always run with `-DryRun` first to preview

---

## ?? Script Dependencies

```
Find-TrackerDbUsages.ps1
  ?
  Reports on all TrackerDb usage

Generate-PocoClasses.ps1
  ?
  Creates missing POCO classes

Migrate-TrackerDbFile.ps1 ? Called by Migrate-AllTrackerDbFiles.ps1
  ?
  Migrates individual files

Migrate-AllTrackerDbFiles.ps1
  ?
  Batch migration by phase

Verify-Migration.ps1
  ?
  Checks completeness
```

---

## ?? Examples

### Example 1: Complete Phase 1 Migration

```powershell
# 1. See what will be done
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only -DryRun

# 2. Execute migration
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only

# 3. Manual updates in Visual Studio
# - Search for "TODO" in modified files
# - Update parameters and SQL
# - Add error handling

# 4. Verify
.\DevTools\Scripts\Verify-Migration.ps1

# 5. Test and commit
git add Controls\CustomersTbl.cs Controls\ItemTypeTbl.cs Controls\OrderDataControl.cs
git commit -m "Phase 1 migration complete"
```

---

### Example 2: Migrate Single File

```powershell
# Test on single file first
.\DevTools\Scripts\Migrate-TrackerDbFile.ps1 `
    -FilePath "Controls\AreaPrepDaysTbl.cs" `
    -DryRun

# Execute migration
.\DevTools\Scripts\Migrate-TrackerDbFile.ps1 `
    -FilePath "Controls\AreaPrepDaysTbl.cs"

# Manual updates...

# Verify single file
# (open in Visual Studio and check for old patterns)
```

---

### Example 3: Check Progress Anytime

```powershell
# Quick check
.\DevTools\Scripts\Verify-Migration.ps1

# Detailed report
.\DevTools\Scripts\Verify-Migration.ps1 -DetailedReport
```

---

## ?? Related Documentation

See `DevTools\Documentation\` for:
- Complete_Migration_Execution_Guide.md
- Quick_Reference_TrackerDb_Migration.md
- AreaPrepDaysTbl_Migration_Example.md
- Migration_Progress_Tracker.md

---

## ? Script Maintenance

### Adding New Patterns

To add new patterns to automation, edit `Migrate-TrackerDbFile.ps1`:

```powershell
$replacements = @{
    # Add new pattern here
    'OldPattern' = 'NewPattern'
}
```

### Adding New Phases

To add files to phases, edit `Migrate-AllTrackerDbFiles.ps1`:

```powershell
$phase1Files = @(
    "Controls\NewFile.cs"  # Add here
)
```

---

## ?? Troubleshooting

### Script Errors

**Error:** "Execution policy doesn't allow..."  
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

**Error:** "File not found..."  
```powershell
# Use full path
.\DevTools\Scripts\Migrate-TrackerDbFile.ps1 -FilePath "C:\SRC\ASP.net\TrackerSQL\Controls\File.cs"
```

**Error:** "Access denied..."  
```powershell
# Run as administrator or check file permissions
```

---

## ?? Support

For issues or questions:
1. Check this README
2. Review Complete_Migration_Execution_Guide.md
3. Check script comments
4. Review example usage above

---

**Good luck with your migration!** ??

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-05-14 | Initial script collection created |
