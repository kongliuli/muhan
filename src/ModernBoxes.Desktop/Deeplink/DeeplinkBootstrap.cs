using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Infrastructure.Deeplink;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.ViewModels;
using ModernBoxes.Presentation.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Desktop.Deeplink
{
    internal static class DeeplinkBootstrap
    {
        public static void RegisterBuiltins(DeeplinkRegistry registry, IServiceProvider services)
        {
            var hotkey = services.GetRequiredService<HotkeyViewModel>();
            var search = services.GetRequiredService<SearchViewModel>();

            registry.Register("palette", _ =>
            {
                Application.Current.Dispatcher.Invoke(hotkey.ShowQuickLaunchPalette);
                return Task.CompletedTask;
            });

            registry.Register("search", ctx =>
            {
                var query = ctx.GetArgument();
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    hotkey.ShowQuickLaunchPalette();
                    search.SearchText = query;
                    await search.SearchAsync();
                });
                return Task.CompletedTask;
            });

            registry.Register("note", ctx =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new BaseDialog();
                    dialog.SetTitle("新建便签");
                    dialog.SetHeight(380);
                    dialog.SetContent(new AddNoteDialog());
                    dialog.ShowDialog();
                });
                return Task.CompletedTask;
            });

            registry.Register("show", _ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var main = Application.Current.MainWindow;
                    if (main == null)
                        return;
                    main.Show();
                    main.Activate();
                    main.WindowState = WindowState.Normal;
                });
                return Task.CompletedTask;
            });

            registry.Register("hide", _ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                    Application.Current.MainWindow?.Hide());
                return Task.CompletedTask;
            });
        }
    }
}
