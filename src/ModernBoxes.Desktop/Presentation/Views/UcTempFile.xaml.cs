using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Enums;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Views
{
    /// <summary>
    /// UcTempFile.xaml 的交互逻辑
    /// </summary>
    public partial class UcTempFile : UserControl
    {
        public UcTempFile()
        {
            InitializeComponent();
        }

        /// <summary>删除文件</summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is string filePath)
                WeakReferenceMessenger.Default.Send(filePath, "deleteFile");
        }

        /// <summary>查看文件属性</summary>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is string filePath)
            {
                BaseDialog baseDialog = new BaseDialog();
                baseDialog.SetTitle("文件属性");
                baseDialog.SetHeight(550);
                baseDialog.SetContent(new FilePropertyDialog(filePath));
                baseDialog.ShowDialog();
            }
        }

        /// <summary>从列表移除文件</summary>
        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is string filePath)
                WeakReferenceMessenger.Default.Send(filePath, "RemoveFile");
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            BaseDialog baseDialog = new BaseDialog();
            String? filePath = ((System.Array)e.Data.GetData(DataFormats.FileDrop))?.GetValue(0)?.ToString();
            if (filePath == null)
                return;

            if (File.Exists(filePath))
            {
                // 若是快捷方式，解析目标路径
                if (Path.GetExtension(filePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    filePath = GetIcon.getLinkTarget(filePath);

                baseDialog.SetTitle("添加文件");
                baseDialog.SetHeight(270);
                baseDialog.SetContent(new AddTempFileDialog(filePath));
            }
            else
            {
                baseDialog.SetTitle("提示");
                baseDialog.SetHeight(170);
                baseDialog.SetContent(new UcMessageDialog("请拖入文件哦，文件夹之类的还是不要拖进来啦", MessageDialogState.Info));
            }

            baseDialog.ShowDialog();
        }
    }
}
