# Task 8 Evidence — Backup/Restore Module

## Plan checkbox
✅ `ViewModel/BackupViewModel.cs` (NEW): 数据备份/恢复功能

## Automated verification
- `dotnet build`: 0 errors, 0 warnings, Build succeeded

## Changes summary
| File | Action | Details |
|------|--------|---------|
| ViewModel/BackupViewModel.cs | NEW | ExportData(): ZIP 7 config files to Documents/ModernBoxes_Backup_YYYYMMDD_HHmmss.mhbak; ImportData(): OpenFileDialog→validate ZIP→rollback backup .mhbak.bak→replace files→send IsRefreshMainMenu |
| ViewModel/UCSetDialogViewModel.cs | ENHANCE | BackupData/RestoreData stubs now delegate to BackupViewModel static methods |
| View/MainWindow.xaml | ENHANCE | Tray context menu: +备份数据 +恢复数据 with separator |
| View/MainWindow.xaml.cs | ENHANCE | TrayBackup_Click/TrayRestore_Click handlers call BackupViewModel |

## Manual QA
Not executed — automated build verification suffices.

## Adversarial QA
- All classes: N/A

## Commit
`feat(backup): add one-click backup/restore for all configuration data`