# Todo 6 — NoteCardService extraction

## Verification
- **Build**: dotnet build -warnaserror — 0 errors, 0 warnings
- **LOC**: UCnotesViewModel.cs = 191 lines (target <220) ✅
- **New files**: INoteCardService.cs, NoteCardService.cs
- **Modified**: UCnotesViewModel.cs (298→191 lines)
- **Service separation**: Business logic (CRUD + JSON I/O + Persistence) extracted to service layer
- **ViewModel**: Only UI state (IsEditing, SelectedNote, EditTitle, EditContent) and messenger wiring remain

## Scope check
- [x] INoteCardService interface defined in Core/Interfaces/
- [x] NoteCardService implementation in Application/Notes/
- [x] UCnotesViewModel injects INoteCardService + IPersistenceProvider
- [x] NotesConfig.json schema unchanged
- [x] All existing note features preserved

## QA
- Manual: Create note → Edit → Save → Close → Reopen → Content retained
- Failure: Exception in service → ViewModel shows error via logger
