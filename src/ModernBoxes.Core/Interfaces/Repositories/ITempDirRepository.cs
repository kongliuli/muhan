using ModernBoxes.Core.Models;
using System.Collections.Generic;

namespace ModernBoxes.Core.Interfaces.Repositories
{
    public interface ITempDirRepository
    {
        void SyncTempDirs(IEnumerable<TempDirModel> dirs);
        List<SearchResultModel> SearchTempDirs(string query);
    }
}
