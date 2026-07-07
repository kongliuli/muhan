using FluentAssertions;
using ModernBoxes.Core.Models;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.ViewModels;

[Collection("ViewModelTests")]
[Trait("Category", "ViewModel")]
public class FileCardViewModelTests
{
    private readonly ViewModelTestFixture _fixture;

    public FileCardViewModelTests(ViewModelTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.FileCardServiceMock.Reset();
    }

    [Fact]
    public void Constructor_ShouldSetDefaultProperties()
    {
        var vm = _fixture.CreateFileCardViewModel();

        vm.CardID.Should().Be("file");
        vm.CardHeight.Should().Be(300);
        vm.Preview.Should().Be("pack://application:,,,/Resource/image/previews/file.png");
        vm.IsChecked.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadFilesFromService()
    {
        var files = new List<TempFileModel>
        {
            new() { FilePath = @"C:\file1.txt" },
            new() { FilePath = @"C:\file2.txt" }
        };
        _fixture.FileCardServiceMock
            .Setup(s => s.GetAllFiles())
            .ReturnsAsync(files);
        var vm = _fixture.CreateFileCardViewModel();

        await vm.LoadAsync();

        vm.CardContent.Should().BeEquivalentTo(files);
    }

    [Fact]
    public async Task LoadAsync_ShouldSetEmptyContent_WhenNoFiles()
    {
        _fixture.FileCardServiceMock
            .Setup(s => s.GetAllFiles())
            .ReturnsAsync(new List<TempFileModel>());
        var vm = _fixture.CreateFileCardViewModel();

        await vm.LoadAsync();

        var content = vm.CardContent as IEnumerable<TempFileModel>;
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_ShouldReloadFiles()
    {
        var files = new List<TempFileModel>
        {
            new() { FilePath = @"C:\refreshed.txt" }
        };
        _fixture.FileCardServiceMock
            .Setup(s => s.GetAllFiles())
            .ReturnsAsync(files);
        var vm = _fixture.CreateFileCardViewModel();

        await vm.RefreshAsync();

        var content = vm.CardContent as IEnumerable<TempFileModel>;
        content.Should().NotBeEmpty();
        _fixture.FileCardServiceMock.Verify(s => s.GetAllFiles(), Times.AtLeastOnce);
    }
}
