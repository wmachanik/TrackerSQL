# Generate-PocoClasses.ps1
# Analyzes table classes and generates missing POCO classes
# Created: 2025-05-14

param(
    [string]$RootPath = "C:\SRC\ASP.net\TrackerSQL",
    [string]$ControlsPath = "Controls",
    [string]$PocoPath = "Classes\Poco",
    [switch]$GenerateFiles
)

Write-Host "POCO Class Generator" -ForegroundColor Cyan
Write-Host "====================" -ForegroundColor Cyan
Write-Host ""

$controlsFullPath = Join-Path $RootPath $ControlsPath
$pocoFullPath = Join-Path $RootPath $PocoPath

# Get all *Tbl.cs files
$tableFiles = Get-ChildItem -Path $controlsFullPath -Filter "*Tbl.cs" -File

Write-Host "Found $($tableFiles.Count) table class files" -ForegroundColor Yellow
Write-Host ""

$pocosToCreate = @()

foreach ($tableFile in $tableFiles) {
    $tableName = $tableFile.BaseName

    # Expected POCO name (remove 'Tbl' suffix)
    $pocoName = $tableName -replace 'Tbl$', ''
    $pocoFileName = "$pocoName.cs"
    $pocoFilePath = Join-Path $pocoFullPath $pocoFileName

    # Check if POCO exists
    if (-not (Test-Path $pocoFilePath)) {
        Write-Host "Missing POCO: $pocoName (for $tableName)" -ForegroundColor Yellow

        # Try to parse properties from table class
        $content = Get-Content $tableFile.FullName -Raw

        # Extract properties using regex
        $propertyPattern = 'public\s+(\w+(?:<[\w,\s]+>)?)\s+(\w+)\s*{\s*get\s*=>\s*this\._(\w+);'
        $matches = [regex]::Matches($content, $propertyPattern)

        $properties = @()
        foreach ($match in $matches) {
            $type = $match.Groups[1].Value
            $propertyName = $match.Groups[2].Value

            $properties += [PSCustomObject]@{
                Type = $type
                Name = $propertyName
            }
        }

        $pocosToCreate += [PSCustomObject]@{
            PocoName = $pocoName
            TableName = $tableName
            PocoFilePath = $pocoFilePath
            Properties = $properties
        }
    }
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "--------"
Write-Host "Total table classes: $($tableFiles.Count)" -ForegroundColor White
Write-Host "Missing POCOs: $($pocosToCreate.Count)" -ForegroundColor Yellow
Write-Host ""

if ($pocosToCreate.Count -eq 0) {
    Write-Host "All POCO classes exist!" -ForegroundColor Green
    exit 0
}

Write-Host "POCOs to create:" -ForegroundColor Cyan
foreach ($poco in $pocosToCreate) {
    Write-Host "  - $($poco.PocoName) ($($poco.Properties.Count) properties)" -ForegroundColor Yellow
}
Write-Host ""

if ($GenerateFiles) {
    Write-Host "Generating POCO files..." -ForegroundColor Cyan
    Write-Host ""

    foreach ($poco in $pocosToCreate) {
        Write-Host "Generating: $($poco.PocoName).cs" -ForegroundColor Yellow

        # Generate POCO class content
        $pocoContent = @"
using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// POCO class for $($poco.PocoName)
    /// Generated from $($poco.TableName)
    /// </summary>
    public class $($poco.PocoName)
    {
"@

        foreach ($prop in $poco.Properties) {
            $pocoContent += @"

        public $($prop.Type) $($prop.Name) { get; set; }
"@
        }

        $pocoContent += @"

    }
}
"@

        # Write file
        $pocoContent | Out-File -FilePath $poco.PocoFilePath -Encoding UTF8
        Write-Host "  ? Created: $($poco.PocoFilePath)" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "POCO generation complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "IMPORTANT:" -ForegroundColor Red
    Write-Host "1. Review generated POCO classes" -ForegroundColor Yellow
    Write-Host "2. Add any missing properties" -ForegroundColor Yellow
    Write-Host "3. Add XML documentation comments" -ForegroundColor Yellow
    Write-Host "4. Add to project if not auto-included" -ForegroundColor Yellow
} else {
    Write-Host "Run with -GenerateFiles to create POCO classes" -ForegroundColor Magenta
}

Write-Host ""

return $pocosToCreate
