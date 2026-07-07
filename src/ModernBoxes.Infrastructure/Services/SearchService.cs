using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Search;
using System.Collections.Generic;
using System.Linq;
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

            if (NeedsPinyinExpand(query, results))
            {
                var allMenus = Task.Run(() => _menuRepo.SearchMenus("")).Result;
                var allApps = Task.Run(() => _appRepo.SearchApplications("")).Result;
                var allDirs = Task.Run(() => _dirRepo.SearchTempDirs("")).Result;
                var allFiles = Task.Run(() => _fileRepo.SearchTempFiles("")).Result;
                var allNotes = Task.Run(() => _noteRepo.SearchNotes("")).Result;

                MergePinyinResults(results, allMenus, query, r => r.Name);
                MergePinyinResults(results, allApps, query, r => r.Name);
                MergePinyinResults(results, allDirs, query, r => r.Name);
                MergePinyinResults(results, allFiles, query, r => r.Name);
                MergePinyinResults(results, allNotes, query, r => r.Name);
            }

            return results;
        }

        private static bool NeedsPinyinExpand(string query, List<SearchResultModel> current)
        {
            return current.Count == 0 && ChinesePinyinHelper.LooksLikePinyinQuery(query);
        }

        private static void MergePinyinResults(
            List<SearchResultModel> target,
            List<SearchResultModel>? candidates,
            string query,
            System.Func<SearchResultModel, string> nameSelector)
        {
            if (candidates == null || candidates.Count == 0)
                return;

            foreach (var item in candidates)
            {
                var name = nameSelector(item);
                if (string.IsNullOrEmpty(name))
                    continue;
                if (!ChinesePinyinHelper.Matches(query, name))
                    continue;
                if (target.Any(x => x.Type == item.Type && x.Name == item.Name))
                    continue;
                target.Add(item);
            }
        }
    }
}
