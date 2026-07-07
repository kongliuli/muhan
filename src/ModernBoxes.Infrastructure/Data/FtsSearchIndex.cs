using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace ModernBoxes.Infrastructure.Data
{
    internal static class FtsSearchIndex
    {
        public static void EnsureSchema(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE VIRTUAL TABLE IF NOT EXISTS NotesFts USING fts5(
                    note_id UNINDEXED,
                    title,
                    content,
                    tokenize='trigram'
                );
                CREATE VIRTUAL TABLE IF NOT EXISTS TempFilesFts USING fts5(
                    file_path UNINDEXED,
                    path,
                    file_name,
                    tokenize='trigram'
                );";
            cmd.ExecuteNonQuery();
        }

        public static void RebuildNotes(SqliteConnection conn)
        {
            EnsureSchema(conn);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM NotesFts";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"
                INSERT INTO NotesFts(note_id, title, content)
                SELECT Id, COALESCE(Title, ''), COALESCE(Content, '') FROM Notes";
            cmd.ExecuteNonQuery();
        }

        public static void RebuildTempFiles(SqliteConnection conn)
        {
            EnsureSchema(conn);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM TempFilesFts";
            cmd.ExecuteNonQuery();

            using var select = conn.CreateCommand();
            select.CommandText = "SELECT FilePath FROM TempFiles";
            using var reader = select.ExecuteReader();
            using var insert = conn.CreateCommand();
            insert.CommandText = "INSERT INTO TempFilesFts(file_path, path, file_name) VALUES (@pathKey, @path, @name)";
            var pKey = insert.CreateParameter();
            pKey.ParameterName = "@pathKey";
            var pPath = insert.CreateParameter();
            pPath.ParameterName = "@path";
            var pName = insert.CreateParameter();
            pName.ParameterName = "@name";
            insert.Parameters.Add(pKey);
            insert.Parameters.Add(pPath);
            insert.Parameters.Add(pName);

            while (reader.Read())
            {
                var path = reader.GetString(0);
                pKey.Value = path;
                pPath.Value = path;
                pName.Value = Path.GetFileName(path) ?? path;
                insert.ExecuteNonQuery();
            }
        }

        public static string QuoteFtsLiteral(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;
            return $"\"{query.Replace("\"", "\"\"")}\"";
        }

        public static bool HasNotesIndex(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM NotesFts";
            try
            {
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
            catch (SqliteException)
            {
                return false;
            }
        }

        public static bool HasTempFilesIndex(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM TempFilesFts";
            try
            {
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
            catch (SqliteException)
            {
                return false;
            }
        }
    }
}
