using FluentAssertions;
using ModernBoxes.Core.Models;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.ViewModels;

[Collection("ViewModelTests")]
[Trait("Category", "ViewModel")]
public class NoteCardViewModelTests
{
    private readonly ViewModelTestFixture _fixture;

    public NoteCardViewModelTests(ViewModelTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.NoteCardServiceMock.Reset();
    }

    [Fact]
    public void Constructor_ShouldSetDefaultProperties()
    {
        var vm = _fixture.CreateNoteCardViewModel();

        vm.CardID.Should().Be("note");
        vm.CardHeight.Should().Be(200);
        vm.Preview.Should().Be("pack://application:,,,/Resource/image/previews/notes.png");
        vm.IsChecked.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadNotesFromService()
    {
        var notes = new List<NoteModel>
        {
            new() { Title = "Note A" },
            new() { Title = "Note B" }
        };
        _fixture.NoteCardServiceMock.Setup(s => s.GetAllNotes()).Returns(notes);
        var vm = _fixture.CreateNoteCardViewModel();

        await vm.LoadAsync();

        vm.CardContent.Should().BeEquivalentTo(notes);
    }

    [Fact]
    public async Task LoadAsync_ShouldSetEmptyContent_WhenNoNotes()
    {
        _fixture.NoteCardServiceMock.Setup(s => s.GetAllNotes()).Returns(new List<NoteModel>());
        var vm = _fixture.CreateNoteCardViewModel();

        await vm.LoadAsync();

        var content = vm.CardContent as IEnumerable<NoteModel>;
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_ShouldReloadNotes()
    {
        var notes = new List<NoteModel>
        {
            new() { Title = "Refreshed Note" }
        };
        _fixture.NoteCardServiceMock.Setup(s => s.GetAllNotes()).Returns(notes);
        var vm = _fixture.CreateNoteCardViewModel();

        await vm.RefreshAsync();

        var content = vm.CardContent as IEnumerable<NoteModel>;
        content.Should().NotBeEmpty();
        _fixture.NoteCardServiceMock.Verify(s => s.GetAllNotes(), Times.AtLeastOnce);
    }
}
