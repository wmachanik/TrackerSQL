@echo off
echo ========================================
echo FIX MIGRATION AFTER DROP ALL TABLES
echo ========================================
echo.
echo After dropping all tables, you need to:
echo 1. Recreate the target schema (CREATE TABLE scripts)
echo 2. Regenerate and apply data migration
echo 3. Apply foreign key constraints
echo.
echo This script will guide you through the process.
echo.
echo IMPORTANT: Make sure your MigrationConfig.json has the correct connection strings!
echo.
pause

cd /d "%~dp0"
cd "..\MigrationRunner"

echo.
echo Step 1: Building MigrationRunner...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Please fix build errors first.
    pause
    exit /b 1
)

echo.
echo ========================================
echo STEP 1: Generate CREATE TABLE Script
echo ========================================
echo.
echo This will create the schema definition (tables without data)
echo.
echo Running Option A (Generate CREATE TABLE DDL)...
echo A | dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo Option A failed! Check the output above for errors.
    pause
    exit /b 1
)

echo.
echo ========================================
echo STEP 2: Apply CREATE TABLE Script
echo ========================================
echo.
echo This will create all the target tables in your SQL Server database
echo.
echo Running Option C (Apply DDL scripts)...
echo C | dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo Option C failed! Check connection strings and database permissions.
    pause
    exit /b 1
)

echo.
echo ========================================
echo STEP 3: Generate Data Migration Script
echo ========================================
echo.
echo This will create the script to copy data from AccessSrc to target tables
echo.
echo Running Option M (Generate Data Migration Script)...
echo M | dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo Option M failed! Check the output above for errors.
    pause
    exit /b 1
)

echo.
echo ========================================
echo STEP 4: Apply Data Migration
echo ========================================
echo.
echo This will copy all the data from AccessSrc tables to target tables
echo.
echo Running Option N (Apply Data Migration)...
echo N | dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo Option N had issues - this is common after dropping tables.
    echo Check the logs for specific errors.
    echo.
    echo Common issues after drop:
echo    - Connection timeouts (retry)
echo    - Missing foreign key references (run Option B next)
echo    - Data type conversion issues (check error logs)
    echo.
)

echo.
echo ========================================
echo STEP 5: Apply Foreign Key Constraints
echo ========================================
echo.
echo This will add foreign key relationships between tables
echo.
echo Running Option B (Generate FK constraints)...
echo B | dotnet run

echo.
echo Running Option C again to apply FK constraints...
echo C | dotnet run

echo.
echo ========================================
echo STEP 6: Custom Normalization (if needed)
echo ========================================
echo.
echo This handles special tables like Orders and Recurring Orders
echo.
echo Running Option ! (Custom normalize)...
echo ! | dotnet run

echo.
echo ========================================
echo MIGRATION RESTART COMPLETED
echo ========================================
echo.
echo Check the output above for any errors.
echo.
echo If successful, your tables should now be recreated with:
echo - Proper schema (ignoring tables marked as Ignore=true)
echo - All data migrated from Access
echo - Foreign key constraints applied
echo - Normalized tables properly handled
echo.
echo Next steps:
echo 1. Test your application
echo 2. Verify data integrity
echo 3. Check that ignored tables are NOT present
echo.
pause