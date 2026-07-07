namespace ModernBoxes.Infrastructure.Compat
{
    /// <summary>Desktop 启动时注入 QuickLaunch / Search 回调，供 IPublicAPI 使用。</summary>
    public sealed class SearchHostContext
    {
        public Action? ShowQuickLaunch { get; set; }
        public Action? HideQuickLaunch { get; set; }
        public Action<string, bool>? ChangeQuery { get; set; }
    }
}
