# Build script for ModernBoxes MSIX package
param([string]$Configuration = "Release", [string]$Version = "1.0.0")

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Building ModernBoxes v$Version in $Configuration mode..."

# 1. Publish project
$publishDir = "$projectRoot\publish"
dotnet publish "$projectRoot\ModernBoxes.csproj" -c $Configuration -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

# 2. Check for MakeAppx
$makeAppx = Get-Command "MakeAppx.exe" -ErrorAction SilentlyContinue
if (-not $makeAppx) {
    Write-Host "WARNING: MakeAppx.exe not found. Skipping MSIX packaging."
    Write-Host "Install Windows SDK to enable packaging."
    Write-Host "Publish output is at: $publishDir"
    exit 0
}

# 3. Create MSIX
$outputMsix = "$projectRoot\ModernBoxes_${Version}_x64.msix"
& $makeAppx pack /d $publishDir /p $outputMsix /f "$PSScriptRoot\AppxManifest.xml"
if ($LASTEXITCODE -ne 0) { throw "MakeAppx failed" }

Write-Host "MSIX package created: $outputMsix"

# 4. Sign (if cert available)
$cert = Get-ChildItem "$PSScriptRoot\*.pfx" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($cert) {
    $signTool = Get-Command "SignTool.exe" -ErrorAction SilentlyContinue
    if ($signTool) {
        & $signTool sign /fd SHA256 /f $cert.FullName $outputMsix
        Write-Host "Signed with: $($cert.Name)"
    }
}
