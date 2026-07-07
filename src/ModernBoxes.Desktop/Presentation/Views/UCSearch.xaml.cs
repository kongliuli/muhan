using ModernBoxes.Core.Models;
using ModernBoxes.Presentation.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernBoxes.Presentation.Views
{
    public partial class UCSearch : UserControl
    {
        private SearchViewModel? _viewModel;

        public UCSearch()
        {
            InitializeComponent();
            Loaded += (_, _) => _viewModel = DataContext as SearchViewModel;
            DataContextChanged += (_, _) => _viewModel = DataContext as SearchViewModel;
        }

        public void FocusSearchBox()
        {
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel == null)
                return;

            switch (e.Key)
            {
                case Key.Enter:
                    _viewModel.ExecuteSelectedResult();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    _viewModel.SearchText = "";
                    e.Handled = true;
                    break;
                case Key.Down:
                    _viewModel.MoveSelection(1);
                    if (_viewModel.SelectedIndex >= 0 && _viewModel.SelectedIndex < _viewModel.SearchResults.Count)
                        ResultsListBox.ScrollIntoView(_viewModel.SearchResults[_viewModel.SelectedIndex]);
                    e.Handled = true;
                    break;
                case Key.Up:
                    _viewModel.MoveSelection(-1);
                    if (_viewModel.SelectedIndex >= 0 && _viewModel.SelectedIndex < _viewModel.SearchResults.Count)
                        ResultsListBox.ScrollIntoView(_viewModel.SearchResults[_viewModel.SelectedIndex]);
                    e.Handled = true;
                    break;
                case Key.Tab:
                    if (_viewModel.TryCompleteActionKeyword())
                        e.Handled = true;
                    break;
            }
        }

        private void ResultsListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel == null)
                return;

            if (e.Key == Key.Enter)
            {
                if (ResultsListBox.SelectedItem is SearchResultModel result)
                    _viewModel.ExecuteResult(result);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                FocusSearchBox();
                e.Handled = true;
            }
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null)
                return;

            if (ResultsListBox.SelectedItem is SearchResultModel result)
                _viewModel.ExecuteResult(result);
        }
    }
}
