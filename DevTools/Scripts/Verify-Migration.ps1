# Verify-Migration.ps1
# Verifies TrackerDb migration completeness
# Created: 2025-05-14

param(
    [string]$RootPath = "C:\SRC\ASP.net\TrackerSQL",
    [switch]$DetailedReport
)

Write-Host "TrackerDb Migration Verification" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Patterns to check for (old patterns that should be gone)
$oldPatterns = @{
    "NewTrackerDb" = "new TrackerDb\(\)"
    "TrackerDbVariable" = "TrackerDb\s+\w+\s*="
    "AddParams" = "\.AddParams\("
    "AddWhereParams" = "\.AddWhereParams\("
    "ExecuteNonQuerySQL" = "\.ExecuteNonQuerySQL\("
    "ExecuteSQLGetDataReader" = "\.ExecuteSQLGetDataReader\("
    "TrackerDbClose" = "trackerDb\.Close\(\)"
    "QuestionMarkParams" = "VALUES\s*\([^)]*\?\s*[,)]"
    "WhereQuestionMark" = "WHERE\s+\w+\s*=\s*\?"
}

# Patterns to check for (new patterns that should exist)
$newPatterns = @{
    "NewTrackerSQLDb" = "new TrackerSQLDb\(\)"
    "DBParameterList" = "List<DBParameter>"
    "ExecuteNonQuery" = "\.ExecuteNonQuery\("
    "ExecuteReader" = "\.ExecuteReader\("
    "NamedParams" = "@\w+\s*(?:,|\))"
    "UsingStatement" = "using\s*\(.*TrackerSQLDb"
}

# Get all .cs files, excluding the TrackerDb.cs itself and migration files
$files = Get-ChildItem -Path $RootPath -Filter "*.cs" -Recurse | 
    Where-Object { 
        $_.FullName -notlike "*\TrackerDb.cs" -and
        $_.FullName -notlike "*\TrackerSQLDb.cs" -and
        $_.FullName -notlike "*\Migrations\*" -and
        $_.FullName -notlike "*\bin\*" -and
        $_.FullName -notlike "*\obj\*" -and
        $_.FullName -notlike "*\DevTools\*"
    }

Write-Host "Scanning $($files.Count) files..." -ForegroundColor Yellow
Write-Host ""

# Track results
$filesWithOldPatterns = @()
$filesWithNewPatterns = @()
$fullyMigrated = @()
$notStarted = @()
$inProgress = @()

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $relativePath = $file.FullName.Replace($RootPath, "").TrimStart('\')

    $hasOldPatterns = $false
    $hasNewPatterns = $false
    $oldMatches = @{}
    $newMatches = @{}

    # Check for old patterns
    foreach ($patternName in $oldPatterns.Keys) {
        $pattern = $oldPatterns[$patternName]
        $matches = [regex]::Matches($content, $pattern)

        if ($matches.Count -gt 0) {
            $hasOldPatterns = $true
            $oldMatches[$patternName] = $matches.Count
        }
    }

    # Check for new patterns
    foreach ($patternName in $newPatterns.Keys) {
        $pattern = $newPatterns[$patternName]
        $matches = [regex]::Matches($content, $pattern)

        if ($matches.Count -gt 0) {
            $hasNewPatterns = $true
            $newMatches[$patternName] = $matches.Count
        }
    }

    # Categorize file
    if ($hasOldPatterns -and $hasNewPatterns) {
        $inProgress += [PSCustomObject]@{
            File = $relativePath
            OldMatches = $oldMatches
            NewMatches = $newMatches
            Status = "In Progress"
        }
    }
    elseif ($hasOldPatterns -and -not $hasNewPatterns) {
        $notStarted += [PSCustomObject]@{
            File = $relativePath
            OldMatches = $oldMatches
            Status = "Not Started"
        }
    }
    elseif (-not $hasOldPatterns -and $hasNewPatterns) {
        $fullyMigrated += [PSCustomObject]@{
            File = $relativePath
            NewMatches = $newMatches
            Status = "Fully Migrated"
        }
    }
    # else: No database access at all
}

# Calculate statistics
$totalWithDbAccess = $fullyMigrated.Count + $inProgress.Count + $notStarted.Count
$percentComplete = if ($totalWithDbAccess -gt 0) { 
    [Math]::Round(($fullyMigrated.Count / $totalWithDbAccess) * 100, 2) 
} else { 
    100 
}

# Display summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MIGRATION STATUS SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total files with DB access: $totalWithDbAccess" -ForegroundColor White
Write-Host ""
Write-Host "? Fully Migrated: $($fullyMigrated.Count)" -ForegroundColor Green
Write-Host "?? In Progress: $($inProgress.Count)" -ForegroundColor Yellow
Write-Host "? Not Started: $($notStarted.Count)" -ForegroundColor Red
Write-Host ""
Write-Host "Progress: $percentComplete%" -ForegroundColor $(
    if ($percentComplete -eq 100) { "Green" }
    elseif ($percentComplete -ge 50) { "Yellow" }
    else { "Red" }
)
Write-Host ""

# Detailed reports
if ($DetailedReport) {
    if ($notStarted.Count -gt 0) {
        Write-Host "? NOT STARTED ($($notStarted.Count) files)" -ForegroundColor Red
        Write-Host "-----------------------------------" -ForegroundColor Red
        foreach ($file in $notStarted | Sort-Object File) {
            Write-Host "  $($file.File)" -ForegroundColor Red
            foreach ($pattern in $file.OldMatches.Keys) {
                Write-Host "    - $pattern : $($file.OldMatches[$pattern])" -ForegroundColor Gray
            }
        }
        Write-Host ""
    }

    if ($inProgress.Count -gt 0) {
        Write-Host "?? IN PROGRESS ($($inProgress.Count) files)" -ForegroundColor Yellow
        Write-Host "-----------------------------------" -ForegroundColor Yellow
        foreach ($file in $inProgress | Sort-Object File) {
            Write-Host "  $($file.File)" -ForegroundColor Yellow
            Write-Host "    Old patterns remaining:" -ForegroundColor Gray
            foreach ($pattern in $file.OldMatches.Keys) {
                Write-Host "      - $pattern : $($file.OldMatches[$pattern])" -ForegroundColor Gray
            }
            Write-Host "    New patterns found:" -ForegroundColor Gray
            foreach ($pattern in $file.NewMatches.Keys) {
                Write-Host "      - $pattern : $($file.NewMatches[$pattern])" -ForegroundColor Gray
            }
        }
        Write-Host ""
    }

    if ($fullyMigrated.Count -gt 0) {
        Write-Host "? FULLY MIGRATED ($($fullyMigrated.Count) files)" -ForegroundColor Green
        Write-Host "-----------------------------------" -ForegroundColor Green
        foreach ($file in $fullyMigrated | Sort-Object File) {
            Write-Host "  $($file.File)" -ForegroundColor Green
        }
        Write-Host ""
    }
}

# Overall status
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "OVERALL STATUS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($percentComplete -eq 100) {
    Write-Host "?? MIGRATION COMPLETE!" -ForegroundColor Green
    Write-Host ""
    Write-Host "All files have been migrated from TrackerDb to TrackerSQLDb!" -ForegroundColor Green
    Write-Host ""
    Write-Host "NEXT STEPS:" -ForegroundColor Cyan
    Write-Host "1. Mark TrackerDb.cs as [Obsolete]" -ForegroundColor Yellow
    Write-Host "2. Run full regression testing" -ForegroundColor Yellow
    Write-Host "3. Deploy to staging environment" -ForegroundColor Yellow
    Write-Host "4. Production deployment" -ForegroundColor Yellow
    Write-Host "5. Celebrate! ??" -ForegroundColor Yellow
}
elseif ($percentComplete -ge 75) {
    Write-Host "?? ALMOST THERE!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Migration is $percentComplete% complete" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Files remaining: $($inProgress.Count + $notStarted.Count)" -ForegroundColor Yellow
    Write-Host "  - In Progress: $($inProgress.Count)" -ForegroundColor Yellow
    Write-Host "  - Not Started: $($notStarted.Count)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Keep going! You're close to completion." -ForegroundColor Yellow
}
elseif ($percentComplete -ge 50) {
    Write-Host "?? HALFWAY THERE!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Migration is $percentComplete% complete" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Files remaining: $($inProgress.Count + $notStarted.Count)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Continue with the next phase!" -ForegroundColor Yellow
}
else {
    Write-Host "?? MIGRATION IN PROGRESS" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Migration is $percentComplete% complete" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Files remaining: $($inProgress.Count + $notStarted.Count)" -ForegroundColor White
    Write-Host ""
    Write-Host "Follow the phase-by-phase plan!" -ForegroundColor Yellow
}

Write-Host ""

# Return structured results
return [PSCustomObject]@{
    TotalFiles = $totalWithDbAccess
    FullyMigrated = $fullyMigrated.Count
    InProgress = $inProgress.Count
    NotStarted = $notStarted.Count
    PercentComplete = $percentComplete
    FullyMigratedFiles = $fullyMigrated
    InProgressFiles = $inProgress
    NotStartedFiles = $notStarted
}
