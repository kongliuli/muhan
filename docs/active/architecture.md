# 架构

ModernBoxes 采用多项目分层，主解决方案为 `Sinx.sln`。

## 项目结构

```
src/
├── ModernBoxes.Core/             # 领域模型、枚举、接口（零上层依赖）
├── ModernBoxes.Infrastructure/   # 数据访问、平台服务、业务 Service、插件加载
├── ModernBoxes.Cards/            # 内置卡片 ViewModel（可独立测试，无 WPF）
└── ModernBoxes.Desktop/          # WPF 启动项目：UI、HostedService、资源与配置
tests/
└── ModernBoxes.Tests/            # 单元测试
```

## 依赖方向

```
Desktop ──► Infrastructure ──► Core
Desktop ──► Cards ──────────► Core
Tests   ──► Infrastructure ──► Core
Tests   ──► Cards ──────────► Core
```

- **Core**：`Models`、`Enums`、`Interfaces`（含 `ICard`、`CardExportAttribute`）、Repository 接口
- **Infrastructure**：Repository 实现、SQLite/JSON 双写、`CardPluginLoader`、`*CardService`、`SearchService`、热键/更新/备份等平台逻辑
- **Cards**：内置五张卡片的 ViewModel，标注 `[CardExport]`，供 Desktop 与 Tests 引用
- **Desktop**：`App.xaml`、`MainWindow`、Presentation 层、托盘/热键 HostedService、`WpfUserNotifier` 等 UI 适配

## 卡片插件加载

1. `App.xaml.cs` 启动时调用 `CardPluginLoader.DiscoverAndRegister(services, typeof(NoteCardViewModel).Assembly)` 扫描 **Cards 程序集**
2. 外部插件：将 DLL 放入运行目录 `Plugins/`，启动时同样被扫描
3. 实现 `ICardViewModel`（可继承 `CardBase<T>`）并标注 `[CardExport]`

## 持久化

- **主数据源**：运行目录 JSON 配置文件
- **搜索缓存**：SQLite，`DualWriteProvider` 双写 JSON 与数据库
- **升级迁移**：`ConfigMigrationService` 启动时迁移旧字段，备份至 `.backup/pre-migrate_*`

## UI 解耦要点

| 组件 | 位置 | 说明 |
|------|------|------|
| `HotkeyViewModel` | Desktop | 通过 `IHotkeyActions`（Core）回调打开对话框 |
| `TrayHostedService` / `HotkeyHostedService` | Desktop | WPF 生命周期相关 |
| `IUserNotifier` | Core 接口，Desktop 实现 | `AutoOpenSoftware` 不再引用 Presentation |
| `IIconExtractor` | Core 接口，Infrastructure 实现 | 应用卡片图标提取 |
