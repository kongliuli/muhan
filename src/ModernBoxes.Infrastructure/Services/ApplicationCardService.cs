using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ModernBoxes.Infrastructure.Services
{
    public class ApplicationCardService : IApplicationCardService
    {
        private readonly IPersistenceProvider _persistence;
        private readonly IApplicationRepository _repository;
        private readonly IIconExtractor _iconExtractor;

        public ApplicationCardService(
            IPersistenceProvider persistence,
            IApplicationRepository repository,
            IIconExtractor iconExtractor)
        {
            _persistence = persistence;
            _repository = repository;
            _iconExtractor = iconExtractor;
        }

        public void AddApplication(ApplicationModel app)
        {
            var apps = GetAllApplications();
            apps.Add(app);
            _persistence.SaveAsync("applications", apps).GetAwaiter().GetResult();
        }

        public void RemoveApplication(string filePath)
        {
            var apps = GetAllApplications();
            var model = apps.FirstOrDefault(o => o.AppPath.Contains(filePath));
            if (model != null)
            {
                apps.Remove(model);
                _persistence.SaveAsync("applications", apps).GetAwaiter().GetResult();
            }
        }

        public void LaunchApplication(ApplicationModel app)
        {
            Process process = new Process();
            process.StartInfo.FileName = app.AppPath;
            process.Start();
        }

        public IList<ApplicationModel> GetAllApplications()
        {
            return _persistence.LoadAsync<ApplicationModel>("applications").GetAwaiter().GetResult().ToList();
        }

        public string GetAppIcon(string appPath)
        {
            string iconPath = AppPaths.Icons;
            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}.ico";
            return _iconExtractor.ExtractIconToFile(appPath, iconPath, fileName);
        }
    }
}
