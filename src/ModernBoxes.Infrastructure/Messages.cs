using CommunityToolkit.Mvvm.Messaging.Messages;
using ModernBoxes.Core.Models;

namespace ModernBoxes.Infrastructure
{
    // Main menu
    public sealed record RefreshMenuMessage();
    public sealed record DeleteMenuItemMessage(string MenuName);

    // Applications
    public sealed record ApplicationChangedMessage();
    public sealed record AddApplicationMessage(ApplicationModel App);

    // Temp files
    public sealed record FileChangedMessage();
    public sealed record AddTempFileMessage(TempFileModel File);

    // Temp directories
    public sealed record DirChangedMessage();
    public sealed record AddTempDirMessage(TempDirModel Dir);

    // Component/Cards
    public sealed record RefreshComponentMessage();
    public sealed record ChangeCardHeightMessage(int CardId, double Height);
    public sealed record CheckedCardAppMessage(int CardID, bool IsShow);

    // Cross-cutting
    public sealed record CloseComponentMessage();

    // 主窗口 UI 请求/查询消息：替代 MainWindow 上的静态事件，让 ViewModel 与具体窗口解耦
    public sealed record SetWindowOpacityMessage(double Opacity);
    public sealed record SetComponentWidthMessage(double Width);
    public sealed class GetComponentWidthRequest : RequestMessage<double>;

    // Future use
    public sealed record NoteChangedMessage();
    public sealed record SettingsChangedMessage();
}
