using ModernBoxes.Presentation.Views;
using System.Windows;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Dialogs
{
    internal static class AiResultDialog
    {
        public static void Show(string title, string body)
        {
            var text = new TextBox
            {
                Text = body,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(0),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 360,
                Width = 460,
                Margin = new Thickness(10),
            };

            var dialog = new BaseDialog();
            dialog.SetTitle(title);
            dialog.SetHeight(460);
            dialog.SetContent(text);
            dialog.ShowDialog();
        }
    }
}
