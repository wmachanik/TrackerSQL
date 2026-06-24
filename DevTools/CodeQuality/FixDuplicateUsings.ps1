# Fix Duplicate Using Statements Script
# Scans all C# files and removes duplicate using statements

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Duplicate Using Statements Cleanup" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = "C:\SRC\ASP.net\TrackerSQL"
$filesFixed = 0
$totalDuplicatesRemoved = 0

# Get all C# files (excluding bin, obj, and Migrations)
$csFiles = Get-ChildItem -Path $projectRoot -Filter "*.cs" -Recurse -File | 
    Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\|\\Migrations\\MigrationRunner\\|\\BACKUP_" }

Write-Host "Scanning $($csFiles.Count) C# files..." -ForegroundColor Yellow
Write-Host ""

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    if ([string]::IsNullOrWhiteSpace($content)) { continue }
    
    $lines = Get-Content $file.FullName
    $newLines = New-Object System.Collections.ArrayList
    $usingStatements = New-Object System.Collections.Generic.HashSet[string]
    $duplicatesInFile = 0
    $inUsingBlock = $false
    $firstNonUsingLineIndex = -1
    
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        
        # Check if line is a using statement
        if ($line -match '^\s*using\s+[\w\.]+\s*;?\s*$') {
            $inUsingBlock = $true
            $trimmedLine = $line.Trim()
            
            # Check if this using statement is a duplicate
            if ($usingStatements.Contains($trimmedLine)) {
                $duplicatesInFile++
                # Skip this duplicate line
                continue
            } else {
                $usingStatements.Add($trimmedLine) | Out-Null
                [void]$newLines.Add($line)
            }
        }
        # Check if we're past the using statements block
        elseif ($line -match '^\s*namespace\s+' -or 
                $line -match '^\s*\[assembly:' -or 
                ($line.Trim() -ne '' -and $line -notmatch '^\s*//' -and $inUsingBlock)) {
            if ($inUsingBlock -and $firstNonUsingLineIndex -eq -1) {
                $firstNonUsingLineIndex = $i
            }
            $inUsingBlock = $false
            [void]$newLines.Add($line)
        }
        else {
            [void]$newLines.Add($line)
        }
    }
    
    if ($duplicatesInFile -gt 0) {
        $filesFixed++
        $totalDuplicatesRemoved += $duplicatesInFile
        
        # Write the cleaned content back to the file
        $newContent = $newLines -join "`r`n"
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        
        $relativePath = $file.FullName.Replace($projectRoot + "\", "")
        Write-Host "  ? Fixed: $relativePath" -ForegroundColor Green
        Write-Host "    Removed $duplicatesInFile duplicate using statement(s)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "CLEANUP COMPLETE" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  - Files scanned: $($csFiles.Count)" -ForegroundColor White
Write-Host "  - Files fixed: $filesFixed" -ForegroundColor White
Write-Host "  - Total duplicate usings removed: $totalDuplicatesRemoved" -ForegroundColor White
Write-Host ""

if ($filesFixed -gt 0) {
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Rebuild solution to verify no errors" -ForegroundColor White
    Write-Host "  2. Review changes if needed" -ForegroundColor White
    Write-Host "  3. Commit changes to Git" -ForegroundColor White
} else {
    Write-Host "No duplicate using statements found! ?" -ForegroundColor Green
}
Write-Host ""
