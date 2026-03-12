# Migration Table and Column Renaming Guide

## Overview
This migration applies consistent naming conventions:
- Rename usage-related tables for clarity
- Rename `MachineSN` to `EquipmentSN` everywhere
- Ensure all `Customer/Client` references become `Contact`

## Table Renames

### Usage Tables (Primary Focus)
| Source Table (Access) | Target Table (SQL Server) | Purpose |
|----------------------|---------------------------|---------|
| `ClientUsageTbl` | `ContactsItemsPredictedTbl` | Usage predictions & calculations (NextCoffeeBy, averages, etc.) |
| `ClientUsageLinesTbl` | `ContactsUsageTbl` | Usage tracking lines (CupCount, ServiceTypeID, dates) |
| `ItemUsageTbl` | `ContactsItemUsageTbl` | Items delivered/used (ItemProvidedID, QtyProvided, etc.) |

### Key Differences
- **ContactsItemsPredictedTbl**: One row per contact - stores PREDICTIONS
- **ContactsUsageTbl**: Many rows per contact - stores ACTUAL USAGE EVENTS
- **ContactsItemUsageTbl**: Many rows per contact - stores ITEMS DELIVERED

## Column Renames

### Equipment References
- `MachineSN` ? `EquipmentSN` (in ContactsTbl and any other tables)

### ID Field Standardization
- All `CustomerID` ? `ContactID`
- All `ClientID` ? `ContactID`
- `ClientUsageLineNo` ? `ContactUsageLineNo`

## Primary Keys Fixed

### ContactsItemsPredictedTbl
- **Primary Key**: `ContactID` (NOT identity)
- **Identity**: None (ContactID is FK to ContactsTbl)
- One row per contact

### ContactsUsageTbl
- **Primary Key**: `ContactUsageLineNo` (identity)
- **Identity**: `ContactUsageLineNo`
- Multiple rows per contact

### ContactsItemUsageTbl
- **Primary Key**: `ContactUsageLineNo` (identity)
- **Identity**: `ContactUsageLineNo`
- Multiple rows per contact

## Migration Files Updated

1. **PlanConstraints.json** - Table definitions and primary keys
2. **BulkRenameRules.json** - Automated rename rules
3. **PlanColumns.csv** - Column-specific renames

## How to Run Migration

### Step 1: Run MigrationRunner
```cmd
cd Migrations\MigrationRunner
dotnet run
```

### Step 2: Select Options in Order
1. Export Access Schema (if not done)
2. Apply Bulk Renames
3. Generate DDL Scripts (CREATE TABLE)
4. Generate DML Scripts (INSERT DATA)
5. Review generated SQL files
6. Run scripts on SQL Server

### Step 3: Verify Changes
Check that tables are created with new names:
- `ContactsItemsPredictedTbl`
- `ContactsUsageTbl`
- `ContactsItemUsageTbl`

Check that column `EquipmentSN` exists in `ContactsTbl`

## Code Changes Required After Migration

After running the migration, update the C# application code:

### 1. Rename POCOs (Classes/Poco/)
- `ContactsUsage.cs` ? `ContactsItemsPredicted.cs`
- `ContactUsageLine.cs` ? `ContactUsage.cs`
- Update all property names (MachineSN ? EquipmentSN)

### 2. Rename Repositories (Classes/Sql/)
- `ContactsUsageRepository.cs` ? `ContactsItemsPredictedRepository.cs`
- `ContactUsageLinesRepository.cs` ? `ContactsUsageRepository.cs`
- Update TableName properties

### 3. Update All References
- Search for `ContactsUsageTbl` ? replace with `ContactsItemsPredictedTbl`
- Search for `ContactUsageLinesTbl` ? replace with `ContactsUsageTbl`
- Search for `MachineSN` ? replace with `EquipmentSN`

## Rollback Plan

If migration fails, restore from backup and review error logs in:
- `Migrations/MigrationRunner/Metadata/PlanChanges/`
- Generated SQL scripts in `Data/Metadata/PlanEdits/Sql/`

## Notes

- The BulkRenameRules.json applies token replacements automatically
- PlanConstraints.json ensures correct primary keys
- All old migration SQL files are timestamped and kept for reference
