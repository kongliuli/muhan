using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Infrastructure.Plugins;
using ModernBoxes.Sdk.Host;
using ModernBoxes.Sdk.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModernBoxes.Infrastructure.Compat
{
    public static class WoxPluginLoader
    {
        public static IReadOnlyList<string> LoadFailures { get; private set; } = Array.Empty<string>();

        public static void RegisterSearchPlugins(IServiceCollection services, string? pluginDir = null)
        {
        }

        public static IReadOnlyList<ISearchPlugin> LoadSearchPlugins(
            IPublicAPI api,
            string? pluginDir,
            List<string> failures)
        {
            pluginDir ??= AppPaths.Plugins;
            var plugins = new List<ISearchPlugin>();

            foreach (var entry in Discover(pluginDir, failures))
            {
                var adapter = new WoxPluginAdapter(
                    entry.Instance,
                    entry.QueryMethod,
                    entry.Name,
                    entry.ActionKeyword,
                    entry.Assembly);
                adapter.EnsureInitialized(api);
                plugins.Add(adapter);
            }

            return plugins;
        }

        private sealed record WoxPluginEntry(
            object Instance,
            MethodInfo QueryMethod,
            string Name,
            string ActionKeyword,
            Assembly Assembly);

        private static IEnumerable<WoxPluginEntry> Discover(string pluginDir, List<string> failures)
        {
            if (!Directory.Exists(pluginDir))
                yield break;

            foreach (var pluginFolder in Directory.EnumerateDirectories(pluginDir))
            {
                var manifest = PluginManifestReader.TryRead(Path.Combine(pluginFolder, "plugin.json"));
                if (manifest == null || !string.Equals(manifest.Type, "wox", StringComparison.OrdinalIgnoreCase))
                    continue;

                var dllPath = Path.Combine(pluginFolder, manifest.Main);
                if (!File.Exists(dllPath))
                {
                    failures.Add($"{manifest.Id ?? Path.GetFileName(pluginFolder)} (missing {manifest.Main})");
                    continue;
                }

                foreach (var entry in LoadFromAssembly(dllPath, manifest, failures))
                    yield return entry;
            }
        }

        private static List<WoxPluginEntry> LoadFromAssembly(
            string dllPath,
            PluginManifest manifest,
            List<string> failures)
        {
            var entries = new List<WoxPluginEntry>();
            try
            {
                var context = new CollectiblePluginLoadContext(dllPath);
                var asm = context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));
                foreach (var type in asm.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                        continue;

                    if (!LooksLikeWoxPlugin(type))
                        continue;

                    var query = type.GetMethod("Query", BindingFlags.Instance | BindingFlags.Public);
                    if (query == null)
                        continue;

                    var instance = Activator.CreateInstance(type);
                    if (instance == null)
                        continue;

                    var name = manifest.Name
                        ?? type.GetProperty("Name")?.GetValue(instance)?.ToString()
                        ?? manifest.Id
                        ?? type.Name;

                    entries.Add(new WoxPluginEntry(
                        instance,
                        query,
                        name,
                        manifest.ActionKeyword ?? string.Empty,
                        asm));
                }
            }
            catch (Exception ex)
            {
                failures.Add(manifest.Id ?? Path.GetFileName(dllPath));
                Logger.Warn($"Failed to load Wox plugin: {dllPath}. {ex.Message}");
            }

            return entries;
        }

        private static bool LooksLikeWoxPlugin(Type type)
        {
            if (type.GetInterfaces().Any(i => i.FullName == "Wox.Plugin.IPlugin"))
                return true;

            return type.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public) != null
                   && type.GetMethod("Query", BindingFlags.Instance | BindingFlags.Public) != null;
        }
    }
}
