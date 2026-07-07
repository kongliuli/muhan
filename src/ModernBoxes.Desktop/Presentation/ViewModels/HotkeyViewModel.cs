using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Desktop;
using ModernBoxes.Infrastructure;
using ModernBoxes.Infrastructure.Ai;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.ViewModels;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Sdk.Plugins;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Presentation.ViewModels
{
    public partial class HotkeyViewModel : ObservableObject
    {
        private readonly SearchViewModel _searchViewModel;
        private readonly AiPromptService _ai;
        private readonly IUserNotifier _notifier;
        private QuickLaunchWindow? _quickLaunchWindow;
        private bool _isLoading;

        private string _showHideHotkey = "Ctrl+Shift+M";
        public string ShowHideHotkey
        {
            get => _showHideHotkey;
            set
            {
                if (SetProperty(ref _showHideHotkey, value) && !_isLoading)
                {
                    ConfigHelper.setConfig("ShowHideHotkey", value);
                    RegisterAll();
                }
            }
        }

        private string _quickNoteHotkey = "Ctrl+Shift+N";
        public string QuickNoteHotkey
        {
            get => _quickNoteHotkey;
            set
            {
                if (SetProperty(ref _quickNoteHotkey, value) && !_isLoading)
                {
                    ConfigHelper.setConfig("QuickNoteHotkey", value);
                    RegisterAll();
                }
            }
        }

        private string _paletteHotkey = "Alt+Space";
        public string PaletteHotkey
        {
            get => _paletteHotkey;
            set
            {
                if (SetProperty(ref _paletteHotkey, value) && !_isLoading)
                {
                    ConfigHelper.setConfig("PaletteHotkey", value);
                    RegisterAll();
                }
            }
        }

        private string _translateClipboardHotkey = "Ctrl+Shift+T";
        public string TranslateClipboardHotkey
        {
            get => _translateClipboardHotkey;
            set
            {
                if (SetProperty(ref _translateClipboardHotkey, value) && !_isLoading)
                {
                    ConfigHelper.setConfig("TranslateClipboardHotkey", value);
                    RegisterAll();
                }
            }
        }

        public HotkeyViewModel(SearchViewModel searchViewModel, AiPromptService ai, IUserNotifier notifier)
        {
            _searchViewModel = searchViewModel;
            _ai = ai;
            _notifier = notifier;
            Load();
        }

        public void Load()
        {
            _isLoading = true;

            var showHide = ConfigHelper.getConfig("ShowHideHotkey");
            if (!string.IsNullOrEmpty(showHide))
                _showHideHotkey = showHide;

            var quickNote = ConfigHelper.getConfig("QuickNoteHotkey");
            if (!string.IsNullOrEmpty(quickNote))
                _quickNoteHotkey = quickNote;

            var palette = ConfigHelper.getConfig("PaletteHotkey");
            if (!string.IsNullOrEmpty(palette))
                _paletteHotkey = palette;

            var translate = ConfigHelper.getConfig("TranslateClipboardHotkey");
            if (!string.IsNullOrEmpty(translate))
                _translateClipboardHotkey = translate;

            _isLoading = false;

            OnPropertyChanged(nameof(ShowHideHotkey));
            OnPropertyChanged(nameof(QuickNoteHotkey));
            OnPropertyChanged(nameof(PaletteHotkey));
            OnPropertyChanged(nameof(TranslateClipboardHotkey));
        }

        public void RegisterAll()
        {
            var hk = HotkeyManager.Instance;
            hk.UnregisterAll();

            if (HotkeyManager.TryParseHotkeyString(ShowHideHotkey, out var showMod, out var showKey))
            {
                hk.RegisterHotkeyAction(showMod, showKey, () =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var main = System.Windows.Application.Current.MainWindow;
                        if (main == null) return;
                        if (main.Visibility == System.Windows.Visibility.Visible)
                            main.Hide();
                        else
                        {
                            main.Show();
                            main.Activate();
                            main.WindowState = System.Windows.WindowState.Normal;
                        }
                    });
                });
            }

            if (HotkeyManager.TryParseHotkeyString(QuickNoteHotkey, out var noteMod, out var noteKey))
            {
                hk.RegisterHotkeyAction(noteMod, noteKey, () =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var dialog = new BaseDialog();
                        dialog.SetTitle("新建便签");
                        dialog.SetHeight(380);
                        dialog.SetContent(new AddNoteDialog());
                        dialog.ShowDialog();
                    });
                });
            }

            if (HotkeyManager.TryParseHotkeyString(PaletteHotkey, out var paletteMod, out var paletteKey))
            {
                hk.RegisterHotkeyAction(paletteMod, paletteKey, () =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(ToggleQuickLaunch);
                });
            }

            if (HotkeyManager.TryParseHotkeyString(TranslateClipboardHotkey, out var translateMod, out var translateKey))
            {
                hk.RegisterHotkeyAction(translateMod, translateKey, () =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => _ = TranslateClipboardAsync());
                });
            }
        }

        private async Task TranslateClipboardAsync()
        {
            if (!_ai.IsAvailable)
            {
                _notifier.ShowWarning("剪贴板翻译", "未配置 API 密钥");
                return;
            }

            if (!Clipboard.ContainsText())
            {
                _notifier.ShowWarning("剪贴板翻译", "剪贴板中没有文本");
                return;
            }

            var text = Clipboard.GetText()?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                _notifier.ShowWarning("剪贴板翻译", "剪贴板中没有文本");
                return;
            }

            var translated = await _ai.TranslateAsync(text);
            if (translated == null)
            {
                _notifier.ShowWarning("剪贴板翻译", "翻译失败");
                return;
            }

            Clipboard.SetText(translated);
            AiResultDialog.Show("剪贴板翻译", translated);
        }

        public void ShowQuickLaunchPalette()
        {
            _quickLaunchWindow ??= new QuickLaunchWindow(_searchViewModel);
            _quickLaunchWindow.ShowAndFocus();
        }

        public void HideQuickLaunchPalette()
        {
            if (_quickLaunchWindow?.IsVisible == true)
                _quickLaunchWindow.Hide();
        }

        private void ToggleQuickLaunch()
        {
            _quickLaunchWindow ??= new QuickLaunchWindow(_searchViewModel);
            if (_quickLaunchWindow.IsVisible)
                _quickLaunchWindow.Hide();
            else
                _quickLaunchWindow.ShowAndFocus();
        }
    }
}
