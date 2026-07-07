using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace ModernBoxes.Infrastructure.Plugins
{
    internal static class FlowManifestConverter
    {
        public static PluginManifest? TryFromFlowDirectory(string pluginDirectory)
        {
            var flowJsonPath = Path.Combine(pluginDirectory, "plugin.json");
            if (!File.Exists(flowJsonPath))
                return null;

            try
            {
                var flow = JObject.Parse(File.ReadAllText(flowJsonPath));
                var mainDll = FindMainDll(pluginDirectory);
                if (mainDll == null)
                    return null;

                var id = flow["ID"]?.ToString();
                if (string.IsNullOrWhiteSpace(id))
                    id = new DirectoryInfo(pluginDirectory).Name;

                var language = flow["Language"]?.ToString() ?? "csharp";
                var type = string.Equals(language, "csharp", StringComparison.OrdinalIgnoreCase) ? "wox" : "jsonrpc";

                return new PluginManifest
                {
                    Id = SanitizeFolderName(id),
                    Name = flow["Name"]?.ToString(),
                    Version = flow["Version"]?.ToString(),
                    Author = flow["Author"]?.ToString(),
                    Description = flow["Description"]?.ToString(),
                    ActionKeyword = flow["ActionKeyword"]?.ToString(),
                    Main = Path.GetFileName(mainDll),
                    Type = type,
                    Runtime = string.Equals(type, "jsonrpc", StringComparison.OrdinalIgnoreCase) ? "python" : null,
                    MinHostApiVersion = 1,
                };
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to convert Flow plugin.json in {pluginDirectory}: {ex.Message}");
                return null;
            }
        }

        public static PluginManifest? TryFromFlowStoreEntry(FlowStoreEntry entry, string installedDirectory)
        {
            var manifest = TryFromFlowDirectory(installedDirectory);
            if (manifest == null)
            {
                manifest = new PluginManifest
                {
                    Id = SanitizeFolderName(entry.Id),
                    Name = entry.Name,
                    Version = entry.Version,
                    Author = entry.Author,
                    Description = entry.Description,
                    ActionKeyword = entry.ActionKeyword,
                    Type = entry.IsCSharp ? "wox" : "jsonrpc",
                    MinHostApiVersion = 1,
                };

                var mainDll = FindMainDll(installedDirectory);
                if (mainDll != null)
                {
                    manifest.Main = Path.GetFileName(mainDll);
                    manifest.Type = "wox";
                }
                else
                {
                    var mainPy = Directory.EnumerateFiles(installedDirectory, "main.py", SearchOption.AllDirectories).FirstOrDefault();
                    if (mainPy != null)
                    {
                        manifest.Main = Path.GetRelativePath(installedDirectory, mainPy).Replace('\\', '/');
                        manifest.Type = "jsonrpc";
                        manifest.Runtime = "python";
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            manifest.Id = SanitizeFolderName(entry.Id);
            if (string.IsNullOrWhiteSpace(manifest.Name))
                manifest.Name = entry.Name;
            if (string.IsNullOrWhiteSpace(manifest.ActionKeyword))
                manifest.ActionKeyword = entry.ActionKeyword;

            return manifest;
        }

        public static void WriteMuhanManifest(string pluginDirectory, PluginManifest manifest)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(manifest, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(pluginDirectory, "plugin.json"), json);
        }

        internal static string SanitizeFolderName(string id)
        {
            var chars = id.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_').ToArray();
            var sanitized = new string(chars);
            return string.IsNullOrWhiteSpace(sanitized) ? "plugin" : sanitized.ToLowerInvariant();
        }

        private static string? FindMainDll(string pluginDirectory)
        {
            return Directory.EnumerateFiles(pluginDirectory, "*.dll", SearchOption.AllDirectories)
                .Where(path => !IsHostOrFrameworkDll(path))
                .OrderByDescending(path => Path.GetFileName(path).StartsWith("Flow.Launcher.Plugin", StringComparison.OrdinalIgnoreCase))
                .ThenBy(path => path.Length)
                .FirstOrDefault();
        }

        private static bool IsHostOrFrameworkDll(string path)
        {
            var name = Path.GetFileName(path);
            return name.StartsWith("ModernBoxes", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("Flow.Launcher.dll", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("Wox.Plugin.dll", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("Wox.Core.dll", StringComparison.OrdinalIgnoreCase);
        }
    }
}
