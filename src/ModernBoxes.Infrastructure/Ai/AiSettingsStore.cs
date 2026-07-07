using System.IO;
using System.Text.Json;

namespace ModernBoxes.Infrastructure.Ai
{
    public static class AiSettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        internal static string? SettingsPathOverride { get; set; }

        private static string SettingsPath =>
            SettingsPathOverride ?? AppPaths.Config("ai-settings.json");

        public static AiSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return new AiSettings();

                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AiSettings>(json, JsonOptions) ?? new AiSettings();
            }
            catch
            {
                return new AiSettings();
            }
        }

        public static void Save(AiSettings settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
