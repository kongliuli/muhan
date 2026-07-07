using System;
using System.IO;

namespace ModernBoxes.Infrastructure.Data
{
    /// <summary>
    /// 一次性将旧版程序目录下的用户数据迁移到 %APPDATA%\ModernBoxes。
    /// </summary>
    public static class DataDirectoryMigrationService
    {
        private static readonly string[] ConfigFiles =
        [
            "AllCardsConfig.json",
            "MenuConfig.json",
            "NotesConfig.json",
            "TempDirConfig.json",
            "TempFileConfig.json",
            "UsedApplicationConfig.json",
        ];

        private static readonly string[] Directories = ["icons", "DirCache", "FileCache"];

        public static void MigrateIfNeeded()
        {
            Directory.CreateDirectory(AppPaths.Root);

            var marker = Path.Combine(AppPaths.Root, ".migrated-from-installdir");
            if (File.Exists(marker))
                return;

            var legacy = AppPaths.LegacyDataDir;
            if (string.Equals(Path.GetFullPath(legacy), Path.GetFullPath(AppPaths.Root), StringComparison.OrdinalIgnoreCase))
            {
                File.WriteAllText(marker, DateTime.UtcNow.ToString("O"));
                return;
            }

            var migrated = false;
            foreach (var file in ConfigFiles)
                migrated |= TryMoveFile(Path.Combine(legacy, file), AppPaths.Config(file));

            foreach (var dir in Directories)
                migrated |= TryMoveDirectory(Path.Combine(legacy, dir), Path.Combine(AppPaths.Root, dir));

            TryMoveFile(Path.Combine(legacy, "OneWordCache.json"), Path.Combine(AppPaths.Root, "OneWordCache.json"));

            // App.config 用户设置
            var legacyConfig = Path.Combine(legacy, "ModernBoxes.dll.config");
            if (!File.Exists(legacyConfig))
                legacyConfig = Path.Combine(legacy, "App.config");
            TryMoveFile(legacyConfig, Path.Combine(AppPaths.Root, "ModernBoxes.dll.config"));

            if (migrated)
                Logger.Info("Migrated user data from install dir to AppData");

            File.WriteAllText(marker, DateTime.UtcNow.ToString("O"));
        }

        private static bool TryMoveFile(string source, string dest)
        {
            if (!File.Exists(source) || File.Exists(dest))
                return false;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Move(source, dest);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Could not migrate file {source}: {ex.Message}");
                return false;
            }
        }

        private static bool TryMoveDirectory(string source, string dest)
        {
            if (!Directory.Exists(source) || Directory.Exists(dest))
                return false;
            try
            {
                Directory.Move(source, dest);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Could not migrate directory {source}: {ex.Message}");
                return false;
            }
        }
    }
}
