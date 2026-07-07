# Task 17 Evidence — MSIX packaging + auto-update

## Plan checkbox
✅ `ModernBoxes.csproj` + `installer/` (NEW): 打包与自动更新

## Changes
- `installer/AppxManifest.xml` — MSIX manifest
- `installer/build.ps1` — dotnet publish → MakeAppx → SignTool (graceful SDK-missing fallback)
- `Tool/UpdateChecker.cs` — GitHub Releases API, version compare, MessageBox dialog
- csproj version 1.0.0.0

## Verification
- `dotnet build`: 0 errors, 0 warnings ✅

## Commit
`feat(package): add MSIX packaging and auto-update support`