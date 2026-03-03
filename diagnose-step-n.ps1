# Step N Data Migration Diagnostic Script
# This script diagnoses why step N (data migration) reports success but no data gets migrated

param(
    [string]$ConnectionString = $null
)

Write-Host "=== STEP N DATA MIGRATION DIAGNOSTIC ===" -ForegroundColor Cyan
Write-Host "This diagnoses why data migration reports success but no data appears in target tables" -ForegroundColor Yellow
Write-Host ""

# Get connection string
if (-not $ConnectionString) {
    Write-Host "Please provide SQL Server connection string:" -ForegroundColor Green
    Write-Host "Example: Server=.\SQLEXPRESS;Database=TrackerData;Integrated Security=True;" -ForegroundColor Gray
    $ConnectionString = Read-Host "Connection String"
}

if (-not $ConnectionString) {
    Write-Error "Connection string is required"
    exit 1
}

try {
    # Test connection
    Write-Host "Testing connection..." -ForegroundColor Yellow
    $conn = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $conn.Open()
    Write-Host "? Connection successful" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "=== 1. CHECKING ACCESS SOURCE SCHEMA (AccessSrc) ===" -ForegroundColor Cyan
    
    # Check if AccessSrc schema exists
    $cmd = New-Object System.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM sys.schemas WHERE name = 'AccessSrc'", $conn)
    $schemaExists = $cmd.ExecuteScalar()
    
    if ($schemaExists -eq 0) {
        Write-Host "? AccessSrc schema does NOT exist!" -ForegroundColor Red
        Write-Host "   The data migration expects source data in AccessSrc.TableName format" -ForegroundColor Red
        Write-Host "   You need to run option MS (Stage Access) first!" -ForegroundColor Yellow
    } else {
        Write-Host "? AccessSrc schema exists" -ForegroundColor Green
        
        # Count tables in AccessSrc schema
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'AccessSrc'"
        $tableCount = $cmd.ExecuteScalar()
        Write-Host "   Tables in AccessSrc schema: $tableCount" -ForegroundColor White
        
        if ($tableCount -eq 0) {
            Write-Host "? No tables found in AccessSrc schema!" -ForegroundColor Red
            Write-Host "   Run option MS (Stage Access) to populate AccessSrc tables" -ForegroundColor Yellow
        } else {
            # Show sample AccessSrc tables and their row counts
            Write-Host "   Sample AccessSrc tables:" -ForegroundColor White
            $cmd.CommandText = @"
SELECT TOP 10 
    t.name as TableName,
    (SELECT COUNT(*) FROM sys.partitions p WHERE p.object_id = t.object_id AND p.index_id IN (0,1)) as EstimatedRows
FROM sys.tables t 
JOIN sys.schemas s ON t.schema_id = s.schema_id 
WHERE s.name = 'AccessSrc'
ORDER BY t.name
"@
            $reader = $cmd.ExecuteReader()
            while ($reader.Read()) {
                $tableName = $reader["TableName"]
                $rowCount = $reader["EstimatedRows"]
                Write-Host "     AccessSrc.$tableName : $rowCount rows" -ForegroundColor Gray
            }
            $reader.Close()
        }
    }
    
    Write-Host ""
    Write-Host "=== 2. CHECKING TARGET TABLES ===" -ForegroundColor Cyan
    
    # Check key target tables
    $targetTables = @("ContactsTbl", "AreasTbl", "ItemsTbl", "PeopleTbl", "PaymentTermsTbl")
    foreach ($table in $targetTables) {
        $cmd.CommandText = "IF OBJECT_ID('dbo.$table', 'U') IS NOT NULL SELECT COUNT(*) FROM [$table] ELSE SELECT -1"
        try {
            $count = $cmd.ExecuteScalar()
            if ($count -eq -1) {
                Write-Host "   ? $table does not exist" -ForegroundColor Red
            } elseif ($count -eq 0) {
                Write-Host "   ??  $table exists but is EMPTY" -ForegroundColor Yellow
            } else {
                Write-Host "   ? $table exists with $count rows" -ForegroundColor Green
            }
        } catch {
            Write-Host "   ? Error checking $table : $_" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "=== 3. CHECKING DATA MIGRATION SCRIPT ===" -ForegroundColor Cyan
    
    # Look for the latest DataMigration script
    $migrationsPath = "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql"
    if (Test-Path $migrationsPath) {
        $latestScript = Get-ChildItem "$migrationsPath\DataMigration_*.sql" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        $latestFile = "$migrationsPath\DataMigration_LATEST.sql"
        
        if ($latestScript -or (Test-Path $latestFile)) {
            $scriptPath = if (Test-Path $latestFile) { $latestFile } else { $latestScript.FullName }
            Write-Host "? Found data migration script: $($scriptPath -replace '.*\\', '')" -ForegroundColor Green
            
            # Analyze the script content
            $scriptContent = Get-Content $scriptPath -Raw
            $insertMatches = [regex]::Matches($scriptContent, "INSERT INTO \[(\w+)\]", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
            Write-Host "   INSERT statements found: $($insertMatches.Count)" -ForegroundColor White
            
            if ($insertMatches.Count -gt 0) {
                Write-Host "   Sample target tables being populated:" -ForegroundColor White
                $insertMatches | Select-Object -First 5 | ForEach-Object {
                    Write-Host "     - $($_.Groups[1].Value)" -ForegroundColor Gray
                }
            }
            
            # Check for AccessSrc references
            $accessSrcMatches = [regex]::Matches($scriptContent, "FROM \[AccessSrc\]\.\[(\w+)\]", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
            Write-Host "   AccessSrc table references: $($accessSrcMatches.Count)" -ForegroundColor White
            
        } else {
            Write-Host "? No DataMigration script found!" -ForegroundColor Red
            Write-Host "   Run option M (Generate Data Migration Script) first" -ForegroundColor Yellow
        }
    } else {
        Write-Host "? Migrations directory not found: $migrationsPath" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "=== 4. CHECKING MIGRATION LOGS ===" -ForegroundColor Cyan
    
    $logsPath = "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Logs"
    if (Test-Path $logsPath) {
        $latestLog = Get-ChildItem "$logsPath\ApplyData_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        
        if ($latestLog) {
            Write-Host "? Found latest migration log: $($latestLog.Name)" -ForegroundColor Green
            Write-Host "   Log timestamp: $($latestLog.LastWriteTime)" -ForegroundColor White
            
            # Show last few lines of the log
            $logContent = Get-Content $latestLog.FullName -Tail 10
            Write-Host "   Last 10 lines of log:" -ForegroundColor White
            $logContent | ForEach-Object {
                Write-Host "     $_" -ForegroundColor Gray
            }
            
            # Check for error indicators
            $fullLogContent = Get-Content $latestLog.FullName -Raw
            $errorCount = ([regex]::Matches($fullLogContent, "ERROR", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
            $warningCount = ([regex]::Matches($fullLogContent, "WARN", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
            
            Write-Host "   Errors in log: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
            Write-Host "   Warnings in log: $warningCount" -ForegroundColor $(if ($warningCount -gt 0) { "Yellow" } else { "Green" })
            
        } else {
            Write-Host "? No migration logs found!" -ForegroundColor Red
        }
    } else {
        Write-Host "? Logs directory not found: $logsPath" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "=== 5. DIAGNOSIS SUMMARY ===" -ForegroundColor Cyan
    
    # Provide specific recommendations based on findings
    if ($schemaExists -eq 0) {
        Write-Host "?? PRIMARY ISSUE: AccessSrc schema missing" -ForegroundColor Red
        Write-Host "   SOLUTION: Run step MS (Stage Access) before step N" -ForegroundColor Yellow
        Write-Host "   Command: dotnet run --configuration Release" -ForegroundColor Gray
        Write-Host "           Select 'MS' to stage Access data first" -ForegroundColor Gray
    } elseif ($tableCount -eq 0) {
        Write-Host "?? PRIMARY ISSUE: AccessSrc schema empty" -ForegroundColor Red
        Write-Host "   SOLUTION: Run step MS (Stage Access) to populate source tables" -ForegroundColor Yellow
    } else {
        Write-Host "?? LIKELY ISSUE: Data migration script execution problem" -ForegroundColor Yellow
        Write-Host "   SOLUTION: Check the migration logs for detailed error messages" -ForegroundColor Yellow
        Write-Host "   Try re-running step N (Apply Data Migration)" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "=== RECOMMENDED ACTIONS ===" -ForegroundColor Yellow
    Write-Host "1. If AccessSrc is missing/empty:" -ForegroundColor White
    Write-Host "   cd Migrations\MigrationRunner" -ForegroundColor Gray  
    Write-Host "   dotnet run --configuration Release" -ForegroundColor Gray
    Write-Host "   Select MS (Stage Access to SQL)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. If AccessSrc exists but target tables empty:" -ForegroundColor White
    Write-Host "   Re-run step N (Apply Data Migration)" -ForegroundColor Gray
    Write-Host "   Check the detailed logs in Data\Metadata\PlanEdits\Logs\" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. If still failing:" -ForegroundColor White
    Write-Host "   Run the full pipeline: select Z (Full pipeline)" -ForegroundColor Gray
    Write-Host "   This runs A?B?C?MS?M?N in sequence" -ForegroundColor Gray

    $conn.Close()

} catch {
    Write-Error "Error running diagnostic: $_"
    if ($conn -and $conn.State -eq 'Open') {
        $conn.Close()
    }
    exit 1
}

Write-Host ""
Write-Host "=== DIAGNOSTIC COMPLETE ===" -ForegroundColor Green