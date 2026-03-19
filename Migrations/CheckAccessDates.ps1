# CheckAccessDates.ps1
# This script checks the ClientUsageTbl in the Access database to see if dates are actually present

param(
    [Parameter(Mandatory=$false)]
    [string]$AccessDbPath = "C:\SRC\Data\QuaffeeTracker08.mdb"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Access Database Date Column Diagnostics" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $AccessDbPath)) {
    Write-Host "ERROR: Access database not found at: $AccessDbPath" -ForegroundColor Red
    exit 1
}

Write-Host "Access Database: $AccessDbPath" -ForegroundColor Yellow
Write-Host ""

try {
    $accessConnString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$AccessDbPath"
    
    Write-Host "Connecting to Access database..." -ForegroundColor Yellow
    $accessConn = New-Object System.Data.OleDb.OleDbConnection($accessConnString)
    $accessConn.Open()
    Write-Host "  Connected successfully" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Querying ClientUsageTbl..." -ForegroundColor Yellow
    $cmd = $accessConn.CreateCommand()
    $cmd.CommandText = "SELECT CustomerId, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption FROM ClientUsageTbl ORDER BY CustomerId"
    
    $adapter = New-Object System.Data.OleDb.OleDbDataAdapter($cmd)
    $dataTable = New-Object System.Data.DataTable
    $rowCount = $adapter.Fill($dataTable)
    
    Write-Host "  Total rows: $rowCount" -ForegroundColor Green
    Write-Host ""
    
    # Analyze dates
    $stats = @{
        NextCoffeeBy = 0
        NextCleanOn = 0
        NextFilterEst = 0
        NextDescaleEst = 0
        NextServiceEst = 0
    }
    
    $sampleRows = @()
    
    foreach ($row in $dataTable.Rows) {
        if ($row["NextCoffeeBy"] -ne [DBNull]::Value) { $stats.NextCoffeeBy++ }
        if ($row["NextCleanOn"] -ne [DBNull]::Value) { $stats.NextCleanOn++ }
        if ($row["NextFilterEst"] -ne [DBNull]::Value) { $stats.NextFilterEst++ }
        if ($row["NextDescaleEst"] -ne [DBNull]::Value) { $stats.NextDescaleEst++ }
        if ($row["NextServiceEst"] -ne [DBNull]::Value) { $stats.NextServiceEst++ }
        
        if ($sampleRows.Count -lt 5 -and $row["NextCoffeeBy"] -ne [DBNull]::Value) {
            $sampleRows += [PSCustomObject]@{
                CustomerId = $row["CustomerId"]
                NextCoffeeBy = $row["NextCoffeeBy"]
                NextCleanOn = $row["NextCleanOn"]
                NextFilterEst = $row["NextFilterEst"]
                DataType = $row["NextCoffeeBy"].GetType().Name
            }
        }
    }
    
    Write-Host "Date Column Statistics:" -ForegroundColor Cyan
    Write-Host "  NextCoffeeBy  : $($stats.NextCoffeeBy) non-NULL" -ForegroundColor $(if ($stats.NextCoffeeBy -gt 0) { "Green" } else { "Red" })
    Write-Host "  NextCleanOn   : $($stats.NextCleanOn) non-NULL" -ForegroundColor $(if ($stats.NextCleanOn -gt 0) { "Green" } else { "Red" })
    Write-Host "  NextFilterEst : $($stats.NextFilterEst) non-NULL" -ForegroundColor $(if ($stats.NextFilterEst -gt 0) { "Green" } else { "Red" })
    Write-Host "  NextDescaleEst: $($stats.NextDescaleEst) non-NULL" -ForegroundColor $(if ($stats.NextDescaleEst -gt 0) { "Green" } else { "Red" })
    Write-Host "  NextServiceEst: $($stats.NextServiceEst) non-NULL" -ForegroundColor $(if ($stats.NextServiceEst -gt 0) { "Green" } else { "Red" })
    Write-Host ""
    
    if ($sampleRows.Count -gt 0) {
        Write-Host "Sample rows WITH dates:" -ForegroundColor Yellow
        $sampleRows | Format-Table -AutoSize
    }
    
    Write-Host "First 10 rows (all columns):" -ForegroundColor Yellow
    $dataTable | Select-Object -First 10 | Format-Table CustomerId, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, DailyConsumption -AutoSize
    Write-Host ""
    
    $accessConn.Close()
    
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "CONCLUSION" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $totalDates = $stats.NextCoffeeBy + $stats.NextCleanOn + $stats.NextFilterEst + $stats.NextDescaleEst + $stats.NextServiceEst
    
    if ($totalDates -eq 0) {
        Write-Host "ALL dates are NULL in the Access database!" -ForegroundColor Red
        Write-Host "The migration is working correctly - there are no dates to migrate." -ForegroundColor Yellow
        Write-Host "These dates must be calculated by the application at runtime." -ForegroundColor Yellow
    } else {
        Write-Host "Found $totalDates non-NULL dates in Access!" -ForegroundColor Green
        Write-Host "The import to AccessSrc.ClientUsageTbl is losing these dates." -ForegroundColor Red
        Write-Host "You need to fix the import process." -ForegroundColor Yellow
    }
    
    Write-Host ""
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack trace:" -ForegroundColor Gray
    Write-Host $_.Exception.StackTrace -ForegroundColor Gray
    exit 1
}
