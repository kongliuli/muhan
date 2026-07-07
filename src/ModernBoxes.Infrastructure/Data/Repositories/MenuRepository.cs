using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;

namespace ModernBoxes.Infrastructure.Data.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly DatabaseService _db;

        public MenuRepository(DatabaseService db)
        {
            _db = db;
        }

        public void SyncMenus(IEnumerable<MenuModel> menus)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "DELETE FROM Menus";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO Menus (MenuName, Icon, Target) VALUES (@MenuName, @Icon, @Target)";
            var pMenuName = cmd.CreateParameter(); pMenuName.ParameterName = "@MenuName";
            var pIcon = cmd.CreateParameter(); pIcon.ParameterName = "@Icon";
            var pTarget = cmd.CreateParameter(); pTarget.ParameterName = "@Target";
            cmd.Parameters.Add(pMenuName);
            cmd.Parameters.Add(pIcon);
            cmd.Parameters.Add(pTarget);

            foreach (var m in menus)
            {
                pMenuName.Value = m.MenuName;
                pIcon.Value = (object?)m.Icon ?? DBNull.Value;
                pTarget.Value = (object?)m.Target ?? DBNull.Value;
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        public List<SearchResultModel> SearchMenus(string query)
        {
            var results = new List<SearchResultModel>();
            var searchParam = $"%{query}%";
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT MenuName, Target FROM Menus WHERE MenuName LIKE @search";
            cmd.Parameters.AddWithValue("@search", searchParam);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new SearchResultModel
                {
                    Type = ResultType.Menu,
                    Name = reader.GetString(0),
                    Detail = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    IconText = "\ud83d\udccb",
                    ActionTarget = new MenuModel { MenuName = reader.GetString(0), Target = reader.IsDBNull(1) ? "" : reader.GetString(1) }
                });
            }
            return results;
        }
    }
}
