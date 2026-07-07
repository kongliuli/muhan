using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ModernBoxes.Presentation.Dialogs;

namespace ModernBoxes.Presentation.Views
{
    public partial class UCnotes : UserControl
    {
        private UCnotesViewModel viewModel;

        public UCnotes()
        {
            InitializeComponent();
            viewModel = (UCnotesViewModel)DataContext;
        }

        private void MenuItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is NoteModel note)
            {
                BaseDialog baseDialog = new BaseDialog();
                baseDialog.SetTitle("编辑便签");
                baseDialog.SetHeight(380);
                baseDialog.SetContent(new AddNoteDialog(note));
                baseDialog.ShowDialog();
            }
        }

        private void MenuItem_Pin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is NoteModel note)
            {
                note.IsPinned = true;
                note.UpdatedAt = DateTime.Now;
                _ = viewModel.SaveNotes();
            }
        }

        private void MenuItem_Unpin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is NoteModel note)
            {
                note.IsPinned = false;
                note.UpdatedAt = DateTime.Now;
                _ = viewModel.SaveNotes();
            }
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is NoteModel note)
            {
                var parent = menuItem;
                DependencyObject current = menuItem;
                while (current != null)
                {
                    if (current is ContextMenu cm)
                    {
                        if (cm.PlacementTarget is FrameworkElement target)
                        {
                            AnimateDelete(target, note);
                            return;
                        }
                    }
                    current = VisualTreeHelper.GetParent(current);
                }
                if (parent.Parent is ContextMenu cm2 && cm2.PlacementTarget is FrameworkElement target2)
                {
                    AnimateDelete(target2, note);
                    return;
                }
                viewModel.Notes.Remove(note);
                _ = viewModel.SaveNotes();
            }
        }

        private void AnimateDelete(FrameworkElement element, NoteModel note)
        {
            var storyboard = new Storyboard();

            var opacityAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(opacityAnim, element);
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(opacityAnim);

            storyboard.Completed += (s, a) =>
            {
                viewModel.Notes.Remove(note);
                _ = viewModel.SaveNotes();
            };
            storyboard.Begin(element);
        }

        private void MenuItem_Color_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is String color)
            {
                MenuItem parent = menuItem.Parent as MenuItem;
                if (parent != null && parent.Tag is NoteModel note)
                {
                    note.Color = color;
                    note.UpdatedAt = DateTime.Now;
                    _ = viewModel.SaveNotes();
                }
            }
        }
    }
}
