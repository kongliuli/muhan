using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;

namespace ModernBoxes.Infrastructure.Data.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly DatabaseService _db;

        public ApplicationRepository(DatabaseService db)
        {
            _db = db;
        }

        public void SyncApplications(IEnumerable<ApplicationModel> apps)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "DELETE FROM Applications";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO Applications (FileName, AppPath, Icon) VALUES (@FileName, @AppPath, @Icon)";
            var pFileName = cmd.CreateParameter(); pFileName.ParameterName = "@FileName";
            var pAppPath = cmd.CreateParameter(); pAppPath.ParameterName = "@AppPath";
            var pIcon = cmd.CreateParameter(); pIcon.ParameterName = "@Icon";
            cmd.Parameters.Add(pFileName);
            cmd.Parameters.Add(pAppPath);
            cmd.Parameters.Add(pIcon);

            foreach (var a in apps)
            {
                pFileName.Value = a.FileName;
                pAppPath.Value = a.AppPath;
                pIcon.Value = (object?)a.Icon ?? DBNull.Value;
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        public List<ApplicationModel> GetAllApplications()
        {
            var list = new List<ApplicationModel>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT FileName, AppPath, Icon FROM Applications";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ApplicationModel
                {
                    FileName = reader.GetString(0),
                    AppPath = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Icon = reader.IsDBNull(2) ? null : reader.GetString(2),
                });
            }

            return list;
        }

        public List<SearchResultModel> SearchApplications(string query)
        {
            var results = new List<SearchResultModel>();
            var searchParam = $"%{query}%";
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT FileName, AppPath, Icon FROM Applications WHERE FileName LIKE @search";
            cmd.Parameters.AddWithValue("@search", searchParam);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var appPath = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var iconPath = reader.IsDBNull(2) ? "" : reader.GetString(2);
                results.Add(new SearchResultModel
                {
                    Type = ResultType.Application,
                    Name = reader.GetString(0),
                    Detail = appPath,
                    IconText = "\ud83d\udcbb",
                    IconPath = iconPath,
                    ActionTarget = new ApplicationModel { FileName = reader.GetString(0), AppPath = appPath, Icon = iconPath }
                });
            }
            return results;
        }

        public void AddApplication(ApplicationModel app)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Applications (FileName, AppPath, Icon) VALUES (@FileName, @AppPath, @Icon)";
            cmd.Parameters.AddWithValue("@FileName", app.FileName);
            cmd.Parameters.AddWithValue("@AppPath", app.AppPath);
            cmd.Parameters.AddWithValue("@Icon", (object?)app.Icon ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void DeleteApplication(string appPath)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Applications WHERE AppPath = @AppPath";
            cmd.Parameters.AddWithValue("@AppPath", appPath);
            cmd.ExecuteNonQuery();
        }
    }
}
