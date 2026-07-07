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

```bash
dotnet build examples/SampleClockCard/SampleClockCard.csproj -c Release
copy examples\SampleClockCard\bin\Release\net10.0-windows\SampleClockCard.dll src\ModernBoxes.Desktop\bin\Debug\net10.0-windows\Plugins\
```

重启木函，在卡片管理里启用「示例时钟」。

## 版本兼容

- 宿主 API 版本：`ICardViewModel.HostApiVersion`（当前为 1）
- 插件可通过 `MinHostApiVersion` 声明最低要求；版本不匹配时插件会被跳过并提示

## 示例

见 [`examples/SampleClockCard/`](../examples/SampleClockCard/)。
