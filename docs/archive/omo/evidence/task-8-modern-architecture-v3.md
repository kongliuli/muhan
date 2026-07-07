# Todo 8 — FileCardService extraction

## Verification
- **Build**: dotnet build -warnaserror — 0 errors, 0 warnings
- **LOC**: UctempFileViewModel.cs = 176 lines (target <180) ✅
- **New files**: IFileCardService.cs, FileCardService.cs
- **Modified**: UctempFileViewModel.cs (211→176 lines)
- **Extracted**: AddFile, RemoveFile, DeleteToRecycleBin, OpenFile, OpenFileLocation

## Scope check
- [x] IFileCardService in Core/Interfaces/
- [x] FileCardService in Application/Files/
- [x] Delete-to-recycle-bin behavior preserved
- [x] No WPF references in service layer

## QA
- Manual: Add file → icon shown → right-click delete → recycle bin confirmed
