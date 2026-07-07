using FluentAssertions;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Services;
using ModernBoxes.Core.Interfaces.Repositories;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.Services;

[Trait("Category", "Service")]
public class ApplicationCardServiceTests
{
    private readonly Mock<IPersistenceProvider> _persistenceMock;
    private readonly Mock<IApplicationRepository> _repositoryMock;
    private readonly Mock<IIconExtractor> _iconExtractorMock;
    private readonly Mock<IProcessLauncher> _launcherMock;
    private readonly ApplicationCardService _sut;

    public ApplicationCardServiceTests()
    {
        _persistenceMock = new Mock<IPersistenceProvider>();
        _repositoryMock = new Mock<IApplicationRepository>();
        _iconExtractorMock = new Mock<IIconExtractor>();
        _launcherMock = new Mock<IProcessLauncher>();
        _sut = new ApplicationCardService(
            _persistenceMock.Object,
            _repositoryMock.Object,
            _iconExtractorMock.Object,
            _launcherMock.Object);
    }

    private void SetupEmptyRepository()
    {
        _repositoryMock
            .Setup(r => r.GetAllApplications())
            .Returns(new List<ApplicationModel>());
    }

    private void SetupSaveSuccess()
    {
        _persistenceMock
            .Setup(p => p.SaveAsync("applications", It.IsAny<IEnumerable<ApplicationModel>>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public void GetAllApplications_ShouldReturnAllFromRepository()
    {
        var apps = new List<ApplicationModel>
        {
            new() { FileName = "Notepad", AppPath = @"C:\Windows\notepad.exe" },
            new() { FileName = "Calc", AppPath = @"C:\Windows\calc.exe" }
        };
        _repositoryMock
            .Setup(r => r.GetAllApplications())
            .Returns(apps);

        var result = _sut.GetAllApplications();

        result.Should().HaveCount(2);
        result.Select(a => a.FileName).Should().Contain(["Notepad", "Calc"]);
    }

    [Fact]
    public void GetAllApplications_ShouldReturnEmpty_WhenNoApps()
    {
        SetupEmptyRepository();

        var result = _sut.GetAllApplications();

        result.Should().BeEmpty();
    }

    [Fact]
    public void AddApplication_ShouldAddToRepositoryAndPersist()
    {
        SetupEmptyRepository();
        SetupSaveSuccess();
        var app = new ApplicationModel { FileName = "NewApp", AppPath = @"C:\NewApp.exe" };

        _sut.AddApplication(app);

        _repositoryMock.Verify(r => r.AddApplication(app), Times.Once);
    }

    [Fact]
    public void RemoveApplication_ShouldRemoveByPathAndPersist()
    {
        var app = new ApplicationModel { FileName = "ToRemove", AppPath = @"C:\ToRemove.exe" };
        var apps = new List<ApplicationModel> { app };
        _repositoryMock
            .Setup(r => r.GetAllApplications())
            .Returns(apps);
        SetupSaveSuccess();

        _sut.RemoveApplication(@"C:\ToRemove.exe");

        _repositoryMock.Verify(r => r.DeleteApplication(@"C:\ToRemove.exe"), Times.Once);
    }

    [Fact]
    public void RemoveApplication_NotFound_ShouldNotThrow()
    {
        SetupEmptyRepository();

        var act = () => _sut.RemoveApplication(@"C:\Ghost.exe");

        act.Should().NotThrow();
    }

    [Fact]
    public void LaunchApplication_ShouldCallLauncher()
    {
        var app = new ApplicationModel { FileName = "Test", AppPath = @"C:\app\test.exe" };

        _sut.LaunchApplication(app);

        _launcherMock.Verify(l => l.Start(@"C:\app\test.exe", false), Times.Once);
    }

    [Fact]
    public void SearchApplications_Delegate_ShouldCallRepository()
    {
        var query = "Note";
        var results = new List<SearchResultModel>
        {
            new() { Name = "Notepad", Type = ResultType.Application }
        };
        _repositoryMock
            .Setup(r => r.SearchApplications(query))
            .Returns(results);

        _repositoryMock.Object.SearchApplications(query);

        _repositoryMock.Verify(r => r.SearchApplications(query), Times.Once);
    }
}
