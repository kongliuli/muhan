using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Enums;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.ViewModels;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Views
{
    /// <summary>
    /// UCtempDirectory.xaml 的交互逻辑
    /// </summary>
    public partial class UCtempDirectory : UserControl
    {
        public UCtempDirectory()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is string path)
                WeakReferenceMessenger.Default.Send(path, "detempdir");
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is string path)
            {
                BaseDialog baseDialog = new BaseDialog();
                baseDialog.SetTitle("文件夹属性");
                baseDialog.SetHeight(500);
                baseDialog.SetContent(new DirInformationDialog(path));
                baseDialog.Show();
            }
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            String? dirPath = ((System.Array)e.Data.GetData(DataFormats.FileDrop))?.GetValue(0)?.ToString();
            if (dirPath == null)
                return;

            BaseDialog baseDialog = new BaseDialog();
            if (Directory.Exists(dirPath))
            {
                baseDialog.SetTitle("添加文件夹");
                baseDialog.SetHeight(255);
                baseDialog.SetContent(new AddTempDirDialog(dirPath));
            }
            else
            {
                baseDialog.SetTitle("提示");
                baseDialog.SetHeight(170);
                baseDialog.SetContent(new UcMessageDialog("抱歉，只有文件夹才可以拖入哦", MessageDialogState.Info));
            }
            baseDialog.ShowDialog();
        }

        /// <summary>
        /// 移除临时文件夹（仅从列表移除，不删除磁盘文件）
        /// </summary>
        private void RemoveTempDir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is string dirPath)
                WeakReferenceMessenger.Default.Send(dirPath, "RemoveTempDir");
        }
    }
}
