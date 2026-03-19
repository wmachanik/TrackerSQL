# Import-AccessClientUsageDates.ps1
# This script re-imports ClientUsageTbl from Access to SQL Server with proper date handling
# Addresses the issue where dates are NULL in AccessSrc.ClientUsageTbl

param(
    [Parameter(Mandatory=$false)]
    [string]$AccessDbPath = "C:\SRC\Data\QuaffeeTracker08.mdb",
    
    [Parameter(Mandatory=$false)]
    [string]$SqlServer = ".\SQLExpress",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "OtterDb",
    
    [Parameter(Mandatory=$false)]
    [string]$SqlUserId = "sa",
    
    [Parameter(Mandatory=$false)]
    [string]$SqlPassword = "5QL!Lilith477#",
    
    [Parameter(Mandatory=$false)]
    [switch]$BackupFirst = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$WhatIf = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Access ClientUsageTbl Date Import Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Validate Access database exists
if (-not (Test-Path $AccessDbPath)) {
    Write-Host "ERROR: Access database not found at: $AccessDbPath" -ForegroundColor Red
    exit 1
}

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Access DB: $AccessDbPath"
Write-Host "  SQL Server: $SqlServer"
Write-Host "  Database: $Database"
Write-Host "  Backup First: $BackupFirst"
Write-Host "  WhatIf Mode: $WhatIf"
Write-Host ""

try {
    # Connection strings
    $accessConnString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$AccessDbPath"
    $sqlConnString = "Server=$SqlServer;Database=$Database;User ID=$SqlUserId;Password=$SqlPassword;TrustServerCertificate=True"
    
    Write-Host "Step 1: Connecting to Access database..." -ForegroundColor Yellow
    $accessConn = New-Object System.Data.OleDb.OleDbConnection($accessConnString)
    $accessConn.Open()
    Write-Host "  Connected to Access database" -ForegroundColor Green
    
    Write-Host "Step 2: Reading ClientUsageTbl from Access..." -ForegroundColor Yellow
    $accessCmd = $accessConn.CreateCommand()
    $accessCmd.CommandText = "SELECT CustomerId, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount FROM ClientUsageTbl"
    
    $adapter = New-Object System.Data.OleDb.OleDbDataAdapter($accessCmd)
    $dataTable = New-Object System.Data.DataTable
    $rowCount = $adapter.Fill($dataTable)
    
    Write-Host "  Read $rowCount rows from Access" -ForegroundColor Green
    
    # Count non-null dates in Access
    $datesCount = 0
    foreach ($row in $dataTable.Rows) {
        if ($row["NextCoffeeBy"] -ne [DBNull]::Value) { $datesCount++ }
    }
    Write-Host "  Found $datesCount rows with NextCoffeeBy dates" -ForegroundColor Green
    
    $accessConn.Close()
    
    if ($WhatIf) {
        Write-Host ""
        Write-Host "WhatIf Mode: Would process $rowCount rows with $datesCount date values" -ForegroundColor Cyan
        Write-Host "Sample data:" -ForegroundColor Cyan
        $dataTable | Select-Object -First 5 CustomerId, NextCoffeeBy, NextCleanOn, NextFilterEst | Format-Table
        exit 0
    }
    
    Write-Host "Step 3: Connecting to SQL Server..." -ForegroundColor Yellow
    $sqlConn = New-Object System.Data.SqlClient.SqlConnection($sqlConnString)
    $sqlConn.Open()
    Write-Host "  Connected to SQL Server" -ForegroundColor Green
    
    # Backup if requested
    if ($BackupFirst) {
        Write-Host "Step 4: Creating backup..." -ForegroundColor Yellow
        $backupCmd = $sqlConn.CreateCommand()
        $backupCmd.CommandText = @"
IF OBJECT_ID('AccessSrc.ClientUsageTbl_BACKUP', 'U') IS NOT NULL
    DROP TABLE AccessSrc.ClientUsageTbl_BACKUP;
SELECT * INTO AccessSrc.ClientUsageTbl_BACKUP FROM AccessSrc.ClientUsageTbl;
"@
        $backupCmd.ExecuteNonQuery() | Out-Null
        Write-Host "  Backup created: AccessSrc.ClientUsageTbl_BACKUP" -ForegroundColor Green
    }
    
    Write-Host "Step 5: Clearing AccessSrc.ClientUsageTbl..." -ForegroundColor Yellow
    $clearCmd = $sqlConn.CreateCommand()
    $clearCmd.CommandText = "DELETE FROM AccessSrc.ClientUsageTbl"
    $deletedRows = $clearCmd.ExecuteNonQuery()
    Write-Host "  Deleted $deletedRows existing rows" -ForegroundColor Green
    
    Write-Host "Step 6: Inserting data with proper date handling..." -ForegroundColor Yellow
    
    $insertCmd = $sqlConn.CreateCommand()
    $insertCmd.CommandText = @"
INSERT INTO AccessSrc.ClientUsageTbl 
(CustomerId, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, 
 DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount)
VALUES 
(@CustomerId, @LastCupCount, @NextCoffeeBy, @NextCleanOn, @NextFilterEst, @NextDescaleEst, @NextServiceEst,
 @DailyConsumption, @FilterAveCount, @DescaleAveCount, @ServiceAveCount, @CleanAveCount)
"@
    
    # Add parameters
    [void]$insertCmd.Parameters.Add("@CustomerId", [System.Data.SqlDbType]::Int)
    [void]$insertCmd.Parameters.Add("@LastCupCount", [System.Data.SqlDbType]::Int)
    [void]$insertCmd.Parameters.Add("@NextCoffeeBy", [System.Data.SqlDbType]::DateTime)
    [void]$insertCmd.Parameters.Add("@NextCleanOn", [System.Data.SqlDbType]::DateTime)
    [void]$insertCmd.Parameters.Add("@NextFilterEst", [System.Data.SqlDbType]::DateTime)
    [void]$insertCmd.Parameters.Add("@NextDescaleEst", [System.Data.SqlDbType]::DateTime)
    [void]$insertCmd.Parameters.Add("@NextServiceEst", [System.Data.SqlDbType]::DateTime)
    [void]$insertCmd.Parameters.Add("@DailyConsumption", [System.Data.SqlDbType]::Float)
    [void]$insertCmd.Parameters.Add("@FilterAveCount", [System.Data.SqlDbType]::Int)
    [void]$insertCmd.Parameters.Add("@DescaleAveCount", [System.Data.SqlDbType]::Int)
    [void]$insertCmd.Parameters.Add("@ServiceAveCount", [System.Data.SqlDbType]::Int)
    [void]$insertCmd.Parameters.Add("@CleanAveCount", [System.Data.SqlDbType]::Float)
    
    $insertedCount = 0
    $errorCount = 0
    $progressInterval = [Math]::Max(1, [Math]::Floor($rowCount / 20))
    
    foreach ($row in $dataTable.Rows) {
        try {
            $insertCmd.Parameters["@CustomerId"].Value = if ($row["CustomerId"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["CustomerId"] }
            $insertCmd.Parameters["@LastCupCount"].Value = if ($row["LastCupCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["LastCupCount"] }
            $insertCmd.Parameters["@NextCoffeeBy"].Value = if ($row["NextCoffeeBy"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextCoffeeBy"] }
            $insertCmd.Parameters["@NextCleanOn"].Value = if ($row["NextCleanOn"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextCleanOn"] }
            $insertCmd.Parameters["@NextFilterEst"].Value = if ($row["NextFilterEst"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextFilterEst"] }
            $insertCmd.Parameters["@NextDescaleEst"].Value = if ($row["NextDescaleEst"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextDescaleEst"] }
            $insertCmd.Parameters["@NextServiceEst"].Value = if ($row["NextServiceEst"] -eq [DBNull]::Value) { [DBNull]::Value } else { [DateTime]$row["NextServiceEst"] }
            $insertCmd.Parameters["@DailyConsumption"].Value = if ($row["DailyConsumption"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["DailyConsumption"] }
            $insertCmd.Parameters["@FilterAveCount"].Value = if ($row["FilterAveCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["FilterAveCount"] }
            $insertCmd.Parameters["@DescaleAveCount"].Value = if ($row["DescaleAveCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["DescaleAveCount"] }
            $insertCmd.Parameters["@ServiceAveCount"].Value = if ($row["ServiceAveCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["ServiceAveCount"] }
            $insertCmd.Parameters["@CleanAveCount"].Value = if ($row["CleanAveCount"] -eq [DBNull]::Value) { [DBNull]::Value } else { $row["CleanAveCount"] }
            
            [void]$insertCmd.ExecuteNonQuery()
            $insertedCount++
            
            if ($insertedCount % $progressInterval -eq 0) {
                $pct = [Math]::Round(($insertedCount / $rowCount) * 100, 1)
                Write-Host "  Progress: $insertedCount / $rowCount rows ($pct%)" -ForegroundColor Gray
            }
        }
        catch {
            $errorCount++
            Write-Host "  ERROR on row $insertedCount (CustomerId: $($row['CustomerId'])): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host "  Inserted $insertedCount rows" -ForegroundColor Green
    if ($errorCount -gt 0) {
        Write-Host "  Errors: $errorCount" -ForegroundColor Red
    }
    
    Write-Host "Step 7: Verifying results..." -ForegroundColor Yellow
    $verifyCmd = $sqlConn.CreateCommand()
    $verifyCmd.CommandText = @"
SELECT 
    COUNT(*) AS TotalRows,
    SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS NextCoffeeBy_HasDates,
    SUM(CASE WHEN NextCleanOn IS NOT NULL THEN 1 ELSE 0 END) AS NextCleanOn_HasDates,
    SUM(CASE WHEN NextFilterEst IS NOT NULL THEN 1 ELSE 0 END) AS NextFilterEst_HasDates,
    SUM(CASE WHEN NextDescaleEst IS NOT NULL THEN 1 ELSE 0 END) AS NextDescaleEst_HasDates,
    SUM(CASE WHEN NextServiceEst IS NOT NULL THEN 1 ELSE 0 END) AS NextServiceEst_HasDates
FROM AccessSrc.ClientUsageTbl
"@
    $reader = $verifyCmd.ExecuteReader()
    if ($reader.Read()) {
        Write-Host "  Results in SQL Server:" -ForegroundColor Cyan
        Write-Host "    Total Rows: $($reader['TotalRows'])"
        Write-Host "    NextCoffeeBy with dates: $($reader['NextCoffeeBy_HasDates'])"
        Write-Host "    NextCleanOn with dates: $($reader['NextCleanOn_HasDates'])"
        Write-Host "    NextFilterEst with dates: $($reader['NextFilterEst_HasDates'])"
        Write-Host "    NextDescaleEst with dates: $($reader['NextDescaleEst_HasDates'])"
        Write-Host "    NextServiceEst with dates: $($reader['NextServiceEst_HasDates'])"
    }
    $reader.Close()
    
    $sqlConn.Close()
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Import completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Verify the dates look correct in SQL Server"
    Write-Host "2. If satisfied, delete ContactsItemsPredictedTbl data:"
    Write-Host "   DELETE FROM ContactsItemsPredictedTbl;"
    Write-Host "3. Re-run the migration section from DataMigration_LATEST.sql (lines 2415-2450)"
    Write-Host "   Or run the migration script below..."
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack Trace:" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}
