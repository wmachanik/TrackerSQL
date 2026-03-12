#!/usr/bin/env pwsh
# ConvertMigrationReportToImportFormat.ps1
# Converts the TableMigrationReport-09-Mar-26.csv format to PlanColumns.csv format
# that can be imported by option M

param(
    [string]$InputCsv = "Data\Metadata\PlanEdits\TableMigrationReport-09-Mar-26.csv",
    [string]$OutputDir = "Data\Metadata\PlanEdits"
)

Write-Host "Converting Migration Report to Import Format..." -ForegroundColor Cyan
Write-Host "Input:  $InputCsv" -ForegroundColor Gray
Write-Host "Output: $OutputDir\PlanColumns.csv" -ForegroundColor Gray

if (!(Test-Path $InputCsv)) {
    Write-Error "Input CSV not found: $InputCsv"
    exit 1
}

# Read all lines
$lines = Get-Content $InputCsv -Encoding UTF8

# Output will be PlanColumns.csv format
$outLines = @()
$outLines += "SourceTable,SourceColumn,TargetColumn,Action,Expression"

$currentSourceTable = ""
$currentTargetTable = ""
$inRowsSection = $false

foreach ($line in $lines) {
    # Skip empty lines and separator lines
    if ($line.Trim() -eq "" -or $line.StartsWith("------") -or $line.StartsWith("===")) {
        $inRowsSection = $false
        continue
    }

    # Parse CSV line (simple split, doesn't handle quoted commas perfectly but works for this case)
    $cols = $line -split ','
    
    # Detect table header line: "Table,Before,After,Action"
    if ($cols[0].Trim() -eq "Table") {
        $inRowsSection = $false
        continue
    }

    # Detect rows header line: "Rows:,Before Col name,..."
    if ($cols[0].Trim() -eq "Rows:") {
        $inRowsSection = $true
        continue
    }

    # Check if this is a table definition line (has comma in position 0, table name in position 1-2)
    if (!$inRowsSection -and $cols.Count -ge 4 -and $cols[0].Trim() -eq "") {
        # This is a table line: ,SourceTable,TargetTable,Action
        $currentSourceTable = $cols[1].Trim()
        $currentTargetTable = $cols[2].Trim()
        
        # Skip if target is "n/a" (Ignore action)
        if ($currentTargetTable -eq "n/a") {
            $currentSourceTable = ""
            $currentTargetTable = ""
        }
        continue
    }

    # Process column rows if we're in rows section and have a current table
    if ($inRowsSection -and $currentSourceTable -ne "" -and $cols.Count -ge 13) {
        $beforeCol = $cols[1].Trim()
        $afterCol = $cols[6].Trim()
        $action = $cols[12].Trim()
        
        # Skip if before column is empty or is a placeholder
        if ($beforeCol -eq "" -or $beforeCol -eq "--/--" -or $beforeCol.StartsWith("Rows:")) {
            # Check if this is a NEW column (afterCol exists but beforeCol doesn't)
            if ($afterCol -ne "" -and $afterCol -ne "--/--" -and $action -ne "") {
                # This is a new column - use "(new)" as source
                $outLines += "$currentSourceTable,(new column),$afterCol,$action,"
            }
            continue
        }

        # Skip Drop actions
        if ($action -eq "Drop") {
            continue
        }

        # Use SourceColumn as TargetColumn if TargetColumn is empty
        if ($afterCol -eq "" -or $afterCol -eq "--/--") {
            $afterCol = $beforeCol
        }

        # Output the converted line
        $outLines += "$currentSourceTable,$beforeCol,$afterCol,$action,"
    }
}

# Save output
$outputPath = Join-Path $OutputDir "PlanColumns.csv"
$outLines | Out-File $outputPath -Encoding UTF8
Write-Host "Converted $($outLines.Count - 1) column mappings" -ForegroundColor Green
Write-Host "Saved to: $outputPath" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1) Review the generated PlanColumns.csv" -ForegroundColor Gray
Write-Host "  2) Run MigrationRunner option M (Import CSV)" -ForegroundColor Gray
Write-Host "  3) Choose 'C' for Columns only" -ForegroundColor Gray
Write-Host "  4) Point it to: $outputPath" -ForegroundColor Gray
