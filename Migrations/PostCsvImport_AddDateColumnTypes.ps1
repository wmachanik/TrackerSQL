# Post-CSV-Import Fix: Add ColumnTypes for date conversion
# Run this AFTER Option 11 (CSV import) to ensure date columns are properly handled

$file = "Data\Metadata\PlanEdits\PlanConstraints.json"

if (-not (Test-Path $file)) {
    Write-Host "ERROR: PlanConstraints.json not found at $file" -ForegroundColor Red
    exit 1
}

Write-Host "Adding ColumnTypes metadata for date conversion..." -ForegroundColor Cyan

$json = Get-Content $file -Raw | ConvertFrom-Json

$modified = $false

# Fix ContactsItemsPredictedTbl
$table1 = $json.Tables | Where-Object { $_.Table -eq "ContactsItemsPredictedTbl" }
if ($table1) {
    if (-not $table1.ColumnTypes) {
        $table1 | Add-Member -NotePropertyName ColumnTypes -NotePropertyValue ([PSCustomObject]@{}) -Force
    }
    $table1.ColumnTypes | Add-Member -NotePropertyName "NextCoffeeBy" -NotePropertyValue "datetime" -Force
    $table1.ColumnTypes | Add-Member -NotePropertyName "NextCleanOn" -NotePropertyValue "datetime" -Force
    $table1.ColumnTypes | Add-Member -NotePropertyName "NextFilterEst" -NotePropertyValue "datetime" -Force
    $table1.ColumnTypes | Add-Member -NotePropertyName "NextDescaleEst" -NotePropertyValue "datetime" -Force
    $table1.ColumnTypes | Add-Member -NotePropertyName "NextServiceEst" -NotePropertyValue "datetime" -Force
    Write-Host "? Fixed ContactsItemsPredictedTbl" -ForegroundColor Green
    $modified = $true
}

# Fix RepairsTbl
$table2 = $json.Tables | Where-Object { $_.Table -eq "RepairsTbl" }
if ($table2) {
    if (-not $table2.ColumnTypes) {
        $table2 | Add-Member -NotePropertyName ColumnTypes -NotePropertyValue ([PSCustomObject]@{}) -Force
    }
    $table2.ColumnTypes | Add-Member -NotePropertyName "DateLogged" -NotePropertyValue "datetime" -Force
    $table2.ColumnTypes | Add-Member -NotePropertyName "LastStatusChange" -NotePropertyValue "datetime" -Force
    Write-Host "? Fixed RepairsTbl" -ForegroundColor Green
    $modified = $true
}

# Fix TempCoffeecheckupCustomerTbl
$table3 = $json.Tables | Where-Object { $_.Table -eq "TempCoffeecheckupCustomerTbl" }
if ($table3) {
    if (-not $table3.ColumnTypes) {
        $table3 | Add-Member -NotePropertyName ColumnTypes -NotePropertyValue ([PSCustomObject]@{}) -Force
    }
    $table3.ColumnTypes | Add-Member -NotePropertyName "NextPrepDate" -NotePropertyValue "datetime" -Force
    $table3.ColumnTypes | Add-Member -NotePropertyName "NextDeliveryDate" -NotePropertyValue "datetime" -Force
    $table3.ColumnTypes | Add-Member -NotePropertyName "NextCoffee" -NotePropertyValue "datetime" -Force
    $table3.ColumnTypes | Add-Member -NotePropertyName "NextClean" -NotePropertyValue "datetime" -Force
    $table3.ColumnTypes | Add-Member -NotePropertyName "NextFilter" -NotePropertyValue "datetime" -Force
    $table3.ColumnTypes | Add-Member -NotePropertyName "NextDescal" -NotePropertyValue "datetime" -Force
    $table3.ColumnTypes | Add-Member -NotePropertyName "NextService" -NotePropertyValue "datetime" -Force
    Write-Host "? Fixed TempCoffeecheckupCustomerTbl" -ForegroundColor Green
    $modified = $true
}

if ($modified) {
    $json | ConvertTo-Json -Depth 10 | Set-Content $file -Encoding UTF8
    Write-Host ""
    Write-Host "? ColumnTypes added successfully!" -ForegroundColor Green
    Write-Host "Now run: M ? N to apply data migration with date conversion" -ForegroundColor Cyan
} else {
    Write-Host "? No tables found to fix" -ForegroundColor Yellow
}
