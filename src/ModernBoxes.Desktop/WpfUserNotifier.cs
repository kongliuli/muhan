using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.Views;

namespace ModernBoxes.Desktop;

public class WpfUserNotifier : IUserNotifier
{
    public void ShowWarning(string title, string message)
    {
        var dialog = new BaseDialog();
        dialog.SetTitle(title);
        dialog.SetHeight(170);
        dialog.SetContent(new UcMessageDialog(message, MessageDialogState.waring));
        dialog.ShowDialog();
    }
}
