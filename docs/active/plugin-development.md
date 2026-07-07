# 插件开发指南

基于木函（ModernBoxes）二次开发的卡片插件，三步即可运行。

## 1. 创建类库

- 目标框架：`net10.0-windows`，启用 WPF
- 引用 `ModernBoxes.Core`
- 实现 `ICardViewModel`（推荐继承 `CardBase<T>`）
- 标注 `[CardExport("卡片名", ViewType = typeof(YourView))]`

## 2. 提供视图

- **方式 A（推荐）**：在 `CardExport` 上指定 `ViewType`，视图需继承 `FrameworkElement`
- **方式 B**：在插件程序集中添加 `CardViews.xaml` ResourceDictionary，宿主启动时会自动合并

## 3. 部署

推荐目录结构（`plugin.json` + 主程序集）：

```
Plugins/
  sample-clock/
    plugin.json
    SampleClockCard.dll
```

`plugin.json` 示例：

```json
{
  "id": "sample-clock",
  "name": "示例时钟",
  "version": "1.0.0",
  "author": "muhan",
  "description": "插件示例",
  "main": "SampleClockCard.dll",
  "minHostApiVersion": 1
}
```

构建并复制：

```bash
dotnet build examples/SampleClockCard/SampleClockCard.csproj -c Release
mkdir src\ModernBoxes.Desktop\bin\Debug\net10.0-windows\Plugins\sample-clock
copy examples\SampleClockCard\bin\Release\net10.0-windows\SampleClockCard.dll src\ModernBoxes.Desktop\bin\Debug\net10.0-windows\Plugins\sample-clock\
copy examples\SampleClockCard\plugin.json src\ModernBoxes.Desktop\bin\Debug\net10.0-windows\Plugins\sample-clock\
```

仍支持旧版平铺 `Plugins/*.dll`（无 `plugin.json`），新插件请优先使用子目录 + 清单。

宿主为每个插件使用独立 `AssemblyLoadContext` 加载；`ModernBoxes.Sdk` / `ModernBoxes.Core` 与 WPF 框架程序集由宿主共享。

## 插件安装（Flow 生态）

### 本机 Flow/Wox 导入（设置 → 插件）

自动扫描 `%AppData%\\FlowLauncher\\Plugins` 等目录，将 **C# 插件** 复制到木函 `Plugins/<id>/` 并生成木函格式 `plugin.json`（`type: wox`）。**重启后生效**。

### Flow 社区商店

设置 → 插件 → **加载 Flow 商店** 拉取 [Flow.Launcher.PluginsManifest](https://github.com/Flow-Launcher/Flow.Launcher.PluginsManifest) 清单（jsDelivr CDN），仅展示 **C# 插件**；选中后下载 Release zip 并安装到本地 `Plugins/`，仍走木函 ALC + Wox 适配器。

在 `plugin.json` 中设置 `"type": "wox"`，主程序集需实现 Wox 风格 `Init` + `Query`（或 `Wox.Plugin.IPlugin`）：

```json
{
  "id": "hello-wox",
  "name": "Hello Wox",
  "type": "wox",
  "actionKeyword": "hello",
  "main": "HelloWox.dll",
  "minHostApiVersion": 1
}
```

宿主通过 `WoxPluginAdapter` 将其接入 `QueryEngine`，并通过 `IPublicAPI` / `WoxPublicApiBridge` 提供 `ShowMsg`、`HideMainWindow`、`ChangeQuery`、`ShellOpen` 等常用能力。

## JSON-RPC 插件（Python / 可执行文件）

`"type": "jsonrpc"`，stdin/stdout 单行 JSON-RPC 2.0，方法 `query`：

```json
{
  "id": "hello-jsonrpc",
  "name": "Hello JsonRpc",
  "type": "jsonrpc",
  "runtime": "python",
  "actionKeyword": "py",
  "main": "main.py",
  "minHostApiVersion": 1
}
```

请求（宿主 → 插件）：

```json
{"jsonrpc":"2.0","id":1,"method":"query","params":{"search":"wx","actionKeyword":"","rawQuery":"wx"}}
```

响应（插件 → 宿主）：

```json
{"jsonrpc":"2.0","id":1,"result":[{"Title":"...","SubTitle":"...","ActionCommand":"..."}]}
```

示例见 [`examples/HelloJsonRpcPlugin/`](../examples/HelloJsonRpcPlugin/)。

重启木函，在卡片管理里启用「示例时钟」。

## 版本兼容

- 宿主 API 版本：`CardPluginLoader.HostApiVersion`（当前为 1）
- 插件可通过 `MinHostApiVersion` 声明最低要求；版本不匹配时插件会被跳过并提示

## 示例

见 [`examples/SampleClockCard/`](../examples/SampleClockCard/)。
