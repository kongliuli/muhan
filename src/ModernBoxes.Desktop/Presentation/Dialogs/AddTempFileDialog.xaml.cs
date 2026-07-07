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
    /// <summary>AddTempFileDialog.xaml 的交互逻辑</summary>
    public partial class AddTempFileDialog : UserControl
    {
        public AddTempFileDialog()
        {
            InitializeComponent();
        }

        public AddTempFileDialog(String FileName)
        {
            InitializeComponent();
            var persistence = App.AppHost!.Services.GetRequiredService<IPersistenceProvider>();
            this.DataContext = new AddTempFileDialogViewModel(persistence, FileName);
        }

        public void ChangeToNewDirUI()
        {
            SP_isRef.Visibility = Visibility.Collapsed;
            TB_DirRef.Visibility = Visibility.Collapsed;
            InfoElement.SetPlaceholder(TB_DirPath, "文件名");
            btn_ChooseDirPath.Visibility = Visibility.Collapsed;
        }
    }
}