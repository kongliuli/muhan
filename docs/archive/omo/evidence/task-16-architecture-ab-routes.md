# Task 16 Evidence — Micro-animations and transition polish

## Plan checkbox
✅ `Resource/dictionary/*.xaml` + `**/*.xaml`: 动画与微交互打磨

## Changes
- **Menu hover**: MainWindowResource.xaml — replaced instant IsMouseOver trigger with MouseEnter/MouseLeave EventTriggers + ColorAnimation (200ms, CubicEase)
- **Search focus**: UCSearch.xaml/cs — glow border with Opacity animation on GotFocus/LostFocus (200ms)
- **Card expand/collapse**: UcCompotent.xaml — wrapped cards in hc:TransitioningContentControl TransitionMode="Bottom2TopWithFade" (300ms)
- **Note delete**: UCnotes.xaml.cs — fade-out animation via ContextMenu.PlacementTarget before messenger delete (200ms)
- **Empty state SVG**: UCnotes, UCusedApplications, UCtempDirectory, UcTempFile — TranslateTransform float/bounce (400ms, AutoReverse, RepeatBehavior=Forever, CubicEase InOut)

## Verification
- `dotnet build`: 0 errors, 0 warnings ✅

## Commit
`feat(ux): add micro-animations and transition polish across all cards`