# Fix bad dates and re-run data migration for failing tables
# This script:
# 1. Cleans bad date values in AccessSrc schema
# 2. Re-runs Step N (Apply Data Migration)

Write-Host "=== FIX BAD DATES & RETRY MIGRATION ===" -ForegroundColor Cyan
Write-Host ""

# Get connection string from MigrationConfig.json
$configPath = "Migrations\MigrationRunner\Migrations\MigrationConfig.json"
if (-not (Test-Path $configPath)) {
    Write-Host "ERROR: MigrationConfig.json not found at: $configPath" -ForegroundColor Red
    exit 1
}

$config = Get-Content $configPath | ConvertFrom-Json
$connString = $config.SqlConnectionString

Write-Host "Connection: $($connString.Split(';')[0])" -ForegroundColor Gray
Write-Host ""

# Apply the date fix script
$fixScript = "Data\Metadata\PlanEdits\Sql\FixBadDates_ContactsUsageTbl.sql"
if (-not (Test-Path $fixScript)) {
    Write-Host "ERROR: Fix script not found at: $fixScript" -ForegroundColor Red
    exit 1
}

Write-Host "Step 1: Applying date fixes to AccessSrc schema..." -ForegroundColor Yellow
Write-Host ""

try {
    $result = sqlcmd -S "$($config.SqlServerInstance)" -d "$($config.DatabaseName)" -i "$fixScript" -b
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Date fix script failed!" -ForegroundColor Red
        Write-Host $result
        exit 1
    }
    Write-Host "? Date fixes applied successfully!" -ForegroundColor Green
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Re-running data migration (Option N)..." -ForegroundColor Yellow
Write-Host ""

# Run MigrationRunner Option N
$migrationRunnerPath = "Migrations\MigrationRunner\bin\Debug\net48"
$exePath = Join-Path $migrationRunnerPath "MigrationRunner.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: MigrationRunner.exe not found. Please build the project first." -ForegroundColor Red
    exit 1
}

Push-Location $migrationRunnerPath

try {
    # Run Option N (Apply Data Migration)
    $input = @"
N
X
"@
    
    $input | & $exePath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "SUCCESS: Data migration completed!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "WARNING: Migration completed with exit code: $LASTEXITCODE" -ForegroundColor Yellow
    }
    
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "Next step: Run verification to check if the 3 tables now have data" -ForegroundColor Cyan
Write-Host "  - ContactsUsageTbl (2,160 rows expected)" -ForegroundColor Gray
Write-Host "  - RepairsTbl (905 rows expected)" -ForegroundColor Gray
Write-Host "  - TempCoffeecheckupCustomerTbl (14 rows expected)" -ForegroundColor Gray
