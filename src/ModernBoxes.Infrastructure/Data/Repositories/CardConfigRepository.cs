using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;

namespace ModernBoxes.Infrastructure.Data.Repositories
{
    public class CardConfigRepository : ICardConfigRepository
    {
        private readonly DatabaseService _db;

        public CardConfigRepository(DatabaseService db)
        {
            _db = db;
        }

        public void SyncCardConfigs(IEnumerable<CardContentModel> cards)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "DELETE FROM CardConfigs";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO CardConfigs (Id, CardName, IsChecked) VALUES (@Id, @CardName, @IsChecked)";
            var pId = cmd.CreateParameter(); pId.ParameterName = "@Id";
            var pCardName = cmd.CreateParameter(); pCardName.ParameterName = "@CardName";
            var pIsChecked = cmd.CreateParameter(); pIsChecked.ParameterName = "@IsChecked";
            cmd.Parameters.Add(pId);
            cmd.Parameters.Add(pCardName);
            cmd.Parameters.Add(pIsChecked);

            foreach (var c in cards)
            {
                pId.Value = c.CardID;
                pCardName.Value = c.CardName;
                pIsChecked.Value = c.IsChecked ? 1L : 0L;
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
    }
}
