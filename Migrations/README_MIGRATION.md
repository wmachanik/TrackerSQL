# TrackerSQL Migration - Table and Column Renaming

## Quick Summary

This migration renames usage-related tables for better clarity and standardizes column names.

### Key Changes

| What | Old Name | New Name | Why |
|------|----------|----------|-----|
| **Table** | `ClientUsageTbl` | `ContactsItemsPredictedTbl` | Clarifies it stores predictions |
| **Table** | `ClientUsageLinesTbl` | `ContactsUsageTbl` | Main usage tracking table |
| **Table** | `ItemUsageTbl` | `ContactsItemUsageTbl` | Consistency with other tables |
| **Column** | `MachineSN` | `EquipmentSN` | Consistent terminology |

## Files Updated

### Migration Configuration Files
- ? `Data/Metadata/PlanEdits/PlanConstraints.json` - Primary keys fixed
- ? `Data/Metadata/BulkRenameRules.json` - Automated rename rules
- ? `Migrations/MigrationRunner/Metadata/BulkRenameRules.json` - Same rules for runner
- ? `Migrations/MigrationRunner/Metadata/PlanEdits/PlanColumns.csv` - Column renames

### Documentation
- ? `Migrations/MIGRATION_RENAME_GUIDE.md` - Complete guide
- ? `Migrations/POST_MIGRATION_CODE_UPDATES.md` - Code refactoring checklist
- ? `Data/Metadata/PlanEdits/Sql/VerifyMigrationRenames.sql` - Verification script

### Previous Fixes
- ? `Data/Metadata/PlanEdits/Sql/FixContactUsageTables.sql` - PK fix (now superseded)

## How to Run

### Step 1: Build MigrationRunner
```cmd
cd Migrations\MigrationRunner
dotnet build
```

### Step 2: Run Migration
```cmd
dotnet run
```

### Step 3: Follow Menu Options
1. Export Access Schema (if needed)
2. **Apply Bulk Renames** ?? This applies the new names
3. Generate DDL Scripts
4. Generate DML Scripts
5. Review generated SQL
6. Execute on SQL Server

### Step 4: Verify Database
Run the verification script:
```sql
-- Execute:
Data\Metadata\PlanEdits\Sql\VerifyMigrationRenames.sql
```

### Step 5: Update Application Code
Follow the checklist in:
```
Migrations\POST_MIGRATION_CODE_UPDATES.md
```

## Primary Key Corrections

The migration also fixes incorrect primary keys:

### ContactsItemsPredictedTbl
- **Before**: `ContactID` was IDENTITY (wrong!)
- **After**: `ContactID` is PK but NOT IDENTITY (correct - it's a FK)

### ContactsUsageTbl
- **Before**: `ContactID` was PK with IDENTITY (wrong!)
- **After**: `ContactUsageLineNo` is PK with IDENTITY (correct)

### ContactsItemUsageTbl
- **Before**: `ContactID` was PK with IDENTITY (wrong!)
- **After**: `ContactUsageLineNo` is PK with IDENTITY (correct)

## Why These Changes?

### Problem
1. **Confusing Names**: `ContactsUsageTbl` didn't clearly indicate it stores predictions
2. **Wrong Primary Keys**: Usage tables had ContactID as PK, preventing multiple rows per contact
3. **Inconsistent Naming**: `MachineSN` vs `Equipment` terminology mixed throughout

### Solution
1. **Clear Names**: 
   - `ContactsItemsPredictedTbl` = predictions/calculations
   - `ContactsUsageTbl` = actual usage events
   - `ContactsItemUsageTbl` = items delivered
2. **Correct PKs**: Each table has proper identity columns
3. **Standardized**: All equipment references use "Equipment", all customer/client refs use "Contact"

## Table Purposes After Migration

### ContactsItemsPredictedTbl (One row per contact)
Stores calculated/predicted usage metrics:
- LastCupCount
- NextCoffeeBy
- NextCleanOn
- NextFilterEst
- DailyConsumption
- Various averages

### ContactsUsageTbl (Many rows per contact)
Stores actual usage tracking events:
- ContactUsageLineNo (PK)
- UsageDate
- CupCount
- ItemServiceTypeID
- Qty
- Notes

### ContactsItemUsageTbl (Many rows per contact)
Stores items delivered:
- ContactUsageLineNo (PK)
- DeliveryDate
- ItemProvidedID
- QtyProvided
- ItemPrepTypeID
- ItemPackagingID

## Rollback

If something goes wrong:
1. Restore database from backup
2. Check error logs in `Migrations/MigrationRunner/Metadata/PlanChanges/`
3. Review generated SQL files before execution

## Support Files

All configuration is in:
- `Data/Metadata/` - Main metadata folder
- `Migrations/MigrationRunner/Metadata/` - Migration tool metadata
- Both folders are kept in sync

## Next Steps After Migration

1. ? Run migration
2. ? Verify database with verification script
3. ?? Update application code (see POST_MIGRATION_CODE_UPDATES.md)
4. ?? Test thoroughly
5. ? Commit changes to Git

## Questions?

Refer to:
- `MIGRATION_RENAME_GUIDE.md` - Detailed migration process
- `POST_MIGRATION_CODE_UPDATES.md` - Code refactoring guide
- `VerifyMigrationRenames.sql` - Database verification queries
