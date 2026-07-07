using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Infrastructure.Ai;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Sdk.Plugins;
using System;
using System.Threading.Tasks;

namespace ModernBoxes.Presentation.ViewModels
{
    public class AddNoteDialogViewModel : ObservableObject
    {
        private readonly AiPromptService _ai;
        private readonly IUserNotifier _notifier;

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
        private bool isAiBusy;

        public bool IsAiBusy
        {
            get => isAiBusy;
            set
            {
                isAiBusy = value;
                OnPropertyChanged(nameof(IsAiBusy));
            }
        }

        public bool IsAiAvailable => _ai.IsAvailable;

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

        public RelayCommand PolishNoteCommand =>
            new RelayCommand(_ => _ = RunPolishAsync(), _ => !IsAiBusy && !string.IsNullOrWhiteSpace(NoteContent));

        public RelayCommand SummarizeNoteCommand =>
            new RelayCommand(_ => _ = RunSummarizeAsync(), _ => !IsAiBusy && !string.IsNullOrWhiteSpace(NoteContent));

        public AddNoteDialogViewModel(AiPromptService ai, IUserNotifier notifier)
        {
            _ai = ai;
            _notifier = notifier;
            isEditMode = false;
        }

        public void LoadNote(NoteModel note)
        {
            isEditMode = true;
            editNote = note;
            NoteTitle = note.Title;
            NoteContent = note.Content;
            NoteColor = note.Color;
        }

        private async Task RunPolishAsync()
        {
            IsAiBusy = true;
            try
            {
                var result = await _ai.PolishNoteAsync(NoteContent);
                if (result == null)
                    _notifier.ShowWarning("AI 润色", "未配置 API 密钥或请求失败");
                else
                    NoteContent = result;
            }
            finally
            {
                IsAiBusy = false;
            }
        }

        private async Task RunSummarizeAsync()
        {
            IsAiBusy = true;
            try
            {
                var result = await _ai.SummarizeNoteAsync(NoteContent);
                if (result == null)
                    _notifier.ShowWarning("AI 总结", "未配置 API 密钥或请求失败");
                else
                    AiResultDialog.Show("AI 总结", result);
            }
            finally
            {
                IsAiBusy = false;
            }
        }
    }
}
