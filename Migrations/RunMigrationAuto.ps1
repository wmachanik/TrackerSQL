# Auto Migration Runner
# This script runs the migration automatically with predefined choices

param(
    [string]$AccessConnectionString = "",
    [string]$SqlConnectionString = "Server=.\SQLEXPRESS;Database=OtterDb;Trusted_Connection=True;",
    [string]$Command = "$"  # Default to full pipeline
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AUTO MIGRATION RUNNER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Build the project first
Write-Host "Building MigrationRunner..." -ForegroundColor Yellow
Push-Location "C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner"
$buildResult = dotnet build --configuration Release 2>&1
Pop-Location

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build FAILED!" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

# Run the migration
Write-Host "Running migration with command: $Command" -ForegroundColor Yellow
Write-Host "SQL Connection: $SqlConnectionString" -ForegroundColor Gray
Write-Host ""

$migrationExe = "C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner\bin\Release\MigrationRunner.exe"

if (!(Test-Path $migrationExe)) {
    Write-Host "Migration runner not found at: $migrationExe" -ForegroundColor Red
    exit 1
}

# Create input for the migration runner
# Format: Command + Access CS + SQL CS + Confirmations
$input = @"
$Command
$AccessConnectionString
$SqlConnectionString
YES

YES
Q
"@

Write-Host "Sending commands to MigrationRunner..." -ForegroundColor Yellow
$input | & $migrationExe

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Migration completed!" -ForegroundColor Cyan
Write-Host "Check the logs in:" -ForegroundColor Yellow
Write-Host "  C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Logs\" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan
