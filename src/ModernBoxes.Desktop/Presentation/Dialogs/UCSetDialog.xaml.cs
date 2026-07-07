using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Enums;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernBoxes.Presentation.Dialogs
{
    public partial class UCSetDialog : UserControl
    {
        private readonly UCSetDialogViewModel _viewModel;

        private bool _recordingShowHide;
        private bool _recordingQuickNote;

        public UCSetDialog()
        {
            InitializeComponent();
            _viewModel = (UCSetDialogViewModel)DataContext;
            InitSliderBounds();
            WeakReferenceMessenger.Default.Register<Boolean>(this, "IsSaveConfigData", SaveConfigData);
        }

        private void InitSliderBounds()
        {
            S_ComponentWidth.Maximum = 420;
        }

        private void SaveConfigData(bool obj)
        {
            _viewModel.Save();
        }

        private void RB_light_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Theme = Theme.light;
        }

        private void RB_Dark_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Theme = Theme.dark;
        }

        private void layoutinleft_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CommentLayoutDirection = CommentLayout.left;
        }

        private void layoutinright_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CommentLayoutDirection = CommentLayout.right;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AutoStart = true;
        }

        private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        {
            _viewModel.AutoStart = false;
        }

        private void RB_HoverOpen_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsHover = RB_HoverOpen.IsChecked ?? false;
        }

        private void RB_HoverClose_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsHover = RB_HoverOpen.IsChecked ?? false;
        }

        private void TbShowHideHotkey_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_recordingShowHide)
            {
                _recordingShowHide = true;
                TbShowHideHotkey.Text = "请按下快捷键...";
                TbShowHideHotkey.Focus();
            }
        }

        private void TbShowHideHotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_recordingShowHide) return;

            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                CancelRecordingShowHide();
                return;
            }

            var combo = BuildHotkeyString(e);
            if (!string.IsNullOrEmpty(combo))
            {
                _viewModel.ShowHideHotkey = combo;
                TbShowHideHotkey.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
            _recordingShowHide = false;
            TbShowHideHotkey.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void TbShowHideHotkey_LostFocus(object sender, RoutedEventArgs e)
        {
            CancelRecordingShowHide();
        }

        private void CancelRecordingShowHide()
        {
            _recordingShowHide = false;
            TbShowHideHotkey.Text = _viewModel.ShowHideHotkey;
        }

        private void TbQuickNoteHotkey_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_recordingQuickNote)
            {
                _recordingQuickNote = true;
                TbQuickNoteHotkey.Text = "请按下快捷键...";
                TbQuickNoteHotkey.Focus();
            }
        }

        private void TbQuickNoteHotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_recordingQuickNote) return;

            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                CancelRecordingQuickNote();
                return;
            }

            var combo = BuildHotkeyString(e);
            if (!string.IsNullOrEmpty(combo))
            {
                _viewModel.QuickNoteHotkey = combo;
                TbQuickNoteHotkey.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
            _recordingQuickNote = false;
            TbQuickNoteHotkey.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void TbQuickNoteHotkey_LostFocus(object sender, RoutedEventArgs e)
        {
            CancelRecordingQuickNote();
        }

        private void CancelRecordingQuickNote()
        {
            _recordingQuickNote = false;
            TbQuickNoteHotkey.Text = _viewModel.QuickNoteHotkey;
        }

        private static string BuildHotkeyString(KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.None) return string.Empty;

            bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            bool alt = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
            bool win = Keyboard.Modifiers.HasFlag(ModifierKeys.Windows);

            bool isModifierKey = key == Key.LeftCtrl || key == Key.RightCtrl ||
                                 key == Key.LeftShift || key == Key.RightShift ||
                                 key == Key.LeftAlt || key == Key.RightAlt ||
                                 key == Key.System || key == Key.LWin || key == Key.RWin;

            if (isModifierKey) return string.Empty;

            int modCount = (ctrl ? 1 : 0) + (shift ? 1 : 0) + (alt ? 1 : 0) + (win ? 1 : 0);
            if (modCount < 2) return string.Empty;

            var keyName = key.ToString();
            if (key >= Key.D0 && key <= Key.D9)
                keyName = keyName.Substring(1);
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                return string.Empty;

            var parts = new System.Text.StringBuilder();
            if (ctrl) parts.Append("Ctrl+");
            if (shift) parts.Append("Shift+");
            if (alt) parts.Append("Alt+");
            if (win) parts.Append("Win+");
            parts.Append(keyName);
            return parts.ToString();
        }

        private void BtnClearShowHide_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowHideHotkey = "Ctrl+Shift+M";
        }

        private void BtnClearQuickNote_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.QuickNoteHotkey = "Ctrl+Shift+N";
        }
    }
}
