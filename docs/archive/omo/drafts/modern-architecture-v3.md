# modern-architecture-v3 — Execution Draft

## Status: Wave 3 executing

- **Wave 1** (Todos 1-5): ✅ Complete — Clean Architecture 4 layers, repositories, DI, persistence, ViewModelLocator
- **Wave 2** (Todos 6-10): ✅ Complete — 5 card services extracted, all ViewModels under LOC limit
- **Wave 3** (Todos 11-14): 🔄 In progress — Todo 11 dispatched (bg_fc7b298e, ICard interface)
- **Wave 4** (Todos 15-18): ⏳ Pending — Test suite + quality gates

## Build status
- `dotnet build -warnaserror`: 0 errors, 0 warnings throughout Waves 1-2

## Safety rules (from disaster lessons)
- R1: Never trust agent DoneClaim without independent verification
- R2: Verify files exist + build passes before proceeding
- R3: Compressed summaries are UNRELIABLE — verify facts independently
- R4: Baseline snapshot before structural changes

## Adopted defaults (user approved)
1. Stay WPF (no Avalonia) 2. Clean Architecture + Vertical Slices
3. ICard plugin system 4. Repository pattern 5. ViewModelLocator
6. xUnit TDD 7. Feature folders 8. IHost lifecycle
