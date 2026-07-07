using FluentAssertions;
using ModernBoxes.Core.Models;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.ViewModels;

[Collection("ViewModelTests")]
[Trait("Category", "ViewModel")]
public class DirectoryCardViewModelTests
{
    private readonly ViewModelTestFixture _fixture;

    public DirectoryCardViewModelTests(ViewModelTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.DirectoryCardServiceMock.Reset();
    }

    [Fact]
    public void Constructor_ShouldSetDefaultProperties()
    {
        var vm = _fixture.CreateDirectoryCardViewModel();

        vm.CardID.Should().Be("dir");
        vm.CardHeight.Should().Be(250);
        vm.Preview.Should().Be("pack://application:,,,/Resource/image/previews/dir.png");
        vm.IsChecked.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadDirsFromService()
    {
        var dirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\dir1" },
            new() { TempDirPath = @"C:\dir2" }
        };
        _fixture.DirectoryCardServiceMock
            .Setup(s => s.GetAllDirectories())
            .ReturnsAsync(dirs);
        var vm = _fixture.CreateDirectoryCardViewModel();

        await vm.LoadAsync();

        vm.CardContent.Should().BeEquivalentTo(dirs);
    }

    [Fact]
    public async Task LoadAsync_ShouldSetEmptyContent_WhenNoDirs()
    {
        _fixture.DirectoryCardServiceMock
            .Setup(s => s.GetAllDirectories())
            .ReturnsAsync(new List<TempDirModel>());
        var vm = _fixture.CreateDirectoryCardViewModel();

        await vm.LoadAsync();

        var content = vm.CardContent as IEnumerable<TempDirModel>;
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_ShouldReloadDirs()
    {
        var dirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\refreshed" }
        };
        _fixture.DirectoryCardServiceMock
            .Setup(s => s.GetAllDirectories())
            .ReturnsAsync(dirs);
        var vm = _fixture.CreateDirectoryCardViewModel();

        await vm.RefreshAsync();

        var content = vm.CardContent as IEnumerable<TempDirModel>;
        content.Should().NotBeEmpty();
        _fixture.DirectoryCardServiceMock.Verify(s => s.GetAllDirectories(), Times.AtLeastOnce);
    }
}
