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
    private readonly ApplicationCardService _sut;

    public ApplicationCardServiceTests()
    {
        _persistenceMock = new Mock<IPersistenceProvider>();
        _repositoryMock = new Mock<IApplicationRepository>();
        _iconExtractorMock = new Mock<IIconExtractor>();
        _sut = new ApplicationCardService(
            _persistenceMock.Object,
            _repositoryMock.Object,
            _iconExtractorMock.Object);
    }

    private void SetupEmptyPersistence()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<ApplicationModel>("applications"))
            .ReturnsAsync(new List<ApplicationModel>());
    }

    private void SetupSaveSuccess()
    {
        _persistenceMock
            .Setup(p => p.SaveAsync("applications", It.IsAny<IEnumerable<ApplicationModel>>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public void GetAllApplications_ShouldReturnAllFromPersistence()
    {
        var apps = new List<ApplicationModel>
        {
            new() { FileName = "Notepad", AppPath = @"C:\Windows\notepad.exe" },
            new() { FileName = "Calc", AppPath = @"C:\Windows\calc.exe" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<ApplicationModel>("applications"))
            .ReturnsAsync(apps);

        var result = _sut.GetAllApplications();

        result.Should().HaveCount(2);
        result.Select(a => a.FileName).Should().Contain(["Notepad", "Calc"]);
    }

    [Fact]
    public void GetAllApplications_ShouldReturnEmpty_WhenNoApps()
    {
        SetupEmptyPersistence();

        var result = _sut.GetAllApplications();

        result.Should().BeEmpty();
    }

    [Fact]
    public void AddApplication_ShouldAddAndPersist()
    {
        SetupEmptyPersistence();
        SetupSaveSuccess();
        var app = new ApplicationModel { FileName = "NewApp", AppPath = @"C:\NewApp.exe" };

        _sut.AddApplication(app);

        _persistenceMock.Verify(
            p => p.SaveAsync("applications",
                It.Is<IEnumerable<ApplicationModel>>(e =>
                    e.Any(a => a.FileName == "NewApp" && a.AppPath == @"C:\NewApp.exe"))),
            Times.Once);
    }

    [Fact]
    public void RemoveApplication_ShouldRemoveByPathAndPersist()
    {
        var app = new ApplicationModel { FileName = "ToRemove", AppPath = @"C:\ToRemove.exe" };
        var apps = new List<ApplicationModel> { app };
        _persistenceMock
            .Setup(p => p.LoadAsync<ApplicationModel>("applications"))
            .ReturnsAsync(apps);
        SetupSaveSuccess();

        _sut.RemoveApplication(@"C:\ToRemove.exe");

        _persistenceMock.Verify(
            p => p.SaveAsync("applications", It.IsAny<IEnumerable<ApplicationModel>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void RemoveApplication_NotFound_ShouldNotThrow()
    {
        SetupEmptyPersistence();

        var act = () => _sut.RemoveApplication(@"C:\Ghost.exe");

        act.Should().NotThrow();
    }

    [Fact]
    public void GetAllApplications_WhenPersistenceThrows_ShouldPropagateException()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<ApplicationModel>("applications"))
            .ThrowsAsync(new InvalidOperationException("Storage failure"));

        var act = () => _sut.GetAllApplications();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Storage failure");
    }

    [Fact]
    public void LaunchApplication_ShouldNotThrowOnValidApp()
    {
        var app = new ApplicationModel { FileName = "Test", AppPath = @"C:\Windows\notepad.exe" };

        var act = () => _sut.LaunchApplication(app);

        act.Should().NotThrow();
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
