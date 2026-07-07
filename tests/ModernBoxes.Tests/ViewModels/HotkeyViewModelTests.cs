using FluentAssertions;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Infrastructure.Ai;
using ModernBoxes.Presentation.ViewModels;
using ModernBoxes.Sdk.Plugins;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.ViewModels;

[Collection("Wpf")]
public class HotkeyViewModelTests
{
    [Fact]
    public void TranslateClipboardHotkey_DefaultIsCtrlShiftT()
    {
        var search = new SearchViewModel(
            Mock.Of<ISearchService>(),
            new AiPromptService(new ChatClientService()),
            Mock.Of<IUserNotifier>());

        var sut = new HotkeyViewModel(
            search,
            new AiPromptService(new ChatClientService()),
            Mock.Of<IUserNotifier>());

        sut.TranslateClipboardHotkey.Should().Be("Ctrl+Shift+T");
    }
}
