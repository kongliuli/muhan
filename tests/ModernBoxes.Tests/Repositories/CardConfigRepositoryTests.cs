using FluentAssertions;
using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Infrastructure.Data.Repositories;
using Xunit;

namespace ModernBoxes.Tests.Repositories;

public class CardConfigRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly CardConfigRepository _repo;

    public CardConfigRepositoryTests(DatabaseFixture _)
    {
        _repo = new CardConfigRepository(ModernBoxes.Infrastructure.Data.DatabaseService.Instance);
    }

    [Fact]
    public void SyncCardConfigs_ShouldInsertCardsIntoDatabase()
    {
        var cards = new List<CardContentModel>
        {
            new() { CardID = 1, CardName = "Applications", IsChecked = true },
            new() { CardID = 2, CardName = "Notes", IsChecked = true },
            new() { CardID = 3, CardName = "Files", IsChecked = false }
        };

        _repo.SyncCardConfigs(cards);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM CardConfigs";
        var count = (long)cmd.ExecuteScalar()!;
        count.Should().Be(3);
    }

    [Fact]
    public void SyncCardConfigs_WithUpdatedData_ShouldUpdateExistingCards()
    {
        var initialCards = new List<CardContentModel>
        {
            new() { CardID = 1, CardName = "Apps", IsChecked = true },
            new() { CardID = 2, CardName = "Notes", IsChecked = false }
        };
        _repo.SyncCardConfigs(initialCards);

        var updatedCards = new List<CardContentModel>
        {
            new() { CardID = 1, CardName = "Applications", IsChecked = false },
            new() { CardID = 2, CardName = "Sticky Notes", IsChecked = true },
            new() { CardID = 3, CardName = "Files", IsChecked = true }
        };
        _repo.SyncCardConfigs(updatedCards);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM CardConfigs";
        var count = (long)cmd.ExecuteScalar()!;
        count.Should().Be(3);
    }

    [Fact]
    public void SyncCardConfigs_WithEmptyList_ShouldRemoveAllCards()
    {
        var cards = new List<CardContentModel>
        {
            new() { CardID = 1, CardName = "TestCard", IsChecked = true }
        };
        _repo.SyncCardConfigs(cards);

        _repo.SyncCardConfigs(new List<CardContentModel>());

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM CardConfigs";
        var count = (long)cmd.ExecuteScalar()!;
        count.Should().Be(0);
    }

    public static IEnumerable<object[]> CardCreateData =>
        new List<object[]>
        {
            new object[] { 10, "Weather", true },
            new object[] { 20, "Calendar", false },
            new object[] { 30, "Tasks", true }
        };

    [Theory]
    [MemberData(nameof(CardCreateData))]
    public void CardConfigRepository_Create_ShouldInsertAndVerify(int cardId, string cardName, bool isChecked)
    {
        var cards = new List<CardContentModel> { new() { CardID = cardId, CardName = cardName, IsChecked = isChecked } };

        _repo.SyncCardConfigs(cards);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CardName, IsChecked FROM CardConfigs WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", cardId);
        using var reader = cmd.ExecuteReader();
        reader.Read().Should().BeTrue();
        reader.GetString(0).Should().Be(cardName);
        reader.GetInt64(1).Should().Be(isChecked ? 1L : 0L);
    }

    public static IEnumerable<object[]> CardReadData =>
        new List<object[]>
        {
            new object[] { new[] { (1, "Apps", true), (2, "Notes", false), (3, "Files", true) }, 2, "Notes", false },
            new object[] { new[] { (5, "Music", true), (6, "Video", true), (7, "Books", false) }, 6, "Video", true },
            new object[] { new[] { (100, "Only", true) }, 100, "Only", true }
        };

    [Theory]
    [MemberData(nameof(CardReadData))]
    public void CardConfigRepository_Read_ShouldQueryIndividualCard((int id, string name, bool check)[] data, int queryId, string expectedName, bool expectedChecked)
    {
        var cards = data.Select(d => new CardContentModel { CardID = d.id, CardName = d.name, IsChecked = d.check }).ToList();
        _repo.SyncCardConfigs(cards);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CardName, IsChecked FROM CardConfigs WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", queryId);
        using var reader = cmd.ExecuteReader();
        reader.Read().Should().BeTrue();
        reader.GetString(0).Should().Be(expectedName);
        reader.GetInt64(1).Should().Be(expectedChecked ? 1L : 0L);
    }

    [Fact]
    public void CardConfigRepository_Update_ShouldReplaceAllRecords()
    {
        _repo.SyncCardConfigs(new List<CardContentModel>
        {
            new() { CardID = 1, CardName = "First", IsChecked = true },
            new() { CardID = 2, CardName = "Second", IsChecked = true }
        });

        _repo.SyncCardConfigs(new List<CardContentModel> { new() { CardID = 3, CardName = "Third", IsChecked = false } });

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM CardConfigs";
        ((long)cmd.ExecuteScalar()!).Should().Be(1);

        cmd.CommandText = "SELECT CardName FROM CardConfigs WHERE Id = 3";
        ((string)cmd.ExecuteScalar()!).Should().Be("Third");
    }

    [Fact]
    public void CardConfigRepository_Delete_EmptySyncClearsAllRecords()
    {
        _repo.SyncCardConfigs(new List<CardContentModel>
        {
            new() { CardID = 1, CardName = "A", IsChecked = true },
            new() { CardID = 2, CardName = "B", IsChecked = false }
        });

        _repo.SyncCardConfigs(new List<CardContentModel>());

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM CardConfigs";
        ((long)cmd.ExecuteScalar()!).Should().Be(0);
    }

    [Fact]
    public void CardConfigRepository_LargeBatch100Records()
    {
        var cards = Enumerable.Range(1, 100)
            .Select(i => new CardContentModel { CardID = i, CardName = $"Card_{i:D3}", IsChecked = i % 2 == 0 })
            .ToList();
        _repo.SyncCardConfigs(cards);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM CardConfigs";
        ((long)cmd.ExecuteScalar()!).Should().Be(100);

        cmd.CommandText = "SELECT CardName FROM CardConfigs WHERE Id IN (11, 55, 99)";
        using var reader = cmd.ExecuteReader();
        var names = new List<string>();
        while (reader.Read()) names.Add(reader.GetString(0));
        names.Should().HaveCount(3);
        names.Should().Contain(["Card_011", "Card_055", "Card_099"]);
    }

    [Fact]
    public void CardConfigRepository_DuplicatePrimaryKey_ThrowsUniqueConstraint()
    {
        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO CardConfigs (Id, CardName, IsChecked) VALUES (555, 'First', 1)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO CardConfigs (Id, CardName, IsChecked) VALUES (555, 'Second', 0)";
        var act = () => cmd.ExecuteNonQuery();
        act.Should().Throw<SqliteException>().Where(e => e.Message.Contains("UNIQUE"));
    }
}
