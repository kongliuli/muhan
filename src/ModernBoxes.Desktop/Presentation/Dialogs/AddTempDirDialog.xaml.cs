using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Dialogs
{
    /// <summary>AddTempDirDialog.xaml 的交互逻辑</summary>
    ///

    public partial class AddTempDirDialog : UserControl
    {
        public AddTempDirDialog()
        {
            InitializeComponent();
        }

        public AddTempDirDialog(String DirPath)
        {
            InitializeComponent();
            var persistence = App.AppHost!.Services.GetRequiredService<IPersistenceProvider>();
            this.DataContext = new AddTempDirViewModel(persistence, DirPath);
        }

        public void ChangeToNewDirUI()
        {
            SP_isRef.Visibility = Visibility.Collapsed;
            TB_DirRef.Visibility = Visibility.Collapsed;
            InfoElement.SetPlaceholder(TB_DirPath, "文件夹名称");
            btn_ChooseDirPath.Visibility = Visibility.Collapsed;
        }
    }
}