using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Dialogs
{
    public partial class AddNoteDialog : UserControl
    {
        public AddNoteDialog()
        {
            InitializeComponent();
        }

        public AddNoteDialog(NoteModel note)
        {
            InitializeComponent();
            var vm = ModernBoxes.App.AppHost!.Services.GetRequiredService<AddNoteDialogViewModel>();
            vm.LoadNote(note);
            DataContext = vm;
        }
    }
}
