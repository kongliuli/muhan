# Todo 10 — SearchService extraction

## Verification
- **Build**: dotnet build -warnaserror — 0 errors, 0 warnings
- **LOC**: SearchViewModel.cs = 135 lines (target <180) ✅
- **New files**: ISearchService.cs, SearchService.cs
- **Modified**: SearchViewModel.cs (222→135 lines)
- **Extracted**: SearchAsync logic (SQLite LIKE queries), result classification, debounce timer management

## Scope check
- [x] ISearchService in Core/Interfaces/
- [x] SearchService in Application/Search/
- [x] 300ms debounce timer preserved
- [x] SQLite parameterized queries preserved
- [x] SearchViewModel retains only UI binding properties + input handling

## QA
- Manual: Type "VSCode" → Results appear within 300ms → Click launches app
