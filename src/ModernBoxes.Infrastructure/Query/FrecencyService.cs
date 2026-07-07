using Microsoft.Data.Sqlite;
using ModernBoxes.Infrastructure.Data;
using System;

namespace ModernBoxes.Infrastructure.Query
{
    public sealed class FrecencyService
    {
        private readonly DatabaseService _db;

        public FrecencyService(DatabaseService db)
        {
            _db = db;
        }

        public void EnsureSchema()
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS UsageLog (
                    TargetKey TEXT PRIMARY KEY,
                    HitCount INTEGER NOT NULL DEFAULT 1,
                    LastUsed TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        public void Record(string targetKey)
        {
            if (string.IsNullOrWhiteSpace(targetKey))
                return;

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO UsageLog (TargetKey, HitCount, LastUsed)
                VALUES (@key, 1, @now)
                ON CONFLICT(TargetKey) DO UPDATE SET
                    HitCount = HitCount + 1,
                    LastUsed = @now;";
            cmd.Parameters.AddWithValue("@key", targetKey);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("O"));
            cmd.ExecuteNonQuery();
        }

        public int GetBoost(string targetKey)
        {
            if (string.IsNullOrWhiteSpace(targetKey))
                return 0;

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT HitCount, LastUsed FROM UsageLog WHERE TargetKey = @key";
            cmd.Parameters.AddWithValue("@key", targetKey);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return 0;

            var hits = reader.GetInt32(0);
            var lastUsed = DateTime.Parse(reader.GetString(1), null, System.Globalization.DateTimeStyles.RoundtripKind);
            var days = (DateTime.UtcNow - lastUsed).TotalDays;
            var recency = Math.Max(0, 30 - days) / 30.0;
            return (int)Math.Min(40, hits * 2 * recency);
        }

        public static string MakeKey(string pluginName, string title) => $"{pluginName}|{title}";
    }
}
