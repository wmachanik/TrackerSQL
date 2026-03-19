# Cleanup Obsolete Migration Scripts and Docs
# This removes temporary files created during troubleshooting

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cleaning Up Obsolete Migration Files" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$migrationsPath = "C:\SRC\ASP.net\TrackerSQL\Migrations"

# Files to remove (temporary/obsolete)
$filesToRemove = @(
    "RunFix.ps1",
    "Import-AccessClientUsageDates.ps1",
    "README_FIX.md",
    "HOW_TO_APPLY_THE_FIX.md",
    "DATETIME_CONVERSION_ERROR_FIXED.md",
    "READY_TO_GENERATE.md",
    "AUTOMATED_SOLUTION_COMPLETE.md",
    "FULLY_AUTOMATED_NOW.md",
    "ORDER_MATTERS_MIGRATION_PROCESS.md"
)

Write-Host "Files to remove:" -ForegroundColor Yellow
foreach ($file in $filesToRemove) {
    $fullPath = Join-Path $migrationsPath $file
    if (Test-Path $fullPath) {
        Write-Host "  ? Found: $file" -ForegroundColor Gray
    } else {
        Write-Host "  - Not found: $file" -ForegroundColor DarkGray
    }
}

Write-Host ""
$response = Read-Host "Delete these files? (Y/N)"

if ($response -eq "Y" -or $response -eq "y") {
    Write-Host ""
    Write-Host "Deleting files..." -ForegroundColor Yellow
    
    $deleted = 0
    $notFound = 0
    
    foreach ($file in $filesToRemove) {
        $fullPath = Join-Path $migrationsPath $file
        if (Test-Path $fullPath) {
            try {
                Remove-Item $fullPath -Force
                Write-Host "  ? Deleted: $file" -ForegroundColor Green
                $deleted++
            } catch {
                Write-Host "  ? Failed to delete: $file - $($_.Exception.Message)" -ForegroundColor Red
            }
        } else {
            $notFound++
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Summary:" -ForegroundColor Cyan
    Write-Host "  Deleted: $deleted files" -ForegroundColor Green
    if ($notFound -gt 0) {
        Write-Host "  Not found: $notFound files" -ForegroundColor DarkGray
    }
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "? Cleanup complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To run migrations, use:" -ForegroundColor Yellow
    Write-Host "  cd Migrations\MigrationRunner" -ForegroundColor White
    Write-Host "  dotnet run" -ForegroundColor White
    Write-Host "  # Select: `$" -ForegroundColor Gray
    
} else {
    Write-Host ""
    Write-Host "Cancelled - no files deleted" -ForegroundColor Yellow
}

Write-Host ""
