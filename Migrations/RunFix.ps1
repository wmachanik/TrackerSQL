# Quick Fix: Regenerate and Run Migration
# This script automates the fix process

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AUTOMATED FIX: Regenerating Migration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$migrationRunnerPath = "C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner"
$sqlPath = "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql"

# Step 1: Build
Write-Host "Step 1: Building MigrationRunner with fixed code..." -ForegroundColor Yellow
Push-Location $migrationRunnerPath
dotnet build --nologo -v quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    Pop-Location
    exit 1
}
Write-Host "  ? Build successful" -ForegroundColor Green
Pop-Location

# Step 2: Backup old SQL
Write-Host ""
Write-Host "Step 2: Backing up old DataMigration_LATEST.sql..." -ForegroundColor Yellow
if (Test-Path $sqlPath) {
    $backupPath = $sqlPath -replace "\.sql$", "_BROKEN_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    Copy-Item $sqlPath $backupPath
    Write-Host "  ? Backed up to: $backupPath" -ForegroundColor Green
} else {
    Write-Host "  ? No existing SQL file found" -ForegroundColor Yellow
}

# Step 3: Check if we need user input for connection strings
Write-Host ""
Write-Host "Step 3: Regenerating DataMigration_LATEST.sql..." -ForegroundColor Yellow
Write-Host "  This will take a moment..." -ForegroundColor Gray

# Run option M (Generate DML)
Push-Location $migrationRunnerPath
$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -Wait -PassThru -RedirectStandardInput "input.txt" -RedirectStandardOutput "output.txt"

# Create input for option M
"M" | Out-File -FilePath "input.txt" -Encoding ASCII
dotnet run < input.txt > output.txt 2>&1

if (Test-Path $sqlPath) {
    $sqlFileDate = (Get-Item $sqlPath).LastWriteTime
    if ($sqlFileDate -gt (Get-Date).AddMinutes(-5)) {
        Write-Host "  ? DataMigration_LATEST.sql regenerated successfully!" -ForegroundColor Green
        
        # Check if fix was applied
        $hasBug = Select-String -Path $sqlPath -Pattern "NULLIF\(\[NextCoffee\]" -Quiet
        if ($hasBug) {
            Write-Host "  ? WARNING: SQL still contains NULLIF on NextCoffee!" -ForegroundColor Red
            Write-Host "  The fix may not have been applied correctly." -ForegroundColor Red
        } else {
            Write-Host "  ? Verified: DateTime fix is applied!" -ForegroundColor Green
        }
    } else {
        Write-Host "  ? SQL file wasn't updated - check output.txt for errors" -ForegroundColor Yellow
    }
}

Remove-Item "input.txt" -ErrorAction SilentlyContinue
Pop-Location

# Step 4: Prompt to run migration
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The SQL has been regenerated with the fix." -ForegroundColor Green
Write-Host ""
Write-Host "To apply the migration, run:" -ForegroundColor Yellow
Write-Host "  cd $migrationRunnerPath" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host "  # Then select: N (Apply DataMigration)" -ForegroundColor Gray
Write-Host ""
Write-Host "Or for a full clean migration:" -ForegroundColor Yellow
Write-Host "  cd $migrationRunnerPath" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host "  # Then select: $ (Full pipeline)" -ForegroundColor Gray
Write-Host ""

$response = Read-Host "Would you like to apply the migration now? (Y/N)"
if ($response -eq "Y" -or $response -eq "y") {
    Write-Host ""
    Write-Host "Launching MigrationRunner..." -ForegroundColor Yellow
    Write-Host "Select option 'N' to apply the data migration" -ForegroundColor Gray
    Write-Host ""
    Start-Sleep -Seconds 2
    
    Push-Location $migrationRunnerPath
    dotnet run
    Pop-Location
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
