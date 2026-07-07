using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Sdk.Search
{
    public interface ISearchPlugin
    {
        string Name { get; }
        /// <summary>ActionKeyword，空或 * 表示全局匹配。</summary>
        string ActionKeyword { get; }
        int Priority { get; }

        Task<IReadOnlyList<PluginResult>> QueryAsync(Query query, CancellationToken cancellationToken);
    }
}
