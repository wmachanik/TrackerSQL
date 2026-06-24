# Master script to create all missing POCOs and Repositories
# Created: 2026-05-15
# Purpose: One-command migration setup (0 AI calls)

param(
    [switch]$DryRun,
    [switch]$SkipAnalysis
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?  Master POCO & Repository Generator                       ?" -ForegroundColor Cyan
Write-Host "?  Zero AI Calls Required                                   ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Step 1: Analysis
if (-not $SkipAnalysis) {
    Write-Host "STEP 1: Analyzing Legacy Classes..." -ForegroundColor Yellow
    Write-Host "====================================" -ForegroundColor Yellow
    .\DevTools\Scripts\Analyze-Legacy-Classes.ps1
    Write-Host ""
}

# Step 2: Generate Missing POCOs
Write-Host "STEP 2: Generating Missing POCOs..." -ForegroundColor Yellow
Write-Host "====================================" -ForegroundColor Yellow
.\DevTools\Scripts\Batch-Generate-Pocos.ps1 -DryRun:$DryRun
Write-Host ""

# If POCOs were created, re-analyze
if (-not $DryRun) {
    $pocoCreated = $LASTEXITCODE -eq 0
    if ($pocoCreated) {
        Write-Host "Re-analyzing after POCO creation..." -ForegroundColor Gray
        .\DevTools\Scripts\Analyze-Legacy-Classes.ps1
        Write-Host ""
    }
}

# Step 3: Generate Missing Repositories
Write-Host "STEP 3: Generating Missing Repositories..." -ForegroundColor Yellow
Write-Host "===========================================" -ForegroundColor Yellow
.\DevTools\Scripts\Batch-Generate-Repositories.ps1 -DryRun:$DryRun
Write-Host ""

# Step 4: Summary
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?  Generation Complete!                                      ?" -ForegroundColor Green
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""

if ($DryRun) {
    Write-Host "This was a DRY RUN." -ForegroundColor Yellow
    Write-Host "Run without -DryRun to create files." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Command to execute:" -ForegroundColor Cyan
    Write-Host "  .\DevTools\Scripts\Master-Generate-All.ps1" -ForegroundColor White
} else {
    Write-Host "Next Steps (No AI Required!):" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1. FILL CRUD Methods (2-3 hours, 0 AI calls)" -ForegroundColor Cyan
    Write-Host "     • Open each Repository in Classes\Sql\" -ForegroundColor Gray
    Write-Host "     • Find matching legacy class in Controls\" -ForegroundColor Gray
    Write-Host "     • Copy INSERT SQL -> fill template" -ForegroundColor Gray
    Write-Host "     • Copy UPDATE SQL -> fill template" -ForegroundColor Gray
    Write-Host "     • DELETE already done in template" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  2. COPY Custom Methods (2-3 hours, 5-10 AI calls)" -ForegroundColor Cyan
    Write-Host "     • See CustomMethodNames in CSV" -ForegroundColor Gray
    Write-Host "     • Simple methods: copy-paste pattern (0 AI)" -ForegroundColor Gray
    Write-Host "     • Complex methods: use AI for help (1-2 calls each)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  3. UPDATE Callers (1-2 hours, 0 AI calls)" -ForegroundColor Cyan
    Write-Host "     • Find: new CustomersTbl()" -ForegroundColor Gray
    Write-Host "     • Replace: new ContactsRepository()" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  4. TEST (1 hour)" -ForegroundColor Cyan
    Write-Host "     • Build: dotnet build" -ForegroundColor Gray
    Write-Host "     • Test: dotnet test" -ForegroundColor Gray
    Write-Host "     • Smoke test key workflows" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Reference Docs:" -ForegroundColor Yellow
    Write-Host "  • DevTools\Documentation\Minimal_AI_Repository_Migration_Plan.md"
    Write-Host "  • DevTools\QUICK_START_REPOSITORY_MIGRATION.md"
    Write-Host "  • DevTools\Documentation\Naming_Convention_Reference.md"
}

Write-Host ""
