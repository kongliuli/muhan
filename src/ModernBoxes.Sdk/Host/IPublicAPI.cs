namespace ModernBoxes.Sdk.Host
{
    /// <summary>宿主暴露给插件的 API 子集，对齐 Flow Launcher IPublicAPI 常用能力。</summary>
    public interface IPublicAPI
    {
        void ShowMsg(string title, string message);
        void HideApp();
        void ShowApp();
        void ChangeQuery(string query, bool requery = true);
        void ShellRun(string command);
        object? GetChatClient();
    }
}
