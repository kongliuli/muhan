# Task 15 Evidence — Global hotkey system

## Plan checkbox
✅ `Tool/HotkeyManager.cs` (NEW) + `ViewModel/HotkeyViewModel.cs` (NEW): 全局快捷键系统

## Changes
- `HotkeyManager.cs` — singleton, RegisterHotKey/UnregisterHotKey P/Invoke, WM_HOTKEY hook
- `HotkeyViewModel.cs` — App.config persistence, Ctrl+Shift+Key / Win+Shift+Key only
- `UCSetDialog` — 快捷键 Expander with PreviewKeyDown recording
- `MainWindow` — SourceInitialized wiring + App.Exit cleanup
- Conflict detection for duplicate hotkey bindings

## Verification
- `dotnet build`: 0 errors, 0 warnings ✅

## Commit
`feat(hotkey): add customizable global hotkey system`