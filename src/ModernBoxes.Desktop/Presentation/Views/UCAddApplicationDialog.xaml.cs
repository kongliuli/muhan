using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Views
{
    /// <summary>UCAddApplicationDialog.xaml 的交互逻辑</summary>
    public partial class UCAddApplicationDialog : UserControl
    {
        public UCAddApplicationDialog()
        {
            InitializeComponent();
        }

        public UCAddApplicationDialog(string ApplicationPath)
        {
            InitializeComponent();
            var persistence = App.AppHost!.Services.GetRequiredService<IPersistenceProvider>();
            this.DataContext = new UCAddApplicationDialogViewModel(persistence, ApplicationPath);
        }

    }
}