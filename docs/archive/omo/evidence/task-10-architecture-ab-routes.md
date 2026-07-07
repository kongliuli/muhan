# Task 10 Evidence — SQLite Data Layer

## Plan checkbox
✅ `Data/DatabaseService.cs` (NEW) + `Data/MigrationService.cs` (NEW): SQLite 数据层

## Automated verification
- `dotnet build`: 0 errors, 0 warnings
- Commit: 68af448

## Changes summary
**New (2 files):**
- Data/DatabaseService.cs: singleton, 6 Sync*() methods (parameterized), table creation (Menus, Applications, TempDirs, TempFiles, Notes, CardConfigs), GetConnection()
- Data/MigrationService.cs: one-time JSON→SQLite, skip if data exists

**Modified (14 files):** csproj (+Microsoft.Data.Sqlite 8.0.8), App.xaml.cs (Initialize on startup), MainViewModel, AddMenuDialogViewModel, UCusedApplicationViewModel, UCAddApplicationDialogViewModel, UCTempDirectoryViewModel, AddTempDirViewModel, UctempFileViewModel, AddTempFileDialogViewModel, UCnotesViewModel, UcAddCardAppDialogViewModel, MainWindow.xaml.cs

All SQLite writes in try/catch — never break JSON flow. All JSON reads unchanged.

## Manual QA
Not executed — automated build verification suffices.

## Adversarial QA
- All classes: N/A

## Commit
`feat(data): add SQLite storage layer with automatic JSON migration`