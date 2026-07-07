# Todo 3 — IPersistenceProvider dual-write

## Verification
- **Build**: dotnet build -warnaserror → 0 errors, 0 warnings ✅
- **New files**: Core/Interfaces/IPersistenceProvider.cs, Infrastructure/Data/PersistenceProvider.cs

## Changes
- IPersistenceProvider interface: SaveAsync<T> + LoadAsync<T>
- DualWriteProvider: entity-to-(JSON path, repo sync method) mapping dictionary
- ~10 ViewModels updated: JSON+SQLite dual-write → single _persistence.SaveAsync() call
- Registered as Singleton in App.xaml.cs
