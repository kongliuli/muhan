using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
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
    public partial class UCusedApplications : UserControl
    {
        public UCusedApplications()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Type type = sender.GetType();
            PropertyInfo? propertyInfo = type.GetProperty("CommandParameter");
            String? path = propertyInfo?.GetValue(sender)?.ToString();
            if (path != null)
                WeakReferenceMessenger.Default.Send<String>(path, "path");
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            BaseDialog baseDialog = new BaseDialog();
            String? applicationPath = ((System.Array)e.Data.GetData(DataFormats.FileDrop))?.GetValue(0)?.ToString();
            if (applicationPath == null)
                return;

            var ext = applicationPath.Substring(applicationPath.LastIndexOf('.') + 1);
            if (ext == "exe" || ext == "lnk")
            {
                if (ext == "lnk" && File.Exists(applicationPath))
                    applicationPath = GetIcon.getLinkTarget(applicationPath);

                baseDialog.SetTitle("添加应用");
                baseDialog.SetHeight(270);
                baseDialog.SetContent(new UCAddApplicationDialog(applicationPath));
            }
            else
            {
                baseDialog.SetTitle("提示");
                baseDialog.SetContent(new UcMessageDialog("请拖入应用程序或可执行文件的快捷方式", MessageDialogState.Info));
                baseDialog.SetHeight(170);
            }
            baseDialog.ShowDialog();
        }
    }
}
