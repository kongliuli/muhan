using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.View;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Presentation.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Presentation.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IPersistenceProvider _persistence;

        public Boolean isFirst = true;

        /// <summary>
        /// 主菜单集合
        /// </summary>
        private ObservableCollection<MenuModel> menus = new ObservableCollection<MenuModel>();

        public ObservableCollection<MenuModel> MenuList
        {
            get { return menus; }
            set { menus = value; OnPropertyChanged("MenuList"); }
        }

        /// <summary>
        /// 点击菜单项：打开文件/文件夹，或展开组件应用面板
        /// </summary>
        public RelayCommand OpenApp { get; }

        /// <summary>
        /// 打开添加菜单对话框
        /// </summary>
        public RelayCommand AddMenuDialog { get; }

        /// <summary>
        /// 打开设置对话框
        /// </summary>
        public RelayCommand OpenSetDialog { get; }

        public MainViewModel(IPersistenceProvider persistence)
        {
            _persistence = persistence;
            Logger.Info("MainViewModel initializing");

            OpenApp = new RelayCommand((o) =>
            {
                var target = o?.ToString() ?? string.Empty;
                if (File.Exists(target))
                {
                    Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                }
                else if (Directory.Exists(target))
                {
                    Process.Start("explorer.exe", target.Replace('/', '\\'));
                }
                else if (target == AppConstants.ComponentAppMenuName)
                {
                    WeakReferenceMessenger.Default.Send<Boolean>(true, "isShow");
                }
            }, x => true);

            AddMenuDialog = new RelayCommand((o) =>
            {
                AddMenuDialog addmenuDialog = new AddMenuDialog();
                addmenuDialog.ShowDialog();
            }, x => true);

            OpenSetDialog = new RelayCommand((o) =>
            {
                BaseDialog dialog = new BaseDialog();
                dialog.SetTitle("设置");
                dialog.setDialogSize(560, 800);
                dialog.SetContent(new UCSetDialog());
                dialog.ShowDialog();
            }, x => true);

            WeakReferenceMessenger.Default.Register<RefreshMenuMessage>(this, (r, m) => RefreshMenu());
            WeakReferenceMessenger.Default.Register<DeleteMenuItemMessage>(this, (r, m) => DeleteMenuItem(m.MenuName));
            _ = loadMenu();
            WeakReferenceMessenger.Default.Register<Boolean>(this, "IsRefreshMainMenu", RefreshMainMenu);
        }

        /// <summary>
        /// 刷新主菜单
        /// </summary>
        private void RefreshMainMenu(bool obj)
        {
            if (obj)
            {
                MenuList.Clear();
                _ = loadMenu();
            }
        }

        /// <summary>
        /// 删除菜单项
        /// </summary>
        private async void DeleteMenuItem(string menuName)
        {
            Logger.Info($"Deleting menu item: {menuName}");
            try
            {
                // 删除组件应用菜单项时同步关闭卡片面板
                if (menuName == AppConstants.ComponentAppMenuName)
                {
                    WeakReferenceMessenger.Default.Send(new CloseComponentMessage());
                }
                MenuModel? menuModel = MenuList.FirstOrDefault(x => x.MenuName == menuName);
                MenuList.Remove(menuModel);
                await _persistence.SaveAsync("menus", MenuList);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error deleting menu item");
            }
        }

        /// <summary>
        /// 添加菜单后刷新界面
        /// </summary>
        private void RefreshMenu()
        {
            MenuList.Clear();
            _ = loadMenu();
        }

        /// <summary>
        /// 加载主菜单
        /// </summary>
        private async Task loadMenu()
        {
            try
            {
                Logger.Info("Loading main menu");
                var menus = await _persistence.LoadAsync<MenuModel>("menus");
                foreach (var menu in menus)
                {
                    MenuList.Add(menu);
                }
                isFirst = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading main menu");
            }
        }


    }
}
