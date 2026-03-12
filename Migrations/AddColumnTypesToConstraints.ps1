# Add ColumnTypes to PlanConstraints.json for tables with date conversion issues

$file = "Data\Metadata\PlanEdits\PlanConstraints.json"
$content = Get-Content $file -Raw

Write-Host "Adding ColumnTypes metadata to fix date conversion issues..." -ForegroundColor Cyan

# Fix RepairsTbl
$pattern1 = '("Table": "RepairsTbl"[^}]+?"NotNullColumns": \[\][^}]+?"ColumnTypes": )\{\}'
$replacement1 = @'
$1{
        "RepairID": "INT",
        "ContactID": "INT",
        "ContactName": "NVARCHAR(255)",
        "ContactEmail": "NVARCHAR(255)",
        "JobCardNumber": "NVARCHAR(50)",
        "DateLogged": "datetime",
        "LastStatusChange": "datetime",
        "EquipTypeID": "INT",
        "EquipSerialNumber": "NVARCHAR(100)",
        "SwopOutMachineID": "INT",
        "EquipConditionID": "INT",
        "TakenFrother": "BIT",
        "TakenBeanLid": "BIT",
        "TakenWaterLid": "BIT",
        "BrokenFrother": "BIT",
        "BrokenBeanLid": "BIT",
        "BrokenWaterLid": "BIT",
        "RepairFaultID": "INT",
        "RepairFaultDesc": "NVARCHAR(500)",
        "RepairStatusID": "INT",
        "RelatedOrderID": "INT",
        "Notes": "NVARCHAR(MAX)"
      }
'@

if ($content -match $pattern1) {
    $content = $content -replace $pattern1, $replacement1
    Write-Host "? Added ColumnTypes for RepairsTbl" -ForegroundColor Green
} else {
    Write-Host "? RepairsTbl pattern not found" -ForegroundColor Yellow
}

# Fix TempCoffeecheckupCustomerTbl
$pattern2 = '("Table": "TempCoffeecheckupCustomerTbl"[^}]+?"NotNullColumns": \[\][^}]+?"ColumnTypes": )\{\}'
$replacement2 = @'
$1{
        "TCCID": "INT",
        "ContactID": "INT",
        "CompanyName": "NVARCHAR(255)",
        "ContactFirstName": "NVARCHAR(100)",
        "ContactAltFirstName": "NVARCHAR(100)",
        "AreaID": "INT",
        "EmailAddress": "NVARCHAR(255)",
        "AltEmailAddress": "NVARCHAR(255)",
        "ContactTypeID": "INT",
        "EquipTypeID": "INT",
        "TypicallySecToo": "BIT",
        "PreferedAgentID": "INT",
        "SalesAgentID": "INT",
        "UsesFilter": "BIT",
        "Enabled": "BIT",
        "AlwaysSendChkUp": "BIT",
        "ReminderCount": "INT",
        "NextPrepDate": "datetime",
        "NextDeliveryDate": "datetime",
        "NextCoffee": "datetime",
        "NextClean": "datetime",
        "NextFilter": "datetime",
        "NextDescal": "datetime",
        "NextService": "datetime",
        "RequiresPurchOrder": "BIT"
      }
'@

if ($content -match $pattern2) {
    $content = $content -replace $pattern2, $replacement2
    Write-Host "? Added ColumnTypes for TempCoffeecheckupCustomerTbl" -ForegroundColor Green
} else {
    Write-Host "? TempCoffeecheckupCustomerTbl pattern not found" -ForegroundColor Yellow
}

# Save
Set-Content $file -Value $content -NoNewline

Write-Host ""
Write-Host "? ColumnTypes metadata added!" -ForegroundColor Green
Write-Host "Now run: M (regenerate), then N (apply)" -ForegroundColor Cyan
