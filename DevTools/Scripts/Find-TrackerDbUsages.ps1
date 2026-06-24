# Find-TrackerDbUsages.ps1
# Analyzes all .cs files for TrackerDb (OleDb) usage patterns
# Created: 2025-05-14

param(
    [string]$RootPath = "C:\SRC\ASP.net\TrackerSQL",
    [string]$OutputFile = "DevTools\Documentation\TrackerDb_Usage_Report.txt"
)

Write-Host "TrackerDb Usage Analyzer" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host ""

# Patterns to search for
$patterns = @{
    "NewTrackerDb" = "new TrackerDb\(\)"
    "TrackerDbVariable" = "TrackerDb\s+\w+\s*="
    "AddParams" = "\.AddParams\("
    "AddWhereParams" = "\.AddWhereParams\("
    "ExecuteNonQuerySQL" = "\.ExecuteNonQuerySQL\("
    "ExecuteSQLGetDataReader" = "\.ExecuteSQLGetDataReader\("
    "TrackerDbClose" = "trackerDb\.Close\(\)"
}

# Results storage
$results = @{}

# Get all .cs files, excluding TrackerDb.cs itself and migration files
$files = Get-ChildItem -Path $RootPath -Filter "*.cs" -Recurse | 
    Where-Object { 
        $_.FullName -notlike "*\TrackerDb.cs" -and 
        $_.FullName -notlike "*\TrackerSQLDb.cs" -and
        $_.FullName -notlike "*\Migrations\*" -and
        $_.FullName -notlike "*\bin\*" -and
        $_.FullName -notlike "*\obj\*"
    }

Write-Host "Scanning $($files.Count) files..." -ForegroundColor Yellow
Write-Host ""

$totalMatches = 0
$filesWithMatches = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $fileMatches = @{}
    $hasMatches = $false

    foreach ($patternName in $patterns.Keys) {
        $pattern = $patterns[$patternName]
        $matches = [regex]::Matches($content, $pattern)

        if ($matches.Count -gt 0) {
            $fileMatches[$patternName] = $matches.Count
            $totalMatches += $matches.Count
            $hasMatches = $true
        }
    }

    if ($hasMatches) {
        $filesWithMatches++
        $relativePath = $file.FullName.Replace($RootPath, "").TrimStart('\')
        $results[$relativePath] = $fileMatches
    }
}

# Generate Report
$report = @()
$report += "TrackerDb Usage Analysis Report"
$report += "================================"
$report += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$report += ""
$report += "SUMMARY"
$report += "-------"
$report += "Total Files Scanned: $($files.Count)"
$report += "Files with TrackerDb Usage: $filesWithMatches"
$report += "Total Pattern Matches: $totalMatches"
$report += ""
$report += "FILES REQUIRING MIGRATION"
$report += "========================="
$report += ""

# Sort files by priority (Controls folder first, then by match count)
$sortedResults = $results.GetEnumerator() | Sort-Object {
    $priority = 0
    if ($_.Key -like "Controls\*Tbl.cs") { $priority = 1 }
    elseif ($_.Key -like "Controls\*DAL.cs") { $priority = 2 }
    elseif ($_.Key -like "Controls\*.cs") { $priority = 3 }
    elseif ($_.Key -like "Classes\*.cs") { $priority = 4 }
    elseif ($_.Key -like "Pages\*.cs") { $priority = 5 }
    else { $priority = 6 }

    # Return sort key: lower priority number comes first, then by total matches descending
    $totalFileMatches = ($_.Value.Values | Measure-Object -Sum).Sum
    return @($priority, -$totalFileMatches)
}

$currentCategory = ""
foreach ($fileEntry in $sortedResults) {
    $filePath = $fileEntry.Key
    $matches = $fileEntry.Value

    # Categorize files
    $category = "OTHER"
    if ($filePath -like "Controls\*Tbl.cs") { $category = "CONTROLS - TABLE CLASSES" }
    elseif ($filePath -like "Controls\*DAL.cs") { $category = "CONTROLS - DATA ACCESS" }
    elseif ($filePath -like "Controls\*.cs") { $category = "CONTROLS - OTHER" }
    elseif ($filePath -like "Classes\*.cs") { $category = "CLASSES" }
    elseif ($filePath -like "Pages\*.cs") { $category = "PAGES" }
    elseif ($filePath -like "Tools\*.cs") { $category = "TOOLS" }

    if ($category -ne $currentCategory) {
        $report += ""
        $report += $category
        $report += ("-" * $category.Length)
        $currentCategory = $category
    }

    $totalFileMatches = ($matches.Values | Measure-Object -Sum).Sum
    $report += ""
    $report += "File: $filePath"
    $report += "  Total Matches: $totalFileMatches"

    foreach ($patternName in $matches.Keys | Sort-Object) {
        $count = $matches[$patternName]
        $report += "    - $patternName : $count"
    }
}

# Add Migration Priority Recommendations
$report += ""
$report += ""
$report += "MIGRATION PRIORITY RECOMMENDATIONS"
$report += "==================================="
$report += ""

$report += "PHASE 1 - CRITICAL (Week 1)"
$report += "---------------------------"
$highPriority = $sortedResults | Where-Object { 
    $_.Key -like "Controls\CustomersTbl.cs" -or
    $_.Key -like "Controls\OrdersTbl.cs" -or
    $_.Key -like "Controls\ItemTypeTbl.cs" -or
    $_.Key -like "Controls\OrderDataControl.cs"
}
foreach ($file in $highPriority) {
    $totalMatches = ($file.Value.Values | Measure-Object -Sum).Sum
    $report += "  - $($file.Key) ($totalMatches usages)"
}

$report += ""
$report += "PHASE 2 - HIGH IMPACT (Week 2)"
$report += "-------------------------------"
$medHighPriority = $sortedResults | Where-Object { 
    $_.Key -like "Controls\ReoccuringOrderDAL.cs" -or
    $_.Key -like "Controls\ItemUsageTbl.cs" -or
    $_.Key -like "Controls\PersonsTbl.cs" -or
    $_.Key -like "Controls\OrderDetailDAL.cs" -or
    $_.Key -like "Controls\ClientUsageTbl.cs"
}
foreach ($file in $medHighPriority) {
    $totalMatches = ($file.Value.Values | Measure-Object -Sum).Sum
    $report += "  - $($file.Key) ($totalMatches usages)"
}

$report += ""
$report += "PHASE 3 - SUPPORTING TABLES (Week 3)"
$report += "-------------------------------------"
$report += "  All *Tbl.cs and *DAL.cs files not in Phase 1 or 2"

$report += ""
$report += "PHASE 4 - LOOKUP TABLES (Week 4)"
$report += "---------------------------------"
$report += "  Simple CRUD operations, logging, configuration"

# Output to console
foreach ($line in $report) {
    Write-Host $line
}

# Save to file
$reportPath = Join-Path $RootPath $OutputFile
New-Item -Path (Split-Path $reportPath -Parent) -ItemType Directory -Force | Out-Null
$report | Out-File -FilePath $reportPath -Encoding UTF8

Write-Host ""
Write-Host "Report saved to: $reportPath" -ForegroundColor Green
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Cyan
Write-Host "1. Review the report file" -ForegroundColor Yellow
Write-Host "2. Start with Phase 1 files (CRITICAL)" -ForegroundColor Yellow
Write-Host "3. Use AreaPrepDaysTbl.cs as migration template" -ForegroundColor Yellow
Write-Host "4. Test thoroughly after each file migration" -ForegroundColor Yellow
Write-Host ""

# Return summary object
return [PSCustomObject]@{
    TotalFiles = $files.Count
    FilesWithUsages = $filesWithMatches
    TotalMatches = $totalMatches
    Results = $results
    ReportPath = $reportPath
}
