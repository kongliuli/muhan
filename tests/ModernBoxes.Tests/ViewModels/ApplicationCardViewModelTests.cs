using FluentAssertions;
using ModernBoxes.Core.Models;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.ViewModels;

[Collection("ViewModelTests")]
[Trait("Category", "ViewModel")]
public class ApplicationCardViewModelTests
{
    private readonly ViewModelTestFixture _fixture;

    public ApplicationCardViewModelTests(ViewModelTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ApplicationCardServiceMock.Reset();
    }

    [Fact]
    public void Constructor_ShouldSetDefaultProperties()
    {
        var vm = _fixture.CreateApplicationCardViewModel();

        vm.CardID.Should().Be("app");
        vm.CardHeight.Should().Be(350);
        vm.Preview.Should().Be("pack://application:,,,/Resource/image/previews/application.png");
        vm.IsChecked.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadAppsFromService()
    {
        var apps = new List<ApplicationModel>
        {
            new() { FileName = "Notepad" },
            new() { FileName = "Calc" }
        };
        _fixture.ApplicationCardServiceMock.Setup(s => s.GetAllApplications()).Returns(apps);
        var vm = _fixture.CreateApplicationCardViewModel();

        await vm.LoadAsync();

        vm.CardContent.Should().BeEquivalentTo(apps);
    }

    [Fact]
    public async Task LoadAsync_ShouldSetEmptyContent_WhenNoApps()
    {
        _fixture.ApplicationCardServiceMock.Setup(s => s.GetAllApplications()).Returns(new List<ApplicationModel>());
        var vm = _fixture.CreateApplicationCardViewModel();

        await vm.LoadAsync();

        var content = vm.CardContent as IList<ApplicationModel>;
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_ShouldReloadApps()
    {
        var apps = new List<ApplicationModel>
        {
            new() { FileName = "RefreshedApp" }
        };
        _fixture.ApplicationCardServiceMock.Setup(s => s.GetAllApplications()).Returns(apps);
        var vm = _fixture.CreateApplicationCardViewModel();

        await vm.RefreshAsync();

        var content = vm.CardContent as IList<ApplicationModel>;
        content.Should().NotBeEmpty();
        _fixture.ApplicationCardServiceMock.Verify(s => s.GetAllApplications(), Times.AtLeastOnce);
    }
}
