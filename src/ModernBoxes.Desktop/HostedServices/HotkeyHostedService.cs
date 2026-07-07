using Microsoft.Extensions.Hosting;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Desktop.HostedServices
{
    public class HotkeyHostedService : IHostedService
    {
        private readonly HotkeyViewModel _hotkeyViewModel;

        public HotkeyHostedService(HotkeyViewModel hotkeyViewModel)
        {
            _hotkeyViewModel = hotkeyViewModel;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _hotkeyViewModel.RegisterAll();
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            HotkeyManager.Instance.UnregisterAll();
            return Task.CompletedTask;
        }
    }
}
