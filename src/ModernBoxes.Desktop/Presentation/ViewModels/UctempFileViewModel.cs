using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Dialogs;
using ModernBoxes.Presentation.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Presentation.ViewModels
{
    public class UctempFileViewModel : ObservableObject
    {
        private readonly IFileCardService _fileCardService;

        private TempFileModel tempFile = new TempFileModel();

        public TempFileModel TempFile
        {
            get { return tempFile; }
            set { tempFile = value; OnPropertyChanged("TempFile"); }
        }

        private ObservableCollection<TempFileModel> tempFiles = new ObservableCollection<TempFileModel>();

        public ObservableCollection<TempFileModel> TempFiles
        {
            get
            {
                BgEmptyShow = tempFiles.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                return tempFiles;
            }
            set { tempFiles = value; OnPropertyChanged("TempFiles"); }
        }

        private Visibility bgEmptyShow;

        public Visibility BgEmptyShow
        {
            get { return bgEmptyShow; }
            set { bgEmptyShow = value; OnPropertyChanged("BgEmptyShow"); }
        }

        public RelayCommand NewTempFile
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    BaseDialog baseDialog = new BaseDialog();
                    baseDialog.SetTitle("新建文件");
                    AddTempFileDialog addTempFileDialog = new AddTempFileDialog();
                    addTempFileDialog.ChangeToNewDirUI();
                    baseDialog.SetHeight(200);
                    baseDialog.SetContent(addTempFileDialog);
                    baseDialog.ShowDialog();
                }, x => true);
            }
        }

        public RelayCommand AddTempFile
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    BaseDialog baseDialog = new BaseDialog();
                    baseDialog.SetTitle("选择文件");
                    baseDialog.SetContent(new AddTempFileDialog());
                    baseDialog.ShowDialog();
                }, x => true);
            }
        }

        public RelayCommand OpenFile
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    try
                    {
                        _fileCardService.OpenFile(o.ToString());
                    }
                    catch (Exception)
                    {
                        Logger.Error("File not found when opening temp file");
                        BaseDialog dialog = new BaseDialog();
                        dialog.SetTitle("提示");
                        dialog.SetContent(new UcMessageDialog("没有找到该文件", MessageDialogState.danger));
                        dialog.SetHeight(200);
                        dialog.ShowDialog();
                    }
                }, x => true);
            }
        }

        public RelayCommand OpenFileLocation
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    _fileCardService.OpenFileLocation(o.ToString());
                }, x => true);
            }
        }

        public UctempFileViewModel(IFileCardService fileCardService)
        {
            _fileCardService = fileCardService;
            WeakReferenceMessenger.Default.Register<FileChangedMessage>(this, (r, m) => RefreshFileData());
            WeakReferenceMessenger.Default.Register<AddTempFileMessage>(this, (r, m) => AddFileItem(m.File));
            WeakReferenceMessenger.Default.Register<string>(this, "deleteFile", (filePath) => _ = DoDeleteFile(filePath));
            WeakReferenceMessenger.Default.Register<string>(this, "RemoveFile", (filePath) => _ = RemoveFile(filePath));
            _ = init();
        }

        private void AddFileItem(TempFileModel model)
        {
            TempFiles.Add(model);
        }

        private void RefreshFileData()
        {
            TempFiles.Clear();
            _ = init();
        }

        public async Task RemoveFile(string filePath)
        {
            await _fileCardService.RemoveFile(filePath);
            TempFiles.Remove(TempFiles.FirstOrDefault(o => o.FilePath == filePath));
        }

        public async Task DoDeleteFile(string filePath)
        {
            try
            {
                TempFileModel? model = TempFiles.FirstOrDefault(o => o.FilePath == filePath);
                if (File.Exists(filePath))
                {
                    await _fileCardService.DeleteToRecycleBin(filePath);
                }
                else
                {
                    BaseDialog baseDialog = new BaseDialog();
                    UcMessageDialog ucMessageDialog = new UcMessageDialog("文件不存在可能被删除", MessageDialogState.waring);
                    baseDialog.SetTitle("提示");
                    baseDialog.SetContent(ucMessageDialog);
                    baseDialog.ShowDialog();
                }
                TempFiles.Remove(model);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error deleting temp file");
                BaseDialog baseDialog = new BaseDialog();
                UcMessageDialog ucMessageDialog = new UcMessageDialog(ex.Message, MessageDialogState.waring);
                baseDialog.SetTitle("提示");
                baseDialog.SetContent(ucMessageDialog);
                baseDialog.ShowDialog();
            }
        }

        private async Task init()
        {
            var files = await _fileCardService.GetAllFiles();
            foreach (var file in files)
                TempFiles.Add(file);
        }
    }
}
