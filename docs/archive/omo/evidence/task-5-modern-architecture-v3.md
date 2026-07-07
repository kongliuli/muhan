# Todo 5 — ViewModelLocator auto-binding

## Verification
- **Build**: dotnet build -warnaserror → 0 errors, 0 warnings ✅
- **New files**: Presentation/ViewModels/ViewModelLocator.cs (18 properties)
- **Modified**: App.xaml (xmlns:vm + Locator resource), 15+ XAML files (DataContext bindings), 15+ code-behind files (ServiceLocator.GetService removed)

## Fix round
- HotkeyViewModel namespace correction (ModernBoxes.Infrastructure → ModernBoxes.Infrastructure.Hotkey)
- 4 files updated with using ModernBoxes.Infrastructure.Hotkey
