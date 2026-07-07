using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace ModernBoxes.Infrastructure.Data
{
    public static class ConfigMigrationService
    {
        private const string OldLayoutKey = "compontentLayout";
        private const string NewLayoutKey = "componentLayout";

        public static void MigrateIfNeeded(IConfigBackupService backupService)
        {
            bool needsAppConfig = NeedsAppConfigMigration();
            bool needsCardsJson = NeedsCardsJsonMigration();
            if (!needsAppConfig && !needsCardsJson)
                return;

            var backupDir = backupService.CreatePreMigrateBackup();
            if (backupDir == null)
            {
                Logger.Warn("Skipping config migration: backup failed");
                return;
            }

            if (needsAppConfig && !TryMigrateAppConfig())
            {
                Logger.Warn("App.config migration failed; originals preserved in backup");
                return;
            }

            if (needsCardsJson && !TryMigrateAllCardsConfig())
            {
                Logger.Warn("AllCardsConfig.json migration failed; originals preserved in backup");
            }
        }

        private static bool NeedsAppConfigMigration()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings[OldLayoutKey] != null
                   && config.AppSettings.Settings[NewLayoutKey] == null;
        }

        private static bool NeedsCardsJsonMigration()
        {
            var path = AppPaths.Config("AllCardsConfig.json");
            if (!File.Exists(path))
                return false;
            try
            {
                var text = File.ReadAllText(path);
                if (text.Length <= 8)
                    return false;
                return text.Contains("\"Priview\"", StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryMigrateAppConfig()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var old = config.AppSettings.Settings[OldLayoutKey];
                if (old == null)
                    return true;

                var value = old.Value;
                if (config.AppSettings.Settings[NewLayoutKey] == null)
                    config.AppSettings.Settings.Add(NewLayoutKey, value);
                else
                    config.AppSettings.Settings[NewLayoutKey].Value = value;

                config.AppSettings.Settings.Remove(OldLayoutKey);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                Logger.Info($"Migrated App.config key {OldLayoutKey} -> {NewLayoutKey}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "App.config migration error");
                return false;
            }
        }

        private static bool TryMigrateAllCardsConfig()
        {
            var path = AppPaths.Config("AllCardsConfig.json");
            var tempPath = path + ".migrate.tmp";
            try
            {
                var text = File.ReadAllText(path);
                var array = JArray.Parse(text);
                foreach (var token in array.Children<JObject>())
                {
                    if (token["Priview"] != null && token["Preview"] == null)
                    {
                        token["Preview"] = token["Priview"];
                        token.Remove("Priview");
                    }
                }

                File.WriteAllText(tempPath, array.ToString());
                File.Move(tempPath, path, overwrite: true);
                Logger.Info("Migrated AllCardsConfig.json Priview -> Preview");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "AllCardsConfig.json migration error");
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { /* ponytail: best-effort cleanup */ }
                }
                return false;
            }
        }
    }
}
