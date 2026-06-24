# Batch generates all missing repositories from CSV analysis
# Created: 2026-05-15
# Purpose: Create all missing repos in one go (0 AI calls)

param(
    [string]$CsvPath = "DevTools\Documentation\Legacy_To_Poco_Mapping.csv",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "Batch Repository Generator" -ForegroundColor Cyan
Write-Host "==========================" -ForegroundColor Cyan
Write-Host ""

# Read CSV
if (-not (Test-Path $CsvPath)) {
    Write-Host "ERROR: CSV not found: $CsvPath" -ForegroundColor Red
    Write-Host "Run .\DevTools\Scripts\Analyze-Legacy-Classes.ps1 first" -ForegroundColor Yellow
    exit 1
}

$mapping = Import-Csv $CsvPath

# Filter to missing repos only
$missing = $mapping | Where-Object { $_.RepoExists -eq "NO" }

Write-Host "Found $($missing.Count) missing repositories" -ForegroundColor Yellow
Write-Host ""

if ($missing.Count -eq 0) {
    Write-Host "All repositories already exist! ?" -ForegroundColor Green
    exit 0
}

# Sort by priority and custom method count
$sorted = $missing | Sort-Object @{Expression={if($_.Priority -eq "HIGH"){1}else{2}}}, @{Expression={[int]$_.CustomMethods}; Descending=$true}

$successCount = 0
$skipCount = 0
$failCount = 0

foreach ($item in $sorted) {
    $pocoName = $item.PocoName.Trim()
    $repoName = $item.RepoName.Trim()

    Write-Host "[$($successCount + $skipCount + $failCount + 1)/$($missing.Count)] Processing: $pocoName -> $repoName" -ForegroundColor Cyan

    # Skip if POCO doesn't exist
    if ($item.PocoExists -eq "NO") {
        Write-Host "  ? SKIP: POCO does not exist ($pocoName.cs)" -ForegroundColor Yellow
        $skipCount++
        continue
    }

    # Generate repository
    try {
        if ($DryRun) {
            Write-Host "  [DRY RUN] Would create: $repoName" -ForegroundColor Gray
        } else {
            .\DevTools\Scripts\Generate-Repository.ps1 -PocoName $pocoName -ErrorAction Stop
            Write-Host "  ? Created: $repoName" -ForegroundColor Green
        }
        $successCount++
    }
    catch {
        Write-Host "  ? FAILED: $($_.Exception.Message)" -ForegroundColor Red
        $failCount++
    }

    Write-Host ""
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "? Created: $successCount" -ForegroundColor Green
Write-Host "? Skipped: $skipCount" -ForegroundColor Yellow
Write-Host "? Failed: $failCount" -ForegroundColor Red
Write-Host "Total: $($missing.Count)"
Write-Host ""

if ($DryRun) {
    Write-Host "This was a DRY RUN. Run without -DryRun to execute." -ForegroundColor Yellow
} else {
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Open each generated repository in Classes\Sql\"
    Write-Host "  2. Fill in INSERT/UPDATE SQL from legacy Controls\*Tbl.cs"
    Write-Host "  3. Copy custom methods (see CustomMethodNames in CSV)"
    Write-Host "  4. Build and test"
}
