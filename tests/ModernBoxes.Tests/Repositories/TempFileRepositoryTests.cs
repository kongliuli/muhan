using FluentAssertions;
using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Infrastructure.Data.Repositories;
using Xunit;

namespace ModernBoxes.Tests.Repositories;

public class TempFileRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly TempFileRepository _repo;

    public TempFileRepositoryTests(DatabaseFixture _)
    {
        _repo = new TempFileRepository(ModernBoxes.Infrastructure.Data.DatabaseService.Instance);
    }

    [Fact]
    public void SyncTempFiles_ShouldInsertFilesIntoDatabase()
    {
        var files = new List<TempFileModel>
        {
            new() { FilePath = @"C:\Docs\report.pdf", FileKind = DirEnum.dirPrimary },
            new() { FilePath = @"C:\Docs\notes.txt", FileKind = DirEnum.dirSecondary }
        };

        _repo.SyncTempFiles(files);

        var results = _repo.SearchTempFiles("report");
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("report.pdf");
        results[0].Type.Should().Be(ResultType.File);
    }

    [Fact]
    public void SearchTempFiles_ShouldReturnMatchingFiles()
    {
        var files = new List<TempFileModel>
        {
            new() { FilePath = @"D:\Projects\api.cs", FileKind = DirEnum.dirPrimary },
            new() { FilePath = @"D:\Projects\app.ts", FileKind = DirEnum.dirPrimary },
            new() { FilePath = @"D:\Backups\data.zip", FileKind = DirEnum.dirSecondary }
        };
        _repo.SyncTempFiles(files);

        var results = _repo.SearchTempFiles("Projects");

        results.Should().HaveCount(2);
        results.Select(r => r.Name).Should().Contain(["api.cs", "app.ts"]);
    }

    [Fact]
    public void SyncTempFiles_WithNewData_ShouldReplacePreviousFiles()
    {
        var oldFiles = new List<TempFileModel>
        {
            new() { FilePath = @"C:\old.txt", FileKind = DirEnum.dirPrimary }
        };
        _repo.SyncTempFiles(oldFiles);

        var newFiles = new List<TempFileModel>
        {
            new() { FilePath = @"C:\new.txt", FileKind = DirEnum.dirDanger }
        };
        _repo.SyncTempFiles(newFiles);

        _repo.SearchTempFiles("old").Should().BeEmpty();
        _repo.SearchTempFiles("new").Should().HaveCount(1);
    }

    public static IEnumerable<object[]> FileCreateData =>
        new List<object[]>
        {
            new object[] { @"C:\Work\budget.xlsx", DirEnum.dirPrimary, "budget.xlsx" },
            new object[] { @"D:\Temp\junk.tmp", DirEnum.dirDanger, "junk.tmp" },
            new object[] { @"E:\Docs\manual.pdf", DirEnum.dirSecondary, "manual.pdf" }
        };

    [Theory]
    [MemberData(nameof(FileCreateData))]
    public void TempFileRepository_Create_ShouldInsertAndFind(string filePath, DirEnum kind, string expectedName)
    {
        var files = new List<TempFileModel> { new() { FilePath = filePath, FileKind = kind } };

        _repo.SyncTempFiles(files);

        var results = _repo.SearchTempFiles(expectedName);
        results.Should().HaveCount(1);
        results[0].Name.Should().Be(expectedName);
        results[0].Type.Should().Be(ResultType.File);
    }

    public static IEnumerable<object[]> FileReadData =>
        new List<object[]>
        {
            new object[] { new[] { @"C:\Code\script.cs", @"C:\Code\helper.cs", @"D:\Docs\readme.md" }, "script", 1 },
            new object[] { new[] { @"D:\Logs\app.log", @"D:\Logs\err.log", @"D:\Data\info.dat" }, "log", 2 },
            new object[] { new[] { @"E:\A.txt", @"E:\B.csv" }, "zip", 0 }
        };

    [Theory]
    [MemberData(nameof(FileReadData))]
    public void TempFileRepository_Read_ShouldSearchByPartialPath(string[] paths, string query, int expectedCount)
    {
        var files = paths.Select(p => new TempFileModel { FilePath = p, FileKind = DirEnum.dirPrimary }).ToList();
        _repo.SyncTempFiles(files);

        var results = _repo.SearchTempFiles(query);
        results.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void TempFileRepository_Update_ShouldReplaceAllRecords()
    {
        _repo.SyncTempFiles(new List<TempFileModel> { new() { FilePath = @"C:\first.txt", FileKind = DirEnum.dirPrimary } });

        _repo.SyncTempFiles(new List<TempFileModel> { new() { FilePath = @"C:\second.txt", FileKind = DirEnum.dirDanger } });

        _repo.SearchTempFiles("first").Should().BeEmpty();
        _repo.SearchTempFiles("second").Should().HaveCount(1);
    }

    [Fact]
    public void TempFileRepository_Delete_EmptySyncClearsAllRecords()
    {
        _repo.SyncTempFiles(new List<TempFileModel>
        {
            new() { FilePath = @"C:\f1.txt", FileKind = DirEnum.dirPrimary },
            new() { FilePath = @"C:\f2.txt", FileKind = DirEnum.dirSecondary }
        });

        _repo.SyncTempFiles(new List<TempFileModel>());

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM TempFiles";
        ((long)cmd.ExecuteScalar()!).Should().Be(0);
    }

    [Fact]
    public void TempFileRepository_LargeBatch100Records()
    {
        var files = Enumerable.Range(1, 100)
            .Select(i => new TempFileModel { FilePath = $@"C:\Files\doc_{i:D3}.txt", FileKind = DirEnum.dirPrimary })
            .ToList();
        _repo.SyncTempFiles(files);

        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM TempFiles";
        ((long)cmd.ExecuteScalar()!).Should().Be(100);

        cmd.CommandText = "SELECT FilePath FROM TempFiles WHERE FilePath IN ('C:\\Files\\doc_005.txt', 'C:\\Files\\doc_047.txt', 'C:\\Files\\doc_091.txt')";
        using var reader = cmd.ExecuteReader();
        var paths = new List<string>();
        while (reader.Read()) paths.Add(reader.GetString(0));
        paths.Should().HaveCount(3);
    }

    [Fact]
    public void TempFileRepository_DuplicatePrimaryKey_ThrowsUniqueConstraint()
    {
        using var conn = ModernBoxes.Infrastructure.Data.DatabaseService.Instance.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO TempFiles (Id, FilePath, FileKind) VALUES (666, 'dup.txt', 'dirPrimary')";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO TempFiles (Id, FilePath, FileKind) VALUES (666, 'dup2.txt', 'dirSecondary')";
        var act = () => cmd.ExecuteNonQuery();
        act.Should().Throw<SqliteException>().Where(e => e.Message.Contains("UNIQUE"));
    }
}
