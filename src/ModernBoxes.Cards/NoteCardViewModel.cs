using ModernBoxes.Core.Interfaces;
using ModernBoxes.Sdk.Plugins;
using ModernBoxes.Core.Models;
using System.Threading.Tasks;

namespace ModernBoxes.Cards;

[CardExport("便签", Author = "ModernBoxes", Version = "3.0", Description = "便签卡片-快速记录便签")]
public class NoteCardViewModel : CardBase<NoteModel>
{
    private readonly INoteCardService _service;

    public NoteCardViewModel(INoteCardService service) : base(new NoteModel())
    {
        _service = service;
        CardID = "note";
        CardHeight = 200;
        Preview = "pack://application:,,,/Resource/image/previews/notes.png";
        IsChecked = false;
    }

    public override Task LoadAsync()
    {
        CardContent = _service.GetAllNotes();
        return Task.CompletedTask;
    }

    public override Task RefreshAsync() => LoadAsync();
}
