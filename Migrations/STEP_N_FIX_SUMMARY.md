# Step N Data Migration Fix Summary

## Problems Identified

### 1. **Malformed SQL Error Handling** (FIXED)
The original code generated SQL with error handling all on one line:
```sql
BEGIN TRY DELETE FROM [Table]; END TRY BEGIN CATCH PRINT N'ERROR: ...' END CATCH
```
This caused error messages to be printed as literal SQL code rather than executed.

### 2. **Incorrect TRUNCATE vs DELETE Logic** (FIXED)
The purge logic was checking if other tables reference the target table, but:
- SQL Server TRUNCATE won't work even on disabled foreign keys if FK constraints exist
- The check was backwards (checking if OTHER tables reference this one, not if this table has FKs)
- Since FKs are already disabled in the first section, we should just use DELETE

### 3. **Primary Key Violations** (ROOT CAUSE)
Your actual errors were:
- `ContactsItemUsageTbl` - duplicate key value (1)
- `ContactUsageLinesTbl` - duplicate key value (0)

These happened because the **purge was failing silently** - tables still had data when the INSERT with IDENTITY_INSERT ON was attempted.

## Changes Made

### File: `Migrations/MigrationRunner/DmlScriptGenerator.cs`

#### Change 1: Simplified Purge Logic (Lines 137-164)
**Before:**
```csharp
// Complex TRUNCATE vs DELETE logic with nested fallback
sb.AppendLine($"    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk");
sb.AppendLine($"                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id");
sb.AppendLine($"                   WHERE rt.name = N'{tEsc}')");
sb.AppendLine($"        TRUNCATE TABLE {Qi(t)};");
sb.AppendLine("    ELSE");
// ... complex fallback logic
```

**After:**
```csharp
// Simple: Always use DELETE since FKs are already disabled
sb.AppendLine("-- Using DELETE instead of TRUNCATE because SQL Server TRUNCATE checks for FK existence even when disabled");
sb.AppendLine($"PRINT N'Purging {Qi(t)}';");
sb.AppendLine("BEGIN TRY");
sb.AppendLine($"    DELETE FROM {Qi(t)};");
sb.AppendLine($"    PRINT N'Successfully purged {Qi(t)}';");
sb.AppendLine("END TRY");
sb.AppendLine("BEGIN CATCH");
sb.AppendLine($"    PRINT N'ERROR: Failed to purge {Qi(t)}: ' + ERROR_MESSAGE();");
sb.AppendLine($"    PRINT N'      This may indicate foreign keys were not properly disabled.';");
sb.AppendLine("END CATCH");
```

#### Change 2: Removed Dead Code (Lines 658-664)
Removed unused `FindConstraints` overload that referenced non-existent `TableIndex` property.

## What This Fix Does

1. **Clearer Error Messages**: You'll now see actual SQL error messages instead of malformed SQL code
2. **Reliable Purge**: Always uses DELETE which works with disabled foreign keys
3. **Better Diagnostics**: Clear messages when purge succeeds or fails

## Next Steps

### 1. Regenerate the DataMigration Script
Run **Option M** in MigrationRunner to regenerate `DataMigration_LATEST.sql` with the fixed logic.

### 2. Check the Generated SQL
Review `Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql` to verify:
- Foreign keys are disabled (should see `ALTER TABLE ... NOCHECK CONSTRAINT` commands)
- Purge section uses simple DELETE statements
- Error messages are properly formatted

### 3. Run the Migration
Run **Option N** to apply the data migration.

### 4. Review the Log
Check `Data\Metadata\PlanEdits\Logs\ApplyData_[timestamp].log` for:
- **Success indicators**: "Successfully purged [TableName]"
- **Error indicators**: "ERROR: Failed to purge [TableName]" with actual SQL error details
- **Data loading**: "OK migrate [TableName]" messages

## Troubleshooting

### If Purge Still Fails

**Check for:**
1. **Active transactions** holding locks on tables
2. **Triggers** that might be preventing DELETE
3. **Computed columns** or **indexed views** that reference the tables
4. **Replication** or **Change Data Capture (CDC)** enabled on tables

**To diagnose:**
```sql
-- Check for triggers
SELECT t.name AS TableName, tr.name AS TriggerName, tr.is_disabled
FROM sys.triggers tr
JOIN sys.tables t ON t.object_id = tr.parent_id
WHERE t.name IN ('ContactsItemUsageTbl', 'ContactUsageLinesTbl', ...);

-- Check for locks
SELECT * FROM sys.dm_tran_locks WHERE resource_type = 'OBJECT';

-- Check for active foreign keys
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    fk.is_disabled,
    fk.is_not_trusted
FROM sys.foreign_keys fk
WHERE OBJECT_NAME(fk.parent_object_id) IN ('ContactsItemUsageTbl', 'ContactUsageLinesTbl', ...);
```

### If You Still Get Duplicate Key Errors

This means the purge succeeded BUT some data remains. This could happen if:
1. **The purge ran in a separate transaction** that was rolled back
2. **SET XACT_ABORT is causing transaction issues**

**Solution**: Manually clear the tables before running Step N:
```sql
-- Disable all FKs first
-- Then manually delete from each table in reverse dependency order
DELETE FROM [UsedItemGroupsTbl];
DELETE FROM [TransactionTypesTbl];
-- ... etc
```

## Verification

After successful migration, verify:
1. All target tables have data: `SELECT COUNT(*) FROM [TableName]`
2. Foreign keys are re-enabled: Check the end of the log for "ALTER TABLE ... WITH CHECK CHECK CONSTRAINT"
3. No orphan records: The script includes orphan checks at the end

## Additional Notes

- The migration script disables FKs, purges tables, loads data, then re-enables FKs
- IDENTITY_INSERT is temporarily enabled for tables with identity columns
- The script attempts to re-enable FKs WITH CHECK (validating data); if that fails, it enables WITHOUT CHECK and reports warnings
- All operations are logged to help diagnose issues
