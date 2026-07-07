using FluentAssertions;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Services;
using ModernBoxes.Core.Interfaces.Repositories;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.Services;

[Trait("Category", "Service")]
public class SearchServicePinyinTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task SearchAsync_ShouldExpandWithPinyin_WhenSqlReturnsEmpty()
    {
        var menuRepo = new Mock<IMenuRepository>();
        var appRepo = new Mock<IApplicationRepository>();
        var dirRepo = new Mock<ITempDirRepository>();
        var fileRepo = new Mock<ITempFileRepository>();
        var noteRepo = new Mock<INoteRepository>();

        menuRepo.Setup(r => r.SearchMenus("wx")).Returns(new List<SearchResultModel>());
        appRepo.Setup(r => r.SearchApplications("wx")).Returns(new List<SearchResultModel>());
        dirRepo.Setup(r => r.SearchTempDirs("wx")).Returns(new List<SearchResultModel>());
        fileRepo.Setup(r => r.SearchTempFiles("wx")).Returns(new List<SearchResultModel>());
        noteRepo.Setup(r => r.SearchNotes("wx")).Returns(new List<SearchResultModel>());

        appRepo.Setup(r => r.SearchApplications(""))
            .Returns(new List<SearchResultModel>
            {
                new() { Name = "微信", Type = ResultType.Application, Detail = "C:\\WeChat.exe" }
            });

        var sut = SearchServiceTestFactory.Create(menuRepo, appRepo, dirRepo, fileRepo, noteRepo);

        var results = await sut.SearchAsync("wx");

        results.Should().ContainSingle(r => r.Name == "微信");
    }
}
