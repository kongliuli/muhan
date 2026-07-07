using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ModernBoxes.Presentation.Views
{
    public partial class UCSearch : UserControl
    {
        private readonly SearchViewModel _viewModel;

        public UCSearch()
        {
            InitializeComponent();
            _viewModel = (SearchViewModel)DataContext;
            SearchTextBox.GotFocus += SearchTextBox_GotFocus;
            SearchTextBox.LostFocus += SearchTextBox_LostFocus;
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)FindResource("SearchFocusIn");
            storyboard.Begin(SearchBorder);
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)FindResource("SearchFocusOut");
            storyboard.Begin(SearchBorder);
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.SelectFirstResult();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel.SearchText = "";
                e.Handled = true;
            }
        }

        private void ResultsListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ResultsListBox.SelectedItem is SearchResultModel result)
                    _viewModel.ExecuteResult(result);
                e.Handled = true;
            }
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem is SearchResultModel result)
                _viewModel.ExecuteResult(result);
        }
    }
}
