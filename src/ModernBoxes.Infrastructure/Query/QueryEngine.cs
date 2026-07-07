using ModernBoxes.Core.Models;
using ModernBoxes.Sdk.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Query
{
    public sealed class QueryEngine
    {
        private readonly SearchPluginRegistry _registry;
        private readonly FrecencyService _frecency;
        private static readonly TimeSpan PluginTimeout = TimeSpan.FromMilliseconds(800);

        public QueryEngine(SearchPluginRegistry registry, FrecencyService frecency)
        {
            _registry = registry;
            _frecency = frecency;
            _frecency.EnsureSchema();
        }

        public async Task<List<SearchResultModel>> SearchAsync(string rawQuery, CancellationToken cancellationToken = default)
        {
            var query = new Sdk.Search.Query(rawQuery ?? string.Empty);
            if (string.IsNullOrWhiteSpace(query.Search) && string.IsNullOrEmpty(query.ActionKeyword))
                return new List<SearchResultModel>();

            var candidates = new List<(PluginResult Result, ISearchPlugin Plugin)>();

            foreach (var plugin in _registry.Snapshot())
            {
                if (!query.MatchesPlugin(plugin.ActionKeyword))
                    continue;

                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(PluginTimeout);
                    var batch = await plugin.QueryAsync(query, cts.Token).ConfigureAwait(false);
                    foreach (var item in batch)
                        candidates.Add((item, plugin));
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // ponytail: 单插件超时跳过，不拖垮整体
                }
            }

            return candidates
                .Select(c => ToSearchResult(c.Result, c.Plugin))
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Name)
                .ToList();
        }

        public void RecordSelection(SearchResultModel result)
        {
            var key = FrecencyService.MakeKey(result.Type.ToString(), result.Name);
            _frecency.Record(key);
        }

        private SearchResultModel ToSearchResult(PluginResult r, ISearchPlugin plugin)
        {
            if (r.ContextData is SearchResultModel existing)
            {
                var boostKey = FrecencyService.MakeKey(existing.Type.ToString(), existing.Name);
                existing.Score = r.Score + _frecency.GetBoost(boostKey);
                return existing;
            }

            return new SearchResultModel
            {
                Name = r.Title,
                Detail = r.SubTitle,
                IconText = string.IsNullOrEmpty(r.IcoPath) ? "🔌" : r.IcoPath,
                IconPath = r.IcoPath,
                ActionTarget = r.ContextData,
                ExecuteAction = r.Action,
                Score = r.Score,
            };
        }
    }
}
