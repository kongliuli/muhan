# Task 4 Evidence — NLog Logging System

## Plan checkbox
✅ `Tool/Logger.cs` (NEW): 引入 NLog 日志系统

## Automated verification
- `dotnet build`: 0 errors, 0 warnings ✅
- NLog.config exists in `bin/Debug/net10.0-windows/` ✅
- Zero `Trace.WriteLine` calls remain in source ✅

## Changes summary
| File | Action |
|------|--------|
| Tool/Logger.cs | **NEW** — static facade with Info/Warn/Error(2)/Debug, async dispatch via Task.Run |
| NLog.config | **NEW** — daily rotation, 7-day retention, to %APPDATA%/ModernBoxes/logs/ |
| ModernBoxes.csproj | Added NLog 5.3.4 NuGet + NLog.config as Content PreserveNewest |
| 11 files | Replaced 15 `Trace.WriteLine` calls with `Logger.Error(ex, "context")` |

## Manual QA
Not executed — build + grep verification suffices.

## Adversarial QA
- All classes: N/A

## Commit
`feat(core): integrate NLog for file-based application logging`