# Todo 4 — IHostedService startup lifecycle

## Verification
- **Build**: dotnet build -warnaserror → 0 errors, 0 warnings ✅
- **Files**: 3 IHostedService classes in Infrastructure/HostedServices/

## Services created
- TrayHostedService: Tray icon initialization → moved from MainWindow.initConfig()
- HotkeyHostedService: Global hotkey registration → moved from MainWindow.SourceInitialized
- AutoUpdateService: Version check → moved from App.xaml.cs fire-and-forget

## App.xaml.cs
- 3 AddHostedService registrations
- StopAsync on application exit
