# ModernBoxes

基于 [木函](https://github.com/HerbertHe/ModernBoxes) 的二次开发版本。

Windows 桌面效率启动器：无边框侧边栏窗口，通过可切换的「卡片」快速访问应用、便签、临时文件/文件夹、一言等内容。支持系统托盘、全局热键、搜索与数据备份。

## 快速开始

```bash
dotnet build Sinx.sln
dotnet run --project src/ModernBoxes.Desktop/ModernBoxes.Desktop.csproj
dotnet test tests/ModernBoxes.Tests/ModernBoxes.Tests.csproj
```

需要 [.NET 10 SDK](https://dotnet.microsoft.com/download)，Windows 10/11。

## 文档

详细说明见 [`docs/active/`](docs/active/)：

- [架构与项目分层](docs/active/architecture.md)
- [构建、运行与测试](docs/active/development.md)

历史规划与 AI 工具链记录见 [`docs/archive/`](docs/archive/)（只读参考）。
