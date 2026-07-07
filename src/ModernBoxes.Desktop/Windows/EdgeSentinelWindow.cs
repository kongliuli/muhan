using System;
using System.Windows;
using System.Windows.Media;

namespace ModernBoxes.Desktop.Windows
{
    /// <summary>
    /// 屏幕左/右边缘 4px 透明热区，用于悬停唤出卡片面板。
    /// </summary>
    public sealed class EdgeSentinelWindow : Window
    {
        public event Action? EdgeEnter;
        public event Action? EdgeLeave;

        public EdgeSentinelWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            Topmost = true;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            Width = 4;
            MouseEnter += (_, _) => EdgeEnter?.Invoke();
            MouseLeave += (_, _) => EdgeLeave?.Invoke();
        }

        public void Reposition(bool leftSide)
        {
            var area = SystemParameters.WorkArea;
            Height = area.Height;
            Top = area.Top;
            Left = leftSide ? area.Left : area.Right - Width;
        }
    }
}
