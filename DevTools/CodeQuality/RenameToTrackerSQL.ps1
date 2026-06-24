# Comprehensive Namespace Rename: TrackerDotNet ? TrackerSQL
# This script renames all TrackerDotNet references to TrackerSQL

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Namespace Rename: TrackerDotNet ? TrackerSQL" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = "C:\SRC\ASP.net\TrackerSQL"
$backupFolder = Join-Path $projectRoot "BACKUP_BEFORE_RENAME_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

# Step 1: Create backup
Write-Host "Step 1: Creating backup..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null
Copy-Item -Path "$projectRoot\*.csproj" -Destination $backupFolder -Force
Write-Host "  ? Backed up project files to: $backupFolder" -ForegroundColor Green

# Step 2: Update project file (AssemblyName and RootNamespace)
Write-Host ""
Write-Host "Step 2: Updating TrackerSQL.csproj..." -ForegroundColor Yellow
$projFile = Join-Path $projectRoot "TrackerSQL.csproj"
$projContent = Get-Content $projFile -Raw
$projContent = $projContent.Replace("<RootNamespace>TrackerDotNet</RootNamespace>", "<RootNamespace>TrackerSQL</RootNamespace>")
$projContent = $projContent.Replace("<AssemblyName>TrackerDotNet</AssemblyName>", "<AssemblyName>TrackerSQL</AssemblyName>")
Set-Content -Path $projFile -Value $projContent -NoNewline
Write-Host "  ? Updated AssemblyName: TrackerDotNet ? TrackerSQL" -ForegroundColor Green
Write-Host "  ? Updated RootNamespace: TrackerDotNet ? TrackerSQL" -ForegroundColor Green

# Step 3: Find and replace in all C# files
Write-Host ""
Write-Host "Step 3: Updating C# source files..." -ForegroundColor Yellow

$csFiles = Get-ChildItem -Path $projectRoot -Filter "*.cs" -Recurse -File | 
    Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\|\\Migrations\\MigrationRunner\\|\\BACKUP_" }

$fileCount = 0
$totalReplaced = 0

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Replace namespace declarations
    $content = $content -replace "namespace TrackerDotNet\.", "namespace TrackerSQL."
    $content = $content -replace "namespace TrackerDotNet([^.])", "namespace TrackerSQL$1"
    
    # Replace using statements
    $content = $content -replace "using TrackerDotNet\.", "using TrackerSQL."
    $content = $content -replace "using TrackerDotNet;", "using TrackerSQL;"
    
    # Replace fully qualified references
    $content = $content -replace "TrackerDotNet\.Classes\.", "TrackerSQL.Classes."
    $content = $content -replace "TrackerDotNet\.Controls\.", "TrackerSQL.Controls."
    $content = $content -replace "TrackerDotNet\.DataSets\.", "TrackerSQL.DataSets."
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $fileCount++
        $replacements = ([regex]::Matches($originalContent, "TrackerDotNet")).Count
        $totalReplaced += $replacements
        Write-Host "  ? Updated: $($file.Name) ($replacements replacements)" -ForegroundColor Gray
    }
}

Write-Host "  ? Updated $fileCount files with $totalReplaced total replacements" -ForegroundColor Green

# Step 4: Update ASPX files
Write-Host ""
Write-Host "Step 4: Updating ASPX files..." -ForegroundColor Yellow

$aspxFiles = Get-ChildItem -Path $projectRoot -Filter "*.aspx" -Recurse -File |
    Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\" }

$aspxCount = 0
$aspxReplaced = 0

foreach ($file in $aspxFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Replace Inherits attributes
    $content = $content -replace 'Inherits="TrackerDotNet\.', 'Inherits="TrackerSQL.'
    $content = $content -replace 'CodeBehind="TrackerDotNet\.', 'CodeBehind="TrackerSQL.'
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $aspxCount++
        $replacements = ([regex]::Matches($originalContent, "TrackerDotNet")).Count
        $aspxReplaced += $replacements
        Write-Host "  ? Updated: $($file.Name) ($replacements replacements)" -ForegroundColor Gray
    }
}

Write-Host "  ? Updated $aspxCount ASPX files with $aspxReplaced total replacements" -ForegroundColor Green

# Step 5: Summary
Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "RENAME COMPLETE!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  - C# files updated: $fileCount" -ForegroundColor White
Write-Host "  - ASPX files updated: $aspxCount" -ForegroundColor White
Write-Host "  - Total replacements: $($totalReplaced + $aspxReplaced)" -ForegroundColor White
Write-Host "  - Backup location: $backupFolder" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Close Visual Studio if open" -ForegroundColor White
Write-Host "  2. Delete bin\ and obj\ folders" -ForegroundColor White
Write-Host "  3. Reopen solution in Visual Studio" -ForegroundColor White
Write-Host "  4. Rebuild Solution (Ctrl+Shift+B)" -ForegroundColor White
Write-Host "  5. Run full regression tests" -ForegroundColor White
Write-Host ""
Write-Host "If issues occur, restore from: $backupFolder" -ForegroundColor Yellow
Write-Host ""
