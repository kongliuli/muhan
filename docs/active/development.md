# 开发指南

## 环境要求

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## 构建

```bash
dotnet build Sinx.sln -warnaserror
```

## 运行

默认启动项目为 `ModernBoxes.Desktop`：

```bash
dotnet run --project src/ModernBoxes.Desktop/ModernBoxes.Desktop.csproj
```

或在 Visual Studio 中打开 `Sinx.sln`，将 **ModernBoxes.Desktop** 设为启动项目后 F5。

## 测试

```bash
dotnet test tests/ModernBoxes.Tests/ModernBoxes.Tests.csproj
```

当前目标：145+ 测试全绿，0 警告。

> 测试项目引用 Cards、Core、Infrastructure，**不引用 Desktop**（避免 WPF exe 导致 `dotnet test` 挂起）。

## 项目 TFM

| 项目 | TargetFramework |
|------|-----------------|
| Core | `net10.0` |
| Cards | `net10.0` |
| Infrastructure | `net10.0-windows` |
| Desktop | `net10.0-windows`（UseWPF） |
| Tests | `net10.0-windows` |

共享属性见根目录 `Directory.Build.props`（Nullable、TreatWarningsAsErrors 等）。

## 配置文件

JSON 与 `NLog.config`、`App.config` 位于 `src/ModernBoxes.Desktop/`，构建时复制到输出目录。运行时路径与单体时代一致（应用目录下）。

## 插件开发

1. 新建类库，引用 `ModernBoxes.Core`（及需要的 Infrastructure 包）
2. 实现 `ICardViewModel`，添加 `[CardExport(...)]`
3. 编译后将 DLL 放入 `{运行目录}/Plugins/`
4. 重启应用

内置卡片实现可参考 `src/ModernBoxes.Cards/`。
