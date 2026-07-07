using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using ModernBoxes.Core.Enums;

namespace ModernBoxes.Presentation.ViewModels
{
    public class AddTempDirViewModel : ObservableObject
    {
        private readonly IPersistenceProvider _persistence;

        /// <summary>
        /// 重要程度单选按钮，默认选中第一项
        /// </summary>
        private List<Boolean> dirKind = new List<bool>() { true, false, false, false };

        public List<Boolean> DirKind
        {
            get { return dirKind; }
            set { dirKind = value; OnPropertyChanged("DirKind"); }
        }

        private TempDirModel dirmodel = new TempDirModel();

        public TempDirModel DirModel
        {
            get { return dirmodel; }
            set { dirmodel = value; OnPropertyChanged("DirModel"); }
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        public RelayCommand AddTempDir { get; }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        public RelayCommand ChoseDir { get; }

        public AddTempDirViewModel(IPersistenceProvider persistence)
        {
            _persistence = persistence;

            AddTempDir = new RelayCommand(async (o) =>
            {
                if (string.IsNullOrEmpty(DirModel.TempDirPath))
                    return;
                try
                {
                    ToggleButton? TB_DirRef = o as ToggleButton;
                    // 读取重要程度
                    if (dirKind[0])
                        DirModel.TempDirImportantKind = DirEnum.dirDanger;
                    if (dirKind[1])
                        DirModel.TempDirImportantKind = DirEnum.dirWaring;
                    if (dirKind[2])
                        DirModel.TempDirImportantKind = DirEnum.dirPrimary;
                    if (dirKind[3])
                        DirModel.TempDirImportantKind = DirEnum.dirSecondary;

                    var dirCache = Path.Combine(AppContext.BaseDirectory, "DirCache");
                    if (TB_DirRef?.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        // 新建文件夹模式：在缓存目录下创建
                        DirModel.TempDirPath = Path.Combine(dirCache, DirModel.TempDirPath);
                        Directory.CreateDirectory(DirModel.TempDirPath);
                    }
                    else if (TB_DirRef != null && TB_DirRef.IsChecked != true)
                    {
                        // 非引用模式：把目标文件夹复制进缓存目录
                        FileHelper.CopyFolder(DirModel.TempDirPath, dirCache);
                        DirModel.TempDirPath = Path.Combine(dirCache, Path.GetFileName(DirModel.TempDirPath.TrimEnd('\\')));
                    }

                    var tempDirs = (await _persistence.LoadAsync<TempDirModel>("tempdirs")).ToList();
                    tempDirs.Add(DirModel);
                    await _persistence.SaveAsync("tempdirs", tempDirs);

                    WeakReferenceMessenger.Default.Send(new AddTempDirMessage(DirModel));
                    WeakReferenceMessenger.Default.Send<Boolean>(true, "IsCloseBaseDialog");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error adding temp dir");
                }
            }, x => true);

            ChoseDir = new RelayCommand((o) =>
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DirModel.TempDirPath = dialog.SelectedPath;
                }
            }, x => true);
        }

        public AddTempDirViewModel(IPersistenceProvider persistence, String dirPath) : this(persistence)
        {
            DirModel.TempDirPath = dirPath;
        }
    }
}
