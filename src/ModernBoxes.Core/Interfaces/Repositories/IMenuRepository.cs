using ModernBoxes.Core.Models;
using System.Collections.Generic;

namespace ModernBoxes.Core.Interfaces.Repositories
{
    public interface IMenuRepository
    {
        void SyncMenus(IEnumerable<MenuModel> menus);
        List<SearchResultModel> SearchMenus(string query);
    }
}
