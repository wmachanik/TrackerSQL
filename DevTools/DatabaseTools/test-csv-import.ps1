# Test script to verify the CSV import fix
Write-Host "Testing CSV import fix..." -ForegroundColor Green

# Get the current state before import
$beforeTargetTable = (Get-Content 'Data\Metadata\AccessSchema\CustomersTbl.schema.json' | ConvertFrom-Json).Plan.TargetTable
$beforeClassification = (Get-Content 'Data\Metadata\AccessSchema\CustomersTbl.schema.json' | ConvertFrom-Json).Plan.Classification

Write-Host "BEFORE import:" -ForegroundColor Yellow
Write-Host "  CustomersTbl -> TargetTable: $beforeTargetTable"
Write-Host "  CustomersTbl -> Classification: $beforeClassification"

# Check if PackagingTbl exists and its current state
if (Test-Path 'Data\Metadata\AccessSchema\PackagingTbl.schema.json') {
    $beforePackagingTarget = (Get-Content 'Data\Metadata\AccessSchema\PackagingTbl.schema.json' | ConvertFrom-Json).Plan.TargetTable
    Write-Host "  PackagingTbl -> TargetTable: $beforePackagingTarget"
}

Write-Host "`nRunning CSV import (Option 11)..." -ForegroundColor Cyan

# Change to the MigrationRunner directory and run option 11
Set-Location "Migrations\MigrationRunner"

# Run the migration runner with option 11
$output = ""
try {
    # Create a simple input string for option 11, then quit
    "11`n`nq" | dotnet run --no-build 2>&1 | Tee-Object -Variable output
} catch {
    Write-Host "Error running migration: $_" -ForegroundColor Red
}

# Go back to root directory
Set-Location "..\..\"

Write-Host "`nOutput from migration runner:" -ForegroundColor Blue
Write-Host $output

# Check the state after import
if (Test-Path 'Data\Metadata\AccessSchema\CustomersTbl.schema.json') {
    $afterTargetTable = (Get-Content 'Data\Metadata\AccessSchema\CustomersTbl.schema.json' | ConvertFrom-Json).Plan.TargetTable
    $afterClassification = (Get-Content 'Data\Metadata\AccessSchema\CustomersTbl.schema.json' | ConvertFrom-Json).Plan.Classification
    
    Write-Host "`nAFTER import:" -ForegroundColor Yellow
    Write-Host "  CustomersTbl -> TargetTable: $afterTargetTable"
    Write-Host "  CustomersTbl -> Classification: $afterClassification"
    
    # Check if the fix worked
    if ($afterTargetTable -eq "ContactsTbl") {
        Write-Host "`nSUCCESS: TargetTable correctly renamed to ContactsTbl!" -ForegroundColor Green
    } else {
        Write-Host "`nFAILED: TargetTable still shows '$afterTargetTable' instead of 'ContactsTbl'" -ForegroundColor Red
    }
    
    if ($afterClassification -eq "Rename") {
        Write-Host "SUCCESS: Classification correctly set to Rename!" -ForegroundColor Green
    } else {
        Write-Host "FAILED: Classification still shows '$afterClassification' instead of 'Rename'" -ForegroundColor Red
    }
}

# Check PackagingTbl too
if (Test-Path 'Data\Metadata\AccessSchema\PackagingTbl.schema.json') {
    $afterPackagingTarget = (Get-Content 'Data\Metadata\AccessSchema\PackagingTbl.schema.json' | ConvertFrom-Json).Plan.TargetTable
    Write-Host "  PackagingTbl -> TargetTable: $afterPackagingTarget"
    
    if ($afterPackagingTarget -eq "ItemPackagingsTbl") {
        Write-Host "SUCCESS: PackagingTbl correctly renamed to ItemPackagingsTbl!" -ForegroundColor Green
    } else {
        Write-Host "FAILED: PackagingTbl still shows '$afterPackagingTarget' instead of 'ItemPackagingsTbl'" -ForegroundColor Red
    }
}