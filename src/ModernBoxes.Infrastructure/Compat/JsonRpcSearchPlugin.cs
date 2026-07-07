using ModernBoxes.Infrastructure.Plugins;
using ModernBoxes.Sdk.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Compat
{
    internal sealed class JsonRpcSearchPlugin : ISearchPlugin
    {
        private static int _nextId;
        private readonly PluginManifest _manifest;
        private readonly string _pluginDirectory;
        private readonly ProcessStartInfo _startInfo;

        public JsonRpcSearchPlugin(PluginManifest manifest, string pluginDirectory)
        {
            _manifest = manifest;
            _pluginDirectory = pluginDirectory;
            _startInfo = JsonRpcProcessClient.CreateStartInfo(manifest, pluginDirectory);
        }

        public string Name => _manifest.Name ?? _manifest.Id;
        public string ActionKeyword => _manifest.ActionKeyword ?? string.Empty;
        public int Priority => 35;

        public async Task<IReadOnlyList<PluginResult>> QueryAsync(Sdk.Search.Query query, CancellationToken cancellationToken)
        {
            var id = Interlocked.Increment(ref _nextId);
            var request = JsonRpcProtocol.BuildQueryRequest(
                id,
                query.Search,
                query.ActionKeyword,
                query.Raw);

            try
            {
                var responseLine = await JsonRpcProcessClient.CallAsync(
                    _startInfo,
                    request,
                    TimeSpan.FromMilliseconds(800),
                    cancellationToken).ConfigureAwait(false);

                var items = JsonRpcProtocol.ParseQueryResponse(responseLine);
                return items.Select(MapResult).ToList();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Warn($"JsonRpc plugin '{Name}' query failed: {ex.Message}");
                return Array.Empty<PluginResult>();
            }
        }

        private static PluginResult MapResult(PluginResultDto dto) =>
            new()
            {
                Title = dto.Title,
                SubTitle = dto.SubTitle,
                IcoPath = dto.IcoPath,
                Score = dto.Score,
                Action = string.IsNullOrWhiteSpace(dto.ActionCommand)
                    ? null
                    : () =>
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c {dto.ActionCommand}",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            });
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    },
            };
    }
}
