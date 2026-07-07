using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModernBoxes.Infrastructure.Data.Repositories
{
    public class TempDirRepository : ITempDirRepository
    {
        private readonly DatabaseService _db;

        public TempDirRepository(DatabaseService db)
        {
            _db = db;
        }

        public void SyncTempDirs(IEnumerable<TempDirModel> dirs)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "DELETE FROM TempDirs";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO TempDirs (TempDirPath, TempDirImportantKind) VALUES (@TempDirPath, @TempDirImportantKind)";
            var pPath = cmd.CreateParameter(); pPath.ParameterName = "@TempDirPath";
            var pKind = cmd.CreateParameter(); pKind.ParameterName = "@TempDirImportantKind";
            cmd.Parameters.Add(pPath);
            cmd.Parameters.Add(pKind);

            foreach (var d in dirs)
            {
                pPath.Value = d.TempDirPath;
                pKind.Value = d.TempDirImportantKind.ToString();
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        public List<SearchResultModel> SearchTempDirs(string query)
        {
            var results = new List<SearchResultModel>();
            var searchParam = $"%{query}%";
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TempDirPath FROM TempDirs WHERE TempDirPath LIKE @search";
            cmd.Parameters.AddWithValue("@search", searchParam);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var dirPath = reader.GetString(0);
                results.Add(new SearchResultModel
                {
                    Type = ResultType.Directory,
                    Name = Path.GetFileName(dirPath.TrimEnd('\\', '/')) ?? dirPath,
                    Detail = dirPath,
                    IconText = "\ud83d\udcc1",
                    ActionTarget = new TempDirModel { TempDirPath = dirPath }
                });
            }
            return results;
        }
    }
}
