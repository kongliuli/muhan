using Newtonsoft.Json;
using System.IO;

namespace ModernBoxes.Infrastructure.Plugins
{
    internal static class PluginManifestReader
    {
        public static PluginManifest? TryRead(string pluginJsonPath)
        {
            if (!File.Exists(pluginJsonPath))
                return null;

            try
            {
                var json = File.ReadAllText(pluginJsonPath);
                var manifest = JsonConvert.DeserializeObject<PluginManifest>(json);
                if (manifest == null || string.IsNullOrWhiteSpace(manifest.Main))
                    return null;
                return manifest;
            }
            catch (JsonException ex)
            {
                Logger.Warn($"Invalid plugin.json: {pluginJsonPath}. {ex.Message}");
                return null;
            }
        }
    }
}
