using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Presentation.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using ModernBoxes.Core.Enums;

namespace ModernBoxes.Presentation.ViewModels
{
    public class AddMenuDialogViewModel : ObservableObject
    {
        private MenuModel menuModel = new MenuModel();

        public MenuModel Menu
        {
            get { return menuModel; }
            set { menuModel = value; OnPropertyChanged("Menu"); }
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        public RelayCommand CloseDialog { get; }

        /// <summary>
        /// 添加菜单项
        /// </summary>
        public RelayCommand AddMenu { get; }

        /// <summary>
        /// 选择文件
        /// </summary>
        public RelayCommand ChooseFilePath { get; }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        public RelayCommand ChooseDirPath { get; }

        private readonly IPersistenceProvider _persistence;

        public AddMenuDialogViewModel(IPersistenceProvider persistence)
        {
            _persistence = persistence;

            CloseDialog = new RelayCommand((o) =>
            {
                (o as Window)?.Close();
            }, x => true);

            AddMenu = new RelayCommand(async (o) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(Menu.MenuName) || string.IsNullOrEmpty(Menu.Target))
                    {
                        ShowWarning("路径和名称不能为空");
                        return;
                    }

                    bool isComponentApp = Menu.Target == AppConstants.ComponentAppMenuName
                        || Menu.MenuName == AppConstants.ComponentAppMenuName;
                    if (!File.Exists(Menu.Target) && !Directory.Exists(Menu.Target) && !isComponentApp)
                    {
                        ShowWarning("路径所指向的文件或文件夹不存在");
                        return;
                    }

                    var menus = (await _persistence.LoadAsync<MenuModel>("menus")).ToList();

                    if (isComponentApp)
                    {
                        Menu.Icon = AppConstants.ComponentAppMenuName;
                        if (menus.Any(m => m.Target == Menu.Target))
                        {
                            ShowWarning("只允许添加一个组件应用");
                            return;
                        }
                    }

                    menus.Add(Menu);
                    await _persistence.SaveAsync("menus", menus);
                    WeakReferenceMessenger.Default.Send(new RefreshMenuMessage());
                    WeakReferenceMessenger.Default.Send<Boolean>(true, "IsCloseDialog");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error adding menu item");
                    ShowWarning($"添加失败：{ex.Message}");
                }
            }, x => true);

            ChooseFilePath = new RelayCommand((o) =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "*|*";
                openFileDialog.Title = "选择一个文件";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Menu.Target = openFileDialog.FileName;
                    if (Menu.Target.Substring(Menu.Target.LastIndexOf('.') + 1) == "exe")
                    {
                        String iconPath = Path.Combine(AppContext.BaseDirectory, "icons");
                        String fileName = $"{DateTime.Now:yyyyMMddHHmmss}.ico";
                        GetIcon.getFileIcon(Menu.Target, iconPath + "\\", fileName);
                        Menu.Icon = Path.Combine(iconPath, fileName);
                    }
                    else
                    {
                        Menu.Icon = Menu.Target.Substring(Menu.Target.LastIndexOf('.') + 1);
                    }
                }
            }, x => true);

            ChooseDirPath = new RelayCommand((o) =>
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Menu.Target = dialog.SelectedPath;
                    Menu.Icon = dialog.SelectedPath;
                }
            }, x => true);
        }

        private static void ShowWarning(string message)
        {
            BaseDialog baseDialog = new BaseDialog();
            baseDialog.SetTitle("提示");
            baseDialog.SetContent(new UcMessageDialog(message, MessageDialogState.waring));
            baseDialog.ShowDialog();
        }
    }
}
