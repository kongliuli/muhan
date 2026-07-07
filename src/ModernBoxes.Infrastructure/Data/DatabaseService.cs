using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace ModernBoxes.Infrastructure.Data
{
    public class DatabaseService
    {
        private static DatabaseService? _instance;
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private readonly string _connectionString;

        private DatabaseService()
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ModernBoxes", "modernboxes.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            _connectionString = $"Data Source={dbPath}";
        }

        public SqliteConnection GetConnection() => new SqliteConnection(_connectionString);

        public void CreateTables()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Menus (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MenuName TEXT NOT NULL,
                    Icon TEXT,
                    Target TEXT
                );
                CREATE TABLE IF NOT EXISTS Applications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FileName TEXT NOT NULL,
                    AppPath TEXT NOT NULL,
                    Icon TEXT
                );
                CREATE TABLE IF NOT EXISTS TempDirs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TempDirPath TEXT NOT NULL,
                    TempDirImportantKind TEXT
                );
                CREATE TABLE IF NOT EXISTS TempFiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilePath TEXT NOT NULL,
                    FileKind TEXT
                );
                CREATE TABLE IF NOT EXISTS Notes (
                    Id TEXT PRIMARY KEY,
                    Title TEXT,
                    Content TEXT,
                    Color TEXT,
                    IsPinned INTEGER DEFAULT 0,
                    CreatedAt TEXT,
                    UpdatedAt TEXT
                );
                CREATE TABLE IF NOT EXISTS CardConfigs (
                    Id INTEGER PRIMARY KEY,
                    CardName TEXT,
                    IsChecked INTEGER DEFAULT 0
                );
            ";
            cmd.ExecuteNonQuery();
            FtsSearchIndex.EnsureSchema(conn);
        }

        public void Initialize()
        {
            CreateTables();
        }

        public long GetTableCount(string tableName)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            return (long)cmd.ExecuteScalar()!;
        }
    }
}
