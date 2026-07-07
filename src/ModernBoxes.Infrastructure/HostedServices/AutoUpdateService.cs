using Microsoft.Extensions.Hosting;
using ModernBoxes.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.HostedServices
{
    public class AutoUpdateService : IHostedService
    {
        private readonly IUserNotifier _notifier;

        public AutoUpdateService(IUserNotifier notifier)
        {
            _notifier = notifier;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;
                var checker = new UpdateChecker(_notifier);
                await checker.CheckForUpdatesAsync();
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
