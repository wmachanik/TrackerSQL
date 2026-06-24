# Organize DevTools - Move PowerShell scripts and support files to DevTools folder
# This script organizes the root folder by moving development tools to a dedicated folder

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Organizing Development Tools" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

$rootPath = "C:\SRC\ASP.net\TrackerSQL"
$devToolsPath = Join-Path $rootPath "DevTools"

# Ensure DevTools folder exists
if (!(Test-Path $devToolsPath)) {
    New-Item -Path $devToolsPath -ItemType Directory -Force | Out-Null
    Write-Host "? Created DevTools folder" -ForegroundColor Green
}

# Create subfolders for better organization
$folders = @{
    "Scripts" = "PowerShell automation scripts"
    "DatabaseTools" = "Database migration and diagnostic scripts"
    "CodeQuality" = "Code analysis and refactoring tools"
    "Documentation" = "Quick reference and summary docs"
}

foreach ($folder in $folders.Keys) {
    $folderPath = Join-Path $devToolsPath $folder
    if (!(Test-Path $folderPath)) {
        New-Item -Path $folderPath -ItemType Directory -Force | Out-Null
        Write-Host "  ? Created $folder subfolder - $($folders[$folder])" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Moving files to DevTools..." -ForegroundColor Yellow
Write-Host ""

$moved = 0

# Database-related scripts
$dbScripts = @(
    "diagnose-contacts-orders.ps1",
    "diagnose-step-n.ps1",
    "fix-bad-dates-and-migrate.ps1",
    "fix-missing-database.ps1",
    "regenerate-ddl-and-migrate.ps1",
    "test-csv-import.ps1",
    "test-csv-line-parsing.ps1",
    "test-csv-parser.ps1",
    "test-drop-column-fix.ps1",
    "test-error-reporting-fix.ps1"
)

foreach ($script in $dbScripts) {
    $sourcePath = Join-Path $rootPath $script
    if (Test-Path $sourcePath) {
        $destPath = Join-Path (Join-Path $devToolsPath "DatabaseTools") $script
        Move-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "  ? Moved $script to DevTools/DatabaseTools/" -ForegroundColor Green
        $moved++
    }
}

# Code quality scripts
$codeQualityScripts = @(
    "FixDuplicateUsings.ps1",
    "RenameToTrackerSQL.ps1",
    "RenameCitiesToAreas.ps1",
    "ScanCodeQuality.ps1"
)

foreach ($script in $codeQualityScripts) {
    $sourcePath = Join-Path $rootPath $script
    if (Test-Path $sourcePath) {
        $destPath = Join-Path (Join-Path $devToolsPath "CodeQuality") $script
        Move-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "  ? Moved $script to DevTools/CodeQuality/" -ForegroundColor Green
        $moved++
    }
}

# General utility scripts
$utilityScripts = @(
    "UnblockBinaries.ps1"
)

foreach ($script in $utilityScripts) {
    $sourcePath = Join-Path $rootPath $script
    if (Test-Path $sourcePath) {
        $destPath = Join-Path (Join-Path $devToolsPath "Scripts") $script
        Move-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "  ? Moved $script to DevTools/Scripts/" -ForegroundColor Green
        $moved++
    }
}

# Quick reference documentation
$quickDocs = @(
    "CODE_CLEANUP_COMPLETE.md",
    "NAMESPACE_RENAME_COMPLETE.md",
    "QUICK_FIX_WINDOWS_DEFENDER.md"
)

foreach ($doc in $quickDocs) {
    $sourcePath = Join-Path $rootPath $doc
    if (Test-Path $sourcePath) {
        $destPath = Join-Path (Join-Path $devToolsPath "Documentation") $doc
        Move-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "  ? Moved $doc to DevTools/Documentation/" -ForegroundColor Green
        $moved++
    }
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "ORGANIZATION COMPLETE" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  - Files moved: $moved" -ForegroundColor White
Write-Host "  - Target folder: DevTools/" -ForegroundColor White
Write-Host ""
Write-Host "Folder Structure:" -ForegroundColor White
Write-Host "  DevTools/" -ForegroundColor Cyan
Write-Host "    ??? Scripts/              (General utility scripts)" -ForegroundColor Gray
Write-Host "    ??? DatabaseTools/        (DB migration & diagnostics)" -ForegroundColor Gray
Write-Host "    ??? CodeQuality/          (Code analysis & refactoring)" -ForegroundColor Gray
Write-Host "    ??? Documentation/        (Quick reference docs)" -ForegroundColor Gray
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Update any documentation that references these scripts" -ForegroundColor White
Write-Host "  2. Update build scripts (UnblockBinaries.ps1 path in .csproj)" -ForegroundColor White
Write-Host "  3. Create README.md in DevTools folder" -ForegroundColor White
Write-Host ""
