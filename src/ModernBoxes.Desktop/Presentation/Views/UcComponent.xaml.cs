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

        private void UserControl_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Console.WriteLine(ScrollViewer.VerticalOffset + "  " + e.Delta);
            ScrollViewer.ScrollToVerticalOffsetWithAnimation(ScrollViewer.VerticalOffset - e.Delta);
        }
    }
}
