# Migrate-TrackerDbFile.ps1
# Semi-automated migration tool for converting TrackerDb to TrackerSQLDb
# Created: 2025-05-14

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,

    [switch]$DryRun,
    [switch]$CreateBackup = $true
)

$ErrorActionPreference = "Stop"

Write-Host "TrackerDb File Migration Tool" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

# Validate file exists
if (-not (Test-Path $FilePath)) {
    Write-Error "File not found: $FilePath"
    exit 1
}

$fileName = Split-Path $FilePath -Leaf
Write-Host "Processing: $fileName" -ForegroundColor Yellow
Write-Host ""

# Create backup if requested
if ($CreateBackup -and -not $DryRun) {
    $backupPath = "$FilePath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $FilePath $backupPath
    Write-Host "Backup created: $backupPath" -ForegroundColor Green
}

# Read file content
$content = Get-Content $FilePath -Raw

# Pattern replacements
$replacements = @{
    # 1. Replace TrackerDb variable declarations
    'TrackerDb\s+(\w+)\s*=\s*new\s+TrackerDb\(\)' = 'using (var $1 = new TrackerSQLDb())`n    {'

    # 2. Replace close statements with closing brace
    '(\w+)\.Close\(\);' = '} // Auto-dispose'

    # 3. Replace ExecuteNonQuerySQL calls (need manual parameter handling)
    '\.ExecuteNonQuerySQL\(([^)]+)\)' = '.ExecuteNonQuery(sql, parameters) // TODO: Add parameters list'

    # 4. Replace ExecuteSQLGetDataReader calls
    '\.ExecuteSQLGetDataReader\(([^)]+)\)' = '.ExecuteReader(sql, parameters) // TODO: Add parameters list'
}

$originalContent = $content
$modifiedContent = $content

# Track changes
$changesCount = 0

foreach ($pattern in $replacements.Keys) {
    $replacement = $replacements[$pattern]
    $matches = [regex]::Matches($modifiedContent, $pattern)

    if ($matches.Count -gt 0) {
        Write-Host "Found $($matches.Count) instances of pattern: $pattern" -ForegroundColor Yellow
        $modifiedContent = [regex]::Replace($modifiedContent, $pattern, $replacement)
        $changesCount += $matches.Count
    }
}

# Report results
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "--------"
Write-Host "Total patterns replaced: $changesCount" -ForegroundColor $(if ($changesCount -gt 0) { "Yellow" } else { "Gray" })

if ($DryRun) {
    Write-Host ""
    Write-Host "DRY RUN - No changes written" -ForegroundColor Magenta
    Write-Host ""
    Write-Host "Preview of changes:" -ForegroundColor Cyan
    Write-Host "===================" -ForegroundColor Cyan
    Write-Host $modifiedContent.Substring(0, [Math]::Min(1000, $modifiedContent.Length))
    Write-Host "..." -ForegroundColor Gray
} else {
    # Write modified content
    $modifiedContent | Out-File -FilePath $FilePath -Encoding UTF8 -NoNewline
    Write-Host ""
    Write-Host "File updated successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "IMPORTANT MANUAL STEPS:" -ForegroundColor Red
    Write-Host "1. Review all TODO comments in the file" -ForegroundColor Yellow
    Write-Host "2. Convert AddParams/AddWhereParams to List<DBParameter>" -ForegroundColor Yellow
    Write-Host "3. Update SQL queries to use @ParamName instead of ?" -ForegroundColor Yellow
    Write-Host "4. Wrap IDataReader in using statements" -ForegroundColor Yellow
    Write-Host "5. Add error handling and logging" -ForegroundColor Yellow
    Write-Host "6. Test thoroughly!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Open the file in Visual Studio"
Write-Host "2. Search for 'TODO' comments"
Write-Host "3. Use Quick_Reference_TrackerDb_Migration.md for patterns"
Write-Host "4. Test each method as you complete it"
Write-Host ""

return [PSCustomObject]@{
    FilePath = $FilePath
    ChangesCount = $changesCount
    Modified = -not $DryRun
    BackupPath = if ($CreateBackup -and -not $DryRun) { $backupPath } else { $null }
}
