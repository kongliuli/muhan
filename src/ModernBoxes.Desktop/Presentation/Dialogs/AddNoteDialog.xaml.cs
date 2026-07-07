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
            this.DataContext = new AddNoteDialogViewModel(note);
        }
    }
}
