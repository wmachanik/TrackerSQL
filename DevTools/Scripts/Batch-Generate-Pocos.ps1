# Identifies missing POCOs and creates templates
# Created: 2026-05-15
# Purpose: Create missing POCO classes (0 AI calls)

param(
    [string]$CsvPath = "DevTools\Documentation\Legacy_To_Poco_Mapping.csv",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "Missing POCO Generator" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan
Write-Host ""

# Read CSV
if (-not (Test-Path $CsvPath)) {
    Write-Host "ERROR: CSV not found: $CsvPath" -ForegroundColor Red
    exit 1
}

$mapping = Import-Csv $CsvPath

# Filter to missing POCOs only
$missing = $mapping | Where-Object { $_.PocoExists -eq "NO" }

Write-Host "Found $($missing.Count) missing POCOs" -ForegroundColor Yellow
Write-Host ""

if ($missing.Count -eq 0) {
    Write-Host "All POCOs already exist! ?" -ForegroundColor Green
    exit 0
}

function Get-LegacyClassProperties {
    param([string]$LegacyClassPath)

    if (-not (Test-Path $LegacyClassPath)) {
        return @()
    }

    $content = Get-Content $LegacyClassPath -Raw

    # Extract public properties
    $propertyMatches = [regex]::Matches($content, 'public\s+(\w+(?:<[\w,\s]+>)?)\s+(\w+)\s*{\s*get;\s*set;?\s*}')

    $properties = @()
    foreach ($match in $propertyMatches) {
        $properties += @{
            Type = $match.Groups[1].Value
            Name = $match.Groups[2].Value
        }
    }

    return $properties
}

function Generate-PocoClass {
    param(
        [string]$PocoName,
        [string]$LegacyClassPath,
        [switch]$DryRun
    )

    $pocoPath = "Classes\Poco\$PocoName.cs"

    if (Test-Path $pocoPath) {
        Write-Host "  ? SKIP: POCO already exists" -ForegroundColor Yellow
        return
    }

    # Get properties from legacy class
    $properties = Get-LegacyClassProperties -LegacyClassPath $LegacyClassPath

    if ($properties.Count -eq 0) {
        Write-Host "  ? WARNING: No properties found in legacy class" -ForegroundColor Yellow
        Write-Host "  Creating minimal POCO template" -ForegroundColor Gray
    }

    # Generate POCO code
    $code = @"
using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// POCO for $PocoName
    /// Auto-generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    /// Source: $LegacyClassPath
    /// </summary>
    public class $PocoName : ILookupEntity
    {
"@

    if ($properties.Count -gt 0) {
        foreach ($prop in $properties) {
            $code += "`n        public $($prop.Type) $($prop.Name) { get; set; }"
        }
    } else {
        # Minimal template
        $code += @"

        // TODO: Add properties from legacy class or database schema
        // Example:
        // public int ${PocoName}ID { get; set; }
        // public string Name { get; set; }
        // public bool Enabled { get; set; }
"@
    }

    $code += @"


        // ILookupEntity implementation
        public int ID 
        { 
            get 
            { 
                // TODO: Return the primary key property
                // Example: return ${PocoName}ID;
                throw new NotImplementedException("Set ID property in $PocoName.cs");
            } 
        }

        public string DisplayText 
        { 
            get 
            { 
                // TODO: Return display text for dropdowns/lists
                // Example: return Name;
                throw new NotImplementedException("Set DisplayText property in $PocoName.cs");
            } 
        }
    }
}
"@

    if ($DryRun) {
        Write-Host "  [DRY RUN] Would create: $pocoPath" -ForegroundColor Gray
        Write-Host "  Properties: $($properties.Count)" -ForegroundColor Gray
    } else {
        Set-Content -Path $pocoPath -Value $code -Encoding UTF8
        Write-Host "  ? Created: $pocoPath" -ForegroundColor Green
        Write-Host "  Properties: $($properties.Count)" -ForegroundColor Gray
    }
}

$successCount = 0
$skipCount = 0
$failCount = 0

foreach ($item in $missing) {
    $pocoName = $item.PocoName.Trim()
    $legacyClass = $item.LegacyClass.Trim()
    $legacyPath = "Controls\$legacyClass.cs"

    Write-Host "[$($successCount + $skipCount + $failCount + 1)/$($missing.Count)] Processing: $legacyClass -> $pocoName" -ForegroundColor Cyan

    try {
        Generate-PocoClass -PocoName $pocoName -LegacyClassPath $legacyPath -DryRun:$DryRun
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
    Write-Host "  1. Review generated POCOs in Classes\Poco\"
    Write-Host "  2. Fill in TODO sections (ID and DisplayText)"
    Write-Host "  3. Verify properties match database schema"
    Write-Host "  4. Re-run analysis: .\DevTools\Scripts\Analyze-Legacy-Classes.ps1"
}
