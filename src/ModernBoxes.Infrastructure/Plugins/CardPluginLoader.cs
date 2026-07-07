using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModernBoxes.Infrastructure.Plugins
{
    public static class CardPluginLoader
    {
        private static readonly List<CardPluginMetadata> _availableCards = new();
        private static readonly object _lock = new();

        /// <summary>启动时加载失败的插件 DLL 文件名列表，供 Desktop 层通知用户。</summary>
        public static IReadOnlyList<string> PluginLoadFailures { get; private set; } = Array.Empty<string>();

        public record CardPluginMetadata(
            string CardName,
            string? Author,
            string? Version,
            string? Description,
            Type ViewModelType,
            Type CardType
        );

        public static IReadOnlyList<CardPluginMetadata> GetAvailableCards()
        {
            lock (_lock)
            {
                return _availableCards.AsReadOnly();
            }
        }

        public static void DiscoverAndRegister(
            IServiceCollection services,
            Assembly? cardsAssembly = null,
            string? pluginDir = null)
        {
            var discoveredTypes = new HashSet<Type>();

            ScanAssembly(Assembly.GetExecutingAssembly(), discoveredTypes);
            if (cardsAssembly != null)
                ScanAssembly(cardsAssembly, discoveredTypes);

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
                services.AddTransient(type);

                var attr = type.GetCustomAttribute<CardExportAttribute>();
                if (attr != null)
                {
                    metadataList.Add(new CardPluginMetadata(
                        attr.CardName,
                        attr.Author,
                        attr.Version,
                        attr.Description,
                        type,
                        type
                    ));
                }
            }

            lock (_lock)
            {
                _availableCards.AddRange(metadataList);
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
