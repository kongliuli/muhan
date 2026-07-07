using FluentAssertions;
using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Services;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.Services;

[Trait("Category", "Service")]
public class DirectoryCardServiceTests
{
    private readonly Mock<IPersistenceProvider> _persistenceMock;
    private readonly DirectoryCardService _sut;

    public DirectoryCardServiceTests()
    {
        _persistenceMock = new Mock<IPersistenceProvider>();
        _sut = new DirectoryCardService(_persistenceMock.Object);
    }

    [Fact]
    public async Task GetAllDirectories_ShouldReturnAllFromPersistence()
    {
        var dirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\temp\dir1", TempDirImportantKind = DirEnum.dirPrimary },
            new() { TempDirPath = @"C:\temp\dir2", TempDirImportantKind = DirEnum.dirSecondary }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<TempDirModel>("tempdirs"))
            .ReturnsAsync(dirs);

        var result = await _sut.GetAllDirectories();

        result.Should().HaveCount(2);
        result.Should().Contain(d => d.TempDirPath == @"C:\temp\dir1");
    }

    [Fact]
    public async Task GetAllDirectories_ShouldReturnEmpty_WhenNoDirs()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempDirModel>("tempdirs"))
            .ReturnsAsync(new List<TempDirModel>());

        var result = await _sut.GetAllDirectories();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddDirectory_ShouldAddAndPersist()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempDirModel>("tempdirs"))
            .ReturnsAsync(new List<TempDirModel>());
        _persistenceMock
            .Setup(p => p.SaveAsync("tempdirs", It.IsAny<IEnumerable<TempDirModel>>()))
            .Returns(Task.CompletedTask);
        var model = new TempDirModel
        {
            TempDirPath = @"C:\newdir",
            TempDirImportantKind = DirEnum.dirPrimary
        };

        await _sut.AddDirectory(model);

        _persistenceMock.Verify(
            p => p.SaveAsync("tempdirs", It.IsAny<IEnumerable<TempDirModel>>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveDirectory_ShouldRemoveByPathAndPersist()
    {
        var dirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\keep" },
            new() { TempDirPath = @"C:\remove" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<TempDirModel>("tempdirs"))
            .ReturnsAsync(dirs);
        _persistenceMock
            .Setup(p => p.SaveAsync("tempdirs", It.IsAny<IEnumerable<TempDirModel>>()))
            .Returns(Task.CompletedTask);

        await _sut.RemoveDirectory(@"C:\remove");

        _persistenceMock.Verify(
            p => p.SaveAsync("tempdirs",
                It.Is<IEnumerable<TempDirModel>>(e =>
                    e.All(d => d.TempDirPath != @"C:\remove"))),
            Times.Once);
    }

    [Fact]
    public async Task RemoveDirectory_NotFound_ShouldNotThrow()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempDirModel>("tempdirs"))
            .ReturnsAsync(new List<TempDirModel>());
        _persistenceMock
            .Setup(p => p.SaveAsync("tempdirs", It.IsAny<IEnumerable<TempDirModel>>()))
            .Returns(Task.CompletedTask);

        var act = () => _sut.RemoveDirectory(@"C:\ghost");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ChangeImportance_ShouldUpdateKindAndPersist()
    {
        var dirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\dir1", TempDirImportantKind = DirEnum.dirSecondary }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<TempDirModel>("tempdirs"))
            .ReturnsAsync(dirs);
        _persistenceMock
            .Setup(p => p.SaveAsync("tempdirs", It.IsAny<IEnumerable<TempDirModel>>()))
            .Returns(Task.CompletedTask);

        await _sut.ChangeImportance(@"C:\dir1", DirEnum.dirDanger);

        _persistenceMock.Verify(
            p => p.SaveAsync("tempdirs",
                It.Is<IEnumerable<TempDirModel>>(e =>
                    e.Any(d => d.TempDirPath == @"C:\dir1" &&
                               d.TempDirImportantKind == DirEnum.dirDanger))),
            Times.Once);
    }

    [Fact]
    public async Task ChangeImportance_NotFound_ShouldNotPersist()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempDirModel>("tempdirs"))
            .ReturnsAsync(new List<TempDirModel>());

        await _sut.ChangeImportance(@"C:\ghost", DirEnum.dirDanger);

        _persistenceMock.Verify(
            p => p.SaveAsync("tempdirs", It.IsAny<IEnumerable<TempDirModel>>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllDirectories_WhenPersistenceThrows_ShouldPropagateException()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempDirModel>("tempdirs"))
            .ThrowsAsync(new InvalidOperationException("Storage failure"));

        Func<Task> act = () => _sut.GetAllDirectories();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Storage failure");
    }

    [Fact]
    public void OpenDirectory_ShouldCompleteWithoutException()
    {
        var act = () => _sut.OpenDirectory(@"C:\Windows");

        act.Should().NotThrow();
    }
}
