using ModernBoxes.Core.Interfaces;
using ModernBoxes.Sdk.Plugins;
using ModernBoxes.Core.Models;
using System.Threading.Tasks;

namespace ModernBoxes.Cards;

[CardExport("临时文件", Author = "ModernBoxes", Version = "3.0", Description = "临时文件卡片-管理临时文件")]
public class FileCardViewModel : CardBase<TempFileModel>
{
    private readonly IFileCardService _service;

    public FileCardViewModel(IFileCardService service) : base(new TempFileModel())
    {
        _service = service;
        CardID = "file";
        CardHeight = 300;
        Preview = "pack://application:,,,/Resource/image/previews/file.png";
        IsChecked = false;
    }

    public override async Task LoadAsync()
    {
        CardContent = await _service.GetAllFiles();
    }

    public override Task RefreshAsync() => LoadAsync();
}
