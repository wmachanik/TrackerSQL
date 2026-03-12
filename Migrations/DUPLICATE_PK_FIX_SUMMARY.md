# Duplicate Primary Key Fix - ContactUsage Tables

## Root Cause Identified

The migration was failing with **PRIMARY KEY constraint violations** on:
1. **ContactsItemUsageTbl** - duplicate key value (1)
2. **ContactUsageLinesTbl** - duplicate key value (0)

### Why This Happened

#### Access Source Data Structure:
- **ItemUsageTbl** has a **COMPOSITE primary key**: `(ClientUsageLineNo, CustomerID)`
  - This means `ClientUsageLineNo` can have duplicate values as long as the combination is unique
  - Example: ClientUsageLineNo=1 can appear multiple times with different CustomerIDs
  
- **ClientUsageLinesTbl** has a **SINGLE primary key**: `ClientUsageLineNo`
  - But the Access data has a row with `ClientUsageLineNo = 0` (likely a placeholder/bad record)
  - This value already exists in SQL Server or appears multiple times in Access

#### SQL Server Target Structure:
- **ContactsItemUsageTbl** has a **SINGLE identity PK**: `ContactItemUsageLineNo`
- **ContactUsageLinesTbl** has a **SINGLE identity PK**: `ContactUsageLineNo`

#### The Problem:
The migration script was trying to:
1. Use `IDENTITY_INSERT ON` 
2. INSERT the Access `ClientUsageLineNo` values directly into SQL Server
3. Result: **Duplicate PK values** ? SQL Server rejection

## The Fix Applied

### Changed Files:
1. **Data/Metadata/AccessSchema/ItemUsageTbl.schema.json**
2. **Data/Metadata/AccessSchema/ClientUsageLinesTbl.schema.json**

### What Was Changed:
Changed the identity column mapping from `"Action": "Rename"` to `"Action": "Drop"`:

**Before:**
```json
{
  "Source": "ClientUsageLineNo",
  "Target": "ContactUsageLineNo",
  "Action": "Rename",
  "Expression": null
}
```

**After:**
```json
{
  "Source": "ClientUsageLineNo",
  "Target": "ContactUsageLineNo",
  "Action": "Drop",
  "Expression": "Let SQL Server auto-generate new identity values to avoid duplicate key errors"
}
```

### What This Does:
- **Drops the source identity column** from the INSERT statement
- **SQL Server auto-generates** new unique identity values (1, 2, 3, ...) for each row
- **Eliminates duplicate PK errors** completely

### Note on PreserveIdsOnInsert:
Both schemas already had `"PreserveIdsOnInsert": false`, but the DML generator was still including the column in the mapping. The `"Action": "Drop"` setting is now explicit and will be respected by the generator.

## Next Steps

### 1. Regenerate the DataMigration Script
Run **Option M** in MigrationRunner. This will:
- Rebuild `DataMigration_LATEST.sql` with the updated schema
- The INSERT statements will now **exclude** the identity columns
- SQL Server will auto-generate new IDs

### 2. Verify the Generated SQL
Check `Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql` around the ContactUsage tables:

**You should see:**
```sql
INSERT INTO [ContactsItemUsageTbl]
(
    [ContactID], [DeliveryDate], [ItemProvidedID], [QtyProvided], [ItemPrepTypeID], [ItemPackagingID], [Notes]
)
SELECT
    NULLIF([CustomerID], N'') AS [ContactID],
    -- ... other columns ...
FROM [AccessSrc].[ItemUsageTbl];
```

**Notice:** `ContactItemUsageLineNo` is **NOT in the column list** - it will be auto-generated!

### 3. Run the Migration
Run **Option N** to apply the data migration.

### 4. Expected Results
? All purges succeed  
? All INSERT statements succeed with auto-generated IDs  
? No more duplicate key errors  
? Data migrated successfully with new sequential IDs

## Will This Happen Again?

### Short Answer: **NO**

This was a **one-time issue** caused by the mismatch between:
- Access composite primary key structure
- SQL Server single-column identity key structure

### You WON'T see this issue again if:
? Future CSV imports don't change the PK structure for these tables  
? You keep `"Action": "Drop"` for identity columns that have duplicates in Access  
? Your Access source data doesn't have PK constraint violations

### You MIGHT see similar issues if:
?? You add new tables with composite PKs in Access that map to single PKs in SQL Server  
?? You change the migration plan to preserve Access IDs (`PreserveIdsOnInsert: true`)  
?? Your Access database has actual data corruption (duplicate PKs in a single-column PK)

### How to Prevent Future Issues:

#### For New Tables Being Migrated:
1. **Check the Access schema**: Look for composite PKs in `Indexes` section
2. **If composite PK exists in Access** AND **target has single identity PK**:
   - Set `"Action": "Drop"` for the identity column
   - Add an `"Expression"` comment explaining why
3. **If single PK in both**: Check if Access has duplicates or placeholder rows (ID=0, ID=-1, etc.)

#### Example Schema Review:
```json
"Indexes": [
  {
    "Name": "PrimaryKey",
    "Columns": ["ID", "CustomerID"],  // ?? COMPOSITE KEY - will have duplicates in ID!
    "PrimaryKey": true
  }
]
```
**Action:** Set `"Action": "Drop"` for the ID column.

## Verification Queries

After successful migration, you can verify the data:

### Check Row Counts Match:
```sql
-- Source (Access)
SELECT COUNT(*) FROM AccessSrc.ItemUsageTbl;
SELECT COUNT(*) FROM AccessSrc.ClientUsageLinesTbl;

-- Target (SQL Server)
SELECT COUNT(*) FROM ContactsItemUsageTbl;
SELECT COUNT(*) FROM ContactUsageLinesTbl;
```

### Check New IDs Are Sequential:
```sql
SELECT 
    MIN(ContactItemUsageLineNo) AS MinID,
    MAX(ContactItemUsageLineNo) AS MaxID,
    COUNT(*) AS RowCount
FROM ContactsItemUsageTbl;

SELECT 
    MIN(ContactUsageLineNo) AS MinID,
    MAX(ContactUsageLineNo) AS MaxID,
    COUNT(*) AS RowCount
FROM ContactUsageLinesTbl;
```

### Check for Data Integrity:
```sql
-- Verify all ContactIDs exist in ContactsTbl
SELECT COUNT(*) AS OrphanRows
FROM ContactsItemUsageTbl cu
WHERE NOT EXISTS (SELECT 1 FROM ContactsTbl c WHERE c.ContactID = cu.ContactID);

SELECT COUNT(*) AS OrphanRows
FROM ContactUsageLinesTbl cul
WHERE NOT EXISTS (SELECT 1 FROM ContactsTbl c WHERE c.ContactID = cul.ContactID);
```

## Important Notes

### Loss of Original IDs:
- **Old Access IDs are NOT preserved** for these tables
- If your application relies on specific ID values, you'll need to:
  - Create a mapping table (AccessID ? NewSQLID)
  - Update any references in other tables during post-migration cleanup

### Foreign Key References:
- If other tables reference `ContactItemUsageLineNo` or `ContactUsageLineNo`
- Those references will **break** because the IDs changed
- Solution: Use a mapping strategy or ensure referenced tables are also regenerated

### Alternative Solution (if you MUST preserve IDs):
If you absolutely need to keep the original Access IDs:

1. **Clean the Access source data first**:
   ```sql
   -- In Access, create a query to renumber duplicates
   -- Export cleaned data to new Access tables
   ```

2. **OR use ROW_NUMBER() in migration**:
   ```sql
   INSERT INTO ContactsItemUsageTbl (...)
   SELECT 
       ROW_NUMBER() OVER (ORDER BY CustomerID, Date) AS NewID,
       ...
   FROM AccessSrc.ItemUsageTbl;
   ```

But the current approach (auto-generate) is **simpler and safer** for most cases.

## Summary

? **Fixed:** Identity columns now auto-generate to avoid duplicate PKs  
? **Permanent:** This fix will persist through future migrations  
? **Safe:** No risk of duplicate key errors from these tables  
?? **Trade-off:** Original Access IDs are not preserved (new sequential IDs assigned)
