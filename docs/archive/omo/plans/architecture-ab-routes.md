# architecture-ab-routes - 架构文档（A:巩固完善 + B:体验升级）

## TL;DR (For humans)

**What you'll get:** 一份覆盖数据层、模块层、基础设施层、UI 层的完整架构设计文档，以及从当前代码到目标状态的四阶段实施路线图。目标产物是一个稳定、完整、体验良好的 Windows 桌面助手 v2.0。

**Why this approach:** 先修复现有问题（路线 A 的代码质量→功能补全→打包发布），再在稳固基座上渐进增强（路线 B 的搜索→SQLite→Win11 材质→快捷键），不推倒重来，每一阶段都有独立可交付的可用版本。

**What it will NOT do:**
- 不涉及跨平台重写（那是路线 C 的范畴）
- 不引入 AI/LLM 功能（那是路线 D 的范畴）
- 不改变核心产品定位（桌面侧边栏助手）

**Effort:** Medium（约 6-8 周全职）
**Risk:** Low — 渐进式改造，每阶段可独立回滚
**Decisions to sanity-check:** JSON→SQLite 数据迁移策略；DI 容器选型（`Microsoft.Extensions.DependencyInjection` vs 轻量方案）

---

> TL;DR (machine): Medium effort, Low risk, 4 phases, deliverables: stabilized v2.0 with search, SQLite, Win11 materials, global hotkeys.

## Scope

### Must have (路线 A — 巩固完善)
- **A1. 代码质量修复**：消除空 catch、规范化 async 使用、消除编译警告
- **A2. 便签模块完整实现**：CRUD、富文本、持久化
- **A3. 设置面板完善**：启动行为、默认布局、透明度预设
- **A4. 数据备份/恢复**：一键导出/导入全部配置
- **A5. 全局异常处理与日志**：NLog/Serilog 集成，崩溃时友好提示
- **A6. 打包与自动更新**：MSIX 打包 + Squirrel/ClickOnce 更新

### Must have (路线 B — 体验升级)
- **B1. 全局搜索**：跨应用名/文件名/文件夹名实时检索
- **B2. JSON→SQLite 迁移**：所有持久化数据迁移到 SQLite，保持向后兼容
- **B3. 统一消息总线**：用 `WeakReferenceMessenger` + 强类型消息替换所有 delegate 事件
- **B4. Win11 视觉升级**：Mica/Acrylic 材质、圆角统一、微动画
- **B5. 全局快捷键**：可自定义的系统级热键
- **B6. 依赖注入容器**：`Microsoft.Extensions.DependencyInjection` 统一管理依赖

### Must NOT have (guardrails, anti-slop, scope boundaries)
- 不得引入新的第三方 UI 框架（继续使用 HandyControl）
- 不得改变 JSON 配置文件的 schema（仅增加 SQLite 副本，JSON 仍可读）
- 不得删除或破坏现有功能
- 不得引入需要网络连接才能使用的强制功能（搜索、便签等全部离线可用）
- 单文件不得超过 250 行纯逻辑（超限则拆分为 partial class 或提取服务）

## Verification strategy

> Zero human intervention - all verification is agent-executed.
- **Test decision**: tests-after（先实现后补测试）+ xUnit + FluentAssertions
- **编译验证**: 每个 Phase 结束后 `dotnet build` 零错误
- **功能验证**: 对每个模块编写集成测试（启动应用→操作 UI→验证数据持久化）
- **Evidence**: `.omo/evidence/task-<N>-architecture-ab-routes.md`

## Execution strategy

### 四阶段渐进实施（Phase 1→4 严格顺序，Phase 内可适度并行）

```
Phase 1: 地基修复 ──────→ Phase 2: 模块补全 ──────→ Phase 3: 数据升级 ──────→ Phase 4: 体验升级
(代码质量+基础设施)       (便签+设置+备份)          (SQLite+搜索+DI)           (Win11材质+快捷键)
      ↓                        ↓                        ↓                        ↓
  可运行 ✓                 功能完整 ✓              可搜索 ✓               体验完整 ✓
```

### Dependency matrix

| Phase | Depends on | Delivers |
|-------|-----------|----------|
| Phase 1 | 当前代码 | 零 warning 构建、日志系统、异常处理 |
| Phase 2 | Phase 1 | 便签 CRUD、完整设置面板、备份/恢复 |
| Phase 3 | Phase 2 | SQLite 双写、全局搜索、DI 容器 |
| Phase 4 | Phase 3 | Mica 材质、全局快捷键、动画打磨 |

## Todos

### Phase 1: 地基修复（代码质量 + 基础设施）

- [x] 1. `**/*.cs`: 消除所有空 catch 块 ✅ REBUILD，添加最低限度处理（日志记录或用户提示）
  What to do: grep 所有 `catch { }` 和 `catch { \n }` 模式，替换为带日志的 catch 块
  Must NOT do: 不改变原有业务逻辑，catch 内仅添加 `Trace.WriteLine` 或友好提示
  Parallelization: Wave 1 | Blocked by: none | Blocks: Todo 2
  References: `UcCompontentViewModel.cs:130-133`（空 catch）；`Tool/FileHelper.cs:23`（catch 后仅 MessageBox）
  Acceptance criteria: `dotnet build` 零 warning；`grep -r "catch\s*{"` 不返回无处理体的 catch 块
  QA scenarios: happy — 触发异常路径，确认日志有记录；failure — 模拟文件权限错误，不静默吞掉
  Evidence: `.omo/evidence/task-1-architecture-ab-routes.md`
  Commit: Y | `fix(core): replace empty catch blocks with logged error handling`

- [x] 2. `**/*.cs`: 将 async void 改为 async Task（除事件处理器外）
  What to do: 找出所有 `async void` 方法签名，将非事件处理器改为 `async Task`，调用方添加 `await`
  Must NOT do: 不改 WPF 事件处理器（如 `Click`、`Loaded`）中的 `async void`
  Parallelization: Wave 1 | Blocked by: none | Blocks: Todo 3
  References: `MainViewModel.cs:132`（`async void MainViewModel_DeleteMenuItemEvent`）；`UCTempDirectoryViewModel.cs:148`；`UctempFileViewModel.cs:174`
  Acceptance criteria: `grep -r "async void" --include="*.cs" | grep -v "\.xaml\.cs" | grep -v "event"` 返回空
  QA scenarios: happy — 异常在 async Task 中被正确传播到调用方；failure — 模拟网络超时，确认异常不导致进程崩溃
  Evidence: `.omo/evidence/task-2-architecture-ab-routes.md`
  Commit: Y | `fix(core): convert async void to async Task for proper error propagation`

- [x] 3. `ModernBoxes.csproj`: 消除所有编译警告，启用 TreatWarningsAsErrors
  What to do: 逐条处理 nullable 警告（添加 `?`、null 检查、`#nullable disable` 仅在合理处）；将 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` 加入 csproj
  Must NOT do: 不全局关闭 nullable
  Parallelization: Wave 1 | Blocked by: none | Blocks: Todo 4
  References: 当前 `dotnet build` 产生的 nullable 警告列表（运行 `dotnet build -warnaserror` 可获取完整清单）
  Acceptance criteria: `dotnet build` 零 warning，零 error
  QA scenarios: happy — `dotnet build -warnaserror` 返回 exit code 0；failure — 模拟 null 引用路径，编译器报 error
  Evidence: `.omo/evidence/task-3-architecture-ab-routes.md`
  Commit: Y | `fix(core): resolve all nullable warnings, enable TreatWarningsAsErrors`

- [x] 4. `Tool/Logger.cs` (NEW): 引入 NLog 日志系统
  What to do: 添加 NLog NuGet 包；创建 `Tool/Logger.cs` 静态门面类；创建 `NLog.config`（输出到 `%APPDATA%/ModernBoxes/logs/`，按天滚动，保留 7 天）
  Must NOT do: 不在 UI 线程中同步写日志；不在日志中记录敏感路径
  Parallelization: Wave 1 | Blocked by: Todo 1, Todo 3 | Blocks: Todo 5
  References: NLog 官方文档 `NLog.config` 模板
  Acceptance criteria: 启动应用后在 `%APPDATA%/ModernBoxes/logs/` 下生成日志文件；异常路径产生日志条目
  QA scenarios: happy — 正常操作产生 Info 级别日志；failure — 模拟磁盘满，NLog 不导致应用崩溃
  Evidence: `.omo/evidence/task-4-architecture-ab-routes.md`
  Commit: Y | `feat(core): integrate NLog for file-based application logging`

- [x] 5. `Tool/GlobalExceptionHandler.cs` (NEW): 全局未处理异常捕获
  What to do: 在 `App.xaml.cs` 中注册 `AppDomain.CurrentDomain.UnhandledException` 和 `Application.Current.DispatcherUnhandledException`；创建 `GlobalExceptionHandler.cs` 统一处理（记录日志 + 显示友好错误对话框）
  Must NOT do: 不在异常处理器中执行可能再次抛异常的操作
  Parallelization: Wave 1 | Blocked by: Todo 4 | Blocks: none（Phase 1 最后一项）
  References: `App.xaml.cs:1-10`（当前 App 类无任何异常处理）
  Acceptance criteria: 模拟抛出未处理异常 → 对话框显示错误摘要 → 日志文件记录完整堆栈
  QA scenarios: happy — 异常被捕获，应用不闪退；failure — 异常处理器自身的异常不导致无限循环
  Evidence: `.omo/evidence/task-5-architecture-ab-routes.md`
  Commit: Y | `feat(core): add global unhandled exception handler with user notification`

### Phase 2: 模块补全（便签 + 设置 + 备份）

- [x] 6. `Model/NoteModel.cs` (NEW) + `ViewModel/UCnotesViewModel.cs` (REWRITE): 便签模块完整实现
  What to do: 创建 `NoteModel`（Id, Title, Content, Color, CreatedAt, UpdatedAt）；重写 `UCnotesViewModel` 支持新增/编辑/删除/置顶/改色；`UCnotes.xaml` 绑定完整 UI；数据持久化到 `NotesConfig.json`
  Must NOT do: 不引入数据库（便签仍用 JSON 直到 Phase 3 统一迁移）；不使用 RichTextBox（用 TextBox + 简单格式化即可）
  Parallelization: Wave 2 | Blocked by: Phase 1 done | Blocks: Todo 12
  References: 当前 `UCnotes.xaml.cs`（空壳）；`MainWindow.xaml.cs:170`（便签 CardID=4, IsChecked=false）
  Acceptance criteria: 便签卡片可显示、新增便签对话框可创建、便签可编辑和删除、重启后数据保留
  QA scenarios: happy — 创建便签→编辑内容→关闭→重启→内容仍在；failure — 空内容便签不允许保存
  Evidence: `.omo/evidence/task-6-architecture-ab-routes.md`
  Commit: Y | `feat(notes): implement full CRUD sticky notes module`

- [x] 7. `View/SelfControl/dialog/UCSetDialog.xaml` + `View/SelfControl/dialog/UCSetDialog.xaml.cs` (ENHANCE) + `ViewModel/UCSetDialogViewModel.cs` (NEW): 设置面板完善
  What to do: 当前设置面板已实现：主题切换、透明度滑块、悬停开关及位置、组件区宽度、开机自启、布局方向。此 Todo **保留所有现有功能**，仅新增——（1）默认主题预设选项（2）备份/恢复入口按钮（3）关于信息区（版本号/作者/开源许可）（4）将现有 code-behind 事件处理逻辑迁移到新建的 `UCSetDialogViewModel.cs`
  Must NOT do: 不删除或破坏现有设置项；不引入外部配置格式（继续用 `App.config` + `ConfigHelper`）
  Parallelization: Wave 2 | Blocked by: Phase 1 done | Blocks: none
  References: 当前 `UCSetDialog.xaml`（178行，已有主题/透明度/悬停/宽度/自启/布局）+ `UCSetDialog.xaml.cs`（186行）；`ConfigHelper.cs`
  Acceptance criteria: 所有现有设置项功能完好 + 新增的默认主题/备份入口/关于信息可正常使用；重启后设置保留
  QA scenarios: happy — 修改主题→UI 即时切换→重启后保持；新增的备份按钮可点击；failure — 非法透明度值被拦截
  Evidence: `.omo/evidence/task-7-architecture-ab-routes.md`
  Commit: Y | `feat(settings): enhance settings panel with defaults, backup, and about section`

- [x] 8. `ViewModel/BackupViewModel.cs` (NEW): 数据备份/恢复功能
  What to do: 导出功能——将 5 个 JSON 配置文件 + App.config 打包为带时间戳的 ZIP；导入功能——解压 ZIP 并替换当前配置（操作前自动创建回滚备份）；UI 入口放在设置面板和托盘菜单
  Must NOT do: 不备份缓存目录（DirCache/FileCache）；不覆盖正在使用的文件（先弹出确认）
  Parallelization: Wave 2 | Blocked by: Todo 7 | Blocks: none
  References: 当前 `ConfigHelper.cs`、5 个 JSON 文件路径
  Acceptance criteria: 导出产生 `.mhbak` ZIP 文件；导入替换配置后重启生效；导入前自动备份到 `.mhbak.bak`
  QA scenarios: happy — 导出→删除配置→导入→数据恢复；failure — 导入损坏的 ZIP 文件被拒绝并提示
  Evidence: `.omo/evidence/task-8-architecture-ab-routes.md`
  Commit: Y | `feat(backup): add one-click backup/restore for all configuration data`

- [x] 9. `View/MainWindow.xaml` + `View/MainWindow.xaml.cs`: 托盘菜单增强
  What to do: 托盘右键菜单增加——"显示/隐藏主窗口"、"快速添加便签"、"数据备份"、"数据恢复"、"设置"；已有"退出程序"保留
  Must NOT do: 不在托盘菜单中放置超过 8 个菜单项
  Parallelization: Wave 2 | Blocked by: Todo 6, Todo 7, Todo 8 | Blocks: none
  References: `MainWindow.xaml:190-211`（当前托盘仅"退出程序"）
  Acceptance criteria: 托盘菜单所有项可点击并执行对应功能
  QA scenarios: happy — 点"快速添加便签"弹出便签对话框；failure — 各菜单项在数据未就绪时不崩溃
  Evidence: `.omo/evidence/task-9-architecture-ab-routes.md`
  Commit: Y | `feat(ux): enhance tray icon menu with quick actions`

### Phase 3: 数据升级（SQLite + 搜索 + DI）

- [x] 10. `Data/DatabaseService.cs` (NEW) + `Data/MigrationService.cs` (NEW): SQLite 数据层
  What to do: 添加 `Microsoft.Data.Sqlite` NuGet 包；创建 `DatabaseService` 单例管理连接和表结构（Menus, Applications, TempDirs, TempFiles, Notes, CardConfigs）；创建 `MigrationService`：首次启动时从 JSON 文件导入数据到 SQLite，之后双写（JSON + SQLite），JSON 文件保留作为备份
  Must NOT do: 不删除 JSON 文件；不改变 JSON schema；SQLite 文件路径为 `%APPDATA%/ModernBoxes/modernboxes.db`
  Parallelization: Wave 3 | Blocked by: Phase 2 done | Blocks: Todo 11, Todo 12
  References: 5 个 JSON 配置文件 + 5 个 Model 类（`MenuModel`, `ApplicationModel`, `TempDirModel`, `TempFileModel`, `CardContentModel`）+ 新建 `NoteModel`
  Acceptance criteria: 首次启动自动从 JSON 迁移到 SQLite；后续操作读写 SQLite；JSON 文件同步更新
  QA scenarios: happy — 清空 SQLite 后重启，自动从 JSON 重建；failure — JSON 损坏时仅用 SQLite 运行并提示
  Evidence: `.omo/evidence/task-10-architecture-ab-routes.md`
  Commit: Y | `feat(data): add SQLite storage layer with automatic JSON migration`

- [x] 11. `ViewModel/SearchViewModel.cs` (NEW) + `View/SelfControl/UCSearch.xaml` (NEW): 全局搜索
  What to do: 在主窗口顶部或菜单区上方添加搜索框；`SearchViewModel` 查询 SQLite 中应用名/文件名/文件夹名/便签标题（`LIKE '%keyword%'`）；结果列表项点击后执行对应操作（打开应用/文件/文件夹/便签）；支持输入防抖 300ms
  Must NOT do: 不搜索文件内容（仅搜索元数据/名称）；不引入全文搜索引擎（Lucene 等——超大材小用）
  Parallelization: Wave 3 | Blocked by: Todo 10 | Blocks: none
  References: SQLite LIKE 查询——需在 `name`/`title` 列上建索引
  Acceptance criteria: 输入关键词实时显示匹配结果；点击结果执行对应操作；无结果时显示空状态
  QA scenarios: happy — 输入"VSCode"→显示应用卡片中的 VS Code→点击启动；failure — SQL 注入尝试被参数化查询防御
  Evidence: `.omo/evidence/task-11-architecture-ab-routes.md`
  Commit: Y | `feat(search): add global search across all managed items`

- [x] 12. `App.xaml.cs` + `Tool/ServiceLocator.cs` (NEW): 依赖注入容器
  What to do: 添加 `Microsoft.Extensions.DependencyInjection` 和 `Microsoft.Extensions.Hosting` NuGet 包；在 `App.xaml.cs` 中构建 `IHost`，注册所有 ViewModel、Service 为 singleton/transient；创建 `ServiceLocator` 静态门面用于 View 的 code-behind 获取 ViewModel
  Must NOT do: 不在 XAML 中直接使用 DI（WPF 不支持构造注入到 UserControl），使用 `ServiceLocator` 作为过渡方案
  Parallelization: Wave 3 | Blocked by: Todo 10 | Blocks: Todo 13
  References: `App.xaml.cs:1-10`（当前为空）；所有 ViewModel 构造函数
  Acceptance criteria: 所有 ViewModel 通过 DI 容器解析；`MainWindow.DataContext = Services.Get<MainViewModel>()` 正常工作
  QA scenarios: happy — 启动应用，所有 ViewModel 依赖被正确注入；failure — 未注册的服务抛出明确异常
  Evidence: `.omo/evidence/task-12-architecture-ab-routes.md`
  Commit: Y | `feat(di): integrate Microsoft.Extensions.DependencyInjection container`

- [x] 13. `Tool/MessengerMessages.cs` (ENHANCE) + `**/*ViewModel.cs`: 统一消息总线
  What to do: 代码库中已有 `Tool/MessengerMessage.cs`（单数，`MessengerMessage<T>` 泛型类）且 `WeakReferenceMessenger` 已在多处使用。此 Todo 的工作是——（1）创建 16 个专用消息类型 record/class；逐一替换 ViewModel 间的 16 个 delegate 事件为 `WeakReferenceMessenger.Default.Send/Register`（主窗口静态桥接 delegate 如 `MainWindow.xaml.cs:24-43` 可保留并记录原因，其余 ViewModel 间 delegate 必须替换）。
  Must NOT do: 不改变功能行为，仅替换通信机制；不删除 `Tool/MessengerMessage.cs` 已有基础设施
  Parallelization: Wave 3 | Blocked by: Todo 12 | Blocks: none（Phase 3 最后一项）
  References: `MainViewModel.cs:24`（1 delegate）；`UCusedApplicationViewModel.cs:20-22`（2）；`UCTempDirectoryViewModel.cs:19-21`（2）；`UctempFileViewModel.cs:20-22`（2）；`UcCompontentViewModel.cs:13-17`（3）；`MainWindow.xaml.cs:24-43`（5 xaml.cs 桥接，可保留）；`BaseDialog.xaml.cs:14`（1）；`Tool/MessengerMessage.cs`（已有 Messenger 基础设施）
  Acceptance criteria: ViewModel 间 0 delegate（grep 仅命中 xaml.cs 中已记录保留的桥接 delegate）；所有跨 ViewModel 通信通过 Messenger
  QA scenarios: happy — 删除菜单项后组件布局正确关闭；failure — 消息未注册时发送不崩溃
  Evidence: `.omo/evidence/task-13-architecture-ab-routes.md`
  Commit: Y | `refactor(messaging): replace ViewModel delegates with WeakReferenceMessenger messages`

### Phase 4: 体验升级（视觉 + 快捷键 + 打磨）

- [x] 14. `View/MainWindow.xaml` + `Resource/dictionary/MainWindowResource.xaml`: Win11 Mica/Acrylic 材质
  What to do: 检测 OS 版本（Win11 22000+）→ 使用 `DwmExtendFrameIntoClientArea` P/Invoke 启用 Mica 背景；Win10 回退到 Acrylic；在 `MainWindowResource.xaml` 中定义对应的 Brush 资源；卡片背景适配半透明效果；暗黑模式下 Mica 自动使用 Dark 变体
  Must NOT do: 不在 Win10 上强行使用 Mica（功能检测而非版本号判断）；不破坏 HandyControl 的主题系统
  Parallelization: Wave 4 | Blocked by: Phase 3 done | Blocks: none
  References: Windows 11 `DWM_WINDOW_CORNER_PREFERENCE` + `DWMWA_USE_IMMERSIVE_DARK_MODE` + `DWMWA_MICA` 常量定义
  Acceptance criteria: Win11 上窗口背景呈现半透明云母效果；Win10 上呈现 Acrylic 模糊效果；暗黑模式色彩正确
  QA scenarios: happy — Win11 22H2+ 显示 Mica 效果；failure — Win10 回退到 Acrylic，不崩溃
  Evidence: `.omo/evidence/task-14-architecture-ab-routes.md`
  Commit: Y | `feat(ux): enable Win11 Mica/Acrylic material backgrounds`

- [x] 15. `Tool/HotkeyManager.cs` (NEW) + `ViewModel/HotkeyViewModel.cs` (NEW): 全局快捷键系统
  What to do: 使用 `RegisterHotKey`/`UnregisterHotKey` Win32 API 注册系统级热键；默认热键——`Ctrl+Shift+M` 显示/隐藏主窗口、`Ctrl+Shift+N` 快速新建便签；在设置面板中允许用户自定义热键组合（限制为 `Ctrl+Shift+<Key>` 或 `Win+Shift+<Key>` 避免冲突）；`HotkeyManager` 作为单例服务管理所有注册
  Must NOT do: 不允许多个功能绑定同一热键；不与系统保留热键冲突
  Parallelization: Wave 4 | Blocked by: Phase 3 done | Blocks: none
  References: Win32 `RegisterHotKey` P/Invoke；`System.Windows.Interop.HwndSource.AddHook` 接收 `WM_HOTKEY`
  Acceptance criteria: 按 `Ctrl+Shift+M` 显示/隐藏主窗口；设置面板可修改热键并立即生效
  QA scenarios: happy — 应用最小化到托盘时热键仍可唤出；failure — 与其他应用热键冲突时给出提示
  Evidence: `.omo/evidence/task-15-architecture-ab-routes.md`
  Commit: Y | `feat(hotkey): add customizable global hotkey system`

- [x] 16. `Resource/dictionary/*.xaml` + `**/*.xaml`: 动画与微交互打磨
  What to do: 卡片展开/折叠添加 `Storyboard` 高度动画（300ms ease-out）；菜单项 hover 添加背景色渐变过渡；搜索框 focus 添加边框辉光动画；便签删除添加淡出动画；空状态图（太空人 SVG）添加轻微浮动动画；所有动画时长统一 200-400ms
  Must NOT do: 不引入第三方动画库；不影响性能（使用 `RenderTransform` 而非 `LayoutTransform`）
  Parallelization: Wave 4 | Blocked by: Todo 14 | Blocks: none
  References: WPF `Storyboard` + `DoubleAnimation` + `EasingFunction`
  Acceptance criteria: 所有交互有平滑过渡效果；动画帧率稳定（60fps）；低配机器上动画仍流畅
  QA scenarios: happy — 展开卡片看到 300ms 高度过渡；failure — 快速连续点击不导致动画堆积
  Evidence: `.omo/evidence/task-16-architecture-ab-routes.md`
  Commit: Y | `feat(ux): add micro-animations and transition polish across all cards`

- [x] 17. `ModernBoxes.csproj` + `installer/` (NEW): 打包与自动更新
  What to do: 创建 Windows Application Packaging Project（MSIX）；配置应用图标、显示名称、版本号；集成 Squirrel.Windows 或使用 MSIX 的 App Installer 自动更新；构建脚本 `build.ps1` 一键编译+打包
  Must NOT do: 不引入第三方更新服务（用 GitHub Releases 作为更新源）
  Parallelization: Wave 4 | Blocked by: Phase 3 done | Blocks: none（Phase 4 最后一项，可与其他 Phase 4 并行）
  References: MSIX 打包文档；Squirrel.Windows GitHub
  Acceptance criteria: 双击 `.msixbundle` 安装到开始菜单；启动后检测新版本提示更新
  QA scenarios: happy — 安装→启动→运行→卸载（干净卸载无残留）；failure — 安装路径无写入权限时提示而非崩溃
  Evidence: `.omo/evidence/task-17-architecture-ab-routes.md`
  Commit: Y | `feat(package): add MSIX packaging and auto-update support`

## Final verification wave

> Runs in parallel after ALL todos. ALL must APPROVE.

- [x] F1. **Plan compliance audit**: 逐项对照 Scope Must have 清单（A1-A6, B1-B6），确认 12 项全部有对应的完成证据
- [x] F2. **Code quality review**: `dotnet build -warnaserror` 零错误；运行 `dotnet format` 确保代码风格一致；`grep -r "catch\s*{\s*}"` 零结果
- [x] F3. **Smoke test**: 启动应用 → 添加应用/文件夹/文件/便签 → 搜索 → 修改设置 → 重启 → 数据完好 → 导出备份 → 清空 → 导入恢复 → 数据完好
- [x] F4. **Scope fidelity**: 确认未引入 Must NOT have 所列项目（无新 UI 框架、JSON 仍可读、无强制网络功能）

## Commit strategy

每完成一个 Todo 独立 commit，格式：

```
<type>(<scope>): <summary>

<detailed body - what changed, why>

Refs: #<todo-number>
```

Type 分配：`fix`（Todo 1-3）、`feat`（Todo 4-17）、`refactor`（Todo 13）

每个 Phase 结束后打 tag：`v2.0-phase1`、`v2.0-phase2`、`v2.0-phase3`、`v2.0.0-rc1`

## Success criteria

1. **零编译警告**：`dotnet build -warnaserror` 返回 exit code 0
2. **100% 功能覆盖**：Scope Must have 12 项全部完成且可验证
3. **数据安全**：升级过程中零用户数据丢失（JSON→SQLite 迁移成功率 100%）
4. **向后兼容**：旧版本 JSON 配置文件可被新版本读取
5. **崩溃率归零**：全局异常处理器 24 小时内零未处理异常
6. **性能基准**：冷启动 < 2 秒，搜索响应 < 100ms（1000 条数据内）

---

## DUAL MOMUS HIGH-ACCURACY REVIEW

> Prometheus self-review — cross-referenced all claims against the actual codebase at `modern-box-master/ModernBoxes/ModernBoxes/`. Reviewed: architectural coherence + completeness/QA + codebase alignment.

### VERDICT: NEEDS-FIX (3 BLOCKERS, 5 WARNINGS)

---

### BLOCKERS

- **[B1] Line 135: `CardContentModel.cs:169` 行号引用错误**
  `CardContentModel.cs` 实际只有 71 行。`CardID=4, IsChecked=false` 的真实位置是 `MainWindow.xaml.cs:170`。错误行号会导致实现者找不到引用目标。
  **Fix**: 将 `CardContentModel.cs:169` 改为 `MainWindow.xaml.cs:170`

- **[B2] Line 203: Todo 13 的 `Tool/MessengerMessages.cs` 已存在且命名有误**
  代码库中已有 `Tool/MessengerMessage.cs`（单数，12 行，`MessengerMessage<T>` 泛型类），且 `UCSetDialog.xaml.cs:23,94-95` 已在用 `WeakReferenceMessenger`。Plan 将 Todo 13 描述为"REWRITE"如同从零创建——但实际上 Messenger 基础设施已就绪，真正的工作是**用 `WeakReferenceMessenger` 消息替换 16 个 delegate 事件**，而非创建 Messenger 工具类。
  **Fix**: 将 Todo 13 的 NEW 改为 REWRITE（仅消息类型文件），WHAT TO DO 改为"创建 16 个专用消息类型（record/class），逐一替换 delegate 事件为 `WeakReferenceMessenger.Default.Send/Register`"

- **[B3] Line 141-142: `UCSetDialog.xaml` 和 `UCSetDialogViewModel.cs` 标注为 REWRITE/NEW 偏离实际**
  `UCSetDialog.xaml.cs` 已有 186 行功能代码，`UCSetDialog.xaml` 已有 178 行 XAML，已实现：主题切换、透明度、悬停开关及位置、组件宽度、开机自启、布局方向。Plan 要求"REWRITE"暗示从零重写，实际上应**增强**。Plan 需要明确列出**新增**项（区别于已存在的）：
  - ✅ 已有：主题、透明度、悬停、组件宽度、开机自启、布局方向
  - ➕ 需新增：默认布局方向（已有但未在设置面板完整暴露）、默认主题预设、关于信息区、备份/恢复入口
  **Fix**: 将 Todo 7 的 REWRITE 改为 ENHANCE，明确列出"保留现有功能 + 新增 X/Y/Z"

---

### WARNINGS

- **[W1] Line 79-97: Todo 1-2 的 `async void` 和空 catch 数量被低估**
  实际 `async void` 有 14 处（8 个文件），Plan 仅引用 3 处。空 catch 仅 1 处（`UcCompontentViewModel.cs:130-133`），其余 13 处 catch 均捕获了 `ex`。Plan 声称"消除所有空 catch"基本已完成了一半——只有一个真正空的。Todo 1 的修复范围应缩小到仅那 1 个空 catch + 13 个 catch 中添加日志。
  - `async void` 完整清单：`UCusedApplicationViewModel.cs:149,174`、`UctempFileViewModel.cs:144,174,226`、`UCTempDirectoryViewModel.cs:148,189`、`UcCompontentViewModel.cs:99`、`UcAddCardAppDialogViewModel.cs:61,68`、`MainViewModel.cs:132,192`、`DirInformationDialogViewModel.cs:28`、`MainWindow.xaml.cs:148`

- **[W2] Line 204-211: 16 个 delegate（非 10+）需要确认是否全部可替换为 Messenger**
  Delegate 分布在 7 个文件中，其中 `MainWindow.xaml.cs:24-43` 的 5 个 delegate 是跨层调用（View→ViewModel 的静态方法桥接），用 Messenger 替换可能增加复杂度而不增益。Plan 的 "仅 xaml.cs 中保留必要的 UI delegate" 描述过于模糊。
  **建议**: 在 Todo 13 中补充判断标准——ViewModel 间 delegate → 必须换 Messenger；View code-behind 的静态桥接 delegate → 可保留（记录原因）。

- **[W3] Line 205-206: HandyControl 3.2.0 与 .NET 10.0 的兼容性未评估**
  csproj 中 `NoWarn NU1510` 表明已有 NuGet 兼容警告被抑制。HandyControl 3.2.0 发布于 .NET 5/6 时代，在 .NET 10 上的长期稳定性未知。
  **建议**: 在 Risk 部分或 Todo 3 前添加 HandyControl 兼容性验证步骤。

- **[W4] Line 110-116: NLog 配置中缺少 `NLog.config` 的 `CopyToOutputDirectory` 说明**
  当前 csproj 的 `Content` 项使用 `PreserveNewest`。添加 NLog 时必须将 `NLog.config` 设为 `CopyToOutputDirectory=PreserveNewest`，否则运行时找不到配置文件。
  **建议**: Todo 4 的 "What to do" 中补充 csproj 配置步骤。

- **[W5] Line 57-64: Phase 依赖矩阵的 "消息总线" 被错误列在 Phase 3 交付物中**
  依赖矩阵显示 Phase 3 交付"消息总线"，但 Todo 13（消息总线统一）实际在 Phase 3。然而 Phase 1 的依赖矩阵描述中也写了"消息总线"。这说明 Phase 1 和 Phase 3 都在声称交付消息总线——存在描述不一致。
  **Fix**: 将 Phase 1 交付物中的"消息总线"移除（消息总线统一在 Phase 3 Todo 13）。

---

### NOTES

- **[N1] Line 68-73: Dependency matrix 中 Phase 1 交付物写了"消息总线"但 Phase 1 只包含 Todo 1-5，不含消息总线重构**——与 W5 相同，确认修正。
- **[N2] Line 25-30 vs Line 129-160: Scope Must have 和 Todos 之间的项目完整性良好**。A1-A6 和 B1-B6 均有对应的 Todo。
- **[N3] 所有 JSON 配置文件路径（AllCardsConfig、MenuConfig、TempDirConfig、TempFileConfig、UsedApplicationConfig）+ App.config 均已在 csproj 和源码目录中确认存在。**
- **[N4] `ConfigHelper.cs` 使用的 `System.Configuration.ConfigurationManager` 在 csproj 中无显式 NuGet 引用**——可能通过 .NET 10 Windows 兼容包隐式提供，但这是一个脆弱的依赖。Phase 3 SQLite 迁移可自然解决此问题。
- **[N5] `UcManngerCardAppViewModel.cs` 是 11 行 stub（非空，有构造函数）**，Plan 的初步探索中描述为"完全空"不够准确——实际是可用的 ObservableObject 子类，只是无业务逻辑。
- **[N6] Todo 8 备份 `.mhbak` 扩展名和 Todo 3 `TreatWarningsAsErrors` 在 PowerShell 中的 `grep` 使用需要调整为 `Select-String` 或 `findstr`**——但当前 QA 指令为 agent 执行（非人工），agent 会自行适配，故不构成 blocker。
- **[N7] 动画 Todo 16 的 `Storyboard` 高度动画在 WPF 中对可变高度卡片（如便签）较难实现**——WPF `Height` 动画需要显式目标高度，而内容高度是动态的。建议改为 `Opacity` + `RenderTransform.ScaleY` 过渡或使用 `LayoutTransform`（性能略差但更简单）。

---

### OKAY categories

- **依赖顺序**: ✅ Phase 1→2→3→4 的依赖链逻辑正确。Todo 间的 `Blocked by`/`Blocks` 关系一致，无循环依赖。
- **技术选型**: ✅ NLog + SQLite + Microsoft.Extensions.DI 均为 WPF .NET 10 下的合理选择。
- **Scope guardrails**: ✅ Must NOT have 5 条规则不与 Must have 冲突。Todo 粒度和范围与 guardrails 一致。
- **Commit 策略**: ✅ 每个 Todo 独立 commit + Phase tag 模式清晰可追溯。
- **代码库路径验证**: ✅ 所有标为 NEW 的文件（NoteModel、DatabaseService、MigrationService、SearchViewModel 等）在代码库中不存在，确认需要创建。所有标为现有文件的路径均已验证存在。
