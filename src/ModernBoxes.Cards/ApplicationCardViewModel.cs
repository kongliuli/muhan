using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using System.Threading.Tasks;

namespace ModernBoxes.Cards;

[CardExport("应用", Author = "ModernBoxes", Version = "3.0", Description = "应用卡片-快速启动常用应用")]
public class ApplicationCardViewModel : CardBase<ApplicationModel>
{
    private readonly IApplicationCardService _service;

    public ApplicationCardViewModel(IApplicationCardService service) : base(new ApplicationModel())
    {
        _service = service;
        CardID = "app";
        CardHeight = 350;
        Preview = "pack://application:,,,/Resource/image/previews/application.png";
        IsChecked = false;
    }

    public override Task LoadAsync()
    {
        CardContent = _service.GetAllApplications();
        return Task.CompletedTask;
    }

    public override Task RefreshAsync() => LoadAsync();
}
