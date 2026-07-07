using ModernBoxes.Core.Models;
using System.Collections.Generic;

namespace ModernBoxes.Core.Interfaces
{
    public interface IApplicationCardService
    {
        void AddApplication(ApplicationModel app);
        void RemoveApplication(string filePath);
        void LaunchApplication(ApplicationModel app);
        IList<ApplicationModel> GetAllApplications();
        string GetAppIcon(string appPath);
    }
}
