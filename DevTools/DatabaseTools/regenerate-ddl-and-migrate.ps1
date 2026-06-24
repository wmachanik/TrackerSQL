# Regenerate DDL and Data Migration after PlanConstraints.json fix
# This script automates the regeneration process

Write-Host "=== REGENERATE DDL & DATA MIGRATION ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will:" -ForegroundColor Yellow
Write-Host "  1. Regenerate CREATE TABLE DDL with fixed column definitions" -ForegroundColor Yellow
Write-Host "  2. Apply DDL to recreate tables with correct structure" -ForegroundColor Yellow
Write-Host "  3. Stage Access data to SQL" -ForegroundColor Yellow  
Write-Host "  4. Regenerate data migration script" -ForegroundColor Yellow
Write-Host "  5. Apply data migration to populate tables" -ForegroundColor Yellow
Write-Host ""

$response = Read-Host "Do you want to proceed with full regeneration? (Y/N)"
if ($response -ne 'Y' -and $response -ne 'y') {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Starting MigrationRunner..." -ForegroundColor Green

# Change to the MigrationRunner directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$migrationRunnerPath = Join-Path $scriptPath "Migrations\MigrationRunner\bin\Debug\net48"
$exePath = Join-Path $migrationRunnerPath "MigrationRunner.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: MigrationRunner.exe not found at: $exePath" -ForegroundColor Red
    Write-Host "Please build the MigrationRunner project first." -ForegroundColor Red
    exit 1
}

Push-Location $migrationRunnerPath

try {
    Write-Host ""
    Write-Host "=== OPTION Z: Full Migration Pipeline ===" -ForegroundColor Cyan
    Write-Host "Running automated sequence: A -> B -> C -> MS -> M -> N" -ForegroundColor Yellow
    Write-Host ""
    
    # Run with automated answers
    # Use 'Z' to run full pipeline, then answer prompts automatically
    $input = @"
Z
Y
Y
Y
Y
"@
    
    $input | & $exePath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "SUCCESS: Full migration pipeline completed!" -ForegroundColor Green
        Write-Host ""
        Write-Host "The following has been done:" -ForegroundColor Cyan
        Write-Host "  ? DDL regenerated with fixed column definitions" -ForegroundColor Green
        Write-Host "  ? Tables recreated with correct structure" -ForegroundColor Green
        Write-Host "  ? Access data staged to [AccessSrc]" -ForegroundColor Green
        Write-Host "  ? Data migration script regenerated" -ForegroundColor Green
        Write-Host "  ? Data migrated to target tables" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "  1. Run verification script to check if tables now have data" -ForegroundColor Yellow
        Write-Host "  2. Check the failed tables: ContactsUsageTbl, ContactsItemUsageTbl, ContactUsageLinesTbl, RepairsTbl" -ForegroundColor Yellow
    } else {
        Write-Host ""
        Write-Host "WARNING: Migration completed with code: $LASTEXITCODE" -ForegroundColor Yellow
        Write-Host "Check the logs in Migrations\MigrationRunner\Metadata\PlanEdits\Logs\" -ForegroundColor Yellow
    }
    
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "Script completed. Check output above for any errors." -ForegroundColor Cyan
