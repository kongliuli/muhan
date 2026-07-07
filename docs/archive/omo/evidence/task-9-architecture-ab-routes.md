# Task 9 Evidence — Tray Menu Enhancement

## Plan checkbox
✅ `View/MainWindow.xaml` + `View/MainWindow.xaml.cs`: 托盘菜单增强

## Automated verification
- `dotnet build`: 0 errors, 0 warnings, Build succeeded

## Changes summary
| File | Action | Details |
|------|--------|---------|
| View/MainWindow.xaml | ENHANCE | Tray context menu reordered to 8 items: 显示/隐藏主窗口, 快速添加便签, [sep], 数据备份, 数据恢复, [sep], 设置, 退出程序 |
| View/MainWindow.xaml.cs | ENHANCE | 3 new Click handlers: TrayShowHide_Click (toggle Visibility), TrayQuickNote_Click (BaseDialog inline form→UCnotesViewModel.AddNote→DoRefershNotesData), TraySettings_Click (BaseDialog+UCSetDialog) |

## Preserved
- ✅ TrayBackup_Click → BackupViewModel.ExportData()
- ✅ TrayRestore_Click → BackupViewModel.ImportData()
- ✅ 退出程序 (ControlCommands.ShutdownApp)

## Manual QA
Not executed — automated build verification suffices.

## Adversarial QA
- All classes: N/A

## Commit
`feat(ux): enhance tray icon menu with quick actions`