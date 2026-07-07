using FluentAssertions;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure.Services;
using ModernBoxes.Core.Interfaces.Repositories;
using Moq;
using Xunit;

namespace ModernBoxes.Tests.Services;

[Trait("Category", "Service")]
public class NoteCardServiceTests
{
    private readonly Mock<IPersistenceProvider> _persistenceMock;
    private readonly Mock<INoteRepository> _noteRepoMock;
    private readonly NoteCardService _sut;

    public NoteCardServiceTests()
    {
        _persistenceMock = new Mock<IPersistenceProvider>();
        _noteRepoMock = new Mock<INoteRepository>();
        _sut = new NoteCardService(_persistenceMock.Object, _noteRepoMock.Object);
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadNotesFromPersistence()
    {
        var notes = new List<NoteModel>
        {
            new() { Title = "Note 1", Content = "Content 1" },
            new() { Title = "Note 2", Content = "Content 2" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ReturnsAsync(notes);

        await _sut.LoadAsync();

        var all = _sut.GetAllNotes();
        all.Should().HaveCount(2);
        all.Should().Contain(n => n.Title == "Note 1");
        all.Should().Contain(n => n.Title == "Note 2");
    }

    [Fact]
    public async Task LoadAsync_ShouldHandleEmptyResult()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ReturnsAsync(new List<NoteModel>());

        await _sut.LoadAsync();

        _sut.GetAllNotes().Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_ShouldHandleException()
    {
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ThrowsAsync(new InvalidOperationException("Persistence failure"));

        Func<Task> act = () => _sut.LoadAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Persistence failure");
    }

    [Fact]
    public async Task AddNote_ShouldAddAndPersist()
    {
        _persistenceMock
            .Setup(p => p.SaveAsync("notes", It.IsAny<IEnumerable<NoteModel>>()))
            .Returns(Task.CompletedTask);
        await _sut.LoadAsync();
        var note = new NoteModel { Title = "New Note", Content = "New Content" };

        _sut.AddNote(note);

        var all = _sut.GetAllNotes();
        all.Should().Contain(n => n.Title == "New Note");
        _persistenceMock.Verify(
            p => p.SaveAsync("notes", It.IsAny<IEnumerable<NoteModel>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateNote_ShouldUpdateExistingAndPersist()
    {
        var noteId = Guid.NewGuid();
        var notes = new List<NoteModel>
        {
            new() { Id = noteId, Title = "Old Title", Content = "Old Content" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ReturnsAsync(notes);
        _persistenceMock
            .Setup(p => p.SaveAsync("notes", It.IsAny<IEnumerable<NoteModel>>()))
            .Returns(Task.CompletedTask);
        await _sut.LoadAsync();

        var updated = new NoteModel { Id = noteId, Title = "New Title", Content = "New Content" };
        _sut.UpdateNote(updated);

        var all = _sut.GetAllNotes();
        var existing = all.First(n => n.Id == noteId);
        existing.Title.Should().Be("New Title");
        existing.Content.Should().Be("New Content");
        _persistenceMock.Verify(
            p => p.SaveAsync("notes", It.IsAny<IEnumerable<NoteModel>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateNote_NotFound_ShouldNotPersist()
    {
        await _sut.LoadAsync();
        _persistenceMock.Invocations.Clear();

        var updated = new NoteModel { Id = Guid.NewGuid(), Title = "Ghost" };
        _sut.UpdateNote(updated);

        _persistenceMock.Verify(
            p => p.SaveAsync("notes", It.IsAny<IEnumerable<NoteModel>>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteNote_ShouldRemoveAndPersist()
    {
        var noteId = Guid.NewGuid();
        var notes = new List<NoteModel>
        {
            new() { Id = noteId, Title = "To Delete" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ReturnsAsync(notes);
        _persistenceMock
            .Setup(p => p.SaveAsync("notes", It.IsAny<IEnumerable<NoteModel>>()))
            .Returns(Task.CompletedTask);
        await _sut.LoadAsync();

        _sut.DeleteNote(noteId);

        _sut.GetAllNotes().Should().NotContain(n => n.Id == noteId);
        _persistenceMock.Verify(
            p => p.SaveAsync("notes", It.IsAny<IEnumerable<NoteModel>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteNote_NotFound_ShouldNotThrow()
    {
        await _sut.LoadAsync();
        _persistenceMock.Invocations.Clear();

        var act = () => _sut.DeleteNote(Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public async Task SearchNotes_WithQuery_ShouldReturnMatching()
    {
        var notes = new List<NoteModel>
        {
            new() { Title = "Shopping List", Content = "Milk, Eggs" },
            new() { Title = "Meeting Notes", Content = "Discuss shopping budget" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ReturnsAsync(notes);
        await _sut.LoadAsync();

        var results = _sut.SearchNotes("shopping");
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchNotes_WithNoMatch_ShouldReturnEmpty()
    {
        var notes = new List<NoteModel>
        {
            new() { Title = "ABC", Content = "DEF" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ReturnsAsync(notes);
        await _sut.LoadAsync();

        var results = _sut.SearchNotes("xyz");
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchNotes_EmptyQuery_ShouldReturnAll()
    {
        var notes = new List<NoteModel>
        {
            new() { Title = "A" },
            new() { Title = "B" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ReturnsAsync(notes);
        await _sut.LoadAsync();

        var results = _sut.SearchNotes("");
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchNotes_NullQuery_ShouldReturnAll()
    {
        var notes = new List<NoteModel>
        {
            new() { Title = "A" }
        };
        _persistenceMock
            .Setup(p => p.LoadAsync<NoteModel>("notes"))
            .ReturnsAsync(notes);
        await _sut.LoadAsync();

        var results = _sut.SearchNotes(null!);
        results.Should().HaveCount(1);
    }
}
