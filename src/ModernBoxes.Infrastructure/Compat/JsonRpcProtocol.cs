using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ModernBoxes.Infrastructure.Compat
{
    internal static class JsonRpcProtocol
    {
        public static string BuildQueryRequest(int id, string search, string actionKeyword, string rawQuery) =>
            JsonConvert.SerializeObject(new
            {
                jsonrpc = "2.0",
                id,
                method = "query",
                @params = new { search, actionKeyword, rawQuery },
            });

        public static IReadOnlyList<PluginResultDto> ParseQueryResponse(string line)
        {
            var root = JObject.Parse(line);
            if (root["error"] != null)
                throw new InvalidOperationException(root["error"]?["message"]?.ToString() ?? "jsonrpc error");

            var result = root["result"];
            if (result == null)
                return Array.Empty<PluginResultDto>();

            return result.ToObject<List<PluginResultDto>>() ?? new List<PluginResultDto>();
        }
    }

    internal sealed class PluginResultDto
    {
        [JsonProperty("Title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("SubTitle")]
        public string SubTitle { get; set; } = string.Empty;

        [JsonProperty("IcoPath")]
        public string IcoPath { get; set; } = string.Empty;

        [JsonProperty("Score")]
        public int Score { get; set; } = 50;

        [JsonProperty("ActionCommand")]
        public string? ActionCommand { get; set; }
    }
}
