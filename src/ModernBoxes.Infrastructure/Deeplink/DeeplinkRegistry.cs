namespace ModernBoxes.Infrastructure.Deeplink
{
    public delegate Task DeeplinkHandler(DeeplinkContext context);

    public sealed class DeeplinkRegistry
    {
        private readonly Dictionary<string, DeeplinkHandler> _handlers =
            new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<string> Commands => _handlers.Keys;

        public void Register(string command, DeeplinkHandler handler)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentException("command required", nameof(command));

            _handlers[command] = handler;
        }

        public bool Unregister(string command) => _handlers.Remove(command);

        public async Task<bool> TryDispatchAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (!MuhanUriParser.TryParse(uri.ToString(), out var parsed))
                return false;

            if (!_handlers.TryGetValue(parsed.Host, out var handler))
                return false;

            cancellationToken.ThrowIfCancellationRequested();
            await handler(new DeeplinkContext(parsed)).ConfigureAwait(false);
            return true;
        }

        public Task<bool> TryDispatchAsync(string raw, CancellationToken cancellationToken = default) =>
            MuhanUriParser.TryParse(raw, out var uri)
                ? TryDispatchAsync(uri, cancellationToken)
                : Task.FromResult(false);
    }
}
