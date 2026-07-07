using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace ModernBoxes.Infrastructure.Deeplink
{
    public static class ProtocolRegistration
    {
        public const string Scheme = "muhan";

        public static void RegisterIfNeeded()
        {
            var exe = ResolveExecutablePath();
            if (string.IsNullOrEmpty(exe))
                return;

            using var schemeKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{Scheme}");
            schemeKey.SetValue(string.Empty, $"URL:{Scheme} Protocol");
            schemeKey.SetValue("URL Protocol", string.Empty);

            using var iconKey = schemeKey.CreateSubKey("DefaultIcon");
            iconKey.SetValue(string.Empty, $"\"{exe}\",0");

            using var commandKey = schemeKey.CreateSubKey(@"shell\open\command");
            commandKey.SetValue(string.Empty, $"\"{exe}\" \"%1\"");
        }

        private static string ResolveExecutablePath()
        {
            if (File.Exists(AppPaths.Executable))
                return AppPaths.Executable;

            return Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
        }
    }
}
