using ModernBoxes.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernBoxes.Core.Interfaces
{
    public interface ISearchService
    {
        Task<List<SearchResultModel>> SearchAsync(string query);
        void RecordSelection(SearchResultModel result);
    }
}
