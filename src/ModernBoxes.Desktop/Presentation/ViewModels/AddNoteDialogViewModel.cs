using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using System;

namespace ModernBoxes.Presentation.ViewModels
{
    public class AddNoteDialogViewModel : ObservableObject
    {
        private String noteTitle = String.Empty;

        public String NoteTitle
        {
            get { return noteTitle; }
            set { noteTitle = value; OnPropertyChanged("NoteTitle"); }
        }

        private String noteContent = String.Empty;

        public String NoteContent
        {
            get { return noteContent; }
            set { noteContent = value; OnPropertyChanged("NoteContent"); }
        }

        private String noteColor = "#FFE4B5";

        public String NoteColor
        {
            get { return noteColor; }
            set { noteColor = value; OnPropertyChanged("NoteColor"); }
        }

        private NoteModel? editNote;

        private Boolean isEditMode;

        public RelayCommand SaveNote
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (isEditMode && editNote != null)
                    {
                        editNote.Title = NoteTitle;
                        editNote.Content = NoteContent;
                        editNote.Color = NoteColor;
                        editNote.UpdatedAt = DateTime.Now;
                        WeakReferenceMessenger.Default.Send<NoteModel>(editNote, "UpdateNoteModel");
                    }
                    else
                    {
                        NoteModel note = new NoteModel
                        {
                            Title = NoteTitle,
                            Content = NoteContent,
                            Color = NoteColor
                        };
                        WeakReferenceMessenger.Default.Send<NoteModel>(note, "AddNoteModel");
                    }
                    WeakReferenceMessenger.Default.Send<Boolean>(true, "IsCloseBaseDialog");
                }, x => true);
            }
        }

        public AddNoteDialogViewModel()
        {
            isEditMode = false;
        }

        public AddNoteDialogViewModel(NoteModel note)
        {
            isEditMode = true;
            editNote = note;
            NoteTitle = note.Title;
            NoteContent = note.Content;
            NoteColor = note.Color;
        }
    }
}
