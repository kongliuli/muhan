using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Presentation.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Presentation.ViewModels
{
    public class UCnotesViewModel : ObservableObject
    {
        private readonly INoteCardService _noteService;

        private ObservableCollection<NoteModel> notes = new ObservableCollection<NoteModel>();

        public ObservableCollection<NoteModel> Notes
        {
            get
            {
                if (notes.Count > 0)
                    IsShowBgEmpty = Visibility.Collapsed;
                else
                    IsShowBgEmpty = Visibility.Visible;
                return notes;
            }
            set
            {
                notes = value;
                if (notes.Count > 0)
                    IsShowBgEmpty = Visibility.Collapsed;
                else
                    IsShowBgEmpty = Visibility.Visible;
                OnPropertyChanged("Notes");
            }
        }

        private Visibility isShow = Visibility.Visible;

        public Visibility IsShowBgEmpty
        {
            get { return isShow; }
            set { isShow = value; OnPropertyChanged("IsShowBgEmpty"); }
        }

        private NoteModel? selectedNote;

        public NoteModel SelectedNote
        {
            get { return selectedNote; }
            set { selectedNote = value; OnPropertyChanged("SelectedNote"); }
        }

        private String searchKeyword = String.Empty;

        public String SearchKeyword
        {
            get { return searchKeyword; }
            set
            {
                searchKeyword = value;
                OnPropertyChanged("SearchKeyword");
                SearchNotes();
            }
        }

        public RelayCommand AddNote
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    BaseDialog baseDialog = new BaseDialog();
                    baseDialog.SetTitle("新建便签");
                    baseDialog.SetHeight(380);
                    baseDialog.SetContent(new AddNoteDialog());
                    baseDialog.ShowDialog();
                }, x => true);
            }
        }

        public RelayCommand DeleteNoteCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (o is NoteModel model)
                    {
                        _noteService.DeleteNote(model.Id);
                        Notes = new ObservableCollection<NoteModel>(_noteService.GetAllNotes());
                    }
                }, x => true);
            }
        }

        public RelayCommand EditNoteCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (o is NoteModel model)
                    {
                        BaseDialog baseDialog = new BaseDialog();
                        baseDialog.SetTitle("编辑便签");
                        baseDialog.SetHeight(380);
                        baseDialog.SetContent(new AddNoteDialog(model));
                        baseDialog.ShowDialog();
                    }
                }, x => true);
            }
        }

        public RelayCommand TogglePinCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (o is NoteModel model)
                    {
                        model.IsPinned = !model.IsPinned;
                        model.UpdatedAt = DateTime.Now;
                        _noteService.UpdateNote(model);
                        Notes = new ObservableCollection<NoteModel>(_noteService.GetAllNotes());
                    }
                }, x => true);
            }
        }

        public RelayCommand ChangeColorCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (o is Tuple<NoteModel, String> param)
                    {
                        param.Item1.Color = param.Item2;
                        param.Item1.UpdatedAt = DateTime.Now;
                        _noteService.UpdateNote(param.Item1);
                        Notes = new ObservableCollection<NoteModel>(_noteService.GetAllNotes());
                    }
                }, x => true);
            }
        }

        public void SearchNotes()
        {
            Notes = new ObservableCollection<NoteModel>(_noteService.SearchNotes(SearchKeyword));
        }

        public UCnotesViewModel(INoteCardService noteService)
        {
            _noteService = noteService;
            _ = LoadNotes();
            WeakReferenceMessenger.Default.Register<NoteModel>(this, "AddNoteModel", (model) =>
            {
                _noteService.AddNote(model);
                Notes = new ObservableCollection<NoteModel>(_noteService.GetAllNotes());
            });
            WeakReferenceMessenger.Default.Register<NoteModel>(this, "UpdateNoteModel", (model) =>
            {
                _noteService.UpdateNote(model);
                Notes = new ObservableCollection<NoteModel>(_noteService.GetAllNotes());
            });
        }

        public async Task SaveNotes()
        {
            _noteService.SaveNotes(Notes);
        }

        public async Task LoadNotes()
        {
            try
            {
                await _noteService.LoadAsync();
                Notes = new ObservableCollection<NoteModel>(_noteService.GetAllNotes());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading notes");
            }
        }
    }
}
