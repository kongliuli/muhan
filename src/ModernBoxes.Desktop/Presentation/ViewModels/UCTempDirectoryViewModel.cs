using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Presentation.Dialogs;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Presentation.ViewModels
{
    public class UCTempDirectoryViewModel : ObservableObject
    {
        private readonly IDirectoryCardService _service;

        private ObservableCollection<TempDirModel> tempDirs = new ObservableCollection<TempDirModel>();

        public ObservableCollection<TempDirModel> TempDirs
        {
            get
            {
                if (tempDirs.Count > 0)
                    BgEmptyShow = Visibility.Collapsed;
                else
                    BgEmptyShow = Visibility.Visible;
                return tempDirs;
            }
            set { tempDirs = value; OnPropertyChanged("TempDirs"); }
        }

        private Visibility bgEmptyShow = Visibility.Visible;

        public Visibility BgEmptyShow
        {
            get { return bgEmptyShow; }
            set { bgEmptyShow = value; OnPropertyChanged("BgEmptyShow"); }
        }

        public RelayCommand AddTempDir
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    BaseDialog baseDialog = new BaseDialog();
                    baseDialog.SetContent(new AddTempDirDialog());
                    baseDialog.SetTitle("添加文件夹");
                    baseDialog.SetHeight(255);
                    baseDialog.ShowDialog();
                }, x => true);
            }
        }

        public RelayCommand OpenDir
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    _service.OpenDirectory(o.ToString());
                }, x => true);
            }
        }

        public RelayCommand NewTempDir
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    BaseDialog dialog = new BaseDialog();
                    dialog.SetTitle("新建文件夹");
                    AddTempDirDialog addTempDirDialog = new AddTempDirDialog();
                    addTempDirDialog.ChangeToNewDirUI();
                    dialog.SetContent(addTempDirDialog);
                    dialog.ShowDialog();
                }, x => true);
            }
        }

        public UCTempDirectoryViewModel(IDirectoryCardService service)
        {
            _service = service;
            WeakReferenceMessenger.Default.Register<String>(this, "detempdir", (path) => _ = DoDeleteTempDir(path));
            WeakReferenceMessenger.Default.Register<String>(this, "RemoveTempDir", RemoveTempDir);
            WeakReferenceMessenger.Default.Register<DirChangedMessage>(this, (r, m) => RefreshData());
            WeakReferenceMessenger.Default.Register<AddTempDirMessage>(this, (r, m) => AddTempDirItem(m.Dir));
            _ = init();
        }

        public async void RemoveTempDir(String path)
        {
            TempDirs.Remove(TempDirs.FirstOrDefault(o => o.TempDirPath == path));
            await _service.RemoveDirectory(path);
        }

        private void AddTempDirItem(TempDirModel model)
        {
            TempDirs.Add(model);
        }

        public async Task DoDeleteTempDir(String path)
        {
            TempDirModel? tempDirModel = TempDirs.FirstOrDefault(x => x.TempDirPath == path);
            TempDirs.Remove(tempDirModel);
            if (Directory.Exists(path))
            {
                FileSystem.DeleteDirectory(tempDirModel.TempDirPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            else
            {
                BaseDialog baseDialog = new BaseDialog();
                baseDialog.SetTitle("提示");
                baseDialog.SetHeight(170);
                UcMessageDialog ucMessage = new UcMessageDialog("目标文件夹不存在,或许已被移除", MessageDialogState.waring);
                baseDialog.SetContent(ucMessage);
                baseDialog.Show();
            }
            await _service.RemoveDirectory(path);
        }

        public async void RefreshData()
        {
            TempDirs.Clear();
            var dirs = await _service.GetAllDirectories();
            foreach (var d in dirs)
                TempDirs.Add(d);
            if (TempDirs.Count > 0)
                BgEmptyShow = Visibility.Collapsed;
        }

        private async Task init()
        {
            var dirs = await _service.GetAllDirectories();
            foreach (var d in dirs)
                TempDirs.Add(d);
            if (TempDirs.Count > 0)
                BgEmptyShow = Visibility.Collapsed;
        }
    }
}
