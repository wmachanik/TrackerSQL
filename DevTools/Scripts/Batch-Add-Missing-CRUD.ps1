# Batch adds missing CRUD to all repositories
# Created: 2026-05-15
# Purpose: Process all repos, add only missing CRUD methods (0 AI calls)

param(
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?  Batch Add Missing CRUD Methods                           ?" -ForegroundColor Cyan
Write-Host "?  Zero AI Calls Required                                   ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Find all repository files
$repoFiles = Get-ChildItem -Path "Classes\Sql" -Filter "*Repository.cs" -File

Write-Host "Found $($repoFiles.Count) repositories to check" -ForegroundColor Yellow
Write-Host ""

$stats = @{
    Total = $repoFiles.Count
    Complete = 0
    Updated = 0
    Skipped = 0
    Failed = 0
}

$updatedRepos = @()
$completeRepos = @()

foreach ($repo in $repoFiles) {
    $repoPath = $repo.FullName -replace [regex]::Escape((Get-Location).Path + "\"), ""
    $repoName = $repo.BaseName

    Write-Host "[$($stats.Complete + $stats.Updated + $stats.Skipped + $stats.Failed + 1)/$($stats.Total)] Checking: $repoName" -ForegroundColor Cyan

    try {
        # Run add-missing-CRUD script
        $output = .\DevTools\Scripts\Add-Missing-CRUD.ps1 -RepositoryPath $repoPath -DryRun:$DryRun 2>&1

        # Check if methods were added
        if ($output -like "*All standard CRUD methods already exist*") {
            Write-Host "  ? Complete (all CRUD exists)" -ForegroundColor Green
            $stats.Complete++
            $completeRepos += $repoName
        }
        elseif ($output -like "*Adding * missing method*") {
            # Extract count
            $count = [regex]::Match($output, 'Adding (\d+) missing').Groups[1].Value
            Write-Host "  + Updated ($count methods added)" -ForegroundColor Cyan
            $stats.Updated++
            $updatedRepos += @{
                Name = $repoName
                MethodsAdded = $count
            }
        }
        else {
            Write-Host "  ? Unknown status" -ForegroundColor Yellow
            $stats.Skipped++
        }
    }
    catch {
        Write-Host "  ? ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $stats.Failed++
    }

    Write-Host ""
}

# Summary
Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?  Summary                                                   ?" -ForegroundColor Green
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Total Repositories: $($stats.Total)" -ForegroundColor White
Write-Host "  ? Already Complete: $($stats.Complete)" -ForegroundColor Green
Write-Host "  + Updated: $($stats.Updated)" -ForegroundColor Cyan
Write-Host "  ? Skipped: $($stats.Skipped)" -ForegroundColor Yellow
Write-Host "  ? Failed: $($stats.Failed)" -ForegroundColor Red
Write-Host ""

if ($stats.Updated -gt 0) {
    Write-Host "Repositories Updated:" -ForegroundColor Cyan
    foreach ($repo in $updatedRepos) {
        Write-Host "  • $($repo.Name) ($($repo.MethodsAdded) methods)" -ForegroundColor Gray
    }
    Write-Host ""
}

if ($DryRun) {
    Write-Host "This was a DRY RUN." -ForegroundColor Yellow
    Write-Host "Run without -DryRun to save changes." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Command to execute:" -ForegroundColor Cyan
    Write-Host "  .\DevTools\Scripts\Batch-Add-Missing-CRUD.ps1" -ForegroundColor White
} else {
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  For each updated repository:" -ForegroundColor Cyan
    Write-Host "    1. Open the repository file" -ForegroundColor Gray
    Write-Host "    2. Find 'TODO' comments" -ForegroundColor Gray
    Write-Host "    3. Fill in INSERT/UPDATE SQL from legacy Controls\*Tbl.cs" -ForegroundColor Gray
    Write-Host "    4. Use guide: DevTools\Documentation\Zero_AI_CRUD_Fill_Guide.md" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Build and test:" -ForegroundColor Cyan
    Write-Host "    dotnet clean" -ForegroundColor Gray
    Write-Host "    dotnet build" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Estimate: 10-15 minutes per repository with TODO" -ForegroundColor Yellow
    Write-Host "  Total time: ~$($stats.Updated * 12) minutes ($([math]::Round($stats.Updated * 12 / 60, 1)) hours)" -ForegroundColor Yellow
}

Write-Host ""
