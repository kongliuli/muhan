using System;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Reflection;

namespace ModernBoxes.Infrastructure
{
    public class UpdateChecker
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/ModernBoxes/ModernBoxes/releases/latest";
        private static readonly HttpClient _httpClient = new();

        static UpdateChecker()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ModernBoxes-UpdateChecker");
        }

        public async System.Threading.Tasks.Task<bool> CheckForUpdatesAsync()
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

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var result = MessageBox.Show(
                            $"发现新版本: {tagName}\n\n{body}\n\n是否前往下载更新？",
                            "ModernBoxes - 发现新版本",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes && htmlUrl != null)
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(htmlUrl) { UseShellExecute = true });
                    });
                    return true;
                }
            }
            catch
            {
                // Silent fail �?don't bother user if update check fails
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
