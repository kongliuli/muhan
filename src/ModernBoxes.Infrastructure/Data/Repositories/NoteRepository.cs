using Microsoft.Data.Sqlite;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;

namespace ModernBoxes.Infrastructure.Data.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly DatabaseService _db;

        public NoteRepository(DatabaseService db)
        {
            _db = db;
        }

        public void SyncNotes(IEnumerable<NoteModel> notes)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "DELETE FROM Notes";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO Notes (Id, Title, Content, Color, IsPinned, CreatedAt, UpdatedAt) VALUES (@Id, @Title, @Content, @Color, @IsPinned, @CreatedAt, @UpdatedAt)";
            var pId = cmd.CreateParameter(); pId.ParameterName = "@Id";
            var pTitle = cmd.CreateParameter(); pTitle.ParameterName = "@Title";
            var pContent = cmd.CreateParameter(); pContent.ParameterName = "@Content";
            var pColor = cmd.CreateParameter(); pColor.ParameterName = "@Color";
            var pIsPinned = cmd.CreateParameter(); pIsPinned.ParameterName = "@IsPinned";
            var pCreatedAt = cmd.CreateParameter(); pCreatedAt.ParameterName = "@CreatedAt";
            var pUpdatedAt = cmd.CreateParameter(); pUpdatedAt.ParameterName = "@UpdatedAt";
            cmd.Parameters.Add(pId);
            cmd.Parameters.Add(pTitle);
            cmd.Parameters.Add(pContent);
            cmd.Parameters.Add(pColor);
            cmd.Parameters.Add(pIsPinned);
            cmd.Parameters.Add(pCreatedAt);
            cmd.Parameters.Add(pUpdatedAt);

            foreach (var n in notes)
            {
                pId.Value = n.Id.ToString();
                pTitle.Value = n.Title;
                pContent.Value = n.Content;
                pColor.Value = n.Color;
                pIsPinned.Value = n.IsPinned ? 1L : 0L;
                pCreatedAt.Value = n.CreatedAt.ToString("o");
                pUpdatedAt.Value = n.UpdatedAt.ToString("o");
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            FtsSearchIndex.RebuildNotes(conn);
        }

        public List<SearchResultModel> SearchNotes(string query)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            if (FtsSearchIndex.HasNotesIndex(conn))
            {
                try
                {
                    return SearchNotesFts(conn, query);
                }
                catch (SqliteException)
                {
                    // ponytail: FTS 语法异常时回退 LIKE
                }
            }
            return SearchNotesLike(conn, query);
        }

        private static List<SearchResultModel> SearchNotesFts(SqliteConnection conn, string query)
        {
            var results = new List<SearchResultModel>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT note_id, title, content FROM NotesFts WHERE NotesFts MATCH @search";
            cmd.Parameters.AddWithValue("@search", FtsSearchIndex.QuoteFtsLiteral(query));
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var title = reader.IsDBNull(1) || string.IsNullOrEmpty(reader.GetString(1))
                    ? "\u65e0\u6807\u9898"
                    : reader.GetString(1);
                var content = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var preview = content.Length > 50 ? content[..50] + "..." : content;
                results.Add(new SearchResultModel
                {
                    Type = ResultType.Note,
                    Name = title,
                    Detail = preview,
                    IconText = "\ud83d\udcdd",
                    ActionTarget = new NoteModel
                    {
                        Id = Guid.Parse(reader.GetString(0)),
                        Title = title,
                        Content = content
                    }
                });
            }
            return results;
        }

        private static List<SearchResultModel> SearchNotesLike(SqliteConnection conn, string query)
        {
            var results = new List<SearchResultModel>();
            var searchParam = $"%{query}%";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Title, Content FROM Notes WHERE Title LIKE @search OR Content LIKE @search";
            cmd.Parameters.AddWithValue("@search", searchParam);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var title = reader.IsDBNull(1) ? "\u65e0\u6807\u9898" : reader.GetString(1);
                var content = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var preview = content.Length > 50 ? content[..50] + "..." : content;
                results.Add(new SearchResultModel
                {
                    Type = ResultType.Note,
                    Name = title,
                    Detail = preview,
                    IconText = "\ud83d\udcdd",
                    ActionTarget = new NoteModel
                    {
                        Id = Guid.Parse(reader.GetString(0)),
                        Title = title,
                        Content = content
                    }
                });
            }
            return results;
        }
    }
}
