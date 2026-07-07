using ModernBoxes.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernBoxes.Core.Interfaces
{
    public interface INoteCardService
    {
        void AddNote(NoteModel note);
        void UpdateNote(NoteModel note);
        void DeleteNote(Guid id);
        IEnumerable<NoteModel> GetAllNotes();
        IEnumerable<NoteModel> SearchNotes(string query);
        void SaveNotes(IEnumerable<NoteModel> notes);
        Task LoadAsync();
    }
}
