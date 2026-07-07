using ModernBoxes.Infrastructure.Ai;
using ModernBoxes.Sdk.Host;
using ModernBoxes.Sdk.Plugins;
using System.Diagnostics;

namespace ModernBoxes.Infrastructure.Compat
{
    public sealed class PublicAPIHost : IPublicAPI
    {
        private readonly SearchHostContext _context;
        private readonly IUserNotifier _notifier;
        private readonly ChatClientService _chat;

        public PublicAPIHost(SearchHostContext context, IUserNotifier notifier, ChatClientService chat)
        {
            _context = context;
            _notifier = notifier;
            _chat = chat;
        }

        public void ShowMsg(string title, string message) =>
            _notifier.ShowWarning(title, message);

        public void HideApp() => _context.HideQuickLaunch?.Invoke();

        public void ShowApp() => _context.ShowQuickLaunch?.Invoke();

        public void ChangeQuery(string query, bool requery = true) =>
            _context.ChangeQuery?.Invoke(query, requery);

        public void ShellRun(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }

        public object? GetChatClient() => _chat.GetClient();
    }
}
