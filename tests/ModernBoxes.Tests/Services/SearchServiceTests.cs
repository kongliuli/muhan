using FluentAssertions;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Services;
using ModernBoxes.Core.Interfaces.Repositories;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.Services;

[Trait("Category", "Service")]
public class SearchServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly Mock<IMenuRepository> _menuRepoMock;
    private readonly Mock<IApplicationRepository> _appRepoMock;
    private readonly Mock<ITempDirRepository> _dirRepoMock;
    private readonly Mock<ITempFileRepository> _fileRepoMock;
    private readonly Mock<INoteRepository> _noteRepoMock;
    private readonly SearchService _sut;

    public SearchServiceTests(DatabaseFixture _)
    {
        _menuRepoMock = new Mock<IMenuRepository>();
        _appRepoMock = new Mock<IApplicationRepository>();
        _dirRepoMock = new Mock<ITempDirRepository>();
        _fileRepoMock = new Mock<ITempFileRepository>();
        _noteRepoMock = new Mock<INoteRepository>();
        _sut = SearchServiceTestFactory.Create(
            _menuRepoMock,
            _appRepoMock,
            _dirRepoMock,
            _fileRepoMock,
            _noteRepoMock);
    }

    [Fact]
    public async Task SearchAsync_ShouldAggregateResultsFromAllRepos()
    {
        var query = "test";
        _menuRepoMock
            .Setup(r => r.SearchMenus(query))
            .Returns(new List<SearchResultModel>
            {
                new() { Name = "Menu1", Type = ResultType.Menu }
            });
        _appRepoMock
            .Setup(r => r.SearchApplications(query))
            .Returns(new List<SearchResultModel>
            {
                new() { Name = "App1", Type = ResultType.Application }
            });
        _dirRepoMock
            .Setup(r => r.SearchTempDirs(query))
            .Returns(new List<SearchResultModel>
            {
                new() { Name = "Dir1", Type = ResultType.Directory }
            });
        _fileRepoMock
            .Setup(r => r.SearchTempFiles(query))
            .Returns(new List<SearchResultModel>
            {
                new() { Name = "File1", Type = ResultType.File }
            });
        _noteRepoMock
            .Setup(r => r.SearchNotes(query))
            .Returns(new List<SearchResultModel>
            {
                new() { Name = "Note1", Type = ResultType.Note }
            });

        var results = await _sut.SearchAsync(query);

        results.Should().HaveCount(5);
        results.Select(r => r.Type).Should().Contain([
            ResultType.Menu,
            ResultType.Application,
            ResultType.Directory,
            ResultType.File,
            ResultType.Note
        ]);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenAllReposReturnEmpty()
    {
        var query = "nothing";
        _menuRepoMock.Setup(r => r.SearchMenus(query)).Returns(new List<SearchResultModel>());
        _appRepoMock.Setup(r => r.SearchApplications(query)).Returns(new List<SearchResultModel>());
        _dirRepoMock.Setup(r => r.SearchTempDirs(query)).Returns(new List<SearchResultModel>());
        _fileRepoMock.Setup(r => r.SearchTempFiles(query)).Returns(new List<SearchResultModel>());
        _noteRepoMock.Setup(r => r.SearchNotes(query)).Returns(new List<SearchResultModel>());

        var results = await _sut.SearchAsync(query);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldHandlePartialRepoFailure()
    {
        var query = "partial";
        _menuRepoMock
            .Setup(r => r.SearchMenus(query))
            .Throws(new InvalidOperationException("Menu repo failed"));
        _appRepoMock
            .Setup(r => r.SearchApplications(query))
            .Returns(new List<SearchResultModel>
            {
                new() { Name = "App1", Type = ResultType.Application }
            });
        _dirRepoMock
            .Setup(r => r.SearchTempDirs(query))
            .Returns(new List<SearchResultModel>());
        _fileRepoMock
            .Setup(r => r.SearchTempFiles(query))
            .Returns(new List<SearchResultModel>());
        _noteRepoMock
            .Setup(r => r.SearchNotes(query))
            .Returns(new List<SearchResultModel>());

        Func<Task> act = () => _sut.SearchAsync(query);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Menu repo failed");
    }

    [Fact]
    public async Task SearchAsync_RapidCalls_ShouldExecuteForEachCall()
    {
        var query = "rapid";
        _menuRepoMock.Setup(r => r.SearchMenus(query)).Returns(new List<SearchResultModel>());
        _appRepoMock.Setup(r => r.SearchApplications(query)).Returns(new List<SearchResultModel>());
        _dirRepoMock.Setup(r => r.SearchTempDirs(query)).Returns(new List<SearchResultModel>());
        _fileRepoMock.Setup(r => r.SearchTempFiles(query)).Returns(new List<SearchResultModel>());
        _noteRepoMock.Setup(r => r.SearchNotes(query)).Returns(new List<SearchResultModel>());

        var tasks = new List<Task>();
        for (var i = 0; i < 5; i++)
            tasks.Add(_sut.SearchAsync(query));

        await Task.WhenAll(tasks);

        _menuRepoMock.Verify(r => r.SearchMenus(query), Times.Exactly(5));
        _appRepoMock.Verify(r => r.SearchApplications(query), Times.Exactly(5));
        _dirRepoMock.Verify(r => r.SearchTempDirs(query), Times.Exactly(5));
        _fileRepoMock.Verify(r => r.SearchTempFiles(query), Times.Exactly(5));
        _noteRepoMock.Verify(r => r.SearchNotes(query), Times.Exactly(5));
    }

    [Fact]
    public async Task SearchAsync_WithDebounceWrapper_CallsServiceOnlyOnceWhenRapidlyInvoked()
    {
        var query = "debounce";
        var serviceCallCount = 0;
        var innerMock = new Mock<ISearchService>();
        innerMock
            .Setup(s => s.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<SearchResultModel>())
            .Callback(() => serviceCallCount++);

        var debouncedService = new DebouncedSearchService(innerMock.Object, TimeSpan.FromMilliseconds(100));

        var tasks = new List<Task<List<SearchResultModel>>>();
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(debouncedService.SearchAsync(query));
            await Task.Delay(10);
        }

        await Task.WhenAll(tasks);

        serviceCallCount.Should().Be(1);
    }

    private sealed class DebouncedSearchService : ISearchService
    {
        private readonly ISearchService _inner;
        private readonly TimeSpan _debounceInterval;
        private CancellationTokenSource? _cts;
        private Task<List<SearchResultModel>>? _currentTask;
        private readonly object _lock = new();

        public DebouncedSearchService(ISearchService inner, TimeSpan debounceInterval)
        {
            _inner = inner;
            _debounceInterval = debounceInterval;
        }

        public Task<List<SearchResultModel>> SearchAsync(string query)
        {
            lock (_lock)
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var token = _cts.Token;
                _currentTask = DebouncedSearch(query, token);
                return _currentTask;
            }
        }

        public void RecordSelection(SearchResultModel result) => _inner.RecordSelection(result);

        private async Task<List<SearchResultModel>> DebouncedSearch(string query, CancellationToken ct)
        {
            try
            {
                await Task.Delay(_debounceInterval, ct);
            }
            catch (TaskCanceledException)
            {
                return new List<SearchResultModel>();
            }

            return await _inner.SearchAsync(query);
        }
    }
}
