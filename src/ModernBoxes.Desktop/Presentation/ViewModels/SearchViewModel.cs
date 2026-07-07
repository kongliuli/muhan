using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
namespace ModernBoxes.Presentation.ViewModels
{
    public class SearchViewModel : ObservableObject
    {
        private readonly ISearchService _searchService;
        private readonly DispatcherTimer _debounceTimer;

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    OnSearchTextChanged();
            }
        }

        private ObservableCollection<SearchResultModel> _searchResults = new();
        public ObservableCollection<SearchResultModel> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (SetProperty(ref _isSearching, value))
                {
                    OnPropertyChanged(nameof(HasResults));
                    OnPropertyChanged(nameof(HasNoResults));
                }
            }
        }

        public bool HasResults => !IsSearching && !string.IsNullOrWhiteSpace(SearchText) && SearchResults.Count > 0;
        public bool HasNoResults => !IsSearching && !string.IsNullOrWhiteSpace(SearchText) && SearchResults.Count == 0;

        public ICommand SelectResultCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public SearchViewModel(ISearchService searchService)
        {
            _searchService = searchService;

            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _debounceTimer.Tick += DebounceTimer_Tick;

            SearchResults.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(HasNoResults));
            };

            SelectResultCommand = new RelayCommand(o =>
            {
                if (o is SearchResultModel result)
                    ExecuteResult(result);
            });

            ClearSearchCommand = new RelayCommand(_ => SearchText = "");
        }

        private void OnSearchTextChanged()
        {
            _debounceTimer.Stop();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResults.Clear();
                IsSearching = false;
                return;
            }
            _debounceTimer.Start();
        }

        private async void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            _debounceTimer.Stop();
            await SearchAsync();
        }

        public async Task SearchAsync()
        {
            var keyword = SearchText.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                SearchResults.Clear();
                IsSearching = false;
                return;
            }

            IsSearching = true;
            try
            {
                var results = await _searchService.SearchAsync(keyword);
                SearchResults = new ObservableCollection<SearchResultModel>(results);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "SearchAsync failed");
            }

            IsSearching = false;
        }

        public void ExecuteResult(SearchResultModel result)
        {
            try
            {
                switch (result.Type)
                {
                    case ResultType.Application:
                        if (result.ActionTarget is ApplicationModel app && File.Exists(app.AppPath))
                            Process.Start(new ProcessStartInfo(app.AppPath) { UseShellExecute = true });
                        break;
                    case ResultType.File:
                        if (result.ActionTarget is TempFileModel file && File.Exists(file.FilePath))
                            Process.Start(new ProcessStartInfo(file.FilePath) { UseShellExecute = true });
                        break;

                    case ResultType.Directory:
                        if (result.ActionTarget is TempDirModel dir && Directory.Exists(dir.TempDirPath))
                            Process.Start("explorer.exe", dir.TempDirPath);
                        break;

                    case ResultType.Note:
                        if (result.ActionTarget is NoteModel note)
                        {
                            BaseDialog baseDialog = new BaseDialog();
                            baseDialog.SetTitle("编辑便签");
                            baseDialog.SetHeight(380);
                            baseDialog.SetContent(new AddNoteDialog(note));
                            baseDialog.ShowDialog();
                        }
                        break;

                    case ResultType.Menu:
                        if (result.ActionTarget is MenuModel menu)
                        {
                            if (File.Exists(menu.Target))
                                Process.Start(new ProcessStartInfo(menu.Target) { UseShellExecute = true });
                            else if (Directory.Exists(menu.Target))
                                Process.Start("explorer.exe", menu.Target);
                            else if (menu.MenuName == "组件应用")
                                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(true, "isShow");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"ExecuteResult failed for {result.Name}");
            }
        }

        public void SelectFirstResult()
        {
            if (SearchResults.Count > 0)
                ExecuteResult(SearchResults[0]);
        }
    }
}
