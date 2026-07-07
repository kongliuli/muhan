using Microsoft.Extensions.Hosting;
using ModernBoxes.Infrastructure.Update;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.HostedServices
{
    public class AutoUpdateService : IHostedService
    {
        private readonly VelopackUpdateService _updates;

        public AutoUpdateService(VelopackUpdateService updates)
        {
            _updates = updates;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;
                await _updates.CheckAndDownloadAsync(cancellationToken);
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
