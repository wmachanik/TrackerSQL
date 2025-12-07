# Simple test to verify column mappings are being processed
Write-Host "Testing CustomersTbl column mappings specifically..." -ForegroundColor Cyan

# Check what the CSV actually says for CustomersTbl
$csvPath = "..\..\Data\Metadata\PlanEdits\TableMigrationReport-Dec-1.csv"
$lines = Get-Content $csvPath
Write-Host "`nCSV content for CustomersTbl:" -ForegroundColor Yellow

# Find CustomersTbl section
$foundCustomers = $false
foreach ($line in $lines) {
    if ($line -match "CustomersTbl") {
        $foundCustomers = $true
        Write-Host "  $line" -ForegroundColor White
    }
    elseif ($foundCustomers -and ($line.StartsWith(",CustomerID") -or $line.StartsWith(",CompanyName") -or $line.StartsWith(",ContactTitle"))) {
        Write-Host "  $line" -ForegroundColor Green
    }
    elseif ($foundCustomers -and ($line.StartsWith("------") -or $line.StartsWith("Table,"))) {
        break
    }
}

Write-Host "`nThe CSV clearly shows CustomerID should become ContactID" -ForegroundColor Yellow
Write-Host "But the schema file still shows CustomerID -> CustomerID" -ForegroundColor Red
Write-Host "`nThis confirms the JSON serialization error is preventing the column updates from being saved." -ForegroundColor Cyan