using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.HostedServices
{
    public class AutoUpdateService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;
                var checker = new UpdateChecker();
                await checker.CheckForUpdatesAsync();
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
