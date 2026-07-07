using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModernBoxes.Infrastructure.Plugins
{
    internal static class FlowPluginPaths
    {
        public static IEnumerable<string> DiscoverFlowPluginRoots()
        {
            var roots = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FlowLauncher", "Plugins"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowLauncher", "Plugins"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Flow Launcher", "Plugins"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wox", "Plugins"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wox", "Plugins"),
            };

            return roots.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<string> EnumerateInstalledPluginFolders(string flowRoot) =>
            Directory.EnumerateDirectories(flowRoot);
    }
}
