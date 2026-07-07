using ModernBoxes.Infrastructure.Deeplink;

namespace ModernBoxes.Infrastructure.Deeplink
{
    public sealed class DeeplinkDispatcher
    {
        private readonly DeeplinkRegistry _registry;

        public DeeplinkDispatcher(DeeplinkRegistry registry)
        {
            _registry = registry;
        }

        public Task DispatchStartupArgsAsync(IEnumerable<string> args, CancellationToken cancellationToken = default) =>
            DispatchAllAsync(MuhanUriParser.ParseAll(args), cancellationToken);

        public async Task DispatchAllAsync(IEnumerable<Uri> uris, CancellationToken cancellationToken = default)
        {
            foreach (var uri in uris)
            {
                if (!await _registry.TryDispatchAsync(uri, cancellationToken).ConfigureAwait(false))
                    Logger.Warn($"Unknown deeplink command: {uri.Host}");
            }
        }
    }
}
