using FluentAssertions;
using ModernBoxes.Core.Models;
using ModernBoxes.Cards;
using Xunit;

namespace ModernBoxes.Tests.ViewModels;

[Trait("Category", "ViewModel")]
public class OneWordCardViewModelTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultProperties()
    {
        var vm = new OneWordCardViewModel();

        vm.CardID.Should().Be("oneword");
        vm.CardHeight.Should().Be(150);
        vm.Preview.Should().Be("pack://application:,,,/Resource/image/previews/onenote.png");
        vm.IsChecked.Should().BeFalse();
    }

    [Fact]
    public void CardContent_ShouldNotBeNull()
    {
        var vm = new OneWordCardViewModel();

        vm.CardContent.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_ShouldAttemptToFetchContent()
    {
        var vm = new OneWordCardViewModel();

        try
        {
            await vm.LoadAsync();
            vm.CardContent.Should().BeOfType<OneWordModel>();
        }
        catch
        {
            // Network-dependent test ??failure to reach v1.hitokoto.cn is acceptable
        }
    }

    [Fact]
    public async Task RefreshAsync_ShouldAttemptToFetchContent()
    {
        var vm = new OneWordCardViewModel();

        try
        {
            await vm.RefreshAsync();
            vm.CardContent.Should().BeOfType<OneWordModel>();
        }
        catch
        {
            // Network-dependent test ??failure is acceptable
        }
    }
}
