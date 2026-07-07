# Todo 9 — DirectoryCardService extraction

## Verification
- **Build**: dotnet build -warnaserror — 0 errors, 0 warnings
- **LOC**: UCTempDirectoryViewModel.cs = <150 lines (target <150) ✅
- **New files**: IDirectoryCardService.cs, DirectoryCardService.cs
- **Modified**: UCTempDirectoryViewModel.cs (167→<150 lines)
- **Extracted**: AddDir, RemoveDir, OpenDir, ChangeImportance

## Scope check
- [x] IDirectoryCardService in Core/Interfaces/
- [x] DirectoryCardService in Application/Directories/
- [x] Importance color coding preserved (red/yellow/blue/green)
- [x] No WPF references in service layer

## QA
- Manual: Add folder → change importance color → restart → colors retained
