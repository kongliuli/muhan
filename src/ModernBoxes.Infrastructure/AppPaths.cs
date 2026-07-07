using System;
using System.IO;

namespace ModernBoxes.Infrastructure
{
    /// <summary>
    /// 统一应用路径：用户数据在 %APPDATA%，插件与程序资源在安装目录。
    /// 便携版（portable.marker）数据在安装目录 Data/ 下，且默认关闭更新。
    /// </summary>
    public static class AppPaths
    {
        public static string InstallDir => AppContext.BaseDirectory;

        internal static bool? PortableModeOverride { get; set; }

        public static bool IsPortableMode => PortableModeOverride ?? DetectPortable();

        public static bool UpdatesDisabled =>
            IsPortableMode ||
            string.Equals(
                Environment.GetEnvironmentVariable("MODERNBOXES_NO_UPDATE"),
                "1",
                StringComparison.Ordinal);

        public static string Root => IsPortableMode
            ? Path.Combine(InstallDir, "Data")
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ModernBoxes");

        public static string Config(string fileName) => Path.Combine(Root, fileName);

        public static string Settings => Path.Combine(Root, "settings.json");

        public static string Icons => Path.Combine(Root, "icons");

        public static string DirCache => Path.Combine(Root, "DirCache");

        public static string FileCache => Path.Combine(Root, "FileCache");

        public static string Plugins => Path.Combine(InstallDir, "Plugins");

        public static string Executable => Path.Combine(InstallDir, "ModernBoxes.exe");

        public static string LegacyDataDir => InstallDir;

        private static bool DetectPortable()
        {
            if (string.Equals(
                    Environment.GetEnvironmentVariable("MODERNBOXES_PORTABLE"),
                    "1",
                    StringComparison.Ordinal))
                return true;

            return File.Exists(Path.Combine(InstallDir, "portable.marker"));
        }
    }
}
