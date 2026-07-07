using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Dialogs
{
    /// <summary>
    /// FilePropertyDialog.xaml µÄ½»»¥Âß¼­
    /// </summary>
    public partial class FilePropertyDialog : UserControl
    {
        public FilePropertyDialog(String FilePath)
        {
            InitializeComponent();
            this.DataContext = new FilePropertyDialogViewModel(FilePath);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(true, "IsCloseBaseDialog");
        }
    }
}