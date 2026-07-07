using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using System;
using System.IO;
using System.Linq;

namespace ModernBoxes.Presentation.ViewModels
{
    public class UCAddApplicationDialogViewModel : ObservableObject
    {
        private readonly IPersistenceProvider _persistence;

        private ApplicationModel appModel = new ApplicationModel();

        public ApplicationModel AppModel
        {
            get { return appModel; }
            set { appModel = value; OnPropertyChanged("AppModel"); }
        }

        /// <summary>添加应用</summary>
        public RelayCommand AddApplication { get; }

        /// <summary>选择应用路径</summary>
        public RelayCommand ChoseApplicationPath { get; }

        /// <summary>选择图标路径</summary>
        public RelayCommand ChosePhotoPath { get; }

        public UCAddApplicationDialogViewModel(IPersistenceProvider persistence)
        {
            _persistence = persistence;

            AddApplication = new RelayCommand(async (o) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(AppModel.AppPath))
                        return;

                    var usedList = (await _persistence.LoadAsync<ApplicationModel>("applications")).ToList();
                    usedList.Add(AppModel);

                    if (string.IsNullOrEmpty(AppModel.Icon))
                    {
                        var iconPath = AppPaths.Icons + Path.DirectorySeparatorChar;
                        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}.ico";
                        GetIcon.getFileIcon(AppModel.AppPath, iconPath, fileName);
                        AppModel.Icon = iconPath + fileName;
                    }

                    await _persistence.SaveAsync("applications", usedList);
                    WeakReferenceMessenger.Default.Send(new AddApplicationMessage(AppModel));
                    WeakReferenceMessenger.Default.Send<Boolean>(true, "IsCloseBaseDialog");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error adding application");
                }
            }, x => true);

            ChoseApplicationPath = new RelayCommand((o) =>
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "可执行程序|*.exe";
                if (openFileDialog.ShowDialog() == true)
                    AppModel.AppPath = openFileDialog.FileName;
            }, x => true);

            ChosePhotoPath = new RelayCommand((o) =>
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "图标文件|*.ico;*.png;*.jpg";
                if (openFileDialog.ShowDialog() == true)
                    AppModel.Icon = openFileDialog.FileName;
            }, x => true);
        }

        public UCAddApplicationDialogViewModel(IPersistenceProvider persistence, String applicationPath) : this(persistence)
        {
            AppModel.AppPath = applicationPath;
        }
    }
}
