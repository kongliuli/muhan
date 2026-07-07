using ModernBoxes.Core.Models;
using System.Collections.Generic;

namespace ModernBoxes.Core.Interfaces.Repositories
{
    public interface INoteRepository
    {
        void SyncNotes(IEnumerable<NoteModel> notes);
        List<SearchResultModel> SearchNotes(string query);
    }
}
