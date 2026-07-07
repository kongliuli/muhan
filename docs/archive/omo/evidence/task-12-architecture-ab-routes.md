# Task 12 Evidence — DI container

## Plan checkbox
✅ `App.xaml.cs` + `Tool/ServiceLocator.cs` (NEW): 依赖注入容器

## Files created
- `Tool/ServiceLocator.cs` — static `GetService<T>()` facade
- `ViewModel/HotkeyViewModel.cs` — Phase 4 stub

## Files modified
- `ModernBoxes.csproj` — +Microsoft.Extensions.DependencyInjection 8.0.1, +Microsoft.Extensions.Hosting 8.0.1
- `App.xaml.cs` — IHost builder, 20 singletons registered, `App.Services` exposed
- `ViewModel/DirInformationDialogViewModel.cs` — +parameterless ctor + `Load(string)`
- `ViewModel/FilePropertyDialogViewModel.cs` — +parameterless ctor + `Load(string)`
- 14 code-behind files — all `new ViewModel()` → `ServiceLocator.GetService<ViewModel>()`

## Registered as Singleton (20)
MainViewModel, UCusedApplicationViewModel, UCTempDirectoryViewModel, UctempFileViewModel, UCnotesViewModel, UcCompontentViewModel, UCSetDialogViewModel, BackupViewModel, SearchViewModel, AddMenuDialogViewModel, UCAddApplicationDialogViewModel, AddTempDirViewModel, AddTempFileDialogViewModel, UcAddCardAppDialogViewModel, DirInformationDialogViewModel, OneWordViewModel, HotkeyViewModel, UcManngerCardAppViewModel, FilePropertyDialogViewModel, DatabaseService

## Verification
- `dotnet build`: 0 errors, 0 warnings ✅

## Commit
`feat(di): integrate Microsoft.Extensions.DependencyInjection container`