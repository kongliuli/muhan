using FluentAssertions;
using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Infrastructure.Data.Repositories;
using Xunit;

namespace ModernBoxes.Tests.Repositories;

public class TempDirRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly TempDirRepository _repo;

    public TempDirRepositoryTests(DatabaseFixture _)
    {
        _repo = new TempDirRepository(ModernBoxes.Infrastructure.Data.DatabaseService.Instance);
    }

    [Fact]
    public void SyncTempDirs_ShouldInsertDirectoriesIntoDatabase()
    {
        var dirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\Projects\MyApp", TempDirImportantKind = DirEnum.dirPrimary },
            new() { TempDirPath = @"C:\Temp\Scratch", TempDirImportantKind = DirEnum.dirSecondary }
        };

        _repo.SyncTempDirs(dirs);

        var results = _repo.SearchTempDirs("MyApp");
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("MyApp");
        results[0].Type.Should().Be(ResultType.Directory);
    }

    [Fact]
    public void SearchTempDirs_ShouldReturnMatchingDirectories()
    {
        var dirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"D:\Work\Designs", TempDirImportantKind = DirEnum.dirPrimary },
            new() { TempDirPath = @"D:\Work\Documents", TempDirImportantKind = DirEnum.dirPrimary },
            new() { TempDirPath = @"E:\Backup\Photos", TempDirImportantKind = DirEnum.dirSecondary }
        };
        _repo.SyncTempDirs(dirs);

        var results = _repo.SearchTempDirs("Work");

        results.Should().HaveCount(2);
        results.Select(r => r.Name).Should().Contain(["Designs", "Documents"]);
    }

    [Fact]
    public void SyncTempDirs_WithNewData_ShouldReplacePreviousDirectories()
    {
        var oldDirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\OldDir", TempDirImportantKind = DirEnum.dirPrimary }
        };
        _repo.SyncTempDirs(oldDirs);

        var newDirs = new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\NewDir", TempDirImportantKind = DirEnum.dirDanger }
        };
        _repo.SyncTempDirs(newDirs);

        _repo.SearchTempDirs("Old").Should().BeEmpty();
        _repo.SearchTempDirs("New").Should().HaveCount(1);
    }

    public static IEnumerable<object[]> DirCreateData =>
        new List<object[]>
        {
            new object[] { @"C:\Dev\WebApp", DirEnum.dirPrimary, "WebApp" },
            new object[] { @"D:\Downloads\Temp", DirEnum.dirDanger, "Temp" },
            new object[] { @"E:\Archive\OldProjects", DirEnum.dirSecondary, "OldProjects" }
        };

    [Theory]
    [MemberData(nameof(DirCreateData))]
    public void TempDirRepository_Create_ShouldInsertAndFind(string path, DirEnum kind, string expectedName)
    {
        var dirs = new List<TempDirModel> { new() { TempDirPath = path, TempDirImportantKind = kind } };

        _repo.SyncTempDirs(dirs);

        var results = _repo.SearchTempDirs(expectedName);
        results.Should().HaveCount(1);
        results[0].Name.Should().Be(expectedName);
        results[0].Type.Should().Be(ResultType.Directory);
    }

    public static IEnumerable<object[]> DirReadData =>
        new List<object[]>
        {
            new object[] { new[] { @"C:\Apps\Editor", @"C:\Apps\Tools", @"D:\Media" }, "Apps", 2 },
            new object[] { new[] { @"D:\Code\Tests", @"D:\Code\Src", @"E:\Docs" }, "Code", 2 },
            new object[] { new[] { @"C:\One", @"D:\Two", @"E:\Three" }, "xyz", 0 }
        };

    [Theory]
    [MemberData(nameof(DirReadData))]
    public void TempDirRepository_Read_ShouldSearchByPartialPath(string[] paths, string query, int expectedCount)
    {
        var dirs = paths.Select(p => new TempDirModel { TempDirPath = p, TempDirImportantKind = DirEnum.dirPrimary }).ToList();
        _repo.SyncTempDirs(dirs);

        var results = _repo.SearchTempDirs(query);
        results.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void TempDirRepository_Update_ShouldReplaceAllRecords()
    {
        _repo.SyncTempDirs(new List<TempDirModel> { new() { TempDirPath = @"C:\First", TempDirImportantKind = DirEnum.dirPrimary } });

        _repo.SyncTempDirs(new List<TempDirModel> { new() { TempDirPath = @"C:\Second", TempDirImportantKind = DirEnum.dirSecondary } });

        _repo.SearchTempDirs("First").Should().BeEmpty();
        _repo.SearchTempDirs("Second").Should().HaveCount(1);
    }

    [Fact]
    public void TempDirRepository_Delete_EmptySyncClearsAllRecords()
    {
        _repo.SyncTempDirs(new List<TempDirModel>
        {
            new() { TempDirPath = @"C:\Dir1", TempDirImportantKind = DirEnum.dirPrimary },
            new() { TempDirPath = @"C:\Dir2", TempDirImportantKind = DirEnum.dirSecondary }
        });

        _repo.SyncTempDirs(new List<TempDirModel>());

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM TempDirs";
        ((long)cmd.ExecuteScalar()!).Should().Be(0);
    }

    [Fact]
    public void TempDirRepository_LargeBatch100Records()
    {
        var dirs = Enumerable.Range(1, 100)
            .Select(i => new TempDirModel { TempDirPath = $@"C:\Dirs\Folder_{i:D3}", TempDirImportantKind = DirEnum.dirPrimary })
            .ToList();
        _repo.SyncTempDirs(dirs);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM TempDirs";
        ((long)cmd.ExecuteScalar()!).Should().Be(100);

        cmd.CommandText = "SELECT TempDirPath FROM TempDirs WHERE TempDirPath IN ('C:\\Dirs\\Folder_012', 'C:\\Dirs\\Folder_050', 'C:\\Dirs\\Folder_088')";
        using var reader = cmd.ExecuteReader();
        var paths = new List<string>();
        while (reader.Read()) paths.Add(reader.GetString(0));
        paths.Should().HaveCount(3);
    }

    [Fact]
    public void TempDirRepository_DuplicatePrimaryKey_ThrowsUniqueConstraint()
    {
        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO TempDirs (Id, TempDirPath, TempDirImportantKind) VALUES (777, 'Dup', 'dirPrimary')";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO TempDirs (Id, TempDirPath, TempDirImportantKind) VALUES (777, 'Dup2', 'dirSecondary')";
        var act = () => cmd.ExecuteNonQuery();
        act.Should().Throw<SqliteException>().Where(e => e.Message.Contains("UNIQUE"));
    }
}
