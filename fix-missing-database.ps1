# Fix Missing TrackerData Database
# This creates the missing database and sets up the complete migration pipeline

Write-Host "=== FIXING MISSING TRACKERDATA DATABASE ===" -ForegroundColor Cyan
Write-Host "Creating TrackerData database and running full migration pipeline..." -ForegroundColor Yellow
Write-Host ""

try {
    # 1. Create TrackerData database
    Write-Host "1. Creating TrackerData database..." -ForegroundColor Yellow
    sqlcmd -S .\SQLEXPRESS -E -Q "IF DB_ID('TrackerData') IS NULL CREATE DATABASE TrackerData; PRINT 'TrackerData database created or already exists';"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? TrackerData database created successfully" -ForegroundColor Green
    } else {
        Write-Error "Failed to create TrackerData database"
        exit 1
    }
    
    # 2. Create AccessSrc schema
    Write-Host "2. Creating AccessSrc schema..." -ForegroundColor Yellow
    sqlcmd -S .\SQLEXPRESS -E -d TrackerData -Q "IF SCHEMA_ID('AccessSrc') IS NULL EXEC('CREATE SCHEMA AccessSrc'); PRINT 'AccessSrc schema created';"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? AccessSrc schema created successfully" -ForegroundColor Green
    } else {
        Write-Error "Failed to create AccessSrc schema"
        exit 1
    }
    
    # 3. Verify database and schema
    Write-Host "3. Verifying setup..." -ForegroundColor Yellow
    $result = sqlcmd -S .\SQLEXPRESS -E -d TrackerData -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.schemas WHERE name = 'AccessSrc'"
    
    if ($result.Trim() -eq "1") {
        Write-Host "? Database and schema setup verified" -ForegroundColor Green
    } else {
        Write-Error "Database setup verification failed"
        exit 1
    }
    
    Write-Host ""
    Write-Host "=== DATABASE SETUP COMPLETE ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "NEXT STEPS:" -ForegroundColor Yellow
    Write-Host "1. Go to Migrations\MigrationRunner directory:" -ForegroundColor White
    Write-Host "   cd Migrations\MigrationRunner" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Run the full migration pipeline:" -ForegroundColor White
    Write-Host "   dotnet run --configuration Release" -ForegroundColor Gray
    Write-Host "   Select option 'Z' (Full pipeline)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. When prompted for connection strings, use:" -ForegroundColor White
    Write-Host "   Access: (path to your Access .mdb file)" -ForegroundColor Gray
    Write-Host "   SQL: Server=.\SQLEXPRESS;Database=TrackerData;Integrated Security=True;" -ForegroundColor Gray
    Write-Host ""
    Write-Host "The full pipeline will run steps A?B?C?MS?M?N?! in sequence" -ForegroundColor Cyan
    Write-Host "This should resolve the data migration issue completely." -ForegroundColor Green
    
} catch {
    Write-Error "Error in database setup: $_"
    exit 1
}