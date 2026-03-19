# Fixing NULL Dates in AccessSrc.ClientUsageTbl

## Problem Description

The date columns (`NextCoffeeBy`, `NextCleanOn`, `NextFilterEst`, `NextDescaleEst`, `NextServiceEst`) in `AccessSrc.ClientUsageTbl` are NULL even though the dates exist in the original Access database.

## Root Cause

The issue occurs during the **initial import** from Access to SQL Server, not during the `DataMigration_LATEST.sql` execution. The dates are lost at the import stage, which means:

1. **Access Database** ? Has dates ?
2. **Import to SQL Server** ? Dates lost ? (THIS IS THE PROBLEM)
3. **AccessSrc.ClientUsageTbl** ? NULL dates
4. **DataMigration_LATEST.sql** ? Copies NULL values to `ContactsItemsPredictedTbl`

## Diagnosis Steps

1. **Run the diagnostic script**:
   ```sql
   -- Execute this file
   Data\Metadata\PlanEdits\Sql\DiagnoseAndFixAccessSrcDates.sql
   ```

2. **Check the column data types**:
   - If columns are `VARCHAR/NVARCHAR` ? Dates were imported as text and need conversion
   - If columns are `DATETIME/DATETIME2` ? Import process failed to convert dates

3. **Verify Access database has dates**:
   - Open the Access database
   - Run: `SELECT * FROM ClientUsageTbl WHERE NextCoffeeBy IS NOT NULL`
   - Confirm dates are present in Access

## Solutions

### Option 1: Re-import from Access (Recommended)

**Use SQL Server Import and Export Wizard:**

1. Backup current data:
   ```sql
   SELECT * INTO AccessSrc.ClientUsageTbl_BACKUP FROM AccessSrc.ClientUsageTbl;
   DROP TABLE AccessSrc.ClientUsageTbl;
   ```

2. In SQL Server Management Studio:
   - Right-click on database ? Tasks ? Import Data
   - Source: Microsoft Access Database Engine
   - Browse to your Access .mdb/.accdb file
   - Destination: Your SQL Server database
   - Select `ClientUsageTbl` table
   - **IMPORTANT**: Click "Edit Mappings" and set:
     - `NextCoffeeBy` ? `datetime` (not varchar)
     - `NextCleanOn` ? `datetime`
     - `NextFilterEst` ? `datetime`
     - `NextDescaleEst` ? `datetime`
     - `NextServiceEst` ? `datetime`
   - Destination schema: `AccessSrc`
   - Run the import

3. Verify dates imported correctly:
   ```sql
   SELECT COUNT(*) AS TotalRows,
          SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS NextCoffeeBy_HasDates
   FROM AccessSrc.ClientUsageTbl;
   ```

### Option 2: Use SSMA (SQL Server Migration Assistant for Access)

1. Download and install SSMA for Access from Microsoft
2. Create a new SSMA project
3. Connect to both Access and SQL Server
4. Select `ClientUsageTbl` for migration
5. Review and adjust data type mappings (ensure dates map to datetime/datetime2)
6. Run the migration to `AccessSrc` schema

### Option 3: Fix Column Types if Already Text

If the columns are already imported as VARCHAR/NVARCHAR, use the conversion script in `DiagnoseAndFixAccessSrcDates.sql`:

```sql
-- This adds new datetime columns and converts the text values
-- See the commented section at the bottom of DiagnoseAndFixAccessSrcDates.sql
```

### Option 4: PowerShell Script to Re-import

Create a PowerShell script using ODBC:

```powershell
# Import-AccessDates.ps1
param(
    [string]$AccessDbPath = "C:\Path\To\Your\Database.accdb",
    [string]$SqlServer = "localhost",
    [string]$Database = "OtterDb"
)

# Connection strings
$accessConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$AccessDbPath"
$sqlConn = "Server=$SqlServer;Database=$Database;Integrated Security=True"

# Read from Access
$accessCmd = New-Object System.Data.OleDb.OleDbCommand
$accessCmd.Connection = New-Object System.Data.OleDb.OleDbConnection($accessConn)
$accessCmd.Connection.Open()
$accessCmd.CommandText = "SELECT CustomerId, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount FROM ClientUsageTbl"

$adapter = New-Object System.Data.OleDb.OleDbDataAdapter($accessCmd)
$dataTable = New-Object System.Data.DataTable
$adapter.Fill($dataTable)

# Clear SQL table
$sqlCmd = New-Object System.Data.SqlClient.SqlCommand
$sqlCmd.Connection = New-Object System.Data.SqlClient.SqlConnection($sqlConn)
$sqlCmd.Connection.Open()
$sqlCmd.CommandText = "DELETE FROM AccessSrc.ClientUsageTbl"
$sqlCmd.ExecuteNonQuery()

# Insert into SQL with proper date handling
$sqlCmd.CommandText = @"
INSERT INTO AccessSrc.ClientUsageTbl 
(CustomerId, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, 
 DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount)
VALUES 
(@CustomerId, @LastCupCount, @NextCoffeeBy, @NextCleanOn, @NextFilterEst, @NextDescaleEst, @NextServiceEst,
 @DailyConsumption, @FilterAveCount, @DescaleAveCount, @ServiceAveCount, @CleanAveCount)
"@

$sqlCmd.Parameters.Add("@CustomerId", [System.Data.SqlDbType]::Int) | Out-Null
$sqlCmd.Parameters.Add("@LastCupCount", [System.Data.SqlDbType]::Int) | Out-Null
$sqlCmd.Parameters.Add("@NextCoffeeBy", [System.Data.SqlDbType]::DateTime) | Out-Null
$sqlCmd.Parameters.Add("@NextCleanOn", [System.Data.SqlDbType]::DateTime) | Out-Null
$sqlCmd.Parameters.Add("@NextFilterEst", [System.Data.SqlDbType]::DateTime) | Out-Null
$sqlCmd.Parameters.Add("@NextDescaleEst", [System.Data.SqlDbType]::DateTime) | Out-Null
$sqlCmd.Parameters.Add("@NextServiceEst", [System.Data.SqlDbType]::DateTime) | Out-Null
$sqlCmd.Parameters.Add("@DailyConsumption", [System.Data.SqlDbType]::Float) | Out-Null
$sqlCmd.Parameters.Add("@FilterAveCount", [System.Data.SqlDbType]::Int) | Out-Null
$sqlCmd.Parameters.Add("@DescaleAveCount", [System.Data.SqlDbType]::Int) | Out-Null
$sqlCmd.Parameters.Add("@ServiceAveCount", [System.Data.SqlDbType]::Int) | Out-Null
$sqlCmd.Parameters.Add("@CleanAveCount", [System.Data.SqlDbType]::Float) | Out-Null

foreach ($row in $dataTable.Rows) {
    $sqlCmd.Parameters["@CustomerId"].Value = $row["CustomerId"]
    $sqlCmd.Parameters["@LastCupCount"].Value = if ($row["LastCupCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["LastCupCount"] }
    $sqlCmd.Parameters["@NextCoffeeBy"].Value = if ($row["NextCoffeeBy"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextCoffeeBy"] }
    $sqlCmd.Parameters["@NextCleanOn"].Value = if ($row["NextCleanOn"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextCleanOn"] }
    $sqlCmd.Parameters["@NextFilterEst"].Value = if ($row["NextFilterEst"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextFilterEst"] }
    $sqlCmd.Parameters["@NextDescaleEst"].Value = if ($row["NextDescaleEst"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextDescaleEst"] }
    $sqlCmd.Parameters["@NextServiceEst"].Value = if ($row["NextServiceEst"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextServiceEst"] }
    $sqlCmd.Parameters["@DailyConsumption"].Value = if ($row["DailyConsumption"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["DailyConsumption"] }
    $sqlCmd.Parameters["@FilterAveCount"].Value = if ($row["FilterAveCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["FilterAveCount"] }
    $sqlCmd.Parameters["@DescaleAveCount"].Value = if ($row["DescaleAveCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["DescaleAveCount"] }
    $sqlCmd.Parameters["@ServiceAveCount"].Value = if ($row["ServiceAveCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["ServiceAveCount"] }
    $sqlCmd.Parameters["@CleanAveCount"].Value = if ($row["CleanAveCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["CleanAveCount"] }
    
    $sqlCmd.ExecuteNonQuery() | Out-Null
}

$sqlCmd.Connection.Close()
$accessCmd.Connection.Close()

Write-Host "Import complete!"
```

## After Fixing AccessSrc

Once `AccessSrc.ClientUsageTbl` has proper dates:

1. **Delete data from ContactsItemsPredictedTbl**:
   ```sql
   DELETE FROM ContactsItemsPredictedTbl;
   ```

2. **Re-run the migration**:
   ```sql
   -- Run the ClientUsageTbl -> ContactsItemsPredictedTbl section from DataMigration_LATEST.sql
   ```

3. **Verify the dates**:
   ```sql
   SELECT COUNT(*) AS TotalRows,
          SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS NextCoffeeBy_HasDates,
          SUM(CASE WHEN NextCleanOn IS NOT NULL THEN 1 ELSE 0 END) AS NextCleanOn_HasDates
   FROM ContactsItemsPredictedTbl;
   ```

## Prevention for Future Migrations

When setting up new Access-to-SQL Server migrations:

1. Always use explicit data type mappings in your import tool
2. Test with a small dataset first
3. Verify date columns immediately after import
4. Document the import process for repeatability
5. Consider using SSMA for Access for better control over data type mappings

## Related Files

- `Data\Metadata\PlanEdits\Sql\DiagnoseAndFixAccessSrcDates.sql` - Diagnostic script
- `Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql` - Main migration script (lines 2400-2450)
- `Data\Metadata\PlanEdits\Sql\FixContactsItemsPredictedTbl_Dates.sql` - Post-migration fix (only if needed)
