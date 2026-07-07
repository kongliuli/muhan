using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
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

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel == null)
                return;

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
            if (_viewModel == null)
                return;

            if (e.Key == Key.Enter)
            {
                if (ResultsListBox.SelectedItem is SearchResultModel result)
                    _viewModel.ExecuteResult(result);
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
