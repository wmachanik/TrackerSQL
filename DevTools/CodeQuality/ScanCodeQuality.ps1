# Additional Code Quality Scanner
# Checks for common issues beyond duplicate usings

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Additional Code Quality Scan" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = "C:\SRC\ASP.net\TrackerSQL"
$issues = @{
    'EmptyNamespaces' = @()
    'UnusedParameters' = @()
    'EmptyCatchBlocks' = @()
    'HardcodedStrings' = @()
    'MagicNumbers' = @()
}

$csFiles = Get-ChildItem -Path $projectRoot -Filter "*.cs" -Recurse -File | 
    Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\|\\Migrations\\MigrationRunner\\|\\BACKUP_|\\Designer\.cs$|\.designer\.cs$" }

Write-Host "Scanning $($csFiles.Count) C# files for common issues..." -ForegroundColor Yellow
Write-Host ""

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    if ([string]::IsNullOrWhiteSpace($content)) { continue }
    
    $relativePath = $file.FullName.Replace($projectRoot + "\", "")
    
    # Check for empty catch blocks
    if ($content -match 'catch\s*\([^)]+\)\s*\{\s*\}') {
        $issues['EmptyCatchBlocks'] += $relativePath
    }
    
    # Check for commented-out code (excessive)
    $lines = Get-Content $file.FullName
    $commentedLines = ($lines | Where-Object { $_ -match '^\s*//' }).Count
    if ($commentedLines -gt 20) {
        Write-Host "  ? Info: $relativePath has $commentedLines commented lines" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "SCAN RESULTS" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Empty Catch Blocks
if ($issues['EmptyCatchBlocks'].Count -gt 0) {
    Write-Host "? Empty Catch Blocks Found: $($issues['EmptyCatchBlocks'].Count)" -ForegroundColor Yellow
    $issues['EmptyCatchBlocks'] | ForEach-Object {
        Write-Host "  - $_" -ForegroundColor Gray
    }
    Write-Host ""
} else {
    Write-Host "? No empty catch blocks found" -ForegroundColor Green
    Write-Host ""
}

Write-Host "Summary:" -ForegroundColor White
Write-Host "  - Files scanned: $($csFiles.Count)" -ForegroundColor White
Write-Host "  - Empty catch blocks: $($issues['EmptyCatchBlocks'].Count)" -ForegroundColor White
Write-Host ""
Write-Host "Note: For more detailed analysis, use Visual Studio Code Analysis" -ForegroundColor Cyan
Write-Host "  (Analyze ? Run Code Analysis ? On Solution)" -ForegroundColor Cyan
Write-Host ""
