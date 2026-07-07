using Microsoft.Data.Sqlite;
using ModernBoxes.Infrastructure.Data;
using System.Reflection;

namespace ModernBoxes.Tests;

public class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _keeper;

    public DatabaseFixture()
    {
        _keeper = new SqliteConnection("Data Source=:memory:;Mode=Memory;Cache=Shared");
        _keeper.Open();

        var field = typeof(DatabaseService).GetField("_connectionString",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(DatabaseService.Instance, "Data Source=:memory:;Mode=Memory;Cache=Shared");

        DatabaseService.Instance.CreateTables();
    }

    public void Dispose()
    {
        _keeper.Close();
        _keeper.Dispose();
    }
}
