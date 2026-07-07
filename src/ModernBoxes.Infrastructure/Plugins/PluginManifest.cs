using Newtonsoft.Json;

namespace ModernBoxes.Infrastructure.Plugins
{
    public sealed class PluginManifest
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("author")]
        public string? Author { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("main")]
        public string Main { get; set; } = "";

        [JsonProperty("minHostApiVersion")]
        public int MinHostApiVersion { get; set; } = 1;

        [JsonProperty("type")]
        public string Type { get; set; } = "card";

        [JsonProperty("actionKeyword")]
        public string? ActionKeyword { get; set; }

        /// <summary>jsonrpc 类型：python | 空=直接执行 main</summary>
        [JsonProperty("runtime")]
        public string? Runtime { get; set; }
    }
}
