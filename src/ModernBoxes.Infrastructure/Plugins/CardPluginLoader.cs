using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Interfaces;
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
        private static readonly object _lock = new();

        public const int HostApiVersion = ICardViewModel.HostApiVersion;

        /// <summary>启动时加载失败的插件 DLL 文件名列表，供 Desktop 层通知用户。</summary>
        public static IReadOnlyList<string> PluginLoadFailures { get; private set; } = Array.Empty<string>();

        public record CardPluginMetadata(
            string CardName,
            string? Author,
            string? Version,
            string? Description,
            Type ViewModelType,
            Type CardType,
            Type? ViewType
        );

        public static IReadOnlyList<CardPluginMetadata> GetAvailableCards()
        {
            lock (_lock)
            {
                return _availableCards.AsReadOnly();
            }
        }

        private static readonly List<Assembly> _loadedAssemblies = new();

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
            var loadedAssemblies = _loadedAssemblies;
            loadedAssemblies.Clear();
            loadedAssemblies.Add(Assembly.GetExecutingAssembly());

            ScanAssembly(Assembly.GetExecutingAssembly(), discoveredTypes);
            if (cardsAssembly != null)
            {
                ScanAssembly(cardsAssembly, discoveredTypes);
                loadedAssemblies.Add(cardsAssembly);
            }

            pluginDir ??= AppPaths.Plugins;
            var failedPlugins = new List<string>();
            if (Directory.Exists(pluginDir))
            {
                foreach (var dllPath in Directory.EnumerateFiles(pluginDir, "*.dll"))
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(dllPath);
                        ScanAssembly(asm, discoveredTypes);
                        loadedAssemblies.Add(asm);
                    }
                    catch (Exception ex)
                    {
                        var name = Path.GetFileName(dllPath);
                        failedPlugins.Add(name);
                        Logger.Warn($"Failed to load plugin DLL: {dllPath}. {ex.Message}");
                    }
                }
            }

            if (failedPlugins.Count > 0)
                PluginLoadFailures = failedPlugins;

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

                services.AddTransient(type);
                metadataList.Add(new CardPluginMetadata(
                    attr.CardName,
                    attr.Author,
                    attr.Version,
                    attr.Description,
                    type,
                    type,
                    attr.ViewType
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
