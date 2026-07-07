using ModernBoxes.Sdk.Host;

namespace ModernBoxes.Infrastructure.Compat
{
    /// <summary>方法签名对齐 Wox.Plugin.PublicAPI，供反射 Init 注入。</summary>
    public sealed class WoxPublicApiBridge
    {
        private readonly IPublicAPI _host;

        public WoxPublicApiBridge(IPublicAPI host)
        {
            _host = host;
        }

        public void ShowMsg(string title, string subTitle, string icon = "") =>
            _host.ShowMsg(title, subTitle);

        public void ShowStatus(string status) =>
            _host.ShowMsg("木函", status);

        public void HideMainWindow() => _host.HideApp();

        public void ShowMainWindow() => _host.ShowApp();

        public void ChangeQuery(string query, bool select = true) =>
            _host.ChangeQuery(query, select);

        public void ShellOpen(string command) => _host.ShellRun(command);

        public void ShellRun(string command) => _host.ShellRun(command);
    }
}
