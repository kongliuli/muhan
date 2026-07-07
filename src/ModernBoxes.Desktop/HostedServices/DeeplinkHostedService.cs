using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernBoxes.Desktop.Deeplink;
using ModernBoxes.Infrastructure.Deeplink;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Desktop.HostedServices
{
    public sealed class DeeplinkHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly DeeplinkDispatcher _dispatcher;
        private readonly DeeplinkRegistry _registry;

        public DeeplinkHostedService(
            IServiceProvider services,
            DeeplinkDispatcher dispatcher,
            DeeplinkRegistry registry)
        {
            _services = services;
            _dispatcher = dispatcher;
            _registry = registry;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ProtocolRegistration.RegisterIfNeeded();
            DeeplinkBootstrap.RegisterBuiltins(_registry, _services);

            await _dispatcher.DispatchStartupArgsAsync(StartupContext.Args, cancellationToken);

            StartupContext.Gate?.StartPipeServer(forwarded =>
            {
                Application.Current?.Dispatcher.InvokeAsync(async () =>
                    await _dispatcher.DispatchStartupArgsAsync(forwarded));
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
