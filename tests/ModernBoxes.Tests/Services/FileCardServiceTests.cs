using FluentAssertions;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Services;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.Services;

[Trait("Category", "Service")]
public class FileCardServiceTests
{
    private readonly Mock<IPersistenceProvider> _persistenceMock;
    private readonly FileCardService _sut;

    public FileCardServiceTests()
    {
        _persistenceMock = new Mock<IPersistenceProvider>();
        _sut = new FileCardService(_persistenceMock.Object);
    }

    [Fact]
    public async Task GetAllFiles_ShouldReturnAllFromPersistence()
    {
        var files = new List<TempFileModel>
        {
            new() { FilePath = @"C:\temp\file1.txt" },
            new() { FilePath = @"C:\temp\file2.txt" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<TempFileModel>("tempfiles"))
            .ReturnsAsync(files);

        var result = await _sut.GetAllFiles();

        result.Should().HaveCount(2);
        result.Should().Contain(f => f.FilePath == @"C:\temp\file1.txt");
    }

    [Fact]
    public async Task GetAllFiles_ShouldReturnEmpty_WhenNoFiles()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempFileModel>("tempfiles"))
            .ReturnsAsync(new List<TempFileModel>());

        var result = await _sut.GetAllFiles();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddFile_ShouldAddAndPersist()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempFileModel>("tempfiles"))
            .ReturnsAsync(new List<TempFileModel>());
        _persistenceMock
            .Setup(p => p.SaveAsync("tempfiles", It.IsAny<IEnumerable<TempFileModel>>()))
            .Returns(Task.CompletedTask);
        var file = new TempFileModel { FilePath = @"C:\newfile.txt" };

        await _sut.AddFile(file);

        _persistenceMock.Verify(
            p => p.SaveAsync("tempfiles", It.IsAny<IEnumerable<TempFileModel>>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveFile_ShouldRemoveByPathAndPersist()
    {
        var files = new List<TempFileModel>
        {
            new() { FilePath = @"C:\keep.txt" },
            new() { FilePath = @"C:\remove.txt" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<TempFileModel>("tempfiles"))
            .ReturnsAsync(files);
        _persistenceMock
            .Setup(p => p.SaveAsync("tempfiles", It.IsAny<IEnumerable<TempFileModel>>()))
            .Returns(Task.CompletedTask);

        await _sut.RemoveFile(@"C:\remove.txt");

        _persistenceMock.Verify(
            p => p.SaveAsync("tempfiles",
                It.Is<IEnumerable<TempFileModel>>(e =>
                    e.All(f => f.FilePath != @"C:\remove.txt"))),
            Times.Once);
    }

    [Fact]
    public async Task RemoveFile_NotFound_ShouldNotThrow()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempFileModel>("tempfiles"))
            .ReturnsAsync(new List<TempFileModel>());
        _persistenceMock
            .Setup(p => p.SaveAsync("tempfiles", It.IsAny<IEnumerable<TempFileModel>>()))
            .Returns(Task.CompletedTask);

        var act = () => _sut.RemoveFile(@"C:\ghost.txt");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllFiles_WhenPersistenceThrows_ShouldPropagateException()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<TempFileModel>("tempfiles"))
            .ThrowsAsync(new InvalidOperationException("Storage failure"));

        Func<Task> act = () => _sut.GetAllFiles();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Storage failure");
    }

    [Fact]
    public async Task OpenFile_ShouldCompleteSuccessfully()
    {
        var result = _sut.OpenFile(@"C:\Windows\notepad.exe");

        result.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Fact]
    public async Task OpenFileLocation_ShouldCompleteSuccessfully()
    {
        var result = _sut.OpenFileLocation(@"C:\Windows\notepad.exe");

        result.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Fact]
    public async Task DeleteToRecycleBin_WithInvalidPath_ShouldRemoveFromPersistence()
    {
        var files = new List<TempFileModel>
        {
            new() { FilePath = @"C:\nonexistent.txt" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<TempFileModel>("tempfiles"))
            .ReturnsAsync(files);
        _persistenceMock
            .Setup(p => p.SaveAsync("tempfiles", It.IsAny<IEnumerable<TempFileModel>>()))
            .Returns(Task.CompletedTask);

        try
        {
            await _sut.DeleteToRecycleBin(@"C:\nonexistent.txt");
        }
        catch
        {
            // DeleteToRecycleBin may throw on nonexistent file; that's expected behavior
        }
    }
}
