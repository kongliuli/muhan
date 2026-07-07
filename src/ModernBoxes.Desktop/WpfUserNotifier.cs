using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Sdk.Plugins;
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

    public bool ShowConfirm(string title, string message)
    {
        return System.Windows.MessageBox.Show(
            message,
            title,
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Information) == System.Windows.MessageBoxResult.Yes;
    }
}
