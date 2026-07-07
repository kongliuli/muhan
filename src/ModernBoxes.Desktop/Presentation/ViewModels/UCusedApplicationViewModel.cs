using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Presentation.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ModernBoxes.Core.Enums;

namespace ModernBoxes.Presentation.ViewModels
{
    public class UCusedApplicationViewModel : ObservableObject
    {
        private readonly IApplicationCardService _service;

        private ObservableCollection<ApplicationModel> apps = new ObservableCollection<ApplicationModel>();

        public ObservableCollection<ApplicationModel> Apps
        {
            get
            {
                if (apps.Count > 0)
                    IsShowBgEmpty = Visibility.Collapsed;
                else
                    IsShowBgEmpty = Visibility.Visible;
                return apps;
            }
            set
            {
                apps = value;
                if (apps.Count > 0)
                    IsShowBgEmpty = Visibility.Collapsed;
                else
                    IsShowBgEmpty = Visibility.Visible;
                OnPropertyChanged("Apps");
            }
        }

        private Visibility isShow = Visibility.Visible;

        public Visibility IsShowBgEmpty
        {
            get { return isShow; }
            set { isShow = value; OnPropertyChanged("IsShowBgEmpty"); }
        }

        /// <summary>打开添加应用对话框</summary>
        public RelayCommand OpenAddApplicationDialog { get; }

        /// <summary>运行程序</summary>
        public RelayCommand RunApplication { get; }

        public UCusedApplicationViewModel(IApplicationCardService service)
        {
            _service = service;

            OpenAddApplicationDialog = new RelayCommand((o) =>
            {
                BaseDialog baseDialog = new BaseDialog();
                baseDialog.SetTitle("添加应用");
                baseDialog.SetHeight(270);
                baseDialog.SetContent(new UCAddApplicationDialog());
                baseDialog.ShowDialog();
            }, x => true);

            RunApplication = new RelayCommand((o) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(o?.ToString() ?? "") { UseShellExecute = true });
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    BaseDialog dialog = new BaseDialog();
                    dialog.SetTitle("错误");
                    dialog.SetContent(new UcMessageDialog("找不到应用程序", MessageDialogState.danger));
                    dialog.SetHeight(180);
                    dialog.ShowDialog();
                }
            }, x => true);

            WeakReferenceMessenger.Default.Register<ApplicationChangedMessage>(this, (r, m) => RefreshData());
            WeakReferenceMessenger.Default.Register<AddApplicationMessage>(this, (r, m) => Apps.Add(m.App));
            WeakReferenceMessenger.Default.Register<String>(this, "path", (path) => _ = RemoveApplication(path));
            LoadData();
        }

        private void RefreshData()
        {
            Apps.Clear();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                foreach (var app in _service.GetAllApplications())
                    Apps.Add(app);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load applications");
            }
        }

        private async Task RemoveApplication(string path)
        {
            try
            {
                ApplicationModel? model = Apps.FirstOrDefault(o => o.AppPath.Contains(path));
                if (model != null)
                {
                    _service.RemoveApplication(model.AppPath);
                    Apps.Remove(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error removing application");
            }
        }
    }
}
