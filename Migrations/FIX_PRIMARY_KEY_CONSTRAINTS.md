# FIX FOR PRIMARY KEY ISSUES IN CREATE TABLES SCRIPT

## Problem Summary
The CREATE TABLE script was not generating correct primary keys for three tables:
1. `ContactUsageLinesTbl` - missing PK `ContactUsageLineNo`
2. `ContactsItemsPredictedTbl` - missing PK `ContactsItemsPredictedID`
3. `ContactsItemUsageTbl` - missing PK `ContactItemUsageLineNo`

## Root Cause
The `PlanConstraints.json` file had INCORRECT primary key definitions for these tables. 
All three tables were incorrectly set to use `ContactID` as their primary key, when they should have their own dedicated identity columns.

## Why This Happened
The CSV format in `TableMigrationReport-09-Mar-26.csv` is a **migration report format**, NOT an import format. The system does NOT read this CSV to generate CREATE TABLE scripts. Instead, it reads:
1. AccessSchema/*.schema.json files (for column definitions)
2. PlanConstraints.json (for PKs, FKs, identity columns)

The PlanConstraints.json was generated incorrectly, likely from an earlier partial import or manual editing.

## Solution

### Step 1: Run the Fix Script
```powershell
.\Migrations\FixPlanConstraintsPrimaryKeys.ps1
```

This script will:
- Backup your current PlanConstraints.json
- Fix the three incorrect primary key definitions
- Update the identity column definitions

### Step 2: Verify the Changes
Check `Data\Metadata\PlanEdits\PlanConstraints.json` and verify:

```json
{
  "Table": "ContactUsageLinesTbl",
  "PrimaryKey": ["ContactUsageLineNo"],
  "IdentityColumns": ["ContactUsageLineNo"],
  ...
}
{
  "Table": "ContactsItemsPredictedTbl",
  "PrimaryKey": ["ContactsItemsPredictedID"],
  "IdentityColumns": ["ContactsItemsPredictedID"],
  ...
}
{
  "Table": "ContactsItemUsageTbl",
  "PrimaryKey": ["ContactItemUsageLineNo"],
  "IdentityColumns": ["ContactItemUsageLineNo"],
  ...
}
```

### Step 3: Regenerate CREATE TABLE Script
Run MigrationRunner and:
1. Choose option **M** (Generate CREATE TABLE)
2. Verify the output includes:
```sql
CREATE TABLE [ContactUsageLinesTbl]
(
    [ContactUsageLineNo] INT IDENTITY(1,1) NOT NULL
    ...
    CONSTRAINT [PK_ContactUsageLinesTbl] PRIMARY KEY CLUSTERED ([ContactUsageLineNo])
);

CREATE TABLE [ContactsItemsPredictedTbl]
(
    [ContactsItemsPredictedID] INT IDENTITY(1,1) NOT NULL
    ...
    CONSTRAINT [PK_ContactsItemsPredictedTbl] PRIMARY KEY CLUSTERED ([ContactsItemsPredictedID])
);

CREATE TABLE [ContactsItemUsageTbl]
(
    [ContactItemUsageLineNo] INT IDENTITY(1,1) NOT NULL
    ...
    CONSTRAINT [PK_ContactsItemUsageTbl] PRIMARY KEY CLUSTERED ([ContactItemUsageLineNo])
);
```

### Step 4: Regenerate Data Migration Script
Run option **N** to regenerate the data migration script with the corrected primary keys.

## Additional Notes

### About the CSV Format
The `TableMigrationReport-09-Mar-26.csv` is a HUMAN-READABLE migration documentation format. It is NOT used by the migration tool to generate SQL scripts. If you want to import changes from CSV, you need to:

1. Export the current plan using option **L** (Export plan to CSV)
2. Edit the generated `PlanColumns.csv`, `PlanTables.csv`, etc.
3. Import using option **M** (Import plan from CSV)

The export format is different from the migration report format. The export uses these files:
- `PlanTables.csv` - columns: SourceTable, TargetTable, Classification, PreserveIds, Reviewed, Ignore
- `PlanColumns.csv` - columns: SourceTable, SourceColumn, TargetColumn, Action, Expression
- `PlanNormalize.csv` - columns: SourceTable, HeaderTable, LineTable, etc.
- `PlanNormalizeAssignments.csv` - columns: SourceTable, SourceColumn, Part, TargetColumn

### How to Update PlanConstraints.json Manually
If you need to manually update primary keys in the future:

1. Open `Data\Metadata\PlanEdits\PlanConstraints.json`
2. Find the table definition
3. Update `PrimaryKey` array with correct column name(s)
4. Update `IdentityColumns` array if it's an identity column
5. Save and regenerate scripts

Example:
```json
{
  "Table": "YourTableName",
  "PrimaryKey": ["YourPrimaryKeyColumn"],
  "IdentityColumns": ["YourPrimaryKeyColumn"],
  "ForeignKeys": [],
  "NotNullColumns": [],
  "ColumnTypes": {}
}
```

## Verification Checklist
- [ ] PlanConstraints.json has correct PK for ContactUsageLinesTbl
- [ ] PlanConstraints.json has correct PK for ContactsItemsPredictedTbl
- [ ] PlanConstraints.json has correct PK for ContactsItemUsageTbl
- [ ] CREATE TABLE script generates correct PKs
- [ ] Data migration script handles identity columns correctly
- [ ] Build succeeds without errors
