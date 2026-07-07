using FluentAssertions;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Ai;
using ModernBoxes.Presentation.ViewModels;
using ModernBoxes.Sdk.Plugins;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.ViewModels;

[Collection("Wpf")]
public class SearchViewModelTests
{
    private readonly Mock<ISearchService> _search = new();
    private readonly Mock<IUserNotifier> _notifier = new();
    private readonly ChatClientService _chat = new();
    private readonly AiPromptService _ai;

    public SearchViewModelTests()
    {
        _ai = new AiPromptService(_chat);
    }

    private SearchViewModel CreateSut() =>
        new(_search.Object, _ai, _notifier.Object);

    [Fact]
    public async Task SearchAsync_NoResultsWithoutAi_DoesNotAddFallback()
    {
        _search.Setup(s => s.SearchAsync("ghost")).ReturnsAsync(new List<SearchResultModel>());
        var sut = CreateSut();
        sut.SearchText = "ghost";

        await sut.SearchAsync();

        sut.SearchResults.Should().BeEmpty();
    }

    [Fact]
    public void TryCompleteActionKeyword_MatchesPartialPrefix()
    {
        var sut = CreateSut();
        sut.SearchText = "no";

        var ok = sut.TryCompleteActionKeyword();

        ok.Should().BeTrue();
        sut.SearchText.Should().Be("note ");
    }

    [Fact]
    public void ResetForLaunch_ClearsState()
    {
        var sut = CreateSut();
        sut.SearchText = "app test";
        sut.SearchResults.Add(new SearchResultModel { Name = "x" });

        sut.ResetForLaunch();

        sut.SearchText.Should().BeEmpty();
        sut.SearchResults.Should().BeEmpty();
        sut.IsSearching.Should().BeFalse();
    }
}
