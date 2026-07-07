using Microsoft.Extensions.Hosting;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.HostedServices
{
    public class StartMenuScannerService : IHostedService
    {
        private readonly IApplicationCardService _appService;
        private readonly IPersistenceProvider _persistence;

        public StartMenuScannerService(IApplicationCardService appService, IPersistenceProvider persistence)
        {
            _appService = appService;
            _persistence = persistence;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (ConfigHelper.getConfig("StartMenuScanned") == "true")
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                try
                {
                    await ScanAndImportAsync();
                    ConfigHelper.setConfig("StartMenuScanned", "true");
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Start menu scan failed: {ex.Message}");
                }
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task ScanAndImportAsync()
        {
            var existingPaths = _appService.GetAllApplications()
                .Select(a => a.AppPath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var imported = _appService.GetAllApplications().ToList();
            var added = 0;

            foreach (var (name, path) in EnumerateStartMenuLinks())
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    continue;
                if (!existingPaths.Add(path))
                    continue;

                imported.Add(new ApplicationModel
                {
                    FileName = name,
                    AppPath = path,
                    Icon = string.Empty,
                });
                added++;
            }

            if (added > 0)
            {
                await _persistence.SaveAsync("applications", imported);
                Logger.Info($"Imported {added} start menu applications");
            }
        }

        private static IEnumerable<(string Name, string TargetPath)> EnumerateStartMenuLinks()
        {
            var roots = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
                Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            };

            foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(root))
                    continue;
                foreach (var lnk in Directory.EnumerateFiles(root, "*.lnk", SearchOption.AllDirectories))
                {
                    var target = ResolveShortcutTarget(lnk);
                    if (string.IsNullOrWhiteSpace(target))
                        continue;
                    var name = Path.GetFileNameWithoutExtension(lnk);
                    yield return (name, target);
                }
            }
        }

        private static string? ResolveShortcutTarget(string shortcutPath)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                    return null;
                dynamic shell = Activator.CreateInstance(shellType)!;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                return (string?)shortcut.TargetPath;
            }
            catch
            {
                return null;
            }
        }
    }
}
