# Migrate-AllTrackerDbFiles.ps1
# Batch migration tool for all TrackerDb files
# Created: 2025-05-14

param(
    [string]$RootPath = "C:\SRC\ASP.net\TrackerSQL",
    [switch]$Phase1Only,
    [switch]$Phase2Only,
    [switch]$Phase3Only,
    [switch]$Phase4Only,
    [switch]$DryRun,
    [string]$OutputLog = "DevTools\Documentation\Migration_Log.txt"
)

$ErrorActionPreference = "Stop"

Write-Host "TrackerDb Batch Migration Tool" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host ""

# Define file groups by phase
$phase1Files = @(
    "Controls\CustomersTbl.cs",
    "Controls\ItemTypeTbl.cs",
    "Controls\OrderDataControl.cs"
)

$phase2Files = @(
    "Controls\ReoccuringOrderDAL.cs",
    "Controls\PersonsTbl.cs",
    "Controls\ItemUsageTbl.cs",
    "Controls\ClientUsageTbl.cs",
    "Controls\OrderDetailDAL.cs"
)

$phase3Files = @(
    "Controls\TempOrdersDAL.cs",
    "Controls\CustomersAccInfoTbl.cs",
    "Controls\ClientUsageLinesTbl.cs",
    "Controls\EquipTypeTbl.cs",
    "Controls\TempCoffeeCheckup.cs",
    "Controls\NextPrepDateByAreaTbl.cs",
    "Controls\RepairsTbl.cs",
    "Controls\MachineConditionsTbl.cs",
    "Controls\SysDataTbl.cs",
    "Controls\LogTbl.cs",
    "Controls\InvoiceTypeTbl.cs",
    "Controls\OrderItemTbl.cs",
    "Controls\SectionTypesTbl.cs",
    "Controls\OrderCheckTbl.cs",
    "Controls\TransactionTypesTbl.cs",
    "Controls\TempOrdersLinesTbl.cs",
    "Controls\AreaPrepDaysTbl.cs",
    "Controls\PackagingTbl.cs",
    "Controls\PaymentTermsTbl.cs",
    "Controls\SentRemindersLogTbl.cs"
)

$phase4Files = @(
    "Controls\RepairStatusesTbl.cs",
    "Controls\TempOrdersHeaderTbl.cs",
    "Controls\CustomerTypeTbl.cs",
    "Controls\ItemGroupTbl.cs",
    "Controls\OrderCls.cs",
    "Controls\PrepTypesTbl.cs",
    "Controls\OrderDetailData.cs",
    "Controls\PriceLevelsTbl.cs",
    "Controls\ItemContactRequires.cs",
    "Controls\AreaTblDAL.cs",
    "Controls\HolidayClosureProvider.cs",
    "Controls\CustomerTrackedServiceItems.cs",
    "Controls\OrderCheck.cs",
    "Controls\ActiveDeliveryData.cs",
    "Controls\ContactsThatMayNeedNextWeek.cs",
    "Controls\ClientAwayPeriod.cs",
    "Pages\DeliverySheet.aspx.cs",
    "Pages\ViewMyOrder.aspx.cs",
    "Pages\NewOrderDetail.aspx.cs",
    "Pages\DeleteOrderLine.aspx.cs",
    "Classes\GeneralTrackerDbTools.cs",
    "Classes\TrackerTools.cs",
    "Classes\DateMatrixBuilder.cs",
    "Tools\XMLtoSQL.aspx.cs",
    "Managers\CoffeeCheckupManager.cs"
)

# Determine which files to process
$filesToProcess = @()

if ($Phase1Only) {
    $filesToProcess = $phase1Files
    $phaseName = "Phase 1"
} elseif ($Phase2Only) {
    $filesToProcess = $phase2Files
    $phaseName = "Phase 2"
} elseif ($Phase3Only) {
    $filesToProcess = $phase3Files
    $phaseName = "Phase 3"
} elseif ($Phase4Only) {
    $filesToProcess = $phase4Files
    $phaseName = "Phase 4"
} else {
    # All files
    $filesToProcess = $phase1Files + $phase2Files + $phase3Files + $phase4Files
    $phaseName = "All Phases"
}

Write-Host "Processing: $phaseName" -ForegroundColor Yellow
Write-Host "Total files: $($filesToProcess.Count)" -ForegroundColor Yellow
Write-Host "Dry Run: $DryRun" -ForegroundColor $(if ($DryRun) { "Magenta" } else { "Green" })
Write-Host ""

# Confirmation
if (-not $DryRun) {
    $confirmation = Read-Host "This will modify $($filesToProcess.Count) files. Continue? (yes/no)"
    if ($confirmation -ne "yes") {
        Write-Host "Migration cancelled." -ForegroundColor Yellow
        exit 0
    }
}

# Process each file
$results = @()
$successful = 0
$failed = 0

foreach ($relativeFile in $filesToProcess) {
    $fullPath = Join-Path $RootPath $relativeFile

    Write-Host ""
    Write-Host "Processing: $relativeFile" -ForegroundColor Cyan

    if (-not (Test-Path $fullPath)) {
        Write-Host "  File not found - SKIPPED" -ForegroundColor Yellow
        $results += [PSCustomObject]@{
            File = $relativeFile
            Status = "Not Found"
            Changes = 0
            Error = "File not found"
        }
        $failed++
        continue
    }

    try {
        # Call the single-file migration script
        $result = & "$RootPath\DevTools\Scripts\Migrate-TrackerDbFile.ps1" `
            -FilePath $fullPath `
            -DryRun:$DryRun `
            -CreateBackup:(-not $DryRun)

        $results += [PSCustomObject]@{
            File = $relativeFile
            Status = "Success"
            Changes = $result.ChangesCount
            Error = $null
        }
        $successful++

        Write-Host "  ? Completed - $($result.ChangesCount) changes" -ForegroundColor Green
    }
    catch {
        Write-Host "  ? Failed - $($_.Exception.Message)" -ForegroundColor Red
        $results += [PSCustomObject]@{
            File = $relativeFile
            Status = "Failed"
            Changes = 0
            Error = $_.Exception.Message
        }
        $failed++
    }
}

# Generate summary report
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MIGRATION SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Phase: $phaseName" -ForegroundColor Yellow
Write-Host "Total Files: $($filesToProcess.Count)" -ForegroundColor White
Write-Host "Successful: $successful" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Gray" })
Write-Host "Total Changes: $(($results | Measure-Object -Property Changes -Sum).Sum)" -ForegroundColor Yellow
Write-Host ""

if ($failed -gt 0) {
    Write-Host "Failed Files:" -ForegroundColor Red
    $results | Where-Object { $_.Status -eq "Failed" } | ForEach-Object {
        Write-Host "  - $($_.File): $($_.Error)" -ForegroundColor Red
    }
    Write-Host ""
}

# Save log
$logPath = Join-Path $RootPath $OutputLog
$logContent = @"
TrackerDb Migration Log
=======================
Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Phase: $phaseName
Dry Run: $DryRun

SUMMARY
-------
Total Files: $($filesToProcess.Count)
Successful: $successful
Failed: $failed
Total Changes: $(($results | Measure-Object -Property Changes -Sum).Sum)

DETAILED RESULTS
----------------
"@

foreach ($result in $results) {
    $logContent += "`n$($result.File)`n"
    $logContent += "  Status: $($result.Status)`n"
    $logContent += "  Changes: $($result.Changes)`n"
    if ($result.Error) {
        $logContent += "  Error: $($result.Error)`n"
    }
}

$logContent | Out-File -FilePath $logPath -Encoding UTF8

Write-Host "Log saved to: $logPath" -ForegroundColor Green
Write-Host ""

if (-not $DryRun) {
    Write-Host "NEXT STEPS:" -ForegroundColor Cyan
    Write-Host "1. Review all modified files for TODO comments" -ForegroundColor Yellow
    Write-Host "2. Update parameter handling (AddParams -> List<DBParameter>)" -ForegroundColor Yellow
    Write-Host "3. Update SQL queries (? -> @ParamName)" -ForegroundColor Yellow
    Write-Host "4. Add error handling and logging" -ForegroundColor Yellow
    Write-Host "5. Test each file thoroughly" -ForegroundColor Yellow
    Write-Host "6. Update Migration_Progress_Tracker.md" -ForegroundColor Yellow
} else {
    Write-Host "DRY RUN COMPLETE - No files were modified" -ForegroundColor Magenta
    Write-Host "Run without -DryRun to apply changes" -ForegroundColor Yellow
}

Write-Host ""

return $results
