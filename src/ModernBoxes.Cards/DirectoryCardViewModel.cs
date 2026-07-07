using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using System.Threading.Tasks;

namespace ModernBoxes.Cards;

[CardExport("临时目录", Author = "ModernBoxes", Version = "3.0", Description = "临时目录卡片-管理临时文件夹")]
public class DirectoryCardViewModel : CardBase<TempDirModel>
{
    private readonly IDirectoryCardService _service;

    public DirectoryCardViewModel(IDirectoryCardService service) : base(new TempDirModel())
    {
        _service = service;
        CardID = "dir";
        CardHeight = 250;
        Preview = "pack://application:,,,/Resource/image/previews/dir.png";
        IsChecked = false;
    }

    public override async Task LoadAsync()
    {
        CardContent = await _service.GetAllDirectories();
    }

    public override Task RefreshAsync() => LoadAsync();
}
