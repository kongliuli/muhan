using FluentAssertions;
using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Infrastructure.Data.Repositories;
using Xunit;

namespace ModernBoxes.Tests.Repositories;

public class NoteRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly NoteRepository _repo;

    public NoteRepositoryTests(DatabaseFixture _)
    {
        _repo = new NoteRepository(ModernBoxes.Infrastructure.Data.DatabaseService.Instance);
    }

    [Fact]
    public void SyncNotes_ShouldInsertNotesIntoDatabase()
    {
        var notes = new List<NoteModel>
        {
            new() { Id = Guid.NewGuid(), Title = "Shopping List", Content = "Milk, Eggs, Bread", Color = "#FFFF00", IsPinned = true },
            new() { Id = Guid.NewGuid(), Title = "Meeting Notes", Content = "Discuss Q3 roadmap", Color = "#FF0000", IsPinned = false }
        };

        _repo.SyncNotes(notes);

        var results = _repo.SearchNotes("Shopping");
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Shopping List");
        results[0].Type.Should().Be(ResultType.Note);
    }

    [Fact]
    public void SearchNotes_ShouldSearchByTitleAndContent()
    {
        var notes = new List<NoteModel>
        {
            new() { Id = Guid.NewGuid(), Title = "Groceries", Content = "Buy vegetables and fruit", Color = "#00FF00", IsPinned = false },
            new() { Id = Guid.NewGuid(), Title = "Workout Plan", Content = "Monday: cardio", Color = "#0000FF", IsPinned = true },
            new() { Id = Guid.NewGuid(), Title = "Recipe", Content = "Pasta with vegetables", Color = "#FFFF00", IsPinned = false }
        };
        _repo.SyncNotes(notes);

        var byTitle = _repo.SearchNotes("Groceries");
        byTitle.Should().HaveCount(1);

        var byContent = _repo.SearchNotes("vegetables");
        byContent.Should().HaveCount(2);
    }

    [Fact]
    public void SyncNotes_WithNewData_ShouldReplacePreviousNotes()
    {
        var oldNotes = new List<NoteModel>
        {
            new() { Id = Guid.NewGuid(), Title = "Old Note", Content = "Old content", Color = "#AAAAAA", IsPinned = false }
        };
        _repo.SyncNotes(oldNotes);

        var newNotes = new List<NoteModel>
        {
            new() { Id = Guid.NewGuid(), Title = "New Note", Content = "New content", Color = "#BBBBBB", IsPinned = true }
        };
        _repo.SyncNotes(newNotes);

        _repo.SearchNotes("Old").Should().BeEmpty();
        _repo.SearchNotes("New").Should().HaveCount(1);
    }

    public static IEnumerable<object[]> NoteCreateData =>
        new List<object[]>
        {
            new object[] { "Project Plan", "Deliverables: API, UI, Docs", "#FF5733", true },
            new object[] { "Bug Tracker", "Fix login crash on null input", "#33FF57", false },
            new object[] { "Weekly Review", "Sprint goals met, velocity up", "#3357FF", false }
        };

    [Theory]
    [MemberData(nameof(NoteCreateData))]
    public void NoteRepository_Create_ShouldInsertAndFindByTitle(string title, string content, string color, bool isPinned)
    {
        var notes = new List<NoteModel> { new() { Id = Guid.NewGuid(), Title = title, Content = content, Color = color, IsPinned = isPinned } };

        _repo.SyncNotes(notes);

        var results = _repo.SearchNotes(title);
        results.Should().HaveCount(1);
        results[0].Name.Should().Be(title);
        results[0].Type.Should().Be(ResultType.Note);
    }

    public static IEnumerable<object[]> NoteReadData =>
        new List<object[]>
        {
            new object[] { "API Docs", "Endpoint /users returns JSON", "Swagger setup", "Setup instructions for API", "API", 2 },
            new object[] { "User Guide", "Formatting tips", "Deploy Plan", "Release format v2", "format", 2 },
            new object[] { "Deploy", "Release v2", "Hotfix v1", "Rollback plan", "NOMATCH", 0 }
        };

    [Theory]
    [MemberData(nameof(NoteReadData))]
    public void NoteRepository_Read_ShouldSearchTitleAndContent(string title1, string content1, string title2, string content2, string query, int expectedCount)
    {
        var notes = new List<NoteModel>
        {
            new() { Id = Guid.NewGuid(), Title = title1, Content = content1, Color = "#AAAAAA", IsPinned = false },
            new() { Id = Guid.NewGuid(), Title = title2, Content = content2, Color = "#BBBBBB", IsPinned = false }
        };
        _repo.SyncNotes(notes);

        var results = _repo.SearchNotes(query);
        results.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void NoteRepository_Update_ShouldReplaceAllRecords()
    {
        _repo.SyncNotes(new List<NoteModel> { new() { Id = Guid.NewGuid(), Title = "First", Content = "c1", Color = "#111111", IsPinned = false } });

        _repo.SyncNotes(new List<NoteModel> { new() { Id = Guid.NewGuid(), Title = "Second", Content = "c2", Color = "#222222", IsPinned = true } });

        _repo.SearchNotes("First").Should().BeEmpty();
        _repo.SearchNotes("Second").Should().HaveCount(1);
    }

    [Fact]
    public void NoteRepository_Delete_EmptySyncClearsAllRecords()
    {
        _repo.SyncNotes(new List<NoteModel>
        {
            new() { Id = Guid.NewGuid(), Title = "A", Content = "a", Color = "#AAA", IsPinned = false },
            new() { Id = Guid.NewGuid(), Title = "B", Content = "b", Color = "#BBB", IsPinned = true }
        });

        _repo.SyncNotes(new List<NoteModel>());

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Notes";
        ((long)cmd.ExecuteScalar()!).Should().Be(0);
    }

    [Fact]
    public void NoteRepository_LargeBatch100Records()
    {
        var notes = Enumerable.Range(1, 100)
            .Select(i => new NoteModel
            {
                Id = Guid.NewGuid(),
                Title = $"Note_{i:D3}",
                Content = $"Content of note number {i:D3}",
                Color = "#FFE4B5",
                IsPinned = i % 2 == 0
            })
            .ToList();
        _repo.SyncNotes(notes);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Notes";
        ((long)cmd.ExecuteScalar()!).Should().Be(100);

        cmd.CommandText = "SELECT Title FROM Notes WHERE Title IN ('Note_004', 'Note_053', 'Note_096')";
        using var reader = cmd.ExecuteReader();
        var titles = new List<string>();
        while (reader.Read()) titles.Add(reader.GetString(0));
        titles.Should().HaveCount(3);
    }

    [Fact]
    public void NoteRepository_DuplicatePrimaryKey_ThrowsUniqueConstraint()
    {
        var duplicateId = Guid.NewGuid();

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Notes (Id, Title, Content, Color, IsPinned, CreatedAt, UpdatedAt) VALUES (@Id, 'First', 'c1', '#111', 0, @Now, @Now)";
        cmd.Parameters.AddWithValue("@Id", duplicateId.ToString());
        cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow.ToString("o"));
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO Notes (Id, Title, Content, Color, IsPinned, CreatedAt, UpdatedAt) VALUES (@Id, 'Second', 'c2', '#222', 0, @Now, @Now)";
        var act = () => cmd.ExecuteNonQuery();
        act.Should().Throw<SqliteException>().Where(e => e.Message.Contains("UNIQUE"));
    }
}
