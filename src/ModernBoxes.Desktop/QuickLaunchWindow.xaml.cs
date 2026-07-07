using ModernBoxes.Presentation.ViewModels;
using ModernBoxes.Presentation.Views;
using System.Windows;
using System.Windows.Input;

namespace ModernBoxes.Desktop
{
    public partial class QuickLaunchWindow : Window
    {
        private readonly SearchViewModel _searchViewModel;

        public QuickLaunchWindow(SearchViewModel searchViewModel)
        {
            _searchViewModel = searchViewModel;
            Title = "木函";
            Width = 640;
            Height = 420;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = System.Windows.Media.Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;

            var search = new UCSearch { DataContext = searchViewModel };
            Content = new System.Windows.Controls.Border
            {
                CornerRadius = new CornerRadius(12),
                Background = (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("RegionBrush"),
                Child = search,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 24,
                    ShadowDepth = 0,
                    Opacity = 0.35,
                },
            };

            PreviewKeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Hide();
                    e.Handled = true;
                }
            };

            Deactivated += (_, _) => Hide();
        }

        public void ShowAndFocus()
        {
            _searchViewModel.SearchText = string.Empty;
            _searchViewModel.SearchResults.Clear();
            if (!IsVisible)
                Show();
            Activate();
            Focus();
        }
    }
}
