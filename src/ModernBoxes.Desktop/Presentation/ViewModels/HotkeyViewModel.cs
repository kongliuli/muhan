using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Presentation.Dialogs;

namespace ModernBoxes.Presentation.ViewModels
{
    public partial class HotkeyViewModel : ObservableObject
    {
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

        public HotkeyViewModel()
        {
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

            _isLoading = false;

            OnPropertyChanged(nameof(ShowHideHotkey));
            OnPropertyChanged(nameof(QuickNoteHotkey));
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
        }
    }
}
