$source = "G:\My Drive\Hockey\WPGMHA.old"
$destination = "C:\Backup\WPGMHA_extracted\WPGMHA.Old"

foreach ($file in $realMissing) {
    $sourcePath = $source + $file
    $destPath = $destination + $file

    $destFolder = Split-Path $destPath
    if (!(Test-Path $destFolder)) {
        New-Item -ItemType Directory -Path $destFolder -Force | Out-Null
    }

    Copy-Item -Path $sourcePath -Destination $destPath -Force

    Write-Host "Copied:" $file
}
