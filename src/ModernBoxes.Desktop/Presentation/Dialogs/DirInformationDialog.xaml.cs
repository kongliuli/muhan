using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Dialogs
{
    public partial class DirInformationDialog : UserControl
    {
        public DirInformationDialog(String path)
        {
            InitializeComponent();
            var persistence = App.AppHost!.Services.GetRequiredService<IPersistenceProvider>();
            this.DataContext = new DirInformationDialogViewModel(persistence, path);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send<Boolean>(true, "IsCloseBaseDialog");
        }
    }
}