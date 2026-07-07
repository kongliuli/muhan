using Microsoft.Extensions.Hosting;
using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.HostedServices
{
    public class FirstRunSetupService : IHostedService
    {
        private readonly IPersistenceProvider _persistence;
        private static bool _setupDone;

        public FirstRunSetupService(IPersistenceProvider persistence)
        {
            _persistence = persistence;
        }

        public async Task EnsureFirstRunAsync()
        {
            if (_setupDone)
                return;

            try
            {
                var cardsPath = AppPaths.Config("AllCardsConfig.json");
                if (!File.Exists(cardsPath))
                    return;

                var config = await FileHelper.ReadFile(cardsPath);
                if (ConfigHelper.getConfig("isFirst") != string.Empty && config.Length >= 8)
                {
                    _setupDone = true;
                    return;
                }

                ConfigHelper.setConfig("isFirst", "true");
                ConfigHelper.SetComponentLayout(CommentLayout.right);
                Directory.CreateDirectory(AppPaths.DirCache);
                Directory.CreateDirectory(AppPaths.FileCache);
                ConfigHelper.setConfig("autoOpen", false);

                var defaultCards = new List<CardContentModel>
                {
                    new() { CardName = "一言", IsChecked = true, CardID = 0, CardHeight = 100, Preview = "/Resource/image/previews/onenote1.png" },
                    new() { CardName = "应用", IsChecked = true, CardID = 1, CardHeight = 235, Preview = "/Resource/image/previews/application.png" },
                    new() { CardName = "文件夹", IsChecked = true, CardID = 2, CardHeight = 235, Preview = "/Resource/image/previews/dir1.png" },
                    new() { CardName = "文件", IsChecked = true, CardID = 3, CardHeight = 235, Preview = "/Resource/image/previews/file1.png" },
                    new() { CardName = "便签", IsChecked = false, CardID = 4, CardHeight = 235, Preview = "/Resource/image/previews/notes1.png" },
                };
                await _persistence.SaveAsync("cardconfigs", defaultCards);

                ConfigHelper.setConfig("theme", Theme.light);
                ConfigHelper.setConfig("IsHover", false);
                ConfigHelper.setConfig("HoverPosition", HoverPosition.LEFT);
                ConfigHelper.setConfig("WindowOpacity", 1);
                ConfigHelper.setConfig("autoOpen", false);

                _setupDone = true;
                Logger.Info("First-run setup completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "First-run setup failed");
                throw;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
