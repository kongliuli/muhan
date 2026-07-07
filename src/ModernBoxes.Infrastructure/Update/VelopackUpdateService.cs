using Velopack;
using Velopack.Sources;

namespace ModernBoxes.Infrastructure.Update
{
    public sealed class VelopackUpdateService
    {
        public const string PackId = "ModernBoxes";
        public const string GitHubRepoUrl = "https://github.com/kongliuli/muhan";

        public UpdateManager? TryCreateManager()
        {
            if (AppPaths.UpdatesDisabled)
                return null;

            try
            {
                var source = new GithubSource(GitHubRepoUrl, accessToken: null, prerelease: false);
                return new UpdateManager(source);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>静默检查并下载增量包；下次启动时 Velopack 自动应用。</summary>
        public async Task<bool> CheckAndDownloadAsync(CancellationToken cancellationToken = default)
        {
            var mgr = TryCreateManager();
            if (mgr == null || !mgr.IsInstalled)
                return false;

            try
            {
                var update = await mgr.CheckForUpdatesAsync().ConfigureAwait(false);
                if (update == null)
                    return false;

                await mgr.DownloadUpdatesAsync(update).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Velopack update check failed");
                return false;
            }
        }
    }
}
