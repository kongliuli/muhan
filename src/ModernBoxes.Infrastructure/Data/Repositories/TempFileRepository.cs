using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModernBoxes.Infrastructure.Data.Repositories
{
    public class TempFileRepository : ITempFileRepository
    {
        private readonly DatabaseService _db;

        public TempFileRepository(DatabaseService db)
        {
            _db = db;
        }

        public void SyncTempFiles(IEnumerable<TempFileModel> files)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "DELETE FROM TempFiles";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO TempFiles (FilePath, FileKind) VALUES (@FilePath, @FileKind)";
            var pPath = cmd.CreateParameter(); pPath.ParameterName = "@FilePath";
            var pKind = cmd.CreateParameter(); pKind.ParameterName = "@FileKind";
            cmd.Parameters.Add(pPath);
            cmd.Parameters.Add(pKind);

            foreach (var f in files)
            {
                pPath.Value = f.FilePath;
                pKind.Value = f.FileKind.ToString();
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            FtsSearchIndex.RebuildTempFiles(conn);
        }

        public List<SearchResultModel> SearchTempFiles(string query)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            if (FtsSearchIndex.HasTempFilesIndex(conn))
            {
                try
                {
                    return SearchTempFilesFts(conn, query);
                }
                catch (SqliteException)
                {
                    // ponytail: FTS 语法异常时回退 LIKE
                }
            }
            return SearchTempFilesLike(conn, query);
        }

        private static List<SearchResultModel> SearchTempFilesFts(SqliteConnection conn, string query)
        {
            var results = new List<SearchResultModel>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT file_path, file_name FROM TempFilesFts
                WHERE TempFilesFts MATCH @search";
            cmd.Parameters.AddWithValue("@search", FtsSearchIndex.QuoteFtsLiteral(query));
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var filePath = reader.GetString(0);
                results.Add(new SearchResultModel
                {
                    Type = ResultType.File,
                    Name = reader.IsDBNull(1) ? Path.GetFileName(filePath) ?? filePath : reader.GetString(1),
                    Detail = filePath,
                    IconText = "\ud83d\udcc4",
                    ActionTarget = new TempFileModel { FilePath = filePath }
                });
            }
            return results;
        }

        private static List<SearchResultModel> SearchTempFilesLike(SqliteConnection conn, string query)
        {
            var results = new List<SearchResultModel>();
            var searchParam = $"%{query}%";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT FilePath FROM TempFiles WHERE FilePath LIKE @search";
            cmd.Parameters.AddWithValue("@search", searchParam);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var filePath = reader.GetString(0);
                results.Add(new SearchResultModel
                {
                    Type = ResultType.File,
                    Name = Path.GetFileName(filePath) ?? filePath,
                    Detail = filePath,
                    IconText = "\ud83d\udcc4",
                    ActionTarget = new TempFileModel { FilePath = filePath }
                });
            }
            return results;
        }
    }
}
