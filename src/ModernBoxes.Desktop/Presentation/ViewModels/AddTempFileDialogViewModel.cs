using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.Views;
using ModernBoxes.Presentation.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using ModernBoxes.Core.Enums;

namespace ModernBoxes.Presentation.ViewModels
{
    public class AddTempFileDialogViewModel : ObservableObject
    {
        private readonly IPersistenceProvider _persistence;

        private List<Boolean> fileKind = new List<bool>() { true, false, false, false };

        public List<Boolean> FileKind
        {
            get { return fileKind; }
            set { fileKind = value; OnPropertyChanged("FileKind"); }
        }

        private TempFileModel tempFile = new TempFileModel();

        public TempFileModel TempFile
        {
            get { return tempFile; }
            set { tempFile = value; OnPropertyChanged("TempFile"); }
        }

        /// <summary>
        /// 确认添加临时文件
        /// </summary>
        public RelayCommand AddTempFile { get; }

        /// <summary>
        /// 选择文件
        /// </summary>
        public RelayCommand ChoseFile { get; }

        public AddTempFileDialogViewModel(IPersistenceProvider persistence)
        {
            _persistence = persistence;

            AddTempFile = new RelayCommand(async (o) =>
            {
                if (string.IsNullOrEmpty(TempFile.FilePath))
                    return;
                try
                {
                    ToggleButton? TB_DirRef = o as ToggleButton;
                    // 读取重要程度
                    if (FileKind[0])
                        TempFile.FileKind = DirEnum.dirDanger;
                    if (FileKind[1])
                        TempFile.FileKind = DirEnum.dirWaring;
                    if (FileKind[2])
                        TempFile.FileKind = DirEnum.dirPrimary;
                    if (FileKind[3])
                        TempFile.FileKind = DirEnum.dirSecondary;

                    var fileCache = Path.Combine(AppContext.BaseDirectory, "FileCache");
                    if (TB_DirRef?.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        // 新建文件模式：在缓存目录下创建空文件
                        TempFile.FilePath = Path.Combine(fileCache, TempFile.FilePath);
                        File.Create(TempFile.FilePath).Dispose();
                    }
                    else if (TB_DirRef != null && TB_DirRef.IsChecked != true)
                    {
                        // 非引用模式：把目标文件移动到缓存目录
                        var dest = Path.Combine(fileCache, Path.GetFileName(TempFile.FilePath));
                        File.Move(TempFile.FilePath, dest);
                        TempFile.FilePath = dest;
                    }

                    var tempFiles = (await _persistence.LoadAsync<TempFileModel>("tempfiles")).ToList();
                    tempFiles.Add(TempFile);
                    Logger.Info($"Saving temp file: {TempFile.FilePath}");
                    await _persistence.SaveAsync("tempfiles", tempFiles);

                    WeakReferenceMessenger.Default.Send(new AddTempFileMessage(TempFile));
                    WeakReferenceMessenger.Default.Send<Boolean>(true, "IsCloseBaseDialog");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error adding temp file");
                    BaseDialog dialog = new BaseDialog();
                    dialog.SetTitle("错误");
                    dialog.SetContent(new UcMessageDialog(ex.Message, MessageDialogState.danger));
                    dialog.SetHeight(180);
                    dialog.ShowDialog();
                }
            }, x => true);

            ChoseFile = new RelayCommand((o) =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "*|*";
                openFileDialog.Title = "选择一个文件";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TempFile.FilePath = openFileDialog.FileName;
                }
            }, x => true);
        }

        public AddTempFileDialogViewModel(IPersistenceProvider persistence, String fileName) : this(persistence)
        {
            TempFile.FilePath = fileName;
        }
    }
}
