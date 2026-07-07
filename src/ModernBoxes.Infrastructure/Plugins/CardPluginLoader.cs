using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Sdk.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ModernBoxes.Infrastructure.Plugins
{
    public static class CardPluginLoader
    {
        private static readonly List<CardPluginMetadata> _availableCards = new();
        private static readonly List<Assembly> _loadedAssemblies = new();
        private static readonly List<CollectiblePluginLoadContext> _loadContexts = new();
        private static readonly Dictionary<Assembly, PluginManifest> _manifestByAssembly = new();
        private static readonly Dictionary<Assembly, string> _directoryByAssembly = new();
        private static readonly object _lock = new();

        public const int HostApiVersion = 1;

        /// <summary>启动时加载失败的插件标识，供 Desktop 层通知用户。</summary>
        public static IReadOnlyList<string> PluginLoadFailures { get; private set; } = Array.Empty<string>();

        public record CardPluginMetadata(
            string CardName,
            string? Author,
            string? Version,
            string? Description,
            Type ViewModelType,
            Type CardType,
            Type? ViewType,
            string? PluginId = null,
            string? PluginDirectory = null
        );

        public static IReadOnlyList<CardPluginMetadata> GetAvailableCards()
        {
            lock (_lock)
            {
                return _availableCards.AsReadOnly();
            }
        }

        public static void MergePluginResourceDictionaries()
        {
            MergePluginResourceDictionaries(_loadedAssemblies);
        }

        public static void DiscoverAndRegister(
            IServiceCollection services,
            Assembly? cardsAssembly = null,
            string? pluginDir = null)
        {
            var discoveredTypes = new HashSet<Type>();
            var failedPlugins = new List<string>();

            _loadedAssemblies.Clear();
            _loadContexts.Clear();
            _manifestByAssembly.Clear();
            _directoryByAssembly.Clear();
            _loadedAssemblies.Add(Assembly.GetExecutingAssembly());

            ScanAssembly(Assembly.GetExecutingAssembly(), discoveredTypes);
            if (cardsAssembly != null)
            {
                ScanAssembly(cardsAssembly, discoveredTypes);
                _loadedAssemblies.Add(cardsAssembly);
            }

            pluginDir ??= AppPaths.Plugins;
            DiscoverExternalPlugins(pluginDir, discoveredTypes, failedPlugins);

            var metadataList = new List<CardPluginMetadata>();
            foreach (var type in discoveredTypes)
            {
                var attr = type.GetCustomAttribute<CardExportAttribute>();
                if (attr == null)
                    continue;

                if (attr.MinHostApiVersion > HostApiVersion)
                {
                    Logger.Warn($"Skipped plugin '{attr.CardName}': requires host API {attr.MinHostApiVersion}, current {HostApiVersion}");
                    failedPlugins.Add($"{attr.CardName} (API {attr.MinHostApiVersion})");
                    continue;
                }

                _manifestByAssembly.TryGetValue(type.Assembly, out var manifest);
                _directoryByAssembly.TryGetValue(type.Assembly, out var pluginDirectory);

                services.AddTransient(type);
                metadataList.Add(new CardPluginMetadata(
                    manifest?.Name ?? attr.CardName,
                    manifest?.Author ?? attr.Author,
                    manifest?.Version ?? attr.Version,
                    manifest?.Description ?? attr.Description,
                    type,
                    type,
                    attr.ViewType,
                    manifest?.Id,
                    pluginDirectory
                ));
            }

            if (failedPlugins.Count > 0)
                PluginLoadFailures = failedPlugins;

            lock (_lock)
            {
                _availableCards.Clear();
                _availableCards.AddRange(metadataList);
            }
        }

        private static void DiscoverExternalPlugins(
            string pluginDir,
            HashSet<Type> discoveredTypes,
            List<string> failedPlugins)
        {
            if (!Directory.Exists(pluginDir))
                return;

            var loadedDllPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pluginFolder in Directory.EnumerateDirectories(pluginDir))
            {
                var manifestPath = Path.Combine(pluginFolder, "plugin.json");
                var manifest = PluginManifestReader.TryRead(manifestPath);
                if (manifest == null)
                    continue;

                if (string.Equals(manifest.Type, "wox", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.Equals(manifest.Type, "jsonrpc", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (manifest.MinHostApiVersion > HostApiVersion)
                {
                    var label = string.IsNullOrWhiteSpace(manifest.Id)
                        ? Path.GetFileName(pluginFolder)
                        : manifest.Id;
                    failedPlugins.Add($"{label} (API {manifest.MinHostApiVersion})");
                    continue;
                }

                var dllPath = Path.Combine(pluginFolder, manifest.Main);
                if (!File.Exists(dllPath))
                {
                    var label = string.IsNullOrWhiteSpace(manifest.Id)
                        ? Path.GetFileName(pluginFolder)
                        : manifest.Id;
                    failedPlugins.Add($"{label} (missing {manifest.Main})");
                    continue;
                }

                if (TryLoadPluginAssembly(dllPath, pluginFolder, manifest, discoveredTypes, failedPlugins))
                    loadedDllPaths.Add(Path.GetFullPath(dllPath));
            }

            // ponytail: 兼容旧版 Plugins/*.dll 平铺部署
            foreach (var dllPath in Directory.EnumerateFiles(pluginDir, "*.dll"))
            {
                if (loadedDllPaths.Contains(Path.GetFullPath(dllPath)))
                    continue;

                TryLoadPluginAssembly(dllPath, Path.GetDirectoryName(dllPath)!, null, discoveredTypes, failedPlugins);
            }
        }

        private static bool TryLoadPluginAssembly(
            string dllPath,
            string pluginDirectory,
            PluginManifest? manifest,
            HashSet<Type> discoveredTypes,
            List<string> failedPlugins)
        {
            try
            {
                var context = new CollectiblePluginLoadContext(dllPath);
                var asm = context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));
                _loadContexts.Add(context);
                _loadedAssemblies.Add(asm);
                if (manifest != null)
                    _manifestByAssembly[asm] = manifest;
                _directoryByAssembly[asm] = pluginDirectory;
                ScanAssembly(asm, discoveredTypes);
                return true;
            }
            catch (Exception ex)
            {
                failedPlugins.Add(manifest?.Id ?? Path.GetFileName(dllPath));
                Logger.Warn($"Failed to load plugin DLL: {dllPath}. {ex.Message}");
                return false;
            }
        }

        private static void MergePluginResourceDictionaries(IEnumerable<Assembly> assemblies)
        {
            if (Application.Current == null)
                return;
            foreach (var asm in assemblies)
            {
                try
                {
                    var resourceName = asm.GetManifestResourceNames()
                        .FirstOrDefault(n => n.EndsWith("CardViews.g.resources", StringComparison.OrdinalIgnoreCase)
                                             || n.EndsWith("CardViews.xaml", StringComparison.OrdinalIgnoreCase));
                    if (resourceName == null)
                        continue;

                    var uri = new Uri($"/{asm.GetName().Name};component/CardViews.xaml", UriKind.Relative);
                    var dict = new ResourceDictionary { Source = uri };
                    System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Plugin resource merge skipped for {asm.GetName().Name}: {ex.Message}");
                }
            }
        }

        private static void ScanAssembly(Assembly asm, HashSet<Type> results)
        {
            try
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    if (type.GetCustomAttribute<CardExportAttribute>() != null
                        && typeof(ICardViewModel).IsAssignableFrom(type))
                    {
                        results.Add(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Logger.Warn($"Type load errors while scanning assembly {asm.GetName().Name}: {ex.Message}");
                foreach (var t in ex.Types)
                {
                    if (t != null && !t.IsAbstract && !t.IsInterface
                        && t.GetCustomAttribute<CardExportAttribute>() != null
                        && typeof(ICardViewModel).IsAssignableFrom(t))
                    {
                        results.Add(t);
                    }
                }
            }
        }
    }
}
