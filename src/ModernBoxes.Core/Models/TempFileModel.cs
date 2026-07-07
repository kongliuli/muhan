using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Enums;
using System;

namespace ModernBoxes.Core.Models
{
    public class TempFileModel : ObservableObject
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        private String filePath;

        public String FilePath
        {
            get { return filePath; }
            set { filePath = value; OnPropertyChanged("FilePath"); }
        }

        /// <summary>
        /// 文件类型
        /// </summary>
        private DirEnum fileKind;

        public DirEnum FileKind
        {
            get { return fileKind; }
            set { fileKind = value; OnPropertyChanged("FileKind"); }
        }
    }
}