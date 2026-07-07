using ModernBoxes.Infrastructure.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Plugins
{
    public sealed class PluginCatalogService
    {
        private static readonly string[] StoreManifestUrls =
        {
            "https://cdn.jsdelivr.net/gh/Flow-Launcher/Flow.Launcher.PluginsManifest@plugin_api_v2/plugins.json",
            "https://raw.githubusercontent.com/Flow-Launcher/Flow.Launcher.PluginsManifest/plugin_api_v2/plugins.json",
        };

        private readonly HttpClient _http;
        private readonly SearchPluginReloadService _reload;
        private List<FlowStoreEntry>? _cachedStore;

        public PluginCatalogService(SearchPluginReloadService reload)
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _reload = reload;
        }

        public IReadOnlyList<string> DiscoverLocalFlowRoots() =>
            FlowPluginPaths.DiscoverFlowPluginRoots().ToList();

        public PluginInstallResult ImportFromLocalFlowInstallations()
        {
            Directory.CreateDirectory(AppPaths.Plugins);
            var imported = 0;
            string? lastName = null;
            foreach (var root in FlowPluginPaths.DiscoverFlowPluginRoots())
            {
                foreach (var folder in FlowPluginPaths.EnumerateInstalledPluginFolders(root))
                {
                    if (TryImportDirectory(folder, overwrite: false, out var name))
                    {
                        imported++;
                        lastName = name;
                    }
                }
            }

            if (imported > 0)
                _reload.Reload();

            return new PluginInstallResult
            {
                Success = imported > 0,
                PluginName = imported == 1 ? lastName : imported > 1 ? $"{imported} 个插件" : null,
                Reloaded = imported > 0,
            };
        }

        public async Task<IReadOnlyList<FlowStoreEntry>> FetchFlowStoreCatalogAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedStore != null)
                return _cachedStore;

            foreach (var url in StoreManifestUrls)
            {
                try
                {
                    var json = await _http.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
                    var items = JsonConvert.DeserializeObject<List<FlowStoreEntry>>(json) ?? new List<FlowStoreEntry>();
                    if (items.Count > 0)
                    {
                        _cachedStore = items
                            .Where(p => !string.IsNullOrWhiteSpace(p.UrlDownload))
                            .Where(p => p.IsCSharp)
                            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                            .ToList();
                        return _cachedStore;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Flow store manifest fetch failed: {url}. {ex.Message}");
                }
            }

            return Array.Empty<FlowStoreEntry>();
        }

        /// <summary>下载 zip → 解压 → 写入 Plugins/ + plugin.json → 热重载搜索插件。</summary>
        public async Task<PluginInstallResult> InstallFromFlowStoreAsync(
            FlowStoreEntry entry,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entry.UrlDownload))
                return new PluginInstallResult { Success = false, PluginName = entry.Name, ErrorMessage = "该插件没有提供下载地址" };

            Directory.CreateDirectory(AppPaths.Plugins);
            var tempRoot = Path.Combine(Path.GetTempPath(), "muhan-plugin-" + Guid.NewGuid().ToString("N"));
            var zipPath = Path.Combine(tempRoot, "plugin.zip");
            Directory.CreateDirectory(tempRoot);

            try
            {
                var bytes = await _http.GetByteArrayAsync(entry.UrlDownload, cancellationToken).ConfigureAwait(false);
                await File.WriteAllBytesAsync(zipPath, bytes, cancellationToken).ConfigureAwait(false);

                var extractDir = Path.Combine(tempRoot, "extract");
                ZipFile.ExtractToDirectory(zipPath, extractDir);

                var contentDir = FindPluginContentRoot(extractDir);
                if (!InstallFromFlowStoreContent(contentDir, entry, out var manifest))
                    return new PluginInstallResult { Success = false, PluginName = entry.Name, ErrorMessage = "插件包里没有可识别的 plugin.json 或主程序 DLL" };

                _reload.Reload();
                return new PluginInstallResult
                {
                    Success = true,
                    PluginId = manifest!.Id,
                    PluginName = manifest.Name ?? entry.Name,
                    Reloaded = true,
                };
            }
            catch (Exception ex)
            {
                Logger.Warn($"Install Flow plugin failed: {entry.Name}. {ex.Message}");
                return new PluginInstallResult { Success = false, PluginName = entry.Name, ErrorMessage = ex.Message };
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempRoot))
                        Directory.Delete(tempRoot, recursive: true);
                }
                catch
                {
                    // ponytail: 临时目录清理失败可忽略
                }
            }
        }

        public bool TryImportDirectory(string sourceDirectory, bool overwrite) =>
            TryImportDirectory(sourceDirectory, overwrite, out _);

        private bool TryImportDirectory(string sourceDirectory, bool overwrite, out string? pluginName)
        {
            pluginName = null;
            if (!Directory.Exists(sourceDirectory))
                return false;

            var manifest = FlowManifestConverter.TryFromFlowDirectory(sourceDirectory);
            if (manifest == null || !string.Equals(manifest.Type, "wox", StringComparison.OrdinalIgnoreCase))
                return false;

            var targetDir = Path.Combine(AppPaths.Plugins, manifest.Id);
            if (Directory.Exists(targetDir) && !overwrite)
                return false;

            CopyDirectory(sourceDirectory, targetDir, overwrite);
            FlowManifestConverter.WriteMuhanManifest(targetDir, manifest);
            pluginName = manifest.Name ?? manifest.Id;
            return true;
        }

        private static bool InstallFromFlowStoreContent(
            string contentDir,
            FlowStoreEntry entry,
            out PluginManifest? manifest)
        {
            manifest = FlowManifestConverter.TryFromFlowStoreEntry(entry, contentDir);
            if (manifest == null)
                return false;

            var targetDir = Path.Combine(AppPaths.Plugins, manifest.Id);
            CopyDirectory(contentDir, targetDir, overwrite: true);
            FlowManifestConverter.WriteMuhanManifest(targetDir, manifest);
            return true;
        }

        private static string FindPluginContentRoot(string extractDir)
        {
            if (File.Exists(Path.Combine(extractDir, "plugin.json"))
                || Directory.EnumerateFiles(extractDir, "*.dll").Any())
                return extractDir;

            var nested = Directory.EnumerateDirectories(extractDir).FirstOrDefault();
            return nested ?? extractDir;
        }

        private static void CopyDirectory(string sourceDir, string targetDir, bool overwrite)
        {
            if (Directory.Exists(targetDir))
            {
                if (!overwrite)
                    return;
                Directory.Delete(targetDir, recursive: true);
            }

            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(sourceDir, file);
                var dest = Path.Combine(targetDir, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(file, dest, overwrite: true);
            }
        }
    }
}
