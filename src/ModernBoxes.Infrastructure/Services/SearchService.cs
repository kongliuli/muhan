using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Services
{
    public class SearchService : ISearchService
    {
        private readonly IMenuRepository _menuRepo;
        private readonly IApplicationRepository _appRepo;
        private readonly ITempDirRepository _dirRepo;
        private readonly ITempFileRepository _fileRepo;
        private readonly INoteRepository _noteRepo;

        public SearchService(
            IMenuRepository menuRepo,
            IApplicationRepository appRepo,
            ITempDirRepository dirRepo,
            ITempFileRepository fileRepo,
            INoteRepository noteRepo)
        {
            _menuRepo = menuRepo;
            _appRepo = appRepo;
            _dirRepo = dirRepo;
            _fileRepo = fileRepo;
            _noteRepo = noteRepo;
        }

        public async Task<List<SearchResultModel>> SearchAsync(string query)
        {
            var results = new List<SearchResultModel>();

            var menuTask = Task.Run(() => _menuRepo.SearchMenus(query));
            var appTask = Task.Run(() => _appRepo.SearchApplications(query));
            var dirTask = Task.Run(() => _dirRepo.SearchTempDirs(query));
            var fileTask = Task.Run(() => _fileRepo.SearchTempFiles(query));
            var noteTask = Task.Run(() => _noteRepo.SearchNotes(query));

            await Task.WhenAll(menuTask, appTask, dirTask, fileTask, noteTask);

            results.AddRange(menuTask.Result);
            results.AddRange(appTask.Result);
            results.AddRange(dirTask.Result);
            results.AddRange(fileTask.Result);
            results.AddRange(noteTask.Result);

            return results;
        }
    }
}
