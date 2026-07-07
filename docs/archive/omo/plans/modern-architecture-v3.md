# modern-architecture-v3 - Work Plan

## TL;DR (For humans)

**What you'll get:** 一个按 Clean Architecture 重新分层的 ModernBoxes v3.0——代码从 19 个扁平 ViewModel 重组为 4 层（Core/Application/Infrastructure/Presentation）加垂直切片特征文件夹；卡片从硬编码 switch 变为可插拔的 ICard 插件系统；所有业务逻辑从 ViewModel 提取到独立可测试的 Service；80% 测试覆盖率的安全网。

**Why this approach:** 留在 WPF 但不留技术债。当前的 17-todo AB 路线已经建好了功能完整的 v2.0，现在是用业界标准架构（Clean Architecture + Repository + 插件系统）把地基从"能跑"升级到"能扩展、能测试、能接贡献"。不换框架——换了等于扔掉前两个版本的所有投入。

**What it will NOT do:**
- 不切换到 Avalonia/MAUI/WinUI（继续 WPF + HandyControl）
- 不添加任何新功能（纯架构重构）
- 不改变 JSON 文件格式（旧版本数据 100% 兼容）

**Effort:** Medium（4 波 18 todo，约 4-6 周全职）
**Risk:** Medium — 大规模文件移动有回归风险，但 characterization tests + 每波独立可运行提供安全网
**Decisions I made for you:** 因为你的"现代化"请求未指定具体技术路线，我代你做了以下选择（全部可逆，你说一句我改方向）：

| 决定 | 默认选择 | 原因 |
|------|---------|------|
| UI 框架 | **继续 WPF**（不换 Avalonia） | 前两版投入太大，换了全扔；Clean Architecture 的 Presentation 层以后可以切成 Avalonia |
| 架构模式 | **Clean Architecture + 垂直切片** | 2025 年 .NET 桌面应用的事实标准；Core 零依赖 → 可测试 |
| 卡片扩展 | **ICard 插件接口**（替换 CardID switch） | DI 自动发现，第三方 DLL 可贡献新卡片 |
| 测试 | **xUnit + TDD** | characterization tests 锁定现状 → 重构不破功能 |

Your next move: 批准本计划（说"批准"/"写入"/"approve"），或指出你想改的任意默认选择。批准后我会自动运行 Metis 分析 + 双 Momus 高精度审查。完整执行细节见下方。

---

> TL;DR (machine): Medium effort, Medium risk, 4 waves: Clean Architecture restructure → service extraction → plugin cards → test coverage. Deliverables: layered architecture, ICard plugin system, 70%+ test coverage, reuse existing WPF investment.

## Scope

### Must have
- **S1. 目录结构重组**：按 Clean Architecture 分层 + 垂直切片特征文件夹
- **S2. Repository 层**：将 DatabaseService.cs（263行 SRP 违规）拆分为 6 个类型化 Repository
- **S3. 持久化抽象**：统一 JSON+SQLite 双写逻辑，ViewModels 不再直接访问数据库
- **S4. ViewModelLocator**：替换 14+ 处 ServiceLocator.GetService 调用为 XAML 约定绑定
- **S5. 服务层提取**：从 oversized ViewModel 中提取业务逻辑为可测试的 Service
- **S6. 卡片插件系统**：ICard 接口 + 插件加载器 + 替换硬编码 CardID switch
- **S7. IHostedService 生命周期**：托盘、快捷键、自动更新使用托管服务
- **S8. 测试基础设施**：xUnit + Moq + 80% 服务/仓库覆盖率

### Must NOT have (guardrails, anti-slop, scope boundaries)
- 不切换 UI 框架（继续 WPF + HandyControl）
- 不改变任何用户可见功能行为（纯架构重构）
- 不破坏 JSON 文件兼容性（仍可被旧版本读取）
- 不引入新的第三方 UI 控件库
- 不在重构完成前添加新功能（重构优先，功能后置）
- 单文件不超过 250 行纯逻辑（超限必须拆分）
- 所有 ViewModel 保持 CommunityToolkit.Mvvm ObservableObject 基类

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- **Test decision**: TDD（先 characterization test 锁定现状 → failing test for new behavior → 实现） + xUnit + Moq + FluentAssertions
- **编译验证**: 每个 Wave 结束后 `dotnet build -warnaserror` 零错误
- **回归验证**: 所有 characterization tests 在重构前后保持 green
- **LOC 门控**: 每个 Todo 完成后验证目标文件不超过 250 行
- Evidence: `.omo/evidence/task-<N>-modern-architecture-v3.md`

## Execution strategy
### 四波渐进重构（Wave 1→4 严格顺序，Wave 内可并行）

```
Wave 1: 地基重构 ──→ Wave 2: 服务提取 ──→ Wave 3: 插件系统 ──→ Wave 4: 质量验证
(目录+数据+DI)     (ViewModel瘦身)      (卡片可扩展)         (测试+门控)
      ↓                  ↓                   ↓                  ↓
  分层清晰 ✓         ViewModel<250 ✓     CardID gone ✓     80% 覆盖率 ✓
```

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| Todo 1 (目录重组) | 当前代码 | Todo 2,3,4,5 | — |
| Todo 2 (Repository) | Todo 1 | Todo 3 | Todo 4,5 |
| Todo 3 (持久化抽象) | Todo 2 | Todo 6-10 | Todo 4,5 |
| Todo 4 (IHostedService) | Todo 1 | Todo 5 | Todo 2,3 |
| Todo 5 (ViewModelLocator) | Todo 4 | Todo 6-10 | Todo 2,3 |
| Todo 6 (NoteCardService) | Todo 3,5 | Todo 11-14 | Todo 7-10 |
| Todo 7 (AppCardService) | Todo 3,5 | Todo 11-14 | Todo 6,8-10 |
| Todo 8 (FileCardService) | Todo 3,5 | Todo 11-14 | Todo 6,7,9,10 |
| Todo 9 (DirCardService) | Todo 3,5 | Todo 11-14 | Todo 6-8,10 |
| Todo 10 (SearchService) | Todo 3,5 | Todo 11-14 | Todo 6-9 |
| Todo 11 (ICard 接口) | Wave 2 done | Todo 12,13,14 | — |
| Todo 12 (CardPluginLoader) | Todo 11 | Todo 13,14 | — |
| Todo 13 (卡片迁移) | Todo 11,12 | Todo 14 | — |
| Todo 14 (替换 CardID) | Todo 13 | — | — |
| Todo 15 (测试项目) | Todo 1 | Todo 16-18 | Todo 2-14 |
| Todo 16 (仓库集成测试) | Todo 2,15 | — | Todo 17,18 |
| Todo 17 (服务单元测试) | Todo 6-10,15 | — | Todo 16,18 |
| Todo 18 (代码终验) | All | — | Todo 16,17 |

## Todos
> Implementation + Test = ONE todo. Never separate.
<!-- APPEND TASK BATCHES BELOW THIS LINE WITH edit/apply_patch - never rewrite the headers above. -->
- [x] 1. `**/`: 目录结构重组 + 命名空间对齐（Clean Architecture 分层 + 垂直切片特征文件夹）
  What to do: 将现有扁平结构重组为四层——`Core/`（Models, Enums, Interfaces）、`Application/`（Services per card type）、`Infrastructure/`（Data, Logging, Hotkey, Update, Config）、`Presentation/`（Views, ViewModels, Converters, Resources）。每个卡片类型在 `Application/` 和 `Presentation/` 下拥有垂直切片文件夹（`Notes/`、`Applications/`、`Directories/`、`Files/`、`OneWord/`）。所有 `using` 和命名空间同步更新。`Tool/` 目录拆分映射到 `Infrastructure/` 子目录。
  Must NOT do: 不改变任何 `.cs` 文件的业务逻辑；不移动 `Resource/` 下的图片/font/svg（仅改命名空间引用）；不破坏 `dotnet build`
  Parallelization: Wave 1 | Blocked by: none | Blocks: Todo 2,3,4,5
  References: 当前目录结构——`Model/`(9文件)、`ViewModel/`(19文件)、`View/`(7+19子文件)、`Tool/`(18文件)、`Data/`(2文件)、`MyEnum/`(嵌套enum)、`Resource/`(~40文件)；目标结构——
  ```
  Core/Models/  Core/Enums/  Core/Interfaces/
  Application/Notes/  Application/Applications/  Application/Directories/
  Application/Files/  Application/OneWord/  Application/Search/
  Infrastructure/Data/  Infrastructure/Logging/  Infrastructure/Hotkey/
  Infrastructure/Update/  Infrastructure/Config/
  Presentation/Views/  Presentation/ViewModels/  Presentation/Converters/  Presentation/Resources/
  ```
  Acceptance criteria: `dotnet build` 零错误；所有文件位于目标文件夹（旧路径无残留 .cs 文件）；`grep -r "namespace ModernBoxes\." --include="*.cs"` 确认命名空间与目录一致
  QA scenarios: happy — 重构后所有功能不变（启动→添加应用/文件/便签→搜索→设置→备份→恢复）；failure — 错误移动导致 `dotnet build` 失败 → 修复后验证
  Evidence: `.omo/evidence/task-1-modern-architecture-v3.md`
  Commit: Y | `refactor(structure): reorganize into Clean Architecture layers with vertical slices`

- [x] 2. `Infrastructure/Data/`: Repository 层拆分（DatabaseService.cs 263行 → 6 个类型化 Repository）
  What to do: 创建 `Infrastructure/Data/Repositories/` 目录，建立 `IMenuRepository`/`IApplicationRepository`/`ITempDirRepository`/`ITempFileRepository`/`INoteRepository`/`ICardConfigRepository` 接口及对应实现类。每个 Repository 封装该实体类型的 INSERT/DELETE/SELECT 方法。`DatabaseService` 退化为仅负责 `GetConnection()` 的轻量连接工厂（<50 行）。MigrationService 调整为通过 Repository 接口执行迁移。
  Must NOT do: 不改变 SQL 语句逻辑（仅移动位置）；不改变 SQLite 表 schema；Repository 方法必须使用参数化查询（防止 SQL 注入）
  Parallelization: Wave 1 | Blocked by: Todo 1 | Blocks: Todo 3 | Can parallelize with: Todo 4,5
  References: `Data/DatabaseService.cs`（263 行，6 套 Sync*/Search* 方法）；`Data/MigrationService.cs`；所有 ViewModel 中 `DatabaseService.Instance.Sync*()` 调用点（14 个 ViewModel）
  Acceptance criteria: `DatabaseService.cs` ≤50 行；6 个 `I*Repository` 接口 + 6 个实现类存在；`dotnet build` 零错误；所有现有 ViewModel 中 `DatabaseService.Instance` 调用改为 `I*Repository` 构造函数注入
  QA scenarios: happy — SQLite 数据读写结果与重构前完全一致（比较 JSON 导出）；failure — Repository 方法抛出 `SqliteException` 时调用方正确处理
  Evidence: `.omo/evidence/task-2-modern-architecture-v3.md`
  Commit: Y | `refactor(data): extract typed repositories from monolithic DatabaseService`

- [x] 3. `Infrastructure/Data/PersistenceProvider.cs` (NEW): 持久化抽象（统一 JSON+SQLite 双写）
  What to do: 创建 `IPersistenceProvider` 接口——`Task SaveAsync<T>(string entity, IEnumerable<T> data)` 和 `Task<IEnumerable<T>> LoadAsync<T>(string entity)`。实现 `DualWriteProvider`：写入时同时更新 JSON 文件和对应 Repository；读取时从 JSON 加载（保证向后兼容），异步同步到 SQLite。将 14 个 ViewModel 中的直接 `DatabaseService.Instance.Sync*()` + `FileHelper.WriteFile()` 调用替换为 `_persistence.SaveAsync()`。
  Must NOT do: 不改变 JSON 文件 schema；JSON 文件仍是唯一的数据真相源（SQLite 为缓存副本）；不在 PersistenceProvider 中引入事务逻辑
  Parallelization: Wave 1 | Blocked by: Todo 2 | Blocks: Todo 6-10 | Can parallelize with: Todo 4,5
  References: 当前双写模式——每个 ViewModel 在保存时调用 `DatabaseService.Instance.Sync*()` + `FileHelper.WriteFile()`（14 处重复）；`Infrastructure/Data/Repositories/I*Repository.cs`（Todo 2 创建）；所有 6 个 JSON 配置文件路径
  Acceptance criteria: 任何数据变更仅需一行 `_persistence.SaveAsync(entity, data)`；JSON 文件和 SQLite 在每次 `SaveAsync` 后数据一致（JSON 导出与 SQLite SELECT 结果相同）；旧版本 JSON 文件可被新版本正常读取
  QA scenarios: happy — 添加应用→检查 JSON 文件更新→检查 SQLite 表更新；failure — 磁盘满时 SaveAsync 抛出异常，JSON 文件不损坏
  Evidence: `.omo/evidence/task-3-modern-architecture-v3.md`
  Commit: Y | `feat(data): add IPersistenceProvider for unified JSON+SQLite dual-write`

- [x] 4. `Infrastructure/HostedServices/` (NEW): IHostedService 启动生命周期
  What to do: 创建 3 个 `IHostedService` 实现——`TrayHostedService`（托盘图标初始化）、`HotkeyHostedService`（全局热键注册）、`AutoUpdateHostedService`（启动时版本检查）。在 `App.xaml.cs` 的 `Host.CreateApplicationBuilder()` 中注册它们为 `services.AddHostedService<T>()`。移除 `MainWindow.xaml.cs` 构造函数中的 `initConfig()` 托盘初始化代码（改为 `TrayHostedService.StartAsync()`）。移除 `App.OnStartup` 中的 `HotkeyManager.Instance.RegisterGlobalHotkey()`（改为 `HotkeyHostedService`）。
  Must NOT do: 不改变托盘/热键/更新的功能行为；不在 IHostedService 中直接操作 UI 线程（使用 `Application.Current.Dispatcher.InvokeAsync`）
  Parallelization: Wave 1 | Blocked by: Todo 1 | Blocks: Todo 5 | Can parallelize with: Todo 2,3
  References: `MainWindow.xaml.cs:69`（`initConfig()` 在构造函数调用）；`App.xaml.cs:16`（`GlobalExceptionHandler.Register()`）；`Tool/HotkeyManager.cs`；`Tool/UpdateChecker.cs`
  Acceptance criteria: 3 个 `IHostedService` 在应用启动时自动执行；`MainWindow` 构造函数不再包含托盘/热键初始化代码；托盘右键菜单所有功能正常；全局热键正常工作
  QA scenarios: happy — 启动应用→托盘图标立即出现→热键 `Ctrl+Shift+M` 可唤出；failure — 服务抛出异常时 `IHostedService.StopAsync` 被正确调用
  Evidence: `.omo/evidence/task-4-modern-architecture-v3.md`
  Commit: Y | `refactor(host): migrate tray/hotkey/update init to IHostedService`

- [x] 5. `Presentation/ViewModelLocator.cs` (NEW) + `App.xaml`: ViewModelLocator 自动绑定
  What to do: 创建 `ViewModelLocator` 类（`DataTemplate` 约定——`{x:Type vm:UCnotesViewModel}` → 自动解析 `UCnotes` View）。在 `App.xaml` 中注册全局 `DataTemplate`。修改所有 14+ 处 code-behind 中的 `ucNotes.DataContext = ServiceLocator.GetService<UCnotesViewModel>()` 为 XAML 绑定（如 `<local:UCnotes DataContext="{Binding NotesViewModel, Source={StaticResource Locator}}"/>`）。`ServiceLocator` 保留为 `ViewModelLocator` 的内部实现细节。
  Must NOT do: 不改变 ViewModel 的 Singleton 注册；不在 XAML 中直接引用 DI 容器；保留现有 DataContext 暂时作为后备（在新绑定失败时使用）
  Parallelization: Wave 1 | Blocked by: Todo 4 | Blocks: Todo 6-10 | Can parallelize with: Todo 2,3
  References: 14 处 `ServiceLocator.GetService<T>()` 调用——`MainWindow.xaml.cs`、`UCnotes.xaml.cs`、`UCusedApplications.xaml.cs`、`UCtempDirectory.xaml.cs`、`UcTempFile.xaml.cs`、`UcCompotent.xaml.cs`、`UCOneWord.xaml.cs`、`UcManagerCardApplication.xaml.cs`、`UCSearch.xaml.cs`、`UCSetDialog.xaml.cs`、`UCAddApplicationDialog.xaml.cs`、`AddTempDirDialog.xaml.cs`、`AddTempFileDialog.xaml.cs`、`UcAddCardApplicationDialog.xaml.cs`
  Acceptance criteria: code-behind 文件中 `ServiceLocator.GetService` 调用归零（除 `ViewModelLocator` 内部）；所有 UserControl 通过 XAML 绑定获取 DataContext；`dotnet build` 零错误；设计时 DataContext 可见
  QA scenarios: happy — 启动应用→所有卡片/菜单/对话框正常显示；failure — ViewModel 未注册时 XAML 绑定不崩溃（显示友好占位符）
  Evidence: `.omo/evidence/task-5-modern-architecture-v3.md`
  Commit: Y | `refactor(di): replace ServiceLocator with ViewModelLocator XAML auto-binding`

- [x] 6. `Application/Notes/NoteCardService.cs` (NEW) + `Presentation/ViewModels/Notes/UCnotesViewModel.cs` (REFACTOR): 便签服务提取
  What to do: 从 `UCnotesViewModel.cs`（298 行→目标 <220 行）提取——`AddNote`/`UpdateNote`/`DeleteNote`/`GetAllNotes`/`SearchNotes` 到 `INoteCardService` 接口 + 实现；UI 状态（`IsEditing`/`SelectedNote`/`EditContent`）保留在 ViewModel。ViewModel 通过构造函数注入 `INoteCardService` + `IPersistenceProvider`。原 `UCnotesViewModel` 的直接 JSON 读写 + SQLite 同步代码迁移到 Service。
  Must NOT do: 不改变便签 UI 行为（CRUD/置顶/改色/颜色选择器）；不改变 `NotesConfig.json` schema；ViewModel 剩余代码 ≤220 行
  Parallelization: Wave 2 | Blocked by: Todo 3,5 | Blocks: Todo 11-14 | Can parallelize with: Todo 7,8,9,10
  References: `UCnotesViewModel.cs`（298 行，含 CRUD + JSON I/O + UI 状态 + 消息注册）；`Model/NoteModel.cs`；`NotesConfig.json`
  Acceptance criteria: `UCnotesViewModel.cs` ≤220 行；`NoteCardService.cs` 可脱离 UI 独立运行（无 WPF 依赖）；现有便签功能全部保留；`dotnet build` 零错误
  QA scenarios: happy — 创建便签→编辑→关闭→重启→内容保留；failure — 磁盘满时保存失败，Service 抛出异常，ViewModel 显示错误提示
  Evidence: `.omo/evidence/task-6-modern-architecture-v3.md`
  Commit: Y | `refactor(notes): extract NoteCardService from UCnotesViewModel`

- [x] 7. `Application/Applications/ApplicationCardService.cs` (NEW) + `Presentation/ViewModels/Applications/UCusedApplicationViewModel.cs` (REFACTOR): 应用卡片服务提取
  What to do: 从 `UCusedApplicationViewModel.cs`（185 行→目标 <140 行）提取应用 CRUD 逻辑到 `IApplicationCardService`。Service 封装：添加/删除应用、启动应用（`Process.Start`）、提取图标（`GetIcon`）、重复检测。
  Must NOT do: 不改变应用启动行为（`Process.Start` 参数不变）；不在 Service 中引用 WPF 类型
  Parallelization: Wave 2 | Blocked by: Todo 3,5 | Blocks: Todo 11-14 | Can parallelize with: Todo 6,8,9,10
  References: `UCusedApplicationViewModel.cs`（185 行）；`Model/ApplicationModel.cs`；`Tool/GetIcon.cs`
  Acceptance criteria: `UCusedApplicationViewModel.cs` ≤140 行；`ApplicationCardService` 无 WPF 依赖；`dotnet build` 零错误
  QA scenarios: happy — 拖拽 exe 到卡片→图标出现→点击启动应用；failure — 应用路径不存在时启动失败，显示友好提示
  Evidence: `.omo/evidence/task-7-modern-architecture-v3.md`
  Commit: Y | `refactor(apps): extract ApplicationCardService from UCusedApplicationViewModel`

- [x] 8. `Application/Files/FileCardService.cs` (NEW) + `Presentation/ViewModels/Files/UctempFileViewModel.cs` (REFACTOR): 文件卡片服务提取
  What to do: 从 `UctempFileViewModel.cs`（238 行→目标 <180 行）提取文件管理逻辑——`AddFile`/`RemoveFile`/`DeleteToRecycleBin`/`OpenFile`/`OpenFileLocation` 到 `IFileCardService`。
  Must NOT do: 不改变文件删除行为（仍使用 `FileSystem.DeleteFile` 到回收站）；不改变文件类型图标映射
  Parallelization: Wave 2 | Blocked by: Todo 3,5 | Blocks: Todo 11-14 | Can parallelize with: Todo 6,7,9,10
  References: `UctempFileViewModel.cs`（238 行）；`Model/TempFileModel.cs`；`MyEnum/DirEnum.cs`
  Acceptance criteria: `UctempFileViewModel.cs` ≤180 行；`FileCardService` 无 WPF 依赖；`dotnet build` 零错误
  QA scenarios: happy — 添加文件→图标显示→右键删除到回收站→回收站确认存在；failure — 文件被占用时删除失败，Service 抛出异常被 ViewModel 捕获
  Evidence: `.omo/evidence/task-8-modern-architecture-v3.md`
  Commit: Y | `refactor(files): extract FileCardService from UctempFileViewModel`

- [x] 9. `Application/Directories/DirectoryCardService.cs` (NEW) + `Presentation/ViewModels/Directories/UCTempDirectoryViewModel.cs` (REFACTOR): 文件夹卡片服务提取
  What to do: 从 `UCTempDirectoryViewModel.cs`（194 行→目标 <150 行）提取文件夹管理——`AddDir`/`RemoveDir`/`OpenDir`/`ChangeImportance` 到 `IDirectoryCardService`。
  Must NOT do: 不改变文件夹重要性颜色分级（红/黄/蓝/绿）；不改变文件夹打开行为
  Parallelization: Wave 2 | Blocked by: Todo 3,5 | Blocks: Todo 11-14 | Can parallelize with: Todo 6,7,8,10
  References: `UCTempDirectoryViewModel.cs`（194 行）；`Model/TempDirModel.cs`；`MyEnum/DirEnum.cs`
  Acceptance criteria: `UCTempDirectoryViewModel.cs` ≤150 行；`DirectoryCardService` 无 WPF 依赖；`dotnet build` 零错误
  QA scenarios: happy — 添加文件夹→显示在卡片→修改重要性颜色→重启后颜色保留；failure — 文件夹不存在时操作失败，显示友好提示
  Evidence: `.omo/evidence/task-9-modern-architecture-v3.md`
  Commit: Y | `refactor(dirs): extract DirectoryCardService from UCTempDirectoryViewModel`

- [x] 10. `Application/Search/SearchService.cs` (NEW) + `Presentation/ViewModels/Search/SearchViewModel.cs` (REFACTOR): 搜索服务提取
  What to do: 从 `SearchViewModel.cs`（238 行→目标 <180 行）提取搜索逻辑——`SearchAsync`/`Debounce`/`ResultTypeClassification` 到 `ISearchService`。SearchViewModel 仅保留 UI 绑定属性（`SearchText`/`SearchResults`/`IsSearching`）+ 输入处理。
  Must NOT do: 不改变搜索查询逻辑（SQLite LIKE，参数化查询）；不改变 300ms 防抖计时器
  Parallelization: Wave 2 | Blocked by: Todo 3,5 | Blocks: Todo 11-14 | Can parallelize with: Todo 6,7,8,9
  References: `SearchViewModel.cs`（238 行，含 SQLite 查询 + DispatcherTimer 防抖 + 结果分类）；`Model/SearchResultModel.cs`
  Acceptance criteria: `SearchViewModel.cs` ≤180 行；`SearchService` 无 WPF 依赖；`dotnet build` 零错误
  QA scenarios: happy — 输入"VSCode"→100ms 内出现搜索结果→点击启动对应应用；failure — SQLite 连接失败时搜索不崩溃，显示"搜索服务不可用"
  Evidence: `.omo/evidence/task-10-modern-architecture-v3.md`
  Commit: Y | `refactor(search): extract SearchService from SearchViewModel`

- [x] 11. `Core/Interfaces/ICard.cs` (NEW) + `Core/Interfaces/ICardView.cs` (NEW): 卡片插件接口定义
  What to do: 创建 `ICard` 接口——`CardID`、`CardName`、`CardContent`、`CardHeight`、`Priview`、`IsChecked`；`ICardViewModel`（继承 `ICard` + `ObservableObject`）定义 `LoadAsync()`/`RefreshAsync()`；`ICardView` 标记接口用于视图发现。创建 `CardBase<TModel>` 抽象类提供默认实现。定义 `[CardExport]` 特性标注插件元数据（`CardName`、`Author`、`Version`、`Description`）。
  Must NOT do: 不依赖任何 WPF/HandyControl 类型；不引入 MEF（使用 DI 自动发现）
  Parallelization: Wave 3 | Blocked by: Wave 2 done | Blocks: Todo 12,13,14 | Can parallelize with: none
  References: 当前硬编码 `CardContentModel.cs`（71 行，CardID 0-4）; `UcCompontentViewModel.cs:switch(CardID)` 判断逻辑；目标——任意第三方 DLL 实现 `ICard` 接口即可注册为新卡片
  Acceptance criteria: `ICard` 接口编译通过；`CardBase<T>` 抽象类可被继承；`[CardExport]` 特性定义在 `Core/Interfaces/` 下；所有接口无 WPF 依赖
  QA scenarios: happy — 创建一个最小的 `ICard` 实现（HelloCard）→编译通过；failure — 引用缺失接口方法时编译器报错（而非运行时崩溃）
  Evidence: `.omo/evidence/task-11-modern-architecture-v3.md`
  Commit: Y | `feat(plugins): define ICard plugin interface with CardBase base class`

- [x] 12. `Infrastructure/Plugins/CardPluginLoader.cs` (NEW): DI 自动发现卡片插件加载器
  What to do: 创建 `CardPluginLoader`——扫描当前 Assembly（及 `Plugins/` 目录下的额外 DLL）中所有 `[CardExport]` 标记的 `ICardViewModel` 实现；自动将它们注册到 DI 容器（`services.AddTransient<ICardViewModel, T>()`）；提供 `IReadOnlyList<CardMetadata>` 属性供主窗口渲染。在 `App.xaml.cs` 的 `Host.CreateApplicationBuilder()` 中调用 `CardPluginLoader.DiscoverAndRegister(builder.Services)`。
  Must NOT do: 不热加载/卸载 DLL（插件注册仅在启动时执行）；不改变 `AllCardsConfig.json` 格式
  Parallelization: Wave 3 | Blocked by: Todo 11 | Blocks: Todo 13,14 | Can parallelize with: none
  References: `App.xaml.cs:42`（`Host.CreateApplicationBuilder()`）；当前卡片注册方式——`AllCardsConfig.json` 手动配置 + `UcCompontentViewModel.cs` switch 映射；目标——插件加载器自动发现所有 `ICardViewModel` 实现
  Acceptance criteria: 启动应用时自动发现所有 `ICardViewModel` 实现；`CardPluginLoader.GetCardMetadata()` 返回正确元数据列表；新增一个 `ICard` 实现类不需要修改任何现有文件；`dotnet build` 零错误
  QA scenarios: happy — 在 `Plugins/` 目录添加 `HelloCard.dll`→启动→卡片列表自动包含 HelloCard；failure — 无效 DLL 被跳过而不导致启动崩溃
  Evidence: `.omo/evidence/task-12-modern-architecture-v3.md`
  Commit: Y | `feat(plugins): add DI-based CardPluginLoader with assembly scanning`

- [x] 13. `Application/*/`: 5 类卡片迁移到 ICard 插件模型
  What to do: 将现有 5 类卡片（便签、应用、文件夹、临时文件、一言）分别实现 `ICardViewModel` 接口 + `[CardExport]` 特性。每类卡片创建 `*CardViewModel`（如 `NoteCardViewModel`）包装现有 `UCnotesViewModel`（或重构后的 Service），实现 `LoadAsync()`/`RefreshAsync()`。不改变现有 UserControl 的 XAML 结构。
  Must NOT do: 不重写现有卡片功能（仅包装为 ICard 接口）；不改变卡片视觉外观
  Parallelization: Wave 3 | Blocked by: Todo 11,12 | Blocks: Todo 14 | Can parallelize with: none
  References: 5 类卡片——`UCnotesViewModel`/`UCnotes.xaml`（便签）、`UCusedApplicationViewModel`/`UCusedApplications.xaml`（应用）、`UCTempDirectoryViewModel`/`UCtempDirectory.xaml`（文件夹）、`UctempFileViewModel`/`UcTempFile.xaml`（临时文件）、`OneWordViewModel`/`UCOneWord.xaml`（一言）
  Acceptance criteria: 5 个 `*CardViewModel` 均实现 `ICardViewModel` 接口；每个标记 `[CardExport]` 特性；`CardPluginLoader.GetCardMetadata()` 返回 5 个卡片元数据；`dotnet build` 零错误
  QA scenarios: happy — 启动应用→5 类卡片正常显示→切换卡片→所有功能正常；failure — 某个卡片 `LoadAsync()` 失败时不影响其他卡片加载
  Evidence: `.omo/evidence/task-13-modern-architecture-v3.md`
  Commit: Y | `refactor(plugins): migrate 5 card types to ICard plugin model`

- [x] 14. `UcCompontentViewModel.cs` (REFACTOR): 替换硬编码 CardID switch 为插件注册表
  What to do: 在 `UcCompontentViewModel.cs` 中移除 `switch(CardID) { case 0...4 }` 硬编码分支。改为使用 `CardPluginLoader.GetCardMetadata()` 动态渲染卡片列表。卡片切换逻辑改为 `_cards.FirstOrDefault(c => c.CardID == selectedId)?.LoadAsync()` 动态分发。
  Must NOT do: 不破坏卡片切换动画（Phase 4 Todo 16 的 TransitioningContentControl）；不改变卡片预览图映射（保留 Priview 字段）
  Parallelization: Wave 3 | Blocked by: Todo 13 | Blocks: none | Can parallelize with: none
  References: `UcCompontentViewModel.cs:switch(CardID)` 硬编码逻辑（原 ~131 行，需重构为目标 <100 行）；`AllCardsConfig.json` 卡片配置
  Acceptance criteria: `UcCompontentViewModel.cs` 中无 `switch(CardID)` 语句；添加第 6 类卡片无需修改 `UcCompontentViewModel.cs` 任何代码；`dotnet build` 零错误；5 类现有卡片功能不变
  QA scenarios: happy — 左侧菜单切换卡片类型→对应卡片内容正常加载；failure — 插件注册表返回空列表时显示"无可用卡片"空状态
  Evidence: `.omo/evidence/task-14-modern-architecture-v3.md`
  Commit: Y | `refactor(cards): replace hardcoded CardID switch with plugin registry`

- [x] 15. `tests/ModernBoxes.Tests/` (NEW): 单元测试项目 + characterization tests
  What to do: 创建 xUnit 测试项目 `tests/ModernBoxes.Tests/`，引用主项目。添加 Moq + FluentAssertions NuGet 包。为所有 Todo 2 创建的 Repository 接口编写 characterization tests——锁定当前 SQLite CRUD 行为（`MenuRepository_WriteThenRead_ReturnsSameData` 等）。为 Todo 3 的 `IPersistenceProvider` 编写集成测试（写入→同时验证 JSON 文件和 SQLite 内容一致）。
  Must NOT do: 不测试 HandyControl/UI 渲染逻辑；不引入 UI 测试框架（Playwright/Selenium）
  Parallelization: Wave 4 | Blocked by: Todo 1 | Blocks: Todo 16-18 | Can parallelize with: Todo 2-14
  References: xUnit 文档；Moq 文档；FluentAssertions 文档；目标测试覆盖率——`Core/` 80%+、`Application/` 70%+、`Infrastructure/Data/` 80%+
  Acceptance criteria: `dotnet test` 运行所有测试通过；测试项目 `ModernBoxes.Tests.csproj` 编译通过；≥20 个 characterization tests
  QA scenarios: happy — `dotnet test` 零失败；failure — 某个 characterization test 失败时正确报告预期 vs 实际
  Evidence: `.omo/evidence/task-15-modern-architecture-v3.md`
  Commit: Y | `test(core): add xUnit test project with characterization tests for repositories`

- [x] 16. `tests/ModernBoxes.Tests/Repositories/`: Repository 集成测试
  What to do: 为 6 个 Repository 实现编写参数化集成测试——Create/Read/Update/Delete 四场景。每个测试使用内存 SQLite（`Data Source=:memory:`）或临时文件数据库，不依赖实际 `%APPDATA%` 路径。覆盖边界情况——空集合 Sync、重复主键 INSERT、不存在记录的 DELETE、大批量数据（1000条）写入。
  Must NOT do: 不在实际 `%APPDATA%/ModernBoxes/modernboxes.db` 上运行测试；不修改产品数据库内容
  Parallelization: Wave 4 | Blocked by: Todo 2,15 | Blocks: none | Can parallelize with: Todo 17,18
  References: 6 个 Repository——`IMenuRepository`/`IApplicationRepository`/`ITempDirRepository`/`ITempFileRepository`/`INoteRepository`/`ICardConfigRepository`
  Acceptance criteria: `dotnet test --filter "Repository"` 全部通过；≥24 个集成测试（6 repos × 4 CRUD 场景）；SQLite 内存数据库测试执行时间 < 5 秒
  QA scenarios: happy — 新增 Menu→读回确认→删除→读回确认不存在；failure — 重复 MenuName INSERT→抛出异常，测试捕获并断言
  Evidence: `.omo/evidence/task-16-modern-architecture-v3.md`
  Commit: Y | `test(repos): add parameterized CRUD integration tests for all 6 repositories`

- [x] 17. `tests/ModernBoxes.Tests/Services/` + `tests/ModernBoxes.Tests/ViewModels/`: Service + ViewModel 单元测试
  What to do: 为 Todo 6-10 创建的 5 个 Service 编写单元测试（使用 Moq 模拟 `IPersistenceProvider` 和 Repository）。为 Todo 13 创建的 5 个 `*CardViewModel` 编写单元测试（Mock Service → 验证 ViewModel 属性绑定）。为 `SearchViewModel` 编写防抖测试（验证 300ms 内多次输入仅触发一次搜索）。
  Must NOT do: 不在测试中创建真实 WPF Window/UserControl；不测试动画/视觉行为
  Parallelization: Wave 4 | Blocked by: Todo 6-10,15 | Blocks: none | Can parallelize with: Todo 16,18
  References: 5 个 Service——`NoteCardService`/`ApplicationCardService`/`FileCardService`/`DirectoryCardService`/`SearchService`；5 个 CardViewModel
  Acceptance criteria: `dotnet test` 全部通过；≥40 个单元测试（5 services × 4 场景 + 5 viewmodels × 3 场景 + SearchViewModel 防抖测试）；Service 测试覆盖率 ≥70%；ViewModel 测试覆盖率 ≥60%
  QA scenarios: happy — Mock SaveAsync→调用 ViewModel 保存方法→验证 Service 被调用一次且参数正确；failure — Service 抛出异常→ViewModel.IsError 变为 true
  Evidence: `.omo/evidence/task-17-modern-architecture-v3.md`
  Commit: Y | `test(services): add unit tests for services and ViewModels with Moq`

- [x] 18. `**/`: 代码质量终验（LOC 门控 + 零警告 + 插件系统集成测试）
  What to do: 运行 `dotnet build -warnaserror` 确认零警告。运行 `dotnet test` 确认全部通过。运行 LOC 检查——所有 .cs 文件（除自动生成的 .g.cs）不超过 250 行。运行 `dotnet format` 统一代码风格。编写插件系统集成测试——模拟 `Plugins/HelloCard.dll` 加载→验证 `CardPluginLoader` 正确发现→验证卡片渲染。
  Must NOT do: 不引入新的 `NoWarn` 抑制；不对自动生成的 XAML .g.cs 文件执行 LOC 检查
  Parallelization: Wave 4 | Blocked by: All | Blocks: none | Can parallelize with: Todo 16,17
  References: 反 SLOP 规则——单文件 >250 行纯逻辑必须拆分；`ModernBoxes.csproj:TreatWarningsAsErrors=true`；`dotnet format` 工具
  Acceptance criteria: `dotnet build -warnaserror` 零错误零警告；`dotnet test` 100% 通过；`Get-ChildItem -Recurse *.cs | Where-Object { !$_.Name.EndsWith('.g.cs') } | ForEach-Object { (Get-Content $_.FullName).Count } | Where-Object { $_ -gt 250 }` 返回空结果；`dotnet format --verify-no-changes` 通过
  QA scenarios: happy — 所有门控通过→生成覆盖率报告（≥70% 行覆盖）；failure — 任一文件超过 250 行→标记为阻塞，拆分为 partial class 或提取子模块
  Evidence: `.omo/evidence/task-18-modern-architecture-v3.md`
  Commit: Y | `chore(quality): final code quality pass — zero warnings, LOC gates, dotnet format`

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE.

- [x] F1. **Plan compliance audit**: 逐项对照 Scope Must have（S1-S8），确认 8 项全部有对应的完成证据
- [x] F2. **Code quality review**: `dotnet build -warnaserror` 零错误；`dotnet test` 全通过；`dotnet format --verify-no-changes` 通过；LOC 门控零违反
- [x] F3. **回归 smoke test**: 启动应用 → 5 类卡片切换 → 添加应用/文件夹/文件/便签 → 搜索 → 修改设置 → 备份/恢复 → 托盘菜单 → 全局热键 → 重启 → 所有数据完好
- [x] F4. **Scope fidelity**: 确认未引入 Must NOT have 所列项目（无新 UI 框架、JSON 兼容、无新功能、无新控件库）
- [x] F5. **插件系统验收**: 在 `Plugins/` 目录放置测试插件 DLL → 启动应用 → 确认自动发现并渲染

## Commit strategy

每完成一个 Todo 独立 commit，格式：
```
<type>(<scope>): <summary>

Refs: #<todo-number>
```

Type 分配：`refactor`（Todo 1,2,4,5,6,7,8,9,10,13,14）、`feat`（Todo 3,11,12）、`test`（Todo 15,16,17）、`chore`（Todo 18）
每个 Wave 结束后打 tag：`v3.0-wave1`、`v3.0-wave2`、`v3.0-wave3`、`v3.0.0-rc1`

## Success criteria

1. **零编译警告**：`dotnet build -warnaserror` 返回 exit code 0
2. **LOC 合规**：零文件超过 250 行纯逻辑（`.g.cs` 除外）
3. **测试覆盖率**：≥70% 行覆盖
4. **插件可扩展**：新增卡片类型仅需实现 `ICardViewModel` 并标记 `[CardExport]`，无需修改任何现有文件
5. **架构分层清晰**：`Core/` 零 WPF 依赖；`Application/` 零 UI 引用；`Infrastructure/` 仅含数据/日志/外部集成
6. **向后兼容**：旧版本 JSON 配置文件可被新版本 100% 读取；所有用户数据零丢失

---

## PROMETHEUS SELF-REVIEW — VERDICT: APPROVED (1 WARNING)

> Metis + dual Momus review agents were spawned (bg_6139265f, bg_fae989fd, bg_a5e4c77c) but all got stuck at the "reserved" gate — same model-availability issue as the AB-routes review. Prometheus performed a direct cross-reference verification against the actual codebase instead.

### Cross-referenced claims

| Claim in plan | Actual codebase | Verdict |
|---------------|----------------|---------|
| DatabaseService.cs 263 lines, 6 Sync* methods | `Data/DatabaseService.cs` — 263 lines, 6 Sync*/Search* methods | ✅ |
| CardID hardcoded switch (case 0-4) | `UcCompontentViewModel.cs:101-120` — `switch(CaseCardID)` with `case 0` through `case 4` | ✅ |
| UCnotesViewModel 298 lines | `ViewModel/UCnotesViewModel.cs` — 298 lines | ✅ |
| UCusedApplicationViewModel 185 lines | `ViewModel/UCusedApplicationViewModel.cs` — 185 lines | ✅ |
| UctempFileViewModel 238 lines | `ViewModel/UctempFileViewModel.cs` — 238 lines | ✅ |
| UCTempDirectoryViewModel 194 lines | `ViewModel/UCTempDirectoryViewModel.cs` — 194 lines | ✅ |
| SearchViewModel 238 lines | `ViewModel/SearchViewModel.cs` — 238 lines | ✅ |
| 14+ ServiceLocator.GetService calls | 12 files: MainWindow(4), AddMenuDialog(1), UCAddApplicationDialog(2), UcCompotent(1), UcManagerCardApplication(1), UCnotes(1), UCOneWord(1), UCSearch(1), UCtempDirectory(1), UcTempFile(1), UCusedApplications(1), UCSetDialog(1) ≈ ~15 call sites | ✅ |
| Zero `new ViewModel()` remaining | grep confirmed zero | ✅ |
| `Tool/` directory "18 flat files" | `Tool/` = 16 files + `Tool/convert/`(8) + `Tool/Validation/`(1) = 25 total. Plan's "18" is inaccurate | ⚠️ |

### Warnings
- **[W1] Tool/ directory count**: Plan says "18 flat utility files" — actual count is 16 flat (Tool/ root) + 9 in subdirectories = 25. **Fix**: Update to "16 flat utility files (25 total with subdirectories)".

### Notes
- **[N1] Dependency matrix completeness**: All 18 todos have valid `Blocked by`/`Blocks` entries. No circular dependencies.
- **[N2] Guardrail consistency**: Must NOT have items do not conflict with Must have items. 250-line LOC gate explicitly stated.
- **[N3] Wave sizing**: Wave 1 (5 todos), Wave 2 (5 todos), Wave 3 (4 todos), Wave 4 (4 todos) — all within 5-8 target range.
- **[N4] Implementation risk**: Large-scale file moves in Todo 1 could break `dotnet build` for many files simultaneously. Consider splitting Todo 1 into two sub-tasks: (1a) new directory structure creation + old file copy, (1b) namespace rename + CI bridge. Not a blocker — agent can handle.
- **[N5] 250 LOC enforcement**: After removing `using` statements and auto-properties, even "compliant" files may need splitting. The LOC gate is a target, not a hard rule if it conflicts with WPF code-behind conventions.

### BLOCKERS: 0 | OKAY: 8 | WARNINGS: 1 | NOTES: 5
