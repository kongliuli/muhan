# Todo 7 — ApplicationCardService extraction

## Verification
- **Build**: dotnet build -warnaserror — 0 errors, 0 warnings
- **LOC**: UCusedApplicationViewModel.cs = <140 lines (target <140) ✅
- **New files**: IApplicationCardService.cs, ApplicationCardService.cs
- **Modified**: UCusedApplicationViewModel.cs (161→<140 lines)
- **Extracted**: AddApp, RemoveApp, LaunchApp, GetIcon, duplicate detection

## Scope check
- [x] IApplicationCardService in Core/Interfaces/
- [x] ApplicationCardService in Application/Applications/
- [x] No WPF references in service layer
- [x] Process.Start behavior unchanged

## QA
- Manual: Drag exe → Launch → Icon extracted → Click opens app
