using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace ModernBoxes.Infrastructure.Services
{
    public class ApplicationCardService : IApplicationCardService
    {
        private readonly IPersistenceProvider _persistence;
        private readonly IApplicationRepository _repository;
        private readonly IIconExtractor _iconExtractor;
        private readonly IProcessLauncher _launcher;

        public ApplicationCardService(
            IPersistenceProvider persistence,
            IApplicationRepository repository,
            IIconExtractor iconExtractor,
            IProcessLauncher launcher)
        {
            _persistence = persistence;
            _repository = repository;
            _iconExtractor = iconExtractor;
            _launcher = launcher;
        }

        public void AddApplication(ApplicationModel app)
        {
            _repository.AddApplication(app);
            PersistSnapshotFireAndForget();
        }

        public void RemoveApplication(string filePath)
        {
            var apps = GetAllApplications();
            var model = apps.FirstOrDefault(o => o.AppPath.Contains(filePath));
            if (model == null)
                return;

            _repository.DeleteApplication(model.AppPath);
            PersistSnapshotFireAndForget();
        }

        public void LaunchApplication(ApplicationModel app) =>
            _launcher.Start(app.AppPath);

        public IList<ApplicationModel> GetAllApplications() =>
            _repository.GetAllApplications();

        public string GetAppIcon(string appPath)
        {
            string iconPath = AppPaths.Icons;
            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}.ico";
            return _iconExtractor.ExtractIconToFile(appPath, iconPath, fileName);
        }

        private void PersistSnapshotFireAndForget()
        {
            var apps = GetAllApplications();
            _ = Task.Run(async () =>
            {
                try
                {
                    await _persistence.SaveAsync("applications", apps).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to persist applications snapshot");
                }
            });
        }
    }
}
