using ModernBoxes.Core.Models;
using System.Collections.Generic;

namespace ModernBoxes.Core.Interfaces.Repositories
{
    public interface IApplicationRepository
    {
        void SyncApplications(IEnumerable<ApplicationModel> apps);
        List<ApplicationModel> GetAllApplications();
        List<SearchResultModel> SearchApplications(string query);
        void AddApplication(ApplicationModel app);
        void DeleteApplication(string appPath);
    }
}
