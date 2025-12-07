@echo off
echo Testing MigrationRunner Option X (Drop All Tables)
echo.
echo This test will:
echo 1. Build the MigrationRunner
echo 2. Show the menu options to verify Option X exists
echo 3. Exit immediately without executing anything dangerous
echo.

cd /d "%~dp0"
cd "Migrations\MigrationRunner"

echo Building MigrationRunner...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Build successful! 
echo.
echo To test the Drop All Tables functionality:
echo 1. Run: dotnet run
echo 2. Look for Option X in the menu
echo 3. DO NOT actually select it unless you want to drop all tables!
echo.
echo The menu should show:
echo   X^) Drop ALL tables in database ^(DANGER: Deletes all data!^)
echo.
echo Press any key to continue...
pause > nul

echo.
echo Starting MigrationRunner to show menu...
echo ^(It will automatically exit after showing options^)
echo.

REM Using echo to simulate input that exits immediately
echo 0 | dotnet run