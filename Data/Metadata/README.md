# TrackerSQL Database Migration Documentation

## Overview

This directory contains the metadata and migration scripts for transitioning from Microsoft Access to SQL Server. The migration involves renaming tables and columns to follow a more consistent naming convention.

## Key Migration Scripts

### 1. CreateTables_LATEST.sql
**Location:** `Data\Metadata\PlanEdits\Sql\CreateTables_LATEST.sql`

Creates all SQL Server tables with the new schema. This script:
- Drops existing tables (if specified)
- Creates 47 tables with proper data types
- Sets up primary keys
- Configures identity columns

### 2. DataMigration_LATEST.sql
**Location:** `Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql`

Migrates data from Access database to SQL Server:
- Handles 43 tables
- Maps old Access column names to new SQL Server names
- Converts data types appropriately
- Preserves identity values
- Includes orphan detection for foreign key integrity

### 3. AddForeignKeys_LATEST.sql
**Location:** `Data\Metadata\PlanEdits\Sql\AddForeignKeys_LATEST.sql`

Adds foreign key constraints after data migration:
- Creates referential integrity constraints
- Links normalized tables (e.g., OrderLinesTbl to OrdersTbl)

## Important Naming Conventions

### Access Database ? SQL Server Mapping

The migration follows these naming patterns:

| Concept | Access Naming | SQL Server Naming | Example |
|---------|--------------|-------------------|---------|
| **Customers** | Customer* | Contact* | CustomersTbl ? ContactsTbl |
| **Clients** | Client* | Contact* | ClientUsageTbl ? ContactsUsageTbl |
| **Cities/Locations** | City* | Area* | CityTbl ? AreasTbl |
| **Roasting/Prep** | Roast* | Prep* | RoastDate ? PrepDate |
| **Machines** | Machine* | Equip* | MachineConditionsTbl ? EquipConditionsTbl |
| **Coffee Items** | ItemType* | Item* | ItemTypeTbl ? ItemsTbl |
| **Packaging** | Packaging* | ItemPackaging* | PackagingTbl ? ItemPackagingsTbl |
| **Prep Types** | PrepType* | ItemPrepType* | PrepTypesTbl ? ItemPrepTypesTbl |
| **Service Types** | ServiceType* | ItemServiceType* | ServiceTypesTbl ? ItemServiceTypesTbl |
| **People** | Person* | People* | PersonsTbl ? PeopleTbl |
| **Recurring** | Reoccur* | Recurr* | ReoccuranceTypeTbl ? RecurranceTypesTbl |

### Critical Column Name Changes

#### ItemPackagingsTbl (formerly PackagingTbl)
```sql
-- Access Database
PackagingID        ? ItemPackagingID
Description        ? ItemPackagingDesc
AdditionalNotes    ? AdditionalNotes (unchanged)
Symbol             ? Symbol (unchanged)
Colour             ? Colour (unchanged)
BGColour           ? BGColour (unchanged)
```

#### ItemPrepTypesTbl (formerly PrepTypesTbl)
```sql
-- Access Database
PrepID             ? ItemPrepID
PrepType           ? ItemPrepType
IdentifyingChar    ? IdentifyingChar (unchanged)
```

#### ContactTypesTbl (formerly CustomerTypeTbl)
```sql
-- Access Database
CustTypeID         ? ContactTypeID
CustTypeDesc       ? ContactTypeDesc
Notes              ? Notes (unchanged)
```

#### PeopleTbl (formerly PersonsTbl)
```sql
-- Access Database
PersonID           ? PersonID (unchanged)
Person             ? Person (unchanged)
Abreviation        ? Abbreviation (note spelling correction)
Enabled            ? Enabled (unchanged)
NormalDeliveryDoW  ? NormalDeliveryDoW (unchanged)
SecurityUsername   ? SecurityUsername (unchanged)
```

#### EquipTypesTbl (formerly EquipTypeTbl)
```sql
-- Access Database
EquipTypeId        ? EquipTypeID
EquipTypeName      ? EquipTypeName (unchanged)
EquipTypeDesc      ? EquipTypeDescription
```

#### PaymentTermsTbl
```sql
-- No table name change, columns unchanged:
PaymentTermID, PaymentTermDesc, PaymentDays, DayOfMonth, UseDays, Enabled, Notes
```

#### PriceLevelsTbl
```sql
-- No table name change, columns unchanged:
PriceLevelID, PriceLevelDesc, PricingFactor, Enabled, Notes
```

## Repository Naming Standards

When creating or updating repository classes in `Classes\Sql\`, follow these conventions:

### Table Name Property
```csharp
protected override string TableName => "ItemPackagingsTbl";  // Use SQL Server table name
```

### Key Column Property
```csharp
protected override string KeyColumn => "ItemPackagingID";  // Use SQL Server primary key name
```

### Core Columns Property
```csharp
protected override string CoreColumns => "ItemPackagingID, ItemPackagingDesc, AdditionalNotes, Symbol, Colour, BGColour";
```

### Lookup Columns Property
```csharp
protected override string LookupColumns => "ItemPackagingID, ItemPackagingDesc";
```

### DO NOT Map Column Names in Code
? **WRONG** - Do not add custom mapping logic:
```csharp
// BAD - Don't do this!
public override List<ItemPackaging> GetAll(string SortBy)
{
    if (SortBy == "ItemPackagingDesc") SortBy = "Description";  // NO!
    // ...
}
```

? **CORRECT** - Let the base class handle it:
```csharp
// GOOD - Let RepositoryBase handle GetAll, GetById, etc.
public class ItemPackagingsRepository : RepositoryBase<ItemPackaging>
{
    protected override string TableName => "ItemPackagingsTbl";
    protected override string KeyColumn => "ItemPackagingID";
    protected override string CoreColumns => "ItemPackagingID, ItemPackagingDesc, AdditionalNotes, Symbol, Colour, BGColour";
    protected override string LookupColumns => "ItemPackagingID, ItemPackagingDesc";
    
    // Only add custom methods if absolutely necessary
}
```

## POCO Class Guidelines

POCO classes in `Classes\Poco\` should match SQL Server column names exactly:

```csharp
namespace TrackerDotNet.Classes.Poco
{
    public class ItemPackaging
    {
        public int ItemPackagingID { get; set; }          // Not PackagingID
        public string ItemPackagingDesc { get; set; }     // Not Description
        public string AdditionalNotes { get; set; }
        public string Symbol { get; set; }
        public int? Colour { get; set; }
        public string BGColour { get; set; }
    }
}
```

## Migration Strategy

### Phase 1: Schema Creation
1. Run `CreateTables_LATEST.sql` on SQL Server to create empty tables
2. Verify all tables and columns are created correctly

### Phase 2: Data Migration
1. Link Access database to SQL Server (optional, for AccessSrc schema approach)
2. Run `DataMigration_LATEST.sql` to copy data
3. Verify identity columns are seeded correctly
4. Check for orphaned records (script provides diagnostics)

### Phase 3: Constraints
1. Run `AddForeignKeys_LATEST.sql` to add referential integrity
2. Verify foreign key constraints

### Phase 4: Code Migration
1. Create POCO classes matching SQL Server schema
2. Create Repository classes inheriting from `RepositoryBase<T>`
3. Update application code to use new repositories
4. Test thoroughly

## Common Issues and Solutions

### Issue: "Invalid column name 'Description'"
**Cause:** Repository or code using old Access column name  
**Solution:** Update to use `ItemPackagingDesc` instead of `Description`

### Issue: "Invalid object name 'PackagingTbl'"
**Cause:** Code referencing old Access table name  
**Solution:** Update to use `ItemPackagingsTbl`

### Issue: Repository returns empty results
**Cause:** Custom GetAll/GetById methods with incorrect column mapping  
**Solution:** Remove custom methods and let `RepositoryBase<T>` handle them

### Issue: Foreign key constraint violations
**Cause:** Orphaned records in child tables  
**Solution:** Run orphan check section of `DataMigration_LATEST.sql` and clean up data

## Connection Strings

The application supports two connection string names:

1. **TrackerDataSQL** (preferred for SQL Server)
```xml
<connectionStrings>
  <add name="TrackerDataSQL" 
       connectionString="Data Source=SERVER;Initial Catalog=TrackerDB;Integrated Security=true;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

2. **Tracker08ConnectionString** (legacy, for backward compatibility)
```xml
<connectionStrings>
  <add name="Tracker08ConnectionString" 
       connectionString="Data Source=SERVER;Initial Catalog=TrackerDB;Integrated Security=true;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

The `TrackerSQLDb` class in `Classes\TrackerSQLDb.cs` handles this fallback logic.

## Access Schema Reference

Original Access database schema metadata is stored in:
- **Location:** `Data\Metadata\AccessSchema\`
- **Format:** JSON files describing Access table structure
- **Purpose:** Used by migration script generator to create mapping logic

## Testing Checklist

After migration, verify:

- [ ] All tables exist with correct names
- [ ] All columns exist with correct names and data types
- [ ] Identity columns are properly configured
- [ ] Foreign keys are in place
- [ ] Data counts match between Access and SQL Server
- [ ] No orphaned records (use diagnostic queries)
- [ ] POCO classes match SQL Server schema
- [ ] Repository classes use correct table/column names
- [ ] Application can read data successfully
- [ ] Application can insert/update/delete data successfully

## Troubleshooting Commands

### Check Table Existence
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
ORDER BY TABLE_NAME;
```

### Check Column Names for a Table
```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ItemPackagingsTbl'
ORDER BY ORDINAL_POSITION;
```

### Verify Identity Columns
```sql
SELECT t.name AS TableName, c.name AS IdentityColumn
FROM sys.identity_columns ic
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
JOIN sys.tables t ON t.object_id=c.object_id
ORDER BY t.name;
```

### Check Foreign Keys
```sql
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTableName,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumnName
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
ORDER BY TableName, ForeignKeyName;
```

## Version History

- **2025-01-13:** Latest schema with 47 tables, including RecurringOrdersTbl and RecurringOrderItemsTbl normalization
- **Initial:** Original Access database structure

## Support

For questions or issues with the migration:
1. Check this README first
2. Review the migration scripts in `Data\Metadata\PlanEdits\Sql\`
3. Consult the Access schema metadata in `Data\Metadata\AccessSchema\`
4. Review Git commit history for TrackerSQL repository

---
**Last Updated:** 2025-01-13  
**Maintained By:** Development Team  
**Repository:** https://github.com/wmachanik/TrackerSQL
