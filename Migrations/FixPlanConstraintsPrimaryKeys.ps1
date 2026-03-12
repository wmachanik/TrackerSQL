#!/usr/bin/env pwsh
# FixPlanConstraintsPrimaryKeys.ps1
# Fixes the incorrect primary key definitions in PlanConstraints.json

$constraintsPath = "Data\Metadata\PlanEdits\PlanConstraints.json"

Write-Host "Fixing PlanConstraints.json primary keys..." -ForegroundColor Cyan

if (!(Test-Path $constraintsPath)) {
    Write-Error "PlanConstraints.json not found at: $constraintsPath"
    exit 1
}

# Read and parse JSON
$jsonContent = Get-Content $constraintsPath -Raw -Encoding UTF8
$constraints = $jsonContent | ConvertFrom-Json

# Backup
$backupPath = $constraintsPath + ".backup_" + (Get-Date -Format "yyyyMMdd_HHmmss")
Copy-Item $constraintsPath $backupPath
Write-Host "Backup created: $backupPath" -ForegroundColor Gray
Write-Host ""

$fixed = 0

# Fix 1: ContactUsageLinesTbl
$table = $constraints.Tables | Where-Object { $_.Table -eq "ContactUsageLinesTbl" }
if ($table -and $table.PrimaryKey -contains "ContactID") {
    Write-Host "Fixing ContactUsageLinesTbl..." -ForegroundColor Yellow
    Write-Host "  Old PK: $($table.PrimaryKey)" -ForegroundColor Gray
    Write-Host "  Old Identity: $($table.IdentityColumns)" -ForegroundColor Gray
    $table.PrimaryKey = @("ContactUsageLineNo")
    $table.IdentityColumns = @("ContactUsageLineNo")
    Write-Host "  New PK: ContactUsageLineNo" -ForegroundColor Green
    Write-Host "  New Identity: ContactUsageLineNo" -ForegroundColor Green
    $fixed++
}

# Fix 2: ContactsItemsPredictedTbl
$table = $constraints.Tables | Where-Object { $_.Table -eq "ContactsItemsPredictedTbl" }
if ($table -and $table.PrimaryKey -contains "ContactID") {
    Write-Host ""
    Write-Host "Fixing ContactsItemsPredictedTbl..." -ForegroundColor Yellow
    Write-Host "  Old PK: $($table.PrimaryKey)" -ForegroundColor Gray
    Write-Host "  Old Identity: $($table.IdentityColumns)" -ForegroundColor Gray
    $table.PrimaryKey = @("ContactsItemsPredictedID")
    $table.IdentityColumns = @("ContactsItemsPredictedID")
    Write-Host "  New PK: ContactsItemsPredictedID" -ForegroundColor Green
    Write-Host "  New Identity: ContactsItemsPredictedID" -ForegroundColor Green
    $fixed++
}

# Fix 3: ContactsItemUsageTbl
$table = $constraints.Tables | Where-Object { $_.Table -eq "ContactsItemUsageTbl" }
if ($table -and $table.PrimaryKey -contains "ContactID") {
    Write-Host ""
    Write-Host "Fixing ContactsItemUsageTbl..." -ForegroundColor Yellow
    Write-Host "  Old PK: $($table.PrimaryKey)" -ForegroundColor Gray
    Write-Host "  Old Identity: $($table.IdentityColumns)" -ForegroundColor Gray
    $table.PrimaryKey = @("ContactItemUsageLineNo")
    $table.IdentityColumns = @("ContactItemUsageLineNo")
    Write-Host "  New PK: ContactItemUsageLineNo" -ForegroundColor Green
    Write-Host "  New Identity: ContactItemUsageLineNo" -ForegroundColor Green
    $fixed++
}

if ($fixed -eq 0) {
    Write-Host "No fixes needed - all primary keys are already correct!" -ForegroundColor Green
    exit 0
}

# Save with proper formatting
$jsonOutput = $constraints | ConvertTo-Json -Depth 10
$jsonOutput | Out-File $constraintsPath -Encoding UTF8 -NoNewline

Write-Host ""
Write-Host "Successfully fixed $fixed table(s)!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1) Run MigrationRunner" -ForegroundColor Gray
Write-Host "  2) Choose option M (Generate CREATE TABLE)" -ForegroundColor Gray
Write-Host "  3) Verify PKs are correct in generated script" -ForegroundColor Gray
Write-Host "  4) Choose option N to regenerate data migration" -ForegroundColor Gray
