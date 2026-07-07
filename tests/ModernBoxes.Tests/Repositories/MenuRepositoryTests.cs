using FluentAssertions;
using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Infrastructure.Data.Repositories;
using Xunit;

namespace ModernBoxes.Tests.Repositories;

public class MenuRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly MenuRepository _repo;

    public MenuRepositoryTests(DatabaseFixture _)
    {
        _repo = new MenuRepository(ModernBoxes.Infrastructure.Data.DatabaseService.Instance);
    }

    [Fact]
    public void SyncMenus_ShouldInsertMenusIntoDatabase()
    {
        var menus = new List<MenuModel>
        {
            new() { MenuName = "Notepad", Icon = "icon1", Target = "notepad.exe" },
            new() { MenuName = "Calculator", Icon = "icon2", Target = "calc.exe" }
        };

        _repo.SyncMenus(menus);

        var results = _repo.SearchMenus("Note");
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Notepad");
        results[0].Type.Should().Be(ResultType.Menu);
    }

    [Fact]
    public void SearchMenus_ShouldReturnMatchingMenus()
    {
        var menus = new List<MenuModel>
        {
            new() { MenuName = "VisualStudio 2025", Icon = "icon3", Target = "devenv.exe" },
            new() { MenuName = "Android Studio", Icon = "icon4", Target = "studio.exe" }
        };
        _repo.SyncMenus(menus);

        var results = _repo.SearchMenus("Studio");

        results.Should().HaveCount(2);
        results.Select(r => r.Name).Should().Contain(["VisualStudio 2025", "Android Studio"]);
    }

    [Fact]
    public void SyncMenus_WithNewData_ShouldReplacePreviousMenus()
    {
        var oldMenus = new List<MenuModel>
        {
            new() { MenuName = "OldApp", Icon = "old", Target = "old.exe" }
        };
        _repo.SyncMenus(oldMenus);

        var newMenus = new List<MenuModel>
        {
            new() { MenuName = "NewApp", Icon = "new", Target = "new.exe" }
        };
        _repo.SyncMenus(newMenus);

        _repo.SearchMenus("Old").Should().BeEmpty();
        _repo.SearchMenus("New").Should().HaveCount(1);
    }

    public static IEnumerable<object[]> MenuCreateData =>
        new List<object[]>
        {
            new object[] { "Git Bash", "git", @"C:\git\bash.exe" },
            new object[] { "PowerShell", "ps", @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" },
            new object[] { "VS Code", "code", @"D:\Tools\VSCode\Code.exe" }
        };

    [Theory]
    [MemberData(nameof(MenuCreateData))]
    public void MenuRepository_Create_ShouldInsertAndFind(string name, string icon, string target)
    {
        var menus = new List<MenuModel> { new() { MenuName = name, Icon = icon, Target = target } };

        _repo.SyncMenus(menus);

        var results = _repo.SearchMenus(name);
        results.Should().HaveCount(1);
        results[0].Name.Should().Be(name);
        results[0].Type.Should().Be(ResultType.Menu);
    }

    public static IEnumerable<object[]> MenuReadData =>
        new List<object[]>
        {
            new object[] { new[] { "Chrome", "Firefox", "Edge" }, "Fire", 1, "Firefox" },
            new object[] { new[] { "SQL Server", "MySQL Workbench", "PostgreSQL" }, "Server", 1, "SQL Server" },
            new object[] { new[] { "OneNote", "Notion", "Obsidian" }, "No", 2, null! }
        };

    [Theory]
    [MemberData(nameof(MenuReadData))]
    public void MenuRepository_Read_ShouldSearchByPartialMatch(string[] names, string query, int expectedCount, string? exactMatch)
    {
        var menus = names.Select(n => new MenuModel { MenuName = n, Icon = "icon", Target = "target.exe" }).ToList();
        _repo.SyncMenus(menus);

        var results = _repo.SearchMenus(query);
        results.Should().HaveCount(expectedCount);
        if (exactMatch != null)
            results[0].Name.Should().Be(exactMatch);
    }

    [Fact]
    public void MenuRepository_Update_ShouldReplaceAllRecords()
    {
        _repo.SyncMenus(new List<MenuModel> { new() { MenuName = "App1", Icon = "a1", Target = "a1.exe" } });

        _repo.SyncMenus(new List<MenuModel>
        {
            new() { MenuName = "App2", Icon = "a2", Target = "a2.exe" },
            new() { MenuName = "App3", Icon = "a3", Target = "a3.exe" }
        });

        _repo.SearchMenus("App1").Should().BeEmpty();
        _repo.SearchMenus("App2").Should().HaveCount(1);
        _repo.SearchMenus("App3").Should().HaveCount(1);
    }

    [Fact]
    public void MenuRepository_Delete_EmptySyncClearsAllRecords()
    {
        _repo.SyncMenus(new List<MenuModel>
        {
            new() { MenuName = "App1", Icon = "a1", Target = "a1.exe" },
            new() { MenuName = "App2", Icon = "a2", Target = "a2.exe" }
        });
        _repo.SearchMenus("App").Should().HaveCount(2);

        _repo.SyncMenus(new List<MenuModel>());

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Menus";
        ((long)cmd.ExecuteScalar()!).Should().Be(0);
    }

    [Fact]
    public void MenuRepository_LargeBatch100Records()
    {
        var menus = Enumerable.Range(1, 100)
            .Select(i => new MenuModel { MenuName = $"Menu_{i:D3}", Icon = $"icon_{i}", Target = $"target_{i}.exe" })
            .ToList();
        _repo.SyncMenus(menus);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Menus";
        ((long)cmd.ExecuteScalar()!).Should().Be(100);

        cmd.CommandText = "SELECT MenuName FROM Menus WHERE MenuName IN ('Menu_007', 'Menu_042', 'Menu_099')";
        using var reader = cmd.ExecuteReader();
        var names = new List<string>();
        while (reader.Read()) names.Add(reader.GetString(0));
        names.Should().HaveCount(3);
        names.Should().Contain(["Menu_007", "Menu_042", "Menu_099"]);
    }

    [Fact]
    public void MenuRepository_DuplicatePrimaryKey_ThrowsUniqueConstraint()
    {
        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Menus (Id, MenuName, Icon, Target) VALUES (999, 'Dup', 'd', 'd.exe')";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO Menus (Id, MenuName, Icon, Target) VALUES (999, 'Dup2', 'd2', 'd2.exe')";
        var act = () => cmd.ExecuteNonQuery();
        act.Should().Throw<SqliteException>().Where(e => e.Message.Contains("UNIQUE"));
    }
}
