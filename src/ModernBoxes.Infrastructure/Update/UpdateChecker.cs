using ModernBoxes.Core.Interfaces;
using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure
{
    public class UpdateChecker
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/kongliuli/muhan/releases/latest";
        private static readonly HttpClient _httpClient = new();
        private readonly IUserNotifier _notifier;

        public UpdateChecker(IUserNotifier notifier)
        {
            _notifier = notifier;
        }

        static UpdateChecker()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ModernBoxes-UpdateChecker");
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(GitHubApiUrl);
                using var doc = JsonDocument.Parse(json);
                var tagName = doc.RootElement.GetProperty("tag_name").GetString();
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

                if (tagName != null && IsNewerVersion(tagName.TrimStart('v'), currentVersion))
                {
                    var htmlUrl = doc.RootElement.GetProperty("html_url").GetString();
                    var body = doc.RootElement.GetProperty("body").GetString();

                    var go = _notifier.ShowConfirm(
                        "ModernBoxes - 发现新版本",
                        $"发现新版本: {tagName}\n\n{body}\n\n是否前往下载更新？");
                    if (go && htmlUrl != null)
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(htmlUrl) { UseShellExecute = true });
                    return true;
                }
            }
            catch
            {
                // Silent fail — don't bother user if update check fails
            }
            return false;
        }

        private static bool IsNewerVersion(string latest, string current)
        {
            if (Version.TryParse(latest, out var lv) && Version.TryParse(current, out var cv))
                return lv > cv;
            return false;
        }
    }
}
