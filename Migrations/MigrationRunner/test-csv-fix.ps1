# Direct test of the CSV import fix
Write-Host "Testing CSV import fix directly..." -ForegroundColor Green

# Check current state BEFORE any import
$beforeState = @{}
$testTables = @("CustomersTbl", "PackagingTbl", "CityTbl")

foreach ($table in $testTables) {
    $schemaPath = "..\..\Data\Metadata\AccessSchema\$table.schema.json"
    if (Test-Path $schemaPath) {
        try {
            $content = Get-Content $schemaPath | ConvertFrom-Json
            $beforeState[$table] = @{
                TargetTable = $content.Plan.TargetTable
                Classification = $content.Plan.Classification
            }
        } catch {
            Write-Host "Error reading $table schema: $_" -ForegroundColor Red
        }
    }
}

Write-Host "`nBEFORE import:" -ForegroundColor Yellow
foreach ($table in $testTables) {
    if ($beforeState.ContainsKey($table)) {
        Write-Host "  $table -> TargetTable: $($beforeState[$table].TargetTable), Classification: $($beforeState[$table].Classification)"
    }
}

# Now create a simple test program that calls the CSV import directly
$testCode = @'
using System;
using System.IO;
using MigrationRunner;

class TestProgram {
    static void Main() {
        try {
            Console.WriteLine("Running CSV import test...");
            
            var csvPath = @"..\..\Data\Metadata\PlanEdits\TableMigrationReport-Dec-1.csv";
            var migrationsDir = @".\Migrations";
            
            Console.WriteLine($"CSV Path: {Path.GetFullPath(csvPath)}");
            Console.WriteLine($"CSV Exists: {File.Exists(csvPath)}");
            
            if (!File.Exists(csvPath)) {
                Console.WriteLine("CSV file not found!");
                return;
            }
            
            string constraintsPath;
            int result = PlanHumanReviewImporter.ImportPlan(migrationsDir, csvPath, out constraintsPath);
            
            Console.WriteLine($"Import result: {result}");
            Console.WriteLine($"Constraints path: {constraintsPath}");
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
'@

# Write the test program
$testCode | Out-File -FilePath "TestImport.cs" -Encoding UTF8

# Compile and run the test
Write-Host "`nCompiling test program..." -ForegroundColor Cyan
try {
    $buildResult = dotnet run --project . --verbosity quiet --property:StartupObject=TestProgram TestImport.cs 2>&1
    Write-Host "Build/Run output:" -ForegroundColor Blue
    $buildResult | ForEach-Object { Write-Host "  $_" }
} catch {
    Write-Host "Error running test: $_" -ForegroundColor Red
}

# Clean up
if (Test-Path "TestImport.cs") {
    Remove-Item "TestImport.cs" -Force
}

Write-Host "`nChecking AFTER state..." -ForegroundColor Cyan

# Check state AFTER import
$afterState = @{}
foreach ($table in $testTables) {
    $schemaPath = "..\..\Data\Metadata\AccessSchema\$table.schema.json"
    if (Test-Path $schemaPath) {
        try {
            $content = Get-Content $schemaPath | ConvertFrom-Json
            $afterState[$table] = @{
                TargetTable = $content.Plan.TargetTable
                Classification = $content.Plan.Classification
            }
        } catch {
            Write-Host "Error reading $table schema: $_" -ForegroundColor Red
        }
    }
}

Write-Host "`nAFTER import:" -ForegroundColor Yellow
foreach ($table in $testTables) {
    if ($afterState.ContainsKey($table)) {
        $before = $beforeState[$table]
        $after = $afterState[$table]
        
        $changed = ($before.TargetTable -ne $after.TargetTable) -or ($before.Classification -ne $after.Classification)
        $color = if ($changed) { "Green" } else { "White" }
        
        Write-Host "  $table -> TargetTable: $($after.TargetTable), Classification: $($after.Classification)" -ForegroundColor $color
        
        if ($changed) {
            Write-Host "    CHANGED from: TargetTable: $($before.TargetTable), Classification: $($before.Classification)" -ForegroundColor Green
        }
    }
}

# Check specific expected results
Write-Host "`nValidation:" -ForegroundColor Cyan
$expectedChanges = @{
    "CustomersTbl" = @{ TargetTable = "ContactsTbl"; Classification = "Rename" }
    "PackagingTbl" = @{ TargetTable = "ItemPackagingsTbl"; Classification = "Rename" } 
    "CityTbl" = @{ TargetTable = "AreasTbl"; Classification = "Rename" }
}

$successCount = 0
foreach ($table in $expectedChanges.Keys) {
    if ($afterState.ContainsKey($table)) {
        $expected = $expectedChanges[$table]
        $actual = $afterState[$table]
        
        if ($actual.TargetTable -eq $expected.TargetTable -and $actual.Classification -eq $expected.Classification) {
            Write-Host "  ? $table correctly renamed: $($actual.TargetTable) ($($actual.Classification))" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "  ? $table NOT correct: Expected $($expected.TargetTable) ($($expected.Classification)), Got $($actual.TargetTable) ($($actual.Classification))" -ForegroundColor Red
        }
    }
}

Write-Host "`nSUMMARY:" -ForegroundColor White
if ($successCount -eq $expectedChanges.Count) {
    Write-Host "?? ALL TESTS PASSED! CSV import fix is working correctly!" -ForegroundColor Green
} else {
    Write-Host "? $successCount/$($expectedChanges.Count) tests passed. CSV import needs more work." -ForegroundColor Red
}