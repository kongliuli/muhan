using System;
using System.IO;

namespace ModernBoxes.Infrastructure.Data
{
    public class ConfigBackupService : IConfigBackupService
    {
        private static readonly string[] ConfigFiles =
        {
            "AllCardsConfig.json",
            "MenuConfig.json",
            "TempDirConfig.json",
            "TempFileConfig.json",
            "UsedApplicationConfig.json",
            "NotesConfig.json",
            "settings.json"
        };

        private static string AppDir => AppPaths.Root;

        public string? CreatePreMigrateBackup()
        {
            var backupDir = Path.Combine(AppDir, ".backup", $"pre-migrate_{DateTime.Now:yyyyMMdd_HHmmss}");
            try
            {
                Directory.CreateDirectory(backupDir);
                foreach (var file in ConfigFiles)
                {
                    var src = Path.Combine(AppDir, file);
                    if (File.Exists(src))
                        File.Copy(src, Path.Combine(backupDir, file), overwrite: true);
                }
                Logger.Info($"Pre-migrate backup created at {backupDir}");
                return backupDir;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Pre-migrate backup failed");
                return null;
            }
        }
    }
}
