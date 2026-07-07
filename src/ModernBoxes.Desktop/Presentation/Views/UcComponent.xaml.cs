using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.View;
using System;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Views
{
    public partial class UcComponent : UserControl
    {
        public UcComponent()
        {
            InitializeComponent();
        }

        private void ManageCards_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new BaseDialog();
            dialog.SetTitle("管理小组件");
            dialog.setDialogSize(565, 400);
            dialog.SetContent(new UcAddCardApplicationDialog());
            dialog.ShowDialog();
        }

        private void UserControl_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Console.WriteLine(ScrollViewer.VerticalOffset + "  " + e.Delta);
            ScrollViewer.ScrollToVerticalOffsetWithAnimation(ScrollViewer.VerticalOffset - e.Delta);
        }
    }
}
