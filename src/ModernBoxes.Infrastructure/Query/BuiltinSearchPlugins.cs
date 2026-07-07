using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Search;
using ModernBoxes.Sdk.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Query
{
    internal abstract class RepositorySearchPluginBase : ISearchPlugin
    {
        public abstract string Name { get; }
        public virtual string ActionKeyword => string.Empty;
        public virtual int Priority => 50;

        protected abstract List<SearchResultModel> Search(string term);

        public Task<IReadOnlyList<PluginResult>> QueryAsync(Sdk.Search.Query query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var term = string.IsNullOrEmpty(query.ActionKeyword) ? query.Search : query.Raw.Trim();
            if (string.IsNullOrWhiteSpace(term))
                return Task.FromResult<IReadOnlyList<PluginResult>>(Array.Empty<PluginResult>());

            var results = Search(term) ?? new List<SearchResultModel>();
            if (ChinesePinyinHelper.LooksLikePinyinQuery(term) && results.Count == 0)
            {
                results = Search(string.Empty) ?? new List<SearchResultModel>();
                results = results.Where(r => ChinesePinyinHelper.Matches(term, r.Name)).ToList();
            }

            IReadOnlyList<PluginResult> mapped = results
                .Select(r => new PluginResult
                {
                    Title = r.Name,
                    SubTitle = r.Detail,
                    IcoPath = r.IconText,
                    Score = 50,
                    ContextData = r,
                })
                .ToList();

            return Task.FromResult(mapped);
        }
    }

    internal sealed class MenuSearchPlugin : RepositorySearchPluginBase
    {
        private readonly IMenuRepository _repo;
        public override string Name => "菜单";
        public MenuSearchPlugin(IMenuRepository repo) => _repo = repo;
        protected override List<SearchResultModel> Search(string term) => _repo.SearchMenus(term);
    }

    internal sealed class ApplicationSearchPlugin : RepositorySearchPluginBase
    {
        private readonly IApplicationRepository _repo;
        public override string Name => "应用";
        public ApplicationSearchPlugin(IApplicationRepository repo) => _repo = repo;
        protected override List<SearchResultModel> Search(string term) => _repo.SearchApplications(term);
    }

    internal sealed class TempDirSearchPlugin : RepositorySearchPluginBase
    {
        private readonly ITempDirRepository _repo;
        public override string Name => "文件夹";
        public TempDirSearchPlugin(ITempDirRepository repo) => _repo = repo;
        protected override List<SearchResultModel> Search(string term) => _repo.SearchTempDirs(term);
    }

    internal sealed class TempFileSearchPlugin : RepositorySearchPluginBase
    {
        private readonly ITempFileRepository _repo;
        public override string Name => "文件";
        public TempFileSearchPlugin(ITempFileRepository repo) => _repo = repo;
        protected override List<SearchResultModel> Search(string term) => _repo.SearchTempFiles(term);
    }

    internal sealed class NoteSearchPlugin : RepositorySearchPluginBase
    {
        private readonly INoteRepository _repo;
        public override string Name => "便签";
        public NoteSearchPlugin(INoteRepository repo) => _repo = repo;
        protected override List<SearchResultModel> Search(string term) => _repo.SearchNotes(term);
    }
}
