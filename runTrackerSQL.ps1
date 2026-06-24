$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
$iis = "C:\Program Files\IIS Express\iisexpress.exe"

$projectPath = "C:\SRC\ASP.net\TrackerSQL"
$solution = "$projectPath\TrackerSQL.sln"

# Kill IIS Express if already running
Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process

Write-Host "Building..."
& $msbuild $solution /p:Configuration=Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed"
    exit
}

Write-Host "Starting IIS Express..."
& $iis /path:$projectPath /port:5000

Start-Sleep -Seconds 2
Start-Process "http://localhost:5000"

Write-Host "App running at http://localhost:5000"
