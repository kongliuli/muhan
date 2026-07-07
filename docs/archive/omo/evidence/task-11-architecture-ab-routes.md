# Task 11 Evidence — Global search

## Plan checkbox
✅ `ViewModel/SearchViewModel.cs` (NEW) + `View/SelfControl/UCSearch.xaml` (NEW): 全局搜索

## Files created
- `Model/SearchResultModel.cs` — ResultType enum + SearchResultModel class
- `ViewModel/SearchViewModel.cs` — parameterized SQLite LIKE across 4 tables, 300ms DispatcherTimer debounce, relevance sorting, action dispatch
- `View/SelfControl/UCSearch.xaml` — search TextBox + results ListBox with type badges
- `View/SelfControl/UCSearch.xaml.cs` — DataContext wiring, Enter key handler

## Files modified
- `View/MainWindow.xaml` — added RowDefinitions for search bar at Row 0

## Verification
- `dotnet build`: 0 errors, 0 warnings ✅

## Commit
`feat(search): add global search across all managed items`