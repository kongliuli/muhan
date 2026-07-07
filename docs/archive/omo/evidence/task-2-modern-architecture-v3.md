# Todo 2 — Repository layer split

## Verification
- **Build**: dotnet build -warnaserror → 0 errors, 0 warnings ✅
- **Files**: 12 repo files (6 interfaces + 6 implementations) in Infrastructure/Data/Repositories/
- **DatabaseService.cs**: Under 50 lines (connection factory + CreateTables)
- **Caller updates**: 8 ViewModels updated (MainViewModel, UCusedApplicationVM, UCTempDirectoryVM, UctempFileVM, UCnotesVM, UcAddCardAppDialogVM, AddTempDirVM, AddTempFileDialogVM, AddMenuDialogVM)

## Repositories created
- IMenuRepository / MenuRepository
- IApplicationRepository / ApplicationRepository
- ITempDirRepository / TempDirRepository
- ITempFileRepository / TempFileRepository
- INoteRepository / NoteRepository
- ICardConfigRepository / CardConfigRepository

## Fix round
After initial dispatch, 10 build errors found — ViewModels still calling DatabaseService.Instance.Sync*(). Fix task resolved all 10 by adding repository ServiceLocator injection in parameterless constructors.
