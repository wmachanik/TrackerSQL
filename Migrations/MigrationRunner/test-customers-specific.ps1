# Focused test on CustomersTbl specifically
Write-Host "Testing CustomersTbl CSV import specifically..." -ForegroundColor Green

# Check if CustomersTbl is in the parsed mappings
Write-Host "`nChecking CSV parsing for CustomersTbl..." -ForegroundColor Cyan

# Look at the CSV lines around CustomersTbl
$csvPath = "Data\Metadata\PlanEdits\TableMigrationReport-Dec-1.csv"
$lines = Get-Content $csvPath
$customerLine = $lines | Select-String "CustomersTbl" | Select-Object -First 1
Write-Host "Found CustomersTbl line: $($customerLine.Line)" -ForegroundColor Yellow

# Check current schema state
$schemaPath = "Data\Metadata\AccessSchema\CustomersTbl.schema.json"
if (Test-Path $schemaPath) {
    $beforeSchema = Get-Content $schemaPath | ConvertFrom-Json
    Write-Host "`nCurrent CustomersTbl schema:" -ForegroundColor Yellow
    Write-Host "  TargetTable: $($beforeSchema.Plan.TargetTable)"
    Write-Host "  Classification: $($beforeSchema.Plan.Classification)"
    
    # Let's examine what specific error occurs for CustomersTbl
    Write-Host "`nLet's test with just a simplified CSV containing only CustomersTbl..." -ForegroundColor Cyan
    
    # Create a minimal test CSV with just CustomersTbl
    $testCsv = @"
Table Migration report: Test,,,,,,,,,,,,,,,,,,,,,,
================================,,,,,,,,,,,,,,,,,,,,,,
Table,Before,After,Action,,,,,,,,,,,,,,,,,,,
,CustomersTbl,ContactsTbl,Rename,,,,,,,,,,,,,,,,,,,
Rows:,Before Col name,Before Type,Before Key,Before Auto,--/--,After Col Name,After Type,After Key,After Auto,After NotNull,Presserve ID,Action,Source Col,,,,,,,,,
,CustomerID,INT,No,No,--/--,ContactID,INT,PK,Yes,Yes,n/a,Rename,CustomerID,,,,,,,,,
,CompanyName,NVARCHAR(50),No,No,--/--,CompanyName,NVARCHAR(50),No,No,Yes,n/a,Copy,CompanyName,,,,,,,,,
-------------------------------/-------------------------------,,,,,,,,,,,,,,,,,,,,,,
"@

    $testCsvPath = "test-customers-only.csv"
    $testCsv | Out-File -FilePath $testCsvPath -Encoding UTF8
    
    Write-Host "Created test CSV: $testCsvPath" -ForegroundColor Green
    Write-Host "Test CSV content:" -ForegroundColor Blue
    Get-Content $testCsvPath | ForEach-Object { Write-Host "  $_" }
    
    # Now test the import with this simplified CSV
    Write-Host "`nTesting import with simplified CSV..." -ForegroundColor Cyan
    
    cd "Migrations\MigrationRunner"
    
    $testImportCode = @'
using System;
using System.IO;
using MigrationRunner;

class TestCustomersImport {
    static void Main() {
        try {
            Console.WriteLine("=== Testing CustomersTbl Import ===");
            
            var csvPath = @"..\..\test-customers-only.csv";
            var migrationsDir = @".\Migrations";
            
            Console.WriteLine($"Using CSV: {Path.GetFullPath(csvPath)}");
            
            string constraintsPath;
            int result = PlanHumanReviewImporter.ImportPlan(migrationsDir, csvPath, out constraintsPath);
            
            Console.WriteLine($"Import result: {result}");
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
'@

    $testImportCode | Out-File -FilePath "TestCustomersImport.cs" -Encoding UTF8
    
    Write-Host "Running simplified CustomersTbl test..." -ForegroundColor Cyan
    try {
        $result = dotnet run --project . --verbosity quiet --property:StartupObject=TestCustomersImport TestCustomersImport.cs 2>&1
        Write-Host "Import test result:" -ForegroundColor Blue
        $result | ForEach-Object { Write-Host "  $_" }
    } catch {
        Write-Host "Error running test: $_" -ForegroundColor Red
    }
    
    # Clean up test files
    if (Test-Path "TestCustomersImport.cs") { Remove-Item "TestCustomersImport.cs" -Force }
    cd ".."
    cd ".."
    if (Test-Path $testCsvPath) { Remove-Item $testCsvPath -Force }
    
    # Check if CustomersTbl was updated
    Write-Host "`nChecking CustomersTbl after test import..." -ForegroundColor Cyan
    if (Test-Path $schemaPath) {
        $afterSchema = Get-Content $schemaPath | ConvertFrom-Json
        Write-Host "CustomersTbl schema after import:" -ForegroundColor Yellow
        Write-Host "  TargetTable: $($afterSchema.Plan.TargetTable)"
        Write-Host "  Classification: $($afterSchema.Plan.Classification)"
        
        $changed = ($beforeSchema.Plan.TargetTable -ne $afterSchema.Plan.TargetTable) -or ($beforeSchema.Plan.Classification -ne $afterSchema.Plan.Classification)
        
        if ($changed) {
            Write-Host "? CustomersTbl was updated!" -ForegroundColor Green
        } else {
            Write-Host "? CustomersTbl was NOT updated - issue still exists!" -ForegroundColor Red
        }
    }
    
} else {
    Write-Host "CustomersTbl schema file not found!" -ForegroundColor Red
}