using Microsoft.Extensions.Hosting;
using ModernBoxes.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Desktop.HostedServices
{
    public class TrayHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                if (mainWindow == null)
                {
                    Logger.Warn("TrayHostedService: MainWindow not available yet");
                    return;
                }

                var notifyIcon = mainWindow.FindName("NotifyIconContextContent");
                if (notifyIcon == null)
                    Logger.Warn("TrayHostedService: NotifyIconContextContent not found");
                else
                    Logger.Info("TrayHostedService: system tray ready");
            }).Task;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                {
                    var notifyIcon = mainWindow.FindName("NotifyIconContextContent");
                    if (notifyIcon is IDisposable disposable)
                        disposable.Dispose();
                }
            });
            return Task.CompletedTask;
        }
    }
}
