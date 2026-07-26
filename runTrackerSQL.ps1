$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
$iis = "C:\Program Files\IIS Express\iisexpress.exe"

$projectPath = "C:\SRC\ASP.net\TrackerSQL"
$project = "$projectPath\TrackerSQL.csproj"

Write-Host "Building..."

Get-ChildItem "$projectPath\bin" -Filter "TrackerSQL (*).*" -ErrorAction SilentlyContinue |
    Remove-Item -Force

Get-ChildItem "$projectPath\obj\Debug" -Filter "TrackerSQL (*).*" -ErrorAction SilentlyContinue |
    Remove-Item -Force

& $msbuild $project /t:Build /m /p:Configuration=Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed"
    exit 1
}

if (-not (Get-Process iisexpress -ErrorAction SilentlyContinue)) {
    Write-Host "Starting IIS Express..."

    Start-Job {
        & "C:\Program Files\IIS Express\iisexpress.exe" `
          /path:"C:\SRC\ASP.net\TrackerSQL" `
          /port:5000
    } | Out-Null

    Start-Sleep 3
    Start-Process "http://localhost:5000"
}

Write-Host "Build successful"