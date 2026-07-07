# Build & publish ModernBoxes for GitHub Release
param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$desktopProject = Join-Path $projectRoot "src\ModernBoxes.Desktop\ModernBoxes.Desktop.csproj"
$publishDir = Join-Path $projectRoot "publish"
$zipPath = Join-Path $projectRoot "ModernBoxes-$Version-win-x64.zip"

Write-Host "Publishing ModernBoxes v$Version ($Configuration)..."

dotnet publish $desktopProject `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath

Write-Host "Publish output: $publishDir"
Write-Host "Release zip: $zipPath"

# Optional MSIX when Windows SDK is available
$makeAppx = Get-Command "MakeAppx.exe" -ErrorAction SilentlyContinue
if ($makeAppx) {
    $outputMsix = Join-Path $projectRoot "ModernBoxes_${Version}_x64.msix"
    & $makeAppx pack /d $publishDir /p $outputMsix /f (Join-Path $PSScriptRoot "AppxManifest.xml")
    if ($LASTEXITCODE -eq 0) { Write-Host "MSIX: $outputMsix" }
}
