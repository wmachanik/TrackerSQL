# Builds the driver app into this site, then you can publish TrackerSQL.
#   .\Build-DriverApp.ps1           web copy (/driver/) and the Android APK
#   .\Build-DriverApp.ps1 -SiteOnly web copy only
param([switch]$SiteOnly)

$ErrorActionPreference = 'Stop'
$driver = Resolve-Path (Join-Path $PSScriptRoot '..\TrackerDriver')
Push-Location $driver
try {
    npm run build:site
    if (-not $SiteOnly) {
        npm run apk
    }
}
finally {
    Pop-Location
}
