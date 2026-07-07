# Task 14 Evidence — Win11 Mica/Acrylic material

## Plan checkbox
✅ `View/MainWindow.xaml` + `Resource/dictionary/MainWindowResource.xaml`: Win11 Mica/Acrylic 材质

## Changes
- P/Invoke `DwmSetWindowAttribute` with `DWMWA_MICA=38` for Win11 22000+
- Feature detection fallback to Acrylic on Win10
- `DWMWA_USE_IMMERSIVE_DARK_MODE` for dark theme
- ThemeChanged event for auto-switching
- Semi-transparent brush resources in Resource dictionaries
- `SourceInitialized` hook for window composition

## Verification
- `dotnet build`: 0 errors, 0 warnings ✅

## Commit
`feat(ux): enable Win11 Mica/Acrylic material backgrounds`