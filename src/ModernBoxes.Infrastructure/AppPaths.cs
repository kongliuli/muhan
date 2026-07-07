using System;
using System.IO;

namespace ModernBoxes.Infrastructure
{
    /// <summary>
    /// 统一应用数据根目录，避免 CurrentDirectory 与 BaseDirectory 混用。
    /// </summary>
    public static class AppPaths
    {
        public static string Root => AppContext.BaseDirectory;

        public static string Config(string fileName) => Path.Combine(Root, fileName);

        public static string Icons => Path.Combine(Root, "icons");

        public static string DirCache => Path.Combine(Root, "DirCache");

        public static string FileCache => Path.Combine(Root, "FileCache");

        public static string Plugins => Path.Combine(Root, "Plugins");

        public static string Executable => Path.Combine(Root, "ModernBoxes.exe");
    }
}
