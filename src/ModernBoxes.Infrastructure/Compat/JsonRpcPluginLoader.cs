using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Infrastructure.Plugins;
using ModernBoxes.Sdk.Search;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModernBoxes.Infrastructure.Compat
{
    public static class JsonRpcPluginLoader
    {
        public static IReadOnlyList<string> LoadFailures { get; private set; } = Array.Empty<string>();

        public static void RegisterSearchPlugins(IServiceCollection services, string? pluginDir = null)
        {
            // ponytail: 保留兼容；实际加载由 SearchPluginReloadService 负责
        }

        public static IReadOnlyList<ISearchPlugin> LoadSearchPlugins(
            string? pluginDir,
            List<string> failures)
        {
            pluginDir ??= AppPaths.Plugins;
            var plugins = new List<ISearchPlugin>();

            if (!Directory.Exists(pluginDir))
                return plugins;

            foreach (var pluginFolder in Directory.EnumerateDirectories(pluginDir))
            {
                var manifest = PluginManifestReader.TryRead(Path.Combine(pluginFolder, "plugin.json"));
                if (manifest == null || !string.Equals(manifest.Type, "jsonrpc", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (manifest.MinHostApiVersion > CardPluginLoader.HostApiVersion)
                {
                    failures.Add($"{manifest.Id} (API {manifest.MinHostApiVersion})");
                    continue;
                }

                var mainPath = Path.Combine(pluginFolder, manifest.Main);
                if (!File.Exists(mainPath))
                {
                    failures.Add($"{manifest.Id ?? Path.GetFileName(pluginFolder)} (missing {manifest.Main})");
                    continue;
                }

                try
                {
                    plugins.Add(new JsonRpcSearchPlugin(manifest, pluginFolder));
                }
                catch (Exception ex)
                {
                    failures.Add(manifest.Id ?? Path.GetFileName(pluginFolder));
                    Logger.Warn($"Failed to load jsonrpc plugin in {pluginFolder}: {ex.Message}");
                }
            }

            return plugins;
        }
    }
}
