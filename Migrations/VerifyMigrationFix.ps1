# Quick verification script to check if all fixes are in place

Write-Host "=== Checking Schema Files ===" -ForegroundColor Cyan
$item = Get-Content "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema\ItemUsageTbl.schema.json" | ConvertFrom-Json
$action1 = ($item.Plan.ColumnActions | Where-Object { $_.Source -eq "ClientUsageLineNo" }).Action
Write-Host "ItemUsageTbl.ClientUsageLineNo Action: $action1" -ForegroundColor $(if($action1 -eq "Drop"){"Green"}else{"Red"})

$client = Get-Content "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema\ClientUsageLinesTbl.schema.json" | ConvertFrom-Json
$action2 = ($client.Plan.ColumnActions | Where-Object { $_.Source -eq "ClientUsageLineNo" }).Action
Write-Host "ClientUsageLinesTbl.ClientUsageLineNo Action: $action2" -ForegroundColor $(if($action2 -eq "Drop"){"Green"}else{"Red"})

Write-Host "`n=== Checking Code Fix ===" -ForegroundColor Cyan
$codeContent = Get-Content "C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner\DmlScriptGenerator.cs" -Raw
if ($codeContent -like "*CRITICAL FIX: Use 'cols' (filtered list) instead of 'tm.Columns'*") {
    Write-Host "? Code fix is present in DmlScriptGenerator.cs" -ForegroundColor Green
} else {
    Write-Host "? Code fix is MISSING from DmlScriptGenerator.cs" -ForegroundColor Red
}

Write-Host "`n=== Checking Build Status ===" -ForegroundColor Cyan
$exe = Get-Item "C:\SRC\ASP.net\TrackerSQL\Migrations\MigrationRunner\bin\Debug\net48\MigrationRunner.exe" -ErrorAction SilentlyContinue
if ($exe) {
    Write-Host "MigrationRunner.exe last built: $($exe.LastWriteTime)" -ForegroundColor Yellow
    $age = (Get-Date) - $exe.LastWriteTime
    if ($age.TotalMinutes -lt 5) {
        Write-Host "? Build is recent" -ForegroundColor Green
    } else {
        Write-Host "? Build is OLD - REBUILD NEEDED!" -ForegroundColor Red
    }
}

Write-Host "`n=== Checking Generated SQL ===" -ForegroundColor Cyan
$sqlContent = Get-Content "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" -Raw
if ($sqlContent -match "ContactsItemUsageTbl.*?IdentityInsert=ON") {
    Write-Host "? SQL STILL has 'IdentityInsert=ON' for ContactsItemUsageTbl - STALE!" -ForegroundColor Red
    Write-Host "   You MUST REBUILD and run Option M!" -ForegroundColor Red
} else {
    Write-Host "? SQL looks correct (no IdentityInsert=ON for ContactsItemUsageTbl)" -ForegroundColor Green
}

Write-Host "`n=== ACTION REQUIRED ===" -ForegroundColor Yellow
Write-Host "1. Close MigrationRunner if running"
Write-Host "2. Rebuild the project in Visual Studio"
Write-Host "3. Run MigrationRunner again"
Write-Host "4. Type M (regenerate migration script)"
Write-Host "5. Look for 'SKIPPING dropped column' messages"
Write-Host "6. Type N (apply migration)"
