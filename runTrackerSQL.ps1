$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
$iis = "C:\Program Files\IIS Express\iisexpress.exe"

$projectPath = "C:\SRC\ASP.net\TrackerSQL"
$project = "$projectPath\TrackerSQL.csproj"
$siteUrl = "http://localhost:5000"

Write-Host "Building..."

Get-ChildItem "$projectPath\bin" -Filter "TrackerSQL (*).*" -ErrorAction SilentlyContinue |
    Remove-Item -Force

Get-ChildItem "$projectPath\obj\Debug" -Filter "TrackerSQL (*).*" -ErrorAction SilentlyContinue |
    Remove-Item -Force

& $msbuild $project /t:Build /m /p:Configuration=Debug /v:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed (exit $LASTEXITCODE). Fix errors above, then run this script again."
    exit 1
}

$wasRunning = $null -ne (Get-Process iisexpress -ErrorAction SilentlyContinue)
if ($wasRunning) {
    Write-Host "Stopping IIS Express so the site loads the new TrackerSQL.dll..."
    Get-Process iisexpress | Stop-Process -Force
    Start-Sleep -Seconds 1
}

Write-Host "Starting IIS Express at $siteUrl ..."
Start-Process -FilePath $iis -ArgumentList @("/path:`"$projectPath`"", "/port:5000")
Start-Sleep -Seconds 2

if ($wasRunning) {
    Write-Host "Build successful — refresh the browser (Ctrl+F5) to pick up changes."
} else {
    Write-Host "Build successful — opening site."
    Start-Process $siteUrl
}
