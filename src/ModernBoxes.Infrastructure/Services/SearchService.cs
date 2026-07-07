using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Query;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Services
{
    public class SearchService : ISearchService
    {
        private readonly QueryEngine _engine;

        public SearchService(QueryEngine engine)
        {
            _engine = engine;
        }

        public Task<List<SearchResultModel>> SearchAsync(string query) =>
            _engine.SearchAsync(query, CancellationToken.None);

        public void RecordSelection(SearchResultModel result) =>
            _engine.RecordSelection(result);
    }
}
