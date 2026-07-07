using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Infrastructure.Ai;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Sdk.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
namespace ModernBoxes.Presentation.ViewModels
{
    public class SearchViewModel : ObservableObject
    {
        public static readonly IReadOnlyList<string> ActionKeywords =
            new[] { "app", "menu", "dir", "file", "note" };

        private readonly ISearchService _searchService;
        private readonly AiPromptService _ai;
        private readonly IUserNotifier _notifier;
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
            set
            {
                if (SetProperty(ref _searchResults, value))
                    ResetSelection();
            }
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
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

        public SearchViewModel(ISearchService searchService, AiPromptService ai, IUserNotifier notifier)
        {
            _searchService = searchService;
            _ai = ai;
            _notifier = notifier;

            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _debounceTimer.Tick += DebounceTimer_Tick;

            SearchResults.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(HasNoResults));
                ResetSelection();
            };

            SelectResultCommand = new RelayCommand(o =>
            {
                if (o is SearchResultModel result)
                    ExecuteResult(result);
            });

            ClearSearchCommand = new RelayCommand(_ => SearchText = "");
        }

        public void ResetForLaunch()
        {
            SearchText = string.Empty;
            SearchResults.Clear();
            SelectedIndex = -1;
            IsSearching = false;
        }

        public bool TryCompleteActionKeyword()
        {
            var text = SearchText ?? string.Empty;
            if (text.Contains(' '))
                return false;

            var partial = text.TrimStart();
            if (string.IsNullOrEmpty(partial))
                return false;

            var match = ActionKeywords
                .FirstOrDefault(k => k.StartsWith(partial, StringComparison.OrdinalIgnoreCase)
                                     && !k.Equals(partial, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                return false;

            SearchText = match + " ";
            return true;
        }

        public void MoveSelection(int delta)
        {
            if (SearchResults.Count == 0)
            {
                SelectedIndex = -1;
                return;
            }

            var next = SelectedIndex < 0 ? 0 : SelectedIndex + delta;
            if (next < 0)
                next = 0;
            if (next >= SearchResults.Count)
                next = SearchResults.Count - 1;
            SelectedIndex = next;
        }

        private void ResetSelection()
        {
            SelectedIndex = SearchResults.Count > 0 ? 0 : -1;
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
                if (results.Count == 0 && _ai.IsAvailable)
                    results.Add(CreateAiFallbackResult(keyword));
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
                _searchService.RecordSelection(result);
                if (result.ExecuteAction != null)
                {
                    result.ExecuteAction();
                    return;
                }

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

        public void ExecuteSelectedResult()
        {
            if (SelectedIndex >= 0 && SelectedIndex < SearchResults.Count)
                ExecuteResult(SearchResults[SelectedIndex]);
            else if (SearchResults.Count > 0)
                ExecuteResult(SearchResults[0]);
        }

        public void SelectFirstResult() => ExecuteSelectedResult();

        private SearchResultModel CreateAiFallbackResult(string keyword) =>
            new()
            {
                Name = $"问 AI：{keyword}",
                Detail = "未找到本地结果，使用 AI 回答",
                IconText = "🤖",
                Score = -1,
                ExecuteAction = () =>
                {
                    _ = RunAiSearchAsync(keyword);
                    return true;
                },
            };

        private async Task RunAiSearchAsync(string keyword)
        {
            var answer = await _ai.AnswerSearchAsync(keyword);
            if (answer == null)
                _notifier.ShowWarning("问 AI", "未配置 API 密钥或请求失败");
            else
                AiResultDialog.Show("问 AI", answer);
        }
    }
}
