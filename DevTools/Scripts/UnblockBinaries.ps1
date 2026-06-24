# UnblockBinaries.ps1
# Automatically unblock DLLs after build to prevent Windows Defender Application Control from blocking them

Write-Host "Unblocking binaries..." -ForegroundColor Yellow

# Unblock bin folder
$binPath = Join-Path $PSScriptRoot "bin"
if (Test-Path $binPath) {
    Get-ChildItem -Path $binPath -Recurse -Filter "*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
        Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
        Write-Host "  Unblocked: $($_.Name)" -ForegroundColor Green
    }
} else {
    Write-Host "  Bin path not found: $binPath" -ForegroundColor Red
}

# Unblock ASP.NET temporary files
$tempPath = "$env:LOCALAPPDATA\Temp\Temporary ASP.NET Files"
if (Test-Path $tempPath) {
    Get-ChildItem -Path $tempPath -Recurse -Filter "*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
        Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
    }
    Write-Host "  Unblocked ASP.NET temp files" -ForegroundColor Green
} else {
    Write-Host "  ASP.NET temp path not found (this is OK)" -ForegroundColor Gray
}

Write-Host "Unblocking complete!" -ForegroundColor Green
