using System;

namespace ModernBoxes.Sdk.Search
{
    /// <summary>插件搜索结果，字段命名对齐 Wox.Plugin.Result。</summary>
    public sealed class PluginResult
    {
        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string IcoPath { get; set; } = string.Empty;
        public int Score { get; set; } = 50;
        public object? ContextData { get; set; }
        public Func<bool>? Action { get; set; }
    }
}
