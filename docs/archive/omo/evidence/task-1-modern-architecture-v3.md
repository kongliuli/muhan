# Todo 1 — Directory structure restructure

## Verification
- **Build**: dotnet build -warnaserror → 0 errors, 0 warnings ✅
- **File count**: 85 .cs files preserved (same as pre-restructure)
- **Old dirs cleaned**: Model/, MyEnum/, ViewModel/, View/, Tool/, Data/ all empty/deleted

## Directory structure achieved
```
Core/          → Models/ (10 files), Enums/ (5 files), Interfaces/ (empty)
Application/   → Notes/, Applications/, Directories/, Files/, OneWord/, Search/ (all empty for Wave 2)
Presentation/  → ViewModels/ (19), Views/ (with subdirs), Dialogs/, Converters/, Validation/, Resources/
Infrastructure/ → Data/, Config/, Logging/, Hotkey/, Update/, Platform/, HostedServices/ (empty), Plugins/ (empty)
Infrastructure root → ServiceLocator, Messages, MessengerCompat, MessengerMessage, RelayCommand, ChangeTheme
```

## Key fixes during execution
- Chinese character encoding corruption (UTF-8 vs UTF-16 LE) fixed across 17 XAML + multiple .cs files
- MainWindow moved from View/ to project root
- BaseDialog + AddMenuDialog moved to Presentation/Dialogs/
- All namespaces updated (10 namespace mappings)
- All XAML clr-namespace references updated
- All using directives updated

## Scope check
- [x] No business logic changed
- [x] Resource/image/ + Resource/svg/ preserved in place
- [x] All JSON config files at root preserved
- [x] Build passes independently verified
