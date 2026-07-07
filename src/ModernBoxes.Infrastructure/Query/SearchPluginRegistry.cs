using ModernBoxes.Sdk.Search;
using System.Collections.Generic;
using System.Linq;

namespace ModernBoxes.Infrastructure.Query
{
    public sealed class SearchPluginRegistry
    {
        private readonly object _lock = new();
        private IReadOnlyList<ISearchPlugin> _plugins = new List<ISearchPlugin>();

        public IReadOnlyList<ISearchPlugin> Snapshot()
        {
            lock (_lock)
                return _plugins;
        }

        public void Replace(IEnumerable<ISearchPlugin> plugins)
        {
            lock (_lock)
                _plugins = plugins.OrderByDescending(p => p.Priority).ToList();
        }
    }
}
