using ModernBoxes.Core.Models;
using System.Collections.Generic;

namespace ModernBoxes.Core.Interfaces.Repositories
{
    public interface ITempFileRepository
    {
        void SyncTempFiles(IEnumerable<TempFileModel> files);
        List<SearchResultModel> SearchTempFiles(string query);
    }
}
