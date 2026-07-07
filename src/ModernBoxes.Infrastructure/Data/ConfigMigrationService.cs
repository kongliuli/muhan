using ModernBoxes.Infrastructure.Data;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace ModernBoxes.Infrastructure.Data
{
    public static class ConfigMigrationService
    {
        private const string OldLayoutKey = "compontentLayout";
        private const string NewLayoutKey = "componentLayout";

        public static void MigrateIfNeeded(IConfigBackupService backupService)
        {
            bool needsLayout = NeedsLayoutMigration();
            bool needsCardsJson = NeedsCardsJsonMigration();
            if (!needsLayout && !needsCardsJson)
                return;

            var backupDir = backupService.CreatePreMigrateBackup();
            if (backupDir == null)
            {
                Logger.Warn("Skipping config migration: backup failed");
                return;
            }

            if (needsLayout && !TryMigrateLayoutKey())
            {
                Logger.Warn("settings.json layout migration failed; originals preserved in backup");
                return;
            }

            if (needsCardsJson && !TryMigrateAllCardsConfig())
            {
                Logger.Warn("AllCardsConfig.json migration failed; originals preserved in backup");
            }
        }

        private static bool NeedsLayoutMigration()
        {
            return !string.IsNullOrEmpty(ConfigHelper.getConfig(OldLayoutKey))
                   && string.IsNullOrEmpty(ConfigHelper.getConfig(NewLayoutKey));
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

        private static bool TryMigrateLayoutKey()
        {
            try
            {
                var old = ConfigHelper.getConfig(OldLayoutKey);
                if (string.IsNullOrEmpty(old))
                    return true;
                ConfigHelper.SetComponentLayout(old);
                Logger.Info($"Migrated settings key {OldLayoutKey} -> {NewLayoutKey}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "settings layout migration error");
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
