namespace ModernBoxes.Infrastructure.Plugins
{
    public sealed class PluginInstallResult
    {
        public bool Success { get; init; }
        public string? PluginId { get; init; }
        public string? PluginName { get; init; }
        public bool Reloaded { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
