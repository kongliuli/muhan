using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Services
{
    public class NoteCardService : INoteCardService
    {
        private readonly IPersistenceProvider _persistence;
        private readonly INoteRepository _noteRepo;
        private List<NoteModel> _notes = new();

        public NoteCardService(IPersistenceProvider persistence, INoteRepository noteRepo)
        {
            _persistence = persistence;
            _noteRepo = noteRepo;
        }

        public async Task LoadAsync()
        {
            var loaded = await _persistence.LoadAsync<NoteModel>("notes");
            _notes = loaded.ToList();
        }

        public void AddNote(NoteModel note)
        {
            _notes.Add(note);
            SaveNotes(_notes);
        }

        public void UpdateNote(NoteModel note)
        {
            var existing = _notes.FirstOrDefault(n => n.Id == note.Id);
            if (existing != null)
            {
                existing.Title = note.Title;
                existing.Content = note.Content;
                existing.Color = note.Color;
                existing.UpdatedAt = DateTime.Now;
                SaveNotes(_notes);
            }
        }

        public void DeleteNote(Guid id)
        {
            _notes.RemoveAll(n => n.Id == id);
            SaveNotes(_notes);
        }

        public IEnumerable<NoteModel> GetAllNotes()
        {
            return _notes;
        }

        public IEnumerable<NoteModel> SearchNotes(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _notes;
            return _notes.Where(n =>
                n.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                n.Content.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        public void SaveNotes(IEnumerable<NoteModel> notes)
        {
            _ = _persistence.SaveAsync("notes", notes);
        }
    }
}
