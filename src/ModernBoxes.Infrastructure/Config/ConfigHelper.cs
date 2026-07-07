using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModernBoxes.Infrastructure
{
    public class ConfigHelper
    {
        private static readonly object _lock = new();
        private static Dictionary<string, string>? _settings;

        public const string ComponentLayoutKey = "componentLayout";
        private const string LegacyComponentLayoutKey = "compontentLayout";

        private static Dictionary<string, string> Settings
        {
            get
            {
                lock (_lock)
                {
                    if (_settings != null)
                        return _settings;

                    Directory.CreateDirectory(AppPaths.Root);
                    _settings = LoadSettingsFile();
                    MigrateLegacyAppConfig(_settings);
                    return _settings;
                }
            }
        }

        public static void setConfig(string key, object value)
        {
            lock (_lock)
            {
                Settings[key] = value.ToString() ?? "";
                SaveSettingsFile(Settings);
            }
        }

        public static string getConfig(string key)
        {
            lock (_lock)
            {
                return Settings.TryGetValue(key, out var value) ? value : string.Empty;
            }
        }

        public static string GetComponentLayout()
        {
            var value = getConfig(ComponentLayoutKey);
            if (string.IsNullOrEmpty(value))
                value = getConfig(LegacyComponentLayoutKey);
            return value;
        }

        public static void SetComponentLayout(object value)
        {
            setConfig(ComponentLayoutKey, value);
            lock (_lock)
            {
                if (Settings.Remove(LegacyComponentLayoutKey))
                    SaveSettingsFile(Settings);
            }
        }

        private static Dictionary<string, string> LoadSettingsFile()
        {
            var path = AppPaths.Settings;
            if (!File.Exists(path))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var json = File.ReadAllText(path);
                var obj = JObject.Parse(json);
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in obj.Properties())
                    dict[prop.Name] = prop.Value.Type == JTokenType.Null ? "" : prop.Value.ToString();
                return dict;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load settings.json: {ex.Message}");
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void SaveSettingsFile(Dictionary<string, string> settings)
        {
            var path = AppPaths.Settings;
            var temp = path + ".tmp";
            var obj = new JObject();
            foreach (var kv in settings)
                obj[kv.Key] = kv.Value;
            File.WriteAllText(temp, obj.ToString());
            File.Move(temp, path, overwrite: true);
        }

        private static void MigrateLegacyAppConfig(Dictionary<string, string> settings)
        {
            var legacyPath = Path.Combine(AppPaths.Root, "ModernBoxes.dll.config");
            if (!File.Exists(legacyPath))
                return;

            try
            {
                var map = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(
                    new System.Configuration.ExeConfigurationFileMap { ExeConfigFilename = legacyPath },
                    System.Configuration.ConfigurationUserLevel.None);

                foreach (System.Configuration.KeyValueConfigurationElement item in map.AppSettings.Settings)
                {
                    if (!settings.ContainsKey(item.Key))
                        settings[item.Key] = item.Value ?? "";
                }

                SaveSettingsFile(settings);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Legacy App.config migration skipped: {ex.Message}");
            }
        }
    }
}
