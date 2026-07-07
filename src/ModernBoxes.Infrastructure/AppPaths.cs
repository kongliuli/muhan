using System;
using System.IO;

namespace ModernBoxes.Infrastructure
{
    /// <summary>
    /// 统一应用路径：用户数据在 %APPDATA%，插件与程序资源在安装目录。
    /// </summary>
    public static class AppPaths
    {
        public static string InstallDir => AppContext.BaseDirectory;

        public static string Root => Path.Combine(
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
    }
}
