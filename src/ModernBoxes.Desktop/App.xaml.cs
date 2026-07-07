using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Sdk.Plugins;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Desktop;
using ModernBoxes.Cards;
using ModernBoxes.Desktop.HostedServices;
using ModernBoxes.Infrastructure;
using ModernBoxes.Infrastructure.Ai;
using ModernBoxes.Infrastructure.Compat;
using ModernBoxes.Infrastructure.Data;
using ModernBoxes.Infrastructure.Data.Repositories;
using ModernBoxes.Infrastructure.HostedServices;
using ModernBoxes.Infrastructure.Platform;
using ModernBoxes.Infrastructure.Plugins;
using ModernBoxes.Infrastructure.Query;
using ModernBoxes.Infrastructure.Services;
using ModernBoxes.Infrastructure.Update;
using ModernBoxes.Infrastructure.Deeplink;
using ModernBoxes.Sdk.Host;
using ModernBoxes.Presentation.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ModernBoxes
{
    public partial class App : System.Windows.Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .UseDefaultServiceProvider((_, options) => options.ValidateOnBuild = true)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<UCusedApplicationViewModel>();
                    services.AddSingleton<UCTempDirectoryViewModel>();
                    services.AddSingleton<UctempFileViewModel>();
                    services.AddSingleton<UCnotesViewModel>();
                    services.AddSingleton<UcComponentViewModel>();
                    services.AddSingleton<UCSetDialogViewModel>();
                    services.AddSingleton<HotkeyViewModel>();
                    services.AddSingleton<BackupViewModel>();
                    services.AddSingleton<SearchViewModel>();
                    services.AddSingleton<OneWordViewModel>();
                    services.AddSingleton<UcManagerCardAppViewModel>();
                    services.AddTransient<AddMenuDialogViewModel>();
                    services.AddTransient<UCAddApplicationDialogViewModel>();
                    services.AddTransient<AddTempDirViewModel>();
                    services.AddTransient<AddTempFileDialogViewModel>();
                    services.AddTransient<UcAddCardAppDialogViewModel>();
                    services.AddTransient<AddNoteDialogViewModel>();
                    services.AddSingleton<IUserNotifier, WpfUserNotifier>();
                    services.AddSingleton<AutoOpenSoftware>(sp =>
                        new AutoOpenSoftware(sp.GetRequiredService<IUserNotifier>()));
                    services.AddSingleton<IIconExtractor, IconExtractor>();
                    services.AddSingleton<IProcessLauncher, ProcessLauncher>();
                    services.AddSingleton(DatabaseService.Instance);
                    services.AddSingleton<IMenuRepository, MenuRepository>();
                    services.AddSingleton<IApplicationRepository, ApplicationRepository>();
                    services.AddSingleton<ITempDirRepository, TempDirRepository>();
                    services.AddSingleton<ITempFileRepository, TempFileRepository>();
                    services.AddSingleton<INoteRepository, NoteRepository>();
                    services.AddSingleton<ICardConfigRepository, CardConfigRepository>();
                    services.AddSingleton<INoteCardService, NoteCardService>();
                    services.AddSingleton<IDirectoryCardService, DirectoryCardService>();
                    services.AddSingleton<IFileCardService, FileCardService>();
                    services.AddSingleton<IApplicationCardService, ApplicationCardService>();
                    services.AddSingleton<PluginCatalogService>();
                    services.AddSingleton<PluginManagerViewModel>();
                    services.AddSingleton<SearchHostContext>();
                    services.AddSingleton<IPublicAPI, PublicAPIHost>();
                    services.AddModernBoxesAi();
                    services.AddModernBoxesQuery();
                    services.AddSingleton<ISearchService, SearchService>();
                    services.AddSingleton<IPersistenceProvider, DualWriteProvider>();
                    services.AddSingleton<IConfigBackupService, ConfigBackupService>();

                    services.AddSingleton<DeeplinkRegistry>();
                    services.AddSingleton<DeeplinkDispatcher>();
                    services.AddSingleton<VelopackUpdateService>();
                    services.AddSingleton<FirstRunSetupService>();
                    services.AddHostedService(sp => sp.GetRequiredService<FirstRunSetupService>());
                    services.AddHostedService<TrayHostedService>();
                    services.AddHostedService<HotkeyHostedService>();
                    services.AddHostedService<EdgeHoverHostedService>();
                    services.AddHostedService<DeeplinkHostedService>();
                    services.AddHostedService<AutoUpdateService>();
                    services.AddHostedService<StartMenuScannerService>();

                    CardPluginLoader.DiscoverAndRegister(services, typeof(NoteCardViewModel).Assembly);
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            GlobalExceptionHandler.Register();

            DataDirectoryMigrationService.MigrateIfNeeded();

            ConfigMigrationService.MigrateIfNeeded(
                AppHost!.Services.GetRequiredService<IConfigBackupService>());

            DatabaseService.Instance.Initialize();
            MigrationService.Migrate(
                AppHost.Services.GetRequiredService<IMenuRepository>(),
                AppHost.Services.GetRequiredService<IApplicationRepository>(),
                AppHost.Services.GetRequiredService<ITempDirRepository>(),
                AppHost.Services.GetRequiredService<ITempFileRepository>(),
                AppHost.Services.GetRequiredService<INoteRepository>(),
                AppHost.Services.GetRequiredService<ICardConfigRepository>());

            await AppHost.Services.GetRequiredService<FirstRunSetupService>().EnsureFirstRunAsync();

            CardPluginLoader.MergePluginResourceDictionaries();

            base.OnStartup(e);

            await AppHost.StartAsync();

            WireSearchHostContext();
            AppHost.Services.GetRequiredService<SearchPluginReloadService>().Reload();
            NotifyPluginLoadFailures();

            Exit += async (_, _) =>
            {
                using var cts = new System.Threading.CancellationTokenSource(5000);
                await AppHost!.StopAsync(cts.Token);
            };
        }

        private static void WireSearchHostContext()
        {
            var ctx = AppHost!.Services.GetRequiredService<SearchHostContext>();
            var hotkey = AppHost.Services.GetRequiredService<HotkeyViewModel>();
            var search = AppHost.Services.GetRequiredService<SearchViewModel>();

            ctx.ShowQuickLaunch = () => Current.Dispatcher.Invoke(hotkey.ShowQuickLaunchPalette);
            ctx.HideQuickLaunch = () => Current.Dispatcher.Invoke(hotkey.HideQuickLaunchPalette);
            ctx.ChangeQuery = (query, requery) => Current.Dispatcher.Invoke(async () =>
            {
                search.SearchText = query;
                if (requery)
                    await search.SearchAsync();
            });
        }

        private static IEnumerable<string> SearchPluginReloadServiceFailures()
        {
            if (AppHost == null)
                yield break;
            foreach (var failure in AppHost.Services.GetRequiredService<SearchPluginReloadService>().LastFailures)
                yield return failure;
        }

        private static void NotifyPluginLoadFailures()
        {
            var failures = CardPluginLoader.PluginLoadFailures
                .Concat(SearchPluginReloadServiceFailures())
                .Distinct()
                .ToList();
            if (failures.Count == 0)
                return;

            var notifier = AppHost!.Services.GetRequiredService<IUserNotifier>();
            notifier.ShowWarning("插件加载", $"以下插件加载失败，已跳过：\n{string.Join("\n", failures)}");
        }
    }
}
