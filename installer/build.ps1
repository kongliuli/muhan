# Build & publish ModernBoxes for GitHub Release (Velopack + zip + portable)
param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$desktopProject = Join-Path $projectRoot "src\ModernBoxes.Desktop\ModernBoxes.Desktop.csproj"
$publishDir = Join-Path $projectRoot "publish"
$releasesDir = Join-Path $projectRoot "releases"
$icon = Join-Path $projectRoot "src\ModernBoxes.Desktop\Resource\logo.ico"
$zipPath = Join-Path $projectRoot "ModernBoxes-$Version-win-x64.zip"
$portableZip = Join-Path $projectRoot "ModernBoxes-$Version-portable-win-x64.zip"

Write-Host "Publishing ModernBoxes v$Version ($Configuration)..."

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
if (Test-Path $releasesDir) { Remove-Item $releasesDir -Recurse -Force }

dotnet publish $desktopProject `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -p:Version=$Version `
    -o $publishDir

if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

if (Get-Command vpk -ErrorAction SilentlyContinue) {
    Write-Host "Packing Velopack release..."
    vpk pack `
        --packId ModernBoxes `
        --packVersion $Version `
        --packDir $publishDir `
        --mainExe ModernBoxes.exe `
        --packTitle "木函" `
        --icon $icon `
        --outputDir $releasesDir
    if ($LASTEXITCODE -ne 0) { throw "vpk pack failed" }
}
else {
    Write-Warning "vpk not found; skip Velopack installer (dotnet tool install -g vpk)"
}

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath

$portableDir = Join-Path $projectRoot "publish-portable"
if (Test-Path $portableDir) { Remove-Item $portableDir -Recurse -Force }
Copy-Item $publishDir $portableDir -Recurse
Set-Content -Path (Join-Path $portableDir "portable.marker") -Value "" -Encoding ascii
if (Test-Path $portableZip) { Remove-Item $portableZip -Force }
Compress-Archive -Path (Join-Path $portableDir "*") -DestinationPath $portableZip
Remove-Item $portableDir -Recurse -Force

Write-Host "Publish output: $publishDir"
Write-Host "Release zip: $zipPath"
Write-Host "Portable zip: $portableZip"
if (Test-Path $releasesDir) { Write-Host "Velopack releases: $releasesDir" }
