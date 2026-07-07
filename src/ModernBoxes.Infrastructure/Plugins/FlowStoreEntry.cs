using Newtonsoft.Json;

namespace ModernBoxes.Infrastructure.Plugins
{
    public sealed class FlowStoreEntry
    {
        [JsonProperty("ID")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("Description")]
        public string? Description { get; set; }

        [JsonProperty("Author")]
        public string? Author { get; set; }

        [JsonProperty("Version")]
        public string? Version { get; set; }

        [JsonProperty("Language")]
        public string? Language { get; set; }

        [JsonProperty("ActionKeyword")]
        public string? ActionKeyword { get; set; }

        [JsonProperty("UrlDownload")]
        public string? UrlDownload { get; set; }

        [JsonProperty("Website")]
        public string? Website { get; set; }

        public bool IsCSharp => string.Equals(Language, "csharp", System.StringComparison.OrdinalIgnoreCase);
    }
}
