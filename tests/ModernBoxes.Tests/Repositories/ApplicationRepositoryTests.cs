using FluentAssertions;
using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Infrastructure.Data.Repositories;
using Xunit;

namespace ModernBoxes.Tests.Repositories;

public class ApplicationRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly ApplicationRepository _repo;

    public ApplicationRepositoryTests(DatabaseFixture _)
    {
        _repo = new ApplicationRepository(ModernBoxes.Infrastructure.Data.DatabaseService.Instance);
    }

    [Fact]
    public void AddApplication_ShouldInsertApplicationIntoDatabase()
    {
        var app = new ApplicationModel { FileName = "Notepad", AppPath = @"C:\Windows\notepad.exe", Icon = "icon1" };

        _repo.AddApplication(app);

        var results = _repo.SearchApplications("Note");
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Notepad");
        results[0].Type.Should().Be(ResultType.Application);
    }

    [Fact]
    public void SearchApplications_ShouldReturnMatchingApplications()
    {
        _repo.AddApplication(new ApplicationModel { FileName = "Chrome", AppPath = @"C:\Program Files\Chrome\chrome.exe", Icon = "chrome" });
        _repo.AddApplication(new ApplicationModel { FileName = "Firefox", AppPath = @"C:\Program Files\Firefox\firefox.exe", Icon = "ff" });
        _repo.AddApplication(new ApplicationModel { FileName = "Notepad", AppPath = @"C:\Windows\notepad.exe", Icon = "note" });

        var results = _repo.SearchApplications("Fire");

        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Firefox");
    }

    [Fact]
    public void DeleteApplication_ShouldRemoveApplicationFromDatabase()
    {
        var appPath = @"C:\Apps\test.exe";
        _repo.AddApplication(new ApplicationModel { FileName = "TestApp", AppPath = appPath, Icon = "test" });

        _repo.DeleteApplication(appPath);

        _repo.SearchApplications("Test").Should().BeEmpty();
    }

    [Fact]
    public void SyncApplications_ShouldReplaceAllApplications()
    {
        _repo.AddApplication(new ApplicationModel { FileName = "OldApp", AppPath = @"C:\old.exe", Icon = "old" });

        var newApps = new List<ApplicationModel>
        {
            new() { FileName = "NewApp1", AppPath = @"C:\new1.exe", Icon = "n1" },
            new() { FileName = "NewApp2", AppPath = @"C:\new2.exe", Icon = "n2" }
        };
        _repo.SyncApplications(newApps);

        _repo.SearchApplications("Old").Should().BeEmpty();
        _repo.SearchApplications("New").Should().HaveCount(2);
    }

    public static IEnumerable<object[]> AppCreateData =>
        new List<object[]>
        {
            new object[] { "WinRAR", @"C:\Program Files\WinRAR\WinRAR.exe", "rar" },
            new object[] { "7-Zip", @"C:\Program Files\7-Zip\7zFM.exe", "7z" },
            new object[] { "VLC", @"C:\Program Files\VideoLAN\VLC\vlc.exe", "vlc" }
        };

    [Theory]
    [MemberData(nameof(AppCreateData))]
    public void ApplicationRepository_Create_ShouldInsertAndFind(string fileName, string appPath, string icon)
    {
        var app = new ApplicationModel { FileName = fileName, AppPath = appPath, Icon = icon };

        _repo.AddApplication(app);

        var results = _repo.SearchApplications(fileName);
        results.Should().HaveCount(1);
        results[0].Name.Should().Be(fileName);
        results[0].Type.Should().Be(ResultType.Application);
    }

    public static IEnumerable<object[]> AppReadData =>
        new List<object[]>
        {
            new object[] { new[] { "Docker Desktop", "Docker Compose", "Kubernetes" }, "Docker", 2 },
            new object[] { new[] { "Python 3.12", "PyCharm", "Python 3.11" }, "Python", 2 },
            new object[] { new[] { "Steam", "Epic Games", "GOG Galaxy" }, "XYZ", 0 }
        };

    [Theory]
    [MemberData(nameof(AppReadData))]
    public void ApplicationRepository_Read_ShouldSearchByPartialMatch(string[] fileNames, string query, int expectedCount)
    {
        foreach (var name in fileNames)
            _repo.AddApplication(new ApplicationModel { FileName = name, AppPath = $@"C:\Apps\{name}.exe", Icon = "app" });

        var results = _repo.SearchApplications(query);
        results.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void ApplicationRepository_Update_SyncWithNewDataReplacesAll()
    {
        _repo.AddApplication(new ApplicationModel { FileName = "Keep", AppPath = @"C:\keep.exe", Icon = "k" });

        _repo.SyncApplications(new List<ApplicationModel>
        {
            new() { FileName = "Replaced", AppPath = @"C:\replaced.exe", Icon = "r" }
        });

        _repo.SearchApplications("Keep").Should().BeEmpty();
        _repo.SearchApplications("Replaced").Should().HaveCount(1);
    }

    [Fact]
    public void ApplicationRepository_Delete_NotFoundPathDoesNotThrow()
    {
        _repo.SyncApplications(new List<ApplicationModel>());

        var act = () => _repo.DeleteApplication(@"C:\nonexistent.exe");
        act.Should().NotThrow();

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Applications";
        ((long)cmd.ExecuteScalar()!).Should().Be(0);
    }

    [Fact]
    public void ApplicationRepository_EmptySyncClearsAllRecords()
    {
        _repo.AddApplication(new ApplicationModel { FileName = "App1", AppPath = @"C:\app1.exe", Icon = "a1" });
        _repo.AddApplication(new ApplicationModel { FileName = "App2", AppPath = @"C:\app2.exe", Icon = "a2" });

        _repo.SyncApplications(new List<ApplicationModel>());

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Applications";
        ((long)cmd.ExecuteScalar()!).Should().Be(0);
    }

    [Fact]
    public void ApplicationRepository_LargeBatch100Records()
    {
        var apps = Enumerable.Range(1, 100)
            .Select(i => new ApplicationModel { FileName = $"App_{i:D3}", AppPath = $@"C:\Apps\app_{i:D3}.exe", Icon = $"icon_{i}" })
            .ToList();
        _repo.SyncApplications(apps);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Applications";
        ((long)cmd.ExecuteScalar()!).Should().Be(100);

        cmd.CommandText = "SELECT FileName FROM Applications WHERE FileName IN ('App_003', 'App_055', 'App_098')";
        using var reader = cmd.ExecuteReader();
        var names = new List<string>();
        while (reader.Read()) names.Add(reader.GetString(0));
        names.Should().HaveCount(3);
        names.Should().Contain(["App_003", "App_055", "App_098"]);
    }

    [Fact]
    public void ApplicationRepository_DuplicatePrimaryKey_ThrowsUniqueConstraint()
    {
        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Applications (Id, FileName, AppPath, Icon) VALUES (888, 'Dup', 'd.exe', 'd')";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO Applications (Id, FileName, AppPath, Icon) VALUES (888, 'Dup2', 'd2.exe', 'd2')";
        var act = () => cmd.ExecuteNonQuery();
        act.Should().Throw<SqliteException>().Where(e => e.Message.Contains("UNIQUE"));
    }
}
