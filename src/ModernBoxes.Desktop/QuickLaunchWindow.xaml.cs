using ModernBoxes.Presentation.ViewModels;
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
            InitializeComponent();
            SearchPanel.DataContext = searchViewModel;

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
            _searchViewModel.ResetForLaunch();
            if (!IsVisible)
                Show();
            Activate();
            SearchPanel.FocusSearchBox();
        }
    }
}
